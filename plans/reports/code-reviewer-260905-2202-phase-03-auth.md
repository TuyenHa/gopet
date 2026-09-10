# Code Review — Phase 3 (Auth / Login)

Ngày: 2026-09-05 22:02 · Reviewer: code-reviewer
Phạm vi: `Assets/Scripts/Net/Auth/*` (6 file, 453 dòng), `tests/Gopet.Net.Tests/AuthTests.cs`,
`tests/Gopet.Net.LiveSmoke/{LoginChecks,MessagePump,RegisterCheck}.cs`,
`tools/packet-diff/index.js` (`--opcodes-only`).

Đã kiểm chứng lại: `dotnet build` UnityCompat (netstandard2.1 / C#9) → 0 warning, 0 error.
`dotnet test` Gopet.Net.Tests → 138/138 pass. Mọi file < 200 dòng.

## Đánh giá chung

Parser đúng từng field so với server — đối chiếu tay `Player.cs:104-134/1035-1050`,
`GameController.cs:714-740/1584-1589`: **không có chỗ nào đọc thiếu, thừa hay sai thứ tự.**
Hai câu hỏi khó nhất trong phần này (port lặp 3 lần, tên gửi 2 lần) đều xử lý đúng và có
test bằng byte thật. Kiến trúc event-only, không đụng UnityEngine, đúng ý.

**Nhưng có một lỗi hành vi nghiêm trọng mà cả 138 unit test lẫn 14/14 live smoke đều xanh:**
client trả lời `CHECK_SPEED` **ngay lập tức**, trong khi client J2ME gốc chờ đúng số
mili-giây server gửi. Byte trên dây giống hệt nhau — chỉ khác thời điểm — nên mọi phép
kiểm hiện có đều mù với nó.

---

## CRITICAL

### C1. `CHECK_SPEED` trả lời tức thì → bị server coi là speed-hack + flood 2 gói/giây

`Assets/Scripts/Net/Auth/AuthHandler.cs:100-109`

```csharp
private void OnCheckSpeed(Message m)
{
    // Server gửi kèm số mili-giây nó sẽ chờ, nhưng chỉ để tham khảo:
    // onClientSpeedRespose() không đọc gì từ gói trả lời.
    m.Reader.ReadInt();          // <-- đọc rồi VỨT
    m.Reader.ExpectFullyConsumed("CHECK_SPEED");
    _send(AuthPackets.CheckSpeedReply());   // <-- trả lời ngay
}
```

Comment nói sai. `onClientSpeedRespose()` đúng là không đọc thân gói trả lời, nhưng nó
**đo thời gian** (`Player.cs:330-336`):

```csharp
s_SpeedStopWatch.Stop();
if (s_SpeedStopWatch.Elapsed + TimeSpan.FromSeconds(2) < m_SpeedTime)
{
    //user.ban(UserData.BAN_TIME, "HackSpeed", ... + 1 giờ);
    //session.Close();
    HistoryManager.Instance.add(... "Hệ thống ban acc do người dùng speed" ...);
    return;                      // <-- thoát sớm, stopwatch KHÔNG chạy lại
}
```

`int` server gửi (`Player.cs:307`) **là hạn client phải chờ**, không phải thông tin tham khảo.

**Bằng chứng — dump của chính server (`GServer/bin/Debug/net8.0/log/packet-dump-server.log`),
độ trễ OUT `5167` → IN `5167`:**

| Độ trễ trả lời | Số lần | Client |
|---|---|---|
| 0.00–0.04 s | **802** | Unity (phiên soak hôm nay) |
| 15.0–30.0 s | ~45 | J2ME emulator |

Ba mẫu của J2ME khớp chính xác giá trị server gửi:
`0x5dc0`=24000ms → trả lời sau 24.08s · `0x55f0`=22000ms → 22.07s · `0x4268`=17000ms → 17.03s.
J2ME chờ `waitMs` rồi mới trả lời, sai số ~30-80ms.

**Hậu quả (đã quan sát được, không phải suy đoán):**

1. Mỗi lần trả lời đều rơi vào nhánh "speed hack" → mỗi nhịp đẩy một bản ghi vào
   `HistoryManager`. Lệnh `user.ban(...1 giờ)` + `session.Close()` ở build này **đang bị
   comment**; bất kỳ build nào bỏ comment (hoặc server production gốc) sẽ **ban tài khoản
   và ngắt kết nối**.
2. Nhánh đó `return` sớm nên stopwatch đứng yên → tick 500ms sau (`Data/map/GopetMap.cs:91`
   → `Place.cs:54` → `Player.cs:273`) thấy `IsRunning == false` và gửi `CHECK_SPEED` mới.
   Vòng lặp chặt: **2 gói/giây/kết nối**. Đo được **850 gói `CHECK_SPEED` trong 29 phút**
   một phiên soak, so với ~45 gói của emulator cùng khoảng thời gian — gấp ~20 lần.
3. Vi phạm ràng buộc "không đổi một byte nào so với client J2ME" ở mức hành vi: chạy
   `packet-diff --opcodes-only` giữa hai dump login sẽ lệch ngay ở gói 81 đầu tiên vì số
   lượng khác hẳn.

**Cửa sổ hợp lệ của server:** `[m_SpeedTime − 2s, 3 × m_SpeedTime)` — dưới cận dưới là
speed-hack (`Player.cs:332`), quá cận trên là `session.Close()` (`Player.cs:296-299`).

**Cách sửa** (giữ netstandard2.1, không UnityEngine, vẫn test được ngoài Editor — đừng
dùng `Timer`/`Thread` trong `AuthHandler`, hãy để luồng chính bơm như phần còn lại):

```csharp
// AuthHandler.cs
private const int MinWaitMs = 1000;      // chặn giá trị dị làm trả lời tức thì
private const int MaxWaitMs = 120000;    // ...hoặc treo vĩnh viễn
private bool _speedReplyPending;
private long _speedReplyDueMs;

private void OnCheckSpeed(Message m)
{
    // Server ĐO thời gian tới lúc ta trả lời (Player.cs:330-336). Trả lời ngay =
    // rơi vào nhánh speed-hack + server gửi lại nhịp mới sau 500ms => flood.
    // Client J2ME chờ đúng waitMs rồi mới trả lời (đo từ dump: waitMs + 30..80ms).
    var waitMs = m.Reader.ReadInt();
    m.Reader.ExpectFullyConsumed("CHECK_SPEED");

    if (waitMs < MinWaitMs) waitMs = MinWaitMs;
    if (waitMs > MaxWaitMs) waitMs = MaxWaitMs;

    _speedReplyDueMs = _now() + waitMs + 100;   // +100ms cho chắc, vẫn xa cận trên 3x
    _speedReplyPending = true;
}

/// <summary>Gọi mỗi frame từ GopetClient.Update(). Không gọi thì kết nối bị đóng sau ~3 nhịp.</summary>
public void Tick(long nowMs)
{
    if (!_speedReplyPending || nowMs < _speedReplyDueMs) return;
    _speedReplyPending = false;
    _send(AuthPackets.CheckSpeedReply());
    SpeedChecked?.Invoke();
}

/// <summary>Bỏ nhịp đang chờ khi kết nối đứt — tránh gửi vào socket đã đóng ở lần nối lại.</summary>
public void Reset() => _speedReplyPending = false;
```

`_now` truyền vào constructor (`Func<long>`) để test tiêm đồng hồ giả; netstandard2.1 không
có `Environment.TickCount64` nên dùng `Stopwatch`. `MessagePump.Until` phải gọi
`auth.Tick(...)` trong vòng lặp.

---

## HIGH

### H1. Check "L" của live smoke không thể phát hiện C1 — luôn xanh với cả hai hành vi

`tests/Gopet.Net.LiveSmoke/LoginChecks.cs:151-155`

```csharp
Report.Check("L. Đã trả lời ít nhất một nhịp CHECK_SPEED", _speedReplies > before, ...);
```

Giữ 40s: hành vi đúng cho 1-2 nhịp, hành vi sai (C1) cho ~80 nhịp. Cả hai đều `> before`.
Đây chính là lý do lỗi lọt qua 14/14.

Sửa — thêm **cận trên**:

```csharp
// 40s giữ, chu kỳ 15-30s => tối đa 3 nhịp. Nhiều hơn nghĩa là đang trả lời
// quá sớm và server gửi lại mỗi tick 500ms (Player.cs:330-336).
var replies = _speedReplies - before;
Report.Check("L. Trả lời CHECK_SPEED đúng nhịp (1..3 lần trong 40s)",
    replies >= 1 && replies <= 3, $"đã trả lời {replies} nhịp — quá nhiều = trả lời sớm");
```

### H2. `AuthHandler` và `AuthRules` không có một unit test nào

`tests/Gopet.Net.Tests/AuthTests.cs` chỉ phủ builder thuần (`ClientInfo`, `AuthPackets.Login`)
và parser thuần (`ServerList`, `LoginSuccess`). File duy nhất có logic hành vi —
`AuthHandler` — hoàn toàn không được test. Không ngẫu nhiên mà C1 nằm đúng ở đó.

Bổ sung (dùng `MessageRouter` thật + `Action<Message>` giả, không cần socket):
- `OnCheckSpeed`: dispatch `81 67 <int 20000>` → **chưa** gửi gì; `Tick(due-1)` → chưa gửi;
  `Tick(due)` → gửi đúng 2 byte `5167`. Test này fail với code hiện tại → đúng ý.
- `OnCreateChar` → bắn `CharacterCreationRequired`; gói dài/ngắn hơn `sbyte+int+int` ném `ProtocolException`.
- `OnLoginFailed` / `OnRedDialog` → đúng chuỗi; thừa byte thì ném.
- `AuthRules`: `"Abc123"` (hoa) và `"abc-123"` bị chặn; `"abc12"` (5 ký tự) qua `IsValidUsername`
  nhưng trượt `IsValidRegistration` — chính là điểm hai hàm khác nhau, hiện không ai giữ.

### H3. Live smoke bỏ qua âm thầm 4 check và vẫn thoát 0

`LoginChecks.cs:45-50` + `LoginChecks.cs:103-109` + `Report.cs:8-11`

`Report` chỉ đếm **failure**; check không chạy thì không để lại dấu vết. Nếu lượt 2 lại rơi
vào `_needsCharacter` (tên nhân vật trùng — `GameController.cs:729-733` gửi `redDialog` rồi
đóng; hoặc tên trượt `CreateCharLaw`), `Attempt()` `return false` và các check **I, J, K, L
không bao giờ được gọi** → in "LIVE SMOKE OK", exit 0. Harness báo xanh khi chưa hề đăng nhập.

Sửa (nên làm cả hai):
```csharp
// LoginChecks.Run()
if (!Attempt())
{
    ResetState();
    if (!Attempt())
        Report.Fail("I-L. Đăng nhập", "lượt 2 vẫn đòi tạo nhân vật — không chạy được check I..L");
}
```
và cho `Report` biết trước danh sách check bắt buộc, cuối phiên fail cho check nào chưa chạy.

### H4. `okDialog` (opcode 71) không có handler → đăng nhập treo im lặng

`AuthHandler.RegisterOn` (`AuthHandler.cs:50-61`) đăng ký -36, 64, 3, 4, 10, 21, 81/103.
Thiếu **71**. Server dùng `okDialog` (`Player.cs:735-742`: `Message(71)`, `putsbyte(0)`,
`putUTF(text)`) ở đúng các nhánh chặn đăng nhập:

- `login()` `Player.cs:600-603` — `isShowMessageWhenLogin` bật → `okDialog` rồi `return`,
  không có `LOGIN_SUCCES`, không có `LOGIN_FAILED`.
- `doRegister()` `Player.cs:188` — cùng nhánh.
- `Player.cs:225` — đăng ký thành công.

Bật cờ đó lên là client đứng chờ tới hết timeout mà không hiển thị gì, đúng loại triệu
chứng mà các comment trong phase này cố công phòng tránh. Lưu ý 71 là opcode **bao ngoài**
(byte đầu thân gói là 0), nên dùng `RegisterEnvelope(71)` + `RegisterSub(71, 0, ...)`,
đừng đăng ký top-level — sau này đăng ký nhầm kiểu sẽ ném "Opcode 71 đã có handler".

---

## MEDIUM

### M1. `packet-diff`: hai đường ra kết quả xanh giả

`tools/packet-diff/index.js:76-101, 141-147`

1. **Dump rỗng ⇒ "OK"**. `n = Math.max(0,0) = 0`, vòng lặp không chạy, in
   `OK — 0 gói khớp về chuỗi opcode` và exit 0. Sai đường dẫn, sai `--direction`, hoặc chạy
   trước khi có gói nào — đều báo pass. Với `--opcodes-only` (cờ mới) khả năng này càng cao
   vì cờ này chính là để dùng khi diff đang ồn.
2. **`--direction` không được validate**. `--direction in` (thường) hay `--direction Out`
   lọc sạch mọi gói → rơi vào (1). `--direction` đặt cuối không có giá trị → `undefined` →
   im lặng bỏ lọc, ngược hẳn ý người dùng.

```js
const VALID_DIRECTIONS = ['IN', 'OUT'];
if (dirIdx >= 0 && !VALID_DIRECTIONS.includes(directionFilter)) {
    console.error(`--direction phải là IN hoặc OUT (nhận: ${directionFilter})`);
    process.exit(2);
}
if (n === 0) {
    console.error('Không có gói nào để so — sai đường dẫn hay sai --direction?');
    process.exit(2);
}
```

3. **Cột `enc` không bao giờ được so**, ở mọi chế độ. Một client gửi mã hoá còn client kia
   gửi plaintext là desync nghiêm trọng nhất có thể, mà công cụ vẫn in "OK". Thêm vào cạnh
   phép so opcode (dòng 112) — nó không liên quan gì tới `--opcodes-only`.
4. Dòng usage `index.js:88` thiếu `--opcodes-only`, trong khi comment đầu file `:9` đã có.

### M2. `ClientInfoPacket` trùng lặp `ClientInfo` — check byte-exact đang xác thực nhầm class

`tests/Gopet.Net.LiveSmoke/ClientInfoPacket.cs` dựng lại y hệt gói mà
`Assets/Scripts/Net/Auth/ClientInfo.cs:48-58` dựng. `ProtocolChecks.cs:28` dùng bản sao;
`LoginChecks.cs:83` dùng bản thật. Check **D** (`DumpChecks.cs:38-43`, so hex từng byte) chạy
trên dump của `ProtocolChecks` ⇒ **nó chứng minh bản sao đúng, không chứng minh code ship đúng**.
Bản thật chỉ được kiểm gián tiếp qua check G (server trả `1`) — yếu hơn hẳn.

Sửa: xoá `ClientInfoPacket.cs`, `ProtocolChecks` dùng
`new ClientInfo { Info = "unity-live-smoke", TrailingField = "ref-test" }`. Hằng hex viết tay
trong `DumpChecks` giữ nguyên — nó vẫn là oracle độc lập, đúng như comment `DumpChecks.cs:16-20`
mô tả, chỉ là bây giờ nó soi đúng đối tượng.

### M3. `Login_ThieuHaiTruongCuoi_LechDoDai` — tên hứa nhiều hơn nó kiểm

`tests/Gopet.Net.Tests/AuthTests.cs:60-71`. `Assert.True(full.Length > short3.Length)` bắt được
"thiếu UTF thứ 4" (khi đó hai độ dài bằng nhau) nhưng **không** bắt được "bỏ 8 byte padding" —
bỏ padding thì vẫn còn dài hơn, test vẫn xanh. Đúng cái mà tên test nói là nó giữ.
Test byte-exact ngay trên (`:51-58`) đã phủ trọn cả hai. Hoặc siết:

```csharp
// 2 byte length + 5 byte "1.4.3" + 8 byte padding = 15
Assert.Equal(short3.ToWire().Length + 15, full.ToWire().Length);
```
hoặc xoá hẳn cho đỡ trùng (DRY).

### M4. Số dòng trích dẫn sai

| Chỗ | Ghi | Thực tế |
|---|---|---|
| `LoginSuccess.cs:5` | `GameController.cs:1579-1585` | **1584-1589** |
| `ClientInfo.cs:41` | `Player.cs:120` (Refcode) | **119** |
| `AuthHandler.cs:102-103` | "chỉ để tham khảo" | Sai nội dung — xem C1 |

Đúng: `AuthPackets.cs:23` (`Player.cs:134`), `AuthRules.cs:19,26` (`Player.cs:606`),
`AuthRules.cs:48` (`Player.cs:196`), `AuthPackets.cs:63,66` (`GameController.cs:718`, `:740`),
`ServerEntry.cs:22` (`1034-1052`), `RegisterCheck.cs:13` (`Player.cs:191`),
`ClientInfo.cs:5` (`104-129`).

### M5. Plan `phase-03-login-handshake.md` — ba chỗ sai sự thật, code lại đúng

- Bảng "Mã trả về khi login hỏng": *"Thành công | opcode **21**"*. Sai. 21 = `CREATE_CHAR`
  (tài khoản **chưa có nhân vật**); thành công là opcode **3** `LOGIN_SUCCES`
  (`GameController.cs:1584`). Bảng này mâu thuẫn với chính mục "#2 Tài khoản chưa có nhân vật"
  ở dưới. Code của `AuthHandler` làm đúng — chỉ tài liệu sai, và đây đúng loại sai lệch dễ
  làm người sau sửa code cho "khớp plan".
- Cùng bảng: *"Sai tài khoản/mật khẩu → opcode 4 (dialog đỏ)"*. 4 là `LOGIN_FAILED`
  (`Player.cs:684-690`); dialog đỏ là opcode **10** (`Player.cs:387`). Hai đường khác nhau.
- `LOGIN_SUCCES (từ GameController.cs:1576-1582)` → thực tế **1584-1589**.
  `CHECK_SPEED ... (Player.cs:293)` → thực tế **307**. `session.Close() (Player.cs:283)` →
  thực tế **298**. `showListServer() Player.cs:998-1020` → thực tế **1016-1053**.
- Mục "Related Code Files / Create" liệt kê `Net/Handlers/AuthHandler.cs` và
  `Net/Handlers/ServerListHandler.cs`. Thực tế: `Net/Auth/AuthHandler.cs`, và `ServerList`
  nằm trong `Net/Auth/ServerEntry.cs` (không có `ServerListHandler.cs`). Không có file nào
  ở `Net/Handlers/`.

### M6. Plan là nguồn của C1 — sửa code mà không sửa plan thì lỗi quay lại

Implementation Step 6: *"`CHECK_SPEED` — nhận, đọc `int`, trả lời lại."* Không nói phải chờ.
Ghi chú ngay dưới còn khẳng định *"Logic ban vì trả lời quá nhanh hiện đã bị comment nên chỉ
ghi log"* — đúng với build này, nhưng bỏ qua vế thứ hai (nhánh đó `return` sớm, không cho
stopwatch chạy lại ⇒ flood 500ms) và tạo cảm giác trả lời sớm là vô hại. Viết lại thành:
"đọc `int waitMs`, **chờ đúng `waitMs`** rồi mới trả lời — cửa sổ hợp lệ
`[waitMs − 2s, 3×waitMs)`; trả lời sớm bị ghi log speed-hack và server bắn lại mỗi 500ms".

