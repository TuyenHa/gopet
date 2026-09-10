# Thành Phố Linh Thú — Fix 4 mục P0/P1 (2026-09-10)

**Scope:** 4 mục ROI cao từ báo cáo [`analysis-260910-1822`](analysis-260910-1822-linh-thu-city-remaining-work.md).
**Kết quả:** `verify.ps1` 9/10 (chỉ FAIL rule 200 dòng ở các file preexisting; không phát sinh violation mới).

---

## 1. Building shops clickable (item A trong báo cáo)

**Vấn đề:** `MapRenderer.cs:118` bỏ qua toàn bộ entity `Kind==0` → 20+ shop/nhà không bấm được.

**Fix:**
- **Mới:** `UiLogic/BuildingDispatcher.cs` — bảng `buildingType (0-32) → BuildingAction` khớp 1-1 với `eg.java a(Object)`. Trả `Send | LocalMenu | Toast | Noop`.
- **Mới:** `Runtime/World/MapBuildingView.cs` — clickable zone + `JarNameLabel` cho label building (Nón/Giày/Cà phê/…). Kích thước lấy từ `entity.Raw5[3][4]`, fallback 48×48.
- **Sửa:** `MapRenderer.BuildMapEntities` — thêm nhánh `Kind==0` spawn `MapBuildingView`, cascade event `BuildingSelected`.
- **Sửa:** `MapScene.cs` + `GameSession.cs` — cascade sự kiện, `OnBuildingSelected` gửi opcode / toast tương ứng.

**Đã map opcode chắc chắn** (khớp `SRCGOPETGOC/GServer/Server/MenuController.cs:423-441`):
- Type 17 (Thời trang): `PET_SERVICE / REQUEST_SHOP_SKIN (60)`
- Type 18 (Nón): `REQUEST_SHOP(3)` = SHOP_HAT
- Type 22 (Vật phẩm): `REQUEST_SHOP(4)` = SHOP_FOOD
- Type 25 (Pet shop): `REQUEST_SHOP(8)` = SHOP_PET
- Type 26 (Đấu trường): `PET_SERVICE / 58 / 0`
- Type 31: `PET_SERVICE / GYM (21)`

**Còn LocalMenu (chưa impl dialog client-side):** real-estate 0-4, garden 7, ticket 8, café 10, change-zone 11, mailbox 12, mini-games 13-16, shoes 19, beauty 20, hair 21, garage 23, generic 27-30. Bấm hiện toast "chưa mở trong Unity (menu local, chờ phase kế)". Đây không phải regression — jar cd 100x cần dialog client riêng, thuộc scope Phase 8.

---

## 2. Tap avatar khác → TargetPlayerMenu (item D)

**Vấn đề:** `GameSession.OpenTargetPlayerMenu` đã public nhưng 0 call site — không có event tap trên avatar.

**Fix:**
- **Sửa:** `Runtime/World/PlayerAvatar.cs` — implement `IPointerClickHandler` + thêm `BoxCollider2D` 48×64. Phát event `Tapped`.
- **Sửa:** `MapScene.cs` — tất cả 3 nơi spawn avatar (self / other / entering) gọi helper `SpawnAvatar()` gộp; cascade `AvatarTapped` lên GameSession.
- **Sửa:** `GameSession.Interactions.cs` (mới, partial class) — `OnAvatarTapped` filter self, mở `TargetPlayerMenu` cho người khác.

Camera đã có `Physics2DRaycaster` từ trước (`GameSession.cs:107`), tap hoạt động ngay.

---

## 3. Guild chat SEND với clanId (item E)

**Vấn đề:** `GameHud.cs:155` chat kênh Guild chỉ gửi `RequestGuildHistory()`, log "chưa hoàn thiện — cần clanId".

**Fix:**
- **Mới:** `Net/Guild/GuildInfoHandler.cs` — parse `PET_SERVICE/CLAN(91)/CLAN_INFO(14)` để lấy `clanId` self. Bỏ qua description lines (không cần cho chat SEND).
- **Sửa:** `ChatChannelPackets.SendGuildChat(int clanId, string text)` — wire format khớp `GameController.cs:2491-2493`: `[81][91][21][int clanId][utf text]`.
- **Sửa:** `GameHud.cs` — thêm `Func<int> ClanIdProvider`, dùng để gọi `SendGuildChat`. Khi clanId chưa có thì auto-gửi `RequestClanInfo()` prime + log rõ.
- **Sửa:** `GameSession.cs` — register `GuildInfoHandler`, bơm `ClanIdProvider` vào HUD, prime `RequestClanInfo()` ngay sau login.

**Đã reverse:** server `case GopetCMD.CLAN` (line 1035, top-level, không phải nested trong processPet). Đường về `clanInfo()` (2509) — `clanMessage(CLAN_INFO)` = `PET_SERVICE(81) → CLAN(91) → CLAN_INFO(14) → int clanId → sbyte count → count × UTF`.

---

## 4. Portal label + warp fade (item B, C)

**Vấn đề:** Portal name dùng `TextMesh` thô (lệch style jar); warp không có visual feedback.

