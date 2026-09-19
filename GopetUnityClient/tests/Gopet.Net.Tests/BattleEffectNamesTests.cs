using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Khoá chặt ánh xạ skillID → tên file hiệu ứng. Đã sai một lần (cộng dư 8),
    /// hậu quả: kỹ năng 101..116 phát nhầm hiệu ứng, 117..124 mất hẳn hiệu ứng.</summary>
    public sealed class BattleEffectNamesTests
    {
        /// <summary>Đối chiếu với bảng `skill` trong DB (30 dòng, ID 101..130).</summary>
        [Theory]
        [InlineData(101, "SongKich")]      // song kích
        [InlineData(102, "Satthuong")]     // giảm sát thương
        [InlineData(103, "CuongNo")]       // cuồng nộ
        [InlineData(104, "Bang")]          // băng
        [InlineData(105, "SamSet")]        // sấm sét
        [InlineData(106, "Lua")]           // lửa
        [InlineData(107, "hadoc")]         // hạ độc
        [InlineData(109, "voanh")]         // vô ảnh
        [InlineData(110, "fandame")]       // phản đòn
        [InlineData(111, "hutmau")]        // hút máu
        [InlineData(123, "fandame")]       // Phản Nghịch — cố ý dùng chung với 110, xem Overrides
        [InlineData(124, "Meteor")]        // kỹ năng atlas cuối cùng
        public void SkillIdAnhXaDungTenHieuUng(int skillId, string expected) =>
            Assert.Equal(expected, BattleEffectNames.Resolve(skillId));

        [Fact]
        public void KhongSkillIdAtlasNaoBiLotNull()
        {
            // Chốt chặn lệch chỉ số: 101..124 đều phải ra một tên nào đó.
            for (var id = 101; id <= 124; id++)
            {
                Assert.False(string.IsNullOrEmpty(BattleEffectNames.Resolve(id)),
                    $"skillId {id} không ra tên hiệu ứng");
            }
        }

        [Fact]
        public void BangAtlasKhongTrungKhongRong()
        {
            // Bất biến của RIÊNG mảng atlas — bản sao dx.java thì 24 ô phải khác nhau.
            // Không kiểm qua Resolve() nữa: Overrides cố tình cho 2 skill dùng chung 1 hiệu ứng.
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var name in BattleEffectNames.Atlas)
            {
                Assert.False(string.IsNullOrEmpty(name));
                Assert.True(seen.Add(name), $"atlas trùng tên {name}");
            }
            Assert.Equal(24, seen.Count);
        }

        [Fact]
        public void OverrideThangAtlasTheoChiSo()
        {
            // 123 vẫn nằm trong dải atlas, nhưng Overrides phải được tra TRƯỚC.
            Assert.Equal("ZeusWraith", BattleEffectNames.Atlas[123 - BattleEffectNames.FirstSkillId]);
            Assert.Equal("fandame", BattleEffectNames.Resolve(123));
        }

        [Theory]
        [InlineData(125)]
        [InlineData(130)]
        public void KyNang125Den130DungDuongAnu(int skillId)
        {
            Assert.True(BattleEffectNames.IsActorAnimation(skillId));
            Assert.Equal(skillId.ToString(), BattleEffectNames.Resolve(skillId));
        }

        [Fact]
        public void DonThuongVaChiMangDungSpriteChem()
        {
            Assert.Equal("SlashEffect", BattleEffectNames.Resolve(0)); // SKILL_NORMAL
            Assert.Equal("SlashEffect", BattleEffectNames.Resolve(2)); // SKILL_CRIT
        }

        [Fact]
        public void DonTruotKhongVeGiCa()
        {
            // SKILL_MISS = 1: chỉ hiện chữ "TRƯỢT", không có sprite hiệu ứng.
            Assert.Null(BattleEffectNames.Resolve(1));
        }

        /// <summary>105 "sấm sét" diễn bằng sét giáng, không phải mưa nhiều ngọn như lửa.</summary>
        [Theory]
        [InlineData(105, true)]    // sấm sét
        [InlineData(106, false)]   // lửa — mưa lửa
        [InlineData(101, false)]
        [InlineData(0, false)]     // đòn thường
        public void ChiSamSetDungKieuSetGiang(int skillId, bool expected)
        {
            Assert.Equal(expected, BattleEffectNames.UsesBoltStrike(skillId));
        }

        [Fact]
        public void SkillIdNgoaiDaiThiKhongVe()
        {
            Assert.Null(BattleEffectNames.Resolve(131));
            Assert.Null(BattleEffectNames.Resolve(999));
        }
    }
}
