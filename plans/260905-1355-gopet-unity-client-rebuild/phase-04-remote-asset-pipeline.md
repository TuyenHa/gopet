---
phase: 4
title: "Remote Asset Pipeline"
status: complete
priority: P1
effort: "1w"
dependencies: [3]
---

# Phase 4: Remote Asset Pipeline

## Overview

Client yêu cầu ảnh theo đường dẫn, server trả về raw PNG, Unity nạp thành `Texture2D` + cache xuống đĩa.

**Phase rẻ nhất so với giá trị mang lại.** 10.586 PNG đã nằm sẵn trên server (`GServer/assets/`, 141 MB) — không cần export, không cần convert, Unity giải mã PNG sẵn.

## Requirements

**Functional**
- Yêu cầu ảnh theo đường dẫn, nhận PNG, dựng `Texture2D`
- Cache đĩa để không tải lại mỗi phiên
- Nhiều nơi cùng yêu cầu một ảnh → chỉ gửi 1 request
- Placeholder trong lúc chờ tải

**Non-functional**
- Cache có giới hạn dung lượng, xoá theo LRU
- Không block main thread khi giải mã PNG

## Architecture

### Giao thức (`GameController.requestImg()`, `GameController.cs:864`)

Client gửi `COMMAND_IMAGE` (96) kèm `gameType`, `type`, `path`.

Server trả về:
```
sbyte  gameType
sbyte  type
UTF    originPath      <- đường dẫn gốc client đã gửi, dùng làm khoá cache
int    bufferLength
byte[] pngData
```

Điều kiện server trả về (`gameType == 0` và `type != 10 && type != 11`):
- `PlatformHelper.hasAssets(path)` → đọc file từ `GServer/assets/`
- hoặc `path == "img/captcha.png"` → ảnh captcha sinh động cho phiên đó

Nếu `path` không phải file có thật, server thử `int.Parse(path)` rồi tra `GopetManager.itemAssetsIcon[id]` — tức **đường dẫn có thể là số ID item**.

### Luồng

```
UI cần icon "items/123.png"
  -> AssetCache.Get(path)
       -> có trong memory? trả ngay
       -> có trên đĩa? load async -> trả
       -> chưa có: enqueue request, trả placeholder
            -> server trả PNG -> ghi đĩa -> Texture2D -> callback cập nhật UI
```

**Chống trùng request:** giữ `Dictionary<string, List<Action<Texture2D>>>` các request đang chờ. Ảnh thứ 2 yêu cầu cùng path chỉ thêm callback, không gửi gói mới.

### Cache đĩa

`Application.persistentDataPath/assetcache/` — tên file là hash của path (tránh ký tự lạ và đường dẫn quá dài trên Windows).

Client J2ME cũ cũng cache qua RMS (`ef.java` dùng `a.a("image10_" + id)`), nên đây không phải sáng kiến mới — đúng thiết kế gốc.

## Related Code Files

**Create**
- `Assets/Scripts/Assets/RemoteAssetCache.cs` — API chính, dưới 200 dòng
- `Assets/Scripts/Assets/AssetDiskStore.cs` — đọc/ghi/xoá LRU
- `Assets/Scripts/Net/Handlers/ImageHandler.cs` — xử lý `COMMAND_IMAGE`

**Read for context**
- `SRCGOPETGOC/GServer/Server/GameController.cs:864-932` — `requestImg()`
- `SRCGOPETGOC/GServer/Util/PlatformHelper.cs` — `hasAssets`, `loadAssets`
- `client.jar_Decompiler.com/ef.java:84-101` — cách client cũ cache

## Implementation Steps

1. **`ImageHandler`** — parse `COMMAND_IMAGE`, đọc đúng thứ tự `gameType, type, originPath, length, bytes`.

2. **`RemoteAssetCache.Get(path, callback)`**
   - Tra memory cache (`Dictionary<string, Texture2D>`)
   - Tra đĩa (async, không block)
   - Chưa có → thêm callback vào danh sách chờ + gửi request nếu chưa gửi

3. **Nạp texture**
   ```csharp
   var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
   tex.LoadImage(pngBytes);        // Unity tự giải mã PNG
   tex.filterMode = FilterMode.Point;   // giữ nét pixel art
   ```
   > `FilterMode.Point` quan trọng: asset gốc là pixel art low-res, bilinear sẽ làm mờ nhoè.

4. **Ghi đĩa** — ghi PNG thô (không phải texture đã giải mã) để tiết kiệm chỗ và load lại nhanh.

5. **LRU + giới hạn dung lượng** — mặc định 200 MB. Ghi `lastAccess` vào file index, quét khi vượt ngưỡng.

6. **Placeholder** — 1 texture xám 1×1 hoặc icon "?" dùng chung, trả về ngay để UI không bị null.

7. **Captcha** — `path == "img/captcha.png"` không được cache, phải tải mới mỗi lần (ảnh sinh riêng cho phiên).

