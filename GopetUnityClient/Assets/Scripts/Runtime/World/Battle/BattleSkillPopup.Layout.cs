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

            var bg = go.GetComponent<Image>();
            bg.sprite = PanelSprites.Rounded(8);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.09f, 0.14f, 0.24f, 0.97f);

            var borderGo = new GameObject("Viền", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(go.transform, false);
            UiBuilder.Stretch((RectTransform)borderGo.transform);
            var border = borderGo.GetComponent<Image>();
            border.sprite = PanelSprites.Rounded(8, 2);
            border.type = Image.Type.Sliced;
            border.color = new Color(0.62f, 0.72f, 0.88f, 0.9f);
            border.raycastTarget = false;

            var popup = go.AddComponent<BattleSkillPopup>();
            popup._cooldowns = cooldowns;

            AddTitle(go.transform, font, popup);
            var content = AddScrollList(go.transform, listHeight);
            foreach (var skill in skills) popup._rows.Add(popup.AddRow(content, skill, font));

            go.SetActive(false); // mở bằng nút tròn
            return popup;
        }

        private static void AddTitle(Transform parent, Font font, BattleSkillPopup popup)
        {
            var label = UiBuilder.MakeText(parent, font, "Tiêu đề", 12, false);
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(Pad + 4f, -TitleHeight);
            rect.offsetMax = new Vector2(-(TitleHeight + Pad), -Pad);
            label.text = "KỸ NĂNG";
            label.alignment = TextAnchor.MiddleLeft;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.color = new Color(1f, 0.85f, 0.35f);

            var btnGo = new GameObject("Đóng", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            var bRect = (RectTransform)btnGo.transform;
            bRect.anchorMin = bRect.anchorMax = new Vector2(1f, 1f);
            bRect.pivot = new Vector2(1f, 1f);
            bRect.anchoredPosition = new Vector2(-Pad, -Pad);
            bRect.sizeDelta = new Vector2(18f, 18f);
            var img = btnGo.GetComponent<Image>();
            img.sprite = PanelSprites.Rounded(4, 1);
            img.type = Image.Type.Sliced;
            img.color = new Color(0.75f, 0.80f, 0.90f, 0.85f);
            var x = UiBuilder.MakeText(btnGo.transform, font, "X", 12, true);
            x.text = "✕";
            x.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(x, FontStyle.Bold);
            x.color = Color.white;
            btnGo.GetComponent<Button>().onClick.AddListener(() => popup.SetOpen(false));
        }

        private static Transform AddScrollList(Transform parent, float listHeight)
        {
            var viewGo = new GameObject("Khung cuộn", typeof(RectTransform), typeof(Image),
                typeof(Mask), typeof(ScrollRect));
            viewGo.transform.SetParent(parent, false);
            var vRect = (RectTransform)viewGo.transform;
            vRect.anchorMin = new Vector2(0f, 1f);
            vRect.anchorMax = new Vector2(1f, 1f);
            vRect.pivot = new Vector2(0.5f, 1f);
            vRect.offsetMin = new Vector2(Pad, -(TitleHeight + listHeight));
            vRect.offsetMax = new Vector2(-Pad, -TitleHeight);

            var maskImg = viewGo.GetComponent<Image>();
            maskImg.sprite = PanelSprites.Rounded(6);
            maskImg.type = Image.Type.Sliced;
            viewGo.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Nội dung", typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewGo.transform, false);
            var cRect = (RectTransform)contentGo.transform;
            cRect.anchorMin = new Vector2(0f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(0.5f, 1f);
            cRect.offsetMin = cRect.offsetMax = Vector2.zero;

            var layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = RowGap;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewGo.GetComponent<ScrollRect>();
            scroll.content = cRect;
            scroll.viewport = vRect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 16f;
            return contentGo.transform;
        }
    }
}