**Fix:**
- **Sửa:** `MapPortalView.cs` — thay `TextMesh` bằng `JarNameLabel` (bitmap font jar, đồng bộ style với NPC/player name). Bỏ import `Gopet.Runtime.UI` không còn dùng.
- **Mới:** `Runtime/UI/WarpFadeOverlay.cs` — CanvasGroup full-screen black, fade 0.35s. `FadeOut` khi bấm portal, `FadeIn` khi map mới nạp xong. Chặn raycast trong lúc tween để tránh double-click warp.
- **Sửa:** `GameSession.cs` — dựng 1 overlay ở đầu session, hook vào `PortalSelected` + `MapLoaded`.

---

## Files thay đổi

**Mới (5 file):**
- `Assets/Scripts/UiLogic/BuildingDispatcher.cs` (~170 line)
- `Assets/Scripts/Runtime/World/MapBuildingView.cs` (48)
- `Assets/Scripts/Runtime/World/GameSession.Interactions.cs` (54)
- `Assets/Scripts/Net/Guild/GuildInfoHandler.cs` (40)
- `Assets/Scripts/Runtime/UI/WarpFadeOverlay.cs` (63)

**Sửa (7 file):**
- `Assets/Scripts/Runtime/World/MapRenderer.cs` — spawn MapBuildingView cho Kind==0
- `Assets/Scripts/Runtime/World/MapScene.cs` — cascade BuildingSelected/AvatarTapped; gộp SpawnAvatar helper
- `Assets/Scripts/Runtime/World/PlayerAvatar.cs` — IPointerClickHandler + collider + Tapped event
- `Assets/Scripts/Runtime/World/MapPortalView.cs` — JarNameLabel thay TextMesh
- `Assets/Scripts/Runtime/World/GameSession.cs` — partial, wire building/avatar/warp/guild; _hasPetFollowing tracking
- `Assets/Scripts/Runtime/World/GameHud.cs` — ClanIdProvider + SendGuildChat wiring
- `Assets/Scripts/Net/Chat/ChatChannelPackets.cs` — SendGuildChat method

---

## Verify pipeline

```
--- 1/10  GopetCmd.cs khop server         OK
--- 2/10  Asmdef references               OK
--- 3/10  Asset .dat/.png/.wav            OK
--- 4/10  netstandard2.1 compile          OK
--- 5/10  Unit test (560 pass)            OK
--- 6/10  Runtime + Unity DLL             OK
--- 7/10  Editor + Unity DLL              OK
--- 8/10  PlayMode compile                OK
--- 9/10  LiveSmoke compile               OK
--- 10/10 Rule 200 dòng                   FAIL (chỉ file preexisting: LoginFormView*, ShopPopupView, UiRoot, CurrencyBar, GameSession, LoginFormViewTests)
```

**Không có violation mới.** File mới đều <200 dòng. `MapScene.cs` = 200 (edge, đã trim). `GameSession.cs` giữ nguyên 530 (pre-existing; đã tách interactions ra file riêng để không tăng thêm).

---

## Câu hỏi chưa giải quyết

1. **Type 19 Giày (pet shoes)** — jar `cd(1004)` mở kiosk client-side; server không có `SHOP_SHOES`. Trong dispatcher đang route LocalMenu — cần verify server có expose shoes qua kiosk riêng không (KIOSK_HAT có, có tương ứng cho giày?).
2. **Types 20/21/23 (Mỹ viện/Tóc/Gara)** — chưa thấy `SHOP_*` id ở server; jar `cd(1005/1006/1008)` local. Cần đọc kỹ `MenuController.selectMenu.cs` xem có route ngầm không.
3. **Warp fade timing** — hiện `MapLoaded` fire ngay khi map render xong; nếu server gửi thêm entities/mob-zone sau đó thì fade sẽ mở khi cảnh chưa đầy đủ. Có thể cần delay 1 frame hoặc chờ `MAP_UPDATE` hoàn tất.
4. **PVP invitation** — `TargetPlayerMenu` mở qua tap avatar OK, nhưng flow "gửi mời PK → server SEND_YES_NO → user accept" chưa e2e verify với server thật.
5. **CLAN_INFO auto-prime** — hiện gửi `RequestClanInfo()` ngay sau login. Nếu server trả `showListClan()` (khi user chưa vào bang) thì handler không set clanId — đúng ý, nhưng cần verify không spam gói.

---

## Còn lại (không nằm trong scope 4 mục P0/P1)

Từ [`analysis-260910-1822`](analysis-260910-1822-linh-thu-city-remaining-work.md) — chưa làm, cần plan riêng:
- 4 mini-game scene (Caro / Cờ tướng / Tiến lên / Phỏm)
- Guild full flow view (ranking / tạo / cống hiến)
- NPC-specific view (Trần Trấn Shop Pet preview, Top rankings, Điểm danh Noel calendar, Quest hằng ngày list, tour thiên đình, hiến pet)
- Trade / gift (chưa reverse opcode server)
- Channel selector UI + VN/EN toggle
- Local menu dialog cho real-estate / café / gara / vườn / vé (nhánh LocalMenu trong dispatcher đang toast)
