---
phase: 3
title: "Login & Handshake"
status: complete
priority: P1
effort: "1w"
dependencies: [2]
---

# Phase 3: Login & Handshake

## Overview

Handshake ứng dụng (`CLIENT_INFO`) → chọn server → đăng ký/đăng nhập → nhận `LOGIN_SUCCES`.

**Đây là mốc chứng minh khả thi.** Tới được đây thì phần còn lại là khối lượng, không còn là rủi ro kiến trúc.

## Requirements

**Functional**
- Gửi `CLIENT_INFO` đúng thứ tự field, qua được 2 cổng chặn của server
- Nhận và hiển thị danh sách server (`SERVER_LIST`)
- Đăng nhập, đăng ký, đổi mật khẩu
- Xử lý `LOGIN_FAILED`, dialog đỏ, 2FA OTP nếu tài khoản có bật

**Non-functional**
- Lưu thông tin đăng nhập an toàn (`PlayerPrefs` không đủ — dùng keystore/keychain trên mobile)

## Architecture

### Cổng chặn — sai là bị `session.Close()` không báo lỗi

Từ `Player.cs:104-129`, server đóng kết nối im lặng trong 2 trường hợp:

1. `languageCode` không có trong `GopetManager.Language` → `setClientOK(false)` + `Close()`
2. `ApplicationVersion < GopetManager.VERSION_142` → dialog đỏ + `Close()`
   > `VERSION_142 = Version.Parse("1.4.2")` (`GopetManager.cs:643`). Client cũ khai `1.4.3` nên qua được. Client Unity gửi `"1.4.3"` là an toàn.

Debug lần đầu sẽ gặp: "kết nối được rồi tự rớt, không có thông báo". Kiểm 2 chỗ này trước.

### `CLIENT_INFO` (opcode -36) — thứ tự bắt buộc

```
sbyte  CLIENT_TYPE
int    PROVIDER
UTF    version         -> phải >= VERSION_142
UTF    info
int    displayWidth
int    displayHeight
UTF    languageCode    -> phải khớp key trong GopetManager.Language
UTF    Refcode
```

Server trả về opcode `-36` với 1 byte: `1` = OK, `0` = từ chối.

Giá trị tham chiếu lấy từ `client.jar/META-INF/MANIFEST.MF`:
- `MIDlet-Version: 1.4.3`
- `ProviderId: 4`
- `RefCode: ref-mcb22qre14br-144706345912071136064`

### Opcode liên quan

| Opcode | Tên | Hướng |
|---|---|---|
| -36 | `CLIENT_INFO` | 2 chiều |
| 64 | `SERVER_LIST` | C→S yêu cầu, S→C danh sách |
| 1 | `LOGIN` | C→S: 4 UTF + 8 byte — xem "Wire thật" bên dưới, KHÔNG phải 3 UTF |
| 35 | `REGISTER` | C→S: `UTF username, UTF password` |
| 100 | `CHANGE_PASSWORD` | C→S: `int id, UTF oldPass, UTF newPass` |
| 3 | `LOGIN_SUCCES` | S→C |
| 4 | `LOGIN_FAILED` | S→C: `UTF text` |
| 103 | `CHECK_SPEED` | S→C ping, C→S phải trả lời |

### `LOGIN_SUCCES` (từ `GameController.cs:1576-1582`)

```
int    user_id
UTF    name
UTF    name        <- gửi 2 lần, đúng như vậy
UTF    serverIP
int    serverPort
```

### Ràng buộc đăng nhập phía server (`Player.cs:572-655`)

- `username` phải khớp regex `^[a-z0-9]+$` — chỉ thường + số
- Đăng ký: username và password đều `>= 6` ký tự, username `< 25`, password `< 60`
- Quá 10 lần thử/5 phút từ cùng IP → chặn
- `Ipv4Tracker`: 400 lần/phút mỗi IP
- Tài khoản có `secretKey` → server hỏi OTP 2FA qua `INPUT_OTP_2FA`

### Wire thật đã quan sát (2026-09-05)

Bắt được từ phiên `client.jar` gốc chạy trên emulator login vào server test (P1).
**Đây là sự thật trên dây**, không phải suy từ code — dùng nó làm chuẩn cho client Unity.

#### `CLIENT_INFO` thật — 109 byte

