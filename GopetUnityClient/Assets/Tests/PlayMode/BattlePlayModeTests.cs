using System.Collections;
using System.IO;
using System.Linq;
using Gopet.Net;
using Gopet.Net.Battle;
using Gopet.Runtime;
using Gopet.Runtime.Assets;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class BattlePlayModeTests
    {
        [UnityTest]
        public IEnumerator BattleView_CoBaNut_GuiDanh_VaHienKetQua()
        {
            var root = new GameObject("Battle test root");
            var client = root.AddComponent<GopetClient>();
            var cache = new RemoteAssetCache(client, client.Router,
                Path.Combine(Application.temporaryCachePath, "gopet-battle-test"));
            Message sent = null;
            var handler = new BattleHandler(value => sent = value, 7);
            var start = StartData();
            var view = BattleView.Create(root.transform, start, handler, cache);
            yield return null;

            var buttons = view.GetComponentsInChildren<Button>();
            Assert.AreEqual(3, buttons.Length, "JAR có đúng Đánh/Kỹ năng/Vật phẩm ở thanh chính.");
            buttons[0].onClick.Invoke();
            Assert.IsNotNull(sent);
            var round = Message.FromWire(sent.ToWire(), false);
            Assert.AreEqual(GopetCmd.PET_SERVICE, round.Id);
            Assert.AreEqual(GopetCmd.PET_BATTLE, round.Reader.ReadSByte());
            Assert.AreEqual(GopetCmd.PetBattle_ATTACK, round.Reader.ReadSByte());

            view.Apply(new BattleTurn
            {
                BattleId = 7, ActorId = 7, RemainingMs = 1000, TurnDurationMs = 15000,
                Type = BattleTurn.Normal,
                Effects = new[] { new BattleEffect { ActorId = 99, SkillId = 0, HpDelta = -25 } }
            });
            view.ShowResult(new BattleResult
            {
                BattleId = 7, WinnerId = 7, Coin = 5, Experience = 10,
                Messages = new[] { "Nhận thưởng" }
            });
            view.ShowResult(new BattleResult { BattleId = 7, WinnerId = 99 });
            yield return null;
            Assert.IsNotNull(view.transform.Find("Kết quả"));
            Assert.AreEqual(1, view.transform.Cast<Transform>().Count(child => child.name == "Kết quả"),
                "Gói kết quả lặp không được dựng hai panel.");
            Assert.AreEqual(1, view.GetComponentsInChildren<Button>().Length);
            cache.Dispose();
            Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator EffectDyVaAnu_DungDuocAssetGoc()
        {
            var root = new GameObject("Effect root", typeof(RectTransform));
            var target = new GameObject("Target", typeof(RectTransform)).GetComponent<RectTransform>();
            target.SetParent(root.transform, false);
            BattleEffectView.Play(root.transform, target, 101);
            BattleEffectView.Play(root.transform, target, 125);
            yield return null;
            Assert.IsNotNull(root.transform.Find("Hiệu ứng voanh"));
            Assert.IsNotNull(root.transform.Find("Hiệu ứng ANU 125"));
            Object.Destroy(root);
        }

        private static BattleStart StartData() => new BattleStart
        {
            BattleId = 7, Kind = BattleKind.Mob, RemainingMs = 5000,
            TurnDurationMs = 15000, IsParticipant = true,
            LocalPet = Pet(7, "Mèo", 100, new[]
            {
                new BattleSkill { Id = 101, Name = "Cào Lv.1", MpCost = 5 }
            }),
            Opponent = Pet(99, "Sói", 80, System.Array.Empty<BattleSkill>())
        };

        private static BattlePet Pet(int id, string name, int hp, BattleSkill[] skills) => new BattlePet
        {
            ActorId = id, Name = name, ImagePath = string.Empty, FrameCount = 1,
            Level = 1, Hp = hp, MaxHp = hp, Mp = 20, MaxMp = 20, Skills = skills
        };
    }
}
