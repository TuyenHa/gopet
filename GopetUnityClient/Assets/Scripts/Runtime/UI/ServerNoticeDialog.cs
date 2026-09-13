using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Popup một nút dùng cho thông báo thành công/lỗi từ server.</summary>
    public sealed class ServerNoticeDialog : MonoBehaviour
    {
        public event Action Closed;

        public static ServerNoticeDialog Create(Transform parent, string message, bool error)
        {
            var backdrop = new GameObject("Server notice backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

            var view = backdrop.AddComponent<ServerNoticeDialog>();
            backdrop.GetComponent<Button>().onClick.AddListener(view.Close);

            var panel = new GameObject("Server notice panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(backdrop.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(420f, 190f);

            var panelImage = panel.GetComponent<Image>();
            RoundedUiSprite.Apply(panelImage);
            panelImage.color = new Color(0.96f, 0.98f, 1f, 1f);
            var outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.6f, 1f, 1f);
            outline.effectDistance = new Vector2(2f, -2f);

            var text = UiBuilder.MakeText(panel.transform, UiBuilder.BuiltinFont(), "Message", 15, false);
            text.text = message;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            var textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(28f, 58f);
            textRect.offsetMax = new Vector2(-28f, -36f);

            var ok = new GameObject("OK", typeof(RectTransform), typeof(Image), typeof(Button));
            ok.transform.SetParent(panel.transform, false);
            var okRect = (RectTransform)ok.transform;
            okRect.anchorMin = new Vector2(0.5f, 0f);
            okRect.anchorMax = new Vector2(0.5f, 0f);
            okRect.pivot = new Vector2(0.5f, 0f);
            okRect.sizeDelta = new Vector2(150f, 38f);
            okRect.anchoredPosition = new Vector2(0f, 12f);
            var okImage = ok.GetComponent<Image>();
            RoundedUiSprite.Apply(okImage);
            okImage.color = error
                ? new Color(0.78f, 0.28f, 0.28f, 1f)
                : UiBuilder.ButtonFace;
            var okText = UiBuilder.MakeText(ok.transform, UiBuilder.BuiltinFont(), "Label", 14, true);
            okText.text = "OK";
            okText.alignment = TextAnchor.MiddleCenter;
            okText.fontStyle = FontStyle.Bold;
            okText.color = Color.white;
            ok.GetComponent<Button>().onClick.AddListener(view.Close);

            var close = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            close.transform.SetParent(panel.transform, false);
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.sizeDelta = new Vector2(34f, 34f);
            closeRect.anchoredPosition = new Vector2(-18f, -18f);
            var closeImage = close.GetComponent<Image>();
            var closeSprite = HudSkin.Get(HudSkin.Close);
            if (closeSprite != null) closeImage.sprite = closeSprite;
            else
            {
                closeImage.color = new Color(0.86f, 0.28f, 0.28f, 1f);
                RoundedUiSprite.Apply(closeImage);
            }
            var closeText = UiBuilder.MakeText(close.transform, UiBuilder.BuiltinFont(), "X", 18, true);
            closeText.text = "×";
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.color = Color.white;
            close.GetComponent<Button>().onClick.AddListener(view.Close);
            return view;
        }

        private void Close() => Closed?.Invoke();
    }
}
