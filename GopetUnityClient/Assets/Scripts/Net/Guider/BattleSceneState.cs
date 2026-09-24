using System.Collections.Generic;

namespace Gopet.Net.Guider
{
    /// <summary>
    /// Danh mục khung cảnh màn đấu + quyền sở hữu + khung cảnh đang chọn (sub-command
    /// <c>TYPE_BATTLE_BG_STATE</c> trong <c>COMMAND_GUIDER</c>). Tên và giá do server gửi nên
    /// chỉnh giá không cần phát hành lại client.
    ///
    /// <para>Layout khớp <c>BattleBackgroundService.SendState</c>: selectedId(sbyte), n(sbyte),
    /// lặp n: id(sbyte) + name(utf) + priceGold(long) + owned(bool).</para>
    /// </summary>
    public sealed class BattleSceneState
    {
        public sealed class Entry
        {
            public int Id;
            public string Name;
            public long PriceGold;
            public bool Owned;
        }

        public int SelectedId;
        public IReadOnlyList<Entry> Entries = new Entry[0];

        public static BattleSceneState Parse(Message message)
        {
            var r = message.Reader;
            var selected = r.ReadSByte();
            var count = r.ReadSByte();
            if (count < 0) throw new ProtocolException($"TYPE_BATTLE_BG_STATE count âm: {count}.");
            var entries = new Entry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = new Entry
                {
                    Id = r.ReadSByte(),
                    Name = r.ReadUtf(),
                    PriceGold = r.ReadLong(),
                    Owned = r.ReadBool(),
                };
            }
            r.ExpectFullyConsumed("TYPE_BATTLE_BG_STATE");
            return new BattleSceneState { SelectedId = selected, Entries = entries };
        }
    }
}