---

## LOW

- **L1.** `AuthRules.cs:17` `RegexOptions.Compiled`. Hai lần gọi cho chuỗi ngắn — codegen tốn
  hơn phần tiết kiệm, và `Compiled` là mìn quen thuộc trên IL2CPP/full-AOT (không có
  `Reflection.Emit`). Bỏ cờ đi. Hoặc bỏ luôn `Regex`: `^[a-z0-9]+$` là một vòng `for` bốn dòng.
- **L2.** `AuthRules` validate **trước** khi trim; server `Trim()` rồi mới regex
  (`Player.cs:597-606`). `"abc123 "` bị client chặn nhưng server chấp nhận. Trim trước khi
  validate và trước khi đưa vào `AuthPackets.Login`.
- **L3.** `GopetSocket.Send` (`GopetSocket.cs:99-108`) kiểm `_connected` rồi mới `_outgoing.Add`.
  `Dispose` gọi `CompleteAdding()` ⇒ có khe hở kiểm-rồi-làm. Hiện **an toàn** vì cả dispatch
  lẫn `Dispose` đều ở luồng chính, nhưng chỉ cần một lần `Dispose` từ luồng khác là
  `InvalidOperationException` ném ra giữa `AuthHandler.OnCheckSpeed`. Bọc `Add` trong
  `try { } catch (InvalidOperationException) { message.Dispose(); }`.
