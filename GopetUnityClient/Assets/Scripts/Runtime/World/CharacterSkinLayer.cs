using System.Collections.Generic;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;

namespace Gopet.Runtime.World
{
    /// <summary>Retains skin state when SEND_SKIN arrives before its avatar packet.</summary>
    public sealed class CharacterSkinLayer
    {
        private readonly MapScene _scene;
        private readonly RemoteAssetCache _assets;
        private readonly Dictionary<int, string> _paths = new Dictionary<int, string>();

        public CharacterSkinLayer(MapScene scene, RemoteAssetCache assets, CharacterSkinHandler handler)
        {
            _scene = scene;
            _assets = assets;
            handler.SkinsReceived += Apply;
            scene.AvatarSpawned += OnAvatarSpawned;
            scene.MapLoaded += _paths.Clear;
        }

        private void Apply(CharacterSkinUpdate[] updates)
        {
            foreach (var update in updates)
            {
                if (string.IsNullOrEmpty(update.FrameImagePath)) _paths.Remove(update.UserId);
                else _paths[update.UserId] = update.FrameImagePath;
                _scene.ApplySkin(update.UserId, update.FrameImagePath, _assets);
            }
        }

        private void OnAvatarSpawned(PlayerAvatar avatar)
        {
            if (_paths.TryGetValue(avatar.UserId, out var path)) avatar.ApplySkin(path, _assets);
        }
    }
}
