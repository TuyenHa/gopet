using Gopet.UiLogic;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>PathSampler — sample điểm liên tục, xoá điểm giữa nếu thẳng hàng, flush mỗi 2s.</summary>
    public sealed class PathSamplerTests
    {
        [Fact]
        public void Start_MoDauGhiVaXoaSanh()
        {
            var s = new PathSampler();
            s.Start(1000);
            Assert.True(s.Recording);
            Assert.Equal(0, s.SampleCount);
        }

        [Fact]
        public void Sample_KhiChuaStart_KhongLamGi()
        {
            var s = new PathSampler();
            s.Sample(10, 10);
            Assert.Equal(0, s.SampleCount);
        }

        [Fact]
        public void Sample_TrungDiemCuoi_BoQua()
        {
            var s = new PathSampler();
            s.Start(0);
            s.Sample(10, 10);
            s.Sample(10, 10);
            Assert.Equal(1, s.SampleCount);
        }

        [Fact]
        public void Sample_BaDiemThangHang_XoaDiemGiua()
        {
            var s = new PathSampler();
            s.Start(0);
            s.Sample(0, 0);
            s.Sample(10, 0);
            s.Sample(20, 0);   // 3 điểm cùng trục X → xoá điểm giữa (10,0)
            Assert.Equal(2, s.SampleCount);
        }

        [Fact]
        public void Sample_KhongThangHang_GiuNguyen()
        {
            var s = new PathSampler();
            s.Start(0);
            s.Sample(0, 0);
            s.Sample(10, 0);
            s.Sample(10, 10);  // rẽ góc
            Assert.Equal(3, s.SampleCount);
        }

        [Fact]
        public void ShouldFlush_DungMocHaiGiay()
        {
            var s = new PathSampler();
            s.Start(0);
            Assert.False(s.ShouldFlush(1999));
            Assert.True(s.ShouldFlush(2000));
        }

        [Fact]
        public void Flush_TraMangInterleavedX_YVaGomThemDiemCuoi()
        {
            var s = new PathSampler();
            s.Start(0);
            s.Sample(10, 20);
            s.Sample(30, 40);
            // Flush đảm bảo (50, 60) là điểm cuối kể cả chưa sample.
            var arr = s.Flush(50, 60);
            Assert.Equal(new[] { 10, 20, 30, 40, 50, 60 }, arr);
            Assert.False(s.Recording);
        }

        [Fact]
        public void Flush_LuonToiThieuHaiDiem_KhopVerifyServer()
        {
            var s = new PathSampler();
            s.Start(0);
            // Không sample gì, flush ngay — server verify readArrayLength(2, 64).
            var arr = s.Flush(100, 100);
            Assert.True(arr.Length >= 4, "Ít nhất 2 điểm (4 int) để server chấp nhận");
        }

        [Fact]
        public void Sample_ZigZagQuaTran_KhongVuot64Int_KhopReadArrayLength()
        {
            // Người chơi zig-zag 200 frame ở 60 fps — không cap → mảng vượt 400 int,
            // MapHandler.SendMove ném ArgumentException, avatar đứng hình câm.
            var s = new PathSampler();
            s.Start(0);
            // Các điểm này không collinear (đảo dấu y mỗi bước) → không tối ưu được.
            for (var i = 0; i < 200; i++) s.Sample(i, i % 2 == 0 ? 0 : 1);
            var arr = s.Flush(999, 999);
            Assert.True(arr.Length >= 4, "vẫn tối thiểu 2 cặp");
            Assert.True(arr.Length <= 64, $"vượt 64 int ({arr.Length}) — server sẽ từ chối");
            // Điểm cuối vẫn phải là đích, không bị cắt mất.
            Assert.Equal(999, arr[arr.Length - 2]);
            Assert.Equal(999, arr[arr.Length - 1]);
        }
    }
}
