using System;
using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Quái đi lảng vảng quanh chỗ sinh. Hai thứ phải chắc: không đi lạc khỏi khu của
    /// nó, và không xuyên qua chỗ không đứng được.
    /// </summary>
    public sealed class MobWanderTests
    {
        private const float Step = 1f / 30f;

        private static void Run(MobWander wander, float seconds, Func<int, int, bool> canStand)
        {
            for (var elapsed = 0f; elapsed < seconds; elapsed += Step) wander.Tick(Step, canStand);
        }

        private static float DistanceFrom(MobWander wander, int homeX, int homeY)
        {
            var dx = wander.X - homeX;
            var dy = wander.Y - homeY;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        [Fact]
        public void Wander_RoiChoBanDau()
        {
            var wander = new MobWander(300, 300, seed: 7);

            Run(wander, 30f, (x, y) => true);

            Assert.True(DistanceFrom(wander, 300, 300) > 1f, "Quái vẫn đứng chôn chân.");
        }

        [Fact]
        public void Wander_KhongDiLacKhoiKhuCuaNo()
        {
            var wander = new MobWander(300, 300, seed: 11);

            for (var elapsed = 0f; elapsed < 120f; elapsed += Step)
            {
                wander.Tick(Step, (x, y) => true);
                Assert.True(DistanceFrom(wander, 300, 300) <= MobWander.Radius + 1f,
                    $"Quái đi ra xa {DistanceFrom(wander, 300, 300):F1}px, quá bán kính cho phép.");
            }
        }

        /// <summary>Bản đồ chặn nửa bên phải: quái không được lọt sang đó.</summary>
        [Fact]
        public void Wander_KhongXuyenChoChanDuong()
        {
            var wander = new MobWander(300, 300, seed: 3);

            for (var elapsed = 0f; elapsed < 120f; elapsed += Step)
            {
                wander.Tick(Step, (x, y) => x <= 300);
                Assert.True(wander.X <= 300.5f, $"Quái lọt sang chỗ bị chặn: x={wander.X:F1}.");
            }
        }

        /// <summary>
        /// Bị nhốt kín thì không được rời ô đang đứng. Xê dịch dưới một pixel thì không
        /// tính: <c>MobWanderer</c> làm tròn về pixel trước khi đặt quái nên mắt không thấy.
        /// </summary>
        [Fact]
        public void Wander_BiNhotKin_KhongRoiOHienTai()
        {
            var wander = new MobWander(300, 300, seed: 5);

            Run(wander, 30f, (x, y) => x == 300 && y == 300);

            Assert.Equal(300, (int)MathF.Round(wander.X));
            Assert.Equal(300, (int)MathF.Round(wander.Y));
        }

        /// <summary>Đi một đoạn rồi phải đứng nghỉ, không chạy vòng vòng không ngừng.</summary>
        [Fact]
        public void Wander_CoLucDungNghi()
        {
            var wander = new MobWander(300, 300, seed: 13);
            var resting = 0;

            for (var elapsed = 0f; elapsed < 60f; elapsed += Step)
            {
                wander.Tick(Step, (x, y) => true);
                if (wander.Resting) resting++;
            }

            Assert.True(resting > 0, "Quái không nghỉ lần nào.");
        }
    }
}
