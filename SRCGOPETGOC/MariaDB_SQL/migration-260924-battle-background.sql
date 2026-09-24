-- Khung cảnh màn đấu (mua bằng vàng). Chạy 1 lần trên DB game (bảng `player`).
-- BattleBgOwned: JSON list id đã mua (id 0 = rừng mặc định, không lưu).
-- BattleBgSelected: id đang chọn, 0 = rừng mặc định.
ALTER TABLE `player`
  ADD COLUMN IF NOT EXISTS `BattleBgOwned` mediumtext NOT NULL DEFAULT '[]',
  ADD COLUMN IF NOT EXISTS `BattleBgSelected` INT NOT NULL DEFAULT 0;
