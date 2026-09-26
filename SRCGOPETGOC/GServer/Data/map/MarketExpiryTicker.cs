using Gopet.Util;

namespace Gopet.Data.Map
{
    /// <summary>
    /// 1 tick duy nhất chạy expire cho toàn bộ 6 loại ki ốt (<see cref="MarketPlace.kiosks"/>),
    /// thay cho vòng lặp <c>kiosk.update()</c> cũ nằm trong <see cref="MarketPlace.update"/> —
    /// map 22 tạo 10 <see cref="MarketPlace"/> (xem <see cref="MarketMap.createZoneDefault"/>)
    /// nên trước đây expire chạy trùng 10 lần mỗi tick, và chỉ chạy khi có ai online cập nhật
    /// map 22. Đăng ký trong <c>RuntimeServer.instance.runtimes</c> nên chạy mỗi ~5s
    /// (<see cref="RuntimeServer.TIME_WAIT"/>) bất kể map 22 có người hay không.
    /// </summary>
    public class MarketExpiryTicker : IRuntime
    {
        public void Update()
        {
            foreach (Kiosk kiosk in MarketPlace.kiosks)
            {
                try
                {
                    kiosk.update();
                }
                catch (Exception e)
                {
                    e.printStackTrace();
                }
            }
        }
    }
}
