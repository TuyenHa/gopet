---
title: "Hoàn thiện parity JAR → Unity còn thiếu"
description: >-
  Đóng các khoảng trống chức năng còn lại giữa client J2ME trong
  client.jar_Decompiler.com và GopetUnityClient, ưu tiên các packet đang bị bỏ qua,
  pet profile/gym, đồng bộ inventory, social/mail, settings và kiểm chứng live.
status: implemented-awaiting-manual-certification
priority: P0
branch: ""
tags: [unity, jar-parity, protocol, pet, social, live-smoke]
blockedBy: []
blocks: []
created: "2026-09-12T00:00:00+07:00"
createdBy: "codex"
---

# Hoàn thiện parity JAR → Unity còn thiếu

## 1. Mục tiêu

Hoàn thiện những chức năng **thực sự hoạt động trong client JAR** nhưng Unity hiện
chưa có, đang bỏ packet, hoặc mới nối được một nửa.

Plan này không tự mở rộng thành việc viết game mới. Những mục chỉ có nhãn hoặc
command chết trong JAR, hoặc chưa có implementation phía server, được tách khỏi
phạm vi parity.

### Definition of Done

- Không còn packet server dùng trong luồng chuẩn bị rơi vào `OnUnhandled` mà không
  có quyết định và lý do được ghi lại.
- Xem thông tin pet/người chơi, Gym, chat global, trang bị/gem/tattoo, mail,
  settings và logout chạy end-to-end trên Unity.
- Toàn bộ unit test, compile checks, PlayMode tests và LiveSmoke liên quan đều xanh.
- 24/24 map được smoke test về load, collision, portal, NPC, mob và building.
- Có bằng chứng kiểm thử thủ công cho các luồng không thể tự động hoá hoàn toàn.

## 2. Baseline đã xác nhận ngày 2026-09-12

| Hạng mục | Kết quả |
|---|---|
| Opcode Unity ↔ server | 180 hằng số khớp |
| Asset lấy từ JAR | 54 ảnh DAT, 327 PNG, 10 WAV, 24 map, 9 map animation, 27 battle animation, 6 actor animation |
| Chuỗi ngôn ngữ | 134 VN + 134 EN, khớp nguồn |
| Unit test offline | 633/633 pass |
| Compile Net `netstandard2.1` | Pass, 0 warning/error |
| Compile Runtime với Unity DLL | Pass, 0 warning/error |
| Compile Editor | Pass, 0 warning/error |
| Compile PlayMode tests | Pass, 0 warning/error |
| Compile LiveSmoke | Pass, 0 warning/error |
| `verify.ps1` tổng | Fail ở policy file >200 dòng; không fail compile/test |

Worktree hiện có lượng thay đổi chưa commit lớn. Trước khi triển khai phải tạo một
checkpoint an toàn và tuyệt đối không ghi đè các thay đổi không liên quan.

## 3. Khoảng trống đã xác nhận

### P0 — Chức năng đang hỏng hoặc phản hồi server bị mất

| ID | Chức năng | Hiện trạng | Nguồn đối chiếu |
|---|---|---|---|
| P0-01 | Nhận chat cộng đồng/global | Unity gửi được `CHAT_GLOBAL` nhưng chưa register handler nhận | `PlayerManager.chatGlobal`, `GameHud.SubmitChat`, `ChatHandler` |
| P0-02 | Xem thông tin pet/người chơi | Unity gửi `GET_PLAYER_INFO`; server trả `PET_SERVICE/MAGIC`, nhưng Unity chưa parser/view | `GameController.magic`, `TargetPlayerPackets.RequestInfo` |
| P0-03 | Pet Gym/cộng tiềm năng | Server gửi `GYM` và `UP_TIEM_NANG`; Unity chưa có handler/UI | `GameController.gym`, `updateTiemnang`, JAR `fy.java` |

### P1 — Luồng có thể thành công nhưng UI/state không đầy đủ

