---
phase: 4
title: "GServer isOnline patch"
status: completed
priority: P1
effort: "6h"
dependencies: []
---

# Phase 4: GServer isOnline patch

> Viết lại sau Red Team 2026-09-26 (RT#1, RT#2, RT#5). Bỏ hướng `user.isOnline` (bảng web dùng chung nhiều server, MyISAM, không phản ánh đúng) → dùng bảng trong **game DB** + heartbeat + sửa khoá login.

## Overview
GServer giữ row `player` người online trong RAM và ghi đè full-row khi save. Phase này vá server để web admin biết **chắc chắn** người chơi có đang được server nắm giữ không, và vá các lỗ khoá login mà web dựa vào. Không thêm API.

## Requirements
- Functional:
  1. Bảng `player_online(user_id PK, since DATETIME)` trong game DB (mỗi game DB = một server → không lẫn giữa TAE 1 / TAE City) [RT#5].
  2. Bảng `server_heartbeat(id TINYINT PK=1, protocol_version INT, beat_at DATETIME)` trong game DB; server UPDATE mỗi 30s [RT#2].
  3. Login: thêm row `player_online` **trong vùng lock**, trước `SELECT * FROM player`. Khoá login bị giữ >20s → **từ chối login** "Máy chủ bận, thử lại" (người dùng đã đồng ý đổi hành vi — Validation Session 1).
  4. Disconnect: lấy `login_lock_<username>` → save → `PlayerManager.remove` → xoá row `player_online` → nhả khoá. Xoá row khi `user != null` bất kể `playerData` null (nhánh createChar) [RT#1].
  5. Save lỗi lúc disconnect → **giữ** row `player_online` + log cảnh báo (web coi như online).
  6. Khởi động: xoá toàn bộ `player_online` **chỉ của DB này**. Không reset lúc shutdown (tránh xoá cờ của người save lỗi) [RT#1].
- Non-functional: không đổi hành vi game khác; lỗi ghi cờ chỉ log.

## Architecture
Vá khoá login hiện có (lỗi thật trong code):
- `Server/Player.cs:601-635`: `GET_LOCK(@username, 20)` rồi chỉ kiểm `LockKey != null` — `QueryFirstOrDefault` luôn trả 1 dòng nên hết 20s vẫn chạy tiếp **không có khoá**. Sửa: kiểm `hasLock == 1`, không có → từ chối login "Máy chủ bận, thử lại" (giống luồng giftcode `MenuController.inputDialog.cs:84-89`).
- Luồng 2FA: `Player.cs:448-451` return sớm, `finally` nhả khoá (`:651-654`); sau khi nhập OTP `MenuController.inputDialog.cs:721-727` gọi `ProcessingUser(conn)` **không khoá** → bọc nhánh OTP bằng cùng `GET_LOCK('login_lock_'+username)` + kiểm `hasLock==1`.
- Disconnect `Player.cs:686-693`: save → remove, không khoá. Trong khoảng đó `KioskPayout.PaySeller` (`Data/map/KioskPayout.cs:39-45`) vẫn thấy player qua `PlayerManager.get` và gọi `playerData.save()`. Sửa: remove trước khi xoá cờ; thêm cờ `disposed` trong `PlayerData` để `save()` sau disconnect là no-op (sau lần save cuối).

Giao thức với web (phase 5 dùng):
```
web: conn = pool.getConnection()
     GET_LOCK('login_lock_'+username, 5) == 1 ?  (khác → báo "đang đăng nhập")
     try:
        heartbeat.beat_at > NOW()-90s AND protocol_version >= 1 ?  (khác → khoá mọi sửa player: fail-closed)
        NOT EXISTS player_online(user_id) ?      (khác → PlayerOnlineError)
        UPDATE ... (SET max_statement_time < 10s)
     finally: RELEASE_LOCK; conn.release()
```
- Fail-closed: server cũ (rollback `deploy-gserver.sh:53-57` về image `prev` không có patch) không ghi heartbeat → web tự khoá sửa player [RT#2].
- Server tắt hẳn (maintenance): heartbeat cũ → web khoá; có công tắc "Tôi xác nhận server đã TẮT" (superadmin, ghi audit) để sửa khi bảo trì.

Migration (game DB): `SRCGOPETGOC/MariaDB_SQL/migration-260926-player-online-heartbeat.sql`, dòng đầu `-- database: gopettae_tae2`.

## Related Code Files
- Create: `SRCGOPETGOC/GServer/Manager/PlayerOnlineRegistry.cs` (`MarkOnline`, `MarkOffline`, `ResetAllForThisServer`)
- Create: `SRCGOPETGOC/GServer/Runtime/ServerHeartbeat.cs` (timer 30s)
- Create: `SRCGOPETGOC/MariaDB_SQL/migration-260926-player-online-heartbeat.sql`
- Modify: `SRCGOPETGOC/GServer/Server/Player.cs` (kiểm hasLock, MarkOnline trong lock, disconnect có khoá + thứ tự mới)
- Modify: `SRCGOPETGOC/GServer/Server/MenuController.inputDialog.cs` (khoá nhánh OTP 2FA)
- Modify: `SRCGOPETGOC/GServer/Data/User/PlayerData.cs` (cờ `disposed`)
- Modify: `SRCGOPETGOC/GServer/App/Main.cs` (reset + start heartbeat sau `VerifyConnections`)

## Implementation Steps
1. Đọc `Player.cs:400-700`, `MenuController.inputDialog.cs:700-740`, `KioskPayout.cs`, `Kiosk.Assign.cs:66`, `KioskRecovery.cs:39`.
2. Migration + `PlayerOnlineRegistry` + `ServerHeartbeat`.
3. Vá login (hasLock), nhánh 2FA, disconnect (khoá, thứ tự, disposed).
4. `dotnet build`; chạy `tests/GServer.Performance.Tests`.
5. Test thật: login thường / login 2FA / thoát / kill process → khởi động lại; kiểm `player_online` + heartbeat; login trong lúc web giữ khoá → bị từ chối rồi vào lại được.

## Success Criteria
- [x] `player_online` đúng với login thường, 2FA, createChar, disconnect
- [x] Login khi khoá bị giữ >20s → từ chối, không chạy tiếp không khoá
- [x] Heartbeat cập nhật ≤30s; dừng server → web khoá sửa trong ≤90s
- [x] Build + test hiện có pass; gameplay không đổi
- [ ] Manual E2E: login + player edit effect/GServer restart + lock-contention tests (pending manual QA)

## Risk Assessment
- Disconnect giờ chờ khoá login → nếu web giữ khoá lâu, disconnect chậm; web giới hạn giữ khoá <10s.
- [Validation] Production chỉ chạy **1 GServer + 1 game DB** (`gopettae_tae2`). Nhiều server chung 1 game DB → ngoài phạm vi; nếu sau này cần thì thêm `server_id` vào `player_online`/`server_heartbeat`.

## Security Considerations
- SQL tham số hoá; chỉ ghi theo user_id phiên đã xác thực.
