using UnityEngine;

namespace Gopet.Runtime.Audio
{
    /// <summary>
    /// Nhạc nền + hiệu ứng, và nhớ trạng thái bật/tắt giữa hai lần mở app — tương
    /// đương <c>ISoundManagerSDK.loadMusicState</c>/<c>saveMusicState</c> của bản jar
    /// (<c>fr.java</c> command 3282/3283): nhạc nền và hiệu ứng có hai cờ riêng.
    ///
    /// <para>Hai <see cref="AudioSource"/> tách biệt: nhạc nền lặp và phát dài, hiệu
    /// ứng phát một lần và có thể chồng lên nhau (đánh liên tục) — dùng chung một
    /// nguồn thì hiệu ứng sau cắt ngang hiệu ứng trước.</para>
    /// </summary>
    public sealed class SoundManager : MonoBehaviour
    {
        private const string LegacyPrefsKey = "Gopet.SoundEnabled";
        private const string MusicPrefsKey = "Gopet.MusicEnabled";
        private const string EffectsPrefsKey = "Gopet.EffectsEnabled";

        /// <summary>Âm thanh dùng chung của phiên client hiện tại. Null trước khi bootstrap
        /// hoàn tất và sau khi bị hủy, vì vậy các lớp world có thể gọi an toàn.</summary>
        public static SoundManager Instance { get; private set; }

        private AudioSource _music;
        private AudioSource _effects;

        /// <summary>Bài nhạc đang MUỐN phát, kể cả khi đang tắt tiếng — để bật lại thì phát tiếp đúng bài đó.</summary>
        private string _pendingMusic;

        public bool MusicEnabled { get; private set; }
        public bool EffectsEnabled { get; private set; }
        public bool Enabled => MusicEnabled || EffectsEnabled;

        public static SoundManager Create(Transform parent)
        {
            var go = new GameObject("SoundManager");
            go.transform.SetParent(parent, false);

            var manager = go.AddComponent<SoundManager>();
            Instance = manager;
            manager._music = go.AddComponent<AudioSource>();
            manager._music.loop = true;
            manager._music.playOnAwake = false;

            manager._effects = go.AddComponent<AudioSource>();
            manager._effects.loop = false;
            manager._effects.playOnAwake = false;

            // Mặc định bật — người chơi tắt lần trước mới nhớ là tắt, không phải
            // ngược lại. Đọc int vì PlayerPrefs không có kiểu bool.
            var legacy = PlayerPrefs.GetInt(LegacyPrefsKey, 1);
            manager.MusicEnabled = PlayerPrefs.GetInt(MusicPrefsKey, legacy) != 0;
            manager.EffectsEnabled = PlayerPrefs.GetInt(EffectsPrefsKey, legacy) != 0;

            return manager;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Phát nhạc nền, lặp. Gọi lại với cùng tên khi đang phát thì không làm gì — tránh giật nhạc lúc dựng lại màn hình.</summary>
        public void PlayMusic(string name)
        {
            _pendingMusic = name;
            if (!MusicEnabled) return;

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
            if (!EffectsEnabled) return;

            _effects.PlayOneShot(SoundBank.Get(name));
        }

        public void SetEnabled(bool value)
        {
            SetMusicEnabled(value);
            SetEffectsEnabled(value);
            PlayerPrefs.SetInt(LegacyPrefsKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetMusicEnabled(bool value)
        {
            if (MusicEnabled == value) return;
            MusicEnabled = value;
            PlayerPrefs.SetInt(MusicPrefsKey, value ? 1 : 0);

            if (!value)
            {
                _music.Stop();
            }
            else if (_pendingMusic != null)
            {
                _music.clip = SoundBank.Get(_pendingMusic);
                _music.Play();
            }
            PlayerPrefs.Save();
        }

        public void SetEffectsEnabled(bool value)
        {
            if (EffectsEnabled == value) return;
            EffectsEnabled = value;
            PlayerPrefs.SetInt(EffectsPrefsKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
