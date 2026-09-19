using System;
using System.Text;
using Gopet.Net.Battle;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    public sealed class BattleHudPanel : MonoBehaviour
    {
        private static readonly Color HpColor = new Color(0.78f, 0.15f, 0.15f, 1f);
        private static readonly Color MpColor = new Color(0.3f, 0.55f, 0.95f, 1f);
        private static readonly Color BorderBlue = new Color(0.12f, 0.38f, 0.68f, 1f);
        private static readonly Color BorderRed = new Color(0.68f, 0.14f, 0.14f, 1f);
        private static readonly Color InnerFill = new Color(0.04f, 0.12f, 0.22f, 0.97f);
        private static Sprite _rounded;

        private BattleStatBar _hpBar, _mpBar;
        private Text _nameLabel, _buffLabel;
        private Text _atkLabel, _defLabel, _critLabel;
        private RawImage _avatar;
        private string _petName;
        private int _actorId, _frameCount;

        public int ActorId => _actorId;
        public Text BuffLabel => _buffLabel;

        public static BattleHudPanel Create(Transform parent, BattlePet pet, bool isLeft,
            Font font, RemoteAssetCache assets)
        {
            var borderColor = isLeft ? BorderBlue : BorderRed;
            var go = new GameObject(isLeft ? "HUD trái" : "HUD phải",
                typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(isLeft ? 0f : 0.7f, 0.72f);
            rect.anchorMax = new Vector2(isLeft ? 0.3f : 1f, 1f);
            rect.offsetMin = new Vector2(8f, 0f);
            rect.offsetMax = new Vector2(-8f, -36f);
            var outerImg = go.GetComponent<Image>();
            outerImg.sprite = RoundedSprite(); outerImg.type = Image.Type.Sliced;
            outerImg.color = borderColor;

            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(go.transform, false);
            var iRect = (RectTransform)inner.transform;
            iRect.anchorMin = Vector2.zero; iRect.anchorMax = Vector2.one;
            iRect.offsetMin = new Vector2(2f, 2f); iRect.offsetMax = new Vector2(-2f, -2f);
            var innerImg = inner.GetComponent<Image>();
            innerImg.sprite = RoundedSprite(); innerImg.type = Image.Type.Sliced;
            innerImg.color = InnerFill;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(inner.transform, false);
            var cRect = (RectTransform)content.transform;
            cRect.anchorMin = Vector2.zero; cRect.anchorMax = Vector2.one;
            cRect.offsetMin = new Vector2(68f, 0f);
            cRect.offsetMax = Vector2.zero;

            var panel = go.AddComponent<BattleHudPanel>();
            panel._actorId = pet.ActorId; panel._frameCount = pet.FrameCount;
            BuildAvatar(go.transform, panel, assets, pet);
            BuildContent(content.transform, panel, pet, font);
            return panel;
        }

        private static void BuildAvatar(Transform outer, BattleHudPanel panel,
            RemoteAssetCache assets, BattlePet pet)
        {
            const float size = 60f;
            var go = new GameObject("Avatar", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(outer, false);
            panel._avatar = go.GetComponent<RawImage>();
            panel._avatar.raycastTarget = false;
            var r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            r.anchoredPosition = new Vector2(8f, 0f);
            r.sizeDelta = new Vector2(size, size);
            assets.Get(pet.ImagePath, ImagePackets.TypeNpc, panel.SetAvatarTexture);
        }

        private static void BuildContent(Transform content, BattleHudPanel panel,
            BattlePet pet, Font font)
        {
            panel._nameLabel = UiBuilder.MakeText(content, font, "Tên", 13, false);
            var nr = panel._nameLabel.rectTransform;
            nr.anchorMin = new Vector2(0f, 1f); nr.anchorMax = new Vector2(1f, 1f); nr.pivot = new Vector2(0f, 1f);
            nr.offsetMin = new Vector2(4f, -16f); nr.offsetMax = new Vector2(-4f, -2f);
            panel._nameLabel.fontStyle = FontStyle.Bold; panel._nameLabel.alignment = TextAnchor.MiddleLeft;
            panel._petName = pet.Name; panel.SetNameText(pet.Level);

            panel._hpBar = BattleStatBar.Create(content, font, "HP", HpColor, 24f, 0f); panel._hpBar.Set(pet.Hp, pet.MaxHp);
            panel._mpBar = BattleStatBar.Create(content, font, "MP", MpColor, 48f, 0f); panel._mpBar.Set(pet.Mp, pet.MaxMp);

            var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(content, false);
            UiBuilder.PlaceRow((RectTransform)sep.transform, 72f, 1f, 2f);
            sep.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.4f, 0.6f);

            var statsRow = new GameObject("Stats", typeof(RectTransform));
            statsRow.transform.SetParent(content, false);
            UiBuilder.PlaceRow((RectTransform)statsRow.transform, 76f, 14f, 4f);
            panel._atkLabel = MakeStatGroup(statsRow.transform, "Battle/icon-atk", 0f, 0.33f, font);
            panel._defLabel = MakeStatGroup(statsRow.transform, "Battle/icon-def", 0.33f, 0.66f, font);
            panel._critLabel = MakeStatGroup(statsRow.transform, "Battle/icon-crit", 0.66f, 1f, font);
            panel._atkLabel.text = "--"; panel._defLabel.text = "--"; panel._critLabel.text = "--%";

            panel._buffLabel = UiBuilder.MakeText(content, font, "Buff", 9, false);
            UiBuilder.PlaceRow(panel._buffLabel.rectTransform, 98f, 10f, 4f);
            panel._buffLabel.alignment = TextAnchor.MiddleLeft;
            panel._buffLabel.color = new Color(0.9f, 0.85f, 0.35f, 1f);
        }

        private static Text MakeStatGroup(Transform parent, string iconRes,
            float xMin, float xMax, Font font)
        {
            var go = new GameObject(iconRes, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = new Vector2(xMin, 0f); r.anchorMax = new Vector2(xMax, 1f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            iconGo.GetComponent<Image>().sprite = BattleSkin.Load(iconRes);
            iconGo.GetComponent<Image>().preserveAspect = true;
            var ir = (RectTransform)iconGo.transform;
            ir.anchorMin = ir.anchorMax = ir.pivot = new Vector2(0f, 0.5f);
            ir.anchoredPosition = Vector2.zero; ir.sizeDelta = new Vector2(12f, 12f);
            var t = UiBuilder.MakeText(go.transform, font, "Value", 10, false);
            t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.offsetMin = new Vector2(14f, 0f); t.rectTransform.offsetMax = Vector2.zero;
            t.alignment = TextAnchor.MiddleLeft; t.color = UiBuilder.TextMuted;
            return t;
        }

        private static Sprite RoundedSprite()
        {
            if (_rounded != null) return _rounded;
            const int r = 10, s = r * 2 + 2;
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

        private void SetNameText(int level) =>
            _nameLabel.text = $"{_petName}  <color=#4CD964>Lv.{level}</color>";

        private void SetAvatarTexture(Texture2D texture)
        {
            if (this == null || _avatar == null || texture == null) return;
            _avatar.texture = texture;
            _avatar.uvRect = new Rect(0f, 0f, 1f / Mathf.Max(1, _frameCount), 1f);
        }

        public void UpdateVitals(int hp, int mp, int maxHp, int maxMp)
        {
            _hpBar.Set(hp, maxHp); _mpBar.Set(mp, maxMp);
        }

        public void UpdateStats(BattleActorStats stats)
        {
            if (stats == null) return;
            SetNameText(stats.Level);
            _atkLabel.text = $"{stats.Atk}";
            _defLabel.text = $"{stats.Def}";
            _critLabel.text = $"{stats.CritPermille / 10f:0.#}%";
        }

        public void UpdateBuffs(BattleActorBuffs actor)
        {
            if (_buffLabel == null || actor == null) return;
            var sb = new StringBuilder();
            var n = Math.Min(actor.Entries.Length, 6);
            for (var i = 0; i < n; i++)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(BuffTypeLabels.For(actor.Entries[i].TypeId));
                if (actor.Entries[i].TurnsLeft > 0) sb.Append('(').Append(actor.Entries[i].TurnsLeft).Append(')');
            }
            if (actor.Entries.Length > n) sb.Append(" +").Append(actor.Entries.Length - n);
            _buffLabel.text = sb.ToString();
        }
    }
}
