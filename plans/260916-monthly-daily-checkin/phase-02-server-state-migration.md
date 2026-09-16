# Phase 02 — Server: PlayerData state + DB migration

## Context
- State người chơi ở `GServer/Data/User/PlayerData.cs`. Field lưu vào bảng `player`, persist qua `saveStatic` (PlayerData.cs:181-241, danh sách cột SET thủ công).
- Mẫu field cũ: `DailyNoelTime` (DateTime), `DailyNoelIndex` (byte) — PlayerData.cs:110/115.
- Load player từ DB qua Dapper (map cột → property theo tên).

## Overview
- **Priority**: cao (logic phase-03 phụ thuộc).
- **Status**: chưa làm.
- Thêm state theo dõi tháng + các ngày đã nhận trong tháng; migration cột DB + seed 2 item hộp.

## Requirements
### Field mới trên `PlayerData`
- `public int DailyCheckinMask { get; set; } = 0;`
  Bitmask 31 bit — bit `(d-1)` bật = đã nhận quà ngày `d` của **tháng hiện hành**.
- `public int DailyCheckinMonthKey { get; set; } = 0;`
  Khoá tháng gắn với mask, dạng `year*100 + month` (vd 202609). Khác tháng hiện tại → reset mask.

> Dùng `int` mask thay vì mảng: gọn, dễ persist 1 cột, đủ 31 bit. Reset = gán mask=0 + cập nhật monthKey (xem phase-03).

### Migration DB
Script `GServer/backup_sql/migrations/2026-09-16-daily-checkin.sql`:
```sql
ALTER TABLE `player`
  ADD COLUMN `DailyCheckinMask` INT NOT NULL DEFAULT 0,
  ADD COLUMN `DailyCheckinMonthKey` INT NOT NULL DEFAULT 0;

INSERT INTO `item`(`itemId`,`name`,`description`,`type`,`iconPath`,`isStackable`,`canTrade`,`price`)
VALUES
 (240024,'Hộp quà bí ẩn tuần 4','Mở ra nhận ngẫu nhiên 1 phần thưởng.', 24, 'items/240024.png', 1, 0, 10),
 (240025,'Hộp quà bí ẩn cuối tháng','Mở ra nhận ngẫu nhiên 1 phần thưởng.', 24, 'items/240025.png', 1, 0, 10);
```
- `<type>`/`<icon>`: copy từ dòng item `240023` (query DB để lấy giá trị chuẩn).
- Chạy trên DB đang vận hành → ALTER thêm cột default 0, an toàn (không khoá lâu với bảng player vừa phải; nếu bảng lớn cân nhắc giờ thấp tải).

### Cập nhật persist
- Thêm `DailyCheckinMask = @DailyCheckinMask, DailyCheckinMonthKey = @DailyCheckinMonthKey,` vào câu UPDATE trong `saveStatic` (PlayerData.cs:184-241).
- **Lưu ý bug tiềm ẩn**: dòng cuối hiện tại `NumUseGiftBox2025 = @NumUseGiftBox2025` **thiếu dấu phẩy** trước `WHERE` (PlayerData.cs:240) — khi chèn cột mới phải đặt cột mới TRƯỚC dòng đó và giữ đúng dấu phẩy, tránh lỗi SQL.

## Related Code Files
- Sửa: `GServer/Data/User/PlayerData.cs` (2 property + 2 dòng UPDATE).
- Tạo: `GServer/backup_sql/migrations/2026-09-16-daily-checkin.sql`.
- Đọc tham chiếu: dòng item `240023` trong `backup_sql/game14-9-2026_0-26.sql` để lấy type/icon.

## Todo
- [ ] Thêm 2 property vào PlayerData.
- [ ] Thêm 2 cột vào câu UPDATE `saveStatic` (đúng vị trí dấu phẩy).
- [ ] Viết migration SQL (ALTER + INSERT 2 item).
- [ ] Chạy migration lên DB dev, verify cột + item tồn tại (`psql`/`mysql`).
- [ ] Compile server.

## Success Criteria
- Load/save player không lỗi. Cột mới có trong bảng `player`, default 0. 2 item hộp query được.

## Security / Rủi ro
- ALTER trên bảng `player` production → backup trước, chạy giờ thấp tải.
- `canTrade=0` cho 2 hộp để tránh gom/bán trục lợi (khớp phase-01).

## Đã chốt
- `type=24` (khớp item hộp Tết `240023`), `iconPath` riêng `items/240024.png`/`items/240025.png`, `canTrade=0`, `isStackable=1`.
- Ảnh icon tạo ở phase-08 (image-gen), copy vào `GServer/assets/items/` + asset client tương ứng.
