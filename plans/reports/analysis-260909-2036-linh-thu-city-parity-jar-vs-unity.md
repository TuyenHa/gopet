# Đối chiếu map "Thành Phố Linh Thú" — jar J2ME vs Unity client

**Nguồn:** `client.jar_Decompiler.com/` (jar J2ME đã decompile) + `GopetUnityClient/Assets/Scripts/` + `SRCGOPETGOC/GServer/` (để xác thực template).
**Ngày:** 2026-09-09
**Map:** `mapId = 11`, `name = "Thành Phố Linh Thú"` (`server_db.sql:2097`)
**Ghi chú:** trong yêu cầu người dùng viết "chức năng nfc" — hiểu là **NPC** (không tồn tại NFC/Near-Field trong project).

---

## 1. Bối cảnh nhanh

- Map 11 là **hub khởi đầu**: server bơm player vào đây ngay sau `loginOK()` (`Player.cs:522`). Unity đã bake `DefaultMapId = 11` (`GameSession.cs:31`).
- Server template khai báo **5 NPC cố định** (`map.npc = [-1,-7,-15,-24,-25]`); các công trình/shop nhỏ do `eg.java` render theo `type byte` mà server gửi qua `GAME_OBJECT`.
- Client jar **không hard-code tên map/tên NPC** — mọi entity đến từ opcode; client vẽ dựa vào cấu trúc byte.
- Unity đã có **tầng giao thức** (Net/Map, Net/Guider, Net/Battle, Net/Chat) và **thư viện UI generic** (ChoiceDialog, InputDialog, GenericMenu, ShopPopup). Cái thiếu chủ yếu là **UI bấm được** để kích những opcode client-driven, và các **luồng cho từng NPC**.

---

## 2. Bản đồ tổng thể — jar có gì trên map 11

### 2.1 NPC (server template)

| NPC ID | Tên | Toạ độ | Ảnh | Menu (option → label) |
|---|---|---|---|---|
| **-1** | **TRAN CHAN** (Trần Trấn) | (374, 117) | `tran_tran.png` | 1: Nhận pet miễn phí · 2: Shop Pet · 3: Top Pet · 4: Top Đại Gia · 41: Top Phú Hộ · 60: Nhập mã quà tặng · 81: Gộp đồ server cũ |
| **-7** | **BAC SI XI TIN** (Bác sĩ xì tin) | (470, 344) | `bac_si_xi_tin.png` | 22: Hồi sinh pet sau PK · 23: Nhận nhiệm vụ hằng ngày · 24: Tẩy gym |
| **-15** | **SU GIA BANG HOI** (Sứ giả bang hội) | (326, 236) | `Su gia bang hoi.png` | 44: Vào khu vực bang · 45: Top lvl bang hội · 46: Tạo bang hội · 47: Sự kiện bang hội · 48: Cống hiến bang hội |
| **-24** | **ÔNG GIÀ NOEL** | (226, 396) | `giaNoel.png` | 87: Điểm danh |
| **-25** | **SU GIA THIEN THAN** (Sứ giả thiên thần) | (322, 332) | `su_gia_thien_than.png` | 88: Hướng dẫn lên thiên đình · 89: Hiến tặng thú cưng |

Nguồn: `server_db.sql:2183-2207`, `GServer/Data/Npc/*Menu.cs`.

### 2.2 Công trình / shop-building trên map (`eg.java:55-155`)

Đây là các entity server có thể đặt trên map (được nhận dạng bằng `type byte` trong `GAME_OBJECT`). Bấm vào là mở luồng tương ứng.

