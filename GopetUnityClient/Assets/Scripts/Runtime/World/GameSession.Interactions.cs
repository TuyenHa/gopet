using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Xử lý tap avatar khác & bấm building trên map. Tách khỏi <c>GameSession.cs</c>
    /// để giữ file chính gọn — cả hai đều dựa vào state đã có trong partial gốc.
    /// </summary>
    public sealed partial class GameSession
    {
        private void OnAvatarTapped(PlayerAvatar avatar)
        {
            if (avatar == null) return;
            // Self: chưa có menu riêng cho self trên jar hub — bỏ qua, tránh tự-target.
            if (avatar.UserId == _login.UserId)
            {
                Debug.Log("[Gopet] Tap self avatar — bỏ qua (chưa có menu self).");
                return;
            }
            OpenTargetPlayerMenu(avatar.UserId, avatar.PlayerName);
        }

        private void OnBuildingSelected(JarMapEntity entity)
        {
            var action = BuildingDispatcher.Dispatch(entity.BuildingType, _hasPetFollowing);
            var label = BuildingDispatcher.LabelOf(entity.BuildingType);
            switch (action.Type)
            {
                case BuildingAction.Kind.Send:
                    if (!_actionThrottle.TryAcquire($"building:{entity.BuildingType}", 500, out var remainingMs))
                    {
                        action.Packet.Dispose();
                        ShowToast($"Thao tác quá nhanh, thử lại sau {remainingMs} ms.");
                        return;
                    }
                    _client.Send(action.Packet);
                    Debug.Log($"[Gopet] Building '{label}' (type {entity.BuildingType}) → gửi opcode {action.Packet.Id}.");
                    break;
                case BuildingAction.Kind.LocalMenu:
                    ShowToast($"'{label}' — chưa mở trong Unity (menu local, chờ phase kế).");
                    Debug.Log($"[Gopet] Building '{label}' (type {entity.BuildingType}) → menu local {action.Menu} (chưa impl).");
                    break;
                case BuildingAction.Kind.Toast:
                    ShowToast(action.ToastText);
                    break;
                case BuildingAction.Kind.Noop:
                    Debug.Log($"[Gopet] Building type {entity.BuildingType} — không có hành động (jar noop).");
                    break;
            }
        }

        private void ShowToast(string text)
        {
            if (_hudParent == null) return;
            ToastView.Create(_hudParent, UiBuilder.BuiltinFont(), text);
        }
    }
}
