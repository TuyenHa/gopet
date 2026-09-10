# goPet Unity Client

Client Unity nói đúng giao thức của `SRCGOPETGOC/GServer`. Không đổi byte nào trên dây.

Plan đầy đủ: `plans/260905-1355-gopet-unity-client-rebuild/`

## Nguyên tắc kiến trúc

**1. `Assets/Scripts/Net` không phụ thuộc UnityEngine.**
Enforce bằng `Gopet.Net.asmdef` (`noEngineReferences: true`). Nhờ vậy tầng giao thức unit test được ngoài Unity, chạy trong vài giây thay vì phải mở Editor. Cầu nối sang Unity nằm ở `Assets/Scripts/Runtime/GopetClient.cs`.

**2. Opcode sinh tự động, không gõ tay.**
`GopetCmd.cs` sinh từ `GServer/Server/GopetCMD.cs`. 177 hằng số — gõ tay là sẽ sai, và sai opcode thì lỗi hiện ra ở tận đâu.

**3. Test đối chiếu bản port độc lập, không phải round-trip.**
`tools/gen-test-vectors` port TEA sang JS **riêng** từ `TEA.cs` của server, rồi sinh vector. Test C# so `Tea.cs` với vector đó. Round-trip tự nó không chứng minh gì — một bản port sai đối xứng vẫn round-trip tốt.

**4. Lỗi giao thức nổ ngay tại chỗ.**
Đọc quá cuối gói, độ dài âm, mảng vượt ngưỡng → `ProtocolException`. Gọi `ExpectFullyConsumed()` ở cuối mỗi handler. Desync bắt được lúc parse rẻ hơn nhiều so với truy ngược từ triệu chứng ở màn hình.

**5. Luồng nền không đụng Unity API.**
`GopetSocket` đẩy gói vào `ConcurrentQueue`; `GopetClient.Update()` rút ra và dispatch trên luồng chính.

## Cấu trúc

