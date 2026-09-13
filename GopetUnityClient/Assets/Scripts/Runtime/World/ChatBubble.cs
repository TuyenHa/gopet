using System.Collections.Generic;
using System.Text;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>Bong bóng chat viền xanh, nền sáng và có đuôi trỏ xuống avatar.</summary>
    public sealed partial class ChatBubble : MonoBehaviour, IPointerClickHandler
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
            AttachOrUpdate(avatar.transform, text, OffsetY, LifetimeSeconds);
        }

        /// <summary>
        /// Hiển thị một bong bóng trên một đối tượng bất kỳ của map. Ngoài nhân vật,
        /// NPC cũng dùng cùng thành phần này để lời giới thiệu có đúng kiểu chat
        /// quen thuộc của game.
        /// </summary>
        public static void AttachOrUpdate(Transform anchor, string text, float offsetY, float lifetime,
            float scale = 1f)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(text)) return;

            var existing = anchor.GetComponentInChildren<ChatBubble>();
            if (existing != null)
            {
                existing.SetText(text);
                existing._remaining = lifetime;
                existing.transform.localScale = Vector3.one * scale;
                return;
            }

            var root = new GameObject("ChatBubble", typeof(BoxCollider2D));
            root.transform.SetParent(anchor, false);
            root.transform.localPosition = new Vector3(0f, offsetY, 0f);
            root.transform.localScale = Vector3.one * scale;

            var bubble = root.AddComponent<ChatBubble>();
            bubble._remaining = lifetime;
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
            var clickArea = GetComponent<BoxCollider2D>();
            if (clickArea != null)
            {
                clickArea.size = new Vector2(boxWidth, boxHeight + TailHeight);
                clickArea.offset = new Vector2(0f, TailHeight * 0.5f);
            }
            if (_textTransform != null)
                _textTransform.localPosition = new Vector3(0f, TailHeight * 0.5f, 0f);
        }

        /// <summary>Bấm vào bong bóng của NPC cũng là bấm vào NPC.</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            var npc = GetComponentInParent<WorldActorView>();
            npc?.OnPointerClick(eventData);
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

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) Destroy(gameObject);
        }
    }
}
