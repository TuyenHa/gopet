using System.Collections;
using System.Linq;
using Gopet.Net.Battle;
using Gopet.Runtime.UI;
using Gopet.Runtime.World.Battle;
using Gopet.UiLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gopet.PlayModeTests
{
    public sealed class BattleVictoryPopupTests
    {
        private GameObject _root;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Popup test", typeof(RectTransform), typeof(Canvas));
            _root.GetComponent<RectTransform>().sizeDelta = new Vector2(960, 540);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_root);

        [UnityTest]
        public IEnumerator RewardsAndSkills_AreLiteral_AndOkConfirmsOnlyOnce()
        {
            var tracker = Tracker();
            tracker.RecordTurn(new BattleTurn { BattleId = 7, ActorId = 7,
                Effects = new[] { new BattleEffect { SkillId = 101, ActorId = -99 } } });
            var summary = tracker.Complete(new BattleResult {
                BattleId = 7, WinnerId = 7, Coin = 120, Experience = 350,
                Messages = new[] { "Bùa x1", "<b>Hồng ngọc</b> x2" }
            }, 83);
            var confirmations = 0;
            var popup = BattleVictoryPopup.Create(_root.transform, summary, () => confirmations++);
            yield return null;
            var labels = popup.GetComponentsInChildren<Text>();
            var text = string.Join("\n", labels.Select(t => t.text));
            StringAssert.Contains("1 phút 23 giây", text);
            StringAssert.Contains("Bùa x1", text);
            StringAssert.Contains("<b>Hồng ngọc</b> x2", text);
            StringAssert.Contains("Cào", text);
            StringAssert.Contains("120", text);
            StringAssert.Contains("350", text);
            Assert.IsTrue(labels.All(t => !t.supportRichText));
            Assert.IsTrue(popup.GetComponent<Image>().raycastTarget);
            var buttons = popup.GetComponentsInChildren<Button>();
            Assert.AreEqual(1, buttons.Length);
            Assert.AreEqual("OK", buttons[0].GetComponentInChildren<Text>().text);
            buttons[0].onClick.Invoke();
            buttons[0].onClick.Invoke();
            Assert.AreEqual(1, confirmations);
            Assert.IsFalse(buttons[0].interactable);
        }

        [Test]
        public void EmptyRewardAndSkillLists_ExplainWhatHappened()
        {
            var summary = Tracker().Complete(new BattleResult { BattleId = 7, WinnerId = 7 }, 8);
            var popup = BattleVictoryPopup.Create(_root.transform, summary, () => { });
            var text = string.Join("\n", popup.GetComponentsInChildren<Text>().Select(t => t.text));
            StringAssert.Contains("Không nhận được vật phẩm", text);
            StringAssert.Contains("Không sử dụng kỹ năng", text);
        }

        [UnityTest]
        public IEnumerator LongRewards_ScrollWhileTitleAndOkStayOutsideViewport()
        {
            var result = new BattleResult { BattleId = 7, WinnerId = 7,
                Messages = Enumerable.Range(1, 64).Select(i =>
                    "Vật phẩm có tên rất dài để kiểm tra xuống dòng " + i + " x1").ToArray() };
            var popup = BattleVictoryPopup.Create(_root.transform, Tracker().Complete(result, 62), () => { });
            foreach (var size in new[] { new Vector2(640, 360), new Vector2(1920, 1080) })
            {
                _root.GetComponent<RectTransform>().sizeDelta = size;
                yield return null;
                Canvas.ForceUpdateCanvases();
                var scroll = popup.GetComponentInChildren<ScrollRect>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
                Assert.Greater(scroll.content.rect.height, scroll.viewport.rect.height);
                var ok = popup.GetComponentInChildren<Button>().GetComponent<RectTransform>();
                Assert.IsFalse(ok.IsChildOf(scroll.viewport));
                var panel = (RectTransform)ok.parent;
                Assert.LessOrEqual(panel.rect.width, size.x);
                Assert.LessOrEqual(panel.rect.height, size.y);
                Assert.GreaterOrEqual(ok.anchoredPosition.y, 0);
                Assert.IsNotNull(scroll.viewport.GetComponent<RectMask2D>());
            }
        }

        private static BattleSummaryTracker Tracker() => new BattleSummaryTracker(new BattleStart {
            BattleId = 7, Kind = BattleKind.Mob, IsParticipant = true,
            LocalPet = new BattlePet { ActorId = 7,
                Skills = new[] { new BattleSkill { Id = 101, Name = "Cào" } } },
            Opponent = new BattlePet { ActorId = -99, Name = "Sói" }
        }, 0);
    }
}
