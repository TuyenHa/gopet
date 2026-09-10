---
phase: 8
title: "Long Tail Features (Outline)"
status: in-progress
priority: P3
effort: "8-12w"
dependencies: [7]
---

# Phase 8: Long Tail Features (Outline)

## Implementation progress (2026-09-10)

| Workstream | Status | Evidence |
|---|---|---|
| Server dialogs | Complete | Popup/banner, red/green modal, image captcha and captcha follow-up input are wired through the real router/UI stack. |
| HUD/world critical updates | Partial | `UPDATE_HP_BOSS`, `TIME_PLACE`, `SHOW_BIG_TEXT_EFF`, `FAST_REMOVE_MOB`, pet evolution and the gem inventory/enchant/up-tier flows are implemented. |
| Client request throttling | Complete baseline | Challenge, PK and building/shop entry requests have cooldown feedback. |
| Server hardening | Partial | Kiosk self-buy/duplicate-buy/atomic coin debit, final admin authorization checks, and invalid pet-evolution ID guards fixed. Movement validation and speed-ban remain intentionally disabled pending live tuning. |
| Remaining long-tail domains | Pending | Kiosk specialist view, full clan management, full PvP, remaining pet systems, scalable top lists, special places and event scope. |

Verification: 622 .NET unit tests pass; Net/Runtime/PlayMode compile gates pass with 0 errors; asmdef references pass. Unity batch PlayMode could not start because the local Unity Licensing/Package Manager IPC service failed before tests were created.

Progress snapshot: **37 complete, 17 partial, 26 not started — 43 items still incomplete.**

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
| `MENU_KIOSK_HAT/WEAPON/AMOUR/GEM/PET/OHTER` (806-811) | `Kiosk.cs`, `MarketPlace.cs` | ⬜ |
| `MENU_KIOSK_*_SELECT` (1006-1011) — xem sạp cụ thể | `MenuController.selectMenu.cs:1329-...` | ⬜ |
| `MENU_OPTION_KIOSK` (1088), `MENU_OPTION_KIOSK_CANCEL_ITEM` (1089), `MENU_OPTION_BUY_KIOSK_ITEM` (1090) | `sendMenu.cs:1002,1013` | ⬜ |
| `SHOW_TATTO_PET_IN_KIOSK` (99) | `GopetCMD.cs:164` | ⬜ |
| `SELECT_KIOSK_ITEM` (85), `REMOVE_SELL_ITEM` (87) | `GopetCMD.cs:115-117` | 🟡 chỉ hằng số |

**Lỗ hổng kinh tế đã báo (mục 4 report `analysis-260905-1330`)**:
- Thiếu `return` sau `Kiosk.cs:175` (mua lẻ đồ của chính mình) — cho phép rửa tiền / trốn thuế
- TOCTOU trên tiền tệ (`checkCoin` ngoài mutex, `addCoin` trong)
- Kế toán sai luồng mua lẻ (`sumVal`)

**Kiosk pet cần `MENU_KIOSK_PET_SELECT` (1010) + `MENU_CHOOSE_PET_FROM_PACKAGE_PET` (1045)** — jar hiển thị *khung pet* (frame + level + stat + skill), không phải icon phẳng như item. Yêu cầu view riêng khác `GenericMenuView`.

## 3. Guild / Clan (bang hội)

Toàn bộ khung xương server (sub-command của `PET_SERVICE`/`CLAN` = 81/91):

