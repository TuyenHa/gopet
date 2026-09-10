# Code Review — Phase 5.1: Asset từ jar + màn đăng nhập theo bản J2ME

Ngày: 2026-09-06 · Reviewer: `code-reviewer` · Plan:
[`phase-05-1-jar-assets-and-login-skin.md`](../260905-1355-gopet-unity-client-rebuild/phase-05-1-jar-assets-and-login-skin.md)

## Scope

| | |
|---|---|
| File đọc | 2 tool Node, 9 file C# runtime/logic, 1 file Editor, 10 file test, `verify.ps1`, `README.md`, `gu.java`, `fb.java`, `a.java` |
| LOC (file mới của phase) | ~2.340 |
| Kiểm chứng chạy thật trong lần review này | giải lại 5 file `.dat` bằng Node độc lập, đối chiếu offset ↔ chữ ký PNG; đọc `strings-vi.json` thật; đếm/đo `Assets/Resources/Jar` |
| KHÔNG chạy lại | PlayMode (theo yêu cầu — báo cáo 108/108 xem như còn hiệu lực) |

Đánh giá chung: **chất lượng cao và trung thực về giới hạn của chính nó.** Ranh
giới phạm vi ghi rõ, tool tất định có `--check`, `Resources.Load` gom đúng ba
chỗ, đường FormView cũ giữ nguyên thật (không phải "may mà test còn xanh").
Nhưng có **một lỗi production thật bị chính test che mất** (H1), **một chỗ đọc
sai `fb.java` mà plan khẳng định là đã kiểm** (H2), và một nhóm test không thể đỏ.

---

## Critical

Không có. Không có lỗ bảo mật, không mất dữ liệu, không phá vỡ API đang dùng.

---

## High

### H1 — `JarSplashScreen`: `OnEnable` chạy TRƯỚC khi gán `_sound`/`_minimumSeconds` → không có nhạc, splash tự đóng sau 1 frame

`Assets/Scripts/Runtime/UI/JarSplashScreen.cs:46-49`

```csharp
var splash = go.AddComponent<JarSplashScreen>();   // <- Unity gọi Awake + OnEnable NGAY ở đây
splash._sound = sound;                              // <- quá muộn
splash._minimumSeconds = minimumSeconds;            // <- quá muộn
```

`AddComponent` trên một GameObject **đang active** gọi `Awake`/`OnEnable` ngay
trong lời gọi đó. Lúc `OnEnable` chạy:

- `_sound == null` → `_sound?.PlayMusic("s_login")` **không phát gì**.
- `_minimumSeconds == 0` → `StartCoroutine(DismissAfter(0))`; `WaitForSeconds(0)`
  tỉnh dậy ở frame kế → **`Dismiss()` sau đúng 1 frame**, bất kể tham số truyền vào.

Hệ quả ở production (`GopetBootstrap.cs:96`): splash loé một frame, **không có
nhạc `s_login` nào cả**, và `_flow.Start(host, port)` chạy sớm hơn dự kiến 2 giây.
Tức là hai tiêu chí nghiệm thu của phase — "màn splash: logo + nhạc `s_login`" và
"nhạc nền phát ở màn đăng nhập" — **thực tế chưa đạt**, dù test xanh.

Test không bắt được vì nó tự vá quanh chính lỗi này:

```csharp
// JarSplashScreenTests.cs:54-55
splash.gameObject.SetActive(false);
splash.gameObject.SetActive(true); // ép chạy lại OnEnable trong test
```

Dòng đó chính là bằng chứng: phải ép `OnEnable` chạy LẠI thì mới có nhạc, nghĩa
là lần chạy đầu (lần production dùng) không có nhạc.

**Sửa** — dựng object ở trạng thái tắt rồi mới bật, hoặc bỏ hẳn `OnEnable`:

```csharp
var go = new GameObject("JarSplashScreen", typeof(RectTransform), typeof(Image));
go.SetActive(false);                 // <- trước khi AddComponent
...
var splash = go.AddComponent<JarSplashScreen>();
splash._sound = sound;
splash._minimumSeconds = minimumSeconds;
go.SetActive(true);                  // <- OnEnable chạy khi field đã đủ
```

Rồi **bỏ hai dòng `SetActive` trong test** — nếu bỏ mà test vẫn xanh thì lỗi đã
hết thật; nếu đỏ thì chưa. Đó là phép thử đúng cho ca này.

