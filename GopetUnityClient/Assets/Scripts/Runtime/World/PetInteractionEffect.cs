using System.Collections.Generic;
using Gopet.Runtime.UI;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Hiệu ứng hôn / chơi với / xoa đầu pet, chạy đúng bộ khung hình của jar (<c>bi.java</c>
    /// dựng <c>bh</c>, <c>dz</c> quay vòng khung theo <c>pet/petInteract/&lt;tên&gt;</c>).
    ///
    /// <para>Đứng YÊN một chỗ suốt thời gian diễn: jar chốt toạ độ <c>i/j</c> lúc tạo rồi
    /// không đụng nữa, nên hiệu ứng KHÔNG bám theo pet. Bám theo sẽ thấy icon trượt đi khi
    /// pet lùi về chỗ cũ.</para>
    ///
    /// <para>Vòng khung chạy lặp; ai gọi quyết định sống bao lâu (2 giây cho hôn/xoa đầu,
    /// 8 giây cho chơi — <c>bi.java</c> case 0/2 và case 1).</para>
    /// </summary>
    public sealed class PetInteractionEffect : MonoBehaviour
    {
        /// <summary>Tên file ảnh + file mô tả khung, theo thứ tự type của server
        /// (<c>ON_PET_INTERACT_KISS/PLAY/POKE</c> = 0/1/2).</summary>
        private static readonly string[] Names = { "kiss", "play", "poke" };

        /// <summary>Jar nâng chỗ vẽ lên bấy nhiêu pixel so với điểm neo (<c>bh.a</c>:
        /// <c>j - camY - 31</c> cho hôn/xoa đầu, <c>- 11</c> cho chơi).</summary>
        private static readonly int[] DrawLift = { 31, 11, 31 };

        private static readonly Dictionary<int, JarEffectAnimation> AnimCache =
            new Dictionary<int, JarEffectAnimation>();

        private JarEffectAnimation _anim;
        private SpriteRenderer[] _pieces;
        private int _type;
        private int _step;
        private float _nextStepTime;

        /// <summary>Nâng lên bao nhiêu pixel so với điểm neo, để người gọi tính sẵn vị trí.</summary>
        public static int LiftOf(int type) => Valid(type) ? DrawLift[type] : 0;

        /// <summary>
        /// Dựng hiệu ứng tại <paramref name="worldPosition"/> (đã cộng sẵn phần nâng của
        /// <see cref="LiftOf"/>) và tự huỷ sau <paramref name="seconds"/>.
        /// </summary>
        public static PetInteractionEffect Attach(Transform parent, Vector3 worldPosition,
            int type, int sortingOrder, float seconds)
        {
            if (!Valid(type) || seconds <= 0f) return null;

            var go = new GameObject($"Pet interaction {Names[type]}");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPosition;

            var effect = go.AddComponent<PetInteractionEffect>();
            effect._type = type;
            effect._anim = Load(type);
            effect._pieces = new SpriteRenderer[Mathf.Max(1, effect._anim.MaxPiecesPerFrame)];
            for (var i = 0; i < effect._pieces.Length; i++)
            {
                var piece = new GameObject($"Piece {i}", typeof(SpriteRenderer));
                piece.transform.SetParent(go.transform, false);
                effect._pieces[i] = piece.GetComponent<SpriteRenderer>();
                effect._pieces[i].sortingOrder = sortingOrder;
                effect._pieces[i].enabled = false;
            }

            effect.ShowStep(0);
            Destroy(go, seconds);
            return effect;
        }

        private static bool Valid(int type) => type >= 0 && type < Names.Length;

        private static JarEffectAnimation Load(int type)
        {
            if (AnimCache.TryGetValue(type, out var cached)) return cached;
            var path = $"pet/petInteract/{Names[type]}";
            return AnimCache[type] = JarEffectAnimation.Parse(JarSkin.RawBytes(path));
        }

        private void Update()
        {
            if (_anim == null || _anim.Steps.Length == 0 || Time.time < _nextStepTime) return;
            ShowStep((_step + 1) % _anim.Steps.Length);
        }

        private void ShowStep(int index)
        {
            var step = _anim.Steps[index];
            _step = index;
            _nextStepTime = Time.time + step.DurationMs / 1000f;

            var frame = _anim.Frames[step.FrameIndex];
            var sheet = $"pet/petInteract/{Names[_type]}";
            for (var i = 0; i < _pieces.Length; i++)
            {
                if (i >= frame.Length) { _pieces[i].enabled = false; continue; }

                var piece = frame[i];
                var region = _anim.Regions[piece.RegionIndex];
                _pieces[i].sprite = TileAssetProvider.Region(sheet, region.X, region.Y,
                                                             region.Width, region.Height);
                // Jar cộng thẳng (dx, dy) vào chỗ vẽ, mà trục Y của jar hướng XUỐNG.
                _pieces[i].transform.localPosition = new Vector3(piece.OffsetX, -piece.OffsetY, 0f);
                _pieces[i].enabled = true;
            }
        }
    }
}
