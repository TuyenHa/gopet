# Đánh giá: dựng lại giao diện + asset theo bản jar J2ME

**Ngày:** 2026-09-06
**Câu hỏi:** giao diện đăng nhập và map giống bản jar, ảnh và âm thanh copy từ
`client.jar_Decompiler.com` — mất bao lâu?
**Trạng thái:** đánh giá, chưa làm gì.

## Kết luận ngắn

| Hạng mục | Ước lượng | Rủi ro |
|---|---|---|
| Copy ảnh + âm thanh, dựng đường ống asset cục bộ | **1–1.5 ngày** | Thấp |
| Giải 5 file `.dat` kho ảnh | **0.5 ngày** | **Rất thấp** — format đã giải xong hôm nay |
| Màn đăng nhập giống bản jar | **2–4 ngày** | Trung bình — phụ thuộc quyết định độ phân giải |
| Âm thanh trong game (nhạc nền + hiệu ứng nút) | **1 ngày** | Thấp |
| **Cộng phần "giao diện đăng nhập + asset"** | **~1 tuần** | |
| Map giống bản jar | **không phải việc thêm** — đó chính là P6 | Trung bình |

Điều quan trọng nhất: **kiến trúc không phải sửa gì.** Giao diện hiện tại sơ sài vì
P5 cố ý dựng bộ khung chức năng trước, không đụng tới mỹ thuật. Thay ảnh là thay một
lớp da bên ngoài, không phải viết lại.

## Đã kiểm chứng có gì trong jar

```
327 PNG    256 trong newMapData (tile bản đồ), 30 skill chiến đấu,
           13 nút pet, 11 ảnh pet, 8 chiến đấu, 6 ảnh rời (logo, icon, khoá…)
 10 WAV    12 MB. s_login 1.6 MB, s_outMap_0/1 ~5 MB mỗi file (nhạc nền),
           còn lại là hiệu ứng: đánh, chí mạng, trượt, nút bấm, lên cấp
 29 DAT     5 kho ảnh (lg, common, avatar, buttonicon, mui) + 24 file map
  6 ANU    hoạt ảnh skill chiến đấu — thuộc P7, chưa cần bây giờ
```

Ảnh rời quan trọng: `meLogo.png` / `taeLogo.png` đều **260×72**, `icon.png` 54×54.
`lg.dat` (màn đăng nhập) chứa một ảnh **173×92**.

## Phát hiện đáng giá nhất: format `.dat` kho ảnh rất đơn giản

`gu.java` chỉ 86 dòng. Format:

```
int      count
int×count offset      <- cộng thêm (count<<2)+4
rồi:     các khối PNG nguyên vẹn
```

Kiểm chứng bằng cách giải thật, đếm chữ ký PNG ở từng offset:

| File | Byte | Số mục | Kết quả |
|---|---|---|---|
| `lg.dat` | 4.841 | 2 | 1/2 khớp chữ ký PNG |
| `common.dat` | 9.396 | 27 | 26/27 khớp |
| `avatar.dat` | 5.022 | 17 | 16/17 khớp |
| `buttonicon.dat` | 670 | 4 | 3/4 khớp |
| `mui.dat` | 5.929 | 9 | 8/9 khớp |

Mục cuối mỗi file không khớp là do cách tính biên khối cuối, không phải format sai.

**Nghĩa là:** ~30 dòng C# là giải được toàn bộ 59 ảnh trong đó. Đây KHÔNG phải việc
dịch ngược. Trước khi đo thì đây là ẩn số lớn nhất của cả hạng mục.

`maps/*.dat` thì **khác hẳn** — đọc 4 byte đầu ra số lượng vô lý (51 triệu), tức là
format khác. Đó đúng là thứ `ef.java:104-130` mô tả và là rủi ro đã ghi sẵn cho P6.

## Ảnh trong game KHÔNG nằm trong phạm vi này

Server có **31.772 PNG** và client đã lấy qua mạng được từ P4 (`RemoteAssetCache`:
bộ nhớ → đĩa → server, có gộp request và hết hạn). Icon shop, vật phẩm, pet, NPC đều
đi đường đó.

Phần lấy từ jar chỉ là **lớp vỏ do client tự vẽ**: logo, nền màn đăng nhập, nút bấm,
tile bản đồ, âm thanh.

## Việc phải quyết trước khi bắt tay: độ phân giải

Đây là thứ quyết định ước lượng 2 ngày hay 4 ngày, và tôi không tự quyết được.

