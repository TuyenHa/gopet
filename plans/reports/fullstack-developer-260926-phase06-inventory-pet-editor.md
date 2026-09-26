# Phase 6 — Inventory & Pet Editor — báo cáo triển khai (2026-09-26)

Plan: `plans/260926-1100-web-admin-nextjs/phase-06-inventory-and-pet-editor.md`
App: `D:\game\webadmin` — Server source đọc: `D:\game\SRCGOPETGOC\GServer`

## Trạng thái: DONE

## Server invariants đã đọc & áp dụng (đầu `item-json.ts`/`pet-json.ts`/`lossless-json.ts`)
- `Util/Utilities.cs:346-390` (BinarySearch/BinaryObjectAdd), `Data/User/PlayerData.cs:22-23,300-345` (addItem/addPet luôn `.Sort(BinaryCompare)`), `Util/BinaryCompare.cs` → mọi danh sách item theo invType + `pets` phải sắp tăng dần theo id, web sort lại sau mọi sửa (`sortAllInvTypes`, `sortPetList`).
- `Server/MenuController.selectMenu.cs` case `MENU_SELECT_PET_TO_DEF_LEAGUE`/`MENU_PET_INVENTORY` (~451-489): pet chọn theo/thủ đài bị gỡ khỏi `pets`, sống ở cột `petSelected`/`PetDefLeague` (1 object, không phải mảng) — `collectAllPets`/`findDuplicatePetIds` gộp cả 3 nguồn để kiểm trùng petId.
- `Data/item/Item.cs`: field cố định (`itemId`, `count`, `lvl`, `expire`, `canTrade`, `petEuipId`…), atk/def/hp/mp random lúc tạo → web KHÔNG tạo item mới, chỉ sửa `count/lvl/expire/canTrade/durability`. `durability` chỉ tồn tại khi `ShouldSerializedurability()`=true (`Util/EquipDurability.cs`, Max=80) — `applyItemEdit` ném lỗi nếu cố sửa field chưa từng có thay vì tự thêm.
- `Server/Player.cs` / `PlayerData.addItem` (~326): gộp stack theo `itemTemplateId+canTrade` — ghi chú lại, web không tự gộp (không tạo/di chuyển item).
- `Data/pet/Pet.cs:20,64`, `applyInfo` (~309-323): item xoá phải gỡ khỏi `equip` của đúng pet — `deleteItemAction` tìm pet theo `petEuipId` trên cả 3 cột rồi gọi `removeFromEquip`.
- `Adapter/JsonAdapter.cs`: Newtonsoft `DefaultValueHandling.Include`+`NullValueHandling.Include`, không camelCase.

## Big-number / lossless quyết định
- Quét toàn bộ `gopettae_tae2.player` (`items`/`pets`/`petSelected`/`PetDefLeague`) bằng `REGEXP '[0-9]{16,}'` → **không có** chuỗi số nào ≥16 chữ số. id là `int` C# (~2.1 tỷ) → `Number` JS an toàn, không cần `BigInt`.
- Phát hiện dữ liệu thật có field `float` (`gemOptionValue`) ghi dạng `5.0`/`10.0` — `JSON.parse`/`JSON.stringify` chuẩn sẽ rút gọn thành `5`/`10` (mất định dạng dù giá trị không đổi), vi phạm yêu cầu round-trip. → Tự viết bộ tokenizer JSON tối giản (`lossless-json.ts` + `lossless-json-parser.ts`, không thêm dependency vào package.json vì nằm ngoài file sở hữu) giữ số chưa sửa dưới dạng `RawNumber` (chuỗi gốc), chỉ format lại field CHÍNH TA gán giá trị mới.
- Giới hạn đã biết: object key kiểu số không âm (map `items` theo invType) bị JS tự sắp lại thứ tự — vô hại vì đây là Dictionary/HashMap phía server (thứ tự key không mang nghĩa), dữ liệu thật luôn đã tăng dần sẵn (đã kiểm tra fixture).

## Files
### Tạo mới
- `webadmin/src/lib/game-json/lossless-json-types.ts` (21 dòng) — `RawNumber`, `JsonNode`.
- `webadmin/src/lib/game-json/lossless-json-parser.ts` (172 dòng) — tokenizer.
- `webadmin/src/lib/game-json/lossless-json.ts` (59 dòng) — API công khai + doc bất biến big-number.
- `webadmin/src/lib/game-json/item-json.ts` (116 dòng) — parse/stringify `items`, `findItem`, `sortAllInvTypes`, `applyItemEdit`, `deleteItem`, `itemPetEquipId`.
- `webadmin/src/lib/game-json/pet-json.ts` (126 dòng) — parse/stringify `pets`/`petSelected`/`PetDefLeague`, `collectAllPets`, `findDuplicatePetIds`, `applyPetEdit`, `removeFromEquip`.
- `webadmin/src/lib/players/inventory-actions.ts` (196 dòng) — `updateItemFieldsAction`, `deleteItemAction` (transaction xoá item + dọn `equip`).
- `webadmin/src/lib/players/pet-actions.ts` (134 dòng) — `updatePetFieldsAction` (theo đúng cột nguồn pets/petSelected/PetDefLeague).
- `webadmin/src/app/(admin)/players/[id]/inventory-tab.tsx` (180 dòng) — bảng item theo invType + sửa/xoá + favouriteList/skin/wing (read-only, giữ thông tin từ tab cũ).
- `webadmin/src/app/(admin)/players/[id]/pet-tab.tsx` (151 dòng) — 3 khối "Đang theo/Thủ đài/Trong kho" + cảnh báo trùng petId.
- `webadmin/src/components/players/inventory-item-edit-form.tsx`, `inventory-item-delete-button.tsx`, `pet-edit-form.tsx` — client components.
- `webadmin/tests/unit/game-json-item.test.ts` (99 dòng, 8 test), `game-json-pet.test.ts` (126 dòng, 13 test).
- `webadmin/tests/fixtures/player-items-102.json`, `player-pets-4.json`, `player-pet-selected-102.json`, `player-pet-def-league-4.json` — trích THẬT từ `gopettae_tae2.player` (ID 102/4), rút gọn mảng nhưng giữ đúng định dạng số gốc (dùng chính bộ lossless parser để trim, không qua `JSON.parse` thường — tránh tự phá format khi tạo fixture). Player 102 có item đang trang bị pet (petEuipId) khớp `equip` của `petSelected` → dùng cho test xoá-item-gỡ-equip.
  - Lưu ý đặt tên: 2 file petSelected/PetDefLeague dùng tên `player-pet-selected-*.json`/`player-pet-def-league-*.json` (không khớp literal glob `player-pets-*.json` vì đây là cột object đơn, không phải mảng `pets`) — quyết định có chủ đích để tránh nhầm lẫn, vẫn nằm trong `tests/fixtures/`.

