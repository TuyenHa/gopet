using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Việc ÁP layout vào RectTransform thật. Toán học của <see cref="PixelCanvasLayout"/>
    /// đã tự test bằng xunit — ở đây chỉ kiểm việc gắn kết quả đó vào Unity không sai,
    /// và không phụ thuộc <c>Screen.width/height</c> thật (giá trị đó trong batchmode
    /// phụ thuộc Editor Game View, không đoán trước được).
    /// </summary>
    public sealed class PixelCanvasTests
    {
        private GameObject _host;
        private PixelCanvas _canvas;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("Host", typeof(RectTransform));
            _canvas = PixelCanvas.Create(_host.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        [Test]
        public void ApplyLayout_DatDungKichThuocVaTiLeContent()
        {
            var layout = PixelCanvasLayout.Compute(2400f, 1080f);
            _canvas.ApplyLayout(layout);

            Assert.AreEqual(600f, _canvas.Content.sizeDelta.x, 0.01f);
            Assert.AreEqual(240f, _canvas.Content.sizeDelta.y, 0.01f);
            Assert.AreEqual(4f, _canvas.Content.localScale.x, 0.01f);
            Assert.AreEqual(4f, _canvas.Content.localScale.y, 0.01f);
        }

        /// <summary>Content luôn neo giữa — đây là cách "canh giữa theo chiều dọc" thành hình thật, không chỉ là con số.</summary>
        [Test]
        public void Content_NeoGiuaManHinh()
        {
            _canvas.ApplyLayout(PixelCanvasLayout.Compute(1920f, 1080f));

            Assert.AreEqual(new Vector2(0.5f, 0.5f), _canvas.Content.anchorMin);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), _canvas.Content.anchorMax);
            Assert.AreEqual(Vector2.zero, _canvas.Content.anchoredPosition);
        }

        /// <summary>Đổi từ máy rộng sang máy khác thì Content phải cập nhật lại, không giữ layout cũ.</summary>
        [Test]
        public void ApplyLayoutLaiVoiMayKhac_CapNhatDungTiLeMoi()
        {
            _canvas.ApplyLayout(PixelCanvasLayout.Compute(1920f, 1080f));
            _canvas.ApplyLayout(PixelCanvasLayout.Compute(2732f, 2048f));

            Assert.AreEqual(8f, _canvas.Content.localScale.x, 0.01f);
            Assert.AreEqual(341.5f, _canvas.Content.sizeDelta.x, 0.01f);
        }

        /// <summary>Toạ độ gốc trên-trái của jar (0,0) phải đổi thành góc trên-trái thật của khung logic.</summary>
        [Test]
        public void ToAnchoredPosition_GocTrenTrai_RaDungGocKhungLogic()
        {
            var layout = PixelCanvasLayout.Compute(1920f, 1080f); // logicalWidth = 480
            _canvas.ApplyLayout(layout);

            var topLeft = _canvas.ToAnchoredPosition(0f, 0f);

            Assert.AreEqual(-240f, topLeft.x, 0.01f); // -logicalWidth/2
            Assert.AreEqual(120f, topLeft.y, 0.01f);  // +240/2
        }

        /// <summary>Tâm khung jar (logicalWidth/2, 120) phải đổi thành đúng gốc uGUI (0,0).</summary>
        [Test]
        public void ToAnchoredPosition_TamKhung_RaGocUgui()
        {
            var layout = PixelCanvasLayout.Compute(1920f, 1080f);
            _canvas.ApplyLayout(layout);

            var center = _canvas.ToAnchoredPosition(layout.LogicalWidth / 2f, 120f);

            Assert.AreEqual(0f, center.x, 0.01f);
            Assert.AreEqual(0f, center.y, 0.01f);
        }

        /// <summary>Tạo xong đã có sẵn một layout hợp lệ ngay — không phải đợi frame đầu chạy Update().</summary>
        [Test]
        public void TaoXong_DaCoLayoutHopLeNgay()
        {
            Assert.Greater(_canvas.Layout.Scale, 0);
            Assert.Greater(_canvas.Content.sizeDelta.x, 0f);
        }

        /// <summary>Canvas riêng phải đặt sortingOrder tường minh — hai Canvas cùng mặc định 0 thì thứ tự vẽ không đảm bảo.</summary>
        [Test]
        public void Canvas_DatSortingOrderTuongMinh()
        {
            var canvas = _canvas.GetComponent<Canvas>();

            Assert.AreEqual(PixelCanvas.SortingOrder, canvas.sortingOrder);
        }
    }
}