- **L4.** Dump gói chứa **mật khẩu dạng rõ**: `packet-dump-server.log` giải hex ra
  `gopettest..abc12345..ref-...`, và `PacketLogger` phía client ghi payload **trước** khi mã hoá
  nên `packet-dump-unity.log` cũng vậy. `.gitignore:31-32` đã chặn commit nên rủi ro thấp — nhưng
  ghi một dòng cảnh báo vào `tools/README.md` (đừng đính dump vào issue/report), và đổi mật khẩu
  tài khoản harness thành thứ dùng một lần. `Program.cs:50` đang để `GOPET_PASS` mặc định `abc12345`.
- **L5.** `LoginChecks.cs:45` — `!Attempt() && _needsCharacter`: `Attempt()` chỉ trả `false` ở
  đúng nhánh `_needsCharacter`, vế thứ hai thừa. `ResetState()` không reset `_speedReplies`
  (vô hại vì `HoldConnection` chụp `before`, nhưng lệch với tên hàm).
- **L6.** `MessagePump.Until` bỏ qua giá trị trả về ở cả 5 chỗ gọi. Không sai (trạng thái được
  kiểm lại ngay sau), nhưng nếu định giữ vậy thì nên trả `void`, hoặc dùng giá trị đó để in lý
  do timeout — hiện có một `bool` mang thông tin mà không ai đọc.

