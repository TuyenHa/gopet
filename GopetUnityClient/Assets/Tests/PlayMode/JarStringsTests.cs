using System;
using Gopet.Runtime.UI;
using NUnit.Framework;

namespace Gopet.PlayModeTests
{
    /// <summary>Nạp chuỗi từ <c>Resources/Jar/Strings</c> theo chỉ số. Cần asset thật đã bóc bằng <c>tools/extract-jar-strings</c>.</summary>
    public sealed class JarStringsTests
    {
        [Test]
        public void Vi_CacChiSoDaBiet_RaDungChu()
        {
            Assert.AreEqual("T.Khoản", JarStrings.Vi(5));
            Assert.AreEqual("M.Kh:", JarStrings.Vi(348));
            Assert.AreEqual("Đăng nhập", JarStrings.Vi(266));
            Assert.AreEqual("Nếu chưa có tên, xin đăng ký", JarStrings.Vi(331));
        }

        [Test]
        public void En_CoSan_ChoNguoiDungCuoiSauNay()
        {
            Assert.AreEqual("Pass:", JarStrings.En(348));
        }

        [Test]
        public void ChiSoSai_ThiNemChuKhongTraRong()
        {
            Assert.Throws<InvalidOperationException>(() => JarStrings.Vi(999999));
        }
    }
}
