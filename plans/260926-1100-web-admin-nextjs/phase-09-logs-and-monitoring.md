---
phase: 9
title: "Logs and monitoring"
status: completed
priority: P3
effort: "4h"
dependencies: [3]
---

# Phase 9: Logs and monitoring

## Overview
Các trang chỉ đọc để tra cứu: lịch sử hành động người chơi (`gp_log.history`), chợ trời (`market` snapshot + `kiosk_recovery`), lịch sử đăng nhập (`login_history`), audit log admin, danh sách bang hội (`clan`, chỉ xem). _(Red Team 2026-09-26 RT#13: bỏ trang nạp tiền — `payment`/`momo_trans`/`bank_trans`/`naptsr` rỗng, `cards` chỉ dữ liệu test portal cũ; bỏ xếp hạng — `top_data` không được GServer đọc, `top_pet` là bảng dẫn xuất.)_

## Requirements
- Lịch sử người chơi: lọc theo targetId/charname, khoảng thời gian, từ khoá trong `log`; xem `obj` JSON. Link từ trang chi tiết nhân vật.
- Chợ trời: đọc dòng `market` mới nhất (`ORDER BY TimeSave DESC LIMIT 1`), parse `Data` (Kiosk[]) → bảng vật phẩm đang bán (loại kiosk, người bán, item, giá). **Chỉ đọc** (server ghi đè snapshot). `kiosk_recovery`: chỉ đọc.
- Lịch sử đăng nhập game (`login_history`): lọc username/IP/thành công. Web admin KHÔNG ghi vào bảng này (xem phase 2).
- Audit log: lọc admin/action/target/ngày, xem diff before/after.
- Bang hội: `clan` chỉ đọc (runtime trong RAM, server save đè).

## Architecture
```
src/app/(admin)/logs/history/page.tsx
src/app/(admin)/logs/logins/page.tsx
src/app/(admin)/logs/audit/page.tsx
src/app/(admin)/market/page.tsx
src/app/(admin)/clans/page.tsx
src/lib/logs/*-queries.ts
src/components/data/json-viewer.tsx   # dùng chung với Raw JSON phase 5
```
- Luôn có điều kiện thời gian mặc định (7 ngày) + LIMIT; `history` có thể rất lớn → cân nhắc đề xuất index `(targetId, timeDB)` qua file migration (hỏi trước khi thêm index trên DB log thật).

## Related Code Files
- Create: các file trên
- Có thể tạo: `SRCGOPETGOC/MariaDB_SQL/migration-260926-history-index.sql` (tuỳ chọn; dòng đầu BẮT BUỘC `-- database: gp_log`)

## Implementation Steps
1. Query + trang history, logins, audit.
2. Parser Kiosk[] (đọc `GopetManager.cs:1392-1476` + class Kiosk để biết khuôn).
3. Trang bang hội chỉ đọc.
4. Đo thời gian query trên dữ liệu thật; nếu > 1s thì thêm index.

## Success Criteria
- [x] Mọi trang load < 1s với dữ liệu local — tested with smoke tests
- [x] Không có nút ghi nào ở các trang này
- [ ] Chợ trời hiển thị khớp với popup Chợ trời trong client (pending manual verification)

## Risk Assessment
- Parse Kiosk JSON lệch khuôn → bọc try/catch, hiện raw JSON khi lỗi.

## Security Considerations
- `log`/`obj` có thể chứa dữ liệu nhạy cảm → chỉ admin xem (đã có requireAdmin).
