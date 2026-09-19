using System.Collections.Generic;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Renderer cho ActorFactory `.anu` của skill 125..130.</summary>
    public sealed class BattleActorEffectView : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private readonly List<Image> _parts = new List<Image>();
        private JarActorAnimation _animation;
        private Texture2D _texture;
        private int _step, _last;
        private float _next;
        private Vector3 _travelDelta;
        private Vector2 _travelOrigin, _stepOffset;
        private float _travelStart, _travelSeconds;

        /// <returns>Số giây tới lúc chạm đích — xem BattleEffectView.Play.</returns>
        public static float Play(Transform parent, RectTransform target, int skillId,
            Vector3? fromWorld = null)
        {
            var metadata = Resources.Load<TextAsset>($"Jar/BattleAnimations/skills/{skillId}.anu");
            if (metadata == null) return 0f;
            var source = JarSkin.Raw($"pet/battle/skills/{skillId}");
            var go = new GameObject($"Hiệu ứng ANU {skillId}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).position = target.position;
            // Cùng hệ số với sprite pet — xem BattleSkin.SpriteScale.
            go.transform.localScale = Vector3.one * BattleSkin.SpriteScale;
            var view = go.AddComponent<BattleActorEffectView>();
            view._animation = JarActorAnimation.Parse(metadata.bytes);
            view._texture = source.texture;
            if (view._animation.Clips.Length == 0) { Destroy(go); return 0f; }
            view._step = view._animation.Clips[0].First;
            view._last = view._animation.Clips[0].Last;
            view.ShowStep();
            view.BeginTravel(fromWorld, target);
            return view._travelSeconds;
        }

        /// <summary>Đường bay từ pet ra pet — xem BattleEffectView.BeginTravel. Ở đây cộng
        /// DỒN vào anchoredPosition vì hoạt cảnh .anu tự dịch chuyển qua step.DeltaX/Y,
        /// nên phải giữ phần dịch đó thay vì ghi đè vị trí.</summary>
        private void BeginTravel(Vector3? fromWorld, RectTransform target)
        {
            if (fromWorld == null || target == null) return;
            _travelDelta = target.position - fromWorld.Value;
            _travelStart = Time.unscaledTime;
            _travelSeconds = 0.5f;
            transform.position = fromWorld.Value;
            _travelOrigin = ((RectTransform)transform).anchoredPosition;
        }

        private void Update()
        {
            if (_travelSeconds > 0f)
            {
                var t = Mathf.Clamp01((Time.unscaledTime - _travelStart) / _travelSeconds);
                var rect = (RectTransform)transform;
                rect.anchoredPosition = _travelOrigin + (Vector2)_travelDelta * t + _stepOffset;
            }
            if (Time.unscaledTime < _next) return;
            _step++;
            if (_step > _last) { Destroy(gameObject); return; }
            ShowStep();
        }

        private void ShowStep()
        {
            var step = _animation.Steps[_step];
            var frame = _animation.Frames[step.Frame];
            for (var i = 0; i < _parts.Count; i++) _parts[i].gameObject.SetActive(i < frame.Parts.Length);
            for (var i = 0; i < frame.Parts.Length; i++)
            {
                var part = frame.Parts[i];
                var region = _animation.Regions[part.Region];
                var image = Part(i); image.gameObject.SetActive(true);
                image.sprite = RegionSprite(part.Region, region);
                image.rectTransform.sizeDelta = new Vector2(region.Width, region.Height);
                image.rectTransform.anchoredPosition = new Vector2(part.X + region.Width * 0.5f,
                    -part.Y - region.Height * 0.5f);
                ApplyTransform(image.rectTransform, part.Transform);
            }
            // Hoạt cảnh .anu tự dịch mỗi step; dồn vào _stepOffset để đường bay ở Update
            // không ghi đè mất phần dịch này.
            _stepOffset += new Vector2(step.DeltaX, -step.DeltaY);
            if (_travelSeconds <= 0f)
            {
                ((RectTransform)transform).anchoredPosition += new Vector2(step.DeltaX, -step.DeltaY);
            }
            _next = Time.unscaledTime + Mathf.Max(1, step.DurationTicks) * 0.04f;
        }

        private Image Part(int index)
        {
            while (_parts.Count <= index)
            {
                var go = new GameObject($"Mảnh {_parts.Count}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var image = go.GetComponent<Image>(); image.raycastTarget = false;
                _parts.Add(image);
            }
            return _parts[index];
        }

        private Sprite RegionSprite(int index, JarActorAnimation.Region region)
        {
            var key = $"{_texture.GetHashCode()}|{index}";
            if (SpriteCache.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            sprite = Sprite.Create(_texture,
                new Rect(region.X, _texture.height - region.Y - region.Height, region.Width, region.Height),
                new Vector2(0.5f, 0.5f), 1f);
            return SpriteCache[key] = sprite;
        }

        private static void ApplyTransform(RectTransform rect, int transform)
        {
            var mirror = transform == 1 || transform == 2 || transform == 4 || transform == 7;
            rect.localScale = mirror ? new Vector3(-1f, 1f, 1f) : Vector3.one;
            var angle = transform == 4 || transform == 6 ? 90f :
                transform == 5 || transform == 7 ? -90f : transform == 1 || transform == 3 ? 180f : 0f;
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
