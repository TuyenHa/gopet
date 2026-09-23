using Gopet.Net.Images;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Trang "Điểm danh": panel trái (tiêu đề, đếm ngược hết tháng, chuỗi liên tục,
    /// rương, nút chính) và lưới 6 cột. Tách khỏi file chính để mỗi file dưới 200 dòng.
    /// </summary>
    public sealed partial class DailyCheckinView
    {
        private void BuildSidePanel(RectTransform panel)
        {
            var go = new GameObject("Side", typeof(RectTransform));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            // Trang đã trừ sẵn khay tab, nên panel trái chạy hết chiều cao của nó.
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(SidePanelWidth, 0f);

            // Tiêu đề "Điểm Danh Tháng"
            var title = UiBuilder.MakeText(go.transform, _font, "MonthTitle", 13, false);
            title.text = "Điểm Danh Tháng";
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.color = TitleBlue;
            title.alignment = TextAnchor.UpperCenter;
            var tr = (RectTransform)title.transform;
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -2f);
            tr.sizeDelta = new Vector2(0f, 16f);

            _countdownText = UiBuilder.MakeText(go.transform, _font, "Countdown", 9, false);
            _countdownText.color = CountdownRed;
            _countdownText.alignment = TextAnchor.UpperCenter;
            var cr = (RectTransform)_countdownText.transform;
            cr.anchorMin = new Vector2(0f, 1f); cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.anchoredPosition = new Vector2(0f, -19f);
            cr.sizeDelta = new Vector2(0f, 12f);

            _streakText = UiBuilder.MakeText(go.transform, _font, "Streak", 9, false);
            _streakText.color = SubText;
            _streakText.alignment = TextAnchor.UpperCenter;
            var sr = (RectTransform)_streakText.transform;
            sr.anchorMin = new Vector2(0f, 1f); sr.anchorMax = new Vector2(1f, 1f);
            sr.pivot = new Vector2(0.5f, 1f);
            sr.anchoredPosition = new Vector2(0f, -33f);
            sr.sizeDelta = new Vector2(0f, 12f);

            var chestGo = new GameObject("Chest", typeof(RectTransform), typeof(RawImage));
            chestGo.transform.SetParent(go.transform, false);
            var chestRect = (RectTransform)chestGo.transform;
            chestRect.anchorMin = new Vector2(0.5f, 0.5f);
            chestRect.anchorMax = new Vector2(0.5f, 0.5f);
            chestRect.pivot = new Vector2(0.5f, 0.5f);
            chestRect.sizeDelta = new Vector2(96f, 96f);
            chestRect.anchoredPosition = new Vector2(0f, 2f);
            _chestImage = chestGo.GetComponent<RawImage>();
            _chestImage.color = new Color(1f, 1f, 1f, 0f);
            if (_assets != null)
            {
                _assets.Get(ChestIconPath, ImagePackets.TypeIcon, _chestImage, tex =>
                {
                    if (_chestImage == null) return;
                    _chestImage.texture = tex;
                    _chestImage.color = Color.white;
                });
            }

            var btnGo = new GameObject("MainBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(go.transform, false);
            var brect = (RectTransform)btnGo.transform;
            brect.anchorMin = new Vector2(0.5f, 0f); brect.anchorMax = new Vector2(0.5f, 0f);
            brect.pivot = new Vector2(0.5f, 0f);
            brect.sizeDelta = new Vector2(120f, 26f);
            brect.anchoredPosition = new Vector2(0f, 2f);
            var bimg = btnGo.GetComponent<Image>();
            RoundedUiSprite.Apply(bimg);
            bimg.color = BtnDisabled;
            _mainButtonLabel = UiBuilder.MakeText(btnGo.transform, _font, "Label", 11, true);
            _mainButtonLabel.text = "Đã điểm danh";
            _mainButtonLabel.alignment = TextAnchor.MiddleCenter;
            _mainButtonLabel.color = Color.white;
            _mainButton = btnGo.GetComponent<Button>();
            _mainButton.onClick.AddListener(() => _guider?.DoDailyCheckin());
        }

        private void BuildGrid(RectTransform panel)
        {
            var go = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            go.transform.SetParent(panel, false);
            _grid = (RectTransform)go.transform;
            _grid.anchorMin = new Vector2(0f, 0f);
            _grid.anchorMax = new Vector2(1f, 1f);
            _grid.offsetMin = new Vector2(SidePanelWidth + 10f, 0f);
            _grid.offsetMax = Vector2.zero;

            var layout = go.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(CellSize, CellHeight);
            layout.spacing = new Vector2(CellSpacing, CellSpacing);
            layout.padding = new RectOffset(2, 2, 2, 2);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 6;
            // Cell cố định nên phần dư chia đều hai bên, grid không dạt về góc trái.
            layout.childAlignment = TextAnchor.MiddleCenter;
        }

        /// <summary>Dựng lại grid + panel trái từ trạng thái server.</summary>
    }
}
