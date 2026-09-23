using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Font UI là Be Vietnam Pro, chữ đậm là file SemiBold thật.
    ///
    /// <para>Thiếu asset thì <c>UiBuilder</c> lùi về Arial / đậm giả mà vẫn có chữ —
    /// không test hiển thị nào khác bắt được. Các test này bắt việc file font bị
    /// xoá/đổi tên/dời chỗ khỏi <c>Resources/Fonts/BeVietnamPro</c>.</para>
    /// </summary>
    public sealed class UiFontTests
    {
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("Host", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        [Test]
        public void FontMacDinh_LaBeVietnamPro()
        {
            StringAssert.Contains("BeVietnamPro", UiBuilder.DefaultFont().name,
                "UI đang lùi về font dựng sẵn — kiểm Resources/Fonts/BeVietnamPro.");
        }

        [Test]
        public void ChuDam_DungFileSemiBoldThat()
        {
            var text = UiBuilder.MakeText(_host.transform, UiBuilder.DefaultFont(), "Label", 14, false);

            UiBuilder.SetFontStyle(text, FontStyle.Bold);

            StringAssert.Contains("SemiBold", text.font.name, "Chữ đậm phải dùng file SemiBold.");
            Assert.AreEqual(FontStyle.Normal, text.fontStyle,
                "Đã có file SemiBold thì không được chồng thêm đậm giả của Unity.");
        }

        [Test]
        public void BoDam_TraVeFontMacDinh()
        {
            var text = UiBuilder.MakeText(_host.transform, UiBuilder.DefaultFont(), "Label", 14, false);

            UiBuilder.SetFontStyle(text, FontStyle.Bold);
            UiBuilder.SetFontStyle(text, FontStyle.Normal);

            Assert.AreSame(UiBuilder.DefaultFont(), text.font);
            Assert.AreEqual(FontStyle.Normal, text.fontStyle);
        }

        [Test]
        public void ChuDamTheGioi_DoiMaterialTheoFont()
        {
            var go = new GameObject("Mesh", typeof(TextMesh));
            go.transform.SetParent(_host.transform, false);
            var mesh = go.GetComponent<TextMesh>();
            mesh.font = UiBuilder.DefaultFont();

            UiBuilder.SetFontStyle(mesh, FontStyle.Bold);

            Assert.AreSame(mesh.font.material, go.GetComponent<MeshRenderer>().sharedMaterial,
                "TextMesh vẽ bằng material của font — lệch font/material là ra chữ rác.");
        }
    }
}