| Sub | Tên | Unity | Ghi chú |
|---|---|---|---|
| 1 | `GUILD_LIST` | 🟡 hằng số | Danh sách bang tìm gia nhập |
| 2 | `GUILD_JOIN` / `GUILD_REQUEST_JOIN` | 🟡 | Xin gia nhập |
| 3 | `GUILD_LIST_MEMBER` / `CLAN_INFO_MEMBER` | 🟡 | Danh sách thành viên (có view profile) |
| 6 | `GUILD_KICK_MEMBER` | 🟡 | Trưởng bang đuổi thành viên |
| 9 | `DONATE_CLAN` / `PLAYER_DONATE_CLAN` (10) | 🟡 | Đóng góp xu/vàng cho bang |
| 13 | `SEARCH_GUILD` | 🟡 | Tìm bang theo tên |
| 14 | `CLAN_INFO` | 🟡 | Xem info bang |
| 15/16 | `GUILD_TOP_GROWTH_POINT/TOP_FUND` | 🟡 | Top bang |
| 20/21/22 | `GUILD_CHAT`/`PLAYER_CHAT`/`ON_PLAYER_CHAT` | 🟡 | Chat trong bang (**riêng, không phải chat khu vực**) |
| 23 | `GUILD_NAME_IN_PLACE` | 🟡 | Tag bang hiện trên đầu người chơi cùng map |
| 24-27 | `GUILD_CLAN_SKILL`/`UNLOCK_SKILL`/`RENT_SKILL`/`SHOW_OHTER_...` | 🟡 | Kỹ năng bang (mua/thuê/hiện) — có bảng lvl riêng |

Server-side có `ClanPlace`, `ClanHouseTemplate`, `ClanSkillTemplate`, `ClanSkillLvlTemplate`, `ClanPotentialSkill`, `ClanMember`, `ClanRequestJoin`, `ClanMemberDonateInfo` (12 file `Data/Clan/`). **View quản lý bang (approve/upgrade nhà chính/nhà kỹ năng)** cần dựng riêng, jar dùng nhiều menu lồng nhau (`MENU_APPROVAL_CLAN_MEMBER` 1024, `MENU_UPGRADE_MEMBER_DUTY` 1028, `MENU_SELECT_SKILL_CLAN_TO_RENT` 1030, `MENU_PLUS_SKILL_CLAN` 1031, `MENU_SELECT_TYPE_MONEY_TO_RENT_SKILL_CLAN` 1057).

## 4. PvP full (PK, Arena, Pet League)

Đã có `PLAYER_CHALLENGE` (12) + `PLAYER_BATTLE` (59) trong P7 LiveSmoke. Còn thiếu:

| Feature | Opcode / menu | Unity |
|---|---|---|
| PK 1-hit (dùng thẻ PK) | `PLAYER_PK` (96, sub của PET_SERVICE), `MENU_SELECT_ITEM_PK` (1012) | 🟡 chỉ packet |
| Arena vé báo danh + top | `MENU_SELECT_TYPE_PAYMENT_TO_ARENA_JOURNALISM` (1052), `ArenaEvent.cs` | ⬜ |
| Pet League (chọn pet phòng thủ, top) | `MENU_SELECT_PET_TO_DEF_LEAGUE` (1051), `OP_SELECT_PET_DEF_LEAGUE` (65), `OP_PET_LEAGUE_BETA` (64), `OP_TOP_AREAN_POINT` (66) | ⬜ |
| AntiPK (buff từ cánh) | `Player.cs:982 UpdateAntiPK()` | ⬜ |
| Shop đấu trường thường + sinh tử | `OP_SHOP_ARENA` (11), `ShopArena.cs`, `ShopArenaTemplate.cs` | ⬜ |
| Vượt ải (dùng `ChallengePlace`) | NPC `-28` option 10, `ChallengePlace.cs` (223 dòng) | ⬜ |
| Top challenge | `OP_SHOW_TOP_CHALLENGE` (80), `TopChallenge.cs` | ⬜ |

## 5. Pet đầy đủ — mảng chưa cover

Phase 5.1 (Linh Thú city) đã làm `PetEquipView` 5-slot + `EnchantEvolveView`. Còn:

