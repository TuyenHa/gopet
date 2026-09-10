---
phase: 1
title: Server Hardening & Verify Infra
status: complete
priority: P1
effort: 1w
dependencies: []
---

# Phase 1: Server Hardening & Verify Infra

## Overview

Vá các lỗ hổng làm sập server test, dựng server test riêng, và xây hạ tầng đối chiếu gói tin (emulator J2ME + packet logger 2 đầu).

**Không có phase này thì mọi phase sau đều đau.** Client Unity đang dev gửi sai một int là giết server. Và không có công cụ đối chiếu thì debug desync là mò kim đáy bể.

## Requirements

**Functional**
- GServer chạy được trên môi trường test riêng, tách khỏi production
- Packet logger ghi hex dump + opcode + timestamp ở cả server và client
- Emulator J2ME chạy được `client.jar` gốc, kết nối vào server test

**Non-functional**
- Không đổi một byte nào trong format gói tin
- Logger bật/tắt bằng config, không ảnh hưởng hiệu năng khi tắt

## Architecture

```
[client.jar gốc]  --TCP--> [GServer test] <--TCP--  [Unity client đang dev]
   (emulator)                    |
                                 v
                        packet-dump-server.log
                                 |
                          diff tool so sánh
                                 ^
                        packet-dump-unity.log
```

Nguyên tắc: cùng một hành động (login, mở menu, đi lại) thực hiện trên cả 2 client → 2 file dump phải giống nhau về opcode và độ dài. Lệch ở đâu thấy ngay ở đó.

## Related Code Files

**Modify (GServer)**
- `SRCGOPETGOC/GServer/Server/GameController.cs:215` — giới hạn độ dài mảng `points`
- `SRCGOPETGOC/GServer/Server/GameController.cs:784` — giới hạn độ dài mảng `texts`
- `SRCGOPETGOC/GServer/APIs/HttpServer.cs:68` — bind `127.0.0.1` thay vì `0.0.0.0`
- `SRCGOPETGOC/GServer/Server/IO/MsgSender.cs` — hook packet logger (gửi)
- `SRCGOPETGOC/GServer/Server/IO/MsgReader.cs` — hook packet logger (nhận)
- `SRCGOPETGOC/GServer/config/server.json` — thêm cờ `enablePacketLog`
- `SRCGOPETGOC/GServer/App.config` — chuyển credential sang biến môi trường

**Create**
- `SRCGOPETGOC/GServer/Logging/PacketLogger.cs` — ghi hex dump, dưới 200 dòng
- `tools/packet-diff/` — script so sánh 2 file dump (Node hoặc Python)

## Implementation Steps

1. **Vá mảng không giới hạn** (`GameController.cs:215`)
   ```csharp
   int len = message.reader().readInt();
   if (len < 2 || len > 64) { player.session.Close(); return; }
   int[] points = new int[len];
   ```
   Hiện tại `points[points.Length - 2]` cũng panic khi mảng < 2 phần tử — chặn luôn.

2. **Vá tương tự** `GameController.cs:784` (`new String[message.readInt()]`).

3. **Đóng HTTP API** — đổi `Application.RunAsync($"http://0.0.0.0:{Port}")` thành `127.0.0.1`. Bật kiểm tra `apiKey` (trường đã có sẵn trong `ServerSetting` nhưng chưa dùng ở đâu).

4. **Gỡ credential** khỏi `App.config` và `config/database.json` → đọc từ biến môi trường. Thêm `.gitignore` cho các file config chứa secret.

5. **Dựng server test** — MySQL riêng, import từ `MariaDB_SQL/*.sql`, đổi `server.json` sang port khác production. Tuyệt đối không dev trên `160.30.160.83`.

6. **Viết `PacketLogger.cs`**
   - Ghi: timestamp, hướng (IN/OUT), opcode, độ dài, hex dump (giới hạn 256 byte đầu)
   - Hook vào `MsgSender.doSendMessage()` và `MsgReader.readMessage()`
   - Bật/tắt bằng `ServerSetting.instance.enablePacketLog`

7. **Dựng emulator J2ME** — thử KEmulator hoặc MicroEmulator. Cần `client.jar` trỏ được vào server test (kiểm tra cách client chọn server: `Player.showNormalServer()` gửi danh sách từ `GopetManager.ServerInfos`, nên thêm entry trỏ về localhost).

8. **Viết packet-diff tool** — đọc 2 file dump, so sánh chuỗi (opcode, length), in ra dòng đầu tiên lệch nhau.

9. **Kiểm chứng đường ống** — chạy client cũ trên emulator, login, đi lại, mở 1 menu. Xác nhận dump có nội dung hợp lý và diff tool chạy được (tự so với chính nó phải ra "không lệch").

## Success Criteria

