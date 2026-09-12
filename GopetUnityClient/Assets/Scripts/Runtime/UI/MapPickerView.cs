using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Danh sách map server cho phép người chơi dịch chuyển tới.</summary>
    public sealed class MapPickerView : MonoBehaviour
    {
        private Font _font;
        private RectTransform _content;
        private Text _title;
        private bool _decided;

        public event Action<int> Chosen;
        public event Action CloseRequested;

        public static MapPickerView Create(Transform parent, Font font)
        {
            var root = new GameObject("MapPickerView", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.58f);
            var view = root.AddComponent<MapPickerView>();
            view._font = font;
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());
            view.Build();
            return view;
        }

        public void Bind(string title, IReadOnlyList<string> labels, IReadOnlyList<bool> disabled = null)
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            _decided = false;
            _title.text = title;
            for (var i = _content.childCount - 1; i >= 0; i--) Destroy(_content.GetChild(i).gameObject);
            for (var i = 0; i < labels.Count; i++)
                MakeOption(labels[i], i, disabled == null || i >= disabled.Count || !disabled[i]);
        }

        private void Build()
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.08f);
            panelRect.anchorMax = new Vector2(0.5f, 0.92f);
            panelRect.sizeDelta = new Vector2(440f, 0f);
            var panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.07f, 0.11f, 0.17f, 0.99f);
            RoundedUiSprite.Apply(panelImage);

            _title = UiBuilder.MakeText(panel.transform, _font, "Title", 20, false);
            _title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(_title.rectTransform, 12f, 36f, 16f);

            var scroll = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.transform.SetParent(panel.transform, false);
            var scrollRect = (RectTransform)scroll.transform;
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(16f, 58f);
            scrollRect.offsetMax = new Vector2(-16f, -56f);
            scroll.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.2f);
            scroll.GetComponent<Image>().raycastTarget = true;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scroll.transform, false);
            UiBuilder.Stretch((RectTransform)viewport.transform);
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            _content = (RectTransform)content.transform;
            _content.anchorMin = new Vector2(0f, 1f); _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0.5f, 1f); _content.sizeDelta = Vector2.zero;
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f; layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlHeight = false; layout.childForceExpandWidth = true;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var sr = scroll.GetComponent<ScrollRect>();
            sr.viewport = (RectTransform)viewport.transform; sr.content = _content;
            sr.horizontal = false; sr.movementType = ScrollRect.MovementType.Clamped;

            var close = MakeButton(panel.transform, "Đóng", 42f);
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = new Vector2(0.5f, 0f); closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0f); closeRect.anchoredPosition = new Vector2(0f, 10f);
            closeRect.sizeDelta = new Vector2(150f, 42f);
            close.onClick.AddListener(() => CloseRequested?.Invoke());
        }

        private void MakeOption(string label, int index, bool enabled)
        {
            var button = MakeButton(_content, label, 58f);
            button.interactable = enabled;
            button.onClick.AddListener(() => Choose(index));
        }

        private Button MakeButton(Transform parent, string label, float height)
        {
            var go = new GameObject("MapOption", typeof(RectTransform), typeof(Image), typeof(Button),
                typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var image = go.GetComponent<Image>();
            image.color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(image);
            var text = UiBuilder.MakeText(go.transform, _font, "Label", 16, true);
            text.text = label; text.alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }

        private void Choose(int index)
        {
            if (_decided) return;
            _decided = true;
            Chosen?.Invoke(index);
        }
    }
}
