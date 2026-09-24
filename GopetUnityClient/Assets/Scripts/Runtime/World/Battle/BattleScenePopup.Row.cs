using System.Globalization;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Một dòng khung cảnh: ảnh thu nhỏ | tên + trạng thái | nút hành động.</summary>
    public sealed partial class BattleScenePopup
    {
        private const float ThumbWidth = 66f;
        private const float ButtonWidth = 92f;

        private static readonly Color Gold = new Color(1f, 0.85f, 0.35f);
        private static readonly Color Green = new Color(0.45f, 0.85f, 0.45f);
        private static readonly Color Red = new Color(1f, 0.45f, 0.4f);

        private void AddRow(BattleSceneState.Entry entry)
        {
            var go = new GameObject(entry.Name, typeof(RectTransform), typeof(Image),
                typeof(LayoutElement));
            go.transform.SetParent(_content, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(0f, RowHeight);
            // Bắt buộc: VerticalLayoutGroup không suy chiều cao từ sizeDelta (xem BattleSkillPopup.Row).
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = le.minHeight = RowHeight;

            var selected = _state.SelectedId == entry.Id;
            var plate = go.GetComponent<Image>();
            plate.sprite = PanelSprites.Rounded(5);
            plate.type = Image.Type.Sliced;
            plate.color = selected ? new Color(0.20f, 0.30f, 0.20f, 0.95f) : new Color(0.13f, 0.19f, 0.30f, 0.9f);

            AddThumb(go.transform, entry.Id);

            var textLeft = Pad + ThumbWidth + 8f;
            var name = MakeLabel(go.transform, "Tên", 13, new Vector2(0f, 0.5f), Vector2.one,
                new Vector2(textLeft, 0f), new Vector2(-(ButtonWidth + Pad * 2f), -4f), TextAnchor.LowerLeft);
            name.text = entry.Name;
            UiBuilder.SetFontStyle(name, FontStyle.Bold);

            var info = MakeLabel(go.transform, "Trạng thái", 11, Vector2.zero, new Vector2(1f, 0.5f),
                new Vector2(textLeft, 4f), new Vector2(-(ButtonWidth + Pad * 2f), 0f), TextAnchor.UpperLeft);
            var affordable = !_gold.HasValue || _gold.Value >= entry.PriceGold;
            if (selected) { info.text = "Đang dùng"; info.color = Green; }
            else if (entry.Owned) { info.text = "Đã sở hữu"; info.color = UiBuilder.TextMuted; }
            else
            {
                info.text = $"Giá: {FormatGold(entry.PriceGold)} vàng";
                info.color = affordable ? Gold : Red;
            }

            if (selected) return; // khung cảnh đang dùng: không cần nút
            if (entry.Owned) AddButton(go.transform, "Chọn", UiBuilder.ButtonFace, true, () => OnSelect(entry.Id));
            else AddButton(go.transform, affordable ? "Mua" : "Thiếu vàng",
                new Color(0.80f, 0.55f, 0.12f, 1f), affordable, () => OnBuy(entry));
        }

        private static void AddThumb(Transform row, int sceneId)
        {
            var go = new GameObject("Ảnh", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(row, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(Pad, 0f);
            rect.sizeDelta = new Vector2(ThumbWidth, ThumbWidth * 2f / 3f); // nền tỉ lệ 3:2
            var img = go.GetComponent<Image>();
            img.sprite = BattleSkin.Load(BattleSceneCatalog.BackgroundPath(sceneId));
            img.color = img.sprite == null ? new Color(0.2f, 0.25f, 0.3f) : Color.white;
            img.raycastTarget = false;
        }

        private void AddButton(Transform row, string label, Color face, bool interactable,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Nút", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(row, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-Pad, 0f);
            rect.sizeDelta = new Vector2(ButtonWidth, 30f);
            var img = go.GetComponent<Image>();
            // Kiểu nút chung (khung vàng, mặt xanh) như nút "Kích ẩn". Thiếu ảnh thì về nút bo góc màu phẳng.
            if (!GameButtonSkin.Apply(img, rect.sizeDelta.y))
            {
                img.sprite = PanelSprites.Rounded(6);
                img.type = Image.Type.Sliced;
                img.color = face;
            }
            var text = UiBuilder.MakeText(go.transform, _font, "Nhãn", 13, true);
            text.text = label;
            GameButtonSkin.StyleLabel(text);
            var button = go.GetComponent<Button>();
            button.targetGraphic = img;
            button.interactable = interactable && !_busy;
            button.onClick.AddListener(onClick);
        }

        private Text MakeLabel(Transform parent, string name, int size, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAnchor align)
        {
            var text = UiBuilder.MakeText(parent, _font, name, size, false);
            var rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            text.alignment = align;
            text.color = UiBuilder.TextMain;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        /// <summary>12000 → "12.000" (dấu chấm ngăn nghìn kiểu Việt, không phụ thuộc culture máy).</summary>
        internal static string FormatGold(long value) =>
            value.ToString("#,0", CultureInfo.InvariantCulture).Replace(',', '.');
    }
}
