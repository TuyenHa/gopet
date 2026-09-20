using Gopet.Net.Battle;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>
    /// Ảnh đại diện trên thẻ máu của trận đấu.
    ///
    /// <para>Ô ảnh có cạnh cố định, còn ảnh pet/quái thì mỗi con một cỡ (24x28, 30x30…).
    /// Nhét thẳng ảnh cho đầy ô là vừa PHÓNG LẺ (nhoè) vừa MÉO dáng — ảnh 24x28 kéo
    /// thành vuông 60x60. Nên ô giữ nguyên chỗ, còn ảnh thì đặt theo đúng tỉ lệ gốc và
    /// phóng bằng bội nguyên của pixel màn, chấp nhận nhỏ hơn ô một chút.</para>
    /// </summary>
    public sealed partial class BattleHudPanel
    {
        /// <summary>Cạnh ô ảnh đại diện. Ảnh thật luôn nhỏ hơn hoặc bằng ô này.</summary>
        private const float AvatarBox = 60f;

        private static void BuildAvatar(Transform outer, BattleHudPanel panel,
            RemoteAssetCache assets, BattlePet pet)
        {
            var go = new GameObject("Avatar", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(outer, false);
            panel._avatar = go.GetComponent<RawImage>();
            panel._avatar.raycastTarget = false;
            var r = (RectTransform)go.transform;
            r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            r.anchoredPosition = new Vector2(8f, 0f);
            r.sizeDelta = new Vector2(AvatarBox, AvatarBox);
            assets.Get(pet.ImagePath, ImagePackets.TypeNpc, panel.SetAvatarTexture);
        }

        private void SetAvatarTexture(Texture2D texture)
        {
            if (this == null || _avatar == null || texture == null) return;
            _avatar.texture = texture;
            _avatar.uvRect = new Rect(0f, 0f, 1f / Mathf.Max(1, _frameCount), 1f);

            // Ô ảnh vốn cố định AvatarBox×AvatarBox: ảnh 24×28 bị kéo thành vuông 60×60,
            // vừa phóng lẻ (nhoè) vừa méo dáng. Đổi cỡ ô theo đúng tỉ lệ ảnh, phóng bằng
            // bội nguyên của pixel màn — nhỏ hơn ô một chút nhưng nét và không méo.
            var frameWidth = (float)texture.width / Mathf.Max(1, _frameCount);
            var scale = BattleSkin.SnappedFitScale(this, AvatarBox, frameWidth, texture.height);
            ((RectTransform)_avatar.transform).sizeDelta =
                new Vector2(frameWidth * scale, texture.height * scale);
        }
    }
}
