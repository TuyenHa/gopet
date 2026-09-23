using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup danh sách nhiệm vụ, dựng trên <see cref="GamePopupFrame"/> nên cùng khung,
    /// badge tiêu đề và bảng màu với popup cửa hàng.
    ///
    /// <para>Nhận cả hai menu nhiệm vụ của server: <c>MENU_SHOW_MY_LIST_TASK</c> (1034,
    /// nhiệm vụ đang nhận) và <c>MENU_SHOW_LIST_TASK</c> (1033, nhiệm vụ NPC giao) —
    /// <c>MenuController.sendMenu.cs</c>. Dòng nào cũng có <c>showDialog</c> nên bấm vào
    /// là đi qua hộp xác nhận, y như <see cref="GenericMenuView"/>.</para>
    ///
    /// <para>Dòng là <see cref="PopupTextRow"/> trong suốt, cách nhau bằng vạch mảnh:
    /// không có nền đặc tô đè lên góc bo nên viền ngoài liền mạch, bốn góc không trắng.</para>
    /// </summary>
    public sealed class TaskListPopupView : MonoBehaviour
    {
        public const int NpcTaskMenuId = 1033;
        public const int MyTaskMenuId = 1034;

        private readonly List<PopupTextRow> _rows = new List<PopupTextRow>();

        private GuiderHandler _guider;
        private Font _font;
        private GamePopupFrame _frame;
        private PopupItemList _list;
        private MenuScreen _screen;

        public event Action Closed;

        /// <summary>Hỏi trước khi chọn. <see cref="UiRoot"/> dựng hộp thoại đè lên popup.</summary>
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        public int RowCount => _rows.Count;

        public static bool IsTaskMenu(MenuScreen screen) =>
            screen != null && (screen.ListId == NpcTaskMenuId || screen.ListId == MyTaskMenuId);

        public static TaskListPopupView Create(Transform parent, Font font, GuiderHandler guider)
        {
            var frame = GamePopupFrame.Create(parent, font, "Nhiệm vụ", footer: string.Empty);
            frame.gameObject.name = "TaskListPopup";

            var view = frame.gameObject.AddComponent<TaskListPopupView>();
            view._frame = frame;
            view._guider = guider;
            view._font = font;
            frame.Closed += () => view.Closed?.Invoke();

            view._list = PopupItemList.Create(frame.Content, font);
            return view;
        }

        public void Bind(MenuScreen screen)
        {
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));

            foreach (var row in _rows)
                if (row != null) Destroy(row.gameObject);
            _rows.Clear();

            var items = screen.Items ?? Array.Empty<MenuItemInfo>();
            for (var i = 0; i < items.Length; i++)
            {
                var row = PopupTextRow.Create(_list.Rows, _font);
                row.Bind(items[i]?.Title, items[i]?.Description);
                // Dòng cuối tắt vạch: để lại là thừa một nét sát mép trong của khung.
                row.SetSeparatorVisible(i < items.Length - 1);
                ((RectTransform)row.transform).anchoredPosition =
                    new Vector2(0f, -i * PopupTextRow.Height);

                var captured = i;
                row.Clicked += () => OnRowClicked(captured);
                _rows.Add(row);
            }

            _list.SetRowsHeight(items.Length * PopupTextRow.Height);
            _list.ShowPlaceholder(items.Length == 0 ? "Chưa có nhiệm vụ nào." : null);
            _frame.SetFooter(screen.ListId == NpcTaskMenuId
                ? "Chạm vào nhiệm vụ để nhận."
                : "Chạm vào nhiệm vụ để xem tiến độ.");
        }

        /// <summary>Công khai để test gọi thẳng, khỏi phải mò <c>Button</c> trong cây GameObject.</summary>
        public void OnRowClicked(int index)
        {
            if (_screen == null || _guider == null) return;
            if (index < 0 || index >= _screen.Items.Length) return;

            switch (MenuSelection.Decide(_screen, index))
            {
                case MenuAction.Confirm when ConfirmRequested != null:
                    ConfirmRequested(MenuSelection.PromptFor(_screen, index), () => Send(index));
                    return;
                case MenuAction.Confirm:
                case MenuAction.Send:
                    Send(index);
                    return;
            }
        }

        private void Send(int index)
        {
            var screen = _screen;
            _guider.Select(screen, index);
            if (MenuSelection.ShouldCloseAfter(screen, index)) Closed?.Invoke();
        }
    }
}
