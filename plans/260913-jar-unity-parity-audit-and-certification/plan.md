---
title: "Audit và chứng nhận parity JAR → Unity"
description: >-
  Kiểm kê chức năng thực sự hoạt động trong client J2ME, đối chiếu implementation Unity,
  đóng các khoảng trống còn lại và tạo bằng chứng release-level thay cho suy luận từ file tồn tại.
status: in_progress
priority: P0
created: "2026-09-13T00:00:00+07:00"
createdBy: codex
---

# Audit và chứng nhận parity JAR → Unity

## 1. Kết luận audit ngày 2026-09-13

**Chưa thể xác nhận đã migrate hết 100%.** Phần code audit tự động đã hoàn tất, nhưng project
chưa thể gắn nhãn `certified` cho tới khi chạy được PlayMode thật, Live E2E và device certification:

- `verify.ps1` qua 10/10; 669/669 unit test qua; mọi tầng compile với 0 warning/error.
- Gate protocol sinh từ call-site server có 71 route `handled`, 1 `intentionally-ignored`,
  2 `server-unused`, 0 `missing`; gate này đã được ghép vào `verify.ps1`.
- Đã đóng ba route thật bị thiếu: đổi mật khẩu opcode 100, public chat `81/66`, và chọn nguyên
  liệu cường hóa `81/47` bị che sau helper động.
- PlayMode batch bị chặn trước test runner bởi mutex `Unity.Licensing.Client`, nên chưa có XML.
- LiveSmoke chưa chạy vì không có server lắng nghe ở `127.0.0.1:19180`; file server packet dump
  đang có thay đổi từ trước được giữ nguyên, không dùng làm output test.
- Các luồng phụ thuộc dữ liệu/timing thực tế chưa được chứng nhận end-to-end.

Không dùng một phần trăm tổng duy nhất vì login, battle và một mini-game không có cùng trọng số.
Trạng thái dưới đây được chấm theo bằng chứng: **Hoàn thành**, **Một phần**, **Chưa chứng nhận**,
**Không thuộc parity**.

## 2. Ma trận chức năng chi tiết

