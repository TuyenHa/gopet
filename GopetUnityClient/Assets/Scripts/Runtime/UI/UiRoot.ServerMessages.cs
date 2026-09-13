using Gopet.Net.Guider;
namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        private ServerNoticeDialog _serverNotice;

        public void ShowServerError(string text)
        {
            ShowServerNotice(text, true);
            RefreshAtmAfterNotice();
        }

        public void ShowServerSuccess(string text)
        {
            ShowServerNotice(text, false);
            RefreshAtmAfterNotice();
        }

        private void RefreshAtmAfterNotice()
        {
            // Dựng notice trước để response không bị che hoặc bị thay thế bởi
            // lần request lại menu ATM. Request này chỉ cập nhật số dư/menu sau đó.
            if (_atmPopup != null) _atmPopup.RequestAtm();
        }

        private void ShowServerNotice(string text, bool error)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            if (_serverNotice != null) Close(_serverNotice);

            var view = ServerNoticeDialog.Create(transform, text, error);
            _serverNotice = view;
            view.Closed += () =>
            {
                if (_serverNotice == view) _serverNotice = null;
                Close(view);
            };
            Push(view, view.gameObject);
        }

        private void ShowPopup(string text)
        {
            ShowToast(text);
        }

        private void ShowImageDialog(ImageDialogSpec spec)
        {
            var view = ImageDialogView.Create(transform, _font);
            view.Bind(spec, _assets);
            view.Confirmed += () =>
            {
                _guider.SelectImageDialog(spec.DialogId);
                Close(view);
            };
            Push(view, view.gameObject);
        }
    }
}
