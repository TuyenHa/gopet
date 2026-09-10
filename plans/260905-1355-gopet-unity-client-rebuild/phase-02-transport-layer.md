---
phase: 2
title: Transport Layer
status: complete
priority: P1
effort: 1w
dependencies:
  - 1
---

# Phase 2: Transport Layer

## Overview

Tầng vận chuyển của client Unity: TCP socket + TEA + khung gói + đọc/ghi big-endian + handshake 9 byte.

**Đặc tả đã đầy đủ 100%** — đối chiếu được cả từ server (`Server/IO/*.cs`) lẫn client decompile (`eo/en/ep/eq/er.java`). Đây là phase rủi ro thấp nhất nhưng là nền của mọi thứ sau.

Ước lượng code: **200-300 dòng C#**.

## Requirements

**Functional**
- Kết nối TCP tới GServer, gửi handshake 9 byte, nhận/gửi gói tin mã hoá TEA
- Đọc/ghi đúng big-endian (Java) — không dùng `BinaryReader` mặc định của .NET
- Thread nhận + hàng đợi gửi, không block main thread của Unity
- Packet logger phía client, format giống P1 để diff được

**Non-functional**
- Không đổi một byte nào so với client J2ME cũ
- Reconnect an toàn, không rò socket khi mất kết nối

## Architecture

### Handshake (từ `eq.java:a(long)` — đã khôi phục nguyên vẹn)

```
Client -> Server: đúng 9 byte
  [0]    = 0x09                          (độ dài)
  [1..8] = System.currentTimeMillis() big-endian
```

Server (`Session.readKey()`, `Session.cs:80`) đọc 9 byte, lấy `[1..8]` dựng khoá TEA. Client cũng dùng chính giá trị đó.

### Khung gói

```
[int32 BE: payloadLen + 1][byte: isEncrypted][payload...]
payload[0] = opcode (sbyte), payload[1..] = body
```

`isEncrypted == 1` → payload đã qua TEA 128-bit.

### Luồng thread trong Unity

```
ReceiveThread (background)  --> ConcurrentQueue<Message> --> pump ở Update() --> handler
SendQueue (ConcurrentQueue) --> SendThread (background)  --> socket
```

**Quan trọng:** mọi thứ đụng tới `GameObject`/`Texture2D` phải chạy ở main thread. Thread nhận chỉ đẩy `Message` vào queue; `MonoBehaviour.Update()` rút ra và dispatch.

## Related Code Files

**Create (Unity project)**
- `Assets/Scripts/Net/GopetSocket.cs` — TCP, 2 thread, hàng đợi
- `Assets/Scripts/Net/Tea.cs` — port thẳng từ `GServer/Server/IO/TEA.cs`
- `Assets/Scripts/Net/Message.cs` — khung gói, mirror `GServer/Server/IO/Message.cs`
- `Assets/Scripts/Net/JavaBinaryReader.cs` — big-endian + `readUTF`
- `Assets/Scripts/Net/JavaBinaryWriter.cs` — big-endian + `writeUTF`
- `Assets/Scripts/Net/GopetCmd.cs` — **sinh tự động** từ `GServer/Server/GopetCMD.cs`
- `Assets/Scripts/Net/ClientPacketLogger.cs` — dump format giống P1
- `tools/gen-gopet-cmd/` — script sinh `GopetCmd.cs`

**Read for context**
- `SRCGOPETGOC/GServer/Server/IO/TEA.cs` — thuật toán TEA
- `SRCGOPETGOC/GServer/Server/IO/IOExtension.cs:68-91` — `WriteInt`/`ReadJavaInt`
- `SRCGOPETGOC/GServer/Server/IO/DataOutputStream.cs:137-149` — `writeUTF`
- `SRCGOPETGOC/GServer/Server/IO/MsgSender.cs:60-90` — logic đóng gói
- `SRCGOPETGOC/GServer/Server/IO/MsgReader.cs:50-100` — logic mở gói
- `client.jar_Decompiler.com/{eq,ep,en,er}.java` — đối chiếu chéo

## Implementation Steps

1. **`JavaBinaryReader` / `JavaBinaryWriter`** — big-endian trước tiên
   ```csharp
   // .NET BinaryWriter là little-endian, Java là big-endian
   public void WriteInt(int v) {
       _s.WriteByte((byte)(v >> 24)); _s.WriteByte((byte)(v >> 16));
       _s.WriteByte((byte)(v >> 8));  _s.WriteByte((byte)v);
   }
   ```
   Đối chiếu `IOExtension.cs:68-78`.

2. **`writeUTF` / `readUTF`** — server ghi **UTF-8 thường** + prefix `short` độ dài (`DataOutputStream.cs:137`), **không** phải Java modified UTF-8. C# dùng `Encoding.UTF8` là đúng.
   > Ngoại lệ đã biết: ký tự ngoài BMP (emoji) sẽ lệch — Java ghi CESU-8 6 byte, .NET ghi 4 byte. Hiếm gặp, ghi nhận lại chứ chưa xử lý.

