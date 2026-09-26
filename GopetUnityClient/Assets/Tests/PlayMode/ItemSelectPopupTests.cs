using System.Collections.Generic;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class ItemSelectPopupTests
    {
        [Test]
        public void Handles_ChiNhanMenuNguyenLieuVaTayGym()
        {
            Assert.IsTrue(ItemSelectPopupView.Handles(new MenuScreen { ListId = 1014 }));
            Assert.IsTrue(ItemSelectPopupView.Handles(new MenuScreen { ListId = 1047 }));
            Assert.IsTrue(ItemSelectPopupView.Handles(new MenuScreen { ListId = 800 }));
            Assert.IsTrue(ItemSelectPopupView.Handles(new MenuScreen { ListId = 1023 }));
            Assert.IsTrue(ItemSelectPopupView.Handles(new MenuScreen { ListId = 81004 }));
            // Học kỹ năng: nhúng vào tab Pet của Hành lý — thiếu dòng này là CreateEmbedded ném.
            Assert.IsTrue(ItemSelectPopupView.Handles(new MenuScreen { ListId = 799 }));
            Assert.IsFalse(ItemSelectPopupView.Handles(new MenuScreen { ListId = 1033 }));
        }

        [Test]
        public void Bind_MoiDongMotThe_ChonGuiLenServer()
        {
            var host = new GameObject("ItemSelectHost", typeof(RectTransform));
            var sent = new List<Message>();
            try
            {
                var screen = new MenuScreen
                {
                    ListId = 800,
                    Items = new[]
                    {
                        new MenuItemInfo { Title = "Xóa sức mạnh (str)", Description = "với giá 2 (vang)", CanSelect = true },
                        new MenuItemInfo { Title = "Xóa tốc độ (agi)", Description = "", CanSelect = true }
                    }
                };
                var view = ItemSelectPopupView.Create(host.transform, null, screen, new GuiderHandler(sent.Add), null);
                view.Bind(screen);
                Assert.AreEqual(2, view.RowCount);
                Assert.IsFalse(view.CanBind(new MenuScreen { ListId = 1014 }));

                view.OnRowClicked(0);
                Assert.AreEqual(1, sent.Count);
            }
            finally
            {
                Object.DestroyImmediate(host);
                foreach (var message in sent) message.Dispose();
            }
        }

        [Test]
        public void Humanize_DoiTagChiSoVaTien()
        {
            Assert.AreEqual("Xóa 1 sức mạnh (STR) với giá 2 vàng",
                JarIconTokens.Humanize("Xóa 1 sức mạnh (str) với giá 2 (vang)"));
        }
    }
}
