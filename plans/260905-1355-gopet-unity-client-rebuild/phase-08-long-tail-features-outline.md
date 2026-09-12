---
phase: 8
title: "Long Tail Features (Outline)"
status: in-progress
priority: P3
effort: "8-12w"
dependencies: [7]
---

# Phase 8: Long Tail Features (Outline)

## Implementation progress (2026-09-11)

| Workstream | Status | Evidence |
|---|---|---|
| Server dialogs | Complete | Popup/banner, red/green modal, image captcha and captcha follow-up input are wired through the real router/UI stack. |
| HUD/world critical updates | Complete | `UPDATE_HP_BOSS`, `TIME_PLACE`, `SHOW_BIG_TEXT_EFF`, `FAST_REMOVE_MOB`, pet evolution and the gem inventory/enchant/up-tier flows are implemented. `REMOVE_MOB(42)` and `GL_MAIL(20)` confirmed never sent by server — N/A. |
| Client request throttling | Complete baseline | Challenge, PK and building/shop entry requests have cooldown feedback. |
| Server hardening | Complete baseline | Kiosk self-buy/duplicate-buy/atomic coin debit, final admin authorization checks, invalid pet-evolution guards, server-side path-distance/rate validation and temporary speed-hack bans are enabled. Live tuning of thresholds remains an operations task. |
| Guild / Clan | Complete | Full handler+parser+models+packets (`GuildInfoHandler`, `GuildPackets`, `GuildModels`), 5-tab `GuildView` UI (Info/Members/Chat/Top/Skills), `GuildNameLayer` for avatar clan tags, wired into `GameSession`. |
| GenericMenuView coverage | Complete | Kiosk browse, pet tattoo, reincarnation, sacrifice, fusion, learn skill, lock/unlock items, merge part, star pet — all confirmed to flow through `sendMenu` → `COMMAND_GUIDER` → `GenericMenuView`. No feature-specific code needed. |
| Remaining long-tail domains | Partial | Kiosk owner card renders frame, full pet details and supports sell/cancel. Arena hub, registration, shop, top and League defender/top are wired. ChallengePlace/MarketPlace live-certified (2026-09-11); ArenaPlace/ClanPlace live certification and audio coverage remain blocked/pending. |

Verification: Gopet.Net, Gopet.Runtime, Gopet.PlayModeTests all compile with 0 errors; `dotnet test tests/Gopet.Net.Tests` 625/625; `Gopet.Net.LiveSmoke` green against a running GServer, including 2 new place-entry checks (2026-09-11).

Progress snapshot: **~65 complete, ~6 partial, ~7 not started, 1 moved to its own plan (Pet League)** — remaining work is concentrated in ArenaPlace/ClanPlace live certification (both blocked on non-code decisions), audio coverage, migration/release decisions and the separate admin dashboard.

### Remaining implementation backlog (updated 2026-09-11)

- **Offline Pet League engine** — **moved out of phase-08, into its own future plan** (user decision, 2026-09-11). Scout investigation found `OP_PET_LEAGUE_BETA` has zero server code (not even a stub — only the opcode constant exists) and the Unity client has zero League code of any kind (no packets, handlers, or UI). This is a from-scratch two-sided feature, not a small gap, and needs product decisions first (battle-resolution style, reward formula, matchmaking, defender AI) — out of scope for a mechanical phase-08 pass. `PetDefLeague` persistence (defender selection) and `TopPetLeague` ranking already work and are unaffected.
- **PK-card selection** — server implementation of `MENU_SELECT_ITEM_PK` after the product rule for consuming PK cards is confirmed.
- **ArenaPlace live certification** — blocked, see §11 (wall-clock event, no forced-trigger path; decided not to add a debug opcode).
- **ClanPlace live certification** — blocked, see §11 (needs a funded test account to create/join a clan; 220k currency, smoke account has 0).
- **ChallengePlace / MarketPlace live certification** — ✅ done, see §11 (`ChallengePlaceChecks.cs`, `MarketPlaceChecks.cs`, live-smoke green against real GServer).
- **Migration rehearsal** — backup, dry run, idempotency and rollback for JAR-player migration.
- **Audio live-play certification** — all ten J2ME SFX triggers are wired; verify their volume and timing during a real combat/map-transition session.
- **Admin HTTP dashboard** — intentionally separate from the Unity client.
- **AntiPK client indicator** — blocked on a protocol decision: no wire field currently carries `Player.AntiPK` (or the wing's `itemOption`/`itemOptionValue` it's derived from) to any client. Adding one means changing the packet format, which the project's own ground rule forbids without an explicit go-ahead.

