using Gopet.Net.Guider;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Vùng nội dung TOÀN KHỔ của tab Pet (thay 2 block trái-phải) cho Kho ngọc, Hình xăm /
    /// Tẩy xăm, Tẩy gym, Học kỹ năng — hiện ngay trong Hành lý thay vì mở popup riêng.
    ///
    /// <para>Tẩy gym (menu 800) bind thẳng ở <see cref="TryConsumeMenu"/>. Kho ngọc và Hình
    /// xăm không phải MenuScreen (handler riêng dựng view) nên chủ view hỏi
    /// <see cref="PetContentHost"/> lấy chỗ nhúng: <c>UiRoot.GemInventoryHost</c>,
    /// <c>GameSession.ShowTattooScreen</c>.</para>
    /// </summary>
    public sealed partial class CharacterHubPopupView
    {
        private const int GymMenuId = 800;
        /// <summary>Danh sách kỹ năng học được (<c>MENU_LEARN_NEW_SKILL</c>).</summary>
        private const int LearnSkillMenuId = 799;

        private GameObject[] _petSplitPanes;
        private GameObject _petFullPane;
        private Transform _petFullHost;
        private ItemSelectPopupView _petGymView;

        private void BuildPetFullPane(float paneTop)
        {
            var pane = MakePane("Pet content", 0f, 1f, paneTop);
            _petFullPane = pane.gameObject;
            _petFullHost = MakeListHost(pane, 6f);
            _petFullPane.SetActive(false);
        }

        /// <summary>
        /// Chỗ nhúng nội dung của <paramref name="action"/> nếu tab Pet đang mở đúng trang đó;
        /// null = không nhúng được (người gọi mở popup riêng như cũ).
        /// </summary>
        public Transform PetContentHost(CharacterMenuAction action)
        {
            if (_activeTab != CharacterHubTab.Pet || _petRail == null || _petFullHost == null) return null;
            return PetTabActions[_petListTab] == action && _petFullPane.activeSelf ? _petFullHost : null;
        }

        /// <summary>Menu server của các tab toàn khổ dạng danh sách chọn: Tẩy gym (800),
        /// Học kỹ năng (799). Chỉ nhúng khi tab Pet đang mở đúng trang đó.</summary>
        private bool TryBindPetGym(MenuScreen screen)
        {
            var action = screen.ListId == GymMenuId ? CharacterMenuAction.PetGymReset
                : screen.ListId == LearnSkillMenuId ? CharacterMenuAction.PetLearnSkill
                : (CharacterMenuAction?)null;
            if (action == null || PetContentHost(action.Value) == null)
                return false;
            if (_petGymView == null)
            {
                _petGymView = ItemSelectPopupView.CreateEmbedded(_petFullHost, UiBuilder.DefaultFont(),
                    screen, _guider, _assets);
                _petGymView.ConfirmRequested += ShowEmbeddedConfirm;
            }
            _petGymView.Bind(screen);
            return true;
        }

        private void ClearPetFullHost()
        {
            if (_petFullHost == null) return;
            for (var i = _petFullHost.childCount - 1; i >= 0; i--)
                Destroy(_petFullHost.GetChild(i).gameObject);
            _petGymView = null;
        }
    }
}
