using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Thanh HUD góc trên-phải: <b>Cửa hàng</b>, <b>Dịch vụ</b>, <b>Sự kiện</b>, <b>Hộp thư</b>.
    /// Bám góc màn hình thật trên canvas chung, không co theo letterbox của PixelCanvas.
    ///
    /// <para>Icon load từ <see cref="HudSkin"/>; thiếu file thì <b>lùi về nhãn chữ
    /// gọn</b> chứ không ném — mất icon không đáng để chặn HUD.</para>
    /// </summary>
    public sealed class ShopServiceEventHud : MonoBehaviour
    {
        /// <summary>
        /// Cạnh nút HUD theo tỉ lệ chiều cao. <see cref="MinimapWidget"/> dùng chung để
        /// nằm đúng hàng với ba nút này.
        /// </summary>
        public const float SizeFrac = 0.075f;

        /// <summary>Khoảng cách giữa các nút, theo tỉ lệ chiều cao.</summary>
        public const float GapFrac = 0.006f;

        /// <summary>Khoảng cách của ô ngoài cùng tới mép phải.</summary>
        public const float ReservedRightFrac = 0.012f;

        public const float TopMarginFrac = 0.015f;

        /// <summary>
        /// Ô ngoài cùng bên PHẢI của hàng để trống cho minimap — ba nút vì thế lùi sang
        /// trái đúng một ô. Minimap tự neo vào ô này (<see cref="MinimapWidget"/>); hai
        /// bên dùng chung hằng số nên không thể lệch nhau.
        /// </summary>
        public const float MinimapSlotFrac = SizeFrac;

        /// <summary>
        /// Nhãn ("Cửa hàng"/"Sự kiện"…) thò xuống dưới đáy nút bao nhiêu, tính theo cạnh
        /// nút. <see cref="MinimapWidget"/> dùng để kéo dài xuống ĐÚNG mép dưới của chữ.
        /// </summary>
        public const float LabelBottomFrac = 0.25f;

        public event Action ShopClicked;
        public event Action MarketClicked;
        public event Action ServiceClicked;
        public event Action EventClicked;
        public event Action MailClicked;

        private UnreadCountBadge _mailBadge;

        public static ShopServiceEventHud Create(Transform parent, Font font)
        {
            var go = new GameObject("ShopServiceEventHud", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            // Trải kín màn — mỗi nút con tự neo theo tỉ lệ, bám phải-trên.
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var view = go.AddComponent<ShopServiceEventHud>();

            // Xếp phải → trái: minimap (ô trống), Hộp thư, Sự kiện, Dịch vụ, Cửa hàng, Chợ trời.
            // Bỏ Bang hội theo yêu cầu user.
            var right = ReservedRightFrac + MinimapSlotFrac + GapFrac;
            var mail = MakeButton(go.transform, font, HudSkin.Mail, "Hộp thư", right,
                () => view.MailClicked?.Invoke());
            view._mailBadge = UnreadCountBadge.Attach(mail, font);
            right += SizeFrac + GapFrac;

            MakeButton(go.transform, font, HudSkin.Event, "Sự kiện", right,
                () => view.EventClicked?.Invoke());
            right += SizeFrac + GapFrac;

            MakeButton(go.transform, font, HudSkin.Service, "Dịch vụ", right,
                () => view.ServiceClicked?.Invoke());
            right += SizeFrac + GapFrac;

            MakeButton(go.transform, font, HudSkin.Shop, "Cửa hàng", right,
                () => view.ShopClicked?.Invoke());
            right += SizeFrac + GapFrac;

            MakeButton(go.transform, font, HudSkin.Market, "Chợ trời", right,
                () => view.MarketClicked?.Invoke());

            return view;
        }

        /// <summary>Số thư chưa đọc trên icon hộp thư; 0 là ẩn huy hiệu.</summary>
        public void SetMailCount(int count) => _mailBadge?.SetCount(count);

        private static Transform MakeButton(Transform parent, Font font, string spriteName,
            string label, float rightOffset, Action onClick)
        {
            var rect = MakeIconButton(parent, font, spriteName, label, onClick);
            rect.anchorMin = new Vector2(1f - rightOffset - SizeFrac, 1f - TopMarginFrac - SizeFrac);
            rect.anchorMax = new Vector2(1f - rightOffset, 1f - TopMarginFrac);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        /// <summary>
        /// Dựng một nút HUD (icon + nhãn chữ trắng viền đen thò dưới đáy) — CHƯA neo vị
        /// trí; người gọi tự đặt anchor. Dùng chung cho hàng nút góc phải và nút Pet dưới
        /// minimap để mọi icon HUD cùng một kiểu.
        /// </summary>
        internal static RectTransform MakeIconButton(Transform parent, Font font, string spriteName,
            string label, Action onClick)
        {
            var go = new GameObject($"Hud_{spriteName}", typeof(RectTransform),
                typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var icon = go.GetComponent<Image>();
            icon.preserveAspect = true;
            var sprite = HudSkin.Get(spriteName);
            if (sprite != null)
            {
                icon.sprite = sprite;
                icon.color = Color.white;
            }
            else
            {
                // Không có sprite: giữ Image cho raycast nhưng cho nền xanh mờ +
                // đặt chữ CHÍNH GIỮA nút. Không thì Image trong suốt = không bấm được.
                icon.color = UiBuilder.ButtonFace;
            }

            // Nhãn NHỎ HƠN tên nhân vật (CharacterHud dùng 15) một chút, chữ TRẮNG
            // + viền ĐEN (Outline). Đọc được cả khi nút phủ lên nền sáng lẫn nền tối.
            var text = UiBuilder.MakeText(go.transform, font, "Label", 10, stretch: true);
            text.alignment = TextAnchor.LowerCenter;
            text.text = label;
            text.color = Color.white;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1f, -1f);

            // Nhãn SÁT LÊN icon — mép trên nhãn chạm gần đáy icon (anchor y=0.1 vào
            // trong nút một chút), không còn khoảng trống thừa. Vẫn mở rộng ngang
            // ngoài rìa nút để chữ dài (Cửa hàng) không bị cắt.
            var labelRect = text.rectTransform;
            labelRect.anchorMin = new Vector2(-0.2f, -LabelBottomFrac);
            labelRect.anchorMax = new Vector2(1.2f, 0.1f);
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
            return (RectTransform)go.transform;
        }
    }
}