| type | Nhãn client | Đích client-side (cd) | Ghi chú |
|---|---|---|---|
| 0-4 | Nhà hẻm / mặt tiền / Biệt thự / Dinh thự / Nhà | cd 2/3/4/5/502 | Bất động sản |
| 6 | Thú cưng | menu pet | |
| 7 | Vườn | cd 10 | Trồng cây |
| 8 | Phòng vé | menu 574 | Vé sự kiện |
| 10 | Cà phê | cd 2019 | Tụ tập |
| 11 | Khu | cd 1103 | Chuyển khu |
| 12 | Hộp thư | cd 2020 | Thư |
| 13-16 | Caro / Cờ tướng / Tiến lên / Phỏm | cd 602/603/606/607 | Mini-game |
| 17 | Thời trang | opcode 60 | Đồ char (thời trang) |
| 18 | Nón | cd 1003 | Mũ pet |
| 19 | Giày | cd 1004 | Giày pet |
| 20 | Mỹ viện | cd 1005 | Beauty |
| 21 | Tóc | cd 1006 | Tóc char |
| 22 | Vật phẩm | cd 1009 | Potion/thức ăn battle |
| 23 | Gara | cd 1008 | Xe |
| 24 | Trò chơi trong nhà | | |
| 25 | Pet shop | | |
| 26 | Đấu trường | opcode 58 | PVP/PVE |
| 27-30 | (không nhãn client) | `dc.d(1..4)` | Cờ mini-game khác |
| 31 | (không nhãn) | opcode 21 | |
| 32 | (không nhãn) | opcode 45 (recovery) | Reset pet HP |

> Server có thể bind bất kỳ shop nào trên "Thành Phố Linh Thú" — muốn liệt kê chính xác cái nào đang đặt phải đọc `maps/11.dat` (binary) hoặc dump `GAME_OBJECT` runtime; đây là ranh giới giữa client-agnostic (jar biết kiểu) và server-driven (server chọn đặt cái nào).

### 2.3 Cổng dịch chuyển (portal / waypoint)

Server bơm portal cùng với `MapUpdate` — client jar (và Unity) render bằng `MapPortalView` (Unity) / `eg` với `type = -1` (jar). Danh sách cụ thể ở `maps/11.dat`.

### 2.4 Menu nhân vật chính (`fr.java:127-140`)

Nút "Menu" trong HUD → 12 lựa chọn:

| cd | Nhãn | Đích |
|---|---|---|
| 340 | Thêm bạn | submenu (Danh sách / Yêu cầu / Sổ đen / Thêm) |
| 341 | Hộp thư | submenu (Hộp đến / Gửi) |
| 335 | Chat cộng đồng | opcode 92 sub 2 |
| 336 | Chat bang hội | opcode 91 sub 20 |
| 331 | Chat khu vực | Mở dialog gõ text (`new h().d()`) |
| 3286 | Chat (khác) | `new h().d(); h.a = 1` |
| 330 | Tủ quần áo | opcode 62 |
| 317 | Chọn pet | opcode 5 |
| 327 | Đổi mật khẩu | 3-field form → `cx.c()` |
| 0 | Đăng xuất | Reset về màn login |
| 3281 | Cài đặt | Nhạc nền/hiệu ứng/restart/ẩn UI |
| 10000 | Thoát | Đóng app |

### 2.5 Tương tác pet (HUD pet — `fr.java:150-183, 295-298`)

| Nút (jar) | Chuỗi | Opcode |
|---|---|---|
| Chơi với pet | `gw.a(94)` | `dc.a(1)` — 81/17/1 |
| Hôn pet | `gw.a(95)` | `dc.a(0)` — 81/17/0 |
| Xoa đầu pet | `gw.a(96)` | `dc.a(2)` — 81/17/2 |
| **Hồi phục** (cd 323) | `gw.a(115)` | `dc.a(true)` — 81/45/1 |
| Cho pet đi | (`dj.a=1`) | `dc.b(petId)` — 81/11 |
| Đổi pet | cd 322 | 81/37/4 + petId |
| Thay đổi trạng thái pet (đi/theo/ngừng) | cd 318/319/321 | 81/37/1..3 |
| Chọn pet ưa thích | cd 320 | UI mở list pet |
| Thách đấu | cd 313 | 81/12 (challenge) |