### H2 — Nhãn ô nhập: dùng SAI chỉ số chuỗi, và chuỗi lấy từ `JarStrings` chỉ dùng để ĐẶT TÊN GameObject

`Assets/Scripts/Runtime/UI/JarLoginView.cs:98,101` + `125-127`

```csharp
_username = MakeField(font, "T.Khoản", JarStrings.Vi(5),   usernameTop);
_password = MakeField(font, "M.Kh:",  JarStrings.Vi(348), passwordTop);

private InputField MakeField(Font font, string placeholder, string label, float top)
{
    var go = new GameObject($"Field_{label}", ...);   // label chỉ vào ĐÂY
    ...
    placeholderText.text = placeholder;               // chữ hiện lên là literal cứng
```

Hai vấn đề chồng nhau:

1. **Tham số bị đảo vai.** Chữ người chơi nhìn thấy là literal `"T.Khoản"` /
   `"M.Kh:"` gõ cứng trong code; giá trị lấy từ bảng chuỗi của jar chỉ dùng để đặt
   tên GameObject. Toàn bộ lý do tồn tại của `JarStrings` (một nguồn, đổi ngôn ngữ
   được) bị vô hiệu ở đúng hai chỗ quan trọng nhất của màn hình. Không test nào so
   `placeholder.text` với `JarStrings.Vi(...)`, nên nếu bảng chuỗi đổi thì UI vẫn
   giữ chữ cũ và không ai biết.

2. **Chỉ số sai so với `fb.java` ở đúng cỡ màn 320×240 mà plan đã chốt.**
   `fb.a(int)` phân nhánh theo `BaseCanvas.w`:

   ```java
   if (BaseCanvas.w >= 240) {        // 320x240 đi nhánh NÀY
      this.b.a(a.a(298));            // hàng 1: "Tên"
      this.c.a(a.a(375));            // hàng 2: "Mật khẩu"
      this.d.a(a.a(397));            // hàng 3: "Nhập lại"  (chỉ dùng khi đăng ký)
   } else {                          // màn hẹp < 240
      this.c.a(a.a(348));            // "M.Kh:"   <- viết tắt cho màn HẸP
      this.d.a(a.a(482));            // "Gõ lại"
   }
   ```

   Đối chiếu thẳng `strings-vi.json`: `298` = `"Tên"`, `375` = `"Mật khẩu"`,
   `348` = `"M.Kh:"`, `5` = `"T.Khoản"`. **`fb.java` không tham chiếu chỉ số `5`
   một lần nào** — nó không phải nhãn ô tài khoản. Và `348` là bản viết tắt của
   nhánh `w < 240`, dùng ở khung 320 là sai nhánh.

   Plan đã sửa chỗ này một lần (`203` → `5`) và lần sửa đó vẫn chưa đúng. Số đúng
   ở cỡ màn tham chiếu: **`298` và `375`**. Cần sửa cả plan lẫn code.

3. Phụ: bản jar vẽ nhãn **cố định** cạnh ô; bản Unity dùng nó làm *placeholder*,
   nên nhãn biến mất ngay khi có chữ (mà `SetCredentials` điền sẵn tài khoản đã
   lưu → mất luôn từ đầu). Ô mật khẩu lúc đó chỉ còn một dãy chấm không nhãn.

---

## Medium

### M1 — `JarSplashScreen.Dismiss()` bắn `Finished` TRƯỚC khi tự dọn → subscriber ném là treo app vĩnh viễn

`JarSplashScreen.cs:67-78`. `_dismissed = true` → `Finished?.Invoke()` →
`SetActive(false)` → `Destroy`. Ở production subscriber là
`() => _flow.Start(host, port)` (`GopetBootstrap.cs:97`), tức là cả đường
`LoginFlow.Connect` → `ConnectRequested` → `GopetClient.Connect`. Một exception ở
đó làm hai dòng dọn dẹp **không bao giờ chạy**: panel phủ kín màn hình, alpha
0,97, `raycastTarget` bật, còn `_dismissed` đã `true` nên chạm lần hai cũng không
làm gì. Người chơi ngồi trước màn splash chết cứng, không log, không nút.

Sửa: dọn trước, bắn sự kiện sau (hoặc `try { Finished?.Invoke(); } finally { … }`).