| Nhóm JAR | Chức năng | Trạng thái Unity | Bằng chứng / phần còn thiếu |
|---|---|---|---|
| Kết nối | Framing, TEA, handshake, opcode âm, UTF, keep-alive | Hoàn thành | Net compile dưới `netstandard2.1`; test wire/TEA; LiveSmoke cũ đã đối chiếu byte |
| Tài khoản | Chọn server, login, register, login fail, nhớ tài khoản | Hoàn thành | `AuthHandler`, `LoginFlow`, `CredentialStore`, các test auth/login |
| Nhân vật | Tạo nhân vật, validate tên, preview | Hoàn thành | `CharacterCreationView`, PlayMode tests đã có nhưng cần chạy lại trong Editor |
| Session | Logout, login lại, thoát ứng dụng, đổi mật khẩu | Hoàn thành về code | Cần live 5 vòng để bắt duplicate handler/socket leak |
| Asset | 54 ảnh DAT, 327 PNG, 10 WAV, string VN/EN | Hoàn thành | Asset/string source gate qua |
| Map | 24 map, tile, collision, camera, movement, joystick | Hoàn thành về code | Asset/layout test qua; còn manual 24-map certification |
| Map | Portal, warp, fade, tên map, minimap/map picker | Hoàn thành về code | Có handler/view/test; còn kiểm tra đích portal và screenshot thật |
| World | Player/NPC/mob/building spawn, animation, skin, wing, label | Hoàn thành phần lõi | Building đã render/tap; visual và remote NPC image cần manual |
| Building | Type 8, 11, 12, 17, 22, 25–32 | Hoàn thành hoặc server-driven | Ticket/channel/mail/shop/arena/gym/profile đã dispatch |
| Building | Type 0–4, 7, 10, 19–21, 23 | Không thuộc parity | Đã trace: command local không có case trong `dv.java`; Unity trả `Noop` |
| Building | Type 18, 22 | Unity extension | JAR command inert; Unity chủ động mở shop Mũ/Thức ăn thật, không tính là bằng chứng parity |
| Building | Type 13–16 (Caro/Cờ tướng/Tiến lên/Phỏm) | Không thuộc parity | Đã xác minh: JAR chỉ có nhãn + command chết, 24 map không dùng type này, server không có engine; Unity packet stub đang nhầm `REQUEST_SHOP` |
| NPC/UI server | Talk NPC, option, list, form input, image dialog, yes/no, banner | Hoàn thành | Guider/router/UI generic đã wire |
| HUD | HP/MP/EXP/level, energy/currency, event/boss/status | Hoàn thành về code | `PlayerStatsHandler`, `CharacterHud`, `GameHud.Status`; cần live value checks |
| Pet cơ bản | Chọn/follow/unfollow, profile self/other, cảm xúc, hồi phục | Hoàn thành về code | Handler, packets, radial/profile UI |
| Pet gym | STR/AGI/INT, điểm tiềm năng, update tại chỗ | Hoàn thành về code | Parser/UI/unit tests; LiveSmoke cũ có MAGIC/GYM |
| Pet item | Rương, 5 slot trang bị, equip/unequip/remove | Hoàn thành về code | Delta handler + snapshot refresh; cần live với item fixture |
| Gem/tattoo | Kho gem, gắn/tháo, enchant/tier, tattoo tạo/xóa/enchant | Hoàn thành về code | Parser/packet/UI tests; tattoo cần live account đủ material |
| Pet upgrade | Đổi tên, cường hóa/tiến hóa/up tier | Hoàn thành về code | `PetUpgradeHandler/View`; cần live economy/result validation |
| Battle | PvE/PvP turn, attack/skill/item/result/effect/float text | Hoàn thành về code | Battle handler/view/tests; PvP invitation cần live hai client |
| Battle | Auto attack chu kỳ 4 giây | Hoàn thành | `AutoAttackLoop`, settings/menu và unit tests |
| Shop/economy | Shop server, pet/skin/weapon/armour/hat/food, ATM | Hoàn thành chủ yếu server-driven | Shop popup + generic ATM menu; cần mua/bán live |
| Ký gửi | Nhận danh sách kiosk và UI listing | Hoàn thành về code | `KioskHandler`, `KioskListingView`; thao tác giao dịch cần live |
| Chat | Khu vực, cộng đồng/global, bubble, bounded history | Hoàn thành về code | Parser/UI/cooldown; global cần test hai client |
| Mail | Inbox, full content, mark, xóa, soạn/gửi | Hoàn thành về code | Handler/view/tests; cần E2E hai tài khoản |
| Bạn bè/target | Danh sách server-driven, add, block, info, PK, xem equip | Hoàn thành phần trigger/UI | Tap avatar đã nối `TargetPlayerMenu`; response đặc thù cần live certify |
| Bang hội | Danh sách/tìm/join/info/member/kick/top/quỹ/skill/chat | Hoàn thành về code | Có handler/model/view/wiring và fixture parser/state/chat/malformed; còn live account có bang |
| Kênh/teleport | Danh sách khu, chuyển khu, danh sách điểm đến | Hoàn thành về code | Handler/view hiện có; cần live server state |
| Nhiệm vụ/sự kiện | Mở danh sách nhiệm vụ, NPC/event menu | Hoàn thành theo UI server-driven | Nội dung do server cấp; cần smoke daily/event đặc biệt |
| Cài đặt | Music/effect riêng, tổng âm thanh, VN/EN, auto attack | Hoàn thành về code | Persist/UI có; cần kiểm tra refresh toàn bộ UI sau đổi ngôn ngữ |
| Audio/visual | 10 SFX, pixel canvas, JAR skin/font/animation | Chưa chứng nhận | Chưa nghe timing/volume và chưa screenshot-diff 3 tỉ lệ màn hình |
| Protocol tổng thể | 180 hằng opcode + server writer routes | Hoàn thành về static gate | 71 handled, 1 intentionally-ignored, 2 server-unused, 0 missing; report JSON được sinh tự động |

## 3. Phạm vi không nên tính là “thiếu migrate” khi chưa có bằng chứng

- HTTP admin, migration database, anti-cheat/telemetry server: không thuộc client Unity.
- Pet League beta: chỉ có hằng số, không có engine hoàn chỉnh ở JAR/server.
- Building/menu có nhãn nhưng command không có handler trong JAR: là UI chết của bản cũ, không phải
  chức năng đang chạy. Chỉ port nếu chủ dự án muốn biến nó thành tính năng Unity mới.
- Bốn mini-game đã được xác minh là JAR-inert/server-unused và không xuất hiện trong 24 map.
  `MiniGamePackets` hiện là stub sai protocol, không phải implementation game và cần được dọn bỏ.

## 4. Kế hoạch thực hiện

### Phase 0 — Khóa baseline và sửa release gate (0,5 ngày, P0)

1. Giữ nguyên thay đổi không liên quan ở server packet dump.
2. Tách ba file vượt 200 dòng mà không đổi behavior.
3. Chạy lại `verify.ps1`; yêu cầu 10/10, 654+ test, 0 warning/error.

**Done:** gate xanh hoàn toàn và diff chỉ chứa refactor có test bảo vệ.

### Phase 1 — Ma trận protocol đầy đủ (1–2 ngày, P0)

