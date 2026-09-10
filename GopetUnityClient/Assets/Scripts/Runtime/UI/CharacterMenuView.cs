using System;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Panel trung tâm màn hình liệt kê 12 mục menu char (xem <see cref="CharacterMenu"/>).
    ///
    /// <para>Không dùng <see cref="GenericMenuView"/> vì mục ở đây là danh sách CỐ ĐỊNH
    /// client-side, không phải <c>MenuScreen</c> server bơm xuống. Server-side menu
    /// (chọn xong sẽ nhận response) sẽ dùng <see cref="GenericMenuView"/> ở Phase 4.</para>
    /// </summary>
    public sealed class CharacterMenuView : MonoBehaviour
    {
        private const float PanelWidth = 520f;
        private const float ItemHeight = 34f;
        private const float PanelPadding = 8f;
        private const float TitleHeight = 24f;

        public event Action<CharacterMenuAction> ItemSelected;
        public event Action CloseRequested;

        public static CharacterMenuView Create(Transform parent)
        {
            var backdrop = new GameObject("Character Menu Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            var backdropImg = backdrop.GetComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.4f);
            backdropImg.raycastTarget = true;

            var view = backdrop.AddComponent<CharacterMenuView>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var panelRect = (RectTransform)panel.transform;
            var itemCount = CharacterMenu.Entries.Count;
            var rowCount = (itemCount + 1) / 2;
            var panelHeight = TitleHeight + ItemHeight * rowCount + PanelPadding * 2;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(PanelWidth, panelHeight);
            var panelImg = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(panelImg);
            panelImg.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);
            panelImg.raycastTarget = true;   // ăn click, không cho backdrop nhận

            view.BuildContent(panel.transform);
            return view;
        }

        private void BuildContent(Transform panel)
        {
            var font = UiBuilder.BuiltinFont();

            var title = UiBuilder.MakeText(panel, font, "Title", 15, false);
            title.text = "Menu";
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UiBuilder.TextMain;
            SetTopRect(title.rectTransform, PanelPadding, TitleHeight);

            for (var i = 0; i < CharacterMenu.Entries.Count; i++)
            {
                var row = i / 2;
                var column = i % 2;
                MakeRow(panel, font, CharacterMenu.Entries[i],
                    PanelPadding + TitleHeight + row * ItemHeight, column);
            }
        }

        private void MakeRow(Transform panel, Font font, CharacterMenuEntry entry, float top, int column)
        {
            var go = new GameObject($"Row:{entry.Action}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(column == 0 ? 0f : 0.5f, 1f);
            rect.anchorMax = new Vector2(column == 0 ? 0.5f : 1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(column == 0 ? PanelPadding : 2f, -(top + ItemHeight - 2f));
            rect.offsetMax = new Vector2(column == 0 ? -2f : -PanelPadding, -top);

            var img = go.GetComponent<Image>();
            img.color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(img);

            var label = UiBuilder.MakeText(go.transform, font, "Label", 14, false);
            label.text = entry.Label;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = UiBuilder.TextMain;
            UiBuilder.Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(12f, 0f);
            label.rectTransform.offsetMax = new Vector2(-8f, 0f);

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                ItemSelected?.Invoke(entry.Action);
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
