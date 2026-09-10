using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Hiển thị image dialog của server; captcha dùng view này trước form nhập mã.</summary>
    public sealed class ImageDialogView : MonoBehaviour
    {
        private RawImage _image;
        private Text _hint;
        private Button _continue;
        private bool _confirmed;

        public event Action Confirmed;

        public string ImagePath { get; private set; }
        public RawImage Image => _image;
        public Button ContinueButton => _continue;

        public static ImageDialogView Create(Transform parent, Font font)
        {
            var go = new GameObject("ImageDialogView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.94f);

            var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(go.transform, false);
            var cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(360f, 240f);
            card.GetComponent<Image>().color = UiBuilder.Panel;

            var view = go.AddComponent<ImageDialogView>();
            view.Build(card.transform, font);
            return view;
        }

        public void Bind(ImageDialogSpec spec, RemoteAssetCache assets)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));

            ImagePath = spec.ImagePath;
            _confirmed = false;
            _hint.text = "Chạm Tiếp tục rồi nhập mã trong ảnh";

            var rect = _image.rectTransform;
            var scale = Mathf.Min(300f / spec.Width, 110f / spec.Height, 1f);
            rect.sizeDelta = new Vector2(spec.Width * scale, spec.Height * scale);

            if (assets == null) return;
            assets.Get(spec.ImagePath, ImagePackets.TypeIcon, texture =>
            {
                if (this != null && _image != null) _image.texture = texture;
            });
        }

        public void Confirm()
        {
            if (_confirmed) return;
            _confirmed = true;
            Confirmed?.Invoke();
        }

        private void Build(Transform card, Font font)
        {
            var title = UiBuilder.MakeText(card, font, "Title", 19, false);
            title.text = "Xác nhận bảo mật";
            title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(title.rectTransform, 14f, 28f, 18f);

            var imageGo = new GameObject("Captcha", typeof(RectTransform), typeof(RawImage));
            imageGo.transform.SetParent(card, false);
            _image = imageGo.GetComponent<RawImage>();
            _image.color = Color.white;
            var imageRect = _image.rectTransform;
            imageRect.anchorMin = imageRect.anchorMax = new Vector2(0.5f, 1f);
            imageRect.pivot = new Vector2(0.5f, 1f);
            imageRect.anchoredPosition = new Vector2(0f, -52f);

            _hint = UiBuilder.MakeText(card, font, "Hint", 14, false);
            _hint.alignment = TextAnchor.MiddleCenter;
            _hint.color = UiBuilder.TextMuted;
            UiBuilder.PlaceRow(_hint.rectTransform, 166f, 22f, 14f);

            var buttonGo = new GameObject("Continue", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(card, false);
            UiBuilder.PlaceRow((RectTransform)buttonGo.transform, 194f, 34f, 90f);
            buttonGo.GetComponent<Image>().color = UiBuilder.ButtonFace;
            _continue = buttonGo.GetComponent<Button>();
            _continue.onClick.AddListener(Confirm);

            var label = UiBuilder.MakeText(buttonGo.transform, font, "Label", 16, true);
            label.text = "Tiếp tục";
            label.alignment = TextAnchor.MiddleCenter;
        }
    }
}
