using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Khối "VS" giữa màn đấu — ghim đỉnh cách lề dưới top bar (32px) 10px,
    /// biển "Đến lượt bạn" nằm dưới badge đúng <see cref="GapPx"/>.
    ///
    /// <para>Biển là một KHUNG bo góc nền tối, có vạch dọc ở đầu chữ. Trước đây chữ trần với
    /// viền trắng + bóng đổ; nền rừng sáng tối lẫn lộn nên chữ lúc rõ lúc chìm. Khung nền tự
    /// nó tách chữ khỏi mọi nền, không cần viền quanh nét chữ nữa.</para></summary>
    public sealed class BattleVsIndicator : MonoBehaviour
    {
        private const float RefHeight = 540f;      // CanvasScaler referenceResolution.y
        private const float GapPx = 5f;            // khoảng hở giữa đáy badge và đỉnh nhãn
        private const int FontSize = 13;
        // Biển ôm sát chữ: chữ nhỏ đi mà khung giữ nguyên thì thừa một khoảng rỗng hai bên.
        private const float PlaqueWidthPx = 156f;
        private const float PlaqueHeightPx = 28f;
        private const int CornerRadiusPx = 9;
        private const int BorderPx = 2;

        /// <summary>Vạch dọc ở đầu chữ: cách mép trái bao nhiêu, dày và cao bao nhiêu.</summary>
        private const float BarInsetPx = 10f, BarWidthPx = 4f, BarHeightPx = 15f;

        /// <summary>Khoảng hở giữa vạch và chữ, cộng thêm lề phải cho cân.</summary>
        private const float BarTextGapPx = 7f;

        /// <summary>Vàng của viền biển — cùng tông với badge VS ngay trên nó.</summary>
        private static readonly Color Gold = new Color(1f, 0.78f, 0.28f, 1f);

        /// <summary>Xanh da trời cho vạch đầu chữ.</summary>
        private static readonly Color SkyBlue = new Color(0.35f, 0.78f, 1f, 1f);

        private GameObject _plaque;
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

            // Cách đáy badge đúng GapPx. Quy về tỉ lệ vì anchor tính theo chiều cao canvas,
            // mà CanvasScaler khớp theo chiều cao (matchWidthOrHeight = 1) nên 1px luôn là
            // 1/540 bất kể tỉ lệ màn hình.
            var labelTopY = 0.58f + deltaY - GapPx / RefHeight;
            indicator.BuildPlaque(go.transform, font, labelTopY);
            return indicator;
        }

        /// <summary>Chỉ báo khi đến lượt người chơi cục bộ; lượt quái/đối thủ thì im lặng.
        /// Ẩn cả BIỂN chứ không chỉ xoá chữ — bỏ lại cái khung rỗng thì trông như lỗi.</summary>
        public void SetTurn(bool isLocalTurn)
        {
            if (_plaque != null) _plaque.SetActive(isLocalTurn);
            if (_turnLabel != null) _turnLabel.text = "Đến lượt bạn";
        }

        private void BuildPlaque(Transform parent, Font font, float topY)
        {
            _plaque = new GameObject("Biển lượt", typeof(RectTransform));
            _plaque.transform.SetParent(parent, false);
            var rect = (RectTransform)_plaque.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, topY);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(PlaqueWidthPx, PlaqueHeightPx);

            // Nền trước, viền sau: hai Image chồng nhau vì PanelSprites tách nền/viền qua
            // kênh alpha, mỗi lớp mới tô được một màu riêng.
            AddPanel(rect, PanelSprites.Rounded(CornerRadiusPx), new Color(0.04f, 0.07f, 0.13f, 0.9f));
            AddPanel(rect, PanelSprites.Rounded(CornerRadiusPx, BorderPx), Gold);

            var barGo = new GameObject("Vạch đầu chữ", typeof(RectTransform), typeof(Image));
            barGo.transform.SetParent(rect, false);
            barGo.GetComponent<Image>().color = SkyBlue;
            var bar = (RectTransform)barGo.transform;
            bar.anchorMin = bar.anchorMax = new Vector2(0f, 0.5f);
            bar.pivot = new Vector2(0f, 0.5f);
            bar.anchoredPosition = new Vector2(BarInsetPx, 0f);
            bar.sizeDelta = new Vector2(BarWidthPx, BarHeightPx);

            _turnLabel = UiBuilder.MakeText(rect, font, "Thông báo lượt", FontSize, false);
            var text = _turnLabel.rectTransform;
            text.anchorMin = Vector2.zero;
            text.anchorMax = Vector2.one;
            // Chừa chỗ cho vạch bên trái, lề phải bằng đúng lề trái cho chữ đứng cân giữa.
            var left = BarInsetPx + BarWidthPx + BarTextGapPx;
            text.offsetMin = new Vector2(left, 0f);
            text.offsetMax = new Vector2(-left, 0f);
            _turnLabel.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(_turnLabel, FontStyle.Bold);
            _turnLabel.color = Color.white;
        }

        private static void AddPanel(Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject("Lớp", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;   // 9-slice: viền giữ nguyên độ dày ở mọi cỡ
            image.color = color;
            image.raycastTarget = false;
        }
    }
}