## Overview

Phần đuôi dài: các tính năng còn lại, animation/âm thanh, và **bật lại chống gian lận trước khi phát hành**.

> **Đây là phác thảo, chưa phải plan chi tiết.** Viết chi tiết sau khi hết P6/P7.

## Cơ sở đối chiếu

Tất cả entry dưới đây có xuất xứ verify được bằng grep vào một trong ba nguồn:
- `SRCGOPETGOC/GServer/Server/GopetCMD.cs` — **175 opcode**
- `SRCGOPETGOC/GServer/Server/MenuController.cs` — **216 hằng số** (`MENU_*`, `OP_*`, `DIALOG_*`, `INPUT_TYPE_*`, `IMGDIALOG_*`), **142 case trong `selectMenu.cs`** + **121 case trong `sendMenu.cs`**
- Client jar `client.jar_Decompiler.com/*.java` (232 file)

Cột "Unity" ghi trạng thái theo `Assets/Scripts/` **tại thời điểm viết plan này** (2026-09-10):
- ✅ Có handler + UI
- 🟡 Chỉ hằng số opcode / packet gửi, chưa có handler nhận hoặc UI riêng
- ⬜ Chưa có gì

## 1. Server-driven dialog thiếu — CHẶN NHIỀU LUỒNG (P1 upgrade)

Server dùng 4 loại dialog "flat" (không đi qua `COMMAND_GUIDER`) để bắn thông báo. Unity mới đăng ký 1/4, phần lớn phản hồi server hiện không hiện gì:

| Sub / opcode | Nguồn | Unity | Ghi chú |
|---|---|---|---|
| `SERVER_MESSAGE`/`POPUP_MESSAGE` (45/5) | `Player.cs:712 Popup()` | ✅ | Toast runtime đã nối vào `UiRoot` |
| `SERVER_MESSAGE`/`BANNER_MESSAGE` (45/1) | `Player.cs:721 showBanner()` | ✅ | Đã nối ticker trên HUD |
| Opcode **10** (`redDialog`) | `Player.cs:382` | ✅ | Modal lỗi đỏ sau khi login |
| Opcode **71** (`okDialog`) | `Player.cs:730` | ✅ | Modal thành công xanh sau khi login |
| `SERVER_MESSAGE`/`SEND_YES_NO` (45/4) | `Player.cs`/`MenuController.showYNDialog` | ✅ | Đang có `ChoiceDialogView` |
| `GUIDER_IMGDIALOG` (11) + `IMGDIALOG_CAPTCHA` (0) | `GameController.showCaptchaDialog` (`171`) | ✅ | Tải ảnh, xác nhận dialog và gửi input captcha qua UI server-driven |

**Hệ quả nghiêm trọng**: mọi luồng đã port đều "chạy" theo nghĩa đúng byte, nhưng khi server từ chối (thiếu tiền, sai type, bị ban…), user không thấy gì ⇒ trải nghiệm như bug. Cần đăng ký nốt trong `UiRoot` như đang có với `YesNoAsked`.

## 2. Chợ (Kiosk) — phần lớn nội dung "long tail" nằm ở đây

Chợ giữa player-với-player, quy mô: **7 loại kiosk × 2 chiều (xem/chủ)** + 1 màn hình quản lý:

| Menu server | Nguồn | Unity |
|---|---|---|
| `MENU_KIOSK_HAT/WEAPON/AMOUR/GEM/PET/OHTER` (806-811) | `Kiosk.cs`, `MarketPlace.cs` | ✅ qua GenericMenuView (sendMenu) |
| `MENU_KIOSK_*_SELECT` (1006-1011) — xem sạp cụ thể | `MenuController.selectMenu.cs:1329-...` | ✅ qua GenericMenuView |
| `MENU_OPTION_KIOSK` (1088), `MENU_OPTION_KIOSK_CANCEL_ITEM` (1089), `MENU_OPTION_BUY_KIOSK_ITEM` (1090) | `sendMenu.cs:1002,1013` | ✅ qua GenericMenuView |
| `SHOW_TATTO_PET_IN_KIOSK` (99) | `GopetCMD.cs:164` | ✅ owner card renders frame plus level/stat/skill/tattoo; server enriches the existing KIOSK description field without changing the J2ME wire layout |
| `SELECT_KIOSK_ITEM` (85), `REMOVE_SELL_ITEM` (87) | `GopetCMD.cs:115-117` | ✅ `KioskHandler` + `KioskListingView` parse owner state and send sell/cancel actions |

