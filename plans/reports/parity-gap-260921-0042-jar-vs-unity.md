# Parity gap JAR → Unity — 2026-09-21

**Phạm vi:** liệt kê tính năng client JAR gốc mà client Unity chưa migrate.
**Nguồn:** `client.jar_Decompiler.com/` (jar), `SRCGOPETGOC/GServer/` (sự thật về opcode/menu),
`GopetUnityClient/Assets/Scripts/` (Unity).
**Không sửa code.** Bổ sung cho `plans/260913-jar-unity-parity-audit-and-certification/plan.md`.

---

## 0. Ba kết luận khung (đọc trước khi đọc bảng)

1. **Không thiếu opcode nhận.** 187 hằng `GopetCmd` của Unity trùng 100% với `GopetCMD.cs` server
   (diff rỗng). Gate `check-protocol-coverage` báo 74 handled / 0 missing. Gap còn lại nằm ở
   **phía GỬI** (Unity không bao giờ phát một số sub-command) và ở **UI cho response**.

2. **JAR gần như hoàn toàn server-driven.** Chỉ ~15 màn hình là do client tự vẽ (`fb` login,
   `fr` world+hub, `fu` kho/trang bị/ngọc, `fn` ngân hàng+nạp, `fk` hộp thư, `m` soạn thư,
   `h` chat, `fj`/`ey` list+bang hội, `fv` xăm, `ds` tiến hoá pet, `ex` cường hoá, `fa` ký gửi,
   `et` điểm danh, `ca` ô kỹ năng, `ev` chọn khu). Mọi thứ còn lại (nhiệm vụ, đấu trường, shop,
   gym, NPC, hầu hết menu) là widget dựng từ gói tin vào `es`/`fj`/`gd`/`gi`/`k`. Unity đã có
   đủ hạ tầng generic đó (`GenericMenuView`, `ListOptionScreen`, `ChoiceDialogView`,
   `InputDialogView`, `ImageDialogView`, `YesNoDialog`, `AnimationMenuView`) — nên phần lớn
   MENU_* của server **tự động có màn hình**.

3. **Giả thiết "BuildingDispatcher.LocalMenu có ~14 mục, Unity mới nối 4" là dương tính giả.**
   `UiLogic/BuildingDispatcher.cs:74-140` chỉ trả 4 giá trị `LocalMenu` (ChangeZone, TicketRoom,
   Mailbox, PetProfile); 10 giá trị enum còn lại (RealEstate*, Garden, Cafe, PetShoesKiosk,
   BeautySalon, HairSalon, Garage) **không có đường sinh ra** — mọi buildingType tương ứng đều
   trả `Noop`. Nhánh `default` trong `Runtime/World/GameSession.Interactions.cs:64-69` hiện đã
   được đổi thành `Debug.LogWarning` và không tới được. Xác nhận từ jar: `dv.java:33-57` chỉ xử
   lý 5 command (101, 900, 1103, 1204, 2020), `default:` rỗng; các command 2/3/4/5/10/502/602/
   603/606/607/1003-1009/2019/1202/1203 mà `eg.java:208-330` phát ra **không có case ở bất kỳ
   file .java nào** → nhãn chết của bản cũ, code đã bị strip chứ không phải bị tắt.

---

## 1. Gap thật — xếp theo mức chặn gameplay

### P0-1. Học / thay kỹ năng pet — KHÔNG CÓ ĐƯỜNG VÀO

| | |
|---|---|
| JAR | `fy.java:112` gắn nút "Học" (`gw.a(132)`) lên từng ô kỹ năng; `fy.java:77` hỏi xác nhận "Bạn có chắc muốn thay thế chiêu thức cũ?" (`gw.a(133)`); chọn xong gọi `dc.c(skillId)` = `dc.java:37-43` → `en(81).a(31)` = `PET_SERVICE / MAGIC_LEARN_SKILL`. Ô trống gửi `dc.c(-1)`. |
| Server | `GameController.cs:1034-1037` → `learnSkill()` `GameController.cs:1391-1399` → `MenuController.sendMenu(MENU_LEARN_NEW_SKILL=799)`; chọn dòng xử lý tại `MenuController.selectMenu.cs:331+` (trừ coin, chặn skill cần card, hỏi thay chiêu cũ). Flow server **hoàn chỉnh**. |
| Unity | `GopetCmd.MAGIC_LEARN_SKILL` **không được tham chiếu ở bất kỳ file nào ngoài `Net/GopetCmd.cs`**. `Runtime/UI/PetProfileView.cs:78-81` chỉ in danh sách kỹ năng ra text, không có nút. |
| Thiếu | Nút "Học/Thay" trên từng slot kỹ năng trong `PetProfileView` (hoặc `PetGymView`) + gửi `81/31 + int skillId`. Response đã có màn hình (menu 799 đi qua `GenericMenuView`). |