```
Assets/Scripts/
├── Net/                        thuần C#, không UnityEngine
│   ├── Gopet.Net.asmdef        noEngineReferences: true
│   ├── GopetCmd.cs             SINH TỰ ĐỘNG — không sửa tay
│   ├── Tea.cs                  port từ GServer/Server/IO/TEA.cs
│   ├── Message.cs              khung gói
│   ├── JavaBinaryReader.cs     big-endian + readUTF
│   ├── JavaBinaryWriter.cs     big-endian + writeUTF
│   ├── MessageRouter.cs        định tuyến 2 tầng (opcode + sub-command)
│   ├── PacketFramer.cs         format khung, test được qua MemoryStream
│   ├── Handshake.cs            9 byte đầu phiên, tách để test không cần socket
│   ├── GopetSocket.cs          TCP, handshake, 2 luồng
│   ├── PacketLogger.cs         hex dump để diff với server
│   └── Auth/                   luồng đăng nhập
│       ├── ClientInfo.cs       gói -36, hai cổng chặn của server
│       ├── AuthPackets.cs      LOGIN / REGISTER / CREATE_CHAR / CHECK_SPEED
│       ├── AuthRules.cs        ràng buộc tài khoản, chép từ server
│       ├── ServerEntry.cs      đọc SERVER_LIST
│       ├── LoginSuccess.cs     đọc LOGIN_SUCCES
│       └── AuthHandler.cs      gắn tất cả vào MessageRouter
│   └── Images/                 đường ống ảnh
│       ├── ImagePackets.cs     gói COMMAND_IMAGE (96)
│       ├── ImageResponse.cs    đọc PNG server trả về
│       ├── ImageHandler.cs     gộp request trùng, giới hạn gói bay, hết hạn
│       └── AssetDiskStore.cs   cache đĩa + LRU (thuần .NET, test được)
│   └── Guider/                 họ gói UI (122) + hộp Có/Không (45)
│       ├── MenuItemInfo.cs     một dòng menu; showDialog là field ĐIỀU KIỆN
│       ├── MenuScreen.cs       SHOW_MENU_ITEM — định dạng chung của 162 màn hình
│       ├── ListOptionScreen.cs GUIDER_LIST_OPTION — trùng sub 3 với gói đi ngược chiều
│       ├── GuiderDialogs.cs    NPC option, hộp nhập liệu, hộp Có/Không
│       ├── GuiderPackets.cs    các gói client gửi lên
│       └── GuiderHandler.cs    gắn tất cả vào MessageRouter
├── UiLogic/                    thuần C#, không UnityEngine — logic trình bày
│   ├── Gopet.UiLogic.asmdef    noEngineReferences: true
│   ├── MenuSelection.cs        chặn / hỏi trước / gửi ngay
│   ├── DialogStack.cs          chồng màn hình, đóng đúng thứ tự
│   ├── MenuVirtualizer.cs      cuộn tới đâu thì dựng dòng nào
│   ├── LoginStage.cs           các chặng của luồng đăng nhập
│   ├── LoginFlow.cs            máy trạng thái đăng nhập (nối, bắt tay, mất kết nối)
│   ├── LoginFlow.Account.cs    tài khoản, mật khẩu, tạo nhân vật
│   ├── LoginFlow.ServerList.cs chọn máy chủ = nối lại tới địa chỉ nó cho
│   ├── LoginFlow.Timeout.cs    hạn chờ hồi âm — server im lặng cũng là một kết cục
│   ├── SecretBox.cs            AES-256-CBC + HMAC-SHA256, encrypt-then-MAC
│   ├── CredentialStore.cs      nhớ tài khoản giữa hai lần mở app
│   ├── FpsSampler.cs           fps trung bình + 1% thấp, cho MenuBench
│   ├── PixelCanvasLayout.cs    khung 320×240 giống bản jar, neo theo chiều cao
│   └── JarStringTable.cs       đọc JSON phẳng của tools/extract-jar-strings
├── Runtime/                    được dùng UnityEngine
│   ├── Gopet.Runtime.asmdef
│   ├── GopetClient.cs          MonoBehaviour, bơm gói sang luồng chính
│   ├── GopetBootstrap.cs       ráp tất cả lại — bấm Play là chạy
│   ├── GopetBootstrap.JarSkin.cs  khoá hướng ngang, âm thanh, PixelCanvas, splash
│   ├── Input/
│   │   └── InputRouter.cs      Esc = back, Enter = xác nhận (Input System)
│   ├── Audio/
│   │   ├── SoundBank.cs        nạp AudioClip từ Resources/Jar/Audio theo tên
│   │   └── SoundManager.cs     nhạc nền + hiệu ứng + nhớ bật/tắt (PlayerPrefs)
│   ├── Assets/
│   │   ├── RemoteAssetCache.cs bộ nhớ → đĩa → server
│   │   └── TextureFactory.cs   PNG → Texture2D, FilterMode.Point
│   └── UI/
│       ├── UiRoot.cs           gói tin → DialogStack → view
│       ├── GenericMenuView.cs  danh sách dùng chung, có virtualization
│       ├── MenuItemRow.cs      một dòng
│       ├── ChoiceDialogView.cs text + N nút (Có/Không, NPC, list-option)
│       ├── InputDialogView.cs  ô nhập do SERVER mô tả
│       ├── UiBuilder.cs        neo/xếp hàng uGUI dùng chung
│       ├── FormView.cs         biểu mẫu do CLIENT dựng (đăng nhập, tạo nhân vật)
│       ├── LoginScreens.cs     chặng nào thì hiện màn nào
│       ├── LoginScreens.Account.cs  ô nhập tài khoản + nhớ/quên
│       ├── MenuBench.cs        dụng cụ đo fps menu 200 dòng trên thiết bị thật
│       ├── JarSkin.cs          nạp Sprite từ Resources/Jar/Art theo tên
│       ├── JarStrings.cs       nạp chuỗi VN/EN từ Resources/Jar/Strings theo chỉ số
│       ├── PixelCanvas.cs      canvas riêng, phóng nguyên khối cho màn giống bản jar
│       ├── JarLoginView.cs     màn đăng nhập giống bản jar (banner + 2 hàng)
│       └── JarSplashScreen.cs  logo + nhạc s_login lúc mở app
└── Editor/
    └── JarAssetImportSettings.cs  ép Point/Uncompressed cho ảnh, Vorbis/ADPCM cho âm thanh

tools/
├── gen-gopet-cmd/              GopetCMD.cs (server) -> GopetCmd.cs
├── gen-test-vectors/           port TEA độc lập -> TestVectors.json
├── packet-diff/                so 2 packet dump, tìm chỗ desync
├── unpack-jar-dat/             giải .dat + copy PNG/WAV -> Assets/Resources/Jar
└── extract-jar-strings/        bóc bảng chuỗi VN+EN từ a.java -> Assets/Resources/Jar/Strings

tests/
├── Gopet.Net.Tests/            xunit, compile thẳng source của Assets/Scripts
├── Gopet.Net.UnityCompat/      chỉ compile dưới netstandard2.1, không chạy gì
├── Gopet.Runtime.UnityCompat/  compile tầng Runtime với DLL thật của Unity
├── Gopet.Editor.UnityCompat/   compile Assets/Editor với DLL thật của Unity
├── Gopet.PlayMode.Compile/     compile PlayMode test (bắt lỗi TRƯỚC khi phải đóng Editor)
└── Gopet.Net.LiveSmoke/        chạy tầng Net thật với GServer thật

Assets/Resources/Jar/           asset giải từ client.jar_Decompiler.com — xem tools/unpack-jar-dat
Assets/Tests/PlayMode/          nghiệm thu UI bằng máy — xem run-playmode-tests.ps1
```

