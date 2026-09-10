# Đánh giá khả thi: dựng lại client goPet bằng Unity, giữ nguyên GServer

Ngày: 2026-09-05
Phạm vi: `D:\game\client.jar_Decompiler.com` (221 file Java decompile) + `D:\game\SRCGOPETGOC\GServer` (server C# .NET 8)

---

## 0. Kết luận ngắn

**Khả thi — và khả thi cao hơn bình thường**, nhưng **không phải bằng cách port code decompile**.

Ba lý do khiến dự án này dễ hơn một cuộc "viết lại client" thông thường:

1. **Bạn có source server.** Đó mới là đặc tả giao thức chuẩn xác. Code decompile chỉ là bản đối chiếu.
2. **UI do server điều khiển.** 162 màn hình menu/dialog đều đi qua **một** định dạng gói duy nhất (`showMenuItem`) — Unity chỉ cần 1 renderer generic, không phải 162 màn hình.
3. **Asset đã nằm sẵn trên server dưới dạng PNG** (10.586 file) và được stream theo đường dẫn. Unity nạp thẳng bằng `Texture2D.LoadImage()`.

Điều **không** khả thi: biên dịch lại đống code decompile. **139/221 file (63%) không compile được** — sẽ giải thích ở mục 2.

**Ước lượng:** 2-3 tháng cho vertical slice chơi được (login → map → di chuyển → chat → battle cơ bản), 5-8 tháng để đạt parity, với 1-2 dev có kinh nghiệm. Rủi ro chính là **desync giao thức**, không phải Unity.

---

## 1. Bản decompile — chất lượng và giá trị thật

### Chất lượng: tốt

Decompiler (FernFlower/Procyon) đọc ra code sạch, logic đầy đủ. Ví dụ `er.java` khớp **1:1** với `Server/IO/TEA.cs`:

```java
// er.java — client
for(int var6 = 0; var3-- > 0; var5 += ((var4 << 4) + this.a[2] ^ var4) + (var6 ^ var4 >>> 5) + this.a[3]) {
   var6 -= 1640531527;
   var4 += ((var5 << 4) + this.a[0] ^ var5) + (var6 ^ var5 >>> 5) + this.a[1];
}
```

Cùng hằng số `1640531527`, cùng cấu trúc với `TEA.brew()` trong `Server/IO/TEA.cs`. Không có nghi ngờ gì về tính đúng đắn.

### Nhưng: code KHÔNG compile được

Obfuscator đã đổi tên theo kiểu chỉ hợp lệ ở tầng bytecode. Java bytecode phân biệt field bằng **tên + kiểu (descriptor)**, nên `eo.java` hợp lệ khi là `.class`:

```java
public final class eo {
   public cy a;                    // <- 8 field
   public DataOutputStream a;      // <- cùng tên "a"
   public DataInputStream a;       // <- kiểu khác nhau
   private SocketConnection a;
   public boolean a;
   private eq a;
   private ep a;
   public int a;
   public er a;
   public String a;
}
```

Ở tầng **source** Java thì đây là lỗi biên dịch. Ngoài ra decompiler còn sinh ra cú pháp không tồn tại:

```java
var10000.<init>(var10002);   // ep.java — không phải Java hợp lệ
```

**Đếm được: 139/221 file (63%) có field trùng tên.** Bao gồm toàn bộ lớp mạng trọng yếu (`eo`, `en`, `ep`, `eq`).

### Kết luận về vai trò của bản decompile

| Dùng để | Đánh giá |
|---|---|
| Biên dịch lại / port trực tiếp | **Không được.** 63% file lỗi cú pháp, 213/221 class tên vô nghĩa |
| Đặc tả giao thức mạng | **Xuất sắc.** Đối chiếu chéo với server, độ tin cậy rất cao |
| Đặc tả format file `.dat` | **Xuất sắc.** Đây là thứ **chỉ** có ở client, không có ở server |
| Tham chiếu logic UI/animation | Tốt, nhưng phải đọc thủ công vì tên bị obfuscate |

**Giá trị lớn nhất của bản decompile chính là format `.dat`** — map, sprite, animation. Đây là phần duy nhất server không mô tả được.

---

## 2. Giao thức mạng — đã khôi phục 100%

Đây là phần quan trọng nhất và tin tốt là nó **nhỏ và đã rõ hoàn toàn**.

### Handshake (từ `eq.java:a(long)`)

```
Client -> Server: 9 byte
  [0]     = 0x09              (độ dài)
  [1..8]  = System.currentTimeMillis(), big-endian
```

Server (`Session.readKey()`, `Session.cs:80`) đọc 9 byte, lấy `[1..8]` làm khoá TEA. Khớp chính xác.

### Khung gói tin

```
[int32 BE: length+1][byte: isEncrypted][payload...]
payload[0] = opcode (sbyte), phần còn lại = body
```

Nếu `isEncrypted == 1` thì payload đã qua TEA 128-bit. Đối chiếu `eq.java` (gửi) / `ep.java` (nhận) với `MsgSender.doSendMessage()` / `MsgReader.readMessage()` — trùng khớp từng dòng.

### Chi tiết cần lưu ý khi viết bằng C#/Unity

| Vấn đề | Xử lý |
|---|---|
| **Big-endian** | Java/J2ME là BE, `BinaryReader` của .NET là LE. Phải tự viết reader/writer — copy đúng logic `Server/IO/IOExtension.cs:68-91` |
| **`writeUTF`** | Server (`DataOutputStream.cs:137`) ghi **UTF-8 thường** + prefix 2 byte length, **không** phải Java modified UTF-8. → C# dùng `Encoding.UTF8` là đúng. (Ngoại lệ: ký tự ngoài BMP như emoji sẽ lệch, Java ghi CESU-8 6 byte, .NET ghi 4 byte — hiếm gặp) |
| **`sbyte` vs `byte`** | Server dùng `sbyte[]` xuyên suốt để mô phỏng `byte[]` của Java. Unity client nên dùng `byte[]` và cast khi so sánh opcode âm |
| **Opcode âm** | `setClientOK` gửi opcode `-36`. Phải xử lý như `sbyte`, không phải `byte` |

**Khối lượng:** toàn bộ tầng transport (socket + TEA + Message + IO extension) ước chừng **200-300 dòng C#**. Rủi ro thấp, đặc tả đầy đủ. Đây là việc nên làm đầu tiên và có thể xong trong 1 tuần.

### Handshake ứng dụng (`Player.cs:104-129`)

Client phải gửi `CLIENT_INFO` theo đúng thứ tự:

```
sbyte  CLIENT_TYPE
int    PROVIDER
UTF    version        -> phải >= VERSION_142, nếu không server đóng kết nối
UTF    info
int    displayWidth
int    displayHeight
UTF    languageCode   -> phải có trong GopetManager.Language, nếu không server đóng kết nối
UTF    Refcode
```

Hai chỗ này (`version`, `languageCode`) là "cổng chặn" — sai là bị `session.Close()` ngay, không có thông báo lỗi. Cần chú ý khi debug lần đầu.

---

## 3. Tại sao dự án này dễ hơn bạn nghĩ

### 3.1 UI hoàn toàn do server điều khiển — đây là điểm mấu chốt

`MenuController` có **162 hằng số** `MENU_*` / `DIALOG_*` / `INPUT_*`. Nhưng tất cả đều được đóng gói bằng **một** hàm duy nhất (`GameController.cs:818`):

```csharp
public void showMenuItem(int listID, sbyte type, String title, JArrayList<MenuItemInfo> menuItemInfos)
{
    Message message = new Message(GopetCMD.COMMAND_GUIDER);
    message.putsbyte(GopetCMD.SHOW_MENU_ITEM);
    message.putInt(listID);
    message.putsbyte(type);
    message.putUTF(title);
    message.putInt(menuItemInfos.Count);
    for (...) {
        message.putInt(itemId);
        message.putUTF(imgPath);      // đường dẫn asset trên server
        message.putUTF(titleMenu);
        message.putUTF(desc);
        message.putsbyte(canSelect);
        message.putbool(showDialog);
        if (showDialog) { putUTF(dialogText); putUTF(leftCmd); putUTF(rightCmd); }
        message.putsbyte(saleStatus);
        message.putbool(closeScreenAfterClick);
        // + mảng PaymentOption
    }
}
```

Server gửi: tiêu đề, danh sách item (icon + tên + mô tả + có chọn được không + text dialog). Client chỉ **render một danh sách** rồi gửi lại index đã chọn.

**Hệ quả:** Unity không cần dựng 162 màn hình. Cần đúng **4 component generic**:

1. Menu danh sách (icon + title + desc, cuộn được)
2. Dialog Yes/No
3. Dialog nhập liệu
4. Dialog NPC option

Shop, kho đồ, clan, kiosk, nhiệm vụ, xăm, ghép đồ... đều chạy qua 4 component này. **Đây là thứ tiết kiệm nhiều tháng công.**

### 3.2 Asset stream sẵn từ server dưới dạng PNG

`GameController.requestImg()` (dòng 864): client gửi đường dẫn, server đọc file từ `GServer/assets/` và trả về **raw PNG bytes**.

```
GServer/assets/  = 141 MB, 10.586 file PNG
client.jar       = chỉ 327 PNG (phần bootstrap)
```

Unity chỉ cần:

```csharp
var tex = new Texture2D(2, 2);
tex.LoadImage(pngBytes);   // Unity giải mã PNG sẵn
```

Không cần export asset, không cần convert. Nên thêm cache đĩa (client J2ME cũ đã làm vậy qua RMS, xem `ef.java`) để không tải lại mỗi lần.

### 3.3 Format map đơn giản, hợp với Unity Tilemap

Từ `ef.java:104-130`:

```
byte    numTileImages
byte    numExtraTiles
short[] tileImageIds      (numTileImages + numExtraTiles phần tử)
byte[]  tileTypes
byte    widthInTiles
byte    heightInTiles
byte    numLayers
byte[numLayers][height][width]  tile indices
```

Tile 24×24 px, nhiều layer, index 1 byte. **Map thẳng sang Unity `Tilemap` gần như 1:1.** Rủi ro thấp.

Có sẵn: 24 map trong jar + 265 file `newMapData/` + 44 trong `GServer/assets/maps`.

### 3.4 Có thể chạy song song hai client trên cùng server

Vì bạn **không đổi giao thức**, client J2ME cũ và client Unity mới có thể cùng kết nối vào một GServer. Điều này cho phép:

- Đối chiếu hành vi từng bước khi debug
- Migrate dần, không cần "big bang"
- Người chơi cũ không bị gián đoạn

**Đây là chiến lược nên chọn.** Đừng đụng vào giao thức trong giai đoạn 1.

---

## 4. Khối lượng công việc thật

### Con số đo được từ source server

| Chỉ số | Giá trị |
|---|---|
| Opcode server xử lý (client → server) | 119 |
| Opcode server gửi đi (server → client) | 21 top-level |
| Lệnh `put*` (field client phải parse đúng thứ tự) | **696** |
| Lệnh `read*` (field client phải gửi) | 145 |
| Màn hình menu/dialog | 162 (nhưng dùng chung 4 renderer) |

**696 vị trí `put*` chính là khối lượng thật.** Mỗi cái là một field client phải đọc đúng kiểu, đúng thứ tự. Sai một byte là desync toàn bộ stream.

Tin tốt: chúng tập trung vào **2 opcode bao ngoài**:

- `PET_SERVICE` (81) — 16 nơi tạo message, mang ~60 sub-command gameplay
- `COMMAND_GUIDER` (122) — 6 nơi, mang toàn bộ UI

Nghĩa là bạn viết 2 dispatcher lớn, không phải 119 handler rời rạc.

### Phân rã theo giai đoạn

| Giai đoạn | Nội dung | Ước lượng | Rủi ro |
|---|---|---|---|
| **P1** | Transport: TCP + TEA + Message + BE IO + handshake | 1 tuần | Thấp — đặc tả đầy đủ |
| **P2** | Login / register / chọn server / CLIENT_INFO gate | 1 tuần | Thấp |
| **P3** | 4 UI component generic (menu, yes/no, input, npc option) | 3-4 tuần | Thấp |
| **P4** | Asset pipeline: request → PNG → Texture2D + cache đĩa | 1 tuần | Thấp |
| **P5** | Map: parse `.dat` → Unity Tilemap, nhân vật, di chuyển, waypoint | 3-4 tuần | **Trung bình** — phải reverse `.dat` |
| **P6** | Pet battle: render + gửi action + hiệu ứng skill | 4-6 tuần | **Trung bình** — nhiều state |
| **P7** | Đuôi dài: kho đồ, clan, kiosk, nhiệm vụ, thư, BXH, event | 6-10 tuần | Trung bình — nhiều nhưng lặp lại |
| **P8** | Sprite/animation `.anu`, âm thanh, polish | 3-4 tuần | Trung bình |

**Vertical slice chơi được (P1-P5): ~2-3 tháng.**
**Parity đầy đủ: ~5-8 tháng** với 1-2 dev.

---

## 5. Rủi ro và cách xử lý

### 5.1 Desync giao thức — rủi ro số 1

Sai thứ tự đọc một field → toàn bộ stream lệch → lỗi xuất hiện ở chỗ hoàn toàn khác. Đây là thứ giết chết tiến độ, không phải Unity.

**Cách xử lý (bắt buộc làm từ đầu):**

- Viết **packet logger** ở cả server (`MsgSender.doSendMessage`) và Unity client, dump hex + opcode ra file.
- Chạy client J2ME cũ trên emulator (KEmulator / Sjboy / MicroEmulator) song song với client Unity, **so sánh dump từng gói**. Sai ở đâu thấy ngay ở đó.
- Sinh code parser từ source server nếu được — 696 vị trí `put*` là quá nhiều để làm tay không sai.

### 5.2 Unity WebGL không dùng được TCP thuần — điểm quyết định

`System.Net.Sockets` **không chạy** trên WebGL. Nếu muốn build web thì cần WebSocket proxy đứng giữa (client WS ↔ proxy ↔ TCP GServer). Không khó (~200 dòng Node/C#) nhưng phải quyết **ngay từ đầu** vì ảnh hưởng kiến trúc.

Android / iOS / Windows / macOS: TCP thuần chạy bình thường, không vấn đề.

> **Cần bạn quyết:** target platform là gì?

### 5.3 Format `.dat` và `.anu` — phần reverse engineering thật sự

Đây là phần **duy nhất** server không mô tả. Phải đọc `ef.java` (map), `dy.java` (sprite/animation), `a.java` để dựng parser. Map thì đã rõ (mục 3.3), animation `.anu` chưa khảo sát.

**Giảm rủi ro:** viết tool CLI đứng riêng, đọc `.dat` xuất ra PNG/JSON, kiểm tra bằng mắt trước khi nhúng vào Unity.

### 5.4 Server hiện có lỗ hổng nghiêm trọng

Xem báo cáo trước (`analysis-260905-1330-gopet-server-source.md`). Đáng chú ý với dự án này:

- `GameController.cs:215` — `new int[readInt()]`, client tự quyết độ dài mảng. Trong lúc dev, client Unity gửi sai một int là **giết server**. Nên vá trước khi bắt đầu.
- HTTP API `:8082` mở toang, không auth.
- Chống hack-move và ban speed-hack **đã bị comment** (`GameController.cs:227-241`, `Player.cs:320`). Client Unity dễ bị hack hơn client J2ME obfuscate — cần bật lại validate server-side **trước** khi phát hành.

### 5.5 Không nên đổi giao thức ở giai đoạn 1

Cám dỗ sẽ rất lớn: TEA yếu, khoá do client gửi, protocol rườm rà. **Đừng.** Đổi giao thức = mất khả năng chạy song song + mất khả năng đối chiếu với client cũ, đúng lúc bạn cần nó nhất. Để dành cho giai đoạn 2, sau khi client Unity đã đạt parity.

---

## 6. Kiến trúc đề xuất cho Unity client

```
Assets/Scripts/
├── Net/
│   ├── GopetSocket.cs        TCP + thread nhận/gửi + hàng đợi
│   ├── Tea.cs                port thẳng từ Server/IO/TEA.cs
│   ├── Message.cs            khung gói tin, mirror Server/IO/Message.cs
│   ├── JavaBinaryReader.cs   big-endian + readUTF
│   ├── JavaBinaryWriter.cs   big-endian + writeUTF
│   └── GopetCmd.cs           copy y nguyên từ Server/GopetCMD.cs
├── Protocol/
│   ├── Handlers/             1 file / nhóm opcode
│   └── PacketLogger.cs       dump hex để đối chiếu
├── Assets/
│   └── RemoteAssetCache.cs   request path -> PNG -> Texture2D + cache đĩa
├── UI/
│   ├── GenericMenuView.cs    phục vụ toàn bộ 162 menu
│   ├── YesNoDialog.cs
│   ├── InputDialog.cs
│   └── NpcOptionDialog.cs
├── World/
│   ├── MapDatParser.cs       .dat -> Tilemap
│   ├── PlayerController.cs
│   └── MapRenderer.cs
└── Battle/
    └── BattleScene.cs
```

**Nguyên tắc:** `GopetCmd.cs` phải là bản copy nguyên văn của `Server/GopetCMD.cs`. Cân nhắc viết script sinh tự động file này từ source C# server để không bao giờ lệch.

Áp dụng quy tắc dự án: file dưới 200 dòng, kebab-case cho asset, mỗi handler một file.

---

## 7. Khuyến nghị

**Nên làm.** Nhưng theo thứ tự này:

1. **Tuần 0** — Vá `GameController.cs:215` và `:784` (giới hạn độ dài mảng), đóng HTTP API vào `127.0.0.1`. Không có bước này thì client Unity đang dev sẽ liên tục làm sập server test.
2. **Tuần 1** — Dựng tầng transport + packet logger. Mục tiêu duy nhất: kết nối được, qua handshake, server không đóng kết nối.
3. **Tuần 2-3** — Login thành công, vào được nhân vật. Đây là mốc chứng minh khả thi. **Nếu tới được đây thì phần còn lại chỉ là khối lượng, không còn là rủi ro.**
4. Sau đó theo P3-P8.

**Không nên:** cố sửa 139 file decompile cho compile được rồi port sang C#. Chi phí cao hơn viết mới, kết quả là code không ai đọc nổi (tên `a`, `eo`, `gw`), và vẫn phải viết lại toàn bộ tầng render vì J2ME `Graphics` không có tương đương trong Unity (74 file phụ thuộc `javax.microedition.lcdui`, 227 lời gọi `drawImage`/`drawString`).

**Cách dùng bản decompile cho đúng:** giữ nó làm tài liệu tra cứu. Khi không rõ server gửi cái gì thì đọc `GameController.cs`; khi không rõ format file `.dat` thì đọc `ef.java`/`dy.java`. Đừng compile nó.

---

## 8. Câu hỏi cần bạn trả lời trước khi bắt đầu

1. **Target platform?** Nếu có WebGL thì phải thiết kế WebSocket proxy ngay từ đầu — ảnh hưởng toàn bộ tầng Net.
2. **Giữ nguyên gameplay hay nhân dịp làm mới?** Giữ nguyên = giữ protocol = dễ hơn nhiều. Đổi gameplay = phải sửa cả server.
3. **Có emulator J2ME chạy được client cũ không?** Rất quan trọng cho việc đối chiếu gói tin. Nếu không có, thời gian debug sẽ tăng đáng kể.
4. **Nhân sự và thời gian?** Con số 5-8 tháng dựa trên 1-2 dev có kinh nghiệm Unity + hiểu networking. Ít hơn thì nên thu hẹp phạm vi P7 (bỏ event theo năm, clan nâng cao).
5. **Server production `160.30.160.83` có đang chạy không?** Nếu có, cần dựng server test riêng — đừng dev client mới trên server thật (xem rủi ro 5.4).
6. **Có bản `client.jar` version nào cũ hơn/mới hơn không?** Đối chiếu 2 version giúp xác định field nào là mở rộng về sau, hữu ích khi đọc protocol.
