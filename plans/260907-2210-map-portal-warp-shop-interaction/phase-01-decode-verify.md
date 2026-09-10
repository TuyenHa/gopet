---
phase: 1
title: Decode & Verify
status: completed
priority: P1
effort: 0.5d
dependencies: []
---

# Phase 1: Decode & Verify

## Overview
Xác minh nguyên nhân portal/shop không hoạt động bằng cách DUMP entity thực tế của map 11 và đối chiếu jar, chốt mapping trước khi code. Không đoán — đo.

## Requirements
- Functional: biết chính xác map 11 có bao nhiêu entity, mỗi entity `kind/type/x/y/ExtraA/ExtraB/name`; xác nhận `ExtraA`=mapId đích, `ExtraB`=waypointIndex.
- Non-functional: không sửa runtime ở phase này (chỉ đọc/đo/ghi chú).

## Architecture
Nguồn dữ liệu: `GopetUnityClient/Assets/Resources/Jar/Maps/11.bytes` (parse bởi `JarMapLayout`). Đối chiếu jar `eg.java::a(dv,DataInputStream)` + `eg.java::a(Object)` (switch buildingType) + `GServer/Server/GameController.cs:280` (warp) + `MenuController` (shop).

## Related Code Files
- Read: `GopetUnityClient/Assets/Scripts/UiLogic/JarMapLayout.cs` (parse), `JarMapEntity.cs`
- Read: `GopetUnityClient/Assets/Scripts/Runtime/World/MapRenderer.cs` (`BuildMapEntities`), `MapPortalView.cs`
- Read: `client.jar_Decompiler.com/eg.java` (case -1 portal; case 0-32 building→shop), `ef.java` (map id→tên)
- Read: `SRCGOPETGOC/GServer/Server/GameController.cs` (opcode 25), `MenuController*.cs` (shop/menu opcodes)
- Create: `plans/260907-2210-map-portal-warp-shop-interaction/reports/decode-map11-entities.md`

## Implementation Steps
1. Viết script tạm (scratchpad, C# dotnet-script hoặc python) parse `11.bytes` theo đúng thứ tự `JarMapLayout`: header → tiles → objects → **entities** (int count; mỗi entity: byte kind, byte type, short x, short y, 5 byte, nếu kind!=0: byte ExtraA, byte ExtraB, UTF name, byte skip) → waypoints. Dump toàn bộ entities map 11.
2. Phân loại: bao nhiêu `kind!=0` (portal/named) vs `kind==0` (building). Với portal: liệt kê `ExtraA, ExtraB, name`. Đối chiếu `ExtraA` với danh sách mapId (`MapTemplate`: 12=Ải, 13=Linh Lâm, 22=Chợ trời…) để xác nhận là map đích.
3. Đối chiếu jar `ef.a(id)` — tìm bảng id→tên map phía client (jar) và nguồn tương ứng trong Unity (`Resources/Jar/Strings`? server `Language.MapLanguage`?). Chốt nguồn tên map cho nhãn portal.
4. Map buildingType→shop: từ jar `eg.a(Object)` liệt kê từng case (0-32) → hành động (menu code `cd` cục bộ hay `en(81).a(sub)` server). Ghi lại case nào là 7 shop user cần: giáp(ARMOUR), mũ(HAT), vũ khí(WEAPON), thức ăn(FOOD), atm, magic, gym. Truy `en(81)`/`cd(code)` → opcode + tham số server (`GopetCmd`, `MenuController.showNpcOption`/`selectMenu`).
5. Xác nhận wire `SendWarp` hiện tại (`MapHandler.SendWarp(mapId,index,version)`) khớp server `ON_PLAYER_WARPING` (readInt×3). Xác nhận vì sao user không sang được: (a) portal không spawn, (b) ExtraA/ExtraB sai, hay (c) label ẩn.
6. Viết `reports/decode-map11-entities.md`: bảng entities map 11 + bảng buildingType→shop-opcode + kết luận root-cause cho từng triệu chứng.

## Success Criteria
- [ ] Dump được danh sách entities map 11 (số lượng portal/building + field từng cái)
- [ ] Chốt `ExtraA`=mapId đích, `ExtraB`=waypointIndex (hoặc phát hiện khác → ghi rõ)
- [ ] Bảng buildingType→(opcode/menu server) cho ≥7 shop user cần
- [ ] Xác định nguồn tên map cho nhãn portal (Unity)
- [ ] Root-cause rõ cho 3 triệu chứng (không warp / không tên map / shop không bấm)

## Risk Assessment
- Format `11.bytes` có thể khác giả định (object/animation block trước entities) → parse sai. Mitigation: bám sát `JarMapLayout` đọc tuần tự, verify bằng count hợp lý.
- Shop có thể mở qua NPC-option (GAME_OBJECT) thay vì building → nếu vậy chuyển trọng tâm Phase 3 sang NPC menu. Ghi rõ trong report.
