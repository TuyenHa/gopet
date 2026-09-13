using System;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// HUD góc trên-trái style jar cổ: portrait pet trong khung tròn + badge level nhỏ
    /// dưới chân portrait + 2 thanh pill HP (xanh lá) / MP (xanh dương).
    ///
    /// <para><b>HP/MP là stat PET</b>, không phải char. Portrait dùng frame đầu của pet
    /// sprite (server bơm qua <c>SEND_LIST_PET_ZONE</c>). Level lấy từ cùng gói. HP/MP
    /// realtime từ <c>MY_PET_INFO</c>.</para>
    ///
    /// <para><b>EXP bỏ khỏi HUD</b> — server không bơm pet exp realtime (chỉ level-up
    /// event); hiện thanh EXP luôn "--%" tạo cảm giác broken. Bỏ để layout gọn 2 bar.</para>
    /// </summary>
    public sealed class CharacterHud : MonoBehaviour
    {
        // Ngắn hơn bản cũ 20 px; portrait giữ nguyên, chỉ thu phần tên và thanh HP/MP.
        private const float PanelWidth = 240f;
        private const float PanelHeight = 88f;
        private const float PortraitSize = 56f;
        private const float PortraitLeft = 6f;
        private const float PortraitTop = 6f;
        private const float BadgeSize = 24f;
        private const float BarLeft = PortraitLeft + PortraitSize + 6f;
        private const float BarWidth = PanelWidth - BarLeft - 8f;
        private const float BarHeight = 22f;
        private const float NameTop = 2f;

        private Text _name;
        private Text _mapName;
        private Text _levelBadge;
        private Image _portrait;
        private RemoteAssetCache _assets;

        public event Action Clicked;

        public StatBar Hp { get; private set; }
        public StatBar Mp { get; private set; }
        /// <summary>Deprecated — pet EXP chưa có realtime; property giữ để tương thích, không dùng.</summary>
        public StatBar Experience { get; private set; }
        public string PlayerName => _name == null ? string.Empty : _name.text;
        public string MapName => _mapName == null ? string.Empty : _mapName.text;

        public static CharacterHud Create(Transform parent, string playerName)
        {
            var go = new GameObject("Character HUD", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(12f, -12f);
            rect.sizeDelta = new Vector2(PanelWidth, PanelHeight);

            // Panel nền tối bán trong suốt, viền cong — hợp với style jar.
            var panel = go.GetComponent<Image>();
            panel.raycastTarget = true;
            RoundedUiSprite.Apply(panel);
            panel.color = new Color(0.08f, 0.11f, 0.16f, 0.72f);

            var hud = go.AddComponent<CharacterHud>();
            go.GetComponent<Button>().transition = Selectable.Transition.None;
            go.GetComponent<Button>().onClick.AddListener(() => hud.Clicked?.Invoke());
            hud.Build(UiBuilder.BuiltinFont());
            hud.SetName(playerName);
            return hud;
        }

        public void SetName(string playerName)
        {
            var cleaned = Gopet.UiLogic.JarIconTokens.Strip(playerName ?? string.Empty);
            _name.text = string.IsNullOrWhiteSpace(cleaned) ? "NHÂN VẬT" : cleaned;
        }

        public void SetLevel(int level)
        {
            if (_levelBadge == null) return;
            _levelBadge.text = level.ToString();
        }

        public void SetMapName(string mapName)
        {
            _mapName.text = string.IsNullOrWhiteSpace(mapName) ? "Bản đồ chưa xác định" : mapName;
        }

        public void SetPortrait(Sprite sprite)
        {
            if (_portrait == null) return;
            _portrait.sprite = sprite;
            _portrait.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.15f);
        }

        /// <summary>Nạp portrait từ image server (frame đầu của pet sprite strip).</summary>
        public void SetPetPortrait(string frameImagePath)
        {
            if (_assets == null || string.IsNullOrEmpty(frameImagePath)) return;
            _assets.Get(frameImagePath, ImagePackets.TypeNpc, tex =>
            {
                if (tex == null || _portrait == null) return;
                // Sprite strip: chỉ lấy frame đầu (width / frameCount) — nhưng đây HUD
                // static, dùng nguyên texture cũng OK (đa số pet strip 4 frame gần đều).
                // Nếu portrait lệch, tighten crop ở đây với PetZoneEntry.FrameNum.
                var w = tex.width;
                var sprite = Sprite.Create(tex, new Rect(0f, 0f, w, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                SetPortrait(sprite);
            });
        }

        public void BindAssets(RemoteAssetCache assets)
        {
            _assets = assets;
        }

        /// <summary>Cập nhật HP/MP. EXP field bỏ qua (không có realtime data).</summary>
        public void SetStats(int hp, int maxHp, int mp, int maxMp,
            int experience = 0, int experienceToNextLevel = 0)
        {
            Hp.SetValue(hp, maxHp);
            Mp.SetValue(mp, maxMp);
            // EXP giữ null-check nếu ai đó gọi legacy
            if (Experience != null && experienceToNextLevel > 0)
                Experience.SetValue(experience, experienceToNextLevel);
        }

        private void Build(Font font)
        {
            // Portrait tròn — Mask + RoundedUiSprite trên viewport
            var frame = MakeImage(transform, "Portrait Frame", new Color(0.75f, 0.55f, 0.2f, 1f));
            SetRect(frame.rectTransform, PortraitLeft, PortraitTop, PortraitSize, PortraitSize);
            RoundedUiSprite.Apply(frame);

            var viewport = MakeImage(frame.transform, "Portrait Mask", Color.white);
            RoundedUiSprite.Apply(viewport);
            Inset(viewport.rectTransform, 3f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            _portrait = MakeImage(viewport.transform, "Portrait", new Color(1f, 1f, 1f, 0.15f));
            _portrait.preserveAspect = true;
            UiBuilder.Stretch(_portrait.rectTransform);

            BuildLevelBadge(font, frame.transform);

            _name = UiBuilder.MakeText(transform, font, "Player Name", 12, false);
            SetRect(_name.rectTransform, BarLeft, NameTop, BarWidth, 14f);
            _name.fontStyle = FontStyle.Bold;
            _name.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            var shadow = _name.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(1f, -1f);

            // 2 thanh pill: HP xanh lá (top), MP xanh dương (bottom). Font 12, no percent.
            Hp = StatBar.Create(transform, font, "HP Bar", "HP",
                new Color(0.24f, 0.85f, 0.28f, 1f), 18f, x: BarLeft, width: BarWidth);
            Mp = StatBar.Create(transform, font, "MP Bar", "MP",
                new Color(0.22f, 0.55f, 0.95f, 1f), 42f, x: BarLeft, width: BarWidth);
            Hp.SetUnavailable();
            Mp.SetUnavailable();

            _mapName = UiBuilder.MakeText(transform, font, "Map Name", 11, false);
            SetRect(_mapName.rectTransform, BarLeft, 66f, BarWidth, 17f);
            _mapName.alignment = TextAnchor.MiddleLeft;
            _mapName.color = Color.white;
            _mapName.text = "Bản đồ chưa xác định";
        }

        private void BuildLevelBadge(Font font, Transform frame)
        {
            var badge = MakeImage(frame, "Level Badge", new Color(0.95f, 0.6f, 0.2f, 1f));
            RoundedUiSprite.Apply(badge);
            var br = badge.rectTransform;
            br.anchorMin = br.anchorMax = new Vector2(0f, 0f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.anchoredPosition = new Vector2(BadgeSize * 0.15f, BadgeSize * 0.15f);
            br.sizeDelta = new Vector2(BadgeSize, BadgeSize);

            var inner = MakeImage(badge.transform, "Level Inner", new Color(0.98f, 0.75f, 0.2f, 1f));
            RoundedUiSprite.Apply(inner);
            Inset(inner.rectTransform, 2f);

            _levelBadge = UiBuilder.MakeText(inner.transform, font, "Level", 13, true);
            _levelBadge.text = "--";
            _levelBadge.fontStyle = FontStyle.Bold;
            _levelBadge.alignment = TextAnchor.MiddleCenter;
            _levelBadge.color = new Color(0.15f, 0.08f, 0.02f, 1f);
        }

        private static Image MakeImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void SetRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void Inset(RectTransform rect, float amount)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(amount, amount);
            rect.offsetMax = new Vector2(-amount, -amount);
        }
    }
}
