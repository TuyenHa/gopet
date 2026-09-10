using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>
    /// Chốt chặn của <see cref="JavaBinaryReader"/> trước gói dị dạng.
    ///
    /// <para>Đây là tuyến phòng thủ THẬT, không phải lớp dự phòng: ngưỡng riêng ở
    /// từng parser tầng trên chỉ là lớp thứ hai và rất dễ quên khi thêm parser mới.</para>
    /// </summary>
    public sealed class ReaderGuardTests
    {
        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MaxValue - 1)]
        [InlineData(1_000_000)]
        public void DocSoByteKhongLo_NemChuKhongCapPhat(int count)
        {
            // Viết kiểm tra dạng cộng (`_pos + count > _data.Length`) thì với count
            // sát int.MaxValue phép cộng tràn thành ÂM, lọt qua, rồi cấp phát một
            // mảng khổng lồ. Dạng trừ không tràn.
            var reader = new JavaBinaryReader(new byte[] { 1, 2, 3, 4 });
            reader.ReadSByte();   // đẩy _pos lên 1 để phép cộng có cơ hội tràn

            Assert.Throws<ProtocolException>(() => reader.ReadBytes(count));
        }

        [Fact]
        public void DocSoByteAm_Nem()
        {
            var reader = new JavaBinaryReader(new byte[] { 1, 2, 3, 4 });

            Assert.Throws<ProtocolException>(() => reader.ReadBytes(-1));
        }

        [Fact]
        public void DocVuaDuDenCuoiGoi_VanChayBinhThuong()
        {
            // Chốt chặn không được chặt quá tới mức chặn cả trường hợp hợp lệ.
            var reader = new JavaBinaryReader(new byte[] { 1, 2, 3, 4 });

            Assert.Equal(new byte[] { 1, 2, 3, 4 }, reader.ReadBytes(4));
            Assert.Equal(0, reader.Remaining);
        }
    }

    /// <summary>Gỡ đăng ký handler — cần cho thành phần có vòng đời ngắn hơn router.</summary>
    public sealed class RouterUnregisterTests
    {
        [Fact]
        public void DangKyLai_SauKhiGo_KhongNem()
        {
            // Không có Unregister thì dựng lại lần hai trên cùng Router sẽ ném —
            // chính là chuyện xảy ra mỗi lần nạp lại scene trong Unity.
            var router = new MessageRouter();

            router.Register(GopetCmd.COMMAND_IMAGE, _ => { });
            Assert.True(router.Unregister(GopetCmd.COMMAND_IMAGE));

            router.Register(GopetCmd.COMMAND_IMAGE, _ => { });
        }

        [Fact]
        public void DangKyLai_KhongGo_VanNem()
        {
            var router = new MessageRouter();
            router.Register(GopetCmd.COMMAND_IMAGE, _ => { });

            Assert.Throws<System.InvalidOperationException>(
                () => router.Register(GopetCmd.COMMAND_IMAGE, _ => { }));
        }

        [Fact]
        public void GoCaiChuaDangKy_TraVeFalse()
        {
            Assert.False(new MessageRouter().Unregister(GopetCmd.COMMAND_IMAGE));
        }

        [Fact]
        public void SauKhiGo_HandlerKhongConDuocGoi()
        {
            var router = new MessageRouter();
            var calls = 0;
            router.Register(GopetCmd.COMMAND_IMAGE, _ => calls++);
            router.Unregister(GopetCmd.COMMAND_IMAGE);

            using var built = Message.Create(GopetCmd.COMMAND_IMAGE).PutSByte(0);
            router.Dispatch(Message.FromWire(built.ToWire(), false));

            Assert.Equal(0, calls);
        }
    }
}
