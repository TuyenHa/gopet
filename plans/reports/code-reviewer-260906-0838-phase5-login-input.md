---
title: Code review — Phase 5 (login, credential store, input routing, bootstrap)
date: 2026-09-06
reviewer: code-reviewer
scope: Assets/Scripts/{UiLogic,Runtime}/**, Assets/Tests/PlayMode/**, tests/Gopet.Net.Tests/**
verdict: DONE_WITH_CONCERNS
---

# Code Review — Phase 5 remainder

## Scope

| | |
|---|---|
| Files đọc | 15 file sản phẩm + 8 file test (~1.700 dòng mới) |
| Đối chiếu | `GServer/Server/Player.cs`, `GameController.cs`, `MenuController.inputDialog.cs`, `LiveSmoke/LoginChecks.cs` |
| Không chạy | test (theo yêu cầu) — mọi kết luận từ đọc code + đối chiếu server |

**Lưu ý:** file đang được sửa TRONG LÚC review (09:14–09:18: `LoginFlow.cs` đổi,
`LoginFlow.Credentials.cs` → `LoginFlow.Account.cs`, thêm `LoginFlowHarness.cs` +
`LoginFlowRejectionTests.cs`). Mọi kết luận dưới đây đã kiểm lại với trạng thái **09:18**.
Nhánh `_rejection`/`_autoLogin` vừa thêm đã sửa đúng hai lỗi mà bản 09:04 mắc phải
(mất câu từ chối sau cú đóng, và tự đăng nhập lại thành vòng lặp đốt hạn mức 10 lần/5 phút).

## Đánh giá chung

Chất lượng trên mặt bằng: `LoginFlow` thuần C# tách khỏi view, mỗi quyết định khó đều có
comment trỏ về dòng server tương ứng, `InputRouterTests` bơm phím thật qua Input System
(đúng bài học #14), `MatKhau_KhongNamNguyenVanTrenDia` và `LoginFlowRejectionTests` là loại
test biết đỏ. Vấn đề còn lại nằm ở **ranh giới giữa các mảnh** — nơi test đơn vị của từng
mảnh mù: transport ↔ máy trạng thái, view ↔ view, client ↔ server ở nhánh 2FA.

---

## CRITICAL

### C1 — `GopetClient`: cú đứt của socket CŨ giết socket MỚI, thành vòng lặp không thoát

`Assets/Scripts/Runtime/GopetClient.cs:95-121, 169-182`

`Cleanup()` gọi `_socket.Dispose()`; `GopetSocket.Dispose` đóng stream → luồng đọc ném →
`Fail()` → `Disconnected?.Invoke` (`GopetSocket.cs:165-174`) → handler ghi
`_pendingDisconnectReason`. Cờ này **không được xoá** khi việc đóng là do chính ta chủ động.

Diễn biến khi `ChooseServer` chọn máy chủ có địa chỉ khác (`LoginFlow.cs:142-144` → `Connect`):

1. `Connect()` → `Cleanup()` → Dispose socket đang SỐNG → `_pendingDisconnectReason = "Lỗi luồng đọc: …"`, `_connectAllowedAt = now + 2.5s`.
2. Các frame kế: `_socket == null` → `Update` **return sớm ở dòng 102-106**, không ai tiêu thụ cờ.
3. Sau 2.5s: `OpenSocket()` mở socket mới, bắn `Connected` → flow gửi `CLIENT_INFO`.
4. **Cùng frame đó**, dòng 117 đọc phải cờ CŨ → `Cleanup()` giết socket vừa mở → `Disconnected` → flow báo mất kết nối / nối lại.
5. Bước 4 lại Dispose một socket đang sống → sinh cờ mới → **quay lại bước 2**. Vòng lặp tự nuôi, chu kỳ 2.5 giây, không bao giờ vào được game.

Vì sao CI và live smoke không thấy: `LoginChecks` chưa bao giờ đi nhánh "chọn máy chủ rồi nối
lại", và trong dev `SERVER_LIST` trả về đúng `127.0.0.1` nên `LoginFlow` đi nhánh tắt "cùng
địa chỉ thì giữ nguyên kết nối". Trên server thật `showListServer` (`Player.cs:1016-1044`) gửi
`serverInfo.IpAddress` lấy từ cấu hình → **mọi lần đăng nhập production đều rơi vào nhánh này**.

Sửa (cả ba, không chỉ một):

```csharp
private void Update()
{
    // 1. Tiêu thụ lý do TRƯỚC khi mở socket mới, và KHÔNG nằm sau return sớm.
    var reason = _pendingDisconnectReason;
    if (reason != null)
    {
        _pendingDisconnectReason = null;
        Cleanup();
        Disconnected?.Invoke(reason);
    }

    if (_connectPending && Time.unscaledTime >= _connectAllowedAt) OpenSocket();
    ...
}

private void Cleanup()
{
    if (_socket == null) return;
    _socket.Dispose();
    _socket = null;
    _pendingDisconnectReason = null;   // 2. cú đóng do CHÍNH TA gây ra không phải sự kiện để báo lên
    _connectAllowedAt = ...;
}
```

3. Chắc hơn nữa: buộc lý do vào đúng instance sinh ra nó —
`var s = new GopetSocket(_logger); s.Disconnected += r => { if (ReferenceEquals(s, _socket)) _pendingDisconnectReason = r; };`

**Test hồi quy (đang thiếu hoàn toàn):** không có một test nào chạm `GopetClient` — mà đây là
file chứa toàn bộ logic hoãn-nối + cooldown. Thêm PlayMode `[UnityTest]`: dựng `TcpListener`
thật trên hai cổng, `Connect(A)` → chờ `Connected`, `Connect(B)` → bơm frame ~3.5 giây →
assert `IsConnected == true` và `Disconnected` **không** bắn lần nào. Test này đỏ với code hiện tại.

---

## HIGH

### H1 — Hộp OTP 2FA bị màn đăng nhập che kín và chặn luôn cú chạm

`GopetBootstrap.cs:60,66` + `LoginScreens.cs:99-112` + `FormView.cs:52`

`UiRoot` tạo TRƯỚC `LoginScreens` dưới cùng một Canvas → uGUI vẽ sibling sau đè lên sibling
trước → **LoginScreens luôn nằm trên UiRoot**. Ở chặng `LoggingIn`, `ShowMessage("Đang kết nối…")`
dựng một `FormView` trải kín màn hình, và `FormView.Create` gắn `Image` (sprite null = ô trắng
đục, `raycastTarget = true`).

Server hỏi OTP bằng `showInputDialog(INPUT_OTP_2FA, …)` (`Player.cs:400-403`) đúng lúc stage là
`LoggingIn` → `InputDialogView` được dựng làm con của `UiRoot` → **vừa bị che vừa bị chặn
raycast**. Tài khoản bật 2FA không đăng nhập được, và triệu chứng là "treo ở Đang kết nối".

Doc comment của `LoginScreens` khẳng định ngược lại ("nó nằm đè lên màn đăng nhập") — claim này
sai với thứ tự sibling thực tế.

Sửa: `_ui.transform.SetAsLastSibling()` sau khi tạo `LoginScreens` (hoặc đảo thứ tự tạo), chắc
hơn thì cho UiRoot một `Canvas` riêng với `sortingOrder` cao hơn. Test PlayMode: ở chặng
`LoggingIn` bơm gói `TYPE_DIALOG_INPUT` qua `TestPackets`, rồi assert bằng raycast thật
(`GraphicRaycaster.Raycast` tại tâm màn hình trúng InputField của dialog, không phải Image của form).

### H2 — Sai OTP: client gửi lại `LOGIN` trên kết nối mà server đã câm

`LoginFlow.Account.cs:44-51` + test `LoginFlowRejectionTests.SaiOtp_KhongDongKetNoi_ThiGuiLaiDuocNgay`

Server, khi OTP sai, **chỉ gửi dialog đỏ** và không hiện lại hộp nhập
(`MenuController.inputDialog.cs:729-731`). `player.user` vẫn khác null, nên lần `LOGIN` thứ hai
trên cùng kết nối bị chặn ngay dòng đầu:

```csharp
// Player.cs:587-588
if (this.user != null && !IsPassOtp) return;
```

Server **không trả lời gì cả**. Client ở lại `LoggingIn` → màn "Đang kết nối…" **không có nút
nào** → treo vĩnh viễn, chỉ thoát bằng cách tắt app. Đúng loại lỗi mà nguyên tắc #9 và #11 nói tới.

Test mới `SaiOtp_KhongDongKetNoi_ThiGuiLaiDuocNgay` đang **khẳng định chính cái hành vi hỏng
này** (assert stage = `LoggingIn` sau khi gửi lại) — nó xanh trong khi wire thì im lặng.

Sửa: sau mỗi lần bị từ chối, lần đăng nhập kế tiếp phải đi trên **kết nối mới**. Nhánh sai mật
khẩu dù sao cũng đã bị server đóng, nên quy tắc này đúng cho cả hai kiểu:

```csharp
public bool SubmitCredentials(string username, string password)
{
    ...
    _username = username; _password = password ?? string.Empty;
    if (_rejection != null) { _rejection = null; _autoLogin = true; Connect(); return true; }
    _rejection = null;
    SendLogin();
    return true;
}
```

Sửa test thành: "gửi lại sau khi sai OTP thì phải NỐI LẠI trước, không gửi LOGIN trên kết nối
cũ" (`Connects.Count` tăng, `CountSent(LOGIN)` không tăng cho tới khi bắt tay xong).
Lưu ý: bản sửa này gọi `Connect()` khi socket đang sống → **phải sửa C1 trước**, nếu không nó
rơi thẳng vào vòng lặp của C1.

### H3 — Không có đồng hồ canh: mọi lần "gửi mà không ai trả lời" đều treo màn hình

`GopetClient.cs:83-92` + `LoginScreens.cs:70-76`

`Send()` khi chưa nối chỉ `Debug.LogWarning` rồi `Dispose()` gói — **người gọi không biết gói đã
rơi**. `LoginFlow` vẫn `Enter(LoggingIn)`, và màn `LoggingIn` không có nút nào. Kịch bản thật:
server đóng sau `LOGIN_FAILED` (`Player.cs:655-658`), người chơi gõ lại và bấm trong khoảng 1
giây đó → gói rơi → sự kiện `Disconnected` đã tiêu thụ xong → không còn sự kiện nào đánh thức
UI → treo.

Sửa, nên làm cả hai:
- `Send` trả `bool` hoặc bắn `SendFailed`; bootstrap nối vào `_flow.OnDisconnected("Chưa kết nối")`.
- Đồng hồ canh trong `LoginFlow`, đập nhịp từ `GopetClient.Ticked`: ở `Connecting`/`Handshaking`/`LoggingIn` quá ~20 giây → `Enter(Disconnected, "Máy chủ không trả lời.")`. Cái này chặn được cả H2, C1 và mọi nhánh chưa nghĩ tới — hiện KHÔNG có giới hạn thời gian nào ở tầng UI.

### H4 — Tên nhân vật trùng: người chơi không bao giờ biết vì sao, và kẹt vòng lặp

`LoginFlow.Account.cs:53-67` + `LoginFlow.cs:148-156` + `GameController.cs:723-730`

Server: tên trùng → `redDialog(DuplicateNameChar)` → `Sleep(1000)` → `session.Close()`.

Client: dialog → `OnLoginRejected` (Stage = `CreatingCharacter`) → `_rejection` được nhớ,
Enter(`EnteringCredentials`) hiện câu ~1 giây. Rồi cú đóng tới → `_expectingCloseAfterCreate`
còn true → `_autoLogin = true` → nối lại → tự đăng nhập → server gọi `createChar` →
`OnCharacterRequired()` → `Enter(CreatingCharacter, **null**)` → **câu "tên đã có người dùng"
bị xoá**. Người chơi thấy y hệt màn tạo nhân vật ban đầu, gõ lại đúng cái tên đó, lặp vô hạn.
Client không kiểm trước được vì chỉ server biết tên trùng (`AuthRules` đã ghi đúng điều đó).

Sửa gọn nhất — mang câu từ chối qua chặng tạo nhân vật:

```csharp
public void OnCharacterRequired()
{
    var reason = _rejection;
    _rejection = null;
    Enter(LoginStage.CreatingCharacter, reason);
}
```
và `LoginScreens.ShowCreateCharacter` dùng `_flow.Notice` khi khác null, chỉ rơi về câu hướng
dẫn mặc định khi null (hiện đang ghi đè vô điều kiện — `LoginScreens.cs:118`).

Kèm theo: `_expectingCloseAfterCreate` chỉ được xoá ở `OnDisconnected`. Nếu `CREATE_CHAR` rơi
(H3) hoặc server từ chối mà giữ kết nối, cờ này ở lại **mãi mãi** và nuốt cú rớt mạng thật kế
tiếp. Xoá nó trong `OnLoginRejected` (mọi phản hồi lúc `CreatingCharacter` nghĩa là lần tạo đó
không thành).

### H5 — `CredentialStore` chỉ bắt lỗi I/O; lỗi crypto thoát ra và chặn luôn đường vào game

`CredentialStore.cs:78-98` + `LoginScreens.cs:94-96`

Doc của class hứa "hỏng ở bất kỳ khâu nào đều quy về chưa lưu gì". Bộ lọc thật chỉ có
`IOException | UnauthorizedAccessException | NotSupportedException`. `SecretBox.Seal` có thể ném
`CryptographicException`, và trên **IL2CPP + managed stripping**, `Aes.Create()` (tra tên qua
`CryptoConfig`, tức reflection) là chỗ nổi tiếng trả null / ném sau khi build player — Editor và
xUnit chạy Mono nên không bao giờ thấy.

Hậu quả nặng hơn "không lưu được": `OnStageChanged(Ready)` gọi `Remember()` **trước**
`Finished?.Invoke()` (`LoginScreens.cs:94-96`). Exception ở đó bay ra khỏi `StageChanged` →
`Finished` không bao giờ bắn → **đăng nhập đúng nhưng không vào được game**, chỉ vì tiện ích
nhớ mật khẩu.

Sửa: (1) đảo thứ tự — `Finished?.Invoke()` trước, `Remember()` sau; (2) `Save`/`Load` bắt
`Exception` rộng (hoặc thêm `CryptographicException`, `ArgumentException`); (3) giữ
`System.Security.Cryptography.Algorithms` trong `link.xml` và xác nhận trên một build IL2CPP
thật — hạng mục "chưa kiểm trên máy thật" cùng nhóm với đo fps.

---

## MEDIUM

### M1 — `SecretBox`: comment nói so sánh hằng thời gian, code thoát sớm

`SecretBox.cs:96-101`

```csharp
// So sánh hết mảng dù đã lệch: thoát sớm là rò thời gian.
if (sealedBytes[bodyLength + i] != expected[i]) return false;   // ← thoát ngay ở byte lệch
```
Rủi ro thực tế ≈ 0 (file cục bộ, không có oracle từ xa) nhưng comment nói sai về một thuộc tính
bảo mật còn hại hơn không có comment: người sau sẽ tin nó. Sửa code
(`diff |= a[i] ^ b[i]` rồi `return diff == 0`) hoặc sửa comment cho đúng phạm vi đe doạ đã tuyên bố.

### M2 — `SecretBoxTests.SuaMotByte_BiPhatHien` không phân biệt được có HMAC hay không

`tests/Gopet.Net.Tests/SecretBoxTests.cs:37-48`

Plaintext `"abc12345"` → ciphertext đúng **một** block. Lật `bytes[20]` (nằm trong block đó) làm
padding hỏng → `TransformFinalBlock` ném → `TryOpen` trả false **kể cả khi bỏ hẳn khối kiểm MAC**.
Xác suất test đỏ khi xoá MAC ≈ 0,4%. Đúng bẫy nguyên tắc #6.

Hai ca phân biệt được:
- Lật một byte trong **block ciphertext đầu tiên** của plaintext dài ≥ 32 byte: CBC làm hỏng block
  đầu, padding ở block cuối vẫn hợp lệ → không có MAC thì `TryOpen` trả **true** kèm chuỗi rác.
- Lật một byte trong **32 byte MAC cuối**: không kiểm MAC thì giải mã vẫn ra đúng plaintext → true.

### M3 — Một khoá dùng cho cả AES lẫn HMAC

`SecretBox.cs:44-66`. Không khai thác được với hai primitive khác nhau, nhưng lệch chuẩn. Tách
khoá con từ cùng `device.key`: `enc = SHA256(key || 0x01)`, `mac = SHA256(key || 0x02)`. Rẻ, và
làm ngay bây giờ thì chưa ai có file cũ để phải di trú.

### M4 — `ManTaoNhanVat_HaiNutLaHaiGioiTinh` không thể đỏ

`Assets/Tests/PlayMode/LoginScreensTests.cs:137-151`

Bấm nút "Nữ" rồi assert `Stage == CreatingCharacter` — nhưng `SubmitCharacter` **không đổi stage
bao giờ**. Test xanh kể cả khi: không gửi `CREATE_CHAR`, gửi giới tính sai, hay nuốt luôn cú bấm
nút thứ hai. Đúng bài học #13.

Sửa: nối `_flow.SendRequested`, assert đã gửi `CREATE_CHAR` **và** byte cuối payload = 1 khi bấm
nút 1, = 0 khi bấm nút 0.

### M5 — Ánh xạ giới tính 0 = Nam chưa được kiểm chứng ở đâu

`LoginScreens.cs:120-127`. Server nhận `sbyte` rồi ghi thẳng vào DB (`GameController.cs:737`,
`PlayerData.create`), không validate; dấu vết duy nhất trong source là
`gender == 0 ? "34_icon.png" : "33_icon.png"` (`GameController.cs:4756`). Comment "đúng thứ tự
server đọc" không có gì đỡ. Đặt nhầm là lỗi **vĩnh viễn với người chơi** (không đổi giới tính
được). Kiểm bằng emulator J2ME tạo nhân vật rồi soi `packet-dump-server.log`, hoặc đọc hàng
`player` trong DB — rồi ghi kết quả vào comment.

### M6 — Mật khẩu luôn được lưu; đường "không cho nhớ" không nối vào UI nào

`GopetBootstrap.cs:28-29`, `LoginScreens.Account.cs:47-51`

`rememberAccount = true` mặc định, và khi có store thì `Remember()` **luôn** lưu cả mật khẩu.
`SavedCredentials.HasPassword` và nhánh `Save(user, null)` chỉ sống trong unit test — không người
chơi nào chọn được. Trên máy dùng chung đây là mặc định đáng bàn, nhất là khi `SecretBox` đã tự
nhận chỉ chống đọc lướt.

Đề xuất: một ô "Nhớ mật khẩu" trên `FormView` (hoặc nút thứ ba), mặc định TẮT cho mật khẩu, BẬT
cho tên tài khoản. Và `Remember()` hiện **không có test nào** — đây là đường ghi bí mật xuống
đĩa, nên có một PlayMode test dùng `CredentialStore` trỏ vào thư mục tạm.

### M7 — Packet log bật mặc định trong build, và giờ sống cả phiên

`GopetClient.cs:29-30, 127-140`

`enablePacketLog = true` mặc định; `PacketLogger` ghi payload **trước** khi TEA mã hoá, tức mật
khẩu dạng rõ, vào `Application.persistentDataPath` — trên Android là thư mục app đọc được bởi bất
kỳ ai có adb/root, tồn tại đến khi gỡ app. Thay đổi lần này (logger sống cả phiên thay vì mỗi kết
nối) làm file lớn hơn và lâu hơn.

Đề xuất: `#if DEVELOPMENT_BUILD || UNITY_EDITOR` quanh khối tạo logger, hoặc che payload của
opcode `LOGIN`. Giữ mặc định bật ở Editor — nguyên tắc #3 vẫn đúng, chỉ là đừng ship.

### M8 — `FormView.Bind` huỷ hàng cũ mà không tắt trước

`FormView.cs:70-80` vs `LoginScreens.cs:146-152`

`LoginScreens.Discard` đã học bài `SetActive(false)` trước `Destroy` (Destroy chỉ có hiệu lực cuối
frame, trong khoảng đó view cũ vẫn ăn click). `FormView.Bind` thì `Destroy` thẳng, và hàng mới đặt
chồng đúng vị trí hàng cũ. Hiện `Bind` chỉ được gọi một lần ngay sau `Create` nên chưa nổ; nó sẽ
nổ đúng ngày ai đó gọi `Bind` lần hai. Test `BindLai_KhongGiuLaiOCu` chỉ đếm `Fields.Count` nên
không thấy — thêm assert `oldButton.gameObject.activeSelf == false`.

### M9 — Enter đi thẳng vào form đăng nhập kể cả khi dialog của server đang ở trên

`GopetBootstrap.cs:112-117`. Cùng gốc với H1: khi `_ui.Current != null`, phím Enter nên thuộc về
dialog trên cùng, không phải `_login.Form`. Hiện vô hại chỉ vì màn `LoggingIn` có
`DefaultButton = -1`; đổi một dòng ở `ShowMessage` là thành bấm nhầm nút. Chặn:
`if (_ui.Current != null) return;` trong handler `Submitted`.

---

## LOW

- **L1** `FpsSampler.OnePercentLowFps` (`FpsSampler.cs:60-72`): `floor(N*0.99) == N` với mọi
  `N ≤ 100` → trả về **frame chậm nhất**, không phải phân vị 99 như doc nói. Test
  `MotPhanTramThap_BatDuocCuKhung` (đúng 100 mẫu) khoá luôn hành vi "max". Với `N = 200` nó lại
  thành phân vị thật, tức số đo đổi ý nghĩa theo độ dài phép đo. Hoặc đổi doc, hoặc lấy **trung
  bình 1% mẫu chậm nhất** (định nghĩa thường dùng) và sửa test theo.
- **L2** `SubmitCredentials` không chặn bấm hai lần: gửi 2 gói `LOGIN`; gói thứ hai server bỏ qua
  (`Player.cs:587`) nhưng vẫn thêm một dòng `login_history` — mà 10 dòng/5 phút là ngưỡng khoá.
  Chặn bằng `if (Stage == LoginStage.LoggingIn) return false;`.
- **L3** `Finished` chỉ `Debug.Log` (`GopetBootstrap.cs:68`): sau khi đăng nhập xong màn hình
  trắng trơn. Chấp nhận được ở P5 (P6 mới có map), nhưng nên hiện một dòng chữ tạm.
- **L4** `FormViewTests.Click` gọi `ExecuteEvents.Execute` thẳng lên GameObject → bỏ qua raycast.
  Bài kiểm chiều rộng đã bù phần lớn rủi ro, nhưng ít nhất một ca nên đi qua `GraphicRaycaster`
  thật như `PointerPathTests`.
- **L5** `LoginFlow.cs` đã 195/200 dòng. Lần thêm nhánh sau là phải tách file — cân nhắc tách
  "nối/ngắt/thử lại" khỏi "chọn máy chủ" ngay bây giờ.

---

## Đối chiếu giao thức (mục 1 của yêu cầu)

| Điểm | Server | Client | Kết luận |
|---|---|---|---|
| Thứ tự nối → CLIENT_INFO → SERVER_LIST → LOGIN | `LoginChecks` chạy thật | `LoginFlow` | Khớp |
| Đóng kết nối sau `CREATE_CHAR` | `GameController.cs:737-740` | `_expectingCloseAfterCreate` | Khớp; còn thiếu chỗ xoá cờ (H4) |
| Cooldown 2 giây/IP | `Server.cs:73`, `GopetSocket.ReconnectCooldownMs` | `_connectAllowedAt` + 0.5s biên | Đúng ý, nhưng bị C1 vô hiệu |
| 2FA là `TYPE_DIALOG_INPUT` thường | `Player.cs:400-403` | không dựng màn riêng | Kiến trúc đúng, nhưng bị che (H1) và không gửi lại được (H2) |
| `CREATE_CHAR` = UTF + sbyte | `GameController.cs:348` | `AuthPackets.CreateCharacter` | Khớp byte |
| Ràng buộc tên nhân vật | `GameController.cs:718-721` regex + 5..20 | `AuthRules.IsValidCharacterName` | Khớp 1:1 |
| Sai mật khẩu → `LOGIN_FAILED` + đóng | `Player.cs:655-658` | `_rejection` + nối lại | Khớp (vừa sửa xong) |
| Sai OTP → dialog đỏ, GIỮ kết nối, `user != null` | `inputDialog.cs:729-731`, `Player.cs:587` | gửi lại `LOGIN` trên kết nối cũ | **Sai** (H2) |

## Test không thể đỏ (mục 2)

1. `SecretBoxTests.SuaMotByte_BiPhatHien` — M2, xanh kể cả khi bỏ hẳn HMAC.
2. `LoginScreensTests.ManTaoNhanVat_HaiNutLaHaiGioiTinh` — M4, không assert gì về gói gửi đi.
3. `LoginFlowRejectionTests.SaiOtp_KhongDongKetNoi_ThiGuiLaiDuocNgay` — H2, khoá đúng hành vi mà
   server không đáp lại.
4. `FormViewTests.BindLai_KhongGiuLaiOCu` — M8, không thấy hàng cũ còn sống hết frame.
5. Không có test nào cho `GopetClient` (C1) và `LoginScreens.Remember` (M6) — hai chỗ vừa thay đổi
   nhiều nhất.

## Vòng đời Unity (mục 3)

Đã kiểm, **không có vấn đề mới**: `Destroy` hoãn cuối frame (đã xử ở `Discard`, còn sót ở `Bind` —
M8); `?.` trên `UnityEngine.Object` (bootstrap dùng `!= null` đúng, có comment giải thích); huỷ
đăng ký sự kiện (`LoginScreens.OnDestroy`, `RemoteAssetCache`, `InputRouter.OnDisable/OnDestroy`
đều đủ); `Time.unscaledTime` dùng đúng chỗ (không bị `timeScale` ảnh hưởng); khởi tạo hai lần đã
chặn bằng exception ở cả `UiRoot` lẫn `LoginScreens`; `OnApplicationQuit` → `OnDestroy` chịu được
gọi hai lần (`_disposed` interlocked, `_logger` set null). `Connected` bắn từ `Update` nên mọi
callback đều ở luồng chính — đúng luật thread của `GopetSocket`.

## Điểm tốt

- `LoginFlow` thuần C#, không `UnityEngine` → nhánh khó nhất của phase test được ngoài Editor.
- Mỗi quyết định lạ đều có comment trỏ về dòng server tương ứng — review được mà không phải mở song song hai repo.
- `InputRouterTests` bơm phím vào `Keyboard` ảo và `Esc_DongManHinhTrenCung` đi trọn đường thật (#14).
- `MatKhau_KhongNamNguyenVanTrenDia` là loại test biết đỏ, và comment nói rõ vì sao round-trip test không đủ.
- `LoginFlowHarness` gom "đăng nhập bình thường trông thế nào" về một chỗ (#8).
- Thêm `Gopet.PlayModeTests.asmdef` đóng đúng lỗ hổng của nguyên tắc #12.
- `Cleanup()` đặt cooldown ở MỌI đường ngắt, không chỉ đường tạo nhân vật.

## Hành động đề xuất, theo thứ tự

1. **C1** — sửa `GopetClient` (xoá cờ trong `Cleanup`, tiêu thụ lý do trước khi mở socket) + PlayMode test hai listener. Chặn phase nếu chưa xong.
2. **H1** — thứ tự sibling UiRoot/LoginScreens + test raycast ở chặng `LoggingIn`.
3. **H2** — bắt buộc nối lại trước lần đăng nhập thứ hai; sửa test OTP theo hành vi server.
4. **H3** — đồng hồ canh cho `Connecting/Handshaking/LoggingIn`; `Send` báo được thất bại.
5. **H4** — mang `_rejection` qua `OnCharacterRequired`; xoá `_expectingCloseAfterCreate` trong `OnLoginRejected`.
6. **H5** — đảo `Finished`/`Remember`, nới bộ lọc exception, `link.xml` cho crypto.
7. **M2, M4** — sửa hai test không thể đỏ (rẻ, làm cùng lượt).
8. M1, M3, M5–M9 rồi L1–L5.

## Câu hỏi chưa trả lời

- `serverInfo.IpAddress` trên production trỏ về đâu — có bao giờ trùng host bootstrap không? Nếu luôn trùng thì C1 hạ xuống High, nhưng vẫn phải sửa vì `SERVER_LIST` do cấu hình quyết định.
- Giới tính 0 là Nam hay Nữ (M5) — cần một lượt tạo nhân vật bằng emulator J2ME để chốt.
- `Aes.Create()` có sống qua IL2CPP + stripping mức mặc định của project không (H5) — chỉ build player thật mới trả lời được.
