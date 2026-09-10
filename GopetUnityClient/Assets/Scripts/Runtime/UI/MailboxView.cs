using System;
using Gopet.Net.Social;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hộp thư đơn giản: tiêu đề + short content mỗi thư, tap → mở toàn văn.
    /// Chưa hỗ trợ mark/remove — để sau (Phase 4b).
    /// </summary>
    public sealed class MailboxView : MonoBehaviour
    {
        private const float PanelWidth = 480f;
        private const float PanelHeight = 360f;
        private const float RowHeight = 48f;
        private const float Padding = 8f;

        public event Action CloseRequested;

        public static MailboxView Create(Transform parent, Mailbox mailbox)
        {
            var backdrop = new GameObject("Mailbox Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var view = backdrop.AddComponent<MailboxView>();
            backdrop.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            var img = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.11f, 0.14f, 0.2f, 0.98f);

            view.BuildContent(panel.transform, mailbox);
            return view;
        }

        private void BuildContent(Transform panel, Mailbox mailbox)
        {
            var font = UiBuilder.BuiltinFont();

            var title = UiBuilder.MakeText(panel, font, "Title", 16, false);
            title.text = mailbox.Letters.Length == 0
                ? "Hộp thư (trống)"
                : $"Hộp thư ({mailbox.Letters.Length} thư)";
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UiBuilder.TextMain;
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(Padding, -(Padding + 24f));
            titleRect.offsetMax = new Vector2(-Padding, -Padding);

            for (var i = 0; i < mailbox.Letters.Length; i++)
            {
                MakeRow(panel, font, mailbox.Letters[i], i);
            }
        }

        private static void MakeRow(Transform panel, Font font, Letter letter, int index)
        {
            var go = new GameObject($"Letter:{letter.LetterId}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            var top = 32f + Padding + (RowHeight + 4f) * index;
            rect.offsetMin = new Vector2(Padding, -(top + RowHeight));
            rect.offsetMax = new Vector2(-Padding, -top);
            var img = go.GetComponent<Image>();
            img.color = letter.IsMark ? new Color(0.32f, 0.28f, 0.18f, 1f) : UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(img);

            var titleText = UiBuilder.MakeText(go.transform, font, "Title", 13, false);
            titleText.text = string.IsNullOrEmpty(letter.Title) ? "(không tiêu đề)" : letter.Title;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.UpperLeft;
            titleText.color = UiBuilder.TextMain;
            var tRect = titleText.rectTransform;
            tRect.anchorMin = new Vector2(0f, 0f);
            tRect.anchorMax = new Vector2(1f, 1f);
            tRect.offsetMin = new Vector2(12f, 4f);
            tRect.offsetMax = new Vector2(-12f, -4f);

            var body = UiBuilder.MakeText(go.transform, font, "Body", 11, false);
            body.text = letter.ShortContent ?? string.Empty;
            body.alignment = TextAnchor.LowerLeft;
            body.color = UiBuilder.TextMuted;
            var bRect = body.rectTransform;
            bRect.anchorMin = new Vector2(0f, 0f);
            bRect.anchorMax = new Vector2(1f, 1f);
            bRect.offsetMin = new Vector2(12f, 4f);
            bRect.offsetMax = new Vector2(-12f, -20f);
        }
    }
}
