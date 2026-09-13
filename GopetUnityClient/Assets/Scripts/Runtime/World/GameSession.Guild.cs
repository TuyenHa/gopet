using Gopet.Net.Guild;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
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
            if (_guildInfoHandler.ClanId > 0)
                _client.Send(GuildPackets.RequestClanInfo());
            else
                _client.Send(GuildPackets.RequestGuildList());
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
