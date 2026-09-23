using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Ô ngày trong lưới điểm danh: nền/viền theo trạng thái, số ngày, dấu ✓ hoặc icon
    /// quà. Tách khỏi file chính để mỗi file dưới 200 dòng.
    /// </summary>
    public sealed partial class DailyCheckinView
    {
        private void BuildCell(DailyCheckinState.DayInfo day)
        {
            var cell = new GameObject($"Day{day.Day}", typeof(RectTransform), typeof(Image));
            cell.transform.SetParent(_grid, false);
            var bg = cell.GetComponent<Image>();
            RoundedUiSprite.Apply(bg);
            var colors = CellColors(day.State);
            bg.color = colors.bg;
            var outline = cell.AddComponent<Outline>();
            outline.effectColor = colors.border;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var num = UiBuilder.MakeText(cell.transform, _font, "Num", 9, false);
            num.text = day.Day.ToString();
            num.alignment = TextAnchor.UpperCenter;
            UiBuilder.SetFontStyle(num, FontStyle.Bold);
            num.color = NumColor(day.State);
            var nr = (RectTransform)num.transform;
            nr.anchorMin = new Vector2(0f, 1f); nr.anchorMax = new Vector2(1f, 1f);
            nr.pivot = new Vector2(0.5f, 1f);
            nr.offsetMin = new Vector2(0f, -12f); nr.offsetMax = new Vector2(0f, -1f);

            if (day.State == DailyCheckinState.Received) BuildCheckMark(cell.transform);
            else BuildItemIcon(cell.transform, day);

            var badge = UiBuilder.MakeText(cell.transform, _font, "Badge", 7, false);
            badge.text = StateBadge(day.State);
            badge.alignment = TextAnchor.LowerCenter;
            badge.color = BadgeColor(day.State);
            var br = (RectTransform)badge.transform;
            br.anchorMin = new Vector2(0f, 0f); br.anchorMax = new Vector2(1f, 0f);
            br.pivot = new Vector2(0.5f, 0f);
            br.offsetMin = new Vector2(1f, 1f); br.offsetMax = new Vector2(-1f, 10f);

            if (day.State == DailyCheckinState.Claimable)
            {
                var btn = cell.AddComponent<Button>();
                btn.targetGraphic = bg;
                btn.onClick.AddListener(() => _guider?.DoDailyCheckin());
            }
        }

        /// <summary>Dấu ✓ to xanh cho ngày đã nhận — thay cho icon quà.</summary>
        private void BuildCheckMark(Transform parent)
        {
            var t = UiBuilder.MakeText(parent, _font, "Check", 16, false);
            t.text = "✓";
            t.alignment = TextAnchor.MiddleCenter;
            t.color = CheckGreen;
            UiBuilder.SetFontStyle(t, FontStyle.Bold);
            var r = (RectTransform)t.transform;
            r.anchorMin = new Vector2(0f, 0f); r.anchorMax = new Vector2(1f, 1f);
            r.offsetMin = new Vector2(2f, 10f); r.offsetMax = new Vector2(-2f, -10f);
        }

        private void BuildItemIcon(Transform parent, DailyCheckinState.DayInfo day)
        {
            if (string.IsNullOrEmpty(day.IconPath) || _assets == null) return;
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            iconGo.transform.SetParent(parent, false);
            var iconRect = (RectTransform)iconGo.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
            var raw = iconGo.GetComponent<RawImage>();
            raw.color = new Color(1f, 1f, 1f, 0f);
            _assets.Get(day.IconPath, ImagePackets.TypeIcon, tex =>
            {
                if (raw == null) return;
                raw.texture = tex; raw.color = Color.white;
            });
        }

        // ==== Helpers ====

        /// <summary>"Còn X ngày Y giờ" tới cuối tháng (reset điểm danh).</summary>
        internal static string FormatMonthlyRemaining(DateTime now)
        {
            var end = new DateTime(now.Year, now.Month,
                DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);
            var diff = end - now;
            if (diff.TotalSeconds < 0) diff = TimeSpan.Zero;
            return $"⏰ Còn {diff.Days} ngày {diff.Hours} giờ";
        }

        private static (Color bg, Color border) CellColors(byte state) => state switch
        {
            DailyCheckinState.Received => (CellReceivedBg, CellReceivedBorder),
            DailyCheckinState.Claimable => (CellTodayBg, CellTodayBorder),
            DailyCheckinState.Missed => (CellMissedBg, CellMissedBorder),
            _ => (CellLockedBg, CellLockedBorder),
        };

        private static Color NumColor(byte state) => state switch
        {
            DailyCheckinState.Received => CheckGreen,
            DailyCheckinState.Claimable => new Color(0.85f, 0.55f, 0.10f, 1f),
            DailyCheckinState.Missed => SubText,
            _ => SubText,
        };

        private static Color BadgeColor(byte state) => state switch
        {
            DailyCheckinState.Received => CheckGreen,
            DailyCheckinState.Claimable => new Color(0.85f, 0.55f, 0.10f, 1f),
            _ => SubText,
        };

        private static string StateBadge(byte state) => state switch
        {
            DailyCheckinState.Received => "Đã nhận",
            DailyCheckinState.Claimable => "Điểm danh",
            DailyCheckinState.Missed => "Đã lỡ",
            _ => "Điểm danh",
        };
    }
}
