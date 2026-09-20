using Gopet.Net.Guild;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        public void OpenGuildPopup() => OpenGuildView();

        private void OpenGuildView()
        {
            if (_guildView != null) return;
            _guildView = GuildView.Create(_hudParent);
            _guildView.CloseRequested += CloseGuildView;
            _guildView.SearchRequested += q => _client.Send(GuildPackets.SearchGuild(q));
            _guildView.JoinRequested += id => _client.Send(GuildPackets.RequestJoin(id));
            _guildView.KickRequested += id => _client.Send(GuildPackets.KickMember(id));
            _guildView.ChatSent += (id, txt) =>
            {
                if (TryChatCooldown("guild-chat")) _client.Send(GuildPackets.SendChat(id, txt));
            };
            _guildView.TopFundRequested += () => _client.Send(GuildPackets.TopFund());
            _guildView.MemberListRequested += () => _client.Send(GuildPackets.RequestClanMemberInfo(0, false));
            _guildView.ChatHistoryRequested += () => _client.Send(GuildPackets.RequestChatHistory());
            _guildView.DonateRequested += () => _client.Send(GuildPackets.DonateClan());
            _guildView.SkillRentRequested += idx => _client.Send(GuildPackets.RentSkill(idx));
            _guildView.DonateOptionChosen += id => _client.Send(GuildPackets.PlayerDonateClan(id));
            _guildView.UnlockSkillSlotRequested += () => _client.Send(GuildPackets.UnlockSkillSlot());
            // LUÔN hỏi CLAN_INFO (sub 14), kể cả khi chưa có bang: server tự rẽ nhánh — có bang
            // thì trả thông tin bang, chưa có thì gọi showListClan() (GameController.cs:2617-2620),
            // tức đúng danh sách bang để xin vào.
            //
            // KHÔNG dùng RequestGuildList() (sub 1): switch clan() của server không có case 1 và
            // cũng không có default, nên gói rơi im lặng — tab bang hội trống vĩnh viễn với người
            // chưa vào bang.
            _client.Send(GuildPackets.RequestClanInfo());
        }

        private void CloseGuildView()
        {
            if (_guildView == null) return;
            Object.Destroy(_guildView.gameObject);
            _guildView = null;
        }

        private void OnGuildClanInfo(GuildClanInfo info)
        {
            _hud.SetGuildAvailable(info.ClanId > 0);
            _guildView?.ShowGuildInfo(info.ClanId, info.DescriptionLines);
            if (info.ClanId > 0 && _hud.IsGuildChannel)
                _client.Send(GuildPackets.RequestChatHistory());
        }
    }
}
