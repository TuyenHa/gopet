using System;
using System.Text;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Màn thông tin pet dùng chung cho pet bản thân và pet người chơi khác.</summary>
    public sealed class PetProfileView : MonoBehaviour
    {
        /// <summary>Chiều cao một dòng kỹ năng và số dòng tối đa panel chứa vừa.</summary>
        private const float SkillRowHeight = 26f;
        private const int MaxSkillRows = 5;

        public event Action CloseRequested;
        public event Action GymRequested;
        public event Action TattooRequested;

        /// <summary>Xin mở menu chọn kỹ năng. Tham số là <c>PetProfilePackets.LearnNewSlot</c>
        /// khi học vào ô trống, hoặc id kỹ năng đang có khi muốn THAY chính nó.</summary>
        public event Action<int> LearnSkillRequested;

        public static PetProfileView Create(Transform parent, PetProfile profile,
            RemoteAssetCache assets, bool editable)
        {
            var root = new GameObject("Pet Profile", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var view = root.AddComponent<PetProfileView>();
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());
            view.Build(profile, assets, editable);
            return view;
        }

        private void Build(PetProfile value, RemoteAssetCache assets, bool editable)
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(560f, 430f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());

            var title = UiBuilder.MakeText(panel.transform, UiBuilder.DefaultFont(), "Title", 20, false);
            UiBuilder.PlaceRow(title.rectTransform, 12f, 34f, 16f);
            title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.text = value.Name ?? "Pet";

            var body = UiBuilder.MakeText(panel.transform, UiBuilder.DefaultFont(), "Details", 15, false);
            UiBuilder.PlaceRow(body.rectTransform, 54f, 150f, 24f);
            body.alignment = TextAnchor.UpperLeft;
            body.text = Describe(value);

            BuildSkillRows(panel.transform, value, editable);

            if (editable)
            {
                var gym = MakeButton(panel.transform, "Cộng tiềm năng", new Vector2(-190f, 18f));
                gym.onClick.AddListener(() => GymRequested?.Invoke());
                var tattoo = MakeButton(panel.transform, "Hình xăm", new Vector2(-65f, 18f));
                tattoo.onClick.AddListener(() => TattooRequested?.Invoke());
                var learn = MakeButton(panel.transform, "Học kỹ năng", new Vector2(60f, 18f));
                learn.onClick.AddListener(() =>
                    LearnSkillRequested?.Invoke(PetProfilePackets.LearnNewSlot));
            }
            var close = MakeButton(panel.transform, "Đóng", new Vector2(95f, 18f));
            close.onClick.AddListener(() => CloseRequested?.Invoke());
            if (editable) close.GetComponent<RectTransform>().anchoredPosition = new Vector2(180f, 18f);
        }

        /// <summary>Kỹ năng tách thành TỪNG DÒNG thay vì gộp vào khối text: mỗi dòng cần một
        /// nút "Thay" riêng, mà nút thì không gắn vào giữa một đoạn văn bản được.</summary>
        private void BuildSkillRows(Transform panel, PetProfile value, bool editable)
        {
            var header = UiBuilder.MakeText(panel, UiBuilder.DefaultFont(), "SkillsHeader", 15, false);
            UiBuilder.PlaceRow(header.rectTransform, 208f, 22f, 24f);
            header.alignment = TextAnchor.MiddleLeft;
            UiBuilder.SetFontStyle(header, FontStyle.Bold);
            header.text = value.Skills.Length > 0 ? "Kỹ năng:" : "Kỹ năng: (chưa có)";

            for (var i = 0; i < value.Skills.Length && i < MaxSkillRows; i++)
            {
                var skill = value.Skills[i];
                var row = UiBuilder.MakeText(panel, UiBuilder.DefaultFont(), $"Skill{i}", 14, false);
                UiBuilder.PlaceRow(row.rectTransform, 232f + i * SkillRowHeight, SkillRowHeight, 24f);
                row.alignment = TextAnchor.MiddleLeft;
                row.text = $"• {skill.Name} (MP {skill.MpCost}) — {skill.Description}";
                if (!editable) continue;

                // Chừa chỗ cho nút Thay, nếu không chữ dài sẽ chạy xuống dưới nút.
                row.rectTransform.offsetMax = new Vector2(-110f, row.rectTransform.offsetMax.y);
                var swap = MakeButton(panel, "Thay", Vector2.zero);
                var rect = swap.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(76f, SkillRowHeight - 2f);
                rect.anchoredPosition = new Vector2(-24f, -(232f + i * SkillRowHeight));
                // Gửi ID THẬT của kỹ năng, không phải chỉ số dòng: server tra
                // skillId_learn theo id để biết thay vào ô nào.
                var skillId = skill.Id;
                swap.onClick.AddListener(() => LearnSkillRequested?.Invoke(skillId));
            }
        }

        private static string Describe(PetProfile value)
        {
            var text = new StringBuilder();
            text.Append("Cấp ").Append(value.Level)
                .Append("   Hệ ").Append(value.Element)
                .Append("   Lớp ").Append(value.PetClass).AppendLine();
            text.Append("EXP: ").Append(value.Experience).Append(" / ")
                .Append(value.ExperienceToNextLevel).AppendLine();
            text.Append("STR ").Append(value.Str).Append("   AGI ").Append(value.Agi)
                .Append("   INT ").Append(value.Int).AppendLine();
            text.Append("ATK ").Append(value.Atk).Append("   DEF ").Append(value.Def).AppendLine();
            text.Append("HP ").Append(value.Hp).Append('/').Append(value.MaxHp)
                .Append("   MP ").Append(value.Mp).Append('/').Append(value.MaxMp).AppendLine();
            text.Append("Điểm tiềm năng: ").Append(value.PotentialPoints).AppendLine();
            if (value.Tattoos.Length > 0)
            {
                text.AppendLine().AppendLine("Hình xăm:");
                foreach (var tattoo in value.Tattoos) text.Append("• ").AppendLine(tattoo.Name);
            }
            return text.ToString();
        }

        private static Button MakeButton(Transform parent, string label, Vector2 position)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(170f, 42f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }
    }
}
