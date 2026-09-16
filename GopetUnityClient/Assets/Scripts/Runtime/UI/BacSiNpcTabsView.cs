using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup 2 tab cho NPC "Bác Sĩ Xì Tin" (npcId -7). Server gửi options
    /// [22, 23, 24] nhưng client bỏ 23 (nhiệm vụ hằng ngày) theo yêu cầu user:
    /// <list type="bullet">
    /// <item>Tab "Hồi sinh pet sau PK" (22): body có description + nút "Hồi sinh
    /// ngay". Click nút mới gửi <c>SelectNpcOption(22)</c> — tránh trừ vàng nhầm
    /// nếu user chỉ đổi tab thăm dò.</item>
    /// <item>Tab "Tẩy gym" (24): click tab tự gửi option → server trả
    /// <c>MENU_DELETE_TIEM_NANG</c> → bind vào <see cref="GenericMenuView"/>.</item>
    /// </list>
    /// Style bám <see cref="HeavenNpcTabsView"/> — cùng khung màu popup shop.
    /// </summary>
    public sealed class BacSiNpcTabsView : MonoBehaviour
    {
        // Option IDs — khớp MenuController.cs (server).
        internal const int OpReviveAfterPk = 22;
        internal const int OpDeleteTiemNang = 24;
        private const int MenuDeleteTiemNangListId = 800;

        private const float Width = 560f;
        private const float Height = 320f;
        private const float Padding = 8f;
        private const float Gap = 6f;
        private const float TabHeight = 30f;

        private static readonly Color PanelBg = new Color(0.985f, 0.992f, 1f, 0.995f);
        private static readonly Color PanelBorder = new Color(0.26f, 0.58f, 0.95f, 1f);
        private static readonly Color TabActive = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color TabInactive = new Color(0.35f, 0.65f, 1f, 1f);
        private static readonly Color TabText = new Color(0.14f, 0.24f, 0.44f, 1f);
        private static readonly Color TextDark = new Color(0.14f, 0.18f, 0.25f, 1f);
        private static readonly Color BodyBg = new Color(0.995f, 1f, 1f, 0.94f);
        private static readonly Color ActionBtn = new Color(0.22f, 0.55f, 0.28f, 1f);

        public event Action<int> OptionRequested;
        public event Action Closed;
        public event Action<Gopet.UiLogic.MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        private int _activeOptionId;
        private Image[] _tabBackgrounds;
        private NpcOptions.Option[] _options;
        private GameObject _revivePanel;
        private GenericMenuView _menuView;
        private Font _font;

        public static BacSiNpcTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var root = new GameObject("BacSiNpcTabs", typeof(RectTransform), typeof(Image));
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

            var view = root.AddComponent<BacSiNpcTabsView>();
            view._font = font;
            view._options = FilterVisible(options?.Options);
            view.BuildTabs(root.transform);
            view.BuildBody(root.transform);
            view.BuildClose(root.transform);
            view.SelectFirstTab();
            return view;
        }

        /// <summary>Nhận MenuScreen server trả cho tab 2/3. Không match listId thì bỏ qua.</summary>
        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            if (screen == null || _menuView == null) return false;
            // Tab hồi sinh không cần menu — bỏ qua để dispatcher khác xử.
            if (_activeOptionId != OpDeleteTiemNang || screen.ListId != MenuDeleteTiemNangListId)
                return false;

            // MENU_DELETE_TIEM_NANG (800) và menu nhiệm vụ hằng ngày đều là list
            // simple row (không phải pet grid) → dùng GenericMenuView cho cả 2.
            _menuView.Bind(screen, assets, guider);
            ShowOnly(_menuView.gameObject);
            return true;
        }

        private void BuildTabs(Transform parent)
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

                var label = UiBuilder.MakeText(go.transform, _font, "Label", 10, true);
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

        private void BuildBody(Transform parent)
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

            _revivePanel = BuildRevivePanel(body.transform);
            _menuView = GenericMenuView.Create(body.transform, _font);
            _menuView.SetLightCards(true);
            // EMBEDDED scroll — nhúng trong body sẵn có. EnableInteractiveScroll sẽ dựng
            // full-screen backdrop+panel riêng đè lên tab view (bug quan sát 2026-09-16).
            _menuView.EnableEmbeddedScroll(Height - TabHeight - Padding * 2f - Gap);
            _menuView.ConfirmRequested += (prompt, confirm) => ConfirmRequested?.Invoke(prompt, confirm);
            _menuView.gameObject.SetActive(false);
        }

        private GameObject BuildRevivePanel(Transform parent)
        {
            var panel = new GameObject("Revive", typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            UiBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(20f, 20f);
            rect.offsetMax = new Vector2(-20f, -20f);

            var desc = UiBuilder.MakeText(panel.transform, _font, "Desc", 12, true);
            desc.text = "Pet đang theo bị chết sau PK? Nhấn nút bên dưới để hồi sinh (tốn vàng).";
            desc.color = TextDark;
            desc.alignment = TextAnchor.UpperCenter;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Overflow;
            var dr = (RectTransform)desc.transform;
            dr.anchorMin = new Vector2(0f, 1f); dr.anchorMax = new Vector2(1f, 1f);
            dr.pivot = new Vector2(0.5f, 1f);
            dr.sizeDelta = new Vector2(0f, 60f);
            dr.anchoredPosition = new Vector2(0f, -8f);

            var btnGo = new GameObject("ReviveBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panel.transform, false);
            var brect = (RectTransform)btnGo.transform;
            brect.anchorMin = brect.anchorMax = new Vector2(0.5f, 0.5f);
            brect.pivot = new Vector2(0.5f, 0.5f);
            brect.sizeDelta = new Vector2(180f, 46f);
            brect.anchoredPosition = new Vector2(0f, -10f);
            var bimg = btnGo.GetComponent<Image>();
            RoundedUiSprite.Apply(bimg);
            bimg.color = ActionBtn;
            var lbl = UiBuilder.MakeText(btnGo.transform, _font, "Label", 14, true);
            lbl.text = "Hồi sinh ngay";
            lbl.alignment = TextAnchor.MiddleCenter;
            lbl.color = Color.white;
            lbl.fontStyle = FontStyle.Bold;
            btnGo.GetComponent<Button>().onClick.AddListener(() => OptionRequested?.Invoke(OpReviveAfterPk));

            return panel;
        }

        private void BuildClose(Transform parent)
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
                var label = UiBuilder.MakeText(go.transform, _font, "X", 18, true);
                label.text = "×"; label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white; label.fontStyle = FontStyle.Bold;
            }
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

        private void SelectFirstTab()
        {
            if (_options.Length > 0) SelectTab(_options[0].Id, notify: false);
        }

        private void SelectTab(int optionId, bool notify = true)
        {
            _activeOptionId = optionId;
            for (var i = 0; i < _options.Length; i++)
            {
                if (_tabBackgrounds != null && _tabBackgrounds[i] != null)
                    _tabBackgrounds[i].color = _options[i].Id == optionId ? TabActive : TabInactive;
            }

            // Tab hồi sinh: hiện panel button, không gửi server ngay.
            // Tab khác: hiện empty area (chờ MenuScreen về), và gửi option để server trả list.
            if (optionId == OpReviveAfterPk)
            {
                ShowOnly(_revivePanel);
            }
            else
            {
                // Chưa có menu → giấu cả 2 view menu; TryConsumeMenu sẽ ShowOnly khi có gói.
                if (_revivePanel != null) _revivePanel.SetActive(false);
                if (_menuView != null) _menuView.gameObject.SetActive(false);
                if (notify) OptionRequested?.Invoke(optionId);
            }
        }

        private void ShowOnly(GameObject visible)
        {
            if (_revivePanel != null) _revivePanel.SetActive(_revivePanel == visible);
            if (_menuView != null) _menuView.gameObject.SetActive(_menuView.gameObject == visible);
        }

        private static NpcOptions.Option[] FilterVisible(NpcOptions.Option[] options)
        {
            if (options == null || options.Length == 0) return Array.Empty<NpcOptions.Option>();

            var visibleCount = 0;
            for (var i = 0; i < options.Length; i++)
            {
                if (options[i] != null && options[i].Id != 23) visibleCount++;
            }

            var visible = new NpcOptions.Option[visibleCount];
            var writeIndex = 0;
            for (var i = 0; i < options.Length; i++)
            {
                var option = options[i];
                if (option == null || option.Id == 23) continue;
                visible[writeIndex++] = option;
            }

            return visible;
        }
    }
}