1. Trích mọi packet server có thể gửi từ các call-site ghi `Message`/controller, không chỉ hằng số.
2. Trích mọi opcode/sub-command mà JAR đọc và mọi `Register/RegisterSub` của Unity.
3. Sinh một file machine-readable với bốn trạng thái: `handled`, `intentionally-ignored`,
   `server-unused`, `missing`.
4. Thêm contract test làm CI fail khi xuất hiện route mới chưa phân loại.
5. Với mỗi `missing`, lưu fixture byte thực hoặc fixture dựng từ chính server writer.

**Done:** không còn server→client route không có quyết định; ma trận được sinh lại tự động.

### Phase 2 — Đóng code gap thật (1–3 ngày, P0/P1)

1. Trace các building local còn toast qua JAR và server; chỉ implement những flow có call path thật.
2. Bổ sung test parser/view/wiring chuyên biệt cho Guild (đây là vùng code mới nhưng coverage chưa rõ).
3. Bổ sung test logout/relogin lifecycle, language refresh và target-player response.
4. Xử lý mọi route `missing` tìm thấy ở Phase 1 theo thứ tự: mất state → chặn gameplay → cosmetic.
5. Dọn `MiniGamePackets`, test và tài liệu suy luận sai; type 13–16 phải là `Noop/JAR-inert`.

**Done:** không còn toast “chờ phase” trên bất kỳ flow JAR-active nào; test malformed packet đi kèm.

### Phase 3 — Chạy Unity PlayMode thật (0,5–1 ngày, P0)

1. Đóng mọi Unity Editor đang giữ project.
2. Chạy `run-playmode-tests.ps1`, đọc XML result thay vì chỉ dựa exit code.
3. Sửa mọi failure; chạy lại tới khi toàn bộ pass.
4. Lưu XML/log và số lượng test vào báo cáo certification.

**Done:** PlayMode chạy trong Unity Editor xanh, không chỉ compile project test.

### Phase 4 — Live E2E với server thật (1–2 ngày, P1)

Chuẩn bị hai tài khoản fixture: có pet/equipment/gem/tattoo/gold và một tài khoản có bang. Chạy:

1. Login/create/logout/relogin 5 vòng; không socket leak, không duplicate handler.
2. Hai client: place/global/guild chat, mail send/read/mark/delete, friend/target/PK invite.
3. Pet: profile self/other, Gym, equip/gem/tattoo, upgrade/evolve, recovery.
4. Economy: shop, ATM, kiosk; xác nhận server state sau đóng/mở lại UI.
5. Map: load 24 map, collision, portal, NPC, mob, building; log mọi `OnUnhandled` là test failure.
6. Arena đúng cửa sổ event và ClanPlace bằng account có bang.

**Done:** checklist có packet log và ảnh bằng chứng; không unhandled trong happy path.

### Phase 5 — Visual/audio/device certification (1–2 ngày, P2)

1. Screenshot-diff JAR/Unity tại 16:9, 19.5:9 và 4:3 cho login, HUD, battle và 24 map.
2. Kiểm tra touch target, safe area, keyboard/input, camera/sorting trên thiết bị thật.
3. Nghe 10 SFX và xác nhận timing/volume cho login, warp và battle.
4. Đo menu 200 dòng: FPS trung bình và 1% low theo target device.

**Done:** sai khác có threshold/waiver rõ ràng; không còn lỗi console hoặc visual blocker.

### Phase 6 — Chốt release parity (0,5 ngày)

1. Cập nhật ma trận chức năng bằng bằng chứng cuối cùng.
2. Đóng toàn bộ P0/P1 hoặc ghi waiver có chủ sở hữu và lý do.
3. Chạy lại full verify + PlayMode + LiveSmoke.
4. Gắn trạng thái `certified` và lưu report trong thư mục plan này.

## 5. Thứ tự và ước lượng

Thứ tự bắt buộc: **Phase 0 → 1 → 2 → 3 → 4 → 5 → 6**.

Ước lượng để chứng nhận phần client đang có: **4,5–8 ngày công**, chưa tính thời gian chờ event
Arena và chuẩn bị tài khoản dữ liệu. Nếu muốn bổ sung bốn mini-game, đó là epic sản phẩm mới ở cả
Unity lẫn server và không nằm trong ước lượng parity này.

## 6. Release criteria

- `verify.ps1` 10/10 và Unity PlayMode test thật đều xanh.
- Ma trận protocol đầy đủ không có `missing` hoặc route chưa phân loại.
- Không có `OnUnhandled` trong toàn bộ kịch bản chuẩn.
- Tất cả chức năng JAR-active có UI trigger, response handling và state refresh tương ứng.
- 24 map và ba tỉ lệ màn hình được chứng nhận; audio được nghe trên thiết bị thật.
- Mọi mục không port phải có bằng chứng là JAR-inert, server-unused hoặc được duyệt là ngoài phạm vi.
