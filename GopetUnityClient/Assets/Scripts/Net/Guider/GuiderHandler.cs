using System;

namespace Gopet.Net.Guider
{
    /// <summary>
    /// Dịch họ gói UI thành sự kiện. Thuần C#, không UnityEngine — tầng trình bày
    /// nghe sự kiện, còn việc đọc gói được test ngoài Editor.
    ///
    /// <para>Hai bao ngoài, không phải một: menu và dialog đi trong
    /// <c>COMMAND_GUIDER</c> (122), riêng hộp thoại Có/Không đi trong
    /// <c>SERVER_MESSAGE</c> (45).</para>
    ///
    /// <para><b>Class này cố ý không biết gì về nghiệp vụ.</b> Nó không phân biệt
    /// shop với kho đồ; nó chỉ chuyển tiếp danh sách. Giữ được như vậy thì 162 màn
    /// hình của server chạy mà không cần một dòng code riêng nào.</para>
    /// </summary>
    public sealed class GuiderHandler
    {
        private readonly Action<Message> _send;

        public GuiderHandler(Action<Message> send)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
        }

        /// <summary>Server gửi một màn hình danh sách. Dùng cho hầu hết mọi menu trong game.</summary>
        public event Action<MenuScreen> MenuShown;

        /// <summary>Màn hình lựa chọn đơn giản (không icon). Chọn dòng vẫn trả lời bằng SELECT_MENU_ELEMENT.</summary>
        public event Action<ListOptionScreen> ListOptionShown;

        public event Action<NpcOptions> NpcOptionsShown;

        public event Action<InputDialogSpec> InputDialogShown;

        public event Action<YesNoRequest> YesNoAsked;

        public event Action<string> PopupShown;

        public event Action<string> BannerShown;

        /// <summary>Thông báo boss chuyên biệt; ticker gameplay chỉ nghe sự kiện này.</summary>
        public event Action<string> BossBannerShown;

        public event Action<ImageDialogSpec> ImageDialogShown;

        /// <summary>Server gửi trạng thái lịch điểm danh theo tháng.</summary>
        public event Action<DailyCheckinState> DailyCheckinShown;

        /// <summary>Server gửi danh mục/sở hữu/lựa chọn khung cảnh màn đấu.</summary>
        public event Action<BattleSceneState> BattleSceneStateReceived;