### M2 — Hai Canvas ScreenSpaceOverlay cùng `sortingOrder = 0`, thứ tự vẽ để mặc

`GopetBootstrap.cs:49` dựng `canvas` (chứa `LoginScreens` + `UiRoot`), dòng 57
dựng `_pixelCanvas` (chứa splash + `JarLoginView`). Cả hai `sortingOrder = 0`.
Khi hoà, Unity phá hoà bằng thứ tự đăng ký nội bộ (thực tế là thứ tự tạo) — đúng
là thứ ta muốn ở đây, nhưng **không có gì trong code nói lên điều đó** và không có
test nào phủ. Đây đúng loại lỗi H1 của phiên trước (layering giữa `UiRoot` và
`LoginScreens`), lần này chuyển sang cấp Canvas nên `BootstrapLayeringTests` — vốn
so `GetSiblingIndex()` trong CÙNG một parent — mù hoàn toàn với nó.

Comment ở `GopetBootstrap.cs:92-95` lập luận "không cần lo thứ tự vì LoginScreens
chưa hiện gì". Lập luận đó đúng cho splash, nhưng không phủ ca ngược: khi
`JarLoginView` đang hiện trên `_pixelCanvas`, một dialog của server đi qua
`UiRoot` (canvas kia) sẽ nằm ở lớp không xác định so với nó. Hôm nay chưa nổ vì
OTP chỉ tới ở chặng `LoggingIn` (lúc đó `JarLoginView` đã bị `ClearViews` huỷ) —
nhưng đó là may, không phải thiết kế.

Sửa rẻ: đặt `sortingOrder` tường minh (ví dụ main = 0, pixel = 10) + một test
`BootstrapLayeringTests` so `sortingOrder` giữa hai canvas.

### M3 — `PixelCanvas.Recompute` chặn thiếu chiều cao; `force:true` không chặn gì

```csharp
_lastScreenSize = new Vector2Int(Screen.width, Screen.height);
if (!force && Screen.width <= 0) return;
ApplyLayout(PixelCanvasLayout.Compute(Screen.width, Screen.height));
```

`Compute` ném `ArgumentOutOfRangeException` khi **rộng HOẶC cao** ≤ 0, còn guard
chỉ nhìn chiều rộng. Thu nhỏ cửa sổ (Windows standalone / Editor) là `Screen.*` về
0 → exception không bắt ném thẳng ra từ `Update()`. Nặng hơn: `Create()` gọi
`Recompute(force: true)`, bỏ qua guard hoàn toàn — nếu lúc `GopetBootstrap.Start()`
màn hình chưa có kích thước hợp lệ thì **cả `Start()` chết**, kéo theo không có
client nào được dựng.

Sửa: guard cả hai chiều, áp cho cả nhánh `force`, và bỏ qua (giữ layout cũ) thay
vì ném.

### M4 — Dòng bản quyền bị cắt cụt; chữ bị phóng mờ trong khung pixel-perfect

- Chuỗi `353` thật là **ba dòng**: `"\nBản quyền 2011 ME Corp.\nGiấy phép cung cấp
  MXH số 35/GXN-TTĐT. Cấp ngày 06/05/2011."`. `JarLoginView.cs:113` đặt nó vào
  `Text` rộng 200, cao 18, cỡ chữ 9, `alignment = UpperCenter`, `verticalOverflow`
  mặc định = `Truncate`. Một dòng trống dẫn đầu ăn mất dòng đầu tiên, câu giấy phép
  chắc chắn wrap rồi bị cắt. Người xem sẽ thấy khoảng một dòng rưỡi.
- `PixelCanvas` phóng `Content` bằng `localScale = N`. uGUI `Text` rasterize glyph
  theo `fontSize` rồi mới bị transform phóng N lần → **chữ nhoè đúng ở màn hình mà
  cả phase này tồn tại để làm nó sắc**. Ảnh thì sắc (`FilterMode.Point`), chữ thì
  không. Muốn chữ sắc phải nhân `fontSize` theo N và chia lại scale, hoặc chấp nhận
  và ghi rõ là chấp nhận.

Cả hai chỉ lộ ra ở bước **"đặt cạnh ảnh chụp FreeJ2ME"** — đúng cái TODO duy nhất
còn để trống. Đừng đóng phase trước khi làm bước đó.

### M5 — 15 MB asset nằm dưới `Resources/`, không strip được, texture ép RGBA32

