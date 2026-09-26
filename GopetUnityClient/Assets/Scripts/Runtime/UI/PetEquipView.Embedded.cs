using Gopet.Net.Images;
using Gopet.Net.Pet;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class PetEquipView
    {
        private Image _embeddedPortrait;

        public static PetEquipView CreateEmbedded(Transform parent, RemoteAssetCache assets)
        {
            var go = new GameObject("Embedded Pet Equip", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            var view = go.AddComponent<PetEquipView>();
            view._assets = assets;
            view.BuildEmbeddedContent();
            return view;
        }

        private void BuildEmbeddedContent()
        {
            var font = UiBuilder.DefaultFont();
            _headerName = UiBuilder.MakeText(transform, font, "Header", 15, false);
            UiBuilder.SetFontStyle(_headerName, FontStyle.Bold);
            _headerName.alignment = TextAnchor.MiddleCenter;
            _headerName.color = new Color(0.14f, 0.24f, 0.44f, 1f);
            HeaderRect(_headerName.rectTransform, 2f, 22f);

            _headerStats = UiBuilder.MakeText(transform, font, "Stats", 11, false);
            _headerStats.alignment = TextAnchor.MiddleCenter;
            _headerStats.color = new Color(0.25f, 0.36f, 0.52f, 1f);
            HeaderRect(_headerStats.rectTransform, 23f, 18f);

            var portraitGo = new GameObject("Pet Portrait", typeof(RectTransform), typeof(Image));
            portraitGo.transform.SetParent(transform, false);
            var portraitRect = (RectTransform)portraitGo.transform;
            portraitRect.anchorMin = portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
            portraitRect.pivot = new Vector2(0.5f, 0.5f);
            portraitRect.anchoredPosition = new Vector2(0f, 8f);
            portraitRect.sizeDelta = new Vector2(130f, 145f);
            _embeddedPortrait = portraitGo.GetComponent<Image>();
            _embeddedPortrait.preserveAspect = true;
            _embeddedPortrait.color = new Color(1f, 1f, 1f, 0.12f);

            AddEmbeddedSlot(EquipSlot.Hat, "Nón", 10f, 50f);
            AddEmbeddedSlot(EquipSlot.Weapon, "Vũ khí", 10f, 130f);
            AddEmbeddedSlot(EquipSlot.Armor, "Giáp", 326f, 50f);
            AddEmbeddedSlot(EquipSlot.Glove, "Bao tay", 326f, 130f);
            AddEmbeddedSlot(EquipSlot.Boot, "Giày", 0f, 0f);
            // Giày neo GIỮA MÉP DƯỚI thay vì toạ độ cứng từ đỉnh: panel thấp hơn 294 là ô
            // tràn ra ngoài popup. Host đã thụt 4 so với panel "Trang bị pet" → +1 = cách 5.
            var boot = (RectTransform)_slots[EquipSlot.Boot].transform;
            boot.anchorMin = boot.anchorMax = new Vector2(0.5f, 0f);
            boot.pivot = new Vector2(0.5f, 0f);
            boot.anchoredPosition = new Vector2(0f, 1f);

            var hidden = MakeEmbeddedButton("Kích ẩn", 1f, 8f);
            hidden.onClick.AddListener(() => HiddenStatsRequested?.Invoke());
        }

        private void AddEmbeddedSlot(EquipSlot slot, string label, float x, float y)
        {
            var view = PetEquipSlot.Create(transform, slot, label, new Vector2(x, -y), _assets);
            view.Clicked += () => OnSlotTap(view);
            _slots[slot] = view;
        }

        private Button MakeEmbeddedButton(string label, float anchorX, float offsetX)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(anchorX, 0f);
            rect.pivot = new Vector2(anchorX, 0f);
            rect.anchoredPosition = new Vector2(anchorX == 1f ? -offsetX : offsetX, 6f);
            rect.sizeDelta = new Vector2(80f, 28f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 11, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }

        private void LoadEmbeddedPortrait(PetEquipInfo info)
        {
            if (_embeddedPortrait == null || _assets == null || string.IsNullOrEmpty(info.FrameImage)) return;
            _assets.Get(info.FrameImage, ImagePackets.TypeNpc, _embeddedPortrait, texture =>
            {
                if (texture == null || _embeddedPortrait == null) return;
                var frameCount = Mathf.Max(1, info.FrameNumber);
                var width = Mathf.Max(1, texture.width / frameCount);
                _embeddedPortrait.sprite = Sprite.Create(texture,
                    new Rect(0f, 0f, width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                _embeddedPortrait.color = Color.white;
            });
        }
    }
}
