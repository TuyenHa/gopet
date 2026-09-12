---
title: goPet Unity Client Rebuild
description: >-
  Viết mới client goPet bằng Unity nói đúng giao thức của GServer hiện có. Không
  đổi byte nào trong gói tin.
status: in-progress
priority: P1
branch: ''
tags:
  - unity
  - protocol
  - client
  - reverse-engineering
blockedBy: []
blocks: [260907-2210-map-portal-warp-shop-interaction, 260909-2100-linh-thu-city-parity-implementation]
created: '2026-09-05T07:03:44.496Z'
createdBy: 'ck:plan'
source: skill
---

# goPet Unity Client Rebuild

## Overview

Thay client J2ME (`SRCGOPETGOC/client.jar`) bằng client Unity mới, **giữ nguyên GServer và giao thức**.

Không port code decompile — 139/221 file không compile được (field trùng tên hợp lệ ở bytecode, không hợp lệ ở source). Bản decompile dùng làm **tài liệu tra cứu**, đặc biệt cho format `.dat` là thứ server không mô tả.

### Quyết định đã chốt

| Hạng mục | Chốt |
|---|---|
| Nền tảng | Android + iOS + PC. Không WebGL → TCP thuần, không cần proxy |
| Phạm vi | Vertical slice trước (P1-P6 chi tiết), P7-P8 chỉ phác thảo |
| GServer | Chỉ vá bảo mật, **không đổi format gói tin** |
| Verify | Dựng emulator J2ME ở P1 để đối chiếu gói tin |

### Tại sao khả thi

1. **Có source server** — đó là đặc tả giao thức chuẩn, không phải đoán từ bytecode
2. **UI do server điều khiển** — 162 màn hình đi qua 1 định dạng gói (`showMenuItem`) → Unity chỉ cần 4 component generic
3. **Asset stream sẵn** — 10.586 PNG trên server, client nạp bằng `Texture2D.LoadImage()`
4. **Giao thức đã khôi phục 100%** — handshake + khung gói + TEA khớp 1:1 giữa client decompile và server

### Rủi ro số 1: desync giao thức

Đọc sai thứ tự một field → cả stream lệch → lỗi hiện ở chỗ hoàn toàn khác. Đây là thứ giết tiến độ, **không phải Unity**. Toàn bộ P1 tồn tại để dựng hạ tầng chống việc này.

Khối lượng thật: **696 vị trí `put*`** trên server = số field client phải parse đúng thứ tự. Nhưng chúng tập trung vào 2 opcode bao ngoài: `PET_SERVICE` (81) và `COMMAND_GUIDER` (122).

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Server Hardening & Verify Infra](./phase-01-server-hardening-verify-infra.md) | Complete |
| 2 | [Transport Layer](./phase-02-transport-layer.md) | Complete |
| 3 | [Login & Handshake](./phase-03-login-handshake.md) | Complete (UI hoãn sang P5) |
| 4 | [Remote Asset Pipeline](./phase-04-remote-asset-pipeline.md) | Complete (Play mode hoãn sang P5) |
| 5 | [Generic UI Components](./phase-05-generic-ui-components.md) | Code xong, chờ nghiệm thu (chạy lại PlayMode sau đợt sửa review; đo fps trên máy; diff dump với client cũ; bấm Play nghiệm thu ảnh P4) |
| 5.1 | [Asset từ jar + màn đăng nhập J2ME](./phase-05-1-jar-assets-and-login-skin.md) | Code xong, verify + PlayMode xanh (108/108); còn đối chiếu ảnh chụp FreeJ2ME bằng tay |
| 6 | [Map Rendering & Movement](./phase-06-map-rendering-movement.md) | End-to-end xanh (INIT_PLAYER/MAP_UPDATE/move-echo LiveSmoke pass); còn Memory Profiler + Android fps + jar diff-dump chờ device/manual |
| 7 | [Pet Battle](./phase-07-pet-battle-outline.md) | PvE + PvP live-smoke đều xanh (byte thật qua GServer, 2 tài khoản PvP riêng) — chỉ còn PlayMode Editor Test Runner, chờ đóng Unity |
| 8 | [Long Tail Features (Outline)](./phase-08-long-tail-features-outline.md) | In progress — ~65 items complete; dialogs/HUD/guild/security/kiosk hardening done; ChallengePlace/MarketPlace live-certified; ArenaPlace/ClanPlace blocked; Pet League → separate plan |

