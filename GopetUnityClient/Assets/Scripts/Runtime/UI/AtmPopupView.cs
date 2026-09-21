using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup <b>Dịch vụ</b> — ngân hàng và quy đổi tiền tệ, đi theo flow server:
    /// menu 1039/1040 và input 16/32.
    ///
    /// <para>Dùng chung <see cref="GamePopupFrame"/> với popup cửa hàng nên khung,
    /// badge tiêu đề, nút X và băng chân giống hệt nhau — đổi tông một chỗ là cả hai
    /// popup đổi theo.</para>
    ///
    /// <para>Nội dung bên trong nằm ở <c>AtmPopupView.Body.cs</c>.</para>
    /// </summary>
    public sealed partial class AtmPopupView : MonoBehaviour
    {
        public const int AtmMenuId = 1039;
        public const int ExchangeGoldMenuId = 1040;
        public const int GoldToCoinDialogId = 16;
        public const int LuaToCoinDialogId = 32;

        // Khớp popup cửa hàng: khay tab cao 38, tab viên thuốc cao 20 bo góc 6.
        private const float RailHeight = 38f;
        private const float RailPadding = 11f;
        private const float TabWidth = 77f;
        private const float TabHeight = 20f;
        private const float TabRadius = 6f;
        private const float RailGap = 6f;

        private static readonly Color DarkText = PopupPalette.TextDark;

        private Font _font;
        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private GamePopupFrame _frame;
        private Transform _body;
        private Action _requestAtm;

        public event Action Closed;
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        public static AtmPopupView Create(Transform parent, Font font, GuiderHandler guider,
            RemoteAssetCache assets, Action requestAtm)
        {
            var frame = GamePopupFrame.Create(parent, font, "Dịch vụ",
                footer: "Ngân hàng và quy đổi tiền tệ");
            frame.gameObject.name = "AtmPopup";

            var view = frame.gameObject.AddComponent<AtmPopupView>();
            view._frame = frame;
            view._font = font ?? UiBuilder.BuiltinFont();
            view._guider = guider ?? throw new ArgumentNullException(nameof(guider));
            view._assets = assets;
            view._requestAtm = requestAtm ?? throw new ArgumentNullException(nameof(requestAtm));
            frame.Closed += () => view.Closed?.Invoke();

            view.BuildTab(frame.Content);
            view.BuildBody(frame.Content);
            view.ShowLoading("Đang tải thông tin ATM...");
            return view;
        }

        public void RequestAtm()
        {
            ShowLoading("Đang tải thông tin ATM...");
            _requestAtm();
        }

        public bool TryConsumeListOption(ListOptionScreen screen)
        {
            if (screen == null || screen.ListId != AtmMenuId) return false;
            ShowAtmOptions(screen);
            return true;
        }

        public bool TryConsumeMenu(MenuScreen screen)
        {
            if (screen == null || screen.ListId != ExchangeGoldMenuId) return false;

            // Dùng ĐÚNG khung danh sách của cửa hàng (PopupItemList) chứ không nhúng
            // GenericMenuView: menu generic dựng thẻ nền tối, lạc hẳn khỏi tông sáng
            // của khung popup.
            ClearBody();
            var list = PopupItemList.Create(_body, _font, withPanel: false);
            list.Bind(screen, _assets, "Đổi");
            list.Activated += index => SelectExchange(screen, index);
            return true;
        }

        private void SelectExchange(MenuScreen screen, int index)
        {
            if (index < 0 || index >= screen.Items.Length) return;

            var item = screen.Items[index];
            if (!item.CanSelect) return;

            void Send() => _guider.Select(screen, index,
                item.PaymentOptions != null && item.PaymentOptions.Length > 0 ? 0 : -1);

            if (item.ShowDialog && ConfirmRequested != null)
                ConfirmRequested(MenuSelection.PromptFor(screen, index), Send);
            else
                Send();
        }

        public bool TryConsumeInput(InputDialogSpec spec)
        {
            if (spec == null || (spec.DialogId != GoldToCoinDialogId && spec.DialogId != LuaToCoinDialogId))
                return false;
            ShowExchangeInput(spec);
            return true;
        }

        /// <summary>Khay trắng + một tab "ATM", cùng khuôn với hàng tab của cửa hàng.</summary>
        private void BuildTab(RectTransform parent)
        {
            var rail = new GameObject("TabRail", typeof(RectTransform), typeof(Image));
            rail.transform.SetParent(parent, false);
            var railRect = (RectTransform)rail.transform;
            railRect.anchorMin = new Vector2(0f, 1f);
            railRect.anchorMax = new Vector2(1f, 1f);
            railRect.pivot = new Vector2(0f, 1f);
            railRect.offsetMin = new Vector2(0f, -RailHeight);
            railRect.offsetMax = Vector2.zero;
            RoundedBorder.Apply(rail, RoundedUiSprite.DefaultRadius, Color.white,
                PopupPalette.Hairline);

            var tab = new GameObject("Tab_ATM", typeof(RectTransform), typeof(Image), typeof(Button));
            tab.transform.SetParent(rail.transform, false);
            var rect = (RectTransform)tab.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(TabWidth, TabHeight);
            rect.anchoredPosition = new Vector2(RailPadding, 0f);

            var image = tab.GetComponent<Image>();
            RoundedUiSprite.Apply(image, TabRadius);
            image.color = PopupPalette.TabActive;

            var label = UiBuilder.MakeText(tab.transform, _font, "Label", 11, true);
            label.text = "ATM";
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;
            tab.GetComponent<Button>().onClick.AddListener(RequestAtm);
        }

        /// <summary>
        /// Khung trắng bo góc chứa nội dung, nằm dưới khay tab.
        ///
        /// <para><see cref="_body"/> trỏ vào một lớp CON rỗng chứ không phải chính tấm
        /// khung: <see cref="RoundedBorder"/> dựng nền trắng bằng một con tên "Fill",
        /// mà <see cref="ClearBody"/> thì xoá sạch con của <see cref="_body"/> — trỏ
        /// thẳng vào khung là lần xoá đầu tiên đã ăn mất nền, chỉ còn trơ lớp viền.</para>
        /// </summary>
        private void BuildBody(RectTransform parent)
        {
            var panel = new GameObject("Body", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -(RailHeight + RailGap));
            RoundedBorder.Apply(panel, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                PopupPalette.Hairline);

            var content = new GameObject("BodyContent", typeof(RectTransform));
            content.transform.SetParent(panel.transform, false);
            UiBuilder.Stretch((RectTransform)content.transform);
            _body = content.transform;
        }
    }
}
