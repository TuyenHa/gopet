using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Chọn KIỂU diễn cho ảnh ghi đè ở <c>Resources/Battle/fx/&lt;skillId&gt;.png</c>.
    ///
    /// <para>Cùng một cơ chế ghi đè nhưng mỗi hệ kỹ năng cần một nhịp khác nhau: lửa thì
    /// dội xuống rồi bùng thành cột, sét thì giáng tức thì. Để phép chọn ở đây thay vì trong
    /// <c>BattleEffectView</c> để mỗi lớp hiệu ứng không phải biết lớp kia tồn tại.</para></summary>
    public static class BattleSkillFx
    {
        /// <param name="fromWorld">Điểm xuất phát gợi ý, tính theo phía pet ra đòn. Lửa dùng
        /// nó để rơi CHÉO từ hướng kẻ tấn công; sét bỏ qua vì sét giáng thẳng từ trời.</param>
        /// <param name="impactSeconds">Giây tới lúc chạm đích — người gọi chờ rồi mới trừ máu.</param>
        /// <returns>false nếu kỹ năng này không có ảnh ghi đè, khi đó rơi về atlas jar.</returns>
        public static bool TryPlay(Transform parent, RectTransform target, int skillId,
            Vector3? fromWorld, out float impactSeconds)
        {
            return BattleEffectNames.UsesBoltStrike(skillId)
                ? BattleBoltStrikeFx.TryPlay(parent, target, skillId, out impactSeconds)
                : BattleFlameFallFx.TryPlay(parent, target, skillId, fromWorld, out impactSeconds);
        }
    }
}
