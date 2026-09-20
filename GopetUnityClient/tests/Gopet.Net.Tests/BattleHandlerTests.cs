using Gopet.Net.Battle;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed class BattleHandlerTests
    {
        [Fact]
        public void MobBattle_DocDungHaiPetVaSkill()
        {
            var (handler, router) = NewHandler(77);
            BattleStart received = null;
            handler.BattleStarted += value => received = value;
            using var packet = Message.Create(GopetCmd.ATTACK_MOB)
                .PutInt(2500).PutInt(15000).PutInt(77);
            WriteOwned(packet, 11, "pets/me.png", "Mèo", 420, 90, 500, 100);
            packet.PutInt(9001);
            WritePassive(packet, 22, "pets/mob.png", "Sói", 350, 1, 400, 1, false);
            Dispatch(router, packet);

            Assert.NotNull(received);
            Assert.Equal(BattleKind.Mob, received.Kind);
            Assert.Equal(77, received.BattleId);
            Assert.True(received.IsParticipant);
            Assert.Equal(9001, received.Opponent.ActorId);
            Assert.Equal("Cào Lv.2", received.LocalPet.Skills[0].Name);
            Assert.Equal(12, received.LocalPet.Skills[0].MpCost);
        }

        /// <summary>Quái lấy máu thật từ bảng <c>gopet_mob</c> nhưng trần lại tính bằng công
        /// thức (<c>Mob.initMob</c>), nên wire gửi được hp &gt; maxHp (lv45: 384310 / 192155).
        /// Trần phải nới theo hp, nếu không client kẹp mất nửa pool và báo quái chết sớm gấp
        /// đôi — trận vẫn chạy mà không bao giờ có băng chiến thắng.</summary>
        [Fact]
        public void MobBattle_HpLonHonMaxHp_ThiNoiTranTheoHp()
        {
            var (handler, router) = NewHandler(77);
            BattleStart received = null;
            handler.BattleStarted += value => received = value;
            using var packet = Message.Create(GopetCmd.ATTACK_MOB)
                .PutInt(2500).PutInt(15000).PutInt(77);
            WriteOwned(packet, 11, "pets/me.png", "Mèo", 420, 90, 500, 100);
            packet.PutInt(9001);
            WritePassive(packet, 22, "pets/mob.png", "Hắc nham long", 384310, 240155, 192155, 240155, false);
            Dispatch(router, packet);

            Assert.Equal(384310, received.Opponent.Hp);
            Assert.Equal(384310, received.Opponent.MaxHp);
            Assert.Equal(240155, received.Opponent.MaxMp);
        }

        [Fact]
        public void PlayerBattle_ChiuDuocCoLegacyCoHoacKhong()
        {
            foreach (var withFlag in new[] { false, true })
            {
                var (handler, router) = NewHandler(8);
                BattleStart received = null;
                handler.BattleStarted += value => received = value;
                using var packet = Message.Create(GopetCmd.PLAYER_BATTLE)
                    .PutInt(1000).PutInt(15000).PutInt(8).PutSByte(1);
                WriteOwned(packet, 10, "a", "Ta", 10, 5, 10, 5);
                packet.PutInt(9);
                WritePassive(packet, 20, "b", "Bạn", 8, 3, 9, 4, true);
                if (withFlag) packet.PutBool(false);
                Dispatch(router, packet);
                Assert.True(received.LocalStarts);
                Assert.Equal(101, received.Opponent.Skills[0].Id);
            }
        }

        [Fact]
        public void Turn_DocMainMpVaDayDuEffectLegacy()
        {
            var (handler, router) = NewHandler(7);
            BattleTurn received = null;
            handler.TurnReceived += value => received = value;
            using var packet = Message.Create(GopetCmd.PET_BATTLE)
                .PutInt(7).PutInt(7).PutInt(12000).PutInt(15000).PutSByte(BattleTurn.Wait)
                .PutInt(0).PutUtf("").PutInt(-12).PutInt(1)
                .PutInt(9001).PutInt(105).PutUtf("")
                .PutInt(0).PutInt(0).PutInt(0).PutInt(-83).PutInt(-2).PutInt(0).PutInt(0);
            Dispatch(router, packet);

            Assert.Equal(-12, received.MainMpDelta);
            Assert.Single(received.Effects);
            Assert.Equal(105, received.Effects[0].SkillId);
            Assert.Equal(-83, received.Effects[0].HpDelta);
            Assert.Equal(-2, received.Effects[0].MpDelta);
        }

        [Fact]
        public void ResultVaLevel_DocDungFormatServer()
        {
            var (handler, router) = NewHandler(7);
            BattleResult result = null;
            var level = 0;
            handler.BattleEnded += value => result = value;
            handler.PetLevelUpdated += value => level = value;
            using var end = Message.Create(GopetCmd.PET_BATTLE_STATE)
                .PutInt(7).PutInt(7).PutSByte(0).PutInt(15).PutInt(44).PutSByte(1)
                .PutUtf("Nhận được vật phẩm").PutUtf("2");
            Dispatch(router, end);
            using var up = Message.Create(GopetCmd.UPDATE_PET_LVL).PutInt(0).PutInt(0).PutInt(12);
            Dispatch(router, up);

            Assert.Equal(15, result.Coin);
            Assert.Equal(44, result.Experience);
            Assert.Equal("Nhận được vật phẩm", result.Messages[0]);
            Assert.Equal(12, level);
        }

        [Fact]
        public void LenhDanhSkillItemVaMob_KhopJar()
        {
            Message sent = null;
            var handler = new BattleHandler(value => sent = value, 1);
            handler.SendAttackMob(55);
            AssertBody(sent, GopetCmd.ATTACK_MOB, 55);
            handler.SendNormalAttack();
            var attack = Round(sent); Assert.Equal(GopetCmd.PET_BATTLE, attack.ReadSByte());
            Assert.Equal(GopetCmd.PetBattle_ATTACK, attack.ReadSByte()); Assert.Equal(0, attack.Remaining);
            handler.SendSkill(108);
            var skill = Round(sent); Assert.Equal(GopetCmd.PET_BATTLE, skill.ReadSByte());
            Assert.Equal(GopetCmd.PET_BATTLE_USE_SKILL, skill.ReadSByte()); Assert.Equal(108, skill.ReadInt());
            handler.SendUseItem();
            var item = Round(sent); Assert.Equal(GopetCmd.PET_BATTLE, item.ReadSByte());
            Assert.Equal(GopetCmd.PET_BATTLE_USE_ITEM, item.ReadSByte()); Assert.Equal(0, item.ReadInt());
        }

        [Fact]
        public void FastRemove_UsesOwnerUserIdAsBattleId()
        {
            var (handler, router) = NewHandler(7);
            var removedId = -1;
            handler.BattleRemoved += value => removedId = value;
            using var packet = Message.Create(GopetCmd.FAST_REMOVE_MOB).PutInt(77);

            Dispatch(router, packet);

            Assert.Equal(77, removedId);
        }

        [Fact]
        public void BuffState_ParseHaiActorMoiActorHaiBuff()
        {
            var (handler, router) = NewHandler(50);
            BattleBuffState state = null;
            handler.BuffStateReceived += s => state = s;
            using var packet = Message.Create(GopetCmd.PET_BATTLE_BUFF)
                .PutInt(50).PutSByte(2)
                .PutInt(50).PutSByte(2)
                    .PutInt(30).PutInt(1).PutSByte(2)   // STUN, val 1, 2 lượt còn lại
                    .PutInt(24).PutInt(100).PutSByte(3) // RECOVERY_HP
                .PutInt(99).PutSByte(1)
                    .PutInt(28).PutInt(50).PutSByte(5); // DAMGE_TOXIC_IN_5_TURN

            Dispatch(router, packet);

            Assert.NotNull(state);
            Assert.Equal(50, state.BattleId);
            Assert.Equal(2, state.Actors.Length);
            Assert.Equal(50, state.Actors[0].ActorId);
            Assert.Equal(2, state.Actors[0].Entries.Length);
            Assert.Equal(30, state.Actors[0].Entries[0].TypeId);
            Assert.Equal(2, state.Actors[0].Entries[0].TurnsLeft);
            Assert.Equal(24, state.Actors[0].Entries[1].TypeId);
            Assert.Equal(100, state.Actors[0].Entries[1].Value);
            Assert.Equal(99, state.Actors[1].ActorId);
            Assert.Single(state.Actors[1].Entries);
        }

        [Fact]
        public void BuffState_TooManyActorsThrowsProtocolException()
        {
            var (handler, router) = NewHandler(1);
            using var packet = Message.Create(GopetCmd.PET_BATTLE_BUFF)
                .PutInt(1).PutSByte(9); // > MaxBuffActors=2
            Assert.Throws<ProtocolException>(() => Dispatch(router, packet));
        }

        [Fact]
        public void Stats_ParseFullPacket()
        {
            var (handler, router) = NewHandler(1);
            BattleStatsState received = null;
            handler.StatsReceived += s => received = s;
            using var packet = Message.Create(GopetCmd.PET_BATTLE_STATS)
                .PutInt(42)     // battleId
                .PutSByte(2)    // 2 actors
                .PutInt(10)     // actor 1 id
                .PutInt(5)      // level
                .PutInt(120)    // atk
                .PutInt(80)     // def
                .PutShort(230)  // critPermille
                .PutSByte(1)    // 1 skill
                .PutInt(301)    // skillId
                .PutInt(15)     // mpCost
                .PutInt(20)     // actor 2 id
                .PutInt(3)      // level
                .PutInt(65)     // atk
                .PutInt(45)     // def
                .PutShort(0)    // critPermille
                .PutSByte(0);   // 0 skills
            Dispatch(router, packet);
            Assert.NotNull(received);
            Assert.Equal(42, received.BattleId);
            Assert.Equal(2, received.Actors.Length);
            Assert.Equal(120, received.Actors[0].Atk);
            Assert.Equal(80, received.Actors[0].Def);
            Assert.Equal(230, received.Actors[0].CritPermille);
            Assert.Single(received.Actors[0].Skills);
            Assert.Equal(301, received.Actors[0].Skills[0].SkillId);
            Assert.Equal(15, received.Actors[0].Skills[0].MpCost);
            Assert.Empty(received.Actors[1].Skills);
        }

        [Fact]
        public void Stats_ZeroActorsIsValid()
        {
            var (handler, router) = NewHandler(1);
            BattleStatsState received = null;
            handler.StatsReceived += s => received = s;
            using var packet = Message.Create(GopetCmd.PET_BATTLE_STATS)
                .PutInt(1).PutSByte(0);
            Dispatch(router, packet);
            Assert.NotNull(received);
            Assert.Empty(received.Actors);
        }

        [Fact]
        public void Stats_TooManyActorsThrows()
        {
            var (handler, router) = NewHandler(1);
            using var packet = Message.Create(GopetCmd.PET_BATTLE_STATS)
                .PutInt(1).PutSByte(5);
            Assert.Throws<ProtocolException>(() => Dispatch(router, packet));
        }

        [Fact]
        public void SendSurrender_CorrectBytes()
        {
            Message sent = null;
            var handler = new BattleHandler(m => sent = m, 1);
            handler.SendSurrender();
            Assert.NotNull(sent);
            var r = Round(sent);
            Assert.Equal(GopetCmd.PET_SERVICE, sent.Id);
            Assert.Equal(GopetCmd.PET_BATTLE, r.ReadSByte());
            Assert.Equal(GopetCmd.PET_BATTLE_SURRENDER, r.ReadSByte());
        }

        [Fact]
        public void MobBattle_DocDuPetHoc30KyNang()
        {
            var (handler, router) = NewHandler(77);
            BattleStart received = null;
            handler.BattleStarted += value => received = value;
            using var packet = Message.Create(GopetCmd.ATTACK_MOB)
                .PutInt(2500).PutInt(15000).PutInt(77);
            WriteOwnedManySkills(packet, 30);
            packet.PutInt(9001);
            WritePassive(packet, 22, "pets/mob.png", "Sói", 350, 1, 400, 1, false);
            Dispatch(router, packet);

            Assert.Equal(30, received.LocalPet.Skills.Length);
            Assert.Equal(130, received.LocalPet.Skills[29].Id);
        }

        [Fact]
        public void MobBattle_QuaTranKyNangThiNem()
        {
            var (_, router) = NewHandler(77);
            using var packet = Message.Create(GopetCmd.ATTACK_MOB)
                .PutInt(2500).PutInt(15000).PutInt(77);
            WriteOwnedManySkills(packet, 65);
            // Gói vẫn ĐỦ phần quái: nếu cắt ngắn ở đây thì reader hết dữ liệu cũng ném
            // ProtocolException, test sẽ xanh cả khi trần bị nới hoặc gỡ hẳn.
            packet.PutInt(9001);
            WritePassive(packet, 22, "pets/mob.png", "Sói", 350, 1, 400, 1, false);

            var ex = Assert.Throws<ProtocolException>(() => Dispatch(router, packet));
            Assert.Contains("kỹ năng", ex.Message);
        }

        /// <summary>Pet của ta với <paramref name="count"/> kỹ năng, id đánh số từ 101.</summary>
        private static void WriteOwnedManySkills(Message m, int count)
        {
            m.PutInt(11).PutUtf("pets/me.png").PutSByte(4).PutShort(-2).PutUtf("Mèo").PutInt(3);
            for (var i = 0; i < 5; i++) m.PutInt(i + 1);
            m.PutInt(420).PutInt(9000).PutInt(500).PutInt(9000).PutSByte((sbyte)count);
            for (var i = 0; i < count; i++)
                m.PutInt(101 + i).PutUtf($"Kỹ năng {i}").PutUtf("Mô tả").PutInt(10 + i);
        }

        private static void WriteOwned(Message m, int template, string image, string name,
            int hp, int mp, int maxHp, int maxMp)
        {
            m.PutInt(template).PutUtf(image).PutSByte(4).PutShort(-2).PutUtf(name).PutInt(3);
            for (var i = 0; i < 5; i++) m.PutInt(i + 1);
            m.PutInt(hp).PutInt(mp).PutInt(maxHp).PutInt(maxMp).PutSByte(1)
                .PutInt(101).PutUtf("Cào Lv.2").PutUtf("Gây sát thương").PutInt(12);
        }

        private static void WritePassive(Message m, int template, string image, string name,
            int hp, int mp, int maxHp, int maxMp, bool skill)
        {
            m.PutInt(template).PutUtf(image).PutSByte(4).PutShort(0).PutUtf(name).PutInt(2)
                .PutInt(hp).PutInt(mp).PutInt(maxHp).PutInt(maxMp).PutSByte(skill ? 1 : 0);
            if (skill) m.PutInt(101).PutUtf("Đòn đối thủ");
        }

        private static (BattleHandler, MessageRouter) NewHandler(int userId)
        {
            var router = new MessageRouter();
            var handler = new BattleHandler(null, userId); handler.RegisterOn(router);
            return (handler, router);
        }

        private static void Dispatch(MessageRouter router, Message packet)
        {
            var body = packet.ToWire();
            var wire = new byte[body.Length + 1]; wire[0] = unchecked((byte)GopetCmd.PET_SERVICE);
            System.Array.Copy(body, 0, wire, 1, body.Length);
            router.Dispatch(Message.FromWire(wire, false));
        }

        private static JavaBinaryReader Round(Message message) => Message.FromWire(message.ToWire(), false).Reader;
        private static void AssertBody(Message sent, sbyte sub, int value)
        {
            var r = Round(sent); Assert.Equal(GopetCmd.PET_SERVICE, sent.Id);
            Assert.Equal(sub, r.ReadSByte()); Assert.Equal(value, r.ReadInt());
        }
    }
}
