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
        /// <summary>Tab "Nhiệm vụ tiếp theo" — <c>MenuController.MENU_SHOW_NEXT_TASK_GUIDE</c>.</summary>
        public const int NextTaskGuideMenuId = 1093;

        private const int MainTab = 0;
        private const int NextTab = 1;
        private static readonly string[] TabLabels = { "Nhiệm vụ chính", "Nhiệm vụ tiếp theo" };

        private const float Width = 360f;
        private const float Height = 290f;

        private readonly List<PopupTextRow> _rows = new List<PopupTextRow>();

        private GuiderHandler _guider;
        private Font _font;
        private GamePopupFrame _frame;
        private PopupItemList _list;
        private RectTransform _body;
        private PopupTabRail _rail;
        private MenuScreen _screen;
        /// <summary>Danh sách 1034 mới nhất — quay lại tab chính thì bind lại, khỏi xin server
        /// (GameSession nuốt 1034 xin im lặng để cập nhật dòng HUD, sẽ không tới popup).</summary>
        private MenuScreen _myTasks;

        public event Action Closed;

        /// <summary>Hỏi trước khi chọn. <see cref="UiRoot"/> dựng hộp thoại đè lên popup.</summary>
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        public int RowCount => _rows.Count;

        public static bool IsTaskMenu(MenuScreen screen) =>
            screen != null && (screen.ListId == NpcTaskMenuId || screen.ListId == MyTaskMenuId
                               || screen.ListId == NextTaskGuideMenuId);

        public static TaskListPopupView Create(Transform parent, Font font, GuiderHandler guider)
        {
            var frame = GamePopupFrame.Create(parent, font, "Nhiệm vụ", Width, Height, footer: string.Empty);
            frame.gameObject.name = "TaskListPopup";
            frame.UseCompactChrome();

            var view = frame.gameObject.AddComponent<TaskListPopupView>();
            view._frame = frame;
            view._guider = guider;
            view._font = font;
            frame.Closed += () => view.Closed?.Invoke();

            var body = new GameObject("Body", typeof(RectTransform));
            body.transform.SetParent(frame.Content, false);
            view._body = (RectTransform)body.transform;
            view._body.anchorMin = Vector2.zero;
            view._body.anchorMax = Vector2.one;
            view._body.offsetMin = view._body.offsetMax = Vector2.zero;
            view._list = PopupItemList.Create(view._body, font);
            return view;
        }

        public void Bind(MenuScreen screen)
        {
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            if (screen.ListId == MyTaskMenuId) _myTasks = screen;
            // Danh sách NPC mời nhận (1033) không có tab: đó là màn chọn nhận nhiệm vụ.
            if (screen.ListId != NpcTaskMenuId)
            {
                EnsureTabs();
                _rail.Select(screen.ListId == NextTaskGuideMenuId ? NextTab : MainTab, notify: false);
            }
            BindRows(screen);
        }

        /// <summary>Dựng hàng tab lần đầu cần, đẩy danh sách xuống dưới khay.</summary>
        private void EnsureTabs()
        {
            if (_rail != null) return;
            _rail = PopupTabRail.Create(_frame.Content, _font, _frame.ContentWidth, TabLabels);
            _body.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
            _rail.Selected += OnTabSelected;
        }

        private void OnTabSelected(int tab)
        {
            if (tab == NextTab)
            {
                ShowLoading();
                _guider?.RequestNextTaskGuide();
            }
            else if (_myTasks != null)
            {
                _screen = _myTasks;
                BindRows(_myTasks);
            }
        }

        private void ShowLoading()
        {
            ClearRows();
            _list.SetRowsHeight(0f);
            _list.ShowPlaceholder("Đang tải...");
        }

        private void ClearRows()
        {
            foreach (var row in _rows)
                if (row != null) Destroy(row.gameObject);
            _rows.Clear();
        }

        private void BindRows(MenuScreen screen)
        {
            ClearRows();

            var items = screen.Items ?? Array.Empty<MenuItemInfo>();
            var y = 0f;
            for (var i = 0; i < items.Length; i++)
            {
                var row = PopupTextRow.Create(_list.Rows, _font);
                row.Bind(items[i]?.Title, items[i]?.Description);
                // Server >= 1.5.0 gửi tiến độ mỗi yêu cầu một hàng ("Tiêu diệt Khủng long
                // 3 / 10 (tại ...)") — dòng phải cao đủ cho hết các hàng, không thì bị cắt.
                var height = row.FitMultilineSubtitle();
                // Dòng cuối tắt vạch: để lại là thừa một nét sát mép trong của khung.
                row.SetSeparatorVisible(i < items.Length - 1);
                ((RectTransform)row.transform).anchoredPosition = new Vector2(0f, -y);
                y += height;

                var captured = i;
                row.Clicked += () => OnRowClicked(captured);
                _rows.Add(row);
            }

            _list.SetRowsHeight(y);
            var isGuide = screen.ListId == NextTaskGuideMenuId;
            _list.ShowPlaceholder(items.Length > 0 ? null
                : isGuide ? "Chưa có nhiệm vụ nào để nhận tiếp." : "Chưa có nhiệm vụ nào.");
            _frame.SetFooter(screen.ListId == NpcTaskMenuId ? "Chạm vào nhiệm vụ để nhận."
                : isGuide ? "Đi gặp NPC theo hướng dẫn để nhận nhiệm vụ."
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