```
dc 00 00000004 0005 "1.4.3"
   004b "FreeJ2ME-Plus, a Cross-Platform J2ME Emulator.;CLDC-1.1;MIDP-2.0;null;gopet"
   00000140 000000f0 0002 "vi" 0005 "2.4.9"
```

**Bẫy đặt tên:** field thứ 8 server gọi là `Refcode` (`Player.cs:120`), nhưng client
thật gửi `"2.4.9"` vào đó — **không phải** refcode. Refcode thật
(`ref-mcb22qre14br-144706345912071136064`) đi trong gói `LOGIN`, không đi ở đây.

**`CLIENT_INFO` không thể byte-identical giữa hai client.** `info` là chuỗi nền tảng
(J2ME sinh từ system properties; Unity sẽ khác hẳn), và `displayWidth/Height` phụ thuộc
thiết bị. Nên khi đối chiếu gói này bằng packet-diff, **so cấu trúc chứ đừng so byte** —
độ dài lệch ở đây là bình thường, không phải desync.

Muốn giảm nhiễu thì đặt Unity **cùng 320×240** như emulator đang cấu hình.

#### `LOGIN` thật — 77 byte, KHÁC bảng opcode ở trên

```
01 0009 "gopettest" 0008 "abc12345"
   0026 "ref-mcb22qre14br-144706345912071136064"
   0005 "1.4.3"
   0000000000000000
```

Bốn UTF + 8 byte 0. Nhưng server chỉ đọc **ba** UTF:

```csharp
login(ms.reader().readUTF(), ms.reader().readUTF(), ms.reader().readUTF());
//    username                password                <-- server gọi là "version"
```

Tham số thứ ba mang tên `version` (`Player.cs:585`) nhưng thực nhận **refcode**, và
`login()` chỉ `.Trim()` nó rồi **không dùng vào đâu cả**. Chuỗi version thật `"1.4.3"`
cùng 8 byte cuối nằm lại trong buffer, không ai đọc.

Vô hại về chức năng, nhưng client Unity muốn diff sạch thì phải gửi **đủ cả 4 UTF + 8 byte**
y như client cũ. Gửi 3 UTF thì server vẫn cho đăng nhập, và lỗi sẽ chỉ lộ ra dưới dạng
"packet-diff báo lệch độ dài" — rất tốn công truy.

#### Thứ tự gói thật lúc login

```
C→S  -36  CLIENT_INFO          gửi liền nhau, KHÔNG đợi trả lời
C→S   64  xin danh sách server
S→C  -36  dc01                 chấp nhận client
S→C   64  "Localhost"/"127.0.0.1"
C→S    1  LOGIN
S→C   21  thành công
```

Client cũ bắn `CLIENT_INFO` và `SERVER_LIST` liên tiếp rồi mới đọc. Client Unity chờ
trả lời `-36` xong mới gửi tiếp cũng chạy, nhưng thứ tự trên dây sẽ khác → diff lệch.

#### Mã trả về khi login hỏng

| Tình huống | Server trả |
|---|---|
| Thành công, tài khoản đã có nhân vật | opcode **3** `LOGIN_SUCCES` |
| Thành công, tài khoản CHƯA có nhân vật | opcode **21** `CREATE_CHAR` — xem nhánh 2 bên dưới |
| Sai tài khoản/mật khẩu | opcode **4** `LOGIN_FAILED` (kèm chuỗi lý do) rồi `session.Close()` |
| `role = 0` | opcode **10** dialog đỏ, "Tài khoản chưa được kích hoạt" |
| Version cũ hơn 1.4.2 | opcode **10** dialog đỏ |
| `isShowMessageWhenLogin` bật | opcode **71** dialog thường (bao ngoài + sub `0`) |

> Ba opcode dễ lẫn: **3** là đăng nhập xong, **4** là `LOGIN_FAILED` (có chuỗi lý do),
> **10** là dialog đỏ chung (`Player.redDialog` ghi thẳng `(sbyte)10`, không có hằng số).
> Quan sát thấy opcode 21 sau khi đăng nhập **không** có nghĩa là thành công theo nghĩa
> vào được game — nó nghĩa là tài khoản chưa có nhân vật.

#### Gói cuối trước khi server đóng — đã sửa phía server

