using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Popup cửa hàng có 4 tab (Vũ khí / Giáp / Mũ / Thức ăn) và nút X đóng —
    /// mô phỏng ảnh mẫu top-right.
    ///
    /// <para><b>Tận dụng hạ tầng có sẵn:</b> vùng danh sách item bên trong popup
    /// dùng thẳng <see cref="GenericMenuView"/> — không đẻ ra renderer riêng cho
    /// shop. Khi đổi tab, popup <see cref="GuiderHandler.RequestShop"/> shopId mới,
    /// và <see cref="TryConsumeMenu"/> đón gói <see cref="MenuScreen"/> tương ứng
    /// để bind vào view con.</para>
    ///
    /// <para>Cần <see cref="UiRoot"/> gọi <see cref="TryConsumeMenu"/> khi
    /// <see cref="GuiderHandler.MenuShown"/> bắn ra — nếu đúng của shop popup thì
    /// <b>swallow</b> để không đẻ thêm <see cref="GenericMenuView"/> đứng ngoài.</para>
    /// </summary>
    public sealed class ShopPopupView : MonoBehaviour
    {
        // Server ID: MenuController.cs:423-435. GIỮ khớp với server, đừng đổi.
        public const sbyte ShopWeapon = 1;
        public const sbyte ShopArmour = 2;
        public const sbyte ShopHat = 3;
        public const sbyte ShopFood = 4;
        /// <summary>SHOP_SKIN. Server có quirk: SHOP_FOOD ở map 19 trả về SHOP_SKIN (GameController.cs:2367).</summary>
        public const sbyte ShopSkin = 7;

        // Kích thước tính theo ref canvas 720×1280 (GopetBootstrap.Unity.cs:29). Game
        // chạy landscape nên chiều CAO khả kiến chỉ ~430 ref-unit — popup PHẢI thấp
        // hơn ngưỡng đó nhiều, không thì tràn ra ngoài khung hình.
        private const float PopupWidth = 380f;
        private const float PopupHeight = 220f;
        private const float TabHeight = 32f;
        /// <summary>Cạnh nút X (chờm ra ngoài góc trên-phải popup).</summary>
        private const float CloseSize = 34f;

        // Bảng màu bám ảnh mẫu — trắng sáng + viền xanh sinh động, giống bảng màu
        // game (bright cheerful pet). Yellow cho tab active để bật lên rõ.
        private static readonly Color TabActive = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color TabInactive = new Color(0.35f, 0.65f, 1f, 1f);
        private static readonly Color TabText = new Color(0.14f, 0.24f, 0.44f, 1f);
        private static readonly Color PanelBg = new Color(0.96f, 0.98f, 1f, 1f);
        private static readonly Color PanelBorder = new Color(0.28f, 0.6f, 1f, 1f);

        private static readonly (sbyte Id, string Label)[] Tabs =
        {
            (ShopWeapon, "Vũ khí"),
            (ShopArmour, "Giáp"),
            (ShopHat,    "Mũ"),
            (ShopFood,   "Thức ăn"),
        };

        private GuiderHandler _guider;
        private RemoteAssetCache _assets;
        private Font _font;
        private GenericMenuView _menuView;
        private Image[] _tabBackgrounds;
        private Text[] _tabLabels;
        private sbyte _activeShopId = -1;

        public event Action Closed;

        public sbyte ActiveShopId => _activeShopId;

        public static ShopPopupView Create(Transform parent, Font font,
            GuiderHandler guider, RemoteAssetCache assets)
        {
            var frame = new GameObject("ShopPopup", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(parent, false);

            var frameRect = (RectTransform)frame.transform;
            // GIỮA MÀN HÌNH — popup shop là dialog chính, không phải sidebar.
            frameRect.anchorMin = new Vector2(0.5f, 0.5f);
            frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(PopupWidth, PopupHeight);
            frameRect.anchoredPosition = Vector2.zero;

            var bg = frame.GetComponent<Image>();
            // Bo góc panel bằng sprite 9-slice trắng, tô màu qua Image.color — không
            // cần asset riêng, và bo mượt trên mọi kích thước.
            RoundedUiSprite.Apply(bg);
            bg.color = PanelBg;

            var view = frame.AddComponent<ShopPopupView>();
            view._guider = guider;
            view._assets = assets;
            view._font = font;

            view.BuildTabs(frame.transform);
            view.BuildCloseButton(frame.transform);
            view.BuildBody(frame.transform);
            view.BuildBorder(frame.transform);

            view.SelectTab(ShopWeapon);
            return view;
        }

        /// <summary>
        /// Được gọi bởi <see cref="UiRoot"/> mỗi khi <see cref="GuiderHandler.MenuShown"/>
        /// bắn ra. Nếu <paramref name="screen"/> thuộc HỌ shop (bất kỳ shopId nào)
        /// thì popup <b>swallow</b> — kể cả khi listId KHÔNG khớp tab active. Lý do:
        /// <list type="bullet">
        /// <item>Chống race đổi tab: user bấm Vũ khí → server đang trả, user bấm tiếp
        /// Mũ. Gói Vũ khí về sau, khớp shop family nhưng khác <c>_activeShopId</c> — nếu
        /// không nuốt, <see cref="UiRoot"/> đẻ <see cref="GenericMenuView"/> đè popup.</item>
        /// <item>Server quirk: <c>SHOP_FOOD</c> ở map 19 trả về <c>SHOP_SKIN=7</c>
        /// (<c>GameController.cs:2367-2371</c>) — không có tab nhưng vẫn thuộc họ shop.</item>
        /// </list>
        /// Bind vào <see cref="GenericMenuView"/> chỉ khi listId KHỚP tab đang chọn;
        /// còn lại swallow im lặng. Server đặt <c>listId = shopId</c> khi
        /// <c>showShop(...)</c> (<c>MenuController.cs:875</c>).
        /// </summary>
        public bool TryConsumeMenu(MenuScreen screen)
        {
            if (screen == null) return false;
            if (!IsShopFamily(screen.ListId)) return false;

            if (screen.ListId == _activeShopId)
            {
                _menuView.Bind(screen, _assets, _guider);
                _menuView.SetViewport(PopupHeight - TabHeight - 16f);
            }
            return true;
        }

        private static bool IsShopFamily(int listId) =>
            listId == ShopWeapon || listId == ShopArmour || listId == ShopHat
            || listId == ShopFood || listId == ShopSkin;

        private void BuildTabs(Transform parent)
        {
            _tabBackgrounds = new Image[Tabs.Length];
            _tabLabels = new Text[Tabs.Length];

            // Nút X CHỜM RA NGOÀI góc trên-phải popup, không chiếm chỗ trong hàng tab
            // nữa — tab dùng toàn bộ chiều rộng.
            const float sidePadding = 6f;
            const float tabGap = 3f;
            var usableWidth = PopupWidth - sidePadding * 2f;
            var tabWidth = (usableWidth - tabGap * (Tabs.Length - 1)) / Tabs.Length;

            for (var i = 0; i < Tabs.Length; i++)
            {
                var tabGo = new GameObject($"Tab_{Tabs[i].Label}", typeof(RectTransform),
                    typeof(Image), typeof(Button));
                tabGo.transform.SetParent(parent, false);

                var rect = (RectTransform)tabGo.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(tabWidth, TabHeight);
                rect.anchoredPosition = new Vector2(sidePadding + i * (tabWidth + tabGap), -sidePadding);

                _tabBackgrounds[i] = tabGo.GetComponent<Image>();
                // Bo góc mềm giống panel — tabs không còn cạnh vuông cứng.
                RoundedUiSprite.Apply(_tabBackgrounds[i]);
                _tabBackgrounds[i].color = TabInactive;

                var label = UiBuilder.MakeText(tabGo.transform, _font, "Label", 12,
                    stretch: true);
                label.alignment = TextAnchor.MiddleCenter;
                label.text = Tabs[i].Label;
                label.color = TabText;
                label.fontStyle = FontStyle.Bold;
                _tabLabels[i] = label;

                var capturedId = Tabs[i].Id;
                tabGo.GetComponent<Button>().onClick.AddListener(() => SelectTab(capturedId));
            }
        }

        private void BuildCloseButton(Transform parent)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            // CHỜM RA NGOÀI góc trên-phải: pivot ở TÂM để nút nằm nửa trong nửa ngoài
            // rìa popup — style dialog game bo góc quen thuộc.
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(CloseSize, CloseSize);
            rect.anchoredPosition = new Vector2(4f, 4f);

            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                // Không có sprite: nền tròn đỏ + chữ X trắng.
                img.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var xLabel = UiBuilder.MakeText(go.transform, _font, "X", 18, stretch: true);
                xLabel.alignment = TextAnchor.MiddleCenter;
                xLabel.text = "×";
                xLabel.color = Color.white;
                xLabel.fontStyle = FontStyle.Bold;
            }

            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }

        private void BuildBody(Transform parent)
        {
            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(parent, false);

            var rect = (RectTransform)body.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(6f, 6f);
            rect.offsetMax = new Vector2(-6f, -TabHeight - 8f);

            var bodyImg = body.GetComponent<Image>();
            RoundedUiSprite.Apply(bodyImg);
            bodyImg.color = new Color(0.99f, 1f, 1f, 1f);

            _menuView = GenericMenuView.Create(body.transform, _font);
        }

        private void BuildBorder(Transform parent)
        {
            // Viền mỏng bằng Outline component — không cần asset.
            var outline = parent.gameObject.AddComponent<Outline>();
            outline.effectColor = PanelBorder;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private void SelectTab(sbyte shopId)
        {
            if (_activeShopId == shopId) return;

            _activeShopId = shopId;
            for (var i = 0; i < Tabs.Length; i++)
            {
                _tabBackgrounds[i].color = Tabs[i].Id == shopId ? TabActive : TabInactive;
            }

            // Server sẽ trả về MenuScreen, UiRoot forward vào TryConsumeMenu.
            _guider.RequestShop(shopId);
        }
    }
}