### 2.6 HUD số liệu (`fr.java:566-608`)

Client jar vẽ HUD với:
- Icon **năng lượng** (`(nluong)`) + số
- Icon **cấp / EXP** — level hiện tại
- Vòng tròn xoay khi có event/timer (`this.g`)
- Đồng hồ đếm ngược sự kiện
- 4 tiền tệ ẩn/hiện tuỳ trạng thái (`dj.b`, `dj.c` với icon 4 loại: mGold, Đậu, Thóc, Ngọc)

### 2.7 Thao tác dài hạn khác

- **Ngân hàng (ATM)** — mở qua menu chính hoặc NPC-building `type=12` (Hộp thư dùng chung); UI ở `fn.java` (đã có báo cáo cũ chi tiết).
- **Cường hoá / Tiến hoá pet** — `r.java` opcodes 46/76/49/79.
- **Trang bị pet** (5 slot: mũ/giáp/vũ khí/giày/bao tay) — `fu.java`.
- **Ngôn ngữ** — VN/EN toggle (`gw.a(143..145)`).
- **Nhạc nền/hiệu ứng** — bật/tắt (`ISoundManagerSDK`).

---

## 3. Unity hiện có gì (đối chiếu 1-1)

### 3.1 Tầng giao thức đã cover

| Feature | Hạng mục | Đã có ở Unity? | File |
|---|---|---|---|
| Vào map / spawn | `INIT_PLAYER`, `MAP_UPDATE`, `ENTER_MAP`, `EXIT_PLACE` | ✅ | `Net/Map/MapHandler.cs` |
| Di chuyển | `ON_OTHER_USER_MOVE` (recv + send) | ✅ | `Net/Map/MapHandler.cs:93` |
| Warp | `ON_PLAYER_WARPING` | ✅ | `Net/Map/MapHandler.cs:109` |
| NPC/Mob spawn | `GAME_OBJECT`, `SEND_LIST_MOB_ZONE` | ✅ | `Net/Map/WorldObjectHandler.cs` |
| Tương tác pet (recv) | `ON_PET_INTERACT` | ✅ | `Net/Map/WorldObjectHandler.cs:71` |
| Talk NPC / chọn option | `NPC_GUIDER`, `SELECT_OPTION` | ✅ | `Net/Guider/GuiderPackets.cs:50,73` |
| Menu server | `SHOW_MENU_ITEM` (family 122) | ✅ | `Net/Guider/GuiderHandler.cs` + `MenuScreen.cs` |
| Dialog Có/Không | `SERVER_MESSAGE 45` | ✅ | `Net/Guider/GuiderDialogs.cs` |
| Input dialog | `TYPE_DIALOG_INPUT` | ✅ | `Net/Guider/GuiderPackets.cs:85` |
| Shop request | `REQUEST_SHOP` | ✅ | `Net/Guider/GuiderPackets.cs:68` |
| Battle mob/player | `ATTACK_MOB`, `PLAYER_BATTLE`, `PET_BATTLE`, `PET_BATTLE_STATE` | ✅ | `Net/Battle/BattleHandler.cs` |
| Battle actions | attack / skill / item | ✅ | `Net/Battle/BattleHandler.cs:36-40` |
| Auto recovery | `PET_RECOVERY_HP` | ✅ | `Net/Battle/BattleHandler.cs:41` |
| Chat khu vực | `ON_PLACE_CHAT` (send + recv) | ✅ | `Net/Chat/ChatHandler.cs` |
| Update level pet | `UPDATE_PET_LVL` | ✅ | `Net/Battle/BattleHandler.cs:32` |

**Đánh giá:** tầng Net đã đủ để nói chuyện với server cho **hầu hết** tính năng map 11. Cái còn hụt sẽ ở **UI trigger** và các **opcode ngoài họ Guider**.