8. **Test tải hàng loạt** — mở 1 menu có 50 item → 50 request. Xác nhận không trùng, không nghẽn, UI cập nhật dần.

## Success Criteria

**Đã kiểm chứng**

- [x] Yêu cầu 1 đường dẫn có thật → nhận PNG đúng byte (4278/4278)
- [x] 50 request đồng thời cùng 1 path → chỉ 1 gói gửi đi (đếm trên dump server)
- [x] Đường dẫn không tồn tại → hết hạn, không treo, không crash
- [x] Captcha không được ghi đĩa
- [x] Cache vượt hạn mức → xoá theo LRU, file vừa dùng được giữ lại
- [x] Gói `COMMAND_IMAGE` khớp từng byte với client J2ME

**Chỉ nghiệm thu được trong Play mode — hoãn sang P5**

- [ ] Lần 2 lấy từ bộ nhớ, không gửi gói tin
- [ ] Khởi động lại app, ảnh lấy từ đĩa, không gửi gói tin
- [ ] Placeholder hiện ra trong lúc chờ
- [ ] `FilterMode.Point`, ảnh không bị mờ

> Logic của bốn mục này đã viết và compile được với DLL Unity thật, nhưng chưa **chạy**:
> cần một màn hình để hiện ảnh, mà UI thì hoãn sang P5.

### Sửa lại đặc tả sau khi đọc code server

Ba chỗ plan ghi chưa chính xác:

**Captcha KHÔNG phải `"img/captcha.png"`.** Đó chỉ là giá trị khởi tạo (`GameController.cs:183`).
Đường dẫn thật do server sinh kèm **sáu số ngẫu nhiên** nối phía sau (`:195`), mỗi phiên một
khác. Nên nhận diện bằng tiền tố, và vì đường dẫn vốn đã khác nhau mỗi lần nên nguy cơ
không phải "cache nhầm ảnh cũ" mà là "rác đầy đĩa".

**`COMMAND_IMAGE` là opcode đứng riêng**, không phải sub-command (`GameController.cs:341`).

**Server im lặng ở nhiều nhánh hơn plan liệt kê** (`requestImg`): đường dẫn rỗng, đúng bằng
`EMPTY_IMG_PATH` (`dialog/empty.png`), không có file, `gameType != 0`, `type` là 10 hoặc 11,
hoặc captcha chưa sinh. Tất cả đều `return` không một lời. Vì vậy hạn chờ phía client là
**bắt buộc**, không phải tuỳ chọn.

### Kiến trúc: tách theo asmdef chứ không theo thư mục plan đề xuất

Plan đề `Assets/Scripts/Assets/RemoteAssetCache.cs` — nhưng thư mục đó nằm ngoài cả hai
asmdef, sẽ rơi vào `Assembly-CSharp` mặc định. Thay bằng:

| Ở đâu | Gì | Vì sao |
|---|---|---|
| `Net/Images/` | `ImagePackets`, `ImageResponse`, `ImageHandler`, `AssetDiskStore` | thuần C#, test được ngoài Unity |
| `Runtime/Assets/` | `RemoteAssetCache` | phần DUY NHẤT cần `Texture2D` |

`AssetDiskStore` nhận thư mục gốc qua tham số thay vì tự gọi `Application.persistentDataPath`,
nhờ đó cache và LRU test được bằng thư mục tạm — không cần Editor.

### Bằng chứng đã kiểm chứng (2026-09-05)

**Khớp byte với client J2ME.** `ImagePackets.Request("npcs/Su gia bang hoi.png", 2)` khớp
tuyệt đối gói 29 byte bắt được từ phiên client cũ.

Giá trị `type` quan sát được trên dây: **2** cho `npcs/`, **3** cho `anim_characters/`,
`gameMisc/`, `tatoos/`. Server không dùng nó để tra ảnh, chỉ dội lại — nhưng giữ đúng để
dump hai bên còn so được.

**Live, trên phiên đã đăng nhập:**

| Check | Nội dung | Kết quả |
|---|---|---|
| N | Xin `npcs/arena.png` | nhận **4278 byte**, đúng bằng kích thước file trên đĩa server, PNG magic đúng |
| O | 50 nơi cùng xin một ảnh | **1 gói** — đếm độc lập trên dump server, không tin sổ sách của bài test |
| P | Đường dẫn không tồn tại | hết hạn sau 8s, không treo |

**Đột biến, cả ba đều bị bắt:**

| Đột biến | Kết quả |
|---|---|
| Bỏ cơ chế gộp request | 2 test đỏ |
| Bỏ bước "chạm" file lúc đọc (LRU thành FIFO) | 1 test đỏ |
| Sai tên hằng `FilterMode` | bước verify 4/6 đỏ |

### Bịt một lỗ kiểm chứng: `verify.ps1` bước 4/6

