using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup 2 tab cho NPC "Sứ Giả Thiên Đình" (npcId -25, options 88+89):
    /// <list type="bullet">
    /// <item>Tab 1 "Hướng dẫn lên thiên đình" — hiển thị text hướng dẫn ngay,
    /// không cần round-trip server. Text hardcode khớp <c>Language.GuideToHeaven</c>
    /// mặc định — nếu server sửa text thì đồng bộ tay ở đây.</item>
    /// <item>Tab 2 "Hiến tặng thú cưng" — bấm gửi <c>SelectNpcOption(89)</c>,
    /// server trả <c>MENU_PET_SACRIFICE (1084)</c>, <see cref="TryConsumeMenu"/>
    /// bind vào <see cref="PetGridView"/> trong khu vực body của tab.</item>
    /// </list>
    /// Style bám <see cref="TranChanTabsView"/> — cùng khung màu popup shop.
    /// </summary>
    public sealed class HeavenNpcTabsView : MonoBehaviour
    {
        /// <summary>
        /// <c>MENU_PET_SACRIFICE = 1084</c> (<c>MenuController.cs:141</c>) — listId server
        /// trả cho tab "Hiến tặng".
        ///
        /// <para><c>public</c> chứ không <c>internal</c>: PlayMode test là một assembly
        /// RIÊNG trong Unity. Bước compile <c>Gopet.PlayMode.Compile</c> gộp Runtime và
        /// test vào một assembly nên không bắt được lỗi này — chỉ Unity mới báo.</para>
        /// </summary>
        public const int MenuPetSacrificeListId = 1084;

        // Text hardcode khớp Language.GuideToHeaven (LanguageData.cs:581).
        private const string GuideText =
            "Bạn cần làm hết các nhiệm vụ trùng sinh để nhận được cánh bay về trời. " +
            "Nơi thú cưng đột biến kinh khủng khiếp đang trên đầu chúng ta.";

        private const float Width = 520f;
        private const float Height = 320f;

        /// <summary>Băng chân đổi theo tab đang xem; tab lạ dùng câu đầu.</summary>
        private static readonly string[] FooterByTab =
        {
            "Hướng dẫn đường lên thiên đình",
            "Chọn thú cưng để hiến tặng",
        };

        private static readonly Color TextDark = PopupPalette.TextDark;

        public event Action<int> TabChosen;
        public event Action Closed;
        public event Action<Gopet.UiLogic.MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        private int _activeOptionId;
        private GamePopupFrame _frame;
        private PopupTabRail _rail;
        private NpcOptions.Option[] _options;
        private GameObject _guideView;
        private PetGridView _petGrid;

        public static HeavenNpcTabsView Create(Transform parent, Font font, NpcOptions options)
        {
            var frame = GamePopupFrame.Create(parent, font, "Sứ giả", Width, Height,
                footer: FooterByTab[0]);
            frame.gameObject.name = "HeavenNpcTabs";

            var view = frame.gameObject.AddComponent<HeavenNpcTabsView>();
            view._frame = frame;
            view._options = options?.Options ?? Array.Empty<NpcOptions.Option>();
            frame.Closed += () => view.Closed?.Invoke();

            view.BuildTabs(frame, font);
            view.BuildBody(frame.Content, font);
            view.SelectFirstTab();
            return view;
        }

        /// <summary>Bind danh sách pet vào body khi user chọn tab "Hiến tặng".
        /// Chỉ tiêu thụ MenuScreen khi listId khớp <c>MENU_PET_SACRIFICE</c>.</summary>
        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            if (screen == null || _petGrid == null) return false;
            if (screen.ListId != MenuPetSacrificeListId) return false;

            _petGrid.Bind(screen, assets, guider, "Hiến tặng", PetGridMode.Receive);
            _petGrid.gameObject.SetActive(true);
            if (_guideView != null) _guideView.SetActive(false);
            return true;
        }

        private void BuildTabs(GamePopupFrame frame, Font font)
        {
            if (_options.Length == 0) return;

            var labels = new string[_options.Length];
            for (var i = 0; i < _options.Length; i++) labels[i] = _options[i].Text ?? string.Empty;

            _rail = PopupTabRail.Create(frame.Content, font, frame.ContentWidth, labels);
        }

        private void BuildBody(RectTransform content, Font font)
        {
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(content, false);
            var rect = (RectTransform)body.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
            RoundedBorder.Apply(body, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                PopupPalette.Hairline);

            _guideView = BuildGuideText(body.transform, font);
            _petGrid = PetGridView.Create(body.transform, font);
            _petGrid.ConfirmRequested += (prompt, confirm) => ConfirmRequested?.Invoke(prompt, confirm);
            _petGrid.gameObject.SetActive(false);
        }

        private static GameObject BuildGuideText(Transform parent, Font font)
        {
            var go = new GameObject("Guide", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            UiBuilder.Stretch(rect);
            rect.offsetMin = new Vector2(14f, 12f);
            rect.offsetMax = new Vector2(-14f, -12f);

            var text = UiBuilder.MakeText(go.transform, font, "Text", 12, true);
            text.text = GuideText;
            text.color = TextDark;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return go;
        }

        /// <summary>
        /// Chọn tab đầu KHÔNG báo server (tab 1 là text hướng dẫn, hiện ngay). Nối sự
        /// kiện của khay tab SAU đó: <c>Select</c> chỉ bắn khi tab đổi, nối trước là
        /// lần chọn đầu cũng gửi option đi.
        /// </summary>
        private void SelectFirstTab()
        {
            if (_options.Length == 0) return;

            _rail.Select(0);
            SelectTab(_options[0].Id, notify: false);
            _rail.Selected += index => SelectTab(_options[index].Id);
        }

        private void SelectTab(int optionId, bool notify = true)
        {
            _activeOptionId = optionId;

            // Tab 1 (index 0) = guide text hardcode, không cần round-trip server.
            // Tab 2+ = mở list qua SelectNpcOption.
            var isGuideTab = _options.Length > 0 && _options[0].Id == optionId;
            _frame.SetFooter(FooterByTab[isGuideTab ? 0 : 1]);
            if (_guideView != null) _guideView.SetActive(isGuideTab);
            if (_petGrid != null && isGuideTab) _petGrid.gameObject.SetActive(false);

            if (notify && !isGuideTab) TabChosen?.Invoke(optionId);
        }
    }
}
