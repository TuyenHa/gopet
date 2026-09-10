using Gopet.Runtime.UI;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    /// <summary>
    /// Cache sprite phải sống sót qua việc đối tượng bị huỷ.
    ///
    /// <para><b>Lỗi đã trả giá:</b> thoát Play Mode thì Unity huỷ mọi sprite tạo lúc
    /// chạy, nhưng <c>Dictionary</c> TĨNH thì sống tiếp sang lần Play sau và trả về
    /// chính cái xác đó. <c>Image</c> nhận sprite đã huỷ thì coi như không có sprite:
    /// ô nhập mất sạch bo góc. Triệu chứng quái ở chỗ lần chạy ĐẦU luôn đúng, chỉ từ
    /// lần THỨ HAI trở đi mới hỏng — rất dễ đổ oan cho khâu import asset.</para>
    ///
    /// <para>Huỷ thủ công ở đây mô phỏng đúng cú thoát Play Mode ấy.</para>
    /// </summary>
    public sealed class LoginSkinCacheTests
    {
        [Test]
        public void SpriteCatBiHuy_LanSauNapLaiChuKhongTraVeXac()
        {
            var first = LoginSkin.GetSliced(LoginSkin.Field);
            if (first == null) Assert.Ignore("Chưa có bộ art màn đăng nhập trong Resources/Ui/Login.");

            Object.DestroyImmediate(first);

            var second = LoginSkin.GetSliced(LoginSkin.Field);

            Assert.IsNotNull(second, "Cache trả về sprite đã bị huỷ — ô nhập sẽ mất bo góc từ lần chạy thứ hai.");
            Assert.AreNotEqual(Vector4.zero, second.border, "Sprite nạp lại phải giữ viền 9-slice.");
        }

        [Test]
        public void SpriteNapTuResourcesBiDon_LanSauVanCoSprite()
        {
            var first = LoginSkin.Get(LoginSkin.IconUser);
            if (first == null) Assert.Ignore("Chưa có bộ art màn đăng nhập trong Resources/Ui/Login.");

            // KHÔNG huỷ ở đây: đó là asset thật trong Resources, huỷ là xoá khỏi đĩa.
            // Chỉ cần chắc lần gọi thứ hai vẫn ra sprite dùng được.
            var second = LoginSkin.Get(LoginSkin.IconUser);

            Assert.IsNotNull(second);
            Assert.AreSame(first, second, "Cùng một tên phải dùng lại đúng một sprite, không nạp lại mỗi lần.");
        }
    }
}