**Lỗ hổng kinh tế đã báo (mục 4 report `analysis-260905-1330`)**: ✅ đã xử lý trong `Kiosk.cs` — chặn mua đồ của chính mình bằng `return`; khấu trừ coin nguyên tử bằng `TrySpendCoin` bên trong mutex; và cộng `sumVal` đúng bằng giá thực tế đã khấu trừ khi mua lẻ.

**Kiosk pet**: owner card now uses the existing description field for frame + level + stat + skill + tattoo, preserving the J2ME packet layout. Browse/select remains server-driven through `MENU_KIOSK_PET_SELECT` (1010).

## 3. Guild / Clan (bang hội)

Toàn bộ khung xương server (sub-command của `PET_SERVICE`/`CLAN` = 81/91):

| Sub | Tên | Unity | Ghi chú |
|---|---|---|---|
| 1 | `GUILD_LIST` | ✅ | `GuildInfoHandler` parse + `GuildView.Info` hiện danh sách |
| 2 | `GUILD_JOIN` / `GUILD_REQUEST_JOIN` | ✅ | `GuildPackets.RequestJoin` + nút "Gia nhập" trong `GuildView` |
| 3 | `GUILD_LIST_MEMBER` / `CLAN_INFO_MEMBER` | ✅ | `GuildView.Members` hiện danh sách + nút đuổi nếu có quyền |
| 6 | `GUILD_KICK_MEMBER` | ✅ | `GuildPackets.KickMember` wired qua `GameSession` |
| 9 | `DONATE_CLAN` / `PLAYER_DONATE_CLAN` (10) | ✅ | `GuildView` hiện options + confirm |
| 13 | `SEARCH_GUILD` | ✅ | Thanh tìm kiếm trong `GuildView.Info` |
| 14 | `CLAN_INFO` | ✅ | `GuildView.Info` hiện description lines |
| 15/16 | `GUILD_TOP_GROWTH_POINT/TOP_FUND` | ✅ | `GuildView.TopSkills` hiện xếp hạng |
| 20/21/22 | `GUILD_CHAT`/`PLAYER_CHAT`/`ON_PLAYER_CHAT` | ✅ | `GuildView.Chat` lịch sử + real-time |
| 23 | `GUILD_NAME_IN_PLACE` | ✅ | `GuildNameLayer` + `PlayerAvatar.SetClanName` hiện tag trên avatar |
| 24-27 | `GUILD_CLAN_SKILL`/`UNLOCK_SKILL`/`RENT_SKILL`/`SHOW_OHTER_...` | ✅ | `GuildView.TopSkills` hiện 3 slot + thuê/đổi |

Server-side có `ClanPlace`, `ClanHouseTemplate`, `ClanSkillTemplate`, `ClanSkillLvlTemplate`, `ClanPotentialSkill`, `ClanMember`, `ClanRequestJoin`, `ClanMemberDonateInfo` (12 file `Data/Clan/`). **View quản lý bang (approve/upgrade nhà chính/nhà kỹ năng)** cần dựng riêng, jar dùng nhiều menu lồng nhau (`MENU_APPROVAL_CLAN_MEMBER` 1024, `MENU_UPGRADE_MEMBER_DUTY` 1028, `MENU_SELECT_SKILL_CLAN_TO_RENT` 1030, `MENU_PLUS_SKILL_CLAN` 1031, `MENU_SELECT_TYPE_MONEY_TO_RENT_SKILL_CLAN` 1057).

## 4. PvP full (PK, Arena, Pet League)

Đã có `PLAYER_CHALLENGE` (12) + `PLAYER_BATTLE` (59) trong P7 LiveSmoke. Còn thiếu:

