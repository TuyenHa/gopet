using System;
using Gopet.Net.Guider;
using Gopet.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Popup riêng hiển thị bảng TOP LVL bang hội.</summary>
    public sealed class GuildTopPopupView : MonoBehaviour
    {
        private const float Width = 380f;
        private const float Height = 320f;
        private const float Padding = 8f;
        private const float TitleHeight = 30f;

        private static readonly Color PanelBg = new Color(0.96f, 0.98f, 1f, 1f);
        private static readonly Color PanelBorder = new Color(0.28f, 0.6f, 1f, 1f);
        private static readonly Color TextDark = new Color(0.14f, 0.18f, 0.25f, 1f);

        private GenericMenuView _menuView;
        public event Action Closed;

        public static GuildTopPopupView Create(Transform parent, Font font)
        {
            var root = new GameObject("GuildTopPopup", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Width, Height);

            var image = root.GetComponent<Image>();
            RoundedUiSprite.Apply(image);
            image.color = PanelBg;
            var outline = root.AddComponent<Outline>();
            outline.effectColor = PanelBorder;
            outline.effectDistance = new Vector2(2f, 2f);

            var view = root.AddComponent<GuildTopPopupView>();
            var title = UiBuilder.MakeText(root.transform, font, "Title", 18, true);
            title.text = "TOP LVL Bang hội";
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            title.color = TextDark;
            var titleRect = title.rectTransform;
            titleRect.offsetMin = new Vector2(Padding, -(Padding + TitleHeight));
            titleRect.offsetMax = new Vector2(-Padding, -Padding);

            var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
            body.transform.SetParent(root.transform, false);
            var bodyRect = (RectTransform)body.transform;
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(Padding, Padding);
            bodyRect.offsetMax = new Vector2(-Padding, -(Padding + TitleHeight));
            RoundedUiSprite.Apply(body.GetComponent<Image>());
            body.GetComponent<Image>().color = new Color(0.995f, 1f, 1f, 1f);

            view._menuView = GenericMenuView.Create(body.transform, font);
            view._menuView.SetLightCards(true);
            view._menuView.EnableEmbeddedScroll(Height - TitleHeight - Padding * 2f);
            view.BuildClose(root.transform, font);
            return view;
        }

        public bool TryConsumeMenu(MenuScreen screen, RemoteAssetCache assets, GuiderHandler guider)
        {
            if (screen == null || screen.ListId != -1) return false;
            _menuView.Bind(screen, assets, guider);
            _menuView.gameObject.SetActive(true);
            return true;
        }

        private void BuildClose(Transform parent, Font font)
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(4f, 4f);
            rect.sizeDelta = new Vector2(34f, 34f);

            var image = go.GetComponent<Image>();
            image.preserveAspect = true;
            var sprite = HudSkin.Get(HudSkin.Close);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }
            else
            {
                RoundedUiSprite.Apply(image);
                image.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                var label = UiBuilder.MakeText(go.transform, font, "Label", 18, true);
                label.text = "×";
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.fontStyle = FontStyle.Bold;
            }
            go.GetComponent<Button>().onClick.AddListener(() => Closed?.Invoke());
        }
    }
}
