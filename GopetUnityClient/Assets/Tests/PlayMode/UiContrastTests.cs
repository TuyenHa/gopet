using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Chữ phải tương phản với nền ngay phía sau nó.
    ///
    /// <para><b>Vì sao cần một test cho chuyện tưởng như trang trí:</b> <c>Image</c> và
    /// <c>Text</c> của uGUI đều mặc định màu TRẮNG. Dựng view bằng code mà quên đặt màu
    /// thì ra panel trắng + chữ trắng — <b>màn hình trắng trơn</b>, không lỗi, không
    /// cảnh báo. Toàn bộ 59 test PlayMode vẫn xanh vì chúng đọc <c>text</c> chứ không
    /// nhìn màu. Đã trả giá: người dùng bấm Play và không thấy gì.</para>
    /// </summary>
    public sealed class UiContrastTests
    {
        /// <summary>Chênh lệch độ sáng tối thiểu giữa chữ và nền, thang 0..1.</summary>
        private const float MinContrast = 0.25f;

        private GameObject _canvas;

        [SetUp]
        public void SetUp()
        {
            _canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
        }

        [TearDown]
        public void TearDown()
        {
            if (_canvas != null) Object.DestroyImmediate(_canvas);
        }

        /// <summary>Font trả về <c>null</c> thì KHÔNG MỘT CHỮ NÀO được vẽ.</summary>
        [Test]
        public void FontMacDinh_LayDuoc()
        {
            Assert.IsNotNull(UiBuilder.DefaultFont(),
                "Không có font thì Text.font = null và màn hình trống trơn, không lỗi nào báo.");
        }

        [Test]
        public void BieuMauDangNhap_ChuDocDuocTrenNen()
        {
            var form = FormView.Create(_canvas.transform, UiBuilder.DefaultFont());
            form.Bind("Đăng nhập", new[] { "Tài khoản", "Mật khẩu" }, new[] { "Đăng nhập", "Quên" });
            form.SetNotice("Sai mật khẩu");

            AssertReadable(form.gameObject);
        }

        [Test]
        public void HopThoai_ChuDocDuocTrenNen()
        {
            var dialog = ChoiceDialogView.Create(_canvas.transform, UiBuilder.DefaultFont());
            dialog.Bind("Chắc chưa?", new[] { "Đồng ý", "Thôi" });

            AssertReadable(dialog.gameObject);
        }

        [Test]
        public void DongMenu_ChuDocDuocTrenNen()
        {
            var row = MenuItemRow.Create(_canvas.transform, UiBuilder.DefaultFont());
            row.Bind(new MenuItemInfo { Title = "Đổi 2.400 (vàng)", Description = "mô tả", CanSelect = true },
                     0, null, _ => { });

            AssertReadable(row.gameObject);
        }

        /// <summary>Mọi <c>Text</c> bên dưới <paramref name="root"/> phải nổi trên nền gần nhất của nó.</summary>
        private static void AssertReadable(GameObject root)
        {
            var texts = root.GetComponentsInChildren<Text>(true);
            Assert.Greater(texts.Length, 0, "Không có chữ nào để kiểm.");

            foreach (var text in texts)
            {
                if (text.color.a < 0.05f) continue;

                var background = NearestBackground(text.transform);
                Assert.IsNotNull(background, $"\"{text.name}\" không nằm trên nền nào — sẽ đọc trên nền bất kỳ.");

                var gap = Mathf.Abs(Luminance(text.color) - Luminance(background.color));
                Assert.GreaterOrEqual(gap, MinContrast,
                    $"\"{text.name}\" gần như trùng màu nền ({gap:F2} < {MinContrast}) — nhìn ra màn hình trống.");
            }
        }

        /// <summary>Ảnh nền đục gần nhất tính từ chính nó đi ngược lên.</summary>
        private static Image NearestBackground(Transform node)
        {
            for (var current = node; current != null; current = current.parent)
            {
                var image = current.GetComponent<Image>();
                if (image != null && image.color.a > 0.5f) return image;
            }

            return null;
        }

        private static float Luminance(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
    }
}
