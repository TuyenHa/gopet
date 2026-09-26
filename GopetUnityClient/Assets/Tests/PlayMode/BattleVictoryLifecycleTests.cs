using System.Collections;
using System.Linq;
using Gopet.Runtime.World.Battle;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class BattleVictoryLifecycleTests
    {
        [UnityTest]
        public IEnumerator PendingResult_IgnoresDuplicateRemoveTimeoutAndBack_UntilOk()
        {
            using var scene = new BattleVictoryTestScene();
            scene.Start();
            var view = scene.Coordinator.View;
            scene.SkillTurn();
            scene.Result();
            scene.Result(-99, "Không được thay kết quả đầu");
            scene.Remove();
            scene.ExpireTimers();
            Assert.IsTrue(view.HasResult);
            Assert.IsTrue(view.AwaitingVictoryConfirmation);
            Assert.IsNull(view.GetComponentInChildren<BattleVictoryPopup>(), "Chờ hoạt cảnh cuối.");
            view.transform.Find("Top bar/Quay lại").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.AreSame(view, scene.Coordinator.View, "Timeout không được đóng kết quả đang chờ.");
            view.GetComponent<BattleTurnAnimator>().FlushImmediate();
            scene.ExpireTimers();
            yield return null;
            var popup = view.GetComponentInChildren<BattleVictoryPopup>();
            Assert.IsNotNull(popup);
            Assert.AreEqual(1, view.GetComponentsInChildren<BattleVictoryPopup>().Length);
            var content = string.Join("\n", popup.GetComponentsInChildren<Text>().Select(t => t.text));
            StringAssert.Contains("Bùa x1", content);
            StringAssert.Contains("Cuồng nộ", content);
            StringAssert.DoesNotContain("Không được thay kết quả đầu", content);
            view.transform.Find("Top bar/Quay lại").GetComponent<Button>().onClick.Invoke();
            Assert.AreSame(view, scene.Coordinator.View);
            var ok = popup.GetComponentInChildren<Button>();
            var closedBefore = scene.BattleModes.Count(m => !m);
            ok.onClick.Invoke();
            ok.onClick.Invoke();
            Assert.IsNull(scene.Coordinator.View);
            Assert.AreEqual(closedBefore + 1, scene.BattleModes.Count(m => !m));
            Assert.IsEmpty(scene.Sent, "OK không gửi gói lĩnh thưởng hay dịch chuyển.");
            yield return null;
            scene.Start();
            scene.Result();
            var next = scene.Coordinator.View.GetComponentInChildren<BattleVictoryPopup>();
            var nextText = string.Join("\n", next.GetComponentsInChildren<Text>().Select(t => t.text));
            StringAssert.Contains("Không sử dụng kỹ năng", nextText);
        }

        [UnityTest]
        public IEnumerator FinalAnimation_CompletesNaturallyBeforePopup()
        {
            using var scene = new BattleVictoryTestScene();
            scene.Start();
            var view = scene.Coordinator.View;
            scene.SkillTurn();
            scene.Result();
            Assert.IsNull(view.GetComponentInChildren<BattleVictoryPopup>());
            var deadline = Time.realtimeSinceStartup + 10f;
            while (view.GetComponentInChildren<BattleVictoryPopup>() == null
                   && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(view.GetComponent<BattleTurnAnimator>().Idle);
            Assert.IsNotNull(view.GetComponentInChildren<BattleVictoryPopup>());
            Assert.IsTrue(view.AwaitingVictoryConfirmation);
        }

        [UnityTest]
        public IEnumerator MapUpdateAndRepeatedStart_KeepVictoryPopupUntilOk()
        {
            using var scene = new BattleVictoryTestScene();
            scene.Start();
            var view = scene.Coordinator.View;
            scene.SkillTurn();
            scene.Result();
            var modeChanges = scene.BattleModes.Count;
            // Cả khi đòn cuối đang chạy, gói map/start cũng không được bỏ kết quả.
            scene.Coordinator.OnPlaceChanged();
            scene.Start();
            Assert.AreSame(view, scene.Coordinator.View);
            Assert.AreEqual(modeChanges, scene.BattleModes.Count);
            view.GetComponent<BattleTurnAnimator>().FlushImmediate();
            var popup = view.GetComponentInChildren<BattleVictoryPopup>();
            Assert.IsNotNull(popup);
            scene.Coordinator.OnPlaceChanged();
            scene.Start();
            yield return null;
            Assert.AreSame(view, scene.Coordinator.View);
            Assert.IsTrue(popup.gameObject.activeInHierarchy);
            Assert.AreEqual(modeChanges, scene.BattleModes.Count);
            // Gói mở trận tới lúc chờ OK là trận MỚI thật (server chỉ gửi khi mở trận) —
            // hoãn tới OK rồi dựng, không bỏ, không thì thành trận ẩn chặn mọi cú đánh quái.
            popup.GetComponentInChildren<Button>().onClick.Invoke();
            Assert.IsNotNull(scene.Coordinator.View);
            Assert.AreNotSame(view, scene.Coordinator.View);
            Assert.IsTrue(scene.BattleModes.Last());
            Assert.IsEmpty(scene.Sent);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PendingStart_EndedBeforeOk_IsDropped()
        {
            using var scene = new BattleVictoryTestScene();
            scene.Start();
            var view = scene.Coordinator.View;
            scene.Result();
            view.GetComponent<BattleTurnAnimator>().FlushImmediate();
            var popup = view.GetComponentInChildren<BattleVictoryPopup>();
            Assert.IsNotNull(popup);
            scene.Start();   // trận mới mở trong lúc chờ OK → hoãn
            scene.Remove();  // …và đã kết thúc trước khi bấm OK → bỏ
            popup.GetComponentInChildren<Button>().onClick.Invoke();
            Assert.IsNull(scene.Coordinator.View);
            Assert.IsFalse(scene.BattleModes.Last());
            yield return null;
        }

        [UnityTest]
        public IEnumerator MapUpdate_StillClosesAnUnfinishedBattle()
        {
            using var scene = new BattleVictoryTestScene();
            scene.Start();
            scene.Coordinator.OnPlaceChanged();
            Assert.IsNull(scene.Coordinator.View);
            Assert.IsFalse(scene.BattleModes.Last());
            yield return null;
        }

        [UnityTest]
        public IEnumerator LossAndPvp_KeepTheirAutomaticClose()
        {
            using var scene = new BattleVictoryTestScene();
            foreach (var pvp in new[] { false, true })
            {
                scene.Start(pvp);
                scene.Result(pvp ? 7 : -99);
                var view = scene.Coordinator.View;
                Assert.IsFalse(view.AwaitingVictoryConfirmation);
                Assert.IsNull(view.GetComponentInChildren<BattleVictoryPopup>());
                scene.ExpireTimers();
                yield return null;
                Assert.IsNull(scene.Coordinator.View);
            }
        }
    }
}
