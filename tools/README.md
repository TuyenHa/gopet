# Emulator J2ME — chạy client goPet gốc

Chạy `client.jar` gốc để **đối chiếu gói tin** với client Unity đang viết. Đây là công cụ chính chống desync giao thức — rủi ro số 1 của cả dự án.

Cách dùng: làm **cùng một thao tác** trên client cũ và client Unity, rồi so hai packet dump. Lệch ở đâu thấy ngay ở đó.

## Vì sao FreeJ2ME

Mã nguồn mở, còn được bảo trì (fork `TASEmulators/freej2me`), chạy trên Java 17 có sẵn. KEmulator là binary đóng và đã cũ.

## Dựng

```bash
cd tools
git clone --depth 1 https://github.com/TASEmulators/freej2me.git freej2me
bash build-freej2me.sh
```

Kết quả: `tools/freej2me/build/freej2me_plus.jar` (~1.5 MB)

### Vì sao build tay chứ không dùng Ant

`build.xml` của dự án đặt `source/target="1.6"` và `bootclasspath="${java.home}/lib/rt.jar"`. JDK 17 không còn `rt.jar`, cũng không còn hỗ trợ target 1.6 (tối thiểu là 7). Chạy Ant sẽ hỏng ngay bước đầu.

`build-freej2me.sh` gọi thẳng `javac` với `--release 8`.

**Phải là 8, không được 11 trở lên.** FreeJ2ME vendor sẵn `org.xml.sax` và `javax.xml.namespace` trong `src`. Từ Java 9, những package đó thuộc module `java.xml` và JDK cấm "package split" — biên dịch với release 11 hỏng 74 lỗi kiểu *"X is already defined in this compilation unit"*. Release 8 dùng mô hình classpath cũ, cho phép class vendor che class của JDK.

## Trỏ client về server cục bộ

Client **không có màn hình nhập địa chỉ**. Khi RMS chưa có record `server_list` (lần chạy đầu), `fb.java:335` fallback về đúng một entry hardcode:

```java
dw var7 = new dw("test", "160.30.136.115", 19180);
```

`patch-client-server.js` vá chuỗi đó trong constant pool của `fb.class`:

```bash
node patch-client-server.js ../SRCGOPETGOC/client.jar 127.0.0.1
# -> SRCGOPETGOC/client-127.0.0.1.jar
```

An toàn vì class file Java tham chiếu constant pool bằng **index**, không phải offset byte — đổi độ dài một entry `CONSTANT_Utf8` làm file dài/ngắn đi nhưng không hỏng tham chiếu nào. Script tìm theo mẫu `0x01 <u2 độ_dài> <bytes>` nên gần như không thể nhầm, và từ chối chạy nếu tìm thấy nhiều hơn một entry khớp.

File gốc không bị sửa — luôn ghi ra bản sao. Sau khi vá, script tự đọc lại từ jar để kiểm chứng.

## Chạy

```bash
# 1. Database
cd docker && docker compose up -d

# 2. GServer (bật enablePacketLog trong config/server.json)
powershell -ExecutionPolicy Bypass -File docker\run-gserver.ps1

# 3. Client cũ
cd tools/freej2me
java -jar build/freej2me_plus.jar "D:\game\SRCGOPETGOC\client-127.0.0.1.jar"
```

Cửa sổ game mở ra — MIDlet cần thao tác bàn phím để qua màn hình đầu và kết nối.

### Phím

Mặc định của FreeJ2ME (đổi được trong menu của nó):

| Phím máy | Phím điện thoại |
|---|---|
| Mũi tên | D-pad |
| Enter | Fire / OK |
| Q / W | Soft trái / phải |
| 0-9 | Phím số |

## Cấu hình

| File | Nội dung |
|---|---|
| `freej2me/config/MGOMidlet/game.conf` | Riêng cho goPet — độ phân giải, FPS, cờ tương thích |
| `freej2me/freej2me_system/freej2me.conf` | Ánh xạ phím toàn cục |

Đã đặt **320×240 landscape** cho khớp `Nokia-MIDlet-App-Orientation: landscape` trong `META-INF/MANIFEST.MF` của client (mặc định của FreeJ2ME là 240×320 dọc).

Độ phân giải này được gửi lên server trong gói `CLIENT_INFO` (`displayWidth`/`displayHeight`), nên đặt cho client Unity **cùng giá trị** khi đối chiếu — khác nhau thì server có thể trả nội dung khác.

## Đối chiếu gói tin