## Bắt đầu

### Yêu cầu

| | Phiên bản | Ghi chú |
|---|---|---|
| Unity | 6 (6000.x) | dùng C# 9 + netstandard2.1 |
| .NET SDK | 8.0+ | để chạy test ngoài Unity |
| Node.js | 18+ | cho các tool |

> Unity 6 đã đóng gói sẵn .NET SDK ở `<Unity>/Editor/Data/DotNetSdk/dotnet.exe` — dùng được thay cho SDK hệ thống nếu cần.

## Verify

```powershell
powershell -ExecutionPolicy Bypass -File verify.ps1
```

Mười bước, dừng ở bước đầu tiên fail:

1. `GopetCmd.cs` còn khớp `GopetCMD.cs` của server không
2. Các asmdef khai báo đủ tham chiếu
3. Asset lấy từ jar J2ME (`.dat`/`.png`/`.wav`/bảng chuỗi) còn khớp nguồn không
4. Tầng `Net` compile được dưới **netstandard2.1** — ràng buộc thật của Unity
5. Unit test
6. Tầng `Runtime` compile được với **DLL thật của Unity** (bắt sai API `UnityEngine`)
7. Tầng `Editor` compile được với **DLL thật của Unity** (bắt sai API `UnityEditor`)
8. PlayMode test compile được
9. `LiveSmoke` còn compile được (chỉ build, **không chạy** — không cần server)
10. Không file nào vượt 200 dòng **vật lý** (trừ ngoại lệ đã ghi bên dưới)

> Bước 8 từng đếm bằng `Measure-Object -Line`, thứ **bỏ qua dòng trống** — file 260 dòng
> với 60 dòng trống vẫn lọt. Giờ đếm `@(Get-Content).Count`.

Bước 2 bịt một lỗ mà các bước compile khác **không thể** thấy: chúng gộp nhiều thư mục
vào một assembly nên ranh giới asmdef biến mất. Unity thì tôn trọng ranh giới đó và từ
chối compile — và lỗi ấy chỉ lộ ra lúc chạy PlayMode test, tức sau khi đã phải đóng Editor.

