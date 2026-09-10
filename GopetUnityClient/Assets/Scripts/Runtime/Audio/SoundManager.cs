using UnityEngine;

namespace Gopet.Runtime.Audio
{
    /// <summary>
    /// Nhạc nền + hiệu ứng, và nhớ trạng thái bật/tắt giữa hai lần mở app — tương
    /// đương <c>ISoundManagerSDK.loadMusicState</c>/<c>saveMusicState</c> của bản jar
    /// (<c>fx.java</c>). Bản jar chỉ có MỘT cờ bật/tắt chung, không tách riêng nhạc
    /// và hiệu ứng — giữ đúng như vậy, đừng bịa thêm cờ thứ hai.
    ///
    /// <para>Hai <see cref="AudioSource"/> tách biệt: nhạc nền lặp và phát dài, hiệu
    /// ứng phát một lần và có thể chồng lên nhau (đánh liên tục) — dùng chung một
    /// nguồn thì hiệu ứng sau cắt ngang hiệu ứng trước.</para>
    /// </summary>
    public sealed class SoundManager : MonoBehaviour
    {
        private const string PrefsKey = "Gopet.SoundEnabled";

        private AudioSource _music;
        private AudioSource _effects;

        /// <summary>Bài nhạc đang MUỐN phát, kể cả khi đang tắt tiếng — để bật lại thì phát tiếp đúng bài đó.</summary>
        private string _pendingMusic;

        public bool Enabled { get; private set; }

        public static SoundManager Create(Transform parent)
        {
            var go = new GameObject("SoundManager");
            go.transform.SetParent(parent, false);

            var manager = go.AddComponent<SoundManager>();
            manager._music = go.AddComponent<AudioSource>();
            manager._music.loop = true;
            manager._music.playOnAwake = false;

            manager._effects = go.AddComponent<AudioSource>();
            manager._effects.loop = false;
            manager._effects.playOnAwake = false;

            // Mặc định bật — người chơi tắt lần trước mới nhớ là tắt, không phải
            // ngược lại. Đọc int vì PlayerPrefs không có kiểu bool.
            manager.Enabled = PlayerPrefs.GetInt(PrefsKey, 1) != 0;

            return manager;
        }

        /// <summary>Phát nhạc nền, lặp. Gọi lại với cùng tên khi đang phát thì không làm gì — tránh giật nhạc lúc dựng lại màn hình.</summary>
        public void PlayMusic(string name)
        {
            _pendingMusic = name;
            if (!Enabled) return;

            var clip = SoundBank.Get(name);
            if (_music.clip == clip && _music.isPlaying) return;

            _music.clip = clip;
            _music.Play();
        }

        public void StopMusic()
        {
            _pendingMusic = null;
            _music.Stop();
        }

        /// <summary>Phát một hiệu ứng, không lặp. Im lặng bỏ qua nếu đang tắt tiếng.</summary>
        public void PlayEffect(string name)
        {
            if (!Enabled) return;

            _effects.PlayOneShot(SoundBank.Get(name));
        }

        public void SetEnabled(bool value)
        {
            if (Enabled == value) return;

            Enabled = value;
            PlayerPrefs.SetInt(PrefsKey, value ? 1 : 0);
            PlayerPrefs.Save();

            if (!value)
            {
                _music.Stop();
            }
            else if (_pendingMusic != null)
            {
                _music.clip = SoundBank.Get(_pendingMusic);
                _music.Play();
            }
        }
    }
}
