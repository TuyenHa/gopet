using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Ghim node sprite vào lưới pixel nguyên, trong khi node CHA vẫn đi mượt.
    ///
    /// <para>Nhân vật, pet và quái đi bằng <c>tốc độ × deltaTime</c> nên đứng ở toạ độ
    /// lẻ (172.37). Art là pixel art vẽ 1 pixel nguồn = 1 world unit, đứng ở toạ độ lẻ
    /// nghĩa là hàng pixel ngoài rìa sprite bị lấy mẫu lệch — nhìn ra thành nhoè, và
    /// nhoè NHẤT ở mấy con đang chạy. Tile và NPC đứng yên trên toạ độ chẵn nên nét,
    /// đó là lý do chỉ pet với quái bị kêu mờ.</para>
    ///
    /// <para>Bù lệch ở node CON chứ không làm tròn node cha: làm tròn cha thì logic
    /// bám đuôi, va chạm và gói di chuyển đều nhận toạ độ giật nấc. Ở đây chỉ có phần
    /// NHÌN dịch tối đa nửa pixel, mọi thứ khác vẫn mượt như cũ.</para>
    /// </summary>
    public sealed class PixelSnapVisual : MonoBehaviour
    {
        /// <summary>Gắn vào node sprite. Không gắn lên node có sẵn lệch cục bộ khác 0 —
        /// component này chiếm quyền ghi <c>localPosition</c> x/y.</summary>
        public static void Attach(Transform visual)
        {
            if (visual != null) visual.gameObject.AddComponent<PixelSnapVisual>();
        }

        /// <summary>
        /// LateUpdate để chạy SAU mọi code di chuyển của frame. Chạy ở Update thì bù
        /// lệch theo vị trí của frame trước, trật đúng một frame khi đang chạy.
        /// </summary>
        private void LateUpdate()
        {
            var parent = transform.parent;
            if (parent == null) return;

            // Toạ độ THẾ GIỚI mới là cái camera lấy mẫu; bù theo phần lẻ của nó.
            var world = parent.position;
            transform.localPosition = new Vector3(
                Mathf.Round(world.x) - world.x,
                Mathf.Round(world.y) - world.y,
                transform.localPosition.z);
        }
    }
}