### 3.2 Tầng UI/Scene đã có

| Đối tượng | Có trong Unity? | File |
|---|---|---|
| Map render + tile | ✅ | `Runtime/World/MapRenderer.cs`, `MapScene.cs` |
| Portal object trên map | ✅ | `Runtime/World/MapPortalView.cs` |
| Animated map objects | ✅ | `Runtime/World/MapAnimatedObjectView.cs` |
| Player avatar + movement | ✅ | `Runtime/World/PlayerAvatar.cs`, `MovementController.cs` |
| Camera follow | ✅ | `Runtime/World/CameraFollower.cs` |
| Virtual joystick | ✅ | `Runtime/World/VirtualJoystick.cs` |
| Chat bubble | ✅ | `Runtime/World/ChatBubble.cs` |
| Chat input (place) | ✅ | `Runtime/World/GameHud.cs:42-56` |
| Character HUD (portrait + HP/MP/EXP) | ⚠️ Đã dựng khung, **giá trị placeholder** (`SetStats(100,100,...)`), **không nối server** | `Runtime/World/CharacterHud.cs:127` |
| Name label trên đầu nhân vật | ✅ | `Runtime/World/JarNameLabel.cs` |
| Battle scene + view | ✅ | `Runtime/World/BattleView.cs`, `BattleCoordinator.cs` |
| Choice dialog / Input dialog / Menu | ✅ | `Runtime/UI/ChoiceDialogView.cs`, `InputDialogView.cs`, `GenericMenuView.cs` |
| Shop popup | ✅ | `Runtime/UI/ShopPopupView.cs` |
| Toast | ✅ | `Runtime/UI/ToastView.cs` |

---

## 4. Điểm còn thiếu — ưu tiên theo độ visible

Phân 3 tầng: **P0** (chặn trải nghiệm cơ bản) · **P1** (thiếu UI cho protocol đã có) · **P2** (mảng chức năng riêng, cần cả UI + wiring).

### 4.1 P0 — Chặn trải nghiệm cơ bản

| # | Thiếu | Vì sao chặn | Cần làm |
|---|---|---|---|
| **1** | **HUD số liệu thật (level, currency, energy)** | HUD hiện là **placeholder cứng** (`SetStats(100,100,...)` ở `CharacterHud.cs:127`). Người chơi không biết mình còn bao nhiêu HP/MP/EXP/tiền. | Nối vào gói stats server (opcode dj/EXP + PlayerData). Thêm vùng **4 tiền tệ** (mGold/Đậu/Thóc/Ngọc) và **level number** vào panel |
| **2** | **Nút "Menu" mở menu nhân vật** | Không có nút chính → **không truy cập được** 12 mục ở §2.4. Đây là trục vào Bạn bè/Hộp thư/Tủ quần áo/Chọn pet/Cài đặt/Đăng xuất. | Nút Menu ở góc phải HUD; mở `GenericMenuView` với 12 dòng; mỗi dòng gửi đúng opcode như jar (đã liệt kê §2.4) |
| **3** | **Tap NPC không thấy tên/menu tự động** | `WorldObjectHandler.NpcsReceived` đã có; `GameSession.SubscribeWorld(..., guider.TalkToNpc, ...)` đã nối. **Cần verify** ảnh + tên trên map render đúng, và tap thật sự gửi `TalkToNpc`. | Kiểm bằng PlayModeTest / manual click vào từng NPC map 11; nếu chưa vẽ `NpcSpawn.ImagePath` thì bổ sung |
| **4** | **Nút Cảm xúc (Chơi/Hôn/Xoa)** | Client jar có 3 nút với `dc.a(0/1/2)`. Unity **chỉ receive** `ON_PET_INTERACT` chứ **chưa có UI gửi**. Người chơi không tương tác được pet của mình. | Thêm cụm 3 nút hoặc menu con "Cảm xúc" mở khi tap pet; gửi `PET_SERVICE 17 / [0|1|2]` |
| **5** | **Nút Hồi phục pet (`dc.a(true)`, cd 323)** | Sau battle pet gần chết — jar có nút heal ngay HUD (`fr.java:295-298`). Unity chưa có. | Thêm nút hồi phục vào HUD pet; gửi `PET_SERVICE 45 / 1` |