### PlayMode test (phải đóng Unity Editor)

```powershell
powershell -ExecutionPolicy Bypass -File run-playmode-tests.ps1
```

Nghiệm thu UI bằng máy: dựng view, bơm gói tin thật qua `MessageRouter`, kiểm dòng nào
bấm được, gói nào được gửi, chồng dialog đóng đúng thứ tự chưa.

Script đọc **file XML kết quả** chứ không tin exit code — batchmode crash lúc thoát vì
không resolve được endpoint analytics của Unity, sau khi đã ghi xong kết quả.

Bước 4 bịt một lỗ thật: `Gopet.Net.UnityCompat` chỉ phủ `Assets/Scripts/Net`, còn `Runtime/`
dùng `UnityEngine` nên nằm ngoài — `RemoteAssetCache.cs` từng không được **thứ gì** compile
cho tới khi mở Editor. Đường dẫn Unity đọc từ `UNITY_MANAGED_DIR`.

Bước 2 quan trọng hơn vẻ ngoài: test chạy trên `net8.0`, nên một API chỉ có ở .NET 8 mà netstandard2.1 thiếu sẽ **pass test nhưng Unity từ chối**. `tests/Gopet.Net.UnityCompat/` chỉ compile chứ không chạy gì, bật `TreatWarningsAsErrors`, tồn tại đúng để bịt lỗ đó.

### Dựng dự án Unity

Basecode này cố tình **không** kèm `ProjectSettings/` và `Packages/`. Unity sẽ ghi đè chúng theo phiên bản của nó, nên commit sẵn chỉ tạo xung đột.

1. Unity Hub → New Project → **2D (URP)** → Unity 6 LTS
2. Đặt tên `GopetUnityClient`, trỏ vào **chính thư mục này** (Unity sẽ merge vào `Assets/` có sẵn)
3. Mở Editor, chờ compile

### Bấm Play (một bước tay, làm một lần)

`GopetBootstrap` tự dựng Canvas, `EventSystem`, handler giao thức, màn đăng nhập và
phím tắt — nhưng **chưa có scene nào gắn nó**. Scene không commit sẵn component vì
Unity ghi đè file scene theo phiên bản của nó.

1. Mở `Assets/Scenes/SampleScene.unity`
2. Tạo GameObject rỗng, Add Component → **Gopet Bootstrap**
3. Điền `Host` / `Port` của GServer, bấm Play

`GopetClient` được `GopetBootstrap` tự thêm vào cùng GameObject — không cần gắn tay.

### Đo fps menu 200 dòng (cần thiết bị thật)

1. Scene rỗng, GameObject rỗng, Add Component → **Menu Bench**
2. Build sang thiết bị cần đo, chạy ~10 giây
3. Đọc dòng chữ trên màn hình, hoặc `logcat`/Player.log tìm `[Gopet] MenuBench:`

Bench **cuộn tự động** để tính cả chi phí dựng dòng mới; đứng yên nhìn danh sách tĩnh
chỉ đo phần dễ. Kết quả báo cả 1% thấp vì trung bình che mất đúng thứ người chơi cảm nhận.

### Chạy tool

```bash
cd tools
npm run gen:all       # sinh GopetCmd.cs + TestVectors.json
npm run check:cmd     # CI: fail nếu GopetCmd.cs lệch so với server

# Asset từ client J2ME cũ — xem tools/unpack-jar-dat/README.md
node unpack-jar-dat/index.js            # giải .dat + copy PNG/WAV -> Assets/Resources/Jar
node unpack-jar-dat/index.js --check    # CI: fail nếu asset đã giải lệch nguồn
node extract-jar-strings/index.js       # bóc bảng chuỗi VN+EN -> Assets/Resources/Jar/Strings
node extract-jar-strings/index.js --check
```

### Chạy test

```bash
cd tests/Gopet.Net.Tests
dotnet test
```