`Gopet.Net.UnityCompat` chỉ phủ `Assets/Scripts/Net`. Tầng `Runtime/` dùng `UnityEngine`
nên nằm ngoài tầm nó — `RemoteAssetCache.cs` **không được thứ gì compile** cho tới khi
người dùng focus vào Editor, có thể hàng giờ sau khi viết.

Thêm `tests/Gopet.Runtime.UnityCompat/`: compile tầng Runtime thẳng với DLL của Unity đã
cài (`UnityEngine.CoreModule`, `UnityEngine.ImageConversionModule`). Đường dẫn Unity đọc từ
biến môi trường `UNITY_MANAGED_DIR`, mặc định trỏ bản trên máy này.

### Bốn lỗi code review bắt được — đã sửa

Không cái nào lộ ra trong live smoke, vì tất cả chỉ nổ sau một CHUỖI sự kiện chứ
không phải ở đường đi thuận.

**1. `_inFlight` trừ hai lần cho một lần cộng.** Xin ảnh → hết hạn (đã trừ) → xin lại
đúng lúc đủ 8 gói đang bay nên nằm chờ trong hàng đợi → gói muộn của lần trước tới,
khớp entry mới, trừ lần nữa. Bộ đếm tụt dần xuống âm và `MaxInFlight` mất tác dụng
vĩnh viễn. Sửa: chỉ trừ khi `entry.Sent`.

**2. `Dispose` không gỡ đăng ký.** `MessageRouter.Register` **ném** khi opcode đã có
handler, nên dựng `RemoteAssetCache` lần hai trên cùng router — chuyện xảy ra mỗi lần
nạp lại scene — sẽ chết ngay ở constructor. Thêm `MessageRouter.Unregister`, và
`Dispose` gỡ cả `Ticked`, handler, lẫn placeholder.

**3. Tràn số nguyên trong `JavaBinaryReader.Require`.** `_pos + count > _data.Length`
với `count` sát `int.MaxValue` cho tổng ÂM → lọt → cấp phát mảng khổng lồ. Ngưỡng
`MaxImageBytes` ở tầng trên hoá ra là thứ **duy nhất** đang chắn, tức nó gánh việc chứ
không phải phòng thủ thừa như comment ngụ ý. Sửa thành phép trừ.

**4. Một nơi chờ ném lỗi kéo theo 49 nơi còn lại.** Waiter chạm `GameObject` đã Destroy
là mất cả đợt, `PumpQueue` không chạy, và lỗi nổ ngược lên vòng dispatch của frame.
Bọc từng waiter, báo qua sự kiện `WaiterFailed`.

Kèm theo: `Ticked` không còn ngừng bắn sau khi mất kết nối (nếu không, ảnh đang chờ
không bao giờ hết hạn); ghi đè bộ nhớ huỷ texture cũ; `AssetDiskStore` không ném khi
không tạo được thư mục (cache hỏng không được làm sập đường ống); chạm LRU thất bại
không còn vứt tấm PNG vừa đọc thành công; `EvictIfNeeded` không còn quét cả thư mục
sau MỖI lần ghi; giải mã có ngân sách mỗi frame (`MaxDecodesPerFrame`).

**Hai test luôn xanh** cũng bị chỉ ra và đã sửa: một cái assert `InFlightCount == 0`
khi chưa xin gì, một cái so 0 với 0 trên thư mục trống.

Đột biến kiểm lại: khôi phục lỗi 1 → test vòng đời đỏ; khôi phục lỗi 3 → test tràn số đỏ.

199 unit test (thêm 13).

### Còn lại

- **Chưa chạy trong Unity Play mode.** `Texture2D.LoadImage`, `FilterMode.Point` và
  `persistentDataPath` mới chỉ được compile, chưa được thực thi. Nghiệm thu thật rơi vào P5
  khi có màn hình để hiện ảnh.
- **Chưa đo thời gian `LoadImage`** trên ảnh lớn (rủi ro treo main thread trong bảng dưới).
  Cần Play mode mới đo được.

## Risk Assessment

| Rủi ro | Xử lý |
|---|---|
| Server im lặng khi path không tồn tại (`requestImg` chỉ `return`) | Client phải có timeout riêng, không chờ vô hạn. Sau timeout → placeholder |
| Tải hàng loạt làm nghẽn hàng đợi gửi | Giới hạn số request đang bay (ví dụ 8), xếp hàng phần còn lại |
| Ảnh lớn làm treo main thread khi `LoadImage` | Đo trước. Nếu chậm, dùng `AsyncGPUReadback` hoặc chia nhỏ theo frame |
| Đường dẫn có ký tự lạ / quá dài trên Windows | Tên file cache = hash (SHA-256 rút gọn) của path |

## Next Steps

Xong P4 → P5 (Generic UI Components). P5 dùng `RemoteAssetCache` để hiện icon trong menu.