---

## Trả lời trực tiếp các câu hỏi trọng tâm

1. **Parser đọc thiếu/thừa/sai thứ tự?** Không. Đã đối chiếu tay từng field:
   `CLIENT_INFO` 8 field (`Player.cs:106-119`) ✓ · `SERVER_LIST` (`:1035-1050`) ✓ ·
   `LOGIN_SUCCES` 5 field (`GameController.cs:1585-1589`) ✓ · `CREATE_CHAR` sbyte+int+int
   (`:1474-1476`) ✓ · `LOGIN_FAILED` 1 UTF (`Player.cs:687`) ✓ · dialog đỏ 1 UTF (`:388`) ✓ ·
   `CHECK_SPEED` 1 int (`:307`) ✓. Thiếu duy nhất `okDialog` opcode 71 (H4).

2. **`ServerEntry.cs` — port ba lần + khối bool tách rời:** đọc **đúng**. Khớp
   `Player.cs:1039-1041` (ba `putInt(serverInfo.Port)`) và vòng lặp thứ hai `:1047-1051`.
   Test dùng hex thật `...00004aec 00004aec 00004aec 0101` xác nhận. `MaxServers = 256` +
   `ExpectFullyConsumed` là phòng thủ tốt, giữ nguyên.

3. **`LoginSuccess` ném `ProtocolException` khi hai tên khác nhau — có quá gắt?**
   **Hợp lý, giữ nguyên.** `GameController.cs:1586-1587` là hai lần `putString` của **cùng một
   biến** `player.playerData.name` — không có đường nào để chúng khác nhau trên một gói đọc
   đúng. Khác nhau ⇒ chắc chắn parser lệch, fail-fast ở đây rẻ hơn hẳn. Đây là kiểm bất biến,
   không phải kiểm dữ liệu người dùng.

