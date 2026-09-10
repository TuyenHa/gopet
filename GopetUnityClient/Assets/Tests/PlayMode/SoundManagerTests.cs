using Gopet.Runtime.Audio;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Nhạc nền + hiệu ứng + nhớ trạng thái bật/tắt. <c>PlayerPrefs</c> sống trên đĩa
    /// giữa các lần chạy test, nên phải dọn key trước và sau mỗi ca để không ca nào
    /// ăn theo trạng thái ca trước.
    /// </summary>
    public sealed class SoundManagerTests
    {
        private const string PrefsKey = "Gopet.SoundEnabled";

        private GameObject _host;
        private SoundManager _sound;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
            _host = new GameObject("SoundHost");
            _sound = SoundManager.Create(_host.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
            PlayerPrefs.DeleteKey(PrefsKey);
        }

        [Test]
        public void MacDinh_Bat()
        {
            Assert.IsTrue(_sound.Enabled, "Người chơi tắt lần trước mới nhớ là tắt, không phải ngược lại.");
        }

        [Test]
        public void Tat_ThiNhoQuaLanTao()
        {
            _sound.SetEnabled(false);

            var reloaded = SoundManager.Create(new GameObject("Reload").transform);

            Assert.IsFalse(reloaded.Enabled);
        }

        [Test]
        public void PlayMusic_KhiDangTat_KhongPhat()
        {
            _sound.SetEnabled(false);

            _sound.PlayMusic("s_login");

            Assert.IsFalse(GetMusicSource(_sound).isPlaying);
        }

        [Test]
        public void PlayMusic_KhiDangBat_Phat()
        {
            _sound.PlayMusic("s_login");

            Assert.IsTrue(GetMusicSource(_sound).isPlaying);
        }

        /// <summary>Bật lại sau khi tắt phải phát tiếp đúng bài đang muốn nghe, không phải im lặng.</summary>
        [Test]
        public void BatLaiSauKhiTat_TuPhatTiepBaiDangCho()
        {
            _sound.PlayMusic("s_login");
            _sound.SetEnabled(false);
            Assert.IsFalse(GetMusicSource(_sound).isPlaying);

            _sound.SetEnabled(true);

            Assert.IsTrue(GetMusicSource(_sound).isPlaying);
        }

        [Test]
        public void StopMusic_TatCaLuon_KhongTuPhatLaiKhiBatTiengLai()
        {
            _sound.PlayMusic("s_login");
            _sound.StopMusic();
            _sound.SetEnabled(false);

            _sound.SetEnabled(true);

            Assert.IsFalse(GetMusicSource(_sound).isPlaying, "StopMusic phải xoá luôn bài đang chờ, không chỉ tắt tiếng.");
        }

        [Test]
        public void PlayEffect_KhiDangTat_KhongNem()
        {
            _sound.SetEnabled(false);

            Assert.DoesNotThrow(() => _sound.PlayEffect("s_button"));
        }

        private static AudioSource GetMusicSource(SoundManager manager)
        {
            // Nguồn nhạc nền là AudioSource đầu tiên (thứ tự AddComponent trong Create).
            var sources = manager.GetComponents<AudioSource>();
            foreach (var s in sources)
            {
                if (s.loop) return s;
            }

            Assert.Fail("Không tìm thấy AudioSource nào loop=true (nhạc nền).");
            return null;
        }
    }
}
