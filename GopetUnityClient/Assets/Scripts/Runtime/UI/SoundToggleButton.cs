using Gopet.Runtime.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nút tắt/bật âm thanh, cố định góc trên-phải MÀN HÌNH THẬT.
    ///
    /// <para>Đặt lên canvas CHUNG (không phải <see cref="PixelCanvas.Content"/>): nút
    /// phải luôn ở đúng góc màn hình thật, không bị co theo khung letterbox của
    /// PixelCanvas.</para>
    ///
    /// <para><b>Thiếu icon thì lùi về nhãn chữ</b>, không ném: mất một cái icon không
    /// đáng để chặn cả màn hình. Chữ "BẬT"/"TẮT" dùng đúng font đang hiển thị mọi chữ
    /// Việt khác nên chắc chắn đọc được.</para>
    /// </summary>
    public sealed class SoundToggleButton : MonoBehaviour
    {
        /// <summary>
        /// Cạnh nút, theo tỉ lệ CHIỀU CAO màn hình — nút giữ nguyên kích thước tương
        /// đối trên mọi cỡ máy. Đây là nút phụ nên để nhỏ; đừng hạ thêm nữa: trên điện
        /// thoại, dưới mức này là khó chạm trúng.
        /// </summary>
        private const float SizeFrac = 0.05f;

        private const float MarginFrac = 0.025f;

        private SoundManager _sound;
        private Image _icon;
        private Text _label;

        public static SoundToggleButton Create(Transform parent, SoundManager sound, Font font)
        {
            var go = new GameObject("SoundToggleButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f - MarginFrac - SizeFrac, 1f - MarginFrac - SizeFrac);
            rect.anchorMax = new Vector2(1f - MarginFrac, 1f - MarginFrac);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var view = go.AddComponent<SoundToggleButton>();
            view._sound = sound;

            // Ảnh trên chính GameObject của nút chỉ để BẮT cú chạm: trong suốt, nhưng
            // vẫn phải có, vì Button không có Graphic thì không nhận raycast.
            view._icon = go.GetComponent<Image>();
            view._icon.preserveAspect = true;

            view._label = UiBuilder.MakeText(go.transform, font, "Label", 12, stretch: true);
            view._label.alignment = TextAnchor.MiddleCenter;

            go.GetComponent<Button>().onClick.AddListener(view.Toggle);
            view.Refresh();
            return view;
        }

        private void Toggle()
        {
            _sound.SetEnabled(!_sound.Enabled);
            Refresh();
        }

        /// <summary>Đọc thẳng từ <see cref="SoundManager"/> chứ không giữ cờ riêng — hai nguồn sự thật là hai chỗ để lệch nhau.</summary>
        private void Refresh()
        {
            var icon = LoginSkin.Get(_sound.Enabled ? LoginSkin.SoundOn : LoginSkin.SoundOff);

            _icon.sprite = icon;
            _icon.color = icon == null ? UiBuilder.ButtonFace : Color.white;

            _label.enabled = icon == null;
            _label.text = _sound.Enabled ? "BẬT" : "TẮT";
        }
    }
}
