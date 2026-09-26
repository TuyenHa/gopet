using Dapper;
using Gopet.Data.GopetItem;
using Gopet.Util;
using Newtonsoft.Json;

namespace Gopet.Data.Map
{
    public partial class Kiosk
    {
        /// <summary>
        /// Quét listing hết hạn, luôn ghi <c>kiosk_recovery</c> rồi mới đánh dấu hasRemoved/gỡ khỏi
        /// kiosk (kể cả người bán đang online) — xem lý do ở <c>update()</c>. Gọi định kỳ bởi
        /// <see cref="MarketExpiryTicker"/>. Lock theo <see cref="SellItem.Sync"/> và kiểm
        /// tra-rồi-đặt <c>hasRemoved</c> để loại trừ với buy/cancel đang chạy song song.
        /// </summary>
        public void update()
        {
            foreach (SellItem sellItem in kioskItems)
            {
                if (sellItem.expireTime >= Utilities.CurrentTimeMillis)
                {
                    continue;
                }

                bool recovered = false;
                lock (sellItem.Sync)
                {
                    if (sellItem.hasSell || sellItem.hasRemoved)
                    {
                        kioskItems.remove(sellItem);
                        continue;
                    }

                    try
                    {
                        // Luôn ghi kiosk_recovery TRƯỚC khi đánh dấu hasRemoved/gỡ khỏi kiosk — kể
                        // cả người bán đang online. update() chạy trên thread của
                        // MarketExpiryTicker (Runtime thread), không phải thread xử lý gói tin của
                        // player: gọi thẳng addItemToInventory/addPet ở đây có thể đua với
                        // subCountItem/addItem từ thread xử lý packet của chính player đó (mất
                        // stack merge, double-add). Đi qua kiosk_recovery + nhận lại trên thread của
                        // player (KioskRecovery: gói tin kế tiếp nếu online, hoặc lúc login) là cách an toàn nhất, thống nhất 1 luồng online/offline
                        // thay vì 2 luồng dễ lệch nhau. Nếu INSERT lỗi thì không set hasRemoved,
                        // giữ nguyên listing để tick sau thử lại — không mất đồ/tiền.
                        using (var conn = MYSQLManager.create())
                        {
                            conn.Execute("INSERT INTO `kiosk_recovery`(`kioskType`, `user_id`, `item`) VALUES (@kioskType,@user_id,@jsonData)",
                                new { kioskType, user_id = sellItem.user_id, jsonData = JsonConvert.SerializeObject(sellItem) });
                        }

                        sellItem.hasRemoved = true;
                        kioskItems.remove(sellItem);
                        recovered = true;
                        // Người bán đang online: đánh dấu để gói tin kế tiếp của họ (thread của
                        // player) nhận ngay vào túi — xem KioskRecovery.DeliverIfPending.
                        if (PlayerManager.get(sellItem.user_id) != null)
                        {
                            KioskRecovery.MarkPending(sellItem.user_id);
                        }
                        HistoryManager.addHistory(new History(sellItem.user_id).setObj(sellItem).setLog("Ki ốt hết hạn, đã lưu vật phẩm/tiền vào kiosk_recovery"));
                    }
                    catch (Exception e)
                    {
                        e.printStackTrace();
                    }
                }

                if (recovered)
                {
                    GopetManager.SaveMarketNow();
                }
            }
        }
    }
}