| ID | Chức năng | Hiện trạng |
|---|---|---|
| P1-01 | Đồng bộ trang bị pet | Thiếu xử lý `USE_EQUIP_ITEM`, `UNEQUIP_ITEM`, `REMOVE_ITEM_EQUIP` |
| P1-02 | Đồng bộ tháo gem | Thiếu handler `ON_UNQUIP_GEM`; item có thể giữ trạng thái cũ trên UI |
| P1-03 | Tattoo enchant | Menu cơ bản hoạt động, nhưng response `TATTOO/7` chọn nguyên liệu chưa được xử lý |
| P1-04 | Settings | Nút hiện chỉ ghi log “đợi Phase 5” |
| P1-05 | Logout | Chưa dispose phiên chơi và quay lại login |
| P1-06 | Âm thanh | JAR có cờ nhạc nền và hiệu ứng riêng; Unity đang gộp một cờ |
| P1-07 | Đổi ngôn ngữ | `JarStrings` đã hỗ trợ VN/EN và persist, nhưng chưa có UI |
| P1-08 | Tự đánh quái | JAR command 3285 bật/tắt vòng lặp gửi `AUTO_ATTACK_SUPPORT` mỗi khoảng 4 giây; Unity trước đây chỉ gửi một lần |
| P1-09 | Hộp thư | Đọc list được; chưa mở full content, mark, xoá hoặc soạn thư |

### P2 — Parity trải nghiệm và chứng nhận live

| ID | Chức năng | Hiện trạng |
|---|---|---|
| P2-01 | Lịch sử chat/system message | Unity có bubble/ticker nhưng chưa có transcript tương đương JAR |
| P2-02 | Building type 12 | Chưa mở đúng message/mail screen |
| P2-03 | Building type 32 | Đang đặt sai thành `PetFollowTrigger`; JAR thực tế mở `MAGIC` của pet |
| P2-04 | ArenaPlace | Code có, chưa live-certify vì phụ thuộc giờ sự kiện |
| P2-05 | ClanPlace | Code có, chưa live-certify vì cần tài khoản đã có bang |
| P2-06 | Visual parity | Chưa screenshot-diff đầy đủ cho 24 map và các tỉ lệ màn hình |
| P2-07 | Audio live-play | 10 SFX đã wire nhưng chưa xác nhận volume/timing trong phiên thật |

## 4. Ngoài phạm vi parity

### Building có nhãn nhưng không hoạt động trong JAR

Trong `eg.java`, nhiều building tạo local command nhưng `dv.java` không xử lý các
command đó. Chúng không được coi là thiếu parity:

- Nhà ở type 0–4.
- Vườn type 7.
- Cà phê type 10.
- Mini-game type 13–16: Caro, Cờ tướng, Tiến lên, Phỏm.
- Một số kiosk/local screen type 18–23: nón, giày, mỹ viện, tóc, vật phẩm, gara.

Nếu muốn làm các mục này, phải mở plan “Unity extension” riêng và chốt gameplay,
UI, protocol và server behavior trước.

### Pet League beta

`OP_PET_LEAGUE_BETA` mới chỉ là hằng số; phía server và Unity đều không có engine.
Đây là tính năng hai phía viết mới, không phải một lỗi port JAR.

### Không thuộc Unity client

- HTTP admin dashboard.
- Migration dữ liệu production và chính sách rollback.
- Tuning anti-cheat/telemetry vận hành.

## 5. Phases triển khai

## Phase 0 — Khoá baseline và ma trận protocol

**Ưu tiên:** P0  
**Ước lượng:** 0,5–1 ngày  
**Phụ thuộc:** Không

### Công việc

1. Tạo checkpoint/branch sau khi rà soát worktree hiện tại với chủ dự án.
2. Sinh danh sách tự động các opcode/sub-command server có thể gửi xuống.
3. So danh sách đó với `MessageRouter.Register` và `RegisterSub` của Unity.
4. Phân loại từng packet:
   - `handled`;
   - `intentionally ignored`;
   - `server-unused`;
   - `missing`.
5. Thêm contract test để packet mới từ server không thể bị bỏ qua âm thầm.
6. Ghi packet fixture thực tế hoặc fixture byte-for-byte cho các packet còn thiếu.
7. Tách kết quả policy “file >200 dòng” khỏi lỗi compile/test trong báo cáo verify.

### Acceptance Criteria

