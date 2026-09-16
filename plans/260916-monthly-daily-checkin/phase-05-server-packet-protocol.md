# Phase 05 — Server: Packet gửi trạng thái tháng cho client

## Context
- Gói UI đi qua envelope `COMMAND_GUIDER=122`, byte đầu thân = sub-command (GopetCMD.cs:4; MessageRouter client dòng 87-99).
- Mẫu build gói: `GameController.showInputDialog` (GameController.cs:3623-3640) — `new Message(COMMAND_GUIDER); putsbyte(subType); putInt/putUTF...`.
- Client Unity đăng ký handler qua `MessageRouter.RegisterSub(COMMAND_GUIDER, subType, handler)`.

## Overview
- **Priority**: cao (client phase-06 phụ thuộc).
- **Status**: chưa làm.
- Định nghĩa 2 chiều gói tin: client xin trạng thái / xin điểm danh; server trả bảng 31 ngày + mask.

## Protocol
### Sub-command mới (chọn số chưa dùng trong dải TYPE_ của COMMAND_GUIDER)
- `TYPE_DAILY_CHECKIN_STATE` (server→client): gửi trạng thái tháng.
- Client→server: dùng 1 opcode/sub-command để yêu cầu. Cân nhắc tái dùng `COMMAND_GUIDER` với `TYPE_DAILY_CHECKIN_OPEN` và `TYPE_DAILY_CHECKIN_DO`. **Phải verify** danh sách `TYPE_*` hiện có trong GopetCMD.cs để không trùng.

### Gói server→client `TYPE_DAILY_CHECKIN_STATE`
```
putsbyte TYPE_DAILY_CHECKIN_STATE
putsbyte todayDay            // 1..31
putInt   receivedMask        // bitmask 31 bit
putsbyte daysInMonth         // 28..31
loop d=1..daysInMonth:
    putsbyte dayState        // 0=LOCKED,1=CLAIMABLE,2=RECEIVED,3=MISSED (từ GetDayState phase-03)
    putUTF   rewardLabel      // server build: vd "Bình x4 EXP x3", "Kim cương x5", "Hộp quà bí ẩn"
    putInt   iconItemId       // itemId đại diện để client show icon (item đầu của ngày); 0 nếu ngọc/energy
```
> `rewardLabel` server tự dựng từ `DAILY_CHECKIN_GIFTS[d-1]` (ghép tên item + count). Giúp client không hardcode bảng quà → user chỉnh quà chỉ sửa server.

### Client→server
- `TYPE_DAILY_CHECKIN_OPEN`: mở tab → server chạy `SendCheckinState(player)`.
- `TYPE_DAILY_CHECKIN_DO`: bấm nút điểm danh → server chạy `DailyCheckinEvent.DoCheckin(player)` (tự gửi lại STATE + popup).

## Related Code Files
- Sửa: `GServer/Server/GopetCMD.cs` (thêm 3 hằng TYPE_*).
- Sửa: `GServer/Server/GameController.cs` (thêm `SendCheckinState(player)` build gói STATE).
- Sửa: nơi dispatch sub-command của `COMMAND_GUIDER` phía server (handler nhận `TYPE_DAILY_CHECKIN_OPEN/DO`) — tìm switch xử lý guider request để cắm case.
- Helper build label: `DailyCheckinEvent.BuildLabel(int[][] gift, Language)`.

## Todo
- [ ] Xác định & thêm 3 hằng TYPE_* không trùng.
- [ ] `SendCheckinState` + `BuildLabel`.
- [ ] Cắm handler `TYPE_DAILY_CHECKIN_OPEN/DO` vào dispatch guider phía server.
- [ ] Compile + bắt gói bằng packet-dump để verify layout.

## Success Criteria
- Client gửi OPEN → nhận gói STATE đủ `daysInMonth` mục, đúng state từng ngày.
- Bấm DO ngày hợp lệ → nhận popup + gói STATE cập nhật (ngày đó thành RECEIVED).

## Risk
- Trùng số sub-command → gói bị route sai. Verify kỹ GopetCMD.cs.
- `putUTF` label dài × 31 → gói lớn; chấp nhận được (chỉ gửi khi mở tab).

## Unresolved
- Có gửi kèm `iconItemId` không, hay client tự map theo ngày? (đề xuất gửi để linh hoạt).
