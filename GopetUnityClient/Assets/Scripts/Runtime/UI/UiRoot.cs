using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Gopet.Net.Npc;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nối gói tin của server vào màn hình, qua một chồng <see cref="DialogStack"/>.
    ///
    /// <para>Đây là chỗ nguyên tắc cốt lõi của phase thành hiện thực: server gửi
    /// màn hình nào thì dựng đúng loại view cho nó, <b>không nhìn <c>listId</c></b>.
    /// Nhờ vậy 162 màn hình của server chạy mà không có một dòng code riêng nào.</para>
    /// </summary>
    public sealed partial class UiRoot : MonoBehaviour
    {
        public const int SortingOrder = 100;
        private readonly DialogStack _stack = new DialogStack();
        private readonly Dictionary<object, GameObject> _views = new Dictionary<object, GameObject>();
        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private Font _font;
        private ShopPopupView _shopPopup;
        private AtmPopupView _atmPopup;
        private TranChanTabsView _tranChanTabs;
        public Func<MenuScreen, bool> MenuInterceptor { get; set; }
        public DialogStack Stack => _stack;
        /// <summary>Màn hình đang hiện, hoặc <c>null</c> khi không còn gì.</summary>
        public object Current => _stack.Top;

        /// <summary>Popup cửa hàng nếu đang mở, ngược lại <c>null</c>.</summary>
        public ShopPopupView ShopPopup => _shopPopup;
        public AtmPopupView AtmPopup => _atmPopup;

        public static UiRoot Create(Transform parent, Font font)
        {
            var go = new GameObject("UiRoot", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.overrideSorting = true; // menu vật phẩm phải nằm trên Canvas battle
            canvas.sortingOrder = SortingOrder;
            var root = go.AddComponent<UiRoot>();
            root._font = font;
            return root;
        }

        public void Initialize(GuiderHandler guider, RemoteAssetCache assets)
        {
            if (_guider != null)
            {
                throw new InvalidOperationException(
                    "UiRoot đã được khởi tạo. Gọi lần hai là đăng ký trùng, mỗi gói tin sẽ mở hai màn hình.");
            }

            _guider = guider ?? throw new ArgumentNullException(nameof(guider));
            _assets = assets;

            _guider.MenuShown += ShowMenu;
            _guider.ListOptionShown += ShowListOption;
            _guider.YesNoAsked += ShowYesNo;
            _guider.InputDialogShown += ShowInputDialog;
            _guider.NpcOptionsShown += ShowNpcOptions;
            _guider.PopupShown += ShowPopup;
            _guider.ImageDialogShown += ShowImageDialog;

            _stack.TopChanged += OnTopChanged;
        }

        /// <summary>Nút back hoặc Esc: đóng màn hình trên cùng. <c>false</c> khi không còn gì để đóng.</summary>
        public bool Back()
        {
            var top = _stack.Top;
            if (top == null) return false;

            // Đi qua Close() để CÓ MỘT đường tháo duy nhất — không thì Back bỏ qua
            // hooks đặc thù (như bỏ tham chiếu _shopPopup) và mỗi lần thêm state mới
            // trong Close, ai đó lại quên đồng bộ Back.
            Close(top);
            return true;
        }

        private void ShowMenu(MenuScreen screen)
        {
            if (_atmPopup != null && _atmPopup.TryConsumeMenu(screen)) return;

            if (_tranChanTabs != null && _tranChanTabs.TryConsumeMenu(screen, _assets, _guider)) return;

            if (MenuInterceptor != null && MenuInterceptor(screen)) return;

            // Popup cửa hàng đang mở và listId khớp shop tab active → giao cho popup
            // tự bind. Không thì cả hai view chồng nhau và người chơi tưởng bug.
            if (_shopPopup != null && _shopPopup.TryConsumeMenu(screen)) return;

            var view = GenericMenuView.Create(transform, _font);
            view.Bind(screen, _assets, _guider);
            view.EnableInteractiveScroll();
            if (screen.ListId == WingInventoryMenuId && _wingHandler != null)
                view.SelectionOverride = index => TryShowWingActions(view, screen, index);

            // Dòng cần xác nhận: đẩy hộp thoại lên TRÊN menu, không thay thế nó —
            // huỷ thì phải quay lại đúng menu đang xem.
            view.ConfirmRequested += (prompt, onYes) => ShowConfirm(prompt, onYes);
            view.CloseRequested += _ => Close(view);

            Push(view, view.gameObject);
        }

        /// <summary>
        /// Mở popup cửa hàng có 4 tab. Đóng bằng nút X hoặc Esc — cùng đường ra:
        /// <see cref="ShopPopupView.Closed"/> → <see cref="Close"/>.
        ///
        /// <para>Đè lên menu đang có bằng cách push chính popup vào <see cref="DialogStack"/>
        /// — logic <see cref="OnTopChanged"/> tự ẩn view dưới đáy.</para>
        /// </summary>
        public void OpenShopPopup()
        {
            if (_shopPopup != null) return;

            _shopPopup = ShopPopupView.Create(transform, _font, _guider, _assets);
            _shopPopup.Closed += () => Close(_shopPopup);

            Push(_shopPopup, _shopPopup.gameObject);
        }

        /// <summary>Mở popup ATM và yêu cầu đúng menu 1039 của server.</summary>
        public void OpenAtmPopup(Action requestAtm)
        {
            if (_atmPopup != null) return;

            _atmPopup = AtmPopupView.Create(transform, _font, _guider, _assets, requestAtm);
            _atmPopup.Closed += () => Close(_atmPopup);
            _atmPopup.ConfirmRequested += ShowConfirm;
            Push(_atmPopup, _atmPopup.gameObject);
            _atmPopup.RequestAtm();
        }

        /// <summary>Toast nhanh, không chặn tương tác — dùng cho "sắp có" v.v.</summary>
        public void ShowToast(string text)
        {
            ToastView.Create(transform, _font, text);
        }

        private void ShowListOption(ListOptionScreen screen)
        {
            if (_atmPopup != null && _atmPopup.TryConsumeListOption(screen)) return;

            var labels = new string[screen.Options.Length];
            for (var i = 0; i < labels.Length; i++) labels[i] = screen.Options[i].Text;

            var view = ChoiceDialogView.Create(transform, _font);
            view.Bind(screen.Title, labels);
            view.Chosen += index =>
            {
                _guider.Select(screen, index);
                Close(view);
            };
            view.Closed += () => Close(view);

            Push(view, view.gameObject);
        }

        private void ShowYesNo(YesNoRequest request)
        {
            var view = ChoiceDialogView.Create(transform, _font);

            // Server chỉ gửi nội dung, không gửi nhãn nút cho hộp này.
            view.Bind(request.Text, new[] { "Đồng ý", "Thôi" });
            view.Chosen += index =>
            {
                _guider.AnswerYesNo(request.DialogId, index == 0);
                Close(view);
            };
            view.Closed += () => Close(view);

            Push(view, view.gameObject);
        }

        private void ShowInputDialog(InputDialogSpec spec)
        {
            if (_atmPopup != null && _atmPopup.TryConsumeInput(spec)) return;

            var view = InputDialogView.Create(transform, _font);
            view.Bind(spec);
            view.Submitted += (dialogId, texts) =>
            {
                _guider.SubmitInput(dialogId, texts);
                Close(view);
            };

            Push(view, view.gameObject);
        }

        private void ShowNpcOptions(NpcOptions options)
        {
            if (options != null && options.NpcId == -1)
            {
                ShowTranChanTabs(options);
                return;
            }

            var labels = new string[options.Options.Length];
            for (var i = 0; i < labels.Length; i++) labels[i] = options.Options[i].Text;

            var view = ChoiceDialogView.Create(transform, _font);
            view.Bind(string.Empty, labels);
            view.Chosen += index =>
            {
                _guider.SelectNpcOption(options.NpcId, options.Options[index].Id);
                Close(view);
            };
            view.Closed += () => Close(view);

            Push(view, view.gameObject);
        }

        private void ShowTranChanTabs(NpcOptions options)
        {
            if (_tranChanTabs != null) Close(_tranChanTabs);

            var view = TranChanTabsView.Create(transform, _font, options);
            _tranChanTabs = view;
            view.TabChosen += optionId => _guider.SelectNpcOption(options.NpcId, optionId);
            view.ConfirmRequested += ShowConfirm;
            view.Closed += () => Close(view);
            Push(view, view.gameObject);

            // Mở popup là tự chọn tab Nhận pet và tải danh sách pet ngay.
            _guider.SelectNpcOption(options.NpcId, LinhThuCityNpcOptions.TranChanNhanPetMienPhi);
        }

        private void ShowConfirm(MenuSelection.ConfirmPrompt prompt, Action onYes)
        {
            var view = ChoiceDialogView.Create(transform, _font);
            // Một số menu cũ, trong đó có ATM, gửi cả left/right command là
            // "OK". Nút thứ hai vẫn là nhánh huỷ nên không được hiển thị trùng.
            var cancelLabel = prompt.CancelLabel;
            if (string.IsNullOrWhiteSpace(cancelLabel) ||
                string.Equals(cancelLabel.Trim(), prompt.ConfirmLabel?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                cancelLabel = "Hủy";
            }
            view.Bind(prompt.Text, new[] { prompt.ConfirmLabel, cancelLabel });
            view.Chosen += index =>
            {
                Close(view);
                if (index == 0) onYes();
            };
            view.Closed += () => Close(view);

            Push(view, view.gameObject);
        }

        private void Push(object screen, GameObject view)
        {
            _views[screen] = view;
            _stack.Push(screen);
        }

        /// <summary>Đóng một màn hình cụ thể, kể cả khi nó không nằm trên cùng.</summary>
        private void Close(object screen)
        {
            if (!_stack.Remove(screen)) return;
            DestroyView(screen);

            // Bỏ tham chiếu popup shop khi nó bị đóng — không thì lần sau MenuShown
            // vẫn cố gọi TryConsumeMenu trên view đã Destroy.
            if (ReferenceEquals(screen, _shopPopup)) _shopPopup = null;
            if (ReferenceEquals(screen, _atmPopup)) _atmPopup = null;
            if (ReferenceEquals(screen, _tranChanTabs)) _tranChanTabs = null;
        }

        private void DestroyView(object screen)
        {
            if (!_views.TryGetValue(screen, out var view)) return;

            _views.Remove(screen);
            if (view != null) Destroy(view);
        }

        /// <summary>Chỉ màn hình trên cùng được hiện; những cái dưới vẫn sống nhưng ẩn.</summary>
        private void OnTopChanged(object top)
        {
            foreach (var pair in _views)
            {
                if (pair.Value != null) pair.Value.SetActive(ReferenceEquals(pair.Key, top));
            }
        }

        private void OnDestroy()
        {
            UnbindPetUpgrade();
            UnbindGems();
            UnbindAnimationMenus();
            if (_guider == null) return;

            _guider.MenuShown -= ShowMenu;
            _guider.ListOptionShown -= ShowListOption;
            _guider.YesNoAsked -= ShowYesNo;
            _guider.InputDialogShown -= ShowInputDialog;
            _guider.NpcOptionsShown -= ShowNpcOptions;
            _guider.PopupShown -= ShowPopup;
            _guider.ImageDialogShown -= ShowImageDialog;
        }
    }
}
