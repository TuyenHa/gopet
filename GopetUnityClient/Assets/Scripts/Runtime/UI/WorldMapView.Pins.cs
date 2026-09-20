using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Điểm đánh dấu của một map: pin giọt nước vàng, pin xanh có sao cho map đang
    /// đứng. Ảnh sinh bằng
    /// <c>tools/image-gen/make-world-map-pins.py</c> — xem
    /// <c>tools/image-gen/world-map-pins.md</c> trước khi đổi tỉ lệ ảnh.
    /// </summary>
    public sealed partial class WorldMapView
    {
        public const string PinResource = "Ui/WorldMap/world-map-pin";
        public const string CurrentPinResource = "Ui/WorldMap/world-map-pin-current";

        /// <summary>
        /// Pin giọt nước cắm xuống địa điểm; map khoá thay bằng icon ổ khoá. Neo vào
        /// TÂM node chứ không mép trên: node phình ra cho đủ vùng chạm thì pin và thẻ
        /// tên vẫn phải dính nhau như cũ, không bị kéo giãn ra hai đầu.
        /// </summary>
        private void MakePin(Transform parent, bool locked, bool isCurrent)
        {
            var go = new GameObject("Pin", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = locked
                ? new Vector2(LockSize, LockSize)
                : new Vector2(PinWidth, PinHeight);
            rect.anchoredPosition = new Vector2(0f, PinBottomY);

            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.sprite = locked
                ? JarSkin.Raw("lock")
                : PinSprite(isCurrent ? CurrentPinResource : PinResource);
            if (image.sprite != null) return;

            // Thiếu ảnh pin thì rơi về chấm tròn cũ, KHÔNG để trống: node không có gì
            // đánh dấu thì nhìn như tên map lơ lửng giữa tranh.
            image.color = isCurrent ? PinCurrent : new Color(1f, 0.82f, 0.22f, 1f);
            RoundedUiSprite.Apply(image);
        }

        private static Sprite PinSprite(string resource)
        {
            var sprite = Resources.Load<Sprite>(resource);
            if (sprite == null)
                Debug.LogWarning($"[Gopet] Thiếu icon bản đồ tại Resources/{resource}.");
            return sprite;
        }
    }
}
