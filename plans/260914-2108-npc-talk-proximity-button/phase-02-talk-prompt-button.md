# Phase 02 — Nút world-space "Nói chuyện" + wiring theo khoảng cách

## Context
- [plan.md](plan.md) · [phase-01](phase-01-proximity-core.md)
- File đọc: `Runtime/World/WorldActorView.cs` (+ `.Factory.cs`), `WorldActorLayer.cs`,
  `NpcPurposeBubble.cs`, `ChatBubble.cs`, `ChatBubble.Sprite.cs`, `MapScene.cs`,
  `GameSession.cs:139-147`

## Overview
- Priority: P1 — Status: pending — Effort: 1.5h
- Thêm affordance: nút nổi trên đầu NPC gần nhất trong tầm, bấm → gọi đúng callback talk đã
  được truyền vào `CreateNpc` (`GuiderHandler.TalkToNpc`).

## Key Insights (đã xác minh)
- Chuỗi wiring hiện có: `GameSession.cs:141` → `MapScene.SubscribeWorld` (MapScene.cs:193-199)
  → `WorldActorLayer.Subscribe` lưu `_talkToNpc` (WorldActorLayer.cs:16,33) →
  `WorldActorView.CreateNpc(..., clicked)` bind `() => clicked?.Invoke(npc.Id)` (dòng 53) →
  gán vào `_clicked` (dòng 96) → `OnPointerClick` (dòng 211). **Nút chỉ cần gọi lại `_clicked`,
  KHÔNG cần luồng dữ liệu mới, KHÔNG đụng GameSession/MapScene.**
- `ChatBubble.AttachOrUpdate` tìm `anchor.GetComponentInChildren<ChatBubble>()` (ChatBubble.cs:44)
  → nếu nút tái dùng component `ChatBubble` thì `NpcPurposeBubble` (Update mỗi 11s) sẽ GHI ĐÈ
  text nút thành lời thoại. ⇒ Nút phải là class riêng.
- `ChatBubble.OnPointerClick` (ChatBubble.cs:115-118) đã forward click về `WorldActorView` →
  bong bóng chồng lên nút cũng không gây hành vi sai (cùng đích talk).
- Bong bóng thoại đặt tại `PurposeBubbleOffsetY = spriteHeight + 16` (WorldActorView.cs:33-43),
  cao ~18-46px → nút đặt tại `PurposeBubbleOffsetY + 52` để không đè.
- `ChatBubble` dùng `SpriteRenderer` + `TextMesh` + `BoxCollider2D` với sortingOrder 20_000/20_001
  — đúng khuôn để bắt chước; `Physics2DRaycaster` đã có sẵn nên collider nhận `IPointerClickHandler`.
- Di chuyển bằng joystick/phím (`MovementController.Update` → `ReadDirection`), không click-to-move
  ⇒ bấm nút không vô tình ra lệnh đi.

## Requirements
**Functional**
- Nút hiện khi người chơi trong `NpcProximity.ShowRadius` của NPC gần nhất; ẩn khi vượt
  `HideRadius`, khi đổi map, khi chưa có avatar self.
- Bấm nút → gọi đúng `talkToNpc(npc.Id)`; click sprite NPC vẫn hoạt động như cũ.
- Tối đa 1 nút hiện cùng lúc trên toàn map.

**Non-functional**
- Quét khoảng cách 10 lần/giây (throttle 0.1s), brute-force trên `_npcs` (5-10 NPC).
- Không cấp phát mỗi lần quét (tái dùng `List<NpcPoint>`).
- File mới < 200 dòng; label tiếng Việt có dấu → dùng `UiBuilder.BuiltinFont()` như `ChatBubble`
  (font bitmap jar `JarNameLabel` có charset hạn chế, KHÔNG dùng cho chữ "Nói chuyện").

## Architecture
```
MovementController (player đi)
      │  (state: transform)
WorldActorLayer.Update()  ── throttle 0.1s ──►  _scene.Self == null ? ẩn hết
      │ build List<NpcPoint> từ _npcs (localPosition)
      ▼
NpcProximity.Pick(...) ─► id mới  ≠ _promptNpcId ?
      │ yes → view cũ.SetTalkPromptVisible(false); view mới.SetTalkPromptVisible(true)
      ▼
WorldActorView.SetTalkPromptVisible(bool)   (lazy attach NpcTalkPrompt lần đầu)
      ▼
NpcTalkPrompt (SpriteRenderer + TextMesh "Nói chuyện" + BoxCollider2D, IPointerClickHandler)
      │ onClick
      ▼
_clicked  →  talkToNpc(npc.Id)  →  GuiderHandler.TalkToNpc → server
      ▼
ON_NPC_OPTIONS → GuiderHandler.NpcOptionsShown → UiRoot.ShowNpcOptions → ChoiceDialogView (phase 03)
```

## Related Code Files
**Tạo mới**
- `Assets/Scripts/Runtime/World/NpcTalkPrompt.cs`
  - `public static NpcTalkPrompt Attach(Transform npc, float offsetY, System.Action onClick)`
  - `public void SetVisible(bool value)` (bật/tắt `gameObject`)
  - `OnPointerClick` → `onClick`
  - Vẽ: panel bo góc + viền (tái dùng công thức màu của `ChatBubble`: viền `(18,64,83)`,
    nền `(239,250,255)`), chữ 16-18px màu đậm, kích thước cố định ~92x26px (đủ cho "Nói chuyện")
    → sprite dựng 1 lần, cache static theo (w,h).

