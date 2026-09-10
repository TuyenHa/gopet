using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Một thanh chỉ số động trong HUD nhân vật.</summary>
    public sealed class StatBar : MonoBehaviour
    {
        private Image _fill;
        private Text _value;
        private bool _showPercent;

        public float FillAmount => _fill == null ? 0f : _fill.fillAmount;
        public string ValueText => _value == null ? string.Empty : _value.text;

        public static StatBar Create(Transform parent, Font font, string name,
            string badge, Color color, float top, bool showPercent = false,
            float x = 100f, float width = 190f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -top);
            rect.sizeDelta = new Vector2(width, 17f);

            var bar = go.AddComponent<StatBar>();
            bar._showPercent = showPercent;
            // Track chiếm phần còn lại sau badge 31 + gap 3.
            bar.Build(font, badge, color, width - 34f);
            bar.SetUnavailable();
            return bar;
        }

        public void SetValue(int current, int maximum)
        {
            if (maximum <= 0)
            {
                SetUnavailable();
                return;
            }

            current = Mathf.Clamp(current, 0, maximum);
            _fill.fillAmount = (float)current / maximum;
            // Style jar: chỉ current/max, không kèm percent — gọn và rõ ràng.
            // EXP giữ percent vì maxExp lớn không đọc số cụ thể được.
            _value.text = _showPercent
                ? $"{100f * current / maximum:0}%"
                : $"{current}/{maximum}";
        }

        public void SetUnavailable()
        {
            _fill.fillAmount = 0f;
            _value.text = _showPercent ? "--%" : "--/--";
        }

        private void Build(Font font, string badge, Color color, float trackWidth)
        {
            var badgeGo = MakeImage(transform, "Badge", new Color(0.77f, 0.9f, 1f, 0.95f));
            SetRect(badgeGo.rectTransform, 0f, 0f, 31f, 17f);
            RoundedUiSprite.Apply(badgeGo);
            var badgeText = UiBuilder.MakeText(badgeGo.transform, font, "Label", 11, true);
            badgeText.text = badge;
            badgeText.fontStyle = FontStyle.Bold;
            badgeText.alignment = TextAnchor.MiddleCenter;
            badgeText.color = new Color(0.04f, 0.26f, 0.52f, 1f);

            var track = MakeImage(transform, "Track", new Color(0.1f, 0.48f, 0.82f, 1f));
            SetRect(track.rectTransform, 34f, 0f, trackWidth, 17f);
            RoundedUiSprite.Apply(track);

            var viewport = MakeImage(track.transform, "Track Surface",
                new Color(0.12f, 0.26f, 0.42f, 0.9f));
            RoundedUiSprite.Apply(viewport);
            Inset(viewport.rectTransform, 1f);
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            _fill = MakeImage(viewport.transform, "Fill", color);
            UiBuilder.Stretch(_fill.rectTransform);
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _fill.fillOrigin = 0;

            _value = UiBuilder.MakeText(viewport.transform, font, "Value", 11, true);
            _value.fontStyle = FontStyle.Bold;
            _value.alignment = TextAnchor.MiddleCenter;
            _value.color = Color.white;
            var shadow = _value.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(1f, -1f);
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
