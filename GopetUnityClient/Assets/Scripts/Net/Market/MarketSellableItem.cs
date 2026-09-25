using System;
using System.Collections.Generic;

namespace Gopet.Net.Market
{
    /// <summary>
    /// Một món trong túi/rương của người chơi có thể treo bán (TYPE_MARKET_SELLABLE_STATE,
    /// sub 54). Dùng ở popup Đăng bán (phase 5) — phase 4 chỉ dựng sẵn model/parser để phase
    /// sau tái dùng, chưa có UI đọc gói này.
    ///
    /// <para>Field order khớp <c>MarketPacketWriter.WriteSellableRow</c>
    /// (<c>GServer/Data/Market/MarketPacketWriter.cs:34-44</c>). Giá trị <see cref="Source"/>
    /// khớp <c>GopetManager.EQUIP_PET_INVENTORY/NORMAL_INVENTORY/GEM_INVENTORY</c> và hằng số
    /// riêng <c>MarketItemCategory.SourcePet=-1</c> — client chỉ cần ECHO nguyên giá trị này
    /// lại trong <see cref="MarketPackets.Sell"/>, không cần tự suy luận loại ki-ốt.</para>
    /// </summary>
    public sealed class MarketSellableItem
    {
        public const sbyte SourceEquip = 0;
        public const sbyte SourceNormal = 1;
        public const sbyte SourceGem = 4;
        public const sbyte SourcePet = -1;

        public sbyte Source;
        public int Id;
        public string Name;
        public string IconPath;
        public int Count;
        public bool Tradable;
        public string BlockReason;
        public string Desc;

        public static MarketSellableItem Parse(JavaBinaryReader r)
        {
            return new MarketSellableItem
            {
                Source = r.ReadSByte(),
                Id = r.ReadInt(),
                Name = r.ReadUtf(),
                IconPath = r.ReadUtf(),
                Count = r.ReadInt(),
                Tradable = r.ReadBool(),
                BlockReason = r.ReadUtf(),
                Desc = r.ReadUtf(),
            };
        }
    }

    /// <summary>TYPE_MARKET_SELLABLE_STATE (sub 54): chỉ n + rows, không phân trang.</summary>
    public sealed class MarketSellableState
    {
        public IReadOnlyList<MarketSellableItem> Items = Array.Empty<MarketSellableItem>();

        public static MarketSellableState Parse(Message message)
        {
            var r = message.Reader;
            var count = r.ReadShort();
            if (count < 0) throw new ProtocolException($"TYPE_MARKET_SELLABLE_STATE count âm: {count}.");
            var items = new MarketSellableItem[count];
            for (var i = 0; i < count; i++) items[i] = MarketSellableItem.Parse(r);
            r.ExpectFullyConsumed("TYPE_MARKET_SELLABLE_STATE");

            return new MarketSellableState { Items = items };
        }
    }
}
