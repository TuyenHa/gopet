using Gopet.Net.Guider;
using Xunit;

namespace Gopet.Net.Tests
{
    /// <summary>Chuẩn hoá tag (sao)/(saoden) sang glyph — chống rò "(saoden)(saoden)..." ra UI.</summary>
    public sealed class GameTextTagsTests
    {
        [Fact]
        public void Substitute_ThayCaSaoVaSaoDen()
        {
            var result = GameTextTags.Substitute("rùa baby (sao)(sao)(sao)(saoden)(saoden)");
            Assert.Equal("rùa baby ★★★☆☆", result);
        }

        [Fact]
        public void Substitute_KhongCoTag_TraNguyenChuoi()
        {
            var input = "Bạn có muốn mua?";
            Assert.Same(input, GameTextTags.Substitute(input));
        }

        [Fact]
        public void Substitute_ChuoiRong_TraNguyenChuoi()
        {
            Assert.Equal("", GameTextTags.Substitute(""));
            Assert.Null(GameTextTags.Substitute(null));
        }

        [Fact]
        public void Substitute_TagDinhLien_KhongLech()
        {
            Assert.Equal("★☆★", GameTextTags.Substitute("(sao)(saoden)(sao)"));
        }

        [Fact]
        public void Substitute_NgoacDonThuong_KhongDungCham()
        {
            // "(vang)" và các tag khác không phải sao — giữ nguyên, không lệch cursor.
            Assert.Equal("200 (vang) ★", GameTextTags.Substitute("200 (vang) (sao)"));
        }
    }
}