3. **`Tea.cs`** — port `TEA.cs`. Giữ nguyên hằng số `1640531527` và `-957401312`. Chú ý `>>>` của Java = `>>` trên `uint` trong C#, hoặc dùng đúng phép dịch không dấu.

4. **`Message.cs`** — mirror `Server/IO/Message.cs`:
   - Constructor từ opcode (để gửi)
   - Constructor từ `byte[]` (để nhận, `data[0]` = opcode)
   - `PutSByte/PutInt/PutUTF/PutBool/PutShort/PutLong` + reader tương ứng
   - Lưu ý: `putString` và `putUTF` trên server **giống hệt nhau** (đều gọi `writeUTF`) — chỉ là alias, không có khác biệt

5. **`GopetCmd.cs` sinh tự động** — viết script đọc `GServer/Server/GopetCMD.cs`, regex `public const sbyte (\w+) = (-?\d+);`, sinh ra file C# tương ứng. Chạy lại mỗi khi server đổi.
   > Đừng gõ tay 177 hằng số. Gõ tay là sẽ sai.

6. **`GopetSocket.cs`**
   - `Connect(host, port)` → mở TCP → gửi 9 byte handshake → khởi tạo `Tea` với cùng timestamp → start 2 thread
   - Thread nhận: đọc `int32` length → đọc `byte` isEncrypted → đọc payload → giải mã nếu cần → enqueue
   - Thread gửi: dequeue → mã hoá nếu cần → ghi length + flag + payload → flush
   - `Update()` pump: rút queue, gọi handler ở main thread

7. **Chú ý opcode âm** — `CLIENT_INFO = -36`. Phải xử lý như `sbyte`, không phải `byte`. Sai chỗ này là gói tin đầu tiên đã hỏng.

8. **`ClientPacketLogger`** — format y hệt P1 để `packet-diff` chạy được.

9. **Test bằng diff** — dựng scene rỗng, chỉ connect + gửi handshake. Chạy client cũ trên emulator làm cùng việc. Diff 2 dump: 9 byte handshake phải giống nhau về cấu trúc (giá trị timestamp khác nhau là bình thường).

## Success Criteria

- [x] Connect tới server test, server không đóng kết nối ngay
- [x] Server log nhận đủ 9 byte handshake, dựng được khoá TEA
- [x] Gửi 1 gói mã hoá, server giải mã ra đúng opcode
- [x] Nhận 1 gói từ server, giải mã đúng
- [x] `GopetCmd.cs` sinh tự động, khớp 177 hằng số với server
- [x] Round-trip test: gửi → nhận → giá trị khớp cho mọi kiểu (`sbyte/int/short/long/UTF/bool`)
- [x] Ngắt kết nối không rò thread, không rò socket
- [x] Packet dump client đọc được bằng packet-diff tool của P1

### Bằng chứng đã kiểm chứng (2026-09-05)

Kiểm chứng sống chạy bằng `tests/Gopet.Net.LiveSmoke/` — **chính code trong
`Assets/Scripts/Net`**, compile thẳng chứ không phải bản viết lại. Cần một GServer
đang chạy nên cố ý **không** nằm trong `verify.ps1` (verify phải chạy được offline).

```powershell
cd GopetUnityClient\tests\Gopet.Net.LiveSmoke; dotnet run
```

| Tiêu chí | Cách kiểm | Kết quả |
|---|---|---|
| Kết nối không bị đóng ngay | Check A — connect, đợi 500ms | PASS |
| Handshake dựng đúng khoá TEA | Server giải mã được CLIENT_INFO. Khoá sai thì không giải nổi, nên đây là bằng chứng gián tiếp nhưng chặt | PASS |
| Gói mã hoá → server hiểu | Check B — server trả đúng opcode -36 | PASS |
| Giải mã gói nhận | Check C — `setClientOK = 1`, `ExpectFullyConsumed` sạch | PASS |
| 177 opcode | `verify.ps1` bước 1/5 | PASS |
| Round-trip đủ kiểu | 131 unit test (thêm 12: `short`, `bool` trước đó **không có test nào**) | PASS |
| Dump client ghi đúng byte | Check D — so với hằng số hex viết tay | PASS |
| Dump hai đầu khớp từng byte | Check F — client OUT ≡ server IN, tự động | PASS |
| Không rò thread | Check E1 — 5 chu kỳ, `DisposedCleanly` của cả 5 | PASS |
| Không rò socket | Check E2 — cổng lấy từ chính socket, soi cả ESTABLISHED lẫn CLOSE_WAIT | PASS |
| packet-diff đọc được dump | Tự so ra OK; so với bản sửa 1 opcode báo đúng gói #1 lệch | PASS |

**Đối chiếu byte hai đầu.** Dump server và dump client cùng một phiên:

