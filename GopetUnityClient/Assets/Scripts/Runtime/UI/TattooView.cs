using System;
using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup "Hình xăm pet": khung <see cref="GamePopupFrame"/> như Cửa hàng, danh sách ô
    /// xăm trong <see cref="PopupItemList"/> (các dòng ngăn bằng vạch kẻ ngang) và nút
    /// "Tạo hình xăm" xanh dương ở chân. Dòng ô xăm dựng ở <c>TattooView.Rows.cs</c>.
    /// </summary>
    public sealed partial class TattooView : MonoBehaviour
    {
        private const string Title = "Hình xăm pet";
        private const float PopupWidth = 440f;
        private const float PopupHeight = 330f;
        private const float GenerateWidth = 180f;
        private const float GenerateHeight = 34f;
        private const float GenerateGap = 8f;

        private Font _font;

        public event Action CloseRequested;
        public event Action GenerateRequested;
        public event Action<int> RemoveRequested;
        public event Action<int> EnchantRequested;

        public static TattooView Create(Transform parent, TattooScreen screen)
        {
            var root = new GameObject("Tattoo", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            var view = root.AddComponent<TattooView>();
            view._font = UiBuilder.DefaultFont();

            // Lớp tối là ANH EM với khung: Unity dò handler click ngược lên cây cha, nên
            // nút đóng mà nằm ở cha thì bấm đâu trong popup cũng đóng.
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
            dim.transform.SetParent(root.transform, false);
            UiBuilder.Stretch((RectTransform)dim.transform);
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            dim.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var frame = GamePopupFrame.Create(root.transform, view._font, Title, PopupWidth, PopupHeight);
            frame.Closed += () => view.CloseRequested?.Invoke();
            view.BuildList(frame.Content, screen?.Slots ?? Array.Empty<TattooSlot>());
            view.BuildGenerateButton(frame.Content);
            return view;
        }

        /// <summary>Bản nhúng (tab Pet của Hành lý): danh sách ô xăm + nút tạo, không khung/nền mờ.</summary>
        public static TattooView CreateEmbedded(Transform host, TattooScreen screen)
        {
            var root = new GameObject("TattooEmbedded", typeof(RectTransform));
            root.transform.SetParent(host, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            var view = root.AddComponent<TattooView>();
            view._font = UiBuilder.DefaultFont();
            view.BuildList(root.transform, screen?.Slots ?? Array.Empty<TattooSlot>());
            view.BuildGenerateButton(root.transform);
            return view;
        }

        private void BuildList(Transform content, TattooSlot[] slots)
        {
            var area = new GameObject("ListArea", typeof(RectTransform));
            area.transform.SetParent(content, false);
            var rect = (RectTransform)area.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0f, GenerateHeight + GenerateGap);
            rect.offsetMax = Vector2.zero;

            var list = PopupItemList.Create(area.transform, _font);
            for (var i = 0; i < slots.Length; i++)
                BuildRow(list.Rows, slots[i], i, i < slots.Length - 1);
            list.SetRowsHeight(slots.Length * RowHeight);
            list.ShowPlaceholder(slots.Length == 0 ? "Pet chưa có ô xăm nào." : null);
        }

        /// <summary>Nút chính ở chân popup: nền xanh dương, viền sẫm hơn một nấc.</summary>
        private void BuildGenerateButton(Transform content)
        {
            var button = MakePillButton(content, "Tạo hình xăm", 15, PopupPalette.ButtonBlue,
                PopupPalette.HeaderBlue, () => GenerateRequested?.Invoke());
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(GenerateWidth, GenerateHeight);
            rect.anchoredPosition = Vector2.zero;
        }

        /// <summary>Nút viên thuốc: viền 2 đơn vị màu <paramref name="edge"/>, ruột <paramref name="fill"/>, chữ trắng đậm.</summary>
        private Button MakePillButton(Transform parent, string label, int fontSize, Color fill,
            Color edge, Action action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RoundedBorder.Apply(go, RoundedUiSprite.DefaultRadius, fill, edge, 2f);

            var text = UiBuilder.MakeText(go.transform, _font, "Label", fontSize, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => action());
            return button;
        }
    }
}