**P5.1 nằm ngoài đường găng.** Nó là việc mỹ thuật, hoãn hoặc cắt được, không đụng dòng logic nào.

**Mốc vertical slice = hết P6.** Login → vào map → đi lại → chat → mở menu/shop. Đủ chứng minh toàn bộ kiến trúc chạy thật.

## Ước lượng

| Phase | Effort | Rủi ro |
|---|---|---|
| P1 Server hardening + verify infra | 1 tuần | Thấp |
| P2 Transport | 1 tuần | Thấp — đặc tả đầy đủ |
| P3 Login & handshake | 1 tuần | Thấp |
| P4 Asset pipeline | 1 tuần | Thấp |
| P5 Generic UI | 3-4 tuần | Thấp |
| P5.1 Asset từ jar + skin màn đăng nhập | 1 tuần | Thấp — format `.dat` đã giải |
| P6 Map & movement | 3-4 tuần | **Trung bình** — reverse `.dat` |
| **Vertical slice** | **~2.5-3 tháng** | |
| P7 Battle | 4-6 tuần | Trung bình |
| P8 Long tail | 6-10 tuần | Trung bình |
| **Parity đầy đủ** | **~5-8 tháng** | 1-2 dev |

## Dependencies

- **Không có plan nào khác trong `plans/`** — đây là plan đầu tiên
- Tham chiếu 2 report đã có:
  - `plans/reports/analysis-260905-1330-gopet-server-source.md` — phân tích GServer, lỗ hổng bảo mật
  - `plans/reports/feasibility-260905-1400-unity-client-rebuild.md` — đánh giá khả thi, số liệu giao thức

## Nguồn tham chiếu giao thức

| Cần biết | Đọc file |
|---|---|
| Khung gói, TEA, handshake | `GServer/Server/IO/{Message,TEA,MsgSender,MsgReader,Session}.cs` |
| Big-endian IO | `GServer/Server/IO/{IOExtension,DataOutputStream,DataInputStream}.cs` |
| Danh sách opcode | `GServer/Server/GopetCMD.cs` (177 hằng số) |
| Server gửi gì | `GServer/Server/GameController.cs`, `MenuController.sendMenu.cs` |
| Login flow | `GServer/Server/Player.cs:100-169`, `:572-670` |
| Format `.dat` map | `client.jar_Decompiler.com/ef.java:104-130` |
| Format sprite/anim | `client.jar_Decompiler.com/dy.java`, `a.java` |
| Handshake client-side | `client.jar_Decompiler.com/eq.java:a(long)` |

## Nguyên tắc xuyên suốt

