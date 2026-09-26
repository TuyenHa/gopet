using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        /// <summary>Tỉ lệ bề ngang cột trái (Tủ quần áo/Cánh) trong tab Nhân vật.</summary>
        private const float CharacterLeftFraction = 0.36f;

        private void BuildCharacterTab()
        {
            var left = MakePane("Ngoại hình", 0f, CharacterLeftFraction);
            var right = MakePane("Rương đồ", CharacterLeftFraction, 1f);
            MakeTitle(right, "Rương đồ", HubActionIcon(2));

            // Khay tab con kiểu popup Chợ trời: "Tủ quần áo" (mặc định) | "Cánh" — cả hai
            // đổ danh sách vào cùng _leftListHost bên dưới khay.
            _leftListHost = MakeListHost(left, SubRailInset + PopupTabRail.Height + PopupTabRail.Gap);
            _rightListHost = MakeListHost(right, 38f);
            var rail = MakeSubRail(left, _contentWidth * CharacterLeftFraction,
                new[] { "Tủ quần áo", "Cánh" });
            rail.Selected += index => Request(index == 0
                ? CharacterMenuAction.Wardrobe
                : CharacterMenuAction.WingInventory);

            Request(CharacterMenuAction.Inventory);
            rail.Select(0);
        }

        private void CloseThenRequest(CharacterMenuAction action)
        {
            Closed?.Invoke();
            ActionRequested?.Invoke(action);
        }

        private static string OnOff(bool value) => value ? "Bật" : "Tắt";
    }
}
