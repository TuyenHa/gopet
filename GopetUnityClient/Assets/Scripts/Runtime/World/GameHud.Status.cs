using System.Collections;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed partial class GameHud
    {
        private Text _placeTime;
        private Text _bigText;
        private Coroutine _hideBigText;

        public string PlaceTimeText => _placeTime == null ? string.Empty : _placeTime.text;
        public string BigText => _bigText == null ? string.Empty : _bigText.text;

        public void ShowPlaceTime(int seconds)
        {
            if (_placeTime == null) return;
            seconds = Mathf.Max(0, seconds);
            _placeTime.text = $"{seconds / 60:00}:{seconds % 60:00}";
            _placeTime.gameObject.SetActive(true);
        }

        public void ShowBigText(string text)
        {
            if (_bigText == null || string.IsNullOrWhiteSpace(text)) return;
            _bigText.text = text;
            _bigText.gameObject.SetActive(true);
            if (_hideBigText != null) StopCoroutine(_hideBigText);
            _hideBigText = StartCoroutine(HideBigTextAfterDelay());
        }

        private void BuildStatusOverlays(Transform parent, Font font)
        {
            _placeTime = UiBuilder.MakeText(parent, font, "Place Time", 20, false);
            var timerRect = _placeTime.rectTransform;
            timerRect.anchorMin = timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0f, -18f);
            timerRect.sizeDelta = new Vector2(180f, 32f);
            _placeTime.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(_placeTime, FontStyle.Bold);
            _placeTime.gameObject.SetActive(false);

            _bigText = UiBuilder.MakeText(parent, font, "Big Text Effect", 34, false);
            var bigRect = _bigText.rectTransform;
            bigRect.anchorMin = bigRect.anchorMax = new Vector2(0.5f, 0.58f);
            bigRect.pivot = new Vector2(0.5f, 0.5f);
            bigRect.sizeDelta = new Vector2(720f, 72f);
            _bigText.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(_bigText, FontStyle.Bold);
            _bigText.color = new Color(1f, 0.82f, 0.2f, 1f);
            _bigText.gameObject.SetActive(false);
        }

        private IEnumerator HideBigTextAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            if (_bigText != null) _bigText.gameObject.SetActive(false);
            _hideBigText = null;
        }
    }
}