### P0-2. Danh sách bang hội khi người chơi CHƯA có bang — gói bị server nuốt

| | |
|---|---|
| Unity | `Runtime/World/GameSession.Guild.cs:29-31`: `if (ClanId > 0) RequestClanInfo() else RequestGuildList()`. `Net/Guild/GuildPackets.cs:9,28`: `RequestGuildList()` gửi `81/91/sub 1`. |
| Server | `GameController.cs:2541-2592` — `clan(sbyte subCMD, …)` **không có case cho sub 1** và **không có `default:`** → gói rơi im lặng. Server chỉ phát `GUILD_LIST` từ `showListClan()` (`GameController.cs:4573-4589`), được gọi làm **fallback bên trong `clanInfo()` (sub 14)** khi `getClan() == null` (`GameController.cs:2618-2620`), và từ `donateClan`/`kick`/`search` (`:2643, :2675, :2707, :4652`). |
| Hệ quả | Người chơi chưa có bang mở `GuildView` → không nhận gói nào → tab "danh sách bang" trống vĩnh viễn. Chỉ ô "tìm bang" (`SearchGuild` sub 13) còn dùng được. |
| Thiếu | Luôn gửi `RequestClanInfo()` (sub 14) khi mở view; server tự fallback sang list. Hoặc bỏ hẳn `SubGuildList`. |

### P0-3. Góp quỹ bang — ngõ cụt, chọn xong không gửi được

| | |
|---|---|
| JAR | Có luồng đầy đủ: `gw.a(128)` "Nộp quỹ bang", `gw.a(141)` "Bạn muốn đóng góp "; `ey.java` gửi sub 9 rồi sub 10. |
| Server | sub 9 `donateClan()` bơm danh sách mức đóng góp; sub 10 `donateClan(int menuId)` mới thực hiện (`GameController.cs:2551-2562`). |
| Unity | `GameSession.Guild.cs:26` gửi được sub 9. Response vào `Runtime/UI/GuildView.TopSkills.cs:184-196` `ShowDonateOptions()` — **chỉ nối các `Description` thành một khối text**, không dựng dòng bấm được. `GuildPackets.PlayerDonateClan` (`Net/Guild/GuildPackets.cs:41`) **không có call-site nào**. |
| Thiếu | Render `GuildDonateOption[]` thành danh sách bấm được → gửi `PlayerDonateClan(option.Id)`. |

### P0-4. Mở khoá ô kỹ năng bang

`Net/Guild/GuildPackets.cs:58` `UnlockSkillSlot()` (sub 25) **không có call-site**. Server có
`unlockSlotSkillClan()` (`GameController.cs:2583-2585`). JAR có màn hình riêng `ca.java` với ba
nút Mở / Thuê / Đổi (`gw.a(48..51)`). Unity `GuildView.TopSkills.cs` chỉ nối **Thuê**
(`SkillRentRequested` → `RentSkill`, `GameSession.Guild.cs:27`). Thiếu nút "Mở" và "Đổi".

---

## 2. Gap trung bình

### M-1. Top điểm phát triển bang
`GuildPackets.TopGrowth()` (`Net/Guild/GuildPackets.cs:49`, sub 15) không có call-site. JAR có
(`dc.java:119-127` → `81/91/15`, hiển thị ở `ey.java:89`). Unity chỉ có Top quỹ
(`TopFundRequested` → sub 16). Server xử lý cả hai (`GameController.cs:2564-2570`, cùng gọi
`showTopFund()`).

