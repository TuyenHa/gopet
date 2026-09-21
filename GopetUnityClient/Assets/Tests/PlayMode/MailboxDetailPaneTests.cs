using System.Collections;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Ô đọc thư ở nửa phải popup hộp thư: chia đôi đúng chỗ, chọn thư thì đổ nội dung,
    /// và — quan trọng nhất — thư DÀI phải cuộn được chứ không bị cắt cụt.
    /// </summary>
    public sealed class MailboxDetailPaneTests : MailboxTestBase
    {
        /// <summary>
        /// Nội dung chia đôi: danh sách bên trái, ô đọc thư bên phải, KHÔNG chồng nhau.
        /// </summary>
        [UnityTest]
        public IEnumerator NoiDungChiaDoi_DanhSachTrai_DocThuPhai()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var list = WorldRect((RectTransform)view.GetComponentInChildren<PopupItemList>(true).transform);
            var detail = WorldRect((RectTransform)view.GetComponentInChildren<LetterDetailPane>(true).transform);

            Assert.Greater(detail.xMin, list.xMax - 0.5f, "Ô đọc thư phải nằm BÊN PHẢI danh sách.");
            Assert.IsFalse(list.Overlaps(detail), "Danh sách và ô đọc thư không được chồng nhau.");
        }

        [UnityTest]
        public IEnumerator ChuaChonThu_ODocThuHienLoiMoi()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var detail = view.GetComponentInChildren<LetterDetailPane>(true);
            var placeholder = detail.transform.Find("Placeholder").gameObject;
            Assert.IsTrue(placeholder.activeSelf, "Mới mở thì ô đọc thư phải mời chọn thư.");
            Assert.IsFalse(detail.transform.Find("Buttons").gameObject.activeSelf,
                "Chưa chọn thư thì không hiện nút Đánh dấu/Xoá.");
        }

        [UnityTest]
        public IEnumerator BamMotThu_HienNoiDungBenPhai()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var row = view.GetComponentsInChildren<PopupTextRow>(true)[0];
            row.GetComponent<Button>().onClick.Invoke();
            yield return null;

            var detail = view.GetComponentInChildren<LetterDetailPane>(true);
            Assert.AreEqual("Thư admin", detail.transform.Find("Title").GetComponent<Text>().text);
            Assert.AreEqual("Xin chào",
                detail.transform.Find("Body/Text").GetComponent<Text>().text);
            Assert.IsTrue(detail.transform.Find("Buttons").gameObject.activeSelf,
                "Chọn thư rồi thì phải hiện hai nút.");
            Assert.IsFalse(detail.transform.Find("Placeholder").gameObject.activeSelf);
        }

        /// <summary>Thư đã đọc thì nút đánh dấu phải tắt — bấm nữa là vô nghĩa.</summary>
        [UnityTest]
        public IEnumerator ThuDaDoc_NutDanhDauBiTat()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            // Dòng thứ ba sau khi sắp xếp là "Sự kiện" — lá duy nhất IsMark = true.
            view.GetComponentsInChildren<PopupTextRow>(true)[2].GetComponent<Button>().onClick.Invoke();
            yield return null;

            var buttons = view.GetComponentInChildren<LetterDetailPane>(true).transform.Find("Buttons");
            Assert.IsFalse(buttons.Find("Button_0").GetComponent<Button>().interactable,
                "Thư đã đọc thì nút Đánh dấu phải tắt.");
            Assert.IsTrue(buttons.Find("Button_1").GetComponent<Button>().interactable,
                "Nút Xoá vẫn phải bấm được.");
        }

        /// <summary>
        /// Thư dài phải CUỘN được: chiều cao chữ vượt vùng đọc, và vùng đọc là nội dung
        /// của một <see cref="ScrollRect"/> chứ không phải bị cắt cụt.
        /// </summary>
        [UnityTest]
        public IEnumerator ThuDai_ODocThuCuonDuoc()
        {
            var view = MailboxView.Create(Root.transform, OneLongLetter());
            yield return null;

            view.GetComponentsInChildren<PopupTextRow>(true)[0].GetComponent<Button>().onClick.Invoke();
            // Hai frame: ContentSizeFitter đặt chiều cao ở lượt layout kế tiếp.
            yield return null;
            yield return null;

            var detail = view.GetComponentInChildren<LetterDetailPane>(true);
            var scroll = detail.GetComponentInChildren<ScrollRect>(true);
            var body = detail.transform.Find("Body/Text").GetComponent<RectTransform>();

            Assert.AreSame(body, scroll.content,
                "Ô chữ phải LÀ nội dung cuộn của ScrollRect.");
            Assert.IsTrue(scroll.vertical, "Phải bật cuộn dọc.");
            Assert.IsNotNull(scroll.GetComponent<RectMask2D>(),
                "Vùng đọc phải có mặt nạ, không thì chữ tràn ra ngoài khung.");

            var viewportHeight = ((RectTransform)scroll.transform).rect.height;
            Assert.Greater(body.rect.height, viewportHeight,
                $"Thư dài phải cao hơn vùng đọc thì mới có gì để cuộn " +
                $"(chữ {body.rect.height:F0} vs vùng đọc {viewportHeight:F0}).");
        }

        /// <summary>Đổi sang thư khác thì phải cuộn về đầu, không giữ chỗ cuộn của thư trước.</summary>
        [UnityTest]
        public IEnumerator DoiThu_CuonVeDau()
        {
            var view = MailboxView.Create(Root.transform, OneLongLetter());
            yield return null;

            view.GetComponentsInChildren<PopupTextRow>(true)[0].GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return null;

            var scroll = view.GetComponentInChildren<LetterDetailPane>(true)
                .GetComponentInChildren<ScrollRect>(true);
            scroll.verticalNormalizedPosition = 0f; // giả vờ người chơi đã cuộn xuống đáy
            yield return null;

            view.GetComponentsInChildren<PopupTextRow>(true)[0].GetComponent<Button>().onClick.Invoke();
            yield return null;

            Assert.AreEqual(1f, scroll.verticalNormalizedPosition, 0.01f,
                "Mở thư phải bắt đầu từ đầu thư.");
        }
    }
}
