using System.Collections.Generic;
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

        private const float BaseCharacterSize = 2f;
        private const int TailHeight = 5;
        private const int Supersampling = 2;
        private static readonly Color32 OutlineColor = new Color32(18, 64, 83, 255);
        private static readonly Color32 FillColor = new Color32(239, 250, 255, 255);
        private static readonly Dictionary<long, Sprite> SpriteCache = new Dictionary<long, Sprite>();

        private TextMesh _mesh;
        private Transform _textTransform;
        private SpriteRenderer _panel;
        private float _remaining;
        private float _textScale = 1f;
        private bool _needsFit;

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
        /// <param name="scale">Thu/phóng CẢ bong bóng, chữ lẫn khung.</param>
        /// <param name="textScale">Phóng RIÊNG cỡ chữ; khung tự giãn theo để vẫn ôm sát chữ.
        /// Dùng cho bong bóng NPC: chữ cần to hơn chat người chơi mới đọc được sau khi đã bị
        /// <paramref name="scale"/> thu nhỏ.</param>
        public static void AttachOrUpdate(Transform anchor, string text, float offsetY, float lifetime,
            float scale = 1f, float textScale = 1f)
        {
            if (anchor == null || string.IsNullOrWhiteSpace(text)) return;

            var existing = anchor.GetComponentInChildren<ChatBubble>();
            if (existing != null)
            {
                existing._textScale = textScale;
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
            bubble._textScale = textScale;
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
            // Giá trị thật đặt ở SetText để đường cập-nhật-bong-bóng-sẵn-có cũng đổi theo.
            _mesh.characterSize = BaseCharacterSize;
            _mesh.color = new Color(0.06f, 0.08f, 0.1f, 1f);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = _mesh.font.material;
            renderer.sortingOrder = 20_001;
        }

        /// <summary>Bấm vào bong bóng của NPC cũng là bấm vào NPC.</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            var npc = GetComponentInParent<WorldActorView>();
            npc?.OnPointerClick(eventData);
        }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) Destroy(gameObject);
        }
    }
}
