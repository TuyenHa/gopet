using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Nút "Vào" hiện lên khi người chơi đứng gần cổng dịch chuyển, thay vì phải bấm trúng
    /// mũi tên/nhãn tên map. Ẩn mặc định — <see cref="MapPortalView"/> bật/tắt mỗi frame
    /// theo khoảng cách tới self (xem <see cref="MapPortalView.EnterRadius"/>).
    ///
    /// <para>Ảnh nút là PNG dựng sẵn (đã có sẵn chữ "VÀO") ở
    /// <c>Resources/Ui/portal-enter-button.png</c>, không dùng JarFont dựng chữ nữa.</para>
    /// </summary>
    public sealed class PortalEnterButton : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>Bề rộng nút hiển thị trong world (đơn vị = pixel jar). Cao suy theo tỉ lệ ảnh.</summary>
        private const float DisplayWidth = 28f;

        /// <summary>Đường dẫn resource (không kèm đuôi .png). Xem <see cref="JarSkin"/> cho quy ước.</summary>
        private const string SpritePath = "Ui/portal-enter-button";

        private static Sprite _sprite;

        private Action _clicked;

        public static PortalEnterButton Create(Transform parent, Vector3 localPos, Action clicked)
        {
            var go = new GameObject("Enter Button", typeof(SpriteRenderer), typeof(BoxCollider2D));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var button = go.AddComponent<PortalEnterButton>();
            button._clicked = clicked;

            var sprite = LoadSprite();
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 29001; // trên nhãn tên map (29000)

            // Sprite đã render bằng world units theo pixelsPerUnit — chỉ cần đặt collider
            // đúng kích thước hiển thị (rộng × cao theo tỉ lệ ảnh).
            var (w, h) = DisplaySize(sprite);
            go.GetComponent<BoxCollider2D>().size = new Vector2(w, h);

            go.SetActive(false); // chỉ hiện khi MapPortalView phát hiện self đứng gần
            return button;
        }

        /// <summary>Bật/tắt hiển thị — gọi mỗi frame từ <see cref="MapPortalView"/>.</summary>
        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }

        /// <summary>Kích thước hiển thị (world units) — width cố định, height theo tỉ lệ ảnh.</summary>
        public static (float width, float height) DisplaySize(Sprite sprite)
        {
            if (sprite == null) return (DisplayWidth, DisplayWidth);
            var aspect = sprite.rect.height / sprite.rect.width;
            return (DisplayWidth, DisplayWidth * aspect);
        }

        /// <summary>Sprite dùng chung — nạp một lần, cache tĩnh.</summary>
        public static Sprite LoadSprite()
        {
            if (_sprite != null) return _sprite;
            var loaded = Resources.Load<Sprite>(SpritePath);
            if (loaded == null)
            {
                Debug.LogWarning($"[Gopet] Thiếu sprite nút Vào: Resources/{SpritePath}.png");
                return null;
            }
            // Ép pixelsPerUnit sao cho ảnh render đúng DisplayWidth world units.
            // Tự tạo Sprite mới từ texture để override PPU mà không đụng import setting.
            var tex = loaded.texture;
            _sprite = Sprite.Create(tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: tex.width / DisplayWidth);
            return _sprite;
        }

        public void OnPointerClick(PointerEventData eventData) => _clicked?.Invoke();
    }
}
