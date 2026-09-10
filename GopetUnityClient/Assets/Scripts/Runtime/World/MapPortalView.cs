using System;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>Cổng có tên trong đuôi map; bấm để gửi opcode 25.</summary>
    public sealed class MapPortalView : MonoBehaviour, IPointerClickHandler
    {
        // Khớp NameScale của PlayerAvatar — cùng cỡ chữ với tên nhân vật/NPC.
        private const float LabelScale = 0.75f;

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
        /// Vị trí nhãn — nghiệp vụ tính riêng để test đọc được. Trả (localX, localY)
        /// từ portal position để đặt nhãn CĂN GIỮA arrow (dịch cả X + Y), nhích lên
        /// <see cref="LabelLiftAboveCenter"/> để chữ trắng-viền-đen không đè glyph arrow.
        /// </summary>
        public static (float x, float y) LabelLocalOffset(JarMapEntity entity, JarMapLayout map)
        {
            var arrow = FindArrow(entity, map);
            if (arrow == null) return (0f, FallbackLabelOffset);
            // Arrow neo (X,Y) middle-bottom, YOffset dịch dọc, Bounds[3] là height.
            // Jar coord (Y-down): arrow_top = arrow.Y - YOffset - height
            //                     arrow_bot = arrow.Y - YOffset
            //                     arrow_ctr = arrow.Y - YOffset - height/2
            // Unity Y-up local từ portal position:
            //   x = arrow.X - entity.X (giữa arrow theo X)
            //   y = entity.Y - arrow_ctr_jar + LiftAboveCenter
            //     = entity.Y - arrow.Y + YOffset + height/2 + LiftAboveCenter
            var localX = arrow.X - entity.X;
            var localY = entity.Y - arrow.Y + arrow.YOffset + arrow.Bounds[3] * 0.5f + LabelLiftAboveCenter;
            return (localX, localY);
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
        public event Action<JarMapEntity> Selected;

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
            var collider = go.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(w, h);
            collider.offset = new Vector2(0f, h * 0.5f);

            // Portal NAMED không được jar tự vẽ arrow ở portal position — arrow là 1
            // OBJECT RIÊNG trong `map.Objects` do tác giả map đặt, LỆCH cả X lẫn Y so
            // với portal position (map 11 đo được: dx 4-34, dy 3-17). Nhãn cần dịch CẢ
            // 2 TRỤC để căn giữa arrow, không chỉ Y. Xem <see cref="LabelLocalOffset"/>.
            var (labelX, labelY) = LabelLocalOffset(entity, map);

            var label = JarNameLabel.Create(go.transform, new Vector3(labelX, labelY, 0f),
                LabelScale, entity.Name);
            label.SetSortingOrder(29000);
            return view;
        }

        public void OnPointerClick(PointerEventData eventData) => Selected?.Invoke(_entity);
    }
}