Test compile thẳng `Assets/Scripts/Net/**/*.cs` — đúng những file Unity build, không phải bản sao.

### So packet dump

```bash
node tools/packet-diff/index.js dump-j2me.log dump-unity.log --direction OUT --hex
```

## Chi tiết giao thức đã bake vào code

Ba thứ dễ sai nhất, đã xử lý sẵn:

**Bất đối xứng mã hoá.** Client gửi lên thì **mã hoá**, server gửi xuống thì **không**. Đã kiểm chứng: server không có chỗ nào gọi `new Message(x, true)`; `en.java` phía client cũ đặt cờ mã hoá = true cho gói đi. `Message.Create()` mặc định `IsEncrypted = true`.

**Opcode âm.** `CLIENT_INFO = -36`, `SKILL_CLAN_LOCK = -1`. Dùng `sbyte` xuyên suốt, chỉ cast sang `byte` lúc ghi ra dây. Nhầm sang `byte` là gói đầu tiên đã hỏng.

**`writeUTF` là UTF-8 thường.** Server dùng `Encoding.Convert` sang codepage 65001 (`DataOutputStream.cs:137`), **không** phải Java modified UTF-8. Nên `Encoding.UTF8` của C# là đúng.
> Ngoại lệ đã biết, chưa xử lý: ký tự ngoài BMP (emoji) sẽ lệch — Java ghi CESU-8 6 byte, .NET ghi UTF-8 4 byte. Game gốc không dùng.

Và một điều phát hiện khi dựng test vector: **TEA không nhận payload rỗng.** `pack()` của server ghi `dest[1]` khi mảng chỉ dài 1 → `IndexOutOfRangeException`. Không xảy ra thực tế vì mọi `Message` luôn có ít nhất byte opcode, nhưng `Tea.Encrypt` chặn tường minh để lỗi nói đúng nguyên nhân.

## Trạng thái

| Hạng mục | Trạng thái |
|---|---|
| Sinh opcode | ✅ 177 hằng số, `--check` pass |
| Sinh test vector | ✅ 30 TEA + 5 handshake + 6 UTF + 5 key-derivation |
| Kiểm chứng phép duỗi vòng lặp TEA | ✅ 2000 bộ đầu vào ngẫu nhiên, tương đương nguyên bản |
| Compile `net8.0` | ✅ 0 warning |
| Compile `netstandard2.1` (Unity) | ✅ 0 warning, `TreatWarningsAsErrors` |
| Unit test (offline) | ✅ **359/359 pass** |
| PlayMode test (UI) | ⏳ **108/108 ở lượt trước**; sau đó code-reviewer bắt thêm 3 lỗi thật (H1/H2/M1-M3, xem phase-05.1) — đã sửa, **compile-check xanh, chưa chạy lại PlayMode** vì Editor đang mở |
| Kết nối server thật | ✅ live smoke 15/15 — xem dưới |
| Đăng nhập đầy đủ | ✅ CLIENT_INFO → server list → LOGIN_SUCCES → CHECK_SPEED |
| Đối chiếu byte dump server ↔ client | ✅ khớp từng byte cả hai chiều |
| Unity Editor compile được asmdef | ✅ `Library/ScriptAssemblies/Gopet.Net.dll` |
| Màn hình đăng nhập / chọn máy chủ / tạo nhân vật | ✅ có, PlayMode test phủ |
| Bàn phím (Esc = back, Enter = xác nhận) | ✅ test bơm phím thật qua Input System |
| Nhớ tài khoản (mật khẩu không để plaintext) | ✅ `CredentialStore` + `SecretBox` |
| Chạy trong Play mode | ⏳ có `GopetBootstrap` để bấm Play, **chưa ai bấm** |
| Fps menu 200 dòng trên máy thật | ⏳ có `MenuBench` để đo, **chưa có số** |
| Asset từ jar J2ME (`.dat`, PNG, WAV, chuỗi) | ✅ 54 ảnh + 327 PNG + 10 WAV + 134×2 chuỗi, `--check` xanh |
| Màn splash + màn đăng nhập giống bản jar | ✅ `JarSplashScreen` + `JarLoginView`, PlayMode phủ |
| Khung pixel-perfect (`PixelCanvas`) | ✅ neo theo chiều cao, N nguyên — kiểm ở 3 cỡ máy trong plan |
| Đối chiếu ảnh chụp với FreeJ2ME | ⏳ cần thao tác tay, **chưa làm** |