- [x] Gửi packet dị dạng (mảng độ dài 2 tỷ) không làm sập server
- [x] HTTP API không truy cập được từ máy khác
- [x] Không còn credential nào trong file được commit
- [x] Server test chạy độc lập, không đụng production
- [x] `client.jar` gốc chạy trên emulator, login được vào server test
- [x] Packet dump sinh ra ở cả 2 đầu, đọc được
- [x] packet-diff tool so 2 dump và báo đúng vị trí lệch đầu tiên

### Bằng chứng đã kiểm chứng (2026-09-05)

| Tiêu chí | Cách kiểm | Kết quả |
|---|---|---|
| Packet dị dạng | `tools/smoke-test/hardening.js` — 4 ca: độ dài 2 tỷ, 0, âm, và `TYPE_DIALOG_INPUT` 2 tỷ | 4/4 PASS: server ngắt kết nối kẻ gửi, vẫn phục vụ client hợp lệ ngay sau đó |
| HTTP API | `curl` từ `127.0.0.1` và từ `192.168.1.9` | 200 từ loopback, connection refused từ LAN |
| Fail-closed | Đặt `httpBindAddress: "0.0.0.0"` với `apiKey` rỗng | Server **từ chối khởi động** kèm lý do rõ ràng |
| Credential | `App.config` không còn mật khẩu; đọc từ `GOPET_DB_*` | `[DB] OK` cho cả 3 connection string |
| Server test | Docker MariaDB **10.4** (khớp bản gốc), bind loopback | 50+18+1 bảng, GServer nạp toàn bộ template OK |
| Packet dump | Bật `enablePacketLog`, chạy `handshake.js` | Ghi đúng chiều IN/OUT, opcode, cờ mã hoá, hex |
| packet-diff | So dump với chính nó, rồi với bản đã sửa 1 opcode | Báo OK / báo đúng vị trí lệch đầu tiên |

**Test tự kiểm:** tắt server rồi chạy lại `hardening.js` → báo FAIL. Test có khả năng thất bại thật, không phải luôn xanh.

### Emulator J2ME — đã login được vào server test (2026-09-05)

**FreeJ2ME** (`TASEmulators/freej2me`), build từ nguồn bằng `javac --release 8`. Xem `tools/README.md`.

| Bước | Trạng thái |
|---|---|
| Build FreeJ2ME | ✅ `freej2me_plus.jar` 1.5 MB |
| Vá `client.jar` trỏ về `127.0.0.1` | ✅ `patch-client-server.js`, đã tự kiểm chứng |
| Emulator nạp được MIDlet | ✅ sinh ra `config/MGOMidlet/game.conf` |
| Đặt 320×240 landscape | ✅ khớp `Nokia-MIDlet-App-Orientation` |
| **Client login vào server test** | ✅ **xong** — vào tới màn chọn nhân vật |

Dấu vết trong `packet-dump-server.log` của phiên đăng nhập thành công:

```
IN  -36 109B  CLIENT_INFO: FreeJ2ME-Plus...;CLDC-1.1;MIDP-2.0;gopet, ver 1.4.3, 320x240, vi
IN   64   1B  xin danh sách máy chủ
OUT -36   2B  dc01 — server chấp nhận client
OUT  64  41B  "Localhost" / "127.0.0.1"
IN    1  77B  LOGIN gopettest
OUT  21  10B  chấp nhận — KHÔNG phải dialog lỗi (opcode 4/10)
```

Đúng 320×240 như đã cấu hình, nên client Unity đối chiếu được mà không lệch vì độ phân giải.

**Nút thắt thật không phải bàn phím mà là `socket://`.** `Connector.java` của FreeJ2ME
trả về `HttpConnectionImpl` cho cả `socket://`. Client ép kiểu sang `SocketConnection`
(`eo.java:69`) → `ClassCastException`, bị `catch (Exception)` nuốt → treo vĩnh viễn ở
"Đang kết nối…" mà không một dòng lỗi nào. Phải viết `SocketConnectionImpl` (TCP thuần).

Ba bản vá FreeJ2ME + bẫy bộ gõ tiếng Việt + cách tạo tài khoản test: xem `tools/README.md`.

Hai điểm kỹ thuật đáng ghi:

- Build phải dùng `--release 8`. FreeJ2ME vendor `org.xml.sax`/`javax.xml` trong `src`; từ Java 9 chúng thuộc module `java.xml` và JDK cấm package split → release 11 hỏng 74 lỗi.
- Client không có màn hình nhập địa chỉ. `fb.java:335` hardcode `dw("test", "160.30.136.115", 19180)` làm fallback khi RMS chưa có `server_list`. Vá chuỗi trong constant pool của `fb.class` — an toàn vì class file tham chiếu bằng index, không phải offset byte.

Độ phân giải gửi lên server qua `CLIENT_INFO`, nên client Unity phải đặt **cùng 320×240** khi đối chiếu, nếu không server có thể trả nội dung khác.

