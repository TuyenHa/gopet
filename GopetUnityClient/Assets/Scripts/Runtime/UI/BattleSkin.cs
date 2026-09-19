using UnityEngine;

namespace Gopet.Runtime.UI
{
    public static class BattleSkin
    {
        public static Sprite Load(string key, string fallback = null)
        {
            var sprite = string.IsNullOrWhiteSpace(key) ? null : Resources.Load<Sprite>(key);
            return sprite ?? (string.IsNullOrWhiteSpace(fallback) ? null : Resources.Load<Sprite>(fallback));
        }
    }
}
