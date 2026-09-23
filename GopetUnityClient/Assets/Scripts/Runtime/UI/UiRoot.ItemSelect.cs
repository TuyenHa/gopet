using Gopet.Net.Guider;

namespace Gopet.Runtime.UI
{
    /// <summary>Popup chọn một dòng (nguyên liệu hình xăm, tẩy gym…) — xem <see cref="ItemSelectPopupView"/>.</summary>
    public sealed partial class UiRoot
    {
        private ItemSelectPopupView _itemSelectPopup;

        /// <summary>Đang mở đúng loại thì chỉ bind lại; khác loại (khác tiêu đề) thì dựng lại khung.</summary>
        private void ShowItemSelectPopup(MenuScreen screen)
        {
            if (_itemSelectPopup != null && !_itemSelectPopup.CanBind(screen)) Close(_itemSelectPopup);
            if (_itemSelectPopup == null)
            {
                var view = ItemSelectPopupView.Create(transform, _font, screen, _guider, _assets);
                view.Closed += () => Close(view);
                view.ConfirmRequested += ShowConfirm;
                _itemSelectPopup = view;
                Push(view, view.gameObject);
            }
            _itemSelectPopup.Bind(screen);
        }
    }
}