### Hai lỗi phía server phát hiện lúc chạy thật — đã sửa (2026-09-05)

Không nằm trong phạm vi P1 ban đầu, nhưng đều đập thẳng vào client Unity nếu để lại.

#### 1. Gói cuối trước khi đóng phiên bị mất

**Triệu chứng.** Server ghi log là đã gửi opcode 10 ("Phiên bản cũ rồi, bạn vui lòng tải
bản mới nhất") rồi đóng kết nối. Không client nào nhận được — kiểm chứng bằng cả client
C# lẫn một client JS độc lập, cả hai mất y hệt, nên không phải lỗi tầng vận chuyển client.

**Nguyên nhân thật.** Không phải TCP abortive như đoán ban đầu. `Session.Close()` gọi
`sendThread.Interrupt()` **trước khi** luồng gửi kịp đẩy hàng đợi, rồi `Exit()` gọi
`MsgSender.stop()` — mà hàm này có `sendingMessage.Clear()`, xoá thẳng mọi gói còn chờ.

Vì vậy vài chỗ trong code phải `Thread.Sleep(1000)` trước `Close()` cho gói kịp đi
(đường login sai mật khẩu có sleep nên chạy đúng; đường từ chối version không có sleep
nên mất gói). Một cái bẫy chờ sẵn cho mọi đường đóng phiên viết sau này.

**Sửa.** `MsgSender.requestDrain()` + `waitDrained()`: luồng gửi đẩy hết hàng đợi rồi tự
thoát, `Session.Close()` chờ tối đa 1 giây trước khi ngắt. Thêm `Shutdown(SocketShutdown.Send)`
để đóng bằng FIN thay vì RST.

`Thread.Sleep(1000)` ở `Player.cs:658` (sau khi login sai) **không còn cần cho việc gói tới
nơi** nữa. Vẫn để nguyên vì nó còn tác dụng hãm brute-force — gỡ hay giữ là quyết định về
bảo mật, không phải dọn dẹp, nên để người quyết. Nhưng lưu ý nó chặn luồng đọc của phiên
đúng 1 giây mỗi lần đăng nhập sai.

| Kiểm | Trước | Sau |
|---|---|---|
| Client gửi version `1.0.0` | hết 10s không nhận gì | nhận đủ opcode 10 (dialog) **và** `-36 dc00` |
| `hardening.js` gói dị dạng | 4/4 PASS | 4/4 PASS, thêm "có phản hồi trước đó: true" |
| Đăng nhập bình thường | OK | OK — client J2ME vào tới **trong map** |

#### 2. Cổng game bind `0.0.0.0`

P1 đã siết HTTP API về loopback nhưng bỏ sót cổng game (`Server.cs:28` dùng
`IPAddress.Any`), nên server test trên máy dev mở ra toàn mạng LAN — cùng với tài khoản
test mật khẩu yếu và giao thức TEA dùng khoá do chính client gửi lên.

**Sửa.** Thêm `gameBindAddress` vào `server.json`. Mặc định giữ `0.0.0.0` — production
cần người chơi kết nối từ ngoài, đổi mặc định là làm sập mọi bản đang chạy. Config của
máy test đặt `127.0.0.1`.

Khởi động in rõ đang nghe ở đâu, để không phải đi đọc config mới biết:

```
[Máy chủ] Cổng game nghe ở 127.0.0.1:19180 (chỉ cục bộ)
```

| Kiểm | Kết quả |
|---|---|
| Kết nối từ `127.0.0.1` | được |
| Kết nối từ `192.168.1.9` (IP LAN của chính máy) | **connection refused** |
| Địa chỉ sai định dạng trong config | server từ chối khởi động, báo rõ lý do |

Muốn chạy Unity Editor và emulator ở hai máy khác nhau thì đổi lại `0.0.0.0` — một dòng
trong `config/server.json`.

## Risk Assessment

| Rủi ro | Xử lý |
|---|---|
| Emulator J2ME không chạy được `client.jar` (MIDP-2.1/CLDC-1.1, có SDK riêng `thong/sdk`) | Thử lần lượt KEmulator → MicroEmulator → J2ME Loader (Android). Nếu tất cả fail, chuyển sang chiến lược dump server-side thuần: ghi lại session của người chơi thật làm bộ test replay |
| Client cũ hardcode địa chỉ server | Đọc `cx.java`/`aj.java` trong bản decompile để tìm cách trỏ về localhost. Fallback: sửa `ServerInfos` trong DB test |
| Vá mảng làm hỏng hành vi hợp lệ | Log lại độ dài thực tế trong 1 phiên chơi bình thường trước khi chọn ngưỡng |

## Next Steps

Xong P1 → P2 (Transport Layer). P2 phụ thuộc trực tiếp vào packet logger của P1.
