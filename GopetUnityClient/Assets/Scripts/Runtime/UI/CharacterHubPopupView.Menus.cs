using System;
using Gopet.Net.Guider;
using Gopet.Net.Pet;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using Gopet.Runtime.Audio;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class CharacterHubPopupView
    {
        /// <summary>Khe giữa các thẻ nền tối (chọn pet, bạn bè) — không thì dính thành một khối.</summary>
        private const float CardRowGap = 6f;

        private void BindMenu(Transform host, MenuScreen screen, bool left)
        {
            var current = left ? _leftMenu : _rightMenu;
            if (current == null)
            {
                current = GenericMenuView.Create(host, UiBuilder.DefaultFont());
                current.CloseRequested += _ => ClearMenu(left);
                current.ConfirmRequested += ShowEmbeddedConfirm;
                current.EnableEmbeddedScroll(Mathf.Max(80f, ((RectTransform)host).rect.height));
                if (left) _leftMenu = current; else _rightMenu = current;
            }
            current.SelectionOverride = _activeTab == CharacterHubTab.Character &&
                screen.ListId == 81040 ? index => TryUseWing(screen, index) : null;
            current.SetCompactCards(_activeTab == CharacterHubTab.Character &&
                (screen.ListId == 803 || screen.ListId == 81040));
            // Thẻ nền tối: "Chọn pet" (menu 5) và "Danh sách bạn bè" (menu 1060).
            var cardList = (_activeTab == CharacterHubTab.Pet && screen.ListId == 5) ||
                           (_activeTab == CharacterHubTab.Friends && screen.ListId == 1060);
            current.SetRowGap(cardList ? CardRowGap : 0f);
            // Pane Tủ quần áo có thể thấp hơn menu khác; đồng bộ viewport thực tế
            // để thẻ ở dưới luôn kéo/cuộn được thay vì bị cắt mất.
            current.SetViewport(Mathf.Max(80f, ((RectTransform)host).rect.height));
            current.Bind(screen, _assets, _guider);
        }

        private void ClearMenu(bool left)
        {
            var menu = left ? _leftMenu : _rightMenu;
            if (menu != null) Destroy(menu.gameObject);
            if (left) _leftMenu = null; else _rightMenu = null;
        }

        private void ClearBody()
        {
            DismissConfirmation();
            for (var i = _body.childCount - 1; i >= 0; i--) Destroy(_body.GetChild(i).gameObject);
            _leftListHost = _rightListHost = null;
            _friendsEmptyState = null;
            _leftMenu = _rightMenu = null;
            _inventoryGrid = null;
            _embeddedEquip = null;
        }

    }
}
