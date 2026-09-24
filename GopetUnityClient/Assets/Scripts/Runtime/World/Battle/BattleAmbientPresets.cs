using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>
    /// Mật độ và nhịp hiệu ứng của từng khung cảnh. Chỉnh cảm giác (dày/thưa, nhanh/chậm) ở
    /// đây, không phải trong <see cref="BattleAmbientFx"/>.
    ///
    /// <para>Vùng bay của sinh vật tránh dải giữa-dưới (y 0.15..0.45) — chỗ pet đứng và số
    /// damage bay lên — để hiệu ứng không che trận đấu.</para></summary>
    public static class BattleAmbientPresets
    {
        private const string Dir = "Battle/ambient/";

        public static AmbientLayer[] For(BattleAmbientKind kind)
        {
            switch (kind)
            {
                case BattleAmbientKind.Canopy: return Canopy();
                case BattleAmbientKind.Sakura: return Sakura();
                case BattleAmbientKind.Snow: return Snow();
                case BattleAmbientKind.Cave: return Cave();
                case BattleAmbientKind.Fire: return Fire();
                default: return new AmbientLayer[0];
            }
        }

        private static AmbientLayer[] Canopy() => new[]
        {
            new AmbientLayer
            {
                Frames = new[] { Dir + "butterfly-0", Dir + "butterfly-1" }, Count = 4,
                Motion = AmbientMotion.Wander, Region = new Rect(0.04f, 0.45f, 0.92f, 0.45f),
                Scale = new Vector2(0.9f, 1.2f), Speed = new Vector2(35f, 60f), Sway = 10f,
                SwayHz = 1.4f, FrameHz = 7f, HoverChance = 0.3f,
            },
            new AmbientLayer
            {
                Frames = new[] { Dir + "dragonfly-0", Dir + "dragonfly-1" }, Count = 2,
                Motion = AmbientMotion.Wander, Region = new Rect(0.05f, 0.5f, 0.9f, 0.4f),
                Scale = new Vector2(1f, 1.1f), Speed = new Vector2(140f, 200f), Sway = 3f,
                SwayHz = 3f, FrameHz = 22f, HoverChance = 0.7f, FacesLeft = true,
            },
        };

        private static AmbientLayer[] Sakura() => new[]
        {
            new AmbientLayer
            {
                Frames = new[] { Dir + "petal" }, Count = 24, Motion = AmbientMotion.Fall,
                Scale = new Vector2(0.9f, 1.5f), Speed = new Vector2(28f, 55f), Drift = 18f,
                Sway = 26f, SwayHz = 0.45f, Spin = 110f, Alpha = new Vector2(0.75f, 1f),
            },
        };

        private static AmbientLayer[] Snow() => new[]
        {
            // Hai lớp: xa (nhỏ, chậm, mờ) và gần (to, nhanh) tạo chiều sâu.
            new AmbientLayer
            {
                Frames = new[] { Dir + "snowflake" }, Count = 30, Motion = AmbientMotion.Fall,
                Scale = new Vector2(0.5f, 0.8f), Speed = new Vector2(20f, 35f), Drift = -6f,
                Sway = 10f, SwayHz = 0.35f, Alpha = new Vector2(0.45f, 0.7f),
            },
            new AmbientLayer
            {
                Frames = new[] { Dir + "snowflake" }, Count = 16, Motion = AmbientMotion.Fall,
                Scale = new Vector2(1f, 1.4f), Speed = new Vector2(45f, 70f), Drift = -10f,
                Sway = 16f, SwayHz = 0.5f, Spin = 40f, Alpha = new Vector2(0.85f, 1f),
            },
        };

        private static AmbientLayer[] Cave() => new[]
        {
            new AmbientLayer
            {
                Frames = new[] { Dir + "bat-hang" }, Count = 4, Motion = AmbientMotion.Static,
                Region = new Rect(0.12f, 0.86f, 0.76f, 0.06f), Scale = new Vector2(1f, 1.15f),
                Sway = 4f, SwayHz = 0.6f,
            },
            new AmbientLayer
            {
                Frames = new[] { Dir + "bat-fly-0", Dir + "bat-fly-1" }, Count = 3,
                Motion = AmbientMotion.Wander, Region = new Rect(0.04f, 0.5f, 0.92f, 0.42f),
                Scale = new Vector2(1f, 1.2f), Speed = new Vector2(90f, 140f), Sway = 12f,
                SwayHz = 2.2f, FrameHz = 9f, HoverChance = 0.1f,
            },
        };

        private static AmbientLayer[] Fire() => new[]
        {
            // Mưa lửa rơi chéo.
            new AmbientLayer
            {
                Frames = new[] { Dir + "ember" }, Count = 26, Motion = AmbientMotion.Fall,
                Scale = new Vector2(1.2f, 2.2f), Speed = new Vector2(90f, 150f), Drift = -45f,
                Sway = 4f, SwayHz = 1.5f, Alpha = new Vector2(0.7f, 1f),
            },
            // Lửa cháy ở hai mép, chừa dải giữa cho pet.
            new AmbientLayer
            {
                Frames = new[] { "Battle/fx/106" }, Count = 3, Motion = AmbientMotion.Static,
                Region = new Rect(0f, 0.02f, 0.14f, 0.3f), FixedHeight = 70f, Flicker = true,
                Alpha = new Vector2(0.8f, 0.95f),
            },
            new AmbientLayer
            {
                Frames = new[] { "Battle/fx/106" }, Count = 3, Motion = AmbientMotion.Static,
                Region = new Rect(0.86f, 0.02f, 0.14f, 0.3f), FixedHeight = 70f, Flicker = true,
                Alpha = new Vector2(0.8f, 0.95f),
            },
        };
    }
}
