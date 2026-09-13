using System.Collections.Generic;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;

namespace Gopet.Runtime.World
{
    public sealed class CharacterWingLayer
    {
        private readonly MapScene _scene;
        private readonly RemoteAssetCache _assets;
        private readonly Dictionary<int, WingUpdate> _state = new Dictionary<int, WingUpdate>();

        public CharacterWingLayer(MapScene scene, RemoteAssetCache assets, WingHandler handler)
        {
            _scene = scene;
            _assets = assets;
            handler.WingsReceived += Apply;
            scene.AvatarSpawned += OnAvatarSpawned;
            scene.MapLoaded += _state.Clear;
        }

        private void Apply(WingUpdate[] updates)
        {
            foreach (var update in updates)
            {
                if (string.IsNullOrEmpty(update.FrameImagePath)) _state.Remove(update.UserId);
                else _state[update.UserId] = update;
                _scene.ApplyWing(update.UserId, update.FrameImagePath, update.VerticalOffset, _assets);
            }
        }

        private void OnAvatarSpawned(PlayerAvatar avatar)
        {
            if (_state.TryGetValue(avatar.UserId, out var update))
                avatar.ApplyWing(update.FrameImagePath, update.VerticalOffset, _assets);
        }
    }
}
