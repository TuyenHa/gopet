using Gopet.Net.Battle;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    public sealed class BattlePetCard : MonoBehaviour
    {
        private BattlePet _pet;
        private RawImage _petImage;
        private int _frame;
        private float _nextFrame;

        public int ActorId => _pet.ActorId;
        public int Hp => _pet.Hp;
        public int Mp => _pet.Mp;
        public int MaxHp => _pet.MaxHp;
        public int MaxMp => _pet.MaxMp;
        public RectTransform EffectAnchor => _petImage.rectTransform;

        public static BattlePetCard Create(Transform parent, BattlePet pet, bool left,
            RemoteAssetCache assets)
        {
            var go = new GameObject(left ? "Pet trái" : "Pet phải", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(left ? 0.31f : 0.66f, 0.45f);
            rect.sizeDelta = new Vector2(160f, 200f);

            var card = go.AddComponent<BattlePetCard>();
            card._pet = pet;
            card.BuildSprite(left);
            assets.Get(pet.ImagePath, ImagePackets.TypeNpc, card.SetTexture);
            return card;
        }

        public void Apply(int hpDelta, int mpDelta)
        {
            _pet.Hp = Mathf.Clamp(_pet.Hp + hpDelta, 0, Mathf.Max(0, _pet.MaxHp));
            _pet.Mp = Mathf.Clamp(_pet.Mp + mpDelta, 0, Mathf.Max(0, _pet.MaxMp));
            if (hpDelta != 0) BattleFloatText.Create(transform, hpDelta, false);
            if (mpDelta != 0) BattleFloatText.Create(transform, mpDelta, true);
        }

        private void BuildSprite(bool left)
        {
            var imageGo = new GameObject("Pet", typeof(RectTransform), typeof(RawImage));
            imageGo.transform.SetParent(transform, false);
            _petImage = imageGo.GetComponent<RawImage>();
            _petImage.raycastTarget = false;
            var rect = _petImage.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 16f - _pet.VerticalOffset * 2f);
            rect.sizeDelta = new Vector2(96f, 128f);
            rect.localScale = new Vector3(left ? -1f : 1f, 1f, 1f);
        }

        private void SetTexture(Texture2D texture)
        {
            if (this == null || texture == null) return;
            _petImage.texture = texture;
            var count = Mathf.Max(1, _pet.FrameCount);
            var frameWidth = texture.width / count;
            _petImage.rectTransform.sizeDelta = new Vector2(frameWidth * 2f, texture.height * 2f);
            ShowFrame();
        }

        private void Update()
        {
            if (_petImage.texture == null || _pet.FrameCount <= 1 || Time.time < _nextFrame) return;
            _nextFrame = Time.time + 0.16f;
            _frame = (_frame + 1) % _pet.FrameCount;
            ShowFrame();
        }

        private void ShowFrame()
        {
            var count = Mathf.Max(1, _pet.FrameCount);
            _petImage.uvRect = new Rect((float)_frame / count, 0f, 1f / count, 1f);
        }
    }
}