4. **`AuthHandler` tự trả lời `CHECK_SPEED` — có race hay rò khi kết nối đã đóng?**
   Không rò: `GopetSocket.Send` (`:99-108`) thấy `!_connected` thì `message.Dispose()` rồi
   `return`, không ném, không giữ tham chiếu. Race chỉ ở mức lý thuyết (L3). **Vấn đề thật
   không phải race mà là thời điểm — xem C1.** Khi sửa theo C1, nhớ `Reset()` nhịp đang chờ
   lúc mất kết nối, nếu không lần nối lại sẽ gửi một `5167` mồ côi ngay đầu phiên và
   `Player.cs:342-345` (`else { session.Close(); }`) đóng ngay lập tức.

5. **8 byte padding của `LOGIN` — vô hại thật không?** **Vô hại, đã kiểm.**
   `MsgReader.readMessage()` (`IO/MsgReader.cs:50-113`) đọc theo khung có tiền tố độ dài rồi
   dựng `Message` từ đúng payload đó; `Player.cs:134` đọc 3 UTF từ reader **của riêng gói ấy**,
   phần thừa bị vứt cùng gói. Không có chuyện tràn sang gói sau. Cạm bẫy duy nhất phía server:
   `Length > 10000` ném `IOException` → đóng kết nối; 77 byte thì không tới đâu.

