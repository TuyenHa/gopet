using System.Collections.Generic;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Phát atlas hiệu ứng battle theo metadata dy.java gốc của JAR.</summary>
    public sealed class BattleEffectView : MonoBehaviour
    {
        /// <summary>Phóng ở GỐC, không phóng từng mảnh — offset con mới được nhân theo.</summary>
        private const float SpriteScale = BattleSkin.SpriteScale;

        /// <summary>Kéo dài ĐƯỜNG BAY, không kéo giãn nhịp khung hình — hoạt cảnh lặp lại
        /// trong lúc rơi. Kéo giãn khung hình sẽ làm 7 hình trải ra thành giật cục.</summary>
        private const float FallSlowdown = 1.35f;

        private static readonly Dictionary<Texture2D, Dictionary<string, Sprite>> RegionSprites =
            new Dictionary<Texture2D, Dictionary<string, Sprite>>();

        private readonly List<Image> _parts = new List<Image>();
        private JarMapAnimation _animation;
        private JarMapAnimation.Clip _clip;
        private Texture2D _texture;
        private int _sequence;
        private float _next;
        private Vector3 _travelFrom, _travelTo;
        private float _travelStart, _travelSeconds;

        /// <param name="fromWorld">Điểm xuất phát (world); null thì nổ tại chỗ.</param>
        /// <returns>Số giây tới lúc CHẠM ĐÍCH — người gọi chờ rồi mới trừ máu, nếu không pet
        /// gục trước khi ngọn lửa kịp rơi tới.</returns>
        public static float Play(Transform parent, RectTransform target, int skillId,
            Vector3? fromWorld = null)
        {
            // Ảnh ghi đè ở Battle/fx/<skillId> thắng atlas jar — xem BattleSkillFx.
            if (BattleSkillFx.TryPlay(parent, target, skillId, fromWorld, out var impact)) return impact;
            var name = BattleEffectNames.Resolve(skillId);
            if (name == null) return 0f;
            if (BattleEffectNames.IsActorAnimation(skillId))
            {
                return BattleActorEffectView.Play(parent, target, skillId, fromWorld);
            }
            var sprite = JarSkin.Raw($"pet/battle/{(skillId >= 101 ? "skills/" : string.Empty)}{name}");
            var metadataPath = $"Jar/BattleAnimations/{(skillId >= 101 ? "skills/" : string.Empty)}{name}";
            var metadata = Resources.Load<TextAsset>(metadataPath);

            var go = new GameObject($"Hiệu ứng {name}", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).position = target.position;
            go.transform.localScale = Vector3.one * SpriteScale;
            var effect = go.AddComponent<BattleEffectView>();
            effect._texture = sprite.texture;
            if (metadata != null)
            {
                effect._animation = JarMapAnimation.Parse(metadata.bytes);
                if (effect._animation.Clips.Length > 0) effect._clip = effect._animation.Clips[0];
            }
            if (effect._clip == null) effect.ShowFallback(sprite);
            else effect.ShowFrame();
            effect.BeginTravel(fromWorld, target);
            // Thiếu metadata thì rơi về ảnh tĩnh xoay mờ — rất khác bản gốc, dễ tưởng là hỏng.
            if (metadata == null)
            {
                Debug.LogWarning($"[BattleEffect] thiếu metadata {metadataPath} — dùng ảnh tĩnh.");
            }
            return effect._travelSeconds;
        }

        /// <summary>Chuẩn bị đường bay. Thời lượng bám theo hoạt cảnh gốc nhân
        /// <see cref="FallSlowdown"/>, không phải một con số đoán mò.</summary>
        private void BeginTravel(Vector3? fromWorld, RectTransform target)
        {
            if (fromWorld == null || target == null) return;
            _travelFrom = fromWorld.Value;
            _travelTo = target.position;
            _travelStart = Time.unscaledTime;
            _travelSeconds = TotalSeconds() * FallSlowdown;
            if (_travelSeconds <= 0f) return;
            transform.position = _travelFrom;
        }

        private float TotalSeconds()
        {
            if (_clip == null) return 0.65f; // khớp nhánh dự phòng
            var total = 0;
            foreach (var ms in _clip.DurationsMs) total += Mathf.Max(16, ms);
            return total / 1000f;
        }

        private float TravelProgress =>
            _travelSeconds <= 0f ? 1f
                : Mathf.Clamp01((Time.unscaledTime - _travelStart) / _travelSeconds);

        private void TickTravel()
        {
            if (_travelSeconds <= 0f) return;
            // Ease-in (t²) chứ không tuyến tính: vật rơi thì phải nhanh dần, rơi đều trông
            // như bị kéo dây. Cũng cho người chơi kịp thấy nó xuất phát trước khi lao xuống.
            var t = TravelProgress;
            transform.position = Vector3.Lerp(_travelFrom, _travelTo, t * t);
        }

        private void ShowFallback(Sprite sprite)
        {
            var image = NewPart(0);
            image.sprite = sprite;
            image.SetNativeSize();
            // Gốc đã phóng SpriteScale rồi nên mảnh con giữ 1× — trước đây nhân 1.5 ở đây là
            // để bù cho việc gốc không phóng.
            image.rectTransform.localScale = Vector3.one;
            _next = Time.unscaledTime + 0.65f;
        }

        private void Update()
        {
            TickTravel();
            if (_clip == null)
            {
                var group = GetComponent<CanvasGroup>();
                group.alpha = Mathf.Clamp01((_next - Time.unscaledTime) * 3f);
                transform.Rotate(0f, 0f, 180f * Time.unscaledDeltaTime);
                if (Time.unscaledTime >= _next) Destroy(gameObject);
                return;
            }
            if (Time.unscaledTime < _next) return;
            _sequence++;
            if (_sequence >= _clip.FrameIndices.Length)
            {
                // Còn đang rơi thì quay lại đầu hoạt cảnh thay vì biến mất giữa đường.
                if (TravelProgress >= 1f) { Destroy(gameObject); return; }
                _sequence = 0;
            }
            ShowFrame();
        }

        private void ShowFrame()
        {
            if (_clip.FrameIndices.Length == 0) { Destroy(gameObject); return; }
            var frameIndex = _clip.FrameIndices[_sequence];
            if (frameIndex < 0 || frameIndex >= _animation.Frames.Length) return;
            var frame = _animation.Frames[frameIndex];
            for (var i = 0; i < _parts.Count; i++) _parts[i].gameObject.SetActive(i < frame.Parts.Length);
            for (var i = 0; i < frame.Parts.Length; i++)
            {
                var part = frame.Parts[i];
                if (part.RegionIndex < 0 || part.RegionIndex >= _animation.Regions.Length) continue;
                var region = _animation.Regions[part.RegionIndex];
                var image = NewPart(i);
                image.gameObject.SetActive(true);
                image.sprite = RegionSprite(region);
                image.rectTransform.sizeDelta = new Vector2(region.Width, region.Height);
                image.rectTransform.anchoredPosition = new Vector2(part.X + region.Width * 0.5f,
                    -part.Y - region.Height * 0.5f);
                ApplyTransform(image.rectTransform, part.Transform);
            }
            _next = Time.unscaledTime + Mathf.Max(16, _clip.DurationsMs[_sequence]) / 1000f;
        }

        private Sprite RegionSprite(JarMapAnimation.Region region)
        {
            if (!RegionSprites.TryGetValue(_texture, out var sprites))
            {
                sprites = new Dictionary<string, Sprite>();
                RegionSprites[_texture] = sprites;
            }
            var key = $"{region.X}:{region.Y}:{region.Width}:{region.Height}";
            if (sprites.TryGetValue(key, out var sprite) && sprite != null) return sprite;
            sprite = Sprite.Create(_texture,
                new Rect(region.X, _texture.height - region.Y - region.Height, region.Width, region.Height),
                new Vector2(0.5f, 0.5f), 1f);
            sprites[key] = sprite;
            return sprite;
        }

        private Image NewPart(int index)
        {
            while (_parts.Count <= index)
            {
                var child = new GameObject($"Mảnh {_parts.Count}", typeof(RectTransform), typeof(Image));
                child.transform.SetParent(transform, false);
                var image = child.GetComponent<Image>(); image.raycastTarget = false;
                _parts.Add(image);
            }
            return _parts[index];
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
