using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Trang "Hồi sinh pet sau PK" của popup Bác sĩ: một câu mô tả và nút hồi sinh.
    ///
    /// <para>Chỉ khi BẤM NÚT mới gửi <c>SelectNpcOption(22)</c> — đổi tab sang đây mà
    /// gửi luôn là trừ vàng của người chơi chỉ vì họ bấm vào xem.</para>
    /// </summary>
    public sealed partial class BacSiNpcTabsView
    {
        private GameObject BuildRevivePanel(Transform parent)
        {
            var panel = new GameObject("Revive", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            UiBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(20f, 20f);
            rect.offsetMax = new Vector2(-20f, -20f);

            var desc = UiBuilder.MakeText(panel.transform, _font, "Desc", 12, true);
            desc.text = "Pet đang theo bị chết sau PK? Nhấn nút bên dưới để hồi sinh (tốn vàng).";
            desc.color = TextDark;
            desc.alignment = TextAnchor.UpperCenter;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Overflow;
            var dr = (RectTransform)desc.transform;
            dr.anchorMin = new Vector2(0f, 1f); dr.anchorMax = new Vector2(1f, 1f);
            dr.pivot = new Vector2(0.5f, 1f);
            dr.sizeDelta = new Vector2(0f, 60f);
            dr.anchoredPosition = new Vector2(0f, -8f);

            var btnGo = new GameObject("ReviveBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panel.transform, false);
            var brect = (RectTransform)btnGo.transform;
            brect.anchorMin = brect.anchorMax = new Vector2(0.5f, 0.5f);
            brect.pivot = new Vector2(0.5f, 0.5f);
            brect.sizeDelta = new Vector2(180f, 46f);
            brect.anchoredPosition = new Vector2(0f, -10f);
            var bimg = btnGo.GetComponent<Image>();
            RoundedUiSprite.Apply(bimg);
            bimg.color = ActionBtn;
            var lbl = UiBuilder.MakeText(btnGo.transform, _font, "Label", 14, true);
            lbl.text = "Hồi sinh ngay";
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = Color.white;
            UiBuilder.SetFontStyle(lbl, FontStyle.Bold);
            btnGo.GetComponent<Button>().onClick.AddListener(() => OptionRequested?.Invoke(OpReviveAfterPk));

            return panel;
        }

        /// <summary>
        /// Chọn tab đầu KHÔNG báo server. Nối sự kiện của khay tab sau khi đã chọn:
        /// <c>Select</c> chỉ bắn khi tab đổi, nên nối trước là lần chọn đầu cũng gửi
        /// option đi — với tab tốn vàng thì đó là trừ tiền oan.
        /// </summary>
    }
}
