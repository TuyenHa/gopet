using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Nút 🐾 góc phải-dưới HUD — mở <see cref="PetActionRadial"/>.</summary>
    public sealed class PetActionButton : MonoBehaviour
    {
        public event Action Clicked;

        public static PetActionButton Create(Transform parent)
        {
            var go = new GameObject("Pet Action Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-14f, 140f);
            rect.sizeDelta = new Vector2(56f, 56f);

            var img = go.GetComponent<Image>();
            img.color = new Color(0.85f, 0.5f, 0.2f, 0.95f);
            RoundedUiSprite.Apply(img);

            var label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Icon", 20, true);
            label.text = "PET";
            label.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.color = Color.white;

            var comp = go.AddComponent<PetActionButton>();
            go.GetComponent<Button>().onClick.AddListener(() => comp.Clicked?.Invoke());
            return comp;
        }
    }
}
