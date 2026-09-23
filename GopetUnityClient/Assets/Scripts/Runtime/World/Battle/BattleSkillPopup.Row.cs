using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Dựng một dòng kỹ năng trong popup: icon tròn | tên + "MP: n".</summary>
    public sealed partial class BattleSkillPopup
    {
        private BattleSkillEntry AddRow(Transform parent, BattleSkill skill, Font font)
        {
            var go = new GameObject(skill.Name, typeof(RectTransform), typeof(Image),
                typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(0f, RowHeight);

            // LayoutElement bắt buộc: VerticalLayoutGroup với childControlHeight=false không
            // suy ra chiều cao từ sizeDelta, ContentSizeFitter sẽ tính ra 0 và mất cuộn.
            var layoutElement = go.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = RowHeight;
            layoutElement.minHeight = RowHeight;

            var plate = go.GetComponent<Image>();
            plate.sprite = PanelSprites.Rounded(5);
            plate.type = Image.Type.Sliced;
            plate.color = new Color(0.13f, 0.19f, 0.30f, 0.9f);

            AddIcon(go.transform, skill.Id);

            var name = UiBuilder.MakeText(go.transform, font, "Tên", 12, false);
            var nRect = name.rectTransform;
            nRect.anchorMin = new Vector2(0f, 0.44f);
            nRect.anchorMax = new Vector2(1f, 1f);
            nRect.offsetMin = new Vector2(Pad + IconBox + 6f, 0f);
            nRect.offsetMax = new Vector2(-Pad, -3f);
            name.alignment = TextAnchor.LowerLeft;
            UiBuilder.SetFontStyle(name, FontStyle.Bold);
            name.text = skill.Name;
            // MakeText mặc định Overflow — tên dài sẽ tràn ra khỏi popup nếu không chặn.
            name.horizontalOverflow = HorizontalWrapMode.Wrap;
            name.verticalOverflow = VerticalWrapMode.Truncate;

            var mp = UiBuilder.MakeText(go.transform, font, "MP", 10, false);
            var mRect = mp.rectTransform;
            mRect.anchorMin = new Vector2(0f, 0f);
            mRect.anchorMax = new Vector2(1f, 0.44f);
            mRect.offsetMin = new Vector2(Pad + IconBox + 6f, 3f);
            mRect.offsetMax = new Vector2(-Pad, 0f);
            mp.alignment = TextAnchor.UpperLeft;

            var button = go.GetComponent<Button>();
            button.targetGraphic = plate;
            var id = skill.Id;
            button.onClick.AddListener(() => OnSkillClicked(id));

            return new BattleSkillEntry
            {
                SkillId = skill.Id, MpCost = skill.MpCost,
                NameLabel = name, MpLabel = mp, Button = button
            };
        }

        private static void AddIcon(Transform parent, int skillId)
        {
            var ringGo = new GameObject("Vòng icon", typeof(RectTransform), typeof(Image));
            ringGo.transform.SetParent(parent, false);
            var ring = ringGo.GetComponent<Image>();
            ring.sprite = BattleSkin.Load("Battle/skill-icon-ring");
            ring.preserveAspect = true;
            ring.raycastTarget = false;
            var rRect = (RectTransform)ringGo.transform;
            rRect.anchorMin = rRect.anchorMax = new Vector2(0f, 0.5f);
            rRect.pivot = new Vector2(0f, 0.5f);
            rRect.anchoredPosition = new Vector2(Pad, 0f);
            rRect.sizeDelta = new Vector2(IconBox, IconBox);

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(ringGo.transform, false);
            var icon = iconGo.GetComponent<Image>();
            // Icon sinh sẵn theo skillID; thiếu thì rơi về "unknown" chứ không để ô trắng.
            icon.sprite = BattleSkin.Load($"Battle/skills/{skillId}", "Battle/skills/unknown");
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            var iRect = (RectTransform)iconGo.transform;
            iRect.anchorMin = iRect.anchorMax = new Vector2(0.5f, 0.5f);
            iRect.sizeDelta = new Vector2(IconBox * 0.64f, IconBox * 0.64f);
        }
    }
}
