using System.Collections.Generic;
using Gopet.Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Gopet.Runtime.World.Battle
{
    /// <summary>
    /// Bộ phát hạt dùng chung cho hiệu ứng khung cảnh (tuyết, cánh hoa, bướm, dơi, lửa). Vẽ
    /// bằng <see cref="Image"/> trên canvas như các hiệu ứng kỹ năng — ParticleSystem không xếp
    /// lớp được giữa nền và card pet.
    ///
    /// <para>Số hạt cố định theo preset, dựng một lần rồi tái dùng: hạt rơi ra khỏi đáy thì
    /// quay lại đỉnh, không Instantiate/Destroy mỗi frame.</para></summary>
    public sealed class BattleAmbientFx : MonoBehaviour
    {
        private const float Margin = 24f;

        private sealed class Particle
        {
            public AmbientLayer Layer;
            public Sprite[] Frames;
            public RectTransform Rect;
            public Image Img;
            public Vector2 Pos, Target;
            public float Speed, Phase, Rotation, SpinRate, HoverUntil, Alpha;
        }

        private readonly List<Particle> _particles = new List<Particle>();
        private RectTransform _rect;
        private bool _placed;

        /// <summary>Dựng lớp hiệu ứng phủ kín <paramref name="parent"/>; null nếu không có lớp nào.</summary>
        public static BattleAmbientFx Create(Transform parent, AmbientLayer[] layers)
        {
            if (layers == null || layers.Length == 0) return null;
            // Canvas con: hạt đổi vị trí mỗi frame chỉ dựng lại batch của riêng lớp này, không
            // kéo cả canvas màn đấu (HUD, chữ, card pet) dựng lại theo. overrideSorting tắt nên
            // thứ tự lớp vẫn theo cây: trên nền, dưới card pet.
            var go = new GameObject("Hiệu ứng khung cảnh", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var fx = go.AddComponent<BattleAmbientFx>();
            fx._rect = rect;
            foreach (var layer in layers) fx.Spawn(layer);
            return fx;
        }

        private void Spawn(AmbientLayer layer)
        {
            var frames = new Sprite[layer.Frames.Length];
            for (var i = 0; i < frames.Length; i++) frames[i] = BattleSkin.Load(layer.Frames[i]);
            if (frames[0] == null) return; // thiếu asset: bỏ lớp này, không vẽ ô trắng

            var unit = layer.FixedHeight > 0f
                ? layer.FixedHeight / frames[0].rect.height
                : BattleSkin.SnappedSpriteScale(this);
            for (var i = 0; i < layer.Count; i++)
            {
                var go = new GameObject("Hạt", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var p = new Particle
                {
                    Layer = layer, Frames = frames, Rect = (RectTransform)go.transform,
                    Img = go.GetComponent<Image>(), Speed = Range(layer.Speed),
                    Phase = Random.value * Mathf.PI * 2f,
                    SpinRate = layer.Spin * (Random.value < 0.5f ? -1f : 1f),
                    Alpha = Range(layer.Alpha),
                };
                p.Img.sprite = frames[0];
                p.Img.raycastTarget = false;
                p.Img.color = new Color(layer.Tint.r, layer.Tint.g, layer.Tint.b, layer.Tint.a * p.Alpha);
                p.Rect.anchorMin = p.Rect.anchorMax = Vector2.zero;
                // Lửa phình từ gốc, dơi treo đung đưa quanh điểm bám; còn lại quay quanh tâm.
                p.Rect.pivot = layer.Motion != AmbientMotion.Static ? new Vector2(0.5f, 0.5f)
                    : layer.Flicker ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 1f);
                p.Rect.sizeDelta = frames[0].rect.size * unit * Range(layer.Scale);
                _particles.Add(p);
            }
        }

        private void Update()
        {
            var size = _rect.rect.size;
            if (size.x < 1f || size.y < 1f) return; // layout chưa tính xong
            if (!_placed) PlaceAll(size);

            var t = Time.unscaledTime;
            var dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f); // tránh hạt nhảy cóc sau khi app bị treo
            foreach (var p in _particles)
            {
                switch (p.Layer.Motion)
                {
                    case AmbientMotion.Fall: StepFall(p, size, t, dt); break;
                    case AmbientMotion.Wander: StepWander(p, size, t, dt); break;
                    default: StepStatic(p, t); break;
                }
                if (p.Frames.Length > 1 && p.Layer.FrameHz > 0f)
                {
                    var frame = p.Frames[(int)(t * p.Layer.FrameHz + p.Phase * 3f) % p.Frames.Length];
                    if (frame != null) p.Img.sprite = frame;
                }
            }
        }

        /// <summary>Rải hạt khắp vùng ngay từ đầu, không để màn trống chờ hạt rơi từ đỉnh xuống.</summary>
        private void PlaceAll(Vector2 size)
        {
            foreach (var p in _particles)
            {
                p.Pos = RandomIn(p.Layer.Region, size);
                p.Target = RandomIn(p.Layer.Region, size);
                p.Rect.anchoredPosition = p.Pos;
            }
            _placed = true;
        }

        private static void StepFall(Particle p, Vector2 size, float t, float dt)
        {
            var l = p.Layer;
            p.Pos += new Vector2(l.Drift, -p.Speed) * dt;
            if (p.Pos.y < -Margin)
            {
                p.Pos.y = size.y + Margin;
                p.Pos.x = Random.Range(0f, size.x);
            }
            // Cuộn ngang: hạt trôi chéo ra mép này thì vào lại từ mép kia.
            if (p.Pos.x < -Margin) p.Pos.x += size.x + Margin * 2f;
            else if (p.Pos.x > size.x + Margin) p.Pos.x -= size.x + Margin * 2f;

            var sway = Mathf.Sin(t * l.SwayHz * Mathf.PI * 2f + p.Phase) * l.Sway;
            p.Rect.anchoredPosition = new Vector2(p.Pos.x + sway, p.Pos.y);
            if (p.SpinRate != 0f)
            {
                p.Rotation += p.SpinRate * dt;
                p.Rect.localEulerAngles = new Vector3(0f, 0f, p.Rotation);
            }
        }

        private static void StepWander(Particle p, Vector2 size, float t, float dt)
        {
            var l = p.Layer;
            if (t >= p.HoverUntil)
            {
                var delta = p.Target - p.Pos;
                var dist = delta.magnitude;
                if (dist < 4f)
                {
                    p.Target = RandomIn(l.Region, size);
                    if (Random.value < l.HoverChance) p.HoverUntil = t + Random.Range(0.4f, 1.6f);
                }
                else
                {
                    p.Pos += delta / dist * Mathf.Min(dist, p.Speed * dt);
                    // Quay mặt theo hướng bay; sprite gốc nhìn trái thì lật ngược lại.
                    if (Mathf.Abs(delta.x) > 1f)
                    {
                        var right = delta.x > 0f;
                        var sx = right != l.FacesLeft ? 1f : -1f;
                        p.Rect.localScale = new Vector3(sx, 1f, 1f);
                    }
                }
            }
            var bob = Mathf.Sin(t * l.SwayHz * Mathf.PI * 2f + p.Phase) * l.Sway;
            p.Rect.anchoredPosition = new Vector2(p.Pos.x, p.Pos.y + bob);
        }

        private static void StepStatic(Particle p, float t)
        {
            var l = p.Layer;
            if (l.Flicker)
            {
                // Hai sóng lệch tần số cho nhịp lập loè không đều như lửa thật.
                var k = Mathf.Sin(t * 9f + p.Phase) * 0.08f + Mathf.Sin(t * 14.3f + p.Phase * 2f) * 0.05f;
                p.Rect.localScale = new Vector3(1f - k * 0.5f, 1f + k, 1f);
                var c = p.Img.color;
                c.a = Mathf.Clamp01(p.Alpha + k);
                p.Img.color = c;
            }
            else if (l.Sway > 0f)
            {
                var angle = Mathf.Sin(t * l.SwayHz * Mathf.PI * 2f + p.Phase) * l.Sway;
                p.Rect.localEulerAngles = new Vector3(0f, 0f, angle);
            }
        }

        private static Vector2 RandomIn(Rect region, Vector2 size) => new Vector2(
            Random.Range(region.xMin, region.xMax) * size.x,
            Random.Range(region.yMin, region.yMax) * size.y);

        private static float Range(Vector2 r) => Random.Range(r.x, r.y);
    }
}
