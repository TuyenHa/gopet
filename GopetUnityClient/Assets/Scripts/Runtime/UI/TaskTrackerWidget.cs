using System;
using Gopet.Net.Guider;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>Dòng nhiệm vụ đầu tiên nằm ngay dưới khung HP/MP.</summary>
    public sealed class TaskTrackerWidget : MonoBehaviour
    {
        private Text _label;
        public event Action Clicked;

        public static TaskTrackerWidget Create(Transform parent)
        {
            var go = new GameObject("Task Tracker", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(0f, -94f);
            rect.sizeDelta = new Vector2(240f, 34f);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.88f);

            var view = go.AddComponent<TaskTrackerWidget>();
            view._label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 12, true);
            view._label.alignment = TextAnchor.MiddleLeft;
            view._label.color = Color.white;
            view._label.rectTransform.offsetMin = new Vector2(10f, 0f);
            view._label.rectTransform.offsetMax = new Vector2(-8f, 0f);
            view.SetEmpty();
            go.GetComponent<Button>().onClick.AddListener(() => view.Clicked?.Invoke());
            return view;
        }

        public void SetFirstTask(MenuScreen screen)
        {
            MenuItemInfo first = null;
            if (screen?.Items != null)
                for (var i = 0; i < screen.Items.Length; i++)
                    if (screen.Items[i] != null && screen.Items[i].CanSelect)
                    {
                        first = screen.Items[i];
                        break;
                    }

            if (first == null) SetEmpty();
            else _label.text = "Nhiệm vụ: " + (first.Title ?? "Chưa đặt tên");
        }

        private void SetEmpty() => _label.text = "Nhiệm vụ: Chưa có nhiệm vụ";
    }
}
