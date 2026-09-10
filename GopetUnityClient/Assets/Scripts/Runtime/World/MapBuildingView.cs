using System;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Vùng bấm cho building trên map (jar entity <c>Kind==0</c>). Không tự render sprite —
    /// building đã do <see cref="MapRenderer.BuildObjects"/> vẽ. Chỉ thêm collider + nhãn
    /// (khi có tên) và phát <see cref="Selected"/> khi bấm.
    /// </summary>
    public sealed class MapBuildingView : MonoBehaviour, IPointerClickHandler
    {
        private JarMapEntity _entity;
        public event Action<JarMapEntity> Selected;

        public JarMapEntity Entity => _entity;

        public static MapBuildingView Create(Transform parent, JarMapEntity entity, int mapHeight)
        {
            var label = BuildingDispatcher.LabelOf(entity.BuildingType);
            var go = new GameObject($"Building {entity.BuildingType} {label}", typeof(BoxCollider2D));
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<MapBuildingView>();
            view._entity = entity;

            var (x, y) = MapPlacement.JarToWorld(entity.X, entity.Y, mapHeight);
            go.transform.localPosition = new Vector3(x, y, 0f);

            // Raw5[3]/[4] = kích thước bounding của building trong file map (jar eg.java);
            // fallback 32×32 khi server đặt 0 (một số building nhỏ).
            var raw = entity.Raw5;
            var w = raw != null && raw.Length > 4 ? Mathf.Max(32, raw[3]) : 48;
            var h = raw != null && raw.Length > 4 ? Mathf.Max(32, raw[4]) : 48;
            var collider = go.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(w, h);
            // (x,y) entity = giữa đáy building → offset collider lên nửa bề cao.
            collider.offset = new Vector2(0f, h * 0.5f);

            if (!string.IsNullOrEmpty(label))
            {
                var nameLabel = JarNameLabel.Create(go.transform, new Vector3(0f, h + 6f, 0f), 1f, label);
                nameLabel.SetSortingOrder(29000);
            }
            return view;
        }

        public void OnPointerClick(PointerEventData eventData) => Selected?.Invoke(_entity);
    }
}
