using System.Collections.Generic;
using Gopet.Data.GopetItem;
using Gopet.Data.Map;
using Gopet.IO;

namespace Gopet.Data.Market
{
    /// <summary>
    /// SELLABLE (đồ/pet trong túi có thể treo bán) và SELL (đăng bán). Tách khỏi
    /// MarketService.cs để mỗi file dưới 200 dòng — xem đó về List/Buy/Mine/Cancel/Assign.
    /// </summary>
    public static partial class MarketService
    {
        // source echo nguyên từ SELLABLE (xem MarketSellableItem ở client) — chỉ 4 giá trị hợp lệ
        // (pet/equip/normal/gem). Gói tự chế với source khác sẽ trôi tới
        // player.playerData.getInventoryOrCreate(source) và tạo 1 inventory rác trong dict.
        private static readonly sbyte[] ValidSellSources =
        {
            MarketItemCategory.SourcePet,
            MarketItemCategory.SourceEquip,
            MarketItemCategory.SourceNormal,
            MarketItemCategory.SourceGem,
        };

        public static void HandleSellable(Player player)
        {
            if (!TryConsumeRateLimit(player, ObjKeySellableRateLimitMs)) return;
            SendSellableState(player);
        }

        /// <summary>
        /// Đăng bán qua <see cref="Kiosk.TryList"/> (tự suy ra ki ốt đích từ source+template).
        /// Thành công thì đổi thông báo sang câu riêng cho popup thay vì câu "Cập nhật thành
        /// công" dùng chung với luồng NPC map 22.
        /// </summary>
        public static void HandleSell(Player player, sbyte source, int itemOrPetId, int count, int price)
        {
            if (Array.IndexOf(ValidSellSources, source) < 0)
            {
                SendResult(player, ActionSell, false, player.Language.KioskInvalidCategory);
                return;
            }
            KioskResult result = Kiosk.TryList(player, new ListRequest(source, itemOrPetId, count, price));
            string message = result.Ok ? player.Language.MarketSellSuccess : result.Message;
            SendResult(player, ActionSell, result.Ok, message);
            SendMineState(player);
        }

        private static void SendSellableState(Player player)
        {
            List<(sbyte Source, Item Item)> items = new();
            items.AddRange(TagSource(MarketItemCategory.SourceEquip, player.playerData.getInventoryOrCreate(MarketItemCategory.SourceEquip)));
            items.AddRange(TagSource(MarketItemCategory.SourceGem, player.playerData.getInventoryOrCreate(MarketItemCategory.SourceGem)));
            items.AddRange(TagSource(MarketItemCategory.SourceNormal, player.playerData.getInventoryOrCreate(MarketItemCategory.SourceNormal)));

            Message m = new Message(GopetCMD.COMMAND_GUIDER);
            m.putsbyte(GopetCMD.TYPE_MARKET_SELLABLE_STATE);
            m.putShort((short)(items.Count + player.playerData.pets.Count));

            foreach (var (source, item) in items)
            {
                string name = source == MarketItemCategory.SourceNormal ? item.getName(player) : item.getEquipName(player);
                MarketPacketWriter.WriteSellableRow(m, source, item.itemId, name, item.getTemp().getIconPath(), item.count,
                    MarketItemCategory.BlockReason(player, item), item.getDescription(player));
            }
            foreach (Pet pet in player.playerData.pets)
            {
                MarketPacketWriter.WriteSellableRow(m, MarketItemCategory.SourcePet, pet.petId, pet.getNameWithStar(player), pet.getPetTemplate().icon, 1,
                    MarketItemCategory.BlockReason(player, pet), pet.getDesc(player));
            }

            m.cleanup();
            player.session.sendMessage(m);
        }

        private static IEnumerable<(sbyte Source, Item Item)> TagSource(sbyte source, IEnumerable<Item> items)
        {
            foreach (Item item in items)
            {
                yield return (source, item);
            }
        }
    }
}
