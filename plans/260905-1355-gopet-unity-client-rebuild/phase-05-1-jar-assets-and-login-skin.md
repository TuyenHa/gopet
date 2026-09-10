---
phase: 5.1
title: "Asset từ jar + màn đăng nhập theo bản J2ME"
status: in-progress
priority: P2
effort: "1 tuần"
dependencies: [5]
---

# Phase 5.1: Asset từ jar + màn đăng nhập theo bản J2ME

## Context Links

- Đánh giá gốc: [`plans/reports/analysis-260906-1007-port-giao-dien-va-asset-tu-jar.md`](../reports/analysis-260906-1007-port-giao-dien-va-asset-tu-jar.md)
- Phase trước: [`phase-05-generic-ui-components.md`](./phase-05-generic-ui-components.md)
- Đường ống ảnh **từ server**: [`phase-04-remote-asset-pipeline.md`](./phase-04-remote-asset-pipeline.md)

## Overview

**Ưu tiên P2, không chặn vertical slice.** P5 đã dựng xong bộ khung chức năng nhưng cố
ý không đụng mỹ thuật — kết quả là giao diện hộp xám. Phase này thay lớp da: lấy ảnh và
âm thanh từ `client.jar_Decompiler.com`, dựng lại màn đăng nhập theo bản jar.

**Không đụng một dòng logic nào.** `LoginFlow`, `GuiderHandler`, `UiRoot`, `DialogStack`
giữ nguyên. Đây là lý do phase này rẻ và có thể hoãn hoặc cắt bớt mà không ảnh hưởng P6.

### Ba quyết định đã chốt (2026-09-06)

| Câu hỏi | Chốt |
|---|---|
| Xử lý 320×240 trên máy hiện đại | **Phóng nguyên khối, lọc Point** — vuông vức, đúng chất retro |
| Thanh softkey kiểu J2ME (trái/phải dưới đáy) | **Không.** Build cho iOS/Android/PC, dùng nút cảm ứng bình thường |
| Có bản jar chạy được để đối chiếu không | **Có** — FreeJ2ME. Nghiệm thu bằng ảnh chụp, không bằng đọc code |
| Khung 320×240 áp cho màn nào | **Tất cả** — một hệ toạ độ duy nhất cho cả app |
| Bảng chuỗi | **Lấy VN + EN từ `a.java`** của jar |
| Logo splash | **`meLogo.png`**, đi qua `JarSkin` theo tên nên đổi sau rất rẻ |
| Hướng màn hình | **Nằm ngang, khoá từ lúc mở app** — splash, đăng nhập, chọn máy chủ, trong game đều ngang. Khớp đúng 320×240 (4:3 ngang) của bản jar |

### Ranh giới phạm vi — đọc kỹ chỗ này

"Giống bản jar" ở đây nghĩa là **giống về mỹ thuật**: logo, nền, màu, tile, âm thanh.
**Không giống về cơ chế điều khiển.** Cụ thể ba chỗ cố ý khác:

1. **Không có softkey.** Bản jar có thanh hai nút trái/phải dưới đáy; ta dùng nút thường.
2. **Ô nhập không mở hộp thoại riêng.** `ge.java` (widget ô nhập của jar) `import
   javax.microedition.lcdui.TextBox` — bấm vào ô là J2ME bật hộp nhập **toàn màn hình**
   của máy. Unity nhập ngay tại chỗ. Khác hẳn, và khác theo hướng tốt hơn.
3. **Chữ không thể giống hệt.** J2ME dùng font hệ thống của từng máy, không phải bitmap
   font đóng trong jar. Copy được ảnh, không copy được cách máy Nokia rasterize chữ.

Ai nghiệm thu bằng cách so từng pixel sẽ thấy khác ở ba chỗ trên. Đó là **cố ý**.

## Key Insights

### Format `.dat` kho ảnh đã giải xong, không còn là ẩn số

`gu.java` chỉ 86 dòng. Toàn bộ format:

```
int        count
int×count  offset      <- cộng thêm (count<<2)+4
rồi:       các khối PNG nguyên vẹn, nối đuôi nhau
```

Đã kiểm chứng bằng cách giải thật và đếm chữ ký PNG tại từng offset:

| File | Byte | `count` trong header | Số ảnh THẬT |
|---|---|---|---|
| `lg.dat` | 4.841 | 2 | 1 |
| `common.dat` | 9.396 | 27 | 26 |
| `avatar.dat` | 5.022 | 17 | 16 |
| `buttonicon.dat` | 670 | 4 | 3 |
| `mui.dat` | 5.929 | 9 | 8 |
| **Cộng** | | | **54** |

**Sửa lại số liệu ban đầu: 54 ảnh, không phải 59.** Lúc dò tay ở bước đánh giá, "mục
cuối lệch chữ ký PNG" bị đọc nhầm là lỗi vặt về biên. Implementation hôm nay lần ra
nguyên nhân thật: `gu.a(int)` tính kích thước ảnh bằng `a[i+1] - a[i]`; với ảnh cuối
(`i = count-1`) biểu thức đó cần `a[count]` — **ngoài mảng**. Java nuốt exception,
nghĩa là **bản thân client J2ME gốc cũng không đọc được "ảnh thứ count"**. Mục cuối
trong bảng offset không phải một ảnh — nó là **sentinel đánh dấu hết dữ liệu**. Đã kiểm
chứng: `offset[count-1]` luôn đúng bằng độ dài file, cho cả 5/5 file.

Vậy **số ảnh thật = count − 1**, không phải count. `lg.dat` có đúng 1 ảnh (index 0) —
khớp hoàn hảo với `fb.java` chỉ gọi `gu.a(0)`, không gọi index nào khác.

Xem chi tiết và code tại `tools/unpack-jar-dat/dat-bank.js` + README cùng thư mục.

**`maps/*.dat` KHÔNG cùng format.** Đọc 4 byte đầu ra 51.576.993 mục. Đó là format
riêng, `ef.java:104-130` mô tả, và là rủi ro đã ghi sẵn cho P6. **Không đụng ở phase này.**

### `fb.java` chính là màn đăng nhập

Đã định vị (chỗ báo cáo còn để ngỏ). Constructor `fb()` cho biết gần hết bố cục:

```java
this.c = "LOGIN";
this.b = new ge(a, BaseCanvas.Field158 - gs.m - gs.p, ..., gs.m);   // hàng trên
this.c = new ge(a, BaseCanvas.Field158,             ..., gs.m);   // hàng giữa
this.d = new ge(a, BaseCanvas.Field158 + gs.m + gs.p, ..., gs.m);   // hàng dưới
this.a = new gb(1);
this.b = new gj(a.a(331), gv.a);        // nhãn "Nếu chưa có tên, xin đăng ký"
gu.a("/lg.dat"); this.c = gu.a(0);      // ảnh 173×92
```

Rút ra:

- **Ba hàng widget** xếp dọc, cách đều, neo quanh `BaseCanvas.Field158`
- `gs.m = 20` (chiều cao hàng), `gs.p = 3` (khoảng cách), `gs.o = 8`
- Ảnh `lg.dat[0]` là **173×92** — banner/logo của màn đăng nhập
- Nhãn chuỗi thật (nhánh `BaseCanvas.w >= 240`, khớp bề rộng 320 ta bám theo):
  `298` = "Tên" (nhãn ô tài khoản), `375` = "Mật khẩu" (nhãn ô mật khẩu),
  `331` = "Nếu chưa có tên, xin đăng ký", `353` = dòng bản quyền.
  `348` = "M.Kh:" và `482` = "Gõ lại" là bản RÚT GỌN cho nhánh `w < 240` — không
  dùng ở đây.
  > **Sửa lần thứ hai, lần này đối chiếu trực tiếp với đúng nhánh if trong
  > `fb.java` thay vì grep tự do trên toàn bảng chuỗi.** Bản plan đầu ghi nhầm
  > `203` = "T.Khoản" (đọc lướt output `grep`). Bản sửa lần một ra `5` = "T.Khoản"
  > — CHUỖI đúng nhưng CHỈ SỐ sai: không dòng nào trong `fb.java` gọi `a.a(5)`.
  > Implementation ban đầu dùng luôn số `5` này, và code-reviewer bắt được ở vòng
  > review đầu tiên — cùng lúc bắt được một lỗi thứ hai: tham số truyền vào
  > `MakeField` bị đảo, nên dù chỉ số có đúng thì chuỗi thật cũng chỉ dùng để đặt
  > tên GameObject, không hiện lên màn hình. Bài học: tìm một chuỗi bằng cách grep
  > nội dung không chứng minh được INDEX đúng — phải lần theo đúng lời gọi
  > `a.a(N)` trong chính hàm dùng nó.
- Chuỗi có **hai bản ngôn ngữ** trong `a.java` (VN ~dòng 340, EN ~dòng 613)

### Bản jar KHÔNG có hai bố cục — nó có layout co giãn

Ghi nhầm ở bản plan đầu, đã sửa. `fb.a(int)` tính vị trí từ `BaseCanvas.w` / `.h` lúc
chạy; các nhánh `w > 240`, `h >= 176`, `h > 240` chỉ là tinh chỉnh bên trong **một** bố
cục, không phải hai phiên bản để chọn:

```java
String var3 = BaseCanvas.w > 240 ? a.a(331)                    // câu dài, một dòng
                                 : a.a(330) + " " + a.a(362);  // cùng nội dung, tách để cắt bớt
do { var4.e = var4.e.substring(0, var4.e.length() - 1); }      // cắt dần cho vừa bề ngang
while (var2 > BaseCanvas.w - (gs.p << 1) && var4.e.length() > 0);

this.e = BaseCanvas.h >= 176 ? gs.m + (gs.o << 1)   // màn cao thì giãn dòng
                             : gs.m + gs.p;         // màn thấp thì bóp lại
```

Máy J2ME đời đó có đủ cỡ màn (128×128, 176×220, 240×320, 320×240…) nên client buộc phải
tự co.

