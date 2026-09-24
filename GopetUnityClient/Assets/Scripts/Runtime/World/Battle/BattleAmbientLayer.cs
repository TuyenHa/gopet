using UnityEngine;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>Cách một nhóm hạt chuyển động.</summary>
    public enum AmbientMotion
    {
        /// <summary>Rơi theo vận tốc cố định (tuyết, cánh hoa, mưa lửa), lắc hình sin, ra khỏi đáy thì sinh lại ở đỉnh.</summary>
        Fall,
        /// <summary>Bay lượn tới điểm ngẫu nhiên trong vùng, đôi khi dừng lơ lửng (bướm, chuồn chuồn, dơi).</summary>
        Wander,
        /// <summary>Đứng yên tại chỗ, đung đưa nhẹ hoặc nhấp nháy (dơi treo, lửa cháy).</summary>
        Static,
    }

    /// <summary>
    /// Cấu hình một nhóm hạt của <see cref="BattleAmbientFx"/>. Vùng (<see cref="Region"/>) tính
    /// theo tỉ lệ 0..1 của nền, gốc ở góc DƯỚI-trái — độc lập với độ phân giải màn hình.
    /// </summary>
    public sealed class AmbientLayer
    {
        public string[] Frames;             // đường dẫn Resources các khung hình (≥1)
        public int Count = 10;
        public AmbientMotion Motion = AmbientMotion.Fall;
        public Rect Region = new Rect(0f, 0f, 1f, 1f);
        public Vector2 Scale = Vector2.one;  // nhân thêm vào SpriteScale, random trong [x, y]
        public float FixedHeight;           // > 0: bỏ SpriteScale, đặt chiều cao canvas cố định
        public Vector2 Speed = new Vector2(30f, 60f); // đơn vị canvas / giây, random trong [x, y]
        public float Drift;                 // Fall: vận tốc ngang (âm = sang trái)
        public float Sway;                  // biên độ lắc ngang (Fall) / nhấp nhô (Wander, Static)
        public float SwayHz = 0.5f;
        public float Spin;                  // độ / giây, random dấu
        public float FrameHz;               // tốc độ đổi khung (vỗ cánh, lửa lập loè)
        public float HoverChance;           // Wander: xác suất dừng lơ lửng khi tới đích
        public bool FacesLeft;              // Wander: sprite gốc quay đầu sang trái
        public bool Flicker;                // Static: scale/alpha lập loè như lửa
        public Vector2 Alpha = Vector2.one;
        public Color Tint = Color.white;
    }
}
