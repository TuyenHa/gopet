using System;
using System.Collections.Generic;

namespace Gopet.Net.Battle
{
    /// <summary>Giao thức PET_SERVICE của trận đấu; damage luôn do server quyết định.</summary>
    public sealed class BattleHandler
    {
        private const int MaxEffects = 128;
        private readonly Action<Message> _send;
        private readonly int _localUserId;

        /// <summary>battleId (userId người đánh) → mobId của trận PvE đang diễn trong zone.
        /// Server không có gói "quái chết" riêng: thắng quái thì chỉ phát FAST_REMOVE_MOB mang
        /// battleId (PetBattle.sendFastRemove), rồi 3s sau sinh quái MỚI với id mới ở cùng chỗ.
        /// Không tự gỡ theo bảng này thì xác quái cũ ở lại map, bấm vào gửi ATTACK_MOB với id
        /// server đã xoá ⇒ GopetPlace.startFightMob nuốt im lặng, không vào được trận.</summary>
        private readonly Dictionary<int, int> _mobByBattle = new Dictionary<int, int>();

        public event Action<BattleStart> BattleStarted;
        public event Action<BattleTurn> TurnReceived;
        public event Action<BattleResult> BattleEnded;
        public event Action<int> BattleRemoved;
        /// <summary>mobId của con quái vừa bị giết — xem <see cref="_mobByBattle"/>.</summary>
        public event Action<int> MobKilled;
        public event Action<int> PetLevelUpdated;
        public event Action<BattleBuffState> BuffStateReceived;
        public event Action<BattleStatsState> StatsReceived;
        public event Action<BattleExpGain> HitExpReceived;

        public BattleHandler(Action<Message> send = null, int localUserId = -1)
        {
            _send = send;
            _localUserId = localUserId;
        }

