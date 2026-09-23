using System;
using Gopet.Net.Images;
using Gopet.Net.Map;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Small persistent HUD chip for the active EXP multiplier.</summary>
    public sealed class ExpBuffIndicator : MonoBehaviour
    {
        private RawImage _icon;
        private Text _time;
        private long _expiresAt;

        public static ExpBuffIndicator Create(Transform hudParent)
        {
            var go = new GameObject("EXP Buff", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(hudParent, false);
            var rect = (RectTransform)go.transform;
            // Neo vào dòng ngay bên dưới thanh sao/vàng/đậu/lượng, cùng mép trái.
            // Bỏ qua layout chính: indicator không làm thay đổi kích thước CurrencyBar.
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(12f, -144f);
            rect.sizeDelta = new Vector2(42f, 42f);
            var layout = go.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            var panel = go.GetComponent<Image>();
            // Cùng kiểu panel bo góc với HUD nhân vật và các thanh HP/MP.
            panel.color = Color.clear;

            var indicator = go.AddComponent<ExpBuffIndicator>();
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = (RectTransform)iconGo.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(4f, 0f);
            iconRect.sizeDelta = new Vector2(34f, 34f);
            indicator._icon = iconGo.GetComponent<RawImage>();

            indicator._time = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Time", 13, false);
            indicator._time.alignment = TextAnchor.MiddleLeft;
            indicator._time.gameObject.SetActive(false);
            var timeRect = indicator._time.rectTransform;
            timeRect.anchorMin = new Vector2(0f, 0f);
            timeRect.anchorMax = new Vector2(1f, 1f);
            timeRect.offsetMin = new Vector2(43f, 0f);
            timeRect.offsetMax = new Vector2(-4f, 0f);
            go.SetActive(false);
            return indicator;
        }

        public void Apply(ExpBuffStatus status, RemoteAssetCache assets)
        {
            if (status == null) return;
            _expiresAt = status.ExpiresAtUnixSeconds;
            gameObject.SetActive(_expiresAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            if (assets != null && !string.IsNullOrEmpty(status.IconPath))
                assets.Get(status.IconPath, ImagePackets.TypeIcon, _icon, texture =>
                {
                    if (_icon != null) _icon.texture = texture;
                });
            RefreshText();
        }

        private void Update()
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= _expiresAt)
            {
                gameObject.SetActive(false);
                return;
            }
            RefreshText();
        }

        private void RefreshText()
        {
            if (_time == null) return;
            var remaining = Math.Max(0L, _expiresAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            var span = TimeSpan.FromSeconds(remaining);
            _time.text = span.TotalHours >= 1d
                ? $"Tăng EXP  {Math.Floor(span.TotalHours):00}:{span.Minutes:00}:{span.Seconds:00}"
                : $"Tăng EXP  {span.Minutes:00}:{span.Seconds:00}";
        }
    }
}