- Có một bảng coverage duy nhất được test tự động.
- Mọi packet missing trong bảng hiện tại đều có phase xử lý.
- Không sửa protocol hoặc wire format.

## Phase 1 — Đóng các protocol black hole

**Ưu tiên:** P0  
**Ước lượng:** 2 ngày  
**Phụ thuộc:** Phase 0

### Công việc

Thêm parser/model/event cho:

- `PET_SERVICE/CHAT_GLOBAL`.
- `PET_SERVICE/MAGIC`.
- `PET_SERVICE/GYM`.
- `PET_SERVICE/UP_TIEM_NANG`.
- `PET_SERVICE/TATTOO`, tối thiểu sub 7.
- `PET_SERVICE/USE_EQUIP_ITEM`.
- `PET_SERVICE/UNEQUIP_ITEM`.
- `PET_SERVICE/REMOVE_ITEM_EQUIP`.
- `PET_SERVICE/ON_UNQUIP_GEM`.

Mỗi parser phải có:

- Guard cho count, enum, độ dài và `ExpectFullyConsumed`.
- Test happy path.
- Test malformed/truncated packet.
- Test byte order và kiểu `sbyte/int/long/UTF` đúng với Java/server.

### Acceptance Criteria

- Không còn log unhandled cho các sub-command trên.
- `dotnet test tests/Gopet.Net.Tests` xanh.
- Net vẫn compile dưới `netstandard2.1`.

## Phase 2 — Pet Profile và Gym

**Ưu tiên:** P0  
**Ước lượng:** 3–4 ngày  
**Phụ thuộc:** Phase 1

### Công việc

1. Dựng `PetProfileView` hiển thị:
   - avatar/frame pet;
   - tên, hệ, class, level;
   - EXP hiện tại/mốc tiếp theo;
   - STR, AGI, INT, ATK, DEF;
   - HP/MP;
   - skill, mô tả và MP cost;
   - tattoo;
   - điểm tiềm năng.
2. Cho pet của bản thân dùng chế độ interactive.
3. Cho pet của người chơi khác dùng chế độ readonly.
4. Wire `TargetPlayerMenu → RequestInfo → MAGIC → PetProfileView`.
5. Dựng `PetGymView` với ba lựa chọn STR/AGI/INT.
6. Gửi `UP_TIEM_NANG` đúng `int num + sbyte index` theo JAR/server.
7. Áp dụng delta server trả về mà không cần đóng/mở màn hình.
8. Sửa building type 32 để gửi `PET_SERVICE/MAGIC`.
9. Xử lý rõ trường hợp người chơi không dẫn pet.

### Tests

- Unit test parser `MAGIC`, `GYM`, `UP_TIEM_NANG`.
- PlayMode test layout pet profile với tên/mô tả dài.
- PlayMode test cộng từng stat và hết điểm.
- Test xem pet người khác không hiện nút chỉnh sửa.
- LiveSmoke mở pet profile và cộng một điểm bằng tài khoản fixture.

### Acceptance Criteria

- “Xem thông tin” không còn là nút gửi packet rồi im lặng.
- Stat sau Gym khớp phản hồi server.
- Building type 32 hoạt động giống JAR.

## Phase 3 — Đồng bộ trang bị, gem và tattoo

**Ưu tiên:** P1  
**Ước lượng:** 2–3 ngày  
**Phụ thuộc:** Phase 1

### Công việc

1. Dùng một nguồn state thống nhất cho pet equipment và gem.
2. Xử lý delta sau:
   - mặc item;
   - tháo item;
   - huỷ item;
   - gắn gem;
   - tháo gem thường/nhanh;
   - item bị phá huỷ khi enchant thất bại.
3. Parse đầy đủ payload `ON_UNQUIP_GEM`, gồm timestamp/countdown.
4. Refresh slot, icon, level, trạng thái gem và stat pet ngay sau response.
5. Hoàn thiện chọn material 1/2 của tattoo enchant.
6. Giữ GenericMenuView cho các bước server-driven; chỉ dựng UI riêng nơi packet
   không thể biểu diễn bằng menu generic.

### Tests

- Equip/unequip/destroy success và error.
- Gắn/tháo gem, countdown và fast-unmount.
- Tattoo material slot 1/2 và confirm.
- Server delta đến khi view đang đóng không làm lỗi hoặc mở popup ngoài ý muốn.

