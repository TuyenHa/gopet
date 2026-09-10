# Cook Report — Phase 6 (Map Rendering & Movement) — 5 batches

Session: 2026-09-06 20:00 → 21:35 (~1h30)
Plan: `plans/260905-1355-gopet-unity-client-rebuild/phase-06-map-rendering-movement.md`

## Kết quả

| | Trước | Sau |
|---|---|---|
| Test | 379 | **438** (+59) |
| verify.ps1 | 10/10 ✓ | 10/10 ✓ |
| P6 status | pending | **in-progress** |

## 5 batch đã ship

### Batch 1 — Parser + assets
- **Bằng chứng thực nghiệm**: `scratchpad/scan-maps` (dotnet console) chạy `JarMapLayout.Parse` trên cả 24 map, không dư 1 byte
- Mở rộng `JarMapLayout` với `Collision` (đang đọc rồi vứt ở P5.1) + `Entities` (nhà/NPC/cổng dịch chuyển từ `eg.java`) + `Waypoints` (từ `z.java`)
- Tách `JarBigEndianReader` (dùng chung) và `JarMapEntity`/`JarMapWaypoint` (POCO) ra file riêng để giữ dưới rule 200 dòng
- Test: 24 map × Theory + 3 test collision/entity/waypoint

### Batch 2 — MapRenderer + TileAssetProvider
- `TileAssetProvider` — cache tĩnh (imageId × 100 + cellIdx) → Sprite, dùng chung UI login lẫn world gameplay
- `MapRenderer` (world-space) — SpriteRenderer per cell/object, Y-sort objects
- `MapPlacement` (pure C#) — jar↔world coord + sorting math
- **DRY win**: refactor `JarMapBackground` (P5.1) 181→151 dòng, xoá `CellSprite` trùng lặp
- **Lệch plan có chủ đích**: KHÔNG dùng `UnityEngine.Tilemaps.Tilemap`, giữ pattern SpriteRenderer đã chứng minh chạy ở P5.1

### Batch 3 — INIT_PLAYER + ON_PLAYER_ENTER_MAP + PlayerAvatar
- `MapHandler` (thuần C#) — parse opcode 31, 24, 30
- `MapEvents.cs` — `PlayerInit`/`PlayerEnterMap`/`PlayerExitPlace` POCO
- `PlayerAvatar` — dựng theo gender (avatar/0 nam, avatar/1 nữ), sort Y theo target
- `MapScene` — cầu nối MapHandler↔Unity, quản lý dictionary avatar theo userId

### Batch 4 — MovementController + ON_OTHER_USER_MOVE + interpolation
- `MapHandler.SendMove` + `PlayerMoved` event (đọc `points[len-2..len-1]` làm đích, khớp `GameController.cs`)
- `MoveThrottle` (pure) — cooldown 2000ms khớp `TIME_MOVE_SEND` server
- `PositionInterpolator` (pure) — exponential smoothing, `RatePerSecond` cấu hình
- `MovementController` (Unity) — Mouse/Touch → screen→world→jar → `SendMove`. Dùng **Input System** (Mouse.current/Touchscreen.current) vì project tắt legacy Input
- **Bẫy trả giá**: namespace `Gopet.Runtime.World` xung đột với `UnityEngine.Input` — compiler tra `Gopet.Runtime.Input` trước. Fix bằng `UnityEngine.InputSystem.Mouse/Touchscreen`

### Batch 5 — ChatHandler + ChatBubble
- `ChatHandler` — opcode 9 `ON_PLACE_CHAT` cả 2 chiều. Client gửi CHỈ UTF text (server biết user qua session), server broadcast `int userId + UTF text`
- `PlaceChat.IsPetInteraction` phân biệt 3 từ khoá `kiss`/`play`/`poke` (server dịch thành animation, không phát chat)
- `ChatBubble` (world-space TextMesh, tự huỷ 3s) — bong bóng tối giản, tầng animation pet để sau

## File mới (10 file)

**Runtime/World/** (Unity):
- `TileAssetProvider.cs` (55)
- `MapRenderer.cs` (125)
- `MapScene.cs` (114)
- `PlayerAvatar.cs` (85)
- `MovementController.cs` (93)
- `ChatBubble.cs` (61)

**Net/Map/** + **Net/Chat/**:
- `MapEvents.cs` (67)
- `MapHandler.cs` (127)
- `ChatHandler.cs` (65)

**UiLogic/** (pure C#, testable):
- `JarBigEndianReader.cs` (56) — tách từ JarMapLayout
- `JarMapEntity.cs` (41) — POCO Entity + Waypoint
- `MapPlacement.cs` (63)
- `MoveThrottle.cs` (40)
- `PositionInterpolator.cs` (52)

**Tests** (+6 files, +59 tests):
- `JarMapAllMapsTests.cs` (24 map theory)
- `MapPlacementTests.cs` (11)
- `MapHandlerTests.cs` (6)
- `MoveThrottleTests.cs` (4)
- `PositionInterpolatorTests.cs` (5)
- `ChatHandlerTests.cs` (6)

## File refactor

- `Runtime/UI/JarMapBackground.cs`: 181→151 dòng (dùng `TileAssetProvider`)
- `UiLogic/JarMapLayout.cs`: 158→173 (thêm Collision/Entities/Waypoints, tách Reader/Entity)
- `tests/…/JarMapLayoutTests.cs`: +3 test collision/entity/waypoint

## Còn nợ (không blocking vertical slice)

1. **PlayMode/Editor xác nhận bằng mắt** — code compile + unit test xanh, nhưng chưa mở Editor bấm Play. Không có ảnh chụp map render thật.
2. **Camera pixel-perfect** — chưa cấu hình `Camera.orthographic = true`, `pixelsPerUnit`, `FilterMode.Point` cho Editor. Cần một bootstrap `MapScene.Create` đầy đủ.
3. **End-to-end với server** — chưa nối vào `GopetClient.Update()` (dispatch loop) và chưa chạy thử với server thật. Cần đăng ký `MapHandler`/`ChatHandler` cùng `AuthHandler` trong bootstrap.
4. **Diff dump** — `PacketLogger` đã có sẵn nhưng chưa chạy so sánh vs client cũ để đối chiếu byte-for-byte.
5. **NPC/animation** — `JarMapEntity` đã đọc từ format, nhưng chưa dựng NPC lên map hay xử lý click NPC (thuộc P7 hoặc phase interaction sau).
6. **9 file `newMapData/*_b`** — vẫn chưa giải, giữ đề xuất hoãn P8.
7. **Fps đo trên Android** — chưa có build APK, chưa đo.

## Câu hỏi mở

- **Camera follow avatar** hay **camera cố định giữa map**? Vertical slice hiện thiếu, cần chốt trước khi test end-to-end.
- **Pathfinding client-side**? Hiện gửi 1 điểm đích (`points = [x, y]`), khớp thao tác chạm-để-đi. Nếu muốn animation "đi qua từng ô", cần thuật toán A* trên `map.Collision` — thuộc polish, không blocker.
- **Chat input UI** — chưa có ô nhập chat. Hiện chỉ `ChatHandler.SendChat(text)` gọi được từ code. Cần button/text field UI (làm cùng P7 hay riêng?).

## Metrics

- Verify.ps1: 10/10 mỗi batch, cuối cùng cũng 10/10
- Tests: 379 → 438 (+59, +15%)
- Không file nào vượt rule 200 dòng
- 0 warning mới, chỉ giữ 6 warning cũ CS0618 của P5 (`FindObjectsSortMode`)
