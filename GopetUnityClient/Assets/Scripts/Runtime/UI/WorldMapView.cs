using System;
using System.Collections.Generic;
using Gopet.Net.Map;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.UI
{
    /// <summary>
    /// Màn hình bản đồ thế giới — một MÀN RIÊNG phủ kín, không phải popup nổi trên cảnh
    /// chơi: tranh nền vẽ sẵn (<c>Ui/WorldMap/world-map-background</c>), mây trôi, và mỗi
    /// map là một pin + tên đặt đúng vùng địa hình của nó.
    ///
    /// <para>View tự dựng <see cref="Canvas"/> riêng có <c>overrideSorting</c> nên nó nằm
    /// trên toàn bộ HUD; nền tranh phủ kín màn khiến cảnh chơi phía sau khuất hẳn.</para>
    ///
    /// <para>Danh sách map và cờ khoá đến TỪ SERVER (TELE_MENU) — client không tự quyết
    /// map nào mở. Bấm map khoá chỉ hiện lý do server gửi, màn không đóng, không gói warp
    /// nào được gửi đi.</para>
    /// </summary>
    public sealed partial class WorldMapView : MonoBehaviour
    {
        /// <summary>Trên HUD overlay (sortingOrder 35) để bản đồ che trọn cảnh chơi.</summary>
        private const int SortingOrder = 60;

        private const float CloseSize = 40f;
        /// <summary>Pin giọt nước 64×80 (xem <c>tools/image-gen/world-map-pins.md</c>) — giữ đúng tỉ lệ.</summary>
        private const float PinWidth = 18f;
        private const float PinHeight = 22f;
        private const float LabelHeight = 20f;
        /// <summary>Đủ rộng cho tên map dài nhất của server ("Thung lũng Hoàng Nham") ở cỡ chữ 12.</summary>
        private const float LabelMaxWidth = 150f;
        private const float LockSize = 18f;

        private static readonly Color LetterboxFill = new Color(0.03f, 0.09f, 0.16f, 1f);
        private static readonly Color LabelBg = new Color(0.04f, 0.13f, 0.24f, 0.82f);
        private static readonly Color LabelLockedBg = new Color(0.16f, 0.17f, 0.2f, 0.82f);
        private static readonly Color LabelText = new Color(1f, 1f, 1f, 1f);
        private static readonly Color LabelLockedText = new Color(0.72f, 0.75f, 0.8f, 1f);
        /// <summary>Pin đã mang sẵn màu trong ảnh nên chỉ tô trắng; riêng quầng sáng thì nhuộm xanh.</summary>
        private static readonly Color PinCurrent = new Color(0.4f, 1f, 0.55f, 1f);
        private static readonly Color RegionText = new Color(1f, 0.95f, 0.75f, 1f);

        private Font _font;
        private RectTransform _picture;
        /// <summary>Cỡ pixel gốc của tranh nền — dùng để phóng cho phủ kín màn.</summary>
        private Vector2 _pictureSource = Vector2.one;
        private Vector2 _lastScreen;
        private RectTransform _nodeLayer;
        private bool _decided;

        /// <summary>Người chơi chọn một map mở khoá — tham số là mapId.</summary>
        public event Action<int> Chosen;

        /// <summary>Bấm vào map khoá — tham số là lý do server gửi. Màn KHÔNG đóng.</summary>
        public event Action<string> LockedChosen;

        public event Action CloseRequested;

        public static WorldMapView Create(Transform parent, Font font)
        {
            var root = new GameObject("WorldMapView", typeof(RectTransform), typeof(Canvas),
                typeof(GraphicRaycaster), typeof(Image));
            root.transform.SetParent(parent, false);
            UiBuilder.Stretch((RectTransform)root.transform);

            var canvas = root.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = SortingOrder;
            // Nền lót cho phần thừa khi tỉ lệ màn khác tỉ lệ tranh, đồng thời NUỐT cú chạm
            // để không rơi xuống cảnh chơi phía sau.
            root.GetComponent<Image>().color = LetterboxFill;

            var view = root.AddComponent<WorldMapView>();
            view._font = font;
            view.Build();
            return view;
        }

        /// <param name="options">Nguyên văn TELE_MENU của server.</param>
        /// <param name="currentMapId">Map đang đứng — được đánh dấu và không gửi warp khi bấm.</param>
        public void Bind(IReadOnlyList<MapTeleportOption> options, int currentMapId)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _decided = false;
            ClearNodes();

            // Map lạ (server thêm map mới) xếp vào cụm "Khác" theo mapId tăng dần.
            var unknown = new List<int>();
            foreach (var option in options)
                if (!WorldMapLayout.IsHidden(option.MapId) && !WorldMapLayout.IsKnown(option.MapId))
                    unknown.Add(option.MapId);
            unknown.Sort();

            var usedRegions = new HashSet<int>();
            foreach (var option in options)
            {
                if (WorldMapLayout.IsHidden(option.MapId)) continue;
                var placement = WorldMapLayout.Of(option.MapId, unknown);
                if (usedRegions.Add(placement.RegionId)) MakeRegionLabel(placement.RegionId);
                MakeNode(option, placement, option.MapId == currentMapId);
            }
            _needsDeclutter = true;
        }

        private void ClearNodes()
        {
            _placed.Clear();
            if (_nodeLayer == null) return;
            for (var i = _nodeLayer.childCount - 1; i >= 0; i--)
                Destroy(_nodeLayer.GetChild(i).gameObject);
        }

        private void Choose(int mapId)
        {
            if (_decided) return;
            _decided = true;
            Chosen?.Invoke(mapId);
        }
    }
}
