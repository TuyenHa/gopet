using System;
using System.Collections.Generic;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    public sealed class BattleActionBar : MonoBehaviour
    {
        /// <summary>Chỉ các nút bị khoá theo lượt. Nút xin thua CỐ Ý nằm ngoài danh sách:
        /// người chơi phải bỏ cuộc được cả trong lượt quái, và trước đây nó nằm trong đây
        /// nên <see cref="LockSurrender"/> bị huỷ ngay ở gói lượt kế tiếp.</summary>
        private readonly List<Button> _actionButtons = new List<Button>();
        private Button _surrenderBtn;
        private static Sprite _rounded;

        public event Action AttackClicked;
        public event Action PotionClicked;
        public event Action SurrenderClicked;

        public static BattleActionBar Create(Transform parent, Font font, bool isParticipant)
        {
            var go = new GameObject("Thanh hành động", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 120f);

            var bar = go.AddComponent<BattleActionBar>();

            bar._surrenderBtn = MakeSurrenderBtn(go.transform, font);
            bar._surrenderBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayEffect("s_button_ingame");
                bar.SurrenderClicked?.Invoke();
            });

            var potionBtn = MakeItemBtn(go.transform, font);
            potionBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayEffect("s_button_ingame");
                bar.PotionClicked?.Invoke();
            });
            bar._actionButtons.Add(potionBtn);

            var attackBtn = MakeAttackBtn(go.transform, font);
            attackBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayEffect("s_attack");
                bar.AttackClicked?.Invoke();
            });
            bar._actionButtons.Add(attackBtn);

            if (!isParticipant) go.SetActive(false);
            return bar;
        }

        private static Button MakeSurrenderBtn(Transform parent, Font font)
        {
            var go = new GameObject("Xin thua", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(12f, 12f);
            rect.sizeDelta = new Vector2(100f, 40f);
            var img = go.GetComponent<Image>();
            img.sprite = RoundedSprite(); img.type = Image.Type.Sliced;
            img.color = new Color(0.72f, 0.14f, 0.14f, 1f);
            var text = UiBuilder.MakeText(go.transform, font, "Nhãn", 15, true);
            text.text = "Xin thua";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            return go.GetComponent<Button>();
        }

        private static Button MakeItemBtn(Transform parent, Font font)
        {
            var go = new GameObject("Thuốc", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-124f, 20f);
            rect.sizeDelta = new Vector2(52f, 52f);
            var img = go.GetComponent<Image>();
            img.sprite = BattleSkin.Load("Battle/btn-item");
            img.preserveAspect = true;
            img.color = img.sprite == null ? UiBuilder.ButtonFace : Color.white;
            var text = UiBuilder.MakeText(go.transform, font, "Nhãn", 13, true);
            text.text = "Thuốc";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.gameObject.SetActive(img.sprite == null);
            return go.GetComponent<Button>();
        }

        private static Button MakeAttackBtn(Transform parent, Font font)
        {
            var go = new GameObject("Tấn công", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-12f, 20f);
            rect.sizeDelta = new Vector2(104f, 104f);
            var img = go.GetComponent<Image>();
            img.sprite = BattleSkin.Load("Battle/btn-attack-big");
            img.preserveAspect = true;
            img.color = img.sprite == null ? new Color(0.6f, 0.2f, 0.15f, 1f) : Color.white;
            var text = UiBuilder.MakeText(go.transform, font, "Nhãn", 15, true);
            text.text = "Tấn công";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.gameObject.SetActive(img.sprite == null);
            return go.GetComponent<Button>();
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

        /// <summary>Bật/tắt nút hành động theo lượt. Thay cho cặp Lock/Unlock cũ vốn mở
        /// lại sau 3.5s bất kể có tới lượt hay chưa — server sẽ trả
        /// <c>redDialog("Chưa tới lượt của bạn")</c> (<c>PetBattle.cs:317</c>).</summary>
        public void SetActionsInteractable(bool on)
        {
            foreach (var b in _actionButtons) b.interactable = on;
        }

        public void LockSurrender()
        {
            if (_surrenderBtn != null) _surrenderBtn.interactable = false;
        }
    }
}
