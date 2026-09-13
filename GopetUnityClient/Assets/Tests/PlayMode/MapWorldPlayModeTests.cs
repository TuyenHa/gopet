using System.Collections;
using System.Diagnostics;
using System.IO;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace Gopet.PlayModeTests
{
    public sealed class MapWorldPlayModeTests
    {
        [UnityTest]
        public IEnumerator Map11_DungTilemapTheoLayer_VaKhongTaoCellGameObject()
        {
            var watch = Stopwatch.StartNew();
            var renderer = MapRenderer.Create(null, 11);
            watch.Stop();
            Assert.AreEqual(renderer.Map.Layers.Length,
                renderer.GetComponentsInChildren<Tilemap>().Length);
            foreach (var tilemap in renderer.GetComponentsInChildren<Tilemap>())
                Assert.AreEqual(new Vector3(0f, 1f, 0f), tilemap.tileAnchor);
            Assert.IsNull(renderer.transform.Find("Cell"));
            var objects = renderer.transform.Find("Objects");
            var animated = objects.GetComponentsInChildren<MapAnimatedObjectView>();
            Assert.IsNotEmpty(animated, "Map 11 phải dựng object hoạt ảnh từ metadata _b.");
            foreach (Transform child in objects)
            {
                var sprite = child.GetComponent<SpriteRenderer>();
                if (sprite != null) Assert.AreEqual(0f, sprite.sprite.pivot.y, 0.01f,
                    "Object tĩnh phải neo ở giữa đáy như y.java.");
            }
            Assert.Less(watch.ElapsedMilliseconds, 5000,
                "Dựng map 11 trong Editor vượt 5 giây.");
            Object.Destroy(renderer.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TatCa24Map_DungTilemapTrongNganSachEditor()
        {
            var total = Stopwatch.StartNew();
            long slowestMs = 0;
            var slowestMap = -1;
            for (var mapId = 11; mapId <= 34; mapId++)
            {
                var watch = Stopwatch.StartNew();
                var renderer = MapRenderer.Create(null, mapId);
                watch.Stop();
                if (watch.ElapsedMilliseconds > slowestMs)
                {
                    slowestMs = watch.ElapsedMilliseconds;
                    slowestMap = mapId;
                }
                Assert.AreEqual(renderer.Map.Layers.Length,
                    renderer.GetComponentsInChildren<Tilemap>().Length, $"Map {mapId}");
                Assert.IsNull(renderer.transform.Find("Cell"), $"Map {mapId}");
                Object.DestroyImmediate(renderer.gameObject);
            }
            total.Stop();
            TestContext.WriteLine($"24 maps: {total.ElapsedMilliseconds} ms; slowest map {slowestMap}: {slowestMs} ms");
            Assert.Less(total.ElapsedMilliseconds, 5000,
                "Dựng tuần tự 24 map trong Editor vượt 5 giây.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator MovementController_ResetMapKhongGiuMapIdVaToaDoCu()
        {
            var scene = MapScene.Create();
            scene.LoadMap(11);
            var movement = MovementController.Attach(scene, new Gopet.Net.Map.MapHandler(), null, null,
                11, 7, 100, 120);
            movement.ResetForMap(23, 345, 456);
            Assert.AreEqual(23, movement.CurrentMapId);
            Assert.AreEqual(new Vector2(345, 456), movement.JarPosition);
            Object.Destroy(scene.gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TaoAnhCanChinhMapVaAvatar()
        {
            var renderer = MapRenderer.Create(null, 11);
            var avatar = PlayerAvatar.Spawn(null, 1, "Can chinh", 0, 288, 330,
                renderer.Map.HeightPixels);
            var cameraGo = new GameObject("Alignment Camera", typeof(Camera));
            var camera = cameraGo.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = renderer.Map.HeightPixels * 0.5f;
            camera.aspect = (float)renderer.Map.WidthPixels / renderer.Map.HeightPixels;
            camera.transform.position = new Vector3(renderer.Map.WidthPixels * 0.5f,
                renderer.Map.HeightPixels * 0.5f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            var target = new RenderTexture(renderer.Map.WidthPixels, renderer.Map.HeightPixels, 24);
            camera.targetTexture = target;
            yield return null;
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            var output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "TestResults",
                "phase06-alignment-preview.png"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, image.EncodeToPNG());
            Assert.Greater(new FileInfo(output).Length, 1000);
            RenderTexture.active = previous;
            camera.targetTexture = null;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(cameraGo);
            Object.DestroyImmediate(avatar.gameObject);
            Object.DestroyImmediate(renderer.gameObject);
        }

        [UnityTest]
        public IEnumerator Avatar_CoTen_GioiTinhKhacNhau_VaKhongConMarkerDo()
        {
            var root = new GameObject("Avatar test root");
            var male = PlayerAvatar.Spawn(root.transform, 1, "Nam", 0, 100, 100, 480);
            var female = PlayerAvatar.Spawn(root.transform, 2, "Nữ", 1, 120, 100, 480);
            Assert.IsNotNull(male.transform.Find("PlayerName"));
            Assert.IsNull(male.transform.Find("DebugMarker"));
            Assert.IsNotNull(male.transform.Find("Appearance/Part 13"));
            Assert.IsNotNull(female.transform.Find("Appearance/Part 14"));
            Assert.LessOrEqual(male.transform.Find("Appearance/Part 5")
                .GetComponent<SpriteRenderer>().sprite.rect.width, 36f);
            Assert.LessOrEqual(male.transform.Find("Appearance/Part 11")
                .GetComponent<SpriteRenderer>().sprite.rect.width, 33f);
            Object.Destroy(root);
            yield return null;
        }

    }
}
