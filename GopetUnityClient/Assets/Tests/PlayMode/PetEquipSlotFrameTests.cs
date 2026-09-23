using Gopet.Net.Pet;
using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Ô trang bị pet dùng khung viền vàng sinh sẵn: có item → nền xanh, trống → cùng khung
    /// nhưng nền xám (vẫn giữ sao nhỏ ở góc).
    /// </summary>
    public sealed class PetEquipSlotFrameTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp() => _root = new GameObject("Equip Slot Root", typeof(RectTransform), typeof(Canvas));

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_root);

        [Test]
        public void KhungAnh_TonTaiTrongResources()
        {
            Assert.IsNotNull(Resources.Load<Texture2D>("Ui/Generated/equip_slot_filled"));
            Assert.IsNotNull(Resources.Load<Texture2D>("Ui/Generated/equip_slot_empty"));
        }

        [Test]
        public void OTrong_KhungXam_CoItem_KhungXanh()
        {
            var slot = PetEquipSlot.Create(_root.transform, EquipSlot.Hat, "Nón", Vector2.zero, null);
            var frame = slot.GetComponent<Image>();

            Assert.AreEqual("equip_slot_empty", frame.sprite.texture.name);
            Assert.AreEqual(Color.white, frame.color, "Không được tô màu đè lên khung ảnh.");

            slot.SetItem(new PetEquipItem { Type = (int)EquipSlot.Hat, FrameImagePath = "items/1.png" });
            Assert.AreEqual("equip_slot_filled", frame.sprite.texture.name);

            slot.SetItem(null);
            Assert.AreEqual("equip_slot_empty", frame.sprite.texture.name, "Tháo đồ phải về khung xám.");
        }
    }
}
