using System;
using System.Collections.Generic;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Menu HUD phân nhóm, hỗ trợ trang con và nút quay lại.</summary>
    public sealed class CharacterMenuView : MonoBehaviour
    {
        private const float PanelWidth = 520f;
        private const float ItemHeight = 42f;
        private const float PanelPadding = 10f;
        private const float TitleHeight = 34f;
        private const float EmptyHeight = 64f;

        private readonly Stack<CharacterMenuPage> _history = new Stack<CharacterMenuPage>();
        private RectTransform _panel;
        private Transform _content;
        private Font _font;
        private CharacterMenuPage _page;

        public event Action<CharacterMenuAction> ItemSelected;
        public event Action CloseRequested;

        public CharacterMenuPage Page => _page;

        public static CharacterMenuView Create(Transform parent,
            CharacterMenuPage initialPage = CharacterMenuPage.Main)
        {
            var backdrop = new GameObject("Character Menu Backdrop", typeof(RectTransform),
                typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            var backdropImg = backdrop.GetComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.4f);
            backdropImg.raycastTarget = true;

            var view = backdrop.AddComponent<CharacterMenuView>();
            view._font = UiBuilder.BuiltinFont();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            view._panel = (RectTransform)panel.transform;
            view._panel.anchorMin = view._panel.anchorMax = new Vector2(0.5f, 0.5f);
            view._panel.pivot = new Vector2(0.5f, 0.5f);
            var panelImg = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(panelImg);
            panelImg.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);
            panelImg.raycastTarget = true;

            view._content = new GameObject("Content", typeof(RectTransform)).transform;
            view._content.SetParent(panel.transform, false);
            UiBuilder.Stretch((RectTransform)view._content);
            view.ShowPage(initialPage, false);
            return view;
        }

        public void Back()
        {
            if (_history.Count == 0)
            {
                CloseRequested?.Invoke();
                return;
            }

            ShowPage(_history.Pop(), false);
        }

        private void ShowPage(CharacterMenuPage page, bool rememberCurrent)
        {
            if (rememberCurrent) _history.Push(_page);
            _page = page;

            for (var i = _content.childCount - 1; i >= 0; i--)
                Destroy(_content.GetChild(i).gameObject);

            var nodes = CharacterMenu.GetNodes(page);
            var rowCount = Mathf.Max(1, (nodes.Count + 1) / 2);
            var bodyHeight = nodes.Count == 0 ? EmptyHeight : ItemHeight * rowCount;
            _panel.sizeDelta = new Vector2(PanelWidth,
                PanelPadding * 2 + TitleHeight + bodyHeight);

            BuildHeader(page);
            if (nodes.Count == 0)
            {
                BuildEmptyState();
                return;
            }

            for (var i = 0; i < nodes.Count; i++)
                MakeRow(nodes[i], PanelPadding + TitleHeight + (i / 2) * ItemHeight, i % 2);
        }

        private void BuildHeader(CharacterMenuPage page)
        {
            if (_history.Count > 0)
            {
                var back = MakeHeaderButton("Back", "‹", TextAnchor.MiddleCenter);
                var rect = (RectTransform)back.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(PanelPadding, -PanelPadding);
                rect.sizeDelta = new Vector2(38f, TitleHeight);
                back.GetComponent<Button>().onClick.AddListener(Back);
            }

            var title = UiBuilder.MakeText(_content, _font, "Title", 16, false);
            title.text = CharacterMenu.GetTitle(page);
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UiBuilder.TextMain;
            SetTopRect(title.rectTransform, PanelPadding, TitleHeight);
        }

        private void BuildEmptyState()
        {
            var empty = UiBuilder.MakeText(_content, _font, "Empty", 14, false);
            empty.text = "Chưa có sự kiện đang diễn ra";
            empty.alignment = TextAnchor.MiddleCenter;
            empty.color = new Color(0.75f, 0.8f, 0.88f, 1f);
            SetTopRect(empty.rectTransform, PanelPadding + TitleHeight, EmptyHeight);
        }

        private GameObject MakeHeaderButton(string name, string label, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_content, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.18f, 0.25f, 0.38f, 1f);
            RoundedUiSprite.Apply(img);
            var text = UiBuilder.MakeText(go.transform, _font, "Label", 24, true);
            text.text = label;
            text.alignment = alignment;
            text.color = UiBuilder.TextMain;
            return go;
        }

        private void MakeRow(CharacterMenuNode node, float top, int column)
        {
            var suffix = node.TargetPage.HasValue ? node.TargetPage.Value.ToString() : node.Action.ToString();
            var go = new GameObject($"Row:{suffix}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(column == 0 ? 0f : 0.5f, 1f);
            rect.anchorMax = new Vector2(column == 0 ? 0.5f : 1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(column == 0 ? PanelPadding : 3f, -(top + ItemHeight - 3f));
            rect.offsetMax = new Vector2(column == 0 ? -3f : -PanelPadding, -top);

            var img = go.GetComponent<Image>();
            img.color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(img);

            var label = UiBuilder.MakeText(go.transform, _font, "Label", 14, false);
            label.text = node.TargetPage.HasValue ? node.Label + "  ›" : node.Label;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = UiBuilder.TextMain;
            UiBuilder.Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(12f, 0f);
            label.rectTransform.offsetMax = new Vector2(-8f, 0f);

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (node.TargetPage.HasValue) ShowPage(node.TargetPage.Value, true);
                else if (node.Action.HasValue) ItemSelected?.Invoke(node.Action.Value);
            });
        }

        private static void SetTopRect(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(PanelPadding, -(top + height));
            rect.offsetMax = new Vector2(-PanelPadding, -top);
        }
    }
}
