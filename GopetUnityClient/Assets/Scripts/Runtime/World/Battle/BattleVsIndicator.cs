using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Khối "VS" giữa màn đấu — ghim đỉnh cách lề dưới top bar (32px) 10px.</summary>
    public sealed class BattleVsIndicator : MonoBehaviour
    {
        private Text _turnLabel;

        public static BattleVsIndicator Create(Transform parent, Font font)
        {
            const float topBarBottomY = 1f - 32f / 540f;
            var vsTopY = topBarBottomY - 10f / 540f;
            var deltaY = vsTopY - 0.76f;

            var go = new GameObject("VS", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var indicator = go.AddComponent<BattleVsIndicator>();

            var badgeGo = new GameObject("VS badge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(go.transform, false);
            var badge = badgeGo.GetComponent<Image>();
            badge.sprite = BattleSkin.Load("Battle/vs");
            badge.preserveAspect = true;
            var bRect = (RectTransform)badgeGo.transform;
            bRect.anchorMin = new Vector2(0.43f, 0.58f + deltaY);
            bRect.anchorMax = new Vector2(0.57f, 0.76f + deltaY);
            bRect.offsetMin = bRect.offsetMax = Vector2.zero;

            indicator._turnLabel = UiBuilder.MakeText(go.transform, font, "Thông báo lượt", 13, false);
            var tRect = indicator._turnLabel.rectTransform;
            tRect.anchorMin = new Vector2(0.3f, 0.54f + deltaY);
            tRect.anchorMax = new Vector2(0.7f, 0.59f + deltaY);
            tRect.offsetMin = tRect.offsetMax = Vector2.zero;
            indicator._turnLabel.alignment = TextAnchor.MiddleCenter;
            indicator._turnLabel.fontStyle = FontStyle.Bold;
            indicator._turnLabel.color = new Color(1f, 0.92f, 0.45f, 1f);
            return indicator;
        }

        /// <summary>Chỉ báo khi đến lượt người chơi cục bộ; lượt quái/đối thủ thì im lặng.</summary>
        public void SetTurn(bool isLocalTurn) => _turnLabel.text = isLocalTurn ? "Đến lượt bạn" : "";
    }
}
