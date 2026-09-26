---
title: Popup Chợ trời toàn cục
description: >-
  Icon Chợ trời trên HUD mở popup 2 tab (Chợ / Gian hàng của tôi) mua bán ki ốt
  ở mọi map, kèm sửa lỗi kiosk server.
status: completed
priority: P2
effort: 3d
branch: fix/performance
tags:
  - feature
  - frontend
  - backend
  - api
blockedBy: []
blocks: []
created: '2026-09-25T15:56:19.709Z'
createdBy: 'ck:plan'
source: skill
---

# Popup Chợ trời toàn cục

## Overview
Thêm icon "Chợ trời" cạnh "Cửa hàng" trên HUD. Icon mở popup giống style ShopPopupView, có 2 tab:
- **Chợ:** xem đồ người khác đăng bán, lọc theo 7 loại, sắp xếp theo giá, phân trang, có nút Mua.
- **Gian hàng của tôi:** xem đồ mình đang treo, có nút Gỡ và nút Đăng bán. Đăng bán mở popup 2 cột: rương đồ bên trái, chi tiết + ô giá/số lượng bên phải.

Popup dùng lại dữ liệu kiosk sẵn có (`MarketPlace.kiosks`), nên NPC ki ốt ở map 22 vẫn thấy cùng hàng.

## Quyết định đã chốt (user, 2026-09-25)
- Dùng được ở **mọi map**. Luồng mới không cast `MarketPlace`.
- Lọc **7 loại**: Tất cả / Vũ khí / Giáp / Mũ / Ngọc / Pet / Vật phẩm.
- Đồ có số lượng: nhập **số lượng + giá**, người mua mua **trọn gói**. UI mới không có mua lẻ; luồng mua lẻ NPC cũ giữ nguyên.
- **Sửa lỗi kiosk** (owner check, race, mất tiền bán dở). **Không giới hạn** số món treo.
- Giữ nguyên: treo 24h, thuế 5% (`KIOSK_PER_SELL`), giá tính bằng ngọc (coin) 1..2.000.000.000, đồ khóa (`canTrade=false`) không bán được.

- Tab Gian hàng có nút **Gỡ** mỗi món (user OK).
- Tab Chợ hiện cả đồ của mình nhưng **không có nút Mua**.
- **Chỉ định người mua**: chỉ làm **sau khi đăng** (nút "Chỉ định" ở tab Gian hàng của tôi; để trống = bỏ chỉ định). Món chỉ định **ẩn** với người khác ở tab Chợ. Phí vàng giữ và **sửa đúng**: pet 15.000, đồ khác 10.000. Luồng NPC map 22 giữ nguyên tính năng, chỉ sửa phí.
- Popup Đăng bán: đồ khóa hiện mờ; click vào thì toast "Vật phẩm đang khóa, bạn không thể bán được."
- Icon: `tools/image-gen/.env` đã có `OPENAI_API_KEY`.

## Phases
| Phase | Name | Status |
|-------|------|--------|
| 1 | [Server kiosk hardening](./phase-01-server-kiosk-hardening.md) | Completed |
| 2 | [Server market protocol](./phase-02-server-market-protocol.md) | Completed |
| 3 | [Market HUD icon](./phase-03-market-hud-icon.md) | Completed |
| 4 | [Market popup UI](./phase-04-market-popup-ui.md) | Completed |
| 5 | [Sell popup UI](./phase-05-sell-popup-ui.md) | Completed |
| 6 | [Tests and docs](./phase-06-tests-and-docs.md) | Completed |

Thứ tự: 1 → 2 → (3 ∥ 4) → 5 → 6. Phase 3 độc lập với phase 4, làm song song được.

## Key context
- Scout reports: nằm trong kết quả scout của phiên lập plan (xem mục Key Insights của từng phase).
- Server root: `D:\game\SRCGOPETGOC\GServer`. Client scripts: `D:\game\GopetUnityClient\Assets\Scripts`.
- Protocol mới đi theo mẫu battle background (commit 65551c1): sub-command của `COMMAND_GUIDER=122`, dùng sub **47..57**.
- `GopetCmd.cs` phía client sinh tự động: `node GopetUnityClient/tools/gen-gopet-cmd/index.js`, không sửa tay.
- Ref UI: `ShopPopupView*`, `GamePopupFrame`, `PopupTabRail`, `PopupItemList`, `MailboxView` (mẫu 2 cột), `PopupField`, `GameButtonSkin`.

## Dependencies
Không có plan nào chặn. `260924-2152-pet-equipment-durability-repair` cũng sửa `Item`/MenuController nhưng đã merge (commit 74c1721). Cần rebase nếu có xung đột.

## Unresolved
- Kích thước JSON của pet có vượt `kiosk_recovery.item varchar(10000)` không. Phase 1 migrate cột này lên MEDIUMTEXT cho chắc.
