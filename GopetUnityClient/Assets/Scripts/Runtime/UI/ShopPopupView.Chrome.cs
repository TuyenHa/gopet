using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Phần dựng hình riêng của <see cref="ShopPopupView"/>: khay tab và 4 tab. Khung,
    /// badge tiêu đề, nút X và băng chân lấy từ <see cref="GamePopupFrame"/>.
    /// </summary>
    public sealed partial class ShopPopupView
    {
        private const string DefaultFooter = "Chạm nút giá để mua món bạn chọn.";

        private GamePopupFrame _frame;
        private PopupTabRail _rail;
        private PopupItemList _list;

        public static ShopPopupView Create(Transform parent, Font font,
            GuiderHandler guider, RemoteAssetCache assets)
        {
            var frame = GamePopupFrame.Create(parent, font, "Cửa hàng", footer: DefaultFooter);
            frame.gameObject.name = "ShopPopup";

            var view = frame.gameObject.AddComponent<ShopPopupView>();
            view._frame = frame;
            view._guider = guider;
            view._assets = assets;
            view._font = font;
            frame.Closed += () => view.Closed?.Invoke();

            var labels = new string[Tabs.Length];
            for (var i = 0; i < Tabs.Length; i++) labels[i] = Tabs[i].Label;
            view._rail = PopupTabRail.Create(frame.Content, font, frame.ContentWidth, labels);
            view._rail.Selected += index => view.SelectTab(Tabs[index].Id);

            view.BuildList(frame.Content);
            view._list.Activated += view.TryBuy;

            view._rail.Select(0);
            return view;
        }

    }
}
