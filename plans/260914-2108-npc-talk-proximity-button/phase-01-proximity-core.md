# Phase 01 — Tách WorldActorView + logic khoảng cách thuần C#

## Context
- [plan.md](plan.md)
- File đọc: `Assets/Scripts/Runtime/World/WorldActorView.cs`, `WorldActorLayer.cs`,
  `Assets/Scripts/UiLogic/MapPlacement.cs`, `tests/Gopet.Net.Tests/MapPlacementTests.cs`

## Overview
- Priority: P1 (chặn phase 02)
- Status: pending — Effort: 1h
- Dựng nền: tách `WorldActorView` cho dưới 200 dòng, và viết logic chọn NPC gần nhất dạng
  thuần C# (testable, không Unity runtime).

## Key Insights (đã xác minh)
- `WorldActorView.cs` = 213 dòng. `verify.ps1:142-149` allowlist KHÔNG có file này → step 10
  đang FAIL trên master. Thêm code vào file này sẽ làm nặng thêm.
- `WorldActorView.Create/CreateNpc/CreateMob` (dòng 45-126) là khối static dựng object, tách
  ra partial được mà không đụng logic Update/anim.
- Id NPC âm (`LinhThuCityNpcOptions.cs:19` — NPC -1 TRAN CHAN) → sentinel dùng `int.MinValue`.
- `MapPlacement.PixelsPerUnit = 1f` → ngưỡng khoảng cách tính thẳng bằng pixel jar.
  Avatar cao ~64px, rộng 48px (`PlayerAvatar.Spawn` collider) → bán kính hiện 110px ≈ 2 thân
  người, hiện đủ sớm mà không hiện khi còn xa nửa màn hình.

## Requirements
**Functional**
- `NpcProximity.Pick(px, py, npcs, currentId)` trả id NPC nên hiện nút, hoặc `NpcProximity.None`.
- Có hysteresis: NPC đang được chọn chỉ mất nút khi ra khỏi `HideRadius`; NPC mới chỉ được
  chọn khi vào trong `ShowRadius`.
- Khi nhiều NPC trong tầm → chọn NPC GẦN NHẤT (tránh 2 nút chồng nhau).

**Non-functional**
- Thuần C#, không `using UnityEngine` (để test trong `tests/Gopet.Net.Tests`, netstandard2.1).
- Không cấp phát mỗi lần gọi (nhận `IReadOnlyList<NpcPoint>`, caller tái dùng `List`).
- Mọi file mới < 200 dòng.

## Architecture
```
WorldActorLayer (phase 02)          UiLogic.NpcProximity (phase 01, thuần C#)
  self.localPosition  ──┐
  _npcs[id].transform ──┼─► List<NpcPoint> ─► Pick(px,py,npcs,currentId) ─► int id | None
                        │                        · giữ currentId nếu d <= HideRadius
                        │                        · else nearest có d <= ShowRadius
                        └─ currentId (state của layer)
```

## Related Code Files
**Tạo mới**
- `Assets/Scripts/UiLogic/NpcProximity.cs` — struct `NpcPoint { int Id; float X, Y; }`,
  hằng `ShowRadius = 110f`, `HideRadius = 150f`, `None = int.MinValue`, hàm `Pick`.
- `Assets/Scripts/Runtime/World/WorldActorView.Factory.cs` — partial chứa `CreateNpc`,
  `CreateMob`, `Create`, `Frames`.
- `tests/Gopet.Net.Tests/NpcProximityTests.cs`

**Sửa**
- `Assets/Scripts/Runtime/World/WorldActorView.cs` — đổi `sealed class` → `sealed partial class`,
  bỏ phần static đã chuyển sang file Factory (còn ~135 dòng).

**Xoá:** không.

## Implementation Steps
1. Tạo `NpcProximity.cs`: struct `NpcPoint` (ctor 3 tham số, field readonly); `Pick` làm 2 lượt
   — (a) nếu `currentId != None` và tìm thấy nó trong danh sách với `d² <= HideRadius²` thì trả
   luôn `currentId`; (b) ngược lại quét tìm `d²` nhỏ nhất `<= ShowRadius²`, trả id đó hoặc `None`.
   So sánh bằng bình phương, không `Sqrt`.
2. Ném `ArgumentNullException` nếu `npcs == null`; danh sách rỗng → `None`.
3. Tạo `WorldActorView.Factory.cs`: copy nguyên văn các thành viên static `CreateNpc` (45-62),
   `CreateMob` (71-79), `Create` (81-126), `Frames` (176-187) + `FrameCache` (dòng 16) sang,
   giữ nguyên `using`. Đổi khai báo class ở cả 2 file thành `sealed partial class WorldActorView`.
4. Xoá đúng các thành viên đã chuyển khỏi `WorldActorView.cs`, giữ nguyên thứ tự phần còn lại.
5. Kiểm tra không file nào > 200 dòng; chạy `powershell -ExecutionPolicy Bypass -File verify.ps1`
   (đặc biệt step 6 Runtime compile và step 10 kích thước file).
6. Viết `NpcProximityTests.cs`: nearest thắng; ngoài `ShowRadius` → `None`; hysteresis (đang
   chọn, ở 130px → vẫn giữ; ở 160px → đổi/None); danh sách rỗng; id âm không bị nhầm sentinel.

## Todo List
- [ ] `UiLogic/NpcProximity.cs` (NpcPoint, ShowRadius, HideRadius, None, Pick)
- [ ] `WorldActorView.Factory.cs` tách phần static, 2 file đều `partial`
- [ ] `WorldActorView.cs` còn < 200 dòng
- [ ] `tests/Gopet.Net.Tests/NpcProximityTests.cs` (>= 5 case)
- [ ] `verify.ps1` xanh (step 5, 6, 10)

## Success Criteria
- `verify.ps1` step 10 không còn báo `WorldActorView.cs`.
- Toàn bộ test `NpcProximityTests` pass; không test cũ nào đỏ thêm.
- NPC/quái vẫn spawn, vẫn click được (kiểm tra nhanh trong Unity hoặc PlayMode test hiện có).

## Risk Assessment
| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|--------|----------|----------|-----------|
| Tách partial làm sót thành viên → compile lỗi | Trung bình | Thấp | verify.ps1 step 6 bắt ngay |
| Đổi hành vi khi copy (sửa tay lúc chuyển) | Thấp | Cao | Copy nguyên văn, KHÔNG refactor kèm |
| Sentinel trùng id NPC thật | Thấp | Cao | Dùng `int.MinValue`, có test id âm |
| Ngưỡng 110/150px sai cảm giác | Trung bình | Thấp | Hằng số public, chỉnh ở phase 04 sau QA |

## Security Considerations
Không. Thuần client-side, không đổi dữ liệu gửi server.

## Next Steps
→ Phase 02 dùng `NpcProximity` + partial mới để gắn nút.
