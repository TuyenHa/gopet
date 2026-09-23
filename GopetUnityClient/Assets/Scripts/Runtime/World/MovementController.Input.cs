using UnityEngine;
using UnityEngine.InputSystem;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Phần nối với Input System của <see cref="MovementController"/>: dựng/huỷ action và đọc
    /// hướng đi từ bàn phím lẫn joystick cảm ứng.
    ///
    /// <para>Tách file để phần điều khiển chuyển động ở file chính còn dưới ngưỡng 200 dòng.</para>
    /// </summary>
    public sealed partial class MovementController
    {
        private void Awake()
        {
            _wasdAction = new InputAction("Move WASD", InputActionType.Value);
            var composite = _wasdAction.AddCompositeBinding("2DVector");
            composite.With("Up", "<Keyboard>/w");
            composite.With("Down", "<Keyboard>/s");
            composite.With("Left", "<Keyboard>/a");
            composite.With("Right", "<Keyboard>/d");
            _arrowAction = new InputAction("Move Arrows", InputActionType.Value);
            var arrows = _arrowAction.AddCompositeBinding("2DVector");
            arrows.With("Up", "<Keyboard>/upArrow");
            arrows.With("Down", "<Keyboard>/downArrow");
            arrows.With("Left", "<Keyboard>/leftArrow");
            arrows.With("Right", "<Keyboard>/rightArrow");
        }

        private void OnEnable()
        {
            _wasdAction?.Enable();
            _arrowAction?.Enable();
        }

        private void OnDisable()
        {
            _wasdAction?.Disable();
            _arrowAction?.Disable();
        }

        private void OnDestroy()
        {
            _wasdAction?.Dispose();
            _arrowAction?.Dispose();
            _wasdAction = null;
            _arrowAction = null;
        }

        /// <summary>Joystick cảm ứng được ưu tiên; bàn phím chỉ đọc khi không gõ chat.</summary>
        private (float dx, float dy) ReadDirection()
        {
            var mobile = _hud?.Joystick?.Direction ?? Vector2.zero;
            if (mobile.sqrMagnitude > 0.001f) return (mobile.x, -mobile.y);
            if (_hud != null && _hud.IsTyping) return (0f, 0f);
            var keyboard = (_wasdAction?.ReadValue<Vector2>() ?? Vector2.zero) +
                (_arrowAction?.ReadValue<Vector2>() ?? Vector2.zero);
            keyboard = Vector2.ClampMagnitude(keyboard, 1f);
            // Input System uses screen/gamepad convention (up = +Y), while the
            // JAR map coordinates increase downward (up = -Y).
            return (keyboard.x, -keyboard.y);
        }
    }
}
