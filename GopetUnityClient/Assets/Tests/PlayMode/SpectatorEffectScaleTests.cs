using System.Reflection;
using Gopet.Runtime.World;
using NUnit.Framework;
using UnityEngine;

namespace Gopet.PlayModeTests
{
    public sealed class SpectatorEffectScaleTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        [TestCase(1f)]
        [TestCase(1f / 3f)]
        public void ActorTravelReachesTargetUnderScaledParent(float scale)
        {
            var root = new GameObject("Effects", typeof(RectTransform));
            try
            {
                root.transform.localScale = Vector3.one * scale;
                var target = new GameObject("Target", typeof(RectTransform)).GetComponent<RectTransform>();
                target.SetParent(root.transform, false);
                target.position = new Vector3(100, 40, 0);
                var actor = new GameObject("Actor", typeof(RectTransform)).AddComponent<BattleActorEffectView>();
                actor.transform.SetParent(root.transform, false);
                var from = new Vector3(-50, 10, 0);
                typeof(BattleActorEffectView).GetMethod("BeginTravel", PrivateInstance)
                    .Invoke(actor, new object[] { (Vector3?)from, target });
                var delta = (Vector3)typeof(BattleActorEffectView).GetField("_travelDelta", PrivateInstance).GetValue(actor);
                Assert.Less(Vector3.Distance(from + root.transform.TransformVector(delta), target.position), 0.001f);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [TestCase(1f)]
        [TestCase(1f / 3f)]
        public void FlameTrajectoryScalesWithEffectCanvas(float scale)
        {
            var root = new GameObject("Effects", typeof(RectTransform));
            var randomState = Random.state;
            try
            {
                Random.InitState(42);
                root.transform.localScale = Vector3.one * scale;
                var target = new GameObject("Target", typeof(RectTransform)).GetComponent<RectTransform>();
                target.SetParent(root.transform, false);
                target.position = new Vector3(100, 40, 0);
                var build = typeof(BattleFlameFallFx).GetMethod("Build", BindingFlags.Static | BindingFlags.NonPublic);
                for (var i = 0; i < 9; i++)
                {
                    var jet = build.Invoke(null, new object[] { null, target, i, 240f, 60f, 1f,
                        148.8f, 0.2f, root.transform, root.transform });
                    var type = jet.GetType();
                    var from = (Vector3)type.GetField("From").GetValue(jet);
                    var to = (Vector3)type.GetField("To").GetValue(jet);
                    Assert.That(from.y - to.y, Is.InRange(148.8f * 0.85f * scale - 0.001f,
                        148.8f * 1.15f * scale + 0.001f));
                    Assert.LessOrEqual(Mathf.Abs(to.x - target.position.x), 240f * 0.27f * scale + 0.001f);
                }
            }
            finally { Random.state = randomState; Object.DestroyImmediate(root); }
        }
    }
}