**Hệ quả:** "giống bản jar" phụ thuộc cỡ màn hình đem ra so. Và cỡ đó **đã bị chốt từ
P1**, không phải chọn tự do:

```csharp
// Assets/Scripts/Net/Auth/ClientInfo.cs
public int DisplayWidth = 320;   // "server gửi nội dung khác nhau theo độ phân giải"
public int DisplayHeight = 240;
```

Client Unity đang khai 320×240 với server, và server dùng con số đó để quyết định gửi
gì. Nên **chạy FreeJ2ME ở 320×240** rồi khớp theo ảnh chụp đó. Đổi số này là đổi cả nội
dung server trả về, không chỉ đổi hình — nằm ngoài phạm vi phase này.

Ở 320×240 thì `h > 240` là **sai**, nên nhánh canh giữa dùng công thức có bù 20px. Chép
kết quả tính ra ở đúng cỡ này, không chép cả cây if.

### Một hệ toạ độ, HAI mức trung thành

"Áp khung 320×240 cho tất cả" và "chữ to lên" mâu thuẫn nhau nếu hiểu theo nghĩa chặt:
bố cục của jar dùng hàng cao **`gs.m = 20`** cho font máy Nokia (~10-12px). Đặt font 16
vào hàng 20px là tràn.

Gỡ bằng cách tách rõ hai mức, vẫn giữ **một** hệ toạ độ:

| Màn hình | Mức trung thành | Hàng | Lý do |
|---|---|---|---|
| Splash, đăng nhập | **Chép sát** bản jar | 20px như `gs.m` | Chỉ vài nhãn, và đây là chỗ người ta nhìn để nói "giống bản jar" |
| 162 màn menu của server | **Chỉ chung hệ toạ độ** | Rộng rãi (28-32px), chữ to | Chúng **chưa bao giờ** là bố cục của jar |

Điểm mấu chốt của cột cuối: server chỉ gửi **dữ liệu** (tiêu đề, danh sách dòng, cờ) qua
`SHOW_MENU_ITEM`. Hình hài do client tự quyết — P5 đã dựng và nó là của ta, không phải
của jar. Ép hàng 20px của điện thoại 2011 lên đó là tự trói mà chẳng được gì.

### Ảnh trong game không thuộc phase này

Server có **31.772 PNG**, client lấy qua mạng từ P4 (`RemoteAssetCache`). Icon shop,
vật phẩm, pet, NPC đều đi đường đó và **đã chạy**. Phần lấy từ jar chỉ là lớp vỏ do
client tự vẽ: logo, nền, nút, tile bản đồ, âm thanh.

### `TextureFactory` đã đặt sẵn `FilterMode.Point` từ P4

Quyết định "phóng nguyên khối" khớp với đường ống đã có, không phải sửa gì. Nhưng
ảnh **nhập từ đĩa** đi đường Import Settings của Unity chứ không qua `TextureFactory`
— phải đặt riêng, và đó là chỗ dễ quên nhất của cả phase.

## Requirements

**Functional**

- Giải 5 file `.dat` thành PNG rời, tái lập được (chạy lại ra kết quả y hệt)
- 327 PNG + 54 ảnh giải ra nằm trong `Assets/Resources/Jar/`, đúng import setting pixel art
- 10 WAV phát được: nhạc nền theo màn, hiệu ứng khi bấm nút
- Nhớ trạng thái bật/tắt âm thanh giữa hai lần mở app (bản jar có:
  `ISoundManagerSDK.loadMusicState` / `saveMusicState`)
- Màn đăng nhập có logo, nền, bố cục ba hàng theo bản jar

**Non-functional**

- Phóng ảnh **theo bội số nguyên**, lọc Point. Phóng 2.7× là ra pixel méo
- 12 MB WAV phải nén lại — nhạc nền Vorbis, hiệu ứng ngắn để PCM/ADPCM
- Không file `.cs` nào vượt 200 dòng (rule sẵn có, `verify.ps1` bước 8)

## Architecture

### Giải `.dat` bằng tool Node, không bằng script Editor

Theo đúng lệ của repo: `tools/gen-gopet-cmd/` và `tools/gen-test-vectors/` đều là Node
và đều có chế độ `--check` để CI bắt lệch. Việc giải asset cùng tính chất — tất định,
chạy lại được, kiểm được — nên đi cùng đường.

```
tools/unpack-jar-dat/
├── index.js          giải .dat -> PNG, có --check
├── dat-bank.js        format .dat, đọc kỹ trước khi sửa
└── README.md         mô tả format, dẫn nguồn gu.java
```

**Sửa lại đường dẫn đích so với bản plan đầu: `Assets/Resources/Jar/`, không phải
`Assets/Art/`.** `JarSkin`/`SoundBank` nạp asset bằng `Resources.Load(tên)` lúc
runtime — cách duy nhất để tên chuỗi tra ra đúng sprite/clip trong **build thật**
(iOS/Android/PC), không chỉ trong Editor. Đặt ngoài thư mục `Resources/` thì code chạy
tốt trong Editor (Editor có thể tham chiếu asset trực tiếp) nhưng **vỡ trên máy thật**.

Script Editor C# thì chỉ chạy trong Unity, không kiểm được ở `verify.ps1`, và người
review không đọc được kết quả nếu không mở Editor.

### Bảng chuỗi: bóc từ `a.java`, không gõ tay

Cấu trúc đã kiểm, sạch hơn mong đợi:

```java
public static String a(int var0) {
   switch (a) {              // a = ngôn ngữ: 0 = VN, 1 = EN
      case 0:  switch (var0) { case 3: return "Tiện ích"; ... }   // dòng 197-469, 135 case
      case 1:  switch (var0) { case 3: return "Utilities"; ... }  // dòng 470-810, 134 case
      default: return String.valueOf(var0);   // chưa dịch thì trả về chính con số
   }
}
```

Bóc bằng Node, cùng lệ với `gen-gopet-cmd`: `tools/extract-jar-strings/` sinh ra
`Assets/Resources/strings-vi.json` + `strings-en.json`.

Ba chỗ phải cẩn thận:

1. ~~135 ≠ 134 — bản EN thiếu một mục~~ **Sai, đã sửa sau khi bóc thật.** Đây là số
   đếm ẩu bằng `grep -c "case "` trên một dải dòng ước lượng — nó đếm luôn dòng
   `case 0:`/`case 1:` chọn ngôn ngữ, không chỉ case bên trong. Bóc đúng phạm vi
   (`extract-jar-strings/index.js`, giới hạn bằng chuỗi đánh dấu
   `public static String a(int var0) {` và hai lần `default: return
   String.valueOf(var0);`) ra **134 = 134**, khớp nhau tuyệt đối. Vẫn giữ bước so
   hai bảng trong tool (không giả định cân nhau) — lần này đúng nhưng lần sau
   jar đổi thì không chắc.