Đo thật: **381 PNG + 10 WAV, 15 MB (12 MB là audio)** trong
`Assets/Resources/Jar/`. Mọi thứ dưới `Resources/` **luôn** vào bản build, kể cả
256 tile bản đồ chưa ai dùng tới P6. `JarAssetImportSettings` còn ép
`textureCompression = Uncompressed` + `format = RGBA32` cho toàn bộ 381 ảnh.
Cache của `JarSkin`/`SoundBank` là `static` và không bao giờ nhả, nên
`Resources.UnloadUnusedAssets` cũng không lấy lại được gì.

Lý do chọn `Resources/` (tra theo tên chuỗi trong build thật) là đúng và đã ghi rõ
— vấn đề là ô "Kiểm kích thước build" trong Risk Assessment của plan vẫn chưa
tick. Nên đo một bản build thật trước khi coi mục này là xong; nếu phình thì
Addressables là đường ra, không phải bỏ `Resources`.

### M6 — 15 MB asset của bên thứ ba đang chờ được commit, không có `.gitignore`

`D:\game` **chưa phải git repo** (`git status` → not a repository). Khi khởi tạo,
toàn bộ `Assets/Resources/Jar/` sẽ vào theo, gồm cả bảng chuỗi chứa số giấy phép
MXH thật của ME Corp. Chính plan đã ghi: *"Không đưa asset của jar lên repo công
khai nếu chưa rõ ràng bản quyền."*

Điểm mấu chốt: đây là **artefact dẫn xuất thuần tuý** — hai tool đều tất định và
đều có `--check` cần file jar gốc mới chạy được. Nên gitignore chúng gần như không
mất gì, và đó là hướng nhất quán với chính lập luận "tái lập được" của phase.
Quyết dứt điểm trước lần commit đầu, đừng để lỡ tay.

---

## Low

| # | Chỗ | Việc |
|---|---|---|
| L1 | `JarStringTable.cs:31,88` | `Current(json,i)` với input `"{"` ném `IndexOutOfRangeException` chứ không phải `FormatException` như class hứa; `\u` còn <4 ký tự ném `ArgumentOutOfRange`; khoá trùng ghi đè im lặng. Không thủng, chỉ là thông báo lỗi không như quảng cáo |
| L2 | `JarStrings.cs:30` vs `a.java` | Jar thiếu chuỗi thì hiện ra con số (`default: return String.valueOf(var0)`); bản Unity **ném**. Cộng với `CASE_RE` của `extract-jar-strings` lặng lẽ bỏ qua mọi `case` fall-through hoặc trả về biểu thức không phải literal → một bản jar mới có thể biến "hiện ra số" thành "nổ khi dựng màn đăng nhập". `--check` phủ được drift nên chấp nhận được, nhưng khác plan note #3, nên ghi lại |
| L3 | `JarSplashScreen.cs:54`, `JarLoginView.cs:75` | `_sound?.` trên `UnityEngine.Object` — chính `GopetBootstrap.WireKeyboard` có comment cảnh báo `?.` bỏ qua kiểm tra "đã Destroy". Không nhất quán; `SoundManager` bị huỷ sẽ ra `MissingReferenceException` thay vì bỏ qua |
| L4 | `JarAssetImportSettings.cs:39,64` | `assetPath.StartsWith(root)` theo culture hiện hành; nên `StringComparison.Ordinal` cho so khớp đường dẫn |
| L5 | `dat-bank.js:38` | `count < 2` ném với thông báo "ngoài khoảng hợp lệ" — một bank rỗng hợp lệ (count=1, 0 ảnh) sẽ báo sai bản chất. Ngoài ra tool **chặt hơn `gu.java`**: sentinel ≠ độ dài file làm nó từ chối TOÀN BỘ bank, trong khi client gốc vẫn đọc được ảnh 0..count-2. Hai lựa chọn đều bảo vệ được, chỉ cần ghi rõ |
| L6 | `dat-bank.js:11`, README | "Java nuốt exception và trả rỗng" — thực ra `var1` giữ nguyên `new byte[1]` khởi tạo, nên gốc trả **mảng 1 byte** → ảnh 1×1. Kết luận không đổi, chỉ là chữ nghĩa |
| L7 | `SoundManagerTests.cs:45` | `new GameObject("Reload")` không bao giờ bị huỷ — rò một `SoundManager` sống vào scene PlayMode cho các ca sau |
| L8 | `GopetClientReconnectTests.cs:47-51` | `TearDown` có thể `Clear()` trước khi continuation `Accept` kịp `Add` → socket đã accept rò lại tới hết phiên test. Không làm đỏ, chỉ bẩn |
| L9 | `LoginScreens.Account.cs:69` | Closure đọc **field** `_jarLogin` chứ không phải biến cục bộ. Hôm nay không nổ được (`SubmitCredentials` trả `false` không đổi chặng), nhưng bắt biến cục bộ thì lỗi đó thành bất khả thi về mặt cấu trúc |

