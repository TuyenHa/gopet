# Code Review — Phase 4: Remote Asset Pipeline

Ngày: 2026-09-05 23:02
Phạm vi: 11 file (5 production, 4 test, 1 csproj, verify.ps1) — ~920 dòng
Đối chiếu: `GameController.cs:341,872-944,183-195`, `PlatformHelper.cs`, `GopetManager.cs:499`

## Đánh giá chung

Parser đúng từng field, khoá ghép theo `originPath` là đúng (đây là chỗ dễ sai nhất và đã
đúng). Tách Net/Runtime hợp lý, test có sức bắt lỗi thật (đột biến đỏ được). Vấn đề còn lại
tập trung ở **vòng đời** (Dispose/unregister), **kế toán `_inFlight`**, và **chi phí main
thread** — đều là loại chỉ nổ trong Play mode/production, CI hiện tại không thấy.

Không có lỗi trên dây. Không đổi byte nào.

---

## CRITICAL

Không có.

---

## HIGH

### H1. `RemoteAssetCache.Dispose()` không gỡ đăng ký — dựng lại lần 2 sẽ ném
`Assets/Scripts/Runtime/Assets/RemoteAssetCache.cs:135-143`

Dispose chỉ huỷ texture. Còn nguyên:
- `client.Ticked += _handler.Tick` (:42) → handler sống mãi, giữ tham chiếu tới cache đã
  Dispose, mỗi frame vẫn Tick, callback vẫn ghi vào `_memory` đã Clear.
- `router.Register(COMMAND_IMAGE, ...)` (:37) → `MessageRouter.Register` **ném**
  `InvalidOperationException` khi opcode đã có handler (`MessageRouter.cs:35-38`). Reload
  scene / dựng cache thứ hai trên cùng `GopetClient.Router` = crash lúc khởi tạo.
- `_placeholder` không bị Destroy → rò 1 texture mỗi vòng đời.

Sửa:
```csharp
// MessageRouter: thêm
public bool Unregister(sbyte opcode) => _topLevel.Remove(opcode);

// ImageHandler: giữ router đã đăng ký, thêm UnregisterFrom(router)
// RemoteAssetCache.Dispose():
_client.Ticked -= _handler.Tick;
_handler.UnregisterFrom(_router);
if (_placeholder != null) UnityEngine.Object.Destroy(_placeholder);
_placeholder = null;
```
(cần giữ `_client`/`_router` làm field — hiện chỉ dùng trong ctor rồi vứt.)

### H2. `_inFlight` giảm hai lần khi gói tới muộn trúng entry còn xếp hàng
`Assets/Scripts/Net/Images/ImageHandler.cs:136-137`

`OnImage` giảm `_inFlight` mà **không kiểm tra `entry.Sent`**. Kịch bản thật:

1. `Request("a")` → gửi, `_inFlight=1`.
2. `Tick` hết hạn → `_pending.Remove("a")`, `_inFlight=0`. Gói của server vẫn đang trên dây.
3. UI vẽ lại → `Request("a")` lần nữa, lúc này 8 ảnh khác đang bay → "a" nằm trong `_queue`,
   `Sent=false`, **không** tăng `_inFlight`.
4. Gói muộn của bước 1 tới → khớp `_pending["a"]` → `_inFlight--` cho một entry chưa hề
   tăng → `_inFlight=7` trong khi thật sự có 8 gói đang bay.
5. 8 gói kia về → `_inFlight = -1`. Vĩnh viễn lệch, `MaxInFlight` mất tác dụng.

Đây chính là "double-free" mà câu hỏi review đặt ra — có thật, và không test nào bắt.

Sửa:
```csharp
_pending.Remove(response.Path);
if (entry.Sent) _inFlight--;   // entry còn trong _queue thì chưa từng tăng
else _queue.Remove(response.Path);
```
Thêm test: request cho tới khi bão hoà `MaxInFlight`, cho 1 cái hết hạn, request lại path đó
(vào queue), rồi dispatch gói muộn → assert `InFlightCount` không tụt.

### H3. Waiter ném lỗi làm mất các waiter còn lại và ngắt vòng dispatch của frame
`ImageHandler.cs:139-142`, `Tick:102-107`

