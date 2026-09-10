using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Bong bóng chat trên đầu avatar. Tự huỷ sau <see cref="LifetimeSeconds"/>.
    ///
    /// <para><b>Đơn giản có chủ đích</b>: dùng <see cref="TextMesh"/> (legacy, không cần
    /// TextMesh Pro) làm chữ trong world-space. Vertical slice không cần khung nền,
    /// chỉ cần đọc được. Nâng cấp thẩm mỹ (khung, font, wrap) làm sau khi kết nối UI thật.</para>
    /// </summary>
    public sealed class ChatBubble : MonoBehaviour
    {
        /// <summary>Số giây bong bóng hiện trước khi tự huỷ.</summary>
        public const float LifetimeSeconds = 3f;

        /// <summary>Đặt bong bóng bên trên avatar, cách 24 pixel (1 ô).</summary>
        public const float OffsetY = 24f;

        /// <summary>Gắn (hoặc thay) bong bóng cho <paramref name="avatar"/>. Nếu đã có, cập nhật text và reset timer.</summary>
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

            var go = new GameObject("ChatBubble");
            go.transform.SetParent(avatar.transform, false);
            go.transform.localPosition = new Vector3(0f, OffsetY, 0f);

            var bubble = go.AddComponent<ChatBubble>();
            bubble._remaining = LifetimeSeconds;
            bubble._mesh = go.AddComponent<TextMesh>();
            bubble._mesh.anchor = TextAnchor.LowerCenter;
            bubble._mesh.alignment = TextAlignment.Center;
            bubble._mesh.characterSize = 0.5f;
            bubble._mesh.color = Color.white;
            bubble.SetText(text);

            // Trên dải actor (MapPlacement.ActorBaseOrder + Y ≤ ~16135) để bong bóng
            // không bị avatar che.
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 20_000;
        }

        private TextMesh _mesh;
        private float _remaining;

        private void SetText(string text) { if (_mesh != null) _mesh.text = text ?? string.Empty; }

        private void Update()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) Destroy(gameObject);
        }
    }
}