---

## Test không thể đỏ (theo giá trị "mỗi test phải chứng minh được là nó biết đỏ")

| Test | Vì sao không đỏ được | Sửa |
|---|---|---|
| `JarSplashScreenTests.BatDauThi_PhatNhac` | Hai dòng `SetActive(false/true)` ép `OnEnable` chạy lại **chính là thứ làm nó xanh**. Đường production không được test dòng nào | Bỏ hai dòng đó. Nó phải đỏ cho tới khi H1 được sửa |
| `JarSplashScreenTests.HetThoiGianToiThieu_TuDongDong` | Xanh **nhờ** lỗi H1 (đóng sau 1 frame), không phải nhờ `minimumSeconds = 0.05` được tôn trọng. Xoá hẳn tham số đó test vẫn xanh | `minimumSeconds: 0.5`; assert **chưa** `finished` ở mốc 0,1s rồi mới assert `finished` ở 0,7s |
| `JarStringTableTests.FileThatDaSinh_...` | `if (!File.Exists(path)) return;` — đổi bố cục thư mục là ca đối chiếu dữ liệu THẬT duy nhất biến thành no-op xanh | `Assert.Fail`/`Assert.Ignore` thay vì `return` lặng |
| `JarLoginViewTests.ONhap_CoChieuRongThat` | `sizeDelta` gán cứng 200, anchor giữa → `rect.width` luôn đúng 200. Chỉ đỏ khi ai đó sửa hằng số | Kiểm bố cục thật (xem dưới) thay vì kiểm hằng số |
| `JarLoginViewTests.OMatKhau_BiChe` | Chỉ assert `fields[1]` là Password; che nhầm CẢ HAI ô vẫn xanh | Thêm `AreNotEqual(Password, fields[0].contentType)` |
| `JarLoginViewTests.SetNotice_HienDuocVaXoaDuoc` | Tên hứa "xoá được" nhưng không có ca `SetNotice(null)`; tìm nhãn bằng suy đoán màu | Thêm ca xoá; hoặc phơi `Notice` ra property |
| **Không có test nào cho `PlaceAt`/`Build`** | Đúng vùng rủi ro nhất của phase (đổi trục Y-xuống → Y-lên) không có một assert nào. Lệch dấu, đè hàng, đẩy widget ra ngoài khung 240 — không cái nào bị bắt | Xem đề xuất bên dưới |

**Đề xuất test bố cục** (rẻ, và là thứ duy nhất giữ được phần "giống bản jar"):
đọc `anchoredPosition`/`sizeDelta` của từng widget, đổi ngược về jar-space, rồi
assert (a) thứ tự từ trên xuống đúng, (b) không hai khối nào chồng nhau, (c) mọi
khối nằm trong `[0, 240]`.

Tự tính tay lúc review, bố cục **hiện tại đúng**:

| Khối | jar-space |
|---|---|
| banner | 8 – 100 |
| notice | 103 – 123 |
| tài khoản | 123 – 143 |
| mật khẩu | 146 – 166 |
| nút gửi | 169 – 189 |
| ghi chú | 192 – 212 |
| bản quyền | 222 – 240 |

Không chồng, không tràn. Và `PlaceAt` **an toàn khi đổi cỡ màn**: hai số hạng
`LogicalWidth` triệt tiêu nhau (`x` luôn ra 0), nên widget tự canh giữa. Đúng —
nhưng đúng một cách tình cờ và không có gì canh giữ.

---

## Trả lời trực tiếp các câu hỏi trong yêu cầu review

