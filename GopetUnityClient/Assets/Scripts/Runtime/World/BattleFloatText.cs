using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed class BattleFloatText : MonoBehaviour
    {
        private Text _text;
        private float _born;

        public static void Create(Transform parent, int delta, bool mana)
        {
            var go = new GameObject(mana ? "MP nổi" : "HP nổi", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var value = go.GetComponent<Text>();
            value.font = UiBuilder.BuiltinFont(); value.fontSize = 24;
            value.fontStyle = FontStyle.Bold; value.alignment = TextAnchor.MiddleCenter;
            value.text = delta > 0 ? $"+{delta}" : delta.ToString();
            value.color = mana ? new Color(0.4f, 0.8f, 1f) : delta > 0 ? Color.green : Color.red;
            var rect = value.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.6f);
            rect.sizeDelta = new Vector2(180f, 40f);
            var effect = go.AddComponent<BattleFloatText>();
            effect._text = value; effect._born = Time.unscaledTime;
        }

        public static void CreateMiss(Transform parent)
        {
            var go = new GameObject("Trượt", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var value = go.GetComponent<Text>();
            value.font = UiBuilder.BuiltinFont(); value.fontSize = 24;
            value.fontStyle = FontStyle.Bold; value.alignment = TextAnchor.MiddleCenter;
            value.text = "TRƯỢT"; value.color = new Color(1f, 0.85f, 0.25f);
            var rect = value.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.6f);
            rect.sizeDelta = new Vector2(180f, 40f);
            var effect = go.AddComponent<BattleFloatText>();
            effect._text = value; effect._born = Time.unscaledTime;
        }

        private void Update()
        {
            var age = Time.unscaledTime - _born;
            transform.localPosition += Vector3.up * (45f * Time.unscaledDeltaTime);
            var color = _text.color; color.a = 1f - Mathf.Clamp01(age / 1.2f); _text.color = color;
            if (age >= 1.2f) Destroy(gameObject);
        }
    }
}
