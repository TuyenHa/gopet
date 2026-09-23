using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Net.Npc;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Danh sách dài trong popup NPC phải cuộn được: nội dung cao hơn khung nhìn và
    /// <see cref="ScrollRect"/> ở đúng chỗ.
    ///
    /// <para>Đây là thứ dễ hỏng âm thầm: chỉ cần khung nhìn bị tính sai (đổi chiều cao
    /// popup, đổi chiều cao khay tab) là nội dung bị kẹp bằng khung nhìn và danh sách
    /// đứng im, nhìn qua vẫn "có vẻ đúng" vì mấy dòng đầu vẫn hiện.</para>
    /// </summary>
    public sealed class NpcPopupScrollTests
    {
        private GameObject _host;
        private GuiderHandler _guider;
        private List<Message> _sent;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("UiHost", typeof(RectTransform));
            _sent = new List<Message>();
            _guider = new GuiderHandler(_sent.Add);
        }

        [TearDown]
        public void TearDown()
        {
            if (_host != null) Object.DestroyImmediate(_host);
        }

        private static NpcOptions Options(int npcId, params (int id, string text)[] options)
        {
            var list = new NpcOptions.Option[options.Length];
            for (var i = 0; i < options.Length; i++)
                list[i] = new NpcOptions.Option { Id = options[i].id, Text = options[i].text };
            return new NpcOptions { NpcId = npcId, Options = list };
        }

        private static MenuScreen ManyItems(int listId, int count)
        {
            var items = new MenuItemInfo[count];
            for (var i = 0; i < count; i++) items[i] = TestPackets.Item(i, "Pet " + i);
            return TestPackets.Screen(listId, items);
        }

        private static void AssertScrollable(ScrollRect scroll, string what)
        {
            Assert.IsNotNull(scroll, what + ": thiếu ScrollRect");
            Assert.IsTrue(scroll.vertical, what + ": phải cuộn dọc");
            Assert.IsNotNull(scroll.content, what + ": ScrollRect chưa trỏ vào content");
            Assert.Greater(scroll.content.rect.height, scroll.viewport.rect.height,
                what + ": nội dung không cao hơn khung nhìn thì danh sách đứng im");
        }

        [Test]
        public void SuGia_HienTangThuCung_CuonDuocKhiNhieuPet()
        {
            var view = HeavenNpcTabsView.Create(_host.transform, UiBuilder.DefaultFont(),
                Options(-25, (88, "Hướng dẫn"), (89, "Hiến tặng thú cưng")));

            Assert.IsTrue(view.TryConsumeMenu(
                ManyItems(HeavenNpcTabsView.MenuPetSacrificeListId, 30), null, _guider));

            var grid = view.GetComponentInChildren<PetGridView>(true);
            AssertScrollable(grid.GetComponent<ScrollRect>(), "Hiến tặng thú cưng");
        }

        [Test]
        public void BacSi_TayGym_CuonDuocKhiNhieuDong()
        {
            var view = BacSiNpcTabsView.Create(_host.transform, UiBuilder.DefaultFont(),
                Options(-7, (22, "Hồi sinh pet sau PK"), (24, "Tẩy gym")));

            // Phải đang ở tab Tẩy gym thì popup mới nhận gói menu của tab đó.
            view.GetComponentInChildren<PopupTabRail>().Select(1);
            Assert.IsTrue(view.TryConsumeMenu(
                ManyItems(BacSiNpcTabsView.MenuDeleteTiemNangListId, 30), null, _guider));

            var menu = view.GetComponentInChildren<GenericMenuView>(true);
            AssertScrollable(menu.GetComponent<ScrollRect>(), "Tẩy gym");
        }
    }
}