Từng có lỗi: server ghi log đã gửi opcode 10 ("Phiên bản cũ rồi…") rồi đóng, nhưng không
client nào nhận được. Nguyên nhân là `Session.Close()` ngắt luồng gửi trước khi nó kịp
đẩy hàng đợi, và `MsgSender.stop()` xoá sạch hàng chờ. **Đã sửa** — xem `phase-01`.

Nhưng client Unity **vẫn phải chịu được mất kết nối không kèm thông điệp**: mạng đứt,
tiến trình server chết, hay bất kỳ đường đóng nào chưa kịp drain. Đừng thiết kế luồng UI
dựa trên giả định luôn nhận được lý do trước khi rớt.

#### Tài khoản test — hai chỗ vướng

Không có đường tự đăng ký trong server test, phải tạo tay:

- Mật khẩu là **BCrypt work factor 12** (`Util/GopetHashHelper.cs`). Insert plaintext là
  đăng nhập không được, mà thông báo lại là "sai mật khẩu" nên rất dễ đi tìm nhầm chỗ.
- `role` mặc định `0` = `ROLE_NON_ACTIVE` (`Data/User/UserData.cs:20`) → "Tài khoản chưa
  được kích hoạt". Phải đặt `role = 1`.

Tài khoản đang dùng: `gopettest` / `abc12345`. Chi tiết ở `tools/README.md`.

#### Bộ gõ tiếng Việt làm hỏng ô nhập

Unikey/Telex chạy nền nuốt phím: gõ `test1234` ra `tét1234` (`e`+`s` → `é`). Đã mất thời
gian vì chuyện này khi nhập vào emulator, và **ô nhập của Unity sẽ dính y hệt**.

Khi test đăng nhập: chuyển sang chế độ gõ tiếng Anh, hoặc dùng mật khẩu không có chuỗi
kích hoạt dấu Telex (nguyên âm + `s/f/r/x/j`).

### Bốn nhánh chỉ lộ ra khi chạy thật (2026-09-05)

Không có trong bản plan gốc. Cả bốn đều làm client "kết nối được rồi rớt" hoặc
"đăng nhập mãi không xong" mà không một dòng lỗi nào.

#### 1. Server chặn kết nối lại 2 giây mỗi IP

`Server.cs:73` — kết nối thứ hai từ cùng một IP trong vòng 2 giây bị **accept ở tầng
TCP rồi đóng ngay**, không gửi gì cả:

```csharp
if (ConnectionWait.TryGetValue(clientIP, out var dateTime) && dateTime > DateTime.Now)
{
    client.Close();
    continue;
}
else ConnectionWait[clientIP] = DateTime.Now.AddSeconds(2);
```

Logic tự kết nối lại của client **phải** chờ ít nhất chừng đó, nếu không triệu chứng là
"kết nối được rồi rớt ngay" lặp vô hạn. Hằng số đã ghi ở `GopetSocket.ReconnectCooldownMs`.

#### 2. Tài khoản chưa có nhân vật → `CREATE_CHAR`, không phải `LOGIN_SUCCES`

Đăng nhập lần đầu, server trả opcode **21** (`sbyte 0, int 0, int 0`) thay vì `LOGIN_SUCCES`.
Client đáp lại `CREATE_CHAR` với `UTF name, sbyte gender` (tên `^[a-z0-9]+$`, 5-20 ký tự).

**Tạo xong server ĐÓNG kết nối** (`GameController.cs:740`) — phải kết nối lại và đăng nhập
lần nữa mới có `LOGIN_SUCCES`. Chờ trên kết nối cũ là chờ mãi.

#### 3. Đăng nhập trùng bị từ chối

Tài khoản đang online mà đăng nhập tiếp thì server trả "Người chơi khác đăng nhập vào tài khoản".
Nghĩa là **test tự động và thao tác tay phải dùng hai tài khoản khác nhau**, nếu không bài
test đỏ mỗi lúc emulator đang mở.

Đang dùng: `gopettest` cho thao tác tay, `gopetsmoke` cho harness tự động.

#### 4. Đăng ký bị khoá cứng phía server

`doRegister` mở đầu bằng `if (true) { redDialog("Chức năng này bị khóa..."); return; }`
(`Player.cs:191`). Server gốc bắt đăng ký qua web. Client vẫn phải gửi đúng gói, nhưng
tiêu chí "đăng ký thành công" **không áp dụng** với server này.