### Sửa (trong phạm vi cho phép)
- `webadmin/src/app/(admin)/players/[id]/page.tsx` — bỏ `getPlayerInventorySnapshot`/`InventoryReadonlyTab`, mount `<InventoryTab>`/`<PetTab>` (tự truy vấn DB, chỉ nhận `playerId`/`userId`).
- `webadmin/src/components/players/player-detail-tabs.tsx` — tách tab "Hành trang & Pet" cũ thành 2 tab "Vật phẩm"/"Pet".

### Không đụng (thuộc phase 5, để nguyên — không còn được tham chiếu)
- `webadmin/src/components/players/inventory-readonly-tab.tsx`, hàm `getPlayerInventorySnapshot` trong `player-detail-queries.ts` — mồ côi sau khi thay tab nhưng KHÔNG xoá (ngoài phạm vi file sở hữu).

## Bảo mật ghi (khớp bất biến offline-guard phase 4/5)
- Mọi ghi đi qua `withOfflinePlayer` (login_lock + heartbeat + NOT player_online) rồi `executeGuardedUpdate`.
- Optimistic: `WHERE ID=? AND MD5(<cột>)=?` — `expectedMd5` client gửi lấy từ `MD5(cột)` lúc render tab.
- Audit: `audited()` với `detail.before` = TOÀN BỘ text cột cũ (đọc trước, không khoá — điểm chốt an toàn thật là MD5 lúc ghi trong lock; nếu lệch giữa 2 lần đọc thì UPDATE guard tự fail sạch, không có audit `:done` sai).
- Xoá item: `conn.beginTransaction()` bọc UPDATE `items` + (nếu có) UPDATE cột pet chứa `equip` tham chiếu — rollback nếu 1 trong 2 lỗi.
- Sửa pet: client phải gửi đúng `source` (pets/petSelected/PetDefLeague); action verify lại `petId` đúng cột trước khi ghi.

## Tests
- Type check: `npx tsc --noEmit` → PASS (0 lỗi).
- Lint: `npm run lint` → PASS (0 lỗi/cảnh báo, toàn repo).
- Unit tests: `npx vitest run` → **104/104 PASS** (toàn bộ suite, gồm 21 test mới của phase 6: round-trip fixture thật (giữ `5.0` không rút gọn), sort invariant, id uniqueness (pets ∪ petSelected ∪ PetDefLeague), delete-item-clears-equip, applyPetEdit không đụng equip/skill, applyItemEdit từ chối thêm `durability` mới).
- `npx next typegen` → PASS.
- KHÔNG chạy `next build`/`next dev` (theo yêu cầu, tránh xung đột agent khác).
- Chưa test "login thật rồi dùng/trang bị/bán item" (cần GServer chạy + client) — nằm ngoài khả năng của phiên này (không có Unity client / GServer runtime trong sandbox); đề xuất lead/QA làm thủ công theo Success Criteria bước 4 của phase trước khi merge.

## Deviations / quyết định đáng chú ý
1. Không thêm dependency `lossless-json`/`json-bigint` vào `package.json` (ngoài file sở hữu) → tự viết tokenizer tối giản trong `game-json/`.
2. Tách `lossless-json.ts` thành 3 file (`-types`, `-parser`, public API) để mỗi file <200 dòng theo `development-rules.md`.
3. Đặt tên 2 fixture pet đơn khác literal glob `player-pets-*.json` (xem trên) — có ghi chú tại đây.
4. Không xoá `inventory-readonly-tab.tsx`/`getPlayerInventorySnapshot` (ngoài phạm vi) dù không còn dùng — an toàn (không lỗi biên dịch/lint), nhưng là tech-debt nhỏ nếu muốn dọn ở phase sau.
5. UI Pet cho phép sửa: `name, lvl, star(0-5), exp, str, agi, _int, hp, mp, maxHp, maxMp, tiemnang_point, skillPoint` — không sửa `equip/tatto/skill/petIdTemplate` (đổi giống loài) — diễn giải hợp lý của "chỉ số cơ bản" trong phase spec (không liệt kê cụ thể).

## Next steps / theo dõi
- QA thủ công: sửa count/xoá item/đổi chỉ số pet trên player test → đăng nhập game → dùng/trang bị/bán item còn lại bình thường, lưu lại không lỗi (Success Criteria bước 4 của phase).
- Sau khi review, cân nhắc dọn `inventory-readonly-tab.tsx`/`getPlayerInventorySnapshot` không dùng (thuộc phase 5, không tự ý xoá trong phase này).
