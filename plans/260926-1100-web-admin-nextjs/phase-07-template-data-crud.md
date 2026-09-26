---
phase: 7
title: "Template data CRUD"
status: completed
priority: P2
effort: "8h"
dependencies: [3]
---

# Phase 7: Template data CRUD

## Overview
CRUD đầy đủ cho dữ liệu template game (DB `gopettae_tae2`). Server chỉ nạp lúc khởi động (`GopetManager.init()`, `GopetManager.cs:974-1270`) → mọi trang hiện `RestartRequiredBanner`. Dùng **một khung CRUD cấu hình hoá** thay vì viết tay từng bảng (DRY).

## Requirements
> Cập nhật sau Red Team 2026-09-26 (RT#10).

**Vòng 2a (làm trước — bảng hay sửa):** `item`, `shop`, `shoparena`, `drop_item`, `boss`, `map` + `field` (2 dòng), web `server` (3 dòng) — hai bảng cài đặt gộp vào đây thay vì trang riêng.

**Vòng 2b (thêm dần vào registry khi cần):**
| Mục | Bảng |
|---|---|
| Vật phẩm | `tier_item`, `iteminfo` (không PK, chỉ UNIQUE `ID` → dùng `ID` làm khoá) |
| Pet | `gopet_pet`, `pet_class`, `pet_element`, `pet_tier`, `pet_eff`, `petexp`, `hidden_stat` |
| Kỹ năng | `skill`, `skilllv` |
| Shop | `trade_gift` |
| NPC | `npc` |
| Bản đồ | `gopet_map_moblvl` (PK ghép) |
| Quái | `gopet_mob` (**không PK → chỉ xem**), `gopet_mob_location` (PK ghép `mapID,x,y`) |
| Nhiệm vụ / Thành tựu / Xăm | `task`, `task_type`, `achievement`, `tattoo` |
| Nâng cấp | `enchant_wing_data`, `reincarnation` |
| Bang hội | `clan_template`, `clan_skill`, `clan_skill_lvl` |

**Loại khỏi registry:**
- `exchange_gold` — không phải template mà là **hàng đợi vàng** (server cộng + DELETE lúc login, `Player.cs:541-555`) → dùng ở phase 5B "Tặng vàng".
- `clan` — dữ liệu runtime trong RAM, server save đè (`Clan.cs:447`, `AutoSave.cs:43-47`) → chỉ xem ở trang riêng.
- `option_descrtiption` — không thấy GServer truy vấn; xác minh trước khi thêm.

- Mỗi bảng: danh sách (tìm, lọc, phân trang), tạo, sửa, xoá (confirm). **Không** làm clone, không map icon sang asset Unity (đường dẫn jar ≠ Resources Unity) — chỉ hiện chuỗi `iconPath`.
- Registry hỗ trợ **PK ghép** (`pk: string[]`, route `/data/[table]/edit?pk=<json>`); bảng không PK → `readOnly: true`.
- DB có **17 FOREIGN KEY** (`server_db.sql:5250-5311`): xoá dính lỗi 1451 → bắt lỗi, báo "đang được bảng X tham chiếu"; khuyến nghị sửa thay vì xoá.
- Cột FK logic hiển thị tên (ref-picker cho item, pet, npc).
- Cột JSON/longtext → textarea kiểm JSON hợp lệ.
- Ghi chú đặc thù trong registry, vd `map.numPetDie` map 19 đang test `[5]` (gốc `[500]`).

## Architecture
```
src/lib/templates/table-registry.ts     # cấu hình mỗi bảng: db, table, pk, label, columns[{name,label,type,editable,ref?}], searchCols
src/lib/templates/template-queries.ts   # list/get generic theo registry (chỉ tên bảng/cột có trong registry → chống SQL injection)
src/lib/templates/template-actions.ts   # create/update/delete generic + audit (registry = nguồn metadata DUY NHẤT, không Drizzle)
src/lib/templates/zod-from-columns.ts   # sinh zod schema từ columns
src/app/(admin)/data/[table]/page.tsx   # list
src/app/(admin)/data/[table]/edit/page.tsx  # form sửa / new, ?pk=<json> hỗ trợ PK ghép
src/components/templates/template-form.tsx
src/components/templates/ref-picker.tsx # combobox tham chiếu item/pet/npc...
```
- `[table]` phải nằm trong registry, không thì 404. Identifier SQL lấy từ registry, value luôn parameter.
- Bảng không có PK (nếu có) → chỉ xem, hoặc PK ghép từ registry.
- Ghi chú đặc thù trong registry, vd `map.numPetDie` (xem memory: map 19 đang test [5], gốc [500]).

## Related Code Files
- Create: các file trong Architecture
- Modify: `sidebar-nav-config.ts` (link tới `/data/<table>`)

## Implementation Steps
1. Soát PK thật của từng bảng trong `server_db.sql` (khối `ALTER TABLE ... ADD PRIMARY KEY`).
2. Viết registry cho `item` trước, hoàn thiện khung generic, rồi thêm các bảng còn lại.
3. Ref picker cho item/pet/npc/map/skill.
4. Banner restart trên mọi trang `/data/*`; sau mỗi lần lưu toast "Đã lưu — cần restart GServer".
5. Test: sửa giá item trong `shop` → restart server → client thấy giá mới.

## Success Criteria
- [x] Tất cả bảng trong danh sách CRUD được qua 1 khung chung
- [x] Tên bảng/cột ngoài registry bị từ chối
- [ ] Sửa template + restart → thay đổi hiện trong client Unity (pending manual E2E: template edit + GServer restart)
- [x] Audit log mọi thay đổi

## Risk Assessment
- Xoá template đang được người chơi dùng (item trong hành trang) → server có thể lỗi. Giảm thiểu: trước khi xoá `item`/`gopet_pet`, đếm tham chiếu bằng `player.items LIKE '%"itemTemplateId":<id>,%'` và cảnh báo; khuyến nghị sửa thay vì xoá.
- Cột có ngữ nghĩa ẩn (chuỗi id ngăn cách) → ghi mô tả trong registry.

## Security Considerations
- Allowlist bảng/cột; không bao giờ nội suy input người dùng vào identifier.
