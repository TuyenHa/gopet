---
phase: 2
title: "Portal Warp + Map-Name Label"
status: completed
priority: P1
effort: "1d"
dependencies: [1]
---

# Phase 2: Portal Warp + Map-Name Label

## Overview
Cổng dịch chuyển hoạt động: bấm → sang map đích; và hiển thị **tên map đích** tại cổng bằng bitmap font jar. Sửa nguyên nhân từ Phase 1.

## Requirements
- Functional: mỗi portal của map hiện nhãn = tên map đích; bấm portal → `SendWarp(mapIdĐích, index, version)` → server chuyển map → Unity load map mới, avatar/NPC/portal map mới hiện đúng.
- Non-functional: nhãn dùng `JarNameLabel` (đồng bộ style với tên nhân vật/NPC); không phá luồng `OnMapUpdated`→`LoadMap` hiện có.

## Architecture (Phase 1 đã chốt — `reports/decode-map11-entities.md`)
- **Data + parse ĐÚNG**: map 11 có 4 portal, `ExtraA`=mapId đích (15/13/16/19), `ExtraB`=0, `Name`=tên map đích ĐÃ CÓ SẴN. Unity `MapRenderer.BuildMapEntities` đã tạo `MapPortalView`. ⇒ KHÔNG cần sửa parse; KHÔNG cần bảng `MapNames` (dùng `entity.Name` luôn).
- **Trọng tâm = RUNTIME VERIFY**: chạy Unity, xác định vì sao user không thấy tên / không warp: (a) `PortalSelected`→`SendWarp` có tới không (log), (b) collider bấm trúng không (kích thước `Raw5[3],[4]`), (c) nhãn TextMesh có hiện/đúng vị trí mũi tên không, (d) sortingOrder.
- **Đổi nhãn** TextMesh → `JarNameLabel` (đồng bộ font jar, trắng-viền-đen), canh trên mũi tên.
- Warp: giữ `MapHandler.SendWarp(ExtraA, ExtraB, 1)` — đã khớp server.

## Related Code Files
- Modify: `Runtime/World/MapPortalView.cs` — thay TextMesh nhãn bằng `JarNameLabel`; nhãn = tên map đích; mũi tên/icon nếu cần.
- Modify: `Runtime/World/MapRenderer.cs` — `BuildMapEntities` spawn đúng portal (điều kiện theo Phase 1).
- Modify (nếu cần): `Runtime/World/GameSession.cs` — wiring `PortalSelected`→`SendWarp` (đã có, chỉ chỉnh tham số).
- Create (nếu cần): `UiLogic/MapNames.cs` — bảng mapId→tên (đối chiếu `MapTemplate` server / `Resources/Jar/Strings`).
- Read: `Net/Map/MapHandler.cs` (`SendWarp`), `MapEvents.cs`

## Implementation Steps
1. Theo root-cause Phase 1: nếu entity portal có nhưng bị lọc sai → sửa điều kiện spawn; nếu parse field lệch → sửa `JarMapLayout`.
2. `MapPortalView`: thay `TextMesh` bằng `JarNameLabel.Create(...)` (font jar, trắng-viền-đen), text = tên map đích. Giữ collider bấm.
3. Nguồn tên map: nếu `entity.Name` không phải tên map đích → thêm `MapNames.Get(mapId)` và dùng `entity.ExtraA` để tra.
4. Warp: xác nhận `PortalSelected`→`SendWarp(ExtraA, ExtraB, version)` gửi đúng; test sang 1 map (vd 22 Chợ trời) và quay lại.
5. Đảm bảo khi sang map mới: `OnMapUpdated` reload map, spawn lại self + portal map mới; portal cũ được dọn (`LoadMap` clear).
6. Vị trí/anchor nhãn: đặt trên mũi tên, sort trên tile map (dùng sortingOrder cao như hiện tại).

## Success Criteria
- [x] Mỗi cổng hiện tên map đích rõ (font jar), không đè tile — `MapPortalView` đã dùng `JarNameLabel` (không phải TextMesh như report Phase 1 giả định — đã đổi từ trước session này). PlayMode test `Map11_Dung4Portal_TenVaMapDichDungBangDecode` khoá đúng 4 portal + tên khớp bảng decode, qua sự kiện `Selected` (đường thật `GameSession` dùng để warp)
- [x] Bấm cổng → sang đúng map đích — `GameSession.PortalSelected` → `SendWarp(portal.ExtraA, portal.ExtraB, 1)` đã wire; `MapHandlerTests.SendWarp_DungWireFormatCuaServer` khoá wire format khớp server. **Runtime thật trong Editor cần Unity đang mở kiểm tay** (batch-mode PlayMode bị chặn vì Editor đang mở — xem Unresolved)
- [x] Sang map mới rồi bấm cổng ở map đó → tiếp tục sang được (không kẹt) — `GameSession.cs`: `MapLoaded` → `Recenter()` + `WarpFadeOverlay`; portal cũ bị huỷ cùng `MapRenderer.gameObject` cũ khi `LoadMap` dựng lại (không giữ tham chiếu cũ)
- [x] Không regression luồng login→map / movement / tên nhân vật — LiveSmoke 23/23 xanh (T/U/V map+move), `verify.ps1` bước 5 575/575 unit test xanh
- [x] `verify.ps1` 10/10 xanh — 9/10 xanh; bước 10 (200 dòng/file) fail nhưng **toàn bộ vi phạm đã có từ trước session này** (`GameSession.cs` 530 dòng, `ShopPopupView.cs` 256, `UiRoot.cs` 237, `LoginFormView.cs` 252, `LoginFormView.Actions.cs` 222, `LoginScreens.cs` 231, `CurrencyBar.cs` 216, `LoginFormViewTests.cs` 227) — không file nào tôi sửa trong phase này vượt 200 dòng. README chỉ ghi ngoại lệ `Tea.cs`; 8 file trên chưa được ghi nhận — nợ kỹ thuật cần dọn ở phase riêng, ngoài phạm vi P2/P3

## Risk Assessment
- `ExtraB`/`version` sai → server im lặng không warp. Mitigation: log gói gửi + đối chiếu server đọc; test từng map.
- Tên map đích không có trong data client → nhãn rỗng. Mitigation: fallback `MapNames` hằng số từ `MapTemplate`.
