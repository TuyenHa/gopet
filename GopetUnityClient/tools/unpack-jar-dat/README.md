# unpack-jar-dat

Giải asset của client J2ME cũ (`client.jar_Decompiler.com`) vào
`GopetUnityClient/Assets/Resources/Jar/Art/` và `Assets/Resources/Jar/Audio/`.

**Dưới `Resources/` là bắt buộc**, không phải chọn tuỳ ý: `JarSkin`/`SoundBank`
nạp asset bằng `Resources.Load(tên)` lúc RUNTIME — cách duy nhất để tên chuỗi
tra ra đúng sprite/clip trong **build thật** (iOS/Android/PC), không chỉ trong Editor.

## Dùng

```bash
node index.js            # giải + copy, ghi đè
node index.js --check    # chỉ so sánh, dùng cho CI — exit 1 nếu lệch
```

## Ba việc

1. **5 kho ảnh `.dat`** (`lg`, `common`, `avatar`, `buttonicon`, `mui`) — giải theo
   format ở `dat-bank.js`, ghi ra `Assets/Resources/Jar/Art/<bank>/<index>.png`.
2. **PNG rời sẵn có** trong jar (tile map, icon pet, skill chiến đấu…) — copy nguyên
   văn, giữ cấu trúc thư mục gốc, vào `Assets/Resources/Jar/Art/Raw/`.
3. **WAV** (`sound/*.wav`) — copy nguyên văn vào `Assets/Resources/Jar/Audio/`.
4. **24 bố cục map** (`maps/*.dat`) — copy nguyên văn vào `Assets/Resources/Jar/Maps/`,
   đổi đuôi thành `.bytes` (xem mục cuối).

## Format `.dat` — đọc kỹ trước khi sửa

Nguồn: `gu.java` (86 dòng) của client J2ME cũ.

```
int        count               <- SỐ MỤC TRONG BẢNG OFFSET, KHÔNG PHẢI SỐ ẢNH
int×count  offset               <- cộng base = (count<<2)+4 để ra vị trí byte thật
rồi:       các khối PNG nối đuôi nhau
```

**Bẫy đã trả giá — `count` không phải số ảnh, mà là `số ảnh + 1`.**
`gu.a(int)` đọc kích thước ảnh bằng `a[i+1] - a[i]`. Với ảnh cuối (`i = count-1`),
biểu thức đó cần `a[count]` — **ngoài mảng**. Java nuốt exception và trả rỗng; tức là
bản thân client gốc cũng chưa từng đọc được "ảnh thứ count" theo cách đó. Mục cuối
trong bảng offset không phải một ảnh — nó là **sentinel đánh dấu hết dữ liệu**.

Đã kiểm chứng bằng cách đọc cả 5 file thật: `offset[count-1]` (sau khi cộng base)
luôn đúng bằng độ dài file.

| File | `count` trong header | Số ảnh THẬT (`count - 1`) |
|---|---|---|
| `lg.dat` | 2 | 1 |
| `common.dat` | 27 | 26 |
| `avatar.dat` | 17 | 16 |
| `buttonicon.dat` | 4 | 3 |
| `mui.dat` | 9 | 8 |
| **Cộng** | | **54** |

Con số 59 (tổng `count`) từng bị hiểu nhầm là số ảnh trong báo cáo đánh giá ban đầu —
đã sửa lại thành 54 sau khi implementation kiểm chứng.

`lg.dat` chỉ có 1 ảnh thật (index 0) — khớp với `fb.java` gọi `gu.a(0)` cho banner màn
đăng nhập, không gọi index nào khác.

## `maps/*.dat` KHÔNG dùng format này

Format riêng (xem `ef.java:104-190`), nên tool chỉ **copy nguyên văn** 24 file map,
không giải gì cả — phần giải nằm ở `JarMapLayout` bên C#.

**Đổi đuôi `.dat` -> `.bytes` khi copy.** Unity chỉ sinh `TextAsset` cho một danh
sách đuôi định sẵn và `.dat` không nằm trong đó: để nguyên thì `Resources.Load`
trả `null` dù file nằm đúng chỗ.

`maps/11.dat` là **nền màn đăng nhập** — `fb.java` dựng `new ef(11, …)` rồi ghi đè
`b()` của lớp cha để vẽ map thay cho nền màu phẳng.

## Kiểm chứng

- Mọi khối giải ra phải bắt đầu bằng chữ ký PNG (`decodeBank` ném lỗi nếu không).
- `offset[count-1]` phải đúng bằng độ dài file (`decodeBank` ném lỗi nếu không).
- `--check` so sánh SHA-256 từng file đích với nội dung tính từ nguồn — không đọc gì
  ngoài `client.jar_Decompiler.com`, nên không có chuyện "đĩa tự lệch mà không ai biết".
