using Gopet.UiLogic;
using NUnit.Framework;

namespace Gopet.PlayModeTests
{
    public sealed class GemItemTextTests
    {
        [Test]
        public void Parse_TachTenVaHieuUng()
        {
            var text = GemItemText.Parse("ngọc lửa  up: 0 Tăng 5% (hp) Tăng 5% (mp) Tăng 5% (atk)");
            Assert.AreEqual("Ngọc lửa", text.Name);
            Assert.AreEqual("Up 0  ·  Tăng 5% HP  ·  Tăng 5% MP  ·  Tăng 5% ATK", text.Effects);
        }

        [Test]
        public void Parse_KhongHieuUng_ChiConUp()
        {
            var text = GemItemText.Parse("ngọc lửa lev:2  up: 1");
            Assert.AreEqual("Ngọc lửa lev:2", text.Name);
            Assert.AreEqual("Up 1", text.Effects);
        }

        [Test]
        public void Parse_KhongCoUp_GiuTenBoTag()
        {
            var text = GemItemText.Parse("hồng ngọc (saoden)");
            Assert.AreEqual("Hồng ngọc", text.Name);
            Assert.AreEqual(string.Empty, text.Effects);
        }
    }
}
