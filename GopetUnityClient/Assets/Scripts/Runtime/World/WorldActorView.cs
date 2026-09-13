using System;
using System.Collections.Generic;
using Gopet.Net.Images;
using Gopet.Net.Map;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gopet.Runtime.World
{
    /// <summary>NPC hoặc quái nhận động từ server, có ảnh động và nhãn tên.</summary>
    public sealed class WorldActorView : MonoBehaviour, IPointerClickHandler
    {
        private const float NameScale = 0.75f; // cỡ tên NPC/quái, đồng bộ với PlayerAvatar
        private static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();
        private SpriteRenderer _renderer;
        private Transform _visual;   // node chứa sprite; idle-bob ép scale.y node NÀY, không đụng nhãn tên
        private JarNameLabel _label; // tên bằng bitmap font jar, đồng bộ với PlayerAvatar
        private Sprite[] _frames = Array.Empty<Sprite>();
        private Action _clicked;
        private float _nextFrame;
        private int _frame;
        private bool _idleBob;
        private float _bobNext;
        private bool _bobDown;
        private string _baseLabelText;
        private int _labelOrder;

        public int? BossHp { get; private set; }

        /// <summary>Vị trí bong bóng cao hơn tên NPC và đỉnh sprite.</summary>
        internal float PurposeBubbleOffsetY
        {
            get
            {
                var height = _renderer != null && _renderer.sprite != null
                    ? _renderer.sprite.rect.height : 48f;
                // Nhãn tên nằm ở đỉnh sprite + 4px. Chừa thêm một khoảng nhỏ để
                // bong bóng sát đầu NPC nhưng không đè lên nhãn tên.
                return height + 16f;
            }
        }

        public static WorldActorView CreateNpc(Transform parent, NpcSpawn npc, int mapHeight,
            RemoteAssetCache assets, Action<int> clicked)
        {
            // NPC đứng CỐ ĐỊNH tại (x,y) như jar (class `dg` type 1): KHÔNG đi ngang, KHÔNG
            // lật hướng (đó là type 2/3), chỉ NHÚN dọc tại chỗ mỗi 400ms (jar toggle `j`
            // tách sprite ép dọc). Ở đây ép scale.y quanh gốc chân (pivot 0.5,0) nên chân
            // bám đất y như jar.
            var view = Create(parent, $"NPC {npc.Id} {npc.Name}", npc.ImagePath, npc.Name,
                npc.X, npc.Y, mapHeight, npc.FrameCount, assets, () => clicked?.Invoke(npc.Id), npc.Bounds);
            view.EnableIdleBob();
            var hint = NpcPurposeHints.Get(npc);
            if (!string.IsNullOrWhiteSpace(hint))
            {
                var guide = view.gameObject.AddComponent<NpcPurposeBubble>();
                guide.Configure(view, hint);
            }
            return view;
        }

        /// <summary>Nhún dọc tại chỗ 400ms/nhịp — mô phỏng `dg` type 1 của jar.</summary>
        public void EnableIdleBob()
        {
            _idleBob = true;
            _bobNext = Time.time + UnityEngine.Random.Range(0f, 0.4f); // lệch pha, NPC không nhún đồng loạt
        }

        public static WorldActorView CreateMob(Transform parent, MobSpawn mob, int mapHeight,
            RemoteAssetCache assets, Action<int> clicked = null)
        {
            // Dấu boss dùng '*' (★ không có trong charset font jar sẽ thành khoảng trắng).
            var name = mob.IsBoss ? $"* {mob.Name} Lv.{mob.Level}" : $"{mob.Name} Lv.{mob.Level}";
            return Create(parent, $"Mob {mob.Id} {mob.Name}", mob.ImagePath, name,
                mob.X, mob.Y + mob.VerticalOffset, mapHeight, mob.FrameCount, assets,
                () => clicked?.Invoke(mob.Id), null);
        }

        private static WorldActorView Create(Transform parent, string objectName, string imagePath,
            string labelText, int jarX, int jarY, int mapHeight, int frameCount,
            RemoteAssetCache assets, Action clicked, int[] bounds)
        {
            var go = new GameObject(objectName, typeof(BoxCollider2D));
            go.transform.SetParent(parent, false);
            var (x, y) = MapPlacement.JarToWorld(jarX, jarY, mapHeight);
            go.transform.localPosition = new Vector3(x, y, 0f);
            var view = go.AddComponent<WorldActorView>();
            // Sprite ở node con để idle-bob (ép scale.y) chỉ ảnh hưởng ảnh, không méo nhãn tên.
            var spriteGo = new GameObject("Sprite", typeof(SpriteRenderer));
            spriteGo.transform.SetParent(go.transform, false);
            view._visual = spriteGo.transform;
            view._renderer = spriteGo.GetComponent<SpriteRenderer>();
            view._renderer.sortingOrder = MapPlacement.ActorSortingOrder(jarY);
            view._clicked = clicked;
            view.MakeLabel(labelText, view._renderer.sortingOrder + 20);
            view.ConfigureCollider(bounds);

            // Ảnh NPC/quái vốn do jar tải qua mạng (`dg.a` gọi `cp.a(path, 2)`), nhưng
            // phần lớn đã có sẵn cục bộ (unpack từ asset gốc vào Resources/Jar/Art/Raw/npcs).
            // Dùng ngay bản cục bộ nếu có — khỏi chờ round-trip server, và không phụ
            // thuộc server có phục vụ đúng file hay không. Vắng bản cục bộ mới xin mạng.
            var localTexture = JarActorSprites.LoadLocalTexture(imagePath);
            if (localTexture != null)
            {
                view._frames = Frames(imagePath, localTexture, Mathf.Max(1, frameCount));
                view._frame = 0;
                view._renderer.sprite = view._frames[0];
                view.ConfigureCollider(bounds);
                view.PlaceLabel();
            }
            else
            {
                assets.Get(imagePath, ImagePackets.TypeNpc, texture =>
                {
                    if (view == null || texture == null) return;
                    view._frames = Frames(imagePath, texture, Mathf.Max(1, frameCount));
                    view._frame = 0;
                    view._renderer.sprite = view._frames[0];
                    view.ConfigureCollider(bounds);
                    view.PlaceLabel();
                });
            }
            return view;
        }

        private void MakeLabel(string value, int order)
        {
            // Nhãn nằm TRÊN root (không dưới _visual) nên không bị idle-bob ép méo.
            _baseLabelText = value ?? string.Empty;
            _labelOrder = order;
            _label = JarNameLabel.Create(transform, Vector3.zero, NameScale, _baseLabelText);
            _label.SetSortingOrder(order);
            PlaceLabel();
        }

        public void SetBossHp(int hp)
        {
            BossHp = Mathf.Max(0, hp);
            if (_label == null) return;
            _label.SetText($"{_baseLabelText}  HP {BossHp.Value}");
            _label.SetSortingOrder(_labelOrder);
        }

        private void PlaceLabel()
        {
            if (_label == null) return;
            var height = _renderer != null && _renderer.sprite != null
                ? _renderer.sprite.rect.height : 48f;
            _label.transform.localPosition = new Vector3(0f, height + 4f, 0f); // đáy tên trên đỉnh sprite
        }

        private void ConfigureCollider(int[] bounds)
        {
            var collider = GetComponent<BoxCollider2D>();
            var width = _renderer?.sprite != null ? _renderer.sprite.bounds.size.x : 32f;
            var height = _renderer?.sprite != null ? _renderer.sprite.bounds.size.y : 48f;
            if (bounds != null && bounds.Length == 4)
            {
                // Bounds server thường chỉ là ô chân NPC (-25,-25,50,50). Hợp cả
                // ô này lẫn toàn thân sprite để người chơi chạm vào đầu/thân NPC
                // vẫn mở được hội thoại.
                var left = Mathf.Min(-width * 0.5f, bounds[0]);
                var right = Mathf.Max(width * 0.5f, bounds[0] + bounds[2]);
                var bottom = Mathf.Min(0f, bounds[1]);
                var top = Mathf.Max(height, bounds[1] + bounds[3]);
                width = Mathf.Max(12f, right - left);
                height = Mathf.Max(12f, top - bottom);
                collider.offset = new Vector2((left + right) * 0.5f, (bottom + top) * 0.5f);
            }
            else collider.offset = new Vector2(0f, height * 0.5f);
            collider.size = new Vector2(width, height);
        }

        private static Sprite[] Frames(string path, Texture2D texture, int count)
        {
            if (texture.width < count) count = 1;
            var key = $"{path}|{texture.GetHashCode()}|{count}";
            if (FrameCache.TryGetValue(key, out var cached) && cached != null) return cached;
            var width = texture.width / count;
            var result = new Sprite[count];
            for (var i = 0; i < count; i++)
                result[i] = Sprite.Create(texture, new Rect(i * width, 0, width, texture.height),
                    new Vector2(0.5f, 0f), 1f);
            return FrameCache[key] = result;
        }

        private void Update()
        {
            IdleBob();
            // Frame animation cho sprite nhiều frame (quái). NPC map là 1-frame (server gửi
            // frameCount=1) nên không đổi frame — chỉ nhún qua IdleBob, khớp jar.
            if (_frames.Length <= 1 || Time.time < _nextFrame) return;
            _nextFrame = Time.time + 0.16f;
            _frame = (_frame + 1) % _frames.Length;
            _renderer.sprite = _frames[_frame];
        }

        private void IdleBob()
        {
            if (!_idleBob || Time.time < _bobNext) return;
            _bobNext = Time.time + 0.4f;
            _bobDown = !_bobDown;
            if (_visual == null) return;
            var scale = _visual.localScale;
            scale.y = _bobDown ? 0.95f : 1f; // ép dọc SPRITE quanh chân (pivot 0.5,0), chân đứng yên
            _visual.localScale = scale;
        }

        public void OnPointerClick(PointerEventData eventData) => _clicked?.Invoke();
    }
}
