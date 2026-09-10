

using Gopet.Data.User;
using MySqlConnector;
using System.Configuration;

public class MYSQLManager
{
    /// <summary>
    /// Ghép connection string: lấy khung từ App.config rồi cho biến môi trường
    /// đè lên phần thông tin đăng nhập.
    ///
    /// <para>Lý do: App.config nằm trong repo, nên không được chứa mật khẩu thật.
    /// Tên database thì không phải bí mật — giữ ở App.config. Chỉ host/port/user/
    /// password đến từ môi trường.</para>
    ///
    /// <para>Thứ tự ưu tiên:</para>
    /// <list type="number">
    ///   <item><c>GOPET_&lt;TÊN&gt;_CONNSTR</c> — thay trọn chuỗi (vd <c>GOPET_GAMECONNECTSTRING_CONNSTR</c>)</item>
    ///   <item><c>GOPET_DB_HOST</c> / <c>GOPET_DB_PORT</c> / <c>GOPET_DB_USER</c> / <c>GOPET_DB_PASSWORD</c> — đè từng phần</item>
    ///   <item>Giá trị trong App.config</item>
    /// </list>
    /// </summary>
    private static string Resolve(string name)
    {
        var entry = ConfigurationManager.ConnectionStrings[name]
            ?? throw new ConfigurationErrorsException(
                $"Thiếu connection string '{name}' trong App.config. " +
                $"Kiểm tra Gopet.dll.config có nằm cạnh file thực thi không.");

        var full = Environment.GetEnvironmentVariable($"GOPET_{name.ToUpperInvariant()}_CONNSTR");
        if (!string.IsNullOrWhiteSpace(full))
        {
            return full;
        }

        var builder = new MySqlConnectionStringBuilder(entry.ConnectionString);

        var host = Environment.GetEnvironmentVariable("GOPET_DB_HOST");
        if (!string.IsNullOrWhiteSpace(host)) builder.Server = host;

        var port = Environment.GetEnvironmentVariable("GOPET_DB_PORT");
        if (uint.TryParse(port, out var portValue)) builder.Port = portValue;

        var user = Environment.GetEnvironmentVariable("GOPET_DB_USER");
        if (!string.IsNullOrWhiteSpace(user)) builder.UserID = user;

        // Kiểm null chứ không kiểm rỗng: đặt GOPET_DB_PASSWORD="" là cách hợp lệ
        // để nói "không mật khẩu".
        var password = Environment.GetEnvironmentVariable("GOPET_DB_PASSWORD");
        if (password != null) builder.Password = password;

        return builder.ConnectionString;
    }

    public static MySqlConnection create()
    {
        var conn = new MySqlConnection(Resolve("GameConnectString"));
        conn.Open();
        return conn;
    }

    public static MySqlConnection createLogConnection()
    {
        var conn = new MySqlConnection(Resolve("LogConnectString"));
        conn.Open();
        return conn;
    }

    public static MySqlConnection createOld()
    {
        var conn = new MySqlConnection(GameOldSQLInfoStr);
        conn.Open();
        return conn;
    }

    /// <summary>
    /// Kết nối tới database máy chủ CŨ, chỉ dùng cho tính năng gộp dữ liệu
    /// (<c>MenuController.sendMenu.cs:637</c>).
    ///
    /// <para>Dùng cùng cơ chế biến môi trường như <see cref="Resolve"/> để
    /// <c>config/database.json</c> không phải chứa địa chỉ máy chủ thật.
    /// Biến riêng: <c>GOPET_OLDDB_HOST</c> / <c>_PORT</c> / <c>_USER</c> /
    /// <c>_PASSWORD</c> / <c>_NAME</c>.</para>
    /// </summary>
    public static string GameOldSQLInfoStr
    {
        get
        {
            var setting = MysqlSetting.SINGLETON.INSTANCE;

            var host = Environment.GetEnvironmentVariable("GOPET_OLDDB_HOST") ?? setting.host;
            var user = Environment.GetEnvironmentVariable("GOPET_OLDDB_USER") ?? setting.username;
            var password = Environment.GetEnvironmentVariable("GOPET_OLDDB_PASSWORD") ?? setting.password;
            var database = Environment.GetEnvironmentVariable("GOPET_OLDDB_NAME") ?? "gopettae_gopet";

            var builder = new MySqlConnectionStringBuilder
            {
                Server = host,
                Database = database,
                UserID = user,
                Password = password,
                CharacterSet = "utf8",
            };

            var port = Environment.GetEnvironmentVariable("GOPET_OLDDB_PORT");
            if (uint.TryParse(port, out var portValue)) builder.Port = portValue;
            else if (setting.port > 0) builder.Port = (uint)setting.port;

            return builder.ConnectionString;
        }
    }

    public static MySqlConnection createWebMySqlConnection()
    {
        var conn = new MySqlConnection(Resolve("WebConnectString"));
        conn.Open();
        return conn;
    }

    /// <summary>
    /// Kiểm tra cả 3 kết nối lúc khởi động, báo lỗi rõ ràng nếu hỏng.
    ///
    /// Không có bước này thì lỗi cấu hình DB sẽ hiện ra dưới dạng
    /// <c>printStackTrace()</c> giữa lúc nạp template, rất khó lần ra nguyên nhân.
    /// </summary>
    public static void VerifyConnections()
    {
        (string Name, Func<MySqlConnection> Factory)[] targets =
        {
            ("GameConnectString", create),
            ("WebConnectString", createWebMySqlConnection),
            ("LogConnectString", createLogConnection),
        };

        foreach (var (name, factory) in targets)
        {
            try
            {
                using var conn = factory();
                Console.WriteLine($"[DB] OK  {name} -> {conn.Database}@{conn.DataSource}");
            }
            catch (Exception ex)
            {
                var builder = new MySqlConnectionStringBuilder(Resolve(name));
                throw new InvalidOperationException(
                    $"[DB] Không kết nối được '{name}' " +
                    $"(server={builder.Server}:{builder.Port}, database={builder.Database}, user={builder.UserID}). " +
                    $"Đặt GOPET_DB_HOST/GOPET_DB_PORT/GOPET_DB_USER/GOPET_DB_PASSWORD nếu cần. " +
                    $"Nguyên nhân: {ex.Message}", ex);
            }
        }
    }

    public static void Backup(MySqlConnection conn, string filePath)
    {
        using (var cmd = new MySqlCommand())
        {
            cmd.Connection = conn;
            using (var backup = new MySqlBackup(cmd))
            {
                FileInfo f = new FileInfo(filePath);
                f.Directory.Create();
                using (var stream = f.Open(FileMode.OpenOrCreate))
                {
                    backup.ExportToStream(stream);
                    stream.Flush();
                    stream.Close();
                }
            }
        }
    }
}