### 4.2 P1 — Protocol đã có, thiếu UI

| # | Thiếu | Ghi chú |
|---|---|---|
| **6** | **Danh sách bạn / Yêu cầu / Sổ đen / Thêm bạn** | Server gửi qua menu generic (`SHOW_MENU_ITEM`); UI generic của Unity **hiển thị được ngay** khi bấm — chỉ thiếu **nút gọi**. Client jar dùng `en(121)` (opcode 121, sub 1/2/13) — Unity chưa map opcode 121. |
| **7** | **Hộp thư đến / Hộp thư gửi** | Tương tự #6, opcode 121 sub 10; Unity generic menu render được. |
| **8** | **Tủ quần áo (đồ thời trang char)** | Opcode 62 (`fr.java:346`). Trả về menu — render được bằng `GenericMenuView`. |
| **9** | **Chọn pet đang theo** | Opcode 5 (cd 317). Menu chọn pet chính. |
| **10** | **Chat cộng đồng / Chat bang hội** | Opcode 92/1 và 91/20. Server trả về màn nhập text; input dialog Unity render được. |
| **11** | **Đổi mật khẩu tại chỗ** | Form 3-field xử lý client-side rồi gửi `cx.c()` (opcode auth). Cần dựng `FormView`. |
| **12** | **Cài đặt (nhạc nền/nhạc hiệu ứng/ẩn UI)** | Toàn client-side. Unity **đã có `SoundToggleButton.cs`** nhưng chưa gắn vào menu cài đặt. |
| **13** | **Restart connection** (cd 3284) | Reset về màn login. Client-side — `LoginFlow.Reset` là có sẵn. |
| **14** | **Ẩn/hiện UI (cd 3285)** | Client-side toggle. |
| **15** | **Icon Menu / hiển thị vòng xoay timer** | HUD phụ khi có sự kiện đang chạy — cần layout, chưa có. |
| **16** | **Portal-warp visual feedback** | `PortalSelected` đã gửi `SendWarp`, nhưng chưa có transition/fade màn hình khi warp. |

### 4.3 P2 — Mảng tính năng lớn (cần cả UI + wiring)

Từng NPC & building trên map 11:

