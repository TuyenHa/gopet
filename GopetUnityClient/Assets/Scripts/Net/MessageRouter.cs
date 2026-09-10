using System;
using System.Collections.Generic;

namespace Gopet.Net
{
    /// <summary>
    /// Định tuyến gói tin tới handler theo opcode, và theo sub-command với
    /// các opcode bao ngoài.
    ///
    /// <para>Giao thức goPet dồn phần lớn nội dung vào 2 opcode bao ngoài:
    /// <c>PET_SERVICE</c> (81) mang gameplay, <c>COMMAND_GUIDER</c> (122) mang
    /// toàn bộ UI. Vì vậy router có 2 tầng — đăng ký sub-command trực tiếp
    /// thay vì mỗi handler tự đọc rồi switch.</para>
    ///
    /// <para>Handler chạy trên <b>luồng chính</b> (được gọi từ
    /// <c>GopetClient.Update()</c>), nên đụng Unity API thoải mái.</para>
    /// </summary>
    public sealed class MessageRouter
    {
        private readonly Dictionary<sbyte, Action<Message>> _topLevel = new Dictionary<sbyte, Action<Message>>();
        private readonly Dictionary<int, Action<Message>> _subCommands = new Dictionary<int, Action<Message>>();
        private readonly HashSet<sbyte> _envelopes = new HashSet<sbyte>();

        /// <summary>Gọi khi không có handler nào khớp. Mặc định không làm gì.</summary>
        public Action<Message> OnUnhandled { get; set; }

        /// <summary>Gọi khi handler ném lỗi. Mặc định ném tiếp.</summary>
        public Action<Message, Exception> OnError { get; set; }

        /// <summary>Đăng ký handler cho một opcode đứng riêng.</summary>
        public void Register(sbyte opcode, Action<Message> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (_topLevel.ContainsKey(opcode))
            {
                throw new InvalidOperationException($"Opcode {opcode} đã có handler.");
            }

            _topLevel[opcode] = handler;
        }

        /// <summary>
        /// Gỡ handler của một opcode. Cần cho các thành phần có vòng đời ngắn hơn
        /// router — không gỡ thì dựng lại lần hai sẽ ném "opcode đã có handler",
        /// ví dụ lúc nạp lại scene trong Unity.
        /// </summary>
        public bool Unregister(sbyte opcode)
        {
            return _topLevel.Remove(opcode);
        }

        /// <summary>
        /// Khai báo một opcode là "bao ngoài": byte đầu của thân gói là sub-command.
        /// Dùng cho <c>PET_SERVICE</c> và <c>COMMAND_GUIDER</c>.
        /// </summary>
        public void RegisterEnvelope(sbyte opcode)
        {
            _envelopes.Add(opcode);
        }

        /// <summary>Đăng ký handler cho một sub-command trong opcode bao ngoài.</summary>
        public void RegisterSub(sbyte envelopeOpcode, sbyte subCommand, Action<Message> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            if (!_envelopes.Contains(envelopeOpcode))
            {
                throw new InvalidOperationException(
                    $"Opcode {envelopeOpcode} chưa được khai báo qua RegisterEnvelope.");
            }

            var key = SubKey(envelopeOpcode, subCommand);
            if (_subCommands.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Sub-command {subCommand} của opcode {envelopeOpcode} đã có handler.");
            }

            _subCommands[key] = handler;
        }

        public void Dispatch(Message message)
        {
            try
            {
                if (_envelopes.Contains(message.Id))
                {
                    // Đọc sub-command khỏi thân gói; handler đọc tiếp từ đúng vị trí.
                    var sub = message.Reader.ReadSByte();
                    if (_subCommands.TryGetValue(SubKey(message.Id, sub), out var subHandler))
                    {
                        subHandler(message);
                        return;
                    }

                    OnUnhandled?.Invoke(message);
                    return;
                }

                if (_topLevel.TryGetValue(message.Id, out var handler))
                {
                    handler(message);
                    return;
                }

                OnUnhandled?.Invoke(message);
            }
            catch (Exception ex)
            {
                if (OnError == null) throw;
                OnError(message, ex);
            }
        }

        private static int SubKey(sbyte envelope, sbyte sub) => (envelope << 8) | (byte)sub;
    }
}
