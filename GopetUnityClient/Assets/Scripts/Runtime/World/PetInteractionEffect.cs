using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Hiện và animate asset kiss/play/poke phía trên avatar.</summary>
    public sealed class PetInteractionEffect : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private float _born;

        public static void Attach(PlayerAvatar avatar, int type)
        {
            if (avatar == null || type < 0 || type > 2) return;
            var names = new[] { "kiss", "play", "poke" };
            var go = new GameObject($"Pet interaction {names[type]}", typeof(SpriteRenderer));
            go.transform.SetParent(avatar.transform, false);
            go.transform.localPosition = new Vector3(0f, 76f, 0f);
            var effect = go.AddComponent<PetInteractionEffect>();
            effect._renderer = go.GetComponent<SpriteRenderer>();
            effect._renderer.sprite = TileAssetProvider.Object($"pet/petInteract/{names[type]}");
            effect._renderer.sortingOrder = 32000;
            effect._born = Time.time;
        }

        private void Update()
        {
            var age = Time.time - _born;
            if (age >= 1.25f) { Destroy(gameObject); return; }
            var pulse = 1f + Mathf.Sin(age * 14f) * 0.08f;
            transform.localScale = new Vector3(pulse, pulse, 1f);
            transform.localPosition += Vector3.up * (12f * Time.deltaTime);
            var color = _renderer.color;
            color.a = Mathf.Clamp01((1.25f - age) * 2f);
            _renderer.color = color;
        }
    }
}
