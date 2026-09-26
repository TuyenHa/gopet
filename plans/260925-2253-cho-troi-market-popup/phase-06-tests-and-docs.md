---
phase: 6
title: Tests and docs
status: completed
priority: P2
effort: 4h
dependencies:
  - 1
  - 2
  - 3
  - 4
  - 5
---

# Phase 6: Tests and docs

## Overview
Viết test cho logic server (kiosk hardening + query) và parser client, chạy E2E tay với server local, sau đó cập nhật docs.

## Key Insights
- Test server: `D:\game\tests\GServer.Performance.Tests` là console runner tham chiếu `Gopet.csproj` (xem `Program.cs`, `BattleBackgroundTests.cs`, `EquipDurabilityTests.cs` làm mẫu).
- Test client: `GopetUnityClient\Assets\Tests\PlayMode` (ví dụ `MinimapHudLayoutTests.cs`).
- Khởi động server: memory `goserver-startup` (Start-Process Gopet.exe, cần `GOPET_DB_PASSWORD`).

## Related Code Files
- Create: `tests\GServer.Performance.Tests\MarketKioskTests.cs`
  - Owner check khi cancel.
  - Buy và cancel đồng thời (Parallel, 100 lần): chỉ 1 thành công.
  - Expire bán dở cộng 95% ở cả online và offline recovery.
  - `TryList` từ chối đồ khóa, đồ pet đang mặc, đồ có ngọc, count/price sai.
  - Thuế 5% khi buy.
- Create: `tests\GServer.Performance.Tests\MarketQueryTests.cs`
  - Filter 7 loại.
  - Sort 3 chế độ, ổn định.
  - Phân trang, clamp trang.
  - Listing của mình có `isMine=true`; tự mua bị từ chối.
  - Listing chỉ định: ẩn với người thứ ba, hiện với người được chỉ định; `TrySetAssignedName` trừ đúng phí (pet 15.000 / đồ 10.000 vàng), bỏ chỉ định miễn phí, chặn tự chỉ định/tên không tồn tại/chủ khác.
- Modify: `tests\GServer.Performance.Tests\Program.cs` (đăng ký test mới nếu runner cần đăng ký)
- Create: `GopetUnityClient\Assets\Tests\PlayMode\MarketPacketParseTests.cs` (round-trip Row/State/Sellable/Result, `ExpectFullyConsumed`)
- Create: `docs\market-popup.md` (luồng, protocol 47..56, quy tắc kinh doanh, file chính). Theo style `docs\shop-popup.md`.
- Modify: `docs\project-changelog.md`, `docs\development-roadmap.md` (nếu có), `docs\system-architecture.md` (nếu có)

## Implementation Steps
1. Viết và chạy test server: `dotnet run --project tests/GServer.Performance.Tests`. Tất cả phải pass, không mock logic lõi.
2. Viết test parser Unity, chạy PlayMode tests qua Unity CLI hoặc Test Runner.
3. E2E tay với 2 tài khoản (A bán, B mua), server local:
   - A đăng trang bị, ngọc, 5/50 vật phẩm, pet. Đồ khóa bị từ chối.
   - B ở map khác 22 lọc, sort, phân trang, mua. A nhận 95%.
   - A gỡ 1 món. Để 1 món hết hạn (tạm set `HOUR_UPLOAD_ITEM` nhỏ qua config test, nhớ khôi phục).
   - Restart server: listing còn nguyên. Kiểm tra `market` đã lưu ngay sau mutation.
   - NPC ki ốt ở map 22 vẫn hoạt động và thấy cùng hàng.
4. Delegate `code-reviewer` review toàn bộ diff và xử lý các finding.
5. Viết `docs/market-popup.md` + changelog.

## Success Criteria
- [ ] Toàn bộ test server + client pass.
- [ ] Checklist E2E đạt hết.
- [ ] Code review không còn finding critical/high.
- [ ] Docs cập nhật.

## Risk Assessment
- Test đa luồng không ổn định: dùng `Barrier` để các thread bắt đầu cùng lúc, lặp nhiều lần.
- Thay đổi `HOUR_UPLOAD_ITEM` để test: chỉ đổi trong test/config, không commit.
