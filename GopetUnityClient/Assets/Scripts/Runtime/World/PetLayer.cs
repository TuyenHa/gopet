using System.Collections.Generic;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Điều phối spawn/despawn <see cref="PetAvatar"/> dựa trên các event của
    /// <see cref="PetZoneHandler"/> và bảng owner-transform từ <see cref="MapScene"/>.
    ///
    /// <para>Owner transform lấy qua callback <c>_resolveOwner(userId)</c> — thường
    /// là <see cref="MovementController"/>.SelfTransform hoặc actor player-remote đã spawn.
    /// Nếu chưa spawn owner, pet queue lại và spawn khi owner xuất hiện (SelfSpawned event).</para>
    /// </summary>
    public sealed class PetLayer : MonoBehaviour
    {
        public delegate Transform OwnerResolver(int userId);

        private readonly Dictionary<int, PetAvatar> _pets = new Dictionary<int, PetAvatar>();
        private readonly Dictionary<int, PetZoneEntry> _pending = new Dictionary<int, PetZoneEntry>();
        private RemoteAssetCache _assets;
        private OwnerResolver _resolveOwner;

        public static PetLayer Create(Transform parent, RemoteAssetCache assets, OwnerResolver resolveOwner)
        {
            var go = new GameObject("Pet Layer", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var layer = go.AddComponent<PetLayer>();
            layer._assets = assets;
            layer._resolveOwner = resolveOwner;
            return layer;
        }

        public void ApplyZone(PetZoneUpdate update)
        {
            var keep = new HashSet<int>();
            foreach (var e in update.Entries)
            {
                keep.Add(e.OwnerUserId);
                if (_pets.TryGetValue(e.OwnerUserId, out var existing))
                {
                    // Same owner, entry đổi → refresh (Destroy old, Create new).
                    Destroy(existing.gameObject);
                    _pets.Remove(e.OwnerUserId);
                }
                Spawn(e);
            }
            // Bên nào không còn trong list → despawn (owner đã unfollow ở nơi khác).
            var stale = new List<int>();
            foreach (var kv in _pets) if (!keep.Contains(kv.Key)) stale.Add(kv.Key);
            foreach (var id in stale) Remove(id);
        }

        public void Remove(int ownerUserId)
        {
            _pending.Remove(ownerUserId);
            if (_pets.TryGetValue(ownerUserId, out var pet))
            {
                Destroy(pet.gameObject);
                _pets.Remove(ownerUserId);
            }
        }

        /// <summary>Gọi khi 1 player mới spawn (self hoặc other) để try-flush pending pet.</summary>
        public void OnOwnerSpawned(int userId)
        {
            if (_pending.TryGetValue(userId, out var entry))
            {
                _pending.Remove(userId);
                Spawn(entry);
            }
        }

        private void Spawn(PetZoneEntry entry)
        {
            var owner = _resolveOwner?.Invoke(entry.OwnerUserId);
            if (owner == null)
            {
                // Owner chưa spawn — cache lại, spawn khi OnOwnerSpawned được gọi.
                _pending[entry.OwnerUserId] = entry;
                return;
            }
            var pet = PetAvatar.Create(transform, owner, entry, _assets);
            _pets[entry.OwnerUserId] = pet;
        }
    }
}
