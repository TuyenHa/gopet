using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class UiRoot
    {
        /// <summary>Dialog lỗi đỏ top-level opcode 10, chỉ gọi khi đã qua màn đăng nhập.</summary>
        public void ShowServerError(string text) => ShowServerDialog(text, true);

        /// <summary>Dialog thành công xanh top-level opcode 71, chỉ gọi khi đã qua màn đăng nhập.</summary>
        public void ShowServerSuccess(string text) => ShowServerDialog(text, false);

        private void ShowPopup(string text)
        {
            ShowToast(text);
        }

        private void ShowServerDialog(string text, bool isError)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var view = ChoiceDialogView.Create(transform, _font);
            var panel = view.GetComponent<Image>();
            panel.color = isError
                ? new Color(0.42f, 0.08f, 0.09f, 0.98f)
                : new Color(0.05f, 0.34f, 0.19f, 0.98f);
            view.Bind(text, new[] { "Đóng" });
            view.Chosen += _ => Close(view);
            Push(view, view.gameObject);
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
