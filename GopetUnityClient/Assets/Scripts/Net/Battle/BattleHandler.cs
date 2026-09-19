using System;

namespace Gopet.Net.Battle
{
    /// <summary>Giao thức PET_SERVICE của trận đấu; damage luôn do server quyết định.</summary>
    public sealed class BattleHandler
    {
        private const int MaxEffects = 128;
        private readonly Action<Message> _send;
        private readonly int _localUserId;

        public event Action<BattleStart> BattleStarted;
        public event Action<BattleTurn> TurnReceived;
        public event Action<BattleResult> BattleEnded;
        public event Action<int> BattleRemoved;
        public event Action<int> PetLevelUpdated;
        public event Action<BattleBuffState> BuffStateReceived;
        public event Action<BattleStatsState> StatsReceived;

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
            r.ExpectFullyConsumed("ATTACK_MOB");
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
        }

        private static int Count(int value, int max, string label)
        {
            if (value < 0 || value > max) throw new ProtocolException($"Số {label} không hợp lệ: {value}.");
            return value;
        }
    }
}
