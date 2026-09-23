using System;
using Gopet.Net.Guider;
using Gopet.Net.Images;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Rich dialog dạng label + ảnh động do server mô tả.</summary>
    public sealed class AnimationMenuView : MonoBehaviour
    {
        public event Action<AnimationMenuCommand> CommandSelected;

        public static AnimationMenuView Create(Transform parent, Font font,
            AnimationMenuScreen screen, RemoteAssetCache assets)
        {
            var backdrop = new GameObject("AnimationMenuView", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500f, 480f);
            RoundedUiSprite.Apply(panel.GetComponent<Image>());
            panel.GetComponent<Image>().color = UiBuilder.Panel;

            var view = backdrop.AddComponent<AnimationMenuView>();
            view.Build(panel.transform, font ?? UiBuilder.DefaultFont(), screen, assets);
            return view;
        }

        private void Build(Transform panel, Font font, AnimationMenuScreen screen, RemoteAssetCache assets)
        {
            var title = UiBuilder.MakeText(panel, font, "Title", 18, false);
            title.text = screen.Title;
            UiBuilder.SetFontStyle(title, FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            UiBuilder.PlaceRow(title.rectTransform, 8f, 36f, 16f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(panel, false);
            var viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(12f, 56f);
            viewportRect.offsetMax = new Vector2(-12f, -48f);
            viewport.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.11f, 0.75f);
            viewport.GetComponent<Mask>().showMaskGraphic = true;

            var content = new GameObject("Content", typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = Vector2.zero;
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewport.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;

            foreach (var element in screen.Elements)
            {
                if (element.IsImage) AddImage(content.transform, element, assets);
                else AddLabel(content.transform, font, element);
            }
            AddCommands(panel, font, screen.Commands);
        }

        private static void AddLabel(Transform parent, Font font, AnimationMenuElement element)
        {
            var text = UiBuilder.MakeText(parent, font, "Label", element.FontStyle == 3 ? 18 : 13, false);
            text.text = element.TextOrPath;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var layout = text.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = element.FontStyle == 3 ? 32f : 24f;
            layout.preferredHeight = layout.minHeight;
        }

        private static void AddImage(Transform parent, AnimationMenuElement element, RemoteAssetCache assets)
        {
            var go = new GameObject("Image", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 110f;
            var image = go.GetComponent<RawImage>();
            image.color = Color.white;
            if (element.FrameCount > 1)
                image.uvRect = new Rect(0f, 0f, 1f / element.FrameCount, 1f);
            if (assets != null)
                assets.Get(element.TextOrPath, ImagePackets.TypeIcon, texture =>
                {
                    if (image != null) image.texture = texture;
                });
        }

        private void AddCommands(Transform panel, Font font, AnimationMenuCommand[] commands)
        {
            if (commands.Length == 0) return;
            var width = 460f / commands.Length;
            for (var i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                var go = new GameObject($"Command:{command.Id}", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(panel, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(12f + i * width, 10f);
                rect.sizeDelta = new Vector2(width - 6f, 36f);
                go.GetComponent<Image>().color = UiBuilder.ButtonFace;
                var label = UiBuilder.MakeText(go.transform, font, "Label", 14, true);
                label.text = command.Name;
                label.alignment = TextAnchor.MiddleCenter;
                go.GetComponent<Button>().onClick.AddListener(() => CommandSelected?.Invoke(command));
            }
        }
    }
}
