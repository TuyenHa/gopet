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
                ShowForSeconds, CompactScale);
            _showAt = Time.time + RepeatAfterSeconds + Random.Range(-1.5f, 1.5f);
        }
    }
}
