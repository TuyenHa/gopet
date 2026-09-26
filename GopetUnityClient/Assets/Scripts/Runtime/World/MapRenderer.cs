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
    public sealed partial class MapRenderer : MonoBehaviour
    {
        /// <summary>Đổi bộ tile theo map nằm ở <see cref="MapSkinOverrides"/>.</summary>
        private const int BeastCityMapId = MapSkinOverrides.BeastCityMapId;
        private const int GreatSpiritViewMapId = MapSkinOverrides.GreatSpiritViewMapId;
        /// <summary>Đấu trường — bỏ cửa hàng Thức ăn (công trình loại 30), thay bằng hồ sen (MapRenderer.Decor).</summary>
        private const int ArenaMapId = 19;
        /// <summary>Loại công trình "Thức ăn" (BuildingDispatcher case 30).</summary>
        private const int FoodShopBuildingType = 30;
        private const int ShopBuildingAnimationId = 190;
        private const int ShopBuildingBaseImageId = 189;
        private const int PineTreeImageId = 158;
        private const int TopPineRowMaxY = 100;
        /// <summary>Cây ATM (vật hoạt ảnh 72, chân ở ~(440,108)) của Thành Phố Linh Thú — bỏ,
        /// NPC Thợ Rèn (-42) đứng thế chỗ (migration-260926-blacksmith-replace-atm.sql).</summary>
        private const int AtmAnimationId = 72;
        /// <summary>Vùng bấm đi kèm cây ATM (building loại 9 — jar không làm gì khi bấm).</summary>
        private const int AtmBuildingType = 9;
        private const int CloudImageIdMin = 30;
        private const int CloudImageIdMax = 36;

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
            if (_mapId == BeastCityMapId) BuildBeastCityGarden(map);
            if (_mapId == GreatSpiritViewMapId) BuildGreatSpiritViewPond();
            if (_mapId == ArenaMapId) BuildArenaPond();
            BuildMapEntities(map);
        }

        private void BuildBeastCityGarden(JarMapLayout map)
        {
            var garden = new GameObject("Beast City North Garden");
            garden.transform.SetParent(transform, false);
            var order = MapPlacement.ObjectSortingOrder(map.Objects.Length);

            // Bottom-centre anchors in map pixels, matching the reference's north lawn.
            // The existing lamp is at (221, 113); the pavement starts at y = 120.
            PlaceGardenObject(garden.transform, "Left Pine", PineTreeImageId, 148, 96, 1.3f, order);
            PlaceGardenObject(garden.transform, "Right Pine", PineTreeImageId, 316, 96, 1.3f, order);
            PlaceGardenObject(garden.transform, "Left Wooden Bench", 39, 178, 106, 1.5f, order + 1);
            PlaceGardenObject(garden.transform, "Right Wooden Bench", 39, 277, 96, 1.5f, order + 1);
            PlaceGardenObject(garden.transform, "Flower Bush", 40, 20, 113, 1.8f, order + 2);
            for (var i = 0; i < 3; i++)
                PlaceGardenObject(garden.transform, $"Flower Bed Fence {i + 1}", 12,
                    6 + i * 18, 117, 0.85f, order + 3);

            // Reference: east of the TAE machine, above the arena path. The generated
            // 1536x1024 PNG has a 1212px-wide opaque bed and 101px bottom padding.
            // Scale the visible bed to 82 map pixels and place it in the middle of
            // the grass lawn, below the mountain area.
            PlaceGardenObject(garden.transform, "Stone Ring Flower Bed", 11163,
                512, 115, 82f / 1212f, order + 4);
        }

        private void PlaceGardenObject(Transform parent, string objectName, int imageId,
            int jarX, int jarY, float scale, int sortingOrder)
        {
            var sprite = TileAssetProvider.FootObject($"newMapData/{imageId}");
            var placed = PlaceObject(parent, sprite, jarX, jarY, sortingOrder);
            placed.name = objectName;
            placed.transform.localScale = new Vector3(scale, scale, 1f);
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
                        var imageId = MapSkinOverrides.ResolveImageId(_mapId, map.ResourceIds[strip]);

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
                var isCloud = id >= CloudImageIdMin && id <= CloudImageIdMax;
                if (_mapId == BeastCityMapId &&
                    (id == ShopBuildingAnimationId || id == ShopBuildingBaseImageId || isCloud ||
                     id == AtmAnimationId ||
                     (id == PineTreeImageId && item.Y <= TopPineRowMaxY)))
                    continue;
                // Đại Linh Cảnh: hai cụm mây tuyết nằm ngay trên mái nhà. Map đã chuyển
                // sang nhiệt đới nên chúng thành đống tuyết đọng trên mái tranh — bỏ.
                if (_mapId == GreatSpiritViewMapId && isCloud) continue;
                // Đấu trường: cửa hàng Thức ăn và vài vật quanh nó nhường chỗ cho hồ sen.
                if (IsHiddenArenaObject(id, item)) continue;
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
                    var artId = MapSkinOverrides.ResolveObjectImageId(_mapId, id);
                    var sprite = TileAssetProvider.FootObject($"newMapData/{artId}");
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
                    if (_mapId == BeastCityMapId && ((entity.BuildingType >= 27 && entity.BuildingType <= 30) ||
                                                     entity.BuildingType == AtmBuildingType))
                        continue;
                    if (_mapId == ArenaMapId && entity.BuildingType == FoodShopBuildingType)
                        continue;
                    // Nhà/shop: có buildingType (0-32), không có Name. Dựng MapBuildingView để bấm.
                    var building = MapBuildingView.Create(transform, entity, map.HeightPixels);
                    building.Selected += e => BuildingSelected?.Invoke(e);
                    continue;
                }
                if (string.IsNullOrEmpty(entity.Name)) continue;
                // Cổng sang map tạm đóng (vd Chợ trời) — server cũng chặn warp, ẩn luôn cổng.
                if (WorldMapLayout.IsHidden(entity.ExtraA)) continue;
                var portal = MapPortalView.Create(transform, entity, map, map.HeightPixels);
                portal.Selected += e => PortalSelected?.Invoke(e);
                _portals.Add(portal);
                if (_self != null) portal.SetSelfTransform(_self);
            }
        }

        private GameObject PlaceObject(Transform parent, Sprite sprite, int jarX, int jarY, int sortingOrder)
        {
            var (wx, wy) = MapPlacement.JarToWorld(jarX, jarY, Map.HeightPixels);
            var go = new GameObject("Object", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(wx, wy, 0f);

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            ApplyUnlitMaterial(sr);
            return go;
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
