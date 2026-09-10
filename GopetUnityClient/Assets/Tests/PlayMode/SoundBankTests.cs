using System;
using Gopet.Runtime.Audio;
using NUnit.Framework;

namespace Gopet.PlayModeTests
{
    /// <summary>Nạp <c>AudioClip</c> từ <c>Resources/Jar/Audio</c> theo tên. Cần asset thật đã giải bằng <c>tools/unpack-jar-dat</c>.</summary>
    public sealed class SoundBankTests
    {
        [Test]
        public void Get_NapDungClip()
        {
            var clip = SoundBank.Get("s_login");

            Assert.IsNotNull(clip);
        }

        [Test]
        public void GoiHaiLan_TraCungMotInstance()
        {
            var first = SoundBank.Get("s_button");
            var second = SoundBank.Get("s_button");

            Assert.AreSame(first, second, "Không cache — mỗi lần gọi lại nạp lại từ đĩa.");
        }

        [Test]
        public void TenSai_ThiNemChuKhongTraNull()
        {
            Assert.Throws<InvalidOperationException>(() => SoundBank.Get("khong-ton-tai"));
        }
    }
}
