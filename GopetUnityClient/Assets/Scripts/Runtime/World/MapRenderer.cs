using System.Collections.Generic;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Dựng map jar trong world-space. Mỗi lớp nền là một Tilemap, tránh tạo hàng
    /// nghìn GameObject/SpriteRenderer và giảm mạnh thời gian đổi map trên mobile.
    ///
    /// <para><b>Y-flip</b>: jar dùng gốc trên-trái, Y hướng xuống. Unity Y hướng lên.
    /// Toàn bộ chuyển đổi nằm trong <see cref="MapPlacement.JarToWorld"/> — thuần C#,
    /// test được ngoài Editor.</para>
    /// </summary>
    public sealed class MapRenderer : MonoBehaviour
    {
        private const int BeastCityMapId = 11;
        private const int SnowGrassImageIdA = 161;
        private const int SnowGrassImageIdB = 162;
        private const int SnowBorderImageId = 3;
        private const int GrassImageIdA = 11161;
        private const int GrassImageIdB = 11162;
        private const int StoneBorderImageId = 11003;

        private static Material _unlitMaterial;
        private readonly List<MapPortalView> _portals = new List<MapPortalView>();
        private Transform _self;
        private int _mapId;

        /// <summary>Map hiện đang render — để component khác (camera, MovementController) đọc kích thước.</summary>
        public JarMapLayout Map { get; private set; }

        public event System.Action<JarMapEntity> PortalSelected;
        public event System.Action<JarMapEntity> BuildingSelected;

        /// <summary>
        /// MapScene gọi khi self avatar spawn/đổi map — mọi portal cần biết vị trí self
        /// để tự bật/tắt nút "Vào" theo khoảng cách (xem <see cref="MapPortalView"/>).
        /// </summary>
        public void SetSelf(Transform self)
        {
            _self = self;
            foreach (var portal in _portals)
                if (portal != null) portal.SetSelfTransform(self);
        }

        /// <summary>Dựng map lên một <see cref="GameObject"/> mới dưới <paramref name="parent"/>.</summary>
        public static MapRenderer Create(Transform parent, int mapId)
        {
            var go = new GameObject($"Map {mapId}");
            if (parent != null) go.transform.SetParent(parent, false);

            var renderer = go.AddComponent<MapRenderer>();
            renderer._mapId = mapId;
            renderer.Build(JarMaps.Load(mapId));
            return renderer;
        }

        private void Build(JarMapLayout map)
        {
            Map = map;
            BuildTileLayers(map);
            BuildObjects(map);
            BuildMapEntities(map);
        }

        private void BuildTileLayers(JarMapLayout map)
        {
            var gridGo = new GameObject("Tile Grid", typeof(Grid));
            gridGo.transform.SetParent(transform, false);
            gridGo.GetComponent<Grid>().cellSize =
                new Vector3(JarMapLayout.TileSize, JarMapLayout.TileSize, 1f);
            for (var layerIdx = 0; layerIdx < map.Layers.Length; layerIdx++)
            {
                var layerGo = new GameObject($"Layer {layerIdx}", typeof(Tilemap), typeof(TilemapRenderer));
                layerGo.transform.SetParent(gridGo.transform, false);
                var layer = map.Layers[layerIdx];
                var sortingOrder = MapPlacement.LayerSortingOrder(layerIdx);
                var tilemap = layerGo.GetComponent<Tilemap>();
                var tileRenderer = layerGo.GetComponent<TilemapRenderer>();
                tileRenderer.sortingOrder = sortingOrder;
                ApplyUnlitMaterial(tileRenderer);
                // Sprite tile có pivot trên-trái để dùng chung với nền uGUI. Neo nó
                // vào góc trên-trái của cell; mặc định (0.5,0.5) làm cả map lệch 12 px.
                tilemap.tileAnchor = new Vector3(0f, 1f, 0f);
                var bounds = new BoundsInt(0, 0, 0, map.WidthTiles, map.HeightTiles, 1);
                var tiles = new TileBase[map.WidthTiles * map.HeightTiles];

                for (var row = 0; row < map.HeightTiles; row++)
                {
                    for (var col = 0; col < map.WidthTiles; col++)
                    {
                        var tile = layer[row][col];
                        var strip = JarMapLayout.StripOf(tile);
                        if (strip < 0 || strip >= map.ImageCount) continue;

                        var unityRow = map.HeightTiles - 1 - row;
                        var imageId = ResolveSkinImageId(_mapId, map.ResourceIds[strip]);

                        // Các strip thay thế giữ nguyên thứ tự ô gốc, nên dữ liệu map tự xếp đúng
                        // cỏ, nền đường, cạnh và góc đá mà không phải sửa file map nhị phân.
                        tiles[unityRow * map.WidthTiles + col] = TileAssetProvider.TileFromImage(
                            imageId, JarMapLayout.CellOf(tile));
                    }
                }

                tilemap.SetTilesBlock(bounds, tiles);
                tilemap.CompressBounds();
            }
        }

        private static int ResolveSkinImageId(int mapId, int imageId)
        {
            if (mapId == BeastCityMapId)
            {
                if (imageId == SnowGrassImageIdA) return GrassImageIdA;
                if (imageId == SnowGrassImageIdB) return GrassImageIdB;
                if (imageId == SnowBorderImageId) return StoneBorderImageId;
            }

            return imageId;
        }

        /// <summary>
        /// Vẽ vật thể theo ĐÚNG THỨ TỰ trong .dat (painter's algorithm), KHÔNG sort theo Y:
        /// tác giả map xếp thứ tự để lớp chồng đúng (nền vẽ trước, mascot vẽ sau đè lên).
        /// Sorting order = <see cref="MapPlacement.ObjectSortingOrder"/> theo chỉ số file.
        /// </summary>
        private void BuildObjects(JarMapLayout map)
        {
            var container = new GameObject("Objects");
            container.transform.SetParent(transform, false);

            for (var i = 0; i < map.Objects.Length; i++)
            {
                var item = map.Objects[i];
                var idx = item.ResourceIndex;
                if (idx < 0 || idx >= map.ResourceIds.Length) continue;

                var id = map.ResourceIds[idx];
                var footY = item.Y - item.YOffset;
                var order = MapPlacement.ObjectSortingOrder(i);
                if (map.ResourceTypes[idx] == JarMapLayout.TypeAnimation)
                {
                    MapAnimatedObjectView.Create(container.transform, id, item,
                        map.HeightPixels, order);
                }
                else
                {
                    // y.java đặt ảnh tại (x-width/2, y-height): điểm (x,y) là giữa đáy.
                    var sprite = TileAssetProvider.FootObject($"newMapData/{id}");
                    PlaceObject(container.transform, sprite, item.X, footY, order);
                }
            }
        }

        private void BuildMapEntities(JarMapLayout map)
        {
            foreach (var entity in map.Entities)
            {
                if (entity.Kind == 0)
                {
                    // Nhà/shop: có buildingType (0-32), không có Name. Dựng MapBuildingView để bấm.
                    var building = MapBuildingView.Create(transform, entity, map.HeightPixels);
                    building.Selected += e => BuildingSelected?.Invoke(e);
                    continue;
                }
                if (string.IsNullOrEmpty(entity.Name)) continue;
                var portal = MapPortalView.Create(transform, entity, map, map.HeightPixels);
                portal.Selected += e => PortalSelected?.Invoke(e);
                _portals.Add(portal);
                if (_self != null) portal.SetSelfTransform(_self);
            }
        }

        private void PlaceObject(Transform parent, Sprite sprite, int jarX, int jarY, int sortingOrder)
        {
            var (wx, wy) = MapPlacement.JarToWorld(jarX, jarY, Map.HeightPixels);
            var go = new GameObject("Object", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(wx, wy, 0f);

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            ApplyUnlitMaterial(sr);
        }

        private static void ApplyUnlitMaterial(Renderer renderer)
        {
            if (_unlitMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                             ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    _unlitMaterial = new Material(shader)
                    {
                        name = "Gopet World Sprite Unlit",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }
            }

            if (_unlitMaterial != null) renderer.sharedMaterial = _unlitMaterial;
        }
    }
}
