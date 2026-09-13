using System.Collections;
using System.Linq;
using Gopet.Net.Guider;
using Gopet.Runtime.UI;
using Gopet.Runtime.World;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class CharacterHubPopupTests
    {
        [UnityTest]
        public IEnumerator HudNhanVat_ClickMoDuocHub()
        {
            var root = new GameObject("Character HUD test");
            var hud = CharacterHud.Create(root.transform, "Tester");
            var clicked = false;
            hud.Clicked += () => clicked = true;

            hud.GetComponent<Button>().onClick.Invoke();

            Assert.IsTrue(clicked);
            Object.DestroyImmediate(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Hub_CoBonTabVaDieuHuongDungAction()
        {
            var root = new GameObject("Character hub test");
            var guider = new GuiderHandler(_ => { });
            var view = CharacterHubPopupView.Create(root.transform, guider, null, null, false);
            CharacterMenuAction? requested = null;
            view.ActionRequested += action => requested = action;

            view.OpenInitial();
            Assert.AreEqual(CharacterMenuAction.Inventory, requested);
            Assert.AreEqual(4, view.transform.Cast<Transform>()
                .Count(child => child.name.StartsWith("Tab_")));

            view.SelectTab(CharacterHubTab.Pet);
            var selectPet = FindButton(view, "Chọn pet");
            Assert.IsNotNull(selectPet);
            selectPet.onClick.Invoke();
            Assert.AreEqual(CharacterMenuAction.SelectPet, requested);

            Object.DestroyImmediate(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CaiDat_ChuaTuDanhQuai()
        {
            var root = new GameObject("Character hub settings test");
            var view = CharacterHubPopupView.Create(root.transform,
                new GuiderHandler(_ => { }), null, null, false);
            var autoChanged = false;
            view.AutoAttackChanged += enabled => autoChanged = enabled;

            view.SelectTab(CharacterHubTab.Settings);
            var auto = FindButton(view, "Tự đánh quái:");
            Assert.IsNotNull(auto);
            auto.onClick.Invoke();
            Assert.IsTrue(autoChanged);

            Object.DestroyImmediate(root);
            yield return null;
        }

        private static Button FindButton(CharacterHubPopupView view, string text) =>
            view.GetComponentsInChildren<Button>(true).FirstOrDefault(button =>
                button.GetComponentInChildren<Text>()?.text.StartsWith(text) == true);
    }
}
