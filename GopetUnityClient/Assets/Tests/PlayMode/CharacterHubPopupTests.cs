using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Net.Player;
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
            // Tab nằm trong Content của khung popup chung, không còn là con trực tiếp.
            Assert.AreEqual(4, view.GetComponentsInChildren<Transform>(true)
                .Count(child => child.name.StartsWith("Tab_")));

            view.SelectTab(CharacterHubTab.Pet);
            Assert.AreEqual(CharacterMenuAction.PetEquipment, requested,
                "Mở tab Pet phải tự nạp trang bị của pet đang chọn.");
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

        [UnityTest]
        public IEnumerator TuQuanAo_XacNhanTruocKhiGuiLuaChon()
        {
            var root = new GameObject("Character hub wardrobe confirm test");
            var sent = new List<Message>();
            var view = CharacterHubPopupView.Create(root.transform, new GuiderHandler(sent.Add), null, null, false);
            view.OpenInitial();
            var screen = TestPackets.Screen(803,
                TestPackets.Item(27, "Ao dai", showDialog: true, closeAfter: true));

            Assert.IsTrue(view.TryConsumeMenu(screen));
            view.GetComponentInChildren<GenericMenuView>().OnRowClicked(0);
            Assert.IsEmpty(sent, "Chua dong y thi khong duoc gui packet trang bi.");

            view.GetComponentInChildren<YesNoDialog>().GetComponentsInChildren<Button>()
                .First(button => button.name.StartsWith("Btn:")).onClick.Invoke();
            Assert.AreEqual(1, sent.Count);
            using (var round = Message.FromWire(sent[0].ToWire(), false))
            {
                Assert.AreEqual(GopetCmd.COMMAND_GUIDER, round.Id);
                Assert.AreEqual(GopetCmd.SELECT_MENU_ELEMENT, round.Reader.ReadSByte());
                Assert.AreEqual(803, round.Reader.ReadInt());
                Assert.AreEqual(27, round.Reader.ReadInt());
            }

            foreach (var message in sent) message.Dispose();
            Object.DestroyImmediate(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Can_XacNhanVaGuiWingTypeUseTheoJar()
        {
            var root = new GameObject("Character hub wing confirm test");
            var sent = new List<Message>();
            var wings = new WingHandler(sent.Add);
            var view = CharacterHubPopupView.Create(root.transform, new GuiderHandler(sent.Add), null, null, false, wings);
            view.OpenInitial();
            var screen = TestPackets.Screen(81040,
                TestPackets.Item(4, "Can thien than", showDialog: true, closeAfter: true));

            Assert.IsTrue(view.TryConsumeMenu(screen));
            view.GetComponentInChildren<GenericMenuView>().OnRowClicked(0);
            Assert.IsEmpty(sent, "Chua dong y thi khong duoc gui packet canh.");

            view.GetComponentInChildren<YesNoDialog>().GetComponentsInChildren<Button>()
                .First(button => button.name.StartsWith("Btn:")).onClick.Invoke();
            Assert.AreEqual(1, sent.Count);
            using (var round = Message.FromWire(sent[0].ToWire(), false))
            {
                Assert.AreEqual(GopetCmd.PET_SERVICE, round.Id);
                Assert.AreEqual(GopetCmd.WING, round.Reader.ReadSByte());
                Assert.AreEqual(GopetCmd.WING_TYPE_USE, round.Reader.ReadSByte());
                Assert.AreEqual(4, round.Reader.ReadInt());
            }

            foreach (var message in sent) message.Dispose();
            Object.DestroyImmediate(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RuongDo_MoChiTietVaNutDungGuiLuaChon()
        {
            var root = new GameObject("Inventory item details test");
            var sent = new List<Message>();
            var view = CharacterHubPopupView.Create(root.transform, new GuiderHandler(sent.Add), null, null, false);
            view.OpenInitial();
            var item = TestPackets.Item(2, "Bình exp");
            item.Description = "Sử dụng x4 exp trong 30 phút";
            var screen = TestPackets.Screen(81004, item);

            Assert.IsTrue(view.TryConsumeMenu(screen));
            view.GetComponentInChildren<InventoryGridView>().transform.GetChild(0)
                .GetComponent<Button>().onClick.Invoke();
            var popup = view.GetComponentInChildren<InventoryItemPopupView>();
            Assert.IsNotNull(popup);

            FindButton(view, "Dùng").onClick.Invoke();
            Assert.AreEqual(1, sent.Count);
            using (var round = Message.FromWire(sent[0].ToWire(), false))
            {
                Assert.AreEqual(GopetCmd.COMMAND_GUIDER, round.Id);
                Assert.AreEqual(GopetCmd.SELECT_MENU_ELEMENT, round.Reader.ReadSByte());
                Assert.AreEqual(81004, round.Reader.ReadInt());
                Assert.AreEqual(2, round.Reader.ReadInt());
            }

            foreach (var message in sent) message.Dispose();
            Object.DestroyImmediate(root);
            yield return null;
        }

        private static Button FindButton(CharacterHubPopupView view, string text) =>
            view.GetComponentsInChildren<Button>(true).FirstOrDefault(button =>
                button.GetComponentInChildren<Text>()?.text.StartsWith(text) == true);
    }
}