### M-2. Xem kỹ năng bang của người chơi khác
`GuildPackets.ShowClanSkill(userId)` (`Net/Guild/GuildPackets.cs:63`, sub 27) không có call-site.
JAR nối vào popup nhắm người chơi (`fr.java:338-339` → `dc.h`). Server: `GameController.cs:2580-2582`.
`Runtime/UI/TargetPlayerMenu.cs:54-57` hiện chỉ có Thách đấu / PK / Xem đồ pet / Kết bạn.

### M-3. Thách đấu có cược tự nhập
JAR gửi `en(81).a(12)` = `PET_SERVICE/INVITE_MATCH` (`fr.java:224-229`) → server mở
**input dialog nhập giá cược** (`GameController.cs:5040-5052`, `INPUT_DIALOG_CHALLENGE_INVITE`).
Unity gửi **top-level `PLAYER_CHALLENGE` (12)** (`Net/Social/TargetPlayerPackets.cs:13-15`) →
server mở **menu giá cược cố định** `MENU_INTIVE_CHALLENGE=1032` (`GameController.cs:3665-3691`).
Cả hai đều chạy được; khác hành vi: mất khả năng tự nhập mức cược. `GopetCmd.INVITE_MATCH` không
có call-site.

### M-4. Xem hình xăm của pet đang bán trong ký gửi
`GopetCmd.SHOW_TATTO_PET_IN_KIOSK` (81/99) không có call-site. Server:
`GameController.cs:1222-1237` (tra pet trong `MarketPlace.getKiosk(KIOSK_PET)` rồi `showPetTattoUI`).
JAR: list type `81028` có mục "Xem hình xăm" (`fj.java:296-298`, khớp `MENU_KIOSK_PET=81028`).
Unity **không có xử lý đặc biệt cho listId 81028** (chỉ có 81004 tại
`Runtime/UI/CharacterHubPopupView.cs:118` và 81040 tại `Runtime/UI/UiRoot.Wings.cs:8`).
→ Mua pet trong kiosk không xem được xăm.

### M-5. Hộp thư không phân loại
JAR `fi.java` chia thư thành tab **Admin / Sự kiện (`gw.a(69)`) / Bạn bè**. Unity parse
`Letter.Type` (`Net/Social/Letter.cs:7`) nhưng `Runtime/UI/MailboxView.cs` **không dùng field này**
(grep `Type` trong file → 0 hit) → một danh sách phẳng.

### M-6. `BannerShown` không có subscriber
`Net/Guider/GuiderHandler.cs:40,75` phát `BannerShown` cho `BANNER_MESSAGE(1)`; grep toàn repo
`BannerShown +=` → **0**. Đây là handler duy nhất parse rồi bỏ. Ảnh hưởng thấp vì
`Manager/PlayerManager.cs:183-212` đã gửi song song cả `BOSS_BANNER_MESSAGE` (có subscriber:
`Runtime/World/GameSession.cs:169` → `NotificationTicker`); nhưng `Player.showBanner()`
(`Server/Player.cs:729-735`) gửi đơn lẻ vẫn mất.

*(Đã rà toàn bộ event trong `Assets/Scripts/Net/**`: chỉ còn `ClanIdChanged` và `SpeedChecked`
là 0-subscriber, cả hai đúng theo thiết kế — `ClanId` được đọc trực tiếp qua property,
`SpeedChecked` là anti-cheat không cần UI. Không có `Debug.Log` nào trong `Net/`.)*

### M-7. `AnimationMenuView` — lệnh gửi ngược server
`Runtime/UI/UiRoot.AnimationMenu.cs:22-25`: `command.RepliesToServer` → toast
"Thao tác này chưa được server hỗ trợ". Đúng — server không có nhánh nhận
`PET_SERVICE/ANIMATION_MENU` từ client. Ghi nhận là waiver, không phải gap.

---

## 3. Không phải gap — có nhãn ở JAR nhưng là code chết

Xác minh hai chiều (jar `dv.java:33-57` không có case + server không có engine):

| Nhãn JAR | buildingType | command JAR | Kết luận |
|---|---|---|---|
| Nhà hẻm / Nhà mặt tiền / Biệt thự / Dinh thự / Nhà | 0-4 | 2,3,4,5,502 | chết |
| Thú cưng (cửa) / Pet shop (cửa) / Trò chơi trong nhà | 6, 25, 24 | `case` return sớm | chết |
| Vườn | 7 | 10 | chết |
| Phòng vé → Khu giải trí / Khu mua sắm | 8 | 1202/1203 | chết (Unity nối `TicketRoom` → `MapTeleport`, là extension) |
| Cà phê | 10 | 2019 | chết |
| Caro / Cờ tướng / Tiến lên / Phỏm | 13-16 | 602,603,606,607 | chết; 24 map không đặt type này; server không có engine |
| Nón / Giày / Mỹ viện / Tóc / Vật phẩm / Gara | 18-23 | 1003-1009 | chết (Unity type 18/22 chủ động mở shop thật = extension, không tính parity) |

