using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Phát lại lời giới thiệu của NPC theo chu kỳ. Bong bóng chỉ hiện vài giây
    /// như chat nhân vật để không che map, nhưng người chơi mới vẫn luôn có cơ
    /// hội nhìn thấy khi đứng gần NPC.
    /// </summary>
    internal sealed class NpcPurposeBubble : MonoBehaviour
    {
        private const float ShowForSeconds = 4.5f;
        private const float RepeatAfterSeconds = 11f;
        private const float CompactScale = 0.52f;

        /// <summary>
        /// Phóng riêng cỡ chữ trong bong bóng NPC, KHÔNG phóng cả bong bóng.
        ///
        /// <para>Quy đổi ra "size": chiều cao chữ world-space ≈ <c>characterSize × 9</c> (mốc
        /// lấy từ <see cref="JarNameLabel"/>). Bong bóng dùng <c>characterSize</c> 2 rồi bị
        /// <see cref="CompactScale"/> thu lại, nên chữ cao hiệu dụng ≈ 2 × 0.52 × 9 ≈ 9.4 unit,
        /// tức cỡ 9. Lên cỡ 11 (+2 size) thì nhân 11/9.4 ≈ 1.17.</para>
        ///
        /// <para>Dừng ở +2 chứ không +3: khung ôm sát chữ nên nó nở theo cỡ chữ: +2 vẫn còn hẹp
        /// hơn bản cũ ~11%, +3 là khung to lại bằng cũ, mất hết phần vừa tiết kiệm được.</para>
        /// </summary>
        private const float TextScale = 1.17f;

        private WorldActorView _actor;
        private string _hint;
        private float _showAt;

        public void Configure(WorldActorView actor, string hint)
        {
            _actor = actor;
            _hint = hint;
            // Lệch thời điểm theo instance để các NPC không nói cùng một lúc.
            _showAt = Time.time + Random.Range(0.5f, 2.5f);
        }

        private void Update()
        {
            if (_actor == null || string.IsNullOrWhiteSpace(_hint) || Time.time < _showAt) return;
            ChatBubble.AttachOrUpdate(transform, _hint, _actor.PurposeBubbleOffsetY,
                ShowForSeconds, CompactScale, TextScale);
            _showAt = Time.time + RepeatAfterSeconds + Random.Range(-1.5f, 1.5f);
        }
    }
}
