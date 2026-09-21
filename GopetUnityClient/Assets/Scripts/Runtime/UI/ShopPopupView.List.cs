using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Khung danh sách thẻ item của <see cref="ShopPopupView"/>.</summary>
    public sealed partial class ShopPopupView
    {
        /// <summary>
        /// Khung trắng bo góc chứa các thẻ item, cuộn dọc được. Nằm dưới khay tab
        /// trong <see cref="GamePopupFrame.Content"/>, chiếm hết chỗ còn lại.
        ///
        /// <para>Cắt bằng <see cref="RectMask2D"/> chứ không <see cref="Mask"/>: mask
        /// thường cần thêm một Graphic làm khuôn và ăn thêm một lượt stencil, trong khi
        /// đây chỉ cần cắt theo hình chữ nhật.</para>
        /// </summary>
        private void BuildList(RectTransform parent)
        {
            _list = PopupItemList.Create(parent, _font);
            var rect = (RectTransform)_list.transform;
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
        }
    }
}
