# Phase 03 — Góp quỹ và ô kỹ năng bang

## Context Links
- Audit: `plans/reports/parity-gap-260921-0042-jar-vs-unity.md` (P0-3, P0-4)
- Client: `Runtime/UI/GuildView.TopSkills.cs`, `Net/Guild/GuildPackets.cs`
- Server: `GameController.cs:2624-2646` (`donateClan()` gửi danh sách), `:2647-2675`
  (`donateClan(int)`), `:4897-4920` (`unlockSlotSkillClan`)

## Overview
- Ưu tiên: P0 — hai luồng đi vào ngõ cụt ngay trước bước cuối.
- Trạng thái: xong.

## Key Insights
- Góp quỹ: client ĐÃ gửi `DONATE_CLAN` (sub 9) và nhận đủ danh sách mức góp, nhưng `GuildView`
  chỉ nối chúng thành **một khối text**. `GuildPackets.PlayerDonateClan` (sub 10) có sẵn mà
  **0 call-site** → người chơi xem được bảng giá nhưng không góp được.
- Wire sub 10 = **index trong đúng danh sách sub 9 vừa gửi** (`donateInfos.get(menuId)`), nên
  thứ tự dòng phải giữ nguyên: không lọc, không sắp xếp lại.
- Ô kỹ năng bang: mới có "Thuê" (`RentSkill`); `UnlockSkillSlot` **0 call-site**. Server trả về
  Y/N dialog chuẩn nên client không phải dựng hộp thoại riêng.

## Requirements
- Mỗi mức góp quỹ là một dòng bấm được, bấm là góp.
- Có nút "Mở ô kỹ năng" cho bang.

## Architecture
`GuildView` phát `DonateOptionChosen(int index)` và `UnlockSkillSlotRequested()`;
`GameSession.Guild` gửi packet tương ứng. Không thêm màn hình mới.

## Related Code Files
- Sửa: `Runtime/UI/GuildView.TopSkills.cs`, `Runtime/UI/GuildView.cs`,
  `Runtime/World/GameSession.Guild.cs`
- Đọc: `Net/Guild/GuildPackets.cs`

## Implementation Steps
1. Dựng danh sách mức góp thành các dòng bấm được, giữ nguyên index.
2. Thêm nút "Mở ô kỹ năng" cạnh nút "Thuê".
3. Nối hai event trong `GameSession.Guild`.

## Todo List
- [x] Dòng góp quỹ bấm được
- [x] Nút mở ô kỹ năng
- [x] Nối event
- [x] `verify.ps1` xanh

## Success Criteria
- Bấm một mức góp → quỹ bang tăng, server trả `DonateFundOK`.
- Bấm "Mở ô kỹ năng" → hiện Y/N dialog của server, hoặc redDialog nêu điều kiện chưa đủ.

## Risk Assessment
- Sai index = góp nhầm mức tiền. Giữ nguyên thứ tự server gửi.

## Security Considerations
- Server kiểm chức vụ, cấp bang và quỹ; client chỉ gửi index.

## Next Steps
- Phase 04 gắn thêm mục vào cùng khu vực này.
