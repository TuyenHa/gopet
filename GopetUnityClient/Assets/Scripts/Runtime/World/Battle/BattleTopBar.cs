using System;
using Gopet.Net.Battle;
using Gopet.Net.Player;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    public sealed class BattleTopBar : MonoBehaviour
    {
        private Text _starLabel, _coinLabel, _levelLabel;

        public event Action BackClicked;

        public static BattleTopBar Create(Transform parent, Font font, BattleKind kind,
            PlayerStats stats)
        {
            var go = new GameObject("Top bar", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(0f, -32f);
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.02f, 0.1f, 0.18f, 0.97f);

            var bar = go.AddComponent<BattleTopBar>();
            BuildBackButton(go.transform, font, bar);
            BuildTitleGroup(go.transform, font, kind);

            bar._starLabel = CurrencySlot(go.transform, font, "Battle/icon-crit", "star", 0.76f);
            bar._coinLabel = CurrencySlot(go.transform, font, "Battle/icon-coin", "coin", 0.86f);
            bar._levelLabel = CurrencySlot(go.transform, font, "Battle/icon-gold-bar", "level", 0.95f);

            if (stats != null) bar.UpdateCurrency(stats);
            return bar;
        }

        private static void BuildBackButton(Transform parent, Font font, BattleTopBar bar)
        {
            var go = new GameObject("Quay lại", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(6f, 0f);
            rect.sizeDelta = new Vector2(84f, 24f);
            var outerImg = go.GetComponent<Image>();
            RoundedUiSprite.Apply(outerImg);
            outerImg.color = new Color(0.36f, 0.5f, 0.68f, 0.9f);

            var inner = new GameObject("Nền", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(go.transform, false);
            var iRect = (RectTransform)inner.transform;
            iRect.anchorMin = Vector2.zero; iRect.anchorMax = Vector2.one;
            iRect.offsetMin = new Vector2(1.5f, 1.5f); iRect.offsetMax = new Vector2(-1.5f, -1.5f);
            var innerImg = inner.GetComponent<Image>();
            RoundedUiSprite.Apply(innerImg);
            innerImg.color = new Color(0.08f, 0.14f, 0.23f, 0.95f);
            innerImg.raycastTarget = false;

            var text = UiBuilder.MakeText(go.transform, font, "Nhãn", 13, true);
            text.text = "‹ Quay lại";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.87f, 0.91f, 0.97f, 1f);
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;

            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayEffect("s_button_ingame");
                bar.BackClicked?.Invoke();
            });
        }

        private static void BuildTitleGroup(Transform parent, Font font, BattleKind kind)
        {
            var group = new GameObject("Tiêu đề", typeof(RectTransform),
                typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            group.transform.SetParent(parent, false);
            var gRect = (RectTransform)group.transform;
            gRect.anchorMin = gRect.anchorMax = new Vector2(0.5f, 0.5f);
            gRect.pivot = new Vector2(0.5f, 0.5f);
            gRect.anchoredPosition = Vector2.zero;

            var layout = group.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 6f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var fitter = group.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconGo.transform.SetParent(group.transform, false);
            var icon = iconGo.GetComponent<Image>();
            icon.sprite = BattleSkin.Load("Battle/icon-title-swords", "Battle/icon-atk");
            icon.preserveAspect = true;
            var le = iconGo.GetComponent<LayoutElement>();
            le.preferredWidth = 20f; le.preferredHeight = 20f;

            var title = UiBuilder.MakeText(group.transform, font, "Tiêu đề", 16, false);
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            title.text = kind == BattleKind.Mob ? "ĐÁNH QUÁI" : "ĐẤU TRƯỜNG PET";
        }

        public void UpdateCurrency(PlayerStats stats)
        {
            if (stats == null) return;
            _starLabel.text = CompactNumberFormat.Format(stats.Star);
            _coinLabel.text = CompactNumberFormat.Format(stats.Coin);
            _levelLabel.text = $"L{stats.Gold}";
        }

        private static Text CurrencySlot(Transform parent, Font font, string iconRes,
            string name, float xAnchor)
        {
            var slot = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            slot.transform.SetParent(parent, false);
            var r = (RectTransform)slot.transform;
            r.anchorMin = new Vector2(xAnchor - 0.04f, 0f);
            r.anchorMax = new Vector2(xAnchor + 0.04f, 1f);
            r.offsetMin = r.offsetMax = Vector2.zero;
            var layout = slot.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 3f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconGo.transform.SetParent(slot.transform, false);
            var icon = iconGo.GetComponent<Image>();
            icon.sprite = BattleSkin.Load(iconRes);
            icon.preserveAspect = true;
            var le = iconGo.GetComponent<LayoutElement>();
            le.preferredWidth = 14f; le.preferredHeight = 14f;

            var text = UiBuilder.MakeText(slot.transform, font, name, 12, false);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.95f, 0.85f, 0.3f, 1f);
            return text;
        }
    }
}
