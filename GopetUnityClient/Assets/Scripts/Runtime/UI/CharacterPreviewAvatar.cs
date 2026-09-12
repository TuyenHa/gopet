using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    internal sealed class CharacterPreviewAvatar : MonoBehaviour
    {
        private const string MaleResource = "Ui/Character/male";
        private const string FemaleResource = "Ui/Character/female";

        public static CharacterPreviewAvatar Create(Transform parent, int gender)
        {
            var go = new GameObject("AvatarPreview", typeof(RectTransform), typeof(Image), typeof(CharacterPreviewAvatar));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.61f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(108f, 116f);

            var image = go.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>(gender == 0 ? MaleResource : FemaleResource);
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.color = Color.white;

            if (image.sprite == null)
            {
                Debug.LogWarning($"[Gopet] Không tìm thấy preview nhân vật giới tính {gender}.");
            }

            return go.GetComponent<CharacterPreviewAvatar>();
        }
    }
}