| NPC / Building | Chức năng jar | Trạng thái Unity |
|---|---|---|
| **NPC -1 Trần Trấn** — Nhận pet miễn phí | Menu → chọn pet starter, server phát pet | ❌ chưa có UI kết quả (mặc dù menu generic render được) |
| — Shop Pet | Duyệt/mua pet | ❌ cần view riêng cho preview pet |
| — Top Pet / Top Đại Gia / Top Phú Hộ | 3 bảng xếp hạng | ❌ cần table/list view |
| — Nhập mã quà tặng | Input dialog + validate | ⚠️ InputDialog có; chưa có bước hậu-nhận |
| — Gộp đồ server cũ | Confirm dialog | ⚠️ ChoiceDialog có |
| **NPC -7 Bác sĩ xì tin** — Hồi sinh pet sau PK | Server flow | ⚠️ menu render được, chưa có UI xác nhận cost |
| — Nhận nhiệm vụ hằng ngày | Trả về danh sách + reward preview | ❌ chưa có "quest list view" |
| — Tẩy gym | Reset điểm training pet | ⚠️ generic menu render được |
| **NPC -15 Sứ giả bang hội** — Vào khu vực bang | Warp tới clan map | ❌ chưa có UI khu clan |
| — Top lvl bang hội | Ranking | ❌ |
| — Tạo bang hội | Input tên + phí | ⚠️ InputDialog có |
| — Sự kiện bang hội / Cống hiến | Menu | ⚠️ |
| **NPC -24 Ông già Noel** — Điểm danh | Daily check-in | ❌ cần calendar/reward view |
| **NPC -25 Sứ giả thiên thần** — Hướng dẫn lên thiên đình | Tour hướng dẫn | ❌ dialog dài, cần narrator flow |
| — Hiến tặng thú cưng | Chọn pet + xác nhận | ❌ danh sách pet + confirm |
| **Building type 12 Hộp thư (map)** | Mở mail như menu char (§4.2 #7) | ❌ |
| **Building 13-16 Caro/Cờ tướng/Tiến lên/Phỏm** | Mini-game bàn cờ | ❌ **CHƯA CÓ SCENE mini-game** — cần 4 sub-project (`dc.d(1..4)`) |
| **Building 17 Thời trang** | Shop char clothing | ❌ |
| **Building 18 Nón / 19 Giày / 21 Tóc** | Shop pet/char | ⚠️ ShopPopupView có; chưa map từng shop id |
| **Building 20 Mỹ viện** | Thay đổi ngoại hình char | ❌ |
| **Building 22 Vật phẩm** | Potion shop (opcode `REQUEST_SHOP 4`?) | ⚠️ `RequestShop` sẵn — cần trigger |
| **Building 23 Gara** | Shop xe/mount | ❌ |
| **Building 26 Đấu trường** | PVP/PVE — opcode 58 | ❌ |
| **Building 32 Recovery** | `dc.a(true)` — heal pet | ❌ |
| **NPC ATM/Ngân hàng** | UI 4-tab (Nạp/Rút/Đổi/Chuyển) — `fn.java` | ❌ cần view riêng, thao tác 4 tiền tệ |
| **NPC Vườn** | Trồng cây, thu hoạch | ❌ |
| **NPC Cà phê** | Chat/tụ tập | ❌ |
| **NPC Phòng vé** | Vé sự kiện | ❌ |
| **Trang bị pet** (5 slot) | Fu.java UI với gem socket, cường hoá, tiến hoá | ❌ **CHƯA CÓ** — báo cáo cũ `analysis-260908-2209-linh-thu-city-items.md` §3 chi tiết |

### 4.4 Chức năng nhân vật khác (không thuộc NPC)

Từ `fr.java` + code jar chung:

| Thiếu | Diễn giải |
|---|---|
| **Tap người chơi khác** → xem info + thách đấu / kết bạn / chat riêng | jar hiện menu (cd 311/312/313/325/326/333/337); Unity **chưa có "target player" UI** |
| **PK giữa người chơi** | `PLAYER_BATTLE` đã receive; chưa có UI mời PK ↔ chấp nhận |
| **Sit/emote animation** (ngồi/vẫy tay) | jar có (`ei.java` state); Unity `JarActorAnimation` hiện chỉ walk/idle/blink |
| **Show equipment của người khác** | tap → xem đồ pet của họ (opcode 96) |
| **Trade / gift item** | Chưa có |
| **Toggle ẩn/hiện tên người chơi** | Cd 3285 |
| **Report abuse / Block** | Menu con của "target player" |
| **Chuyển kênh (channel)** | Opcode 92 sub 3 (`gw.a(53) "Chuyển kênh"`); chưa có UI |
| **Ngôn ngữ VN/EN** | `gw.a(143)` "Chọn ngôn ngữ"; Unity đã có bảng chuỗi 2 lang nhưng chưa có toggle |

---

## 5. Bảng tổng hợp — cần làm gì để "chơi được map 11"

Chấm sao theo mức độ VISIBLE khi vừa vào map:

| Ưu tiên | Việc | Effort tương đối |
|---|---|---|
| ★★★ | HUD hiển thị số liệu thật (HP/MP/EXP/level/currency/energy) | S — chỉ cần nối opcode stats vào `CharacterHud.SetStats` + thêm 4 icon currency |
| ★★★ | Nút Menu góc phải HUD → mở 12 mục nhân vật | S — dựng `GenericMenuView` dòng cứng; mỗi dòng gọi đúng opcode |
| ★★★ | Kiểm và fix render 5 NPC + tap → mở menu server | S — verify PlayModeTest; nếu ảnh NPC chưa nạp thì thêm loader |
| ★★★ | Cụm 3 nút Cảm xúc pet + nút Hồi phục | S — sub-menu ở pet HUD |
| ★★ | Vẽ + kích hoạt 5 loại building sát thực tế map 11 (đọc `maps/11.dat` xem cái nào có mặt) | M |
| ★★ | UI Ngân hàng (ATM) 4 tab | M — flow ổn định, cần view |
| ★★ | UI trang bị pet 5 slot + gắn/tháo gem | L — báo cáo cũ đã specced |
| ★★ | UI kết bạn / hộp thư / tủ quần áo (opcode 121 & 62) | M — cần map opcode 121 vào MessageRouter |
| ★ | Mini-game Caro/Cờ tướng/Tiến lên/Phỏm | XL — 4 game riêng |
| ★ | PVP mời/nhận + arena | L |
| ★ | Bang hội full flow (khu vực bang, top, tạo, sự kiện, cống hiến) | XL |
| ★ | Điểm danh Noel, hướng dẫn thiên đình, hiến pet | M |
| ★ | Trade/gift, target-player menu, chuyển kênh, đổi ngôn ngữ | M |

---

## 6. Ràng buộc giao thức đã biết (cần nhớ khi dựng UI)

- **Opcode 121** (COMMAND_FRIEND/LETTER) — client jar dùng cho Bạn/Thư; Unity `MessageRouter` **chưa register**. Bắt buộc register trước khi dựng UI Bạn/Thư.
- **Opcode 62** (WEARING?) — jar cd 330 "Tủ quần áo"; server trả menu → Unity render qua Guider generic được **nếu server bọc trong `SHOW_MENU_ITEM`**; nếu không thì cần handler riêng.
- **Opcode 92** (nhiều sub) — Chat cộng đồng (sub 2), chuyển kênh (sub 3). Unity chưa register.
- **Opcode 91** (nhiều sub) — Chat bang (sub 20), tủ trang bị (sub 1/3/14/15/16/26/27), quản lý pet.
- **Opcode 58** — Đấu trường (`fr.java:290-297`).
- **Opcode 54** — cd 324 (không rõ trên map hub, có thể logout/report).
- **Opcode 90/96** — target-player actions.

---

## 7. Câu hỏi chưa giải quyết

1. **`maps/11.dat` (binary)** — chưa parse; muốn biết chính xác **những công trình nào** đang đặt trên "Thành Phố Linh Thú" (mấy shop, mấy portal, ở đâu). Cần thêm decoder cho format map.
2. **Opcode 62 (Tủ quần áo)** — server trả menu hay object list? Cần đọc `GServer/Server/GameController.cs` case 62.
3. **Portal destinations** — bấm portal ở map 11 warp tới map nào (map 3? map 15?). Có file `SendListMobZone` broadcast nhưng portal warp list chưa reverse.
4. **Ảnh NPC** — client jar tải `npcs/xxx.png` runtime từ server; Unity đã có `RemoteAssetCache` — cần confirm request path khớp.
5. **HUD stats packet** — packet nào bơm HP/MP/EXP realtime của self? (`PET_SERVICE` sub gì cho player stats vs pet stats)
6. **Level nhân vật chính** — client hiện chỉ có "level pet" (`UPDATE_PET_LVL`). Char level của người chơi ở packet nào?
7. Người dùng viết "NFC" — xác nhận là **typo của "NPC"** hay ý khác? Trong project không có mã NFC nào.
