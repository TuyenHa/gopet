---
phase: 3
title: "White layout and sidebar"
status: completed
priority: P1
effort: "4h"
dependencies: [2]
---

# Phase 3: White layout and sidebar

## Overview
Khung trang quản trị: nền trắng, sidebar trái cố định, header trên (tên admin, logout), vùng nội dung. Kèm dashboard tổng quan và bộ component bảng/form dùng chung cho các phase sau (DRY).

## Requirements
- Functional: sidebar nhóm menu, highlight route hiện tại, thu gọn được; mobile dùng Sheet trượt. Dashboard số liệu.
- Non-functional: màu trắng/xám nhạt, chữ đậm vừa, font Be Vietnam Pro (đã dùng ở client Unity) qua `next/font/google`, hỗ trợ tiếng Việt.

## Architecture
```
src/app/(admin)/layout.tsx        # requireAdmin() + Sidebar + Header
src/app/(admin)/page.tsx          # Dashboard
src/components/layout/app-sidebar.tsx
src/components/layout/sidebar-nav-config.ts   # 1 nguồn dữ liệu menu
src/components/layout/app-header.tsx
src/components/data/data-table.tsx            # TanStack Table + phân trang server
src/components/data/search-bar.tsx            # query string ?q=&page=
src/components/data/confirm-dialog.tsx
src/components/data/restart-required-banner.tsx
src/lib/pagination.ts                          # parse page/size, LIMIT/OFFSET
```
Menu sidebar (tiếng Việt):
- **Tổng quan**: Dashboard
- **Người dùng**: Tài khoản, Nhân vật
- **Dữ liệu game**: Vật phẩm, Shop, Rơi đồ, Boss, Bản đồ, Cài đặt server (`field`), Máy chủ (`server`) — vòng 2b thêm: Pet, Kỹ năng, NPC, Quái, Nhiệm vụ, Thành tựu, Xăm, Nâng cấp, Bang hội (template)
- **Vận hành**: Giftcode, Thư hệ thống
- **Giám sát**: Lịch sử người chơi, Chợ trời, Bang hội, Lịch sử đăng nhập, Audit log

Style: `bg-white` nội dung, sidebar `bg-white border-r`, mục active `bg-neutral-100 font-medium`, accent 1 màu (vd `blue-600`) cho nút chính.

Dashboard (Server Component, tối giản — Red Team RT#14): tổng user, số người online (`player_online`, có sau phase 4), trạng thái heartbeat server (xanh/đỏ), giftcode còn hiệu lực, 10 dòng audit mới nhất. Menu chỉ hiện mục đã có trang (config có cờ `enabled`).

## Related Code Files
- Create: các file liệt kê trên
- Modify: `webadmin/src/app/layout.tsx` (font, Toaster), `webadmin/src/app/globals.css`
- Delete: `webadmin/src/app/health` (trang tạm phase 1)

## Implementation Steps
1. Route group `(admin)`, layout gọi `requireAdmin()` (chỉ để render tên admin; bảo vệ thật nằm trong DAL — phase 2).
2. `sidebar-nav-config.ts` (label, href, icon lucide) → sidebar render từ đây.
3. Header: tên nhân vật admin, dropdown Đăng xuất.
4. `data-table.tsx` generic: cột, tổng dòng, phân trang qua searchParams (server-side, không load toàn bảng).
5. `restart-required-banner.tsx`: banner vàng "Dữ liệu template chỉ áp dụng sau khi khởi động lại GServer".
6. Dashboard + `loading.tsx` skeleton + `error.tsx`.
7. Kiểm tra responsive ≥ 360px.

## Success Criteria
- [x] Mọi trang admin có sidebar trái + nền trắng
- [x] Menu active đúng, mobile mở Sheet
- [x] Dashboard hiển thị số liệu thật từ DB
- [x] DataTable phân trang server hoạt động với bảng `player`

## Risk Assessment
- Bảng lớn (history) query chậm → luôn LIMIT, tránh `COUNT(*)` không điều kiện trên bảng log (dùng ước lượng hoặc bỏ tổng).

## Security Considerations
- Layout gọi `requireAdmin()`; không dựa riêng vào middleware.