6. **Test mới có fail được không, hay luôn xanh?**
   - Byte-exact `ClientInfo` / `Login` (`:29-58`): fail được, rất mạnh — giữ.
   - `ServerList` / `LoginSuccess` gói thật + nhánh ném: fail được.
   - `Login_ThieuHaiTruongCuoi_LechDoDai`: fail được nhưng **không phủ đúng thứ tên nó nói** (M3).
   - Live smoke **check L**: xanh với cả hành vi đúng lẫn hành vi sai — vô dụng đúng ở chỗ
     cần nhất (H1). Check **I/J/K/L** còn có thể **không chạy** mà harness vẫn exit 0 (H3).
   - `AuthHandler` / `AuthRules`: không có test nào (H2).

7. **200 dòng / DRY / KISS:** không file nào quá 200 dòng (lớn nhất `LoginChecks.cs` 163,
   `packet-diff/index.js` 150). DRY vi phạm một chỗ: `ClientInfoPacket` vs `ClientInfo` (M2).
   Phần còn lại gọn và đúng KISS.

8. **Comment nói sai so với code:** `AuthHandler.cs:102-103` (nghiêm trọng, C1),
   `LoginSuccess.cs:5` và `ClientInfo.cs:41` (số dòng, M4), plan phase-03 (M5, M6).

