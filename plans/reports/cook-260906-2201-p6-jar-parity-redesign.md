# P6 Redesign — Camera + Movement giống bản jar

Session: 2026-09-06 22:00 → 22:30 (~30 min)
Trigger: user câu hỏi mở "camera/pathfinding → làm giống jar"

## Phát hiện từ jar (`ew.java`)

### Camera (`ew.a(int)`, `ew.a(int, boolean)`, dòng 308-312, 545-581, 564-581)
- Follow player, KHÔNG fix trung tâm
- Player ở **2/3 chiều CAO** màn (luôn); ở **1/3 hoặc 2/3 chiều RỘNG** tuỳ `faceDir` (jar: `a()` = face right → 2/3)
- **Clamp cứng** vào biên map — không lộ ngoài
- Viewport = `min(kích thước map, kích thước màn hình)`

### Movement (`ew.c_()`, dòng 436-497)
Hoàn toàn **KHÔNG PHẢI click-to-move** như code cũ:
- Player di chuyển **liên tục** mỗi frame (D-pad giữ)
- Client **lấy mẫu vị trí** vào `Vector this.f` mỗi tick
- **Xoá điểm giữa nếu collinear** với 2 điểm cuối (`ed.a(var24, var27, var20)`) — tối ưu đường thẳng
- Cứ **2 giây** → `r()` flush cả path thành `int[]` gửi server, sau đó `q()` bắt đầu ghi mới
- Ngừng di chuyển sau 2s → `r()` flush lần cuối
- Send format: `cx.a(mapId, unused, dir, points)` = `writeInt(mapId), writeByte(dir), writeInt(unused), writeInt(len), int[len]`

## Thay đổi

### Xoá (YAGNI — thay bằng PathSampler)
- `UiLogic/MoveThrottle.cs` (40 dòng) + 4 test — cooldown 2s không còn cần vì PathSampler tự quản
- `MovementController.TryMoveTo` public API — dùng để test click-to-move nay đã bỏ

### Thêm
| File | Loại | Dòng | Vai trò |
|---|---|---|---|
| `UiLogic/CameraClamp.cs` | pure | 43 | Math camera: EffectiveViewport + TopLeftFor + ClampAxis |
| `UiLogic/PathSampler.cs` | pure | 92 | Sample + collinear removal + 2s flush |
| `Runtime/World/CameraFollower.cs` | Unity | 65 | LateUpdate follow Self, pixel-perfect ortho, khớp `PixelCanvasLayout` |
| `tests/…/CameraClampTests.cs` | test | 51 | 8 test — offset faceDir, clamp, viewport |
| `tests/…/PathSamplerTests.cs` | test | 89 | 8 test — collinear, flush 2s, min 2 điểm |

### Sửa
- `Runtime/World/MovementController.cs` — hoàn toàn rewrite: bỏ Mouse click, đọc WASD/arrow qua `Keyboard.current`, walk continuous, ghép `PathSampler`, update `CameraFollower.FaceRight`
- `Runtime/World/MapScene.cs` — `OnPlayerMoved` bỏ qua echo của chính mình (avoid rubber-band vì MovementController giữ authoritative local)
- `phase-06-map-rendering-movement.md` — mô tả step 8 chi tiết theo redesign

## Metrics

- Test: 438 → **450** (-4 MoveThrottle, +8 PathSampler, +8 CameraClamp)
- verify.ps1: **10/10 xanh**
- File ≤200 dòng: OK
- Wire format: KHÔNG đổi, server không cần biết

## Câu hỏi mở

1. **WalkSpeed = 96 px/s (4 tile/s)** là giá trị đặt — bản jar dùng `playerData.speed`. Cần đối chiếu sau khi có server trả speed thật.
2. **FaceDir 4 chiều (0-3)** hiện dùng — jar dùng 0/1 (right/left) hai giá trị. Server chấp nhận 0-7 tuỳ tính. Chuẩn hoá sau.
3. **Virtual joystick** cho mobile chưa làm — hiện chỉ có phím. UI touch để làm cùng P7 chung với chat input.
4. **Camera zoom** — hiện fix `PixelCanvasLayout.ReferenceHeight = 240` (jar). Không có zoom, đúng tinh thần retro.
