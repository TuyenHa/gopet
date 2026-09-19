using System;
using Gopet.Net.Battle;
using Gopet.Net.Player;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using Gopet.Runtime.World.Battle;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed partial class BattleView : MonoBehaviour
    {
        /// <summary>Mở khoá cưỡng bức nếu gói lượt không bao giờ tới. Lượt server là
        /// <c>GopetManager.TimeNextTurn</c> = 25s; 8s đủ ngắn để không thấy kẹt, đủ dài
        /// để không đè lên round-trip bình thường.</summary>
        private const float PendingWatchdogSeconds = 8f;

        /// <summary>Băng chữ kết quả sống xong thì tự về map — lấy đúng hằng của
        /// <see cref="BattleResultBanner"/> để không đóng lúc chữ còn đang mờ dần.</summary>
        private const float ResultAutoCloseSeconds = BattleResultBanner.TotalSeconds;

        private BattleHandler _handler;
        private BattleStart _start;
        private BattlePetCard _left, _right;
        private BattleHudPanel _hudLeft, _hudRight;
        private BattleSkillPopup _skillPopup;
        private Button _skillButton;
        private BattleActionBar _actionBar;
        private BattleTopBar _topBar;
        private BattleVsIndicator _vsIndicator;
        private GameObject _result;
        private readonly SkillCooldownTracker _cooldowns = new SkillCooldownTracker();
        private BattleTurnState _turn;
        private BattleTurnAnimator _animator;
        private int[] _pendingVitals;
        private BattleResult _pendingResult;
        private float _pendingSince;
        private float _resultShownAt;
        private bool _closeRequested;
        private bool _surrendered;

        public int BattleId => _start.BattleId;
        public bool IsParticipant => _start.IsParticipant;
        public int OpponentActorId => _start.Opponent.ActorId;
        public int TurnDurationMs => _start.TurnDurationMs;

        /// <summary>Đã có kết quả (đang hiện hoặc đang chờ hoạt cảnh xong) — người khác đừng
        /// đóng hộ, để băng chữ kịp diễn.</summary>
        public bool HasResult => _result != null || _pendingResult != null;
        public event Action Closed;
        public event Action Ticked;

        public static BattleView Create(Transform parent, BattleStart start, BattleHandler handler,
            RemoteAssetCache assets, PlayerStats playerStats = null)
        {
            var go = new GameObject("Pet Battle", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 40;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f); scaler.matchWidthOrHeight = 1f;
            var view = go.AddComponent<BattleView>();
            view._handler = handler; view._start = start;
            view.Build(assets, UiBuilder.BuiltinFont(), playerStats);
            handler.BuffStateReceived += view.OnBuff;
            handler.StatsReceived += view.OnStats;
            handler.HitExpReceived += view.OnHitExp;
            return view;
        }

        private void OnDestroy()
        {
            if (_handler == null) return;
            _handler.BuffStateReceived -= OnBuff;
            _handler.StatsReceived -= OnStats;
            _handler.HitExpReceived -= OnHitExp;
        }

        public void Apply(BattleTurn turn)
        {
            if (turn.BattleId != BattleId) return;
            // Trạng thái lượt cập nhật NGAY (nhãn phải đúng tức thì); phần render thì xếp
            // hàng để diễn tuần tự thay vì nổ hết trong một frame.
            _turn.ApplyTurnPacket(turn.ActorId, turn.Type, turn.Effects.Length);
            _vsIndicator?.SetTurn(_turn.IsLocalTurn);
            _cooldowns.OnTurnAdvanced(turn.ActorId, _start.LocalPet.ActorId);
            // Kỹ năng TRƯỢT: server gửi shape của đòn thường (type Normal) thay vì WAIT của
            // kỹ năng, và không trừ MP cũng không đặt cooldown. Gỡ cooldown lạc quan đã đánh
            // dấu lúc bấm, nếu không nút xám oan 3 lượt.
            if (turn.ActorId == _start.LocalPet.ActorId && turn.Type != BattleTurn.Wait)
            {
                _cooldowns.CancelLastUsed();
            }
            _animator?.Enqueue(turn);
            RefreshLocks();
        }

        /// <summary>HP/MP thật của pet mình từ <c>MY_PET_INFO</c>. Cần thiết vì gói lượt loại
        /// WAIT không mang trường hp lên wire (<c>PetBattle.cs:336-343</c>) — uống bình máu
        /// hồi HP mà thanh máu trong trận sẽ không nhúc nhích nếu chỉ nghe opcode 37.
        ///
        /// <para>LUÔN hoãn ít nhất một frame thay vì áp ngay: snapshot này mang giá trị TUYỆT
        /// ĐỐI đã tính cả sát thương của lượt, nhưng vài đường server gửi nó TRƯỚC gói lượt
        /// (<c>addRecovery</c> <c>PetBattle.cs:1184</c>, <c>mobUseSkill</c> <c>:1438</c>).
        /// Áp ngay rồi gói 37 áp delta lần nữa là trừ máu hai lần.</para></summary>
        public void SyncLocalVitals(int hp, int maxHp, int mp, int maxMp)
        {
            if (_left == null) return;
            _pendingVitals = new[] { hp, maxHp, mp, maxMp };
        }

        /// <summary>Đồng bộ trạng thái bật/tắt của mọi nút theo lượt hiện tại.</summary>
        private void RefreshLocks()
        {
            // `_result != null` phải có: server gửi gói kết quả TRƯỚC sendMyPetInfo()
            // (PetBattle.cs:959-967), bỏ qua nó thì snapshot đến sau bật lại nút của trận đã đóng.
            var canAct = _result == null && _turn.CanAct && (_animator == null || _animator.Idle);
            _actionBar?.SetActionsInteractable(canAct);
            _skillPopup?.RefreshState(_left.Mp, !canAct);
            // Nút tròn luôn bấm được để xem kỹ năng; từng dòng mới khoá theo lượt.
            if (_skillButton != null) _skillButton.interactable = _result == null;
        }

        private void Build(RemoteAssetCache assets, Font font, PlayerStats playerStats)
        {
            // Khởi tạo TRƯỚC mọi thao tác dựng UI: nếu một Create() phía dưới ném thì
            // Update() vẫn chạy mỗi frame và sẽ NRE vĩnh viễn ở _turn, kéo theo Ticked
            // chết nên BattleCoordinator.CheckStalled không bao giờ đóng được overlay.
            _turn = new BattleTurnState(_start.LocalPet.ActorId, _start.LocalStarts);

            var bg = new GameObject("Nền", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(transform, false);
            UiBuilder.Stretch((RectTransform)bg.transform);
            var bgImg = bg.GetComponent<Image>();
            bgImg.sprite = BattleSkin.Load("Battle/bg-forest");
            bgImg.color = bgImg.sprite == null ? new Color(0f, 0.08f, 0.13f, 0.92f) : Color.white;
            bgImg.type = Image.Type.Simple;
            bgImg.raycastTarget = false;

            _topBar = BattleTopBar.Create(transform, font, _start.Kind, playerStats);
            _topBar.BackClicked += OnBackClicked;
            _hudLeft = BattleHudPanel.Create(transform, _start.LocalPet, true, font, assets);
            _hudRight = BattleHudPanel.Create(transform, _start.Opponent, false, font, assets);
            _left = BattlePetCard.Create(transform, _start.LocalPet, true, assets);
            _right = BattlePetCard.Create(transform, _start.Opponent, false, assets);

            _vsIndicator = BattleVsIndicator.Create(transform, font);
            _vsIndicator.SetTurn(_turn.IsLocalTurn);

            // Chỉ pet mình có bảng kỹ năng. Quái tự chọn kỹ năng theo useRate phía server
            // (PetBattle.mobAttack) nên không hiển thị danh sách của nó.
            var skillBtnRect = BattleSkillButton.Create(transform, ToggleSkillPopup, out _skillButton);
            _skillPopup = BattleSkillPopup.Create(transform, _start.LocalPet.Skills, font,
                _cooldowns, skillBtnRect);
            _skillPopup.SkillUsed += OnSkillUsed;

            _actionBar = BattleActionBar.Create(transform, font, _start.IsParticipant);
            _actionBar.AttackClicked += OnAttack;
            _actionBar.PotionClicked += OnPotion;
            _actionBar.SurrenderClicked += OnSurrenderClicked;
            _animator = BattleTurnAnimator.Attach(gameObject, Card, RefreshHuds, PlayHitSound,
                OnAnimatorDrained);
            RefreshLocks();
        }

        private void Update()
        {
            // Nếu heuristic "gói vật phẩm" trong BattleTurnState đoán sai, nút sẽ khoá
            // vĩnh viễn. Watchdog này là chốt chặn để trận không bao giờ bấm-không-được.
            if (_turn.ActionPending && Time.unscaledTime - _pendingSince > PendingWatchdogSeconds)
            {
                _turn.ClearPending();
                RefreshLocks();
            }
            // Áp snapshot HP/MP ở frame SAU khi nhận, và chỉ khi hàng đợi đã cạn — xem
            // SyncLocalVitals để biết vì sao không áp ngay.
            if (_pendingVitals != null && (_animator == null || _animator.Idle)) ApplyPendingVitals();
            if (_result != null && Time.unscaledTime - _resultShownAt >= ResultAutoCloseSeconds)
            {
                RequestClose();
            }
            Ticked?.Invoke();
        }
    }
}
