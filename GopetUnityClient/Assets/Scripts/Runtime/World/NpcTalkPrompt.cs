using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Nút nổi "Nói chuyện" world-space, hiện trên đầu NPC khi người chơi đứng gần (xem
    /// <see cref="Gopet.UiLogic.NpcProximity"/>). KHÔNG tái dùng <see cref="ChatBubble"/>:
    /// <c>ChatBubble.AttachOrUpdate</c> tìm component qua <c>GetComponentInChildren</c> trên
    /// cùng transform NPC, nên nếu dùng chung, <see cref="NpcPurposeBubble"/> (chạy định kỳ
    /// mỗi ~11s) sẽ ghi đè nội dung nút bằng lời thoại NPC.
    /// </summary>
    public sealed class NpcTalkPrompt : MonoBehaviour, IPointerClickHandler
    {
        private const string Label = "Nói chuyện";
        private const int PanelWidth = 50;
        private const int PanelHeight = 14;
        private const int Supersampling = 2;
        // Nền xanh đậm + viền xanh sáng, chữ trắng — nổi trên map map sáng lẫn tối,
        // không lẫn với bong bóng thoại (nền trắng viền teal) đang dùng cho hint NPC.
        private static readonly Color32 OutlineColor = new Color32(66, 165, 245, 255);
        private static readonly Color32 FillColor = new Color32(30, 100, 200, 255);
        private static readonly Color TextColor = new Color(1f, 1f, 1f, 1f);
        private static Sprite _cachedSprite;

        private System.Action _onClick;

        /// <summary>Dựng nút làm con của <paramref name="npc"/>, ẩn/hiện qua <see cref="SetVisible"/>.
        /// Destroy theo cha khi NPC bị huỷ (đổi map, NPC rời server) — không cần dọn riêng.
        /// Offset là toạ độ local (x, y) so với NPC (pivot ở chân).</summary>
        public static NpcTalkPrompt Attach(Transform npc, Vector2 offset, System.Action onClick)
        {
            var root = new GameObject("TalkPrompt", typeof(BoxCollider2D));
            root.transform.SetParent(npc, false);
            root.transform.localPosition = new Vector3(offset.x, offset.y, 0f);

            var prompt = root.AddComponent<NpcTalkPrompt>();
            prompt._onClick = onClick;
            prompt.CreatePanel(root.transform);
            prompt.CreateText(root.transform);

            var collider = root.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(PanelWidth, PanelHeight);

            return prompt;
        }

        public void SetVisible(bool value) => gameObject.SetActive(value);

        public void OnPointerClick(PointerEventData eventData) => _onClick?.Invoke();

        private void CreatePanel(Transform parent)
        {
            var go = new GameObject("Panel", typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = 20_050;
            renderer.sprite = PromptSprite();
        }

        private void CreateText(Transform parent)
        {
            var go = new GameObject("Label", typeof(TextMesh));
            go.transform.SetParent(parent, false);
            var mesh = go.GetComponent<TextMesh>();
            mesh.font = UiBuilder.BuiltinFont();
            mesh.fontSize = 32;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontStyle = FontStyle.Bold;
            // characterSize world-space; ChatBubble tham chiếu characterSize 2 ≈ 18px chiều cao.
            // 1.3 ≈ 12px vừa khít khung 14px; bold + fontSize 32 vẫn giữ được độ nét ở kích thước
            // nhỏ, tránh làm panel dài quá cho chữ 10-ký-tự "Nói chuyện".
            mesh.characterSize = 1.3f;
            mesh.color = TextColor;
            mesh.text = Label;

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mesh.font.material;
            renderer.sortingOrder = 20_051;
        }

        private static Sprite PromptSprite()
        {
            if (_cachedSprite != null) return _cachedSprite;

            var texture = new Texture2D(PanelWidth * Supersampling, PanelHeight * Supersampling,
                TextureFormat.RGBA32, false)
            {
                name = "NPC Talk Prompt",
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
                    var outer = InRoundedRect(x, y, 0.5f, 0.5f, PanelWidth - 0.5f, PanelHeight - 0.5f, 6f);
                    var inner = InRoundedRect(x, y, 1.5f, 1.5f, PanelWidth - 1.5f, PanelHeight - 1.5f, 5f);
                    pixels[py * texture.width + px] =
                        inner ? FillColor : outer ? OutlineColor : new Color32(0, 0, 0, 0);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), Supersampling, 0, SpriteMeshType.FullRect);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            _cachedSprite = sprite;
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
    }
}
