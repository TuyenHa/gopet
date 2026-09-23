using Gopet.Net.Guild;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class GuildView
    {
        private Transform _topContainer;
        private Transform _skillsContainer;

        private void BuildTopPage(Transform page)
        {
            var title = UiBuilder.MakeText(page, _font, "TopTitle", 14, false);
            title.text = "Top cống hiến";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.alignment = TextAnchor.UpperCenter;
            title.color = GuildText;
            var tr = title.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = Vector2.one;
            tr.pivot = new Vector2(0.5f, 1f);
            tr.offsetMin = new Vector2(0f, -20f);
            tr.offsetMax = Vector2.zero;

            var listGo = new GameObject("TopList", typeof(RectTransform));
            listGo.transform.SetParent(page, false);
            var lr = (RectTransform)listGo.transform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = new Vector2(0f, -24f);
            _topContainer = listGo.transform;
        }

        public void ShowTopFund(GuildTopResponse response)
        {
            foreach (Transform child in _topContainer) Destroy(child.gameObject);
            if (response.Members == null) return;
            for (var i = 0; i < response.Members.Length; i++)
            {
                var m = response.Members[i];
                var go = new GameObject($"Top:{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_topContainer, false);
                var r = (RectTransform)go.transform;
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(0f, 1f);
                float top = i * 34f;
                r.offsetMin = new Vector2(0f, -(top + 32f));
                r.offsetMax = new Vector2(0f, -top);
                RoundedUiSprite.Apply(go.GetComponent<Image>());
                go.GetComponent<Image>().color = GuildRow;

                var rank = UiBuilder.MakeText(go.transform, _font, "Rank", 12, false);
                rank.text = $"#{i + 1}";
                rank.color = i < 3 ? TabActive : GuildMutedText;
                UiBuilder.SetFontStyle(rank, FontStyle.Bold);
                var rr = rank.rectTransform;
                rr.anchorMin = Vector2.zero;
                rr.anchorMax = new Vector2(0.1f, 1f);
                rr.offsetMin = new Vector2(6f, 0f);
                rr.offsetMax = Vector2.zero;

                var name = UiBuilder.MakeText(go.transform, _font, "Name", 12, false);
                name.text = m.Name;
                name.color = GuildText;
                var nr = name.rectTransform;
                nr.anchorMin = new Vector2(0.1f, 0f);
                nr.anchorMax = new Vector2(0.6f, 1f);
                nr.offsetMin = new Vector2(4f, 0f);
                nr.offsetMax = Vector2.zero;

                var fund = UiBuilder.MakeText(go.transform, _font, "Fund", 11, false);
                fund.text = m.FundInfo;
                fund.color = GuildMutedText;
                fund.alignment = TextAnchor.MiddleRight;
                var fr = fund.rectTransform;
                fr.anchorMin = new Vector2(0.6f, 0f);
                fr.anchorMax = Vector2.one;
                fr.offsetMin = Vector2.zero;
                fr.offsetMax = new Vector2(-8f, 0f);
            }
        }

        private void BuildSkillsPage(Transform page)
        {
            var listGo = new GameObject("SkillsList", typeof(RectTransform));
            listGo.transform.SetParent(page, false);
            UiBuilder.Stretch((RectTransform)listGo.transform);
            _skillsContainer = listGo.transform;
        }

        public void ShowSkills(GuildSkillResponse response)
        {
            foreach (Transform child in _skillsContainer) Destroy(child.gameObject);
            if (response.Slots == null) return;

            bool canEdit = response.Mode == 0;
            var potLabel = UiBuilder.MakeText(_skillsContainer, _font, "Potential", 12, false);
            potLabel.text = $"Điểm tiềm năng bang: {response.PotentialSkill}";
            potLabel.color = GuildMutedText;
            var pr = potLabel.rectTransform;
            pr.anchorMin = new Vector2(0f, 1f);
            pr.anchorMax = Vector2.one;
            pr.pivot = new Vector2(0f, 1f);
            pr.offsetMin = new Vector2(4f, -20f);
            pr.offsetMax = new Vector2(-4f, 0f);

            for (var i = 0; i < response.Slots.Length; i++)
            {
                var slot = response.Slots[i];
                MakeSkillSlotRow(slot, i, canEdit);
            }
        }

        private void MakeSkillSlotRow(GuildSkillSlot slot, int index, bool canEdit)
        {
            var go = new GameObject($"Skill:{index}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_skillsContainer, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0f, 1f);
            float top = 24f + index * 70f;
            r.offsetMin = new Vector2(0f, -(top + 64f));
            r.offsetMax = new Vector2(0f, -top);
            RoundedUiSprite.Apply(go.GetComponent<Image>());

            bool locked = slot.State == -1;
            go.GetComponent<Image>().color = locked
                ? new Color(0.15f, 0.16f, 0.2f, 1f)
                : GuildRow;

            string stateText = locked ? "Khoá" : slot.State == 0 ? "Thuê skill" : "Đang dùng";
            var header = UiBuilder.MakeText(go.transform, _font, "Header", 13, false);
            header.text = $"Slot {index + 1} — {stateText}";
            UiBuilder.SetFontStyle(header, FontStyle.Bold);
            header.color = locked ? GuildMutedText : GuildText;
            var hr = header.rectTransform;
            hr.anchorMin = new Vector2(0f, 0.5f);
            hr.anchorMax = new Vector2(0.65f, 1f);
            hr.offsetMin = new Vector2(8f, 0f);
            hr.offsetMax = Vector2.zero;

            var desc = UiBuilder.MakeText(go.transform, _font, "Desc", 11, false);
            desc.text = slot.Desc1 ?? string.Empty;
            desc.color = GuildMutedText;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            var dr = desc.rectTransform;
            dr.anchorMin = Vector2.zero;
            dr.anchorMax = new Vector2(0.65f, 0.5f);
            dr.offsetMin = new Vector2(8f, 4f);
            dr.offsetMax = Vector2.zero;

            if (!canEdit) return;
            if (locked)
            {
                // Ô khoá: server mở ô KẾ TIẾP theo clan.slotSkill nên nút không cần tham số,
                // và nó trả về Y/N dialog chuẩn — client không phải dựng hộp thoại riêng.
                MakeRowButton(go.transform, "Mở ô", () => UnlockSkillSlotRequested?.Invoke());
                return;
            }

            var btnGo = new GameObject("Action", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(go.transform, false);
            var br = (RectTransform)btnGo.transform;
            br.anchorMin = new Vector2(0.68f, 0.2f);
            br.anchorMax = new Vector2(0.96f, 0.8f);
            br.offsetMin = br.offsetMax = Vector2.zero;
            RoundedUiSprite.Apply(btnGo.GetComponent<Image>());
            btnGo.GetComponent<Image>().color = TabActive;
            var bLabel = UiBuilder.MakeText(btnGo.transform, _font, "BtnLabel", 11, true);
            bLabel.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(bLabel, FontStyle.Bold);
            bLabel.color = new Color(0.1f, 0.08f, 0.02f, 1f);

            var slotIndex = slot.Index;
            if (slot.State == 0)
            {
                bLabel.text = "Thuê";
                btnGo.GetComponent<Button>().onClick.AddListener(() => SkillRentRequested?.Invoke(slotIndex));
            }
            else
            {
                bLabel.text = "Đổi";
                btnGo.GetComponent<Button>().onClick.AddListener(() => SkillRentRequested?.Invoke(slotIndex));
            }
        }
    }
}
