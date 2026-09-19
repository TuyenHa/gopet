using System;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>Cổng có tên trong đuôi map. Đi vào chỗ mũi tên là TỰ sang map; bấm vào mũi
    /// tên hoặc chữ cũng được, cả hai đều gửi opcode 25.
    ///
    /// <para>Trước đây phải bấm nút "Vào" hiện lên khi đứng gần. Bỏ nút vì thao tác thừa:
    /// người chơi đã đi tới tận nơi rồi thì ý định đã rõ.</para></summary>
    public sealed class MapPortalView : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>Nhỏ hơn tên nhân vật/NPC có chủ ý. Nhãn càng rộng thì phép kẹp biên
        /// (<see cref="ClampLocalXToMapWidth"/>) càng đẩy nó xa mũi tên: cổng "Đường lên núi"
        /// map 11 có mũi tên cách mép trái 11px, chữ rộng 109px bị đẩy lệch 48px. Thu nhỏ là
        /// cách trực tiếp nhất kéo tên về gần mũi tên.</summary>
        private const float LabelScale = 0.62f;

        /// <summary>Nhãn không được lệch khỏi mũi tên quá ngần này. Phép kẹp biên map một mình
        /// có thể đẩy nhãn đi rất xa (cổng sát mép map), làm tên trông như của nơi khác. Ưu tiên
        /// BÁM mũi tên: quá ngưỡng thì chấp nhận vài pixel chữ bị mép map cắt.</summary>
        private const float MaxArrowOffset = 26f;

        /// <summary>
        /// Khoảng dịch chữ LÊN trên tâm arrow (px). Nhãn căn giữa arrow theo cả X và Y
        /// nhưng nhích lên vài pixel để chữ không đè hoàn toàn glyph arrow — vẫn "gần"
        /// theo cả 2 trục, đủ đọc.
        /// </summary>
        private const float LabelLiftAboveCenter = 4f;

        /// <summary>Bounds mặc định khi <c>Raw5</c> vô nghĩa — 1 tile, đủ tap arrow.</summary>
        private const float DefaultColliderSide = 24f;

        /// <summary>
        /// Bán kính (px jar) tìm arrow object gần portal. Đo trên map 11: portal xa
        /// arrow tối đa 35 px, để 80 phòng map khác.
        /// </summary>
        private const int ArrowSearchRadius = 80;

        /// <summary>Filter arrow-shape: rộng ≤ 20, cao ≤ 30 — loại cây/nhà (~40+ px).</summary>
        private const int ArrowMaxWidth = 20;
        private const int ArrowMaxHeight = 30;

        /// <summary>
        /// Fallback offset (px trên portal position) khi không tìm được arrow object.
        /// Trung vị 4 portal map 11 = ~15.
        /// </summary>
        private const float FallbackLabelOffset = 15f;

        /// <summary>
        /// Biên tối thiểu (px) giữa mép chữ nhãn và mép map. Portal gần biên (vd map 11
        /// "Đường lên núi" X=45/576, arrow lệch dx=-34) đẩy nhãn ra ngoài map — camera
        /// bị clamp theo map nên phần chữ tràn ra ngoài bị viewport cắt mất, không thấy
        /// full "Đường lên núi". Kẹp lại để cả chữ luôn nằm trong map.
        /// </summary>
        private const float LabelEdgeMargin = 2f;

        /// <summary>Nới thêm quanh vùng chạm để tính "đã rời đi". Phải rời hẳn ra rồi mới
        /// được kích hoạt lần nữa: không có nó thì vừa sang map mới, nếu điểm hồi sinh nằm
        /// trong vùng cổng về, người chơi bị đẩy ngược lại ngay lập tức.</summary>
        private const float RearmPadding = 10f;

        /// <summary>
        /// Tâm arrow trong tọa độ LOCAL (Unity Y-up, gốc = portal position). Fallback khi
        /// không tìm được arrow: (0, <see cref="FallbackLabelOffset"/>) — tương thích hành
        /// vi cũ. Dùng chung cho nhãn (dịch lên <see cref="LabelLiftAboveCenter"/>) và
        /// khoảng cách hiện nút (đo trực tiếp tới điểm này).
        /// </summary>
        public static (float x, float y) ArrowLocalOffset(JarMapEntity entity, JarMapLayout map)
        {
            var arrow = FindArrow(entity, map);
            if (arrow == null) return (0f, FallbackLabelOffset);
            // Arrow neo (X,Y) middle-bottom, YOffset dịch dọc, Bounds[3] là height.
            // Jar coord (Y-down): arrow_ctr_jar_y = arrow.Y - YOffset - height/2
            // Unity Y-up local từ portal position:
            //   x = arrow.X - entity.X
            //   y = entity.Y - arrow_ctr_jar_y = entity.Y - arrow.Y + YOffset + height/2
            var localX = arrow.X - entity.X;
            var localY = entity.Y - arrow.Y + arrow.YOffset + arrow.Bounds[3] * 0.5f;
            return (localX, localY);
        }

        /// <summary>
        /// Vị trí nhãn — nghiệp vụ tính riêng để test đọc được. Trả (localX, localY)
        /// từ portal position để đặt nhãn CĂN GIỮA arrow (dịch cả X + Y), nhích lên
        /// <see cref="LabelLiftAboveCenter"/> để chữ trắng-viền-đen không đè glyph arrow,
        /// rồi kẹp localX theo <see cref="ClampLocalXToMapWidth"/> để chữ không tràn biên map.
        /// </summary>
        public static (float x, float y) LabelLocalOffset(JarMapEntity entity, JarMapLayout map)
        {
            var (ax, ay) = ArrowLocalOffset(entity, map);
            // Nếu ArrowLocalOffset đã fallback (ay == FallbackLabelOffset khi arrow null)
            // thì lift không cộng thêm — hành vi cũ giữ nhãn đúng vị trí FallbackLabelOffset.
            var lift = FindArrow(entity, map) == null ? 0f : LabelLiftAboveCenter;
            return (ClampLocalXToMapWidth(entity, map, ax), ay + lift);
        }

        /// <summary>
        /// Kẹp localX (canh giữa quanh entity.X + localX) sao cho toàn bộ chữ nằm trong
        /// [<see cref="LabelEdgeMargin"/>, map.WidthPixels - <see cref="LabelEdgeMargin"/>].
        /// Nếu chữ rộng hơn cả map (hiếm) thì canh giữa map, chấp nhận tràn đều 2 bên.
        /// </summary>
        private static float ClampLocalXToMapWidth(JarMapEntity entity, JarMapLayout map, float localX)
        {
            if (map == null) return localX;
            var halfTextWidth = JarNameLabel.EstimateWidth(entity.Name) * 0.5f * LabelScale;
            var min = LabelEdgeMargin + halfTextWidth - entity.X;
            var max = map.WidthPixels - LabelEdgeMargin - halfTextWidth - entity.X;
            var clamped = max < min ? (min + max) * 0.5f : Mathf.Clamp(localX, min, max);
            // Kẹp lần hai theo mũi tên: thà chữ bị mép cắt vài pixel còn hơn tên trôi đi xa.
            return Mathf.Clamp(clamped, localX - MaxArrowOffset, localX + MaxArrowOffset);
        }

        private static JarMapObject FindArrow(JarMapEntity entity, JarMapLayout map)
        {
            if (map?.Objects == null || map.ResourceTypes == null) return null;
            JarMapObject best = null;
            var bestDistSq = ArrowSearchRadius * ArrowSearchRadius;
            foreach (var obj in map.Objects)
            {
                if (obj.ResourceIndex < 0 || obj.ResourceIndex >= map.ResourceTypes.Length) continue;
                if (map.ResourceTypes[obj.ResourceIndex] != JarMapLayout.TypeAnimation) continue;
                if (obj.Bounds == null || obj.Bounds.Length < 4) continue;
                if (obj.Bounds[2] > ArrowMaxWidth || obj.Bounds[3] > ArrowMaxHeight) continue;
                var dx = obj.X - entity.X;
                var dy = obj.Y - entity.Y;
                var distSq = dx * dx + dy * dy;
                if (distSq < bestDistSq) { bestDistSq = distSq; best = obj; }
            }
            return best;
        }

        private JarMapEntity _entity;
        private Transform _selfTransform;

        /// <summary>Vùng chạm của cổng trong world coords — bước vào là sang map.</summary>
        private Vector2 _zoneMin, _zoneMax;

        /// <summary>Đã rời xa cổng lần nào chưa. Xem <see cref="RearmRadius"/>.</summary>
        private bool _armed;

        private bool _entered;
        public event Action<JarMapEntity> Selected;

        /// <summary>MapRenderer gọi khi self spawn/đổi map, để portal tự theo dõi khoảng cách.</summary>
        public void SetSelfTransform(Transform self) => _selfTransform = self;

        private void Update()
        {
            if (_entered || _selfTransform == null) return;
            Vector2 p = _selfTransform.position;
            var inside = p.x >= _zoneMin.x && p.x <= _zoneMax.x
                         && p.y >= _zoneMin.y && p.y <= _zoneMax.y;

            if (!_armed)
            {
                _armed = p.x < _zoneMin.x - RearmPadding || p.x > _zoneMax.x + RearmPadding
                         || p.y < _zoneMin.y - RearmPadding || p.y > _zoneMax.y + RearmPadding;
                return;
            }

            if (!inside) return;
            // Chốt một lần: Selected dẫn tới đổi map, mà cho tới lúc map mới dựng xong thì
            // Update vẫn chạy — không chốt thì bắn hàng loạt gói chuyển map.
            _entered = true;
            Selected?.Invoke(_entity);
        }

        public static MapPortalView Create(Transform parent, JarMapEntity entity, JarMapLayout map, int mapHeight)
        {
            var go = new GameObject($"Portal {entity.Name}", typeof(BoxCollider2D));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<MapPortalView>();
            view._entity = entity;
            var (x, y) = MapPlacement.JarToWorld(entity.X, entity.Y, mapHeight);
            go.transform.localPosition = new Vector3(x, y, 0f);

            // Kích thước collider từ Raw5[3][4] — CAST sbyte để byte âm (-14) không
            // biến thành 242. Clamp ≥ 24 để luôn bấm được.
            var raw = entity.Raw5;
            var w = Mathf.Max(DefaultColliderSide, raw != null && raw.Length > 3 ? Mathf.Max(0, (sbyte)raw[3]) : 0);
            var h = Mathf.Max(DefaultColliderSide, raw != null && raw.Length > 4 ? Mathf.Max(0, (sbyte)raw[4]) : 0);

            // Portal NAMED không được jar tự vẽ arrow ở portal position — arrow là 1
            // OBJECT RIÊNG trong `map.Objects` do tác giả map đặt, LỆCH cả X lẫn Y so
            // với portal position (map 11 đo được: dx 4-34, dy 3-17). Nhãn cần dịch CẢ
            // 2 TRỤC để căn giữa arrow, không chỉ Y. Xem <see cref="LabelLocalOffset"/>.
            var (labelX, labelY) = LabelLocalOffset(entity, map);
            var (ax, ay) = ArrowLocalOffset(entity, map);

            // Bấm vào CHỮ tên map (không chỉ mũi tên) cũng phải sang map được — gộp
            // vùng bấm arrow + vùng bấm nhãn thành 1 collider bao trọn cả hai.
            var halfTextWidth = JarNameLabel.EstimateWidth(entity.Name) * 0.5f * LabelScale;
            var labelH = JarNameLabel.Height * LabelScale;
            var xMin = Mathf.Min(-w * 0.5f, labelX - halfTextWidth);
            var xMax = Mathf.Max(w * 0.5f, labelX + halfTextWidth);
            var yMin = Mathf.Min(0f, labelY);
            var yMax = Mathf.Max(h, labelY + labelH);

            // Vùng chạm lấy THẲNG từ dữ liệu map, không tự chế: jar dựng nó bằng
            // `new gy(raw[1], raw[2], raw[3], raw[4])` — offset + kích thước neo vào cổng.
            // Trước đây tôi dùng bán kính quanh mũi tên DÒ ĐƯỢC; vùng đó nhỏ và lệch so với
            // vùng thật (cổng "Đường lên núi" thật là 32×50) nên đi từ dưới lên là lọt ra ngoài.
            // Jar Y hướng XUỐNG còn Unity hướng LÊN nên trục Y đảo dấu.
            var zx = raw != null && raw.Length > 1 ? (sbyte)raw[1] : -w * 0.5f;
            var zy = raw != null && raw.Length > 2 ? (sbyte)raw[2] : 0f;
            var zoneA = go.transform.TransformPoint(zx, -zy - h, 0f);
            var zoneB = go.transform.TransformPoint(zx + w, -zy, 0f);
            view._zoneMin = Vector2.Min(zoneA, zoneB);
            view._zoneMax = Vector2.Max(zoneA, zoneB);

            var collider = go.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(xMax - xMin, yMax - yMin);
            collider.offset = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);

            var label = JarNameLabel.Create(go.transform, new Vector3(labelX, labelY, 0f),
                LabelScale, entity.Name);
            label.SetSortingOrder(29000);

            return view;
        }

        public void OnPointerClick(PointerEventData eventData) => Selected?.Invoke(_entity);
    }
}
