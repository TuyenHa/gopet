using System;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Phần khung dùng chung của các popup trong màn đấu (kỹ năng, khung cảnh).</summary>
    public static class BattlePopupChrome
    {
        /// <summary>Nền xanh đậm bo góc + viền sáng, phủ lên <paramref name="go"/> (cần có Image).</summary>
        public static void ApplyPanel(GameObject go)
        {
            var bg = go.GetComponent<Image>();
            bg.sprite = PanelSprites.Rounded(8);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.09f, 0.14f, 0.24f, 0.97f);

            var borderGo = new GameObject("Viền", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(go.transform, false);
            UiBuilder.Stretch((RectTransform)borderGo.transform);
            var border = borderGo.GetComponent<Image>();
            border.sprite = PanelSprites.Rounded(8, 2);
            border.type = Image.Type.Sliced;
            border.color = new Color(0.62f, 0.72f, 0.88f, 0.9f);
            border.raycastTarget = false;
        }

        /// <summary>Tiêu đề vàng góc trên-trái + nút ✕ góc trên-phải.</summary>
        public static void AddTitle(Transform parent, Font font, string title, int fontSize,
            float height, float pad, Action close)
        {
            var label = UiBuilder.MakeText(parent, font, "Tiêu đề", fontSize, false);
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(pad + 4f, -height);
            rect.offsetMax = new Vector2(-(height + pad), -pad);
            label.text = title;
            label.alignment = TextAnchor.MiddleLeft;
            UiBuilder.SetFontStyle(label, FontStyle.Bold);
            label.color = new Color(1f, 0.85f, 0.35f);

            var btnGo = new GameObject("Đóng", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);
            var bRect = (RectTransform)btnGo.transform;
            bRect.anchorMin = bRect.anchorMax = new Vector2(1f, 1f);
            bRect.pivot = new Vector2(1f, 1f);
            bRect.anchoredPosition = new Vector2(-pad, -pad);
            var side = Mathf.Max(18f, height - pad * 2f);
            bRect.sizeDelta = new Vector2(side, side);
            var img = btnGo.GetComponent<Image>();
            img.sprite = PanelSprites.Rounded(4, 1);
            img.type = Image.Type.Sliced;
            img.color = new Color(0.75f, 0.80f, 0.90f, 0.85f);
            var x = UiBuilder.MakeText(btnGo.transform, font, "X", fontSize, true);
            x.text = "✕";
            x.alignment = TextAnchor.MiddleCenter;
            UiBuilder.SetFontStyle(x, FontStyle.Bold);
            x.color = Color.white;
            btnGo.GetComponent<Button>().onClick.AddListener(() => close());
        }

        /// <summary>Vùng danh sách cuộn dọc nằm ngay dưới tiêu đề cao <paramref name="top"/>.
        /// Trả về transform nội dung để thêm dòng (mỗi dòng cần LayoutElement).</summary>
        public static Transform AddScrollList(Transform parent, float top, float listHeight, float pad,
            float rowGap)
        {
            var viewGo = new GameObject("Khung cuộn", typeof(RectTransform), typeof(Image),
                typeof(Mask), typeof(ScrollRect));
            viewGo.transform.SetParent(parent, false);
            var vRect = (RectTransform)viewGo.transform;
            vRect.anchorMin = new Vector2(0f, 1f);
            vRect.anchorMax = new Vector2(1f, 1f);
            vRect.pivot = new Vector2(0.5f, 1f);
            vRect.offsetMin = new Vector2(pad, -(top + listHeight));
            vRect.offsetMax = new Vector2(-pad, -top);

            var maskImg = viewGo.GetComponent<Image>();
            maskImg.sprite = PanelSprites.Rounded(6);
            maskImg.type = Image.Type.Sliced;
            viewGo.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Nội dung", typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewGo.transform, false);
            var cRect = (RectTransform)contentGo.transform;
            cRect.anchorMin = new Vector2(0f, 1f);
            cRect.anchorMax = new Vector2(1f, 1f);
            cRect.pivot = new Vector2(0.5f, 1f);
            cRect.offsetMin = cRect.offsetMax = Vector2.zero;

            var layout = contentGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = rowGap;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewGo.GetComponent<ScrollRect>();
            scroll.content = cRect;
            scroll.viewport = vRect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 16f;
            return contentGo.transform;
        }
    }
}
