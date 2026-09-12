using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nút "≡" ở góc phải-dưới HUD, mở <see cref="CharacterMenuView"/>.
    ///
    /// <para>Đặt phải-dưới thay vì phải-trên để không giẫm <see cref="Gopet.Runtime.World.CurrencyBar"/>
    /// (phải-trên). Tay cầm phổ biến giơ ngón cái ở nửa dưới, nên góc dưới cũng thuận tay hơn.</para>
    /// </summary>
    public sealed class CharacterMenuButton : MonoBehaviour
    {
        private GameObject _mailBadge;
        public event Action Clicked;

        public static CharacterMenuButton Create(Transform parent)
        {
            var go = new GameObject("Character Menu Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-14f, 76f);
            rect.sizeDelta = new Vector2(56f, 56f);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.18f, 0.55f, 0.9f, 0.95f);
            RoundedUiSprite.Apply(img);

            var label = UiBuilder.MakeText(go.transform, UiBuilder.BuiltinFont(), "Icon", 28, true);
            label.text = "≡"; // ≡
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;

            var comp = go.AddComponent<CharacterMenuButton>();
            comp._mailBadge = MakeMailBadge(go.transform);
            go.GetComponent<Button>().onClick.AddListener(() => comp.Clicked?.Invoke());
            return comp;
        }

        public void SetMailUnread(bool value)
        {
            if (_mailBadge != null) _mailBadge.SetActive(value);
        }

        private static GameObject MakeMailBadge(Transform parent)
        {
            var go = new GameObject("Mail Unread", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-3f, -3f);
            rect.sizeDelta = new Vector2(14f, 14f);
            go.GetComponent<Image>().color = new Color(.95f, .2f, .2f, 1f);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.SetActive(false);
            return go;
        }
    }
}
