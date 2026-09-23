using Gopet.Net.Battle;
using Xunit;

namespace Gopet.Net.Tests
{
    public sealed partial class BattleHandlerTests
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
    }
}
