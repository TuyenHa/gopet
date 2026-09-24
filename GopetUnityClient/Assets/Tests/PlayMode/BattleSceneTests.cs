using System.Collections;
using Gopet.Net;
using Gopet.Net.Guider;
using Gopet.Runtime.World.Battle;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gopet.PlayModeTests
{
    /// <summary>Khung cảnh màn đấu: ánh xạ id, gói STATE, gói gửi đi, đổi nền không rò object.</summary>
    public sealed class BattleSceneTests
    {
        [Test]
        public void Catalog_IdLa_RoiVeRungMacDinh()
        {
            Assert.AreEqual(6, BattleSceneCatalog.Count);
            Assert.AreEqual(0, BattleSceneCatalog.Normalize(-1));
            Assert.AreEqual(0, BattleSceneCatalog.Normalize(99));
            Assert.AreEqual("Battle/bg-forest", BattleSceneCatalog.BackgroundPath(42));
            Assert.AreEqual(BattleAmbientKind.None, BattleSceneCatalog.Ambient(0));
            Assert.AreEqual(BattleAmbientKind.Snow, BattleSceneCatalog.Ambient(3));
            Assert.AreEqual(BattleAmbientKind.Fire, BattleSceneCatalog.Ambient(5));
        }

        [Test]
        public void Catalog_MoiKhungCanh_CoAnhNenVaHieuUng()
        {
            for (var id = 0; id < BattleSceneCatalog.Count; id++)
            {
                Assert.IsNotNull(Resources.Load<Sprite>(BattleSceneCatalog.BackgroundPath(id)),
                    $"thiếu ảnh nền khung cảnh {id}");
                foreach (var layer in BattleAmbientPresets.For(BattleSceneCatalog.Ambient(id)))
                foreach (var frame in layer.Frames)
                    Assert.IsNotNull(Resources.Load<Sprite>(frame), $"thiếu sprite hiệu ứng {frame}");
            }
        }

        [Test]
        public void State_DocDungLayoutServer()
        {
            var state = BattleSceneState.Parse(StateWire(truncate: false));
            Assert.AreEqual(3, state.SelectedId);
            Assert.AreEqual(2, state.Entries.Count);
            Assert.AreEqual("Rừng", state.Entries[0].Name);
            Assert.IsTrue(state.Entries[0].Owned);
            Assert.AreEqual(3, state.Entries[1].Id);
            Assert.AreEqual(12_000L, state.Entries[1].PriceGold);
        }

        [Test]
        public void State_ThieuByte_Nem()
        {
            Assert.That(() => BattleSceneState.Parse(StateWire(truncate: true)), Throws.Exception);
        }

        [Test]
        public void GoiMua_MangDungSubVaId()
        {
            var m = Message.FromWire(GuiderPackets.BuyBattleScene(4).ToWire(), false);
            Assert.AreEqual(GopetCmd.COMMAND_GUIDER, m.Id);
            Assert.AreEqual(GopetCmd.TYPE_BATTLE_BG_BUY, m.Reader.ReadSByte());
            Assert.AreEqual(4, m.Reader.ReadSByte());
        }

        [UnityTest]
        public IEnumerator Backdrop_DoiKhungCanhNhieuLan_KhongRoHat()
        {
            var root = new GameObject("Backdrop test", typeof(RectTransform), typeof(Canvas));
            var backdrop = BattleBackdrop.Create(root.transform, 0);
            Assert.AreEqual(0, backdrop.transform.childCount, "rừng mặc định không có hiệu ứng");

            for (var i = 0; i < 10; i++) { backdrop.Apply(1 + i % 5); yield return null; }
            backdrop.Apply(3);
            yield return null;
            Assert.AreEqual(1, backdrop.transform.childCount, "hiệu ứng cũ phải bị huỷ khi đổi");
            Assert.AreEqual(3, backdrop.SceneId);
            Object.Destroy(root);
        }

        [Test]
        public void Popup_BamTrongPanel_KhongDong_BamNenMo_Dong()
        {
            var root = new GameObject("Popup test", typeof(RectTransform), typeof(Canvas));
            var popup = BattleScenePopup.Create(root.transform, Gopet.Runtime.UI.UiBuilder.DefaultFont());
            popup.SetOpen(true);
            var click = new UnityEngine.EventSystems.PointerEventData(null);

            // Click trượt vào panel đi ngược lên cha như Unity làm — không được chạm nút đóng.
            UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(popup.transform.Find("Panel").gameObject,
                click, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            Assert.IsTrue(popup.IsOpen, "chạm vào panel đã đóng popup");

            UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(popup.transform.Find("Nền mờ").gameObject,
                click, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            Assert.IsFalse(popup.IsOpen, "bấm nền mờ phải đóng popup");
            Object.Destroy(root);
        }

        /// <summary>Gói STATE như server gửi, đã đọc qua byte sub-command giống router.</summary>
        private static Message StateWire(bool truncate)
        {
            var m = Message.Create(GopetCmd.COMMAND_GUIDER, false)
                .PutSByte(GopetCmd.TYPE_BATTLE_BG_STATE)
                .PutSByte(3).PutSByte(2)
                .PutSByte(0).PutUtf("Rừng").PutLong(0).PutBool(true)
                .PutSByte(3).PutUtf("Tuyết trắng").PutLong(12_000);
            if (!truncate) m.PutBool(false);
            var wire = Message.FromWire(m.ToWire(), false);
            wire.Reader.ReadSByte(); // sub-command
            return wire;
        }
    }
}
