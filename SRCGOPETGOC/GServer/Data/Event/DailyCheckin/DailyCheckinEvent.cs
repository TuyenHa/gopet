using Gopet.Data.Collections;
using Gopet.Data.GopetItem;
using Gopet.Manager;
using System;

namespace Gopet.Data.Event.DailyCheckin
{
    /// <summary>
    /// Sự kiện điểm danh theo tháng (thường trực).
    /// Quà gắn theo NGÀY dương lịch (1..31), mỗi ngày điểm danh 1 lần, sang tháng tự reset.
    /// Ngày 28/29 phát hộp quà bí ẩn (item), người chơi tự mở sau → random loot.
    /// </summary>
    public sealed class DailyCheckinEvent : EventBase
    {
        public static readonly DailyCheckinEvent Instance = new DailyCheckinEvent();

        /// <summary>Trạng thái 1 ngày trên lịch điểm danh (khớp giá trị gửi client).</summary>
        public enum DayState : byte
        {
            Locked = 0,     // ngày tương lai
            Claimable = 1,  // hôm nay, chưa nhận
            Received = 2,   // đã nhận
            Missed = 3,     // ngày đã qua, không nhận
        }

        private DailyCheckinEvent()
        {
            this.Name = "Sự kiện điểm danh theo tháng";
        }

        /// <summary>Sự kiện thường trực → luôn bật.</summary>
        public override bool Condition { get => true; set { } }

        public override int[] ItemsOfEvent { get; set; } = new int[]
        {
            GopetManager.ID_BOX_CHECKIN_TUAN4,
            GopetManager.ID_BOX_CHECKIN_CUOITHANG,
        };

        #region Điểm danh
        public static int MonthKeyOf(DateTime now) => now.Year * 100 + now.Month;

        /// <summary>Mask hiệu dụng: nếu tháng lưu khác tháng hiện tại thì coi như chưa nhận gì (0).</summary>
        public static int EffectiveMask(PlayerData pd, DateTime now)
            => pd.DailyCheckinMonthKey == MonthKeyOf(now) ? pd.DailyCheckinMask : 0;

        public static DayState GetDayState(int effectiveMask, int day, int today)
        {
            bool received = (effectiveMask & (1 << (day - 1))) != 0;
            if (received) return DayState.Received;
            if (day == today) return DayState.Claimable;
            if (day < today) return DayState.Missed;
            return DayState.Locked;
        }

        /// <summary>Người chơi bấm nút điểm danh: phát quà của NGÀY hiện tại nếu chưa nhận.</summary>
        public void DoCheckin(Player player)
        {
            var pd = player.playerData;
            DateTime now = DateTime.Now;
            int monthKey = MonthKeyOf(now);
            int day = now.Day;

            // Sang tháng mới → reset mask
            if (pd.DailyCheckinMonthKey != monthKey)
            {
                pd.DailyCheckinMask = 0;
                pd.DailyCheckinMonthKey = monthKey;
            }

            int bit = 1 << (day - 1);
            if ((pd.DailyCheckinMask & bit) != 0)
            {
                player.redDialog(player.Language.DailyNoelFail); // "Đã điểm danh hôm nay rồi"
                return;
            }

            int[][] gift = GopetManager.DAILY_CHECKIN_GIFTS[day - 1];
            JArrayList<Popup> popups = player.controller.onReiceiveGift(gift);
            pd.DailyCheckinMask |= bit;

            JArrayList<string> textInfo = new();
            foreach (Popup popup in popups) textInfo.add(popup.getText());
            player.okDialog(string.Format(player.Language.GetGiftCodeOK, string.Join(", ", textInfo)));

            HistoryManager.addHistory(new History(player)
                .setLog($"Điểm danh tháng - ngày {day}")
                .setObj(new { Day = day, MonthKey = monthKey }));

            player.controller.SendDailyCheckinState(player);
        }
        #endregion

        #region Mở hộp quà bí ẩn
        public override void UseItem(int itemId, Player player)
        {
            switch (itemId)
            {
                case GopetManager.ID_BOX_CHECKIN_TUAN4:
                    OpenBox(player, itemId, GopetManager.BOX_CHECKIN_TUAN4_DATA);
                    break;
                case GopetManager.ID_BOX_CHECKIN_CUOITHANG:
                    OpenBox(player, itemId, GopetManager.BOX_CHECKIN_CUOITHANG_DATA);
                    break;
                default:
                    player.redDialog(player.Language.ThisEventItemIsMaterial);
                    break;
            }
        }

        /// <summary>Mở 1 hộp: trừ 1 hộp, random 1 phần thưởng từ loot pool.</summary>
        private void OpenBox(Player player, int boxItemId, int[][] lootData)
        {
            Item box = player.controller.selectItemsbytemp(boxItemId, GopetManager.NORMAL_INVENTORY);
            if (box == null || box.count <= 0)
            {
                player.redDialog("Bạn không có hộp quà này");
                return;
            }
            player.controller.subCountItem(box, 1, GopetManager.NORMAL_INVENTORY);

            JArrayList<Popup> popups = player.controller.onReiceiveGift(lootData);
            JArrayList<string> textInfo = new();
            foreach (Popup popup in popups) textInfo.add(popup.getText());
            player.okDialog(string.Format(player.Language.GetGiftCodeOK, string.Join(", ", textInfo)));
        }
        #endregion
    }
}
