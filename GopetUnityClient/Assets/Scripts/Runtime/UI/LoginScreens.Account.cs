using Gopet.UiLogic;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nửa "tài khoản" của <see cref="LoginScreens"/>: dựng màn đăng nhập, điền sẵn
    /// từ <see cref="CredentialStore"/>, và quyết định có lưu lại hay không.
    /// </summary>
    public sealed partial class LoginScreens
    {
        /// <summary>
        /// Màn đăng nhập dùng <see cref="LoginFormView"/> (bộ art riêng của dự án),
        /// KHÔNG dùng <see cref="FormView"/> như các chặng khác: nó có nền tranh, hai
        /// nút và ô ghi nhớ — không nhét vừa cái khuôn "tiêu đề + N ô + N nút" chung.
        /// </summary>
        private void ShowLogin()
        {
            var saved = _store != null ? _store.Load() : new SavedCredentials();

            BuildLogin(saved.Username, saved.Password);
        }

        /// <summary>Dựng form đăng nhập và điền sẵn dữ liệu được truyền vào.</summary>
        private void BuildLogin(string username, string password)
        {
            _loginForm = LoginFormView.Create(transform, _font, _sound);
            _loginForm.SetCredentials(username, password);
            _loginForm.SetNotice(_flow.Notice);

            _loginForm.SubmitRequested += (username, password) =>
            {
                // Ghi lại NGAY lúc gửi, không phải lúc thành công: tới lúc
                // LoginStage.Ready thì _loginForm đã bị ClearViews() huỷ mất rồi.
                _remember = _loginForm.Remember;

                if (!_flow.SubmitCredentials(username, password))
                {
                    _loginForm.SetNotice(_flow.Notice);
                    return;
                }

                _sentUsername = username;
                _sentPassword = password;
            };

            _loginForm.RegisterRequested += (username, password) =>
            {
                ShowRegistration(username, password);
            };
        }

        /// <summary>
        /// Mở màn đăng ký riêng. Nút ở màn đăng nhập chỉ điều hướng tới đây; gói
        /// REGISTER chỉ được gửi sau khi người dùng bấm nút "Đăng ký" trên form này.
        /// </summary>
        private void ShowRegistration(string username, string password)
        {
            ClearViews();

            _registrationForm = LoginFormView.CreateRegistration(transform, _font, _sound);
            _registrationForm.SetCredentials(username, password);

            _registrationForm.SubmitRegistrationRequested += (enteredUsername, enteredPassword) =>
            {
                if (!_flow.SubmitRegistration(enteredUsername, enteredPassword))
                {
                    _registrationForm.SetNotice(_flow.Notice);
                }
            };

            _registrationForm.BackRequested += () =>
            {
                var enteredUsername = _registrationForm.Username;
                var enteredPassword = _registrationForm.Password;
                ClearViews();
                BuildLogin(enteredUsername, enteredPassword);
            };
        }

        /// <summary>
        /// Hồi âm gói <c>REGISTER</c> — không đổi màn hình, chỉ cập nhật câu thông
        /// báo trên đúng khung đang hiện. Có thể tới sau khi màn đã đổi (người dùng
        /// tự đăng nhập trong lúc chờ) — cả hai đều <c>null</c> lúc đó, im lặng bỏ qua.
        /// </summary>
        private void OnRegisterReply(string text)
        {
            _loginForm?.SetNotice(text);
            _registrationForm?.SetNotice(text);
        }

        /// <summary>Chỉ lưu sau khi server đã nhận — mật khẩu sai thì lưu làm gì. Bỏ chọn "ghi nhớ" thì xoá luôn phần đã lưu trước đó, không để nó âm thầm sống sót.</summary>
        private void Remember()
        {
            if (_store == null || string.IsNullOrEmpty(_sentUsername)) return;

            if (_remember)
            {
                _store.Save(_sentUsername, _sentPassword);
            }
            else
            {
                _store.Clear();
            }
        }
    }
}