| Feature | Opcode / menu | Unity |
|---|---|---|
| PK 1-hit (dùng thẻ PK) | `PLAYER_PK` (96, sub của PET_SERVICE), `MENU_SELECT_ITEM_PK` (1012) | 🟡 target-player UI + packet wired; PK-card selection flow is still not implemented server-side |
| Arena vé báo danh + top | `MENU_SELECT_TYPE_PAYMENT_TO_ARENA_JOURNALISM` (1052), `ArenaEvent.cs` | ✅ building opens `ARENA_MENU` hub; registration, accumulated-point top and shop use server-driven UI |
| Pet League (chọn pet phòng thủ, top) | `MENU_SELECT_PET_TO_DEF_LEAGUE` (1051), `OP_SELECT_PET_DEF_LEAGUE` (65), `OP_PET_LEAGUE_BETA` (64), `OP_TOP_AREAN_POINT` (66) | 🟡 defender selection (`PetDefLeague`) + `TopPetLeague` ranking work. `OP_PET_LEAGUE_BETA` (the actual battle) is 100% unimplemented — no server handler, no Unity client code at all. **Moved to a separate future plan** (2026-09-11) — needs product decisions (battle-resolution style, reward, matchmaking, defender AI) before implementation, too large for a phase-08 mechanical pass |
| AntiPK (buff từ cánh) | `Player.cs:982 UpdateAntiPK()` | 🟡 blocked — server tracks `Player.AntiPK` purely server-side (checked in `confirmpk`, `GameController.cs:3720`) and never puts it on the wire to any client; `WING/3` sync only carries frame path/count (`WingHandler.cs`), no item-option data. Adding a visible indicator needs a new wire field, which violates the "no protocol byte changes" ground rule — needs an explicit decision, not a silent workaround |
| Shop đấu trường thường + sinh tử | `OP_SHOP_ARENA` (11), `ShopArena.cs`, `ShopArenaTemplate.cs` | ✅ exposed from Arena hub through the generic payment/shop UI |
| Vượt ải (dùng `ChallengePlace`) | NPC `-28` option 10, `ChallengePlace.cs` (223 dòng) | ✅ NPC option + shared map/battle/time-place pipeline; needs live scenario certification |
| Top challenge | `OP_SHOW_TOP_CHALLENGE` (80), `TopChallenge.cs` | ✅ server-driven readonly top UI |

## 5. Pet đầy đủ — mảng chưa cover

Phase 5.1 (Linh Thú city) đã làm `PetEquipView` 5-slot + `EnchantEvolveView`. Còn:

| Feature | Menu / opcode | Unity |
|---|---|---|
| Tattoo pet (5 màn hình) | `MENU_SHOW_ALL_TATTO` (1054), `TATTOO` (90) + 6 sub, `SELECT_ITEM_GEN_TATTO` (1014), `SELECT_ITEM_REMOVE_TATTO` (1015), `SELECT_MATERIAL1/2_TO_ENCHANT_TATOO` (1046/1047) | ✅ qua GenericMenuView |
| Cánh (Wing) + cường hoá | `WING` (92) + 6 sub, `EnchantWingData.cs`, `MENU_WING_INVENTORY` (81040), `MENU_MERGE_WING` (1053), `SELECT_MATERIAL_TO_ENCAHNT_WING` (1043), `SELECT_MONEY_TO_PAY_FOR_ENCHANT_WING` (1044) | ✅ kho cánh có action dùng/gỡ/cường hóa; payment/material/confirm qua menu generic; `WING/3` đồng bộ animation trên mọi avatar |
| Skin nhân vật | `SKIN_INVENTORY` (62), `MENU_SKIN_INVENTORY` (803), `MENU_UNEQUIP_SKIN` (1038), `SEND_SKIN` (61), `REQUEST_SHOP_SKIN` (60) | ✅ shop/kho/dùng/gỡ qua menu generic; `SEND_SKIN` đồng bộ renderer, hướng và bước chân cho mọi avatar trong map |
| Pet reincarnation (trùng sinh) | `MENU_PET_REINCARNATION` (1085), `OP_REINCARNATION` (9), `PetReincarnation.cs` | ✅ qua GenericMenuView |
| Pet fusion (dung hợp) | `MENU_FUSION_MENU_EQUIP/PET/EQUIP_OPTION/…` (1078-1083), `OP_DUNG_HỢP` (86), `MENU_ADMIN_BUFF_DUNG_HỢP` (1087) | ✅ qua GenericMenuView |
| Pet sacrifice (hiến tặng) | `MENU_PET_SACRIFICE` (1084), `OP_HIẾN_TẶNG_THÚ_CƯNG` (89) | ✅ qua GenericMenuView |
| Merge part item/pet (ghép mảnh) | `OP_MERGE_PART_PET` (15), `OP_MERGE_ITEM` (19), `MENU_MERGE_PART_PET` (1004), `MENU_SELECT_ITEM_PART_FOR_STAR_PET` (1013), `SELECT_ITEM_MERGE` (1068-1070) | ✅ qua GenericMenuView |
| Star pet (nâng sao) | `OP_UPGRADE_STAR_PET` (8), `MENU_SELECT_ITEM_PART_FOR_STAR_PET` (1013) | ✅ qua GenericMenuView |
| Học skill mới cho pet | `MENU_LEARN_NEW_SKILL` (799), `PetSkill*.cs` (3 file) | ✅ qua GenericMenuView |
| Xoá điểm tiềm năng (reset gym) | `MENU_DELETE_TIEM_NANG` (800), `OP_DELETE_TIEM_NANG` (24) | ✅ qua GenericMenuView |
| Support/heal khác | `MENU_SELECT_ITEM_SUPPORT_PET` (1016), `MENU_SELECT_SLOT_USE_SKILL_CARD` (1075) | ✅ qua GenericMenuView |

