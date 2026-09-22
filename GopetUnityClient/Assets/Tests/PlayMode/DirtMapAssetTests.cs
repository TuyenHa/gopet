using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class DirtMapAssetTests
    {
        [TestCase(151, 12151)]
        [TestCase(161, 12161)]
        [TestCase(162, 12162)]
        [TestCase(179, 12179)]
        [TestCase(180, 12180)]
        public void AnhDat_GiuNguyenKichThuocVaCatDuMoiO(int originalId, int dirtId)
        {
            var original = Resources.Load<Texture2D>($"Jar/Art/Raw/newMapData/{originalId}");
            var dirt = Resources.Load<Texture2D>($"Jar/Art/Raw/newMapData/{dirtId}");
            Assert.IsNotNull(original, $"Missing original {originalId}");
            Assert.IsNotNull(dirt, $"Missing dirt {dirtId}");
            Assert.AreEqual(original.width, dirt.width);
            Assert.AreEqual(original.height, dirt.height);
            Assert.AreEqual(24, dirt.height);
            Assert.AreEqual(0, dirt.width % 24);
            Assert.AreEqual(FilterMode.Point, dirt.filterMode);
            for (var cell = 0; cell < dirt.width / 24; cell++)
            {
                var sprite = TileAssetProvider.Cell(dirtId, cell);
                Assert.IsNotNull(sprite, $"Missing {dirtId}:{cell}");
                Assert.AreSame(dirt, sprite.texture);
                Assert.AreEqual(new Rect(cell * 24, 0, 24, 24), sprite.rect);
            }
        }
    }
}
