-- Nới cột kiosk_recovery.item lên MEDIUMTEXT: pet nhiều tatto/skill serialize JSON có thể
-- vượt varchar(10000) hiện tại. INSERT kiosk_recovery lỗi lúc ki ốt hết hạn (người bán offline)
-- sẽ làm mất luôn cả đồ lẫn tiền (không insert được, không rollback được đồ về túi vì player
-- đã offline). Chạy 1 lần trên DB game.

-- UNIQUE KEY `item_2` (`item`) USING HASH không khai báo prefix length: hợp lệ trên
-- varchar(10000) (server_db.sql) nhưng MySQL/MariaDB KHÔNG cho phép khoá không prefix trên cột
-- TEXT/MEDIUMTEXT/BLOB — ALTER MODIFY sẽ báo lỗi "BLOB/TEXT column ... used in key specification
-- without a key length" nếu còn khoá này. Bỏ khoá: nó không phục vụ mục đích nghiệp vụ nào (2
-- dòng recovery trùng JSON là chuyện bình thường, không phải lỗi trùng dữ liệu cần chặn). Giữ
-- lại KEY `item` (`item`(768)) — có prefix nên vẫn hợp lệ trên MEDIUMTEXT.
ALTER TABLE `kiosk_recovery` DROP INDEX IF EXISTS `item_2`;

ALTER TABLE `kiosk_recovery` MODIFY `item` MEDIUMTEXT NOT NULL;