### `CHECK_SPEED` — phải trả lời

Server định kỳ gửi `CHECK_SPEED` kèm `int` mili-giây (`Player.cs:293`). Client **phải** trả lời. Không trả lời → `session.Close()` (`Player.cs:283`).
> Logic ban vì trả lời quá nhanh hiện đã bị comment (`Player.cs:320`) nên chỉ ghi log. Nhưng vẫn phải trả lời, nếu không sẽ bị ngắt.

## Related Code Files

**Create**
- `Assets/Scripts/Net/Auth/` — `ClientInfo`, `AuthPackets`, `AuthRules`, `ServerEntry`,
  `LoginSuccess`, `AuthHandler` (đã làm; gộp một thư mục thay vì `Handlers/` như dự kiến ban đầu)
- `Assets/Scripts/Auth/CredentialStore.cs` — lưu tài khoản an toàn theo nền tảng
- `Assets/Scripts/UI/LoginScreen.cs`
- `Assets/Scripts/UI/ServerSelectScreen.cs`

**Read for context**
- `SRCGOPETGOC/GServer/Server/Player.cs:100-169` — dispatch `CLIENT_INFO/LOGIN/REGISTER`
- `SRCGOPETGOC/GServer/Server/Player.cs:572-670` — `login()`, ràng buộc
- `SRCGOPETGOC/GServer/Server/Player.cs:276-330` — `checkSpeed()`
- `SRCGOPETGOC/GServer/Server/Player.cs:998-1020` — `showListServer()`
- `SRCGOPETGOC/GServer/Server/GameController.cs:1571-1589` — `loginOK()`

## Implementation Steps

1. **Gửi `CLIENT_INFO`** với giá trị lấy từ MANIFEST của client cũ. Xác nhận nhận lại byte `1`.
2. **Nếu bị đóng kết nối im lặng** — kiểm `languageCode` trước (đọc bảng `GopetManager.Language` trên server để lấy key hợp lệ), rồi tới version.
3. **`SERVER_LIST`** — gửi opcode 64, parse danh sách, hiển thị. Đọc `showListServer()` để biết format chính xác (có lọc theo version).
4. **Màn hình login** — input username/password, validate client-side theo đúng ràng buộc server (regex `^[a-z0-9]+$`, độ dài) để không tốn round-trip.
5. **Gửi `LOGIN`**, xử lý `LOGIN_SUCCES` và `LOGIN_FAILED`.
6. **`CHECK_SPEED`** — nhận, đọc `int`, rồi **CHỜ ĐÚNG chừng đó mili-giây mới trả lời**.
   `int` là hạn phải chờ, không phải thông tin tham khảo: `onClientSpeedRespose`
   (`Player.cs:331`) so `elapsed + 2s < waitMs` → coi là speed-hack. Cửa sổ hợp lệ là
   `[waitMs - 2s, 3 × waitMs)`. Không trả lời thì bị ngắt kết nối sau ~15-30 giây.
7. **Đăng ký** — opcode 35.
8. **2FA OTP** — nếu server gửi `INPUT_OTP_2FA` thì hiện dialog nhập. Có thể hoãn nếu tài khoản test không bật.
9. **`CredentialStore`** — Android Keystore / iOS Keychain / DPAPI trên Windows. Không lưu mật khẩu thô trong `PlayerPrefs`.
10. **Đối chiếu dump** — login bằng client cũ và client Unity với cùng tài khoản, diff 2 file. Chuỗi opcode phải giống hệt.

## Success Criteria

**Tầng giao thức — xong, kiểm chứng sống**

- [x] `CLIENT_INFO` được chấp nhận, server trả byte `1`
- [x] Nhận và **đọc** đúng danh sách server
- [x] Đăng nhập thành công, nhận đủ 5 field của `LOGIN_SUCCES`
- [x] Sai mật khẩu → đọc đúng thông báo từ `LOGIN_FAILED`
- [x] `CHECK_SPEED` được trả lời, kết nối giữ được > 5 phút không rớt
- [x] Gói `LOGIN` khớp **từng byte** với client cũ (4 UTF + 8 byte, không phải 3 UTF)
- [x] `CLIENT_INFO` khớp **từng byte** khi đặt cùng giá trị client cũ
- [x] Tạo nhân vật ở lần đăng nhập đầu, kết nối lại, vào được game

