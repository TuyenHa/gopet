using System;
using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Chồng màn hình đang mở. Thuần C#, không UnityEngine — tầng view chỉ hiện
    /// đúng cái đang ở trên cùng.
    ///
    /// <para>Server có thể gửi màn hình mới đè lên màn hình đang mở (chọn một dòng
    /// ATM thì ra menu con). Không quản lý thứ tự thì nút back đóng nhầm, hoặc tệ
    /// hơn: đóng hết mà vẫn còn một lớp che, người chơi kẹt không thoát được.</para>
    /// </summary>
    public sealed class DialogStack
    {
        /// <summary>
        /// Chặn chồng vô hạn. Server lỗi hoặc client hiểu nhầm mà đẩy mãi thì
        /// người chơi phải bấm back hàng nghìn lần — thà nổ ra để thấy.
        /// </summary>
        public const int MaxDepth = 32;

        private readonly List<object> _stack = new List<object>();

        public int Depth => _stack.Count;

        public bool IsEmpty => _stack.Count == 0;

        /// <summary>Màn hình đang hiện, hoặc <c>null</c> khi không còn gì.</summary>
        public object Top => _stack.Count == 0 ? null : _stack[_stack.Count - 1];

        /// <summary>Bắn khi màn hình trên cùng đổi — kể cả khi đổi thành <c>null</c>.</summary>
        public event Action<object> TopChanged;

        public void Push(object screen)
        {
            if (screen == null) throw new ArgumentNullException(nameof(screen));

            if (_stack.Count >= MaxDepth)
            {
                throw new InvalidOperationException(
                    $"Chồng màn hình đã {MaxDepth} lớp — nhiều khả năng đang đẩy lặp do lỗi.");
            }

            _stack.Add(screen);
            TopChanged?.Invoke(screen);
        }

        /// <summary>
        /// Đóng màn hình trên cùng. Trả <c>false</c> khi không còn gì để đóng —
        /// lúc đó tầng trên tự quyết (thoát game, về màn chính…).
        /// </summary>
        public bool Pop()
        {
            if (_stack.Count == 0) return false;

            _stack.RemoveAt(_stack.Count - 1);
            TopChanged?.Invoke(Top);
            return true;
        }

        /// <summary>
        /// Đóng đúng một màn hình cụ thể, kể cả khi nó không nằm trên cùng.
        ///
        /// <para>Cần vì server có thể đóng một màn hình ở giữa: dòng có
        /// <c>closeScreenAfterClick</c> đóng CHÍNH màn hình chứa nó, trong khi hộp
        /// xác nhận vừa mở vẫn đang nằm đè lên trên.</para>
        /// </summary>
        public bool Remove(object screen)
        {
            var index = _stack.LastIndexOf(screen);
            if (index < 0) return false;

            var wasTop = index == _stack.Count - 1;
            _stack.RemoveAt(index);

            if (wasTop) TopChanged?.Invoke(Top);
            return true;
        }

        public void Clear()
        {
            if (_stack.Count == 0) return;

            _stack.Clear();
            TopChanged?.Invoke(null);
        }

        /// <summary>Từ dưới lên trên. Dùng để view dựng lại toàn bộ khi cần.</summary>
        public IReadOnlyList<object> Screens => _stack;
    }
}
