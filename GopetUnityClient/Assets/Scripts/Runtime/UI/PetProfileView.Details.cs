using System.Text;
using Gopet.Net.Pet;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Khối chỉ số và danh sách kỹ năng của <see cref="PetProfileView"/>.</summary>
    public sealed partial class PetProfileView
    {
        private const float StatsHeight = 104f;
        private const float SkillsTop = 34f + StatsHeight + 4f;
        private const float SkillRowHeight = 24f;
        private const int MaxSkillRows = 5;

        /// <summary>Kỹ năng tách thành TỪNG DÒNG thay vì gộp vào khối text: mỗi dòng cần một
        /// nút "Thay" riêng, mà nút thì không gắn vào giữa một đoạn văn bản được.</summary>
        private void BuildSkillRows(Transform panel, PetProfile value, bool editable)
        {
            var header = MakeText(panel, "SkillsHeader", 13, PopupPalette.TextDark);
            UiBuilder.PlaceRow(header.rectTransform, SkillsTop, 20f, Pad);
            UiBuilder.SetFontStyle(header, FontStyle.Bold);
            header.text = value.Skills.Length > 0 ? "Kỹ năng" : "Kỹ năng: chưa có";

            for (var i = 0; i < value.Skills.Length && i < MaxSkillRows; i++)
            {
                var skill = value.Skills[i];
                var top = SkillsTop + 22f + i * SkillRowHeight;
                var row = MakeText(panel, $"Skill{i}", 12, PopupPalette.TextMuted);
                UiBuilder.PlaceRow(row.rectTransform, top, SkillRowHeight, Pad);
                row.text = $"• {skill.Name} (MP {skill.MpCost}) — {skill.Description}";
                if (!editable) continue;

                // Chừa chỗ cho nút Thay, nếu không chữ dài sẽ chạy xuống dưới nút.
                row.rectTransform.offsetMax = new Vector2(-(Pad + 64f), row.rectTransform.offsetMax.y);
                // Gửi ID THẬT của kỹ năng, không phải chỉ số dòng: server tra
                // skillId_learn theo id để biết thay vào ô nào.
                var skillId = skill.Id;
                var swap = MakeButton(panel, "Thay", 11, () => LearnSkillRequested?.Invoke(skillId));
                var rect = (RectTransform)swap.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(56f, SkillRowHeight - 4f);
                rect.anchoredPosition = new Vector2(-Pad, -(top + 2f));
            }
        }

        private static string Describe(PetProfile value)
        {
            var text = new StringBuilder();
            text.Append("Hệ ").Append(value.Element)
                .Append("   Lớp ").Append(value.PetClass).AppendLine();
            text.Append("EXP: ").Append(value.Experience).Append(" / ")
                .Append(value.ExperienceToNextLevel).AppendLine();
            text.Append("STR ").Append(value.Str).Append("   AGI ").Append(value.Agi)
                .Append("   INT ").Append(value.Int).AppendLine();
            text.Append("ATK ").Append(value.Atk).Append("   DEF ").Append(value.Def).AppendLine();
            text.Append("HP ").Append(value.Hp).Append('/').Append(value.MaxHp)
                .Append("   MP ").Append(value.Mp).Append('/').Append(value.MaxMp).AppendLine();
            text.Append("Điểm tiềm năng: ").Append(value.PotentialPoints);
            if (value.Tattoos.Length > 0)
            {
                text.AppendLine().Append("Hình xăm: ");
                for (var i = 0; i < value.Tattoos.Length; i++)
                    text.Append(i > 0 ? ", " : string.Empty).Append(value.Tattoos[i].Name);
            }
            return text.ToString();
        }
    }
}
