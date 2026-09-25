using Gopet.Util;
using Newtonsoft.Json;

namespace Gopet.Data.GopetItem
{
    public class SellItem
    {
        public Item ItemSell;
        public int price;
        public long sumVal = 0;
        public bool hasSell = false;
        public bool hasRemoved = false;
        public Pet pet;
        public long expireTime = 0l;
        public int itemId;
        public int user_id = 0;
        public bool IsRetail { get; set; } = false;
        public string AssignedName { get; set; } = null;

        /// <summary>
        /// Tên người bán tại thời điểm treo. Dữ liệu JSON treo trước khi có trường này sẽ là
        /// null, dùng <see cref="GetSellerDisplayName"/> để fallback "???".
        /// </summary>
        public string SellerName { get; set; } = null;

        /// <summary>
        /// Khoá đồng bộ riêng cho từng listing. Mọi thao tác buy/cancel/expire phải
        /// <c>lock (sellItem.Sync)</c> rồi kiểm tra-rồi-đặt <see cref="hasSell"/>/<see cref="hasRemoved"/>
        /// để loại trừ lẫn nhau, tránh nhân đôi đồ/tiền.
        /// </summary>
        [JsonIgnore]
        public readonly object Sync = new();

        public int TotalCount { get; set; } = 1;

        protected SellItem() { }

        public SellItem(int hoursExpire)
        {
            expireTime = Utilities.TimeHours(hoursExpire);
        }

        public SellItem(Item ItemSell, int price, int hoursExpire) : this(hoursExpire)
        {
            this.ItemSell = ItemSell;
            this.price = price;
            this.TotalCount = ItemSell.count;
        }

        public SellItem(int price, Pet pet, int hoursExpire) : this(hoursExpire)
        {

            this.price = price;
            this.pet = pet;
        }

        public void setHasSell(bool b)
        {
            hasSell = b;
        }

        public string getName(Player player)
        {
            if (pet != null)
            {
                return pet.getNameWithoutStar(player) + Utilities.Format(" (Id:%s)", itemId);
            }
            return ItemSell.getTemp().getName(player) + Utilities.Format(" (Id:%s)", itemId);
        }

        public string getFrameImgPath()
        {
            if (pet != null)
            {
                return pet.getPetTemplate().frameImg;
            }
            return ItemSell.getTemp().getIconPath();
        }

        public string getDescription(Player player)
        {
            if (pet != null)
            {
                return pet.getPetTemplate().getDesc();
            }
            return ItemSell.getTemp().getDescription(player);
        }

        /// <summary>Tên người bán để hiển thị UI, fallback "???" cho listing JSON cũ chưa có SellerName.</summary>
        public string GetSellerDisplayName()
        {
            return string.IsNullOrEmpty(SellerName) ? "???" : SellerName;
        }

        /// <summary>
        /// Số ngọc còn phải trả để mua trọn listing này, đã trừ phần bán lẻ dở (sumVal) qua
        /// buyRetail — dùng chung cho giá HIỂN THỊ (<c>MarketPacketWriter.WriteRow</c>) và giá
        /// THỰC TRỪ (<see cref="Gopet.Data.Map.Kiosk.TryBuyWhole"/>). Tách riêng khỏi
        /// <see cref="MathPrice"/> (đơn_giá × số_lượng_còn_lại — dùng cho luồng NPC map 22 cũ,
        /// giữ nguyên hành vi) vì 2 công thức lệch nhau khi giá không chia hết cho TotalCount,
        /// từng gây hiện 1 giá / trừ giá khác cho listing bán lẻ dở.
        /// </summary>
        public long RemainingPrice => Math.Max(0L, price - sumVal);

        public int MathPrice
        {
            get
            {
                if (sumVal > 0 && this.ItemSell != null)
                {
                    int price = Math.Max(1, this.price) / Math.Max(1, this.TotalCount);
                    return Math.Max(1, price * this.ItemSell.count);
                }
                return this.price;
            }
        }
    }
}