        public void RegisterOn(MessageRouter router)
        {
            router.RegisterEnvelope(GopetCmd.COMMAND_GUIDER);
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.SHOW_MENU_ITEM,
                m => MenuShown?.Invoke(MenuScreen.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GuiderPackets.NpcOption,
                m => NpcOptionsShown?.Invoke(NpcOptions.Parse(m)));

            // Sub 3 chiều XUỐNG là GUIDER_LIST_OPTION, chiều LÊN là SELECT_MENU_ELEMENT.
            // Router chỉ thấy chiều xuống nên không nhập nhằng.
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.GUIDER_LIST_OPTION,
                m => ListOptionShown?.Invoke(ListOptionScreen.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.TYPE_DIALOG_INPUT,
                m => InputDialogShown?.Invoke(InputDialogSpec.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.GUIDER_IMGDIALOG,
                m => ImageDialogShown?.Invoke(ImageDialogSpec.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.TYPE_DAILY_CHECKIN_STATE,
                m => DailyCheckinShown?.Invoke(DailyCheckinState.Parse(m)));
            router.RegisterSub(GopetCmd.COMMAND_GUIDER, GopetCmd.TYPE_BATTLE_BG_STATE,
                m => BattleSceneStateReceived?.Invoke(BattleSceneState.Parse(m)));

            router.RegisterEnvelope(GopetCmd.SERVER_MESSAGE);
            router.RegisterSub(GopetCmd.SERVER_MESSAGE, GopetCmd.SEND_YES_NO,
                m => YesNoAsked?.Invoke(YesNoRequest.Parse(m)));
            router.RegisterSub(GopetCmd.SERVER_MESSAGE, GopetCmd.POPUP_MESSAGE,
                m => PopupShown?.Invoke(ServerTextMessage.Parse(m, "POPUP_MESSAGE")));
            router.RegisterSub(GopetCmd.SERVER_MESSAGE, GopetCmd.BANNER_MESSAGE,
                m => BannerShown?.Invoke(ServerTextMessage.Parse(m, "BANNER_MESSAGE")));
            router.RegisterSub(GopetCmd.SERVER_MESSAGE, GopetCmd.BOSS_BANNER_MESSAGE,
                m => BossBannerShown?.Invoke(ServerTextMessage.Parse(m, "BOSS_BANNER_MESSAGE")));
        }

        /// <summary>
        /// Chọn một dòng. Tự đi đúng đường tuỳ dòng đó có lựa chọn thanh toán hay không.
        /// </summary>
        public void Select(MenuScreen screen, int index, int paymentIndex = -1)
        {
            if (screen == null) throw new ArgumentNullException(nameof(screen));
            if (index < 0 || index >= screen.Items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Dòng {index} nằm ngoài màn hình {screen.ListId}.");
            }

            if (!screen.Items[index].CanSelect)
            {
                throw new InvalidOperationException($"Dòng {index} của màn hình {screen.ListId} không cho chọn.");
            }

            // Gửi ItemId, KHÔNG phải chỉ số dòng.
            //
            // Trường int đầu mỗi dòng là giá trị server tự chọn để DỘI LẠI:
            // `putInt(itemId)` nếu dòng có id riêng, ngược lại `putInt(i)`
            // (GameController.cs:837-844). Bên tiêu thụ dùng nó như một ID thật —
            // `clan.getJoinRequestByUserId(index)`, và `index == -1` cho pet đang
            // đeo (selectMenu.cs:423, 1475). Chỉ số dòng không bao giờ âm được.
            //
            // Dòng không có id riêng thì ItemId ĐÃ BẰNG chỉ số dòng, nên cách này
            // đúng cho cả hai trường hợp mà không cần nhánh nào.
            var echo = screen.Items[index].ItemId;

            _send(paymentIndex >= 0
                ? GuiderPackets.SelectMenuElementWithPayment(screen.ListId, echo, paymentIndex)
                : GuiderPackets.SelectMenuElement(screen.ListId, echo));
        }

        /// <summary>
        /// Chọn một dòng của màn hình lựa chọn đơn giản. Dùng chung đường
        /// <c>SELECT_MENU_ELEMENT</c> với <see cref="MenuScreen"/>.
        ///
        /// <para>Gửi <c>Option.Id</c>, KHÔNG phải vị trí dòng. Id không nhất thiết
        /// theo thứ tự: menu lời mời kết bạn dùng <c>0, 1, 3, 2</c>
        /// (<c>sendMenu.cs:245-249</c>) và handler là <c>switch</c> trên chính id đó
        /// (<c>selectMenu.cs:2064</c>) — gửi vị trí thì "Từ chối tất cả" chạy nhầm
        /// thành "Từ chối và chặn".</para>
        ///
        /// <para>Màn ATM có id trùng vị trí nên dump của client J2ME KHÔNG phân biệt
        /// được hai cách; đừng lấy nó làm bằng chứng.</para>
        /// </summary>
        public void Select(ListOptionScreen screen, int index)
        {
            if (screen == null) throw new ArgumentNullException(nameof(screen));
            if (index < 0 || index >= screen.Options.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Dòng {index} nằm ngoài màn hình {screen.ListId}.");
            }

            _send(GuiderPackets.SelectMenuElement(screen.ListId, screen.Options[index].Id));
        }

        public void AnswerYesNo(int dialogId, bool yes) => _send(GuiderPackets.AnswerYesNo(dialogId, yes));

        public void SelectImageDialog(int dialogId) => _send(GuiderPackets.SelectImageDialog(dialogId));

        public void SubmitInput(int dialogId, string[] texts) => _send(GuiderPackets.SubmitInputDialog(dialogId, texts));

        public void SelectNpcOption(int npcId, int optionId) => _send(GuiderPackets.SelectNpcOption(npcId, optionId));

        public void TalkToNpc(int npcId) => _send(GuiderPackets.TalkToNpc(npcId));

        /// <summary>
        /// Mở cửa hàng. Server đáp bằng <see cref="MenuScreen"/> trên
        /// <see cref="MenuShown"/> — <c>ListId</c> khớp <paramref name="shopId"/>.
        /// </summary>
        public void RequestShop(sbyte shopId) => _send(GuiderPackets.RequestShop(shopId));

        /// <summary>Mở tab điểm danh — server đáp bằng <see cref="DailyCheckinShown"/>.</summary>
        public void RequestDailyCheckin() => _send(GuiderPackets.RequestDailyCheckin());

        public void RequestNextTaskGuide() => _send(GuiderPackets.RequestNextTaskGuide());

        /// <summary>Bấm nút điểm danh — server phát quà rồi gửi lại trạng thái mới.</summary>
        public void DoDailyCheckin() => _send(GuiderPackets.DoDailyCheckin());

        public void RequestBattleScenes() => _send(GuiderPackets.RequestBattleScenes());

        public void BuyBattleScene(int sceneId) => _send(GuiderPackets.BuyBattleScene(sceneId));

        public void SelectBattleScene(int sceneId) => _send(GuiderPackets.SelectBattleScene(sceneId));
    }
}
