# Decode map 11 entities — Phase 1 (ground truth)

Nguồn: dump `GopetUnityClient/Assets/Resources/Jar/Maps/11.bytes` (parser theo `JarMapLayout`) + đối chiếu jar `eg.java`, `dc.java` + `GServer`.

## Map 11 = 24×20 tiles, 84 objects, **11 entities**, 3 waypoints

### 4 Portal (kind=1) — parse ĐÚNG, có sẵn mapId đích + tên
| e | pos (x,y) | extraA=mapId đích | extraB | name |
|---|---|---|---|---|
| 0 | (223,461) | 15 | 0 | Đại Linh Cảnh |
| 1 | (536,296) | 13 | 0 | Linh Lâm |
| 2 | (45,165) | 16 | 0 | Đường lên núi |
| 10 | (553,144) | 19 | 0 | Đấu trường |

→ `entity.ExtraA` = **mapId đích**, `entity.ExtraB` = **0 (waypointIndex)**, `entity.Name` = **tên map đích (đã có sẵn trong data)**.
→ `MapHandler.SendWarp(ExtraA, ExtraB, 1)` khớp server `ON_PLAYER_WARPING` (`readInt mapId, readInt index, readInt version`).
→ **Unity ĐÃ parse + spawn đúng** (`MapRenderer.BuildMapEntities` tạo `MapPortalView` cho kind!=0 + name). Vậy "không thấy tên / không warp" là vấn đề RUNTIME (nhãn TextMesh nhỏ/khuất, hoặc warp không tới server), KHÔNG phải parse. → Phase 2 = verify runtime + đổi nhãn sang font jar, KHÔNG phải sửa parse.

### 7 Building (kind=0) — khớp đúng 7 "shop" user cần
Từ jar `eg.a(Object)` switch(buildingType) → `en(81)` (81=buffer size; byte `.a(X)` đầu = COMMAND):

| type | pos | jar gọi | command | = |
|---|---|---|---|---|
| 27 | (133,77) | `dc.d(1)` | 2 REQUEST_SHOP, shopId=1 | **Vũ khí** (SHOP_WEAPON) |
| 28 | (49,76) | `dc.d(2)` | 2 REQUEST_SHOP, shopId=2 | **Giáp** (SHOP_ARMOUR) |
| 29 | (245,58) | `dc.d(3)` | 2 REQUEST_SHOP, shopId=3 | **Mũ/Nón** (SHOP_HAT) |
| 30 | (527,58) | `dc.d(4)` | 2 REQUEST_SHOP, shopId=4 | **Thức ăn** (SHOP_FOOD) |
| 31 | (111,356) | `en(81).a(21)` | 21 GYM | **Gym** (luyện STR/AGI/INT pet, `gym()`) |
| 32 | (484,352) | `dc.b(petId)` | 11 MAGIC + petId | **Magic** (học skill pet, `magic()`/`MAGIC_LEARN_SKILL`) |
| 9 | (439,76) | `cd(18)` menu cục bộ | (client-only) | **ATM/Bank** |

## Trả lời ATM & Gym (câu hỏi user)
- **Gym & ATM đều là BUILDING** (kind=0) trên map, KHÔNG phải NPC. Bấm nhà → gửi opcode.
- **Gym (type 31)**: bấm → `GYM(21)` → server `gym()` mở menu luyện điểm tiềm năng pet (STR/AGI/INT). Khác với **"Tẩy gym"** = option 24 của NPC -7 (Bác sĩ Xì Tin) → reset điểm gym. Hai thứ khác nhau.
- **ATM (type 9)**: bấm → mở menu cục bộ (jar `cd(18)`). Server `requestBank()` (`CHARGE_MONEY_INFO`) **RỖNG — chưa implement**. ⇒ ATM **không hoạt động** trên GServer hiện tại.

## Server request/response shop
- Client gửi `REQUEST_SHOP(2)` + shopId(byte) → `GameController.requestShop` → `MenuController.sendMenu(shopId)` trả danh sách item.
- `GYM(21)` → `gym()`; `MAGIC(11)` + petId → `magic()`; `MAGIC_LEARN_SKILL(31)`.

## Root-cause 3 triệu chứng
1. **Không sang map khác**: data+parse+SendWarp ĐÚNG → nghi runtime (PortalSelected→SendWarp không tới, hoặc collider không bấm trúng). Verify Phase 2.
2. **Không tên map trước mũi tên**: portal có name trong data + `MapPortalView` có nhãn TextMesh → nghi nhãn nhỏ/khuất/không đúng vị trí mũi tên. Đổi sang `JarNameLabel` + canh vị trí.
3. **Shop không bấm được**: `MapRenderer.BuildMapEntities` **`continue` bỏ hết kind==0** → 7 building không render/bấm. Phải thêm `MapBuildingView` + map type→command.

## Unresolved
- Vì sao portal runtime không warp/không hiện tên (cần chạy Unity để chốt: collider? sorting? PortalSelected wiring?).
- `cd(18)` (ATM) mở menu gì phía client jar (không quan trọng vì server rỗng — khả năng skip ATM).
- Định dạng gói shop response (`sendMenu`) chi tiết để render item + mua/bán (Phase 3).
