using System;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Nút tròn "KHUNG CẢNH" bên phải sân đấu, đối xứng với nút kỹ năng bên trái.
    /// Bấm để mở <see cref="BattleScenePopup"/>.</summary>
    public static class BattleSceneButton
    {
        private const float Size = 62f;

        public static Button Create(Transform parent, Font font, Action clicked)
        {
            var go = new GameObject("Nút khung cảnh", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-18f, 22f);
            rect.sizeDelta = new Vector2(Size, Size);

            var img = go.GetComponent<Image>();
            img.sprite = BattleSkin.Load("Battle/btn-scene-round");
            img.preserveAspect = true;
            if (img.sprite == null)
            {
                // Thiếu sprite vẫn phải bấm được: tô tạm + chữ thay vì ô trắng trơ.
                img.sprite = PanelSprites.Rounded(30);
                img.type = Image.Type.Sliced;
                img.color = new Color(0.2f, 0.28f, 0.45f, 0.95f);
                var label = UiBuilder.MakeText(go.transform, font, "Nhãn", 11, true);
                label.text = "Khung\ncảnh";
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
            }

            var button = go.GetComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(() => clicked?.Invoke());
            return button;
        }
    }
}