**1. Format `.dat` — lý thuyết "count−1, mục cuối là sentinel" có đúng không?**
**Đúng, và tôi đã kiểm lại độc lập chứ không đọc theo.** Chạy Node riêng trên cả 5
file: mọi `offset[i]` với `i ≤ count-2` **trùng khít** vị trí chữ ký PNG tìm được
bằng quét toàn file, và `offset[count-1] == độ dài file` ở 5/5. Đọc lại `gu.java`
khẳng định nguyên nhân: `a(int var0)` lấy `a[var0+1] - a[var0]`, nên index đọc
được lớn nhất là `count-2`.

| File | byte | count | ảnh (count−1) | offset ↔ chữ ký PNG | sentinel = len |
|---|---|---|---|---|---|
| `lg` | 4.841 | 2 | 1 | khớp | ✅ |
| `common` | 9.396 | 27 | 26 | khớp | ✅ |
| `avatar` | 5.022 | 17 | 16 | khớp | ✅ |
| `buttonicon` | 670 | 4 | 3 | khớp | ✅ |
| `mui` | 5.929 | 9 | 8 | khớp | ✅ |

Ca sai được nêu trong câu hỏi — không cái nào là lỗ thật:
- **Bank khai đúng 1 ảnh:** `count=1` nghĩa là **0 ảnh** (không có `a[1]` để trừ),
  và code từ chối bằng guard `count < 2`. Đúng bản chất, chỉ thông báo lỗi hơi lệch (L5).
- **Sentinel ≠ độ dài file:** code ném và bỏ cả bank; `gu.java` vẫn đọc được
  0..count-2. Chặt hơn gốc — chọn đúng cho tool build, nên ghi vào README.
- **`count` tràn int32:** `(count<<2)` wrap trong JS có thể qua được guard `base`,
  nhưng `readInt32BE` ném ngay sau đó. Hỏng thì ồn ào, không đọc ngoài mảng.

**2. Toán toạ độ `PixelCanvas`/`JarLoginView`.** Không có lỗi lệch một, không có
lỗi dấu. Bảng jar-space ở trên là kết quả tính tay. `ToAnchoredPosition` đúng
(`y = 120 − jarY` cho gốc trên-trái, Y-xuống). `PlaceAt` đúng và tự canh giữa
được. Vấn đề còn lại **không phải toán**: (a) không có test nào giữ nó (xem trên),
(b) bản jar canh khối ba hàng theo `var12` neo từ ĐÁY (`BaseCanvas.h - gs.m -
this.c`) chứ không xếp từ đỉnh như bản Unity — nên vị trí dọc sẽ nhìn thấy khác
lúc chụp ảnh đối chiếu, (c) `RowWidth = 200` là số tự chọn; tính theo `fb.a()` ở
320 ra ≈194 (`var2 − (gs.o << 1)`). Gần, nhưng nên ghi là "chọn tròn số", đừng để
tưởng là chép ra từ jar.

**3. Trình tự splash → login, và giả định `LoginStage.Idle`.**
**Giả định `Idle` là ĐÚNG** — đã kiểm: `LoginFlow.Stage` khởi tạo `= LoginStage.Idle`,
và `switch` trong `OnStageChanged` không có `case Idle` nào, nên `Initialize()`
chỉ chạy `ClearViews()` rồi rơi ra ngoài, không dựng gì. Không có đường nào
`LoginScreens` hiện được trước splash.

Nhưng đây là **hợp đồng ngầm, không test nào giữ**: ai đó thêm `case
LoginStage.Idle:` (một màn "đang khởi động" chẳng hạn) là hồi quy im lặng ngay.
Thêm một test rẻ: `Initialize` khi `Idle` → `Form == null && ServerPicker == null
&& JarLogin == null`.

Về layering thì **giả định KHÔNG chặt** — xem M2: hai Canvas cùng `sortingOrder`,
thứ tự để mặc cho Unity. Đây đúng loại lỗi H1 phiên trước, chỉ chuyển lên một cấp.

**4. Test không thể đỏ.** 7 cái, xem bảng trên. Nghiêm trọng nhất là hai cái của
`JarSplashScreenTests` — không chỉ yếu, mà đang **che một lỗi production thật**.

**5. Tương thích ngược của `LoginScreens`/`GopetBootstrap`.** **Đạt, và đạt thật
chứ không phải may.** Đã đọc từng dòng:
- `ShowLogin()` chỉ rẽ khi `_pixelCanvas != null`; thân hàm cũ (FormView, nút
  "Quên tài khoản đã lưu", `DefaultButton`, điền sẵn từ `CredentialStore`) không
  đổi một ký tự.
