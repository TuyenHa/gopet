using System;
using Gopet.Net.Images;
using Gopet.Net.Kiosk;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Owner kiosk card with the real item/pet frame supplied by the server.</summary>
    public sealed class KioskListingView : MonoBehaviour
    {
        private RawImage _frame;
        private Text _details;
        private Text _primaryLabel;
        private KioskListing _listing;

        public event Action<bool> ActionChosen;
        public event Action CloseRequested;

        public static KioskListingView Create(Transform parent, Font font)
        {
            var root = new GameObject("KioskListingView", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, .58f);

            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(root.transform, false);
            var rect = (RectTransform)card.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(440f, 290f);
            RoundedUiSprite.Apply(card.GetComponent<Image>());
            card.GetComponent<Image>().color = UiBuilder.Panel;

            var view = root.AddComponent<KioskListingView>();
            view.Build(card.transform, font ?? UiBuilder.BuiltinFont());
            return view;
        }

        public void Bind(KioskListing listing, RemoteAssetCache assets)
        {
            _listing = listing ?? throw new ArgumentNullException(nameof(listing));
            _frame.gameObject.SetActive(listing.HasListing);
            _primaryLabel.text = listing.HasListing ? "Gỡ khỏi ki-ốt" : "Chọn vật phẩm";
            if (!listing.HasListing)
            {
                _details.text = "Ki-ốt đang trống. Chọn vật phẩm hoặc thú cưng để đăng bán.";
                return;
            }

            var mins = Math.Max(0, listing.RemainingSeconds) / 60;
            _details.text = $"{listing.Name}\n{listing.Description}\nCòn {mins} phút";
            if (assets == null || string.IsNullOrEmpty(listing.FrameImagePath)) return;
            assets.Get(listing.FrameImagePath, ImagePackets.TypeNpc, texture =>
            {
                if (this != null && _frame != null) _frame.texture = texture;
            });
        }

        private void Build(Transform card, Font font)
        {
            var title = UiBuilder.MakeText(card, font, "Title", 18, false);
            title.text = "Ki-ốt của bạn";
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(title.rectTransform, 10f, 28f, 14f);

            var image = new GameObject("Frame", typeof(RectTransform), typeof(RawImage));
            image.transform.SetParent(card, false);
            _frame = image.GetComponent<RawImage>();
            var ir = _frame.rectTransform;
            ir.anchorMin = ir.anchorMax = new Vector2(0f, 1f);
            ir.pivot = new Vector2(0f, 1f);
            ir.anchoredPosition = new Vector2(18f, -58f);
            ir.sizeDelta = new Vector2(118f, 118f);

            _details = UiBuilder.MakeText(card, font, "Details", 14, false);
            _details.alignment = TextAnchor.UpperLeft;
            _details.color = UiBuilder.TextMain;
            var dr = _details.rectTransform;
            dr.anchorMin = new Vector2(0f, 1f);
            dr.anchorMax = new Vector2(1f, 1f);
            dr.pivot = new Vector2(0f, 1f);
            dr.offsetMin = new Vector2(148f, -190f);
            dr.offsetMax = new Vector2(-18f, -58f);

            var primary = MakeButton(card, font, "Primary", new Vector2(-8f, 14f));
            _primaryLabel = primary.GetComponentInChildren<Text>();
            primary.onClick.AddListener(() => ActionChosen?.Invoke(_listing != null && _listing.HasListing));
            var close = MakeButton(card, font, "Close", new Vector2(122f, 14f));
            close.GetComponentInChildren<Text>().text = "Đóng";
            close.onClick.AddListener(() => CloseRequested?.Invoke());
        }

        private static Button MakeButton(Transform parent, Font font, string name, Vector2 anchored)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0f);
            rect.pivot = new Vector2(.5f, 0f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(120f, 34f);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            var label = UiBuilder.MakeText(go.transform, font, "Label", 13, true);
            label.text = "Chọn vật phẩm";
            label.alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }
    }
}
