using System;
using System.Collections.Generic;
using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    public sealed partial class InputDialogView
    {
        private void BuildActionButtons(float top)
        {
            ClearActionButtons();

            var row = new GameObject("Buttons", typeof(RectTransform));
            row.transform.SetParent(_panel, false);
            row.tag = "Untagged";
            UiBuilder.PlaceRow((RectTransform)row.transform, top, ButtonRowHeight, PanelPadding);

            MakeButton(row.transform, "Đồng ý", 0, () => Submit());
            MakeButton(row.transform, "Huỷ", 1, RaiseClosed);
        }

        private void ClearActionButtons()
        {
            var existing = _panel.Find("Buttons");
            if (existing != null) Destroy(existing.gameObject);
        }

        private void MakeButton(Transform parent, string label, int index, Action onClick)
        {
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f * index, 0f);
            rect.anchorMax = new Vector2(0.5f + 0.5f * index, 1f);
            rect.offsetMin = new Vector2(index == 0 ? 0f : ButtonGap * 0.5f, 0f);
            rect.offsetMax = new Vector2(index == 0 ? -ButtonGap * 0.5f : 0f, 0f);

            var image = go.GetComponent<Image>();
            image.color = index == 0
                ? UiBuilder.ButtonFace
                : new Color(0.86f, 0.88f, 0.92f, 1f);
            RoundedUiSprite.Apply(image);

            var text = UiBuilder.MakeText(go.transform, _font, "Label", 16, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = index == 0
                ? Color.white
                : new Color(0.14f, 0.18f, 0.25f, 1f);

            go.GetComponent<Button>().onClick.AddListener(() => onClick());
        }

        private void BuildCloseButton()
        {
            var go = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_panel, false);
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
                var label = UiBuilder.MakeText(go.transform, _font, "Label", 18, true);
                label.text = "×";
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                UiBuilder.SetFontStyle(label, FontStyle.Bold);
            }

            go.GetComponent<Button>().onClick.AddListener(RaiseClosed);
        }

        private void RaiseClosed()
        {
            if (_decided) return;
            _decided = true;
            Closed?.Invoke();
        }
    }
}
