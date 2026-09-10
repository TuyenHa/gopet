# Thành Phố Linh Thú (mapId 11) — Còn phần nào chưa làm? (jar ↔ Unity, ngày 2026-09-10)

**Bám báo cáo gốc:** [`analysis-260909-2036-linh-thu-city-parity-jar-vs-unity.md`](analysis-260909-2036-linh-thu-city-parity-jar-vs-unity.md)
**Plan liên quan:**
- `plans/260909-2100-linh-thu-city-parity-implementation/` — 8/8 phase gắn "completed" NHƯNG phase 8 là "BACKLOG PARTIAL CLOSE" (chỉ packets, không UI).
- `plans/260907-2210-map-portal-warp-shop-interaction/` — Phase 1 done, **Phase 2/3/4 vẫn PENDING**.

Kiểm tra chéo mã: `GopetUnityClient/Assets/Scripts/**`.

---

## 1. Đã hoàn thành (verified)

| Mảng | File chốt |
|---|---|
| HUD số liệu thật (HP/MP/Level/portrait từ MY_PET_INFO + STAR_INFO/MONEY_INFO/ENERGY_INFO) | `Runtime/World/CharacterHud.cs`, `GameSession.cs:117-168`, `CurrencyBar.cs` |
| Nút Menu nhân vật 11 mục (Bạn/Thư/Chat/Tủ QA/Chọn pet/Đổi MK/Cài đặt/Đăng xuất/Thoát) | `Runtime/UI/CharacterMenuButton.cs`, `CharacterMenuView.cs` |
| Pet radial 4 nút (Chơi/Hôn/Xoa + Hồi phục) cooldown 500ms | `Runtime/UI/PetActionRadial.cs`, `Net/Pet/PetActionPackets.cs` |
| Opcode 62 (Wardrobe), 91 (Guild/Equip), 92 (Community/Channel), 121 (Friend/Letter) | `Net/Social/`, `Net/Chat/ChatChannelPackets.cs`, `Net/Guild/GuildPackets.cs` |
| Hộp thư — MailboxView | `Runtime/UI/MailboxView.cs`, `Net/Social/LetterHandler.cs` |
| Ngân hàng ATM → route qua MENU_ATM Guider generic (không cần view riêng) | opcode 44 |
| Trang bị pet 5 slot + Gem + Cường/Tiến hoá | `Runtime/UI/PetEquipView.cs`, `EnchantEvolveView.cs`, `PetEquipSlot.cs`, `Net/Pet/PetEquip*` |
| ChangePassword 3-field, Chat channel selector, YesNoDialog | `Runtime/UI/ChangePasswordView.cs`, `ChatChannelSelector.cs`, `YesNoDialog.cs` |
| Portal warp wiring (bấm portal → SendWarp) | `GameSession.cs:182-183` |
| **Packets** cho long-tail (TargetPlayer/Friend/Guild/Channel/MiniGame/Language) + 22 test byte-for-byte | `Net/Social/TargetPlayerPackets.cs`, `Net/Guild/GuildPackets.cs`, `Net/MiniGame/MiniGamePackets.cs` |

---

## 2. **CÒN THIẾU** — chia 3 mức

### 2.1 P0/P1 — Chặn trải nghiệm hoặc gần chặn

| # | Việc | Trạng thái | Bằng chứng |
|---|---|---|---|
| **A** | **Building shops (Kind==0) không bấm được** — jar có 20+ building trên map 11 (shop giáp/mũ/vũ khí/thức ăn/mỹ viện/tóc/gara/hộp thư/đấu trường/recovery/…). Unity `MapRenderer.BuildMapEntities` **bỏ qua toàn bộ** entity `Kind==0`. | ❌ **CHẶN** | `Runtime/World/MapRenderer.cs:118` — `if (entity.Kind == 0 …) continue;` |
| **B** | **Portal name label** dùng `TextMesh` thô, chưa dùng `JarNameLabel` (bitmap font jar) → nhìn lệch style với NPC/player name | ⚠️ hoạt động nhưng chưa parity | `Runtime/World/MapPortalView.cs:29-32` |
| **C** | **Warp visual feedback** — chưa có fade/transition khi đổi map | ⚠️ | (không có transition asset) |
| **D** | **Tap avatar người chơi khác → TargetPlayerMenu** — API `GameSession.OpenTargetPlayerMenu(uid,name)` public (line 337) NHƯNG không call site nào trong `Runtime/World/` gọi vào. Không mời PK / xem info / kết bạn / xem đồ pet của người khác được. | ❌ | grep `OpenTargetPlayerMenu` chỉ ra định nghĩa, 0 call site |
| **E** | **Guild chat SEND** — bấm gửi ở channel Guild sẽ chỉ `RequestGuildHistory()`, không gửi thật vì thiếu clanId | ⚠️ nửa vời | `Runtime/World/GameHud.cs:155-159` — Debug.Log "chưa hoàn thiện" |

