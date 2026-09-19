---
phase: 8
title: Docs & đồng bộ plan parity
status: completed
priority: P3
effort: 3h
dependencies:
  - 7
---

# Phase 8: Docs & đồng bộ plan parity

## Overview

Ghi lại hệ thống chiến đấu vào `docs/` (hiện chỉ có `docs/journals`, chưa có tài
liệu kỹ thuật nào), và sửa nhãn trạng thái sai ở plan
`260913-jar-unity-parity-audit-and-certification`.

## Key Insights

- `docs/` hiện chỉ chứa `journals/`. Các file mà `documentation-management.md` yêu
  cầu (`development-roadmap.md`, `project-changelog.md`, `system-architecture.md`,
  `code-standards.md`) **chưa tồn tại**. Phase này không tạo hết — chỉ tạo phần
  thuộc phạm vi battle, tránh scope creep.
- Plan `260913` ghi `| Battle | PvE/PvP turn, attack/skill/item/result/effect/float
  text | Hoàn thành về code | ...` (`plan.md:56`). Nhãn này sai — đó là lý do lỗi
  đấu trường tồn tại mà không ai biết. Phải sửa kèm lý do, không sửa lặng lẽ.
- Plan `260912-jar-unity-remaining-parity` đang `status:
  implemented-awaiting-manual-certification` — cũng cần soát mục battle.

## Requirements

**Functional**
- Có tài liệu mô tả hệ thống battle: 3 đường vào PvP, vòng lượt, công thức damage
  ở mức khái niệm, các opcode và ai gửi cho ai.
- Bảng opcode battle đầy đủ, đánh dấu opcode mới `PET_BATTLE_BUFF` và version gate.
- Plan `260913` được sửa nhãn và thêm tham chiếu tới plan này.

**Non-functional**
- File docs <= 800 dòng (`docs.maxLoc`).
- Không viết docs cho phần chưa làm.

## Architecture

```
docs/
└── battle-system.md        – mới: đường vào, vòng lượt, opcode, version gate
plans/260913-.../plan.md    – sửa dòng 56, thêm blockedBy
plans/260912-.../plan.md    – soát mục battle nếu có nhãn sai
```

## Related Code Files

- Create: `D:/game/docs/battle-system.md`
- Modify: `D:/game/plans/260913-jar-unity-parity-audit-and-certification/plan.md`
- Modify: `D:/game/plans/260912-jar-unity-remaining-parity/plan.md` (nếu có nhãn sai)
- Modify: `D:/game/plans/260917-1812-pvp-arena-battle-parity/plan.md` (đổi status)

## Implementation Steps

1. Viết `docs/battle-system.md` từ nội dung đã xác minh trong plan này:
   - 3 đường vào PvP và bảng `isPkMode`/`coinBet`;
   - vòng lượt 25s, stun/miss/crit/cooldown 3 lượt;
   - bảng opcode: 36, 37, 16, 45, 59, 63, 64, 99, 18 + opcode buff mới;
   - quy ước `battleId` = userId của **người nhận gói** (nguồn gốc nhiều lỗi);
   - version gate 1.5.0 và lý do tồn tại.
2. Sửa `260913/plan.md` dòng 56: đổi "Hoàn thành về code" sang trạng thái thật, kèm
   một dòng lý do và link tới plan này. Thêm
   `blockedBy: [260917-1812-pvp-arena-battle-parity]` vào frontmatter (khớp hai
   chiều với `blocks` đã đặt ở plan này).
3. Soát `260912/plan.md` phần battle, sửa nếu có nhãn tương tự.
4. Cập nhật trạng thái plan này bằng `ck plan check <phase-id>` cho từng phase đã
   xong, và `status: completed` ở frontmatter khi trọn vẹn.
5. Chạy `/ck:journal` ghi nhật ký kỹ thuật cho đợt làm này.

## Todo List

- [ ] `docs/battle-system.md`
- [ ] Sửa nhãn battle ở plan 260913 + `blockedBy` hai chiều
- [ ] Soát plan 260912
- [ ] `ck plan check` cho các phase đã xong
- [ ] `/ck:journal`

## Success Criteria

- [ ] `docs/battle-system.md` đủ để người mới hiểu luồng mà không phải đọc code.
- [ ] Bảng opcode khớp `GopetCMD.cs` (đối chiếu từng dòng).
- [ ] Plan 260913 không còn nhãn "Hoàn thành" sai cho battle.
- [ ] Quan hệ `blocks`/`blockedBy` khớp hai chiều.
- [ ] `ck plan status` của plan này báo đủ 8/8.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Docs lệch code ngay sau khi viết | Trung bình | Viết **sau** phase 07, đối chiếu từng opcode với `GopetCMD.cs` |
| Scope creep sang tạo đủ 4 file docs bắt buộc | Trung bình | Chỉ tạo `battle-system.md`; 3 file kia tách plan riêng |
| Sửa plan khác gây xung đột với phiên khác đang chạy | Thấp | Chỉ sửa dòng nhãn battle + frontmatter, không cấu trúc lại |

## Security Considerations

- Không đưa credential test (`gopetpvp1`/`gopetpvp2` và mật khẩu) vào `docs/`.
- Không mô tả cách giả mạo gói battle ở mức thao tác được.

## Next Steps

Không có — đây là phase cuối. Sau khi xong, plan `260913` có thể tiếp tục chứng
nhận mảng battle.
