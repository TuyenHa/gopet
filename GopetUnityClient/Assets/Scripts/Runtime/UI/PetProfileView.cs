using System;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Màn thông tin pet dùng chung cho pet bản thân và pet người chơi khác — dựng trên
    /// <see cref="GamePopupFrame"/> (tiêu đề xanh, nút X, nền sáng) như mọi popup khác.
    ///
    /// <para>Tên pet vẽ bằng <see cref="StarNameLabel"/>: server gửi "Rua Test (sao)(saoden)…",
    /// tag sao thành icon sao vàng thay vì hiện chữ thô "(saoden)".</para>
    /// </summary>
    public sealed partial class PetProfileView : MonoBehaviour
    {
        private const float Width = 470f;
        private const float Height = 390f;
        private const float Pad = 10f;

        public event Action CloseRequested;
        public event Action GymRequested;
        public event Action TattooRequested;

        /// <summary>Xin mở menu chọn kỹ năng. Tham số là <c>PetProfilePackets.LearnNewSlot</c>
        /// khi học vào ô trống, hoặc id kỹ năng đang có khi muốn THAY chính nó.</summary>
        public event Action<int> LearnSkillRequested;

        private Font _font;

        public static PetProfileView Create(Transform parent, PetProfile profile,
            RemoteAssetCache assets, bool editable)
        {
            // Nền tối phủ màn hình: chạm ra ngoài popup là đóng, như bản cũ.
            var root = new GameObject("Pet Profile", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var view = root.AddComponent<PetProfileView>();
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());
            view._font = UiBuilder.DefaultFont();
            view.Build(profile ?? new PetProfile(), editable);
            return view;
        }

        private void Build(PetProfile value, bool editable)
        {
            var frame = GamePopupFrame.Create(transform, _font, "Thú cưng", Width, Height);
            frame.Closed += () => CloseRequested?.Invoke();

            // Khung trắng chứa thông tin; chừa đáy cho hàng nút khi là pet của mình.
            var bottom = editable ? PopupButtonRow.Height + 8f : 0f;
            var info = new GameObject("Info", typeof(RectTransform));
            info.transform.SetParent(frame.Content, false);
            var infoRect = (RectTransform)info.transform;
            UiBuilder.Stretch(infoRect);
            infoRect.offsetMin = new Vector2(0f, bottom);
            RoundedBorder.Apply(info, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg, PopupPalette.Hairline);

            var name = StarNameLabel.Create(info.transform, _font, 16, 14f);
            UiBuilder.PlaceRow(name.Rect, 8f, 22f, Pad);
            name.Label.color = PopupPalette.TextDark;
            UiBuilder.SetFontStyle(name.Label, FontStyle.Bold);
            name.SetName(value.Name ?? "Pet", value.Level > 0 ? $" - LV.{value.Level}" : null);

            var stats = MakeText(info.transform, "Details", 12, PopupPalette.TextDark);
            UiBuilder.PlaceRow(stats.rectTransform, 34f, StatsHeight, Pad);
            stats.alignment = TextAnchor.UpperLeft;
            stats.text = Describe(value);

            BuildSkillRows(info.transform, value, editable);
            if (editable) BuildActions(frame.Content);
        }

        /// <summary>Ba nút thao tác pet ở chân — cùng cỡ, bo góc, màu nút chính của popup.</summary>
        private void BuildActions(Transform content)
        {
            var row = new GameObject("Actions", typeof(RectTransform));
            row.transform.SetParent(content, false);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, PopupButtonRow.Height);

            AddAction(rect, 0, "Cộng tiềm năng", () => GymRequested?.Invoke());
            AddAction(rect, 1, "Hình xăm", () => TattooRequested?.Invoke());
            AddAction(rect, 2, "Học kỹ năng", () => LearnSkillRequested?.Invoke(PetProfilePackets.LearnNewSlot));
        }

        private void AddAction(RectTransform row, int index, string label, Action onClick)
        {
            const float gap = 6f;
            var button = MakeButton(row, label, 13, onClick);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(index / 3f, 0f);
            rect.anchorMax = new Vector2((index + 1) / 3f, 1f);
            rect.offsetMin = new Vector2(index == 0 ? 0f : gap * 0.5f, 0f);
            rect.offsetMax = new Vector2(index == 2 ? 0f : -gap * 0.5f, 0f);
        }

        private Button MakeButton(Transform parent, string label, int fontSize, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 6f);
            image.color = PopupPalette.ButtonBlue;
            var text = UiBuilder.MakeText(go.transform, _font, "Label", fontSize, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => onClick?.Invoke());
            return button;
        }

        private Text MakeText(Transform parent, string name, int size, Color color)
        {
            var text = UiBuilder.MakeText(parent, _font, name, size, false);
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }
    }
}
