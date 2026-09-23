using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Mấy mảnh dựng hình của lớp giao diện CŨ: nền tối phủ màn, panel xám đậm, dòng chữ
    /// và nút chữ nhật. Tông này khác hẳn <see cref="GamePopupFrame"/> (nền sáng, viền
    /// xanh) — màn nào còn dùng là còn lạc tông.
    ///
    /// <para>Trước đây nằm nhờ trong <c>LetterDetailView</c>. Hộp thư đã chuyển hẳn sang
    /// khung popup chung nên lớp đó bị xoá, nhưng <see cref="ChatHistoryView"/> và
    /// <see cref="TattooView"/> vẫn dùng mấy hàm này — tách ra đây để không phải giữ lại
    /// một MonoBehaviour rỗng chỉ vì vài hàm tĩnh.</para>
    ///
    /// <para><b>Đừng dùng cho màn mới.</b> Màn mới dựng bằng <see cref="GamePopupFrame"/>.</para>
    /// </summary>
    internal static class LegacyOverlayUi
    {
        public static GameObject Overlay(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, .65f);
            return go;
        }

        public static Transform Panel(Transform parent, Vector2 size)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(.1f, .14f, .2f, .98f);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            return go.transform;
        }

        public static Text Text(Transform parent, string name, string value, int size,
            float top, float height)
        {
            var text = UiBuilder.MakeText(parent, UiBuilder.DefaultFont(), name, size, false);
            text.text = value ?? string.Empty;
            UiBuilder.PlaceRow(text.rectTransform, top, height, 18f);
            text.color = UiBuilder.TextMain;
            return text;
        }

        public static void Button(Transform parent, string label, float left, Action action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(left, 18f);
            rect.sizeDelta = new Vector2(124f, 40f);
            go.GetComponent<Image>().color = UiBuilder.ButtonFace;
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            var text = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 14, true);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            go.GetComponent<Button>().onClick.AddListener(() => action());
        }
    }
}
