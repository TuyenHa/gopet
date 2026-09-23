using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        private static void NormalizeActionLayout(Button button)
        {
            if (button == null) return;
            var icon = button.transform.Find("Action icon") as RectTransform;
            if (icon != null)
            {
                icon.anchorMin = icon.anchorMax = new Vector2(0f, 0.5f);
                icon.pivot = new Vector2(0.5f, 0.5f);
                icon.sizeDelta = new Vector2(30f, 30f);
                icon.anchoredPosition = new Vector2(28f, 0f);
            }

            var text = button.GetComponentInChildren<Text>();
            if (text == null) return;
            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
                textRect.offsetMin = new Vector2(54f, 0f);
            textRect.offsetMax = new Vector2(-12f, 0f);
            textRect.sizeDelta = Vector2.zero;
            text.alignment = TextAnchor.MiddleLeft;
        }

        private static void MakeSwitch(Transform parent, bool enabled, UnityEngine.Events.UnityAction toggle)
        {
            var go = new GameObject("Switch", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(38f, 20f);
            rect.anchoredPosition = new Vector2(-8f, 0f);
            var track = go.GetComponent<Image>();
            RoundedUiSprite.Apply(track);
            track.color = enabled
                ? new Color(0.96f, 0.25f, 0.19f, 1f)
                : new Color(0.66f, 0.70f, 0.76f, 1f);
            var trackOutline = go.AddComponent<Outline>();
            trackOutline.effectColor = enabled
                ? new Color(0.72f, 0.18f, 0.13f, 0.55f)
                : new Color(0.43f, 0.47f, 0.53f, 0.55f);
            trackOutline.effectDistance = new Vector2(0.5f, -0.5f);
            var knob = new GameObject("Knob", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            knob.SetParent(go.transform, false);
            knob.anchorMin = knob.anchorMax = new Vector2(enabled ? 1f : 0f, 0.5f);
            knob.pivot = new Vector2(0.5f, 0.5f);
            knob.sizeDelta = new Vector2(16f, 16f);
            knob.anchoredPosition = new Vector2(enabled ? -10f : 10f, 0f);
            var knobImage = knob.GetComponent<Image>();
            knobImage.sprite = SwitchKnobSprite();
            knobImage.type = Image.Type.Simple;
            knobImage.color = new Color(0.97f, 0.98f, 1f, 1f);
            var knobShadow = knob.gameObject.AddComponent<Shadow>();
            knobShadow.effectColor = new Color(0f, 0f, 0f, 0.22f);
            knobShadow.effectDistance = new Vector2(1f, -1f);
            go.GetComponent<Button>().onClick.AddListener(toggle);
        }

        private static void UpdateSwitch(Transform parent, bool enabled)
        {
            var knob = parent.Find("Switch/Knob") as RectTransform;
            if (knob == null) return;
            var track = knob.parent.GetComponent<Image>();
            if (track != null)
            {
                track.color = enabled
                    ? new Color(0.96f, 0.25f, 0.19f, 1f)
                    : new Color(0.66f, 0.70f, 0.76f, 1f);
                var outline = track.GetComponent<Outline>();
                if (outline != null)
                    outline.effectColor = enabled
                        ? new Color(0.72f, 0.18f, 0.13f, 0.55f)
                        : new Color(0.43f, 0.47f, 0.53f, 0.55f);
            }
            knob.anchorMin = knob.anchorMax = new Vector2(enabled ? 1f : 0f, 0.5f);
            knob.anchoredPosition = new Vector2(enabled ? -10f : 10f, 0f);
        }

        private static Sprite _switchKnobSprite;

        private static Sprite SwitchKnobSprite()
        {
            if (_switchKnobSprite != null) return _switchKnobSprite;
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Settings switch knob";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;
            var pixels = new Color32[size * size];
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                var alpha = Mathf.Clamp01(center + 0.5f - distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            _switchKnobSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f), 100f);
            _switchKnobSprite.name = "Settings switch knob";
            _switchKnobSprite.hideFlags = HideFlags.HideAndDontSave;
            return _switchKnobSprite;
        }

    }
}