```
client OUT  -36 enc=1 len=53  dc00000000040005312e342e33...7265662d74657374
server IN   -36 enc=1 len=53  dc00000000040005312e342e33...7265662d74657374
server OUT  -36 enc=0 len=2   dc01
client IN   -36 enc=0 len=2   dc01
```

Khớp từng byte cả hai chiều. Đây là bằng chứng mạnh nhất cho "không đổi một byte
nào trên dây" — mạnh hơn round-trip tự nhất quán rất nhiều.

**Test tự kiểm.** Năm phép đột biến, cả năm đều bị bắt:

| Đột biến | Kết quả |
|---|---|
| Trỏ vào cổng chết (19999) | FAIL, exit 1 |
| Đổi version → `1.0.0` | FAIL ở check B |
| `WriteShort` đảo sang little-endian | 5 test đỏ |
| `DisplayWidth` 320 → 321 | FAIL ở check D (`0141` thay vì `0140`) |
| Dump server rỗng | FAIL ở check F |

### Sửa sau code review

Bản harness đầu tiên pass 6/6 nhưng **chưa chứng minh được điều nó tuyên bố**:

- Check "dump ghi ra được" chỉ gọi `File.Exists`. File luôn tồn tại vì constructor của
  `PacketLogger` tạo nó ngay — check này không bao giờ đỏ. Thay bằng so nội dung với
  hằng số hex viết tay, cộng check F đối chiếu thẳng với dump server.
- Đếm `Process.Threads.Count` để tìm rò luồng là phép đo lệch về âm tính giả: threadpool
  co giãn hai chiều, và luồng quá hạn join vẫn thoát ngay sau đó nên biến mất trước khi
  kịp đếm. Thay bằng `GopetSocket.DisposedCleanly` — tất định.
- Check socket chỉ lọc `Established`, bỏ sót `CloseWait` — đúng dạng rò kinh điển
  (phía kia FIN mà mình không đóng). Và cổng cục bộ dò qua bảng TCP của hệ điều hành
  nên trên loopback dễ nhặt nhầm kết nối cũ ở TIME_WAIT; giờ lấy thẳng từ socket.
- Hai nhóm check chung một `try`: nhóm giao thức ném thì nhóm rò không chạy mà output
  không nói gì. Tách try riêng từng nhóm.

Sửa luôn trong `GopetSocket`: `Dispose` idempotent (Unity gọi cả `OnDestroy` lẫn
`OnApplicationQuit`), và `Fail` dùng `Interlocked` để hai luồng cùng hỏng không bắn
sự kiện `Disconnected` hai lần.

Báo cáo đầy đủ: `plans/reports/code-reviewer-260905-2038-phase-02-live-smoke.md`

### Phát hiện ngoài phạm vi P2

Khi client gửi version cũ, server **ghi log là đã gửi** gói opcode 10 ("Phiên bản
cũ rồi, bạn vui lòng tải bản mới nhất") nhưng **không client nào nhận được** —
client JS độc lập cũng mất y hệt. Nên đây không phải lỗi tầng vận chuyển C#;
nhiều khả năng server đóng kết nối kiểu abortive, xoá sạch dữ liệu còn trong đệm.

Hệ quả: người chơi dùng bản cũ chỉ thấy mất kết nối, không thấy lý do. Ghi lại
để xử lý ở phía server, không thuộc P2.

### Còn chưa chứng minh

Tầng vận chuyển đã xong và đúng wire. Nhưng **chạy trong Unity Editor thật thì
vẫn chưa** — `Assets/Scripts/Net` mới chỉ được compile dưới `netstandard2.1`
(`tests/Gopet.Net.UnityCompat`), chưa có Editor nào nạp asmdef và `GopetClient`.
Bước 9 của plan có nhắc "dựng scene rỗng"; ở đây thay bằng harness headless vì
nó chứng minh tầng giao thức chặt hơn. Phần Unity Editor để lại cho P3.

## Risk Assessment

| Rủi ro | Xử lý |
|---|---|
| Nhầm `>>>` (Java) với `>>` (C#) trong TEA | Viết unit test: mã hoá rồi giải mã ra đúng chuỗi gốc. Thêm test vector cố định lấy từ dump gói tin thật |
| Nhầm `sbyte`/`byte` với opcode âm | Dùng `sbyte` xuyên suốt cho opcode, chỉ cast khi ghi ra stream |
| Thread nhận đụng Unity API | Nghiêm ngặt: thread chỉ enqueue. Mọi dispatch ở `Update()` |
| `readUTF` lệch do encoding | Test với chuỗi tiếng Việt có dấu (`"Bảo trì cập nhật"`) — trường hợp thật trong `server.json` |

## Next Steps

Xong P2 → P3 (Login & Handshake). P3 là mốc chứng minh: nếu đăng nhập được thì phần còn lại chỉ là khối lượng, không còn là rủi ro.
