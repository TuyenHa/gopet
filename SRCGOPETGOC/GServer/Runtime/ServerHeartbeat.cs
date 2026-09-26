using Dapper;
using Gopet.Util;

/// <summary>
/// Ghi "server này còn sống" vào <c>server_heartbeat</c> (game DB) mỗi 30s, để web admin
/// fail-closed khi server rollback/tắt không kịp dọn cờ (RT#2 — xem
/// phase-04-gserver-isonline-patch.md). Chạy qua <see cref="RuntimeServer"/> (tick 5s) như
/// <see cref="AutoSave"/>/<see cref="DBBackup"/> — không dùng Timer riêng để không thêm
/// luồng nền mới.
///
/// Lỗi ghi CHỈ log — không được làm chết vòng lặp <see cref="RuntimeServer"/>.
/// </summary>
public class ServerHeartbeat : IRuntime
{
    /// <summary>Đổi khi giao thức đọc/ghi bảng player_online + server_heartbeat đổi
    /// không tương thích ngược — web admin dùng để fail-closed với server cũ.</summary>
    public const int ProtocolVersion = 1;
    private const int HeartbeatId = 1;

    private DateTime _nextBeat = DateTime.MinValue;

    public void Update()
    {
        if (_nextBeat > DateTime.Now) return;
        _nextBeat = DateTime.Now.AddSeconds(30);
        try
        {
            using var conn = MYSQLManager.create();
            conn.Execute(
                @"INSERT INTO `server_heartbeat` (`id`, `protocol_version`, `beat_at`)
                  VALUES (@id, @protocolVersion, NOW())
                  ON DUPLICATE KEY UPDATE `protocol_version` = @protocolVersion, `beat_at` = NOW();",
                new { id = HeartbeatId, protocolVersion = ProtocolVersion });
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}