```bash
# Sau khi thao tác trên cả 2 client
node ../GopetUnityClient/tools/packet-diff/index.js \
     ../SRCGOPETGOC/GServer/bin/Debug/net8.0/log/packet-dump-server.log \
     <dump-cua-unity> \
     --direction IN --hex
```

> **Chú ý hướng:** `OUT` của server là `IN` của client và ngược lại. Dùng `--direction` cho đúng chiều.

## Ba chỗ phải vá trong FreeJ2ME

Nguồn vendor trong `freej2me/src`, build lại bằng `build-freej2me.sh`.

### 1. `socket://` trả về kết nối HTTP (chặn hoàn toàn)

`Connector.java` gộp `socket://` chung nhánh với `http://` và trả `HttpConnectionImpl`.
goPet ép kiểu sang `SocketConnection` (`eo.java:69`) → `ClassCastException`, mà chỗ
gọi bọc trong `catch (Exception)` nên **không có lỗi nào hiện ra** — client chỉ đứng
mãi ở "Đang kết nối…". Đây là nút thắt thật, không phải bàn phím.

Thêm `javax/microedition/io/SocketConnectionImpl.java` (TCP thuần, `TCP_NODELAY`)
và tách `socket://` ra khỏi nhánh HTTP.

### 2. TextBox không gõ được bằng bàn phím

FreeJ2ME chỉ có kiểu nhập "carousel": UP/DOWN cuộn bảng ký tự, FIRE chèn một ký tự —
mô phỏng phím điện thoại. Gõ tài khoản + mật khẩu kiểu đó mất hàng phút.

Thêm `TextBox.typeChar()` / `TextBox.backspace()`, và trong `FreeJ2ME.java` định tuyến
phím khi TextBox đang mở: gõ thẳng, Backspace xoá, Enter = phím mềm trái (OK),
Esc = phím mềm phải, mũi tên vẫn đi đường cũ. Đường carousel giữ nguyên.

Phải **nuốt** phím chữ ở nhánh này chứ không đẩy tiếp: mặc định `input_LeftSoft = Q`,
`input_CLR = A`, `input_Star = E` — không nuốt thì gõ chữ 'a' sẽ xoá sạch ô.

### 3. Phím số map vào numpad

`input_Num0..9` mặc định là `VK_NUMPAD0..9` (96–105). Laptop không có numpad thì không
bấm được phím số nào. Đổi sang hàng số trên (`VK_0..VK_9`, 48–57) trong
`freej2me_system/freej2me.conf`.

## Bộ gõ tiếng Việt làm hỏng chữ nhập vào

Unikey/Telex chạy nền sẽ nuốt phím: gõ `test1234` ra `tét1234` (`e`+`s` → `é`).
Emulator không biết gì về chuyện này — nó nhận đúng cái IME đưa xuống.

**Chuyển sang chế độ gõ tiếng Anh trước khi nhập vào emulator.**

## Tài khoản test

| Tài khoản | Mật khẩu | Dùng cho |
|---|---|---|
| `gopettest` | `abc12345` | thao tác tay trên emulator |
| `gopetsmoke` | `abc12345` | harness tự động (`Gopet.Net.LiveSmoke`) |

**Phải là hai tài khoản khác nhau.** Server từ chối đăng nhập trùng ("Người chơi khác
đăng nhập vào tài khoản"), nên dùng chung thì bài test tự động đỏ mỗi lúc emulator đang mở.

Cả hai nằm trong `gopettae_gopet_web`.

Hai chỗ dễ vướng khi tự tạo tài khoản:

- Mật khẩu là BCrypt work factor 12 (`Util/GopetHashHelper.cs`). Không insert plaintext.
- `role` mặc định 0 = `ROLE_NON_ACTIVE` (`Data/User/UserData.cs:20`) → server báo
  "Tài khoản chưa được kích hoạt". Phải đặt `role = 1`.

## Vấn đề đã gặp

**Emulator tự khởi động lại một lần.** Log in `different encoding: Cp1252 while it should be ISO_8859_1. Restarting`. Bình thường — nó tự đặt lại encoding rồi chạy tiếp. Tiến trình java đầu thoát, tiến trình con mới là emulator thật. Truyền `-Dfile.encoding=ISO-8859-1` **không** tránh được (nó so với chuỗi `ISO_8859_1` dùng gạch dưới).

**Nạp thành công thì có `config/MGOMidlet/game.conf`.** Không thấy file này nghĩa là MIDlet chưa nạp được.
