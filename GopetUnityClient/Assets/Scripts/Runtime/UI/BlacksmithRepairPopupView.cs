using System;
using System.Text.RegularExpressions;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Chi tiết một trang bị ở Thợ Rèn: icon, tên, mô tả, thanh độ bền (vd 36/80) và nút
    /// "Sửa chữa". Bấm sửa thì chạy
    /// <see cref="GrindstoneRepairEffect"/> xong mới gửi lệnh lên server.
    ///
    /// <para>Khung dựng qua <see cref="GamePopupFrame"/> nên viền ngoài liền một nét và
    /// bốn góc bo đúng màu — không dùng <see cref="Outline"/> (góc nhoè, lộ trắng).</para>
    /// </summary>
    public sealed class BlacksmithRepairPopupView : MonoBehaviour
    {
        private const float Width = 440f;
        private const float Height = 240f;
        private const float IconSize = 76f;
        private const float TextLeft = IconSize + 14f;
        private const float ButtonWidth = 150f;
        private const float ButtonHeight = 36f;

        /// <summary>Khớp <c>EquipDurability.Describe</c> (server): "Độ bền: X/Max".</summary>
        private static readonly Regex DurabilityPattern = new Regex(@"\s*Độ bền:\s*(\d+)\s*/\s*(\d+)");

        private Image _barFill;
        private Text _durabilityLabel;
        private Button _repairButton;
        private int _max;
        private bool _repairing;

        /// <summary>Đọc độ bền từ mô tả server gửi. <paramref name="at"/> = vị trí bắt đầu chữ độ bền.</summary>
        public static bool TryParseDurability(string text, out int current, out int max, out int at)
        {
            current = max = 0;
            at = -1;
            if (string.IsNullOrEmpty(text)) return false;
            var match = DurabilityPattern.Match(text);
            if (!match.Success) return false;
            current = int.Parse(match.Groups[1].Value);
            max = int.Parse(match.Groups[2].Value);
            at = match.Index;
            return max > 0;
        }

        public static Color DurabilityColor(float ratio) =>
            ratio <= 0f ? new Color(0.86f, 0.25f, 0.25f, 1f)
            : ratio <= 0.25f ? new Color(0.95f, 0.55f, 0.15f, 1f)
            : PopupPalette.PriceGreen;

        /// <param name="stoneCount">Số Đá mài; -1 = không rõ (không chặn nút).</param>
        public static BlacksmithRepairPopupView Create(Transform parent, Font font, MenuItemInfo item,
            RemoteAssetCache assets, int stoneCount, Action repair)
        {
            var root = new GameObject("RepairDetails", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = Vector2.zero;
            var view = root.AddComponent<BlacksmithRepairPopupView>();

            // Lớp tối phủ cả màn hình, chạm ra ngoài là đóng. Là ANH EM đứng trước khung chứ
            // không phải cha: làm cha thì chạm vào thân popup cũng nổi lên Button này và đóng.
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
            dim.transform.SetParent(root.transform, false);
            ((RectTransform)dim.transform).sizeDelta = new Vector2(4000f, 4000f);
            dim.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            dim.GetComponent<Button>().onClick.AddListener(view.Close);

            var frame = GamePopupFrame.Create(root.transform, font, "Sửa trang bị", Width, Height);
            frame.Closed += view.Close;
            view.Build(frame.Content, font, item, assets, stoneCount, repair);
            return view;
        }

        private void Close()
        {
            if (!_repairing && this != null) Destroy(gameObject);
        }

        private void Build(RectTransform content, Font font, MenuItemInfo item, RemoteAssetCache assets,
            int stoneCount, Action repair)
        {
            var hasDurability = TryParseDurability(item.Description, out var current, out _max, out var at);
            var icon = BuildIcon(content, item, assets);

            var title = MakeLabel(content, font, "Name", 16, item.Title ?? "Trang bị", 4f, 24f);
            UiBuilder.SetFontStyle(title, FontStyle.Bold);

            // Bỏ phần chữ độ bền khỏi mô tả vì đã có thanh riêng bên dưới.
            var description = item.Description ?? string.Empty;
            if (at >= 0) description = description.Substring(0, at);
            // Mô tả item của server mở đầu bằng tên món — đã có ở tiêu đề.
            if (!string.IsNullOrEmpty(item.Title) && description.StartsWith(item.Title))
                description = description.Substring(item.Title.Length);
            var desc = MakeLabel(content, font, "Desc", 12, description.Trim(), 30f, 50f);
            desc.color = PopupPalette.TextMuted;

            _durabilityLabel = MakeLabel(content, font, "Durability", 13, string.Empty, 86f, 18f);
            UiBuilder.SetFontStyle(_durabilityLabel, FontStyle.Bold);
            BuildBar(content);
            ShowDurability(hasDurability ? current : 0);

            string hint = null;
            if (hasDurability && current <= 0) hint = "Món này đã HỎNG — mất chỉ số riêng tới khi sửa.";
            if (stoneCount == 0) hint = "Bạn chưa có Đá mài sửa chữa.";
            if (hint != null)
            {
                var hintText = MakeLabel(content, font, "Hint", 12, hint, 128f, 18f);
                hintText.color = new Color(0.82f, 0.22f, 0.22f, 1f);
            }

            var needsRepair = item.CanSelect && (!hasDurability || current < _max);
            var label = needsRepair ? "Sửa chữa" : "Còn nguyên";
            _repairButton = BuildRepairButton(content, font, label, needsRepair && stoneCount != 0);
            _repairButton.onClick.AddListener(() => StartRepair(icon, assets, current, repair));
        }

        private void StartRepair(RectTransform icon, RemoteAssetCache assets, int from, Action repair)
        {
            if (_repairing) return;
            _repairing = true;
            _repairButton.interactable = false;

            var fx = icon.gameObject.AddComponent<GrindstoneRepairEffect>();
            fx.Play(assets,
                t => ShowDurability(Mathf.RoundToInt(Mathf.Lerp(from, _max, t))),
                () =>
                {
                    repair?.Invoke();
                    _repairing = false;
                    Close();
                });
        }

        private void ShowDurability(int value)
        {
            var ratio = _max > 0 ? Mathf.Clamp01(value / (float)_max) : 0f;
            _durabilityLabel.text = _max > 0 ? $"Độ bền: {value}/{_max}" : "Độ bền: --";
            _durabilityLabel.color = ratio <= 0.25f ? DurabilityColor(ratio) : PopupPalette.TextDark;
            _barFill.rectTransform.anchorMax = new Vector2(ratio, 1f);
            _barFill.color = DurabilityColor(ratio);
        }

        private static RectTransform BuildIcon(RectTransform content, MenuItemInfo item, RemoteAssetCache assets)
        {
            var box = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(content, false);
            var rect = (RectTransform)box.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);
            rect.anchoredPosition = new Vector2(IconSize * 0.5f + 4f, -IconSize * 0.5f - 6f);
            RoundedBorder.Apply(box, 8f, PopupPalette.ListBg, PopupPalette.Hairline);

            var image = new GameObject("Image", typeof(RectTransform), typeof(RawImage));
            image.transform.SetParent(box.transform, false);
            var imageRect = (RectTransform)image.transform;
            UiBuilder.Stretch(imageRect);
            imageRect.offsetMin = new Vector2(8f, 8f);
            imageRect.offsetMax = new Vector2(-8f, -8f);
            var raw = image.GetComponent<RawImage>();
            raw.raycastTarget = false;
            if (assets != null && !string.IsNullOrEmpty(item.ImagePath))
                assets.Get(item.ImagePath, ImagePackets.TypeIcon, texture => { if (raw != null) raw.texture = texture; });
            return rect;
        }

        private static Text MakeLabel(RectTransform content, Font font, string name, int size,
            string text, float top, float height)
        {
            var label = UiBuilder.MakeText(content, font, name, size, false);
            label.text = text;
            label.color = PopupPalette.TextDark;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(TextLeft, -top - height);
            rect.offsetMax = new Vector2(-8f, -top);
            return label;
        }

        private void BuildBar(RectTransform content)
        {
            var track = new GameObject("DurabilityBar", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(content, false);
            var rect = (RectTransform)track.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(TextLeft, -118f);
            rect.offsetMax = new Vector2(-8f, -106f);
            var trackImage = track.GetComponent<Image>();
            RoundedUiSprite.Apply(trackImage, 5f);
            trackImage.color = new Color(0.85f, 0.89f, 0.94f, 1f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(track.transform, false);
            var fillRect = (RectTransform)fill.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            _barFill = fill.GetComponent<Image>();
            RoundedUiSprite.Apply(_barFill, 5f);
        }

        private static Button BuildRepairButton(RectTransform content, Font font, string text, bool enabled)
        {
            var go = new GameObject("RepairButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(content, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            rect.anchoredPosition = new Vector2(0f, 2f);

            var image = go.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = UiBuilder.ButtonFace;
            var label = UiBuilder.MakeText(go.transform, font, "Label", 15, true);
            label.text = text;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;

            var button = go.GetComponent<Button>();
            button.interactable = enabled;
            // Nút tắt: làm mờ cả khung lẫn chữ để người chơi thấy rõ là không bấm được.
            if (!enabled) go.AddComponent<CanvasGroup>().alpha = 0.5f;
            return button;
        }
    }
}
