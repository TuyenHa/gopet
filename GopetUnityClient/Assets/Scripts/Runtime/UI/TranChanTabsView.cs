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
        // 298 chọn để vùng nội dung dưới khay tab cao đúng 200 như bản cũ — lưới pet
        // đã căn theo con số đó.
        private const float Width = 520f;
        private const float Height = 298f;
        private const string Footer = "Chọn một pet để nhận hoặc mua";

        /// <summary>Tuỳ chọn server gửi nhưng chưa từng hiện thành tab.</summary>
        private const int HiddenOptionId = 81;
        /// <summary>"Nhận quà tặng" — đã nằm ở tab Quà tặng của popup Sự kiện.</summary>
        private const int GiftCodeOptionId = 60;

        public event Action<int> TabChosen;
        public event Action Closed;
        public event Action<Gopet.UiLogic.MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        private GenericMenuView _menuView;
        private PetGridView _petGrid;
        private TopPatronView _topPatron;
        private PopupTabRail _rail;
        private int[] _optionIds;
        private int _activeOptionId = 1;

        public static TranChanTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var frame = GamePopupFrame.Create(parent, font, "Pet", Width, Height, footer: Footer);
            frame.gameObject.name = "TranChanTabs";

            var view = frame.gameObject.AddComponent<TranChanTabsView>();
            frame.Closed += () => view.Closed?.Invoke();

            view.BuildTabs(frame, font, options);
            view.BuildBody(frame.Content, font);
            return view;
        }

        /// <summary>Nhận các list pet/top/shop mà server trả sau khi chọn tab.</summary>
        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            if (screen == null || _menuView == null) return false;
            var isPetShop = _activeOptionId == 2 || screen.ListId == 8;
            var isTopPet = _activeOptionId == 3 && screen.ListId == -1;
            var isTopPatron = _activeOptionId == 41;
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

        /// <summary>
        /// Khay tab dùng chung với popup cửa hàng. Số tab do server quyết (NpcOptions),
        /// trừ hai tuỳ chọn bị ẩn: 81 (vốn chưa từng hiện) và 60 "Nhận quà tặng" — nhập
        /// giftcode đã chuyển hẳn sang tab Quà tặng của popup Sự kiện.
        /// </summary>
        private void BuildTabs(GamePopupFrame frame, Font font, NpcOptions options)
        {
            if (options?.Options == null) return;

            var visible = options.Options
                .Where(o => o != null && o.Id != HiddenOptionId && o.Id != GiftCodeOptionId)
                .ToArray();
            if (visible.Length == 0) return;

            _optionIds = visible.Select(o => o.Id).ToArray();
            var labels = visible.Select(TabLabel).ToArray();

            _rail = PopupTabRail.Create(frame.Content, font, frame.ContentWidth, labels);
            _rail.Selected += index =>
            {
                _activeOptionId = _optionIds[index];
                TabChosen?.Invoke(_activeOptionId);
            };
            _rail.Select(0);
        }

        private static string TabLabel(NpcOptions.Option option)
        {
            if (option == null) return string.Empty;
            if (option.Id == 1) return "Nhận pet";
            return option.Text ?? string.Empty;
        }

        private void BuildBody(RectTransform content, Font font)
        {
            var body = new GameObject("PetList", typeof(RectTransform), typeof(Image));
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

            _menuView = GenericMenuView.Create(body.transform, font);
            _menuView.SetLightCards(true);
            _menuView.EnableEmbeddedScroll(bodyHeight);
            _petGrid = PetGridView.Create(body.transform, font);
            _petGrid.ConfirmRequested += (prompt, confirm) => ConfirmRequested?.Invoke(prompt, confirm);
            _petGrid.gameObject.SetActive(false);
            _topPatron = TopPatronView.Create(body.transform, font);
            _topPatron.gameObject.SetActive(false);
        }
    }
}
