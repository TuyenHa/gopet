using System;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nút Pet (icon đầu cún) ngay DƯỚI minimap góc phải-trên — mở
    /// <see cref="PetActionRadial"/>. Cùng kiểu icon + nhãn với hàng nút HUD
    /// (<see cref="ShopServiceEventHud"/>), thẳng cột với minimap.
    /// </summary>
    public sealed class PetActionButton : MonoBehaviour
    {
        /// <summary>Khe giữa đáy minimap và đỉnh nút Pet, theo tỉ lệ chiều cao màn.</summary>
        private const float GapBelowMinimapFrac = 0.02f;

        public event Action Clicked;

        public static PetActionButton Create(Transform parent)
        {
            PetActionButton comp = null;
            var rect = ShopServiceEventHud.MakeIconButton(parent, UiBuilder.DefaultFont(), HudSkin.Pet, "Pet",
                () => comp?.Clicked?.Invoke());
            comp = rect.gameObject.AddComponent<PetActionButton>();

            // Cùng bề ngang và mép phải với minimap để hai ô thẳng một cột.
            const float size = ShopServiceEventHud.SizeFrac;
            const float right = 1f - MinimapWidget.MarginFrac;
            const float top = 1f - MinimapWidget.BottomFrac - GapBelowMinimapFrac;
            rect.anchorMin = new Vector2(right - size, top - size);
            rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return comp;
        }
    }
}
