using System.Collections;
using Gopet.Net.Chat;
using Gopet.Runtime.UI;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gopet.PlayModeTests
{
    public sealed class CharacterHudTests
    {
        /// <summary>Tên pet kèm cấp ngay cạnh: "Rua Test - LV.5"; PlayerName vẫn là tên gốc.</summary>
        [UnityTest]
        public IEnumerator Hud_TenKemCapPet()
        {
            var root = new GameObject("Character HUD level root");
            var hud = GameHud.Create(root.transform, new ChatHandler(_ => { }), "Rua Test").Character;
            var label = hud.transform.Find("Player Name").GetComponent<UnityEngine.UI.Text>();
            Assert.AreEqual("Rua Test", label.text);
            hud.SetLevel(5);
            Assert.AreEqual("Rua Test - LV.5", label.text);
            hud.SetName("Pet Moi");
            Assert.AreEqual("Pet Moi - LV.5", label.text);
            Assert.AreEqual("Pet Moi", hud.PlayerName);
            Object.DestroyImmediate(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Hud_DungDungProfileVaCapNhatThanhChiSo()
        {
            var root = new GameObject("Character HUD test root");
            var chat = new ChatHandler(_ => { });
            var gameHud = GameHud.Create(root.transform, chat, "Sư Gia Thiên Thần");
            var hud = gameHud.Character;

            Assert.IsNotNull(hud);
            Assert.AreEqual("Sư Gia Thiên Thần", hud.PlayerName);
            Assert.IsNull(hud.transform.Find("Map Name"), "Tên map đã chuyển lên minimap.");
            Assert.IsNotNull(hud.transform.Find("Portrait Frame/Portrait Mask/Portrait"));
            Assert.AreEqual(UnityEngine.UI.Image.Type.Sliced,
                hud.GetComponent<UnityEngine.UI.Image>().type);
            Assert.Greater(RoundedUiSprite.Get().border.x, 0f);
            Assert.IsNotNull(hud.Hp.transform.Find("Track/Track Surface")
                .GetComponent<UnityEngine.UI.Mask>());
            // Demo default: HP/MP đầy 100%, EXP 0%.
            Assert.AreEqual("100/100 (100%)", hud.Hp.ValueText);
            Assert.AreEqual("0%", hud.Experience.ValueText);
            // BarLeft = 8 (portrait left) + 64 (portrait size) + 8 (gap) = 80.
            Assert.AreEqual(80f,
                ((RectTransform)hud.Hp.transform).anchoredPosition.x, 0.001f);

            hud.SetStats(1250, 1250, 750, 1000, 3555, 10000);

            Assert.AreEqual(1f, hud.Hp.FillAmount, 0.001f);
            Assert.AreEqual("1250/1250 (100%)", hud.Hp.ValueText);
            Assert.AreEqual(0.75f, hud.Mp.FillAmount, 0.001f);
            Assert.AreEqual("35.55%", hud.Experience.ValueText);
            Assert.AreEqual(0.3555f, hud.Experience.FillAmount, 0.001f);

            Object.Destroy(root);
            yield return null;
        }
    }
}
