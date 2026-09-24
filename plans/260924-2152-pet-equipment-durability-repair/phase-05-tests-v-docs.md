---
phase: 5
title: Tests và docs
status: in-progress
priority: P2
effort: 2h
dependencies:
  - 1
  - 2
  - 3
  - 4
---

# Phase 5: Tests và docs

## Implementation Steps
1. Server tests (`tests/GServer.Performance.Tests`, runner `Program.cs`):
   - `EquipDurability`: thắng −1, thua −2, không âm; `Warned` đúng 1 lần khi qua 8; `Broke` đúng 1 lần.
   - Không áp cho item không phải trang bị pet.
   - JSON round-trip: item cũ không có field → 80; item đã mòn giữ số.
   - `EquipRepairService`: đủ Đá mài → trừ 1, đầy; thiếu Đá mài → không đổi; món đầy/không phải trang bị/không thuộc người chơi → từ chối; sửa song song chỉ trừ 1 Đá mài.
   - `Pet.applyInfo` bỏ qua chỉ số món hỏng nhưng vẫn tính bonus set (nếu dựng được Pet không cần DB; không thì kiểm tay).
2. Test tay: đánh quái đến khi mòn, hỏng, sửa ở Thợ Rèn; Đá mài rơi; điểm danh ngày có Đá mài.
3. Docs: `docs/battle-system.md` thêm mục "Độ bền trang bị" (luật, hằng số, hook `win()`, NPC, Đá mài, migration); `docs/deployment-linux-backend.md` thêm migration mới.
4. `code-reviewer`.

## Success Criteria
- [ ] Test server xanh, build client sạch.
- [ ] Docs đủ để chỉnh cân bằng (hằng số nằm ở đâu) mà không phải đọc code.
