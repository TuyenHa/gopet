using System.Collections.Generic;
using Gopet.Net.Guild;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Hiển thị tên bang hội dưới tên người chơi. Server gửi
    /// <c>GUILD_NAME_IN_PLACE</c> (PET_SERVICE/91/23) khi player vào map.
    /// </summary>
    public sealed class GuildNameLayer
    {
        private readonly MapScene _scene;
        private readonly Dictionary<int, JarNameLabel> _labels = new Dictionary<int, JarNameLabel>();
        private readonly Dictionary<int, string> _pending = new Dictionary<int, string>();

        private const float NameScale = 0.6f;
        private const float NameOffsetY = 50f;

        public GuildNameLayer(MapScene scene, GuildInfoHandler handler)
        {
            _scene = scene;
            handler.NameInPlaceReceived += OnNameInPlace;
            scene.AvatarSpawned += OnAvatarSpawned;
        }

        private void OnNameInPlace(GuildNameInPlace data)
        {
            foreach (var entry in data.Entries)
            {
                var xform = _scene.TryGetAvatarTransform(entry.UserId);
                if (xform != null)
                {
                    ApplyLabel(entry.UserId, xform, entry.ClanName);
                }
                else
                {
                    _pending[entry.UserId] = entry.ClanName;
                }
            }
        }

        private void OnAvatarSpawned(PlayerAvatar avatar)
        {
            if (_pending.TryGetValue(avatar.UserId, out var name))
            {
                _pending.Remove(avatar.UserId);
                ApplyLabel(avatar.UserId, avatar.transform, name);
            }
        }

        private void ApplyLabel(int userId, Transform parent, string clanName)
        {
            if (_labels.TryGetValue(userId, out var old) && old != null)
                Object.Destroy(old.gameObject);

            if (string.IsNullOrEmpty(clanName))
            {
                _labels.Remove(userId);
                return;
            }

            var label = JarNameLabel.Create(parent,
                new Vector3(0f, NameOffsetY, 0f), NameScale, $"[{clanName}]");
            _labels[userId] = label;
        }

        public void Clear()
        {
            foreach (var kv in _labels)
                if (kv.Value != null) Object.Destroy(kv.Value.gameObject);
            _labels.Clear();
            _pending.Clear();
        }
    }
}
