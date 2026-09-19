using System;
using System.Collections.Generic;
using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    public sealed class BattleSkillPanel : MonoBehaviour
    {
        private const int MaxVisible = 5;
        private const float RowHeight = 44f;
        private const float Padding = 6f;
        private static readonly Color MpTextColor = new Color(0.3f, 0.85f, 0.4f, 1f);

        private readonly List<SkillRow> _rows = new List<SkillRow>();
        private bool _interactive;
        private SkillCooldownTracker _cooldowns;
        private int _currentMp;
        private static Sprite _rounded;

        public event Action<int> SkillUsed;

        public static BattleSkillPanel Create(Transform parent, BattleSkill[] skills,
            bool interactive, Font font, SkillCooldownTracker cooldowns = null)
        {
            var count = Math.Min(skills.Length, MaxVisible);
            var height = Padding * 2 + 18f + count * RowHeight;
            const float hudBottomY = 0.72f; // khớp anchorMin.y của BattleHudPanel
            var topY = hudBottomY - 40f / 540f;
            var go = new GameObject(interactive ? "Skill trái" : "Skill phải",
                typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(interactive ? 0f : 0.82f, topY - height / 540f);
            rect.anchorMax = new Vector2(interactive ? 0.18f : 1f, topY);
            rect.offsetMin = new Vector2(4f, 0f);
            rect.offsetMax = new Vector2(interactive ? -4f : -15f, 0f);
            var bg = go.GetComponent<Image>();
            bg.sprite = RoundedSprite(); bg.type = Image.Type.Sliced;
            bg.color = new Color(0.03f, 0.12f, 0.2f, 0.95f);

            var panel = go.AddComponent<BattleSkillPanel>();
            panel._interactive = interactive;
            panel._cooldowns = cooldowns;

            var header = UiBuilder.MakeText(go.transform, font, "Tiêu đề", 12, false);
            UiBuilder.PlaceRow(header.rectTransform, 2f, 16f, 4f);
            header.text = "Kỹ năng";
            header.alignment = TextAnchor.MiddleCenter;
            header.fontStyle = FontStyle.Bold;
            header.color = UiBuilder.TextMuted;

            for (var i = 0; i < count; i++)
                panel.AddRow(go.transform, skills[i], i, font);
            return panel;
        }

        private void AddRow(Transform parent, BattleSkill skill, int index, Font font)
        {
            var top = Padding + 18f + index * RowHeight;
            var row = new SkillRow { SkillId = skill.Id, MpCost = skill.MpCost, Name = skill.Name };
            var rowGo = new GameObject(skill.Name, typeof(RectTransform), typeof(Image),
                typeof(Button));
            rowGo.transform.SetParent(parent, false);
            var rect = (RectTransform)rowGo.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(4f, -(top + RowHeight - 2f));
            rect.offsetMax = new Vector2(-4f, -top);
            rowGo.GetComponent<Image>().color = _interactive
                ? UiBuilder.ButtonFace : new Color(0.05f, 0.15f, 0.25f, 0.8f);

            FillRowContent(rowGo.transform, skill, font, out row.NameLabel, out row.MpLabel);

            if (_interactive)
            {
                row.Button = rowGo.GetComponent<Button>();
                var id = skill.Id;
                row.Button.onClick.AddListener(() => SkillUsed?.Invoke(id));
            }
            else
            {
                UnityEngine.Object.Destroy(rowGo.GetComponent<Button>());
            }
            _rows.Add(row);
        }

        private static void FillRowContent(Transform rowTf, BattleSkill skill, Font font,
            out Text nameLabel, out Text mpLabel)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(rowTf, false);
            var iRect = (RectTransform)iconGo.transform;
            iRect.anchorMin = new Vector2(0f, 0.5f);
            iRect.anchorMax = new Vector2(0f, 0.5f);
            iRect.pivot = new Vector2(0f, 0.5f);
            iRect.anchoredPosition = new Vector2(4f, 0f);
            iRect.sizeDelta = new Vector2(28f, 28f);
            var icon = iconGo.GetComponent<Image>();
            icon.sprite = BattleSkin.Load(BattleSkillIconKey.For(skill.Id),
                "Battle/skills/unknown");
            icon.preserveAspect = true;

            nameLabel = UiBuilder.MakeText(rowTf, font, "SkillName", 12, false);
            var nRect = nameLabel.rectTransform;
            nRect.anchorMin = new Vector2(0f, 0.5f);
            nRect.anchorMax = new Vector2(1f, 1f);
            nRect.pivot = new Vector2(0f, 1f);
            nRect.offsetMin = new Vector2(36f, 0f);
            nRect.offsetMax = new Vector2(-4f, -2f);
            nameLabel.text = skill.Name;
            nameLabel.alignment = TextAnchor.MiddleLeft;
            nameLabel.color = Color.white;

            mpLabel = UiBuilder.MakeText(rowTf, font, "MpCost", 10, false);
            var mRect = mpLabel.rectTransform;
            mRect.anchorMin = new Vector2(0f, 0f);
            mRect.anchorMax = new Vector2(1f, 0.5f);
            mRect.pivot = new Vector2(0f, 0f);
            mRect.offsetMin = new Vector2(36f, 2f);
            mRect.offsetMax = new Vector2(-4f, 0f);
            mpLabel.text = skill.MpCost > 0 ? $"MP {skill.MpCost}" : "";
            mpLabel.alignment = TextAnchor.MiddleLeft;
            mpLabel.color = MpTextColor;
        }

        private static Sprite RoundedSprite()
        {
            if (_rounded != null) return _rounded;
            const int r = 8, s = r * 2 + 2;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var px = new Color32[s * s];
            for (int y = 0; y < s; y++) for (int x = 0; x < s; x++)
            {
                float cx = Mathf.Max(0, Mathf.Max(r - x, x - (s - 1 - r)));
                float cy = Mathf.Max(0, Mathf.Max(r - y, y - (s - 1 - r)));
                px[y * s + x] = new Color32(255, 255, 255,
                    (byte)(Mathf.Clamp01(r + 0.5f - Mathf.Sqrt(cx * cx + cy * cy)) * 255));
            }
            tex.SetPixels32(px); tex.Apply();
            _rounded = Sprite.Create(tex, new Rect(0, 0, s, s), Vector2.one * 0.5f,
                100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            return _rounded;
        }

        public void UpdateMpCosts(BattleSkillCost[] costs)
        {
            if (costs == null) return;
            foreach (var cost in costs)
            {
                var row = _rows.Find(r => r.SkillId == cost.SkillId);
                if (row != null) row.MpCost = cost.MpCost;
            }
            RefreshLabels();
        }

        public void RefreshState(int currentMp)
        {
            _currentMp = currentMp;
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            foreach (var row in _rows)
            {
                if (row.NameLabel != null) row.NameLabel.text = row.Name;
                if (row.MpLabel != null)
                {
                    var ready = _cooldowns == null || _cooldowns.IsReady(row.SkillId);
                    var cd = !ready && _cooldowns != null
                        ? $"  CD{_cooldowns.TurnsLeft(row.SkillId)}" : "";
                    row.MpLabel.text = row.MpCost > 0 ? $"MP {row.MpCost}{cd}" : cd;
                }
                if (row.Button != null)
                {
                    var ready = _cooldowns == null || _cooldowns.IsReady(row.SkillId);
                    row.Button.interactable = ready && row.MpCost <= _currentMp;
                }
            }
        }

        private sealed class SkillRow
        {
            public int SkillId, MpCost;
            public string Name;
            public Text NameLabel, MpLabel;
            public Button Button;
        }
    }
}