## 6. Item / kho đồ / merge server cũ

| Feature | Menu / opcode | Unity |
|---|---|---|
| Kho đồ tổng | `MENU_NORMAL_INVENTORY` (81004) | ✅ lối vào “Rương đồ” từ menu nhân vật; danh sách cuộn server-driven và chọn trang bị hoạt động |
| Kho tiền tệ (MoneyDisplay) | `MENU_ITEM_MONEY_INVENTORY` (1027), `MENU_MONEY_DISPLAY_SETTING` (1066) | ✅ inventory + pin/unpin settings flow through GenericMenuView; pinned currencies render in HUD |
| Bán rác | `MENU_SELL_TRASH_ITEM` (1067), `OP_SELL_TRASH_ITEM` (79) | ✅ danh sách vật phẩm + confirm + input số lượng + success/error dialog dùng pipeline generic |
| Khoá / mở khoá item (chống mất trộm) | `MENU_LOCK_ITEM_PLAYER` (1073), `MENU_UNLOCK_ITEM_PLAYER` (1074) | ✅ qua GenericMenuView |
| Buff exp / vật phẩm huỷ đồ | `BuffExp.cs`, `SHOW_EXP` (95) khi có buff | ✅ buff có icon/countdown HUD; huỷ trang bị pet có confirm hai lớp và cập nhật inventory từ server |
| Xem hidden stat | `HiddenStatItemTemplate.cs`, `ItemBattleOptionBuff.cs`, `HIDDEN_STATS_INFO` (101) | ✅ nút “Kích ẩn” trong `PetEquipView`; request không mang user/pet ID, server chỉ hiển thị kích ẩn của pet đang theo của chính người gọi |
| Merge server cũ (data migration người chơi) | `OP_MENU_MERGE_SERVER` (81), `INPUT_TYPE_NAME_PLAYER_TO_ENBALE_MERGE_SERVER` (26), `MENU_ADMIN_MAP` (1019), `MENU_SHOW_ALL_ITEM` (1041) | 🟡 generic menu/input contract works; production migration policy and end-to-end data rehearsal remain |

## 7. Task / achievement / top

| Feature | Menu / opcode | Unity |
|---|---|---|
| Task chính / phụ | `MENU_SHOW_LIST_TASK` (1033), `MENU_SHOW_MY_LIST_TASK` (1034), `MENU_OPTION_TASK` (1035), `SHOW_LIST_TASK` (54), `OP_MAIN_TASK` (0), `OP_EVENT_TASK` (68), `Task*.cs` | ✅ menu nhân vật gửi `SHOW_LIST_TASK`; danh sách, tiến độ, nhận thưởng và huỷ dùng menu server-driven hiện có |
| Danh hiệu (achievement) — dùng/gỡ | `MENU_USE_ACHIEVEMNT` (1058), `MENU_USE_ACHIEVEMNT_OPTION` (1059), `MENU_ADMIN_SHOW_ALL_ACHIEVEMENT` (1055), `MENU_ME_SHOW_ACHIEVEMENT` (1056), `OP_USE_ACHIEVEMENT` (77) | ✅ menu dùng/gỡ + rich `AnimationMenu` chi tiết + decoration đang trang bị trên avatar |
| Top pet / vàng / gem / chi tiêu / event / clan | `Top.cs`, `TopPet.cs`, `TopGold.cs`, `TopSpendGold.cs`, `TopGem.cs`, `TopEvent.cs`, `TopLVLClan.cs`, `TopChallenge.cs`, `TopAccumulatedPoint.cs`, `TopFlowerGold/Gem` (`OP_XEM_TOP_FLOWER_*` 83/84) | ✅ menu server-driven có viewport cuộn thật, virtualization/reuse row và hỗ trợ dòng chỉ đọc |
| Gift code | `INPUT_TYPE_GIFT_CODE` (17), `OP_TYPE_GIFT_CODE` (60) | ✅ NPC option → `InputDialogView` → reply server; có cooldown/error dialog |
| Điểm danh hằng ngày | `OP_ĐIỂM_DANH` (87) | ✅ NPC option + phản hồi success/error server-driven |
| Hướng dẫn lên thiên đình (map 26+ requires wing skin) | `OP_HƯỚNG_DẪN_LÊN_THIÊN_ĐÌNH` (88), `CheckSky` (`GameController.cs:4980`) | ✅ NPC option + hướng dẫn/error dialog; warp/map update dùng pipeline hiện có |

