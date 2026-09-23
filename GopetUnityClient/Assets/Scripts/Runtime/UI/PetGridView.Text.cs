using System.Text.RegularExpressions;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    public sealed partial class PetGridView
    {
        private static void Place(RectTransform rect, float left, float height, float top, float width)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(left, top);
        }

        private static string CapitalizeFirst(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            value = value.Trim();
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string Shorten(string value)
        {
            const int maxLength = 20;
            return value.Length > maxLength ? value.Substring(0, maxLength - 3) + "..." : value;
        }

        private static string CleanDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Thông tin pet";
            var cleaned = Regex.Replace(description,
                @"(?:[+-]?\d+(?:\.\d+)?\s*\(\s*(?:str|int|agi|hp|mp)\s*\)|\(\s*(?:str|int|agi|hp|mp)\s*\)\s*[+-]?\d+(?:\.\d+)?)",
                string.Empty, RegexOptions.IgnoreCase).Trim(' ', ',', ';', '-', '|');
            return string.IsNullOrWhiteSpace(cleaned) ? "Pet đồng hành" : cleaned;
        }

        private static string ShopElementLabel(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Hệ: —";
            var match = Regex.Match(description, @"Hệ\s*:\s*([^.。,;\r\n]+)", RegexOptions.IgnoreCase);
            return match.Success ? "Hệ: " + match.Groups[1].Value.Trim() : "Hệ: —";
        }

        private static string TopRankLabel(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Hạng: —";
            var match = Regex.Match(description,
                @"Hạng\s*(\d+)\s*:\s*Cấp\s*(\d+)\s*hiện\s*có\s*([\d.,]+)\s*kinh\s*nghiệm",
                RegexOptions.IgnoreCase);
            return match.Success
                ? $"Hạng {match.Groups[1].Value}: Cấp {match.Groups[2].Value}, Kinh nghiệm {match.Groups[3].Value}"
                : "Hạng: —";
        }

        private static string FindStat(string value, string shortName, string displayName)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var pattern = $@"(?:([+-]?\d+(?:\.\d+)?)\s*\(\s*{Regex.Escape(shortName)}\s*\)|\(\s*{Regex.Escape(shortName)}\s*\)\s*([+-]?\d+(?:\.\d+)?)|{Regex.Escape(displayName)}\s*[:=]?\s*([+-]?\d+(?:\.\d+)?))";
            var match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) return null;
            for (var i = 1; i < match.Groups.Count; i++)
                if (match.Groups[i].Success) return match.Groups[i].Value;
            return null;
        }

        private static string MaxHp(string description, string strength)
        {
            var explicitHp = FindStat(description, "hp", "máu tối đa");
            if (!string.IsNullOrEmpty(explicitHp)) return explicitHp;
            return int.TryParse(strength, out var baseStrength)
                ? (baseStrength * 4 + 23).ToString()
                : null;
        }

        private static string MaxMp(string description, string agility)
        {
            var explicitMp = FindStat(description, "mp", "MP");
            if (!string.IsNullOrEmpty(explicitMp)) return explicitMp;
            return int.TryParse(agility, out var baseAgility)
                ? (baseAgility * 5 + 22).ToString()
                : null;
        }
    }
}
