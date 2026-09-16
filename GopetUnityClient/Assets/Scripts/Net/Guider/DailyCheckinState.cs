namespace Gopet.Net.Guider
{
    /// <summary>
    /// Trạng thái lịch điểm danh theo tháng (sub-command <c>TYPE_DAILY_CHECKIN_STATE</c>
    /// trong <c>COMMAND_GUIDER</c>). Server dựng nhãn quà nên client chỉ hiển thị,
    /// không hardcode bảng quà.
    ///
    /// <para>Layout khớp <c>GameController.SendDailyCheckinState</c>:
    /// todayDay(sbyte), receivedMask(int), daysInMonth(sbyte),
    /// lặp daysInMonth: state(sbyte) + label(utf) + iconItemId(int).</para>
    /// </summary>
    public sealed class DailyCheckinState
    {
        /// <summary>Trạng thái 1 ngày (khớp enum server).</summary>
        public const byte Locked = 0;
        public const byte Claimable = 1;
        public const byte Received = 2;
        public const byte Missed = 3;

        public int TodayDay;
        public int ReceivedMask;
        public int DaysInMonth;
        public DayInfo[] Days;

        public sealed class DayInfo
        {
            public int Day;        // 1-based
            public byte State;
            public string Label;
            public int IconItemId; // 0 nếu quà không có item (ngọc/vàng/năng lượng)
        }

        public static DailyCheckinState Parse(Message message)
        {
            var r = message.Reader;
            var s = new DailyCheckinState
            {
                TodayDay = r.ReadSByte(),
                ReceivedMask = r.ReadInt(),
                DaysInMonth = r.ReadSByte(),
            };
            s.Days = new DayInfo[s.DaysInMonth];
            for (int i = 0; i < s.DaysInMonth; i++)
            {
                s.Days[i] = new DayInfo
                {
                    Day = i + 1,
                    State = (byte)r.ReadSByte(),
                    Label = r.ReadUtf(),
                    IconItemId = r.ReadInt(),
                };
            }
            return s;
        }

        /// <summary>Hôm nay có điểm danh được không (ô hôm nay đang ở trạng thái Claimable).</summary>
        public bool CanCheckinToday
        {
            get
            {
                if (Days == null) return false;
                foreach (var d in Days)
                {
                    if (d.Day == TodayDay) return d.State == Claimable;
                }
                return false;
            }
        }
    }
}