Client J2ME vẽ ở **320×240**. Logo 260×72 chiếm gần trọn chiều ngang màn hình đó.
Trên điện thoại 1080×2400 ngày nay:

| Cách làm | Kết quả | Công |
|---|---|---|
| **A. Phóng nguyên khối, lọc Point** | Vuông vức, đúng chất retro, giống bản jar nhất. Logo 260px phóng 4× vẫn nét kiểu pixel | ít nhất |
| **B. Phóng có làm mượt** | Mờ. Không nên | ít |
| **C. Vẽ lại ở độ phân giải cao, giữ bố cục** | Đẹp trên máy mới nhưng **không còn "giống bản jar"** | nhiều nhất, cần người vẽ |

Tôi đề nghị **A**. Nó đúng ý "giống bản jar", rẻ nhất, và hợp với việc 31.772 ảnh
phía server cũng là pixel art cùng thời — `TextureFactory` đã đặt `FilterMode.Point`
sẵn từ P4 nên đường ống đã đúng.

Nói thẳng một điều: **chữ sẽ không bao giờ giống hệt bản jar.** J2ME dùng font hệ
thống của từng máy, không phải bitmap font đóng gói trong jar. Copy được ảnh chứ
không copy được cách máy Nokia rasterize chữ. Gần giống thì được.

## Bóc tách công việc

### 1. Đường ống asset cục bộ — 1 ngày
- Script Editor giải 5 file `.dat` ra `Assets/Art/`
- Chép 327 PNG vào, đặt import preset: `FilterMode.Point`, tắt nén, `Sprite`
- 10 WAV → `AudioClip`; nhạc nền nén Vorbis, hiệu ứng để PCM
- Test: số ảnh giải ra đúng bằng số mục khai trong header

### 2. Màn đăng nhập — 2–4 ngày
- **Chưa làm được ngay:** mới tìm ra `fx.java` là màn splash (logo + `s_login`),
  chưa định vị được đúng file vẽ ô nhập tài khoản. Phải đọc thêm ~0.5 ngày.
- Dựng lại: nền, vị trí logo, ô nhập, thanh softkey dưới đáy
- Nối vào `FormView` đã có — logic đăng nhập không đụng tới
- Rủi ro: J2ME vẽ trực tiếp lên canvas theo toạ độ tuyệt đối; uGUI dùng anchor.
  Dịch bố cục 320×240 sang layout co giãn là phần tốn công thật, không phải phần ảnh.

### 3. Âm thanh — 1 ngày
- Nhạc nền theo màn (`s_login`, `s_outMap_0/1`), hiệu ứng nút (`s_button`)
- Nhớ trạng thái bật/tắt như `ISoundManagerSDK.loadMusicState`
- 12 MB WAV cần nén lại cho mobile

### 4. Map — KHÔNG phải việc thêm
Đây chính là P6 (3–4 tuần, đã có trong plan). Việc có sẵn 256 tile trong
`newMapData/` làm P6 **dễ hơn**, không phải khó hơn — trước đó plan giả định phải
lấy tile từ server.

Rủi ro thật của P6 vẫn nguyên: format `maps/*.dat`.

## Đề nghị thứ tự

1. **Làm mục 1 trước (1 ngày).** Rẻ, không rủi ro, và có ảnh thật trong tay thì mọi
   ước lượng sau đó chính xác hơn nhiều.
2. Quyết cách xử lý độ phân giải (A/B/C ở trên).
3. Làm màn đăng nhập.
4. Âm thanh có thể làm song song, không phụ thuộc gì.
5. Map để nguyên trong P6.

## Câu hỏi chưa có lời đáp

- Chọn cách nào trong A/B/C cho độ phân giải?
=> Tạm thời chọn A
- Có cần giống cả **thanh softkey** kiểu J2ME (hai nút trái/phải dưới đáy) không, hay
  dùng nút cảm ứng bình thường? Cái này ảnh hưởng tới cả 162 màn hình menu chứ không
  riêng màn đăng nhập.
  => Không cần hai nút trái/phải, tôi build để dùng cho smart phone ios và android, pc nên dùng cảm ứng
- Bản jar hiện tại có chạy được để chụp màn hình đối chiếu không? Không có ảnh chụp
  thì "giống bản jar" chỉ kiểm được bằng cách đọc code, chậm hơn nhiều.
  => tôi thấy có bản jar chạy được, tôi đã cài phần mềm hỗ trợ FreeJ2ME chạy file jar
