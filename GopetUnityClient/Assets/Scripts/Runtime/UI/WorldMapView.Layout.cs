using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Khung màn bản đồ thế giới: tranh nền phủ kín, lớp mây, lớp pin, tiêu đề và nút
    /// đóng. Tách khỏi phần trạng thái để mỗi file dưới 200 dòng (verify.ps1 bước 10/10).
    /// </summary>
    public sealed partial class WorldMapView
    {
        /// <summary>Tranh nền là asset của dự án (sinh bằng tools/image-gen), không phải thứ giải từ jar.</summary>
        public const string BackgroundResource = "Ui/WorldMap/world-map-background";

        private void Build()
        {
            BuildPicture();
            BuildClouds();
            BuildNodeLayer();

            BuildCloseButton();
        }

        /// <summary>
        /// Tranh giữ tỉ lệ gốc và PHỦ KÍN màn — thừa bao nhiêu thì cắt. Kích thước thật
        /// chốt ở <see cref="LayoutPicture"/> vì lúc dựng RectTransform chưa qua layout.
        /// </summary>
        private void BuildPicture()
        {
            var go = new GameObject("Picture", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            _picture = (RectTransform)go.transform;
            // Ghim MÉP TRÊN: phần bị cắt phải rơi xuống đáy tranh (biển) chứ không phải
            // đỉnh tranh, nơi có cụm đảo Thượng giới.
            _picture.anchorMin = _picture.anchorMax = _picture.pivot = new Vector2(0.5f, 1f);
            _picture.anchoredPosition = Vector2.zero;

            var picture = Resources.Load<Sprite>(BackgroundResource);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.sprite = picture;
            image.color = picture == null ? UiBuilder.JarBackground : Color.white;
            if (picture == null)
                Debug.LogWarning($"[Gopet] Thiếu tranh nền bản đồ tại Resources/{BackgroundResource} — dùng nền màu.");
            _pictureSource = picture == null || picture.rect.height <= 0f
                ? Vector2.one
                : picture.rect.size;
        }

        /// <summary>
        /// Phóng tranh cho phủ kín màn. Để tranh vừa khít trong màn (letterbox) thì hai
        /// dải nền lót hai bên khiến màn bản đồ trông như một popup dán giữa cảnh chơi —
        /// mà đây là một MÀN RIÊNG. Phần tràn ra ngoài bị cắt; nhãn rơi vào vùng cắt
        /// được <see cref="Declutter"/> kéo trở lại trong tầm nhìn.
        /// </summary>
        private void LayoutPicture()
        {
            if (_picture == null) return;
            var screen = ((RectTransform)transform).rect.size;
            if (screen.x <= 0f || screen.y <= 0f || screen == _lastScreen) return;
            _lastScreen = screen;
            var scale = Mathf.Max(screen.x / _pictureSource.x, screen.y / _pictureSource.y);
            _picture.sizeDelta = _pictureSource * scale;
            // Khung tranh đổi cỡ thì khoảng cách pixel giữa các nhãn cũng đổi.
            _needsDeclutter = true;
        }

        /// <summary>
        /// Lớp pin là con của tranh (neo theo tỉ lệ tranh) và là con CUỐI nên nằm trên
        /// lớp mây — mây trôi ngang qua tên map thì không đọc được.
        /// </summary>
        private void BuildNodeLayer()
        {
            var go = new GameObject("Nodes", typeof(RectTransform));
            go.transform.SetParent(_picture, false);
            _nodeLayer = (RectTransform)go.transform;
            UiBuilder.Stretch(_nodeLayer);
        }

        private void BuildCloseButton()
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(CloseSize, CloseSize);
            rect.anchoredPosition = new Vector2(-10f, -10f);

            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var label = UiBuilder.MakeText(go.transform, _font, "Label", 22, true);
                label.alignment = TextAnchor.MiddleCenter;
                label.text = "×";
                label.color = Color.white;
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
            }
            go.GetComponent<Button>().onClick.AddListener(() => CloseRequested?.Invoke());
        }

        private void MakeRegionLabel(int regionId)
        {
            var label = UiBuilder.MakeText(_nodeLayer, _font, $"Region{regionId}", 15, false);
            label.text = WorldMapLayout.RegionNames[regionId];
            label.color = RegionText;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            AddShadow(label.gameObject);

            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(
                WorldMapLayout.RegionLabelX(regionId), WorldMapLayout.RegionLabelY(regionId));
            rect.pivot = new Vector2(0.5f, 0.5f);
            // Ôm vừa chữ: nhãn cụm cố định bề ngang sẽ chiếm chỗ thừa, đẩy oan nhãn map
            // bên cạnh khi giải toạ chồng lấn.
            rect.sizeDelta = new Vector2(
                Mathf.Min(LabelMaxWidth, label.preferredWidth + 8f), LabelHeight);
            rect.anchoredPosition = Vector2.zero;
            RegisterPlaced(rect);
        }

        /// <summary>Chữ trắng trên tranh nhiều màu rất khó đọc nếu không có viền tối.</summary>
        private static void AddShadow(GameObject target)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.4f, -1.4f);
        }
    }
}
