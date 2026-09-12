using System.Collections.Generic;
using System.Text;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Bong bóng chat viền xanh, nền sáng và có đuôi trỏ xuống avatar.</summary>
    public sealed class ChatBubble : MonoBehaviour
    {
        public const float LifetimeSeconds = 3f;
        public const float OffsetY = 84f;

        private const int MaxCharactersPerLine = 26;
        private const int MaxLines = 3;
        private const int TailHeight = 5;
        private const int Supersampling = 2;
        private static readonly Color32 OutlineColor = new Color32(18, 64, 83, 255);
        private static readonly Color32 FillColor = new Color32(239, 250, 255, 255);
        private static readonly Dictionary<long, Sprite> SpriteCache = new Dictionary<long, Sprite>();

        private TextMesh _mesh;
        private Transform _textTransform;
        private SpriteRenderer _panel;
        private float _remaining;

        public static void AttachOrUpdate(PlayerAvatar avatar, string text)
        {
            if (avatar == null) return;

            var existing = avatar.GetComponentInChildren<ChatBubble>();
            if (existing != null)
            {
                existing.SetText(text);
                existing._remaining = LifetimeSeconds;
                return;
            }

            var root = new GameObject("ChatBubble");
            root.transform.SetParent(avatar.transform, false);
            root.transform.localPosition = new Vector3(0f, OffsetY, 0f);

            var bubble = root.AddComponent<ChatBubble>();
            bubble._remaining = LifetimeSeconds;
            bubble.CreatePanel(root.transform);
            bubble.CreateText(root.transform);
            bubble.SetText(text);
        }

        private void CreatePanel(Transform parent)
        {
            var go = new GameObject("Bubble", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, 0.01f);
            _panel = go.GetComponent<SpriteRenderer>();
            _panel.sortingOrder = 20_000;
        }

        private void CreateText(Transform parent)
        {
            var go = new GameObject("Message", typeof(TextMesh));
            go.transform.SetParent(parent, false);
            _textTransform = go.transform;
            _mesh = go.GetComponent<TextMesh>();
            _mesh.font = UiBuilder.BuiltinFont();
            _mesh.fontSize = 32;
            _mesh.anchor = TextAnchor.MiddleCenter;
            _mesh.alignment = TextAlignment.Center;
            // TextMesh.fontSize chủ yếu đổi độ nét atlas; characterSize mới quyết định
            // kích thước chữ thực trong world-space. 0.7 khiến chữ chỉ còn vài pixel.
            _mesh.characterSize = 2f;
            _mesh.color = new Color(0.06f, 0.08f, 0.1f, 1f);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = _mesh.font.material;
            renderer.sortingOrder = 20_001;
        }

        private void SetText(string text)
        {
            var formatted = Wrap(text ?? string.Empty, out var longestLine, out var lineCount);
            if (_mesh != null) _mesh.text = formatted;

            // Ôm sát nội dung: câu ngắn có bong bóng nhỏ, câu dài mới nới rộng.
            // Khung ôm sát số ký tự: tin ngắn nhỏ, tin dài mới nới rộng.
            var boxWidth = Mathf.Clamp(8f + longestLine * 6f, 26f, 164f);
            var boxHeight = 18 + (lineCount - 1) * 14;
            if (_panel != null) _panel.sprite = BubbleSprite(Mathf.RoundToInt(boxWidth), boxHeight);
            if (_textTransform != null)
                _textTransform.localPosition = new Vector3(0f, TailHeight * 0.5f, 0f);
        }

        private static string Wrap(string text, out int longestLine, out int lineCount)
        {
            var words = text.Trim().Split(' ');
            var lines = new List<string>(MaxLines);
            var current = new StringBuilder();

            foreach (var rawWord in words)
            {
                var word = rawWord;
                while (word.Length > MaxCharactersPerLine)
                {
                    if (current.Length > 0)
                    {
                        lines.Add(current.ToString());
                        current.Length = 0;
                        if (lines.Count == MaxLines) break;
                    }
                    lines.Add(word.Substring(0, MaxCharactersPerLine));
                    word = word.Substring(MaxCharactersPerLine);
                    if (lines.Count == MaxLines) break;
                }
                if (lines.Count == MaxLines) break;
                if (word.Length == 0) continue;

                if (current.Length > 0 && current.Length + 1 + word.Length > MaxCharactersPerLine)
                {
                    lines.Add(current.ToString());
                    current.Length = 0;
                    if (lines.Count == MaxLines) break;
                }
                if (current.Length > 0) current.Append(' ');
                current.Append(word);
            }
            if (current.Length > 0 && lines.Count < MaxLines) lines.Add(current.ToString());
            if (lines.Count == 0) lines.Add(string.Empty);

            longestLine = 0;
            foreach (var line in lines) longestLine = Mathf.Max(longestLine, line.Length);
            lineCount = lines.Count;
            return string.Join("\n", lines);
        }

        private static Sprite BubbleSprite(int width, int boxHeight)
        {
            var key = ((long)width << 32) | (uint)boxHeight;
            if (SpriteCache.TryGetValue(key, out var cached) && cached != null) return cached;

            var logicalHeight = boxHeight + TailHeight;
            var texture = new Texture2D(width * Supersampling, logicalHeight * Supersampling,
                TextureFormat.RGBA32, false)
            {
                name = $"Chat Bubble {width}x{logicalHeight}",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[texture.width * texture.height];
            for (var py = 0; py < texture.height; py++)
            {
                for (var px = 0; px < texture.width; px++)
                {
                    var x = (px + 0.5f) / Supersampling;
                    var y = (py + 0.5f) / Supersampling;
                    var outer = InRoundedRect(x, y, 0.5f, TailHeight, width - 0.5f,
                                    logicalHeight - 0.5f, 6f) ||
                                InTail(x, y, width * 0.5f, 4.5f, 0.3f, TailHeight + 1.5f);
                    // Lớp trong chỉ lùi 1px so với viền để đường viền thanh hơn.
                    var inner = InRoundedRect(x, y, 1.5f, TailHeight + 1f, width - 1.5f,
                                    logicalHeight - 1.5f, 5f) ||
                                InTail(x, y, width * 0.5f, 3.2f, 1f, TailHeight + 1f);
                    pixels[py * texture.width + px] = inner
                        ? FillColor
                        : outer ? OutlineColor : new Color32(0, 0, 0, 0);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), Supersampling, 0, SpriteMeshType.FullRect);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            SpriteCache[key] = sprite;
            return sprite;
        }

        private static bool InRoundedRect(float x, float y, float left, float bottom,
            float right, float top, float radius)
        {
            if (x < left || x > right || y < bottom || y > top) return false;
            var centerX = Mathf.Clamp(x, left + radius, right - radius);
            var centerY = Mathf.Clamp(y, bottom + radius, top - radius);
            var dx = x - centerX;
            var dy = y - centerY;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static bool InTail(float x, float y, float centerX, float halfBase,
            float tipY, float topY)
        {
            if (y < tipY || y > topY) return false;
            var progress = (y - tipY) / (topY - tipY);
            return Mathf.Abs(x - centerX) <= Mathf.Lerp(0.6f, halfBase, progress);
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) Destroy(gameObject);
        }
    }
}