2. **Escape của Java** — mục 353 là `"
Bản quyền 2011 ME Corp.
..."`. Parser phải hiểu
   `
`, `\"`, `\`.
3. **`default` trả về con số** — thiếu một mục thì màn hình hiện ra số chứ không nổ. Tiện
   lúc chạy, nhưng nghĩa là test phải đối chiếu **số lượng**, không thể trông chờ vào
   việc nó tự vỡ.

Kiểm: các chỉ số đã biết phải ra đúng chữ — `298` = "Tên", `375` = "Mật khẩu",
`331` = "Nếu chưa có tên, xin đăng ký", `353` = dòng bản quyền, `266` = "Đăng nhập"
(nút gửi). Các chỉ số này đã đối chiếu HAI LƯỢT: đọc giá trị từ `strings-vi.json`
**và** xác nhận đúng lời gọi `a.a(N)` nằm trong nhánh `w >= 240` của `fb.java` — bài
học từ lần sửa `5`/`203` ở trên là chỉ đọc đúng NỘI DUNG chuỗi không đủ, phải khớp
đúng CHỈ SỐ mà hàm thật sự gọi.

### Skin: sprite đi qua một chỗ duy nhất

`UiBuilder` hiện giữ bảng màu. Mở rộng nó thành nơi giữ **cả sprite**, để không có view
nào tự `Resources.Load` — cùng lý do với `GopetCmd.cs` sinh tự động: một chỗ sai còn hơn
mười chỗ sai.

```
Assets/Scripts/Runtime/UI/JarSkin.cs        nạp sprite theo tên, cache
Assets/Scripts/Runtime/Audio/SoundBank.cs   nạp AudioClip theo tên
Assets/Scripts/Runtime/Audio/SoundManager.cs nhạc nền + hiệu ứng + nhớ trạng thái
```

### Bố cục: neo co giãn, KHÔNG toạ độ tuyệt đối

Đây là phần tốn công thật, không phải phần ảnh.

Bản jar vẽ thẳng lên canvas theo toạ độ tuyệt đối trong khung 320×240. uGUI dùng anchor
và co theo màn hình. Chép nguyên số sẽ ra bố cục đúng trên đúng một cỡ màn hình.

Cách làm: dựng một **khung tham chiếu 320×240** ở giữa màn hình, phóng theo bội số
nguyên lớn nhất vừa màn hình, rồi đặt widget theo toạ độ gốc bên trong khung đó. Giữ
được cả tính trung thành lẫn tính co giãn.

> `CanvasScaler` hiện đặt `referenceResolution = 720×1280`. Phải xem lại — hoặc đổi
> sang khung 320×240, hoặc đặt khung riêng bên trong. Quyết ở bước implementation.

## Related Code Files

**Create**

- `tools/unpack-jar-dat/index.js` — giải `.dat` → PNG, có `--check`
- `tools/unpack-jar-dat/dat-bank.js` — format `.dat`
- `tools/unpack-jar-dat/README.md`
- `tools/extract-jar-strings/index.js` — bóc bảng chuỗi VN+EN từ `a.java`, có `--check`
- `Assets/Scripts/Runtime/UI/JarStrings.cs` — nạp chuỗi theo chỉ số, cache
- `Assets/Scripts/Runtime/UI/JarSkin.cs` — nạp sprite theo tên
- `Assets/Scripts/Runtime/Audio/SoundBank.cs`
- `Assets/Scripts/Runtime/Audio/SoundManager.cs`
- `Assets/Scripts/Runtime/UI/PixelCanvas.cs` — khung 320×240 phóng bội số nguyên
- `Assets/Editor/JarAssetImportSettings.cs` — ép Point + không nén cho `Assets/Art/`
- `Assets/Resources/Jar/Art/…`, `Assets/Resources/Jar/Audio/…` — asset đã giải

**Modify**

- `Assets/Scripts/Runtime/UI/UiBuilder.cs` — trỏ sang `JarSkin`
- `Assets/Scripts/Runtime/UI/FormView.cs` — nền/nút lấy sprite thay vì màu phẳng
- `Assets/Scripts/Runtime/UI/LoginScreens.Account.cs` — thêm logo, nhãn theo bản jar
- `Assets/Scripts/Runtime/GopetBootstrap.cs` — dựng `SoundManager`, `PixelCanvas`
- `verify.ps1` — thêm bước kiểm asset đã giải còn khớp `.dat`
- `GopetUnityClient/README.md`

**Read for context**

- `client.jar_Decompiler.com/gu.java` — **format `.dat`, file quan trọng nhất phase này**
- `client.jar_Decompiler.com/fb.java` — màn đăng nhập, constructor cho biết bố cục
- `client.jar_Decompiler.com/ge.java` — widget ô nhập (mở `TextBox` của máy)
- `client.jar_Decompiler.com/gs.java` — hằng số bố cục: `m=20, o=8, p=3`
- `client.jar_Decompiler.com/a.java` — bảng chuỗi, hai ngôn ngữ
- `client.jar_Decompiler.com/fx.java` — màn splash: `meLogo.png` + `s_login`

## Implementation Steps

1. **Tool giải `.dat`.** Viết `tools/unpack-jar-dat/index.js` theo format ở trên. Xử lý
   đúng biên khối cuối cùng — đó là chỗ 5/5 file đều lệch lúc dò.
   > Kiểm: số PNG giải ra phải **đúng bằng** `count` khai trong header, và **mọi** khối
   > phải bắt đầu bằng chữ ký PNG. Còn một mục lệch nghĩa là còn sai.

2. **Chép asset vào `Assets/Resources/Jar/`.** 327 PNG + 54 ảnh giải ra →
   `Assets/Resources/Jar/Art/`; 10 WAV → `Assets/Resources/Jar/Audio/`.
   Bắt buộc dưới `Resources/` để `Resources.Load(tên)` đọc được trong build thật.

3. **Import settings.** `Assets/Editor/JarAssetImportSettings.cs` ép mọi ảnh dưới
   `Assets/Resources/Jar/Art/`: `FilterMode.Point`, `textureCompression = Uncompressed`,
   `spritePixelsPerUnit` thống nhất, tắt mipmap. Cùng file ép luôn WAV dưới
   `Assets/Resources/Jar/Audio/`: nhạc nền (`s_login`, `s_outMap_*`) nén Vorbis +
   streaming, hiệu ứng còn lại giải nén sẵn (ADPCM).
   > Bỏ bước này là ảnh mờ nhoè trên máy thật mà trong Editor vẫn trông ổn.

4. **`PixelCanvas`.** Khung tham chiếu 320×240, phóng theo bội số nguyên lớn nhất vừa
   màn hình, canh giữa. Mọi màn hình "giống bản jar" nằm trong khung này.

5. **`JarSkin` + `SoundBank`.** Nạp theo tên, cache. Tên sai phải ném hoặc log rõ, không
   trả `null` lặng lẽ — sprite `null` là ô trống suốt màn hình, không có lỗi nào báo.

6. **`SoundManager`.** Nhạc nền theo màn (`s_login` ở màn đăng nhập, `s_outMap_0/1` ngoài
   map), hiệu ứng nút (`s_button`, `s_button_ingame`). Nhớ trạng thái bật/tắt.

7. **Màn đăng nhập.** Đọc `fb.java` kỹ hơn constructor, dựng lại trong `PixelCanvas`:
   ảnh `lg.dat[0]` (173×92), ba hàng cao 20 cách 3, nhãn `Tên` (298) / `Mật khẩu` (375), dòng bản
   quyền. **Không** dựng softkey.

8. **Splash.** `meLogo.png` (260×72) + `s_login`, theo `fx.java`.

9. **Đối chiếu bằng ảnh chụp.** Chạy jar trên FreeJ2ME **ở 320×240** (đúng con số
   `ClientInfo` gửi cho server), chụp màn đăng nhập, đặt cạnh ảnh chụp Unity. Đây là
   **cách nghiệm thu chính** của phase này.
   > Cỡ màn khác là ảnh khác — client jar tự co theo `BaseCanvas.w/h`.

## Todo List

- [x] `tools/unpack-jar-dat/` giải đúng 54 ảnh từ 5 file `.dat`
- [x] `--check` bắt được khi asset trong `Assets/Resources/Jar` lệch so với `.dat` —
      kiểm bằng đột biến thật: sửa 1 byte, `--check` báo `LỆCH`, phục hồi thì `OK`
- [x] `tools/extract-jar-strings` bóc bảng chuỗi VN+EN (134=134), có `--check`
- [x] 327 PNG + 10 WAV + 54 ảnh `.dat` + 2 bảng chuỗi nằm trong
      `Assets/Resources/Jar/`, đúng import setting
- [x] `JarAssetImportSettings` ép Point + không nén cho ảnh, Vorbis/ADPCM cho âm
      thanh — kiểm bằng compile-only harness `tests/Gopet.Editor.UnityCompat/`
      (bắt được lỗi API `assetTarget` → `assetImporter` ngay khi viết, không cần mở
      Unity Editor)
- [x] Khoá hướng ngang **bằng code** (`GopetBootstrap.LockLandscape`), không dựa
      `ProjectSettings.asset` — file đó không commit, không đáng tin giữa các máy
- [x] `PixelCanvas` neo theo chiều cao, N nguyên — xunit kiểm cả 3 cỡ máy nêu trong
      plan (2400×1080, 1920×1080, iPad 2732×2048) + PlayMode kiểm việc áp vào
      RectTransform thật
- [x] `JarSkin` / `SoundBank` / `JarStrings` nạp theo tên, tên sai ném rõ ràng — có
      PlayMode test cho cả ba, chạy trên asset THẬT
- [x] `SoundManager` phát nhạc nền + hiệu ứng, nhớ trạng thái bật/tắt qua
      `PlayerPrefs` — 7 PlayMode test bao gồm ca "tắt rồi bật lại thì phát tiếp"
- [x] Màn splash: logo `meLogo` + nhạc `s_login`, tự đóng theo thời gian hoặc chạm
- [x] Màn đăng nhập dựng lại theo `fb.java`: banner `lg.dat[0]`, hai hàng Tên/Mật khẩu
      cao 20 cách 3, nút "Đăng nhập", ghi chú, dòng bản quyền — không softkey
- [ ] Ảnh chụp jar (FreeJ2ME **ở 320×240**) và ảnh chụp Unity đặt cạnh nhau, đã duyệt
      — **cần thao tác tay, chưa làm được**
- [x] `verify.ps1` xanh (10/10) — **PlayMode 107/107**, chạy thật trên máy này
      2026-09-06, không chỉ compile-check
- [x] README + phase doc cập nhật

## Success Criteria

- [x] Giải `.dat` **tái lập được**: chạy hai lần ra byte y hệt; `--check` đỏ khi sửa tay
      (kiểm bằng đột biến thật, không chỉ đọc code)
- [x] Mọi ảnh giải ra bắt đầu bằng chữ ký PNG, số lượng khớp header (`decodeBank` tự
      kiểm cả hai, ném lỗi nếu không)
- [x] Ảnh phóng **không mờ** — kiểm bằng import setting qua compile-only harness,
      không bằng mắt (đúng như plan yêu cầu)
- [ ] Màn đăng nhập đặt cạnh ảnh chụp từ FreeJ2ME — **cần thao tác tay, chưa làm được**
- [x] Nhạc nền phát ở màn đăng nhập, tắt/bật nhớ được qua lần mở app sau — PlayMode
      test `SoundManagerTests.BatLaiSauKhiTat_TuPhatTiepBaiDangCho` xác nhận
- [x] Bấm nút có tiếng — nút "Đăng nhập" của `JarLoginView` gọi
      `SoundManager.PlayEffect("s_button")`, xác nhận bằng PlayMode test đọc
      `AudioSource.isPlaying` thật, không chỉ tin lời gọi hàm
- [x] **Toàn bộ test P5 vẫn xanh** — `LoginScreensTests.cs`/`FormViewTests.cs` không
      sửa một dòng nào; đường FormView cũ vẫn là mặc định khi không có `PixelCanvas`
- [x] Không file `.cs` nào vượt 200 dòng (`verify.ps1` bước 10, quét cả `Assets/Editor`)

## Bằng chứng đã kiểm chứng (2026-09-06)

**Toàn bộ code viết trong phase này đã chạy PlayMode THẬT, không chỉ compile-check.**
`verify.ps1` 10/10; PlayMode ban đầu 107/108, sau khi sửa 108/108 — số liệu bên dưới.

### Một lỗi thật do chính phase này lộ ra, không liên quan gì tới jar/asset

`GopetClientReconnectTests.ServerDongThat_VanBaoMatKetNoi` (viết ở phiên trước, chưa
từng chạy thật lần nào vì Editor lúc đó đang mở) đỏ ngay lần chạy PlayMode đầu tiên
của phase này — 107/108. Đây không phải lỗi của `GopetSocket`/`GopetClient`
(production code), mà là **race condition trong chính test**:

```csharp
private async void Accept(TcpListener listener)
{
    try { _accepted.Add(await listener.AcceptTcpClientAsync()); }
    catch { }
}
```

`_client.IsConnected` chỉ cần bắt tay TCP xong ở tầng OS — nó KHÔNG chờ
`AcceptTcpClientAsync()` (chạy bất đồng bộ) thật sự chạy xong và điền vào
`_accepted`. Test đóng `_accepted` ngay sau khi thấy `IsConnected == true`; nếu
`_accepted` vẫn rỗng lúc đó thì **không có gì bị đóng cả**, và client đợi tới hết
`ConnectTimeout` (10 giây) không hiểu vì sao server "không đóng gì".

Sửa: thêm một `WaitUntil(() => _accepted.Count > 0, ...)` trước khi đóng. Sau khi
sửa: PlayMode 108/108, ổn định.

**Bài học:** `IsConnected` ở phía CLIENT chỉ nói về tầng OS, không nói gì về việc
mã ỨNG DỤNG phía kia (kể cả trong chính bộ test) đã chạy tới đâu. Giả định "client
thấy connected thì server cũng đã accept xong ở tầng người dùng" là giả định sai —
và nó chỉ lộ ra khi test THẬT SỰ CHẠY, không phải lúc compile-check.

### Số liệu

| | Trước sửa | Sau sửa |
|---|---|---|
| PlayMode | 107/108 | **108/108** |
| Unit test (xunit) | 359 | 359 |
| `verify.ps1` | 10/10 | 10/10 |

Test mới của phase này (đã chạy thật, không chỉ compile): `PixelCanvasTests`,
`JarSkinTests`, `SoundBankTests`, `SoundManagerTests`, `JarStringsTests`,
`JarLoginViewTests`, `JarSplashScreenTests`, `LoginScreensJarTests`,
`BootstrapLayeringTests` (từ phiên trước, cũng chạy thật lần đầu ở đây).

### Vòng review độc lập — 3 lỗi thật, cả ba nằm trên đường mọi người chơi đi qua

Sau khi 108/108 xanh, gửi toàn bộ code cho `code-reviewer` độc lập. Bắt được 2 lỗi
HIGH + 1 lỗi MEDIUM nghiêm trọng — cả ba đều có test XANH đè lên, đúng bài học
"test xanh không chứng minh hành vi đúng" mà plan này đã nhắc từ đầu.

**H1 — Splash không phát nhạc, tự đóng sau đúng 1 frame bất kể tham số truyền vào.**

```csharp
var splash = go.AddComponent<JarSplashScreen>();   // Unity chạy Awake+OnEnable NGAY ĐÂY
splash._sound = sound;                              // quá muộn
splash._minimumSeconds = minimumSeconds;            // quá muộn
```

`AddComponent` trên GameObject đang active chạy `OnEnable` **ngay lập tức**, trước
khi hai dòng gán phía dưới kịp chạy. Lúc đó `_sound == null` (nhạc không phát) và
`_minimumSeconds == 0` (`WaitForSeconds(0)` đóng màn ngay frame sau). Hai tiêu chí
"nhạc nền phát ở splash" và thời gian tối thiểu — cả hai đều KHÔNG đúng như tưởng.

Test tự tố cáo chính nó: bản đầu có dòng
`splash.gameObject.SetActive(false); SetActive(true); // ép chạy lại OnEnable trong test`
— phải ép chạy lại OnEnable nghĩa là lần chạy tự nhiên đã không làm đúng việc.