### 2.2 P2 — Có protocol/packets, thiếu UI hoặc scene

| # | Việc | Ghi chú |
|---|---|---|
| F | **4 mini-game bàn cờ** (Caro / Cờ tướng / Tiến lên / Phỏm — building type 13-16) | `MiniGamePackets.OpenMiniGame` có; **scene game engine không có** (mỗi cái 1-2w) |
| G | **Guild full flow** — Vào khu vực bang / Top lvl bang / Tạo bang / Sự kiện / Cống hiến (NPC -15) | 9 sub-command CLAN có packet; **view ranking/member list chưa dựng** |
| H | **NPC -1 Trần Trấn** — Shop Pet preview, Top Pet/Đại Gia/Phú Hộ (3 bảng xếp hạng), Nhập mã quà, Gộp đồ server cũ | Menu render qua GenericMenuView OK; **view riêng cho preview pet + ranking chưa có** |
| I | **NPC -7 Bác sĩ xì tin** — Hồi sinh cost confirm, Quest hằng ngày list, Tẩy gym | Menu OK; **quest list view chưa có** |
| J | **NPC -24 Ông già Noel** — Điểm danh calendar 30 ngày + reward preview | ❌ view chưa có |
| K | **NPC -25 Sứ giả thiên thần** — Hướng dẫn thiên đình (tour dialog dài) + Hiến tặng thú cưng | ❌ tour flow + pet-picker chưa có |
| L | **PVP invitation dialog** end-to-end (opcode 12 challenge → accept) | Verify chưa xong |
| M | **Chuyển kênh (channel)** — packet có, UI selector chưa có | `ChannelPackets` sẵn |
| N | **Toggle ngôn ngữ VN/EN** — persist qua `JarStrings.Language` + PlayerPrefs, nhưng **không có nút bấm** | Setting menu chưa gắn |
| O | **Emote/Sit animation** — `EmoteController.cs` stub client-only, chưa map clip Sit/Wave vào .anu | Không có server opcode → thuần cosmetic |
| P | **Portal-warp destinations tested** — chưa reverse chính xác 4 portal map 11 → map (15/13/16/19) đúng thứ tự hay chưa | Phase 4 test parity chưa chạy |
| Q | **Trade / gift item** | Opcode server chưa reverse |
| R | **Report abuse / Block** | Menu con target-player, chưa có |
| S | **Ẩn/hiện tên người chơi** (cd 3285) | Client-side toggle chưa có UI |

### 2.3 Bám sát danh sách building jar (chưa click được cái nào — do lỗi (A))

Đọc `client.jar_Decompiler.com/eg.java` case 0-32 — trên map 11 có thể xuất hiện các loại sau (server đặt bằng `GAME_OBJECT`):

