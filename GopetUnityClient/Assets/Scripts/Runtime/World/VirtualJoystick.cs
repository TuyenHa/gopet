using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Joystick uGUI dùng được bằng touch và chuột trong Editor.</summary>
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private RectTransform _rect;
        private RectTransform _handle;
        private float _radius;
        private float _handleTravel;

        public Vector2 Direction { get; private set; }

        /// <summary>Khung D-pad (đã khoét ổ ở tâm) — <c>Resources/Ui/dpad.png</c>.</summary>
        private const string FrameSprite = "Ui/dpad";
        /// <summary>Nút tròn ở tâm, trượt theo hướng — <c>Resources/Ui/dpad-knob.png</c>.</summary>
        private const string KnobSprite = "Ui/dpad-knob";

        /// <summary>Khoảng cách tới lề màn. Nút đánh bên phải dùng chung để hai nút ngón
        /// cái nằm đúng một hàng, không cái cao cái thấp.</summary>
        public const float ScreenMargin = 28f;

        public static VirtualJoystick Create(Transform parent)
        {
            var go = new GameObject("Movement Joystick", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(ScreenMargin, ScreenMargin);
            rect.sizeDelta = new Vector2(132f, 132f);

            var stick = go.AddComponent<VirtualJoystick>();
            stick._rect = rect;
            stick._radius = 44f;

            var frame = Resources.Load<Sprite>(FrameSprite);
            var knob = Resources.Load<Sprite>(KnobSprite);
            var image = go.GetComponent<Image>();
            if (frame != null && knob != null)
            {
                // Art D-pad: khung tĩnh + nút tròn TRƯỢT theo hướng (như cần joystick).
                image.sprite = frame;
                image.color = Color.white;
                image.preserveAspect = true;
                stick._handleTravel = 26f; // nút chỉ nhích về phía mũi tên, không chạm mép
                stick.MakeHandle(new Vector2(56f, 56f), knob, Color.white);
            }
            else
            {
                // Thiếu art → đường lùi bằng vòng tối + chấm xanh, vẫn chơi được.
                image.color = new Color(0.05f, 0.08f, 0.12f, 0.48f);
                stick._handleTravel = stick._radius;
                stick.MakeHandle(new Vector2(56f, 56f), null, new Color(0.35f, 0.7f, 1f, 0.8f));
            }
            return stick;
        }

        private void MakeHandle(Vector2 size, Sprite sprite, Color color)
        {
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(transform, false);
            _handle = (RectTransform)handle.transform;
            _handle.anchorMin = _handle.anchorMax = new Vector2(0.5f, 0.5f);
            _handle.sizeDelta = size;
            var img = handle.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false; // để pointer luôn tới nền, kể cả khi nút trượt dưới ngón
        }

        public void OnPointerDown(PointerEventData eventData) => UpdatePointer(eventData);
        public void OnDrag(PointerEventData eventData) => UpdatePointer(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            if (_handle != null) _handle.anchoredPosition = Vector2.zero;
        }

        private void UpdatePointer(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rect, eventData.position, eventData.pressEventCamera, out var local)) return;
            Direction = Vector2.ClampMagnitude(local / _radius, 1f);
            if (_handle != null) _handle.anchoredPosition = Direction * _handleTravel;
        }
    }
}
