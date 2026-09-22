using System.Collections;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace Gopet.PlayModeTests
{
    public sealed class DirtMapRenderingTests
    {
        [UnityTest]
        public IEnumerator MoiMap_DungTextureVaViTriChoTungO()
        {
            for (var mapId = 11; mapId <= 34; mapId++)
            {
                AssertRenderedTiles(mapId);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ChuyenDatBangDat_KhongDungNhamCache()
        {
            foreach (var mapId in new[] { 14, 24, 17, 23, 14 })
            {
                // Deliberately keep the production caches across map transitions.
                AssertRenderedTiles(mapId);
                yield return null;
            }
        }

        private static void AssertRenderedTiles(int mapId)
        {
            var root = new GameObject("Dirt map verification");
            try
            {
                var renderer = MapRenderer.Create(root.transform, mapId);
                var map = renderer.Map;
                for (var layer = 0; layer < map.Layers.Length; layer++)
                {
                    var tilemap = renderer.transform.Find($"Tile Grid/Layer {layer}")
                        .GetComponent<Tilemap>();
                    for (var row = 0; row < map.HeightTiles; row++)
                    for (var col = 0; col < map.WidthTiles; col++)
                    {
                        var packed = map.Layers[layer][row][col];
                        var strip = JarMapLayout.StripOf(packed);
                        var tile = tilemap.GetTile<Tile>(
                            new Vector3Int(col, map.HeightTiles - 1 - row, 0));
                        if (strip < 0 || strip >= map.ImageCount)
                        {
                            Assert.IsNull(tile);
                            continue;
                        }
                        var expectedId = ExpectedImage(mapId, map.ResourceIds[strip]);
                        var expectedTexture = Resources.Load<Texture2D>(
                            $"Jar/Art/Raw/newMapData/{expectedId}");
                        var location = $"Map {mapId} layer {layer} ({col},{row})";
                        Assert.IsNotNull(expectedTexture, location);
                        Assert.IsNotNull(tile, location);
                        Assert.IsNotNull(tile.sprite, location);
                        Assert.AreSame(expectedTexture, tile.sprite.texture, location);
                        Assert.AreEqual(new Rect(JarMapLayout.CellOf(packed) * 24, 0, 24, 24),
                            tile.sprite.rect, location);
                        Assert.AreEqual(Tile.ColliderType.None, tile.colliderType, location);
                    }
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        // Independent expected policy; never call ResolveImageId to build expectations.
        private static int ExpectedImage(int map, int image)
        {
            if (map == 16 && image == 179) return 11179;
            if (map == 16 && image == 180) return 11180;
            if (map == 11 || map == 16 || map == 19)
            {
                if (image == 3) return 11003;
                if (image == 161) return 11161;
                if (image == 162) return 11162;
            }
            if (map == 17 || map == 18 || map == 21 || map == 22)
            {
                if (image == 179) return 12179;
                if (image == 180) return 12180;
            }
            if (map == 12 || map == 13 || map == 14 || map == 15 ||
                map == 17 || map == 18 || map == 20 || map == 22)
            {
                if (image == 151) return 12151;
                if (image == 161) return 12161;
                if (image == 162) return 12162;
            }
            return image;
        }
    }
}