`waiter(response)` không bọc try/catch. Trong Unity, waiter điển hình là closure chạm vào
`Image`/`GameObject` đã bị Destroy → `MissingReferenceException`. Hậu quả dây chuyền:
- 49 waiter còn lại của cùng ảnh không bao giờ được gọi (UI kẹt placeholder).
- `PumpQueue()` ở :144 bị bỏ qua.
- `MessageRouter.OnError == null` → ném tiếp (`MessageRouter.cs:101`) → thoát khỏi vòng
  `while` trong `GopetClient.Update():96` → các gói còn lại trong hàng đợi frame đó bị hoãn.

Cùng vấn đề với `TimedOut?.Invoke(path)` ở :106 — một handler ném là các path hết hạn còn
lại không được thông báo.

Sửa: bọc từng lần gọi
```csharp
foreach (var waiter in entry.Waiters)
{
    try { waiter(response); }
    catch (Exception ex) { Faulted?.Invoke(response.Path, ex); }
}
```

### H4. Tràn số nguyên trong `JavaBinaryReader.Require` — `MaxImageBytes` là thứ duy nhất chắn OOM
`Assets/Scripts/Net/JavaBinaryReader.cs:27-33` (ngoài phạm vi review nhưng phát hiện từ đường ống ảnh)

```csharp
if (_pos + count > _data.Length)   // unchecked: _pos=8, count=int.MaxValue -> âm -> lọt
```
`ReadBytes(int.MaxValue)` qua được `Require` rồi `new byte[int.MaxValue]` → OOM. Hiện chỉ
`ImageResponse.cs:40` chặn được nhờ `MaxImageBytes` — nghĩa là hằng số đó **load-bearing**,
không phải "phòng thủ thừa" như doc gợi ý. Bất kỳ parser nào sau này gọi `ReadBytes` mà quên
chặn là thủng.

Sửa tận gốc:
```csharp
if (count > _data.Length - _pos) throw new ProtocolException(...);
```
và sửa comment ở `ImageResponse.cs:20` cho đúng: đây là chốt chặn OOM, không chỉ là "chặn gói dị dạng".

---

## MEDIUM

### M1. Mất kết nối → `Ticked` ngừng bắn → ảnh không bao giờ hết hạn
`Assets/Scripts/Runtime/GopetClient.cs:91-102`

`Update()` `return` sớm khi `_socket == null`, nên `Ticked?.Invoke()` không chạy. Sau
`Cleanup()` (disconnect), `ImageHandler.Tick` chết → `_pending` giữ nguyên toàn bộ callback,
`_inFlight` kẹt > 0, `TimedOut` không bao giờ bắn, UI đứng ở placeholder vĩnh viễn.
Thêm nữa `Send` lúc mất kết nối chỉ `Dispose()` gói rồi im (`:81-86`) trong khi handler đã
đánh dấu `Sent=true`.

Sửa: đưa `Ticked?.Invoke()` lên trước guard, hoặc bỏ guard và để `_socket?.Incoming`:
```csharp
private void Update()
{
    if (_socket != null) { /* vòng dequeue */ }
    Ticked?.Invoke();
    /* xử lý _pendingDisconnectReason */
}
```
Cân nhắc thêm `ImageHandler.CancelAll(reason)` gọi từ `Disconnected` để giải phóng ngay.

### M2. `Get()` chạy IO đĩa + giải mã PNG đồng bộ trên main thread — trái yêu cầu phi chức năng của plan
`RemoteAssetCache.cs:71-80, 86-105`; `AssetDiskStore.cs:63`

Plan yêu cầu "Không block main thread khi giải mã PNG" và bước 2 ghi "Tra đĩa (async, không
block)". Code hiện tại: `File.ReadAllBytes` + `Texture2D.LoadImage` + `File.WriteAllBytes`
đều đồng bộ. Mở menu 50 icon = 50 lần đọc đĩa + 50 lần decode **trong một frame**
(`maxPacketsPerFrame=64` cho phép cả 50 gói về cùng frame). Đây là cái đã được ghi nhận
"chưa đo" ở cuối plan — nhưng nó là mục non-functional đã cam kết, nên phải hoặc làm, hoặc
hạ cam kết trong plan.

