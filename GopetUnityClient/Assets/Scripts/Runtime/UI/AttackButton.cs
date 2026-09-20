using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nút đánh quái ở góc dưới-phải, đối xứng với cần điều khiển bên trái. Bấm là
    /// đánh con quái gần nhất trong tầm — chạm trúng con quái đang chạy trên màn rất
    /// khó, nhất là khi nó chồng lên NPC hoặc người chơi khác.
    ///
    /// <para>Không có quái trong tầm thì nút MỜ ĐI chứ không biến mất: nút biến mất rồi
    /// hiện lại liên tục theo bước chân người chơi nhìn giật, và mất luôn chỗ để ngón
    /// tay chờ sẵn.</para>
    /// </summary>
    public sealed class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerExitHandler
    {
        /// <summary>Icon hai kiếm bắt chéo — <c>tools/image-gen/make-attack-button.py</c>.</summary>
        public const string IconResource = "Ui/attack-button";

        /// <summary>Xấp xỉ cần điều khiển bên trái (<c>VirtualJoystick</c> 132×132), nhỏ
        /// hơn một chút cho đỡ chiếm màn.</summary>
        public const float Size = 120f;

        /// <summary>Cách lề phải đủ để lọt giữa cụm nút PET/menu (14..70px từ mép phải)
        /// và khung chat ở giữa màn.</summary>
        private const float RightMargin = 80f;

        /// <summary>Cỡ lúc đang giữ — nổi lên đủ thấy nhưng không che mất ngón tay.</summary>
        private const float PressedScale = 1.12f;

        /// <summary>Tốc độ nội suy về cỡ đích; nhanh cho cú bấm "ăn" ngay, không ì.</summary>
        private const float PopSpeed = 16f;

        private static readonly Color Ready = Color.white;
        private static readonly Color NoTarget = new Color(1f, 1f, 1f, 0.42f);

        private Image _icon;
        private float _scale = 1f;
        private float _targetScale = 1f;

        public event Action Clicked;

        public static AttackButton Create(Transform parent)
        {
            var go = new GameObject("Attack Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            // Pivot ở TÂM để cú nảy phình đều bốn phía; nếu pivot ở góc thì nút nở lệch
            // lên trên-trái, nhìn như bị xô chứ không phải nổi lên.
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Size, Size);
            // Đáy nút ngang đáy cần điều khiển; pivot ở tâm nên phải cộng nửa cỡ vào.
            rect.anchoredPosition = new Vector2(
                -(RightMargin + Size * 0.5f),
                World.VirtualJoystick.ScreenMargin + Size * 0.5f);

            var button = go.AddComponent<AttackButton>();
            button._icon = go.GetComponent<Image>();
            button._icon.preserveAspect = true;
            button._icon.sprite = Resources.Load<Sprite>(IconResource);
            if (button._icon.sprite == null)
            {
                // Thiếu art thì vẫn phải đánh được: nút chữ thay cho icon.
                Debug.LogWarning($"[Gopet] Thiếu icon nút đánh tại Resources/{IconResource}.");
                RoundedUiSprite.Apply(button._icon);
                var label = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Label", 18, true);
                label.text = "ĐÁNH";
                label.alignment = TextAnchor.MiddleCenter;
                label.fontStyle = FontStyle.Bold;
                label.color = Color.white;
            }

            var click = go.GetComponent<Button>();
            // Tự lo phản hồi bằng cú nảy; để ColorTint mặc định thì nút vừa phình vừa
            // xỉn màu, trông như lỗi chứ không như được bấm.
            click.transition = Selectable.Transition.None;
            click.onClick.AddListener(() => button.Clicked?.Invoke());
            button.SetTargetInRange(false);
            return button;
        }

        /// <summary>Có quái trong tầm hay không — chỉ đổi độ mờ, nút vẫn bấm được để báo lý do.</summary>
        public void SetTargetInRange(bool inRange)
        {
            if (_icon != null) _icon.color = inRange ? Ready : NoTarget;
        }

        public void OnPointerDown(PointerEventData eventData) => _targetScale = PressedScale;

        public void OnPointerUp(PointerEventData eventData) => _targetScale = 1f;

        /// <summary>Ngón tay trượt ra ngoài nút: phải xẹp lại, nếu không nút kẹt ở cỡ to.</summary>
        public void OnPointerExit(PointerEventData eventData) => _targetScale = 1f;

        /// <summary>
        /// Nảy về cỡ đích thay vì nhảy phắt: đổi cỡ tức thì thì mắt không kịp thấy nút
        /// đã phản hồi. Dùng thời gian KHÔNG theo timeScale — lúc vào trận game có thể
        /// dừng scale, nút vẫn phải nhả ra bình thường.
        /// </summary>
        private void Update()
        {
            if (Mathf.Approximately(_scale, _targetScale)) return;
            _scale = Mathf.MoveTowards(_scale, _targetScale,
                Mathf.Abs(_targetScale - _scale) * PopSpeed * Time.unscaledDeltaTime + 0.002f);
            transform.localScale = Vector3.one * _scale;
        }
    }
}
