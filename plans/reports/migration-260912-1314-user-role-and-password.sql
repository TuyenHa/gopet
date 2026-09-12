-- =============================================================
-- Migration: fix `user.role` = 0 & password plaintext legacy
-- Date : 2026-09-12
-- Target DB: `web` (GopetHashHelper.ComputeHash uses BCrypt cost 12)
-- Notes:
--   * doRegister trong Player.cs (working copy) đã set role=1 & hash BCrypt.
--   * Script này migrate DỮ LIỆU CŨ đã tạo trước fix.
--   * Chạy trên DB web (không phải DB game).
--
-- Khuyến nghị BACKUP trước khi chạy:
--   mysqldump -u root -p web > backup_web_before_migration.sql
-- =============================================================

USE `web`;

-- ---------------------------------------------------------------
-- 1) PROMOTE role = 0 -> 1 cho acc hợp lệ chưa active
--    Điều kiện an toàn:
--      * role = 0 (ROLE_NON_ACTIVE)
--      * chưa bị ban  (isBaned = 0)
--    KHÔNG đụng đến role >= 2 (ROLE_MOD/ADMIN...) hoặc acc bị ban.
-- ---------------------------------------------------------------
SELECT COUNT(*) AS `will_promote_role`
FROM `user`
WHERE `role` = 0 AND `isBaned` = 0;

UPDATE `user`
SET    `role` = 1
WHERE  `role` = 0
  AND  `isBaned` = 0;

-- ---------------------------------------------------------------
-- 2) LIỆT KÊ acc còn lưu password plaintext / sha256 (không phải bcrypt)
--    BCrypt hash luôn bắt đầu bằng $2a$ / $2b$ / $2y$ và dài 60 chars.
--    Chạy SELECT trước để review; UPDATE ở bước 3 (comment) chỉ chạy
--    khi bạn quyết định BẮT BUỘC reset password.
-- ---------------------------------------------------------------
SELECT `user_id`, `username`, `role`,
       CASE
         WHEN `password` LIKE '$2a$%' OR `password` LIKE '$2b$%' OR `password` LIKE '$2y$%'
              THEN 'bcrypt'
         WHEN `password` REGEXP '^[0-9a-fA-F]{64}$'
              THEN 'sha256-legacy'
         ELSE 'plaintext'
       END AS `pwd_type`,
       `create_date`
FROM   `user`
WHERE  NOT (`password` LIKE '$2a$%' OR `password` LIKE '$2b$%' OR `password` LIKE '$2y$%')
ORDER  BY `user_id`;

-- ---------------------------------------------------------------
-- 3) (TÙY CHỌN) Reset password cho các acc plaintext / sha256
--    -> gán bcrypt của chuỗi "changeme123" và cho user tự đổi lại.
--    Bcrypt-hash của "changeme123" (cost 12) được gen sẵn dưới đây;
--    verify: BCrypt.Verify("changeme123", hash) == true.
--
--    Bỏ dấu -- ở đầu 4 dòng UPDATE nếu muốn chạy.
-- ---------------------------------------------------------------
-- UPDATE `user`
-- SET    `password` = '$2a$12$9TxBziJPwVZZ/j4xtZtF5eFngDQu3zC.a1MgnKEX1T8G3w2keHJJG'
-- WHERE  NOT (`password` LIKE '$2a$%' OR `password` LIKE '$2b$%' OR `password` LIKE '$2y$%');

-- NOTE: nếu KHÔNG muốn reset, bạn có thể để nguyên plaintext:
-- GopetHashHelper.VerifyHash đã có fallback bắt SaltParseException, so-sánh
-- Ordinal string cho acc plaintext cũ (Util/GopetHashHelper.cs:22-27).
-- Sau khi user login rồi đổi mật khẩu (requestChangePass ở Player.cs:249),
-- password sẽ tự động được re-hash bằng bcrypt.
