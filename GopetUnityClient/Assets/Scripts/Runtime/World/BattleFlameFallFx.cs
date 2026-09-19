using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Hiệu ứng "mưa lửa" HAI GIAI ĐOẠN: ngọn lửa dội từ trên xuống, chạm đất thì
    /// bùng thành cột lửa cao, cháy một nhịp rồi lụi.
    ///
    /// <para>Hai giai đoạn đọc ra từ SỐ ĐO ảnh mẫu: ngọn lơ lửng trên không cao 41–83 đơn vị
    /// canvas còn cột dưới đất cao 260, và ngọn càng xuống thấp càng to ⇒ lửa đang RƠI chứ
    /// không phải tàn lửa bay lên.</para>
    ///
    /// <para><b>Hai lớp</b> sau/trước card pet: ảnh mẫu cho pet đứng GIỮA đám lửa. Dồn vào một
    /// lớp thì lửa đè kín pet — hiệu ứng thêm vào canvas sau các card nên mặc định nằm trên.</para></summary>
    public sealed partial class BattleFlameFallFx : MonoBehaviour
    {
        private const int JetCount = 9;
        private const float FallSeconds = 0.20f;      // dội từ trên xuống
        private const float GrowSeconds = 0.15f;      // bùng thành cột
        private const float HoldSeconds = 0.28f;
        private const float FadeSeconds = 0.20f;
        private const float StaggerSeconds = 0.040f;
        private const float FadeInSeconds = 0.05f;

        /// <summary>Độ cao thả, theo chiều cao canvas — số pixel cứng sẽ thả ngoài khung ở
        /// máy thấp vì canvas co giãn theo màn hình.</summary>
        private const float DropHeightRatio = 0.62f;

        /// <summary>Chiều cao ngọn cao nhất, theo chiều cao CANVAS (ảnh mẫu: 260 trên canvas 540).
        ///
        /// <para>Neo vào canvas chứ KHÔNG vào chiều cao pet: sprite pet mỗi con một cỡ (texture
        /// cao 36–56 px × <c>BattleSkin.SpriteScale</c> ⇒ 108–168 đơn vị canvas), buộc vào đó
        /// thì con pet nhỏ kéo cả đám lửa bé theo.</para></summary>
        private const float MaxHeightRatio = 0.44f;

        /// <summary>Nửa bề rộng cụm, cũng theo chiều cao canvas (ảnh mẫu: cụm rộng 260). Theo
        /// chiều CAO chứ không phải bề ngang để cụm không đổi hình khi tỉ lệ màn đổi.</summary>
        private const float SpreadRatio = 0.22f;

        /// <summary>Ngọn càng xa tâm càng thấp — cụm mới có dáng vòm như ảnh mẫu.</summary>
        private const float EdgeFalloff = 0.55f;

        /// <summary>Độ lệch ngang khi rơi, theo quãng rơi ⇒ lửa rơi CHÉO. 0.35 ≈ nghiêng 19°
        /// so với phương thẳng đứng. Hướng nghiêng lấy theo phía pet ra đòn, không chọn bừa:
        /// đòn bay từ hướng kẻ tấn công xuống thì đọc ra câu chuyện.</summary>
        private const float DriftRatio = 0.35f;

        /// <summary>Cỡ ngọn lúc ĐANG RƠI so với cột nó sẽ bùng. Ảnh mẫu: 41–83 trên 260.</summary>
        private const float FallMinRatio = 0.28f, FallMaxRatio = 0.42f;

        /// <summary>Cột đứng TRƯỚC pet phải lệch khỏi tâm ít nhất bằng này (theo nửa bề rộng
        /// cụm), nếu không nó che đúng mặt pet.</summary>
        private const float FrontMinOffset = 0.34f;

        private struct Jet
        {
            public RectTransform Rect;
            public CanvasGroup Group;
            public float Delay, FallRatio, Tilt;
            public Vector3 From, To;
        }

        private Jet[] _jets;
        private Transform _back;
        private float _born;

        /// <param name="impactSeconds">Giây tới lúc phần lớn cột đã bùng lên — người gọi chờ
        /// rồi mới trừ máu, nếu không pet gục khi lửa còn chưa chạm đất.</param>
        /// <returns>false nếu kỹ năng này không có ảnh ghi đè.</returns>
        /// <param name="fromWorld">Điểm xuất phát gợi ý (phía pet ra đòn); null thì rơi thẳng.</param>
        public static bool TryPlay(Transform parent, RectTransform target, int skillId,
            Vector3? fromWorld, out float impactSeconds)
        {
            impactSeconds = 0f;
            var sprite = BattleSkin.Load($"Battle/fx/{skillId}");
            if (sprite == null || target == null) return false;

            var canvasHeight = parent is RectTransform root && root.rect.height > 1f
                ? root.rect.height : 540f;
            var petH = Mathf.Max(32f, target.rect.height);
            var aspect = sprite.rect.width / sprite.rect.height;

            // Dấu của độ lệch: lửa nghiêng về phía pet ra đòn. Buff lên chính mình không có
            // hướng nào cả (fromWorld null) thì rơi thẳng.
            var lean = fromWorld == null ? 0f
                : Mathf.Sign(fromWorld.Value.x - target.position.x) * DriftRatio;

            var back = NewLayer(parent, "Lửa sau pet", true);
            var front = NewLayer(parent, "Lửa trước pet", false);

            var fx = front.gameObject.AddComponent<BattleFlameFallFx>();
            fx._back = back;
            fx._born = Time.unscaledTime;
            fx._jets = new Jet[JetCount];
            for (var i = 0; i < JetCount; i++)
            {
                fx._jets[i] = Build(sprite, target, i, canvasHeight, petH, aspect,
                    canvasHeight * DropHeightRatio, lean, back, front);
            }

            // Không chờ cột CUỐI: cụm đã ra dáng từ giữa chừng, chờ hết thì số sát thương
            // hiện ra muộn hẳn so với lúc lửa giáng xuống.
            impactSeconds = FallSeconds + GrowSeconds + JetCount * 0.35f * StaggerSeconds;
            return true;
        }

        private void Update()
        {
            var now = Time.unscaledTime - _born;
            var alive = false;
            for (var i = 0; i < _jets.Length; i++)
            {
                if (Step(_jets[i], now - _jets[i].Delay)) alive = true;
            }
            if (alive) return;
            if (_back != null) Destroy(_back.gameObject);
            Destroy(gameObject);
        }

        /// <returns>false khi ngọn này đã lụi hẳn.</returns>
        private static bool Step(Jet j, float age)
        {
            if (age < 0f) { j.Group.alpha = 0f; return true; }
            j.Group.alpha = 1f;

            if (age < FallSeconds)
            {
                // k = t² cho có gia tốc rơi; rơi đều trông như đang trôi chứ không phải rơi.
                var k = (age / FallSeconds) * (age / FallSeconds);
                j.Rect.position = Vector3.Lerp(j.From, j.To, k);
                // Phóng ĐỀU hai trục: kéo riêng trục Y sẽ bóp méo ngọn lửa. To dần khi xuống
                // thấp, đúng như ảnh mẫu — ngọn càng gần đất càng lớn.
                var s = j.FallRatio * (0.70f + 0.30f * k);
                j.Rect.localScale = new Vector3(s, s, 1f);
                // Nghiêng theo đúng đường bay: ngọn lửa chúc về hướng đang lao tới, đuôi
                // kéo lại phía sau. Rơi chéo mà thân vẫn dựng đứng thì trông như trượt ngang.
                j.Rect.localRotation = Quaternion.Euler(0f, 0f, j.Tilt);
                j.Group.alpha = Mathf.Clamp01(age / FadeInSeconds);
                return true;
            }

            j.Rect.position = j.To;
            var a = age - FallSeconds;
            if (a < GrowSeconds)
            {
                // Mũ 0.55 cho bùng NHANH rồi chậm lại — lửa nổ lên chứ không dâng đều.
                j.Rect.localScale = new Vector3(1f, Mathf.Pow(a / GrowSeconds, 0.55f), 1f);
                // Chạm đất thì dựng thẳng dần: lửa cháy thì bốc lên, không nằm nghiêng.
                j.Rect.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Lerp(j.Tilt, 0f, a / GrowSeconds));
                return true;
            }
            j.Rect.localRotation = Quaternion.identity;

            if (a < GrowSeconds + HoldSeconds) { j.Rect.localScale = Vector3.one; return true; }

            var f = (a - GrowSeconds - HoldSeconds) / FadeSeconds;
            if (f >= 1f) { j.Group.alpha = 0f; return false; }
            // Lụi thì THẤP dần chứ không chỉ mờ đi — lửa tắt là ngọn tụt xuống.
            j.Rect.localScale = new Vector3(1f, 1f - 0.45f * f, 1f);
            j.Group.alpha = 1f - f;
            return true;
        }
    }
}
