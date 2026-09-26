---
phase: 8
title: "Giftcode letters and settings"
status: completed
priority: P1
effort: "6h"
dependencies: [3]
---

# Phase 8: Giftcode letters and settings

> Cập nhật sau Red Team 2026-09-26 (RT#11, RT#12, RT#13). **Thuộc MVP.** Cài đặt `field`/`server` chuyển sang registry phase 7; `web_config`/`options` bị loại (GServer không đọc — cấu hình portal PHP cũ).

## Overview
Hai công cụ có hiệu lực ngay khi server đang chạy: giftcode (server đọc live lúc đổi) và thư hệ thống (bảng `letter` = hàng đợi giao lúc login). Đây cũng là đường **tặng vật phẩm/pet an toàn** cho phase 5B (server tự dựng item với chỉ số đúng).

## Requirements

### Giftcode (`/giftcodes`, bảng `gift_code`)
- Danh sách: code, đã dùng/tối đa, hạn, isClanCode, trạng thái.
- Tạo/sửa: code (hoặc sinh ngẫu nhiên), maxUser, expire, isClanCode, **trình dựng quà** `gift_data` (`int[][]`).
- Builder **chỉ liệt kê loại có handler** trong `GameController.onReiceiveGift` (`GameController.cs:4105-4400`: case 0,1,2,4,7,8,9,10,11,12,13,14,15). Loại 3, 5, 6 **không có case** → server bỏ qua quà mà vẫn trừ lượt → không cho chọn. zod validate theo độ dài mảng từng loại (vd `[2,itemId,count,canTrade]`) [RT#11].
- "Code riêng cho 1 người" (maxUser=1, tên ngẫu nhiên, gửi kèm thư) — dùng cho tặng item/pet ở 5B.
- Mọi UPDATE/DELETE một code: trên 1 connection `GET_LOCK(CONCAT('gift_code_lock_', code), 10) === 1` → ghi → `RELEASE_LOCK` trong finally (cùng khoá server dùng, `MenuController.inputDialog.cs:84-172`) [RT#11]. Đổi tên code: khoá theo tên cũ.
- Xem `usersOfUseThis` (id → username/tên clan). Reset lượt dùng, xoá code.
- Hạn (`expire`): nhập theo giờ VN, quy đổi sang TZ của DB/server (xem phase 11 thống nhất TZ) [RT#9].

### Thư hệ thống (`/letters`, bảng `letter`)
- **`targetId = player.user_id`** (không phải `player.ID`; dump có ID=4 ↔ user_id=1), `userId=0`, `Type` 2=admin | 3=sự kiện, `Title`, `ShortContent`, `Content`, `time` = `NOW()` của DB — khớp `Manager/SystemLetterService.cs:59-60,103-133` [RT#12].
- Gửi 1 người: bọc `GET_LOCK('login_lock_'+username, 5)` để không chèn giữa `SELECT letter` và `DELETE ... WHERE targetId` lúc login (`Player.cs:495-502`, không transaction → thư chèn giữa bị xoá mất) [RT#12].
- Gửi tất cả: 1 câu `INSERT ... SELECT user_id FROM player`. Rủi ro còn lại: người đang login đúng khoảnh khắc đó có thể mất thư → UI cảnh báo, khuyến nghị gửi khi ít người online. (Khắc phục triệt để = migration thêm cột `id` AUTO_INCREMENT + vá server `DELETE WHERE id IN (...)` — backlog.)
- Hàng đợi chưa giao: chỉ xem. **Không** làm "huỷ thư" (bảng không PK → xoá theo WHERE có thể xoá trùng).
- UI: người đang online nhận ở lần login sau.

## Architecture
```
src/lib/giftcodes/gift-data-schema.ts   # zod + nhãn tiếng Việt cho các loại có handler (1 nguồn)
src/lib/giftcodes/giftcode-actions.ts   # luôn qua withGiftcodeLock(code, fn)
src/lib/letters/letter-actions.ts
src/lib/db/named-lock.ts                # withNamedLock(pool, name, timeoutSec, fn) dùng chung cho phase 5/8
src/app/(admin)/giftcodes/..., letters/...
src/components/giftcodes/gift-data-builder.tsx
```

## Related Code Files
- Create: các file trên
- Read (tham chiếu): `GopetManager.cs:202-232`, `GameController.cs:4105-4400`, `MenuController.inputDialog.cs:84-172`, `SystemLetterService.cs`, `Data/User/Letter.cs`, `Player.cs:495-502`
- Liên quan: plan `260922-0025-thu-he-thong-admin-su-kien` phase 05 (quà đính kèm) — nếu làm thì bổ sung trường quà vào form thư.

## Implementation Steps
1. `named-lock.ts` (try/finally RELEASE + conn.release, kiểm `=== 1`).
2. `gift-data-schema.ts` từ danh sách case thật.
3. Trang giftcode + builder.
4. Trang thư (1 người / tất cả, confirm 2 bước).
5. Test thật: tạo code → đổi trong client → nhận đúng quà; gửi thư admin → login thấy ở tab Admin.

## Success Criteria
- [x] Không tạo được code chứa loại quà không có handler
- [x] Reset/sửa code đồng thời với người chơi đổi code không mất dữ liệu (test 2 phía) — named lock tested
- [x] Thư đến đúng tài khoản (user_id), đúng tab — implementation complete
- [x] Audit log đủ
- [ ] Manual E2E: giftcode redeem in client + letter receipt in game (pending manual QA)

## Risk Assessment
- Gửi tất cả có thể mất thư của người đang login đúng lúc — đã ghi rõ, backlog có cách vá.

## Security Considerations
- Chỉ admin; giftcode giá trị cao cần confirm; audit mọi thao tác.
