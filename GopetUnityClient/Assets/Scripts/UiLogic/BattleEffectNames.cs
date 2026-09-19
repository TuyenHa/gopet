using System.Collections.Generic;

namespace Gopet.UiLogic
{
    /// <summary>Ánh xạ <c>TurnEffect.skillId</c> sang tên file hiệu ứng.
    ///
    /// <para>Trường này mang HAI nghĩa tuỳ ngữ cảnh: với đòn thường nó là marker
    /// (<c>SKILL_NORMAL=0</c>, <c>SKILL_MISS=1</c>, <c>SKILL_CRIT=2</c>), với kỹ năng nó là
    /// skillID thật trong bảng <c>skill</c> của DB — 30 dòng, ID <b>101..130</b>.</para>
    ///
    /// <para>Thuần C#, không phụ thuộc Unity, để test được ở <c>Gopet.Net.Tests</c>.</para></summary>
    public static class BattleEffectNames
    {
        /// <summary>Khớp 1-1 với skillID 101..124 theo đúng thứ tự trong DB — chép nguyên
        /// bảng tra của jar (<c>dx.java</c> case 0..23). Giữ nguyên trạng để còn đối chiếu
        /// ngược; muốn đổi hiệu ứng cho một kỹ năng thì thêm vào <see cref="Overrides"/>.</summary>
        public static readonly string[] Atlas =
        {
            "SongKich", "Satthuong", "CuongNo", "Bang", "SamSet", "Lua", "hadoc", "daogam",
            "voanh", "fandame", "hutmau", "manaburn", "thiencanthu", "lienhoancuoc", "lachan",
            "Tornado", "Xayda", "MoonShine", "Thorns", "ThorHammer", "Sword", "Shuriken",
            "ZeusWraith", "Meteor"
        };

        /// <summary>Sai khác CỐ Ý so với jar, tra TRƯỚC <see cref="Atlas"/>.
        ///
        /// <para>Jar cho 123 "Phản Nghịch" dùng <c>ZeusWraith</c> — sét của thần Zeus, chẳng
        /// liên quan gì tới phản đòn. Dùng chung <c>fandame</c> với 110 "phản đòn" cho khớp
        /// nghĩa; hai kỹ năng cùng cơ chế thì nên trông giống nhau.</para>
        ///
        /// <para>Để riêng thay vì sửa thẳng vào <see cref="Atlas"/>: mảng kia là bản sao
        /// nguyên trạng của <c>dx.java</c>, sửa vào đó thì sau này không ai đối chiếu ngược
        /// với jar được nữa.</para></summary>
        public static readonly Dictionary<int, string> Overrides = new Dictionary<int, string>
        {
            { 123, "fandame" },
        };

        /// <summary>105 "sấm sét" trong bảng <c>skill</c>.</summary>
        public const int ThunderSkillId = 105;

        /// <summary>Kỹ năng diễn bằng "sét giáng" — một tia lớn đánh thẳng xuống rồi giật tắt —
        /// thay vì "mưa" nhiều ngọn. Chỉ áp dụng khi kỹ năng CÓ ảnh ghi đè ở
        /// <c>Battle/fx/&lt;skillId&gt;.png</c>; không có ảnh thì vẫn rơi về atlas jar.</summary>
        public static bool UsesBoltStrike(int skillId) => skillId == ThunderSkillId;

        public const int FirstSkillId = 101;
        public const int FirstActorSkillId = 125;
        public const int LastActorSkillId = 130;

        /// <summary>Kỹ năng 125..130 dùng ActorFactory <c>.anu</c> thay vì atlas.</summary>
        public static bool IsActorAnimation(int skillId) =>
            skillId >= FirstActorSkillId && skillId <= LastActorSkillId;

        /// <summary>Tên file hiệu ứng, hoặc null nếu không có gì để vẽ.
        ///
        /// <para>KHÔNG cộng thêm 8. Phép <c>type-101+8</c> của jar (<c>di.java</c>) là index vào
        /// bảng FX của <c>bd.java</c>, KHÔNG phải index vào danh sách tên này. Cộng 8 làm
        /// 101..116 ra sai hiệu ứng và 117..124 vượt mảng ⇒ mất hẳn hiệu ứng.</para></summary>
        public static string Resolve(int skillId)
        {
            if (IsActorAnimation(skillId)) return skillId.ToString();
            if (skillId >= FirstSkillId)
            {
                if (Overrides.TryGetValue(skillId, out var custom)) return custom;
                var index = skillId - FirstSkillId;
                return index < Atlas.Length ? Atlas[index] : null;
            }
            // Đòn thường và chí mạng dùng chung sprite chém; trượt (1) không vẽ gì.
            return skillId == 0 || skillId == 2 ? "SlashEffect" : null;
        }
    }
}
