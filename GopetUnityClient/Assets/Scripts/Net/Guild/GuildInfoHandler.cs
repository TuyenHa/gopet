using System;

namespace Gopet.Net.Guild
{
    /// <summary>
    /// Bắt gói <c>PET_SERVICE (81) / CLAN (91) / CLAN_INFO (14)</c> để lấy <b>clanId</b>
    /// của self. Wire (server) — xem <c>GameController.clanInfo</c> (line 2509):
    /// <code>int clanId, sbyte count, count × UTF description</code>.
    ///
    /// <para>Chỉ đọc clanId — description lines không cần cho chat SEND. View đầy đủ
    /// cho panel bang hội sẽ dựng riêng khi mở Phase 8 guild.</para>
    /// </summary>
    public sealed class GuildInfoHandler
    {
        // Sub trong CLAN family.
        private const sbyte SubClanInfo = 14;

        /// <summary>Bang hiện tại; 0 nếu chưa có bang / chưa nhận CLAN_INFO.</summary>
        public int ClanId { get; private set; }

        public event Action<int> ClanIdChanged;

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, 91, OnClanEnvelope);
        }

        private void OnClanEnvelope(Message message)
        {
            var r = message.Reader;
            if (r.Remaining < 1) return;
            var sub = r.ReadSByte();
            switch (sub)
            {
                case SubClanInfo:
                    var clanId = r.ReadInt();
                    // Bỏ qua phần description — không parse để giữ handler nhỏ.
                    if (clanId != ClanId)
                    {
                        ClanId = clanId;
                        ClanIdChanged?.Invoke(clanId);
                    }
                    break;
                // Sub khác (donate/topFund/…) chưa cần trong scope này.
            }
        }
    }
}
