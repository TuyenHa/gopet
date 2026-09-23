using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Trang xếp hạng Phú Hộ trong bảng Trần Trấn.</summary>
    public sealed class TopPatronView : MonoBehaviour
    {
        // Dòng mô tả kết thúc tại y=43; chừa thêm 15px trước hàng xếp hạng đầu tiên.
        private const float HeaderHeight = 58f;
        private const float RowHeight = 61f;
        private const float Gap = 2f;
        private const float ListWidth = 420f;
        private readonly List<GameObject> _rows = new List<GameObject>();
        private Font _font;
        private RectTransform _content;
        private ScrollRect _scrollRect;

        public static TopPatronView Create(Transform parent, Font font)
        {
            var root = new GameObject("TopPatronView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = Color.clear;
            var view = root.AddComponent<TopPatronView>();
            view._font = font;
            view.BuildHeader();
            view.BuildScrollableList();
            return view;
        }

        public void Bind(MenuScreen screen, RemoteAssetCache assets)
        {
            foreach (var row in _rows) if (row != null) Destroy(row);
            _rows.Clear();
            if (screen?.Items == null) return;
            for (var i = 0; i < screen.Items.Length; i++) BuildRow(screen.Items[i], i, assets);
            _content.sizeDelta = new Vector2(ListWidth, Mathf.Max(1f, screen.Items.Length * (RowHeight + Gap)));
            _content.anchoredPosition = Vector2.zero;
        }

        private void BuildScrollableList()
        {
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(transform, false);
            var viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = new Vector2(0f, -HeaderHeight);
            viewport.GetComponent<Image>().color = Color.clear;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            _content = (RectTransform)content.transform;
            _content.anchorMin = _content.anchorMax = new Vector2(0.5f, 1f);
            _content.pivot = new Vector2(0.5f, 1f);
            _content.sizeDelta = new Vector2(ListWidth, 1f);
            _content.anchoredPosition = Vector2.zero;

            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.viewport = viewportRect;
            _scrollRect.content = _content;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = RowHeight * 0.7f;
        }

        private void BuildHeader()
        {
            MakeGeneratedIcon(transform, "Trophy", new Vector2(0.5f, 1f), new Vector2(-92f, -17f),
                new Vector2(18f, 18f), new Rect(375f, 80f, 515f, 600f));
            var title = UiBuilder.MakeText(transform, _font, "Title", 12, false);
            title.text = "TOP PHÚ HỘ";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(0.07f, 0.34f, 0.72f, 1f);
            PlaceCentered(title.rectTransform, new Vector2(0f, -19f), new Vector2(150f, 20f));
            MakeGeneratedIcon(transform, "SparkLeft", new Vector2(0.5f, 1f), new Vector2(-88f, -17f),
                new Vector2(10f, 10f), new Rect(100f, 60f, 200f, 270f));
            MakeGeneratedIcon(transform, "SparkRight", new Vector2(0.5f, 1f), new Vector2(88f, -17f),
                new Vector2(10f, 10f), new Rect(980f, 70f, 145f, 250f));

            var subtitle = UiBuilder.MakeText(transform, _font, "Subtitle", 8, false);
            subtitle.text = "Danh sách những người đã phú hộ nhiều nhất";
            subtitle.alignment = TextAnchor.MiddleCenter;
            subtitle.color = new Color(0.32f, 0.38f, 0.48f, 1f);
            PlaceCentered(subtitle.rectTransform, new Vector2(0f, -36f), new Vector2(310f, 14f));
        }

        private void BuildRow(MenuItemInfo item, int index, RemoteAssetCache assets)
        {
            var row = new GameObject($"Patron_{index + 1}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(_content, false);
            _rows.Add(row);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, RowHeight);
            // Content đã nằm dưới header, không cộng HeaderHeight lần thứ hai.
            rect.anchoredPosition = new Vector2(0f, -index * (RowHeight + Gap));
            RoundedUiSprite.Apply(row.GetComponent<Image>());
            row.GetComponent<Image>().color = new Color(0.995f, 0.998f, 1f, 1f);

            if (index < 2)
            {
                MakeGeneratedIcon(row.transform, "Medal", new Vector2(0f, 0.5f), new Vector2(25f, 0f),
                    new Vector2(31f, 36f), index == 0
                        ? new Rect(110f, 640f, 440f, 520f)
                        : new Rect(720f, 630f, 420f, 530f));
            }
            else
            {
                var rank = UiBuilder.MakeText(row.transform, _font, "Rank", 12, true);
                rank.text = (index + 1).ToString();
                rank.alignment = TextAnchor.MiddleCenter;
                rank.color = new Color(0.36f, 0.47f, 0.62f, 1f);
                Place(rank.rectTransform, 8f, 0.5f, new Vector2(0f, 0f), new Vector2(42f, 30f));
            }

            var avatar = new GameObject("Avatar", typeof(RectTransform), typeof(RawImage));
            avatar.transform.SetParent(row.transform, false);
            var avatarRect = (RectTransform)avatar.transform;
            avatarRect.anchorMin = avatarRect.anchorMax = new Vector2(0f, 0.5f);
            avatarRect.pivot = new Vector2(0f, 0.5f);
            avatarRect.anchoredPosition = new Vector2(58f, 0f);
            avatarRect.sizeDelta = new Vector2(37f, 45f);
            var avatarImage = avatar.GetComponent<RawImage>();
            avatarImage.texture = assets?.Placeholder;
            if (assets != null && !string.IsNullOrEmpty(item.ImagePath))
            {
                var expected = item.ImagePath;
                assets.Get(expected, ImagePackets.TypeIcon, avatarImage, texture =>
                {
                    if (this != null && avatarImage != null && item.ImagePath == expected) avatarImage.texture = texture;
                });
            }

            var name = UiBuilder.MakeText(row.transform, _font, "Name", 11, false);
            name.text = item.Title ?? "Người chơi";
            UiBuilder.SetFontStyle(name, FontStyle.Bold);
            name.color = new Color(0.14f, 0.19f, 0.29f, 1f);
            Place(name.rectTransform, 106f, 1f, new Vector2(0f, -13f), new Vector2(210f, 18f));

            var amount = UiBuilder.MakeText(row.transform, _font, "Amount", 9, false);
            amount.text = PatronText(item.Description);
            amount.color = new Color(0.35f, 0.41f, 0.51f, 1f);
            Place(amount.rectTransform, 106f, 1f, new Vector2(0f, -34f), new Vector2(290f, 16f));
        }

        private static string PatronText(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return "Đã phú hộ: — ngọc";
            var match = Regex.Match(description, @"\d[\d.,]*");
            return match.Success ? "Đã phú hộ: <color=#087EF4>" + match.Value + "</color> ngọc" : description;
        }

        private static void MakeGeneratedIcon(Transform parent, string name, Vector2 anchor, Vector2 position,
            Vector2 size, Rect source)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var texture = Resources.Load<Texture2D>("Ui/TopPatron/rank-icons");
            if (texture != null)
            {
                go.GetComponent<Image>().sprite = Sprite.Create(texture, source, new Vector2(0.5f, 0.5f), 100f);
                go.GetComponent<Image>().preserveAspect = true;
            }
        }

        private static void Place(RectTransform rect, float x, float anchorY, Vector2 offset, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, anchorY);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(x + offset.x, offset.y);
            rect.sizeDelta = size;
        }

        private static void PlaceCentered(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
