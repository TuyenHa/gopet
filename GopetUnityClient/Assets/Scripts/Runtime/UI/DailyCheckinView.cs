using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup điểm danh theo tháng — layout 2 cột: <b>panel trái</b> (tiêu đề + countdown
    /// hết tháng + chuỗi liên tục + hình rương + nút chính) và <b>grid 6 cột</b> hiển thị
    /// 28-31 ngày. Mỗi ô 3 trạng thái: Đã nhận (viền xanh + ✓), Hôm nay (viền vàng dày +
    /// nhấn), Chưa tới/Đã lỡ (trắng + icon quà). Style bám ShopPopupView + reference mock.
    /// </summary>
    public sealed class DailyCheckinView : MonoBehaviour
    {
        // ==== Bảng màu (bám mock reference) ====
        private static readonly Color PanelBg = new Color(0.97f, 0.99f, 1f, 1f);
        private static readonly Color PanelBorder = new Color(0.28f, 0.6f, 1f, 1f);
        private static readonly Color HeaderColor = new Color(1f, 0.78f, 0.20f, 1f);
        private static readonly Color HeaderText = new Color(0.14f, 0.24f, 0.44f, 1f);
        private static readonly Color TitleBlue = new Color(0.17f, 0.34f, 0.60f, 1f);
        private static readonly Color CountdownRed = new Color(0.90f, 0.30f, 0.20f, 1f);
        private static readonly Color SubText = new Color(0.36f, 0.42f, 0.52f, 1f);

        private static readonly Color CellReceivedBg = new Color(0.90f, 0.97f, 0.92f, 1f);
        private static readonly Color CellReceivedBorder = new Color(0.32f, 0.72f, 0.42f, 1f);
        private static readonly Color CellTodayBg = new Color(1f, 0.96f, 0.78f, 1f);
        private static readonly Color CellTodayBorder = new Color(1f, 0.76f, 0.16f, 1f);
        private static readonly Color CellLockedBg = new Color(1f, 1f, 1f, 1f);
        private static readonly Color CellLockedBorder = new Color(0.85f, 0.88f, 0.93f, 1f);
        private static readonly Color CellMissedBg = new Color(0.94f, 0.94f, 0.95f, 1f);
        private static readonly Color CellMissedBorder = new Color(0.78f, 0.80f, 0.84f, 1f);
        private static readonly Color CheckGreen = new Color(0.20f, 0.66f, 0.34f, 1f);

        private static readonly Color BtnActive = new Color(0.22f, 0.34f, 0.52f, 1f);
        private static readonly Color BtnDisabled = new Color(0.70f, 0.74f, 0.80f, 1f);

        // ==== Kích thước popup (canvas ref 720×1280, chiều cao khả kiến ~430) ====
        // Nhỏ lại theo yêu cầu user: 660×400 → 560×340.
        private const float PanelWidth = 560f;
        private const float PanelHeight = 340f;
        private const float HeaderHeight = 28f;
        private const float CloseSize = 30f;
        private const float SidePanelWidth = 168f;

        // Rương xanh + kim cương, gen bằng tools/image-gen, deploy vào assets/icons/.
        private const string ChestIconPath = "icons/checkin-chest.png";

        // Grid: cell nhỏ vừa đủ hiện day# + icon + text; 6 cột như mock. Shrink lần 2.
        private const float CellSize = 44f;
        private const float CellHeight = 46f;
        private const float CellSpacing = 8f;
        private const float IconSize = 18f;

        private Font _font;
        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private RectTransform _grid;
        private Text _countdownText;
        private Text _streakText;
        private Button _mainButton;
        private Text _mainButtonLabel;
        private RawImage _chestImage;

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
            view.BuildSidePanel(pr);
            view.BuildGrid(pr);
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
            var label = UiBuilder.MakeText(go.transform, _font, "Title", 15, true);
            label.text = "Điểm danh";
            label.alignment = TextAnchor.MiddleCenter;
            label.color = HeaderText;
        }

        /// <summary>Style khớp ShopPopupView: sprite HudSkin.Close chờm góc trên-phải.</summary>
        private void BuildCloseButton(RectTransform panel)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(CloseSize, CloseSize);
            rect.anchoredPosition = new Vector2(4f, 4f);
            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null) { img.sprite = sprite; img.color = Color.white; }
            else
            {
                img.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var x = UiBuilder.MakeText(go.transform, _font, "X", 18, true);
                x.alignment = TextAnchor.MiddleCenter; x.text = "×"; x.color = Color.white;
                x.fontStyle = FontStyle.Bold;
            }
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

        private void BuildSidePanel(RectTransform panel)
        {
            var go = new GameObject("Side", typeof(RectTransform));
            go.transform.SetParent(panel, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(10f, 10f);
            // 10px hở giữa tab header và nội dung: HeaderTopMargin(8) + HeaderHeight + Gap(10).
            rect.offsetMax = new Vector2(SidePanelWidth, -(HeaderHeight + 18f));

            // Tiêu đề "Điểm Danh Tháng"
            var title = UiBuilder.MakeText(go.transform, _font, "MonthTitle", 14, false);
            title.text = "Điểm Danh Tháng";
            title.fontStyle = FontStyle.Bold;
            title.color = TitleBlue;
            title.alignment = TextAnchor.UpperCenter;
            var tr = (RectTransform)title.transform;
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -2f);
            tr.sizeDelta = new Vector2(0f, 18f);

            _countdownText = UiBuilder.MakeText(go.transform, _font, "Countdown", 10, false);
            _countdownText.color = CountdownRed;
            _countdownText.alignment = TextAnchor.UpperCenter;
            var cr = (RectTransform)_countdownText.transform;
            cr.anchorMin = new Vector2(0f, 1f); cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.anchoredPosition = new Vector2(0f, -22f);
            cr.sizeDelta = new Vector2(0f, 14f);

            _streakText = UiBuilder.MakeText(go.transform, _font, "Streak", 10, false);
            _streakText.color = SubText;
            _streakText.alignment = TextAnchor.UpperCenter;
            var sr = (RectTransform)_streakText.transform;
            sr.anchorMin = new Vector2(0f, 1f); sr.anchorMax = new Vector2(1f, 1f);
            sr.pivot = new Vector2(0.5f, 1f);
            sr.anchoredPosition = new Vector2(0f, -38f);
            sr.sizeDelta = new Vector2(0f, 14f);

            var chestGo = new GameObject("Chest", typeof(RectTransform), typeof(RawImage));
            chestGo.transform.SetParent(go.transform, false);
            var chestRect = (RectTransform)chestGo.transform;
            chestRect.anchorMin = new Vector2(0.5f, 0.5f);
            chestRect.anchorMax = new Vector2(0.5f, 0.5f);
            chestRect.pivot = new Vector2(0.5f, 0.5f);
            chestRect.sizeDelta = new Vector2(130f, 130f);
            chestRect.anchoredPosition = new Vector2(0f, 4f);
            _chestImage = chestGo.GetComponent<RawImage>();
            _chestImage.color = new Color(1f, 1f, 1f, 0f);
            if (_assets != null)
            {
                _assets.Get(ChestIconPath, ImagePackets.TypeIcon, tex =>
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
            brect.sizeDelta = new Vector2(150f, 34f);
            brect.anchoredPosition = new Vector2(0f, 4f);
            var bimg = btnGo.GetComponent<Image>();
            RoundedUiSprite.Apply(bimg);
            bimg.color = BtnDisabled;
            _mainButtonLabel = UiBuilder.MakeText(btnGo.transform, _font, "Label", 12, true);
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
            _grid.offsetMin = new Vector2(SidePanelWidth + 12f, 8f);
            // 10px hở giữa tab header và grid — cùng công thức panel trái.
            _grid.offsetMax = new Vector2(-8f, -(HeaderHeight + 18f));

            var layout = go.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(CellSize, CellHeight);
            layout.spacing = new Vector2(CellSpacing, CellSpacing);
            layout.padding = new RectOffset(2, 2, 2, 2);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 6;
        }

        /// <summary>Dựng lại grid + panel trái từ trạng thái server.</summary>
        public void Bind(DailyCheckinState state)
        {
            if (state == null) return;
            for (int i = _grid.childCount - 1; i >= 0; i--)
            {
                Destroy(_grid.GetChild(i).gameObject);
            }
            foreach (var day in state.Days) BuildCell(day);

            _countdownText.text = FormatMonthlyRemaining(DateTime.Now);
            _streakText.text = $"Chuỗi liên tục: {state.Streak} ngày";
            bool can = state.CanCheckinToday;
            _mainButton.interactable = can;
            _mainButton.image.color = can ? BtnActive : BtnDisabled;
            _mainButtonLabel.text = can ? "Điểm danh" : "Đã điểm danh";
        }

        private void BuildCell(DailyCheckinState.DayInfo day)
        {
            var cell = new GameObject($"Day{day.Day}", typeof(RectTransform), typeof(Image));
            cell.transform.SetParent(_grid, false);
            var bg = cell.GetComponent<Image>();
            RoundedUiSprite.Apply(bg);
            var colors = CellColors(day.State);
            bg.color = colors.bg;
            var outline = cell.AddComponent<Outline>();
            outline.effectColor = colors.border;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var num = UiBuilder.MakeText(cell.transform, _font, "Num", 10, false);
            num.text = day.Day.ToString();
            num.alignment = TextAnchor.UpperCenter;
            num.fontStyle = FontStyle.Bold;
            num.color = NumColor(day.State);
            var nr = (RectTransform)num.transform;
            nr.anchorMin = new Vector2(0f, 1f); nr.anchorMax = new Vector2(1f, 1f);
            nr.pivot = new Vector2(0.5f, 1f);
            nr.offsetMin = new Vector2(0f, -14f); nr.offsetMax = new Vector2(0f, -1f);

            if (day.State == DailyCheckinState.Received) BuildCheckMark(cell.transform);
            else BuildItemIcon(cell.transform, day);

            var badge = UiBuilder.MakeText(cell.transform, _font, "Badge", 8, false);
            badge.text = StateBadge(day.State);
            badge.alignment = TextAnchor.LowerCenter;
            badge.color = BadgeColor(day.State);
            var br = (RectTransform)badge.transform;
            br.anchorMin = new Vector2(0f, 0f); br.anchorMax = new Vector2(1f, 0f);
            br.pivot = new Vector2(0.5f, 0f);
            br.offsetMin = new Vector2(1f, 1f); br.offsetMax = new Vector2(-1f, 12f);

            if (day.State == DailyCheckinState.Claimable)
            {
                var btn = cell.AddComponent<Button>();
                btn.targetGraphic = bg;
                btn.onClick.AddListener(() => _guider?.DoDailyCheckin());
            }
        }

        /// <summary>Dấu ✓ to xanh cho ngày đã nhận — thay cho icon quà.</summary>
        private void BuildCheckMark(Transform parent)
        {
            var t = UiBuilder.MakeText(parent, _font, "Check", 20, false);
            t.text = "✓";
            t.alignment = TextAnchor.MiddleCenter;
            t.color = CheckGreen;
            t.fontStyle = FontStyle.Bold;
            var r = (RectTransform)t.transform;
            r.anchorMin = new Vector2(0f, 0f); r.anchorMax = new Vector2(1f, 1f);
            r.offsetMin = new Vector2(2f, 12f); r.offsetMax = new Vector2(-2f, -12f);
        }

        private void BuildItemIcon(Transform parent, DailyCheckinState.DayInfo day)
        {
            if (string.IsNullOrEmpty(day.IconPath) || _assets == null) return;
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            iconGo.transform.SetParent(parent, false);
            var iconRect = (RectTransform)iconGo.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.55f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
            var raw = iconGo.GetComponent<RawImage>();
            raw.color = new Color(1f, 1f, 1f, 0f);
            _assets.Get(day.IconPath, ImagePackets.TypeIcon, tex =>
            {
                if (raw == null) return;
                raw.texture = tex; raw.color = Color.white;
            });
        }

        // ==== Helpers ====

        /// <summary>"Còn X ngày Y giờ" tới cuối tháng (reset điểm danh).</summary>
        internal static string FormatMonthlyRemaining(DateTime now)
        {
            var end = new DateTime(now.Year, now.Month,
                DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);
            var diff = end - now;
            if (diff.TotalSeconds < 0) diff = TimeSpan.Zero;
            return $"⏰ Còn {diff.Days} ngày {diff.Hours} giờ";
        }

        private static (Color bg, Color border) CellColors(byte state) => state switch
        {
            DailyCheckinState.Received => (CellReceivedBg, CellReceivedBorder),
            DailyCheckinState.Claimable => (CellTodayBg, CellTodayBorder),
            DailyCheckinState.Missed => (CellMissedBg, CellMissedBorder),
            _ => (CellLockedBg, CellLockedBorder),
        };

        private static Color NumColor(byte state) => state switch
        {
            DailyCheckinState.Received => CheckGreen,
            DailyCheckinState.Claimable => new Color(0.85f, 0.55f, 0.10f, 1f),
            DailyCheckinState.Missed => SubText,
            _ => SubText,
        };

        private static Color BadgeColor(byte state) => state switch
        {
            DailyCheckinState.Received => CheckGreen,
            DailyCheckinState.Claimable => new Color(0.85f, 0.55f, 0.10f, 1f),
            _ => SubText,
        };

        private static string StateBadge(byte state) => state switch
        {
            DailyCheckinState.Received => "Đã nhận",
            DailyCheckinState.Claimable => "Điểm danh",
            DailyCheckinState.Missed => "Đã lỡ",
            _ => "Điểm danh",
        };
    }
}
