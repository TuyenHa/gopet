using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Ba tab chat luôn nhìn thấy: Khu vực, Thế giới và Bang hội.</summary>
    public sealed class ChatChannelSelector : MonoBehaviour
    {
        public enum Channel { Place, Community, Guild }

        private static readonly Color Selected = new Color(0.82f, 0.57f, 0.08f, 1f);
        private static readonly Color Idle = new Color(0.07f, 0.13f, 0.17f, 0.9f);

        public event Action<Channel> Changed;
        public Channel Current { get; private set; } = Channel.Place;

        private Image _placeBackground;
        private Image _worldBackground;
        private Image _guildBackground;
        private GameObject _guildTab;
        private bool _guildAvailable;

        public static ChatChannelSelector Create(Transform parent)
        {
            var root = new GameObject("Chat Channels", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(5f, -4f);
            rect.sizeDelta = new Vector2(320f, 34f);

            var selector = root.AddComponent<ChatChannelSelector>();
            selector._placeBackground = MakeTab(root.transform, "Place", "Khu vực", 0f,
                () => selector.SetChannel(Channel.Place));
            selector._worldBackground = MakeTab(root.transform, "World", "Thế giới", 106f,
                () => selector.SetChannel(Channel.Community));
            selector._guildBackground = MakeTab(root.transform, "Guild", "Bang hội", 212f,
                () => selector.SetChannel(Channel.Guild));
            selector._guildTab = selector._guildBackground.gameObject;
            selector.Apply();
            return selector;
        }

        public void SetGuildAvailable(bool available)
        {
            _guildAvailable = available;
            if (_guildTab != null) _guildTab.SetActive(available);
            if (!available && Current == Channel.Guild) SetChannel(Channel.Place);
        }

        public void SetChannel(Channel channel)
        {
            if (channel == Channel.Guild && !_guildAvailable) channel = Channel.Place;
            Current = channel;
            Apply();
            Changed?.Invoke(Current);
        }

        private static Image MakeTab(Transform parent, string name, string caption, float x, Action click)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(102f, 34f);

            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            var label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 17, true);
            label.text = caption;
            label.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => click());
            return image;
        }

        private void Apply()
        {
            if (_placeBackground != null)
                _placeBackground.color = Current == Channel.Place ? Selected : Idle;
            if (_worldBackground != null)
                _worldBackground.color = Current == Channel.Community ? Selected : Idle;
            if (_guildBackground != null)
                _guildBackground.color = Current == Channel.Guild ? Selected : Idle;
        }
    }
}