## 8. Bảo mật + auth flow chưa cover

| Feature | Nguồn | Unity |
|---|---|---|
| **Captcha ảnh** (chống bot đánh quái) | `IMGDIALOG_CAPTCHA` (0), `GameController.randomCaptcha/showCaptchaDialog`, `attackMob` chặn khi có captcha | ✅ |
| **2FA OTP** (đăng nhập 2 lớp) | `INPUT_OTP_2FA` (34), `Player.cs:395 show2FADialog()`, `MenuController.inputDialog.cs:695` | ✅ qua `InputDialogView` server-driven |
| **Rate limit login IP** | `Player.cs:606 PlayerManager.Ipv4Tracker` | Không cần client |
| **AntiPK từ cánh** | `Player.cs:982` | 🟡 blocked pending protocol decision — see §4 for detail (no wire field carries this to client today) |
| Đổi mật khẩu | `CHANGE_PASSWORD` (93) | ✅ P5 |

## 9. Emote / animation nhân vật

| Feature | Opcode | Unity |
|---|---|---|
| Menu emote (`showAnimationMenu`) | `ANIMATION_MENU` (100), `SEND_ANIMATION_CHARACTER` (4), `SEND_LIST_ANIMATION_CHARACTER` (5), `AnimationMenu.cs` (74 dòng, có Label/Animation elements + FontStyle) | ✅ rich label/image/animation dialog đã nối `UiRoot`; command đóng hoạt động đúng contract server hiện tại |
| Danh sách animation người khác trong map | `sendListAnimationOfAllPlayers` (`GopetPlace.cs:71`) | ✅ parse stream không-count, giữ pending theo userId, render/cập nhật decoration trên avatar |

## 10. HUD / world updates còn thiếu handler

| Opcode | Sender server | Unity |
|---|---|---|
| `UPDATE_HP_BOSS` (89) | Boss lớn hiển thị HP realtime | ✅ |
| `AUTO_ATTACK_SUPPORT` (22) | Auto-attack (hỗ trợ farming) | ✅ menu nhân vật gửi đúng `PET_SERVICE/22`; server tự chọn quái rảnh và bắt đầu battle |
| `FAST_REMOVE_MOB` (99) | Đóng battle theo owner/battle ID khi server dọn trận | ✅ |
| `REMOVE_MOB` (42) | Xoá quái sau khi bị đánh chết | N/A — server never sends this; mob removal uses `FAST_REMOVE_MOB` (99) |
| `TIME_PLACE` (64) | Bộ đếm thời gian ở place (raid) | ✅ |
| `SHOW_BIG_TEXT_EFF` (63) | Text hiệu ứng lớn (Combo, KO, event) | ✅ |
| `SHOW_UPGRADE_PET` (67) | Mở màn tiến hoá pet | ✅ |
| `PRICE_UPGRADE_PET` (72) | Giá tiến hoá pet | ✅ |
| `INFO_UP_TIER_PET` (70) / `PET_UP_TIER` (71) | Info + kết quả tiến hoá | ✅ |
| `SELECT_PET_UPGRADE` (68) / `PET_UPGRADE_ACTIVE/PASSIVE` (1/2) | Chọn pet chính / pet nguyên liệu | ✅ |
| `PET_UPGRADE_PET_INFO` (69) | Info pet trong dialog tiến hoá | ✅ |
| `SEND_GEM_INFo` (84) | Info gem | ✅ cập nhật trực tiếp kho gem |
| `SELECT_GEM_UP_TIER` (81), `UP_TIER_GEM_ITEM` (79) | Nâng cấp gem | ✅ chọn nguyên liệu + xác nhận server-driven |
| `mapTeleMenu` (`MGO_COMMAND=42` / `TELE_MENU=12`) | Danh sách teleport nhanh (admin/vip) | ✅ menu nhân vật yêu cầu danh sách, parser đủ map/name/description/waypoint và chọn map qua `ON_PLAYER_WARPING` |
| `changeChannel` (`ON_PLAYER_CHANGE_CHANNEL=24`) | Đổi kênh trong map | ✅ menu nhân vật → chọn khu |
| `getChannelInfo` (`ON_PLAYER_GET_CHANNEL_INFO=7`) | Info kênh | ✅ parser danh sách không-count + số người/khoá |
| `GL_MAIL` (20) — server-mail (banner + reward) | `LETTER_COMMAND` sub | N/A — server never sends this sub-command |

## 11. Place types phi-mặc-định

