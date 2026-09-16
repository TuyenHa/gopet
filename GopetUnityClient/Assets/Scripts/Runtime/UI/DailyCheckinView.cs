using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup điểm danh theo tháng: lưới ngày (đã nhận / hôm nay / đã lỡ / chưa tới)
    /// + nút "Điểm danh". Lấy style từ <see cref="ShopPopupView"/> (panel bo góc trắng,
    /// viền xanh, nút X góc). Server dựng nhãn quà nên view chỉ hiển thị.
    /// </summary>
    public sealed class DailyCheckinView : MonoBehaviour
    {
        // Bám bảng màu ShopPopupView.
        private static readonly Color PanelBg = new Color(0.96f, 0.98f, 1f, 1f);
        private static readonly Color PanelBorder = new Color(0.28f, 0.6f, 1f, 1f);
        private static readonly Color HeaderColor = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color HeaderText = new Color(0.14f, 0.24f, 0.44f, 1f);

        // Màu ô theo trạng thái.
        private static readonly Color CellReceived = new Color(0.80f, 0.86f, 0.80f, 1f);
        private static readonly Color CellClaimable = new Color(1f, 0.92f, 0.55f, 1f);
        private static readonly Color CellMissed = new Color(0.72f, 0.73f, 0.76f, 1f);
        private static readonly Color CellLocked = new Color(0.88f, 0.90f, 0.94f, 1f);

        private const float PanelWidth = 560f;
        private const float PanelHeight = 400f;
        private const float HeaderHeight = 36f;
        private const float ButtonHeight = 44f;
        private const float CloseSize = 32f;

        private Font _font;
        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private RectTransform _grid;
        private Button _checkinButton;
        private Text _checkinLabel;

        public event Action Closed;

        public static DailyCheckinView Create(Transform parent, Font font,
            GuiderHandler guider, RemoteAssetCache assets)
        {
            var backdrop = new GameObject("DailyCheckinView", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            var back = backdrop.GetComponent<Image>();
            back.color = new Color(0f, 0f, 0f, 0.42f);
            back.raycastTarget = true;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var pr = (RectTransform)panel.transform;
            pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
            pr.pivot = new Vector2(0.5f, 0.5f);
            pr.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            var pimg = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(pimg);
            pimg.color = PanelBg;
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = PanelBorder;
            outline.effectDistance = new Vector2(2f, 2f);

            var view = backdrop.AddComponent<DailyCheckinView>();
            view._font = font;
            view._guider = guider;
            view._assets = assets;
            view.BuildHeader(pr);
            view.BuildCloseButton(pr);
            view.BuildGrid(pr);
            view.BuildCheckinButton(pr);
            return view;
        }

        private void BuildHeader(RectTransform panel)
        {
            var go = new GameObject("Header", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(panel, false);
            UiBuilder.PlaceRow((RectTransform)go.transform, 8f, HeaderHeight, 8f);
            var img = go.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = HeaderColor;
            var label = UiBuilder.MakeText(go.transform, _font, "Title", 16, true);
            label.text = "Điểm danh";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = HeaderText;
        }

        private void BuildGrid(RectTransform panel)
        {
            var go = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            go.transform.SetParent(panel, false);
            _grid = (RectTransform)go.transform;
            _grid.anchorMin = new Vector2(0f, 0f);
            _grid.anchorMax = new Vector2(1f, 1f);
            // chừa header trên và nút dưới
            _grid.offsetMin = new Vector2(10f, 8f + ButtonHeight + 8f);
            _grid.offsetMax = new Vector2(-10f, -(8f + HeaderHeight + 6f));

            var layout = go.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(72f, 58f);
            layout.spacing = new Vector2(4f, 4f);
            layout.padding = new RectOffset(2, 2, 2, 2);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 7;
        }

        private void BuildCheckinButton(RectTransform panel)
        {
            var go = new GameObject("CheckinButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(200f, ButtonHeight);
            rect.anchoredPosition = new Vector2(0f, 8f);
            var img = go.GetComponent<Image>();
            RoundedUiSprite.Apply(img);
            img.color = new Color(0.22f, 0.34f, 0.52f, 1f);
            _checkinLabel = UiBuilder.MakeText(go.transform, _font, "Label", 16, true);
            _checkinLabel.text = "Điểm danh";
            _checkinLabel.alignment = TextAnchor.MiddleCenter;
            _checkinLabel.color = Color.white;
            _checkinButton = go.GetComponent<Button>();
            _checkinButton.onClick.AddListener(() => _guider?.DoDailyCheckin());
        }

        private void BuildCloseButton(RectTransform panel)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-6f, -6f);
            rect.sizeDelta = new Vector2(CloseSize, CloseSize);
            go.GetComponent<Image>().color = new Color(0.9f, 0.12f, 0.1f, 1f);
            var x = UiBuilder.MakeText(go.transform, _font, "X", 18, true);
            x.text = "×";
            x.alignment = TextAnchor.MiddleCenter;
            x.color = Color.white;
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

        /// <summary>Dựng lại lưới từ trạng thái server gửi.</summary>
        public void Bind(DailyCheckinState state)
        {
            if (state == null) return;
            for (int i = _grid.childCount - 1; i >= 0; i--)
            {
                Destroy(_grid.GetChild(i).gameObject);
            }
            foreach (var day in state.Days)
            {
                BuildCell(day);
            }
            bool can = state.CanCheckinToday;
            _checkinButton.interactable = can;
            _checkinLabel.text = can ? "Điểm danh" : "Đã điểm danh hôm nay";
        }

        private void BuildCell(DailyCheckinState.DayInfo day)
        {
            var cell = new GameObject($"Day{day.Day}", typeof(RectTransform), typeof(Image));
            cell.transform.SetParent(_grid, false);
            var bg = cell.GetComponent<Image>();
            RoundedUiSprite.Apply(bg);
            bg.color = StateColor(day.State);

            var num = UiBuilder.MakeText(cell.transform, _font, "Num", 10, false);
            num.text = day.Day.ToString();
            num.alignment = TextAnchor.UpperLeft;
            var numRect = (RectTransform)num.transform;
            numRect.anchorMin = new Vector2(0f, 1f);
            numRect.anchorMax = new Vector2(1f, 1f);
            numRect.pivot = new Vector2(0f, 1f);
            numRect.offsetMin = new Vector2(4f, -14f);
            numRect.offsetMax = new Vector2(-4f, -2f);

            if (day.IconItemId > 0 && _assets != null)
            {
                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
                iconGo.transform.SetParent(cell.transform, false);
                var iconRect = (RectTransform)iconGo.transform;
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.6f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.sizeDelta = new Vector2(28f, 28f);
                var raw = iconGo.GetComponent<RawImage>();
                var path = $"items/{day.IconItemId}.png";
                _assets.Get(path, ImagePackets.TypeIcon, tex =>
                {
                    if (raw != null) raw.texture = tex;
                });
            }

            var badge = UiBuilder.MakeText(cell.transform, _font, "Badge", 12, false);
            badge.text = StateBadge(day.State);
            badge.alignment = TextAnchor.LowerCenter;
            var badgeRect = (RectTransform)badge.transform;
            badgeRect.anchorMin = new Vector2(0f, 0f);
            badgeRect.anchorMax = new Vector2(1f, 0f);
            badgeRect.pivot = new Vector2(0.5f, 0f);
            badgeRect.offsetMin = new Vector2(2f, 2f);
            badgeRect.offsetMax = new Vector2(-2f, 16f);
            badge.color = HeaderText;
        }

        private static Color StateColor(byte state) => state switch
        {
            DailyCheckinState.Received => CellReceived,
            DailyCheckinState.Claimable => CellClaimable,
            DailyCheckinState.Missed => CellMissed,
            _ => CellLocked,
        };

        private static string StateBadge(byte state) => state switch
        {
            DailyCheckinState.Received => "Đã nhận",
            DailyCheckinState.Claimable => "Hôm nay",
            DailyCheckinState.Missed => "Đã lỡ",
            _ => "",
        };
    }
}