**Không áp dụng với server này**

- [~] Đăng ký tài khoản mới thành công — `doRegister` khoá cứng bằng `if (true)`
      (`Player.cs:191`). Client vẫn gửi đúng gói; kiểm chứng dừng ở dialog "bị khoá".

**Hoãn sang P5 (Generic UI Components)** — quyết định 2026-09-05

- [ ] Hiển thị danh sách server trên màn hình
- [ ] Mất kết nối không kèm thông điệp → có UI xử lý, không treo màn hình
- [ ] Thông tin đăng nhập lưu an toàn, không phải plaintext (`CredentialStore`)
- [ ] 2FA OTP

> Lý do hoãn: tầng giao thức đã chứng minh được không cần Editor. Còn màn hình login
> dựng bây giờ sẽ phải vứt khi P5 dựng hệ component dùng chung — và không tự nghiệm thu
> được, phải bấm Play bằng tay. Làm cùng P5 thì rẻ hơn và có asset của P4 để dùng.

**Còn lại, cần cả hai client cùng chạy**

- [ ] Diff dump login giữa client cũ và Unity: chuỗi opcode khớp

### Bằng chứng đã kiểm chứng (2026-09-05)

**Khớp từng byte với client J2ME** — hai unit test dựng gói bằng code Unity rồi so với
đúng hex bắt được từ phiên client cũ:

| Gói | Byte | Kết quả |
|---|---|---|
| `CLIENT_INFO` (đặt cùng giá trị client cũ) | 109 | khớp tuyệt đối |
| `LOGIN` | 77 | khớp tuyệt đối |

Vector tự bịa chỉ chứng minh code nhất quán với chính nó. Byte thật chứng minh nó nhất
quán với client phải đạt parity.

**Parser đọc gói thật**, không phải dữ liệu dựng tay:

- `SERVER_LIST` — `400000000100094c6f63616c686f7374...0101` → `Localhost (127.0.0.1:19180)`
- `LOGIN_SUCCES` — `03000005b200066b7a6864397800066b7a...` → `#1458 kzhd9x`

**Live smoke 15/15** (`tests/Gopet.Net.LiveSmoke`), chạy qua đúng đường Unity dùng
(`MessageRouter` + `AuthHandler`, rút hàng đợi trên luồng chính như `GopetClient.Update`):

| Check | Nội dung |
|---|---|
| G | `CLIENT_INFO` qua hai cổng chặn |
| H | Đọc được `SERVER_LIST` |
| I, J | Tài khoản mới → tạo nhân vật → kết nối lại → `LOGIN_SUCCES` đủ 5 field |
| K, L | Giữ kết nối **330 giây** không rớt, trả lời `CHECK_SPEED` |
| M | Gói `REGISTER` được server đọc (trả dialog "bị khoá") |

**Chuỗi opcode khớp giữa hai client.** Cùng một dump server, hai phiên:

```
J2ME :  IN -36 (109)   IN 64 (1)   IN 1 (77)
Unity:  IN -36 (39)    IN 64 (1)   IN 1 (54)
```

`packet-diff --opcodes-only` báo OK. Độ dài lệch là **có chủ đích** — `info` là chuỗi nền
tảng và refcode khác nhau; byte-parity đã chứng minh riêng ở unit test khi đặt cùng giá trị.
Cờ `--opcodes-only` thêm vào chính vì không có nó thì diff dừng ngay ở gói đầu và mất khả
năng kiểm điều thực sự cần kiểm.

**Unity Editor compile được.** `Library/ScriptAssemblies/Gopet.Net.dll` sinh lại lúc 22:16,
sau khi thư mục `Auth/` ra đời lúc 22:06 — Editor đã import và biên dịch, không chỉ
`netstandard2.1` giả lập.

### Lỗi nghiêm trọng code review bắt được (2026-09-05)

**`CHECK_SPEED` trả lời tức thì.** Bản đầu đọc `int` rồi vứt và trả lời ngay, comment còn
ghi "chỉ để tham khảo" — sai hẳn. `onClientSpeedRespose` (`Player.cs:331`) **đo thời gian**:
trả lời sớm hơn `waitMs - 2s` là rơi vào nhánh speed-hack. Nhánh đó `return` sớm nên
stopwatch nằm ở trạng thái đã dừng, và nhịp `checkSpeed` kế tiếp lại gửi gói mới.

