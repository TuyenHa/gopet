using System.Collections.Generic;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Keeps server-sent character animations synchronized with avatar lifetime.</summary>
    public sealed class CharacterAnimationLayer
    {
        private readonly MapScene _scene;
        private readonly RemoteAssetCache _assets;
        private readonly Dictionary<int, CharacterAnimationUpdate> _state =
            new Dictionary<int, CharacterAnimationUpdate>();
        private readonly Dictionary<int, GameObject> _roots = new Dictionary<int, GameObject>();

        public CharacterAnimationLayer(MapScene scene, RemoteAssetCache assets,
            CharacterAnimationHandler handler)
        {
            _scene = scene;
            _assets = assets;
            handler.PlayerUpdated += Apply;
            handler.PlayersListed += ApplyList;
            scene.AvatarSpawned += OnAvatarSpawned;
            scene.MapLoaded += Clear;
        }

        private void ApplyList(CharacterAnimationUpdate[] updates)
        {
            Clear();
            foreach (var update in updates) Apply(update);
        }

        private void Apply(CharacterAnimationUpdate update)
        {
            _state[update.UserId] = update;
            Render(update.UserId, _scene.TryGetAvatarTransform(update.UserId));
        }

        private void OnAvatarSpawned(PlayerAvatar avatar)
        {
            if (_state.ContainsKey(avatar.UserId)) Render(avatar.UserId, avatar.transform);
        }

        private void Render(int userId, Transform avatar)
        {
            RemoveRoot(userId);
            if (avatar == null || !_state.TryGetValue(userId, out var update) || update.Animations.Length == 0)
                return;
            var root = new GameObject("Server character animations");
            root.transform.SetParent(avatar, false);
            _roots[userId] = root;
            for (var i = 0; i < update.Animations.Length; i++)
                CharacterAnimationView.Create(root.transform, update.Animations[i], _assets, i);
        }

        private void Clear()
        {
            foreach (var root in _roots.Values)
                if (root != null) MapScene.DestroyWorldObject(root);
            _roots.Clear();
            _state.Clear();
        }

        private void RemoveRoot(int userId)
        {
            if (!_roots.TryGetValue(userId, out var root)) return;
            if (root != null) MapScene.DestroyWorldObject(root);
            _roots.Remove(userId);
        }
    }
}
