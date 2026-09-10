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
        private static readonly Dictionary<Texture2D, Dictionary<string, Sprite>> RegionSprites =
            new Dictionary<Texture2D, Dictionary<string, Sprite>>();

        private static readonly string[] EffectNames =
        {
            "SongKich", "Satthuong", "CuongNo", "Bang", "SamSet", "Lua", "hadoc", "daogam",
            "voanh", "fandame", "hutmau", "manaburn", "thiencanthu", "lienhoancuoc", "lachan",
            "Tornado", "Xayda", "MoonShine", "Thorns", "ThorHammer", "Sword", "Shuriken",
            "ZeusWraith", "Meteor"
        };

        private readonly List<Image> _parts = new List<Image>();
        private JarMapAnimation _animation;
        private JarMapAnimation.Clip _clip;
        private Texture2D _texture;
        private int _sequence;
        private float _next;

        public static void Play(Transform parent, RectTransform target, int skillId)
        {
            var name = ResolveName(skillId);
            if (name == null) return;
            if (skillId >= 125)
            {
                BattleActorEffectView.Play(parent, target, skillId);
                return;
            }
            var sprite = JarSkin.Raw($"pet/battle/{(skillId >= 101 ? "skills/" : string.Empty)}{name}");
            var metadataPath = $"Jar/BattleAnimations/{(skillId >= 101 ? "skills/" : string.Empty)}{name}";
            var metadata = Resources.Load<TextAsset>(metadataPath);

            var go = new GameObject($"Hiệu ứng {name}", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).position = target.position;
            var effect = go.AddComponent<BattleEffectView>();
            effect._texture = sprite.texture;
            if (metadata != null)
            {
                effect._animation = JarMapAnimation.Parse(metadata.bytes);
                if (effect._animation.Clips.Length > 0) effect._clip = effect._animation.Clips[0];
            }
            if (effect._clip == null) effect.ShowFallback(sprite);
            else effect.ShowFrame();
        }

        private static string ResolveName(int skillId)
        {
            if (skillId >= 125 && skillId <= 130) return skillId.ToString();
            if (skillId >= 101)
            {
                var index = skillId - 101 + 8; // đúng phép ánh xạ dx/di.java
                if (index >= 0 && index < EffectNames.Length) return EffectNames[index];
            }
            return skillId == 0 || skillId == 2 ? "SlashEffect" : null;
        }

        private void ShowFallback(Sprite sprite)
        {
            var image = NewPart(0);
            image.sprite = sprite;
            image.SetNativeSize();
            image.rectTransform.localScale = Vector3.one * 1.5f;
            _next = Time.unscaledTime + 0.65f;
        }

        private void Update()
        {
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
            if (_sequence >= _clip.FrameIndices.Length) { Destroy(gameObject); return; }
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