---

## Điểm làm tốt

- Vector test là **byte thật bắt từ client J2ME**, kèm lý do vì sao không dùng vector tự bịa
  (`AuthTests.cs:7-13`). Đây là thứ giữ được parity, không phải test cho có.
- `ExpectFullyConsumed` ở cuối **mọi** parser — bắt lệch ngay tại gói thay vì để nó biểu hiện
  ba màn hình sau. Đã xác nhận là kiểm thật (`JavaBinaryReader.cs:139-146`), không phải no-op.
- `ClientInfo.cs:40-45` và `AuthPackets.cs:13-28` ghi lại đúng hai cái bẫy đặt tên của server
  (`Refcode` không chứa refcode; tham số `version` thực nhận refcode). Không đọc dây thật thì
  không cách nào biết.
- `MessagePump.cs:23-24`: rút hết hàng đợi **trước** khi xét điều kiện — đúng, và lý do được
  ghi lại. Đây là loại lỗi mất gói cuối rất khó truy.
- `AuthPackets.cs:66-68` cảnh báo server đóng kết nối sau `CREATE_CHAR`; `RegisterCheck.cs:11-16`
  nói rõ vì sao không thể kiểm đăng ký thành công thay vì lặng lẽ bỏ.
- `LoginChecks.cs:21` giải thích vì sao phải tạo router mới mỗi lượt — đúng, và không hiển nhiên.

