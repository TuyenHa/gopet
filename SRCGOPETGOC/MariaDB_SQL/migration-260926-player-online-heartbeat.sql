-- database: gopettae_tae2
-- Phase 4 (web admin) — cờ "server đang nắm giữ người chơi này" + heartbeat sống của
-- server, để web admin biết chắc có được sửa thẳng bảng `player` không.
-- Mỗi game DB = một GServer (production chỉ chạy 1 GServer + 1 gopettae_tae2) nên
-- không cần cột server_id — xem phase-04-gserver-isonline-patch.md mục Risk Assessment.

CREATE TABLE IF NOT EXISTS `player_online` (
  `user_id` INT NOT NULL,
  `since` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `server_heartbeat` (
  `id` TINYINT NOT NULL,
  `protocol_version` INT NOT NULL,
  `beat_at` DATETIME NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
