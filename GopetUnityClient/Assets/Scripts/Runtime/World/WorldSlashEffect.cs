using System;
using System.Collections;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Vệt chém bay từ pet sang quái khi bấm nút đánh, rồi mới mở màn đánh quái.
    ///
    /// <para>Có hiệu ứng là để cú bấm có phản hồi NGAY tại chỗ: gói <c>ATTACK_MOB</c> đi
    /// rồi còn chờ server dựng trận, khoảng lặng đó khiến người chơi tưởng nút không ăn và
    /// bấm lại. Vệt chém lấp đúng khoảng lặng ấy.</para>
    ///
    /// <para>Ảnh vẽ vệt hướng sang PHẢI nên chỉ cần xoay theo hướng pet → quái là dùng
    /// được cho mọi hướng — xem <c>tools/image-gen/make-slash-effect.py</c>.</para>
    /// </summary>
    public sealed class WorldSlashEffect : MonoBehaviour
    {
        public const string SpriteResource = "Ui/fx-slash";

        /// <summary>Thời gian vệt bay từ pet tới quái.</summary>
        private const float TravelSeconds = 0.18f;

        /// <summary>Thời gian nở bung + tan ở chỗ quái.</summary>
        private const float ImpactSeconds = 0.20f;

        /// <summary>Bề cao vệt lúc bay và lúc chạm, tính bằng pixel jar (1 unit = 1 px).</summary>
        private const float TravelPixels = 48f;
        private const float ImpactPixels = 88f;

        /// <summary>Góc vung: vệt xuất phát ngửa về sau rồi quét tới, đúng nhịp một nhát vung
        /// tay chứ không phải một vệt trôi ngang. Pha chạm quét nốt phần còn lại.</summary>
        private const float SwingStartDegrees = -42f;
        private const float ImpactSpinDegrees = 34f;

        /// <summary>Vẽ trên mọi sprite của world. HUD nằm ở Canvas riêng nên không bị che.</summary>
        private const int SortingOrder = 10000;

        /// <summary>Nhấc điểm bắn/điểm chạm lên khỏi chân sprite cho khỏi quét dưới đất.</summary>
        private const float BodyCenterOffset = 18f;

        private SpriteRenderer _renderer;
        private Action _done;

        /// <summary>Tổng thời gian một lần diễn — người gọi dùng để khoá nút cho khỏi bấm chồng.</summary>
        public static float TotalSeconds => TravelSeconds + ImpactSeconds;

        /// <summary>
        /// Diễn vệt chém rồi gọi <paramref name="done"/>. THIẾU ẢNH thì gọi
        /// <paramref name="done"/> ngay: hiệu ứng là phần trang trí, không được phép
        /// nuốt mất cú đánh.
        /// </summary>
        public static void Play(Transform parent, Vector3 from, Vector3 to, Action done)
        {
            var sprite = Resources.Load<Sprite>(SpriteResource);
            if (parent == null || sprite == null)
            {
                Debug.LogWarning($"[Gopet] Thiếu ảnh vệt chém '{SpriteResource}' — bỏ qua hiệu ứng.");
                done?.Invoke();
                return;
            }

            var go = new GameObject("Slash", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            var effect = go.AddComponent<WorldSlashEffect>();
            effect._renderer = go.GetComponent<SpriteRenderer>();
            effect._renderer.sprite = sprite;
            effect._renderer.sortingOrder = SortingOrder;
            effect._done = done;
            effect.StartCoroutine(effect.Run(
                from + Vector3.up * BodyCenterOffset,
                to + Vector3.up * BodyCenterOffset,
                sprite));
        }

        private IEnumerator Run(Vector3 from, Vector3 to, Sprite sprite)
        {
            // Quy đổi theo chiều cao THẬT của sprite thay vì hằng số: ảnh nhập với
            // pixelsPerUnit nào thì vệt vẫn ra đúng cỡ đó trong world.
            var unit = sprite.bounds.size.y > 0f ? sprite.bounds.size.y : 1f;
            var direction = to - from;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return Phase(TravelSeconds, t =>
            {
                transform.localPosition = Vector3.Lerp(from, to, t);
                transform.localRotation = Quaternion.Euler(0f, 0f, angle + SwingStartDegrees * (1f - t));
                SetScale(Mathf.Lerp(TravelPixels, ImpactPixels * 0.7f, t) / unit);
                SetAlpha(1f);
            });

            yield return Phase(ImpactSeconds, t =>
            {
                transform.localPosition = to;
                transform.localRotation = Quaternion.Euler(0f, 0f, angle + ImpactSpinDegrees * t);
                SetScale(Mathf.Lerp(ImpactPixels * 0.7f, ImpactPixels, t) / unit);
                SetAlpha(1f - t);
            });

            var done = _done;
            _done = null;
            Destroy(gameObject);
            done?.Invoke();
        }

        /// <summary>Chạy <paramref name="apply"/> với t đi từ 0 tới 1 trong <paramref name="seconds"/>.</summary>
        private static IEnumerator Phase(float seconds, Action<float> apply)
        {
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                apply(Mathf.Clamp01(elapsed / seconds));
                elapsed += Time.deltaTime;
                yield return null;
            }
            apply(1f);
        }

        private void SetScale(float scale) => transform.localScale = new Vector3(scale, scale, 1f);

        private void SetAlpha(float alpha)
        {
            var color = _renderer.color;
            color.a = Mathf.Clamp01(alpha);
            _renderer.color = color;
        }
    }
}
