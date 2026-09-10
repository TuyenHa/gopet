using System;
using Gopet.Net.Auth;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Nửa "đăng nhập bằng gì" của <see cref="LoginFlow"/>: tài khoản, mật khẩu,
    /// tạo nhân vật. Nửa kia (<c>LoginFlow.cs</c>) lo "nối tới đâu".
    /// </summary>
    public sealed partial class LoginFlow
    {
        /// <summary>Đã gửi <c>REGISTER</c>, đang chờ <c>OkDialog</c>/<c>RedDialog</c> trả lời — xem <see cref="OnDialog"/>.</summary>
        private bool _awaitingRegisterReply;

        /// <summary>Server trả lời gói <c>REGISTER</c> — thành công lẫn thất bại đều chỉ là một câu chữ, không đổi <see cref="Stage"/>.</summary>
        public event Action<string> RegisterReplyReceived;

        /// <summary>
        /// Trả <c>false</c> kèm lý do trong <see cref="Notice"/> khi chưa gửi được.
        /// Không đổi <see cref="Stage"/>: khác <c>LOGIN</c>, server không đóng kết
        /// nối sau khi trả lời <c>REGISTER</c> nên màn đăng nhập vẫn đứng nguyên.
        /// </summary>
        public bool SubmitRegistration(string username, string password)
        {
            if (!IsConnected)
            {
                Notice = "Đang nối lại máy chủ, thử lại sau giây lát.";
                return false;
            }

            // Đứng ở màn khác (đang đăng nhập, đang tạo nhân vật...) hoặc đã gửi
            // rồi mà chưa có hồi âm thì bấm lại vô nghĩa — một REGISTER một lúc.
            if (Stage != LoginStage.EnteringCredentials || _awaitingRegisterReply) return false;

            if (!AuthRules.IsValidRegistration(username, password, out var error))
            {
                Notice = error;
                return false;
            }

            _awaitingRegisterReply = true;
            SendRequested?.Invoke(AuthPackets.Register(username, password));
            return true;
        }

        /// <summary>
        /// Dialog chung (<c>OkDialog</c>/<c>RedDialog</c>) từ <see cref="Gopet.Net.Auth.AuthHandler"/>.
        /// Chỉ có nghĩa là hồi âm <c>REGISTER</c> khi đang thật sự chờ nó — mọi
        /// trường hợp khác (sai mật khẩu, sai OTP, sai phiên bản...) vẫn đi đúng
        /// đường cũ qua <see cref="OnLoginRejected"/>, không đổi hành vi.
        /// </summary>
        public void OnDialog(string text)
        {
            if (_awaitingRegisterReply)
            {
                _awaitingRegisterReply = false;
                RegisterReplyReceived?.Invoke(text);
                return;
            }

            OnLoginRejected(text);
        }

        /// <summary>
        /// Trả <c>false</c> kèm lý do trong <see cref="Notice"/> khi chưa gửi được:
        /// tên tài khoản sai, đang nối lại, hoặc đã bấm rồi mà chưa có hồi âm.
        /// </summary>
        public bool SubmitCredentials(string username, string password)
        {
            // Gửi lúc chưa có đường dây thì gói bị bỏ lặng lẽ và màn hình đứng mãi ở
            // "đang đăng nhập". Thà nói thẳng là đang nối lại.
            if (!IsConnected)
            {
                Notice = "Đang nối lại máy chủ, thử lại sau giây lát.";
                return false;
            }

            // Chạm hai lần trong một frame, hoặc bấm lại trong lúc chờ server.
            if (Stage == LoginStage.LoggingIn) return false;

            if (!AuthRules.IsValidUsername(username, out var error))
            {
                Notice = error;
                return false;
            }

            _username = username;
            _password = password ?? string.Empty;

            // Chỉ xoá câu của CHÍNH màn này. "Tên đã có người dùng" phải sống qua lần
            // đăng nhập lại mà server bắt làm ở giữa.
            if (_rejectionStage == LoginStage.EnteringCredentials) _rejection = null;

            SendLogin();
            return true;
        }

        public void OnLoginSucceeded(LoginSuccess success)
        {
            Success = success;
            _rejection = null;
            Enter(LoginStage.Ready, null);
        }

        /// <summary>
        /// Server từ chối, hoặc gửi dialog giữa lúc đăng nhập. Trả người dùng về ô
        /// nhập, giữ nguyên văn câu của server.
        ///
        /// <para>Hai kiểu từ chối, xử lý như nhau ở đây: sai mật khẩu thì server gửi
        /// <c>LOGIN_FAILED</c> rồi <b>đóng kết nối</b> (<c>Player.cs:655</c>); sai OTP
        /// thì chỉ gửi dialog đỏ và <b>giữ kết nối</b>
        /// (<c>MenuController.inputDialog.cs:731</c>). Câu từ chối được nhớ lại để cú
        /// đóng — nếu có — không xoá mất nó.</para>
        /// </summary>
        public void OnLoginRejected(string reason)
        {
            if (Stage != LoginStage.LoggingIn && Stage != LoginStage.CreatingCharacter) return;

            _rejection = reason;
            _rejectionStage = Stage == LoginStage.CreatingCharacter
                ? LoginStage.CreatingCharacter
                : LoginStage.EnteringCredentials;

            _autoLogin = false;

            // Cú đóng sau lời từ chối là cú đóng CỦA LỜI TỪ CHỐI, không phải của gói
            // tạo nhân vật vừa gửi. Để cờ kia sót lại thì lần rớt sau bị nuốt mất.
            _expectingCloseAfterCreate = false;

            // Đang ở màn tạo nhân vật thì ở lại đó: "tên đã có người dùng" phải hiện
            // NGAY TRÊN màn đang gõ tên, không phải trên màn đăng nhập.
            Enter(_rejectionStage, reason);

            // Server GIỮ kết nối khi sai OTP, nhưng lúc đó nó đã có `user` nên gói
            // LOGIN thứ hai bị bỏ qua không một lời hồi âm (`Player.cs:587`). Gửi lại
            // trên chính kết nối ấy là treo màn hình vĩnh viễn — phải nối lại.
            if (IsConnected) Reconnect();
        }

        /// <summary>Giữ lại câu từ chối CỦA MÀN NÀY ("tên đã có người dùng") — nó chính là thứ cần đọc ở đây.</summary>
        public void OnCharacterRequired() => Enter(LoginStage.CreatingCharacter, NoticeFor(LoginStage.CreatingCharacter));

        /// <summary>Câu từ chối nếu nó thuộc về chặng này, ngược lại <c>null</c>.</summary>
        private string NoticeFor(LoginStage stage) => _rejectionStage == stage ? _rejection : null;

        /// <summary>Trả <c>false</c> kèm lý do trong <see cref="Notice"/> khi tên nhân vật không hợp lệ.</summary>
        public bool SubmitCharacter(string name, sbyte gender)
        {
            if (!AuthRules.IsValidCharacterName(name, out var error))
            {
                Notice = error;
                return false;
            }

            // Server đóng kết nối ngay sau khi nhận gói này. Đặt hạn chờ vì cú đóng ấy
            // CHÍNH LÀ hồi âm — không thấy nó thì cũng là hỏng.
            _expectingCloseAfterCreate = true;
            if (_rejectionStage == LoginStage.CreatingCharacter) _rejection = null;
            ArmTimeout(true);

            SendRequested?.Invoke(AuthPackets.CreateCharacter(name, gender));
            return true;
        }

        /// <summary>
        /// Sau khi bắt tay lại. Chỉ tự đăng nhập khi vừa TẠO NHÂN VẬT xong — mọi
        /// trường hợp khác phải hỏi lại, kèm câu từ chối trước đó nếu có.
        /// </summary>
        private void Resume()
        {
            if (_autoLogin && !string.IsNullOrEmpty(_username))
            {
                _autoLogin = false;
                SendLogin();
                return;
            }

            Enter(LoginStage.EnteringCredentials, NoticeFor(LoginStage.EnteringCredentials));
        }

        private void SendLogin()
        {
            Enter(LoginStage.LoggingIn, null);
            SendRequested?.Invoke(AuthPackets.Login(_username, _password, _refCode, _clientInfo.Version));
        }
    }
}
