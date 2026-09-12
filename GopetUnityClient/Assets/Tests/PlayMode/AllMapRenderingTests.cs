using System.Collections;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace Gopet.PlayModeTests
{
    public sealed class AllMapRenderingTests
    {
        [UnityTest]
        public IEnumerator TatCaMapCoTileDeRender()
        {
            var root = new GameObject("All maps test");
            for (var mapId = 11; mapId <= 34; mapId++)
            {
                var renderer = MapRenderer.Create(root.transform, mapId);
                var usedTiles = 0;
                foreach (var tilemap in renderer.GetComponentsInChildren<Tilemap>())
                    usedTiles += tilemap.GetUsedTilesCount();
                Assert.Greater(usedTiles, 0, $"Map {mapId} không có tile nào để render.");
                Object.DestroyImmediate(renderer.gameObject);
                yield return null;
            }
            Object.DestroyImmediate(root);
        }
    }
}
