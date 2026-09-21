using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Dấu chân thú — biểu tượng nhận diện của popup trong game. Hai dạng:
    /// <see cref="Create"/> có vòng tròn trắng bọc ngoài (badge tiêu đề),
    /// <see cref="CreateIcon"/> chỉ mỗi dấu chân (băng chân).
    ///
    /// <para>Thiếu file icon thì <see cref="CreateIcon"/> không dựng gì, còn
    /// <see cref="Create"/> vẫn để lại vòng tròn trắng — mất icon chứ không thủng
    /// một lỗ trong badge.</para>
    /// </summary>
    public static class PawBadge
    {
        /// <summary>Vòng tròn trắng + dấu chân, neo trái, căn giữa theo chiều dọc.</summary>
        public static GameObject Create(Transform parent, float size, Vector2 offset)
        {
            var go = new GameObject("Paw", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = offset;

            var circle = go.GetComponent<Image>();
            circle.sprite = CircleUiSprite.Get();
            circle.color = Color.white;

            var paw = HudSkin.Get(HudSkin.Paw);
            if (paw == null) return go;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRect = (RectTransform)iconGo.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(size * 2f / 3f, size * 2f / 3f);

            var icon = iconGo.GetComponent<Image>();
            icon.sprite = paw;
            icon.preserveAspect = true;
            return go;
        }

        /// <summary>Chỉ dấu chân, không vòng tròn. Trả <c>null</c> khi thiếu file icon.</summary>
        public static GameObject CreateIcon(Transform parent, float size, Vector2 offset)
        {
            var paw = HudSkin.Get(HudSkin.Paw);
            if (paw == null) return null;

            var go = new GameObject("Paw", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = offset;

            var icon = go.GetComponent<Image>();
            icon.sprite = paw;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            return go;
        }
    }
}
