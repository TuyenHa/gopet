using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>
    /// Nền màn đấu: ảnh khung cảnh + hiệu ứng động của nó. Hiệu ứng là CON của nền nên luôn
    /// nằm trên ảnh nền và dưới mọi thứ dựng sau (card pet, HUD, nút) mà không cần chỉnh
    /// sibling index — và không chặn click vì mọi Image đều tắt raycastTarget.
    /// </summary>
    public sealed class BattleBackdrop : MonoBehaviour
    {
        private Image _image;
        private BattleAmbientFx _ambient;

        public int SceneId { get; private set; } = -1;

        public static BattleBackdrop Create(Transform parent, int sceneId)
        {
            var go = new GameObject("Nền", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var backdrop = go.AddComponent<BattleBackdrop>();
            backdrop._image = go.GetComponent<Image>();
            backdrop._image.type = Image.Type.Simple;
            backdrop._image.raycastTarget = false;
            backdrop.Apply(sceneId);
            return backdrop;
        }

        /// <summary>Đổi khung cảnh, kể cả giữa trận. Gọi lại cùng id thì không làm gì.</summary>
        public void Apply(int sceneId)
        {
            sceneId = BattleSceneCatalog.Normalize(sceneId);
            if (sceneId == SceneId) return;
            SceneId = sceneId;

            // Thiếu ảnh (asset chưa import) thì rơi về rừng, rồi về màu nền cũ.
            var sprite = BattleSkin.Load(BattleSceneCatalog.BackgroundPath(sceneId))
                         ?? BattleSkin.Load(BattleSceneCatalog.BackgroundPath(BattleSceneCatalog.DefaultId));
            _image.sprite = sprite;
            _image.color = sprite == null ? new Color(0f, 0.08f, 0.13f, 0.92f) : Color.white;

            if (_ambient != null) Destroy(_ambient.gameObject);
            _ambient = BattleAmbientFx.Create(transform,
                BattleAmbientPresets.For(BattleSceneCatalog.Ambient(sceneId)));
        }
    }
}
