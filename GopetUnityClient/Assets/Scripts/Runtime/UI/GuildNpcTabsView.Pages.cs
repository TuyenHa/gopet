using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Ba trang nội dung tự dựng của popup Bang hội: nút vào khu vực, form tạo bang
    /// và bảng TOP LVL. Các tab còn lại để server bơm màn hình như cũ.
    /// </summary>
    public sealed partial class GuildNpcTabsView
    {
        /// <summary>
        /// <c>MenuController.INPUT_DIALOG_CREATE_CLAN</c>. Handler của server đọc tên
        /// bang từ chính gói gửi lên nên client submit thẳng được, không cần server mở
        /// hộp thoại trước. GIỮ khớp server.
        /// </summary>
        public const int CreateClanDialogId = 14;

        private RectTransform _enterPage;
        private RectTransform _createPage;
        private RectTransform _topPage;
        private PopupInputForm _createForm;
        private GenericMenuView _topList;

        private void BuildPages(RectTransform content)
        {
            _enterPage = BuildEnterPage(content);
            _createPage = BuildCreatePage(content);
            _topPage = BuildTopPage(content);
            ShowPage(null);
        }

        /// <summary>Hiện đúng một trang; <c>null</c> = giấu hết (chờ server bơm màn hình).</summary>
        private void ShowPage(RectTransform page)
        {
            _enterPage.gameObject.SetActive(_enterPage == page);
            _createPage.gameObject.SetActive(_createPage == page);
            _topPage.gameObject.SetActive(_topPage == page);
        }

        /// <summary>Khung trắng chiếm hết chỗ dưới khay tab — nền chung của mọi trang.</summary>
        private static RectTransform MakePanel(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -(PopupTabRail.Height + PopupTabRail.Gap));
            RoundedBorder.Apply(go, RoundedUiSprite.DefaultRadius, PopupPalette.ListBg,
                PopupPalette.Hairline);

            // Nội dung đặt vào một lớp con: RoundedBorder dựng nền bằng một con tên
            // "Fill", trang nào dọn con của panel sẽ ăn mất nền.
            var inner = new GameObject("Content", typeof(RectTransform));
            inner.transform.SetParent(go.transform, false);
            UiBuilder.Stretch((RectTransform)inner.transform);
            return rect;
        }

        private static RectTransform Inner(RectTransform panel) =>
            (RectTransform)panel.Find("Content");

        private RectTransform BuildEnterPage(RectTransform content)
        {
            var panel = MakePanel(content, "Page_Enter");
            var inner = Inner(panel);

            var desc = UiBuilder.MakeText(inner, _font, "Desc", 13, false);
            desc.text = "Khu vực riêng của bang hội bạn đang ở.";
            desc.alignment = TextAnchor.MiddleCenter;
            desc.color = PopupPalette.TextMuted;
            UiBuilder.PlaceRow(desc.rectTransform, 40f, 22f, 20f);

            var btnGo = new GameObject("EnterBtn", typeof(RectTransform), typeof(Image),
                typeof(Button));
            btnGo.transform.SetParent(inner, false);
            var rect = (RectTransform)btnGo.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(150f, 34f);
            rect.anchoredPosition = new Vector2(0f, -6f);

            var image = btnGo.GetComponent<Image>();
            RoundedUiSprite.Apply(image, 6f);
            image.color = PopupPalette.ButtonBlue;

            var label = UiBuilder.MakeText(btnGo.transform, _font, "Label", 13, true);
            label.text = "Vào khu vực";
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;

            // Chỉ khi BẤM mới gửi: chuyển tab sang đây mà dịch chuyển luôn là người
            // chơi bị kéo khỏi map chỉ vì bấm xem.
            btnGo.GetComponent<Button>().onClick.AddListener(() =>
                OptionChosen?.Invoke(Gopet.Net.Npc.LinhThuCityNpcOptions.SuGiaVaoKhuVucBang));
            return panel;
        }

        private RectTransform BuildCreatePage(RectTransform content)
        {
            var panel = MakePanel(content, "Page_Create");
            _createForm = PopupInputForm.Create(Inner(panel), _font, "Tạo bang hội mới",
                "Tên bang:", "Tên bang phải từ 5 đến 20 ký tự, chỉ chữ thường và số.");
            // View không giữ GuiderHandler — UiRoot nối sự kiện này vào SubmitInput,
            // cùng cách OptionChosen đang làm.
            _createForm.Submitted += name => CreateClanSubmitted?.Invoke(name);
            _createForm.Cancelled += () => _rail.Select(0);
            return panel;
        }

        private RectTransform BuildTopPage(RectTransform content)
        {
            var panel = MakePanel(content, "Page_Top");
            _topList = GenericMenuView.Create(Inner(panel), _font);
            _topList.SetLightCards(true);
            _topList.EnableEmbeddedScroll(Height - GamePopupFrame.ContentTop - 36f
                                          - PopupTabRail.Height - PopupTabRail.Gap);
            return panel;
        }
    }
}
