---
title: "Chuyển map từ Building Menu (ChangeZone + TeleportMenu)"
description: >-
  Wire các building entity (Khu, Phòng vé) đến handler đổi kênh và dịch chuyển
  map đã có sẵn. Hiện bấm building hiện toast "chưa mở" — cần nối vào
  ChannelHandler/MapTeleportHandler.
status: planning
priority: P1
branch: ""
tags: [unity, map, building, channel, teleport, jar-parity]
blockedBy:
  - 260907-2210-map-portal-warp-shop-interaction
blocks: []
created: "2026-09-11T16:40:00Z"
createdBy: "ck:plan"
source: skill
---

# Chuyển map từ Building Menu (ChangeZone + TeleportMenu)

## Bối cảnh

Chức năng chuyển map trong Unity client **đã hoạt động ở tầng network**:
- Portal gateway (tap cổng → `SendWarp` cmd 25) ✅
- Server xử lý `ON_PLAYER_WARPING` → gửi `ON_UPDATE_PLAYER_IN_MAP` (cmd 29) ✅
- Client nhận cmd 29 → reload map, respawn avatar ✅
- Teleport menu (`MGO_COMMAND/TELE_MENU`) từ character menu ✅
- Channel handler (get/change zone) từ character menu ✅
- WarpFadeOverlay + sound effects ✅

**Vấn đề**: Bấm building trên map (type 8 "Phòng vé", type 11 "Khu") hiện chỉ hiện
toast "chưa mở trong Unity" vì `GameSession.Interactions.cs` xử lý `LocalMenu` đều
trả toast. Cần wire building → handler tương ứng.

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Wire Building → Handler](./phase-01-wire-building-handler.md) | Pending |
| 2 | [Test & Verify](./phase-02-test-verify.md) | Pending |

## Key Facts

- `BuildingDispatcher.cs` case 11 → `LocalMenu.ChangeZone` → toast (chưa nối)
- `BuildingDispatcher.cs` case 8 → `LocalMenu.TicketRoom` → toast (chưa nối)
- `ChannelHandler` + `GameSession.Channels.cs` đã hoạt động hoàn chỉnh qua character menu
- `MapTeleportHandler` + `GameSession.Teleport.cs` đã hoạt động hoàn chỉnh qua character menu
- JAR: building type 11 ("Khu") → cd(1103) → `cx.c(a.a)` = request channel info
- JAR: building type 8 ("Phòng vé") → YES/NO dialog hỏi có muốn mua vé → mở teleport list
