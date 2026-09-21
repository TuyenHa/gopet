using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Huy hiệu tròn đỏ + số trắng, chờm góc trên-phải một icon. Dùng cho số thư chưa đọc
    /// trên icon hộp thư của HUD; đếm 0 thì tự ẩn.
    ///
    /// <para><b>Vì sao phải có ô vuông trung gian:</b> nút HUD neo theo TỈ LỆ, cạnh ngang lấy
    /// theo chiều rộng màn còn cạnh dọc lấy theo chiều cao — ở 16:9 khung nút rộng gần gấp đôi
    /// chiều cao. Ảnh icon bật <c>preserveAspect</c> nên nó chỉ chiếm ô VUÔNG giữa khung, hai
    /// bên thừa ra một khoảng lớn. Neo huy hiệu vào góc khung là nó văng ra giữa khe, chờm sang
    /// nút bên cạnh. <see cref="AspectRatioFitter"/> dựng đúng ô vuông mà ảnh đang chiếm, huy
    /// hiệu neo vào ô đó mới bám đúng góc icon.</para>
    /// </summary>
    public sealed class UnreadCountBadge : MonoBehaviour
    {
        /// <summary>Quá mốc này thì hiện "99+" — số 3 chữ số không còn đọc được ở cỡ huy hiệu.</summary>
        private const int MaxDisplayed = 99;

        private static readonly Color BadgeRed = new Color(0.92f, 0.20f, 0.20f, 1f);

        private Text _label;

        /// <summary>Gắn vào một nút icon. Trả về huy hiệu đang ẩn; gọi <see cref="SetCount"/> để hiện.</summary>
        public static UnreadCountBadge Attach(Transform iconButton, Font font)
        {
            // Ô vuông trùng đúng vùng ảnh icon đang vẽ — xem phần mô tả class.
            var box = new GameObject("IconBox", typeof(RectTransform), typeof(AspectRatioFitter));
            box.transform.SetParent(iconButton, false);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = Vector2.zero;
            boxRect.anchorMax = Vector2.one;
            boxRect.offsetMin = boxRect.offsetMax = Vector2.zero;
            var fitter = box.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = 1f;

            var go = new GameObject("UnreadBadge", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(box.transform, false);
            var rect = (RectTransform)go.transform;
            // Neo theo tỉ lệ ô vuông để huy hiệu tự co giãn theo độ phân giải, chờm nhẹ ra
            // ngoài mép icon (anchor vượt 1.0) đúng kiểu badge thông báo.
            rect.anchorMin = new Vector2(0.60f, 0.60f);
            rect.anchorMax = new Vector2(1.06f, 1.06f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var circle = go.GetComponent<Image>();
            circle.sprite = CircleUiSprite.Get();
            circle.color = BadgeRed;
            circle.raycastTarget = false; // bấm vào huy hiệu vẫn phải là bấm vào nút hộp thư

            var badge = go.AddComponent<UnreadCountBadge>();
            badge._label = UiBuilder.MakeText(go.transform, font, "Count", 14, stretch: true);
            badge._label.alignment = TextAnchor.MiddleCenter;
            badge._label.color = Color.white;
            badge._label.fontStyle = FontStyle.Bold;
            badge._label.raycastTarget = false;
            // BẮT BUỘC đặt lại hai chế độ tràn: UiBuilder.MakeText để cả hai là Overflow,
            // mà Unity BỎ QUA best-fit khi chữ được phép tràn — không có gì chặn thì không có
            // gì để co lại. Để nguyên thì resizeTextMaxSize dưới đây vô tác dụng và chữ cứ
            // hiện ở fontSize 14 của MakeText.
            badge._label.horizontalOverflow = HorizontalWrapMode.Wrap;
            badge._label.verticalOverflow = VerticalWrapMode.Truncate;
            // bestFit: "99+" phải co lại vừa vòng tròn, số 1 chữ số dừng ở trần 8.
            badge._label.resizeTextForBestFit = true;
            badge._label.resizeTextMinSize = 6;
            badge._label.resizeTextMaxSize = 8;
            // Khung chữ lệch XUỐNG so với tâm vòng tròn: Unity căn giữa theo HỘP DÒNG
            // (ascender + descender), mà chữ số không có phần đuôi dưới nên nhìn bị đội lên.
            // Mức bù tỉ lệ với CỠ CHỮ chứ không phải cỡ vòng tròn: 6% đo lúc chữ còn kẹt ở
            // fontSize 14, nay best-fit đã ghìm về 8 thì 3% là vừa — quá tay là số tụt xuống.
            var labelRect = badge._label.rectTransform;
            // Dịch cả khung sang phải 3% đường kính: chữ số của font mặc định có phần
            // đệm trái dày hơn phải, căn giữa theo hộp chữ thì nét nhìn bị lệch sang trái.
            labelRect.anchorMin = new Vector2(0.13f, 0.12f);
            labelRect.anchorMax = new Vector2(0.93f, 0.82f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            go.SetActive(false);
            return badge;
        }

        /// <summary>Đặt số hiển thị. <c>&lt;= 0</c> là ẩn hẳn huy hiệu.</summary>
        public void SetCount(int count)
        {
            if (count <= 0)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            if (_label != null)
                _label.text = count > MaxDisplayed ? $"{MaxDisplayed}+" : count.ToString();
        }
    }
}