Sửa rẻ nhất, không cần thread: giới hạn số ảnh giải mã mỗi frame (hàng đợi decode do
`Ticked` rút), hoặc dùng `Texture2D.LoadImage` qua `ImageConversion.LoadImage` với hàng đợi
ngân sách ~2ms/frame.

### M3. `EvictIfNeeded()` quét + sort TOÀN BỘ thư mục cache mỗi lần `Write`
`AssetDiskStore.cs:88, 103-106`

Với 10.586 asset thì mỗi ảnh tải về = `GetFiles()` (10k `FileInfo`, mỗi cái một stat) +
`OrderBy` + `Sum`. Nhân với 50 ảnh của một menu, trên main thread (xem M2). Ở cache đầy đây
là hàng trăm ms.

Sửa: giữ tổng dung lượng chạy trong field, chỉ quét khi vượt ngưỡng thật:
```csharp
private long _knownBytes = -1;   // -1 = chưa biết
// Write: nếu _knownBytes < 0 thì _knownBytes = TotalBytes();
//        _knownBytes += png.Length; if (_knownBytes > _maxBytes) { EvictIfNeeded(); _knownBytes = -1; }
```

### M4. `IsCaptcha` dùng `StartsWith` nhạy văn hoá
`Assets/Scripts/Net/Images/ImagePackets.cs:51`

`path.StartsWith(prefix)` mặc định là `StringComparison.CurrentCulture` — so sánh ngôn ngữ
học, ký tự ignorable (zero-width, soft hyphen) có thể khớp/không khớp bất ngờ, và phụ thuộc
locale máy người chơi. So khoá giao thức phải là ordinal.

```csharp
return path != null && path.StartsWith(CaptchaPathPrefix, StringComparison.Ordinal);
```

Về mặt *đủ hay không*: đúng. Server khởi tạo `captchaPath = "img/captcha.png"`
(`GameController.cs:183`) và mỗi lần hiện dialog nối thêm 6 số (`:195`) — tiền tố phủ cả hai
dạng. Không có file thật nào trong `GServer/assets/img/` bắt đầu bằng `captcha` (đã kiểm),
nên không có false positive.

### M5. `_memory` không giới hạn, và ghi đè làm rò Texture2D
`RemoteAssetCache.cs:22, 95`

- Không có LRU bộ nhớ: duyệt đủ nội dung game có thể nạp dần tới hàng nghìn texture, không
  bao giờ nhả. Đĩa có hạn mức 200 MB, RAM/VRAM thì không.
- `_memory[response.Path] = texture;` ghi đè entry cũ mà không `Destroy` cái cũ — xảy ra
  thật khi gói muộn tới sau lần request lại (kịch bản H2).

Sửa: `if (_memory.TryGetValue(path, out var old) && old != null && old != texture) Destroy(old);`
trước khi gán; thêm hạn mức đếm texture (ví dụ 512) với LRU đơn giản.

### M6. Hợp đồng callback "gọi 2 lần" không được ghi ở doc
`RemoteAssetCache.cs:50-53, 82-83`

Doc chỉ nói "có thể gọi NGAY hoặc sau vài frame". Thực tế `onReady` được gọi **1 hoặc 2
lần**:
- 2 lần: placeholder (:82) rồi ảnh thật (:104)
- 1 lần: memory/disk hit, hoặc đường dẫn rỗng
- 1 lần (kẹt placeholder mãi): hết hạn (`ImageHandler.TimedOut` chỉ log, :38) hoặc PNG hỏng (:89-93)

Caller kiểu `onReady: t => list.Add(t)` sẽ nhân đôi. Phải ghi rõ trong doc, và tốt hơn là
tách API: `Get(path, type, onReady)` trả placeholder qua giá trị trả về, callback chỉ bắn
cho ảnh thật.

Ngoài ra không có cách **huỷ** request khi UI bị Destroy giữa chừng — nguồn trực tiếp của H3.

### M7. Ctor `AssetDiskStore` ném khi không tạo được thư mục → chết cả pipeline ảnh
`AssetDiskStore.cs:34`

`Directory.CreateDirectory` có thể ném `IOException`/`UnauthorizedAccessException` (đĩa đầy,
thư mục chỉ đọc, sandbox). Ném từ ctor → `RemoteAssetCache` ctor ném → khởi tạo client hỏng,
chỉ vì *cache* không dùng được. Cache là tối ưu, không phải chức năng (chính comment ở :92
nói vậy) — hành vi phải nhất quán.