1. **Không đổi giao thức.** Client J2ME cũ phải luôn chạy được song song — đó là công cụ debug tốt nhất.
2. **`GopetCmd.cs` là bản copy nguyên văn** của `GServer/Server/GopetCMD.cs`. Viết script sinh tự động, không gõ tay.
3. **Packet logger bật từ ngày đầu**, cả 2 đầu. Không có nó thì debug desync là mò kim đáy bể.
4. File dưới 200 dòng, 1 handler 1 file (theo `.claude/rules/development-rules.md`).
5. Không mock, không giả lập — implement thật.
6. **Mỗi test phải chứng minh được là nó biết đỏ.** Đột biến một chỗ rồi chạy lại; test không đổi màu là test vô dụng. Đã dính: check "dump ghi ra được" chỉ gọi `File.Exists`, mà file luôn tồn tại vì constructor tạo nó — xanh vĩnh viễn (`phase-02`).
7. **Đo cái tất định, đừng đo cái xấp xỉ.** Đếm số luồng của tiến trình để tìm rò là lệch về âm tính giả; hỏi thẳng đối tượng xem nó đã đóng sạch chưa mới đúng. Tương tự: lấy cổng từ chính socket, đừng dò bảng TCP của hệ điều hành.
8. **Bảng đặc tả trong plan là suy từ code, wire thật mới là sự thật.** Gói `LOGIN` trong plan ghi 3 UTF; client cũ gửi 4 UTF + 8 byte, và server đọc 3 rồi gọi nhầm tên field (`phase-03`). Bắt gói thật trước khi viết handler.
9. **Mất kết nối không kèm thông điệp là trạng thái hợp lệ.** Đã từng mất gói cuối vì `Session.Close()` ngắt luồng gửi trước khi drain (đã sửa, `phase-01`) — nhưng mạng đứt hay server chết thì vẫn vậy. Luôn có UI cho trường hợp rớt không rõ lý do.
10. **Chặn TRÊN cũng phải kiểm, không chỉ chặn dưới.** Check "đã trả lời ít nhất một nhịp CHECK_SPEED" xanh với cả 1 lẫn 805 nhịp — và 805 chính là bug. Ngưỡng một phía là ngưỡng mù một nửa (`phase-03`).
11. **File có logic hành vi phải có unit test hành vi.** Byte-exact test chứng minh gói đúng hình dạng, không chứng minh đúng THỜI ĐIỂM. `AuthHandler` từng là file duy nhất có logic mà không có test, và đúng nó chứa lỗi nặng nhất.
12. **Ranh giới assembly chỉ Unity mới thấy.** Project compile-only gộp nhiều thư mục vào một assembly, nên chúng mù với lỗi "asmdef quên khai báo tham chiếu". Đã dính một lần, và chỉ lộ ra sau khi bắt người dùng đóng Editor (`phase-05`). Lint riêng: `tools/check-asmdef-refs/`.
13. **Vector test phải phân biệt được hai cách làm.** Mọi vector menu đều có `itemId == vị trí dòng`, nên gửi vị trí và gửi id cho ra kết quả giống hệt nhau — bug sai hẳn chức năng lọt qua cả test lẫn kiểm chứng live (`phase-05`). Dữ liệu test giống nhau ở chỗ cần phân biệt thì test không kiểm gì cả.
14. **Test gọi thẳng phương thức thì bỏ qua nhánh người dùng thật đi.** 26/26 PlayMode xanh trong khi dòng menu rộng 0 pixel, không bấm được. Ít nhất một test phải đi qua đường vào thật (con trỏ, phím).
15. **Test nhập liệu bằng bàn phím tiếng Anh.** Bộ gõ Telex nuốt phím (`test1234` → `tét1234`); ô nhập của Unity sẽ dính y hệt emulator.
16. **Assembly định sẵn che mất tham chiếu thiếu.** Test PlayMode chạy được suốt vì rơi vào `Assembly-CSharp`, thứ tự động nối mọi assembly `autoReferenced: true`. Cái nào đặt `false` (`Unity.InputSystem.TestFramework`) thì vô hình — và chỉ lộ ra sau khi đã bắt người dùng đóng Editor. Mọi thư mục test phải có asmdef khai báo tường minh (`phase-05`).
17. **Đột biến xong phải `touch` lại file.** Khôi phục bằng cách chuyển backup đè lên trả về mtime CŨ, MSBuild kết luận "không có gì đổi" và dùng lại DLL đã đột biến. Lần chạy sau báo test đỏ trong khi source đúng — suýt đi sửa lỗi không tồn tại (`phase-05`).
18. **Cái thước cũng phải đo lại.** `verify.ps1` đếm dòng bằng `Measure-Object -Line`, thứ **bỏ qua dòng trống** — file 260 dòng với 60 dòng trống vẫn báo "dưới 200" suốt nhiều phase. Guard nào cũng phải tự hỏi nó thật sự đo gì (`phase-05`).
19. **Sự kiện từ tầng dưới phải kèm danh tính của thứ phát ra nó.** Tự tay đóng một socket cũng sinh ra "mất kết nối"; cờ đó sống qua quãng không có socket nào rồi giết socket kế tiếp — vòng lặp vĩnh viễn (`phase-05`).
20. **Đường chạy của máy dev không phải đường chạy thật.** Lỗi trên hỏng 100% ở ngoài kia mà xanh sạch ở đây, chỉ vì danh sách máy chủ trên máy dev luôn trả về `127.0.0.1` nên nhánh nối lại chưa từng chạy. Hỏi thẳng: dữ liệu thật khác dữ liệu test ở chỗ nào (`phase-05`).
21. **Muốn chứng minh chữ ký được kiểm thì phải đột biến chính chữ ký.** Sửa byte trong phần mã hoá vẫn bị đệm CBC bắt, nên test xanh cả khi xoá hẳn HMAC (`phase-05`).
22. **Thứ tự anh em trong uGUI là logic chứ không phải trang trí.** Dựng màn đăng nhập sau `UiRoot` là hộp OTP của server nằm dưới nó — không thấy, không bấm được, không đăng nhập được (`phase-05`).
23. **Bảng đặc tả của chính plan cũng phải chịu wire.** Plan ghi "2 action map cho mobile và PC"; làm ra mới thấy chạm/chuột đã đi qua `EventSystem`, phần thiếu chỉ là bàn phím, và khác biệt nền tảng là "có bàn phím hay không" chứ không phải hai map. Hai map giống hệt nhau là nghi thức (`phase-05`).
24. **`IsConnected` phía client chỉ nói về tầng OS, không nói gì về việc code ứng dụng phía kia đã chạy tới đâu** — kể cả trong chính bộ test. Test tự viết đóng `TcpListener.AcceptTcpClientAsync()` (chạy bất đồng bộ) ngay sau khi thấy `client.IsConnected == true` mà không đợi accept thật sự xong; đóng "chưa có gì để đóng" thì test treo tới hết timeout. Chỉ lộ ra khi test THẬT SỰ CHẠY, không phải lúc compile-check (`phase-05.1`).
25. **Header trong header cũng là chỗ dễ sai.** Đếm số case trong `a.java` bằng `grep -c "case "` trên một dải dòng ước lượng đếm luôn dòng chọn ngôn ngữ (`case 0:`/`case 1:`), ra kết luận sai "134 EN thiếu 1 mục so với 135 VN". Bóc đúng phạm vi bằng công cụ thật cho ra 134 = 134. Số liệu ước lượng bằng mắt trên output `grep` không phải bằng chứng — chỉ tin kết quả từ công cụ đọc đúng cấu trúc (`phase-05.1`).
26. **Tìm chuỗi bằng nội dung không chứng minh được INDEX đúng.** Grep "T.Khoản" trên toàn bảng chuỗi ra đúng chữ nhưng sai chỉ số — không dòng nào trong hàm dùng thật (`fb.java`) gọi tới nó. Sửa hai lần cho cùng một nhãn trước khi lần theo đúng lời gọi `a.a(N)` trong chính hàm sử dụng. Bài học: đối chiếu chuỗi phải đi từ HÀM DÙNG NÓ, không phải từ bảng tổng (`phase-05.1`).
27. **Hai Canvas cùng `sortingOrder` mặc định thì thứ tự vẽ không được đảm bảo.** Tách một canvas mới (`PixelCanvas`) ra khỏi canvas chung mà không đặt `sortingOrder` tường minh là tái diễn đúng lỗi layering đã dính ở P5 (UiRoot/LoginScreens, ranh giới sibling) — chỉ chuyển sang ranh giới Canvas. Bài học lặp lại từ P5 không tự động lan sang ranh giới mới; phải áp dụng lại tường minh (`phase-05.1`).
28. **`AddComponent` trên GameObject đang active chạy `Awake`+`OnEnable` NGAY LẬP TỨC — gán field sau khi gọi là quá muộn.** `OnEnable` đọc field mặc định (null/0), không phải giá trị sắp được gán. Luôn `SetActive(false)` trước `AddComponent`, gán field, rồi `SetActive(true)` (`phase-05.1`).