        public void RegisterOn(MessageRouter router)
        {
            if (router == null) throw new ArgumentNullException(nameof(router));
            router.RegisterEnvelope(GopetCmd.PET_SERVICE);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.ATTACK_MOB, OnMobBattle);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PLAYER_BATTLE, OnPlayerBattle);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_BATTLE, OnTurn);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_BATTLE_STATE, OnResult);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.FAST_REMOVE_MOB, OnFastRemove);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.UPDATE_PET_LVL, OnPetLevel);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_BATTLE_BUFF, OnBuffState);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_BATTLE_STATS, OnStats);
            router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_BATTLE_EXP, OnHitExp);
        }

        /// <summary>EXP nhỏ giọt mỗi đòn trúng trong PvE — số vàng bay trên đầu pet.
        /// Server chỉ gửi cho client &gt;= 1.5.0 (<c>HitExpReward.Send</c>).</summary>
        private void OnHitExp(Message msg)
        {
            var r = msg.Reader;
            var gain = new BattleExpGain
            {
                BattleId = r.ReadInt(), ActorId = r.ReadInt(), Amount = r.ReadInt()
            };
            r.ExpectFullyConsumed("PET_BATTLE_EXP");
            HitExpReceived?.Invoke(gain);
        }

        private void OnBuffState(Message msg)
        {
            var state = BattleAuxPacketReader.ReadBuffState(msg.Reader);
            BuffStateReceived?.Invoke(state);
        }

        private void OnStats(Message msg)
        {
            var state = BattleAuxPacketReader.ReadStatsState(msg.Reader);
            StatsReceived?.Invoke(state);
        }

        public void SendAttackMob(int mobId) => Send(GopetCmd.ATTACK_MOB, m => m.PutInt(mobId));
        public void SendNormalAttack() => Send(GopetCmd.PET_BATTLE, m => m.PutSByte(GopetCmd.PetBattle_ATTACK));
        public void SendSkill(int skillId) => Send(GopetCmd.PET_BATTLE,
            m => m.PutSByte(GopetCmd.PET_BATTLE_USE_SKILL).PutInt(skillId));
        public void SendUseItem() => Send(GopetCmd.PET_BATTLE,
            m => m.PutSByte(GopetCmd.PET_BATTLE_USE_ITEM).PutInt(0));
        public void SendSurrender() => Send(GopetCmd.PET_BATTLE,
            m => m.PutSByte(GopetCmd.PET_BATTLE_SURRENDER));
        public void SetAutoRecovery(bool enabled) => Send(GopetCmd.PET_RECOVERY_HP,
            m => m.PutSByte(enabled ? 1 : 0));

        private void Send(sbyte sub, Func<Message, Message> body)
        {
            if (_send == null) throw new InvalidOperationException("BattleHandler chưa có đường gửi gói tin.");
            _send(body(Message.Create(GopetCmd.PET_SERVICE).PutSByte(sub)));
        }

        private void OnMobBattle(Message message)
        {
            var r = message.Reader;
            var start = Header(r, BattleKind.Mob);
            var ownerId = r.ReadInt();
            start.BattleId = ownerId;
            start.LocalPet = BattleAuxPacketReader.ReadOwnedPet(r, ownerId);
            var mobId = r.ReadInt();
            start.Opponent = BattleAuxPacketReader.ReadPassivePet(r, mobId);
            start.IsParticipant = ownerId == _localUserId;
            // Wire PvE không có cờ "ai đi trước" (khác PLAYER_BATTLE), nên suy ra từ server:
            // constructor PvE đặt setIsActiveTurn(false) (PetBattle.cs:66) ⇒ getUserTurnId()
            // trả mob.getMobId(), và MobAttackTime khởi tạo = DateTime.Now (:33) đã quá hạn
            // ngay ⇒ update() gọi mobAttack() ở tick đầu. QUÁI ĐI TRƯỚC, không phải người chơi.
            // Gói lượt đầu tiên (của quái) sẽ lật IsLocalTurn sang true.
            start.LocalStarts = false;
            r.ExpectFullyConsumed("ATTACK_MOB");
            _mobByBattle[ownerId] = mobId;
            BattleStarted?.Invoke(start);
        }

        private void OnPlayerBattle(Message message)
        {
            var r = message.Reader;
            var start = Header(r, BattleKind.Player);
            var ownerId = r.ReadInt();
            start.BattleId = ownerId;
            start.LocalStarts = r.ReadSByte() == 1;
            start.LocalPet = BattleAuxPacketReader.ReadOwnedPet(r, ownerId);
            var opponentId = r.ReadInt();
            start.Opponent = BattleAuxPacketReader.ReadPassivePet(r, opponentId);
            if (r.Remaining == 1) r.ReadBool();
            start.IsParticipant = ownerId == _localUserId || opponentId == _localUserId;
            r.ExpectFullyConsumed("PLAYER_BATTLE");
            // FAST_REMOVE của trận PvP dùng cùng battleId = userId: xoá dấu trận PvE cũ (đã
            // thua quái nên không có FAST_REMOVE) để khỏi gỡ nhầm con quái còn sống.
            _mobByBattle.Remove(ownerId);
            _mobByBattle.Remove(opponentId);
            BattleStarted?.Invoke(start);
        }

        private static BattleStart Header(JavaBinaryReader r, BattleKind kind) => new BattleStart
        {
            Kind = kind, RemainingMs = r.ReadInt(), TurnDurationMs = r.ReadInt()
        };

        private void OnTurn(Message message)
        {
            var r = message.Reader;
            var turn = new BattleTurn
            {
                BattleId = r.ReadInt(), ActorId = r.ReadInt(), RemainingMs = r.ReadInt(),
                TurnDurationMs = r.ReadInt(), Type = r.ReadSByte()
            };
            if (turn.Type == BattleTurn.Wait)
            {
                r.ReadInt(); r.ReadUtf(); turn.MainMpDelta = r.ReadInt();
            }
            var count = Count(r.ReadInt(), MaxEffects, "hiệu ứng lượt");
            turn.Effects = new BattleEffect[count];
            for (var i = 0; i < count; i++) turn.Effects[i] = ReadEffect(r);
            r.ExpectFullyConsumed("PET_BATTLE");
            TurnReceived?.Invoke(turn);
        }

        private static BattleEffect ReadEffect(JavaBinaryReader r)
        {
            var effect = new BattleEffect { ActorId = r.ReadInt(), SkillId = r.ReadInt() };
            r.ReadUtf();
            for (var i = 0; i < 3; i++) r.ReadInt();
            effect.HpDelta = r.ReadInt(); effect.MpDelta = r.ReadInt();
            r.ReadInt(); r.ReadInt();
            return effect;
        }

        private void OnResult(Message message)
        {
            var r = message.Reader;
            var result = new BattleResult { BattleId = r.ReadInt(), WinnerId = r.ReadInt() };
            r.ReadSByte();
            result.Coin = r.ReadInt(); result.Experience = r.ReadInt();
            var count = Count(r.ReadSByte(), 64, "thông báo kết quả");
            result.Messages = new string[count];
            for (var i = 0; i < count; i++) { result.Messages[i] = r.ReadUtf(); r.ReadUtf(); }
            r.ExpectFullyConsumed("PET_BATTLE_STATE");
            // Quái thắng (hoặc bỏ trận) thì nó còn sống, không có FAST_REMOVE theo sau.
            if (result.WinnerId != result.BattleId) _mobByBattle.Remove(result.BattleId);
            BattleEnded?.Invoke(result);
        }

        private void OnPetLevel(Message message)
        {
            var r = message.Reader;
            r.ReadInt(); r.ReadInt();
            var level = r.ReadInt();
            r.ExpectFullyConsumed("UPDATE_PET_LVL");
            PetLevelUpdated?.Invoke(level);
        }

        private void OnFastRemove(Message message)
        {
            var battleId = message.Reader.ReadInt();
            message.Reader.ExpectFullyConsumed("FAST_REMOVE_MOB");
            BattleRemoved?.Invoke(battleId);
            // Server chỉ phát gói này khi quái hết máu (PetBattle.win / GopetPlace.update).
            if (_mobByBattle.TryGetValue(battleId, out var mobId))
            {
                _mobByBattle.Remove(battleId);
                MobKilled?.Invoke(mobId);
            }
        }

        private static int Count(int value, int max, string label)
        {
            if (value < 0 || value > max) throw new ProtocolException($"Số {label} không hợp lệ: {value}.");
            return value;
        }
    }
}
