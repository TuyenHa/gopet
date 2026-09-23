using Gopet.Runtime.World;
using NUnit.Framework;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Skin kéo về gần <see cref="CharacterSkinView.StandardHeight"/> nhưng tỉ lệ bị kẹp để
    /// không vỡ pixel art. Số đo lấy từ ảnh anim_characters thật trên server.
    /// </summary>
    public sealed class CharacterSkinScaleTests
    {
        [TestCase(70f, 1f)]        // đúng chuẩn → giữ nguyên
        [TestCase(80f, 0.875f)]    // hơi cao → thu vừa đủ về 70
        [TestCase(50f, 1.15f)]     // 12.png thấp nhất → chỉ phóng tối đa ×1.15
        [TestCase(111f, 0.7f)]     // 40.png → chỉ thu tối đa ×0.7
        [TestCase(154f, 0.7f)]     // -1.png cao nhất → vẫn kẹp ×0.7
        public void Skin_KeoVeChuan_TrongGioiHan(float height, float expected)
        {
            Assert.AreEqual(expected, CharacterSkinView.NormalizedScale(height), 0.001f);
        }

        [Test]
        public void Skin_ChuaCoAnh_GiuNguyen()
        {
            Assert.AreEqual(1f, CharacterSkinView.NormalizedScale(0f));
        }
    }
}
