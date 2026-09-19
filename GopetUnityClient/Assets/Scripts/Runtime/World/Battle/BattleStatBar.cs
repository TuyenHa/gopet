using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    public sealed class BattleStatBar : MonoBehaviour
    {
        private Image _fill;
        private Text _label;
        private int _current, _max;
        private Color _normalColor;
        private float _blinkTimer;
        private bool _blinkVisible = true;
        private static Sprite _rounded;

        public static BattleStatBar Create(Transform parent, Font font, string label,
            Color barColor, float top, float width)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(4f, -(top + 16f));
            rect.offsetMax = new Vector2(-4f, -top);

            var lblText = UiBuilder.MakeText(go.transform, font, label + " lbl", 11, false);
            var lblRect = lblText.rectTransform;
            lblRect.anchorMin = new Vector2(0f, 0f);
            lblRect.anchorMax = new Vector2(0f, 1f);
            lblRect.pivot = new Vector2(0f, 0.5f);
            lblRect.sizeDelta = new Vector2(26f, 0f);
            lblRect.anchoredPosition = Vector2.zero;
            lblText.text = label;
            lblText.alignment = TextAnchor.MiddleLeft;
            lblText.fontStyle = FontStyle.Bold;
            lblText.color = Color.white;

            var barGo = new GameObject("Bar", typeof(RectTransform));
            barGo.transform.SetParent(go.transform, false);
            var barRect = (RectTransform)barGo.transform;
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.offsetMin = new Vector2(28f, 0f);
            barRect.offsetMax = Vector2.zero;

            var bg = new GameObject("Nền", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(barGo.transform, false);
            UiBuilder.Stretch((RectTransform)bg.transform);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = RoundedSprite(); bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(barGo.transform, false);
            var fillRect = (RectTransform)fillGo.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var bar = go.AddComponent<BattleStatBar>();
            bar._fill = fillGo.GetComponent<Image>();
            bar._fill.sprite = RoundedSprite(); bar._fill.type = Image.Type.Sliced;
            bar._fill.color = barColor;
            bar._normalColor = barColor;

            bar._label = UiBuilder.MakeText(barGo.transform, font, "Value", 12, true);
            bar._label.alignment = TextAnchor.MiddleCenter;
            bar._label.color = Color.white;
            bar._label.fontStyle = FontStyle.Bold;
            return bar;
        }

        private static Sprite RoundedSprite()
        {
            if (_rounded != null) return _rounded;
            const int r = 5, s = r * 2 + 2;
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

        public void Set(int current, int max)
        {
            _current = Mathf.Max(0, current);
            _max = Mathf.Max(1, max);
            Refresh();
        }

        private void Refresh()
        {
            var ratio = Mathf.Clamp01((float)_current / _max);
            _fill.rectTransform.anchorMax = new Vector2(ratio, 1f);
            _label.text = $"{_current}/{_max}";
        }

        private void Update()
        {
            if (_max <= 0) return;
            var lowHp = _current * 4 <= _max;
            if (!lowHp) { _fill.color = _normalColor; return; }
            _blinkTimer += Time.unscaledDeltaTime;
            if (_blinkTimer >= 0.5f)
            {
                _blinkTimer = 0f;
                _blinkVisible = !_blinkVisible;
                var c = _normalColor;
                _fill.color = _blinkVisible ? c : new Color(c.r, c.g, c.b, 0.3f);
            }
        }
    }
}
