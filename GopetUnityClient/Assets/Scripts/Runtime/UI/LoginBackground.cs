using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Tranh nền màn đăng nhập, phủ kín màn hình.
    ///
    /// <para><b>Khác bản jar một cách CÓ CHỦ Ý.</b> Jar vẽ nền bằng map 11 ghép từ ô
    /// 24×24 và cho trôi ngang (<see cref="JarMapBackground"/> đã port đúng phần đó và
    /// vẫn dùng được cho màn chơi). Màn đăng nhập dùng tranh riêng của dự án.</para>
    ///
    /// <para><b>MỘT lớp duy nhất, phủ kín màn hình.</b> Bản trước dùng hai lớp (một
    /// lớp phủ kín làm nền lót, một lớp vừa trọn khung) để thấy được toàn cảnh mà
    /// không hở viền — nhưng trên màn rộng, phần lót thò ra hai mép trông đúng như
    /// hai tấm ảnh chồng nhau. Nay chỉ còn một tấm: giữ nguyên tỉ lệ, tràn ra ngoài
    /// khung và bị cắt bớt. Ảnh có nhiều trời và cỏ ở hai mép nên cắt không mất gì
    /// đáng kể, còn kéo giãn cho vừa thì cảnh bị méo.</para>
    /// </summary>
    public sealed class LoginBackground : MonoBehaviour
    {
        /// <summary>Nằm ngoài <c>Jar/</c>: asset của dự án, không phải thứ giải ra từ jar.</summary>
        public const string PictureResource = "Ui/login-background";

        public static LoginBackground Create(Transform parent)
        {
            var go = new GameObject("LoginBackground", typeof(RectTransform), typeof(RectMask2D));
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(0); // vẽ DƯỚI mọi thứ khác
            UiBuilder.Stretch((RectTransform)go.transform);

            var picture = Resources.Load<Sprite>(PictureResource);
            if (picture == null)
            {
                Debug.LogWarning($"[Gopet] Không tìm thấy nền màn đăng nhập tại Resources/{PictureResource} — dùng tạm nền màu.");
            }

            AddPicture(go.transform, picture);
            return go.AddComponent<LoginBackground>();
        }

        private static void AddPicture(Transform parent, Sprite picture)
        {
            var go = new GameObject("Picture", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.raycastTarget = false; // nền phủ kín màn, bật raycast là nuốt hết cú chạm
            image.sprite = picture;
            image.color = picture == null ? UiBuilder.JarBackground : Color.white;

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = picture == null
                ? AspectRatioFitter.AspectMode.None
                : AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = picture == null || picture.rect.height <= 0f
                ? 1f
                : picture.rect.width / picture.rect.height;

            // Không có ảnh thì AspectRatioFitter bị tắt, phải tự trải kín khung.
            if (picture == null) UiBuilder.Stretch(rect);
        }
    }
}
