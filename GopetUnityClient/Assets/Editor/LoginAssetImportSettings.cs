using UnityEditor;
using UnityEngine;

namespace Gopet.Editor
{
    /// <summary>
    /// Ép setting cho bộ art màn đăng nhập (<c>Resources/Ui/</c>).
    ///
    /// <para>Ngược hẳn với <c>JarAssetImportSettings</c>: art của jar là pixel art nên
    /// ép lọc Point cho sắc cạnh, còn đây là tranh vẽ độ phân giải cao nên phải lọc
    /// Bilinear — để Point thì viền bo góc và chữ trên nút răng cưa rõ rệt.</para>
    ///
    /// <para><b>Bắt buộc phải là <see cref="TextureImporterType.Sprite"/>:</b> mọi thứ
    /// ở đây nạp bằng <c>Resources.Load&lt;Sprite&gt;</c> lúc runtime. Import nhầm thành
    /// Texture thì hàm đó trả <c>null</c> — UI vẫn dựng được nhưng tụt hết về màu phẳng,
    /// và không có lỗi nào báo ngoài một dòng cảnh báo dễ trôi mất.</para>
    /// </summary>
    public sealed class LoginAssetImportSettings : AssetPostprocessor
    {
        public const string UiRoot = "Assets/Resources/Ui/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(UiRoot)) return;

            var importer = (TextureImporter)assetImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;

            // KHÔNG nén. Nén khối (DXT/ETC) băm ảnh thành ô 4×4 rồi xấp xỉ màu trong
            // mỗi ô — với vùng chuyển màu mượt của nút và nền tối phẳng của ô nhập, nó
            // hiện thành từng vệt loang nhìn như xước. Bộ art này chỉ vài chục file
            // nhỏ nên đổi lại bằng dung lượng là đáng.
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;

            // Lưới FullRect, không phải Tight. uGUI chỉ vẽ được 9-slice trên lưới đầy;
            // với lưới Tight nó lặng lẽ vẽ như ảnh thường, và sprite ô nhập rộng 28px
            // bị kéo ra hơn 300px sẽ dẹt hết bo góc.
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }
    }
}
