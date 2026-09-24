using System;
using System.Collections;
using System.Collections.Generic;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hiệu ứng sửa đồ ở Thợ Rèn: viên Đá mài chà qua lại trên icon trang bị, tia lửa bắn
    /// ra ở chỗ chà, icon rung nhẹ; kết thúc bằng một lần loé sáng và nảy icon.
    ///
    /// <para>Gắn vào icon (<see cref="RectTransform"/> pivot giữa). Tia lửa đặt ở cha của
    /// icon để bay được ra ngoài khung icon. Chạy theo thời gian unscaled — không phụ
    /// thuộc timeScale của trận đấu.</para>
    /// </summary>
    public sealed class GrindstoneRepairEffect : MonoBehaviour
    {
        /// <summary>Icon Đá mài sửa chữa (item 1000091) trên server ảnh.</summary>
        private const string StoneIconPath = "items/1000091.png";
        private const float GrindDuration = 1.2f;
        private const float FlashDuration = 0.3f;
        private const int Strokes = 4;
        private const float SparkInterval = 0.035f;
        private const float Gravity = -520f;

        private static readonly Color StoneFallback = new Color(0.52f, 0.56f, 0.62f, 1f);
        private static readonly Color SparkHot = new Color(1f, 0.93f, 0.55f, 1f);
        private static readonly Color SparkWarm = new Color(1f, 0.55f, 0.12f, 1f);

        private sealed class Spark
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float Life;
            public float Age;
        }

        private readonly List<Spark> _sparks = new List<Spark>();

        /// <param name="progress">0..1 trong lúc chà — để popup cho thanh độ bền đầy dần.</param>
        public void Play(RemoteAssetCache assets, Action<float> progress, Action done) =>
            StartCoroutine(Run(assets, progress, done));

        private IEnumerator Run(RemoteAssetCache assets, Action<float> progress, Action done)
        {
            var target = (RectTransform)transform;
            var sparkLayer = target.parent;
            var home = target.anchoredPosition;
            var size = target.rect.size;
            var stone = CreateStone(target, assets, size.x * 0.55f);

            var elapsed = 0f;
            var nextSpark = 0f;
            while (elapsed < GrindDuration)
            {
                var t = elapsed / GrindDuration;
                // Chà ngang qua lại, hơi nghiêng theo chiều đi.
                var phase = t * Strokes * Mathf.PI * 2f;
                var x = Mathf.Sin(phase) * size.x * 0.32f;
                stone.anchoredPosition = new Vector2(x, size.y * 0.12f);
                stone.localRotation = Quaternion.Euler(0f, 0f, -18f - Mathf.Cos(phase) * 12f);
                target.anchoredPosition = home + UnityEngine.Random.insideUnitCircle * 1.2f;

                if (elapsed >= nextSpark)
                {
                    nextSpark += SparkInterval;
                    var contact = target.anchoredPosition + new Vector2(x, size.y * 0.12f - stone.rect.height * 0.4f);
                    var dir = Mathf.Cos(phase) >= 0f ? -1f : 1f; // bắn ngược chiều chà
                    for (var i = 0; i < 3; i++) SpawnSpark(sparkLayer, contact, dir);
                }

                UpdateSparks(Time.unscaledDeltaTime);
                progress?.Invoke(t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            target.anchoredPosition = home;
            progress?.Invoke(1f);
            Destroy(stone.gameObject);

            var flash = CreateFlash(target);
            elapsed = 0f;
            while (elapsed < FlashDuration || _sparks.Count > 0)
            {
                var t = Mathf.Clamp01(elapsed / FlashDuration);
                flash.color = new Color(1f, 1f, 0.85f, 0.85f * (1f - t));
                target.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(t * Mathf.PI));
                UpdateSparks(Time.unscaledDeltaTime);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            target.localScale = Vector3.one;
            Destroy(flash.gameObject);
            done?.Invoke();
        }

        private static RectTransform CreateStone(RectTransform target, RemoteAssetCache assets, float size)
        {
            // Nền đá xám dự phòng; ảnh item Đá mài về thì phủ lên và nền tắt đi.
            var go = new GameObject("Grindstone", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(target, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(size, size * 0.62f);
            var backing = go.GetComponent<Image>();
            RoundedUiSprite.Apply(backing, 4f);
            backing.color = StoneFallback;
            backing.raycastTarget = false;

            var art = new GameObject("Art", typeof(RectTransform), typeof(RawImage));
            art.transform.SetParent(go.transform, false);
            var artRect = (RectTransform)art.transform;
            artRect.sizeDelta = new Vector2(size, size);
            var raw = art.GetComponent<RawImage>();
            raw.raycastTarget = false;
            raw.enabled = false;
            assets?.Get(StoneIconPath, ImagePackets.TypeIcon, texture =>
            {
                if (raw == null || texture == null || texture == assets.Placeholder
                    || texture == assets.FailedTexture) return;
                raw.texture = texture;
                raw.enabled = true;
                backing.color = Color.clear;
            });
            return rect;
        }

        private static Image CreateFlash(RectTransform target)
        {
            var go = new GameObject("RepairFlash", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(target, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 8f);
            image.raycastTarget = false;
            return image;
        }

        private void SpawnSpark(Transform layer, Vector2 at, float dir)
        {
            var go = new GameObject("Spark", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(layer, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = ((RectTransform)transform).anchorMin;
            var len = UnityEngine.Random.Range(3f, 6f);
            rect.sizeDelta = new Vector2(len, 2f);
            rect.anchoredPosition = at;
            var image = go.GetComponent<Image>();
            image.color = Color.Lerp(SparkHot, SparkWarm, UnityEngine.Random.value);
            image.raycastTarget = false;

            _sparks.Add(new Spark
            {
                Rect = rect,
                Image = image,
                Velocity = new Vector2(dir * UnityEngine.Random.Range(60f, 190f),
                    UnityEngine.Random.Range(40f, 170f)),
                Life = UnityEngine.Random.Range(0.28f, 0.5f),
            });
        }

        private void UpdateSparks(float dt)
        {
            for (var i = _sparks.Count - 1; i >= 0; i--)
            {
                var s = _sparks[i];
                s.Age += dt;
                if (s.Age >= s.Life || s.Rect == null)
                {
                    if (s.Rect != null) Destroy(s.Rect.gameObject);
                    _sparks.RemoveAt(i);
                    continue;
                }
                s.Velocity.y += Gravity * dt;
                s.Rect.anchoredPosition += s.Velocity * dt;
                // Tia lửa xoay theo hướng bay rồi nguội dần.
                s.Rect.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(s.Velocity.y, s.Velocity.x) * Mathf.Rad2Deg);
                var c = s.Image.color;
                c.a = 1f - s.Age / s.Life;
                s.Image.color = c;
            }
        }

        private void OnDestroy()
        {
            foreach (var s in _sparks)
                if (s.Rect != null) Destroy(s.Rect.gameObject);
            _sparks.Clear();
        }
    }
}
