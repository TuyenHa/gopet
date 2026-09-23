using System;
using System.Text;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Tổng kết PvE giữ tới khi bấm OK; nút nằm ngoài phần nội dung cuộn.</summary>
    public sealed class BattleVictoryPopup : MonoBehaviour
    {
        private RectTransform _panel;
        private Button _ok;
        private Action _onOk;
        private bool _confirmed;

        public static BattleVictoryPopup Create(Transform parent, BattleSummarySnapshot summary,
            Action onOk)
        {
            var root = new GameObject("Kết quả thắng quái", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0.01f, 0.04f, 0.08f, 0.78f);
            root.GetComponent<Image>().raycastTarget = true;
            var view = root.AddComponent<BattleVictoryPopup>();
            view._onOk = onOk;
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(Outline));
            panel.transform.SetParent(root.transform, false);
            view._panel = (RectTransform)panel.transform;
            view._panel.anchorMin = view._panel.anchorMax = new Vector2(0.5f, 0.5f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());
            panel.GetComponent<Image>().color = new Color(0.06f, 0.12f, 0.19f, 1f);
            panel.GetComponent<Outline>().effectColor = new Color(0.85f, 0.66f, 0.27f);
            panel.GetComponent<Outline>().effectDistance = new Vector2(2, -2);
            view.Resize();

            var title = Label(panel.transform, "Title", "CHIẾN THẮNG", 28);
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(1f, 0.83f, 0.35f);
            title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(title.rectTransform, 12, 42, 20);
            BuildScroll(panel.transform, summary);
            view.BuildOk();
            return view;
        }

        private void LateUpdate() => Resize();

        private void Resize()
        {
            if (_panel == null) return;
            var rect = ((RectTransform)transform).rect;
            _panel.sizeDelta = new Vector2(Mathf.Min(560, Mathf.Max(1, rect.width - 32)),
                Mathf.Min(480, Mathf.Max(1, rect.height - 32)));
        }

        private static void BuildScroll(Transform parent, BattleSummarySnapshot summary)
        {
            var go = new GameObject("Nội dung cuộn", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            UiBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(22, 76);
            rect.offsetMax = new Vector2(-22, -66);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.12f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(go.transform, false);
            var viewportRect = (RectTransform)viewport.transform;
            UiBuilder.Stretch(viewportRect);
            var details = Label(viewport.transform, "Details", FormatDetails(summary), 19);
            details.rectTransform.anchorMin = new Vector2(0, 1);
            details.rectTransform.anchorMax = Vector2.one;
            details.rectTransform.pivot = new Vector2(0.5f, 1);
            details.rectTransform.sizeDelta = new Vector2(-16, 0);
            details.rectTransform.anchoredPosition = Vector2.zero;
            details.alignment = TextAnchor.UpperLeft;
            details.lineSpacing = 1.2f;
            details.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;
            var scroll = go.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = details.rectTransform;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28;
        }

        private void BuildOk()
        {
            var go = new GameObject("OK", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.anchoredPosition = new Vector2(0, 18);
            rect.sizeDelta = new Vector2(160, 42);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var label = Label(go.transform, "Label", "OK", 21);
            UiBuilder.Stretch(label.rectTransform);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            _ok = go.GetComponent<Button>();
            _ok.onClick.AddListener(() =>
            {
                if (_confirmed) return;
                _confirmed = true;
                _ok.interactable = false;
                _onOk?.Invoke();
            });
        }

        private static Text Label(Transform parent, string name, string value, int size)
        {
            var label = UiBuilder.MakeText(parent, UiBuilder.BuiltinFont(), name, size, false);
            label.text = value;
            label.supportRichText = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.raycastTarget = false;
            return label;
        }

        private static string FormatDetails(BattleSummarySnapshot s)
        {
            var text = new StringBuilder().Append("Đã đánh bại: ").AppendLine(s.OpponentName)
                .Append("Thời gian: ").AppendLine(s.DurationText)
                .AppendLine().AppendLine("Phần thưởng")
                .Append("Ngọc: ").AppendLine(s.Coin.ToString())
                .Append("EXP kết thúc trận: ").AppendLine(s.Experience.ToString());
            if (s.RewardLines.Count == 0) text.AppendLine("Không nhận được vật phẩm");
            foreach (var reward in s.RewardLines) text.AppendLine(reward);
            text.AppendLine().AppendLine("Kỹ năng đã sử dụng");
            if (s.SkillNames.Count == 0) text.AppendLine("Không sử dụng kỹ năng");
            foreach (var skill in s.SkillNames) text.AppendLine(skill);
            return text.ToString();
        }
    }
}