Sửa: `SetActive(false)` trước `AddComponent`, gán field, rồi `SetActive(true)` sau
cùng. Bỏ đoạn ép chạy lại trong test; thêm một test mới bắt được đúng lỗi này —
kiểm màn CHƯA đóng ở 0,2s (< minimumSeconds = 1s) trước khi kiểm nó đóng ở 1,2s.

**H2 — Nhãn ô nhập: sai chỉ số chuỗi, VÀ chuỗi thật chỉ dùng để đặt tên GameObject.**

`MakeField(Font, placeholder, label, top)` — bản đầu gọi
`MakeField(font, "T.Khoản", JarStrings.Vi(5), usernameTop)`: chuỗi hiển thị là
**literal gõ cứng** `"T.Khoản"`, còn `JarStrings.Vi(5)` chỉ rơi vào tên GameObject
(`$"Field_{label}"`) — không bao giờ lên màn hình. `JarStrings` bị bỏ qua đúng chỗ
cần nó nhất.

Và chỉ số `5` bản thân nó cũng sai — **lần sửa thứ hai** cho cùng một nhãn. Bản plan
đầu ghi nhầm `203`; lần sửa một ra `5` = "T.Khoản" (chuỗi đúng, nhưng không dòng nào
trong `fb.java` gọi `a.a(5)` cả — tìm bằng cách grep nội dung chuỗi trên toàn bảng,
không lần theo đúng lời gọi trong hàm dùng nó). Chỉ số đúng, đối chiếu trực tiếp
nhánh `BaseCanvas.w >= 240` của `fb.java` (khớp bề rộng 320 ta bám theo):
`298` = "Tên", `375` = "Mật khẩu". `348`/`482` là bản rút gọn cho `w < 240`, không
áp dụng.

**Bài học kép:** (1) tìm một chuỗi bằng cách grep nội dung không chứng minh được
INDEX đúng — phải lần theo đúng lời gọi `a.a(N)` trong chính hàm dùng nó. (2) Không
test nào kiểm nội dung placeholder thật — mọi test trước đó chỉ đếm số ô
(`Fields.Count == 2`). Thêm `ONhap_PlaceholderDungChuoiTuFbJavaNhanhW240` đọc thẳng
`placeholder.GetComponent<Text>().text`, bắt được cả hai lỗi cùng lúc.

**M1 — `Dismiss()` báo `Finished` trước khi tự huỷ.** Nếu người nghe (`_flow.Start`)
ném — mạng hỏng, DNS sai — panel splash (phủ kín màn hình, chắn raycast) đã set
`_dismissed = true` nên chạm vào cũng vô ích, kẹt lại vĩnh viễn che hết mọi thứ phía
sau. Đảo thứ tự: tự huỷ trước, báo sự kiện sau.

**M2/M3 — Hai canvas cùng sortingOrder mặc định (đều 0); và guard kích thước 0 bị
"force" bỏ qua.**

`PixelCanvas` dựng Canvas RIÊNG với `GopetBootstrap`'s canvas chung — cả hai
`ScreenSpaceOverlay`, cùng `sortingOrder = 0` mặc định thì thứ tự vẽ **không được
đảm bảo**. Đây là đúng loại lỗi layering đã dính ở P5 giữa `UiRoot`/`LoginScreens`
(H1 phiên trước), chỉ chuyển từ ranh giới sibling sang ranh giới Canvas: server có
thể gửi OTP qua `UiRoot` (canvas chung) đúng lúc `JarLoginView` đang hiện trên
`PixelCanvas` — nếu `PixelCanvas` thắng, OTP bị che lại, tái diễn nguyên bài học cũ
dưới lớp áo mới. Sửa: `PixelCanvas.SortingOrder = 0` tường minh,
canvas chung = `PixelCanvas.SortingOrder + 1`, và thêm test kiểm cả giá trị lẫn quan
hệ lớn-hơn.

Cùng lúc, `Recompute(force: true)` (gọi từ `Create()`) bỏ qua luôn guard
"`Screen.width <= 0`", nên một môi trường báo kích thước 0 ở đúng frame khởi tạo
(trình chạy test `-nographics`, frame đầu batchmode) làm `PixelCanvasLayout.Compute`
ném thẳng ra khỏi `GopetBootstrap.Start()` — cả app không mở được. Sửa: guard áp
dụng cho MỌI đường vào, không phân biệt `force`; nhân tiện phát hiện tham số `force`
sau đó không còn được đọc ở đâu cả — xoá luôn.

**Trạng thái sau vòng review này:** đã sửa cả 5, compile-check (Runtime/PlayMode
compile-only) xanh, `verify.ps1` 10/10, 359 unit test xanh. **PlayMode chưa chạy lại
được — Unity Editor đang mở lúc viết dòng này**, cần đóng Editor rồi chạy
`run-playmode-tests.ps1` để xác nhận cuối cùng.

## Bổ sung theo yêu cầu (2026-09-06): bỏ màn chọn máy chủ khi chỉ có một

Server hiện tại chỉ có đúng một máy chủ. Người dùng yêu cầu bỏ màn chọn máy chủ
trong trường hợp đó, giữ lại cho sau này khi thêm nhiều máy chủ.

Sửa tại nguồn — `LoginFlow.OnServerList()` (`Assets/Scripts/UiLogic/LoginFlow.ServerList.cs`):
đúng một máy chủ thì tự gọi `ChooseServer(0)` luôn, không vào `LoginStage.ChoosingServer`.
Nhiều hơn một thì vẫn đi qua màn chọn như cũ — không xoá cơ chế, chỉ bỏ qua khi
không cần dùng tới. Cả `FormView` lẫn `JarLoginView` tự động ăn theo vì cả hai chỉ
phản ứng với `LoginStage`, không tự quyết định có hiện màn chọn hay không.

**Việc phải sửa theo, vì test/helper cũ giả định luôn dừng ở `ChoosingServer`:**
- `tests/Gopet.Net.Tests/LoginFlowHarness.cs` — `ReachCredentials()` chỉ gọi
  `ChooseServer(0)` khi `Stage == ChoosingServer` (trước đó gọi vô điều kiện; gọi
  trùng lần hai không sai về mặt trạng thái nhưng không còn ý nghĩa gì).
- `LoginFlowTests.cs` — `DuongThuan_TuNoiToiVaoGame` bỏ bước qua màn chọn (đi thẳng
  `EnteringCredentials`); `ChonMayChuKhac_NoiLaiToiDiaChiDo` bỏ lời gọi
  `ChooseServer` thừa (gọi lại sau khi đã tự động chọn sẽ đọc nhầm `_host`/`_port`
  vừa được cập nhật, nhảy thẳng `EnteringCredentials` dù kết nối mới chưa xong —
  bug thật nếu không sửa, không chỉ là test lỗi thời).
