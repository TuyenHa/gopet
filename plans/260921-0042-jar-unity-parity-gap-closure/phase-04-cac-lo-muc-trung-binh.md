# Phase 04 — Các lỗ mức trung bình

## Context Links
- Audit: `plans/reports/parity-gap-260921-0042-jar-vs-unity.md` (M-2, M-4, M-5, M-6)

## Overview
- Ưu tiên: trung bình — không chặn luồng nào, nhưng là tính năng jar có mà Unity im lặng.
- Trạng thái: xong.

## Key Insights
- **M-6 Banner thường**: `GuiderHandler.BannerShown` có 0 subscriber. `PlayerManager` gửi kèm
  BOSS_BANNER (đã có UI) nên phần lớn banner vẫn hiện, nhưng `Player.showBanner()` gọi đơn lẻ
  thì mất hẳn. Nối vào `NotificationTicker` là xong — rẻ nhất, làm trước.
- **M-5 Hộp thư phân loại**: `Letter.Type` đã parse nhưng `MailboxView` không dùng. Jar chia
  3 tab Admin / Sự kiện / Bạn bè.
- **M-2 Xem kỹ năng bang người khác**: `GuildPackets.ShowClanSkill` 0 call-site; chỗ gắn là
  `TargetPlayerMenu`.
- **M-4 Xem hình xăm pet trong ký gửi**: `SHOW_TATTO_PET_IN_KIOSK` (81/99) 0 call-site.
- **M-1 Top điểm phát triển**: BỎ — server map thẳng vào `showTopFund()`
  (`GameController.cs:2568-2570`), nội dung trùng Top quỹ đã có.

## Requirements
- Banner thường hiện được.
- Hộp thư lọc theo loại.
- Hai lối vào còn lại chỉ cần một mục menu.

## Architecture
Toàn bộ là nối event có sẵn vào UI có sẵn; không thêm handler protocol nào.

## Related Code Files
- `Runtime/World/GameSession.cs` (BannerShown → Ticker)
- `Runtime/UI/MailboxView*.cs`
- `Runtime/UI/TargetPlayerMenu.cs` + chỗ dựng nó
- `Runtime/UI/KioskListingView.cs` hoặc menu kiosk pet

## Implementation Steps
1. Nối `BannerShown` vào `NotificationTicker`.
2. Thêm bộ lọc loại thư cho `MailboxView`.
3. Thêm mục "Kỹ năng bang" vào menu người chơi khác.
4. Thêm mục "Xem hình xăm" cho kiosk pet.

## Todo List
- [x] M-6 banner
- [x] M-5 lọc thư
- [x] M-2 kỹ năng bang người khác
- [x] M-4 xem hình xăm pet ký gửi

## Success Criteria
- Mỗi mục bấm được và server phản hồi đúng màn tương ứng.

## Risk Assessment
- Thấp; không đụng luồng trận đấu hay giao dịch tiền.

## Security Considerations
- Không mở thêm quyền: mọi kiểm tra nằm ở server.

## Next Steps
- Chạy lại `check-protocol-coverage` để chắc không phát sinh route mới chưa phân loại.