- Hai tham số mới của `Initialize` là optional ở CUỐI, mặc định `null` → mọi lời
  gọi cũ bind y hệt.
- `ClearViews()` thêm `_jarLogin` — no-op có kiểm null trên đường cũ.
- `Discard()` giữ đúng thứ tự `SetActive(false)` rồi mới `Destroy` (đúng bẫy
  "Destroy hoãn tới cuối frame" mà dự án từng dính).
- Thay đổi hành vi **thật sự có một**, và nó nằm ở `GopetBootstrap`:
  `_flow.Start(host, port)` không còn chạy trong `Start()` mà chờ `splash.Finished`.
  An toàn nhờ `Idle` (mục 3), nhưng vì H1 nên độ trễ thực tế là 1 frame chứ không
  phải 2 giây — nghĩa là hành vi "đúng" của thiết kế này **chưa từng chạy lần nào**.

**6. `SoundManager` một cờ + kiểu tự huỷ.**
- Một cờ: **đúng bản jar**, và `StopMusic` xoá `_pendingMusic` nên bật tiếng lại
  không hồi sinh bài đã dừng (có test). Khe hở duy nhất: `SetEnabled(false)` dừng
  nhạc nhưng không dừng hiệu ứng `PlayOneShot` đang bay — bấm nút rồi tắt tiếng
  ngay thì vẫn nghe hết tiếng bấm. Vụn.
- Tự huỷ: `LoginScreens.Discard` **đúng** (tắt trước, huỷ sau).
  `JarSplashScreen.Dismiss` cũng theo đúng thứ tự đó — nhưng bắn sự kiện TRƯỚC cả
  hai, và đó là M1.
- Lưu ý về giá trị chứng minh của test âm thanh: PlayMode test chạy trên scene
  trống, không có `AudioListener`. `AudioSource.isPlaying == true` chứng minh
  nguồn đã được **khởi động**, không chứng minh có **nghe thấy**. (Scene thật
  `SampleScene` có Main Camera + AudioListener nên production ổn.)

---

## Điểm làm tốt

- **`gu.java` được đọc tới nơi tới chốn.** Lần ra `a[i+1] - a[i]` để suy ra
  "client gốc cũng không đọc được ảnh cuối" là suy luận sắc; `decodeBank` còn kiểm
  cả sentinel lẫn chữ ký PNG và **ném** thay vì lặng lẽ ghi ra file rác.
- **`--check` là thứ thật.** So SHA-256 từng file với nội dung tính lại từ nguồn,
  không đọc gì ở đích để tự trấn an. Đã có đột biến thật để kiểm.
- **`tests/Gopet.Editor.UnityCompat/`** — bắt được `assetTarget` → `assetImporter`
  trước khi mở Editor. Đây là loại đầu tư trả lãi mỗi lần đụng `Assets/Editor`.
- **`Resources.Load` gom đúng ba chỗ** (`JarSkin`/`SoundBank`/`JarStrings`) — grep
  toàn `Assets/Scripts` + `Assets/Editor` xác nhận không có chỗ thứ tư. Tên sai thì
  ném kèm câu "đã chạy tool chưa?" — đúng thứ người đọc log cần.
- **`JarSkinTests.BankIndexNgoaiSo_ThiNem`** ghim lý thuyết `count-1` xuống tận
  tầng asset: `Bank("lg", 1)` phải ném. Ca này biết đỏ.
- **Ranh giới phạm vi viết thẳng vào comment của class** (`JarLoginView` không phải
  bản dịch 1:1 của `fb.java`; `JarSplashScreen` khác jar ở LÝ DO đóng màn). Người
  sau đọc code không phải suy diễn.
- **Chẩn đoán race trong `GopetClientReconnectTests` là ĐÚNG.** `IsConnected` chỉ
  phản ánh bắt tay TCP ở tầng OS, không nói gì về việc continuation của
  `AcceptTcpClientAsync` đã điền `_accepted` chưa. Bản sửa đúng hướng, và
  `Assert.IsNotEmpty` kèm theo làm lần đỏ sau này đọc được ngay. (Chỉ an toàn nhờ
  `UnitySynchronizationContext` đưa continuation về main thread — `Accept` phải
  luôn được gọi từ main thread; hôm nay đúng vậy.)

