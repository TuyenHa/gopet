using Gopet.Net.Guild;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class GuildView
    {
        private Transform _membersContainer;
        private Text _membersTitle;
        private bool _canManageMembers;

        private void BuildMembersPage(Transform page)
        {
            _membersTitle = UiBuilder.MakeText(page, _font, "MembersTitle", 14, false);
            _membersTitle.text = "Thành viên";
            _membersTitle.fontStyle = FontStyle.Bold;
            _membersTitle.alignment = TextAnchor.UpperCenter;
            _membersTitle.color = GuildText;
            var tr = _membersTitle.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(0.5f, 1f);
            tr.offsetMin = new Vector2(0f, -20f);
            tr.offsetMax = Vector2.zero;

            var listGo = new GameObject("MembersList", typeof(RectTransform));
            listGo.transform.SetParent(page, false);
            var lr = (RectTransform)listGo.transform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = new Vector2(1f, 1f);
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = new Vector2(0f, -24f);
            _membersContainer = listGo.transform;
        }

        public void ShowMembers(GuildMemberListResponse response)
        {
            _canManageMembers = response.CanManage;
            _membersTitle.text = response.Title ?? "Thành viên";

            foreach (Transform child in _membersContainer) Destroy(child.gameObject);

            if (response.Members == null) return;
            for (var i = 0; i < response.Members.Length; i++)
                MakeMemberRow(_membersContainer, response.Members[i], i);
        }

        private void MakeMemberRow(Transform container, GuildMember member, int index)
        {
            var go = new GameObject($"Member:{member.UserId}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(container, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0f, 1f);
            float top = index * 38f;
            r.offsetMin = new Vector2(0f, -(top + 36f));
            r.offsetMax = new Vector2(0f, -top);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = GuildRow;

            var nameLabel = UiBuilder.MakeText(go.transform, _font, "Name", 12, false);
            nameLabel.text = member.Name;
            nameLabel.color = GuildText;
            var nr = nameLabel.rectTransform;
            nr.anchorMin = Vector2.zero;
            nr.anchorMax = new Vector2(0.55f, 1f);
            nr.offsetMin = new Vector2(8f, 0f);
            nr.offsetMax = Vector2.zero;

            var fundLabel = UiBuilder.MakeText(go.transform, _font, "Fund", 11, false);
            fundLabel.text = member.FundInfo;
            fundLabel.color = GuildMutedText;
            fundLabel.alignment = TextAnchor.MiddleRight;
            var fr = fundLabel.rectTransform;
            fr.anchorMin = new Vector2(0.55f, 0f);
            fr.anchorMax = new Vector2(_canManageMembers ? 0.78f : 1f, 1f);
            fr.offsetMin = Vector2.zero;
            fr.offsetMax = new Vector2(-8f, 0f);

            if (!_canManageMembers) return;
            var kickGo = new GameObject("Kick", typeof(RectTransform), typeof(Image), typeof(Button));
            kickGo.transform.SetParent(go.transform, false);
            var kr = (RectTransform)kickGo.transform;
            kr.anchorMin = new Vector2(0.8f, 0.15f);
            kr.anchorMax = new Vector2(0.98f, 0.85f);
            kr.offsetMin = kr.offsetMax = Vector2.zero;
            RoundedUiSprite.Apply(kickGo.GetComponent<Image>());
            kickGo.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f, 1f);
            var kLabel = UiBuilder.MakeText(kickGo.transform, _font, "KickLabel", 10, true);
            kLabel.text = "Đuổi";
            kLabel.alignment = TextAnchor.MiddleCenter;
            kLabel.fontStyle = FontStyle.Bold;

            var userId = member.UserId;
            kickGo.GetComponent<Button>().onClick.AddListener(() => KickRequested?.Invoke(userId));
        }
    }
}