- Thêm `NhieuMayChu_VanQuaManChon` — đường nhiều máy chủ (chưa dùng, để sẵn cho sau)
  vẫn phải chạy đúng, không bị auto-skip vô tình nuốt mất.
- PlayMode: `LoginScreensTests.cs`'s `ReachCredentials()` sửa tương tự;
  `LoginScreensJarTests.cs` cũng vậy. Tách 3 test liên quan tới danh sách máy chủ
  (2 cũ + 1 mới: `DungMotMayChu_KhongHienManChon_VaoThangDangNhap`) ra
  `LoginScreensServerListTests.cs` — giữ `LoginScreensTests.cs` dưới 200 dòng.

Kiểm bằng đột biến: bỏ nhánh auto-skip → 2 test đỏ; auto-skip nhầm chỉ số 1 thay vì
0 → 30 test đỏ (mọi test đi qua `ReachCredentials`/tương đương). `verify.ps1` 10/10,
360 unit test. **PlayMode chưa chạy lại — vẫn đang chờ Unity Editor đóng** (xem mục
review ở trên, cùng lý do).

## Bổ sung theo yêu cầu (2026-09-06): nền splash phủ full màn hình + xác minh "icon loa"

Người dùng gửi ảnh chụp màn splash trong Unity Editor, yêu cầu: (1) ảnh nền phủ
full màn hình thật (không để lộ sọc letterbox), (2) di chuyển một "icon loa" thấy
đè lên logo lên góc trên-phải, chạm vào để tắt/bật âm thanh.

**Nguyên nhân sọc letterbox:** `JarSplashScreen.Create()` (bản cũ) đặt CẢ nền lẫn
logo làm con của `canvas.Content` — khung đã bị co theo bội số nguyên để giữ tỉ lệ
pixel-art (xem `PixelCanvasLayout`), nên hẹp hơn màn hình thật và để lộ viền màu ở
mép trên/dưới. Sửa: tách nền ra khỏi `canvas.Content`, đặt thẳng lên `canvas.transform`
(gốc Canvas, luôn đúng bằng kích thước màn hình thật) với anchor full-stretch (0,0)–(1,1);
logo vẫn giữ trong `canvas.Content` để không mất độ nét. Vì component `JarSplashScreen`
cần là mục tiêu raycast cho "chạm để bỏ qua", nó chuyển sang gắn lên chính GameObject
nền; logo (nay ở nhánh cây khác) theo dõi riêng qua field `_logoLayer` để `Dismiss()`
dọn dẹp đúng cả hai nhánh — thiếu bước này sẽ để rác logo kẹt lại vĩnh viễn trong
`canvas.Content` sau khi nền đã bị `Destroy()`.

**Xác minh "icon loa":** soi trực tiếp pixel `meLogo.png` (260×72) bằng PIL, phóng
6x và 8x quanh vùng chữ "TAE" nơi ảnh chụp cho thấy có hình tròn — không có icon
loa/tròn nào trong dữ liệu ảnh gốc. Cũng không tìm thấy asset icon loa/mute rời nào
trong toàn bộ tài nguyên đã trích từ jar (`icon.png`, `buttonicon/0-2.png` đều không
khớp). Ảnh chụp có một dấu cộng (crosshair) kéo dài hết chiều ngang/dọc, tâm đúng
ngay vị trí "hình tròn" — đặc điểm của gizmo Scene View (tâm xoay + tay cầm công cụ
Move/Rect) của Unity Editor, không phải phần tử thật trong game. Kết luận: hiện
KHÔNG có nút loa nào tồn tại trong game để "di chuyển" — nếu người dùng vẫn muốn một
nút tắt/bật âm thanh ở góc trên-phải, đây là một tính năng MỚI cần làm từ đầu (icon
cần chọn/tạo riêng, vì jar gốc không có sẵn icon loa rời).

**Test cập nhật** (`Assets/Tests/PlayMode/JarSplashScreenTests.cs`):
- `Create_NapDungLogoTrongContent` thay `Create_HienDungLogo` cũ — tìm logo đúng
  trong `canvas.Content` (cũ tìm bằng `splash.GetComponentInChildren` sẽ sai vì giờ
  logo ở nhánh cây khác với `splash`).
- `Nen_PhuDungManHinhThat_KhongBiLetterbox` — test hồi quy trực tiếp cho yêu cầu
  "ảnh full màn hình": khẳng định nền là con trực tiếp của `canvas.transform` với
  anchor (0,0)-(1,1).
- `Dismiss_DonCaHaiNhanh_KhongDeLaiRacTrongContent` (`[UnityTest]`, không phải
  `[Test]`, vì `Destroy()` hoãn tới cuối frame) — khẳng định dọn cả hai nhánh cây
  sau khi đóng màn.

`verify.ps1` 10/10 (360 unit test, build sạch tất cả các tầng compile). **PlayMode
chưa chạy lại thật** — Unity Editor đang mở lúc sửa (không giữ được project lock
cho batchmode); vì Editor đang mở sẵn, có thể bấm Play trực tiếp để kiểm bằng mắt
thay vì đóng Editor chạy batchmode.

## Bổ sung theo yêu cầu (2026-09-06): nút tắt/bật âm thanh + đăng ký tài khoản + ghi nhớ đăng nhập

Ba tính năng MỚI, không có trong jar gốc:

1. **Nút tắt/bật âm thanh, góc trên-phải màn hình thật** — thay cho "icon loa" mà
   người dùng thấy trong ảnh chụp (đã xác minh ở mục trên: đó là gizmo Scene View
   của Unity Editor, không phải phần tử thật trong game).
2. **Nút "Tạo tài khoản"** cạnh nút "Đăng nhập" trên `JarLoginView`.
3. **Ô "Ghi nhớ tài khoản đăng nhập"** phía trên hai nút đó.

### Phát hiện chặn trước khi làm: server khoá cứng đăng ký

`GServer/Server/Player.cs`'s `doRegister()` có khối `if (true) { redDialog("Chức
năng này bị khóa..."); return; }` — chặn ĐĂNG KÝ VÔ ĐIỀU KIỆN trước khi chạy tới
đoạn kiểm username/password + `INSERT` vào bảng `user` (đoạn đó vẫn còn nguyên,
chỉ là chết — không bao giờ chạy tới). Đây là khoá ở SERVER, không phải hạn chế
của client jar cũ như dòng comment trong `JarLoginView.cs` (cũ) từng diễn giải sai.
Hỏi người dùng và được xác nhận: **mở khoá thật**, không chỉ thêm nút cho có.

Sửa: xoá khối `if (true) {...}` trong `Player.cs:191-195` (repo `SRCGOPETGOC/GServer`,
KHÔNG phải `GopetUnityClient`). Build lại `Gopet.csproj` xác nhận 0 lỗi C# — bước
copy `.exe` báo lỗi vì server ĐANG CHẠY (PID khoá file), không phải lỗi biên dịch.

**Đã build lại và khởi động lại server (2026-09-06).** Lần đầu khởi động lại thất
bại: `Access denied for user 'root'@'172.20.0.1' (using password: NO)` —
`MYSQLManager.VerifyConnections()` không kết nối được MariaDB. Không phải lỗi từ
đăng ký hay từ code sửa ở trên — DB thật ra là container Docker `gopet-mariadb`
(MariaDB 10.4, cổng `127.0.0.1:3306`), CÓ đặt mật khẩu root
(`MYSQL_ROOT_PASSWORD` trong container, xác nhận bằng `docker inspect`); tiến
trình cũ chạy được vì phiên trước đó đã có sẵn biến môi trường `GOPET_DB_PASSWORD`
đúng, còn phiên khởi động lại của tôi thì không, nên gửi mật khẩu rỗng và bị từ
chối — không liên quan gì tới NAT/gateway IP như nghi ngờ ban đầu (đã xác minh
bằng kết nối `pymysql` độc lập, không qua `Gopet.exe`, để loại trừ nguyên nhân do
code). Set đúng `GOPET_DB_PASSWORD` rồi khởi động lại — chạy ổn, cổng game 19180
đã lắng nghe, log sạch không lỗi.

### Đường đi gói REGISTER (35), khác LOGIN ở một điểm quan trọng

Server KHÔNG đóng kết nối sau khi trả lời `REGISTER` (khác `LOGIN` sai mật khẩu,
đóng ngay) — nghĩa là gửi được ngay tại `LoginStage.EnteringCredentials`, không cần
đổi máy trạng thái. Nhưng hồi âm (`OkDialog`/`RedDialog`) dùng CHUNG hai opcode với
đường từ chối đăng nhập, mà `auth.DialogShown` trước đây nối thẳng vào
`LoginFlow.OnLoginRejected` — hàm này tự bỏ qua khi không ở `LoggingIn`/
`CreatingCharacter` (đúng ý đồ: dialog giữa game không bị nuốt), nên hồi âm
REGISTER gửi lúc đang đứng ở `EnteringCredentials` sẽ **rơi vào khoảng trống, không
tới UI được** nếu tái dùng nguyên xi đường cũ.

Sửa bằng một tầng điều phối mới, `LoginFlow.OnDialog(string)`:
- Có `REGISTER` đang chờ hồi âm (`_awaitingRegisterReply`) → bắn
  `RegisterReplyReceived`, KHÔNG gọi `Reconnect()` (khác `OnLoginRejected`, vốn nối
  lại ngầm vì server đã "dùng" kết nối cho phiên đăng nhập cũ — REGISTER không làm
  vậy, nối lại ở đây là thừa và có thể đá văng một lần đăng nhập khác đang chạy
  song song).
- Không có gì đang chờ → gọi `OnLoginRejected(text)` y như cũ — hành vi cũ giữ
  nguyên 100%, xác nhận bằng test `KhongCoRegisterDangCho_OnDialogVanXuLyNhuLoiTuChoiDangNhap`.

`GopetBootstrap.WireFlow`: đổi `auth.DialogShown += _flow.OnLoginRejected` thành
`auth.DialogShown += _flow.OnDialog`.

