using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Hàng tab của popup: khay trắng bo góc viền mảnh, bên trong là các tab viên
    /// thuốc chia đều bề ngang — vàng chữ trắng khi chọn, xanh nhạt chữ navy khi không.
    ///
    /// <para>Mọi con số đo trên ảnh mẫu cửa hàng rồi quy về ref-unit của canvas (ảnh
    /// rộng 1631px ↔ 720 ref, hệ số 0.4415).</para>
    /// </summary>
    public sealed class PopupTabRail : MonoBehaviour
    {
        /// <summary>Chiều cao khay nhãn một dòng. Popup dùng để chừa chỗ cho nội dung bên dưới.</summary>
        public const float Height = 38f;

        /// <summary>Chiều cao khay khi nhãn được phép xuống hai dòng.</summary>
        public const float TallHeight = 52f;

        /// <summary>Khe thở giữa khay và nội dung bên dưới.</summary>
        public const float Gap = 6f;

        /// <summary>Lề trong khay, hai bên hàng tab.</summary>
        private const float RailPadding = 11f;
        private const float TabHeight = 20f;
        private const float TallTabHeight = 34f;
        private const float TabGap = 16f;
        /// <summary>Bán kính góc tab: 14px trên ảnh mẫu, quy về ref-unit là ~6. KHÔNG bo tròn hết.</summary>
        private const float TabRadius = 6f;

        private Image[] _backgrounds;
        private Text[] _labels;

        /// <summary>Người chơi chọn tab thứ mấy. Chỉ bắn khi tab THAY ĐỔI.</summary>
        public event Action<int> Selected;

        public int ActiveIndex { get; private set; } = -1;

        public int TabCount => _backgrounds.Length;

        /// <summary>
        /// Dựng khay chiếm nguyên bề ngang <paramref name="parent"/>, ghim mép trên.
        /// Chưa tab nào được chọn — gọi <see cref="Select"/> sau khi nối sự kiện.
        ///
        /// <para><paramref name="railWidth"/> truyền tường minh chứ không đọc
        /// <c>parent.rect.width</c>: rect của một con neo-giãn chưa chắc đã tính xong
        /// ngay lúc dựng, và đọc trúng lúc nó còn 0 thì mọi tab rộng 0.</para>
        /// </summary>
        /// <param name="twoLines">
        /// Cho nhãn xuống hai dòng và nới khay cao lên. Cần khi nhiều tab chia nhau bề
        /// ngang nên mỗi tab quá hẹp cho nhãn một dòng.
        /// </param>
        public static PopupTabRail Create(RectTransform parent, Font font, float railWidth,
            string[] labels, bool twoLines = false)
        {
            if (labels == null || labels.Length == 0)
                throw new ArgumentException("Khay tab phải có ít nhất một tab.", nameof(labels));

            var go = new GameObject("TabRail", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var railRect = (RectTransform)go.transform;
            railRect.anchorMin = new Vector2(0f, 1f);
            railRect.anchorMax = new Vector2(1f, 1f);
            railRect.pivot = new Vector2(0f, 1f);
            railRect.offsetMin = new Vector2(0f, -(twoLines ? TallHeight : Height));
            railRect.offsetMax = Vector2.zero;

            RoundedBorder.Apply(go, RoundedUiSprite.DefaultRadius, Color.white,
                PopupPalette.Hairline);

            var rail = go.AddComponent<PopupTabRail>();
            rail.BuildTabs(font, labels, railWidth, twoLines);
            return rail;
        }

        /// <summary>Chọn tab. Trùng tab đang chọn thì không làm gì và không bắn sự kiện.
        /// <paramref name="notify"/> = false chỉ tô lại (vd trả highlight về tab cũ sau khi
        /// tab "nút bấm" đã mở popup riêng).</summary>
        public void Select(int index, bool notify = true)
        {
            if (index < 0 || index >= _backgrounds.Length || index == ActiveIndex) return;

            ActiveIndex = index;
            for (var i = 0; i < _backgrounds.Length; i++) Paint(i, i == index);
            if (notify) Selected?.Invoke(index);
        }

        private void BuildTabs(Font font, string[] labels, float railWidth, bool twoLines)
        {
            _backgrounds = new Image[labels.Length];
            _labels = new Text[labels.Length];

            // Chia đều bề ngang còn lại cho các tab, đúng cách hàng tab cửa hàng làm.
            var usable = railWidth - RailPadding * 2f;
            var tabWidth = (usable - TabGap * (labels.Length - 1)) / labels.Length;

            for (var i = 0; i < labels.Length; i++)
            {
                var tabGo = new GameObject($"Tab_{labels[i]}", typeof(RectTransform),
                    typeof(Image), typeof(Button));
                tabGo.transform.SetParent(transform, false);

                var rect = (RectTransform)tabGo.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(tabWidth, twoLines ? TallTabHeight : TabHeight);
                rect.anchoredPosition = new Vector2(RailPadding + i * (tabWidth + TabGap), 0f);

                _backgrounds[i] = tabGo.GetComponent<Image>();
                RoundedUiSprite.Apply(_backgrounds[i], TabRadius);

                // Cỡ 11: chữ cao bằng ~38% chiều cao tab, đúng tỉ lệ đo trên ảnh mẫu.
                var label = UiBuilder.MakeText(tabGo.transform, font, "Label", 11, true);
                label.alignment = TextAnchor.MiddleCenter;
                label.text = labels[i];
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
                // MakeText bật Overflow; nhãn dài phải xuống dòng thay vì tràn ra
                // ngoài viên tab, và cắt nếu vẫn không đủ chỗ.
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                _labels[i] = label;
                Paint(i, false);

                var captured = i;
                tabGo.GetComponent<Button>().onClick.AddListener(() => Select(captured));
            }
        }

        private void Paint(int index, bool active)
        {
            _backgrounds[index].color = active ? PopupPalette.TabActive : PopupPalette.TabInactive;
            _labels[index].color = active ? Color.white : PopupPalette.TextDark;
        }
    }
}