### Acceptance Criteria

- Không còn slot “ma” hoặc item đã thay đổi trên server nhưng UI vẫn giữ bản cũ.
- Mọi hành động thất bại đều hiện dialog/toast server trả về.

## Phase 4 — Chat, mail, settings và vòng đời session

**Ưu tiên:** P1  
**Ước lượng:** 3–4 ngày  
**Phụ thuộc:** Phase 1

### Chat

1. Nhận và lưu bounded history cho chat khu vực, cộng đồng và bang hội.
2. Dựng transcript ba tab, giới hạn số dòng tương đương hoặc an toàn hơn JAR.
3. Vẫn giữ chat bubble trên world cho chat khu vực.
4. Thêm cooldown feedback cho global/guild chat.

### Mail

1. Biến row trong `MailboxView` thành nút thật.
2. Mở title/full content.
3. Gửi mark-read (`LETTER_COMMAND_SET_MARK`).
4. Xoá thư (`LETTER_COMMAND_REMOVE_LETTER`) có confirm.
5. Soạn thư (`LETTER_COMMAND_SEND_LETTER`) với người nhận và nội dung.
6. Refresh unread indicator và danh sách sau mỗi thao tác.

### Settings

1. Tách trạng thái nhạc nền và sound effect thành hai cờ persist riêng.
2. Thêm VN/EN và gọi refresh đối với UI đang mở.
3. Thêm trạng thái tự đánh quái; khi bật gửi ngay rồi lặp `AUTO_ATTACK_SUPPORT` mỗi khoảng 4 giây, khi tắt phải dừng hẳn.
4. Giữ nút quick sound ở login nhưng cho nó phản ánh đúng trạng thái settings.

### Logout

1. Chặn input mới.
2. Dispose socket/session và unregister lifecycle component cần thiết.
3. Huỷ GameSession/world/HUD mà không huỷ bootstrap cần cho login.
4. Quay về login và giữ credential theo lựa chọn “Nhớ tài khoản”.
5. Cho phép login lại mà không duplicate router, event hoặc AudioSource.

### Acceptance Criteria

- Hai client nhìn thấy chat global của nhau.
- Mail gửi, nhận, đọc, mark và xoá được trên server thật.
- Settings được giữ sau restart.
- Logout/login lại 5 vòng không rò socket và không nhân đôi handler.

## Phase 5 — Building parity chính xác

**Ưu tiên:** P2  
**Ước lượng:** 1–2 ngày  
**Phụ thuộc:** Phase 2 và Phase 4

### Công việc

1. Lập bảng đủ building type `0..32` với ba trạng thái:
   - JAR-active;
   - JAR-inert;
   - Unity-extension.
2. Xác nhận và test:
   - type 8: phòng vé/teleport;
   - type 11: đổi khu;
   - type 12: message/mail screen;
   - type 17: shop skin;
   - type 26: arena;
   - type 27–30: weapon/armour/hat/food shop;
   - type 31: Gym;
   - type 32: pet profile/Magic.
3. Không để building JAR-active rơi vào toast “chờ phase kế”.
4. Với type 18–23, quyết định rõ giữ hành vi mở rộng Unity hay quay về JAR-inert.

### Acceptance Criteria

- `BuildingDispatcherTests` phủ đủ type `0..32`.
- Mapping packet của từng building active khớp byte với JAR/server.
- Tài liệu không còn tuyên bố mini-game là parity gap.

## Phase 6 — Live certification và release gate

**Ưu tiên:** P2  
**Ước lượng:** 2–3 ngày cộng thời gian kiểm thử thủ công  
**Phụ thuộc:** Phase 1–5

### Automated checks

1. Chạy toàn bộ `verify.ps1`.
2. Chạy PlayMode tests thật bằng Unity Editor, không chỉ compile project test.
3. LiveSmoke cho:
   - global chat;
   - pet profile self/other;
   - Gym;
   - equip/gem/tattoo;
   - mail;
   - logout/relogin;
   - 24 map cơ bản.
4. Assert không có packet unhandled trong kịch bản chuẩn.

### Manual checks