| Feature | Menu / opcode | Unity |
|---|---|---|
| Tattoo pet (5 màn hình) | `MENU_SHOW_ALL_TATTO` (1054), `TATTOO` (90) + 6 sub, `SELECT_ITEM_GEN_TATTO` (1014), `SELECT_ITEM_REMOVE_TATTO` (1015), `SELECT_MATERIAL1/2_TO_ENCHANT_TATOO` (1046/1047) | 🟡 chỉ hằng số |
| Cánh (Wing) + cường hoá | `WING` (92) + 6 sub, `EnchantWingData.cs`, `MENU_WING_INVENTORY` (81040), `MENU_MERGE_WING` (1053), `SELECT_MATERIAL_TO_ENCAHNT_WING` (1043), `SELECT_MONEY_TO_PAY_FOR_ENCHANT_WING` (1044) | ✅ kho cánh có action dùng/gỡ/cường hóa; payment/material/confirm qua menu generic; `WING/3` đồng bộ animation trên mọi avatar |
| Skin nhân vật | `SKIN_INVENTORY` (62), `MENU_SKIN_INVENTORY` (803), `MENU_UNEQUIP_SKIN` (1038), `SEND_SKIN` (61), `REQUEST_SHOP_SKIN` (60) | ✅ shop/kho/dùng/gỡ qua menu generic; `SEND_SKIN` đồng bộ renderer, hướng và bước chân cho mọi avatar trong map |
| Pet reincarnation (trùng sinh) | `MENU_PET_REINCARNATION` (1085), `OP_REINCARNATION` (9), `PetReincarnation.cs` | ⬜ |
| Pet fusion (dung hợp) | `MENU_FUSION_MENU_EQUIP/PET/EQUIP_OPTION/…` (1078-1083), `OP_DUNG_HỢP` (86), `MENU_ADMIN_BUFF_DUNG_HỢP` (1087) | ⬜ |
| Pet sacrifice (hiến tặng) | `MENU_PET_SACRIFICE` (1084), `OP_HIẾN_TẶNG_THÚ_CƯNG` (89) | ⬜ |
| Merge part item/pet (ghép mảnh) | `OP_MERGE_PART_PET` (15), `OP_MERGE_ITEM` (19), `MENU_MERGE_PART_PET` (1004), `MENU_SELECT_ITEM_PART_FOR_STAR_PET` (1013), `SELECT_ITEM_MERGE` (1068-1070) | ⬜ |
| Star pet (nâng sao) | `OP_UPGRADE_STAR_PET` (8), `MENU_SELECT_ITEM_PART_FOR_STAR_PET` (1013) | ⬜ |
| Học skill mới cho pet | `MENU_LEARN_NEW_SKILL` (799), `PetSkill*.cs` (3 file) | ⬜ |
| Xoá điểm tiềm năng (reset gym) | `MENU_DELETE_TIEM_NANG` (800), `OP_DELETE_TIEM_NANG` (24) | 🟡 |
| Support/heal khác | `MENU_SELECT_ITEM_SUPPORT_PET` (1016), `MENU_SELECT_SLOT_USE_SKILL_CARD` (1075) | ⬜ |

## 6. Item / kho đồ / merge server cũ

