---
title: Map Portals/Warp + Building Shop Interaction (Unity parity với jar)
description: >-
  Cổng dịch chuyển (tên map đích + warp) và tương tác nhà/cửa hàng (giáp/mũ/vũ
  khí/thức ăn/atm/magic/gym) cho map TP Linh Thú, đối chiếu
  client.jar_Decompiler.com.
status: completed
priority: P2
branch: ''
tags:
  - unity
  - map
  - portal
  - shop
  - jar-parity
blockedBy:
  - 260905-1355-gopet-unity-client-rebuild
blocks: []
created: '2026-09-07T16:03:09.578Z'
createdBy: 'ck:plan'
source: skill
---

# Map Portals/Warp + Building Shop Interaction (Unity parity với jar)

## Overview

Map TP Linh Thú (mapId 11) trong Unity hiện thiếu 3 mảng so với jar:
1. **Sang map khác (warp)** — hạ tầng đã có (`MapPortalView`→`SendWarp` opcode 25) nhưng user không sang được → nghi entity portal không spawn / `ExtraA`(mapId)/`ExtraB`(index) sai.
2. **Tên map đích trước mũi tên** — jar hiển thị tên MAP ĐÍCH tại cổng (jar: `ef.a(ExtraA)`), Unity chưa thấy hiện.
3. **Tương tác nhà/cửa hàng** — Unity **bỏ qua toàn bộ building `Kind==0`** (`MapRenderer.BuildMapEntities` `continue`), nên các shop giáp/mũ/vũ khí/thức ăn/atm/magic/gym không bấm được.

Đối chiếu jar `eg.java` (parser entity map) + `GServer` (opcode 25 warp, `MenuController` shop). Tận dụng hạ tầng Unity sẵn có: `JarMapEntity`/`JarMapLayout` (đã parse đủ field), `MapPortalView`, `GuiderHandler`/`MenuScreen` (menu/dialog), `JarNameLabel` (bitmap font jar).

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Decode & Verify](./phase-01-decode-verify.md) | Completed |
| 2 | [Portal Warp + Map-Name Label](./phase-02-portal-warp-map-name-label.md) | Completed — code + PlayMode test viết xong; runtime warp trong Editor cần xác nhận tay (Unity đang mở, batch-mode PlayMode bị chặn) |
| 3 | [Building Shop Interaction](./phase-03-building-shop-interaction.md) | Completed — tìm và sửa bug thật: `REQUEST_SHOP` gửi sai bọc (top-level thay vì sub-command PET_SERVICE), xác nhận bằng LiveSmoke với GServer thật. Mua item thật còn chặn bởi DB test chưa seed |
| 4 | [Test & Parity](./phase-04-test-parity.md) | Completed — 625/625 unit test xanh, live-smoke byte thật xác nhận warp 2 chiều (11↔15) qua GServer đang chạy (`reports/parity-warp-shop.md`); còn đối chiếu ảnh Unity↔jar bằng tay |

## Key facts (đã khảo sát)

- **Entity format jar** (`eg.a(dv, DataInputStream)`): `kind`(byte, 0=nhà/ !=0=named), `type`(byte, buildingType 0-32; -1 nếu named), `x`(short), `y`(short), 5 byte bounds, và nhánh named: `ExtraA`(byte), `ExtraB`(byte), `name`(UTF), 1 byte skip. Unity `JarMapLayout` **đã parse đúng** (`JarMapEntity`).
- **Portal click** (jar `eg.a(Object)` case -1): `dv.a(ExtraA, ExtraB, ef.a(ExtraA))` → warp map `ExtraA`, param `ExtraB`; nhãn = **tên map đích** `ef.a(ExtraA)`.
- **Building click** (case 0-32): mở shop/game theo buildingType — menu cục bộ `cd(code)` hoặc opcode `en(81).a(sub)`.
- **Server warp** (`ON_PLAYER_WARPING=25`): `readInt(mapId), readInt(index), readInt(version)` → `MapManager.maps.get(mapId).addRandom(player)` → phát lại state → Unity `OnMapUpdated`→`LoadMap`.
- **Server shops** (`MenuController`): `SHOP_WEAPON=1, SHOP_ARMOUR=2, SHOP_HAT=3, SHOP_FOOD=4, SHOP_SKIN=7, SHOP_PET=8, SHOP_ENERGY=11`… mở qua hệ menu (`showNpcOption`/`selectMenu`/`sendMenu`).

## Dependencies

- **blockedBy** `260905-1355-gopet-unity-client-rebuild` — mở rộng Phase 06 (map render/movement) & Phase 08 (long-tail). Hạ tầng map/entity/menu từ plan đó là tiền đề.