Sửa: bọc try/catch, đặt cờ `_usable=false`, `TryRead`/`Write` thành no-op.

### M8. `TryRead` vứt bỏ dữ liệu tốt khi bước "chạm" thất bại
`AssetDiskStore.cs:63-69`

Đọc xong `png` rồi mới `SetLastWriteTimeUtc`. Nếu file đang bị khoá (AV quét, tiến trình
khác) thì ném `IOException` → `return false` → **bỏ luôn PNG vừa đọc thành công** và đi tải
lại từ server. Chạm LRU là việc phụ, hỏng thì thôi.

```csharp
png = File.ReadAllBytes(file);
try { File.SetLastWriteTimeUtc(file, DateTime.UtcNow); } catch (IOException) { } catch (UnauthorizedAccessException) { }
return true;
```

---

## LOW

### L1. `ImageHandler.Request` không chặn các nhánh server chắc chắn im lặng
`ImageHandler.cs:62-79`

Đã biết trước server **không bao giờ** trả lời: `path == "dialog/empty.png"`
(`GameController.cs:878`) và `type == 10 || type == 11` (`:899`). Hiện chỉ
`RemoteAssetCache.Get:58` chặn `EmptyImagePath`; `ImageHandler` là API tầng Net dùng chung
(LiveSmoke gọi thẳng) nên chặn phải nằm ở đây, và `type` 10/11 chưa ai chặn — gửi đi là chờ
đủ 10s vô ích, chiếm một slot `MaxInFlight`.

```csharp
if (path == ImagePackets.EmptyImagePath || type == 10 || type == 11)
    throw new ArgumentException("Server không trả lời cho tổ hợp này.");
```

### L2. `DeadlineMs` gán lúc Request là code chết, và item xếp hàng không bao giờ hết hạn
`ImageHandler.cs:73` vs `:94, :123`

`Tick` chỉ xét `pair.Value.Sent`, còn `PumpQueue` ghi đè `DeadlineMs` lúc gửi (:123) → giá
trị gán ở :73 không bao giờ được đọc. Hệ quả có thật: một item nằm trong `_queue` không có
hạn nào cả; nếu 8 slot bị chiếm bởi request tới server im lặng thì item thứ 9 phải đợi thêm
trọn 10s nữa. Hoặc bỏ :73, hoặc dùng nó làm hạn chờ-trong-hàng-đợi (đúng hơn với kỳ vọng
"hạn chờ mỗi ảnh" ở doc :23).

### L3. Comment liệt kê thiếu nhánh im lặng của server
`ImageHandler.cs:10-13`

Liệt kê: đường dẫn không tồn tại / gameType != 0 / type 10-11 / captcha chưa sinh. Thiếu
hai nhánh nữa trong `requestImg`: `path == EMPTY_IMG_PATH` (`:878`) và `path.Length == 0` sau
khi tra `itemAssetsIcon` (`:901`), cộng nhánh `loadAssets` ném lỗi bị nuốt (`:936-940`).
Tất cả đều được timeout phủ, nhưng comment đang là tài liệu đối chiếu nên phải đủ.

### L4. Số dòng tham chiếu server lệch 2-3 dòng ở nhiều chỗ
- `ImagePackets.cs:13` ghi `:895`, `switch (gameType)` thật ở **896**, `case 0` ở **898**
- `ImagePackets.cs:38` ghi `:897`, kiểm tra `type != 10 && type != 11` thật ở **899**
- `ImageResponse.cs:4` ghi `:924-930`, dựng gói thật ở **926-933**
- `ImageResponse.cs:16` ghi `:876-889`, thật là **877** (`originPath`) và **888** (`itemAssetsIcon`)
- Plan `phase-04` ghi `requestImg` ở `GameController.cs:864`, thật là **872**

Đúng: `ImagePackets.cs:7` → `:341`; `:24` → `:878`; `:31` → `:195`; `ImageHandler.cs:12` → `:872-943`.

Đề xuất: trích **tên hàm + đoạn code** thay vì số dòng, số dòng sẽ rot.

