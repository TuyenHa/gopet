using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Viên "chip" nhỏ bo tròn: nền trắng, viền mảnh, icon nhỏ bên trái và một dòng
    /// chữ — dùng cho các ô chỉ số ("Tấn công 80-85") trong thẻ item cửa hàng.
    ///
    /// <para>Bề rộng tính thẳng từ bề ngang chuỗi rồi ghi vào
    /// <see cref="LayoutElement"/> làm CẢ min lẫn preferred: "Tấn công 80-85" dài gần
    /// gấp đôi "Phòng thủ 0", ép cùng một bề rộng thì hoặc cụt chữ hoặc thừa một
    /// khoảng trắng to giữa hai chip.</para>
    ///
    /// <para>Đặt cả <c>minWidth</c> mới là chỗ quan trọng: thiếu nó, khi tổng bề rộng
    /// vượt chỗ trống thì hàng chip bóp mọi chip lại — nền chip co còn chữ thì không,
    /// thành ra chữ tràn đè lên chip bên cạnh. Có min thì chip giữ nguyên cỡ và phần
    /// thừa bị <c>RectMask2D</c> của hàng chip cắt gọn ở mép.</para>
    ///
    /// <para><b>Không</b> dùng <see cref="ContentSizeFitter"/> ở đây: nó nong chip ra
    /// SAU khi hàng chip đã xếp chỗ, nên chip chồng lên nhau ở mép trái.</para>
    /// </summary>
    public static class ShopChip
    {
        private const float Height = 17f;
        private const float IconSize = 10f;

        /// <summary><paramref name="iconName"/> null hoặc thiếu file thì chip chỉ có chữ.</summary>
        public static GameObject Create(Transform parent, Font font, string iconName, string label)
        {
            var go = new GameObject("Chip", typeof(RectTransform), typeof(Image),
                typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            // Bán kính 8 chứ không lấy mặc định 10: chip chỉ cao 17, biên 9-slice của
            // sprite mặc định (12) vượt nửa chiều cao nên Unity co biên dọc mà giữ
            // biên ngang — bốn góc bị kéo thành bầu dục.
            RoundedBorder.Apply(go, 8f, Color.white, PopupPalette.Hairline);

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(4, 5, 0, 0);
            layout.spacing = 3f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            ((RectTransform)go.transform).sizeDelta = new Vector2(0f, Height);

            var hasIcon = AddIcon(go.transform, iconName);
            var text = AddLabel(go.transform, font, label);

            var width = layout.padding.left + layout.padding.right + text.preferredWidth
                        + (hasIcon ? IconSize + layout.spacing : 0f);
            var element = go.GetComponent<LayoutElement>();
            element.minWidth = width;
            element.preferredWidth = width;
            return go;
        }

        /// <summary>Trả về <c>false</c> khi không có icon để đặt — bề rộng chip tính theo đó.</summary>
        private static bool AddIcon(Transform parent, string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return false;

            var sprite = HudSkin.Get(iconName);
            if (sprite == null) return false;

            var go = new GameObject("Icon", typeof(RectTransform), typeof(Image),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;

            var element = go.GetComponent<LayoutElement>();
            element.preferredWidth = IconSize;
            element.preferredHeight = IconSize;
            return true;
        }

        private static Text AddLabel(Transform parent, Font font, string label)
        {
            var text = UiBuilder.MakeText(parent, font, "Label", 9, false);
            text.text = label;
            text.color = PopupPalette.TextDark;
            text.alignment = TextAnchor.MiddleLeft;

            // MakeText bật Overflow cho cả hai chiều nên chữ không tự xuống dòng;
            // preferredWidth của Text vẫn là bề ngang thật của chuỗi, và đó là con số
            // hàng chip bên ngoài dùng để chia chỗ.
            var element = text.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = Height;
            return text;
        }
    }
}
