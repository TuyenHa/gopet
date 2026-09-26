---
phase: 5
title: "Account and player management"
status: completed
priority: P1
effort: "8h"
dependencies: [3]
---

# Phase 5: Account and player management

> Cập nhật sau Red Team 2026-09-26 (RT#1, RT#4, RT#6, RT#7, RT#15). Chia 2 phần: **5A (bảng `user`, MVP, không cần phase 4)** và **5B (sửa `player`, vòng 2, cần phase 4)**.

## Overview
Quản lý tài khoản (`web.user`) và nhân vật (`game.player`). Ưu tiên tận dụng các hàng đợi server đã tự xử lý an toàn (giftcode, thư, `exchange_gold`) thay vì ghi đè row người chơi.

## Requirements

### 5A — Tài khoản (`/accounts`) — MVP
- Danh sách: tìm theo username / user_id / email; cột: id, username, role, ban, coin, tongnap, ngày tạo, "hash: bcrypt/legacy", "2FA: có/không", badge online (từ `player_online` nếu phase 4 đã chạy).
- **Không bao giờ SELECT** `password`, `secretKey` ra UI/RSC/audit — chỉ trả cờ dẫn xuất (`password LIKE '$2%'`, `secretKey IS NOT NULL`) [RT#6]. Tài khoản legacy (plaintext/SHA256 — chuỗi lưu chính là mật khẩu game, `GopetHashHelper.cs:22-26`) được đánh dấu đỏ + nút "Buộc reset".
- [Validation Session 1] Migration `SRCGOPETGOC/MariaDB_SQL/migration-260926-lock-legacy-password-accounts.sql` (dòng đầu `-- database: gopettae_gopet_web`): `UPDATE user SET role=0 WHERE password NOT LIKE '$2%' AND role<>0;` → khoá mọi tài khoản hash cũ (tester, admi11, adm111, adm000). Mở lại = super-admin "Buộc reset" (bcrypt mới + role=1). Tài khoản `admin` (bcrypt, role=3) không bị ảnh hưởng.
- Hành động (an toàn khi server chạy — server chỉ đọc `user` lúc login):
  - Ban có hạn (`isBaned=1, banTime=nowMs+dur`), vĩnh viễn (`isBaned=2`), unban (`isBaned=0, banTime=0`), `banReason`. UI ghi rõ: người đang online chỉ bị chặn từ lần login sau (không kick).
  - Khoá/mở: chỉ đổi `role` 0 ↔ giá trị cũ; **giữ nguyên role > 1** (admin có `role=3`, `web_db.sql:3468`) [RT#15].
  - Reset mật khẩu: `bcryptjs` cost 12 (`$2b$` verify được với BCrypt.Net-Next 4.0.3, `Gopet.csproj:34`).
  - Xoá 2FA: `secretKey=NULL`.
  - Cộng/trừ ngọc `user.coin` (**int(11)**, không phải bigint): delta nguyên tử `UPDATE user SET coin=coin+? WHERE user_id=? AND coin+? BETWEEN 0 AND 2147483647`; zod giới hạn |delta| ≤ 2^31-1 [RT#15].
- **Quyền**: super-admin = `SUPERADMIN_USER_IDS` (env, mặc định `1` = tài khoản `admin` — Validation). Chỉ super-admin được: cấp/thu `isAdmin`, reset mật khẩu/2FA/ban **tài khoản admin khác**, bật công tắc "server đã tắt". Thao tác nhạy cảm yêu cầu nhập lại mật khẩu [RT#7]. Không cho tự thu quyền chính mình.

### 5B — Nhân vật (`/players`) — vòng 2, cần phase 4
- Danh sách + chi tiết (tabs): Tổng quan | Hành trang & Pet (chỉ xem ở 5B; sửa ở phase 6) | Thư | Bạn bè | Nhiệm vụ/thành tựu | Raw JSON. Mọi tab đọc có phép chiếu cột tường minh.
- **Tặng tài nguyên — ưu tiên đường server tự xử lý, hiệu lực cả khi online** [scope RT#3]:
  - Vàng: INSERT `exchange_gold` (server cộng + DELETE lúc login, `Player.cs:541-555`).
  - Vật phẩm/pet: giftcode riêng 1 lượt cho user, hoặc thư (phase 8).
- Sửa trực tiếp row `player` (qua guard phase 4, chỉ khi offline):
  - Tiền tệ `gold/coin/lua`: **chỉ delta** `SET gold=gold+? WHERE ID=? AND gold+?>=0` — vì server tự `coin=coin+@share` lên người offline khi bán ki ốt (`KioskPayout.cs:48-50`) [RT#4].
  - Các cột khác (`star`, `pkPoint`, `EventPoint`, `AccumulatedPoint`, `avatarPath`, `gender`): optimistic `WHERE ID=? AND col=<giá trị cũ>`, `affectedRows=0` → báo xung đột.
  - **Bỏ khỏi form**: `clanId`, `name` (bang giữ tên/member trong RAM và save 3 phút, `Clan.cs:300`, `AutoSave.cs:37-51`), `ArenaPoint`, `KioskFund` (không file C# nào dùng) [RT#4].
  - `isAdmin`: chỉ super-admin; UI ghi rõ "chỉ có hiệu lực từ lần login sau — muốn tước ngay: ban + chờ họ thoát/restart server" (`isAdmin` nằm RAM, `PlayerData.cs:45`, không save) [RT#15].

## Architecture
```
src/lib/players/offline-guard.ts   # withOfflinePlayer(userId, fn) — đúng giao thức phase 4:
                                   #   conn riêng; GET_LOCK(...,5)===1; try{ heartbeat tươi? ; NOT EXISTS player_online ; SET STATEMENT max_statement_time=8 FOR <UPDATE> }
                                   #   finally{ RELEASE_LOCK; conn.release() }  (không để lock kẹt trên conn trong pool) [RT#1]
src/lib/players/player-queries.ts / player-actions.ts
src/lib/accounts/account-queries.ts / account-actions.ts
src/lib/auth/require-superadmin.ts
src/app/(admin)/accounts/..., players/...
```
- Mọi hàm query/action gọi `requireAdmin()` **bên trong** (DAL), không chỉ ở layout [RT#6].
- Action: requireAdmin → zod → (guard) → UPDATE → audit (không chứa hash/secret) → `revalidatePath`.

## Related Code Files
- Create: các file trong Architecture
- Modify: `webadmin/src/components/layout/sidebar-nav-config.ts`

## Implementation Steps
1. 5A: queries có phép chiếu an toàn, actions, super-admin, re-auth.
2. 5B (sau phase 4): `offline-guard.ts` + test tích hợp (online → từ chối; khoá bị giữ → từ chối; heartbeat cũ → từ chối; lock luôn được nhả).
3. Tặng vàng qua `exchange_gold`; sửa delta/optimistic.

## Success Criteria
- [x] Không response nào (HTML/RSC/JSON) chứa `password`/`secretKey`
- [x] Ban/unban/reset mk/xoá 2FA code review pass (security verified)
- [x] Toggle khoá không đổi role=3 thành 1; cộng ngọc vượt int32 bị chặn
- [x] Tặng vàng qua `exchange_gold` đã implement
- [x] Sửa người online / khi server không heartbeat → bị chặn (offline-guard protocol)
- [x] Admin thường không cấp được isAdmin hay reset tài khoản admin khác
- [ ] Manual E2E: ban/unban effect in Unity client (pending manual QA)

## Risk Assessment
- Race với server ghi người offline: đã khử bằng delta/optimistic.
- Tước `isAdmin` không tức thì — đã ghi rõ trên UI.

## Security Considerations
- Mật khẩu reset hiển thị 1 lần, không vào audit. Re-auth cho thao tác nhạy cảm.
