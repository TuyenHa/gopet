using System.Collections;
using Gopet.Net.Battle;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed class BattlePetCard : MonoBehaviour
    {
        /// <summary>Nhịp quay frame lúc đứng yên.</summary>
        private const float IdleFrameInterval = 0.16f;

        /// <summary>Nhịp lúc di chuyển. Sprite pet chỉ là 1 strip N frame dùng chung cho mọi
        /// trạng thái — không có bộ frame "chạy" riêng — nên "chạy" là quay vòng chính các
        /// frame đó nhanh gấp đôi, đúng cách jar làm.</summary>
        private const float RunFrameInterval = 0.08f;

        private const float FaintSeconds = 0.5f;
        private const float FaintAngle = -80f;
        private const float FaintSinkPixels = 6f;
        private const float FaintAlpha = 0.75f;

        private BattlePet _pet;
        private RawImage _petImage;
        private RectTransform _rect;
        private Vector2 _home;
        private float _frameInterval = IdleFrameInterval;
        private bool _fainted;
        private int _frame;
        private float _nextFrame;

        public int ActorId => _pet.ActorId;
        public int Hp => _pet.Hp;
        public int Mp => _pet.Mp;
        public int MaxHp => _pet.MaxHp;
        public int MaxMp => _pet.MaxMp;
        public RectTransform EffectAnchor => _petImage.rectTransform;

        public static BattlePetCard Create(Transform parent, BattlePet pet, bool left,
            RemoteAssetCache assets)
        {
            var go = new GameObject(left ? "Pet trái" : "Pet phải", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(left ? 0.31f : 0.66f, 0.45f);
            rect.sizeDelta = new Vector2(160f, 200f);

            var card = go.AddComponent<BattlePetCard>();
            card._pet = pet;
            card._rect = rect;
            card._home = rect.anchoredPosition;
            card.BuildSprite(left);
            assets.Get(pet.ImagePath, ImagePackets.TypeNpc, card.SetTexture);
            return card;
        }

        public void Apply(int hpDelta, int mpDelta)
        {
            _pet.Hp = Mathf.Clamp(_pet.Hp + hpDelta, 0, Mathf.Max(0, _pet.MaxHp));
            _pet.Mp = Mathf.Clamp(_pet.Mp + mpDelta, 0, Mathf.Max(0, _pet.MaxMp));
            if (hpDelta != 0) BattleFloatText.Create(transform, hpDelta, false);
            // Số MP hiện SAU số HP ~1s — jar phát 2 bước tuần tự (ei.java:213-291),
            // không chồng 2 con số lên nhau.
            if (mpDelta != 0) BattleFloatText.Create(transform, mpDelta, true, 1f);
            FaintIfDown();
        }

        /// <summary>Gán tuyệt đối (không phải delta) — dùng cho snapshot MY_PET_INFO.</summary>
        public void SetVitals(int hp, int maxHp, int mp, int maxMp)
        {
            _pet.MaxHp = Mathf.Max(0, maxHp);
            _pet.MaxMp = Mathf.Max(0, maxMp);
            _pet.Hp = Mathf.Clamp(hp, 0, Mathf.Max(0, _pet.MaxHp));
            _pet.Mp = Mathf.Clamp(mp, 0, Mathf.Max(0, _pet.MaxMp));
            FaintIfDown();
        }

        /// <summary>Hết máu thì đổ vật ra. Jar ép dẹp sprite xuống đất theo từng lát 4px
        /// (<c>ei.java:90-96</c> vẽ lát cao <c>4-r</c>, <c>bd.java:241-252</c> tăng <c>r</c>
        /// 1→4); ở đây làm rõ hơn thành "đổ nghiêng quanh gốc chân + lún + mờ".
        ///
        /// <para>Pivot của sprite là (0.5, 0) tức gốc chân, nên xoay quanh nó ra đúng dáng
        /// keel-over. Card bên trái có <c>localScale.x = -1</c> (lật hình), nên cùng một góc
        /// âm sẽ cho hai bên đổ ra hai phía ngược nhau — đúng ý, không cần phân nhánh.</para></summary>
        private void FaintIfDown()
        {
            if (_fainted || _pet.Hp > 0 || _petImage == null) return;
            _fainted = true;
            StartCoroutine(FaintRoutine());
        }

        private IEnumerator FaintRoutine()
        {
            var rect = _petImage.rectTransform;
            var fromPos = rect.anchoredPosition;
            var toPos = fromPos + new Vector2(0f, -FaintSinkPixels);
            var elapsed = 0f;
            while (elapsed < FaintSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / FaintSeconds));
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, FaintAngle, t));
                rect.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
                _petImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, FaintAlpha, t));
                yield return null;
            }
            rect.localRotation = Quaternion.Euler(0f, 0f, FaintAngle);
            rect.anchoredPosition = toPos;
            _petImage.color = new Color(1f, 1f, 1f, FaintAlpha);
        }

        public float HomeX => _home.x;

        /// <summary>Đã hết máu và đang/đã nằm vật ra.</summary>
        public bool Fainted => _fainted;

        /// <summary>Vị trí anchor ngang (0..1) trong canvas. Hướng và khoảng cách lao phải
        /// tính từ đây chứ KHÔNG từ <see cref="HomeX"/>: <c>Create</c> chỉ đặt anchor và
        /// sizeDelta, không bao giờ đặt <c>anchoredPosition</c>, nên `HomeX` bằng 0 ở cả hai
        /// card — lấy hiệu hai `HomeX` sẽ ra 0 và `Mathf.Sign(0)` = 1, tức luôn lao sang phải.</summary>
        public float AnchorX => _rect != null ? _rect.anchorMin.x : 0f;

        /// <summary>Lao tới cạnh đối phương. Trong lúc di chuyển nhịp frame tăng gấp đôi
        /// cho ra cảm giác chạy (xem <see cref="RunFrameInterval"/>).</summary>
        public IEnumerator PlayLunge(float targetX, float seconds) =>
            MoveX(targetX, seconds, RunFrameInterval);

        public IEnumerator ReturnHome(float seconds) =>
            MoveX(_home.x, seconds, RunFrameInterval);

        /// <summary>Về đúng chỗ tức thì — dùng khi trận kết thúc giữa lúc đang diễn hoạt.</summary>
        public void SnapHome()
        {
            if (_rect != null) _rect.anchoredPosition = _home;
            _frameInterval = IdleFrameInterval;
        }

        private IEnumerator MoveX(float targetX, float seconds, float frameInterval)
        {
            if (_rect == null || seconds <= 0f) { SnapHome(); yield break; }
            _frameInterval = frameInterval;
            var from = _rect.anchoredPosition;
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / seconds));
                _rect.anchoredPosition = new Vector2(Mathf.Lerp(from.x, targetX, t), from.y);
                yield return null;
            }
            _rect.anchoredPosition = new Vector2(targetX, from.y);
            _frameInterval = IdleFrameInterval;
        }

        private void BuildSprite(bool left)
        {
            var imageGo = new GameObject("Pet", typeof(RectTransform), typeof(RawImage));
            imageGo.transform.SetParent(transform, false);
            _petImage = imageGo.GetComponent<RawImage>();
            _petImage.raycastTarget = false;
            var rect = _petImage.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 16f - _pet.VerticalOffset * 2f);
            rect.sizeDelta = new Vector2(96f, 128f);
            rect.localScale = new Vector3(left ? -1f : 1f, 1f, 1f);
        }

        private void SetTexture(Texture2D texture)
        {
            if (this == null || texture == null) return;
            _petImage.texture = texture;
            var count = Mathf.Max(1, _pet.FrameCount);
            var frameWidth = texture.width / count;
            // Cùng hệ số với hoạt cảnh hiệu ứng — xem BattleSkin.SpriteScale.
            _petImage.rectTransform.sizeDelta =
                new Vector2(frameWidth * BattleSkin.SpriteScale, texture.height * BattleSkin.SpriteScale);
            ShowFrame();
        }

        private void Update()
        {
            // Đã xỉu thì đứng hình — pet nằm vật ra mà chân vẫn đi là sai.
            if (_fainted || _petImage.texture == null || _pet.FrameCount <= 1
                || Time.time < _nextFrame) return;
            _nextFrame = Time.time + _frameInterval;
            _frame = (_frame + 1) % _pet.FrameCount;
            ShowFrame();
        }

        private void ShowFrame()
        {
            var count = Mathf.Max(1, _pet.FrameCount);
            _petImage.uvRect = new Rect((float)_frame / count, 0f, 1f / count, 1f);
        }
    }
}