---

## Hành động đề xuất (theo thứ tự)

1. **C1** — hoãn trả lời `CHECK_SPEED` đúng `waitMs`; thêm `Tick(nowMs)` + `Reset()`.
2. **H1** — check L kẹp cận trên (1..3 nhịp/40s), để nó fail được với hành vi cũ.
3. **H2** — unit test `AuthHandler`: nhịp `CHECK_SPEED`, `CREATE_CHAR`, `LOGIN_FAILED`, dialog đỏ; + `AuthRules`.
4. **H3** — `Report` theo dõi check bắt buộc chưa chạy; `LoginChecks.Run` báo fail khi lượt 2 vẫn đòi tạo nhân vật.
5. **H4** — xử lý `okDialog` (envelope 71 / sub 0).
6. **M1** — `packet-diff`: chặn 0 gói, validate `--direction`, so cột `enc`, sửa dòng usage.
7. **M2** — xoá `ClientInfoPacket`, trỏ check D vào `ClientInfo` thật.
8. **M5/M6** — sửa plan: bảng mã trả về, số dòng, đường dẫn file, và Step 6 về `CHECK_SPEED`.
9. **M3, M4, L1-L6** — dọn khi tiện tay.

Sau (1) nên chạy lại live smoke với `GOPET_HOLD_SECONDS=120` rồi đếm lại:
`grep -c "OUT.81.0.6.5167" packet-dump-server.log` phải ra 4-8 gói, không phải ~240.

## Số liệu

- Build netstandard2.1 / C#9: pass, 0 warning (`TreatWarningsAsErrors`).
- Unit test: 138/138 pass, 35ms. Phủ `AuthHandler`: **0%**. Phủ `AuthRules`: **0%**.
- Live smoke: 14/14 pass — nhưng 4 check có thể bị bỏ qua mà vẫn exit 0 (H3), và 1 check
  không phân biệt được đúng/sai (H1).
- Dòng: 453 (Auth) + 136 (AuthTests) + 256 (3 file LiveSmoke) + 150 (packet-diff). Max 163.

## Câu hỏi chưa giải quyết

1. Server production gốc có bỏ comment `user.ban(...)` ở `Player.cs:333` không? Nếu có, C1 là
   lỗi khoá tài khoản chứ không chỉ là flood.
2. Client J2ME chờ bằng `Timer` hay bằng vòng game loop? Ảnh hưởng tới độ chính xác nên nhắm —
   sai số đo được là +30..80ms, dùng `waitMs + 100` là an toàn trong mọi trường hợp.
3. `CLIENT_INFO` `TrailingField = "2.4.9"` — chuỗi này từ đâu ra trong client cũ (hardcode hay
   sinh động)? Nếu sinh động theo build thì giá trị cứng sẽ lệch khi server đổi kiểm tra.
