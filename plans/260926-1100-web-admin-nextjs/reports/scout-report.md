# Scout report — GServer & DB cho web admin (2026-09-26)

Đường dẫn tương đối `SRCGOPETGOC/GServer/`, dump ở `SRCGOPETGOC/MariaDB_SQL/`.

## DB
- `gopettae_tae2` (server_db.sql): player, clan, item, gopet_pet, shop, shoparena, npc, map, boss, drop_item, skill/skilllv, task/task_type, achievement, tattoo, tier_item, iteminfo, trade_gift, pet_tier/pet_class/pet_element/pet_eff, gopet_mob/_location, gopet_map_moblvl, hidden_stat, enchant_wing_data, exchange_gold, reincarnation, gift_code, letter, market, kiosk_recovery, field, top_*, payment...
- `gopettae_gopet_web` (web_db.sql): user (MyISAM), login_history, web_config, options, server, exchange, bank*, cards, momo_trans, naptsr, logs, posts/tags...
- `gp_log` (log_db.sql): history(targetId, log, obj, charname, timeDB).
- MariaDB 10.4, charset lẫn lộn → kết nối utf8mb4. CÓ 17 FOREIGN KEY (server_db.sql:5250-5311); PK ghép ở gopet_mob_location, gopet_map_moblvl, exchange_gold; KHÔNG PK: letter, history, gopet_mob, iteminfo (chỉ UNIQUE), kiosk_recovery.
- `web.user.coin` là int(11) (không phải bigint). Admin có `role=3` (portal PHP cũ) dù server chỉ định nghĩa 0/1.

## Auth
- `Util/GopetHashHelper.cs:9-26`: BCrypt cost 12 (`$2a$12$`), fallback so sánh plaintext khi hash không phải bcrypt.
- Login `Server/Player.cs:599-635`: `GET_LOCK('login_lock_'+username)`, chặn 10 lần sai/5 phút qua `login_history`. Username `^[a-z0-9]+$`.
- `user.role`: 0 = chưa kích hoạt (cấm login), 1 = user. Quyền admin game = `player.isAdmin` (`Data/User/PlayerData.cs:45`).
- Ban `Player.cs:417-446`: isBaned 0/1 (có hạn, banTime epoch ms)/2 (vĩnh viễn); banReason hiện ở màn login. Chỉ check lúc login.
- 2FA: `user.secretKey` (`Player.cs:448`).

## Lưu người chơi
- Load 1 lần lúc login (`Player.cs:473`). Save full-row `PlayerData.cs:202-270` khi disconnect (`Player.cs:686-689`) + AutoSave (`Runtime/AutoSave.cs`, 15 phút).
- Cột save KHÔNG ghi: name, isAdmin, gender, ArenaPoint, KioskFund.
- `user.isOnline` không được code nào ghi → không tin được (phase 4 vá).

## Template
- `Manager/GopetManager.cs:974-1270` init 1 lần (`App/Main.cs:45`), không reload an toàn.
- Live: gift_code, letter. `field` reload qua `/api/server/RefreshField`.

## JSON player
- `items`: `{"<invType>":[Item...]}`; invType 0 đồ pet,1 thường,2 skin,3 cánh,4 ngọc,5 tiền (`GopetManager.cs:444-453`).
- Item (`Data/item/Item.cs:12-76`): itemId (instance), itemTemplateId, itemUID, count, petEuipId, lvl, expire, durability, def/atk/hp/mp, gemInfo, wasSell, canTrade, EnchantInfo, SourcesItem, TimeCreate, NumFusion, version.
- Pet (`Data/pet/Pet.cs`): petIdTemplate, petId, star, Expire, exp, name, str/agi/_int, tiemnang_point, skillPoint, skill int[][], tiemnang, tatto, equip, isUpTier, HiddenStats, PetEffects...
- Serializer Newtonsoft (`Adapter/JsonAdapter.cs`), có null/default.

## Khác
- gift_code: id, code, currentUser, maxUser, gift_data (int[][]), expire, usersOfUseThis (JSON id), isClanCode. Kiểu quà `GopetManager.cs:202-232`, xử lý `GameController.cs:4105`, đổi `MenuController.inputDialog.cs:84-172`.
- letter: userId (0 = hệ thống), targetId, time, Type (1 bạn,2 admin,3 sự kiện), Title, ShortContent, Content. Tham chiếu `Manager/SystemLetterService.cs`.
- market: Id, Data (Kiosk[] JSON), TimeSave — snapshot, chỉ đọc.
- Settings: `config/server.json` (file), DB `field`, web `web_config`, `options` (chứa mật khẩu email plaintext!), `server`, `exchange`.
- HTTP API 8082 tồn tại nhưng user yêu cầu KHÔNG dùng.
