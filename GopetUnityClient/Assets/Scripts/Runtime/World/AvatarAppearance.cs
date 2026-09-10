using System.Collections.Generic;
using Gopet.Runtime.UI;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Ghép avatar mặc định theo đúng vùng cắt và toạ độ của <c>v.java</c>.</summary>
    public sealed class AvatarAppearance : MonoBehaviour
    {
        private readonly struct Part
        {
            public readonly int Kind, Image, X, Y, Layer;
            public Part(int kind, int image, int x, int y, int layer)
            { Kind = kind; Image = image; X = x; Y = y; Layer = layer; }
        }

        private sealed class SwitchPart
        {
            public SpriteRenderer Normal, Step;
        }

        private const float TicksPerSecond = 30f; // jar BaseCanvas.ticks ~30/giây: giữ đúng chu kỳ
        private const int IdleBobPixels = 3;       // jar nhún 1px; tăng cho dễ thấy theo yêu cầu

        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private readonly List<(SpriteRenderer renderer, int layer)> _renderers =
            new List<(SpriteRenderer, int)>();
        private readonly List<SwitchPart> _switchParts = new List<SwitchPart>();
        private Transform _visual;
        private Transform _body;   // thân + parts + mặt: nhún theo idle-bob; bóng & chân đứng yên
        private SpriteRenderer _legNormal;
        private SpriteRenderer _legStep;
        private SpriteRenderer _face;          // part kind 4 — đổi sang sprite blink theo nhịp jar
        private Sprite _faceNormal, _faceBlink;
        private Vector2 _facePosNormal, _facePosBlink;
        private int _phase;        // lệch pha mỗi avatar (jar: tham số var5) để không nhún/nháy đồng loạt
        private bool _moving;
        private bool _showStep;
        private bool _blinking;

        public static AvatarAppearance Create(Transform parent, int gender)
        {
            var go = new GameObject("Appearance");
            go.transform.SetParent(parent, false);
            var appearance = go.AddComponent<AvatarAppearance>();
            appearance._visual = go.transform;
            appearance._phase = Random.Range(0, 512);
            appearance.Build(gender);
            return appearance;
        }

        private void Build(int gender)
        {
            // Thân nhún (idle-bob) nằm dưới _body; bóng và chân bám đất nằm thẳng dưới _visual.
            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(_visual, false);
            _body = bodyGo.transform;

            // v.java dịch gốc về (x-27,y-74). image 15 nằm tại (27,68), anchor HCENTER|TOP.
            Add("Shadow", FullSprite(15, new Vector2(0.5f, 1f)), new Vector2(0f, 6f), 0, false);

            // avatar/0 là atlas da 33x56: thân 33x42 + hai frame chân ở hàng cuối.
            Add("Skin Body", CropSprite(0, 0, 0, 33, 42), new Vector2(-17f, 52f), 1);
            _legNormal = Add("Skin Legs 1", CropSprite(0, 0, 42, 11, 14), new Vector2(-6f, 14f), 2, false);
            _legStep = Add("Skin Legs 2", CropSprite(0, 11, 42, 16, 14), new Vector2(-7f, 14f), 2, false);
            _legStep.gameObject.SetActive(false);

            // Thứ tự layer là trường d trong ab.java: -5, -4, -4, -4, -3, -2.
            var parts = gender != 0
                ? new[] { new Part(2, 14, 0, 15, 10), new Part(3, 10, -1, 14, 11),
                          new Part(4, 8, 18, 35, 12), new Part(9, 6, 21, 65, 13),
                          new Part(8, 12, 22, 59, 14), new Part(7, 4, 22, 51, 15) }
                : new[] { new Part(2, 13, 7, 15, 10), new Part(3, 9, 0, 14, 11),
                          new Part(4, 7, 18, 36, 12), new Part(9, 5, 21, 65, 13),
                          new Part(8, 11, 21, 59, 14), new Part(7, 3, 18, 50, 15) };

            foreach (var part in parts) AddPart(part);
        }

        private void AddPart(Part part)
        {
            var top = 74f - part.Y;
            if (part.Kind != 8 && part.Kind != 9)
            {
                var pos = new Vector2(part.X - 27f, top);
                var renderer = Add($"Part {part.Image}", FullSprite(part.Image, new Vector2(0f, 1f)),
                    pos, part.Layer);
                if (part.Kind == 4)
                {
                    // Mặt: jar chớp mắt bằng cách vẽ avatar image 1 (18x11) thay ảnh mặt thường,
                    // tại jar (21,38) — xem v.java dòng 54-59. Nạp sẵn để đổi qua lại khi blink.
                    _face = renderer;
                    _faceNormal = renderer.sprite;
                    _facePosNormal = pos;
                    _faceBlink = FullSprite(1, new Vector2(0f, 1f));
                    _facePosBlink = new Vector2(21f - 27f, 74f - 38f);
                }
                return;
            }

            // Ảnh tay/chân loại 8/9 chứa hai frame đặt cạnh nhau. v.java chỉ vẽ
            // một vùng mỗi nhịp; dùng nguyên ảnh là nguyên nhân bộ phận văng xa thân.
            var source = JarSkin.Bank("avatar", part.Image);
            var width = Mathf.RoundToInt(source.rect.width);
            var height = Mathf.RoundToInt(source.rect.height);
            var firstWidth = 54 - part.X;
            var secondX = part.Kind == 9 ? width - firstWidth - 3 : width - firstWidth + 1;
            var secondWidth = width - secondX;
            var normal = Add($"Part {part.Image}",
                CropSprite(part.Image, 0, 0, firstWidth, height),
                new Vector2(part.X - 27f, top), part.Layer);
            var step = Add($"Part {part.Image} Step",
                CropSprite(part.Image, secondX, 0, secondWidth, height),
                new Vector2(-27f, top), part.Layer);
            step.gameObject.SetActive(false);
            _switchParts.Add(new SwitchPart { Normal = normal, Step = step });
        }

        private SpriteRenderer Add(string name, Sprite sprite, Vector2 position, int layer, bool bob = true)
        {
            var child = new GameObject(name, typeof(SpriteRenderer));
            child.transform.SetParent(bob ? _body : _visual, false);
            child.transform.localPosition = position;
            var renderer = child.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            _renderers.Add((renderer, layer));
            return renderer;
        }

        private static Sprite FullSprite(int index, Vector2 pivot)
        {
            var source = JarSkin.Bank("avatar", index);
            var key = $"{index}|full|{pivot.x}|{pivot.y}";
            if (SpriteCache.TryGetValue(key, out var cached) && cached != null) return cached;
            return SpriteCache[key] = Sprite.Create(source.texture, source.rect, pivot, 1f);
        }

        private static Sprite CropSprite(int index, int left, int top, int width, int height)
        {
            var key = $"{index}|{left}|{top}|{width}|{height}";
            if (SpriteCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var source = JarSkin.Bank("avatar", index);
            var rect = new Rect(source.rect.x + left,
                source.rect.y + source.rect.height - top - height, width, height);
            return SpriteCache[key] = Sprite.Create(source.texture, rect, new Vector2(0f, 1f), 1f);
        }

        public void SetFacing(int direction)
        {
            if (direction == 0) _visual.localScale = Vector3.one;
            else if (direction == 1) _visual.localScale = new Vector3(-1f, 1f, 1f);
        }

        public void SetMoving(bool moving) => _moving = moving;

        public void SetSortingOrder(int order)
        {
            foreach (var item in _renderers) item.renderer.sortingOrder = order + item.layer;
        }

        private void Update()
        {
            var ticks = Mathf.FloorToInt(Time.time * TicksPerSecond) + _phase;
            ApplyStep(_moving && ticks % 8 < 4);      // jar: chân đổi frame mỗi 4 tick khi đi
            ApplyBob(_moving ? 0 : ((ticks >> 5) & 1) * IdleBobPixels); // đứng thì thân nhún mỗi 32 tick
            ApplyBlink(ticks / 5 % 16 == 13);           // jar: nháy mắt ~5 tick mỗi 80 tick
        }

        private void ApplyStep(bool step)
        {
            if (step == _showStep) return;
            _showStep = step;
            _legNormal.gameObject.SetActive(!step);
            _legStep.gameObject.SetActive(step);
            foreach (var item in _switchParts)
            {
                item.Normal.gameObject.SetActive(!step);
                item.Step.gameObject.SetActive(step);
            }
        }

        private void ApplyBob(int bobDownPx)
        {
            if (_body == null) return;
            var pos = _body.localPosition;
            if (Mathf.Approximately(pos.y, -bobDownPx)) return; // jar Y xuống; Unity Y lên
            pos.y = -bobDownPx;
            _body.localPosition = pos;
        }

        private void ApplyBlink(bool blink)
        {
            if (_face == null || blink == _blinking) return;
            _blinking = blink;
            _face.sprite = blink ? _faceBlink : _faceNormal;
            _face.transform.localPosition = blink ? (Vector3)_facePosBlink : (Vector3)_facePosNormal;
        }
    }
}
