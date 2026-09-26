using System.Linq;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>Tên pet "Rua ★★☆" trong dòng menu: chữ chỉ còn tên, sao thành icon vàng.</summary>
    public sealed class MenuItemRowStarTests
    {
        [Test]
        public void TieuDeCoSao_BoKyTuVaHienIconSao()
        {
            var root = new GameObject("Menu row stars");
            var row = MenuItemRow.Create(root.transform, UiBuilder.DefaultFont());
            row.Bind(new MenuItemInfo { Title = "Rua Test ★★☆", Description = "", CanSelect = true },
                0, null, _ => { });

            var title = row.transform.Find("Title").GetComponent<Text>();
            Assert.AreEqual("Rua Test", title.text);
            var stars = title.GetComponentsInChildren<Image>(false).Where(i => i.name.StartsWith("Star_"));
            Assert.AreEqual(3, stars.Count());

            row.Bind(new MenuItemInfo { Title = "Bình máu", Description = "", CanSelect = true },
                0, null, _ => { });
            Assert.AreEqual(0, title.GetComponentsInChildren<Image>(false).Count(i => i.name.StartsWith("Star_")));
            Object.DestroyImmediate(root);
        }
    }
}
