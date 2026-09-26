using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Tên pet kèm icon sao: server gửi "Rua (sao)(sao)(saoden)…" (đủ 5 tag), ta vẽ chữ
    /// "Rua" rồi 5 icon sao năm cánh vàng <c>Ui/Generated/star-gold</c> (sinh bằng
    /// <c>tools/image-gen/gen-gold-star-icon.py</c>). Theo yêu cầu thiết kế, MỌI tag sao
    /// (đạt lẫn chưa đạt) đều vẽ cùng icon vàng. Tên không có tag sao (vật phẩm) thì chỉ
    /// còn chữ.
    ///
    /// <para>Chữ + sao nằm trong <see cref="HorizontalLayoutGroup"/>: tên dài thì CHỈ chữ
    /// co lại (sao đặt minWidth cố định), sao luôn nằm sát ngay sau tên.</para>
    /// </summary>
    public sealed class StarNameLabel : MonoBehaviour
    {
        private const string StarPath = "Ui/Generated/star-gold";
        private const int MaxStars = 5;

        // Project tắt Domain Reload: sprite tạo lúc chạy bị huỷ khi thoát Play Mode nhưng
        // biến static còn giữ → luôn so null kiểu Unity, không dùng cờ "đã nạp".
        private static Sprite _star;

        private Text _label;
        private Image[] _stars;

        /// <summary>Chữ tên — caller tuỳ chỉnh màu/cỡ/wrap như Text thường.</summary>
        public Text Label => _label;

        public RectTransform Rect => (RectTransform)transform;

        public static StarNameLabel Create(Transform parent, Font font, int fontSize, float starSize)
        {
            var go = new GameObject("StarName", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 1f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var view = go.AddComponent<StarNameLabel>();
            view._label = UiBuilder.MakeText(go.transform, font, "Label", fontSize, false);
            view._label.raycastTarget = false;

            view._stars = new Image[MaxStars];
            for (var i = 0; i < MaxStars; i++)
            {
                var starGo = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                starGo.transform.SetParent(go.transform, false);
                var element = starGo.GetComponent<LayoutElement>();
                element.minWidth = element.preferredWidth = starSize;
                element.minHeight = element.preferredHeight = starSize;
                var image = starGo.GetComponent<Image>();
                image.preserveAspect = true;
                image.raycastTarget = false;
                starGo.SetActive(false);
                view._stars[i] = image;
            }
            return view;
        }

        /// <summary>Đặt tên thô từ server; <paramref name="suffix"/> nối sau chữ (vd " x3").</summary>
        public void SetName(string rawName, string suffix = null)
        {
            var name = JarIconTokens.SplitStars(rawName ?? string.Empty, out var filled, out var empty);
            _label.text = string.IsNullOrEmpty(suffix) ? name : name + suffix;

            var total = filled + empty;
            var sprite = total > 0 ? Star() : null;
            for (var i = 0; i < _stars.Length; i++)
            {
                _stars[i].sprite = sprite;
                _stars[i].gameObject.SetActive(sprite != null && i < total);
            }
        }

        /// <summary>Sprite sao vàng dùng chung (dòng menu cũng vẽ sao bằng nó).</summary>
        internal static Sprite Star()
        {
            if (_star != null) return _star;
            var tex = Resources.Load<Texture2D>(StarPath);
            if (tex == null) return null;
            _star = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
            return _star;
        }
    }
}
