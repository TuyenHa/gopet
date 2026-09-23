using System.Collections.Generic;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Hàng ngang 4 mục tiền tệ chính (Star / Gold / Coin / Lua) đặt giữa-trên màn hình,
    /// style tham chiếu ảnh jar (icon tròn màu + số ngay bên phải).
    ///
    /// <para>Icon là procedural — mỗi loại 1 badge tròn màu riêng + chữ đầu bên trong
    /// (S/V/Đ/L). Không dùng asset ngoài, không lệ thuộc image-gen. Nếu tương lai có
    /// atlas icon từ jar (<c>pet/icons.png</c>), thay <c>MakeIcon</c> bằng sprite.</para>
    ///
    /// <para>MoneyDisplays động (mảnh event) tiếp phía sau 4 currency cố định, icon lấy
    /// từ server qua <see cref="RemoteAssetCache"/>.</para>
    /// </summary>
    public sealed class CurrencyBar : MonoBehaviour
    {
        // Layout hàng ngang COMPACT — không chèn HUD trái (CharacterHud) & phải (ShopServiceEventHud).
        // Bản compact để cụm vàng/đậu/lượng không chạm nhóm icon cửa hàng bên phải.
        private const float IconSize = 14f;
        private const float ItemMinWidth = 48f;
        private const float ItemHeight = 20f;
        private const float ItemGap = 4f;
        private const float PanelPadding = 4f;

        /// <summary>
        /// Mép trái thanh tài nguyên, tính từ lề trái màn: đặt CẠNH PHẢI của CharacterHud
        /// (rộng 240 + margin 12 = 252, cộng thêm khe thở). Băng thông báo ngay dưới dùng
        /// chung con số này để hai thanh thẳng cột với nhau.
        /// </summary>
        public const float LeftMargin = 268f;

        private static readonly (string label, Color color)[] IconStyles =
        {
            ("★", new Color(0.98f, 0.82f, 0.20f, 1f)),   // Star vàng
            ("V", new Color(0.98f, 0.75f, 0.15f, 1f)),   // Vàng - amber
            ("Đ", new Color(0.85f, 0.35f, 0.35f, 1f)),   // Đậu - đỏ
            ("L", new Color(0.60f, 0.85f, 0.35f, 1f)),   // Lúa - xanh lá
        };

        private readonly Dictionary<string, ExtraRow> _extraRows = new Dictionary<string, ExtraRow>();
        private RemoteAssetCache _assets;
        private Font _font;
        private HorizontalLayoutGroup _layout;
        private Text _star;
        private Text _gold;
        private Text _coin;
        private Text _lua;
        private RectTransform _extrasParent;

        public static CurrencyBar Create(Transform parent, RemoteAssetCache assets)
        {
            var go = new GameObject("Currency Bar", typeof(RectTransform), typeof(Image),
                typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);

            // Neo top-left, xem LeftMargin; tránh chạm shop icons ở góc phải-trên
            // (ShopServiceEventHud).
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(LeftMargin, -8f);
            rect.sizeDelta = new Vector2(0f, ItemHeight + PanelPadding * 2);

            var panel = go.GetComponent<Image>();
            panel.raycastTarget = false;
            RoundedUiSprite.Apply(panel);
            panel.color = new Color(0.08f, 0.11f, 0.16f, 0.72f);

            var bar = go.AddComponent<CurrencyBar>();
            bar._assets = assets;
            bar._font = UiBuilder.DefaultFont();
            bar._layout = go.GetComponent<HorizontalLayoutGroup>();
            bar._layout.padding = new RectOffset(
                (int)PanelPadding, (int)PanelPadding, (int)(PanelPadding * 0.5f), (int)(PanelPadding * 0.5f));
            bar._layout.spacing = ItemGap;
            bar._layout.childAlignment = TextAnchor.MiddleCenter;
            bar._layout.childControlWidth = true;
            bar._layout.childControlHeight = true;
            bar._layout.childForceExpandWidth = false;
            bar._layout.childForceExpandHeight = false;
            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            bar.Build();
            bar.ApplyFallback();
            return bar;
        }

        public void ApplyStats(PlayerStats stats)
        {
            if (stats == null) return;
            _star.text = Format(stats.Star);
            _gold.text = Format(stats.Gold);
            _coin.text = Format(stats.Coin);
            _lua.text = Format(stats.Lua);
            SyncExtras(stats.Extras);
        }

        private void ApplyFallback()
        {
            _star.text = "--";
            _gold.text = "--";
            _coin.text = "--";
            _lua.text = "--";
        }

        private void Build()
        {
            _star = MakeItem("Star", IconStyles[0]);
            _gold = MakeItem("Gold", IconStyles[1]);
            _coin = MakeItem("Coin", IconStyles[2]);
            _lua  = MakeItem("Lua",  IconStyles[3]);

            _extrasParent = new GameObject("Extras", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            _extrasParent.SetParent(transform, false);
            var extraLayout = _extrasParent.GetComponent<HorizontalLayoutGroup>();
            extraLayout.spacing = ItemGap;
            extraLayout.childAlignment = TextAnchor.MiddleCenter;
            extraLayout.childControlWidth = true;
            extraLayout.childControlHeight = true;
            extraLayout.childForceExpandWidth = false;
            extraLayout.childForceExpandHeight = false;
            var le = _extrasParent.gameObject.AddComponent<LayoutElement>();
            le.minHeight = ItemHeight;
        }

        private Text MakeItem(string name, (string label, Color color) style)
        {
            var go = new GameObject($"Item:{name}", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(transform, false);
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            // childControlWidth = true là bắt buộc: nếu false, HorizontalLayoutGroup KHÔNG
            // ghi đè sizeDelta của child → icon/text render ở 100×100 mặc định của
            // GameObject mới → item to bằng ½ màn hình. childForceExpandWidth = false
            // giữ chiều rộng đúng theo preferredWidth của child chứ không kéo giãn.
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = ItemMinWidth;
            le.preferredWidth = ItemMinWidth;
            le.preferredHeight = ItemHeight;

            MakeIcon(go.transform, style);

            var text = UiBuilder.MakeText(go.transform, _font, "Value", 11, false);
            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
            var tle = text.gameObject.AddComponent<LayoutElement>();
            tle.preferredWidth = 32f;   // đủ cho "999.9k" ở cỡ chữ compact
            tle.preferredHeight = ItemHeight;
            var shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
            shadow.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private static void MakeIcon(Transform parent, (string label, Color color) style)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(parent, false);
            var img = iconGo.GetComponent<Image>();
            img.color = style.color;
            RoundedUiSprite.Apply(img);
            var le = iconGo.AddComponent<LayoutElement>();
            le.minWidth = IconSize;
            le.minHeight = IconSize;
            le.preferredWidth = IconSize;
            le.preferredHeight = IconSize;

            var label = UiBuilder.MakeText(iconGo.transform, UiBuilder.DefaultFont(), "Glyph", 12, true);
            label.text = style.label;
            label.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.color = new Color(0.15f, 0.08f, 0.02f, 1f);
        }

        /// <summary>
        /// Format số ngắn: &lt;1000 số thô; &lt;1M dạng "12.3k"; &gt;=1M dạng "1.2M".
        /// Giữ bar gọn để không đè HUD trái/phải.
        /// </summary>
        private static string Format(long value)
        {
            if (value < 1000) return value.ToString();
            if (value < 1_000_000) return $"{value / 1000f:0.#}k";
            if (value < 1_000_000_000) return $"{value / 1_000_000f:0.#}M";
            return $"{value / 1_000_000_000f:0.#}B";
        }

        private static string Format(int value) => Format((long)value);

        private void SyncExtras(IReadOnlyList<MoneyDisplay> extras)
        {
            var seen = new HashSet<string>();
            foreach (var extra in extras)
            {
                seen.Add(extra.IconPath);
                if (!_extraRows.TryGetValue(extra.IconPath, out var row))
                {
                    row = ExtraRow.Create(_extrasParent, _font, _assets, extra.IconPath);
                    _extraRows[extra.IconPath] = row;
                }
                row.SetCount(extra.Count);
            }
            var stale = new List<string>();
            foreach (var kv in _extraRows) if (!seen.Contains(kv.Key)) stale.Add(kv.Key);
            foreach (var key in stale)
            {
                Destroy(_extraRows[key].gameObject);
                _extraRows.Remove(key);
            }
        }
    }
}
