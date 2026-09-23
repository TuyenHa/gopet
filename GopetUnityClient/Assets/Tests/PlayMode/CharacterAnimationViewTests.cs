using Gopet.Net.Player;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Danh hiệu xếp trên đầu theo chiều cao THẬT của nhân vật: đầu → tên →
    /// <see cref="PlayerAvatar.TitleGap"/> → danh hiệu (thu nhỏ). Không đặt cứng một độ cao.
    /// </summary>
    public sealed class CharacterAnimationViewTests
    {
        private const float NameHeight = JarNameLabel.Height * 0.75f;
        private PlayerAvatar _avatar;

        [SetUp]
        public void SetUp() => _avatar = PlayerAvatar.Spawn(null, 1, "Self", 0, 100, 100, 480);

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_avatar.gameObject);

        [Test]
        public void DinhDau_DoTheoHinhNhanVat_TenNamSatDau()
        {
            Assert.Greater(_avatar.HeadTopY, 40f, "Chưa đo được chiều cao nhân vật.");
            var name = _avatar.transform.Find("Name");
            Assert.AreEqual(_avatar.HeadTopY, name.localPosition.y, 0.001f, "Tên phải nằm sát đỉnh đầu.");
        }

        [Test]
        public void MocDanhHieu_CachDinhChuTen1px()
        {
            var name = _avatar.transform.Find("Name").GetComponent<JarNameLabel>();
            var nameTop = name.VisibleTopIn(_avatar.transform, float.NaN);

            Assert.AreEqual(1f, PlayerAvatar.TitleGap);
            Assert.AreEqual(nameTop + PlayerAvatar.TitleGap, _avatar.TitleAnchor.localPosition.y, 0.001f);
            // Đỉnh nét chữ thấp hơn đỉnh khung dòng: đo theo khung thì danh hiệu bị đẩy xa.
            Assert.Greater(nameTop, _avatar.HeadTopY, "Tên phải nằm trên đầu.");
            Assert.LessOrEqual(nameTop, _avatar.HeadTopY + NameHeight + 0.001f,
                "Đang đo theo khung dòng chữ thay vì nét chữ.");
        }

        [Test]
        public void DanhHieu_ThuNho_VaCongOffsetServerTheoToaDoJar()
        {
            // Trục y jar hướng xuống: vY dương đẩy danh hiệu xuống dưới.
            var view = CharacterAnimationView.Create(_avatar.TitleAnchor, Title(5, 10), null, 0);

            Assert.AreEqual(5f, view.transform.localPosition.x, 0.001f);
            Assert.AreEqual(-10f, view.transform.localPosition.y, 0.001f);
            Assert.Less(CharacterAnimationView.Scale, 1f, "Danh hiệu phải nhỏ hơn ảnh gốc.");
            Assert.AreEqual(CharacterAnimationView.Scale, view.transform.localScale.x, 0.001f);
        }

        private static CharacterAnimation Title(short x, short y) => new CharacterAnimation
        {
            FrameCount = 2,
            FrameImagePath = "achievement/frame_tanthu.png",
            OffsetX = x,
            OffsetY = y,
        };
    }
}
