using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>NPC hoặc quái nhận động từ server, có ảnh động và nhãn tên.</summary>
    public sealed partial class WorldActorView : MonoBehaviour, IPointerClickHandler
    {
        private const float NameScale = 0.75f; // cỡ tên NPC/quái, đồng bộ với PlayerAvatar
        // Nút "Nói chuyện" đặt sát NGỰC NPC (~1/2 chiều cao sprite), lệch sang TRÁI khỏi thân —
        // không đè lên NPC, không đè nhãn tên (nhãn ở trên đầu). Lệch = nửa panel 25px + nửa
        // bề ngang sprite + đệm 4px (NPC 48px → 53px). Tính theo sprite thật: NPC to (Thợ Rèn)
        // mà lệch cố định thì nút chui vào thân NPC.
        private const float PromptHalfWidth = 25f;
        private const float PromptGap = 4f;
        private const float PromptChestFactor = 0.5f;
        private SpriteRenderer _renderer;
        private Transform _visual;   // node chứa sprite; idle-bob ép scale.y node NÀY, không đụng nhãn tên
        private JarNameLabel _label; // tên bằng bitmap font jar, đồng bộ với PlayerAvatar
        private Sprite[] _frames = Array.Empty<Sprite>();
        private Action _clicked;
        private float _nextFrame;
        private int _frame;
        private bool _idleBob;
        private float _bobNext;
        private bool _bobDown;
        private string _baseLabelText;
        private int _labelOrder;
        private NpcTalkPrompt _talkPrompt;

        public int? BossHp { get; private set; }

        /// <summary>Vị trí bong bóng cao hơn tên NPC và đỉnh sprite.</summary>
        internal float PurposeBubbleOffsetY
        {
            get
            {
                var height = _renderer != null && _renderer.sprite != null
                    ? _renderer.sprite.rect.height : 48f;
                // Nhãn tên nằm ở đỉnh sprite + 4px. Chừa thêm một khoảng nhỏ để
                // bong bóng sát đầu NPC nhưng không đè lên nhãn tên.
                return height + 16f;
            }
        }

        /// <summary>Nhún dọc tại chỗ 400ms/nhịp — mô phỏng `dg` type 1 của jar.</summary>
        public void EnableIdleBob()
        {
            _idleBob = true;
            _bobNext = Time.time + UnityEngine.Random.Range(0f, 0.4f); // lệch pha, NPC không nhún đồng loạt
        }

        /// <summary>Hiện/ẩn nút "Nói chuyện" cạnh NPC (xem <see cref="WorldActorLayer"/>,
        /// nơi quét khoảng cách người chơi để gọi hàm này). Dựng lười — chỉ tạo lần đầu cần hiện.</summary>
        /// <summary>Có nhận bấm không; NPC trang trí thì không — không hiện nút "Nói chuyện".</summary>
        internal bool IsInteractive => _clicked != null;

        internal void SetTalkPromptVisible(bool value)
        {
            if (_talkPrompt == null)
            {
                if (!value) return;
                var hasSprite = _renderer != null && _renderer.sprite != null;
                var spriteHeight = hasSprite ? _renderer.sprite.rect.height : 48f;
                var spriteWidth = hasSprite ? _renderer.sprite.rect.width : 48f;
                var offsetX = -(PromptHalfWidth + spriteWidth * 0.5f + PromptGap);
                var offset = new Vector2(offsetX, spriteHeight * PromptChestFactor);
                _talkPrompt = NpcTalkPrompt.Attach(transform, offset, () => _clicked?.Invoke());
            }
            _talkPrompt.SetVisible(value);
        }

        private void MakeLabel(string value, int order)
        {
            // Nhãn nằm TRÊN root (không dưới _visual) nên không bị idle-bob ép méo.
            _baseLabelText = value ?? string.Empty;
            _labelOrder = order;
            _label = JarNameLabel.Create(transform, Vector3.zero, NameScale, _baseLabelText);
            _label.SetSortingOrder(order);
            PlaceLabel();
        }

        /// <summary>
        /// Dời quái sang chỗ mới (xem <see cref="MobWanderer"/>). Phải cập nhật CẢ thứ
        /// tự vẽ: nó tính theo Y, quái đi xuống mà giữ thứ tự cũ là chui ra sau cái cây
        /// mà đáng lẽ nó đang đứng trước.
        /// </summary>
        internal void MoveTo(int jarX, int jarY, int mapHeightPixels)
        {
            var (x, y) = UiLogic.MapPlacement.JarToWorld(jarX, jarY, mapHeightPixels);
            transform.localPosition = new Vector3(x, y, 0f);

            var order = UiLogic.MapPlacement.ActorSortingOrder(jarY);
            if (order == _renderer.sortingOrder) return;
            _renderer.sortingOrder = order;
            _labelOrder = order + 20;
            _label?.SetSortingOrder(_labelOrder);
        }

        public void SetBossHp(int hp)
        {
            BossHp = Mathf.Max(0, hp);
            if (_label == null) return;
            _label.SetText($"{_baseLabelText}  HP {BossHp.Value}");
            _label.SetSortingOrder(_labelOrder);
        }

        private void PlaceLabel()
        {
            if (_label == null) return;
            var height = _renderer != null && _renderer.sprite != null
                ? _renderer.sprite.rect.height : 48f;
            _label.transform.localPosition = new Vector3(0f, height + 4f, 0f); // đáy tên trên đỉnh sprite
        }

        private void ConfigureCollider(int[] bounds)
        {
            var collider = GetComponent<BoxCollider2D>();
            var width = _renderer?.sprite != null ? _renderer.sprite.bounds.size.x : 32f;
            var height = _renderer?.sprite != null ? _renderer.sprite.bounds.size.y : 48f;
            if (bounds != null && bounds.Length == 4)
            {
                // Bounds server thường chỉ là ô chân NPC (-25,-25,50,50). Hợp cả
                // ô này lẫn toàn thân sprite để người chơi chạm vào đầu/thân NPC
                // vẫn mở được hội thoại.
                var left = Mathf.Min(-width * 0.5f, bounds[0]);
                var right = Mathf.Max(width * 0.5f, bounds[0] + bounds[2]);
                var bottom = Mathf.Min(0f, bounds[1]);
                var top = Mathf.Max(height, bounds[1] + bounds[3]);
                width = Mathf.Max(12f, right - left);
                height = Mathf.Max(12f, top - bottom);
                collider.offset = new Vector2((left + right) * 0.5f, (bottom + top) * 0.5f);
            }
            else collider.offset = new Vector2(0f, height * 0.5f);
            collider.size = new Vector2(width, height);
        }

        private void Update()
        {
            IdleBob();
            // Frame animation cho sprite nhiều frame (quái). NPC map là 1-frame (server gửi
            // frameCount=1) nên không đổi frame — chỉ nhún qua IdleBob, khớp jar.
            if (_frames.Length <= 1 || Time.time < _nextFrame) return;
            _nextFrame = Time.time + 0.16f;
            _frame = (_frame + 1) % _frames.Length;
            _renderer.sprite = _frames[_frame];
        }

        private void IdleBob()
        {
            if (!_idleBob || Time.time < _bobNext) return;
            _bobNext = Time.time + 0.4f;
            _bobDown = !_bobDown;
            if (_visual == null) return;

            // Ép dọc SPRITE quanh chân (pivot 0.5,0) nên chân đứng yên. Hệ số phải làm
            // sprite lùn đi ĐÚNG 1 pixel nguồn, không phải 0.95 cố định: 0.95 cho ra
            // chiều cao lẻ (24 → 22.8 px), pixel bị lấy mẫu lệch và con vật trông nhoè
            // suốt nửa nhịp nhún. Lùn 1 pixel vẫn đủ thấy nhún.
            var height = _renderer != null && _renderer.sprite != null
                ? _renderer.sprite.rect.height
                : 0f;
            var scale = _visual.localScale;
            scale.y = _bobDown && height >= 2f ? (height - 1f) / height : 1f;
            _visual.localScale = scale;
        }

        public void OnPointerClick(PointerEventData eventData) => _clicked?.Invoke();
    }
}
