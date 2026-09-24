---
phase: 5
title: Tests và docs
status: in-progress
priority: P2
effort: 2h
dependencies:
  - 1
  - 4
---

# Phase 5: Tests và docs

## Overview
Khoá hành vi bằng test tự động và cập nhật tài liệu.

## Implementation Steps
1. Server tests (`tests/GServer.Performance.Tests` hoặc project test hiện có): `BattleBackgroundService`
   - mua đủ vàng → trừ đúng, owned, selected;
   - thiếu vàng → không đổi;
   - mua lại cảnh đã có → không trừ;
   - id ngoài danh mục / chọn cảnh chưa có → bị từ chối;
   - gọi mua song song nhiều luồng → chỉ trừ một lần.
2. Client EditMode tests: `BattleSceneCatalog` (id lạ → 0), parse gói `BattleSceneState` (byte mẫu → model; thiếu byte → ném).
3. Test tay trong Unity: 6 khung cảnh, mua/chọn/relog, màn hình hẹp, PvP/đấu trường cũng dùng khung cảnh đã chọn.
4. Docs: thêm mục "Khung cảnh đánh quái" vào `docs/battle-system.md` (bảng opcode 43–46, cột DB, bảng giá, cách thêm khung cảnh mới); ghi `docs/project-changelog.md` nếu có.
5. Chạy `code-reviewer`.

## Success Criteria
- [ ] Toàn bộ test server + client xanh.
- [ ] Docs mô tả đủ để thêm khung cảnh thứ 7 mà không phải đọc code.
