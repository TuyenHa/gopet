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
        /// Biên tối thiểu (px) giữa mép chữ nhãn và mép map. Portal gần biên (vd map 11
        /// "Đường lên núi" X=45/576, arrow lệch dx=-34) đẩy nhãn ra ngoài map — camera
        /// bị clamp theo map nên phần chữ tràn ra ngoài bị viewport cắt mất, không thấy
        /// full "Đường lên núi". Kẹp lại để cả chữ luôn nằm trong map.
        /// </summary>
        private const float LabelEdgeMargin = 4f;

        /// <summary>
        /// Khoảng cách (px) từ CẠNH GẦN NHẤT của hộp nhãn tên map — không phải tâm — mà
        /// player phải đứng trong để nút "Vào" hiện lên. ~1 ô (TileSize=24).
        ///
        /// <para><b>Vì sao đo tới HỘP CHỮ, không tới tâm chữ:</b> chữ tên map dài (vd
        /// "Đường lên núi" ~60 px), nếu đo tới tâm thì đứng trên/dưới chữ thấy gần hơn
        /// hẳn 2 đầu chữ trái/phải — cảm nhận không đều. Đo tới cạnh gần nhất → bán kính
        /// 25px đều nhau ở CẢ 4 PHÍA (trên, dưới, trái, phải hộp chữ), người chơi đi từ
        /// bất kỳ hướng nào cũng thấy nút hiện lên ở cùng khoảng cách.</para>
        /// </summary>
        private const float EnterRadius = 25f;

        /// <summary>Khoảng cách (px) từ nhãn lên nút "Vào", để 2 thứ không đè nhau.</summary>
        private const float ButtonAboveLabel = 6f;

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
            var halfTextWidth = JarFont.Width(entity.Name) * 0.5f * LabelScale;
            var min = LabelEdgeMargin + halfTextWidth - entity.X;
            var max = map.WidthPixels - LabelEdgeMargin - halfTextWidth - entity.X;
            return max < min ? (min + max) * 0.5f : Mathf.Clamp(localX, min, max);
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
        private PortalEnterButton _enterButton;
        private Transform _selfTransform;
        /// <summary>
        /// Hộp bao nhãn tên map trong world coords — mốc để đo <see cref="EnterRadius"/>
        /// tới CẠNH GẦN NHẤT. Set một lần trong <see cref="Create"/>; nhãn không di chuyển.
        /// </summary>
        private Vector2 _labelWorldMin;
        private Vector2 _labelWorldMax;
        public event Action<JarMapEntity> Selected;

        /// <summary>MapRenderer gọi khi self spawn/đổi map, để portal tự theo dõi khoảng cách.</summary>
        public void SetSelfTransform(Transform self) => _selfTransform = self;

        private void Update()
        {
            if (_enterButton == null || _selfTransform == null) return;
            Vector2 p = _selfTransform.position;
            // Khoảng cách² từ p tới hộp [min, max]: mỗi trục kẹp về khoảng, rồi lấy hiệu.
            // Nếu p nằm trong hộp trên trục nào thì hiệu = 0 trên trục đó.
            var dx = Mathf.Max(0f, Mathf.Max(_labelWorldMin.x - p.x, p.x - _labelWorldMax.x));
            var dy = Mathf.Max(0f, Mathf.Max(_labelWorldMin.y - p.y, p.y - _labelWorldMax.y));
            _enterButton.SetVisible(dx * dx + dy * dy <= EnterRadius * EnterRadius);
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

            // Bấm vào CHỮ tên map (không chỉ mũi tên) cũng phải sang map được — gộp
            // vùng bấm arrow + vùng bấm nhãn thành 1 collider bao trọn cả hai.
            var halfTextWidth = JarFont.Width(entity.Name) * 0.5f * LabelScale;
            var labelH = JarFont.Height * LabelScale;
            var xMin = Mathf.Min(-w * 0.5f, labelX - halfTextWidth);
            var xMax = Mathf.Max(w * 0.5f, labelX + halfTextWidth);
            var yMin = Mathf.Min(0f, labelY);
            var yMax = Mathf.Max(h, labelY + labelH);

            // Cache HỘP NHÃN world coords để Update() đo tới cạnh gần nhất (xem
            // <see cref="EnterRadius"/>). Nhãn canh giữa quanh (labelX, labelY):
            //   local X ∈ [labelX - halfW, labelX + halfW], local Y ∈ [labelY, labelY + H]
            // TransformPoint để cộng đúng cả tịnh tiến của parent (MapRenderer).
            view._labelWorldMin = go.transform.TransformPoint(labelX - halfTextWidth, labelY, 0f);
            view._labelWorldMax = go.transform.TransformPoint(labelX + halfTextWidth, labelY + labelH, 0f);

            var collider = go.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(xMax - xMin, yMax - yMin);
            collider.offset = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);

            var label = JarNameLabel.Create(go.transform, new Vector3(labelX, labelY, 0f),
                LabelScale, entity.Name);
            label.SetSortingOrder(29000);

            // Nút "Vào" — ẩn mặc định, MapPortalView.Update() bật lên khi self đứng gần
            // (xem EnterRadius). Bấm nút phát CÙNG sự kiện Selected với bấm mũi tên/nhãn.
            // Sprite pivot ở TÂM: đặt y = top-of-label + gap + halfHeight để đáy nút cách
            // đỉnh chữ đúng ButtonAboveLabel. Kẹp x theo bề rộng nút để không tràn biên map.
            var (buttonWidth, buttonHeight) = PortalEnterButton.DisplaySize(PortalEnterButton.LoadSprite());
            var buttonY = labelY + JarFont.Height * LabelScale + ButtonAboveLabel + buttonHeight * 0.5f;
            var buttonX = ClampButtonLocalX(entity, map, labelX, buttonWidth);
            view._enterButton = PortalEnterButton.Create(go.transform, new Vector3(buttonX, buttonY, 0f),
                () => view.Selected?.Invoke(entity));
            return view;
        }

        /// <summary>
        /// Kẹp X của nút "Vào" để toàn bộ nút (rộng <paramref name="buttonWidth"/>) nằm
        /// trong [<see cref="LabelEdgeMargin"/>, map.WidthPixels - <see cref="LabelEdgeMargin"/>].
        /// Nút rộng hơn chữ tên nên có thể tràn biên ngay cả khi chữ đã được kẹp.
        /// </summary>
        private static float ClampButtonLocalX(JarMapEntity entity, JarMapLayout map, float defaultX, float buttonWidth)
        {
            if (map == null) return defaultX;
            var half = buttonWidth * 0.5f;
            var min = LabelEdgeMargin + half - entity.X;
            var max = map.WidthPixels - LabelEdgeMargin - half - entity.X;
            return max < min ? (min + max) * 0.5f : Mathf.Clamp(defaultX, min, max);
        }

        public void OnPointerClick(PointerEventData eventData) => Selected?.Invoke(_entity);
    }
}
