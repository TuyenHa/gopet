using System.Collections.Generic;
using Gopet.Net.Chat;
using Gopet.Net.Map;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Cầu nối giữa <see cref="MapHandler"/> (mạng, thuần C#) và <see cref="MapRenderer"/> +
    /// <see cref="PlayerAvatar"/> (Unity). Quản avatar người chơi; NPC/quái ở <see cref="WorldActorLayer"/>.
    /// Self xác định qua <see cref="PlayerInit"/>; người khác qua <see cref="PlayerEnterMap"/>.
    /// </summary>
    public sealed class MapScene : MonoBehaviour
    {
        private MapRenderer _map;
        private int _mapId = -1;
        private int _selfUserId = -1;
        private string _selfName = string.Empty;
        private int _selfGender;
        private readonly Dictionary<int, PlayerAvatar> _avatars = new Dictionary<int, PlayerAvatar>();
        private WorldActorLayer _actors;

        public int MapId => _mapId;
        public MapRenderer Map => _map;
        public PlayerAvatar Self => _selfUserId >= 0 && _avatars.TryGetValue(_selfUserId, out var s) ? s : null;

        /// <summary>Transform của avatar player theo userId, hoặc null nếu chưa spawn. Cho PetLayer bám vào.</summary>
        public Transform TryGetAvatarTransform(int userId) =>
            _avatars.TryGetValue(userId, out var a) && a != null ? a.transform : null;

        /// <summary>Tên hiển thị của người chơi trên map, dùng cho lịch sử chat khu vực.</summary>
        public string TryGetAvatarName(int userId) =>
            _avatars.TryGetValue(userId, out var avatar) && avatar != null
                ? avatar.PlayerName
                : null;

        public void ApplySkin(int userId, string path, RemoteAssetCache assets)
        {
            if (_avatars.TryGetValue(userId, out var avatar) && avatar != null)
                avatar.ApplySkin(path, assets);
        }

        public void ApplyWing(int userId, string path, int verticalOffset, RemoteAssetCache assets)
        {
            if (_avatars.TryGetValue(userId, out var avatar) && avatar != null)
                avatar.ApplyWing(path, verticalOffset, assets);
        }

        /// <summary>Map vừa nạp xong (hoặc đổi map) — camera dùng để recenter.</summary>
        public event System.Action MapLoaded;
        /// <summary>Self spawn ở đúng vị trí server (opcode 29).</summary>
        public event System.Action<PlayerEnterMap> SelfSpawned;
        public event System.Action<JarMapEntity> PortalSelected;

        /// <summary>Màn chuyển map — dựng khi chọn cổng, gỡ khi map mới nạp xong.</summary>
        private MapLoadingOverlay _loading;
        public event System.Action<JarMapEntity> BuildingSelected;
        /// <summary>Bấm avatar player (kể cả self). GameSession quyết định mở menu gì.</summary>
        public event System.Action<PlayerAvatar> AvatarTapped;
        /// <summary>Avatar đã được tạo; các lớp trang trí có thể gắn dữ liệu đến sớm đang chờ.</summary>
        public event System.Action<PlayerAvatar> AvatarSpawned;

        public static MapScene Create(Transform parent = null)
        {
            var go = new GameObject("MapScene");
            if (parent != null) go.transform.SetParent(parent, false);
            return go.AddComponent<MapScene>();
        }

        /// <summary>
        /// Nạp và hiện map. Dựng map MỚI TRƯỚC rồi mới huỷ map cũ: nếu file map không có
        /// (<c>JarMaps.Load</c> ném), huỷ trước sẽ để scene rỗng và mọi thứ sau đó hỏng theo.
        /// </summary>
        public void LoadMap(int mapId)
        {
            MapRenderer next;
            try
            {
                next = MapRenderer.Create(transform, mapId);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Gopet] Không nạp được map {mapId}: {ex.Message}. Giữ nguyên map {_mapId}.");
                return;
            }

            if (_map != null) DestroyWorldObject(_map.gameObject);
            foreach (var a in _avatars.Values) if (a != null) DestroyWorldObject(a.gameObject);
            _avatars.Clear();
            _actors?.Clear();

            _map = next;
            _map.PortalSelected += OnPortalSelected;
            _map.BuildingSelected += OnBuildingSelected;
            _mapId = mapId;
            Debug.Log($"[Gopet] Map {mapId} nạp xong: {_map.Map.WidthTiles}×{_map.Map.HeightTiles} ô");
            HideLoading();
            MapLoaded?.Invoke();
        }

        /// <summary>Đăng ký sự kiện MapHandler. Gọi sau khi router đã có handler.</summary>
        public void Subscribe(MapHandler h)
        {
            h.PlayerInitReceived += OnPlayerInit; h.PlayerEntered += OnPlayerEntered;
            h.PlayerExited += OnPlayerExited; h.PlayerMoved += OnPlayerMoved;
            h.MapUpdated += OnMapUpdated;
        }
        public void Unsubscribe(MapHandler h)
        {
            h.PlayerInitReceived -= OnPlayerInit; h.PlayerEntered -= OnPlayerEntered;
            h.PlayerExited -= OnPlayerExited; h.PlayerMoved -= OnPlayerMoved;
            h.MapUpdated -= OnMapUpdated;
        }

        private void OnPlayerInit(PlayerInit init)
        {
            // Chưa có toạ độ — chờ ON_UPDATE_PLAYER_IN_MAP (opcode 29) mang vị trí spawn self.
            _selfUserId = init.UserId;
            _selfName = init.Name;
            _selfGender = init.Gender;
        }

        private PlayerAvatar SpawnAvatar(int userId, string name, int gender, int x, int y, int? faceDir = null)
        {
            var avatar = PlayerAvatar.Spawn(transform, userId, name, gender, x, y, _map.Map.HeightPixels);
            if (faceDir.HasValue) avatar.SetLocomotion(faceDir.Value, false);
            avatar.Tapped += OnAvatarTapped;
            _avatars[userId] = avatar;
            if (userId == _selfUserId) _map.SetSelf(avatar.transform);
            AvatarSpawned?.Invoke(avatar);
            return avatar;
        }

        private void OnPlayerEntered(PlayerEnterMap evt)
        {
            if (_map == null || _avatars.ContainsKey(evt.UserId)) return; // đã có, không dựng đôi
            SpawnAvatar(evt.UserId, evt.Name, evt.Gender, evt.X, evt.Y, evt.FaceDir);
        }

        private void OnAvatarTapped(PlayerAvatar avatar) => AvatarTapped?.Invoke(avatar);

        private void OnPlayerExited(PlayerExitPlace evt)
        {
            if (!_avatars.TryGetValue(evt.UserId, out var avatar)) return;
            if (avatar != null) Destroy(avatar.gameObject);
            _avatars.Remove(evt.UserId);
        }

        private void OnPlayerMoved(PlayerMoved evt)
        {
            // Self: MovementController giữ vị trí authoritative locally; bỏ echo để không giật ngược.
            if (evt.UserId == _selfUserId) return;
            // Người khác: server là nguồn chân lý, nội suy cho mượt.
            if (_avatars.TryGetValue(evt.UserId, out var avatar) && avatar != null)
            {
                avatar.SetLocomotion(evt.Direction, true);
                avatar.MoveAlong(evt.Points);
            }
        }

        private void OnPortalSelected(JarMapEntity entity)
        {
            // Che màn NGAY khi chọn cổng, không đợi server trả lời: quãng chờ round-trip mới
            // là lúc cần che, map mới dựng xong thì chỉ mất một frame.
            if (_loading == null) _loading = MapLoadingOverlay.Create(transform.parent);
            PortalSelected?.Invoke(entity);
        }
        private void OnBuildingSelected(JarMapEntity entity) => BuildingSelected?.Invoke(entity);

        private void HideLoading()
        {
            if (_loading == null) return;
            // KHÔNG huỷ thẳng: màn tự gỡ sau khi đã hiện đủ lâu — xem MinVisibleSeconds.
            _loading.RequestClose();
            _loading = null;
        }

        private void OnMapUpdated(MapUpdate evt)
        {
            if (_selfUserId < 0) return;
            if (_mapId != evt.MapId) // server đưa tới map khác → reload cho khớp toạ độ
            {
                Debug.Log($"[Gopet] Reload map: {_mapId} -> {evt.MapId} (server đưa tới)");
                LoadMap(evt.MapId);
            }
            if (_map == null) return;

            if (!_avatars.ContainsKey(_selfUserId))
                SpawnAvatar(_selfUserId, _selfName, _selfGender, evt.SelfX, evt.SelfY);
            else Self.SnapTo(evt.SelfX, evt.SelfY);

            SelfSpawned?.Invoke(new PlayerEnterMap
            {
                UserId = _selfUserId, X = evt.SelfX, Y = evt.SelfY, Gender = _selfGender
            });

            foreach (var o in evt.Others)
            {
                if (_avatars.ContainsKey(o.UserId)) continue;
                SpawnAvatar(o.UserId, o.Name, o.Gender, o.X, o.Y, o.FaceDir);
            }
        }

        /// <summary>Chat + tương tác pet: hiện bong bóng/hiệu ứng trên avatar tương ứng.</summary>
        public void SubscribeChat(ChatHandler chat) => chat.ChatReceived += OnChatReceived;
        public void UnsubscribeChat(ChatHandler chat) => chat.ChatReceived -= OnChatReceived;

        /// <summary>NPC/quái tách sang <see cref="WorldActorLayer"/>; pet-interact ở lại vì đụng avatar.</summary>
        public void SubscribeWorld(WorldObjectHandler handler, RemoteAssetCache assets,
            System.Action<int> talkToNpc, System.Action<int> attackMob = null)
        {
            handler.PetInteractionReceived += OnPetInteraction;
            _actors = WorldActorLayer.Attach(this);
            _actors.Subscribe(handler, assets, talkToNpc, attackMob);
        }

        public void ApplyBossHp(BossHpUpdate update) => _actors?.ApplyBossHp(update);

        private void OnPetInteraction(PetInteraction evt)
        {
            if (_avatars.TryGetValue(evt.UserId, out var avatar))
                PetInteractionEffect.Attach(avatar, evt.Type);
        }

        private void OnChatReceived(PlaceChat evt)
        {
            // kiss/play/poke đi qua ON_PET_INTERACT, không phải chat thường.
            if (evt.IsPetInteraction) return;
            if (_avatars.TryGetValue(evt.UserId, out var avatar))
                ChatBubble.AttachOrUpdate(avatar, evt.Text);
        }

        internal static void DestroyWorldObject(GameObject value)
        {
            if (value == null) return;
            value.SetActive(false);
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }
    }
}
