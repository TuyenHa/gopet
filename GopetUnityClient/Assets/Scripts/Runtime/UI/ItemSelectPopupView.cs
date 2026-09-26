using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup "chọn một dòng" dùng chung cho các menu <c>TYPE_MENU_SELECT_ELEMENT</c> của
    /// server: nguyên liệu hình xăm, tẩy gym… Dựng trên <see cref="GamePopupFrame"/> +
    /// <see cref="PopupItemList"/> nên cùng khung, thẻ item và vạch ngăn với Cửa hàng.
    /// Tiêu đề, chữ trên nút và câu chân popup tra theo listId trong <see cref="Specs"/>.
    ///
    /// <para>Dòng nào server cũng bật <c>showDialog</c> ("Bạn có muốn chọn…"), nên bấm
    /// nút đi qua hộp xác nhận y như <see cref="GenericMenuView"/>.</para>
    /// </summary>
    public sealed class ItemSelectPopupView : MonoBehaviour
    {
        private sealed class Spec
        {
            public string Title, Action, Footer, Empty;
            /// <summary>Tên dòng là tên ngọc thô ("ngọc lửa up: 0 Tăng 5% (hp)…") — tách như Kho ngọc.</summary>
            public bool GemNames;
            /// <summary>Cỡ khung; mặc định như mọi <see cref="GamePopupFrame"/>.</summary>
            public float Width = GamePopupFrame.DefaultWidth, Height = GamePopupFrame.DefaultHeight;
        }

        private static readonly Spec Material = new Spec
        {
            Title = "Nguyên liệu", Action = "Chọn",
            Footer = "Chạm \"Chọn\" để dùng nguyên liệu.",
            Empty = "Bạn chưa có nguyên liệu phù hợp."
        };

        // listId lấy từ MenuController.cs của server.
        private static readonly Dictionary<int, Spec> Specs = new Dictionary<int, Spec>
        {
            // MENU_SELECT_ITEM_GEN_TATTO / REMOVE_TATTO / MATERIAL1,2_TO_ENCHANT_TATOO
            { 1014, Material }, { 1015, Material }, { 1046, Material }, { 1047, Material },
            // MENU_SELECT_GEM_TO_INLAY
            { 1023, new Spec
                {
                    Title = "Nguyên liệu", Action = "Gắn", GemNames = true,
                    Footer = "Chạm \"Gắn\" để gắn ngọc vào trang bị.",
                    Empty = "Bạn chưa có ngọc nào để gắn."
                }
            },
            // MENU_NORMAL_INVENTORY — mở từ ô trang bị pet còn trống. Tab Nhân vật của
            // Hành lý nhận menu này trước (lưới rương đồ) nên không tới được đây.
            { 81004, new Spec
                {
                    Title = "Rương đồ", Action = "Dùng",
                    Footer = "Chạm \"Dùng\" để sử dụng vật phẩm.",
                    Empty = "Rương đồ đang trống."
                }
            },
            // MENU_DELETE_TIEM_NANG
            { 800, new Spec
                {
                    Title = "Tẩy gym", Action = "Tẩy", Width = 340f, Height = 250f,
                    Footer = "Tẩy 1 điểm chỉ số để nhận lại 1 điểm tiềm năng.",
                    Empty = "Pet chưa có điểm gym nào để tẩy."
                }
            },
        };

        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private GamePopupFrame _frame;
        private PopupItemList _list;
        private MenuScreen _screen;
        private Spec _spec;

        public event Action Closed;

        /// <summary>Hỏi trước khi chọn. <see cref="UiRoot"/> dựng hộp thoại đè lên popup.</summary>
        public event Action<MenuSelection.ConfirmPrompt, Action> ConfirmRequested;

        public int RowCount => _list == null ? 0 : _list.RowCount;
        public int ListId => _screen == null ? 0 : _screen.ListId;

        public static bool Handles(MenuScreen screen) => screen != null && Specs.ContainsKey(screen.ListId);

        /// <summary>Cùng tiêu đề/nút với popup đang mở thì chỉ bind lại, khỏi dựng khung mới.</summary>
        public bool CanBind(MenuScreen screen) => Handles(screen) && ReferenceEquals(Specs[screen.ListId], _spec);

        /// <summary>Khung dựng theo <paramref name="screen"/>; gọi <see cref="Bind"/> ngay sau đó.</summary>
        public static ItemSelectPopupView Create(Transform parent, Font font, MenuScreen screen,
            GuiderHandler guider, RemoteAssetCache assets)
        {
            if (!Handles(screen)) throw new ArgumentException("Menu không thuộc popup chọn dòng.", nameof(screen));
            var spec = Specs[screen.ListId];
            var frame = GamePopupFrame.Create(parent, font, spec.Title, spec.Width, spec.Height,
                footer: string.Empty);
            frame.gameObject.name = "ItemSelectPopup";
            if (spec.Width < GamePopupFrame.DefaultWidth) frame.UseCompactChrome();

            var view = frame.gameObject.AddComponent<ItemSelectPopupView>();
            view._frame = frame;
            view._spec = spec;
            view._guider = guider;
            view._assets = assets;
            frame.Closed += () => view.Closed?.Invoke();

            view._list = PopupItemList.Create(frame.Content, font);
            view._list.Activated += view.OnRowClicked;
            return view;
        }

        /// <summary>Bản nhúng (tab Pet của Hành lý): chỉ danh sách, không khung/băng chân.
        /// Gọi <see cref="Bind"/> ngay sau đó như bản popup.</summary>
        public static ItemSelectPopupView CreateEmbedded(Transform host, Font font, MenuScreen screen,
            GuiderHandler guider, RemoteAssetCache assets)
        {
            if (!Handles(screen)) throw new ArgumentException("Menu không thuộc popup chọn dòng.", nameof(screen));
            var go = new GameObject("ItemSelectEmbedded", typeof(RectTransform));
            go.transform.SetParent(host, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var view = go.AddComponent<ItemSelectPopupView>();
            view._spec = Specs[screen.ListId];
            view._guider = guider;
            view._assets = assets;
            view._list = PopupItemList.Create(go.transform, font);
            view._list.Activated += view.OnRowClicked;
            return view;
        }

        public void Bind(MenuScreen screen)
        {
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
            _list.Bind(Readable(screen, _spec.GemNames), _assets, _spec.Action);
            var empty = screen.Items == null || screen.Items.Length == 0;
            if (empty) _list.ShowPlaceholder(_spec.Empty);
            if (_frame != null) _frame.SetFooter(_spec.Footer);
        }

        /// <summary>Công khai để test gọi thẳng, khỏi phải mò <c>Button</c> trong cây GameObject.</summary>
        public void OnRowClicked(int index)
        {
            if (_screen == null || _guider == null) return;
            if (index < 0 || index >= _screen.Items.Length) return;

            switch (MenuSelection.Decide(_screen, index))
            {
                case MenuAction.Confirm when ConfirmRequested != null:
                    ConfirmRequested(MenuSelection.PromptFor(_screen, index), () => Send(index));
                    return;
                case MenuAction.Confirm:
                case MenuAction.Send:
                    Send(index);
                    return;
            }
        }

        private void Send(int index)
        {
            var screen = _screen;
            _guider.Select(screen, index);
            if (MenuSelection.ShouldCloseAfter(screen, index)) Closed?.Invoke();
        }

        /// <summary>
        /// Bản sao chỉ để HIỂN THỊ, đã đổi tag J2ME ("(str)", "(vang)") thành chữ. Bản gốc
        /// giữ nguyên cho <see cref="Send"/> và hộp xác nhận.
        /// </summary>
        private static MenuScreen Readable(MenuScreen screen, bool gemNames)
        {
            var items = screen.Items ?? Array.Empty<MenuItemInfo>();
            var copy = new MenuItemInfo[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    copy[i] = new MenuItemInfo();
                    continue;
                }
                var title = JarIconTokens.Humanize(item.Title);
                var description = JarIconTokens.Humanize(item.Description);
                if (gemNames)
                {
                    // Hiệu ứng nằm lẫn trong tên → đưa xuống dòng mô tả, tên chỉ còn "Ngọc lửa".
                    var gem = GemItemText.Parse(item.Title);
                    title = gem.Name;
                    if (!string.IsNullOrEmpty(gem.Effects))
                        description = string.IsNullOrEmpty(description) ? gem.Effects : gem.Effects + "\n" + description;
                }
                copy[i] = new MenuItemInfo
                {
                    ItemId = item.ItemId,
                    ImagePath = item.ImagePath,
                    Title = title,
                    Description = description,
                    CanSelect = item.CanSelect,
                    PaymentOptions = item.PaymentOptions
                };
            }
            return new MenuScreen { ListId = screen.ListId, Type = screen.Type, Title = screen.Title, Items = copy };
        }
    }
}
