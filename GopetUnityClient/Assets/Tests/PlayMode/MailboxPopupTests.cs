using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Hộp thư phải dùng đúng khung popup chung (<see cref="GamePopupFrame"/>) như cửa
    /// hàng, tab "Soạn thư" đổi nội dung ngay trong popup, và danh sách ngăn nhau bằng
    /// vạch ngang chứ không phải thẻ nền — thẻ nền sẽ tô đè lên góc bo của khung.
    ///
    /// <para>Phần ô đọc thư bên phải có bộ riêng: <see cref="MailboxDetailPaneTests"/>.</para>
    /// </summary>
    public sealed class MailboxPopupTests : MailboxTestBase
    {
        [UnityTest]
        public IEnumerator DungKhungPopupChung_CoTieuDeHopThu()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var frame = view.GetComponentInChildren<GamePopupFrame>(true);
            Assert.IsNotNull(frame, "Hộp thư phải dựng trên GamePopupFrame như popup cửa hàng.");

            var title = frame.transform.Find("Header/Title").GetComponent<Text>();
            Assert.AreEqual("Hộp thư", title.text);
        }

        [UnityTest]
        public IEnumerator MoiThuMotDong_DongCuoiTatVachNgan()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var rows = view.GetComponentsInChildren<PopupTextRow>(true);
            Assert.AreEqual(3, rows.Length, "Tab 'Tất cả' phải hiện đủ ba thư.");

            var separators = 0;
            foreach (var row in rows)
            {
                var line = row.transform.Find("Separator");
                Assert.IsNotNull(line, "Dòng thư phải có vạch ngăn.");
                if (line.gameObject.activeSelf) separators++;
            }
            Assert.AreEqual(2, separators,
                "Ba dòng thì chỉ hai vạch: dòng cuối phải tắt, không thì thừa nét sát viền.");
        }

        /// <summary>
        /// Dòng thư KHÔNG được có nền đặc: mặt nạ của khung danh sách cắt theo hình chữ
        /// nhật nên nền đặc sẽ tô đè lên bốn góc bo, nhìn ra thành viền đứt, góc trắng.
        /// </summary>
        [UnityTest]
        public IEnumerator DongThu_NenTrongSuot_KhongDeLenGocBo()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            foreach (var row in view.GetComponentsInChildren<PopupTextRow>(true))
            {
                var background = row.GetComponent<Image>();
                Assert.AreEqual(0f, background.color.a, 0.001f,
                    "Nền dòng thư phải trong suốt.");
            }
        }

        [UnityTest]
        public IEnumerator TabSoanThu_HienFormNgayTrongPopup()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var compose = view.GetComponentInChildren<ComposeLetterView>(true);
            var list = view.GetComponentInChildren<PopupItemList>(true);
            Assert.IsFalse(compose.gameObject.activeSelf, "Mới mở thì phải ở danh sách.");

            var rail = view.GetComponentInChildren<PopupTabRail>(true);
            rail.Select(rail.TabCount - 1);
            yield return null;

            Assert.IsTrue(compose.gameObject.activeSelf, "Tab 'Soạn thư' phải hiện form.");
            Assert.IsFalse(list.gameObject.activeSelf, "Form soạn hiện thì danh sách phải ẩn.");
            // Form nằm TRONG popup, không phải một overlay phủ màn riêng.
            Assert.IsTrue(compose.transform.IsChildOf(view.GetComponentInChildren<GamePopupFrame>(true).transform),
                "Form soạn thư phải nằm trong khung popup.");
        }

        /// <summary>
        /// Thư hệ thống phải nhận ra được ngay trong tab "Tất cả", nơi nó nằm lẫn thư bạn bè.
        /// Thư bạn bè không gắn nhãn — tiêu đề của nó đã là tên người gửi.
        /// </summary>
        [UnityTest]
        public IEnumerator ThuHeThong_CoNhanLoai_ThuBanBeThiKhong()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var badges = new List<string>();
            foreach (var row in view.GetComponentsInChildren<PopupTextRow>(true))
            {
                var badge = row.transform.Find("TypeBadge");
                Assert.IsNotNull(badge, "Dòng thư phải có chỗ cho nhãn loại.");
                badges.Add(badge.gameObject.activeSelf
                    ? badge.GetComponentInChildren<Text>(true).text
                    : null);
            }

            CollectionAssert.Contains(badges, "BQT", "Thư admin phải có nhãn BQT.");
            CollectionAssert.Contains(badges, "SK", "Thư sự kiện phải có nhãn SK.");
            CollectionAssert.Contains(badges, null, "Thư bạn bè không được gắn nhãn.");
        }

        /// <summary>Chưa đọc lên trước; cùng nhóm thì thư hệ thống trên thư bạn bè.</summary>
        [UnityTest]
        public IEnumerator SapXep_ChuaDocTruoc_RoiThuHeThong()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var titles = view.GetComponentsInChildren<PopupTextRow>(true)
                .Select(row => row.transform.Find("Title").GetComponent<Text>().text)
                .ToArray();

            // "Thư admin" và "Bạn bè" chưa đọc, "Sự kiện" đã đọc -> hai cái đầu lên trước,
            // trong đó admin đứng trên bạn bè.
            Assert.AreEqual("Thư admin", titles[0]);
            Assert.AreEqual("Bạn bè", titles[1]);
            Assert.AreEqual("Sự kiện", titles[2], "Thư đã đọc phải xuống cuối.");
        }

        [UnityTest]
        public IEnumerator LocTheoTab_ChiHienThuDungLoai()
        {
            var view = MailboxView.Create(Root.transform, ThreeLetters());
            yield return null;

            var rail = view.GetComponentInChildren<PopupTabRail>(true);
            rail.Select(1); // Admin
            yield return null;

            Assert.AreEqual(1, view.GetComponentsInChildren<PopupTextRow>(true).Length,
                "Tab Admin chỉ có một thư.");
        }
    }
}
