using System.Collections.Generic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>
    /// Nhãn tên vẽ bằng bitmap font jar (<see cref="JarFont"/>). Glyph xếp trái→phải, canh giữa.
    /// Theo bản jar: chữ TRẮNG + viền ĐEN 1px (4 hướng). Dùng chung cho người chơi lẫn NPC/quái.
    /// </summary>
    public sealed class JarNameLabel : MonoBehaviour
    {
        private static readonly Vector2[] OutlineOffsets =
            { new Vector2(1f, 0f), new Vector2(-1f, 0f), new Vector2(0f, 1f), new Vector2(0f, -1f) };

        private readonly List<SpriteRenderer> _fill = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> _outline = new List<SpriteRenderer>();

        public static JarNameLabel Create(Transform parent, Vector3 localPos, float scale, string text)
        {
            var go = new GameObject("Name");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * scale;
            var label = go.AddComponent<JarNameLabel>();
            label.SetText(text);
            return label;
        }

        public void SetText(string text)
        {
            Clear();
            if (string.IsNullOrEmpty(text)) return;

            var x = -JarFont.Width(text) * 0.5f; // canh giữa: bắt đầu ở nửa bề rộng bên trái
            foreach (var c in text)
            {
                var sprite = JarFont.Glyph(c);
                if (sprite != null)
                {
                    foreach (var off in OutlineOffsets) // viền đen trước (nằm dưới)
                        _outline.Add(AddGlyph(sprite, x + off.x, off.y, Color.black));
                    _fill.Add(AddGlyph(sprite, x, 0f, Color.white)); // chữ trắng đè lên
                }
                x += JarFont.Width(c);
            }
        }

        private SpriteRenderer AddGlyph(Sprite sprite, float x, float y, Color color)
        {
            var child = new GameObject("g", typeof(SpriteRenderer));
            child.transform.SetParent(transform, false);
            child.transform.localPosition = new Vector3(x, y, 0f);
            var renderer = child.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            return renderer;
        }

        public void SetSortingOrder(int order)
        {
            foreach (var r in _outline) if (r != null) r.sortingOrder = order;      // viền dưới
            foreach (var r in _fill) if (r != null) r.sortingOrder = order + 1;     // chữ trắng trên
        }

        private void Clear()
        {
            foreach (var r in _outline) if (r != null) Destroy(r.gameObject);
            foreach (var r in _fill) if (r != null) Destroy(r.gameObject);
            _outline.Clear();
            _fill.Clear();
        }
    }
}
