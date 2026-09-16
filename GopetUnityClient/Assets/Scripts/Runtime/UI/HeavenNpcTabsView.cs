using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup 2 tab cho NPC "Sứ Giả Thiên Đình" (npcId -25, options 88+89):
    /// <list type="bullet">
    /// <item>Tab 1 "Hướng dẫn lên thiên đình" — hiển thị text hướng dẫn ngay,
    /// không cần round-trip server. Text hardcode khớp <c>Language.GuideToHeaven</c>
    /// mặc định — nếu server sửa text thì đồng bộ tay ở đây.</item>
    /// <item>Tab 2 "Hiến tặng thú cưng" — bấm gửi <c>SelectNpcOption(89)</c>,
    /// server trả <c>MENU_PET_SACRIFICE (1084)</c>, <see cref="TryConsumeMenu"/>
    /// bind vào <see cref="PetGridView"/> trong khu vực body của tab.</item>
    /// </list>
    /// Style bám <see cref="TranChanTabsView"/> — cùng khung màu popup shop.
    /// </summary>
    public sealed class HeavenNpcTabsView : MonoBehaviour
    {
        // MENU_PET_SACRIFICE = 1084 (MenuController.cs:141) — listId server trả cho tab 2.
        internal const int MenuPetSacrificeListId = 1084;

        // Text hardcode khớp Language.GuideToHeaven (LanguageData.cs:581).
        private const string GuideText =
            "Bạn cần làm hết các nhiệm vụ trùng sinh để nhận được cánh bay về trời. " +
            "Nơi thú cưng đột biến kinh khủng khiếp đang trên đầu chúng ta.";

        private const float Width = 520f;
        private const float Height = 280f;
        private const float Padding = 8f;
        private const float Gap = 6f;
        private const float TabHeight = 30f;

        private static readonly Color PanelBg = new Color(0.985f, 0.992f, 1f, 0.995f);
        private static readonly Color PanelBorder = new Color(0.26f, 0.58f, 0.95f, 1f);
        // Bám ShopPopupView: vàng active, xanh nhạt inactive, chữ xanh đậm.
        private static readonly Color TabActive = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color TabInactive = new Color(0.35f, 0.65f, 1f, 1f);
        private static readonly Color TabText = new Color(0.14f, 0.24f, 0.44f, 1f);
        private static readonly Color TextDark = new Color(0.14f, 0.18f, 0.25f, 1f);
        private static readonly Color BodyBg = new Color(0.995f, 1f, 1f, 0.94f);

        public event Action<int> TabChosen;
        public event Action Closed;
        public event Action<Gopet.UiLogic.MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        private int _activeOptionId;
        private Image[] _tabBackgrounds;
        private NpcOptions.Option[] _options;
        private GameObject _guideView;
        private PetGridView _petGrid;

        public static HeavenNpcTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var root = new GameObject("HeavenNpcTabs", typeof(RectTransform), typeof(Image));
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

            var view = root.AddComponent<HeavenNpcTabsView>();
            view._options = options?.Options ?? Array.Empty<NpcOptions.Option>();
            view.BuildTabs(root.transform, font);
            view.BuildBody(root.transform, font);
            view.BuildClose(root.transform, font);
            view.SelectFirstTab();
            return view;
        }

        /// <summary>Bind danh sách pet vào body khi user chọn tab "Hiến tặng".
        /// Chỉ tiêu thụ MenuScreen khi listId khớp <c>MENU_PET_SACRIFICE</c>.</summary>
        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            if (screen == null || _petGrid == null) return false;
            if (screen.ListId != MenuPetSacrificeListId) return false;

            _petGrid.Bind(screen, assets, guider, "Hiến tặng", PetGridMode.Receive);
            _petGrid.gameObject.SetActive(true);
            if (_guideView != null) _guideView.SetActive(false);
            return true;
        }

        private void BuildTabs(Transform parent, Font font)
        {
            var count = _options.Length;
            if (count == 0) return;

            var availableWidth = Width - Padding * 2f;
            var tabWidth = (availableWidth - Gap * (count - 1)) / count;

            _tabBackgrounds = new Image[count];

            for (var i = 0; i < count; i++)
            {
                var option = _options[i];
                var go = new GameObject($"Tab_{option.Id}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(tabWidth, TabHeight);
                rect.anchoredPosition = new Vector2(Padding + i * (tabWidth + Gap), -Padding);

                var background = go.GetComponent<Image>();
                RoundedUiSprite.Apply(background);
                _tabBackgrounds[i] = background;

                var label = UiBuilder.MakeText(go.transform, font, "Label", 10, true);
                label.text = option.Text ?? string.Empty;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = TabText;
                label.fontStyle = FontStyle.Bold;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;


                var captured = option.Id;
                go.GetComponent<Button>().onClick.AddListener(() => SelectTab(captured));
            }
        }

        private void BuildBody(Transform parent, Font font)
        {
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(parent, false);
            var rect = (RectTransform)body.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(Padding, Padding);
            rect.offsetMax = new Vector2(-Padding, -(Padding + TabHeight + Gap));
            RoundedUiSprite.Apply(body.GetComponent<Image>());
            body.GetComponent<Image>().color = BodyBg;

            _guideView = BuildGuideText(body.transform, font);
            _petGrid = PetGridView.Create(body.transform, font);
            _petGrid.ConfirmRequested += (prompt, confirm) => ConfirmRequested?.Invoke(prompt, confirm);
            _petGrid.gameObject.SetActive(false);
        }

        private static GameObject BuildGuideText(Transform parent, Font font)
        {
            var go = new GameObject("Guide", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            UiBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(14f, 12f);
            rect.offsetMax = new Vector2(-14f, -12f);

            var text = UiBuilder.MakeText(go.transform, font, "Text", 12, true);
            text.text = GuideText;
            text.color = TextDark;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return go;
        }

        private void BuildClose(Transform parent, Font font)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(30f, 30f);
            rect.anchoredPosition = new Vector2(5f, 5f);

            var image = go.GetComponent<Image>();
            image.preserveAspect = true;
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null) { image.sprite = sprite; image.color = Color.white; }
            else
            {
                image.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var label = UiBuilder.MakeText(go.transform, font, "X", 18, true);
                label.text = "×"; label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white; label.fontStyle = FontStyle.Bold;
            }
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

        private void SelectFirstTab()
        {
            if (_options.Length == 0) return;
            SelectTab(_options[0].Id, notify: false);
            // Tab 1 = guide text: không cần gọi server, hiển thị ngay.
        }

        private void SelectTab(int optionId, bool notify = true)
        {
            _activeOptionId = optionId;
            // Chỉ đổi background (vàng active / xanh inactive) — chữ giữ nguyên TabText,
            // giống ShopPopupView (line 249).
            for (var i = 0; i < _options.Length; i++)
            {
                if (_tabBackgrounds != null && _tabBackgrounds[i] != null)
                    _tabBackgrounds[i].color = _options[i].Id == optionId ? TabActive : TabInactive;
            }

            // Tab 1 (index 0) = guide text hardcode, không cần round-trip server.
            // Tab 2+ = mở list qua SelectNpcOption.
            var isGuideTab = _options.Length > 0 && _options[0].Id == optionId;
            if (_guideView != null) _guideView.SetActive(isGuideTab);
            if (_petGrid != null && isGuideTab) _petGrid.gameObject.SetActive(false);

            if (notify && !isGuideTab) TabChosen?.Invoke(optionId);
        }
    }
}
