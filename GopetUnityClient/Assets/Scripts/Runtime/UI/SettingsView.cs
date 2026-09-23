using System;
using Gopet.Runtime.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed class SettingsView : MonoBehaviour
    {
        private SoundManager _sound;
        private Text _soundToggle, _music, _effects, _language, _autoAttack, _autoRecovery;
        private bool _autoEnabled;
        private bool _autoRecoveryEnabled;

        public event Action<bool> AutoAttackChanged;
        public event Action<bool> AutoRecoveryChanged;
        public event Action CloseRequested;

        public static SettingsView Create(Transform parent, SoundManager sound, bool autoEnabled,
            bool autoRecoveryEnabled = false)
        {
            var root = new GameObject("Settings", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            var view = root.AddComponent<SettingsView>();
            view._sound = sound;
            view._autoEnabled = autoEnabled;
            view._autoRecoveryEnabled = autoRecoveryEnabled;
            root.GetComponent<Button>().onClick.AddListener(() => view.CloseRequested?.Invoke());
            view.Build();
            return view;
        }

        private void Build()
        {
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(380f, 410f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.98f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());

            var title = UiBuilder.MakeText(panel.transform, UiBuilder.DefaultFont(), "Title", 20, false);
            UiBuilder.PlaceRow(title.rectTransform, 12f, 34f, 14f);
            title.text = "Cài đặt";
            title.alignment = TextAnchor.MiddleCenter;

            _soundToggle = Row(panel.transform, 55f, () => _sound.SetEnabled(!_sound.Enabled));
            _music = Row(panel.transform, 105f, () => _sound.SetMusicEnabled(!_sound.MusicEnabled));
            _effects = Row(panel.transform, 155f, () => _sound.SetEffectsEnabled(!_sound.EffectsEnabled));
            _language = Row(panel.transform, 205f, JarStrings.ToggleLanguage);
            _autoAttack = Row(panel.transform, 255f, ToggleAutoAttack);
            _autoRecovery = Row(panel.transform, 305f, ToggleAutoRecovery);
            var close = Row(panel.transform, 355f, () => CloseRequested?.Invoke());
            close.text = "Đóng";
            Refresh();
        }

        private Text Row(Transform parent, float top, UnityEngine.Events.UnityAction clicked)
        {
            var go = new GameObject("Setting Row", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.offsetMin = new Vector2(18f, -(top + 40f));
            rect.offsetMax = new Vector2(-18f, -top);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 15, true);
            text.alignment = TextAnchor.MiddleCenter;
            go.GetComponent<Button>().onClick.AddListener(() => { clicked(); Refresh(); });
            return text;
        }

        private void ToggleAutoAttack()
        {
            _autoEnabled = !_autoEnabled;
            AutoAttackChanged?.Invoke(_autoEnabled);
        }

        private void ToggleAutoRecovery()
        {
            _autoRecoveryEnabled = !_autoRecoveryEnabled;
            AutoRecoveryChanged?.Invoke(_autoRecoveryEnabled);
        }

        private void Refresh()
        {
            _soundToggle.text = $"Âm thanh: {OnOff(_sound.Enabled)}";
            _music.text = $"Nhạc nền: {OnOff(_sound.MusicEnabled)}";
            _effects.text = $"Hiệu ứng: {OnOff(_sound.EffectsEnabled)}";
            _language.text = $"Ngôn ngữ: {(JarStrings.Current == Language.Vi ? "Tiếng Việt" : "English")}";
            _autoAttack.text = $"Tự đánh quái: {OnOff(_autoEnabled)}";
            _autoRecovery.text = $"Tự hồi HP: {OnOff(_autoRecoveryEnabled)}";
        }

        private static string OnOff(bool value) => value ? "Bật" : "Tắt";
    }
}