| Type | Nhãn client | Trạng thái Unity |
|---|---|---|
| 0-4 | Nhà hẻm/mặt tiền/biệt thự/dinh thự/nhà | ❌ không render click zone |
| 7 | Vườn | ❌ |
| 8 | Phòng vé | ❌ |
| 10 | Cà phê | ❌ |
| 11 | Chuyển khu | ❌ |
| 12 | Hộp thư (building) | ❌ (menu char có, building nhân đôi thì chưa) |
| 13-16 | Caro/Cờ tướng/Tiến lên/Phỏm | ❌ scene game + click zone |
| 17 | Thời trang char (opcode 60) | ❌ |
| 18 | Nón pet (cd 1003) | ⚠️ ShopPopupView có, chưa map id |
| 19 | Giày pet (cd 1004) | ⚠️ như trên |
| 20 | Mỹ viện (cd 1005) | ❌ |
| 21 | Tóc char (cd 1006) | ❌ |
| 22 | Vật phẩm (cd 1009) | ⚠️ `RequestShop` sẵn, chưa trigger |
| 23 | Gara/xe (cd 1008) | ❌ |
| 26 | Đấu trường (opcode 58) | ❌ |
| 32 | Recovery pet (opcode 45) | ⚠️ đã có PET_RECOVERY_HP + nút HUD, building duplicate chưa |

> **Nguyên nhân chung của toàn bộ dòng ❌ trên là (A) — sửa 1 chỗ `MapRenderer.cs:118` + dựng `MapBuildingView` + bảng `buildingType → dispatcher` là mở khoá được toàn bộ.**

---

## 3. Tổng hợp — Nên làm gì tiếp

| Thứ tự | Việc | Effort | Lý do ưu tiên |
|---|---|---|---|
| 1 | Mở Phase 3 plan `260907-2210…` — dựng `MapBuildingView` + bỏ `continue` ở MapRenderer + bảng `buildingType→opcode` (7 shop core) | **M (~1.5d)** | Mở khoá 15+ điểm tương tác trên map hub — ROI cao nhất |
| 2 | Wire tap-avatar → `OpenTargetPlayerMenu` (đã sẵn API) | **S (~0.5d)** | Bật PVP/kết bạn/xem đồ — packets đầy đủ, chỉ thiếu tap event ở `WorldActorView`/`PlayerAvatar` |
| 3 | Guild chat SEND: nắm clanId (từ GuildInfo response) rồi gửi GUILD_PLAYER_CHAT | **S (~0.5d)** | Fix half-done ở `GameHud.cs:158` |
| 4 | Portal label → `JarNameLabel` + fade transition khi warp | **S (~0.5d)** | Parity thị giác |
| 5 | Điểm danh Noel (calendar 30 ngày) + Quest hằng ngày (list view) | **M** | User-facing daily hook |
| 6 | Guild full flow (Top / Tạo / Sự kiện / Cống hiến) view | **M-L** | 9 sub-command đã có packets |
| 7 | Mini-game (Caro trước — luật đơn giản nhất) | **L** | Đơn vị mini-game 6-8w mỗi cái; chọn 1 để "có gì đó chơi" |
| 8 | Channel selector + Language toggle UI | **S** | Ergonomics |
| 9 | Trade/gift — reverse opcode server trước | **L (unknown)** | Cần đọc GServer trước khi lên plan |

---

## 4. Câu hỏi chưa giải quyết

1. Server đặt **chính xác** những `type` nào trên map 11? — cần dump runtime `GAME_OBJECT` hoặc parse `maps/11.dat` (Phase 1 plan cũ tuyên bố xong nhưng chưa thấy report kết quả cụ thể trong repo).
2. Mapping `buildingType → opcode/shop id` — jar `eg.java` case 18/19/22 (Nón/Giày/Vật phẩm) dùng cd `1003/1004/1009` nhưng chưa đối chiếu 1-1 với `MenuController.SHOP_*` server (`SHOP_HAT=3, SHOP_FOOD=4…`).
3. Guild `clanId` — response nào bơm về sau khi login? Cần verify để bật Guild chat SEND.
4. PVP invitation → server dùng `SEND_YES_NO` hay flow riêng? Guider generic có bắt đủ không?
5. Portal destinations — 4 portal map 11 warp về map nào (đọc `maps/11.dat` binary hoặc dump runtime).

---

**Kết luận nhanh cho user:** Về mặt **hạ tầng giao thức**, map 11 đã cover ~95% opcode. Về mặt **UI có thể bấm trên map hub**, còn **~40%** chưa mở khoá — nút chặn duy nhất chính là **building `Kind==0` bị bỏ qua** (`MapRenderer.cs:118`), fix chỗ đó xong sẽ lộ tiếp phần lớn TODO còn lại.
