-- database: gopettae_gopet_web
-- Nhật ký thao tác của web admin. User DB của web (gopet_admin) chỉ có quyền INSERT
-- (docker/webadmin-grants.sql) → không sửa/xoá được audit. Không lưu password/secretKey.
CREATE TABLE IF NOT EXISTS admin_audit_log (
  id BIGINT AUTO_INCREMENT PRIMARY KEY,
  admin_user_id INT NOT NULL,
  admin_username VARCHAR(20) NOT NULL,
  action VARCHAR(64) NOT NULL,
  target VARCHAR(128) NULL,
  detail LONGTEXT NULL,
  ip VARCHAR(64) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  KEY idx_created (created_at),
  KEY idx_target (target),
  KEY idx_action (action)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
