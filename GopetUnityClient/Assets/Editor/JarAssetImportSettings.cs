using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gopet.Editor
{
    /// <summary>
    /// Ép import setting cho asset lấy từ client J2ME cũ (<c>tools/unpack-jar-dat</c>),
    /// thay vì giao cho người nhớ.
    ///
    /// <para><b>Vì sao bắt buộc phải ép ảnh:</b> bỏ bước này thì ảnh mờ nhoè trên máy
    /// thật mà trong Editor vẫn trông ổn — filter mặc định của Unity (<c>Bilinear</c>)
    /// làm mượt pixel art, và mắt nhìn trong scene view ở tỉ lệ 1:1 không thấy khác
    /// biệt. Đây đúng là lớp mỹ thuật của phase 5.1, không phải chi tiết vặt.</para>
    ///
    /// <para>Chỉ áp ảnh cho <c>Assets/Resources/Jar/Art/</c> — ảnh tải từ server qua
    /// <c>RemoteAssetCache</c> đi đường khác (<c>TextureFactory</c>, không qua asset
    /// pipeline của Unity) và đã tự đặt <c>FilterMode.Point</c> ở P4.</para>
    /// </summary>
    public sealed class JarAssetImportSettings : AssetPostprocessor
    {
        /// <summary>Mọi ảnh ép setting ở đây đều nằm dưới thư mục này.</summary>
        public const string JarArtRoot = "Assets/Resources/Jar/Art/";

        /// <summary>Mọi âm thanh ép setting ở đây đều nằm dưới thư mục này.</summary>
        public const string JarAudioRoot = "Assets/Resources/Jar/Audio/";

        /// <summary>PPU thống nhất cho toàn bộ sprite lấy từ jar — tuỳ ý miễn nhất quán.</summary>
        private const float PixelsPerUnit = 32f;

        /// <summary>
        /// Nhạc nền: tên bắt đầu bằng một trong các tiền tố này. Còn lại là hiệu ứng.
        /// Không dò bằng kích thước file — hai file skill lớn (mui.dat) không phải nhạc.
        /// </summary>
        private static readonly string[] MusicPrefixes = { "s_login", "s_outMap" };

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(JarArtRoot)) return;

            var importer = (TextureImporter)assetImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;

            // NPOT scale None: mọi tile 12x12, 28x26... phải giữ đúng kích thước gốc.
            // Cho Unity tự phóng lên bội số 2 là lệch pixel với chính bảng offset đã
            // giải ra ở tools/unpack-jar-dat.
            importer.npotScale = TextureImporterNPOTScale.None;

            var platformSettings = importer.GetDefaultPlatformTextureSettings();
            platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
            platformSettings.format = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(platformSettings);
        }

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(JarAudioRoot)) return;

            var importer = (AudioImporter)assetImporter;
            var name = Path.GetFileNameWithoutExtension(assetPath);
            var settings = importer.defaultSampleSettings;

            if (IsMusic(name))
            {
                // Nhạc nền phát dài, nén được nhiều mà tai khó phân biệt — Vorbis.
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.7f;
            }
            else
            {
                // Hiệu ứng ngắn, phát tức thời khi bấm nút/đánh trúng — giải nén sẵn
                // trong RAM để không có độ trễ giải mã lúc phát.
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.ADPCM;
            }

            importer.defaultSampleSettings = settings;
        }

        private static bool IsMusic(string fileNameWithoutExt)
        {
            foreach (var prefix in MusicPrefixes)
            {
                if (fileNameWithoutExt.StartsWith(prefix)) return true;
            }

            return false;
        }
    }
}
