using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Chuyển từ login vào map bằng một đường cắt ngang ở chính giữa: nửa trên trượt
    /// lên, nửa dưới trượt xuống và để lộ map theo chiều dọc.
    /// </summary>
    public sealed class VerticalSplitRevealTransition : MonoBehaviour
    {
        private const float Duration = 0.85f;
        private const int SortingOrder = 100;

        private RectTransform _top;
        private RectTransform _bottom;
        private Texture2D _screenshot;
        private float _height;
        private float _elapsed;

        /// <summary>Giữ màn login tới cuối frame để chụp, sau đó mới cho map lộ ra.</summary>
        public static VerticalSplitRevealTransition Create(Transform parent, Action hideSource)
        {
            var root = new GameObject("Vertical Split Reveal Transition", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(parent, false);

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            UiBuilder.Stretch((RectTransform)root.transform);

            var transition = root.AddComponent<VerticalSplitRevealTransition>();
            transition.StartCoroutine(transition.CaptureThenReveal(hideSource));
            return transition;
        }

        private IEnumerator CaptureThenReveal(Action hideSource)
        {
            yield return new WaitForEndOfFrame();

            if (Screen.width <= 0 || Screen.height <= 0)
            {
                hideSource?.Invoke();
                Destroy(gameObject);
                yield break;
            }

            var screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
            screenshot.Apply(false, false);

            hideSource?.Invoke();
            BuildHalves(screenshot);
        }

        private void BuildHalves(Texture2D screenshot)
        {
            _screenshot = screenshot;
            _height = screenshot.height;
            var halfHeight = Mathf.FloorToInt(_height / 2f);
            var width = screenshot.width;

            _bottom = MakeHalf("Bottom", screenshot,
                new Rect(0f, 0f, width, halfHeight), new Vector2(0.5f, 1f));
            _top = MakeHalf("Top", screenshot,
                new Rect(0f, halfHeight, width, _height - halfHeight), new Vector2(0.5f, 0f));
        }

        private RectTransform MakeHalf(string name, Texture2D texture, Rect crop, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.sprite = Sprite.Create(texture, crop, Vector2.zero, 100f);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = pivot;
            rect.sizeDelta = crop.size;
            return rect;
        }

        private void Update()
        {
            if (_screenshot == null) return;

            _elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_elapsed / Duration);
            var eased = 1f - (1f - t) * (1f - t);
            var distance = _height * 0.5f * eased;

            if (_top != null) _top.anchoredPosition = new Vector2(0f, distance);
            if (_bottom != null) _bottom.anchoredPosition = new Vector2(0f, -distance);

            if (t >= 1f) Destroy(gameObject);
        }

        private void OnDestroy()
        {
            DestroySprite(_top);
            DestroySprite(_bottom);
            if (_screenshot != null) Destroy(_screenshot);
        }

        private static void DestroySprite(RectTransform rect)
        {
            if (rect == null) return;
            var image = rect.GetComponent<Image>();
            if (image != null && image.sprite != null) Destroy(image.sprite);
        }
    }
}