**Bẫy tự bắt trước khi lộ ra:** rớt mạng đúng lúc đang chờ hồi âm REGISTER thì hồi
âm đó không bao giờ tới — nếu không reset `_awaitingRegisterReply` trong
`OnDisconnected()`, cờ treo mãi mãi chặn hết mọi lần thử đăng ký sau này của phiên
đó. Test `MatKetNoiDangChoHoiAm_KhongKetTreoVinhVien` bắt lỗi này bằng đột biến
(xoá dòng reset → đỏ ngay).

### Ghi nhớ tài khoản: đổi từ "luôn nhớ" sang "tự chọn"

Trước đây `LoginScreens.Remember()` LUÔN lưu sau khi đăng nhập thành công, không có
cách tắt. Thêm ô chọn thay đổi ngữ nghĩa: BẬT (mặc định, khớp hành vi cũ) → lưu như
trước; TẮT → **không lưu VÀ xoá luôn phần đã lưu trước đó** (không để nó âm thầm
sống sót — nếu chỉ ngừng lưu mà không xoá, tài khoản cũ vẫn tự điền lần sau dù đã
tắt "ghi nhớ", sai với kỳ vọng của người dùng về ý nghĩa cái nút).

Trạng thái ô chọn phải đọc RA TRƯỚC khi `JarLoginView` bị `ClearViews()` huỷ (xảy ra
ngay khi `SubmitCredentials` chuyển stage sang `LoggingIn`) — chép vào field
`LoginScreens._remember` ngay trong handler `SubmitRequested`, cùng lúc với
`_sentUsername`/`_sentPassword` (mẫu đã có sẵn trong file, chỉ làm theo).

Không lưu trạng thái ô chọn giữa hai lần mở app (mặc định luôn về BẬT) — đơn giản
hơn nhiều so với thêm một khoá `PlayerPrefs` riêng chỉ cho một checkbox, đánh đổi
chấp nhận được.

### Vì sao không có icon loa thật

Không có icon loa/mute nào trong toàn bộ tài nguyên trích từ jar (đã kiểm
`icon.png`, `buttonicon/0-2.png` ở mục xác minh phía trên), và không có công cụ vẽ/
tạo ảnh nào khả dụng trong môi trường này (`ai-multimodal` báo thiếu mọi API key).
Dùng nhãn chữ "BẬT"/"TẮT" bằng đúng font đang hiển thị mọi chữ Việt khác trong game
— chắc chắn hiện đúng, không phụ thuộc một glyph biểu tượng (emoji loa) có thể
thiếu trên máy người chơi. Có thể đổi sang icon thật sau này nếu người dùng cung
cấp asset hoặc tự vẽ.

### File tách theo quy tắc 200 dòng

`JarLoginView.cs` đã đứng đúng 200 dòng trước khi sửa — tách phần tài khoản MỚI
(nút đăng ký, ô ghi nhớ, `PlaceSideBySide`) ra `JarLoginView.Account.cs`, đúng mẫu
`LoginFlow.Account.cs`/`LoginScreens.Account.cs` đã có sẵn trong dự án.

### File đụng tới

**GServer** (`SRCGOPETGOC/GServer`, KHÁC repo với `GopetUnityClient`):
- `Server/Player.cs` — xoá khối khoá cứng `doRegister()`.

**GopetUnityClient:**
- `Assets/Scripts/Runtime/UI/JarLoginView.cs` — tách bớt, thêm `BuildAccountControls` hook.
- `Assets/Scripts/Runtime/UI/JarLoginView.Account.cs` (MỚI) — nút đăng ký, ô ghi nhớ.
- `Assets/Scripts/Runtime/UI/SoundToggleButton.cs` (MỚI) — nút tắt/bật âm thanh.
- `Assets/Scripts/UiLogic/LoginFlow.Account.cs` — `SubmitRegistration`, `OnDialog`.
- `Assets/Scripts/UiLogic/LoginFlow.cs` — reset `_awaitingRegisterReply` khi mất kết nối.
- `Assets/Scripts/Runtime/UI/LoginScreens.cs` — field `_remember`, nối `RegisterReplyReceived`.
- `Assets/Scripts/Runtime/UI/LoginScreens.Account.cs` — nối nút đăng ký, gate `Remember()`.
- `Assets/Scripts/Runtime/GopetBootstrap.cs` — `auth.DialogShown += _flow.OnDialog`, dựng `SoundToggleButton`.
- Test mới: `LoginFlowRegisterTests.cs` (Gopet.Net.Tests, 7 test), `JarLoginViewAccountTests.cs`,
  `LoginScreensRegisterTests.cs`, `SoundToggleButtonTests.cs` (PlayMode).

