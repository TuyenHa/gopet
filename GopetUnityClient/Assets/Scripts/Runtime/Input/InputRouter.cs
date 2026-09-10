using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gopet.Runtime.Input
{
    /// <summary>
    /// Đưa phím tắt của bàn phím vào tầng UI: <b>Esc = đóng màn hình trên cùng</b>,
    /// <b>Enter = bấm nút mặc định</b>.
    ///
    /// <para>Chạm và chuột KHÔNG đi qua đây. uGUI đã lo cả hai qua
    /// <c>EventSystem</c>, nên thêm một đường nữa chỉ tạo chỗ để hai đường lệch
    /// nhau. Đây là lý do class này chỉ có một action map chứ không phải hai như
    /// plan phác ban đầu: khác biệt giữa mobile và PC nằm ở <b>có bàn phím hay
    /// không</b>, và Input System tự trả lời câu đó — không cần
    /// <c>#if UNITY_ANDROID</c> ở đâu cả.</para>
    ///
    /// <para>Trên Android, nút back của máy được Input System báo về đúng phím
    /// <c>escape</c> của thiết bị bàn phím ảo, nên nó rơi vào cùng một binding.
    /// <b>Chưa kiểm trên máy thật</b> — cùng nhóm với việc đo fps.</para>
    /// </summary>
    public sealed class InputRouter : MonoBehaviour
    {
        private InputAction _cancel;
        private InputAction _submit;

        /// <summary>Esc, hoặc nút back trên Android.</summary>
        public event Action Cancelled;

        /// <summary>Enter, kể cả Enter của bàn phím số.</summary>
        public event Action Submitted;

        public static InputRouter Create(Transform parent)
        {
            var go = new GameObject("InputRouter");
            go.transform.SetParent(parent, false);
            return go.AddComponent<InputRouter>();
        }

        private void Awake()
        {
            // Dựng bằng code chứ không dùng file .inputactions: binding chỉ có hai
            // dòng, mà asset thì không đọc được trong review và không test được
            // ngoài Editor.
            _cancel = new InputAction("UI/Cancel", InputActionType.Button, "<Keyboard>/escape");
            _submit = new InputAction("UI/Submit", InputActionType.Button, "<Keyboard>/enter");
            _submit.AddBinding("<Keyboard>/numpadEnter");

            _cancel.performed += _ => Cancelled?.Invoke();
            _submit.performed += _ => Submitted?.Invoke();
        }

        private void OnEnable()
        {
            _cancel.Enable();
            _submit.Enable();
        }

        private void OnDisable()
        {
            _cancel.Disable();
            _submit.Disable();
        }

        private void OnDestroy()
        {
            _cancel?.Dispose();
            _submit?.Dispose();
        }
    }
}
