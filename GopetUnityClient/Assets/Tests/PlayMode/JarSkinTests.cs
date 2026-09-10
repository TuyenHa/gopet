using System;
using Gopet.Runtime.UI;
using NUnit.Framework;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nạp sprite từ <c>Resources/Jar/Art</c> theo tên. Cần asset thật đã giải bằng
    /// <c>tools/unpack-jar-dat</c> — chạy nó trước khi chạy PlayMode test.
    /// </summary>
    public sealed class JarSkinTests
    {
        /// <summary><c>lg.dat</c> chỉ có 1 ảnh thật (banner màn đăng nhập, 173×92) — xem README của unpack-jar-dat.</summary>
        [Test]
        public void Bank_NapDungAnhDaGiaiTuDat()
        {
            var sprite = JarSkin.Bank("lg", 0);

            Assert.IsNotNull(sprite);
            Assert.AreEqual(173, sprite.texture.width);
            Assert.AreEqual(92, sprite.texture.height);
        }

        /// <summary>Logo splash — PNG rời, không qua kho <c>.dat</c>.</summary>
        [Test]
        public void Raw_NapDungAnhRoi()
        {
            var sprite = JarSkin.Raw("meLogo");

            Assert.IsNotNull(sprite);
            Assert.AreEqual(260, sprite.texture.width);
            Assert.AreEqual(72, sprite.texture.height);
        }

        /// <summary>Ảnh rời nằm trong thư mục con phải giữ đúng cấu trúc thư mục gốc của jar.</summary>
        [Test]
        public void Raw_DuongDanCoThuMucCon()
        {
            var sprite = JarSkin.Raw("pet/button/heal");

            Assert.IsNotNull(sprite);
        }

        [Test]
        public void GoiHaiLan_TraCungMotInstance()
        {
            var first = JarSkin.Bank("common", 0);
            var second = JarSkin.Bank("common", 0);

            Assert.AreSame(first, second, "Không cache — mỗi lần gọi lại nạp lại từ đĩa.");
        }

        [Test]
        public void TenSai_ThiNemChuKhongTraNull()
        {
            Assert.Throws<InvalidOperationException>(() => JarSkin.Bank("khong-ton-tai", 0));
            Assert.Throws<InvalidOperationException>(() => JarSkin.Raw("khong/ton/tai"));
        }

        /// <summary><c>lg.dat</c> chỉ có 1 ảnh thật (index 0) — index 1 phải KHÔNG tồn tại (đó là sentinel, không phải ảnh).</summary>
        [Test]
        public void BankIndexNgoaiSo_ThiNem()
        {
            Assert.Throws<InvalidOperationException>(() => JarSkin.Bank("lg", 1));
        }
    }
}
