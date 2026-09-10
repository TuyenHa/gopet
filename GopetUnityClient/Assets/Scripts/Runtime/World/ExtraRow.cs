using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Một dòng phụ trong <see cref="CurrencyBar"/> — icon do server bơm + số lượng.</summary>
    public sealed class ExtraRow : MonoBehaviour
    {
        private const float IconSize = 18f;
        private const float RowHeight = 22f;

        private Image _icon;
        private Text _count;

        public static ExtraRow Create(Transform parent, Font font, RemoteAssetCache assets, string iconPath)
        {
            var go = new GameObject($"Extra:{iconPath}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(0f, RowHeight);
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = RowHeight;
            layout.minHeight = RowHeight;

            var row = go.AddComponent<ExtraRow>();
            row.Build(font);
            row.LoadIcon(assets, iconPath);
            return row;
        }

        public void SetCount(long count) => _count.text = count.ToString("N0");

        private void Build(Font font)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(transform, false);
            _icon = iconGo.GetComponent<Image>();
            _icon.color = new Color(1f, 1f, 1f, 0.55f);
            _icon.raycastTarget = false;
            var iconRect = _icon.rectTransform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(2f, 0f);
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);

            _count = UiBuilder.MakeText(transform, font, "Count", 13, false);
            _count.fontStyle = FontStyle.Bold;
            _count.alignment = TextAnchor.MiddleLeft;
            var countRect = _count.rectTransform;
            countRect.anchorMin = new Vector2(0f, 0f);
            countRect.anchorMax = new Vector2(1f, 1f);
            countRect.pivot = new Vector2(0f, 0.5f);
            countRect.offsetMin = new Vector2(IconSize + 6f, 0f);
            countRect.offsetMax = new Vector2(-2f, 0f);
            _count.text = "--";
        }

        private void LoadIcon(RemoteAssetCache assets, string iconPath)
        {
            if (assets == null || string.IsNullOrEmpty(iconPath)) return;
            assets.Get(iconPath, ImagePackets.TypeIcon, texture =>
            {
                if (texture == null || _icon == null) return;
                _icon.sprite = Sprite.Create(texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), 100f);
                _icon.color = Color.white;
            });
        }
    }
}
