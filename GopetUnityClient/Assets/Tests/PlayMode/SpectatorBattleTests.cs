using System.Collections;
using System.Linq;
using System.Reflection;
using Gopet.Net.Battle;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class SpectatorBattleTests
    {
        private GameObject _root;
        private Transform _left, _right;
        private SpectatorBattleLayer _layer;
        [SetUp] public void Setup()
        {
            _root = new GameObject("Spectator fixture");
            _left = new GameObject("Left actor").transform; _left.SetParent(_root.transform);
            _right = new GameObject("Right actor").transform; _right.SetParent(_root.transform);
            _right.position = new Vector3(100, 0, 0);
            _layer = SpectatorBattleLayer.Create(_root.transform, id => id < 100 ? _left : _right);
        }
        [TearDown] public void Teardown() { Object.DestroyImmediate(_root); }
        private static BattleStart Start(int id = 1) => new BattleStart
        {
            BattleId = id, Kind = BattleKind.Player, TurnDurationMs = 1000,
            LocalPet = new BattlePet { ActorId = id, Hp = 100, MaxHp = 100, Mp = 20, MaxMp = 20 },
            Opponent = new BattlePet { ActorId = id + 100, Hp = 100, MaxHp = 100, Mp = 20, MaxMp = 20 }
        };
        [UnityTest] public IEnumerator ObserverRendersDamageWithoutActionsAndClearsOnMapChange()
        {
            var handler = new BattleHandler(_ => Assert.Fail("Observer must not send actions"), 999);
            var coordinator = new BattleCoordinator(_root.transform, null, handler,
                _ => Assert.Fail("Observer must not lock movement"), spectators: _layer);
            typeof(BattleCoordinator).GetMethod("OnStarted", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(coordinator, new object[] { Start(), handler });
            yield return null;
            Assert.AreEqual(1, _root.GetComponentsInChildren<SpectatorBattleView>().Length);
            Assert.IsNull(coordinator.View);
            Assert.IsEmpty(_root.GetComponentsInChildren<Button>());
            _layer.Apply(new BattleTurn { BattleId = 1, ActorId = 1,
                Effects = new[] { new BattleEffect { ActorId = 101, HpDelta = -25, SkillId = 101 } } });
            Assert.IsTrue(_root.GetComponentsInChildren<Text>().Any(t => t.text == "75/100 HP"));
            Assert.IsNotEmpty(_root.GetComponentsInChildren<BattleFloatText>());
            coordinator.OnPlaceChanged(); yield return null;
            Assert.AreEqual(0, _layer.Count);
            Assert.IsEmpty(_root.GetComponentsInChildren<SpectatorBattleView>());
            Assert.IsEmpty(_root.GetComponentsInChildren<BattleFloatText>());
        }
        [UnityTest] public IEnumerator FivePairsAndRepeatedSnapshotsDoNotLeaveObjects()
        {
            for (var i = 1; i <= 6; i++) _layer.StartBattle(Start(i));
            _layer.StartBattle(Start()); yield return null;
            Assert.AreEqual(5, _root.GetComponentsInChildren<SpectatorBattleView>().Length);
            _layer.Remove(101); yield return null;
            Assert.AreEqual(4, _root.GetComponentsInChildren<SpectatorBattleView>().Length);
            _layer.Clear(); yield return null;
            Assert.IsEmpty(_root.GetComponentsInChildren<Canvas>());
        }
        [UnityTest] public IEnumerator NormalAndCriticalAttacksUseCompactEffects()
        {
            _layer.StartBattle(Start());
            foreach (var skill in new[] { 0, 2 })
                _layer.Apply(new BattleTurn { BattleId = 1, ActorId = 1,
                    Effects = new[] { new BattleEffect { ActorId = 101, SkillId = skill, HpDelta = -1 } } });
            yield return null;
            Assert.AreEqual(2, _root.GetComponentsInChildren<BattleSlashFx>().Length);
            var effects = (RectTransform)_root.GetComponentInChildren<SpectatorBattleView>().transform.Find("Effects");
            Assert.Greater(effects.rect.height, 1, "Must not trigger overlay height fallback");
            Assert.LessOrEqual(effects.rect.height * effects.lossyScale.y, 80.1f);
        }
        [UnityTest] public IEnumerator DelayedActorsResolveButExpiredActorsAreDropped()
        {
            var saved = _left; _left = null;
            _layer.StartBattle(Start()); yield return null;
            Assert.IsEmpty(_root.GetComponentsInChildren<SpectatorBattleView>());
            _left = saved; yield return null;
            Assert.IsNotEmpty(_root.GetComponentsInChildren<SpectatorBattleView>());
            Object.Destroy(_right.gameObject); yield return null; yield return null;
            Assert.AreEqual(0, _layer.Count);
            _layer.StartBattle(Start(2));
            yield return new WaitForSecondsRealtime(2.1f);
            Assert.AreEqual(0, _layer.Count);
        }
        [UnityTest] public IEnumerator ResultAndMissingEndPacketBothReleaseViews()
        {
            _layer.StartBattle(Start());
            _layer.End(new BattleResult { BattleId = 1, WinnerId = 1 }); yield return null;
            Assert.IsTrue(_root.GetComponentsInChildren<Text>().Any(t => t.text == "WIN"));
            yield return new WaitForSecondsRealtime(2.1f);
            Assert.AreEqual(0, _layer.Count);
            _layer.StartBattle(Start());
            yield return new WaitForSecondsRealtime(3.1f);
            Assert.AreEqual(0, _layer.Count);
        }
    }
}
