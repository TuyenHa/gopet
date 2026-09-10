using System;
using Gopet.Net.Auth;

namespace Gopet.UiLogic
{
    /// <summary>
    /// Danh sách máy chủ. <c>SERVER_LIST</c> là danh sách {tên, ip, cổng}
    /// (<c>Player.showListServer</c>), nên chọn một dòng nghĩa là NỐI LẠI tới địa chỉ
    /// đó — không phải gửi lên server một lựa chọn nào.
    /// </summary>
    public sealed partial class LoginFlow
    {
        public void OnServerList(ServerEntry[] servers)
        {
            if (servers == null || servers.Length == 0)
            {
                Enter(LoginStage.Disconnected, "Máy chủ không trả về danh sách nào.");
                return;
            }

            Servers = servers;

            // Đúng một máy chủ thì khỏi bắt người chơi chọn — tự vào thẳng. Server
            // trả về nhiều hơn một (tương lai) vẫn hiện màn chọn như cũ; đổi hành vi
            // ở ĐÂY, tại nguồn, để mọi tầng view tự động ăn theo mà
            // không phải sửa riêng từng UI.
            if (servers.Length == 1)
            {
                ChooseServer(0);
                return;
            }

            Enter(LoginStage.ChoosingServer, null);
        }

        public void ChooseServer(int index)
        {
            if (Servers == null || index < 0 || index >= Servers.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Máy chủ được chọn không có trong danh sách.");
            }
            var entry = Servers[index];
            _serverChosen = true;

            // Cùng địa chỉ thì giữ nguyên kết nối; nối lại chỉ tốn 2 giây cooldown.
            if (entry.Address == _host && entry.Port == _port)
            {
                Enter(LoginStage.EnteringCredentials, null);
                return;
            }

            _host = entry.Address;
            _port = entry.Port;

            Connect();
        }
    }
}
