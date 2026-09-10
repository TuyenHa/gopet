using System;
using System.Collections.Generic;

namespace Gopet.Net.Player
{
    /// <summary>
    /// Một loại tiền hiển thị động do server bơm qua <c>MONEY_INFO</c>
    /// (danh sách <c>MoneyDisplays</c>). Icon nạp từ <c>iconPath</c>, số ở <c>Count</c>.
    /// </summary>
    public readonly struct MoneyDisplay
    {
        public MoneyDisplay(string iconPath, long count)
        {
            IconPath = iconPath;
            Count = count;
        }

        public string IconPath { get; }
        public long Count { get; }
    }

    /// <summary>
    /// Ảnh chụp stats nhân vật.
    ///
    /// <para><b>Char KHÔNG có HP/MP/Level</b> — đó là stat pet, không phải nhân vật.
    /// Đã kiểm chứng trong <c>PlayerData.cs</c>: char chỉ có <c>star, gold, coin, lua</c>
    /// và danh sách <c>MoneyDisplays</c> động.</para>
    ///
    /// <para>Server bơm 3 gói cập nhật riêng qua <c>PET_SERVICE</c>:</para>
    /// <list type="bullet">
    ///   <item><see cref="GopetCmd.MONEY_INFO"/> (25): star + gold + coin + lua + N money displays</item>
    ///   <item><see cref="GopetCmd.STAR_INFO"/> (94): chỉ star (dùng khi tiêu star lẻ)</item>
    ///   <item><see cref="GopetCmd.ENERGY_INFO"/> (102): star + 3 int (2 sau chưa rõ, server đẩy 0)</item>
    /// </list>
    /// </summary>
    public sealed class PlayerStats
    {
        public int Star { get; internal set; }
        public long Gold { get; internal set; }
        public long Coin { get; internal set; }
        public long Lua { get; internal set; }

        /// <summary>Các loại tiền/mảnh phụ do server đẩy lên; thay đổi theo sự kiện.</summary>
        public IReadOnlyList<MoneyDisplay> Extras { get; internal set; } = Array.Empty<MoneyDisplay>();

        /// <summary>Đúng lần snapshot; tăng mỗi lần server cập nhật để listener biết bỏ qua stale event.</summary>
        public int Version { get; internal set; }

        internal PlayerStats Copy()
        {
            var copy = (PlayerStats)MemberwiseClone();
            var arr = new MoneyDisplay[Extras.Count];
            for (var i = 0; i < arr.Length; i++) arr[i] = Extras[i];
            copy.Extras = arr;
            return copy;
        }
    }
}
