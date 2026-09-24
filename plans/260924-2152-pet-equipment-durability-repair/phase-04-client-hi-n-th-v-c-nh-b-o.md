---
phase: 4
title: Client hiển thị và cảnh báo
status: completed
priority: P2
effort: 2h
dependencies:
  - 1
  - 3
---

# Phase 4: Client hiển thị và cảnh báo

## Overview
Client gần như không phải sửa: chữ độ bền do server dựng, NPC dùng hộp chọn chung. Phase này
kiểm hiển thị và thêm cảnh báo rõ ràng.

## Key Insights
- Tên/mô tả trang bị: `GameController.writeItemEquip` (:1996) → `PetEquipHandler.cs:110-125` →
  `JarIconTokens.Humanize` (`GameSession.EquipConfirm.cs:66`); túi đồ: `InventoryItemPopupView.cs` (~:90).
- Parser chỉ số client: `UiLogic/ShopItemText*.cs`, `GemItemText.cs` — dòng "Độ bền: n/80" không được bị hiểu là chỉ số.
- Gợi ý mục đích NPC: `Runtime/World/NpcPurposeHints.cs:18-33`.

## Architecture
- Cảnh báo sau trận (server gửi, client sẵn có): khi `Wear` trả `Warned`/`Broke`, server gửi
  một banner/toast ngắn "Kiếm X sắp hỏng (8/80)" / "Kiếm X đã hỏng — mang tới Thợ rèn".
  Gửi SAU khi overlay trận đóng (hoặc dùng kênh ticker) để không đè băng kết quả.
- `NpcPurposeHints`: thêm -33 "Sửa trang bị".
- (Tuỳ chọn, chỉ nếu user muốn) viền đỏ ô trang bị hỏng trong màn pet — cần thêm cờ vào gói
  trang bị ⇒ đổi wire format, để sau (YAGNI).

## Related Code Files
- Modify: `Runtime/World/NpcPurposeHints.cs`; có thể `UiLogic/ShopItemText*.cs` nếu parser vướng.
- Server (thuộc phase 1/3 nhưng kiểm ở đây): chuỗi cảnh báo.

## Implementation Steps
1. Mở màn pet, túi đồ, popup xác nhận mặc đồ: đọc được "Độ bền" và "(Hỏng)", không vỡ layout.
2. Thêm hint NPC; compile client.
3. Kiểm cảnh báo hiện đúng lúc, không đè banner CHIẾN THẮNG.

## Success Criteria
- [ ] Độ bền đọc được ở mọi chỗ xem trang bị.
- [ ] Cảnh báo sắp hỏng/đã hỏng hiện 1 lần mỗi mốc, không spam mỗi trận.
- [ ] Gõ vào Thợ Rèn trên client hiện hộp 2 lựa chọn, menu sửa cuộn được khi nhiều món.