### L5. Ghi file cache không nguyên tử
`AssetDiskStore.cs:87`

`File.WriteAllBytes` bị ngắt giữa chừng (kill process, mất điện) để lại file cụt. `TryRead`
sẽ trả file cụt đó; `LoadImage` có thể **thành công một phần** và cho ảnh rác được cache vào
`_memory`. Sửa: ghi `.tmp` rồi `File.Move(tmp, final, overwrite: true)`.

### L6. Không có cơ chế vô hiệu hoá cache khi asset trên server đổi
`AssetDiskStore.cs` toàn bộ

Khoá cache chỉ là hash của path. Server thay `npcs/arena.png` thì client dùng bản cũ mãi mãi
(tới khi LRU đá ra). Chấp nhận được cho bản dựng lại, nhưng nên ghi vào plan như hạn chế đã
biết, hoặc thêm số phiên bản vào tên thư mục cache để có nút xoá.

### L7. `SHA256.Create()` mỗi lần gọi — thừa cho mục đích đặt tên file
`AssetDiskStore.cs:45-46`

Cắt 16 byte SHA-256 **là đủ** chống đụng độ (10.586 asset, xác suất ~1e-30 — trả lời câu hỏi
review: không phải rủi ro). Nhưng dùng crypto hash để đặt tên file kéo theo: một
`IDisposable` cấp phát mỗi lần gọi (`PathFor` gọi trong cả TryRead lẫn Write), và rủi ro
managed-stripping trên IL2CPP. FNV-1a 64-bit là đủ và không phụ thuộc gì.

### L8. `verify.ps1` bước 4 làm verify không chạy được trên máy không cài Unity
`verify.ps1:61-70`

Không có Unity ở `D:\Unity Editor\6000.5.4f1` (CI, máy khác) → `dotnet build` fail →
`VERIFY THAT BAI`, che luôn kết quả 5 bước còn lại. Nên skip có báo:
```powershell
$managed = "D:\Unity Editor\6000.5.4f1\Editor\Data\Managed\UnityEngine"
if ($env:UNITY_MANAGED_DIR) { $managed = $env:UNITY_MANAGED_DIR }
if (-not (Test-Path $managed)) { Write-Host "    SKIP: khong tim thay Unity managed DLL" -ForegroundColor Yellow; return }
```

### L9. Comment đầu `verify.ps1` nói "Bon buoc" nhưng liệt kê 6
`verify.ps1:9` — sửa thành "Sau buoc".

### L10. Rủi ro bảo trì của `Gopet.Runtime.UnityCompat.csproj`
`tests/Gopet.Runtime.UnityCompat/Gopet.Runtime.UnityCompat.csproj:30-31`

`Compile Include="..\..\Assets\Scripts\Runtime\**\*.cs"` sẽ tự động kéo mọi file UI của P5
vào; ngay khi ai đó dùng `UnityEngine.UI` hoặc TextMeshPro, bước 4/6 đỏ với lỗi
"type or namespace not found" khó đoán. Ghi sẵn vào comment: thêm `<Reference>` module mới
khi gặp lỗi này. (Ý tưởng của project này thì đúng — nó đã bắt được đột biến `FilterMode`.)

---

## Chất lượng test

**Bắt được lỗi thật** (đã xác nhận bằng đọc code, không chỉ tin bảng đột biến):
- `DocMotFile_LamNoTreLaiTranhBiXoa` — bỏ `SetLastWriteTimeUtc` là đỏ thật (LRU thành FIFO).
- `NhieuNoiXinCungMotAnh_ChiGuiMotGoi` + `KhiCoTraLoi_MoiNguoiChoDeuDuocGoi` — bỏ gộp là đỏ.
- `Response_DoDaiVoLy_NemProtocolException` — **không** always-green: bỏ `MaxImageBytes` thì
  do H4 sẽ ném `OutOfMemoryException` chứ không phải `ProtocolException` → vẫn đỏ. Tốt.
- `Request_KhopTungByteVoiClientJ2me` — hex `60 00 02 0018 ...` khớp: opcode 96, gameType 0,
  type 2, UTF 24 byte. Đây là test giá trị nhất trong nhóm.

