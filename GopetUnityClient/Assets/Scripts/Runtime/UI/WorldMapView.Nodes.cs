using Gopet.Net.Map;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Một điểm map trên tranh: chấm pin + thẻ tên, bọc trong một vùng chạm rộng bấm
    /// được, đổi thành ổ khoá khi map chưa mở. Cố ý KHÔNG dùng ảnh map thu nhỏ —
    /// thumbnail to che mất tranh nền.
    /// </summary>
    public sealed partial class WorldMapView
    {
        /// <summary>
        /// Vùng chạm của một thành phố. Tên ngắn như "Ải" chỉ cho thẻ rộng ~28px cao
        /// 20px — bấm bằng ngón tay là trượt, nên vùng bấm trùm cả pin lẫn thẻ tên rồi
        /// cộng thêm lề.
        ///
        /// <para>Giãn nhãn dùng chính kích thước này nên hai thành phố sát nhau không
        /// giành mất cú chạm của nhau.</para>
        /// </summary>
        private const float MinTouchWidth = 64f;

        /// <summary>Khe giữa mũi pin và thẻ tên — pin dính thẻ thì trông như một cục.</summary>
        private const float IconGap = 5f;

        /// <summary>Khối nhìn thấy: pin, khe, rồi thẻ tên.</summary>
        private const float BlockHeight = PinHeight + IconGap + LabelHeight;
        private const float NodeHeight = BlockHeight + 6f;

        /// <summary>
        /// Mũi pin và mép trên thẻ tên, tính sao cho cả khối nằm chính giữa vùng chạm —
        /// lệch một chút là thẻ tên thò ra ngoài và mất phần bấm được.
        /// </summary>
        private const float PinBottomY = BlockHeight * 0.5f - PinHeight;
        private const float LabelTopY = PinBottomY - IconGap;

        private void MakeNode(MapTeleportOption option, WorldMapLayout.Node placement, bool isCurrent)
        {
            var go = new GameObject($"Map{option.MapId}", typeof(RectTransform));
            go.transform.SetParent(_nodeLayer, false);
            var rect = (RectTransform)go.transform;
            // Neo theo TỈ LỆ tranh: tranh co giãn/bị cắt thế nào thì pin vẫn dính đúng chỗ.
            rect.anchorMin = rect.anchorMax = new Vector2(placement.X, placement.Y);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            MakePin(go.transform, option.Locked, isCurrent);
            var pill = MakeLabelPill(go.transform, option, isCurrent);
            // Vùng chiếm chỗ của node = vùng chạm (rộng hơn cả thẻ tên lẫn chấm pin).
            rect.sizeDelta = new Vector2(
                Mathf.Max(pill.sizeDelta.x, MinTouchWidth), NodeHeight);
            MakeHitArea(go.transform, pill.GetComponent<Graphic>(), option, isCurrent);
            RegisterPlaced(rect);
        }

        /// <summary>
        /// Thẻ tên có nền mờ — chữ trần trên tranh nhiều màu đọc rất mệt. Chỉ là phần
        /// NHÌN: cú bấm do <see cref="MakeHitArea"/> nhận.
        /// </summary>
        private RectTransform MakeLabelPill(Transform parent, MapTeleportOption option, bool isCurrent)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, LabelTopY);

            var pill = go.GetComponent<Image>();
            pill.color = option.Locked ? LabelLockedBg : LabelBg;
            pill.raycastTarget = false;
            RoundedUiSprite.Apply(pill);
            if (isCurrent)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = PinCurrent;
                outline.effectDistance = new Vector2(1.5f, -1.5f);
            }

            var text = UiBuilder.MakeText(go.transform, _font, "Text", 12, true);
            // Server là nguồn tên chính; MapDisplayNames chỉ là lưới đỡ khi server gửi rỗng.
            text.text = string.IsNullOrEmpty(option.Name) ? MapDisplayNames.Get(option.MapId) : option.Name;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = option.Locked ? LabelLockedText : LabelText;
            text.fontStyle = FontStyle.Bold;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            // Thẻ ôm vừa chữ: tên map dài ngắn khác nhau nhiều, thẻ cố định sẽ hoặc thừa
            // chỗ hoặc cắt chữ.
            rect.sizeDelta = new Vector2(
                Mathf.Min(LabelMaxWidth, text.preferredWidth + 14f), LabelHeight);
            return rect;
        }

        /// <summary>
        /// Vùng chạm vô hình phủ kín node — con CUỐI nên nằm trên cùng khi raycast.
        /// Ảnh alpha 0 vẫn nhận cú chạm (ngưỡng alpha hit-test mặc định là 0), nên
        /// người chơi bấm được cả khoảng trống quanh pin chứ không phải nhắm đúng thẻ tên.
        /// </summary>
        private void MakeHitArea(Transform parent, Graphic highlight, MapTeleportOption option,
            bool isCurrent)
        {
            var go = new GameObject("Hit", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)go.transform);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

            // Bấm vào đâu trong vùng cũng thấy thẻ tên tối đi — phản hồi duy nhất cho
            // biết cú chạm đã ăn, vì vùng chạm tự nó vô hình.
            var button = go.GetComponent<Button>();
            button.targetGraphic = highlight;

            var mapId = option.MapId;
            var reason = option.LockReason;
            var locked = option.Locked;
            button.onClick.AddListener(() =>
            {
                if (locked) LockedChosen?.Invoke(reason);
                else if (isCurrent) LockedChosen?.Invoke("Bạn đang ở bản đồ này.");
                else Choose(mapId);
            });
        }
    }
}