Ngoài ra `cg.java:435-441` dựng menu {"Trò chơi trong nhà", "Thú cưng"} nhưng `cg` không có
`case 53`/`case 54` → cũng chết.

---

## 4. Không tồn tại ở JAR — đừng liệt vào "chưa migrate"

Quét toàn bộ hai bảng chuỗi (`gw.java` id 0-147, `a.java` id 0-671) + toàn cây `.java`,
đối chiếu server:

- **Chợ trời / giao dịch P2P (trade)** — không có chuỗi, không có opcode, không có màn hình.
  (Ký gửi `fa.java` là chợ qua NPC, Unity đã có `KioskHandler`/`KioskListingView`.)
- **Kết hôn, VIP, Chuyển sinh, Vượt ải, Thành tựu, Top toàn server** — 0 hit ở jar; server
  cũng không có (`grep -rl "VIP\|Kết hôn\|Vượt ải"` trong `GServer` chỉ trúng tên event mùa hè).
  Lưu ý: server **có** `MENU_ME_SHOW_ACHIEVEMENT=1056` / `MENU_PET_REINCARNATION=1085` /
  `OP_SHOW_ME_ACHIEVEMENT=71` — đây là tính năng server **mới hơn jar**, và đều đi qua NPC
  option → `ChoiceDialogView`/`GenericMenuView` của Unity nên tự động có UI.
- **Sự kiện** — ở jar chỉ là một tab phân loại thư, không phải màn hình riêng.
- **Nạp thẻ SMS** (`fn.java:122-155` + `cg.java:162-169` `platformRequest("sms:?body=")`) —
  thuần client J2ME, server không có opcode tương ứng (chỉ `CHARGE_MONEY_INFO(44)` = ATM, Unity
  đã có `Net/Bank/BankPackets.cs` + `Runtime/UI/AtmPopupView.cs`). Ngoài phạm vi.
- **Mini-game** — xem §3.

---

## 5. Đã có đủ, không cần làm gì (đã kiểm chứng trong đợt rà này)

Đường gửi + handler + UI đều có: login/đăng ký/đổi mật khẩu, tạo nhân vật, map/portal/warp/minimap,
chat khu vực–cộng đồng–bang, hộp thư (đọc/đánh dấu/xoá/soạn gửi), kết bạn, kho đồ (`81/30`),
kho ngọc (`81/74` + cường hoá/tiến hoá/gắn/tháo), cánh (`81/92` 4 sub), xăm (`81/90` 4 sub),
trang bị pet (`81/28,29,39,56`), tiềm năng (`81/19`), gym (`81/21`), profile pet (`81/11`),
cường hoá/tiến hoá pet (`81/67,68,69,70,71,72`), shop (`81/2` + `81/60`), ATM (`44`),
ký gửi owner-side (`81/85,87`), đấu trường (`81/58` → `MENU_ARENA_HUB=1091` server-driven 5 mục),
nhiệm vụ (`81/54`), PK (`81/96`), xem info/đồ người khác (`81/55`, `81/28`), tự đánh (`81/22`),
điểm danh tháng (`122/41,42`), tương tác pet (kiss/play/poke qua `ON_PLACE_CHAT`),
trận đấu theo lượt (đánh thường/kỹ năng/vật phẩm/đầu hàng/buff/stats/exp mỗi đòn).

---

## 6. Thứ tự đề xuất

1. P0-2 (1 dòng sửa, đang chặn toàn bộ người chơi chưa có bang).
2. P0-3, P0-4 (bang hội — dữ liệu đã về, chỉ thiếu widget bấm).
3. P0-1 (học kỹ năng — cần thêm UI mới, nhưng server sẵn sàng).
4. M-1 → M-5.
5. M-6 (một dòng nối `BannerShown` vào `NotificationTicker`).
