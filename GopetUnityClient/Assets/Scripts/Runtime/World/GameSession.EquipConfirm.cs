using System;
using Gopet.Net.Pet;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using Object = UnityEngine.Object;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Hộp xác nhận cho thao tác trên đồ pet (tháo, huỷ) và toast báo kết quả tháo đồ.
    /// </summary>
    public sealed partial class GameSession
    {
        private YesNoDialog _equipConfirm;

        /// <summary>Item đang chờ server trả kết quả tháo. 0 = không chờ gì.</summary>
        private int _pendingUnequipId;
        private string _pendingUnequipName;

        /// <summary>Tháo đồ phải hỏi trước; bấm OK mới gửi lên server.</summary>
        private void OpenUnequipConfirm(PetEquipItem item)
        {
            var name = ItemName(item);
            ShowEquipConfirm($"Tháo {name} khỏi pet?", "OK", "Huỷ", () =>
            {
                _pendingUnequipId = item.ItemId;
                _pendingUnequipName = name;
                _client.Send(PetEquipPackets.Unequip(item.ItemId));
            });
        }

        /// <summary>Server trả UNEQUIP_ITEM: chỉ báo toast cho đúng món người chơi vừa bấm tháo.</summary>
        private void OnUnequipResult(PetEquipDelta delta)
        {
            if (delta == null || delta.Equipped || delta.Removed) return;
            if (_pendingUnequipId == 0 || delta.ItemId != _pendingUnequipId) return;

            ShowToast(delta.Accepted
                ? $"Tháo {_pendingUnequipName} thành công."
                : $"Không tháo được {_pendingUnequipName}.");
            _pendingUnequipId = 0;
            _pendingUnequipName = null;
        }

        /// <summary>Một hộp xác nhận duy nhất cho đồ pet — mở hộp mới thì hộp cũ bị thay.</summary>
        private void ShowEquipConfirm(string message, string yes, string no, Action onYes)
        {
            CloseEquipConfirm();
            var dialog = YesNoDialog.Create(_hudParent, message, yes, no);
            _equipConfirm = dialog;
            dialog.Confirmed += () =>
            {
                CloseEquipConfirm();
                onYes();
            };
            dialog.Cancelled += CloseEquipConfirm;
        }

        private void CloseEquipConfirm()
        {
            if (_equipConfirm != null) Object.Destroy(_equipConfirm.gameObject);
            _equipConfirm = null;
        }

        private static string ItemName(PetEquipItem item) =>
            JarIconTokens.Humanize(item?.DisplayName ?? string.Empty);
    }
}
