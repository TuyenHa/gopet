using System;
using Gopet.Net.Guider;
using Gopet.Net.Npc;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup <b>Bang hội</b> — NPC "Sứ giả bang hội" (npcId −15).
    ///
    /// <para>Số tab do server quyết: 5 tuỳ chọn cố định trong DB, cộng "Nhận nhiệm vụ
    /// chính" mà <c>MenuController.showNpcOption</c> chèn thêm khi NPC đang có nhiệm
    /// vụ cho người chơi. Vì thế nhãn tab đặt theo <b>option id</b>, không theo vị trí
    /// — chèn thêm một tuỳ chọn là mọi vị trí lệch hết.</para>
    ///
    /// <para>Ba tab dựng nội dung ngay trong popup thay vì để server bơm dialog riêng
    /// đè lên: xem <c>GuildNpcTabsView.Pages.cs</c>.</para>
    /// </summary>
    public sealed partial class GuildNpcTabsView : MonoBehaviour
    {
        private const float Width = 520f;
        private const float Height = 320f;
        private const string Footer = "Chọn một mục của bang hội";

        /// <summary>Bảng TOP LVL bang hội server trả về với <c>listId = -1</c> (<c>showTop</c>).</summary>
        private const int TopClanListId = -1;

        /// <summary>
        /// <c>MenuController.OP_MAIN_TASK</c> — tuỳ chọn server chèn thêm khi có nhiệm
        /// vụ chính. GIỮ khớp server.
        /// </summary>
        private const int OpMainTask = 0;

        private NpcOptions.Option[] _options;
        private Font _font;
        private GamePopupFrame _frame;
        private PopupTabRail _rail;
        private int _activeOptionId = -1;

        public event Action<int> OptionChosen;

        /// <summary>Người chơi gửi tên bang mới từ tab "Tạo". Tham số là tên đã cắt khoảng trắng.</summary>
        public event Action<string> CreateClanSubmitted;

        public event Action Closed;

        public static GuildNpcTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var frame = GamePopupFrame.Create(parent, font, "Bang hội", Width, Height,
                footer: Footer);
            frame.gameObject.name = "GuildNpcTabs";

            var view = frame.gameObject.AddComponent<GuildNpcTabsView>();
            view._frame = frame;
            view._font = font;
            view._options = options?.Options ?? Array.Empty<NpcOptions.Option>();
            frame.Closed += () => view.Closed?.Invoke();

            view.BuildTabs(frame);
            view.BuildPages(frame.Content);
            view.SelectFirstTab();
            return view;
        }

        /// <summary>Bảng TOP LVL hiện ngay trong tab "Top Lvl", không mở popup riêng.</summary>
        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            if (screen == null || _topList == null) return false;
            if (_activeOptionId != LinhThuCityNpcOptions.SuGiaTopLvlBangHoi) return false;
            if (screen.ListId != TopClanListId) return false;

            _topList.Bind(screen, assets, guider);
            ShowPage(_topPage);
            return true;
        }

        private void BuildTabs(GamePopupFrame frame)
        {
            if (_options.Length == 0) return;

            var labels = new string[_options.Length];
            for (var i = 0; i < _options.Length; i++) labels[i] = TabLabel(_options[i]);

            // Nhãn đã gọn còn một từ nên khay giữ chiều cao thường, bằng các popup
            // khác. Nhãn lạ (server gửi chuỗi khác) vẫn tự xuống dòng rồi cắt, không tràn.
            _rail = PopupTabRail.Create(frame.Content, _font, frame.ContentWidth, labels);
        }

        /// <summary>
        /// Nhãn tab gọn theo yêu cầu thiết kế. Chuỗi server dài dòng ("Cống hiến bang
        /// hội") và lặp chữ "bang hội" ở mọi tab, trong khi tiêu đề popup đã nói rồi.
        /// </summary>
        private static string TabLabel(NpcOptions.Option option)
        {
            switch (option.Id)
            {
                case LinhThuCityNpcOptions.SuGiaVaoKhuVucBang: return "Khu vực";
                case LinhThuCityNpcOptions.SuGiaTopLvlBangHoi: return "Top Lvl";
                case LinhThuCityNpcOptions.SuGiaTaoBangHoi: return "Tạo";
                case LinhThuCityNpcOptions.SuGiaSuKienBangHoi: return "Sự kiện";
                case LinhThuCityNpcOptions.SuGiaCongHienBangHoi: return "Cống hiến";
                case OpMainTask: return "Nhiệm vụ";
                default: return option.Text ?? string.Empty;
            }
        }

        /// <summary>
        /// Chọn tab đầu KHÔNG báo server, rồi mới nối sự kiện của khay tab:
        /// <c>Select</c> chỉ bắn khi tab đổi, nối trước là lần chọn đầu cũng gửi option.
        /// </summary>
        private void SelectFirstTab()
        {
            if (_options.Length == 0) return;

            _rail.Select(0);
            SelectTab(_options[0].Id, notify: false);
            _rail.Selected += index => SelectTab(_options[index].Id);
        }

        private void SelectTab(int optionId, bool notify = true)
        {
            _activeOptionId = optionId;

            // Hai tab có nội dung tự dựng và KHÔNG gửi gì khi chỉ mở ra xem:
            //  - "Khu vực bang": vào khu vực là hành động thật, để nút lo.
            //  - "Tạo": gửi option là server bơm hộp thoại nhập riêng đè lên popup;
            //    form nhập đã nằm sẵn trong tab.
            if (optionId == LinhThuCityNpcOptions.SuGiaVaoKhuVucBang)
            {
                ShowPage(_enterPage);
                return;
            }

            if (optionId == LinhThuCityNpcOptions.SuGiaTaoBangHoi)
            {
                _createForm.Reset();
                ShowPage(_createPage);
                return;
            }

            // Tab còn lại: server tự bơm màn hình tiếp theo. Tab Top Lvl chờ gói
            // MenuScreen rồi TryConsumeMenu mới hiện danh sách.
            ShowPage(optionId == LinhThuCityNpcOptions.SuGiaTopLvlBangHoi ? _topPage : null);
            if (notify) OptionChosen?.Invoke(optionId);
        }
    }
}