**Sửa**
- `Assets/Scripts/Runtime/World/WorldActorView.cs` — thêm `_talkPrompt` + `SetTalkPromptVisible(bool)`
  (lazy `NpcTalkPrompt.Attach(transform, PurposeBubbleOffsetY + PromptGap, () => _clicked?.Invoke())`).
- `Assets/Scripts/Runtime/World/WorldActorLayer.cs` — thêm `Update()` throttle + `_promptNpcId`
  (khởi tạo `NpcProximity.None`), reset trong `Clear()` và cuối `OnNpcsReceived`.

**Không đụng:** `GameSession.cs`, `MapScene.cs`, `GuiderHandler.cs`, `GuiderPackets.cs`,
`UiRoot.cs`, `NpcPurposeHints.cs`.

## Implementation Steps
1. `NpcTalkPrompt.cs`: tạo GameObject con `"TalkPrompt"` (BoxCollider2D) dưới transform NPC,
   `localPosition = (0, offsetY, 0)`; con `"Panel"` (SpriteRenderer, sortingOrder 20_050) và
   `"Label"` (TextMesh, sortingOrder 20_051, `UiBuilder.BuiltinFont()`, `characterSize` chỉnh
   theo mẫu `ChatBubble.CreateText` để chữ ra đúng cỡ world-space).
2. Sprite panel: hàm static `PromptSprite(int w, int h)` dựng `Texture2D` supersample x2, bo góc
   6px, viền 1.5px — giữ trong chính file; cache `Dictionary<long, Sprite>` + `HideFlags.HideAndDontSave`
   như `ChatBubble.Sprite.cs:19,45`. KHÔNG sửa `ChatBubble.Sprite.cs` (tránh hồi quy giao diện chat).
3. Collider khớp panel; `SetVisible` chỉ bật/tắt `gameObject` (không destroy → không rác GC khi
   ra vào tầm liên tục).
4. `WorldActorView`: thêm `private const float PromptGap = 52f;`, field `_talkPrompt`,
   `internal void SetTalkPromptVisible(bool value)` — nếu `value` và `_talkPrompt == null` thì
   Attach; nếu `!value` và null thì return sớm.
5. `WorldActorLayer`: field `_nextScan`, `_promptNpcId = NpcProximity.None`, `_points = new List<NpcPoint>()`.
   `Update()`: `if (Time.time < _nextScan) return; _nextScan = Time.time + 0.1f;`
   lấy `var self = _scene?.Self;` → null thì `SetPrompt(NpcProximity.None)` và return;
   clear+fill `_points` từ `_npcs` (bỏ view null), gọi `NpcProximity.Pick(self.transform.localPosition.x,
   self.transform.localPosition.y, _points, _promptNpcId)`, rồi `SetPrompt(id)`.
6. `SetPrompt(int id)`: nếu `id == _promptNpcId` return; ẩn view cũ (nếu còn trong `_npcs`),
   hiện view mới, gán `_promptNpcId = id`.
7. `Clear()` và `OnNpcsReceived` (sau khi `ClearActors`): `_promptNpcId = NpcProximity.None`.
8. Chạy `verify.ps1` (step 6 + step 10).

## Todo List
- [ ] `NpcTalkPrompt.cs` (Attach/SetVisible/OnPointerClick/PromptSprite + cache)
- [ ] `WorldActorView.SetTalkPromptVisible` + `PromptGap`
- [ ] `WorldActorLayer.Update` throttle 0.1s + `_promptNpcId` + reset khi đổi map
- [ ] Không file nào vượt 200 dòng
- [ ] `verify.ps1` xanh

## Success Criteria
- Trong Unity (map 11): đi tới gần TRAN CHAN → nút hiện trên đầu NPC, không đè nhãn tên/bong bóng.
- Bấm nút → popup option xuất hiện (nội dung do phase 03 làm đẹp).
- Đi ra xa → nút biến mất; đi tới NPC khác → nút chuyển sang NPC đó, không bao giờ có 2 nút.
- Đổi map/portal → không còn nút mồ côi.
- Click thẳng sprite NPC vẫn mở thoại (không hồi quy).

## Risk Assessment
| Rủi ro | KN | TĐ | Giảm thiểu |
|--------|----|----|-----------|
| Nút đè bong bóng `NpcPurposeBubble` | Cao | Thấp | Đặt trên bong bóng (+52px); click cả 2 đều ra talk |
| `_clicked` null (NPC dựng ở test) | Thấp | Thấp | `?.Invoke()` |
| Nút còn sống sau khi NPC bị destroy | Thấp | TB | Nút là con của NPC → destroy theo cha |
| Quét mỗi 0.1s tốn CPU | Thấp | Thấp | 5-10 NPC, so bình phương, không alloc |
| Flicker ở biên tầm | TB | Thấp | Hysteresis 110/150 ở phase 01 |
| Chữ có dấu vỡ font | TB | TB | Dùng `UiBuilder.BuiltinFont()` (Arial dynamic), không dùng JarFont |
| `Self` null lúc mới vào map | Cao | Thấp | Guard null → ẩn hết |

## Security Considerations
Nút chỉ gửi `TalkToNpc(npcId)` với id NPC server vừa gửi cho map hiện tại — không mở rộng bề
mặt tấn công; server vẫn tự validate khoảng cách/quyền như với click cũ.

## Next Steps
→ Phase 03 (popup dọc) để 7 option đọc được; → Phase 04 test.
