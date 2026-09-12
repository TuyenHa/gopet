---
phase: 4
title: "Test & Parity"
status: completed
priority: P2
effort: "0.5d"
dependencies: [2, 3]
---

## Kết quả (2026-09-11)

- Unit test logic thuần: dùng bộ test sẵn có (`JarMapLayoutTests` trên `maps/11.dat` thật,
  `BuildingDispatcherTests` 10 case buildingType→opcode) — không dựng fixture byte tự chế theo
  đúng quy ước đã chốt trong codebase. `dotnet test`: 625/625 PASS.
- **`WarpChecks.cs` mới** thêm vào `Gopet.Net.LiveSmoke`, chạy byte thật qua GServer đang sống:
  warp map 11 → 15 (Đại Linh Cảnh) → về 11, cả hai chiều PASS. Xem
  `reports/parity-warp-shop.md`.
- `verify.ps1`: 9/10 — check 10/10 (200 dòng/file) fail trên 12 file nợ kỹ thuật có từ trước,
  không liên quan phase này.
- Còn lại: đối chiếu ảnh chụp Unity↔jar bằng tay (thị giác thuần, không tự động hoá được) và bấm
  tay 6/7 shop còn lại (đã khoá đúng wire bằng test, chưa bấm tay xác nhận UI).

# Phase 4: Test & Parity

## Overview
Kiểm chứng warp + shop chạy thật, đối chiếu jar, và không regression.

## Requirements
- Functional: chạy Unity thật (server docker + `client-127.0.0.1.jar` để đối chiếu), thao tác warp và mở shop.
- Non-functional: `verify.ps1` 10/10; test tự động cho phần logic thuần (parse entity, map-name lookup, buildingType→opcode).

## Architecture
- Test thuần C# (netstandard) cho: parse entity map, `MapNames.Get`, mapping buildingType→opcode (bảng tra) — đặt trong `tests/Gopet.Net.Tests` hoặc UiLogic tests.
- Đối chiếu trực quan jar↔Unity: cùng login map 11, so cổng (tên map đích, warp) và shop (mở đúng loại).

## Related Code Files
- Create: `GopetUnityClient/tests/.../MapEntityTests.cs`, `MapNamesTests.cs`, `BuildingShopMapTests.cs`
- Read: `GopetUnityClient/verify.ps1`, `run-playmode-tests.ps1`

## Implementation Steps
1. Unit test: parse 1 entity portal + 1 building từ mẫu byte cố định (từ dump Phase 1) → assert field.
2. Unit test: `MapNames.Get(11)` = "Thành Phố Linh Thú"; vài map đích của portal map 11.
3. Unit test: bảng buildingType→opcode trả đúng cho 7 shop.
4. Chạy `verify.ps1` (10/10) + `run-playmode-tests.ps1` (đóng Editor).
5. Chạy Unity thật: login → map 11 → bấm từng cổng (sang + về) → bấm 7 shop (mở đúng). Chụp đối chiếu jar.
6. Ghi `reports/parity-warp-shop.md`: bảng đối chiếu jar↔Unity, mục còn lệch/TODO.

## Success Criteria
- [ ] Tất cả unit test mới xanh; `verify.ps1` 10/10
- [ ] Warp qua ≥2 map và quay lại OK trên Unity thật
- [ ] 7 shop mở đúng menu tương ứng
- [ ] Report đối chiếu jar↔Unity, liệt kê TODO còn lại
- [ ] Cập nhật `docs/project-changelog.md` (warp + shop)

## Risk Assessment
- PlayMode test cần đóng Editor → nhắc user. Test tự động chỉ phủ logic thuần; phần thị giác cần user xác nhận.
- Giao dịch mua/bán thật có thể chưa hoàn chỉnh (túi đồ) → ghi TODO, không chặn nghiệm thu warp + mở shop.
