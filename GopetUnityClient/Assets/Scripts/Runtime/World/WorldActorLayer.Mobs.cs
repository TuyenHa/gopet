using System;
using System.Collections.Generic;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Chọn con quái mà nút đánh sẽ nhắm tới: gần người chơi nhất và trong tầm với.
    ///
    /// <para>Server KHÔNG kiểm tra khoảng cách (<c>GopetPlace.startFightMob</c> nhận mọi
    /// mobId của map), nên tầm với là luật của client — đứng giữa map mà bấm một phát
    /// đánh được con quái tận góc bên kia thì nút này thành auto-đánh.</para>
    /// </summary>
    public sealed partial class WorldActorLayer
    {
        /// <summary>
        /// Bán kính BẮT quái, tính bằng pixel jar. Rộng hơn tầm nói chuyện với NPC
        /// (60px) vì quái di chuyển liên tục — tầm quá hẹp thì vừa bấm quái đã trôi ra
        /// khỏi vùng và nút tắt ngay dưới ngón tay.
        /// </summary>
        private const float MobReachRadius = 96f;

        /// <summary>Bán kính NHẢ quái đang nhắm; rộng hơn để nút không nhấp nháy ở biên.</summary>
        private const float MobReleaseRadius = 120f;

        /// <summary>Quái vẽ với pivot ở chân, đẩy điểm tham chiếu lên giữa thân như NPC.</summary>
        private const float MobBodyCenterOffset = 24f;

        private readonly List<NpcPoint> _mobPoints = new List<NpcPoint>();

        /// <summary>Quái nút đánh đang nhắm, hoặc <see cref="NpcProximity.None"/>.</summary>
        public int NearestMobId { get; private set; } = NpcProximity.None;

        /// <summary>Đổi mục tiêu — HUD dùng để bật/mờ nút đánh.</summary>
        public event Action<int> NearestMobChanged;

        private void ScanMobs(Vector3 selfPos)
        {
            _mobPoints.Clear();
            foreach (var pair in _mobs)
            {
                if (pair.Value == null) continue;
                var pos = pair.Value.transform.localPosition;
                _mobPoints.Add(new NpcPoint(pair.Key, pos.x, pos.y + MobBodyCenterOffset));
            }

            SetMobTarget(NpcProximity.Pick(selfPos.x, selfPos.y + NpcBodyCenterOffset,
                _mobPoints, NearestMobId, MobReachRadius, MobReleaseRadius));
        }

        private void ClearMobTarget() => SetMobTarget(NpcProximity.None);

        private void SetMobTarget(int mobId)
        {
            if (mobId == NearestMobId) return;
            NearestMobId = mobId;
            NearestMobChanged?.Invoke(mobId);
        }
    }
}
