using System;
using Gopet.Net.Images;
using Gopet.Net.Map;
using Gopet.Runtime.Assets;
using Gopet.UiLogic;
using UnityEngine;

namespace Gopet.Runtime.World
{
    /// <summary>Phần dựng object tĩnh của <see cref="WorldActorView"/> — tách khỏi phần
    /// instance/Update để mỗi file giữ dưới 200 dòng.</summary>
    public sealed partial class WorldActorView
    {
        public static WorldActorView CreateNpc(Transform parent, NpcSpawn npc, int mapHeight,
            RemoteAssetCache assets, Action<int> clicked)
        {
            // NPC đứng CỐ ĐỊNH tại (x,y) như jar (class `dg` type 1): KHÔNG đi ngang, KHÔNG
            // lật hướng (đó là type 2/3), chỉ NHÚN dọc tại chỗ mỗi 400ms (jar toggle `j`
            // tách sprite ép dọc). Ở đây ép scale.y quanh gốc chân (pivot 0.5,0) nên chân
            // bám đất y như jar.
            // Tên NPC trong DB là HOA TOÀN BỘ vì font bitmap jar thiếu chữ hoa có dấu —
            // xem NpcDisplayNames. Tên object giữ nguyên bản gốc cho dễ tra trong Hierarchy.
            // NPC trang trí: không tên, không nhận bấm (null → không có nhãn, tắt collider).
            var decoration = npc.IsDecoration;
            var view = Create(parent, $"NPC {npc.Id} {npc.Name}", npc.ImagePath,
                decoration ? null : NpcDisplayNames.Prettify(npc.Name),
                npc.X, npc.Y, mapHeight, npc.FrameCount, assets,
                decoration ? null : () => clicked?.Invoke(npc.Id), npc.Bounds);
            view.EnableIdleBob();
            if (decoration) return view;
            var hint = NpcPurposeHints.Get(npc);
            if (!string.IsNullOrWhiteSpace(hint))
            {
                var guide = view.gameObject.AddComponent<NpcPurposeBubble>();
                guide.Configure(view, hint);
            }
            return view;
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
            // Chuẩn hoá MỘT lần ở đây để cả tra ảnh cục bộ lẫn gói xin ảnh dùng chung một
            // dạng đường dẫn — vài dòng `npc.imgPath` trong DB viết kiểu Windows.
            imagePath = JarAssetPath.Normalize(imagePath);
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
            PixelSnapVisual.Attach(spriteGo.transform);
            view._renderer.sortingOrder = MapPlacement.ActorSortingOrder(jarY);
            view._clicked = clicked;
            if (labelText != null) view.MakeLabel(labelText, view._renderer.sortingOrder + 20);
            view.ConfigureCollider(bounds);
            // Không nhận bấm thì không chặn cú chạm đi xuyên xuống mặt đất phía sau.
            go.GetComponent<BoxCollider2D>().enabled = clicked != null;

            // Ảnh NPC/quái vốn do jar tải qua mạng (`dg.a` gọi `cp.a(path, 2)`), nhưng
            // phần lớn đã có sẵn cục bộ (unpack từ asset gốc vào Resources/Jar/Art/Raw/npcs).
            // Dùng ngay bản cục bộ nếu có — khỏi chờ round-trip server, và không phụ
            // thuộc server có phục vụ đúng file hay không. Vắng bản cục bộ mới xin mạng.
            var localTexture = JarActorSprites.LoadLocalTexture(imagePath);
            if (localTexture != null)
            {
                view._frames = SpriteFrameCache.Slice(imagePath, localTexture, frameCount);
                view._frame = 0;
                view._renderer.sprite = view._frames[0];
                view.ConfigureCollider(bounds);
                view.PlaceLabel();
            }
            else
            {
                assets.Get(imagePath, ImagePackets.TypeNpc, view, texture =>
                {
                    if (view == null || texture == null) return;
                    view._frames = SpriteFrameCache.Slice(imagePath, texture, frameCount);
                    view._frame = 0;
                    view._renderer.sprite = view._frames[0];
                    view.ConfigureCollider(bounds);
                    view.PlaceLabel();
                });
            }
            return view;
        }
    }
}
