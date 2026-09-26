using Gopet.Net.Guider;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Tab Pet của popup Hành lý, style popup Chợ trời: khay <see cref="PopupTabRail"/> sáu
    /// tab trải ngang phía trên, dưới là 2 block — trái đổi theo tab, phải luôn là ô trang
    /// bị pet (<see cref="PetEquipView"/> nhúng).
    ///
    /// <para>"Chọn pet" → danh sách pet (menu 5) bên trái. "Trang bị pet" → lưới Rương đồ
    /// (menu 81004) bên trái; chạm món → popup chi tiết "Trang bị"/"Hủy", Trang bị = server
    /// "Dùng" (mặc cho pet đang theo) rồi nạp lại ô bên phải. Kho ngọc / Hình xăm / Tẩy gym
    /// chiếm nguyên vùng nội dung (<c>CharacterHubPopupView.PetContent.cs</c>). Riêng
    /// "Cộng tiềm năng" vẫn là nút mở popup riêng — highlight trả về tab đang xem.</para>
    /// </summary>
    public sealed partial class CharacterHubPopupView
    {
        private const float PetLeftFraction = 0.40f;
        private const int PetGridColumns = 7;
        private const int PetTabSelect = 0;
        private const int PetTabEquip = 1;
        private const int PetTabPotential = 3;

        private static readonly string[] PetTabLabels =
        {
            "Chọn pet", "Trang bị pet", "Kho ngọc", "Cộng tiềm năng", "Hình/tẩy xăm", "Tẩy gym",
            "Học kỹ năng"
        };

        /// <summary>Hành động của từng tab; hai tab đầu đổi danh sách trái (xem <see cref="ShowPetLeft"/>).</summary>
        private static readonly CharacterMenuAction[] PetTabActions =
        {
            CharacterMenuAction.SelectPet, CharacterMenuAction.Inventory, CharacterMenuAction.GemInventory,
            CharacterMenuAction.PetPotential, CharacterMenuAction.PetTattoo, CharacterMenuAction.PetGymReset,
            CharacterMenuAction.PetLearnSkill
        };

        private PopupTabRail _petRail;
        private int _petListTab;
        private GameObject _petGridScroll;
        private MenuScreen _petInventoryScreen;

        private bool PetEquipMode => _petRail != null && _petListTab == PetTabEquip;

        private void BuildPetTab()
        {
            var railHost = new GameObject("PetTabs", typeof(RectTransform));
            railHost.transform.SetParent(_body, false);
            var railRect = (RectTransform)railHost.transform;
            UiBuilder.Stretch(railRect);
            railRect.offsetMin = new Vector2(5f, 5f);
            railRect.offsetMax = new Vector2(-5f, -5f);
            // 7 tab chia bề ngang: mỗi tab ~90px, nhãn dài ("Cộng tiềm năng") có thể phải xuống dòng.
            _petRail = PopupTabRail.Create(railRect, UiBuilder.DefaultFont(), _contentWidth - 10f, PetTabLabels,
                twoLines: true);

            var paneTop = 5f + PopupTabRail.TallHeight + PopupTabRail.Gap;
            var left = MakePane("Pet list", 0f, PetLeftFraction, paneTop);
            var right = MakePane("Trang bị pet", PetLeftFraction, 1f, paneTop);
            _petSplitPanes = new[] { left.gameObject, right.gameObject };
            BuildPetFullPane(paneTop);
            _leftListHost = MakeListHost(left, 6f);

            var equipHost = new GameObject("Pet equipment", typeof(RectTransform)).transform;
            equipHost.SetParent(right, false);
            var equipRect = (RectTransform)equipHost;
            UiBuilder.Stretch(equipRect);
            equipRect.offsetMin = new Vector2(4f, 4f);
            equipRect.offsetMax = new Vector2(-4f, -4f);
            _embeddedEquip = PetEquipView.CreateEmbedded(equipHost, _assets);
            _embeddedEquip.ActionChosen += (item, action) => PetEquipActionChosen?.Invoke(item, action);
            _embeddedEquip.HiddenStatsRequested += () => PetHiddenStatsRequested?.Invoke();
            // Chạm ô trống = muốn mặc đồ → mở lưới Rương đồ bên trái.
            _embeddedEquip.EmptySlotTapped += _ => _petRail.Select(PetTabEquip);

            _petRail.Selected += OnPetTabSelected;
            _petRail.Select(PetTabSelect);
            // Mở tab là tự nạp trang bị của pet đang chọn cho block phải.
            Request(CharacterMenuAction.PetEquipment);
        }

        private void OnPetTabSelected(int index)
        {
            if (index == PetTabPotential)
            {
                Request(PetTabActions[index]);
                _petRail.Select(_petListTab, false);
                return;
            }
            _petListTab = index;
            ShowPetContent();
        }

        /// <summary>Dọn nội dung cũ rồi xin dữ liệu của tab đang chọn. Chọn pet/Trang bị
        /// dùng 2 block trái-phải, các tab còn lại dùng vùng toàn khổ; nội dung về qua
        /// <see cref="TryConsumeMenu"/> (menu 5/81004/800), <see cref="PetContentHost"/>
        /// (Kho ngọc, Hình xăm).</summary>
        private void ShowPetContent()
        {
            var split = _petListTab == PetTabSelect || _petListTab == PetTabEquip;
            foreach (var pane in _petSplitPanes) pane.SetActive(split);
            _petFullPane.SetActive(!split);

            ClearMenu(true);
            if (_petGridScroll != null) Destroy(_petGridScroll);
            _petGridScroll = null;
            _inventoryGrid = null;
            ClearPetFullHost();
            Request(PetTabActions[_petListTab]);
        }

        private void BindPetInventory(MenuScreen screen)
        {
            _petInventoryScreen = screen;
            if (_inventoryGrid == null) BuildPetGrid();
            _inventoryGrid.Bind(screen);
            var gridRect = (RectTransform)_inventoryGrid.transform;
            gridRect.sizeDelta = new Vector2(0f, _inventoryGrid.ContentHeight);
        }

        /// <summary>Lưới 7 cột trong khung cuộn dọc — block trái hẹp nên 100 ô dài hơn pane.</summary>
        private void BuildPetGrid()
        {
            var scrollGo = new GameObject("Pet inventory", typeof(RectTransform), typeof(Image),
                typeof(ScrollRect));
            scrollGo.transform.SetParent(_leftListHost, false);
            var viewport = (RectTransform)scrollGo.transform;
            UiBuilder.Stretch(viewport);
            viewport.offsetMin = new Vector2(4f, 4f);
            viewport.offsetMax = new Vector2(-4f, -4f);
            scrollGo.GetComponent<Image>().color = Color.clear;

            // Bề ngang = cột trái − lề pane 5×2 − lề list host 6×2 − lề khung cuộn 4×2.
            var width = _contentWidth * PetLeftFraction - 30f;
            _inventoryGrid = InventoryGridView.Create(scrollGo.transform, transform, _assets, _guider,
                width, PetGridColumns);
            _inventoryGrid.ItemClicked += ShowPetItemDetails;
            var gridRect = (RectTransform)_inventoryGrid.transform;
            gridRect.anchorMin = new Vector2(0f, 1f);
            gridRect.anchorMax = new Vector2(1f, 1f);
            gridRect.pivot = new Vector2(0.5f, 1f);
            gridRect.anchoredPosition = Vector2.zero;
            gridRect.sizeDelta = new Vector2(0f, _inventoryGrid.ContentHeight);

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = gridRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 20f;
            _petGridScroll = scrollGo;
        }

        private void ShowPetItemDetails(MenuItemInfo item, int index)
        {
            var screen = _petInventoryScreen;
            InventoryItemPopupView.Create(transform, item, _assets, item.CanSelect
                ? () =>
                {
                    _guider?.Select(screen, index);
                    // Nạp lại ô trang bị bên phải + rương (dấu "đang mặc" đổi theo).
                    Request(CharacterMenuAction.PetEquipment);
                    Request(CharacterMenuAction.Inventory);
                }
                : null);
        }
    }
}