1. Screenshot-diff ở tối thiểu:
   - 16:9;
   - 19.5:9;
   - tablet 4:3.
2. Kiểm tra 24 map về camera, sorting, collision, portal và actor placement.
3. Kiểm tra timing/volume 10 SFX trong login, warp và battle thật.
4. ArenaPlace trong đúng khung giờ event.
5. ClanPlace bằng tài khoản fixture đã có bang và đủ currency.

### Code-health gate

1. Tách các partial/component đang vượt 200 dòng khi việc tách không làm giảm độ
   rõ ràng.
2. Nếu giữ ngoại lệ, ghi rõ ngoại lệ trong README và verify allowlist.
3. Không để cảnh báo compile, lỗi Unity Console hoặc socket `CLOSE_WAIT`.

### Acceptance Criteria

- `verify.ps1` xanh hoàn toàn.
- Có báo cáo LiveSmoke và manual certification lưu trong `plans/reports/`.
- Không còn P0/P1 trong bảng gap.

## 6. Thứ tự thực hiện khuyến nghị

1. Phase 0 — protocol matrix và baseline.
2. Phase 1 — handler cho packet đang bị bỏ.
3. Phase 2 — Pet Profile/Gym.
4. Phase 3 — equipment/gem/tattoo state.
5. Phase 4 — social/settings/logout.
6. Phase 5 — building parity.
7. Phase 6 — live certification/release gate.

Tổng ước lượng: **10–14 ngày công**, chưa tính thời gian chờ cửa sổ ArenaEvent và
chuẩn bị tài khoản ClanPlace.

## 7. Rủi ro và cách kiểm soát

| Rủi ro | Tác động | Cách kiểm soát |
|---|---|---|
| Worktree lớn, chưa commit | Dễ ghi đè hoặc trộn thay đổi | Checkpoint trước Phase 0; patch nhỏ theo phase |
| Protocol có opcode trùng giữa top-level/sub-command | Parser bắt sai packet | Luôn test theo envelope + sub-command và fixture byte |
| Server gửi delta không gửi snapshot | UI dễ lệch state | Central state store + refresh request khi cần |
| UI JAR dùng nhiều screen local obfuscated | Dễ hiểu nhầm nhãn là chức năng | Chỉ coi là active khi truy được call site và handler thật |
| Arena/Clan khó tự động hoá | Thiếu bằng chứng live | Fixture account + checklist thủ công có log/ảnh |
| Đổi ngôn ngữ khi UI đang mở | Text trộn VN/EN | Event language-changed và rebuild/refresh view |
| Logout để lại event subscription | Login lần hai nhận packet nhiều lần | Test 5 vòng và kiểm tra router/socket lifecycle |

## 8. Các quyết định cần chốt trước khi mở rộng ngoài parity

1. Có giữ các shortcut Unity cho building type 18–23 hay bám chính xác hành vi
   JAR-inert?
2. Có làm bốn mini-game như tính năng mới không? Nếu có, mỗi game cần plan riêng.
3. Có làm Pet League beta không? Nếu có, cần thiết kế cả server và client.
4. Có port event 2024/2025 đã hết hạn hay loại khỏi production menu?
5. Chính sách migration tài khoản/nhân vật cũ là gì?

## 9. Tài liệu và mã nguồn đối chiếu

- `client.jar_Decompiler.com/fr.java`: menu nhân vật, settings, audio, logout,
  friends/mail và thao tác pet.
- `client.jar_Decompiler.com/eg.java`: building type 0–32.
- `client.jar_Decompiler.com/dv.java`: handler thật của local building command.
- `client.jar_Decompiler.com/fy.java`: Gym và cộng tiềm năng.
- `client.jar_Decompiler.com/m.java`: soạn thư.
- `SRCGOPETGOC/GServer/Server/GameController.cs`: wire format và gameplay handler.
- `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs`: battle command và item menu.
- `GopetUnityClient/Assets/Scripts/Net/MessageRouter.cs`: router hiện tại.
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs`: wire UI/session.
- `GopetUnityClient/Assets/Scripts/UiLogic/BuildingDispatcher.cs`: mapping building hiện tại.
- `GopetUnityClient/verify.ps1`: release verification baseline.
