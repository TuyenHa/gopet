using System;
using Gopet.Net.Guider;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Nhiệm vụ đầu tiên nằm ngay dưới khung HP/MP: tên nhiệm vụ + tiến độ từng yêu cầu
    /// ("Tiêu diệt Khủng long 3 / 10 (tại ...)"), để người chơi khỏi phải bấm mở popup.
    ///
    /// <para>Tiến độ là mô tả server gửi trong menu 1034 (client &gt;= 1.5.0 nhận mỗi yêu
    /// cầu một hàng — <c>MenuController.sendMenu.cs</c>). Khung giãn theo số hàng và báo
    /// <see cref="HeightChanged"/> để chip EXP bên dưới dời theo.</para>
    /// </summary>
    public sealed class TaskTrackerWidget : MonoBehaviour
    {
        /// <summary>Khe giữa HUD nhân vật và dòng nhiệm vụ.</summary>
        public const float Gap = 5f;
        /// <summary>Chiều cao khi chỉ có một dòng tên (chưa có nhiệm vụ / không có tiến độ).</summary>
        public const float Height = 34f;

        /// <summary>Bằng đúng khung HUD nhân vật (avatar + HP/MP) ngay phía trên.</summary>
        private const float Width = World.CharacterHud.PanelWidth;
        private const float PadLeft = 10f;
        private const float PadRight = 8f;
        private const float PadV = 6f;
        private const float TitleHeight = 18f;

        private RectTransform _rect;
        private Text _label;
        private Text _progress;

        public event Action Clicked;
        /// <summary>Chiều cao mới của khung, mỗi khi đổi.</summary>
        public event Action<float> HeightChanged;

        public float CurrentHeight { get; private set; } = Height;

        public static TaskTrackerWidget Create(Transform parent)
        {
            var go = new GameObject("Task Tracker", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            // Cách mép dưới HUD nhân vật đúng Gap.
            rect.anchoredPosition = new Vector2(0f, -(World.CharacterHud.PanelHeight + Gap));
            rect.sizeDelta = new Vector2(Width, Height);
            RoundedUiSprite.Apply(go.GetComponent<Image>());
            go.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.88f);

            var view = go.AddComponent<TaskTrackerWidget>();
            view._rect = rect;
            view._label = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Label", 12, true);
            view._label.color = Color.white;
            view._label.horizontalOverflow = HorizontalWrapMode.Overflow;

            view._progress = UiBuilder.MakeText(go.transform, UiBuilder.DefaultFont(), "Progress", 11, false);
            view._progress.alignment = TextAnchor.UpperLeft;
            view._progress.color = new Color(1f, 0.86f, 0.45f, 1f);
            // Rich text chỉ để tô hàng đã đạt; TaskProgressText đã thay '<' '>' của dữ liệu server.
            view._progress.supportRichText = true;
            view._progress.horizontalOverflow = HorizontalWrapMode.Wrap;
            view._progress.verticalOverflow = VerticalWrapMode.Overflow;
            view._progress.raycastTarget = false;
            var progressRect = view._progress.rectTransform;
            progressRect.anchorMin = new Vector2(0f, 0f);
            progressRect.anchorMax = new Vector2(1f, 1f);
            progressRect.offsetMin = new Vector2(PadLeft, PadV);
            progressRect.offsetMax = new Vector2(-PadRight, -(PadV + TitleHeight));

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
            else Show("Nhiệm vụ: " + (first.Title ?? "Chưa đặt tên"), first.Description);
        }

        private void SetEmpty() => Show("Nhiệm vụ: Chưa có nhiệm vụ", null);

        private void Show(string title, string progress)
        {
            _label.text = title;
            _progress.text = TaskProgressText.Colorize(progress?.Trim());
            var hasProgress = _progress.text.Length > 0;
            _progress.gameObject.SetActive(hasProgress);

            var labelRect = _label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, hasProgress ? 1f : 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = new Vector2(PadLeft, hasProgress ? -(PadV + TitleHeight) : 0f);
            labelRect.offsetMax = new Vector2(-PadRight, hasProgress ? -PadV : 0f);
            _label.alignment = hasProgress ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;

            var height = hasProgress
                ? PadV + TitleHeight + MeasureProgressHeight() + PadV
                : Height;
            height = Mathf.Max(Height, Mathf.Ceil(height));
            _rect.sizeDelta = new Vector2(Width, height);
            if (Mathf.Approximately(height, CurrentHeight)) return;
            CurrentHeight = height;
            HeightChanged?.Invoke(height);
        }

        /// <summary>Đo theo bề ngang cố định của khung — một yêu cầu nhiều map có thể tự
        /// xuống hàng, đếm '\n' là thiếu.</summary>
        private float MeasureProgressHeight()
        {
            var width = Width - PadLeft - PadRight;
            var settings = _progress.GetGenerationSettings(new Vector2(width, 0f));
            return _progress.cachedTextGeneratorForLayout.GetPreferredHeight(_progress.text, settings)
                / _progress.pixelsPerUnit;
        }
    }
}