| Feature | Menu / opcode | Unity |
|---|---|---|
| Kho đồ tổng | `MENU_NORMAL_INVENTORY` (81004) | ✅ lối vào “Rương đồ” từ menu nhân vật; danh sách cuộn server-driven và chọn trang bị hoạt động |
| Kho tiền tệ (MoneyDisplay) | `MENU_ITEM_MONEY_INVENTORY` (1027), `MENU_MONEY_DISPLAY_SETTING` (1066) | 🟡 currency HUD có, không có kho |
| Bán rác | `MENU_SELL_TRASH_ITEM` (1067), `OP_SELL_TRASH_ITEM` (79) | ✅ danh sách vật phẩm + confirm + input số lượng + success/error dialog dùng pipeline generic |
| Khoá / mở khoá item (chống mất trộm) | `MENU_LOCK_ITEM_PLAYER` (1073), `MENU_UNLOCK_ITEM_PLAYER` (1074) | ⬜ |
| Buff exp / vật phẩm huỷ đồ | `BuffExp.cs`, `SHOW_EXP` (95) khi có buff | ✅ buff có icon/countdown HUD; huỷ trang bị pet có confirm hai lớp và cập nhật inventory từ server |
| Xem hidden stat | `HiddenStatItemTemplate.cs`, `ItemBattleOptionBuff.cs` | ⬜ |
| Merge server cũ (data migration người chơi) | `OP_MENU_MERGE_SERVER` (81), `INPUT_TYPE_NAME_PLAYER_TO_ENBALE_MERGE_SERVER` (26), `MENU_ADMIN_MAP` (1019), `MENU_SHOW_ALL_ITEM` (1041) | ⬜ *cần cho migrate người chơi từ jar sang Unity* |

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
| **AntiPK từ cánh** | `Player.cs:982` | ⬜ |
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
| `REMOVE_MOB` (42) | Xoá quái sau khi bị đánh chết | ⬜ |
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
| `GL_MAIL` (20) — server-mail (banner + reward) | `LETTER_COMMAND` sub | 🟡 |

## 11. Place types phi-mặc-định

| Place | Nguồn | Unity |
|---|---|---|
| `ArenaPlace` | 26+ dòng, riêng cho arena/PVP tournament | ⬜ |
| `ChallengePlace` | 223 dòng, boss raid có timeout, ban check | ⬜ |
| `ClanPlace` | Nhà bang hội, chat riêng, không được warp qua `ON_PLAYER_WARPING` mà qua `changeChannel` | ⬜ |
| `MarketPlace` | Chợ (kiosk) — spawn theo kiosk data JSON, không phải mob/npc | ⬜ |

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
- Âm thanh: **10 file `.wav`** trong jar (`s_attack, s_attack_crit, s_attack_miss, s_button, s_button_ingame, s_hit, s_login, s_outMap_0, s_outMap_1, s_pet_level_up`) — `SoundBank`/`SoundManager` có sẵn, cần rà lại xem đã trigger đủ chỗ chưa (button in-game, hit, crit, miss, out-map, pet level up)
- Nhạc nền: KHÔNG có `.mid`/`.mp3` trong jar → jar bản gốc không có BGM, chỉ effect

## Trước khi phát hành — BẮT BUỘC

Client Unity **dễ mổ hơn** client J2ME đã obfuscate. Các biện pháp chống gian lận hiện đang tắt phải bật lại:

1. **Validate vị trí server-side** — `GameController.cs:227-241` đang bị comment. Client tự quyết vị trí hoàn toàn.
2. **Bật lại ban speed-hack** — `Player.cs:320` chỉ ghi log, lệnh ban bị comment.
3. **Kiểm tra quyền admin lớp cuối** — `MenuController.selectMenu.cs:2261,2322` không check `isAdmin` (hiện chưa khai thác được nhưng thiếu defense-in-depth).
4. **Vá lỗi kinh tế kiosk** — xem `plans/reports/analysis-260905-1330-gopet-server-source.md` mục 4:
   - Thiếu `return` ở `Kiosk.cs:175` (mua lẻ đồ của chính mình)
   - TOCTOU trên tiền tệ (`checkCoin` ngoài mutex, `addCoin` trong mutex)
   - Kế toán sai luồng mua lẻ (`sumVal`)
5. **Client-side rate limiting** cho các opcode `PLAYER_CHALLENGE`/`PLAYER_PK`/`REQUEST_SHOP` để tránh spam server, và **hiển thị error dialog** khi bị chặn (cần Popup/redDialog trước — xem mục 1).

> Các mục 1-4 **vượt ra ngoài** phạm vi "chỉ vá bảo mật, giữ format" của P1 vì chúng đổi hành vi game. Cần quyết định riêng trước khi phát hành.

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
