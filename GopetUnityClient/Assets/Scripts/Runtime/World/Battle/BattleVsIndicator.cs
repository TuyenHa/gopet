using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Khối "VS" giữa màn đấu — ghim đỉnh cách lề dưới top bar (32px) 10px,
    /// nhãn "Đến lượt bạn" nằm dưới badge đúng <see cref="GapPx"/>.</summary>
    public sealed class BattleVsIndicator : MonoBehaviour
    {
        private const float RefHeight = 540f;      // CanvasScaler referenceResolution.y
        private const float GapPx = 5f;            // khoảng hở giữa đáy badge và đỉnh nhãn
        private const float LabelHeightPx = 36f;
        private const int FontSize = 22;

        private Text _turnLabel;

        public static BattleVsIndicator Create(Transform parent, Font font)
        {
            const float topBarBottomY = 1f - 32f / 540f;
            var vsTopY = topBarBottomY - 10f / 540f;
            var deltaY = vsTopY - 0.76f;

            var go = new GameObject("VS", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var indicator = go.AddComponent<BattleVsIndicator>();

            var badgeGo = new GameObject("VS badge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(go.transform, false);
            var badge = badgeGo.GetComponent<Image>();
            badge.sprite = BattleSkin.Load("Battle/vs");
            badge.preserveAspect = true;
            var bRect = (RectTransform)badgeGo.transform;
            bRect.anchorMin = new Vector2(0.43f, 0.58f + deltaY);
            bRect.anchorMax = new Vector2(0.57f, 0.76f + deltaY);
            bRect.offsetMin = bRect.offsetMax = Vector2.zero;

            indicator._turnLabel = UiBuilder.MakeText(go.transform, font, "Thông báo lượt", FontSize, false);
            var tRect = indicator._turnLabel.rectTransform;
            // Cách đáy badge đúng GapPx. Quy về tỉ lệ vì anchor tính theo chiều cao canvas,
            // mà CanvasScaler khớp theo chiều cao (matchWidthOrHeight = 1) nên 1px luôn là
            // 1/540 bất kể tỉ lệ màn hình.
            var labelTopY = 0.58f + deltaY - GapPx / RefHeight;
            tRect.anchorMin = new Vector2(0.3f, labelTopY - LabelHeightPx / RefHeight);
            tRect.anchorMax = new Vector2(0.7f, labelTopY);
            tRect.offsetMin = tRect.offsetMax = Vector2.zero;
            indicator._turnLabel.alignment = TextAnchor.MiddleCenter;
            indicator._turnLabel.fontStyle = FontStyle.Bold;
            // Vàng cam đậm, KHÔNG dùng vàng nhạt: viền trắng bao quanh nên chữ phải tối màu
            // hơn viền, nếu không hai sắc sáng đè nhau thành một vệt mờ.
            indicator._turnLabel.color = new Color(1f, 0.72f, 0.05f, 1f);

            // Thứ tự QUAN TRỌNG: Outline trước, Shadow sau. Unity UI cho mỗi hiệu ứng nhân bản
            // luồng đỉnh của các hiệu ứng trước nó, nên Shadow sau sẽ đổ bóng cho cả cụm
            // chữ-kèm-viền. Đảo lại thành Shadow trước thì viền sẽ bao quanh cả bóng, trông bẩn.
            var outline = indicator._turnLabel.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = false;

            // Bóng tối tách chữ khỏi nền rừng sáng — viền trắng một mình chìm nghỉm trên nền sáng.
            var shadow = indicator._turnLabel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(0f, -3f);
            shadow.useGraphicAlpha = false;
            return indicator;
        }

        /// <summary>Chỉ báo khi đến lượt người chơi cục bộ; lượt quái/đối thủ thì im lặng.</summary>
        public void SetTurn(bool isLocalTurn) => _turnLabel.text = isLocalTurn ? "Đến lượt bạn" : "";
    }
}