Tầng mã hoá đã được chứng minh đúng wire: `TeaTests` so byte-với-byte với bản port độc lập, không phải round-trip tự nhất quán.

Server đã chấp nhận handshake do `GopetSocket` sinh ra, và dump hai đầu khớp từng byte. Còn đúng một thứ chưa chứng minh được: **Unity Editor có nuốt asmdef và `GopetClient` không** — chỉ biết khi mở Editor thật.

### Live smoke (cần server đang chạy)

```powershell
cd tests\Gopet.Net.LiveSmoke
dotnet run
```

Chạy chính `Assets/Scripts/Net` với GServer thật:

| Check | Kiểm gì |
|---|---|
| A | Kết nối mở, server không đóng ngay |
| B | Server giải mã được gói mã hoá, trả đúng opcode |
| C | Giải mã phản hồi, `setClientOK = 1`, reader sạch |
| D | Dump client ghi đúng byte — so với hằng số hex viết tay |
| F | **Dump hai đầu khớp từng byte**: OUT của client ≡ IN của server |
| E1 | 5 chu kỳ connect/dispose, cả hai luồng nền đều thoát |
| E2 | Không còn socket ESTABLISHED **hay CLOSE_WAIT** |
| G | `CLIENT_INFO` qua được hai cổng chặn của server |
| H | Đọc được `SERVER_LIST` |
| I,J | Đăng nhập, đọc đủ 5 field `LOGIN_SUCCES` (tự tạo nhân vật nếu tài khoản mới) |
| K,L | Giữ kết nối và trả lời `CHECK_SPEED` |
| M | Gói `REGISTER` được server đọc (đăng ký bị khoá phía server) |

> **`AuthHandler.Tick()` phải được gọi mỗi frame.** Nhịp `CHECK_SPEED` được hoãn tới đúng
> hạn server yêu cầu; quên gọi `Tick()` là không bao giờ trả lời và server đóng kết nối.
> Trả lời SỚM cũng hỏng: server coi là speed-hack rồi bắn lại mỗi 500ms.

Check F là bằng chứng trực tiếp cho "không đổi byte nào trên dây", và nó tự động —
trước đây phải mở hai file ra so bằng mắt.

`verify.ps1` chỉ **build** dự án này chứ không chạy, vì chạy thì cần server thật còn
verify phải chạy được trên máy trống.

Biến môi trường: `GOPET_HOST`, `GOPET_PORT`, `GOPET_DUMP`, `GOPET_SERVER_DUMP`.
Mặc định mọi đường dẫn neo theo vị trí file exe, không theo thư mục hiện hành.

### Ngoại lệ so với rule 200 dòng/file

`Tea.cs` dài 212 dòng. Đây là một thuật toán liền khối — tách `brew`/`unbrew`/`pack`/`unpack` ra file khác chỉ làm khó đọc hơn, và phần lớn số dòng dôi ra là comment giải thích tại sao không được sửa. Giữ nguyên có chủ đích.

Mọi file khác đều dưới 200 dòng.

## Ghi chú bảo mật

TEA yếu và **khoá do chính client sinh rồi gửi lên** trong 9 byte handshake. Đây là thiết kế gốc, giữ nguyên để client J2ME cũ và client Unity chạy song song trên cùng server — công cụ debug tốt nhất cho việc bắt desync. Đừng nhầm nó với một lớp bảo mật thật.

Đổi giao thức là mất khả năng đối chiếu đúng lúc cần nó nhất. Để dành cho sau khi đạt parity.
