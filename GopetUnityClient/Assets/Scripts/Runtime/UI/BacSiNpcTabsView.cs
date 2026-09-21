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
    public sealed partial class BacSiNpcTabsView : MonoBehaviour
    {
        // Option IDs — khớp MenuController.cs (server). Để public vì PlayMode test là
        // assembly riêng trong Unity; internal thì test không thấy.
        public const int OpReviveAfterPk = 22;
        public const int OpDeleteTiemNang = 24;
        /// <summary><c>MENU_DELETE_TIEM_NANG</c> — listId server trả cho tab "Tẩy gym".</summary>
        public const int MenuDeleteTiemNangListId = 800;

        private const float Width = 520f;
        private const float Height = 320f;
        private const string Footer = "Hồi sinh pet và tẩy điểm tiềm năng";

        private static readonly Color TextDark = PopupPalette.TextDark;
        private static readonly Color ActionBtn = PopupPalette.PriceGreen;

        public event Action<int> OptionRequested;
        public event Action Closed;
        public event Action<Gopet.UiLogic.MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        private int _activeOptionId;
        private PopupTabRail _rail;
        private NpcOptions.Option[] _options;
        private GameObject _revivePanel;
        private GenericMenuView _menuView;
        private Font _font;

        public static BacSiNpcTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var frame = GamePopupFrame.Create(parent, font, "Bác sĩ", Width, Height, footer: Footer);
            frame.gameObject.name = "BacSiNpcTabs";

            var view = frame.gameObject.AddComponent<BacSiNpcTabsView>();
            view._font = font;
            view._options = FilterVisible(options?.Options);
            frame.Closed += () => view.Closed?.Invoke();

            view.BuildTabs(frame);
            view.BuildBody(frame.Content);
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

        private void BuildTabs(GamePopupFrame frame)
        {
            if (_options.Length == 0) return;

            var labels = new string[_options.Length];
            for (var i = 0; i < _options.Length; i++) labels[i] = _options[i].Text ?? string.Empty;

            _rail = PopupTabRail.Create(frame.Content, _font, frame.ContentWidth, labels);
        }

        private void BuildBody(RectTransform content)
        {
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(content, false);
            var rect = (RectTransform)body.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
            RoundedBorder.Apply(body, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                PopupPalette.Hairline);

            var bodyHeight = Height - GamePopupFrame.ContentTop - 36f
                             - PopupTabRail.Height - PopupTabRail.Gap;

            _revivePanel = BuildRevivePanel(body.transform);
            _menuView = GenericMenuView.Create(body.transform, _font);
            _menuView.SetLightCards(true);
            // EMBEDDED scroll — nhúng trong body sẵn có. EnableInteractiveScroll sẽ dựng
            // full-screen backdrop+panel riêng đè lên tab view (bug quan sát 2026-09-16).
            _menuView.EnableEmbeddedScroll(bodyHeight);
            _menuView.ConfirmRequested += (prompt, confirm) => ConfirmRequested?.Invoke(prompt, confirm);
            _menuView.gameObject.SetActive(false);
        }

        private void SelectFirstTab()
        {
            if (_options.Length == 0) return;

            _rail.Select(0);
            SelectTab(_options[0].Id, notify: false);
            _rail.Selected += index => SelectTab(_options[index].Id);
        }

        private void SelectTab(int optionId, bool notify = true)
        {
            _activeOptionId = optionId;

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
