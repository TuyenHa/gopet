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
    public sealed partial class DailyCheckinView : MonoBehaviour
    {
        // ==== Bảng màu (bám mock reference) ====
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

        // ==== Kích thước popup (canvas ref rộng 720, chiều cao khả kiến ~405) ====
        // Nhỏ lại theo yêu cầu user: 660×400 → 560×340 → 408×296.
        // Chiều cao tính vừa 6 hàng grid để tháng 31 ngày không bị cắt hàng cuối.
        //
        // 414×302 chọn để GamePopupFrame.Content ra ĐÚNG 392×248 — bằng vùng trong của
        // panel 408×296 cũ sau khi trừ lề và thanh tiêu đề. Nhờ vậy grid 6 cột và panel
        // trái giữ nguyên kích thước đã căn, không phải căn lại từ đầu.
        private const float PanelWidth = 414f;
        private const float PanelHeight = 302f;
        private const float SidePanelWidth = 132f;

        // Rương xanh + kim cương, gen bằng tools/image-gen, deploy vào assets/icons/.
        private const string ChestIconPath = "icons/checkin-chest.png";

        // Grid: cell nhỏ vừa đủ hiện day# + icon + text; 6 cột như mock. Shrink lần 3.
        private const float CellSize = 36f;
        private const float CellHeight = 36f;
        private const float CellSpacing = 5f;
        private const float IconSize = 14f;

        /// <summary>Băng chân đổi theo tab đang xem.</summary>
        private static readonly string[] FooterByTab =
        {
            "Điểm danh mỗi ngày để nhận quà",
            "Có GiftCode là có quà!",
        };

        private Font _font;
        private GamePopupFrame _frame;
        private PopupTabRail _rail;
        private RectTransform _checkinPage;
        private PopupInputForm _giftForm;
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
            var frame = GamePopupFrame.Create(parent, font, "Sự kiện", PanelWidth, PanelHeight,
                footer: FooterByTab[0]);
            frame.gameObject.name = "DailyCheckinView";

            var view = frame.gameObject.AddComponent<DailyCheckinView>();
            view._frame = frame;
            view._font = font;
            view._guider = guider;
            view._assets = assets;
            frame.Closed += () => view.Closed?.Invoke();

            view._rail = PopupTabRail.Create(frame.Content, font, frame.ContentWidth,
                new[] { "Điểm danh", "Quà tặng" });
            view._rail.Selected += view.ShowPage;

            view.BuildCheckinPage(frame.Content);
            view.BuildGiftPage(frame.Content);
            view._rail.Select(0);
            return view;
        }

        /// <summary>Trang nội dung nằm dưới khay tab, chiếm hết chỗ còn lại.</summary>
        private static RectTransform MakePage(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
            return rect;
        }

        private void BuildCheckinPage(RectTransform content)
        {
            _checkinPage = MakePage(content, "Page_Checkin");
            BuildSidePanel(_checkinPage);
            BuildGrid(_checkinPage);
        }

        private void BuildGiftPage(RectTransform content)
        {
            var page = MakePage(content, "Page_Gift");
            _giftForm = PopupInputForm.Create(page, _font, "Nhập mã quà tặng", "Giftcode:");
            _giftForm.Submitted += code =>
                _guider.SubmitInput(PopupInputForm.GiftCodeDialogId, new[] { code });
            // Huỷ = quay lại tab điểm danh, popup không đóng.
            _giftForm.Cancelled += () => _rail.Select(0);
        }

        private void ShowPage(int index)
        {
            _checkinPage.gameObject.SetActive(index == 0);
            _giftForm.transform.parent.gameObject.SetActive(index == 1);
            _frame.SetFooter(FooterByTab[index]);
            if (index == 1) _giftForm.Reset();
        }

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
            SetMainButtonState(state.CanCheckinToday);
        }

        private void SetMainButtonState(bool can)
        {
            _mainButton.interactable = can;
            _mainButton.image.color = can ? BtnActive : BtnDisabled;
            _mainButtonLabel.text = can ? "Điểm danh" : "Đã điểm danh";
        }
    }
}