Kiểm bằng đột biến (LoginFlow, tầng thuần C#): bỏ reset `_awaitingRegisterReply`
khi mất kết nối → đỏ; bỏ guard "một REGISTER một lúc" → đỏ. `verify.ps1` 10/10, 367
unit test (360 cũ + 7 REGISTER). **PlayMode (JarLoginView/LoginScreens/SoundToggleButton)
chưa chạy thật** — cùng lý do Unity Editor đang mở; chỉ xác nhận qua tầng compile-only.

**Việc còn lại (thủ công, ngoài phạm vi sửa code):**
- ~~Build lại và khởi động lại GServer để áp dụng mở khoá đăng ký.~~ **Xong (2026-09-06)** — xem chi tiết ở mục trên.
- Đóng Unity Editor (hoặc tự bấm Play kiểm) để chạy PlayMode thật một lần trước khi coi là xong hẳn.

## Bổ sung theo yêu cầu (2026-09-06): bỏ dòng gợi ý đăng ký web + thêm nền xanh navy cho màn đăng nhập

Hai sửa nhỏ, cùng một lượt:

1. **Xoá dòng "Nếu chưa có tên, xin đăng ký"** (`JarStrings.Vi(331)`, đúng chuỗi
   `a.a(331)` trong `fb.java`) khỏi `JarLoginView.Build()`. Chuỗi này vốn trỏ người
   chơi đi trang web đăng ký (thời jar còn khoá đăng ký) — nay đã có nút "Tạo tài
   khoản" thật ngay trên màn hình, dòng gợi ý không còn cần nữa. Test mới
   `Create_KhongConDongGoiYDangKyWeb` xác nhận không còn `Text` nào mang đúng chuỗi
   này trong `JarLoginView`.

2. **Nền xanh navy cho `JarLoginView`** — phát hiện qua truy `fw.java` (lớp cha của
   `fb.java`): `public void b() { BaseCanvas.g.setColor(gs.a); BaseCanvas.g.fillRect(0,
   0, BaseCanvas.w, BaseCanvas.h); }`, với `gs.a = 345451` (`0x05456B`, RGB
   5/69/107) — tô ĐẦY màn hình trước khi vẽ bất kỳ nội dung nào. `JarLoginView` bản
   port trước đây KHÔNG có bước này: view chỉ có banner + ô nhập trên nền TRONG
   SUỐT, để lộ màu clear-color của camera phía sau (đen) thay vì xanh navy như jar
   — đây chính là thứ người dùng thấy thiếu ("chưa có hình nền giống bên jar").
   Không phải một ẢNH nền — jar tô màu phẳng, không dùng sprite nào cho việc này.

   Thêm hằng màu `UiBuilder.JarBackground` (5/255, 69/255, 107/255) và một
   GameObject nền phủ TOÀN MÀN HÌNH THẬT ở `canvas.transform` (không phải
   `canvas.Content` đã bị letterbox) — cùng kỹ thuật hai-nhánh-cây và cùng bẫy dọn
   dẹp như `JarSplashScreen` (xem mục "nền splash phủ full màn hình" phía trên):
   `JarLoginView` (chính view, ở `canvas.Content`) và nền (ở `canvas.transform`) là
   hai nhánh khác nhau, nên `Destroy(gameObject)` của view không tự kéo theo nền —
   thêm `OnDestroy()` tự dọn, tách riêng vào `JarLoginView.Background.cs` (file thứ
   ba, giữ `JarLoginView.cs` dưới 200 dòng — đã đụng trần lần nữa sau khi thêm nền).
   Test mới `Create_CoNenXanhNavy_PhuDungManHinhThat` xác nhận nền đúng màu, đúng
   anchor full-stretch, đúng cha là `canvas.transform`.

File đụng: `JarLoginView.cs`, `JarLoginView.Account.cs` (chuyển `MakeButton` sang
đây để nhường chỗ), `JarLoginView.Background.cs` (MỚI), `UiBuilder.cs` (thêm màu),
`JarLoginViewTests.cs` (2 test mới). `verify.ps1` 10/10, 367 unit test không đổi
(sửa này thuần Runtime/UI, không đụng logic thuần C#). **PlayMode chưa chạy thật**
— cùng lý do Unity Editor đang mở suốt phiên làm việc này.

## Bổ sung theo yêu cầu (2026-09-06): nền THẬT của màn đăng nhập là MAP 11, không phải màu phẳng

Người dùng đối chiếu ảnh chụp FreeJ2ME với Unity và chỉ ra bên jar có cả một khung
cảnh (cây thông tuyết, nhà, máy "TAE", biển) mà bản port không có.

### Hai kết luận SAI trước đó, và vì sao sai

1. **"Nền là màu phẳng xanh navy"** — đọc `fw.b()` (lớp cha) thấy `fillRect(gs.a)`
   rồi dừng lại ở đó. Nhưng `fb.java` **GHI ĐÈ** `b()`:
   ```java
   public final void b() { this.a.a(this.f, 0, true); }   // vẽ map, không tô màu
   ```
   Bài học: với cây kế thừa, đọc lớp cha KHÔNG đủ — phải kiểm lớp con có override
   hay không. Màu `gs.a` vẫn có thật, nhưng dùng cho các màn khác.

2. **"Cảnh được vẽ bằng lệnh hình học"** — suy đoán sau khi tìm không thấy file ảnh
   nền nào. Sai hoàn toàn: cảnh ghép từ **tile 24×24** trong `newMapData/` (256 ảnh
   ĐÃ trích xuất từ P5.1, nằm sẵn trong repo mà không nhận ra) theo bố cục trong
   `maps/11.dat`.

Kiểm chứng dứt điểm TRƯỚC khi viết code: parse `maps/11.dat` bằng Python rồi render
ra PNG — cho ra đúng khung cảnh trong ảnh chụp (cây thông, nhà GYM/MAGIC, máy TAE).

### Đường đi của nền map

- `fb.java` dựng `new ef(11, new dv())` → nạp `/maps/11.dat`.
- `ef.java:104-190` giải: danh sách tài nguyên → kích thước (24×20 ô = 576×480px) →
  các lớp ô nền → lớp va chạm → 84 vật thể.
- **Ô nền nén trong MỘT byte:** 4 bit cao = chỉ số ảnh dải, 4 bit thấp = ô thứ mấy
  trong dải CỘNG MỘT (0 = ô trống). `drawRegion(img, (cell-1)*24, 0, 24, 24, …)`.
- Vật thể neo GIỮA-DƯỚI, vẽ tại `y - yOffset`.
- `fb.c_()` cộng `this.h = 2` pixel mỗi nhịp và đảo dấu khi chạm mép → **cảnh trôi
  ngang qua lại**, quãng cuộn = 576 − 320 = 256.

### Cài đặt

- `tools/unpack-jar-dat` copy thêm 24 `maps/*.dat` → `Resources/Jar/Maps/*.bytes`.
  **Phải đổi đuôi**: Unity không sinh `TextAsset` cho `.dat`, `Resources.Load` trả
  `null` dù file nằm đúng chỗ.
- `JarMapLayout` (UiLogic, thuần C#) — giải bố cục. `yOffset` đọc **có dấu**: map 11
  có vật thể `-14`, đọc không dấu thì nó tụt 270px ra ngoài map.
- `JarMapScroll` (UiLogic, thuần C#) — camera trôi/đảo chiều. Tách riêng để test được
  cả hai lần đảo chiều trong mili-giây thay vì chờ ~9 giây thật trong PlayMode. Cuộn
  tính theo GIÂY (30 px/s = 2px × ~15 nhịp/s của jar), không theo frame — cộng theo
  frame ở 60 fps thì cảnh chạy nhanh gấp bốn và đổi tốc độ theo máy khoẻ/yếu.
- `JarMapBackground` (Runtime/UI) — dựng ô nền + vật thể, cắt sprite 24×24 từ ảnh dải,
  cache **tĩnh** (`Sprite.Create` sinh đối tượng mới mỗi lần gọi; cache cục bộ nghĩa là
  mỗi lần đăng nhập sai lại bỏ lại vài chục sprite không ai thu hồi). Là con ĐẦU TIÊN
  của `JarLoginView` nên nằm dưới biểu mẫu và tự biến mất khi màn bị huỷ.
- Nền màu phẳng GIỮ NGUYÊN: map chỉ cao 240px jar nên hai vệt letterbox trên/dưới
  vẫn cần một lớp màu che.

Test: 12 test mới ở tầng thuần C# (`JarMapLayoutTests` chạy trên `maps/11.bytes`
THẬT, `JarMapScrollTests`), `JarMapBackgroundTests` + `JarLoginViewBackgroundTests`
bên PlayMode. Kiểm bằng đột biến: bỏ `SignedByte` → đỏ; bỏ kẹp mép khi đảo chiều →
3 test đỏ. `verify.ps1` 10/10, 379 unit test.

**Bẫy công cụ đã trả giá:** khôi phục file sau đột biến bằng `mv` từ bản `.bak` giữ
timestamp CŨ hơn DLL, nên MSBuild coi là đã build rồi và `dotnet test` chạy lại đúng
bản đột biến — verify đỏ dù mã nguồn đã đúng. Phải xoá `bin/obj` (hoặc `touch`) sau
khi khôi phục.

**PlayMode chưa chạy thật** — Unity Editor vẫn mở suốt phiên; mới xác nhận qua tầng
compile-only và qua bản render Python đối chiếu ảnh chụp.

## Bổ sung theo yêu cầu (2026-09-06): màn đăng nhập dùng TRANH RIÊNG, không dùng map nữa

Ngay sau khi nền map chạy được, người dùng đưa một tấm tranh riêng
(`Downloads/OpenAI Playground …png`, 1536×1024, cảnh làng pet) và yêu cầu dùng nó
làm nền màn đăng nhập. Đây là lựa chọn THẨM MỸ của dự án, cố ý khác bản jar.

- Ảnh vào `Assets/Resources/Ui/login-background.png` — **ngoài** `Resources/Jar/`:
  thư mục đó dành riêng cho thứ giải ra từ jar và có `--check` đối chiếu với nguồn;
  bỏ asset lạ vào đó là làm hỏng ý nghĩa của phép kiểm ấy.
- **Không đặt trong `PixelCanvas.Content`.** Khung đó ép ảnh về hệ 320×240 rồi phóng
  lại theo bội số nguyên bằng lọc Point — đúng cho pixel art của jar, nhưng tranh vẽ
  độ phân giải cao qua đó vừa mất chi tiết vừa răng cưa. Nền nằm thẳng trên gốc
  canvas, phủ màn hình THẬT, và cũng nhờ vậy không còn vệt letterbox nào.
- `AspectRatioFitter` chế độ `EnvelopeParent` + `RectMask2D`: phủ kín theo đúng tỉ lệ
  rồi cắt phần thừa. Kéo giãn thì méo, `FitInParent` thì hở viền trên màn 16:9.
- Thiếu file ảnh thì **cảnh báo rồi lùi về nền màu**, không ném: một màn đăng nhập
  xấu vẫn đăng nhập được, còn ném là chặn hẳn đường vào game.

**`JarMapBackground` giữ nguyên, chỉ không dùng ở màn đăng nhập nữa.** Nó là bản port
đúng của cơ chế vẽ map trong jar và chính là thứ P6 cần; test của nó vẫn chạy.

`verify.ps1` 10/10, 379 unit test.

## Bổ sung theo yêu cầu (2026-09-06): thay hẳn form đăng nhập bằng bộ art riêng

Người dùng đưa mockup + một sprite sheet (logo, panel, 4 icon, 2 nút, vật trang trí)
và chốt hai điều: **form ra khỏi khung pixel**, và **tự cắt sprite từ sheet**.

### Vì sao phải rời `PixelCanvas`

Khung đó là hệ 320×240 của jar. Quy panel trong mockup về hệ ấy thì nó rộng ~98px,
trong khi câu "Ghi nhớ tài khoản đăng nhập" cần ~130px ở cỡ chữ 11 — không có cách
nào nhét vừa. Form mới dựng trên canvas thường, **mọi kích thước theo TỈ LỆ** panel
chứ không theo pixel tuyệt đối (canvas dùng chung có `referenceResolution` riêng, đặt
cứng pixel là hỏng ngay khi ai đó chỉnh con số đó), cỡ chữ dùng `resizeTextForBestFit`.

Đánh đổi đã biết: màn đăng nhập nay khác phong cách với 162 màn menu của server
(vẫn theo pixel-art jar). Người dùng đã xác nhận chấp nhận.

### Cắt sprite từ sheet

Sheet là **RGB, KHÔNG có alpha** — nền ca-rô là pixel thật. Không lọc theo màu (sẽ ăn
mất phần kem/trắng của chính sprite: panel, lòng trắng con mắt) mà **flood fill từ 4
mép**: chỉ vùng nền liên thông với biên mới thành trong suốt. Sau đó tách cụm bằng
connected-component → 19 cụm, xuất 14 mảnh vào `Resources/Ui/Login/`.

Riêng **ô nhập không cắt nguyên được** vì trong sheet nó đã có sẵn chữ mẫu
("gopettest"). Lấy mép phải sạch rồi lật ngang để có mép trái → ra một khung rỗng
đối xứng, dùng 9-slice kéo giãn phần giữa. Viền 9-slice cắt **trong code**
(`Sprite.Create` có overload nhận border) chứ không đặt trong Sprite Editor: cái đó
nằm trong file `.meta` do Unity sinh, dễ mất khi ai đó re-import.

Hai nút dùng **nguyên sprite đã có chữ**, không vẽ Text đè lên — hai lớp chữ chồng
nhau chỉ tạo ra vệt lệch.

### Cấu trúc mới

- `LoginSkin` — nạp sprite `Ui/Login/*`, thiếu file thì trả `null` (ngược với
  `JarSkin` vốn ném): mỗi mảnh đều có đường lùi bằng màu phẳng, và ném ở đây là chặn
  hẳn đường vào game vì một chuyện thuần trang trí.
- `LoginBackground` — tranh nền hai lớp, tách khỏi form nên dùng lại được.
- `LoginFormView` (+ `.Fields.cs`, `.Actions.cs`) — panel, logo, hai ô nhập, ô ghi
  nhớ, hai nút. Có thêm **nút con mắt** hiện/che mật khẩu: hữu ích thật vì bộ gõ
  Telex nuốt phím trong ô nhập (đã trả giá ở P1), mà ô bị che thì không nhìn ra hỏng
  ở đâu.
- `LoginAssetImportSettings` — ép `Resources/Ui/` thành Sprite + lọc **Bilinear**
  (ngược với art jar dùng Point). Import nhầm thành Texture thì `Resources.Load<Sprite>`
  trả `null` và toàn bộ UI tụt về màu phẳng mà không có lỗi nào báo.

### Đã xoá

`JarLoginView` (3 file) + 3 file test của nó, và nhánh `FormView` cho màn đăng nhập.
Tham số `pixelCanvas` bị bỏ khỏi `LoginScreens.Initialize` — sau thay đổi này màn
đăng nhập không còn dùng `PixelCanvas`, giữ lại chỉ là một cờ vô nghĩa. `PixelCanvas`
vẫn phục vụ splash và (sau này) các màn theo phong cách jar.

`verify.ps1` 10/10, 379 unit test; test PlayMode mới: `LoginFormViewTests`,
`LoginBackgroundTests`, `LoginScreensFormTests`.

### Ba lỗi hiện ra khi bấm Play, và nguyên nhân thật

1. **"Hai ảnh nền đè lên nhau"** — chính là thiết kế hai lớp ở trên: trên màn rộng,
   lớp lót thò ra hai mép trông như tấm ảnh thứ hai. Bỏ hẳn lớp lót, còn MỘT ảnh phủ
   kín (`EnvelopeParent`). Đổi lại là bị cắt bớt trên/dưới — chấp nhận được vì hai
   mép ảnh chỉ có trời và cỏ.

2. **"Ô nhập có nhiều vết xước"** — sprite ô nhập cắt từ sheet có các pixel chênh nhau
   1-2 mức màu (36,41,45) ↔ (37,42,46). 9-slice kéo dải giữa rộng ~19px ra hơn 600px,
   nên mỗi chênh lệch tí hon ấy thành một sọc dọc rõ mồn một. Vẽ lại sprite ô nhập
   bằng hình bo góc MỘT MÀU (`PIL`, 96×90, bo 26) — kéo giãn bao nhiêu cũng phẳng.

3. **"Nút có vết xước"** — nén texture. `LoginAssetImportSettings` ban đầu quên đặt
   `textureCompression`, nên Unity dùng nén khối mặc định: nó băm ảnh thành ô 4×4 rồi
   xấp xỉ màu, và vùng chuyển màu mượt của nút hiện thành từng vệt loang. Đặt
   `Uncompressed` + tắt crunch (bộ art chỉ vài chục file nhỏ nên đổi bằng dung lượng
   là đáng).

**Lỗi cắt sprite tự phát hiện khi soi lại file:** logo dính một vệt cam lạ ở đáy.
Khung bao của logo trùm xuống tận vùng panel, mà bước cắt đầu tiên crop nguyên khung
chữ nhật nên kéo theo cả viền panel nằm lọt trong đó. Sửa: chỉ giữ pixel mang ĐÚNG
nhãn của cụm đang cắt, không lấy hàng xóm lọt vào khung.

**Sau khi kéo về phải Reimport `Assets/Resources/Ui`** — Unity đã import ảnh trước khi
có `LoginAssetImportSettings`, setting mới không tự áp lên asset đã nằm sẵn trong cache.

## Risk Assessment

| Rủi ro | Xử lý |
|---|---|
| Ảnh mờ vì import setting sai | Ép bằng `AssetPostprocessor`, không giao cho người nhớ. Có test đọc lại setting |
| Phóng lẻ (2.7×) làm pixel méo | `PixelCanvas` chỉ nhận bội số nguyên |
| Sa đà vào "giống từng pixel" | Ranh giới đã ghi rõ ở Overview: khác 3 chỗ là cố ý. Softkey nằm ngoài phạm vi |
| Chép cả `maps/*.dat` vào phase này | **Không.** Format khác hẳn, thuộc P6 |
| 12 MB WAV làm phình bản build | Nén: nhạc nền Vorbis, hiệu ứng PCM/ADPCM. Kiểm kích thước build |
| Chụp jar ở cỡ màn khác 320×240 | Ảnh đối chiếu sẽ sai vì client jar tự co theo màn. FreeJ2ME phải đặt đúng 320×240 — cùng con số `ClientInfo` gửi cho server |
| Sửa view làm hỏng test P5 | Chạy `verify.ps1` + PlayMode sau mỗi bước, không dồn tới cuối |
| Quên khoá hướng màn hình | `ProjectSettings` đang để tự xoay cả 4 hướng. Xoay dọc là khung 4:3 co còn một dải bé tí. Khoá ngang **từ lúc mở app**, và kiểm bằng test đọc lại `ProjectSettings` — đừng tin vào việc nhớ |

### Tỉ lệ khung hình: neo theo CHIỀU CAO, không theo chiều ngang

Đã chốt nằm ngang, nên bài toán đảo chiều so với lúc đầu: máy nằm ngang **rất rộng**
(2400×1080 ≈ 20:9), còn khung jar là 4:3. Thiếu chỗ là thiếu theo **chiều cao**.

Quy tắc của `PixelCanvas`:

```
N            = floor(chiều_cao_màn / 240)      // bội số nguyên, không lấy số lẻ
cao_thật     = 240 × N                          // canh giữa theo chiều dọc
sọc trên/dưới = chiều_cao_màn − cao_thật
rộng_logic   = chiều_rộng_màn / N               // để chiều ngang TRÀN hết màn
```

| Máy (nằm ngang) | N | Cao thật | Sọc trên+dưới | Rộng logic |
|---|---|---|---|---|
| Điện thoại 2400×1080 | 4 | 960 | 120 px | **600** |
| Điện thoại 1920×1080 | 4 | 960 | 120 px | 480 |
| iPad 2732×2048 | 8 | 1920 | 128 px | 341,5 |
| PC cửa sổ 1920×1080 | 4 | 960 | 120 px | 480 |

Vì sao neo theo chiều cao chứ không ép cứng 320×240:

- **Pixel sắc tuyệt đối** — N luôn nguyên, không có chuyện 3,375.
- **Chiều ngang tràn hết màn**, không sọc trái phải. Sọc chỉ còn trên/dưới, ~60px mỗi
  bên trên điện thoại — gần như không thấy nếu lấp bằng màu nền thay vì để đen.
- **Rộng logic luôn ≥ 320**, nên bố cục 320 của jar nằm gọn ở giữa và còn dư lề. Đây
  đúng là hành vi của bản jar trên máy màn rộng: `fb.a()` tự canh giữa bằng
  `this.a = (BaseCanvas.w - var2 >> 1) + this.g`.
- Màn map ở P6 **được nhiều đất hơn** bản gốc. Với game thì đó là lợi, không phải hại.

**Vẫn khai 320×240 với server.** `ClientInfo` giữ nguyên — server dùng con số đó để
quyết định gửi gì, và khai HẸP hơn thực tế là hướng an toàn: nội dung vừa cho 320 thì
hiển nhiên vừa cho 600, thừa lề. Khai rộng hơn thực tế mới là thứ làm cụt chữ. Đổi con
số này là đổi gói tin — nằm ngoài phạm vi phase.

**Phải khoá hướng màn hình, và khoá NGAY TỪ LÚC MỞ APP.** `ProjectSettings` đang để
`defaultScreenOrientation: 4` (tự xoay, cho phép cả 4 hướng) — mặc định của Unity. Để
nguyên thì người chơi xoay dọc là khung 4:3 co lại còn một dải bé tí giữa màn hình.

Cần đặt: `defaultScreenOrientation` = LandscapeLeft (hoặc AutoRotation chỉ bật hai hướng
ngang, để máy lật ngược vẫn dùng được), `allowedAutorotateToPortrait` và
`...PortraitUpsideDown` = 0.

Không có màn nào ở hướng dọc — kể cả splash và đăng nhập. Nhờ vậy mỗi màn hình chỉ có
**một** bố cục, và không có cú chuyển hướng giữa phiên (nguồn lỗi UI kinh điển: canvas
dựng lại, ô nhập mất nội dung đang gõ, bàn phím ảo che mất ô).

## Security Considerations

- **Không có bề mặt tấn công mới.** Asset đọc từ đĩa cục bộ, không qua mạng, không phân
  tích dữ liệu do người khác gửi.
- Tool giải `.dat` đọc offset **từ chính file** → phải kiểm biên trước khi cắt. File hỏng
  hoặc cố tình dị dạng không được làm tool đọc ngoài mảng. Đây là tool build nên hậu quả
  thấp, nhưng thói quen kiểm biên thì giữ nguyên như tầng giao thức.
- Không đưa asset của jar lên repo công khai nếu chưa rõ ràng bản quyền.

## Next Steps

- Xong 5.1 → quay lại **P6 (Map Rendering & Movement)** cho mốc vertical slice.
- 256 tile trong `newMapData/` giải ra ở phase này làm P6 **dễ hơn**: trước đó plan giả
  định phải lấy tile từ server.
- `.anu` (6 file hoạt ảnh skill) để nguyên cho **P7**, không đụng ở đây.

## Unresolved Questions

1. ~~Chép nhánh bố cục nào?~~ **Hết câu hỏi.** Bản jar có layout co giãn chứ không có
   hai bố cục, và cỡ màn để đối chiếu đã bị `ClientInfo` chốt ở 320×240 từ P1. Chạy
   FreeJ2ME ở 320×240.
2. ~~Khung 320×240 áp cho màn nào?~~ **Đã chốt: áp cho tất cả**, một hệ toạ độ duy nhất.
   Chữ cho to lên — nhưng chỉ ở 162 màn menu của server, còn splash và đăng nhập giữ
   hàng 20px của jar. Xem "Một hệ toạ độ, HAI mức trung thành" ở Key Insights.
3. ~~Ngôn ngữ~~ **Đã chốt: lấy VN + EN từ `a.java`.** Cấu trúc đã kiểm, dễ bóc — xem
   "Bảng chuỗi" ở Architecture.
4. ~~Logo splash~~ **Đã chốt: `meLogo.png`.** Đi qua `JarSkin` theo tên nên đổi sau chỉ
   là thay một dòng ánh xạ.
5. ~~Máy cầm dọc hay ngang?~~ **Đã chốt: nằm ngang.** Khoá `defaultScreenOrientation`,
   neo canvas theo chiều cao — xem "Tỉ lệ khung hình" ở Risk Assessment.
6. ~~Màn đăng nhập nằm ngang hay dọc?~~ **Đã chốt: ngang toàn bộ app, khoá ngay từ lúc
   mở.** Không có màn nào ở hướng dọc, nên chỉ một bố cục duy nhất và không tốn thêm công.

**Không còn câu hỏi nào mở. Plan sẵn sàng để thực hiện.**
