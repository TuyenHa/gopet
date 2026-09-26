

using Gopet.Data.GopetClan;
using Gopet.Util;

public class AutoSave : IRuntime
{
    public static DateTime lastTimeSaveClan = DateTime.Now.AddMinutes(1);
    public static DateTime lastTimeSaveMarket = DateTime.Now.AddSeconds(10);
    public static DateTime lastTimeSavePlayer = DateTime.Now.AddMinutes(10);


    public void Update()
    {
        if (lastTimeSavePlayer < DateTime.Now)
        {
            using (var conn = MYSQLManager.create())
            {
                foreach (Player player in PlayerManager.players)
                {
                    if (player.session.isConnected())
                    {
                        if (player.timeSaveDelta < Utilities.CurrentTimeMillis)
                        {
                            if (player.playerData != null && player.playerData.saveIfActive(conn))
                            {
                                player.Popup("Dữ liệu của bạn đã được máy chủ lưu dự phòng thành công");
                                HistoryManager.addHistory(new History(player).setLog("Backup dữ liệu thành công").setObj(player.playerData));
                            }
                            player.timeSaveDelta = Utilities.CurrentTimeMillis + Player.TIME_SAVE_DATA;
                        }
                    }
                }
            }
            lastTimeSavePlayer = DateTime.Now.AddMinutes(10);
        }
        if (lastTimeSaveClan < DateTime.Now)
        {

            using (var conn = MYSQLManager.create())
            {
                foreach (Clan clan in ClanManager.clans)
                {
                    try
                    {
                        clan.save(conn);
                    }
                    catch (Exception e)
                    {
                        e.printStackTrace();
                    }
                }
                lastTimeSaveClan = DateTime.Now.AddMinutes(3);
            }
        }

        if (lastTimeSaveMarket < DateTime.Now)
        {
            // Debounce 10s: chỉ ghi DB nếu Kiosk có mutation (RequestMarketSave) kể từ lần lưu
            // trước, thay vì chờ cố định 30 phút như cũ (mất listing nếu crash giữa chừng).
            GopetManager.FlushMarketSaveIfDirty();
            lastTimeSaveMarket = DateTime.Now.AddSeconds(10);
        }
    }
}
