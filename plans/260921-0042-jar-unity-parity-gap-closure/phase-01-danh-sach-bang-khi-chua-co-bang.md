# Phase 01 — Danh sách bang hội khi chưa có bang

## Context Links
- Audit: `plans/reports/parity-gap-260921-0042-jar-vs-unity.md` (P0-2)
- Client: `Assets/Scripts/Runtime/World/GameSession.Guild.cs:29-31`
- Server: `SRCGOPETGOC/GServer/Server/GameController.cs:2546-2592` (switch `clan()`), `:2596-2621` (`clanInfo`)

## Overview
- Ưu tiên: P0 — tab "Bang hội" trống vĩnh viễn với người chưa vào bang.
- Trạng thái: xong.

## Key Insights
- Client gửi `GUILD_LIST` (sub **1**) khi `ClanId <= 0`. Switch `clan()` của server **không có
  case 1 và không có `default`** → gói rơi im lặng, không lỗi, không phản hồi.
- Đường đúng là `CLAN_INFO` (sub **14**): `clanInfo()` tự rẽ nhánh — có bang thì gửi info,
  **chưa có bang thì gọi `showListClan()`**, tức đúng danh sách bang cần hiện.
- `GuildPackets.RequestGuildList()` vì thế là packet chết. Không xoá: sub 1 vẫn là hằng số
  thật của protocol, chỉ là server bản này không phục vụ.

## Requirements
- Mở màn bang hội khi chưa có bang phải hiện được danh sách bang để xin vào.
- Không đổi hành vi khi đã có bang.

## Architecture
`OpenGuildView()` bỏ nhánh rẽ theo `ClanId`, luôn gửi `RequestClanInfo()`; server tự quyết
trả `CLAN_INFO` hay danh sách bang.

## Related Code Files
- Sửa: `Assets/Scripts/Runtime/World/GameSession.Guild.cs`
- Đọc: `Assets/Scripts/Net/Guild/GuildPackets.cs`

## Implementation Steps
1. Trong `OpenGuildView()`, thay `if/else` bằng một lời gọi `RequestClanInfo()`.
2. Ghi rõ trong comment vì sao KHÔNG dùng `RequestGuildList()`.

## Todo List
- [x] Sửa `OpenGuildView`
- [x] `verify.ps1` xanh

## Success Criteria
- Tài khoản chưa có bang mở tab Bang hội → thấy danh sách bang (server `showListClan`).
- Tài khoản có bang → vẫn thấy info bang như cũ.

## Risk Assessment
- Thấp. Cùng một sub server vẫn đang dùng cho nhánh "đã có bang".

## Security Considerations
- Không đổi quyền: server tự kiểm tư cách thành viên trong `clanInfo()`.

## Next Steps
- Phase 03 dùng lại chính màn này cho nút góp quỹ / ô kỹ năng.
