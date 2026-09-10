
public class ServerSetting : Settings
{

    public static readonly ServerSetting instance = new ServerSetting();

    public int portGopetServer { get; protected set; }
    public int portHttpServer { get; protected set; }

    /// <summary>
    /// Địa chỉ HTTP API lắng nghe. Mặc định loopback: API này tắt được máy chủ
    /// và tạo được vật phẩm, nên không mở ra mạng khi chưa đặt <see cref="apiKey"/>.
    /// </summary>
    public string httpBindAddress { get; protected set; } = "127.0.0.1";

    /// <summary>
    /// Địa chỉ cổng game lắng nghe. Mặc định <c>0.0.0.0</c> vì production cần
    /// người chơi kết nối từ ngoài vào — đổi mặc định sẽ làm sập mọi bản đang chạy.
    ///
    /// Máy dev thì đặt <c>127.0.0.1</c>: server test không có lý do gì phải mở ra
    /// mạng LAN, nhất là khi giao thức dùng khoá TEA do chính client gửi lên.
    /// </summary>
    public string gameBindAddress { get; protected set; } = "0.0.0.0";

    /// <summary>Bật ghi hex dump mọi gói tin ra <c>log/packet-dump-server.log</c>.</summary>
    public bool enablePacketLog { get; protected set; } = false;
    public string webDomainName { get; protected set; }
    public bool initLog { get; protected set; }
    public string outputFileName { get; protected set; }
    public string errorFileName { get; protected set; }
    public int hourMaintenance { get; protected set; }
    public int minMaintenance { get; protected set; }
    public bool isOnlyAdminLogin { get; protected set; }
    public bool isServerTest { get; protected set; }
    public bool isShowMessageWhenLogin { get; protected set; } = false;
    public string messageWhenLogin { get; protected set; }
    public string apiKey { get; protected set; }

    public ServerSetting()
    {

        load(new SettingsFile("server.json"));

    }


    public void load(SettingsFile settingsFile)
    {
        portGopetServer = settingsFile.Data.portGopetServer;
        portHttpServer = settingsFile.Data.portHttpServer;
        // Hai trường dưới thêm sau, nên có thể thiếu ở server.json cũ.
        // Mặc định chọn phía an toàn: loopback, không ghi log gói tin.
        httpBindAddress = (string?)settingsFile.Data.httpBindAddress ?? "127.0.0.1";
        gameBindAddress = (string?)settingsFile.Data.gameBindAddress ?? "0.0.0.0";
        enablePacketLog = (bool?)settingsFile.Data.enablePacketLog ?? false;
        webDomainName = settingsFile.Data.webDomainName;
        initLog = settingsFile.Data.initLog;
        outputFileName = settingsFile.Data.outputFileName;
        errorFileName = settingsFile.Data.errorFileName;
        hourMaintenance = settingsFile.Data.hourMaintenance;
        minMaintenance = settingsFile.Data.minMaintenance;
        isOnlyAdminLogin = settingsFile.Data.isOnlyAdminLogin;
        isServerTest = settingsFile.Data.isServerTest;
        isShowMessageWhenLogin = settingsFile.Data.isShowMessageWhenLogin;
        messageWhenLogin = settingsFile.Data.messageWhenLogin;
        apiKey = settingsFile.Data.apiKey;
    }


    public string toString()
    {
        return "ServerSetting{" + "portGopetServer=" + portGopetServer + ", portHttpServer=" + portHttpServer + ", webDomainName=" + webDomainName + ", initLog=" + initLog + ", outputFileName=" + outputFileName + ", errorFileName=" + errorFileName + ", hourMaintenance=" + hourMaintenance + ", minMaintenance=" + minMaintenance + ", isOnlyAdminLogin=" + isOnlyAdminLogin + ", isServerTest=" + isServerTest + ", isShowMessageWhenLogin=" + isShowMessageWhenLogin + ", messageWhenLogin=" + messageWhenLogin + ", apiKey=" + apiKey + '}';
    }
}
