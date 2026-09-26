using Dapper;
using Gopet.Util;
using MySqlConnector;

/// <summary>
/// Cờ "server này đang nắm giữ người chơi" cho web admin — bảng <c>player_online</c>
/// trong game DB (mỗi game DB = một GServer, xem phase-04-gserver-isonline-patch.md).
/// Web admin không đọc được RAM server nên chỉ tin bảng này + <see cref="ServerHeartbeat"/>
/// để quyết định có được sửa thẳng bảng <c>player</c> không.
///
/// Lỗi ghi cờ được LOG rồi báo cho caller qua giá trị trả về (không ném ra ngoài làm hỏng
/// luồng login/disconnect) — nhưng caller của <see cref="MarkOnline"/> PHẢI coi thất bại là
/// fail-closed (từ chối login), không được âm thầm cho vào game với cờ sai (code review
/// C260926 H1: MarkOnline trước đây nuốt lỗi rồi vẫn cho login tiếp, web thấy nhầm là online
/// nhưng thực ra flag không có).
/// </summary>
public static class PlayerOnlineRegistry
{
    /// <summary>
    /// Đánh dấu online — gọi TRONG vùng khoá <c>login_lock_&lt;username&gt;</c>, trước khi đọc
    /// bảng <c>player</c>. Trả về mốc <c>since</c> đã ghi (giờ máy, cắt về độ chính xác giây vì
    /// cột <c>since</c> là DATETIME không có phần giây lẻ — nếu không cắt, so sánh lại ở
    /// <see cref="MarkOffline"/> sẽ lệch với giá trị MySQL đã cắt khi lưu) khi thành công, hoặc
    /// <c>null</c> nếu ghi thất bại. Caller lưu giá trị này trên <c>Player.onlineFlagSince</c> để
    /// <see cref="MarkOffline"/> chỉ xoá đúng dòng do CHÍNH phiên này ghi (chống 1 phiên xoá
    /// nhầm cờ của phiên khác — code review C1).
    /// </summary>
    public static DateTime? MarkOnline(MySqlConnection gameConn, int userId)
    {
        try
        {
            DateTime since = new DateTime(
                DateTime.Now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond,
                DateTimeKind.Unspecified);
            gameConn.Execute(
                "REPLACE INTO `player_online` (`user_id`, `since`) VALUES (@userId, @since);",
                new { userId, since });
            return since;
        }
        catch (Exception e)
        {
            e.printStackTrace();
            return null;
        }
    }

    /// <summary>
    /// Xoá cờ online của 1 user — gọi lúc disconnect, sau khi save + remove khỏi
    /// <see cref="PlayerManager"/>, và CHỈ khi phiên gọi từng <see cref="MarkOnline"/> thành
    /// công (kiểm tra <c>ownsOnlineFlag</c> ở caller). <paramref name="since"/> phải là giá trị
    /// <see cref="MarkOnline"/> đã trả về cho phiên này — thêm điều kiện <c>since</c> vào WHERE
    /// làm lớp phòng thủ thứ hai: dù caller có tính sai <c>ownsOnlineFlag</c>, lệnh xoá cũng
    /// không đụng tới dòng do một phiên đăng nhập MỚI hơn (REPLACE) ghi đè sau đó.
    /// Không ném lỗi ra ngoài (lời gọi vẫn nên xoá best-effort).
    /// </summary>
    public static void MarkOffline(MySqlConnection gameConn, int userId, DateTime since)
    {
        try
        {
            gameConn.Execute(
                "DELETE FROM `player_online` WHERE `user_id` = @userId AND `since` = @since;",
                new { userId, since });
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    /// <summary>
    /// Xoá TOÀN BỘ cờ online lúc khởi động — chỉ của DB này (1 game DB = 1 server nên an
    /// toàn). KHÔNG gọi lúc shutdown: sẽ xoá mất cờ của người vừa save lỗi lúc disconnect
    /// (cố tình giữ lại để web coi là online, không sửa đè lên).
    /// </summary>
    public static void ResetAllForThisServer()
    {
        try
        {
            using var conn = MYSQLManager.create();
            conn.Execute("DELETE FROM `player_online`;");
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}
