using System;
using Gopet.Net.Social;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class LetterDetailView : MonoBehaviour
    {
        public event Action CloseRequested;
        public event Action<int> MarkRequested;
        public event Action<int> RemoveRequested;

        public static LetterDetailView Create(Transform parent, Letter letter)
        {
            var root = Overlay(parent, "Letter Detail");
            var view = root.AddComponent<LetterDetailView>();
            var panel = Panel(root.transform, new Vector2(440f, 330f));
            var title = Text(panel, "Title", letter.Title, 16, 18f, 40f);
            title.fontStyle = FontStyle.Bold;
            var body = Text(panel, "Content", letter.Content, 14, 66f, 190f);
            body.alignment = TextAnchor.UpperLeft;
            Button(panel, "Đánh dấu", 18f, () => view.MarkRequested?.Invoke(letter.LetterId));
            Button(panel, "Xoá", 158f, () => view.RemoveRequested?.Invoke(letter.LetterId));
            Button(panel, "Đóng", 298f, () => view.CloseRequested?.Invoke());
            return view;
        }

        internal static GameObject Overlay(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, .65f);
            return go;
        }

        internal static Transform Panel(Transform parent, Vector2 size)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(.1f, .14f, .2f, .98f);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            return go.transform;
        }

        internal static Text Text(Transform parent, string name, string value, int size, float top, float height)
        {
            var text = UiBuilder.MakeText(parent, UiBuilder.BuiltinFont(), name, size, false);
            text.text = value ?? string.Empty;
            UiBuilder.PlaceRow(text.rectTransform, top, height, 18f);
            text.color = UiBuilder.TextMain;
            return text;
        }

        internal static void Button(Transform parent, string label, float left, Action action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(left, 18f);
            rect.sizeDelta = new Vector2(124f, 40f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var text = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            go.GetComponent<Button>().onClick.AddListener(() => action());
        }
    }
}
