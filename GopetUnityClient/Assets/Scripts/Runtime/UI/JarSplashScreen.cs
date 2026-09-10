using System.Collections;
using Gopet.Runtime.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Màn splash dùng tranh riêng của dự án, căn giữa và phủ kín toàn bộ màn hình
    /// mà không kéo méo; khi tỉ lệ khung khác ảnh gốc thì chỉ cắt nhẹ ở mép trên/dưới,
    /// kèm nhạc nền <c>s_login</c>.
    ///
    /// <para><b>Khác bản jar ở LÝ DO đóng màn:</b> jar đóng khi tải xong dữ liệu
    /// (<c>fx.java</c> nạp <c>common.dat</c> trong lúc hiện logo — J2ME thật sự mất
    /// thời gian đọc đĩa). Client Unity không có bước tải tương đương — mọi asset đã
    /// nằm sẵn trong <c>Resources</c>. Nên ở đây đóng theo THỜI GIAN tối thiểu, hoặc
    /// chạm màn hình để bỏ qua sớm hơn.</para>
    /// </summary>
    public sealed class JarSplashScreen : MonoBehaviour, IPointerClickHandler
    {
        private const string MusicName = "s_login";

        public const string PictureResource = "Ui/splash-background";

        private SoundManager _sound;
        private float _minimumSeconds;
        private bool _dismissed;
        private bool _minimumElapsed;
        private bool _finishAllowed;

        public event System.Action Finished;

        /// <summary>
        /// Đặt <see cref="JarSplashScreen"/> lên chính GameObject nền: nó phủ toàn màn
        /// hình và chắn raycast, nên đây đúng là nơi nhận sự kiện "chạm để bỏ qua".
        /// </summary>
        public static JarSplashScreen Create(PixelCanvas canvas, SoundManager sound, float minimumSeconds = 2f,
            bool waitForSignal = false)
        {
            var backgroundGo = new GameObject("JarSplashScreen", typeof(RectTransform), typeof(Image));
            backgroundGo.transform.SetParent(canvas.transform, false);
            backgroundGo.transform.SetSiblingIndex(0); // vẽ TRƯỚC — nằm DƯỚI mọi thứ khác trên canvas
            UiBuilder.Stretch((RectTransform)backgroundGo.transform);
            backgroundGo.GetComponent<Image>().color = new Color(0.29f, 0.72f, 0.92f, 1f);

            AddPicture(backgroundGo.transform, Resources.Load<Sprite>(PictureResource));

            // Tắt TRƯỚC khi AddComponent: GameObject đang active thì AddComponent
            // chạy Awake+OnEnable NGAY LẬP TỨC, tức trước khi kịp gán field ở dưới —
            // nhạc không phát (_sound vẫn null) và màn tự đóng ngay ở frame sau
            // (WaitForSeconds(0) vì _minimumSeconds vẫn 0).
            backgroundGo.SetActive(false);
            var splash = backgroundGo.AddComponent<JarSplashScreen>();
            splash._sound = sound;
            splash._minimumSeconds = minimumSeconds;
            splash._finishAllowed = !waitForSignal;
            backgroundGo.SetActive(true);
            return splash;
        }

        private static void AddPicture(Transform parent, Sprite picture)
        {
            if (picture == null)
            {
                Debug.LogWarning($"[Gopet] Không tìm thấy ảnh splash tại Resources/{PictureResource}.");
                return;
            }

            var go = new GameObject("Picture", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            image.sprite = picture;

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = picture.rect.width / picture.rect.height;
        }

        private void OnEnable()
        {
            _sound?.PlayMusic(MusicName);
            StartCoroutine(DismissAfter(_minimumSeconds));
        }

        /// <summary>Cho phép splash đóng sau khi kiểm tra kết nối đã hoàn tất.</summary>
        public void AllowFinish()
        {
            _finishAllowed = true;
            if (_minimumElapsed) Dismiss();
        }

        /// <summary>Chạm màn hình chỉ bỏ qua được khi app đã sẵn sàng rời splash.</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_finishAllowed) Dismiss();
        }

        private IEnumerator DismissAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _minimumElapsed = true;
            if (_finishAllowed) Dismiss();
        }

        private void Dismiss()
        {
            if (_dismissed) return;

            _dismissed = true;

            // Tự dọn mình TRƯỚC khi báo Finished: nền này phủ kín màn hình và chắn
            // tia raycast. Người nghe Finished (GopetBootstrap) gọi _flow.Start(),
            // và nếu bước đó ném — mạng hỏng, DNS sai — nền vẫn phải biến mất trước
            // đó, không thì nó kẹt lại vĩnh viễn che hết màn "mất kết nối" phía sau,
            // và chạm vào cũng vô ích vì _dismissed đã true.
            gameObject.SetActive(false);
            Destroy(gameObject);

            Finished?.Invoke();
        }
    }
}