---

## Recommended Actions

**Chặn nghiệm thu phase:**

1. **H1** — dựng `JarSplashScreen` ở trạng thái tắt rồi mới bật (hoặc bỏ `OnEnable`
   sang một `Begin()` tường minh). Rồi **bỏ hai dòng `SetActive` trong test** và
   xác nhận test vẫn xanh.
2. **H2** — đổi `MakeField` để chữ hiển thị lấy từ `JarStrings`, và sửa chỉ số:
   `298` ("Tên") + `375` ("Mật khẩu"). Sửa cả plan (`5`/`348` là sai). Thêm test so
   `placeholder.text` với `JarStrings.Vi(...)`.
3. **M1** — dọn trước, bắn `Finished` sau.
4. Sửa 3 test không thể đỏ ở nhóm splash + `FileThatDaSinh_...`.

**Trước khi đóng phase:**

5. **M3** — guard cả hai chiều màn hình trong `Recompute`, kể cả nhánh `force`.
6. **M2** — `sortingOrder` tường minh cho hai Canvas + test so lớp giữa chúng.
7. Thêm test bố cục jar-space (thứ tự / không chồng / trong khung 240) và test
   `Idle` → không dựng view nào.
8. **M4** — làm bước chụp ảnh FreeJ2ME 320×240. Dòng bản quyền và độ nét của chữ
   chỉ lộ ra ở đó.

**Trước lần commit đầu tiên:**

9. **M6** — quyết dứt điểm: gitignore `Assets/Resources/Jar/` (chúng là artefact
   dẫn xuất, tool tái tạo được) hay chấp nhận bản quyền một cách tường minh.
10. **M5** — đo kích thước một bản build thật, tick nốt ô trong Risk Assessment.

**Nhặt lúc rảnh:** L1–L9.

---

## Metrics

| | |
|---|---|
| File `.cs` mới/sửa | 12 (tất cả < 200 dòng, `JarLoginView` sát trần ở 194) |
| Tool Node mới | 2, cả hai có `--check` |
| Test mới | 8 PlayMode + 2 xunit |
| Test không thể đỏ | 6 yếu + 1 vùng trống hoàn toàn (bố cục) |
| Lỗi production tìm được | 1 High (H1), 1 Medium treo app (M1), 1 Medium sai chỉ số (H2) |
| `Resources.Load` ngoài 3 lớp bọc | 0 |
| Asset ship | 381 PNG + 10 WAV = 15 MB, tất cả dưới `Resources/` |

## Unresolved Questions

1. **Ba hàng hay hai?** `fb.java` dựng ba `ge`; hàng thứ ba (`a.a(397)` = "Nhập
   lại") là của luồng đăng ký. Bản Unity dựng hai — hợp lý vì server khoá đăng ký,
   nhưng ảnh chụp đối chiếu sẽ lệch một hàng. Coi là cố ý và ghi vào mục "ba chỗ
   khác" của plan, hay dựng hàng thứ ba dạng tắt?
2. **Chữ trong khung phóng:** chấp nhận chữ mềm (dùng scale như hiện tại) hay nhân
   `fontSize` theo N để chữ cũng sắc? Ảnh hưởng tới mọi màn dùng `PixelCanvas` ở P6
   nên quyết sớm thì rẻ hơn.
3. **`Assets/Resources/Jar/` vào git hay không** — chưa có repo nên chưa ai phải
   quyết, nhưng lần commit đầu là hết cửa lùi.

---

**Status:** DONE_WITH_CONCERNS
**Summary:** Phase 5.1 vững về kiến trúc, tool tất định, tương thích ngược đạt
thật; lý thuyết format `.dat` đã kiểm chứng độc lập là đúng 5/5. Nhưng có một lỗi
production bị chính test che (splash không phát nhạc và tự đóng sau 1 frame), một
chỗ đọc sai `fb.java` khiến nhãn ô nhập dùng sai chỉ số VÀ chuỗi từ `JarStrings`
chỉ dùng đặt tên GameObject, cùng một đường treo app nếu subscriber của
`Finished` ném.
**Concerns/Blockers:** H1 + H2 + M1 nên sửa trước khi coi phase là xong — cả ba
đều nằm đúng trên đường người chơi đi qua mỗi lần mở app, và cả ba đều đang có
test xanh phủ lên.
