using System;
using Gopet.Net;
using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Kiểm parser trạng thái điểm danh + 2 gói yêu cầu (OPEN/DO). Dựng gói theo đúng
    /// layout server <c>GameController.SendDailyCheckinState</c>: todayDay(sbyte),
    /// receivedMask(int), daysInMonth(sbyte), rồi mỗi ngày state(sbyte)+label(utf)+iconPath(utf).
    /// </summary>
    public sealed class DailyCheckinStateTests
    {
        /// <summary>Bóc opcode bao ngoài, để sub-command làm Id — đúng cái MessageRouter đưa cho handler.</summary>
        private static DailyCheckinState ParseBuilt(Message built)
        {
            var full = built.ToWire();
            var body = new byte[full.Length - 1];
            Array.Copy(full, 1, body, 0, body.Length);
            return DailyCheckinState.Parse(Message.FromWire(body, false));
        }

        private static Message BuildStateWire(int today, int mask, int days,
            (byte state, string label, string iconPath)[] cells)
        {
            var m = Message.Create(GopetCmd.COMMAND_GUIDER)
                .PutSByte(GopetCmd.TYPE_DAILY_CHECKIN_STATE)
                .PutSByte((sbyte)today)
                .PutInt(mask)
                .PutSByte((sbyte)days);
            foreach (var c in cells)
            {
                m.PutSByte((sbyte)c.state).PutUtf(c.label).PutUtf(c.iconPath);
            }
            return m;
        }

        [Fact]
        public void Parse_DocDungSoNgayNhanVaTrangThai()
        {
            var cells = new (byte, string, string)[3];
            cells[0] = (DailyCheckinState.Received, "Bình x2 EXP x3", "items/61030.png");
            cells[1] = (DailyCheckinState.Claimable, "Kim cương x5", "items/61010.png");
            cells[2] = (DailyCheckinState.Locked, "200 (ngoc)", "");

            var state = ParseBuilt(BuildStateWire(today: 2, mask: 0b01, days: 3, cells));

            Assert.Equal(2, state.TodayDay);
            Assert.Equal(0b01, state.ReceivedMask);
            Assert.Equal(3, state.DaysInMonth);
            Assert.Equal(3, state.Days.Length);

            Assert.Equal(1, state.Days[0].Day);
            Assert.Equal(DailyCheckinState.Received, state.Days[0].State);
            Assert.Equal("Bình x2 EXP x3", state.Days[0].Label);
            Assert.Equal("items/61030.png", state.Days[0].IconPath);

            Assert.Equal(DailyCheckinState.Claimable, state.Days[1].State);
            Assert.Equal("items/61010.png", state.Days[1].IconPath);

            Assert.Equal(DailyCheckinState.Locked, state.Days[2].State);
            Assert.Equal("", state.Days[2].IconPath);
        }

        [Fact]
        public void CanCheckinToday_DungKhiOHomNayLaClaimable()
        {
            var cells = new (byte, string, string)[2];
            cells[0] = (DailyCheckinState.Received, "a", "items/1.png");
            cells[1] = (DailyCheckinState.Claimable, "b", "items/2.png"); // ngày 2 = hôm nay
            var state = ParseBuilt(BuildStateWire(today: 2, mask: 0b01, days: 2, cells));

            Assert.True(state.CanCheckinToday);
        }

        [Fact]
        public void CanCheckinToday_SaiKhiHomNayDaNhan()
        {
            var cells = new (byte, string, string)[2];
            cells[0] = (DailyCheckinState.Received, "a", "items/1.png");
            cells[1] = (DailyCheckinState.Received, "b", "items/2.png"); // ngày 2 = hôm nay, đã nhận
            var state = ParseBuilt(BuildStateWire(today: 2, mask: 0b11, days: 2, cells));

            Assert.False(state.CanCheckinToday);
        }

        [Fact]
        public void RequestDailyCheckin_DungOpcodeVaSub()
        {
            // 7a (COMMAND_GUIDER=122) + 29 (TYPE_DAILY_CHECKIN_OPEN=41)
            using var m = GuiderPackets.RequestDailyCheckin();
            Assert.Equal("7a29", TestVectorData.ToHex(m.ToWire()));
        }

        [Fact]
        public void DoDailyCheckin_DungOpcodeVaSub()
        {
            // 7a (122) + 2a (TYPE_DAILY_CHECKIN_DO=42)
            using var m = GuiderPackets.DoDailyCheckin();
            Assert.Equal("7a2a", TestVectorData.ToHex(m.ToWire()));
        }

        // ==== Streak ====
        // mask bit d-1 tương ứng ngày d. Kiểm 4 tình huống điển hình + edge cases.

        [Fact]
        public void Streak_HomNayChuaNhan_DemNguocTuHomQua()
        {
            // Ngày 1-5 đã nhận (mask=0b011111), hôm nay=6 chưa nhận → streak=5
            var s = new DailyCheckinState { TodayDay = 6, ReceivedMask = 0b011111 };
            Assert.Equal(5, s.Streak);
        }

        [Fact]
        public void Streak_HomNayDaNhan_BaoGomHomNay()
        {
            // 1-6 đã nhận, hôm nay=6 → streak=6
            var s = new DailyCheckinState { TodayDay = 6, ReceivedMask = 0b111111 };
            Assert.Equal(6, s.Streak);
        }

        [Fact]
        public void Streak_CoNgayLo_ChuoiChiTinhDoanCuoi()
        {
            // Nhận 1, lỡ 2, nhận 3-5, hôm nay=6 chưa nhận → streak chỉ đếm 3-5 = 3
            var s = new DailyCheckinState { TodayDay = 6, ReceivedMask = 0b011101 };
            Assert.Equal(3, s.Streak);
        }

        [Fact]
        public void Streak_HomQuaCungLo_TraveVe0()
        {
            // Hôm nay=6 chưa nhận, ngày 5 (bit4) cũng chưa nhận → streak=0
            var s = new DailyCheckinState { TodayDay = 6, ReceivedMask = 0b001111 };
            Assert.Equal(0, s.Streak);
        }

        [Fact]
        public void Streak_NgayDauThang_KhongCrash()
        {
            var s = new DailyCheckinState { TodayDay = 1, ReceivedMask = 0 };
            Assert.Equal(0, s.Streak);
        }
    }
}
