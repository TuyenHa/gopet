using System;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Nút tròn "KỸ NĂNG" bên trái sân đấu. Bấm để mở/đóng
    /// <see cref="BattleSkillPopup"/>.</summary>
    public static class BattleSkillButton
    {
        private const float Size = 62f;

        public static RectTransform Create(Transform parent, Action clicked, out Button button)
        {
            var go = new GameObject("Nút kỹ năng", typeof(RectTransform), typeof(Image),
                typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(18f, 22f);
            rect.sizeDelta = new Vector2(Size, Size);

            var img = go.GetComponent<Image>();
            img.sprite = BattleSkin.Load("Battle/btn-skill-round");
            img.preserveAspect = true;
            // Thiếu sprite thì vẫn phải bấm được — tô tạm thay vì để ô trắng trơ.
            img.color = img.sprite == null ? new Color(0.2f, 0.28f, 0.45f, 0.95f) : Color.white;

            button = go.GetComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(() => clicked?.Invoke());
            return rect;
        }
    }
}
