using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Khung danh sách thẻ item dùng chung trong popup: nền trắng bo góc viền mảnh,
    /// cuộn dọc, mỗi dòng là một <see cref="ShopItemRow"/>.
    ///
    /// <para>Tách khỏi popup cửa hàng để popup Dịch vụ dùng lại nguyên style — trước
    /// đó nó nhúng <see cref="GenericMenuView"/> với thẻ nền tối, lạc hẳn khỏi tông
    /// sáng của khung.</para>
    ///
    /// <para>Danh sách trong popup chỉ vài chục dòng nên dựng thẳng, không
    /// virtualization. Cần cuộn hàng trăm dòng thì vẫn là việc của
    /// <see cref="GenericMenuView"/>.</para>
    /// </summary>
    public sealed class PopupItemList : MonoBehaviour
    {
        private readonly List<ShopItemRow> _rows = new List<ShopItemRow>();

        private Font _font;
        private RectTransform _content;
        private Text _placeholder;

        /// <summary>Người chơi bấm nút hành động của dòng thứ mấy.</summary>
        public event Action<int> Activated;

        public int RowCount => _rows.Count;

        /// <param name="withPanel">
        /// <c>false</c> khi vùng chứa ĐÃ là khung trắng có viền — vẽ thêm một khung
        /// nữa là hai đường viền chồng lên nhau.
        /// </param>
        public static PopupItemList Create(Transform parent, Font font, bool withPanel = true)
        {
            // Cắt bằng RectMask2D chứ không Mask: mask thường cần thêm một Graphic làm
            // khuôn và ăn thêm một lượt stencil, trong khi đây chỉ cần cắt chữ nhật.
            var panel = new GameObject("ItemList", typeof(RectTransform), typeof(Image),
                typeof(RectMask2D), typeof(ScrollRect));
            panel.transform.SetParent(parent, false);

            var rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (withPanel)
            {
                RoundedBorder.Apply(panel, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                    PopupPalette.Hairline);
            }
            else
            {
                // Vẫn cần một Graphic trong suốt: ScrollRect chỉ nhận kéo khi có thứ
                // gì đó bắt được raycast phủ hết vùng cuộn.
                panel.GetComponent<Image>().color = Color.clear;
            }

            var view = panel.AddComponent<PopupItemList>();
            view._font = font;

            var contentGo = new GameObject("Rows", typeof(RectTransform));
            contentGo.transform.SetParent(panel.transform, false);
            view._content = (RectTransform)contentGo.transform;
            view._content.anchorMin = new Vector2(0f, 1f);
            view._content.anchorMax = new Vector2(1f, 1f);
            view._content.pivot = new Vector2(0f, 1f);
            view._content.offsetMin = new Vector2(4f, 0f);
            view._content.offsetMax = new Vector2(-4f, 0f);
            view._content.anchoredPosition = Vector2.zero;

            var scroll = panel.GetComponent<ScrollRect>();
            scroll.viewport = rect;
            scroll.content = view._content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = ShopItemRow.Height * 0.65f;

            view._placeholder = UiBuilder.MakeText(panel.transform, font, "Placeholder", 13, true);
            view._placeholder.alignment = TextAnchor.MiddleCenter;
            view._placeholder.color = PopupPalette.TextMuted;
            view._placeholder.raycastTarget = false;
            view._placeholder.gameObject.SetActive(false);
            return view;
        }

        /// <summary><paramref name="actionLabel"/> là chữ trên nút khi dòng không kèm giá.</summary>
        public void Bind(MenuScreen screen, RemoteAssetCache assets, string actionLabel = "Mua")
        {
            if (screen == null) throw new ArgumentNullException(nameof(screen));

            Clear();
            for (var i = 0; i < screen.Items.Length; i++)
            {
                var row = ShopItemRow.Create(_content, _font);
                row.Bind(screen.Items[i], assets, actionLabel);
                row.SetSeparatorVisible(i < screen.Items.Length - 1);

                var rect = (RectTransform)row.transform;
                rect.anchoredPosition = new Vector2(0f, -i * ShopItemRow.Height);

                var captured = i;
                row.Buy += () => Activated?.Invoke(captured);
                _rows.Add(row);
            }

            // Chỉ đóng chiều CAO. Content neo trái-phải nên sizeDelta.x mang nghĩa
            // "rộng hơn vùng chứa bao nhiêu" — gán 0 là xoá mất lề đặt bằng offset.
            _content.sizeDelta = new Vector2(_content.sizeDelta.x,
                screen.Items.Length * ShopItemRow.Height);
            _content.anchoredPosition = Vector2.zero;
            ShowPlaceholder(screen.Items.Length == 0 ? "Chưa có gì để hiện." : null);
        }

        /// <summary>Chữ giữa khung khi chưa có gì để bày. <c>null</c> = ẩn đi.</summary>
        public void ShowPlaceholder(string message)
        {
            _placeholder.text = message ?? string.Empty;
            _placeholder.gameObject.SetActive(message != null);
        }

        public void Clear()
        {
            foreach (var row in _rows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _rows.Clear();
        }
    }
}
