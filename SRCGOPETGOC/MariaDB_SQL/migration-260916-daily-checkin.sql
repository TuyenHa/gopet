-- Sự kiện điểm danh theo tháng (Monthly Daily Check-in)
-- Chạy 1 lần trên DB `game`. An toàn: thêm cột default 0, thêm 2 item hộp bí ẩn.

-- 1) State người chơi: bitmask ngày đã nhận + khoá tháng
ALTER TABLE `player`
  ADD COLUMN `DailyCheckinMask` INT NOT NULL DEFAULT 0,
  ADD COLUMN `DailyCheckinMonthKey` INT NOT NULL DEFAULT 0;

-- 2) 2 item hộp quà bí ẩn (type=24 khớp hộp Tết 240023; khoá trade; stackable)
INSERT INTO `item`(`itemId`,`name`,`description`,`type`,`iconPath`,`isStackable`,`canTrade`,`price`)
VALUES
 (240024,'Hộp quà bí ẩn tuần 4','Vật phẩm điểm danh, mở ra nhận ngẫu nhiên 1 phần thưởng.',24,'items/240024.png',1,0,10),
 (240025,'Hộp quà bí ẩn cuối tháng','Vật phẩm điểm danh, mở ra nhận ngẫu nhiên 1 phần thưởng.',24,'items/240025.png',1,0,10);
