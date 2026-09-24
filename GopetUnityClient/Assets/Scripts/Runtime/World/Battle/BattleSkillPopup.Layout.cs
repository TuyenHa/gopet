using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Phần dựng giao diện của <see cref="BattleSkillPopup"/>.</summary>
    public sealed partial class BattleSkillPopup
    {
        private const float Width = 178f;
        private const float TitleHeight = 24f;
        private const float RowHeight = 38f;
        private const float RowGap = 3f;
        private const float Pad = 5f;
        private const float IconBox = 26f;

        /// <summary>Cao tối đa vùng danh sách — quá thì cuộn, không cắt bớt dòng.</summary>
        private const float MaxListHeight = 4.5f * (RowHeight + RowGap);

        public static BattleSkillPopup Create(Transform parent, BattleSkill[] skills, Font font,
            SkillCooldownTracker cooldowns, RectTransform anchorTo)
        {
            skills = skills ?? System.Array.Empty<BattleSkill>();
            var listHeight = Mathf.Min(MaxListHeight,
                skills.Length * RowHeight + Mathf.Max(0, skills.Length - 1) * RowGap);
            var height = TitleHeight + listHeight + Pad * 2f;

            var go = new GameObject("Popup kỹ năng", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            // Neo vào mép phải-dưới của nút tròn, giống mũi tên trong mockup.
            rect.anchorMin = rect.anchorMax = anchorTo.anchorMin;
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchorTo.anchoredPosition + new Vector2(38f, -14f);
            rect.sizeDelta = new Vector2(Width, height);

            BattlePopupChrome.ApplyPanel(go);

            var popup = go.AddComponent<BattleSkillPopup>();
            popup._cooldowns = cooldowns;

            AddTitle(go.transform, font, popup);
            var content = AddScrollList(go.transform, listHeight);
            foreach (var skill in skills) popup._rows.Add(popup.AddRow(content, skill, font));

            go.SetActive(false); // mở bằng nút tròn
            return popup;
        }

        private static void AddTitle(Transform parent, Font font, BattleSkillPopup popup) =>
            BattlePopupChrome.AddTitle(parent, font, "KỸ NĂNG", 12, TitleHeight, Pad,
                () => popup.SetOpen(false));

        private static Transform AddScrollList(Transform parent, float listHeight) =>
            BattlePopupChrome.AddScrollList(parent, TitleHeight, listHeight, Pad, RowGap);
    }
}