**Yếu / gần như luôn xanh:**
- `TraLoiChoDuongDanKhongXin_BiBoQua` (`ImageHandlerTests.cs:93-101`): assert
  `InFlightCount == 0` trong khi chưa request gì — 0 là giá trị khởi tạo, test không thể đỏ.
  Sửa: request 1 ảnh trước, dispatch gói lạ, assert `InFlightCount` **vẫn** là 1.
- `GhiRong_BoQua` (`AssetDiskStoreTests.cs:116-125`): `TotalBytes()==0` cũng đúng khi
  `Write` ném và bị nuốt. Thêm assert `TryRead("rong.png") == false`.

**Thiếu:**
- Kịch bản H2 (gói muộn trúng entry đang xếp hàng → `_inFlight` lệch).
- Waiter ném lỗi (H3).
- `_pending` rỗng lại sau khi mọi thứ xong/hết hạn — hiện không có gì chặn `_pending` phình.
  Đề xuất mở `PendingCount` để assert.
- `AssetDiskStore.KeyOf` là hàm public tĩnh nhưng không có test cho tính ổn định của khoá
  (đổi thuật toán = mất sạch cache của người chơi, im lặng). Ghim một vector:
  `KeyOf("npcs/arena.png") == "<hex 32 ký tự>"`.

**Dead code:** `ImageHandlerTests.cs:16-21` `NewHandler()` không ai gọi (mọi test tự dựng
inline) — xoá.

**Rò trong test:** `_sent` giữ `Message` (IDisposable) không bao giờ Dispose
(`ImageHandlerTests.cs:14`). Không ảnh hưởng kết quả nhưng ngược với quy ước `using var` ở
các test khác.

---

## Điểm tốt

- Ghép theo `originPath` — đúng chỗ dễ sai nhất; comment `ImageResponse.cs:14-16` giải thích
  đúng lý do (server dội lại con số, không phải đường đã tra).
- `ExpectFullyConsumed` ở cuối parser: bắt desync ngay tại gói.
- Timeout phía client vì server im lặng — kết luận đúng từ việc đọc `requestImg`, không đoán.
- `AssetDiskStore` nhận root qua tham số thay vì gọi `Application.persistentDataPath` → test
  được ngoài Editor. Quyết định kiến trúc đúng và được ghi lại trong plan.
- Không cache captcha, và lý do ghi đúng (rác đĩa, không phải "ảnh cũ").
- Bước verify 4/6 bịt đúng lỗ hổng thật (Runtime/ trước đó không được gì compile).
- Đã tự sửa lại 3 chỗ sai của plan sau khi đọc code server thay vì làm theo plan.

---

## Hành động đề xuất (theo thứ tự)

1. H2 — sửa kế toán `_inFlight` (`ImageHandler.cs:136`) + test kịch bản gói muộn.
2. H1 — `Dispose` gỡ `Ticked`/router/placeholder; thêm `MessageRouter.Unregister`.
3. H3 — try/catch quanh từng waiter và từng `TimedOut`.
4. H4 — sửa tràn số trong `Require`; cập nhật comment `MaxImageBytes`.
5. M1 — `Ticked?.Invoke()` chạy cả khi mất kết nối.
6. M8, M7, M3 — vá lỗi IO của `AssetDiskStore` (rẻ, độc lập).
7. M5, M6 — hạn mức bộ nhớ + ghi rõ hợp đồng callback.
8. M2 — ngân sách decode mỗi frame; hoặc hạ cam kết non-functional trong plan cho đúng thực tế.
9. L1, L2, L3, L4 + dọn test (bỏ `NewHandler`, làm chặt 2 test yếu, thêm vector `KeyOf`).

## Câu hỏi chưa giải quyết

- Có phiên chơi nào đủ dài để `_memory` thành vấn đề thật không? Cần số liệu từ Play mode P5
  trước khi quyết hạn mức texture.
- `Texture2D.LoadImage` trên ảnh lớn nhất trong `assets/` mất bao lâu? M2 chọn giải pháp nào
  phụ thuộc con số này.
- Có nên cho `RemoteAssetCache.Get` trả `Texture2D` (placeholder) làm giá trị trả về thay vì
  gọi callback lần đầu không? Sẽ làm hợp đồng M6 tự hiển nhiên, nhưng đổi API trước khi P5
  có caller là rẻ nhất — quyết bây giờ.
