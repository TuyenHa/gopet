using System;
using Gopet.Net.Social;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class MailboxView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action<Letter> LetterSelected;
        public event Action ComposeRequested;

        public static MailboxView Create(Transform parent, Mailbox mailbox)
        {
            var root = new GameObject("Mailbox", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, .5f);
            var view = root.AddComponent<MailboxView>();
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());
            view.Build(mailbox);
            return view;
        }

        /// <summary>Loại thư của server (<c>Data/User/Letter.cs:13-15</c>). 0 là "tất cả",
        /// không phải một loại thật — jar cũng chia đúng ba nhóm này.</summary>
        private const sbyte TypeAll = 0, TypeFriend = 1, TypeAdmin = 2, TypeEvent = 3;

        private Mailbox _mailbox;
        private Transform _listPanel;
        private Text _title;
        private sbyte _filter = TypeAll;

        private void Build(Mailbox mailbox)
        {
            _mailbox = mailbox;
            _listPanel = LetterDetailView.Panel(transform, new Vector2(480f, 380f));
            _title = LetterDetailView.Text(_listPanel, "Title", string.Empty, 18, 10f, 34f);
            _title.alignment = TextAnchor.MiddleCenter;
            MakeTopButton(_listPanel, "Soạn thư", () => ComposeRequested?.Invoke(), 12f);
            MakeTopButton(_listPanel, "Tất cả", () => SetFilter(TypeAll), 132f, 68f);
            MakeTopButton(_listPanel, "Admin", () => SetFilter(TypeAdmin), 206f, 68f);
            MakeTopButton(_listPanel, "Sự kiện", () => SetFilter(TypeEvent), 280f, 68f);
            MakeTopButton(_listPanel, "Bạn bè", () => SetFilter(TypeFriend), 354f, 68f);
            Rebuild();
        }

        private void SetFilter(sbyte type)
        {
            if (_filter == type) return;
            _filter = type;
            Rebuild();
        }

        /// <summary>Dựng lại danh sách theo bộ lọc. Xoá cả vùng cuộn cũ chứ không chỉ các dòng:
        /// chiều cao vùng cuộn tính theo SỐ dòng, giữ lại là cuộn hụt hoặc thừa.</summary>
        private void Rebuild()
        {
            var old = _listPanel.Find("Letters");
            if (old != null) Destroy(old.gameObject);

            var letters = Filtered();
            _title.text = _filter == TypeAll
                ? $"Hộp thư ({letters.Count})"
                : $"Hộp thư ({letters.Count}/{_mailbox.Letters.Length})";
            var content = MakeScrollArea(_listPanel, letters.Count);
            for (var i = 0; i < letters.Count; i++) MakeRow(content, letters[i], i * 52f);
        }

        private System.Collections.Generic.List<Letter> Filtered()
        {
            var result = new System.Collections.Generic.List<Letter>();
            foreach (var letter in _mailbox.Letters)
                if (_filter == TypeAll || letter.Type == _filter) result.Add(letter);
            return result;
        }

        private static Transform MakeScrollArea(Transform panel, int count)
        {
            var viewport = new GameObject("Letters", typeof(RectTransform), typeof(Image),
                typeof(Mask), typeof(ScrollRect));
            viewport.transform.SetParent(panel, false);
            var rect = (RectTransform)viewport.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(0f, 10f);
            rect.offsetMax = new Vector2(0f, -84f);
            viewport.GetComponent<Image>().color = new Color(0f, 0f, 0f, .01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, Mathf.Max(rect.rect.height, count * 52f));

            var scroll = viewport.GetComponent<ScrollRect>();
            scroll.viewport = rect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            return content.transform;
        }

        private void MakeRow(Transform panel, Letter letter, float top)
        {
            var go = new GameObject($"Letter:{letter.LetterId}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, top, 46f, 12f);
            go.GetComponent<Image>().color = letter.IsMark ? new Color(.32f, .28f, .18f, 1f) : UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 13, true);
            text.text = $"{letter.Title}\n{letter.ShortContent}";
            text.alignment = TextAnchor.MiddleLeft;
            text.rectTransform.offsetMin = new Vector2(10f, 2f);
            text.rectTransform.offsetMax = new Vector2(-10f, -2f);
            go.GetComponent<Button>().onClick.AddListener(() => LetterSelected?.Invoke(letter));
        }

        private static void MakeTopButton(Transform parent, string label, Action action,
            float x, float width = 112f)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -48f);
            rect.sizeDelta = new Vector2(width, 32f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            go.GetComponent<Button>().onClick.AddListener(() => action());
        }
    }
}
