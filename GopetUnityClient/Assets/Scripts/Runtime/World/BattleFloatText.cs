using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Số damage/hồi phục bay lên trên đầu pet. Sống đúng 1.2s như jar
    /// (<c>bd.java:167-195</c> giữ nhãn 1000ms rồi tắt).</summary>
    public sealed class BattleFloatText : MonoBehaviour
    {
        private const float LifeSeconds = 1.2f;

        private Text _text;
        private float _born;

        /// <param name="delaySeconds">Hoãn trước khi hiện. Jar tách số HP và số MP ra 2 bước
        /// tuần tự cách nhau ~1s (<c>ei.java:213-291</c>) thay vì chồng lên nhau.</param>
        public static void Create(Transform parent, int delta, bool mana, float delaySeconds = 0f)
        {
            var color = mana ? new Color(0.4f, 0.8f, 1f) : delta > 0 ? Color.green : Color.red;
            Spawn(parent, mana ? "MP nổi" : "HP nổi",
                delta > 0 ? $"+{delta}" : delta.ToString(), color, delaySeconds);
        }

        public static void CreateMiss(Transform parent) =>
            Spawn(parent, "Trượt", "TRƯỢT", new Color(1f, 0.85f, 0.25f), 0f);

        private static readonly Color ExpColor = new Color(1f, 0.82f, 0.2f);
        private static readonly Color CoinColor = new Color(1f, 0.45f, 0.62f);

        /// <summary>EXP vàng kim. Giá trị luôn do server gửi xuống (5% EXP giết quái mỗi đòn
        /// trúng, trần 30%/trận — xem <c>HitExpReward</c>), không có con số nào cố định ở client.
        /// Mỗi đòn hiện số trần; phần thưởng cuối trận bật <paramref name="withUnit"/> để đứng
        /// cạnh dòng Ngọc mà vẫn phân biệt được.</summary>
        public static void CreateExp(Transform parent, int amount, float delaySeconds = 0f,
            bool withUnit = false) =>
            Spawn(parent, "EXP nổi", withUnit ? $"+{amount} EXP" : $"+{amount}", ExpColor, delaySeconds);

        /// <summary>Ngọc thưởng cuối trận — hồng ngọc, khác màu EXP để đọc lướt vẫn phân biệt.</summary>
        public static void CreateCoin(Transform parent, int amount, float delaySeconds = 0f) =>
            Spawn(parent, "Ngọc nổi", $"+{amount} Ngọc", CoinColor, delaySeconds);

        private static void Spawn(Transform parent, string name, string label, Color color,
            float delaySeconds)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var value = go.GetComponent<Text>();
            value.font = UiBuilder.BuiltinFont(); value.fontSize = 24;
            value.fontStyle = FontStyle.Bold; value.alignment = TextAnchor.MiddleCenter;
            value.text = label; value.color = color;
            value.raycastTarget = false;
            var rect = value.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.6f);
            rect.sizeDelta = new Vector2(180f, 40f);
            var effect = go.AddComponent<BattleFloatText>();
            effect._text = value;
            effect._born = Time.unscaledTime + delaySeconds;
            if (delaySeconds > 0f) value.color = Transparent(color);
        }

        private static Color Transparent(Color c) => new Color(c.r, c.g, c.b, 0f);

        private void Update()
        {
            var age = Time.unscaledTime - _born;
            if (age < 0f) return; // còn trong khoảng hoãn — đứng yên, trong suốt
            transform.localPosition += Vector3.up * (45f * Time.unscaledDeltaTime);
            var color = _text.color;
            color.a = 1f - Mathf.Clamp01(age / LifeSeconds);
            _text.color = color;
            if (age >= LifeSeconds) Destroy(gameObject);
        }
    }
}
