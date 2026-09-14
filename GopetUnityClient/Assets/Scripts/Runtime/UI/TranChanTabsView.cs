using System;
using System.Linq;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Menu Trần Trấn dạng tab, dùng chung khung màu với popup cửa hàng.</summary>
    public sealed class TranChanTabsView : MonoBehaviour
    {
        private const float Width = 520f;
        private const float Height = 280f;
        private const float Padding = 8f;
        private const float Gap = 5f;
        private const float TabHeight = 28f;
        private const float FooterHeight = 28f;

        private static readonly Color PanelBackground = new Color(0.985f, 0.992f, 1f, 0.995f);
        private static readonly Color Active = new Color(0.035f, 0.42f, 0.94f, 1f);
        private static readonly Color Inactive = new Color(0.93f, 0.94f, 0.97f, 1f);
        private static readonly Color Text = new Color(0.14f, 0.18f, 0.25f, 1f);

        public event Action<int> TabChosen;
        public event Action Closed;
        public event Action<Gopet.UiLogic.MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        private GenericMenuView _menuView;
        private PetGridView _petGrid;
        private TopPatronView _topPatron;
        private GameObject _footer;
        private RectTransform _bodyRect;
        private Image[] _tabBackgrounds;
        private Text[] _tabLabels;
        private int _activeOptionId = 1;

        public static TranChanTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var root = new GameObject("TranChanTabs", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Width, Height);

            var image = root.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = PanelBackground;
            var outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(0.26f, 0.58f, 0.95f, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            var view = root.AddComponent<TranChanTabsView>();
            view.BuildTabs(root.transform, font, options);
            view.BuildBody(root.transform, font);
            view._footer = BuildFooter(root.transform, font);
            view.BuildClose(root.transform, font);
            return view;
        }

        /// <summary>Nhận các list pet/top/shop mà server trả sau khi chọn tab.</summary>
        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            if (screen == null || _menuView == null) return false;
            var isPetShop = _activeOptionId == 2 || screen.ListId == 8;
            var isTopPet = _activeOptionId == 3 && screen.ListId == -1;
            var isTopPatron = _activeOptionId == 41;
            if (_footer != null) _footer.SetActive(!isTopPatron);
            if (_bodyRect != null)
                _bodyRect.offsetMin = new Vector2(Padding, isTopPatron ? Padding : Padding + FooterHeight);
            if (_activeOptionId == 1 || isPetShop || isTopPet)
            {
                _petGrid.Bind(screen, assets, guider,
                    isPetShop ? "Mua" : "Nhận",
                    isTopPet ? PetGridMode.Top : isPetShop ? PetGridMode.Shop : PetGridMode.Receive);
                _petGrid.gameObject.SetActive(true);
                _menuView.gameObject.SetActive(false);
                _topPatron.gameObject.SetActive(false);
            }
            else if (isTopPatron)
            {
                _topPatron.Bind(screen, assets);
                _topPatron.gameObject.SetActive(true);
                _menuView.gameObject.SetActive(false);
                _petGrid.gameObject.SetActive(false);
            }
            else
            {
                _menuView.Bind(screen, assets, guider);
                _menuView.gameObject.SetActive(true);
                _petGrid.gameObject.SetActive(false);
                _topPatron.gameObject.SetActive(false);
            }
            return true;
        }

        private void BuildTabs(Transform parent, Font font, NpcOptions options)
        {
            if (options == null || options.Options == null) return;

            var visibleOptions = options.Options.Where(option => option != null && option.Id != 81).ToArray();
            var count = visibleOptions.Length;
            var columns = Mathf.Max(1, count);
            var availableWidth = Width - Padding * 2f;
            var tabWidth = (availableWidth - Gap * (columns - 1)) / columns;
            var top = Padding;

            _tabBackgrounds = new Image[count];
            _tabLabels = new Text[count];

            for (var i = 0; i < count; i++)
            {
                var option = visibleOptions[i];
                var go = new GameObject($"Tab_{option.Id}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
                var rect = (RectTransform)go.transform;
                var row = i / columns;
                var column = i % columns;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(tabWidth, TabHeight);
                rect.anchoredPosition = new Vector2(Padding + column * (tabWidth + Gap), -(top + row * (TabHeight + Gap)));

                var background = go.GetComponent<Image>();
                RoundedUiSprite.Apply(background);
                background.color = i == 0 ? Active : Inactive;
                _tabBackgrounds[i] = background;

                var label = UiBuilder.MakeText(go.transform, font, "Label", 8, true);
                label.text = TabLabel(option);
                label.alignment = TextAnchor.MiddleCenter;
                label.color = i == 0 ? Color.white : Text;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                _tabLabels[i] = label;

                var captured = option.Id;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _activeOptionId = captured;
                    for (var tab = 0; tab < _tabBackgrounds.Length; tab++)
                    {
                        var selected = visibleOptions[tab].Id == captured;
                        _tabBackgrounds[tab].color = selected ? Active : Inactive;
                        _tabLabels[tab].color = selected ? Color.white : Text;
                    }
                    TabChosen?.Invoke(captured);
                });
            }
        }

        private static string TabLabel(NpcOptions.Option option)
        {
            if (option == null) return string.Empty;
            if (option.Id == 1) return "Nhận pet";
            if (option.Id == 60) return "Nhận quà tặng";
            return option.Text ?? string.Empty;
        }

        private void BuildBody(Transform parent, Font font)
        {
            var body = new GameObject("PetList", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(parent, false);
            var rect = (RectTransform)body.transform;
            _bodyRect = rect;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(Padding, Padding + FooterHeight);
            rect.offsetMax = new Vector2(-Padding, -(Padding + TabHeight + 8f));

            var image = body.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = new Color(0.995f, 1f, 1f, 0.94f);

            _menuView = GenericMenuView.Create(body.transform, font);
            _menuView.SetLightCards(true);
            _menuView.EnableEmbeddedScroll(Height - TabHeight - FooterHeight - Padding * 2f - 8f);
            _petGrid = PetGridView.Create(body.transform, font);
            _petGrid.ConfirmRequested += (prompt, confirm) => ConfirmRequested?.Invoke(prompt, confirm);
            _petGrid.gameObject.SetActive(false);
            _topPatron = TopPatronView.Create(body.transform, font);
            _topPatron.gameObject.SetActive(false);
        }

        private static GameObject BuildFooter(Transform parent, Font font)
        {
            var footer = new GameObject("Footer", typeof(RectTransform), typeof(Image));
            footer.transform.SetParent(parent, false);
            var rect = (RectTransform)footer.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(Padding, Padding);
            rect.offsetMax = new Vector2(-Padding, Padding + FooterHeight - 5f);
            RoundedUiSprite.Apply(footer.GetComponent<Image>());
            footer.GetComponent<Image>().color = new Color(0.955f, 0.973f, 1f, 0.96f);

            var help = UiBuilder.MakeText(footer.transform, font, "Help", 8, false);
            help.text = "?   Chọn một pet để nhận hoặc mua";
            help.color = new Color(0.04f, 0.45f, 0.94f, 1f);
            help.alignment = TextAnchor.MiddleLeft;
            UiBuilder.Stretch(help.rectTransform);
            help.rectTransform.offsetMin = new Vector2(16f, 0f);
            help.rectTransform.offsetMax = new Vector2(-165f, 0f);

            var hint = UiBuilder.MakeText(footer.transform, font, "ScrollHint", 8, false);
            hint.text = "↕  Vuốt để xem thêm pet";
            hint.color = new Color(0.05f, 0.43f, 0.9f, 1f);
            hint.alignment = TextAnchor.MiddleRight;
            UiBuilder.Stretch(hint.rectTransform);
            hint.rectTransform.offsetMin = new Vector2(Width - 185f, 0f);
            hint.rectTransform.offsetMax = new Vector2(-12f, 0f);
            return footer;
        }

        private void BuildClose(Transform parent, Font font)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(28f, 28f);
            rect.anchoredPosition = new Vector2(5f, 5f);

            var image = go.GetComponent<Image>();
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
            else
            {
                RoundedUiSprite.Apply(image);
                image.color = new Color(0.9f, 0.12f, 0.1f, 1f);
                var label = UiBuilder.MakeText(go.transform, font, "Label", 20, true);
                label.text = "×";
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
            }

            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }
    }
}
