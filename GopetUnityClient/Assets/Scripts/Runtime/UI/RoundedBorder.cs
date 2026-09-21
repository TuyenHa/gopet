using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Khung bo góc có viền mảnh ĐỀU, vẽ bằng hai lớp ảnh: lớp ngoài mang màu viền,
    /// lớp trong thụt vào đúng độ dày viền và mang màu nền.
    ///
    /// <para><b>Đừng dùng <see cref="Outline"/> cho việc này.</b> Nó không vẽ viền mà
    /// nhân bản mesh ra bốn vị trí chéo (±x, ±y), nên viền dồn dày ở góc, mỏng dần ở
    /// giữa cạnh và luôn trông nhoè — không bao giờ ra được một nét đều 1px.</para>
    /// </summary>
    public static class RoundedBorder
    {
        /// <summary>
        /// Biến <paramref name="host"/> thành khung có viền. Trả về ảnh nền bên trong
        /// để caller đổi màu sau này nếu cần.
        ///
        /// <para>Lớp nền là con ĐẦU TIÊN và bật <c>ignoreLayout</c>: đứng đầu để vẽ
        /// dưới nội dung, và ignoreLayout để layout group của <paramref name="host"/>
        /// không xếp nó thành một ô như icon hay chữ.</para>
        ///
        /// <para><b>Lớp nền là một CON của <paramref name="host"/>.</b> Màn hình nào
        /// dọn nội dung bằng cách xoá hết con của khung sẽ ăn mất nền và chỉ còn trơ
        /// lớp viền. Đặt nội dung vào một lớp con riêng rồi dọn lớp đó.</para>
        /// </summary>
        public static Image Apply(GameObject host, float radius, Color fill, Color border,
            float thickness = 1f)
        {
            var outer = host.GetComponent<Image>();
            if (outer == null) outer = host.AddComponent<Image>();
            RoundedUiSprite.Apply(outer, radius);
            outer.color = border;

            var go = new GameObject("Fill", typeof(RectTransform), typeof(Image),
                typeof(LayoutElement));
            go.transform.SetParent(host.transform, false);
            go.transform.SetAsFirstSibling();

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(thickness, thickness);
            rect.offsetMax = new Vector2(-thickness, -thickness);

            go.GetComponent<LayoutElement>().ignoreLayout = true;

            var inner = go.GetComponent<Image>();
            // Bán kính trong nhỏ hơn đúng độ dày viền, nếu không hai đường cong lệch
            // nhau và viền phình ra ở bốn góc.
            RoundedUiSprite.Apply(inner, Mathf.Max(radius - thickness, 1f));
            inner.color = fill;
            inner.raycastTarget = false;
            return inner;
        }
    }
}