| Place | Nguồn | Unity |
|---|---|---|
| `ArenaPlace` | 26+ dòng, riêng cho arena/PVP tournament | 🟡 blocked — entry is a wall-clock scheduled event (`ArenaEvent.Condition`: fires only at hours 6/8/11/13/17/19/21/23, minute ≤5) with random player pairing, bypassing `ON_PLAYER_WARPING` entirely (`ArenaData` teleports directly). No client-triggerable path exists to force it for a live-smoke check. Decided NOT to add a debug/admin force-start opcode just for testing (avoids growing attack surface) — this stays a manual-verification item, to be exercised by hand during a real event window |
| `ChallengePlace` | 223 dòng, boss raid có timeout, ban check | ✅ certified live — `ChallengePlaceChecks.cs` (`Gopet.Net.LiveSmoke`) warps to map 12, confirms `TIME_PLACE`/`SHOW_BIG_TEXT_EFF` fire on entry (wait-phase broadcast), and warps back out. Real bytes, GServer live. Wave/turn progression (60s wait + `top_challenge` persistence) not exercised — would need holding the connection ~1min+, deferred as low-value beyond entry proof |
| `ClanPlace` | Nhà bang hội, chat riêng, không đi qua `ON_PLAYER_WARPING` mà qua `OP_ENTER_CLAN_PLACE` (NPC option 44, NPC "SỨ GIẢ BANG HỘI" id -15/-26) | 🟡 blocked — entering requires the test account to already be in a clan, and no clan exists in the dev DB; creating one costs 200,000 coin + 20,000 gold (`GopetManager.COIN_CREATE_CLAN/GOLD_CREATE_CLAN`) which the auto-provisioned smoke account has none of. Not solved by hacking currency into the DB — flagging as needing either a funded test account or a cheaper test path, not attempted unilaterally |
| `MarketPlace` | Chợ (kiosk) — spawn theo kiosk data JSON, không phải mob/npc | ✅ certified live — `MarketPlaceChecks.cs` warps to map 22, confirms `MAP_UPDATE` for the correct map, warps back. Kiosk *interaction* (browse/buy) is separately covered by `ShopChecks`/existing kiosk work — this check only proves the entry path |

Client phải render đúng cả 4 vì server placement/behavior khác `GopetPlace` mặc định.

## 12. Sự kiện theo năm — quyết định phạm vi

Server có 4 event file (1.088 dòng, `Data/Event/Year2024,Year2025`):
- `Summer2024Event` (324 dòng) — làm diều, xếp hạng diều thường/VIP, hướng dẫn
- `TeacherDay2024` (241 dòng) — quà tặng hoa
- `Winter2024Event` (31 dòng) — bánh chưng/bánh tét, `OP_MAKE_CYLINDRICAL_STICKY_RICE_CAKE` (90), `OP_MAKE_SQUARE_STICKY_RICE_CAKE` (91)
- `GameBirthdayEvent` (521 dòng, 2025) — hộp quà, milestone, shop sinh nhật, guide

+ 3 event nền: `ArenaEvent` (189 dòng, ArenaJournalism), `BannerEvent`, `DailyBossEvent` (112 dòng).

**Quyết định cần chốt trước khi làm**: bỏ event đã hết hạn (2024) → cắt được ~600 dòng port + giảm menu phải làm. Nhưng phải chắc **server có tắt các menu này ra khỏi menu NPC** khi event hết — không thì bấm vào sẽ rơi vào path lỗi không có handler client.

## 13. HTTP API server (ngoài giao thức game)

Server chạy song song HTTP API cổng 8082 (`APIs/ServerController.cs` — 26 endpoint):
- Bảo trì, maintenance, shutdown
- Set % exp/gem drop
- Set boss thủ công
- Auto banner theo lịch
- Buff item admin
- Test daily boss

**Không cần Unity port**, nhưng cần **dashboard admin web** để vận hành. Tách ra thành plan riêng nếu làm — không phải phần của client.

## Animation & âm thanh

- Format `.anu` (**6 file actor** + **27 file battle skill** trong jar, đã reverse trong P5.1/P7 — `MapAnimatedObjectView` + `BattleActorEffectView` dùng được)
- Sprite nhân vật/pet: `anim_characters/`, `anim_pets/`, `anim_wings/` **trên server** (tải qua `RemoteAssetCache`) — chưa xác nhận cover đủ 3 loại
- Âm thanh: **10 file `.wav`** trong jar (`s_attack, s_attack_crit, s_attack_miss, s_button, s_button_ingame, s_hit, s_login, s_outMap_0, s_outMap_1, s_pet_level_up`) — ✅ đã wire đủ: login/button, button in-game, normal attack, hit, miss, critical (`TurnEffect.SKILL_CRIT`/`skillId=2`), hai lối out-map và pet level up. Cần xác nhận lại volume/timing bằng live play.
- Nhạc nền: KHÔNG có `.mid`/`.mp3` trong jar → jar bản gốc không có BGM, chỉ effect

