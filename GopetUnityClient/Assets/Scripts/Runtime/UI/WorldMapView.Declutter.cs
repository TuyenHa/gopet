using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Đẩy các nhãn map ra khỏi nhau sau khi dựng xong.
    ///
    /// <para>Bảng toạ độ trong <see cref="UiLogic.WorldMapLayout"/> chỉ biết map nằm ở
    /// vùng địa hình nào, KHÔNG biết tên map dài bao nhiêu — mà tên server gửi xuống dài
    /// ngắn rất khác nhau ("Ải" so với "Chốt chặn cuối cùng"). Xếp lưới cứng là mấy tên
    /// dài chồng lên nhau thành một đống chữ không đọc được.</para>
    ///
    /// <para>Nên sau khi dựng, mỗi nhãn được đẩy khỏi nhãn chạm nó theo trục ít chồng
    /// nhất (tách AABB kinh điển), lặp vài vòng rồi kẹp lại trong khung tranh. Nhãn chỉ
    /// xê dịch quanh chỗ cũ nên vẫn nằm trong vùng địa hình của nó.</para>
    ///
    /// <para>Chạy trong <c>Update</c> chứ không ngay trong <c>Bind</c>: lúc bind,
    /// RectTransform chưa qua layout nên kích thước tranh còn bằng 0.</para>
    /// </summary>
    public sealed partial class WorldMapView
    {
        /// <summary>Đủ để gỡ các đống chữ thường gặp; nhiều hơn chỉ tốn công vô ích.</summary>
        private const int DeclutterPasses = 24;

        /// <summary>Khe hở tối thiểu giữa hai nhãn, pixel.</summary>
        private const float DeclutterGap = 6f;

        private readonly List<RectTransform> _placed = new List<RectTransform>();
        private bool _needsDeclutter;

        private void RegisterPlaced(RectTransform rect) => _placed.Add(rect);

        private void TryDeclutter()
        {
            if (!_needsDeclutter || _picture == null) return;
            var size = _picture.rect.size;
            if (size.x <= 0f || size.y <= 0f) return;
            _needsDeclutter = false;
            Declutter(size, VisibleArea(size));
        }

        /// <summary>
        /// Phần tranh THỰC SỰ nhìn thấy, toạ độ pixel gốc ở tâm tranh. Tranh phủ kín màn
        /// nên phần thừa bị cắt — nhãn nằm trong đó thì mất hút, bấm cũng không tới.
        /// Tranh ghim mép trên nên vùng cắt nằm ở đáy.
        /// </summary>
        private Rect VisibleArea(Vector2 size)
        {
            var screen = ((RectTransform)transform).rect.size;
            if (screen.x <= 0f || screen.y <= 0f)
                return new Rect(-size.x * 0.5f, -size.y * 0.5f, size.x, size.y);
            var halfWidth = Mathf.Min(size.x, screen.x) * 0.5f;
            var top = size.y * 0.5f;
            var bottom = Mathf.Max(-top, top - screen.y);
            return Rect.MinMaxRect(-halfWidth, bottom, halfWidth, top);
        }

        private void Declutter(Vector2 size, Rect visible)
        {
            var count = _placed.Count;
            if (count < 2) return;

            // Quy về pixel, gốc ở tâm tranh: tính chồng lấn bằng tỉ lệ 0..1 sẽ sai vì
            // tranh không vuông — cùng một khoảng cách tỉ lệ ngang và dọc dài khác nhau.
            var centers = new Vector2[count];
            var halves = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                var anchor = _placed[i].anchorMin;
                centers[i] = new Vector2((anchor.x - 0.5f) * size.x, (anchor.y - 0.5f) * size.y);
                halves[i] = _placed[i].sizeDelta * 0.5f;
            }

            // Kẹp NGAY TỪ ĐẦU và sau MỖI vòng, không phải một lần lúc cuối: kẹp sau cùng
            // đẩy nhãn ở rìa chồng trở lại lên nhau mà không còn vòng nào để gỡ.
            ClampAll(centers, halves, visible);
            for (var pass = 0; pass < DeclutterPasses; pass++)
            {
                var moved = false;
                for (var i = 0; i < count; i++)
                for (var j = i + 1; j < count; j++)
                    moved |= Separate(centers, halves, i, j);
                ClampAll(centers, halves, visible);
                if (!moved) break;
            }

            for (var i = 0; i < count; i++)
            {
                var center = centers[i];
                var anchor = new Vector2(center.x / size.x + 0.5f, center.y / size.y + 0.5f);
                _placed[i].anchorMin = _placed[i].anchorMax = anchor;
                _placed[i].anchoredPosition = Vector2.zero;
            }
        }

        /// <summary>Tách hai nhãn chồng nhau theo trục chồng ÍT hơn — đường ra ngắn nhất.</summary>
        private static bool Separate(Vector2[] centers, Vector2[] halves, int i, int j)
        {
            var delta = centers[j] - centers[i];
            var overlapX = halves[i].x + halves[j].x + DeclutterGap - Mathf.Abs(delta.x);
            var overlapY = halves[i].y + halves[j].y + DeclutterGap - Mathf.Abs(delta.y);
            if (overlapX <= 0f || overlapY <= 0f) return false;

            if (overlapY <= overlapX)
            {
                // Hai nhãn trùng tâm hoàn toàn thì delta = 0, không có hướng để đẩy —
                // chọn cứng một hướng, nếu không chúng dính nhau vĩnh viễn.
                var sign = delta.y >= 0f ? 1f : -1f;
                var shift = overlapY * 0.5f * sign;
                centers[i] -= new Vector2(0f, shift);
                centers[j] += new Vector2(0f, shift);
            }
            else
            {
                var sign = delta.x >= 0f ? 1f : -1f;
                var shift = overlapX * 0.5f * sign;
                centers[i] -= new Vector2(shift, 0f);
                centers[j] += new Vector2(shift, 0f);
            }
            return true;
        }

        private static void ClampAll(Vector2[] centers, Vector2[] halves, Rect visible)
        {
            for (var i = 0; i < centers.Length; i++)
                centers[i] = Clamp(centers[i], halves[i], visible);
        }

        /// <summary>Kéo một nhãn vào trong vùng nhìn thấy; vùng hẹp hơn nhãn thì canh giữa.</summary>
        private static Vector2 Clamp(Vector2 center, Vector2 half, Rect visible)
        {
            var minX = visible.xMin + half.x;
            var maxX = visible.xMax - half.x;
            var minY = visible.yMin + half.y;
            var maxY = visible.yMax - half.y;
            return new Vector2(
                minX > maxX ? visible.center.x : Mathf.Clamp(center.x, minX, maxX),
                minY > maxY ? visible.center.y : Mathf.Clamp(center.y, minY, maxY));
        }
    }
}
