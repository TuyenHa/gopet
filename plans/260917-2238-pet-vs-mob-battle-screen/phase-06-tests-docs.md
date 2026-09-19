---
phase: 6
title: "Tests & docs"
status: pending
priority: P2
effort: "3h"
dependencies: [5]
---

# Phase 6: Tests & docs

## Overview
Khoá hành vi mới bằng test ở 3 tầng và cập nhật tài liệu battle.

## Requirements / Steps
1. **Unit** (`GopetUnityClient/tests/Gopet.Net.Tests`):
   - `BattleHandlerTests`: parse `PET_BATTLE_STATS` (đủ, 0 actor, count vượt giới hạn,
     thừa byte), mob start có skill (passive format), `SendSurrender` bytes.
   - `CompactNumberFormatTests`, `BattleSkillIconKeyTests`.
2. **PlayMode** (`run-playmode-tests.ps1`): dựng `BattleView` với start giả lập PvE + PvP,
   kiểm tra có HUD 2 bên, panel phải read-only, stats đến muộn vẫn cập nhật, Xin thua mở
   dialog, teardown không rò object.
3. **LiveSmoke** (`tests/Gopet.Net.LiveSmoke`, server local 19180):
   - Đánh quái có skill → nhận `PET_BATTLE_STATS`, có ≥1 lượt quái dùng skill (lặp đến N lượt).
   - Xin thua PvE → `PET_BATTLE_STATE` winner = mobId.
   - Xin thua PvP (`gopetpvp1`/`gopetpvp2`) → cả hai nhận kết quả.
4. **Server**: `dotnet build` sạch; nếu có project test server thì thêm test `CritPercent`
   khớp `isCrit` và loader `gopet_mob_skill` bỏ qua dòng lỗi.
5. **Docs**: `docs/battle-system.md` — thêm opcode stats/surrender vào §4, mục mới "Skill
   quái" (bảng, AI, sửa `mobUseSkill`), mục "Màn hình chiến đấu Unity". Cập nhật
   `docs/project-changelog.md`, `docs/development-roadmap.md`.
6. `code-reviewer` review toàn bộ diff.

## Success Criteria
- [ ] `verify.ps1`, PlayMode, LiveSmoke đều xanh — không skip, không mock server trong LiveSmoke.
- [ ] Docs phản ánh đúng wire format đã code.
- [ ] Review không còn lỗi mức cao.

## Risk Assessment
- Lượt quái dùng skill là ngẫu nhiên → LiveSmoke seed `useRate = 100` cho 1 quái test
  hoặc lặp đủ lượt; không sửa logic để test qua.