Đo trên dump server, độ trễ giữa `OUT 81/5167` và `IN 81/5167`:

| Độ trễ | Số nhịp | Client |
|---|---|---|
| < 1 giây | **805** | Unity (bản lỗi) |
| 15-31 giây | 45 | J2ME emulator |

Tức khoảng 2 gói/giây thay vì 1 gói mỗi 15-30 giây. Trên server nào bỏ comment dòng
`user.ban(...)` (`Player.cs:334`) thì đó là khoá tài khoản một tiếng.

Sau khi sửa: 1 nhịp trong 40 giây, trả lời sau **29.04s**, 0 nhịp trả lời sớm.

**Vì sao 138 test và 14/14 live smoke đều mù.** Check L chỉ đòi `> 0` nhịp — trả lời tức
thì cũng xanh. Và `AuthHandler` là file duy nhất có logic hành vi lại là file duy nhất
không có unit test nào. Đã sửa cả hai: check L kẹp cả cận trên (`1..hold/15+1`), và thêm
`AuthHandlerTests` + `AuthRulesTests` (160 test).

Cách sửa: `AuthHandler.Tick()` gọi mỗi frame, hoãn trả lời tới đúng hạn. Không dùng Timer
hay luồng riêng — gói phải đi từ luồng chính như mọi thứ khác của giao thức.

**Ba lỗ hổng khác cùng lượt:**

- Harness có thể bỏ qua check I/J/K/L mà vẫn in "LIVE SMOKE OK" — `Report` chỉ đếm số
  check đỏ, không biết check nào đã không chạy.
- Thiếu handler `okDialog` (opcode **71**, bao ngoài + sub `0`). Bật `isShowMessageWhenLogin`
  là client đứng im không rõ lý do.
- `packet-diff` báo OK khi so 0 gói, và không validate `--direction` (viết thường thì lọc
  sạch → xanh giả). Cột `enc` chưa từng được so.

Báo cáo đầy đủ: `plans/reports/code-reviewer-260905-2202-phase-03-auth.md`

**Test tự kiểm.** Bốn phép, cả bốn đều bị bắt:

| Đột biến | Kết quả |
|---|---|
| Sai mật khẩu | check I đỏ, đúng thông điệp `LOGIN_FAILED` của server |
| Không chờ 2s giữa các kết nối | G/H/I đỏ — chính là cách phát hiện ra cơ chế chặn |
| Dùng chung tài khoản với emulator | I đỏ: "Người chơi khác đăng nhập vào tài khoản" |
| Sửa 1 opcode trong dump | `--opcodes-only` báo đúng gói #2 lệch |

## Risk Assessment

| Rủi ro | Xử lý |
|---|---|
| Bị đóng kết nối im lặng, không rõ lý do | Bật `DEBUG_LOG` trên server test — có sẵn `#if DEBUG_LOG` ở nhiều chỗ. Kiểm `languageCode` và version trước tiên |
| Quên trả lời `CHECK_SPEED` → rớt sau 15-30s | Làm ở bước 6, đừng hoãn. Triệu chứng dễ nhầm với lỗi mạng |
| Bị `Ipv4Tracker` chặn khi test nhiều lần | Server test: nới `TimeTracker` hoặc thêm whitelist IP dev |
| Tài khoản test có `secretKey` → kẹt ở 2FA | Dùng tài khoản không bật 2FA cho đến khi làm bước 8 |
| `LOGIN_SUCCES` gửi `name` 2 lần — tưởng đọc sai | Đúng như vậy (`GameController.cs:1578-1579`). Đọc đủ 2 lần |
| Đăng nhập fail mà nghi tầng giao thức | Kiểm DB trước: BCrypt-12 và `role != 0`. Cả hai đều báo ra cùng một dialog đỏ mơ hồ |
| Bộ gõ tiếng Việt làm hỏng ô nhập của Unity | Test bằng bàn phím tiếng Anh. Đã dính lúc nhập vào emulator |
| Diff `CLIENT_INFO` báo lệch rồi đi truy nhầm | `info` và độ phân giải vốn khác nhau giữa hai nền tảng. So cấu trúc, không so byte |

## Next Steps

Xong P3 → P4 (Remote Asset Pipeline). P5 (UI) cần asset để hiện icon nên P4 phải xong trước.
