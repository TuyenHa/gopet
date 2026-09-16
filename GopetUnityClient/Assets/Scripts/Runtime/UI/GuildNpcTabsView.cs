using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Popup tab nhỏ cho NPC Sứ giả bang hội (npcId -15).</summary>
    public sealed class GuildNpcTabsView : MonoBehaviour
    {
        private const float Width = 380f;
        private const float Height = 320f;
        private const float Padding = 6f;
        private const float Gap = 4f;
        private const float TabHeight = 32f;
        private const float TabWidth = 118f;

        private static readonly Color PanelBg = new Color(0.96f, 0.98f, 1f, 1f);
        private static readonly Color PanelBorder = new Color(0.28f, 0.6f, 1f, 1f);
        private static readonly Color TabActive = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color TabInactive = new Color(0.35f, 0.65f, 1f, 1f);
        private static readonly Color TabText = new Color(0.14f, 0.24f, 0.44f, 1f);

        private NpcOptions.Option[] _options;
        private Image[] _backgrounds;
        private Font _font;
        private GenericMenuView _menuView;
        private int _activeOptionId;

        public event Action<int> OptionChosen;
        public event Action Closed;

        public static GuildNpcTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var root = new GameObject("GuildNpcTabs", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Width, Height);

            var image = root.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = PanelBg;
            var outline = root.AddComponent<Outline>();
            outline.effectColor = PanelBorder;
            outline.effectDistance = new Vector2(2f, 2f);

            var view = root.AddComponent<GuildNpcTabsView>();
            view._font = font;
            view._options = options?.Options ?? Array.Empty<NpcOptions.Option>();
            view._activeOptionId = view._options.Length > 0 ? view._options[0].Id : -1;
            view.BuildTabs(root.transform);
            view.BuildBody(root.transform);
            view.BuildClose(root.transform);
            return view;
        }

        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            // Bảng TOP được hiển thị trong GuildTopPopupView riêng.
            return false;
        }

        private void BuildTabs(Transform parent)
        {
            _backgrounds = new Image[_options.Length];
            for (var i = 0; i < _options.Length; i++)
            {
                var go = new GameObject($"Tab_{_options[i].Id}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(TabWidth, TabHeight);
                rect.anchoredPosition = new Vector2(Padding + (i % 3) * (TabWidth + Gap),
                    -(Padding + (i / 3) * (TabHeight + Gap)));

                var bg = go.GetComponent<Image>();
                RoundedUiSprite.Apply(bg);
                bg.color = TabInactive;
                _backgrounds[i] = bg;

                var label = UiBuilder.MakeText(go.transform, _font, "Label", 11, true);
                label.text = _options[i].Text ?? string.Empty;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = TabText;
                label.fontStyle = FontStyle.Bold;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;

                var captured = i;
                go.GetComponent<Button>().onClick.AddListener(() => Choose(captured));
            }
        }

        private void BuildClose(Transform parent)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(34f, 34f);
            rect.anchoredPosition = new Vector2(5f, 5f);
            var image = go.GetComponent<Image>();
            image.sprite = HudSkin.Get(HudSkin.Close);
            image.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

        private void BuildBody(Transform parent)
        {
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(parent, false);
            var rect = (RectTransform)body.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(Padding, Padding);
            rect.offsetMax = new Vector2(-Padding, -(Padding + TabHeight * 2f + Gap));
            RoundedUiSprite.Apply(body.GetComponent<Image>());
            body.GetComponent<Image>().color = new Color(0.995f, 1f, 1f, 1f);

            _menuView = GenericMenuView.Create(body.transform, _font);
            _menuView.SetLightCards(true);
            _menuView.EnableEmbeddedScroll(Height - TabHeight * 2f - Padding * 2f - Gap);
            _menuView.gameObject.SetActive(false);
        }

        private void Choose(int index)
        {
            _activeOptionId = _options[index].Id;
            for (var i = 0; i < _backgrounds.Length; i++)
                _backgrounds[i].color = i == index ? TabActive : TabInactive;
            if (_menuView != null) _menuView.gameObject.SetActive(false);
            OptionChosen?.Invoke(_options[index].Id);
        }
    }
}
