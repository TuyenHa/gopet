using System;
using Gopet.Net.Map;
using Gopet.Runtime.Audio;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    public sealed partial class GameSession
    {
        private void OnTeleportOptionsReceived(MapTeleportOption[] options)
        {
            CloseWorldMap();
            if (options == null || options.Length == 0)
            {
                ShowToast("Không có bản đồ nào khả dụng.");
                return;
            }

            _worldMapView = WorldMapView.Create(_hudParent, UiBuilder.BuiltinFont());
            _worldMapView.Bind(options, _scene.MapId);
            _worldMapView.Chosen += mapId => OnWorldMapChosen(options, mapId);
            // Câu chữ đến TỪ SERVER (TELE_MENU gửi kèm lý do khoá), client không tự chế:
            // thêm luật khoá mới bên server thì chỗ này không phải sửa.
            _worldMapView.LockedChosen += reason =>
                ShowToast(string.IsNullOrEmpty(reason) ? "Bản đồ này chưa mở." : reason);
            _worldMapView.CloseRequested += CloseWorldMap;
        }

        private void OnWorldMapChosen(MapTeleportOption[] options, int mapId)
        {
            var option = Array.Find(options, o => o.MapId == mapId);
            if (option == null) return;
            SoundManager.Instance?.PlayEffect("s_outMap_1");
            _mapHandler.SendWarp(option.MapId, option.WaypointIndex, 1);
            // Tên server gửi là chính; hụt thì lấy bảng tên cục bộ để màn chuyển map
            // không hiện trống trơn.
            _warpFade?.FadeOut(string.IsNullOrEmpty(option.Name)
                ? MapDisplayNames.Get(option.MapId)
                : option.Name);
            CloseWorldMap();
        }

        private void CloseWorldMap()
        {
            if (_worldMapView == null) return;
            UnityEngine.Object.Destroy(_worldMapView.gameObject);
            _worldMapView = null;
        }

        /// <summary>
        /// Chĩa camera minimap vào map vừa vào. Lấy layout TỪ SCENE chứ không nạp lại
        /// từ Resources: scene mới là thứ camera đang chụp, nạp lại là thêm một nguồn
        /// sự thật nữa để lệch nhau. Map chưa dựng xong thì minimap rơi về nhãn "Map N".
        /// </summary>
        private void RefreshMinimap()
        {
            if (_minimap == null) return;
            try
            {
                var layout = _scene.Map != null ? _scene.Map.Map : null;
                var width = layout?.WidthPixels ?? 0;
                var height = layout?.HeightPixels ?? 0;
                _minimapCamera?.Frame(width, height);
                _minimap.SetLiveMap(_minimapCamera?.Target, width, height, _scene.MapId);
                _minimap.BindPlayer(_scene.Self);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Gopet] Không dựng được minimap map {_scene.MapId}: {ex.Message}");
            }
        }
    }
}
