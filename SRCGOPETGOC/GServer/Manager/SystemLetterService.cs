using Dapper;
using Gopet.Data.User;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gửi thư hệ thống (<see cref="Letter.ADMIN"/>, <see cref="Letter.EVENT"/>) — thư KHÔNG
/// có người gửi là người chơi: thông báo của ban quản trị, đền bù, quà sự kiện.
///
/// <para><b>Vì sao không dùng lại <c>GameController.sendLetter</c>:</b> hàm đó gắn chặt
/// vào một người gửi. Nó chặn 30 giây giữa hai lần gửi (<c>LettersSendTime</c>) nên đền bù
/// hàng loạt sẽ bị chặn ngay, nó bắn <c>redDialog</c>/<c>okDialog</c> về phía người gửi mà
/// thư hệ thống thì không có ai để báo, và nó gán <c>letter.userId</c> bằng id người gửi.</para>
///
/// <para><b>Hai đường giao, phải đi cả hai.</b> Người đang online nhận thẳng vào
/// <c>playerData.letters</c> rồi bắn cờ có-thư-mới để huy hiệu số trên HUD cập nhật ngay.
/// Người offline nhận qua bảng <c>letter</c> — bảng này chỉ là HÀNG ĐỢI GIAO, lúc đăng nhập
/// <c>Player.cs</c> nạp hết vào <c>playerData.letters</c> rồi xoá sạch.</para>
///
/// <para><b>Không tự sinh <c>LetterId</c>.</b> <c>addLetter</c> gọi
/// <c>Utilities.BinaryObjectAdd</c>, nó tự gán một id chưa trùng trong danh sách của người
/// đó. Tự đặt id là chắc chắn có ngày trùng.</para>
///
/// <para><b>Không gắn với gói mạng nào.</b> Đây là API nội bộ server, người chơi không có
/// đường nào gọi tới. Đường gửi thư của người chơi luôn ép <see cref="Letter.FRIEND"/> —
/// tuyệt đối đừng cho client truyền loại thư lên.</para>
/// </summary>
public static class SystemLetterService
{
    /// <summary>
    /// Id người gửi của thư hệ thống. Cột <c>userId</c> của bảng <c>letter</c> là NOT NULL
    /// nên phải điền gì đó; 0 không trùng <c>user_id</c> thật nào (tài khoản đánh số từ 1).
    /// </summary>
    public const int SystemSenderId = 0;

    private const string InsertSql =
        "INSERT INTO `letter`(`userId`, `targetId`, `time`, `Type`, `Title`, `ShortContent`, `Content`) " +
        "VALUES (@userId,@targetId,@time,@Type,@Title,@ShortContent,@Content)";

    /// <summary>
    /// Thư chào mừng cho nhân vật vừa tạo. Tách ra khỏi chỗ gọi để nội dung nằm cùng chỗ
    /// với các thư hệ thống khác, khỏi rải chữ vào giữa luồng tạo nhân vật.
    /// </summary>
    public static bool SendWelcome(int userId, string playerName)
    {
        return SendTo(userId, Letter.ADMIN, "Ban quản trị",
            $"Chào mừng {playerName} đến với thế giới Gopet!",
            $"Chào mừng {playerName} đến với thế giới Gopet!\n\n" +
            "Hãy ghé NPC trong thành để nhận pet miễn phí, nhận nhiệm vụ hằng ngày và " +
            "khám phá các bản đồ. Chúc bạn chơi vui!");
    }

    /// <summary>Gửi cho một người chơi theo <c>user_id</c>. Trả về <c>false</c> nếu hỏng.</summary>
    public static bool SendTo(int userId, sbyte type, string title, string shortContent,
        string content)
    {
        try
        {
            var online = PlayerManager.get(userId);
            if (online != null)
            {
                Deliver(online, type, title, shortContent, content);
                return true;
            }

            using var conn = MYSQLManager.create();
            conn.Execute(InsertSql, Build(userId, type, title, shortContent, content));
            return true;
        }
        catch (Exception e)
        {
            GopetManager.ServerMonitor.LogError($"Gửi thư hệ thống cho #{userId} thất bại: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gửi cho một người chơi theo TÊN. Trả về <c>false</c> khi không có ai tên đó — người
    /// đó có thể đang offline, nên phải tra bảng <c>player</c> chứ không chỉ hỏi
    /// <c>PlayerManager</c>.
    /// </summary>
    public static bool SendTo(string playerName, sbyte type, string title,
        string shortContent, string content)
    {
        try
        {
            var online = PlayerManager.get(playerName);
            if (online != null)
            {
                Deliver(online, type, title, shortContent, content);
                return true;
            }

            using var conn = MYSQLManager.create();
            var userId = conn.QueryFirstOrDefault<int?>(
                "SELECT `user_id` FROM `player` WHERE `name` = @name LIMIT 1;",
                new { name = playerName });
            if (userId == null) return false;

            conn.Execute(InsertSql, Build(userId.Value, type, title, shortContent, content));
            return true;
        }
        catch (Exception e)
        {
            GopetManager.ServerMonitor.LogError($"Gửi thư hệ thống cho '{playerName}' thất bại: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gửi cho TOÀN BỘ người chơi. Trả về số người đã nhận (online + offline).
    ///
    /// <para>Người offline ghi bằng MỘT câu lệnh nhận mảng tham số — Dapper tự gộp thành
    /// một lượt, thay vì mở lại vòng lặp ghi từng dòng.</para>
    /// </summary>
    public static int SendToAll(sbyte type, string title, string shortContent, string content)
    {
        try
        {
            // Chốt danh sách online TRƯỚC: có người vào/ra giữa chừng thì cũng không ai bị
            // gửi hai lần, vì tập id online đã cố định để loại khỏi phần ghi đĩa bên dưới.
            var onlinePlayers = PlayerManager.players.ToArray();
            var delivered = new HashSet<int>();

            foreach (var player in onlinePlayers)
            {
                if (player?.playerData == null) continue;
                Deliver(player, type, title, shortContent, content);
                delivered.Add(player.playerData.user_id);
            }

            using var conn = MYSQLManager.create();
            var offline = conn.Query<int>("SELECT `user_id` FROM `player`;")
                .Where(id => !delivered.Contains(id))
                .Select(id => Build(id, type, title, shortContent, content))
                .ToArray();

            if (offline.Length > 0) conn.Execute(InsertSql, offline);
            return delivered.Count + offline.Length;
        }
        catch (Exception e)
        {
            GopetManager.ServerMonitor.LogError($"Gửi thư hệ thống toàn server thất bại: {e.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Giao cho người đang online: thư vào danh sách trong bộ nhớ, rồi bắn cờ có-thư-mới
    /// để chấm đỏ và huy hiệu số cập nhật ngay, không phải đợi lần mở hộp thư sau.
    /// </summary>
    private static void Deliver(Player player, sbyte type, string title, string shortContent,
        string content)
    {
        player.playerData.addLetter(Build(player.playerData.user_id, type, title, shortContent, content));
        player.controller?.sendHasLetter();
    }

    private static Letter Build(int targetId, sbyte type, string title, string shortContent,
        string content)
    {
        return new Letter(type, title ?? string.Empty, shortContent ?? string.Empty,
            content ?? string.Empty)
        {
            userId = SystemSenderId,
            targetId = targetId,
            time = DateTime.Now,
        };
    }
}