## Trước khi phát hành — BẮT BUỘC

Client Unity **dễ mổ hơn** client J2ME đã obfuscate. Các biện pháp chống gian lận nền tảng đã được bật lại; hạng mục còn lại là trải nghiệm báo lỗi/rate-limit ở client:

1. ✅ **Validate vị trí server-side** — `GameController.cs` kiểm tra tần suất, độ dài path và độ dịch chuyển đích; vi phạm lặp lại sẽ bị ban tạm thời.
2. ✅ **Bật lại ban speed-hack** — `Player.onClientSpeedRespose` ban tạm thời một giờ và đóng kết nối; admin được miễn để hỗ trợ vận hành.
3. ✅ **Kiểm tra quyền admin lớp cuối** — các handler quản trị kiểm tra quyền trước khi thực thi.
4. ✅ **Vá lỗi kinh tế kiosk** — self-buy, TOCTOU tiền tệ và kế toán mua lẻ đã được xử lý như mục 2.
5. ✅ **Client-side rate limiting** — `ActionThrottle` (`UiLogic/ActionThrottle.cs`) chặn `PLAYER_CHALLENGE`/`PLAYER_PK` (khoá `target:{action}`, 1500ms, `GameSession.cs:452-458`) và mọi building/shop (khoá `building:{buildingType}`, 500ms, `GameSession.Interactions.cs:32` — bao gồm `REQUEST_SHOP` qua `BuildingDispatcher`), có toast báo lý do khi bị chặn.

> Các ngưỡng chống gian lận ở server cần theo dõi và tinh chỉnh bằng telemetry sau phát hành, để phát hiện false positive.

## Hiệu năng server

Nếu số người chơi tăng, xem mục 5 của report phân tích:
- `Mutex` (kernel object) thay vì `lock` trong `CopyOnWriteArrayList` — chậm 50-100 lần
- `GC.Collect()` mỗi lần disconnect (`Session.cs:142`)
- 2 thread OS mỗi kết nối
- `HashMap` (`Dictionary`) không thread-safe trong `PlayerData.items`

## Thứ tự thực thi đề xuất

Xếp theo mức độ "user thấy game bị hỏng" nếu thiếu:

1. **Mục 1 (dialog server)** — không có thì mọi lỗi câm. Ước 3-5 ngày, dễ, đơn thuần đăng ký handler + view.
2. **Mục 10 (HUD updates)** — boss HP + mob remove: user thấy quái chết mà không biến mất, boss không có thanh HP.
3. **Mục 5 (pet full)** — nhiều screen, nhưng đa số qua `GenericMenuView` — ước 1-2 tuần.
4. **Mục 3 (guild)** — dài, nhiều màn hình, nhưng user có thể chơi solo nếu chưa có.
5. **Mục 2 (kiosk)** — chợ giữa người chơi, kinh tế game phụ thuộc, nhưng cần dựng view chuyên biệt (không phải menu list phẳng).
6. **Mục 4 (PvP full)** — sau khi có guild + có người chơi thật.
7. **Mục 7 (task/achievement/top)** — retention/engagement, không chặn gameplay lõi.
8. **Mục 11 (place types khác)** — cần cho arena/challenge/clan; render chung code, khác data.
9. **Mục 8 (bảo mật)** — captcha + 2FA cần bật trước public launch.
10. **Mục 12 (event)** — quyết định phạm vi trước, có thể cắt hết cái đã hết hạn.
11. **Mục 6 (item merge/lock)**, **9 (emote)** — nice-to-have.
12. **Mục 13 (HTTP admin dashboard)** — tách plan riêng, không phải client.

## Next Steps

Plan chi tiết sau khi hết P7. Trước đó, cần quyết định:
- **Bỏ event nào đã hết hạn** (mục 12) — tiết kiệm ~600 dòng port + N menu
- **Có làm PvP tournament (Arena/League) ở bản đầu không** (mục 4) — cần population đủ lớn mới có nghĩa
- **Có làm guild ở bản đầu không** (mục 3) — cùng lý do
- **Migrate tài khoản jar → Unity**: `OP_MENU_MERGE_SERVER` (mục 6) — có làm không? Nếu không, user cũ mất char
- **Bật captcha/2FA từ ngày đầu hay sau** (mục 8)
- **Lịch phát hành** và chiến lược migrate người chơi từ client cũ
