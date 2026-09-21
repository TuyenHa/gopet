using System.Collections;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Tab "Soạn thư": ô nội dung kéo xuống sát hàng nút, nút cách mép dưới đúng 10, và
    /// gõ dài thì chữ CUỘN trong ô chứ không tràn ra ngoài viền.
    /// </summary>
    public sealed class ComposeLetterTests : MailboxTestBase
    {
        private const float BottomMargin = 10f;

        /// <summary>Mở hộp thư rồi chuyển sang tab cuối ("Soạn thư").</summary>
        private ComposeLetterView OpenComposeTab()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            var rail = view.GetComponentInChildren<PopupTabRail>(true);
            rail.Select(rail.TabCount - 1);
            return view.GetComponentInChildren<ComposeLetterView>(true);
        }

        [UnityTest]
        public IEnumerator NutGuiHuy_CachLeDuoi10()
        {
            var compose = OpenComposeTab();
            yield return null;

            var composeRect = WorldRect((RectTransform)compose.transform);
            var buttons = WorldRect((RectTransform)compose.transform.Find("Buttons"));

            Assert.AreEqual(BottomMargin, buttons.yMin - composeRect.yMin, 0.5f,
                "Hàng nút phải cách mép dưới vùng soạn đúng 10.");
        }

        /// <summary>Ô nội dung phải kéo xuống gần hàng nút, không để thừa mảng trống ở đáy.</summary>
        [UnityTest]
        public IEnumerator ONoiDung_KeoXuongSatHangNut()
        {
            var compose = OpenComposeTab();
            yield return null;

            var content = WorldRect((RectTransform)compose.transform.Find("Field_Nội dung"));
            var error = WorldRect((RectTransform)compose.transform.Find("Error"));
            var buttons = WorldRect((RectTransform)compose.transform.Find("Buttons"));

            Assert.Greater(content.height, 130f,
                $"Ô nội dung phải cao hơn hẳn mức cũ (96); đang {content.height:F0}.");
            Assert.GreaterOrEqual(content.yMin, error.yMax - 0.5f,
                "Ô nội dung không được đè lên dòng báo lỗi.");
            Assert.GreaterOrEqual(error.yMin, buttons.yMax - 0.5f,
                "Dòng báo lỗi không được đè lên hàng nút.");
        }

        /// <summary>
        /// Gõ dài thì <c>InputField</c> tự dời ô chữ theo con trỏ, nhưng KHÔNG tự cắt —
        /// thiếu mặt nạ là chữ vẽ tràn ra ngoài viền ô, đè lên nhãn và nút bên cạnh.
        /// </summary>
        [UnityTest]
        public IEnumerator ONhap_CoMatNa_DeChuCuonTrongO()
        {
            var compose = OpenComposeTab();
            yield return null;

            foreach (var fieldName in new[] { "Field_Người nhận", "Field_Nội dung" })
            {
                var box = compose.transform.Find(fieldName + "/Box");
                Assert.IsNotNull(box, $"Không thấy hộp nhập của {fieldName}.");
                Assert.IsNotNull(box.GetComponent<RectMask2D>(),
                    $"{fieldName} thiếu RectMask2D — gõ dài là chữ tràn ra ngoài viền.");
            }
        }

        [UnityTest]
        public IEnumerator ONoiDung_NhieuDong_ChuTranDuocDeCuon()
        {
            var compose = OpenComposeTab();
            yield return null;

            var input = compose.transform.Find("Field_Nội dung/Box").GetComponent<InputField>();
            Assert.AreEqual(InputField.LineType.MultiLineNewline, input.lineType);
            Assert.AreEqual(VerticalWrapMode.Overflow, input.textComponent.verticalOverflow,
                "Đặt Truncate là mất hẳn phần dưới, cuộn xuống chỉ thấy khoảng trắng.");
        }
    }
}
