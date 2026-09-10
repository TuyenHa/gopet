using Gopet.Net.Battle;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World
{
    /// <summary>Một pet trên sân đấu: sprite động, tên và HP/MP.</summary>
    public sealed class BattlePetCard : MonoBehaviour
    {
        private BattlePet _pet;
        private RawImage _petImage;
        private Text _name, _hp, _mp;
        private int _frame;
        private float _nextFrame;

        public int ActorId => _pet.ActorId;
        public int Mp => _pet.Mp;
        public RectTransform EffectAnchor => _petImage.rectTransform;

        public static BattlePetCard Create(Transform parent, BattlePet pet, bool left,
            RemoteAssetCache assets, Font font)
        {
            var go = new GameObject(left ? "Pet trái" : "Pet phải", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(left ? 0.25f : 0.75f, 0.52f);
            rect.sizeDelta = new Vector2(330f, 300f);
            var card = go.AddComponent<BattlePetCard>();
            card._pet = pet;
            card.Build(font, left);
            assets.Get(pet.ImagePath, ImagePackets.TypeNpc, card.SetTexture);
            return card;
        }

        public void Apply(int hpDelta, int mpDelta)
        {
            _pet.Hp = Mathf.Clamp(_pet.Hp + hpDelta, 0, Mathf.Max(0, _pet.MaxHp));
            _pet.Mp = Mathf.Clamp(_pet.Mp + mpDelta, 0, Mathf.Max(0, _pet.MaxMp));
            Refresh();
            if (hpDelta != 0) BattleFloatText.Create(transform, hpDelta, false);
            if (mpDelta != 0) BattleFloatText.Create(transform, mpDelta, true);
        }

        private void Build(Font font, bool left)
        {
            var panel = new GameObject("Thông tin", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f); rect.sizeDelta = new Vector2(0f, 86f);
            panel.GetComponent<Image>().color = new Color(0.02f, 0.14f, 0.23f, 0.9f);
            _name = Label(panel.transform, font, "Tên", 20, 4f, 28f);
            _hp = Label(panel.transform, font, "HP", 16, 32f, 22f);
            _mp = Label(panel.transform, font, "MP", 16, 56f, 22f);

            var imageGo = new GameObject("Pet", typeof(RectTransform), typeof(RawImage));
            imageGo.transform.SetParent(transform, false);
            _petImage = imageGo.GetComponent<RawImage>();
            _petImage.raycastTarget = false;
            var imageRect = _petImage.rectTransform;
            imageRect.anchorMin = imageRect.anchorMax = new Vector2(0.5f, 0f);
            imageRect.pivot = new Vector2(0.5f, 0f);
            imageRect.anchoredPosition = new Vector2(0f, 16f - _pet.VerticalOffset * 2f);
            imageRect.sizeDelta = new Vector2(96f, 128f);
            imageRect.localScale = new Vector3(left ? 1f : -1f, 1f, 1f);
            Refresh();
        }

        private static Text Label(Transform parent, Font font, string name, int size,
            float top, float height)
        {
            var text = UiBuilder.MakeText(parent, font, name, size, false);
            UiBuilder.PlaceRow(text.rectTransform, top, height, 10f);
            text.alignment = TextAnchor.MiddleCenter;
            return text;
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

        private void Refresh()
        {
            _name.text = $"{_pet.Name}  Lv.{_pet.Level}";
            _hp.text = $"HP  {_pet.Hp}/{_pet.MaxHp}";
            _hp.color = _pet.Hp * 4 <= _pet.MaxHp ? new Color(1f, 0.35f, 0.3f) : Color.white;
            _mp.text = $"MP  {_pet.Mp}/{_pet.MaxMp}";
            _mp.color = new Color(0.45f, 0.75f, 1f);
        }
    }
}
