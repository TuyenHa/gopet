using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Màn hình danh sách dùng chung cho <b>toàn bộ</b> menu của game: shop, kho đồ,
    /// clan, kiosk, nhiệm vụ, xăm, ghép đồ, chọn pet…
    ///
    /// <para><b>Không có một dòng nào rẽ nhánh theo <c>listId</c>.</b> Server có 162
    /// màn hình đi qua đúng một định dạng gói, nên client chỉ nhìn cờ của từng dòng;
    /// thêm <c>switch (listId)</c> là bắt đầu nợ 162 nhánh phải bảo trì tay.</para>
    ///
    /// <para>"Bấm dòng này thì làm gì" nằm ở <see cref="MenuSelection"/>, "cuộn tới đây
    /// thì dựng dòng nào" ở <see cref="MenuVirtualizer"/> — cả hai thuần C#, test được
    /// ngoài Editor. Class này chỉ dựng hình và nối sự kiện.</para>
    /// </summary>
    public sealed class GenericMenuView : MonoBehaviour
    {
        private readonly Dictionary<int, MenuItemRow> _realized = new Dictionary<int, MenuItemRow>();
        private readonly Stack<MenuItemRow> _pool = new Stack<MenuItemRow>();

        private MenuScreen _screen;
        private RemoteAssetCache _assets;
        private GuiderHandler _guider;
        private Font _font;
        private Transform _rowParent;
        private RectTransform _content;
        private Text _title;
        private ScrollRect _scrollRect;
        private bool _compactCards;
        private bool _lightCards;
        private float _rowGap;

        private float _viewportHeight;
        private float _scrollY;

        public MenuScreen Screen => _screen;

        /// <summary>Các dòng đang được dựng thật. Với danh sách dài, ít hơn nhiều so với tổng số dòng.</summary>
        public IReadOnlyCollection<MenuItemRow> Rows => _realized.Values;

        public VisibleRange Visible { get; private set; }

        /// <summary>
        /// Dòng đang dựng cho chỉ số này, hoặc <c>null</c> nếu nó nằm ngoài tầm nhìn.
        ///
        /// <para>Tra theo CHỈ SỐ TRONG DANH SÁCH chứ không phải thứ tự dựng: dòng thứ
        /// 100 vẫn là chỉ số 100 dù nó là GameObject đầu tiên đang sống.</para>
        /// </summary>
        public MenuItemRow RowAt(int index)
        {
            return _realized.TryGetValue(index, out var row) ? row : null;
        }

        /// <summary>Dòng cần hỏi trước khi gửi. Tham số thứ hai là hành động chạy khi người dùng đồng ý.</summary>
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        /// <summary>Dòng có <c>closeScreenAfterClick</c> — màn hình này phải đóng sau khi gửi.</summary>
        public event Action<GenericMenuView> CloseRequested;

        /// <summary>Kiểu card gọn cho các danh sách ngoại hình trong Hành trang.</summary>
        public void SetCompactCards(bool value)
        {
            _compactCards = value;
            foreach (var row in _realized.Values) row.SetCompactCard(value);
        }

        /// <summary>
        /// Khoảng trống giữa hai dòng (ref-unit). Mặc định 0 — dòng liền nhau, ngăn bằng vạch.
        /// Danh sách thẻ nền tối (chọn pet) cần khe hở, không thì các thẻ dính thành một khối.
        /// </summary>
        public void SetRowGap(float gap)
        {
            gap = Mathf.Max(0f, gap);
            if (Mathf.Approximately(gap, _rowGap)) return;
            _rowGap = gap;
            RecycleAll();
            Refresh();
        }

        /// <summary>Bước từ mép trên dòng này tới mép trên dòng kế: cao dòng + khe hở.</summary>
        private float RowStep => MenuItemRow.Height + _rowGap;

        /// <summary>Kiểu card nền sáng cho các popup danh sách pet.</summary>
        public void SetLightCards(bool value)
        {
            _lightCards = value;
            foreach (var row in _realized.Values) row.SetLightCard(value);
        }

        /// <summary>Bấm phải dòng không cho chọn. Có sự kiện để test và để UI báo nhẹ, không im lặng.</summary>
        public event Action<int> BlockedRowTapped;

        /// <summary>Optional specialized action menu. Return true to suppress normal server selection.</summary>
        public Func<int, bool> SelectionOverride { get; set; }

        public static GenericMenuView Create(Transform parent, Font font)
        {
            var go = new GameObject("GenericMenuView", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            // Trải kín vùng chứa, nếu không thì mọi dòng bên trong rộng 0.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var view = go.AddComponent<GenericMenuView>();
            view._font = font;
            view._rowParent = go.transform;
            return view;
        }

        public void Bind(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _assets = assets;
            _guider = guider ?? throw new ArgumentNullException(nameof(guider));

            // Thu hồi HẾT trước khi dựng lại. Refresh() giữ nguyên dòng đã dựng nếu
            // chỉ số của nó vẫn nằm trong tầm nhìn — đúng khi cuộn, nhưng sai khi
            // đổi sang màn hình khác: dòng 0 của menu cũ sẽ ở lại với nội dung cũ.
            RecycleAll();
            _scrollY = 0f;

            if (_title != null) _title.text = screen.Title;
            UpdateContentHeight();

            Refresh();
        }

        /// <summary>
        /// Dựng khung cuộn thật cho runtime. Tách khỏi <see cref="Create"/> để các test logic
        /// và caller nhúng menu vào layout riêng vẫn có thể điều khiển viewport bằng tay.
        /// </summary>
        public void EnableInteractiveScroll(float viewportHeight = 388f)
        {
            if (_scrollRect != null) return;

            var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(transform, false);
            backdrop.transform.SetAsFirstSibling();
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.58f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            panel.transform.SetParent(transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(580f, viewportHeight + 76f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());
            panel.GetComponent<Image>().color = UiBuilder.Panel;

            _title = UiBuilder.MakeText(panel.transform, _font ?? UiBuilder.DefaultFont(), "Title", 18, false);
            _title.text = _screen?.Title ?? string.Empty;
            UiBuilder.SetFontStyle(_title, FontStyle.Bold);
            _title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(_title.rectTransform, 8f, 42f, 54f);

            var close = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            close.transform.SetParent(panel.transform, false);
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-8f, -8f);
            closeRect.sizeDelta = new Vector2(38f, 34f);
            close.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var closeLabel = UiBuilder.MakeText(close.transform, _font ?? UiBuilder.DefaultFont(), "Label", 18, true);
            closeLabel.text = "×";
            closeLabel.alignment = TextAnchor.MiddleCenter;
            close.GetComponent<Button>().onClick.AddListener(() => CloseRequested?.Invoke(this));

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(panel.transform, false);
            var viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(10f, 10f);
            viewportRect.offsetMax = new Vector2(-10f, -58f);
            viewport.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.55f);
            viewport.GetComponent<Mask>().showMaskGraphic = true;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            _content = (RectTransform)content.transform;
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0f, 1f);
            _content.anchoredPosition = Vector2.zero;

            _rowParent = _content;
            foreach (var row in _realized.Values) row.transform.SetParent(_rowParent, false);
            foreach (var row in _pool) if (row != null) row.transform.SetParent(_rowParent, false);

            _scrollRect = panel.GetComponent<ScrollRect>();
            _scrollRect.viewport = viewportRect;
            _scrollRect.content = _content;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = MenuItemRow.Height * 0.65f;
            _scrollRect.onValueChanged.AddListener(_ => OnInteractiveScroll());

            _viewportHeight = viewportHeight;
            UpdateContentHeight();
            Refresh();
        }

        /// <summary>
        /// Đặt chiều cao vùng nhìn để bật virtualization.
        ///
        /// <para>Để 0 (mặc định) thì dựng hết mọi dòng — đúng cho menu ngắn và cho
        /// test không dựng layout. Danh sách vài trăm dòng thì phải đặt, nếu không
        /// là vài trăm GameObject cho một màn hình mắt chỉ thấy chục dòng.</para>
        /// </summary>
        public void SetViewport(float height)
        {
            _viewportHeight = height;
            Refresh();
        }

        /// <summary>Bật cuộn khi menu được nhúng sẵn trong một pane của popup.</summary>
        public void EnableEmbeddedScroll(float viewportHeight)
        {
            if (_scrollRect != null) return;

            var hitArea = gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;
            // Menu nhúng trong popup phải bị cắt ở đúng viewport; nếu thiếu mask,
            // các dòng dài sẽ tràn ra ngoài panel dù ScrollRect vẫn được bật.
            if (gameObject.GetComponent<RectMask2D>() == null)
                gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(transform, false);
            _content = (RectTransform)content.transform;
            _content.anchorMin = new Vector2(0f, 1f);
            _content.anchorMax = new Vector2(1f, 1f);
            _content.pivot = new Vector2(0f, 1f);
            _content.anchoredPosition = Vector2.zero;

            _rowParent = _content;
            foreach (var row in _realized.Values) row.transform.SetParent(_rowParent, false);
            foreach (var row in _pool) if (row != null) row.transform.SetParent(_rowParent, false);

            _scrollRect = gameObject.AddComponent<ScrollRect>();
            _scrollRect.viewport = (RectTransform)transform;
            _scrollRect.content = _content;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = MenuItemRow.Height * 0.65f;
            _scrollRect.onValueChanged.AddListener(_ => OnInteractiveScroll());

            _viewportHeight = viewportHeight;
            UpdateContentHeight();
            Refresh();
        }

        public void SetScroll(float y)
        {
            _scrollY = y;
            Refresh();
        }

        private void Refresh()
        {
            if (_screen == null) return;

            UpdateContentHeight();

            Visible = _viewportHeight > 0f
                ? MenuVirtualizer.Compute(_screen.Items.Length, RowStep, _viewportHeight, _scrollY)
                : new VisibleRange(0, _screen.Items.Length);

            // Thu hồi dòng đã ra khỏi tầm nhìn TRƯỚC, để tái dùng ngay trong lượt này.
            List<int> gone = null;
            foreach (var pair in _realized)
            {
                if (!Visible.Contains(pair.Key)) (gone ??= new List<int>()).Add(pair.Key);
            }

            if (gone != null)
            {
                foreach (var index in gone) Recycle(index);
            }

            for (var i = Visible.First; i < Visible.LastExclusive; i++)
            {
                if (_realized.ContainsKey(i)) continue;
                Realize(i);
            }
        }

        private void Realize(int index)
        {
            var row = _pool.Count > 0 ? _pool.Pop() : MenuItemRow.Create(_rowParent, _font);
            if (row.transform.parent != _rowParent) row.transform.SetParent(_rowParent, false);
            row.gameObject.SetActive(true);
            row.Bind(_screen.Items[index], index, _assets, OnRowClicked, _compactCards);
            row.SetLightCard(_lightCards);
            row.SetSeparatorVisible(index < _screen.Items.Length - 1);

            var rect = (RectTransform)row.transform;
            rect.sizeDelta = new Vector2(_compactCards ? -12f : 0f,
                _compactCards ? MenuItemRow.Height - 6f : MenuItemRow.Height);
            rect.anchoredPosition = new Vector2(_compactCards ? 6f : 0f,
                -MenuVirtualizer.OffsetOf(index, RowStep) - (_compactCards ? 3f : 0f));

            _realized[index] = row;
        }

        private void RecycleAll()
        {
            if (_realized.Count == 0) return;

            var indices = new List<int>(_realized.Keys);
            foreach (var index in indices) Recycle(index);
        }

        private void Recycle(int index)
        {
            var row = _realized[index];
            _realized.Remove(index);

            if (row == null) return;

            row.gameObject.SetActive(false);
            _pool.Push(row);
        }

        private void OnInteractiveScroll()
        {
            if (_content == null) return;
            _scrollY = Mathf.Max(0f, _content.anchoredPosition.y);
            Refresh();
        }

        private void UpdateContentHeight()
        {
            if (_content == null || _screen == null) return;
            _content.sizeDelta = new Vector2(0f,
                // Trừ khe hở sau dòng cuối — không có dòng nào bên dưới để cách.
                Mathf.Max(MenuVirtualizer.ContentHeight(_screen.Items.Length, RowStep)
                    - (_screen.Items.Length > 0 ? _rowGap : 0f), _viewportHeight));
        }

        /// <summary>Cho test và cho input bàn phím gọi thẳng, không phải qua chuột.</summary>
        public void OnRowClicked(int index)
        {
            if (SelectionOverride != null && SelectionOverride(index)) return;
            switch (MenuSelection.Decide(_screen, index))
            {
                case MenuAction.Blocked:
                    BlockedRowTapped?.Invoke(index);
                    return;

                case MenuAction.Confirm:
                    ConfirmRequested?.Invoke(MenuSelection.PromptFor(_screen, index), () => SendSelection(index));
                    return;

                case MenuAction.Send:
                    SendSelection(index);
                    return;
            }
        }

        private void SendSelection(int index)
        {
            _guider.Select(_screen, index);

            if (MenuSelection.ShouldCloseAfter(_screen, index))
            {
                CloseRequested?.Invoke(this);
            }
        }
    }
}
