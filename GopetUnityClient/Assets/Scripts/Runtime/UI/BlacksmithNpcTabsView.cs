using System;
using System.Text.RegularExpressions;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup 2 tab cho NPC "Thợ Rèn" (npcId -42, options 98 + 99), cùng khung
    /// <see cref="GamePopupFrame"/> với popup cửa hàng:
    /// <list type="bullet">
    /// <item>Tab "Sửa trang bị" (98): mở popup là tự gửi option, server trả
    /// <c>MENU_REPAIR_EQUIP (1092)</c> gồm MỌI trang bị pet → vẽ thành lưới 50 ô như
    /// Rương đồ. Chạm một ô mở <see cref="BlacksmithRepairPopupView"/>.</item>
    /// <item>Tab "Độ bền là gì?" (99): text hardcode, không round-trip server.</item>
    /// </list>
    /// Phần lưới nằm ở <c>BlacksmithNpcTabsView.Grid.cs</c>.
    /// </summary>
    public sealed partial class BlacksmithNpcTabsView : MonoBehaviour
    {
        // Khớp MenuController.equipRepair.cs (server). public để PlayMode test thấy.
        public const int OpRepairEquip = 98;
        public const int OpDurabilityHelp = 99;
        public const int MenuRepairEquipListId = 1092;

        // Khớp EquipRepairService.HelpText (server) — server sửa text thì đồng bộ tay ở đây.
        private const string HelpText =
            "Trang bị pet (nón, kiếm, giày, bao tay, giáp) có độ bền tối đa 80. Mỗi trận thắng trừ 1, " +
            "thua trừ 2. Về 0 thì món đó HỎNG và mất chỉ số riêng (bonus set vẫn giữ).\n\n" +
            "Mang tới ta cùng 1 Đá mài sửa chữa để sửa đầy 1 món.\n\n" +
            "Đá mài có khi đánh quái, hạ boss và quà điểm danh.";

        private const float Width = 560f;
        private const float Height = 330f;
        private const string GridFooter = "Chạm món đồ để xem độ bền và sửa chữa";
        private const string HelpFooter = "Độ bền trang bị pet";

        private static readonly Regex StoneCountPattern = new Regex(@"Đá mài:\s*(\d+)");

        public event Action Closed;

        private GamePopupFrame _frame;
        private PopupTabRail _rail;
        private NpcOptions.Option[] _options;
        private GameObject _gridPanel;
        private GameObject _helpPanel;
        private Font _font;
        private RemoteAssetCache _assets;
        private GuiderHandler _guider;
        private MenuScreen _screen;
        private int _npcId;
        /// <summary>-1 = chưa biết (server chưa trả hoặc tiêu đề lạ) → không chặn nút sửa.</summary>
        private int _stoneCount = -1;

        public static BlacksmithNpcTabsView Create(Transform parent, Font font, NpcOptions options,
            RemoteAssetCache assets, GuiderHandler guider)
        {
            var frame = GamePopupFrame.Create(parent, font, "Thợ rèn", Width, Height,
                footer: GridFooter);
            frame.gameObject.name = "BlacksmithNpcTabs";

            var view = frame.gameObject.AddComponent<BlacksmithNpcTabsView>();
            view._frame = frame;
            view._font = font;
            view._assets = assets;
            view._guider = guider;
            view._npcId = options?.NpcId ?? 0;
            view._options = options?.Options ?? Array.Empty<NpcOptions.Option>();
            frame.Closed += () => view.Closed?.Invoke();

            view.BuildTabs();
            view.BuildBody();
            view.SelectFirstTab();
            return view;
        }

        /// <summary>Nhận lưới trang bị từ server. Không phải menu sửa thì để dispatcher khác xử.</summary>
        public bool TryConsumeMenu(MenuScreen screen)
        {
            if (screen == null || screen.ListId != MenuRepairEquipListId) return false;

            _screen = screen;
            var match = StoneCountPattern.Match(screen.Title ?? string.Empty);
            _stoneCount = match.Success ? int.Parse(match.Groups[1].Value) : -1;
            BindGrid(screen);
            UpdateFooter();
            return true;
        }

        private void BuildTabs()
        {
            if (_options.Length == 0) return;

            var labels = new string[_options.Length];
            for (var i = 0; i < _options.Length; i++) labels[i] = _options[i].Text ?? string.Empty;
            _rail = PopupTabRail.Create(_frame.Content, _font, _frame.ContentWidth, labels);
        }

        private void BuildBody()
        {
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(_frame.Content, false);
            var rect = (RectTransform)body.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
            RoundedBorder.Apply(body, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                PopupPalette.Hairline);

            _gridPanel = BuildGridPanel(body.transform);
            _helpPanel = BuildHelpPanel(body.transform);
        }

        private GameObject BuildHelpPanel(Transform parent)
        {
            var go = new GameObject("Help", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            UiBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(16f, 12f);
            rect.offsetMax = new Vector2(-16f, -12f);

            var text = UiBuilder.MakeText(go.transform, _font, "Text", 13, true);
            text.text = HelpText;
            text.color = PopupPalette.TextDark;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            go.SetActive(false);
            return go;
        }

        /// <summary>
        /// Chọn tab đầu rồi mới nối sự kiện khay tab (Select chỉ bắn khi tab đổi). Tab sửa
        /// là tab đầu nên tự xin lưới ngay khi mở popup.
        /// </summary>
        private void SelectFirstTab()
        {
            if (_options.Length == 0) return;

            _rail.Select(0);
            SelectTab(_options[0].Id);
            _rail.Selected += index => SelectTab(_options[index].Id);
        }

        private void SelectTab(int optionId)
        {
            var isHelp = optionId == OpDurabilityHelp;
            _helpPanel.SetActive(isHelp);
            _gridPanel.SetActive(!isHelp);
            UpdateFooter(isHelp);
            // Tự gửi (không qua event): tab đầu được chọn ngay trong Create, lúc UiRoot chưa kịp nối.
            if (!isHelp) _guider?.SelectNpcOption(_npcId, optionId);
        }

        private void UpdateFooter(bool isHelp = false)
        {
            if (isHelp || (_helpPanel != null && _helpPanel.activeSelf))
            {
                _frame.SetFooter(HelpFooter);
                return;
            }
            _frame.SetFooter(_stoneCount >= 0 ? $"Đá mài: {_stoneCount}  ·  {GridFooter}" : GridFooter);
        }
    }
}
