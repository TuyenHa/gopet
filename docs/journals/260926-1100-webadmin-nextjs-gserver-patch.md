# Webadmin Next.js 16 + GServer Online-Lock Patch — 11 Phase Hoàn Thành, 7/10 Code Review

**Date**: 2026-09-26, 11:00–14:37
**Severity**: High (C1 session-flag deletion)
**Component**: webadmin (Next.js 16), GServer (Player.cs online-lock), MariaDB pools
**Status**: In review (fixes applied, pending E2E + docker build)

---

## What Happened

11 pha hoàn thành trong một ngày. Lead xây dựng nền tảng (pha 1-3: mysql2 pools, JWT login game-account + isAdmin guard, audit fail-closed INSERT-only, layout). Sau đó 6 agent song song với file ownership nghiêm ngặt (pha GServer 4/5/7/8/9/11). Pha 6 (inventory/pet) thực hiện riêng. Code review phát hiện **Critical C1**: session thứ hai (OTP dở/đăng nhập trùng/bị ban) ngắt kết nối xoá cờ `player_online` của session 1 → web tưởng offline → admin sửa dữ liệu nhưng AutoSave session 1 ghi đè lại.

---

## The Brutal Truth

Điều này nguy hiểm vì:

1. **C1 lọt qua toàn bộ test đơn lẻ.** Cần 2 session concurrent; vitest chạy từng test riêng biệt.
2. **Hậu quả: dữ liệu mất hoặc hoàn tác.** Admin sửa gold/item cho player P (session S1 online), ghi vào DB, rồi người chơi mở login mới (session S2) rồi tắt → S2 onDisconnected xoá flag → web tưởng P offline → S1 save xoá hoàn toàn edit.
3. **3 điểm yếu session-flag khác (H2/H3/H5)** — disposed race, client gửi userId, giftcode case không khớp lock.
4. **Phase 11 variable syntax `${VAR:?}` phá docker compose trên dev machine** → không khởi động được MariaDB mới, migration tắt lại 4 tài khoản dev vì hash migration khoá legacy-password.

---

## Technical Details

**C1 — Player.cs:748-754 onDisconnected:**
```csharp
if (user != null) MarkOffline(user.user_id);  // Gọi dù session này không pernah MarkOnline
```
Chỉ set flag ở line 476 nếu pass 2FA + ban + duplicate check. 2FA trước duplicate → session S2 ở OTP prompt → tắt app → onDisconnected chạy với `user != null` nhưng `playerData == null` → xoá flag S1.

**H2 — Disposed race (Data/User/PlayerData.cs:761):**
`disposed = true` không synchronized → `PaySeller` addCoin/save() xảy ra AFTER `disposed=true` → no-op → kiosk payout 95% mất.

**H5 — Lock case-sensitive, code lookup case-insensitive:**
Client `abcd` → lock `gift_code_lock_abcd`, web reset lock `gift_code_lock_ABCD` → không mutual exclusion.

**Phase 11 Disaster:**
```bash
# docker-compose.yml
environment:
  TZ: ${TZ:?}  # <-- Bash syntax không support trong docker-compose, phải là ${TZ}
```
Làm hỏng `docker compose up -d` trên máy dev, migrate-db.sh (gọi compose up mariadb) tạo lại container với TZ +07:00, migration khóa 4 tài khoản dev (tuyenha1/2/23...) vì hash cũ không bcrypt.

---

## What We Tried

1. **vitest 140 test (19 DB integration)** — xanh; C1 cần 2-session test → không có.
2. **`tsc --noEmit`, lint** — xanh (syntax/type only).
3. **`next build`** — xanh, 20 routes.
4. **GServer 56/56 test** — xanh.
5. **Smoke: curl all pages** — xanh (stateless, không concurrent session).

---

## Root Cause Analysis

1. **C1 = mô hình session lifecycle sai.** `MarkOffline` gọi trong onDisconnected với điều kiện `user != null` thay vì "session này từng gọi MarkOnline". Chưa định `bool ownsOnlineFlag` trên Player.
2. **Phase 11 dùng `${VAR:?}` cho biến webadmin** → compose interpolate TOÀN file kể cả service thuộc profile khác, nên `docker compose up -d` trên máy dev (không có biến webadmin) lỗi. Sửa: bỏ `:?`, kiểm biến trong `deploy-service.sh`.
3. **Migration khoá tài khoản hash cũ** (đúng quyết định Validation) cũng khoá tài khoản dev local tuyenha1/2/23 — mở lại bằng reset mật khẩu trên web admin.

---

## Lessons Learned

1. **Session state cần guard explicit.** Mỗi lifecycle transition phải set/check flag trước, không dựa vào điều kiện gián tiếp (`user != null` không = "tôi set flag").
2. **Disposed + concurrency = lock(obj).** AutoSave bypass disposed check → phải `lock(playerData)` toàn bộ critical section.
3. **Compose kiểm biến bắt buộc trên cả file, không theo profile.** Biến chỉ cần cho production thì kiểm ở script deploy, không dùng `:?`.
4. **Migration đụng dữ liệu tài khoản: liệt kê trước các dòng bị ảnh hưởng** (SELECT cùng WHERE) để báo user trước khi chạy.
5. **Concurrent-session test bắt buộc.** C1 không lọt nếu harness có 2-session login + one-close-during-OTP.

---

## Fixes Applied (Committed)

- **C1:** `Player.ownsOnlineFlag` (set after MarkOnline succeeds), `MarkOffline` chỉ gọi nếu true. Defense: `DELETE WHERE user_id=? AND since=?`
- **H2:** `lock(playerData)` trong `save()` + dispose block, `PaySeller` cũng `lock` hoặc bypass với SQL delta `coin = coin + @share`.
- **H3:** `withOfflinePlayer` giờ chỉ lấy `playerId`, resolve `user_id` server-side, `UPDATE` thêm `AND user_id=?`.
- **H5:** Lock name lowercase cả hai bên: `gift_code_lock_${code.toLowerCase()}`.
- **Phase 11:** Xoá `:?` syntax → dùng default `${TZ}` hoặc rõ ràng set ngoài.

---

## Verified

- tsc/lint: ✅
- 140/140 vitest (19 DB integration): ✅
- `next build` 20 routes: ✅
- GServer 56/56 test: ✅
- Code review: 7/10 lúc đầu; chưa review lại sau khi sửa (fix đã qua tsc/lint/140 test)

---

## Pending

- E2E với client Unity (login → xem player → edit).
- `docker build` thực tế trên Linux CI.
- M5: JWT không thu hồi ngay sau logout (chấp nhận, 8h window, requireAdmin re-check role).

---

**Status:** IN_REVIEW (fixes committed, no uncommitted changes)
**Summary:** 11 pha xong; critical C1 (session-flag deletion) + H2/H3/H5 đã sửa; phase 11 shell-syntax disaster sửa xong; E2E + docker build pending.
