-- Quyền tối thiểu cho user DB của web admin (gopet_admin). Grant THEO BẢNG, không db.*.
-- Chạy SAU khi đã tạo user và đã chạy migration (bảng admin_audit_log phải tồn tại):
--
--   CREATE USER IF NOT EXISTS 'gopet_admin'@'%' IDENTIFIED BY '<mật khẩu>';
--   docker exec -i -e MYSQL_PWD="$PASS" gopet-mariadb mysql -uroot < docker/webadmin-grants.sql
--
-- Chạy lại nhiều lần an toàn (GRANT cộng dồn). Thêm bảng vào registry template (phase 7)
-- thì thêm dòng GRANT tương ứng ở cuối file.

-- ===== Game DB: đọc toàn bộ, ghi đúng bảng cần =====
GRANT SELECT ON gopettae_tae2.* TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.gift_code TO 'gopet_admin'@'%';
GRANT INSERT ON gopettae_tae2.letter TO 'gopet_admin'@'%';
GRANT INSERT ON gopettae_tae2.exchange_gold TO 'gopet_admin'@'%';
GRANT UPDATE ON gopettae_tae2.player TO 'gopet_admin'@'%';

-- ===== Web DB =====
GRANT SELECT ON gopettae_gopet_web.* TO 'gopet_admin'@'%';
GRANT UPDATE ON gopettae_gopet_web.user TO 'gopet_admin'@'%';
GRANT UPDATE ON gopettae_gopet_web.server TO 'gopet_admin'@'%';
-- Audit chỉ INSERT: web admin không sửa/xoá được lịch sử của chính mình.
GRANT INSERT ON gopettae_gopet_web.admin_audit_log TO 'gopet_admin'@'%';

-- ===== Log DB: chỉ đọc =====
GRANT SELECT ON gp_log.* TO 'gopet_admin'@'%';

-- ===== Template (phase 7) — thêm khi đưa bảng vào registry =====
-- Vòng 2a
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.item TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.shop TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.shoparena TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.drop_item TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.boss TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.map TO 'gopet_admin'@'%';
-- field: chỉ 2 dòng cấu hình cố định → chỉ cho sửa, không thêm/xoá.
GRANT UPDATE ON gopettae_tae2.field TO 'gopet_admin'@'%';
-- gopettae_gopet_web.server: đã GRANT UPDATE ở trên (dòng "Web DB") — không thêm/xoá.

-- Vòng 2b
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.tier_item TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.iteminfo TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.gopet_pet TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.pet_class TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.pet_element TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.pet_tier TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.pet_eff TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.petexp TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.hidden_stat TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.skill TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.skilllv TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.trade_gift TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.npc TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.gopet_map_moblvl TO 'gopet_admin'@'%';
-- gopet_mob: KHÔNG có PRIMARY KEY → chỉ xem (SELECT đã có ở gopettae_tae2.*), không grant ghi.
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.gopet_mob_location TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.task TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.task_type TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.achievement TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.tattoo TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.enchant_wing_data TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.reincarnation TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.clan_template TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.clan_skill TO 'gopet_admin'@'%';
GRANT INSERT, UPDATE, DELETE ON gopettae_tae2.clan_skill_lvl TO 'gopet_admin'@'%';
