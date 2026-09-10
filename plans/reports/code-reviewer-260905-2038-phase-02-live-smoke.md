# Code Review — Phase 2 Transport Layer / Live Smoke

Ngay: 2026-09-05 | Reviewer: code-reviewer
Scope: `tests/Gopet.Net.LiveSmoke/{Program.cs,ClientInfoPacket.cs,*.csproj}`,
phan MOI THEM trong `tests/Gopet.Net.Tests/{WireFormatTests.cs,MessageAndRouterTests.cs}`.
Ngu canh: `Assets/Scripts/Net/*`, `verify.ps1`, `SRCGOPETGOC/GServer`.

## Danh gia chung

Chat luong kha. Harness co suy nghi ve falsifiability (da tu kiem cong chet / version sai).
Fact-check: thu tu field CLIENT_INFO trong `ClientInfoPacket.cs` KHOP chinh xac
`SRCGOPETGOC/GServer/Server/Player.cs:105-119`; `VERSION_142 = 1.4.2` (`Manager/GopetManager.cs:643`)
nen `1.4.3` hop le; `VI_CODE = "vi"` (`:653`) hop le; reply cua `setClientOK`
(`Server/IO/Session.cs:32-39`) dung la opcode -36 + 1 byte -> `ReadBool` + `ExpectFullyConsumed` dung.

Van de chinh: **mot so check khong the do (fail) duoc**, va **hai phep do leak (E1/E2)
deu lech ve phia am tinh gia**. Khong co lo hong bao mat moi.

---

## CRITICAL

Khong co.

---

## HIGH

### H1. Check D la check khong the fail (always green)
`tests/Gopet.Net.LiveSmoke/Program.cs:93`

`PacketLogger` ctor (`Assets/Scripts/Net/PacketLogger.cs:41-45`) tao file va ghi dong header
NGAY tu luc `new PacketLogger(dumpPath)` o `Program.cs:62`. `File.Exists(dumpPath)` do do luon true,
ke ca khi khong mot goi nao duoc log. Check D khong bao gio do -> khong co gia tri.

Nang hon: packet-diff la "cong cu chinh de bat desync" (theo docstring PacketLogger) nhung
harness chi *in ra goi y lenh diff* (`:55`) chu khong chay. Tuyen phong thu so 1 khong duoc kiem.

Sua: payload log cho OUT la `message.ToWire()` truoc ma hoa (`GopetSocket.cs:120`), tuc la
**hoan toan tat dinh** voi input co dinh cua `ClientInfoPacket`. Assert golden hex:

```csharp
var outLine = File.ReadLines(dumpPath).FirstOrDefault(l => l.Contains("\tOUT\t"));
var cols = outLine?.Split('\t');
Check("D. Dump ghi dung goi CLIENT_INFO",
    cols != null && cols[2] == "-36" && cols[5] == ClientInfoPacket.ExpectedWireHex,
    outLine == null ? "khong co dong OUT nao" : $"dump lech: {outLine}");
```
(dat `ExpectedWireHex` const canh `Build()` de doi field la test do ngay).

### H2. E2 chi soi `Established` — bo sot `CloseWait`, tuc dang ro socket kinh dien
`Program.cs:150-156`

`ActiveEstablishedPorts` loc `c.State == TcpState.Established`. Socket bi ro dien hinh
(server da FIN, client khong dong) nam o **CLOSE_WAIT**, khong phai ESTABLISHED -> E2 bao xanh
trong dung truong hop no sinh ra de bat.

Sua: `.Where(c => c.State == TcpState.Established || c.State == TcpState.CloseWait)`.
(`TimeWait`/`FinWait*` la dong chu dong binh thuong, van loai tru.)

### H3. E2 co the pass vacuously khi khong lay duoc local port
`Program.cs:114,127-129`

`LocalPortOf` tra 0 khi khong tim thay. `localPorts.Where(x => x > 0)` loc het -> `Intersect`
rong -> E2 PASS. Neu `IPGlobalProperties` khong tra du lieu (host khac, GOPET_HOST tro ra ngoai,
timing), E2 xanh ma chua do gi.

Sua: chan truoc, coi la fail cua chinh phep do:
```csharp
Check("E2a. Ghi nhan duoc local port moi chu ky",
    localPorts.All(p => p > 0), $"khong soi duoc: [{string.Join(",", localPorts)}]");
```

### H4. `LocalPortOf` khong loc state -> ghi nham port cua ket noi cu (am tinh gia)
`Program.cs:144-148`

`ActiveConnections(remotePort)` khong loc state, `FirstOrDefault()` co the tra ve ket noi
**TIME_WAIT/CLOSE_WAIT con sot lai tu `RunProtocolChecks`** hoac tu chu ky truoc, thay vi
socket cua chu ky hien tai. Hau qua: `localPorts` chua port sai (co the trung nhau),
E2 kiem tra nham socket -> ro that van xanh. Ket noi 127.0.0.1 lam kha nang nay cao hon vi
nhieu entry cung remote port 19180.

Sua tot nhat: lay port tu chinh socket thay vi doan tu bang TCP. Them vao `GopetSocket`:
```csharp
public System.Net.IPEndPoint LocalEndPoint => (System.Net.IPEndPoint)_client?.Client?.LocalEndPoint;
```
roi `localPorts[i] = socket.LocalEndPoint?.Port ?? 0;`. Deterministic, bo duoc ca H3.
Sua toi thieu: loc `State == Established` trong `LocalPortOf` va loai cac port da ghi nhan.

### H5. E1 (dem `Process.Threads.Count`) la phep do yeu — lech ve am tinh gia
`Program.cs:103-125`

Ba nguon sai, deu theo huong **bo sot ro**:

1. **Baseline bi thoi phong.** `threadsBefore` lay o `:106` ngay sau khi `RunProtocolChecks`
   tra ve, tuc dung luc 2 luong "Gopet Read"/"Gopet Write" cua socket dau tien vua duoc
   `Join(1s)` nhung co the chua thoat khoi OS. Baseline +2 -> `grew` am 2 -> che dau
   dung 2 luong ro. Sua: `Thread.Sleep(500); proc.Refresh();` truoc khi lay baseline.
2. **Ro "cham thoat" khong bi bat.** `GopetSocket.Dispose` (`GopetSocket.cs:176-177`)
   **bo qua ket qua tra ve cua `Join`**. Luong khong join kip 1s van thoat sau do (bi
   `_outgoing.Dispose()` o `:179` danh thuc bang ObjectDisposedException) — trong 1000ms ngu
   o `:118` la bien mat het. Day chinh xac la loai bug ma E1 dinh bat.
3. **Nhieu runtime hai chieu.** Threadpool/tiered-JIT/GC co the tang HOAC giam so luong;
   `grew` am cung pass. Nguong `<= 2` la phong doan, khong co co so.

Sua chac chan hon (deterministic, khong nhieu, 3 dong vao prod code, khong dung toi day):
```csharp
// GopetSocket.cs
internal static int LiveWorkerThreads;   // chi de kiem chung
private void ReadLoop()
{
    Interlocked.Increment(ref LiveWorkerThreads);
    try { /* ... */ }
    finally { Interlocked.Decrement(ref LiveWorkerThreads); }
}
// tuong tu WriteLoop
```
E1 khi do: `Check(..., GopetSocket.LiveWorkerThreads == 0, ...)`. Bo hoan toan `Process.Threads`.

Neu khong muon dung prod code: it nhat cho `Dispose` **bao cao join that bai**
(`bool joined = _readThread?.Join(...) ?? true;` -> expose `public bool DisposedCleanly`)
va E1 assert `DisposedCleanly` moi chu ky. Do do cai dung ban quan tam thay vi do gian tiep.

---

## MEDIUM

### M1. Ngoai le trong RunProtocolChecks nuot luon E1/E2
`Program.cs:37-45`, `:86-87`

`RunProtocolChecks` va `CheckNoLeakOnDisconnect` nam trong CUNG mot `try`. `ReadBool()` (`:86`)
va `ExpectFullyConsumed` (`:87`) deu nem `ProtocolException` khi server tra khac du kien
-> nhay thang ra catch -> **E1/E2 khong bao gio chay**, output khong noi la da bo qua.
Mot loi giao thuc nho lam mat luon ket qua leak check.

Sua: tach `try` cho tung pha, va them overload bat ngoai le tai cho:
```csharp
private static void Check(string name, Func<bool> probe, string reason)
{
    try { Check(name, probe(), reason); }
    catch (Exception ex) { Fail(name, $"nem ngoai le: {ex.Message}"); }
}
```

### M2. `WaitForMessage` doi du 10s ngay ca khi ket noi da dut
`Program.cs:132-142`

Khong co race ve du lieu (`ConcurrentQueue.TryDequeue` an toan), nhung vong lap khong nhin
`socket.IsConnected`. Server dong ngay sau CLIENT_INFO (vd. languageCode sai ->
`Player.cs:113-117` `setClientOK(false); session.Close()`) van phai cho het 10s roi bao sai
nguyen nhan ("het 10s khong co phan hoi" thay vi ly do that trong `dropReason`).

Sua:
```csharp
while (sw.Elapsed < timeout)
{
    if (socket.Incoming.TryDequeue(out var msg)) return msg;   // dequeue TRUOC
    if (!socket.IsConnected) return null;                      // roi moi check ket noi
    Thread.Sleep(20);
}
```
Thu tu quan trong: dequeue truoc khi kiem tra `IsConnected`, neu khong se bo mat goi da nam
trong queue tai thoi diem ket noi dut — day moi la race that su o cho nay.
Doi `DateTime.UtcNow` -> `Stopwatch` (mien nhiem voi NTP step).

### M3. `dropReason` doc cheo luong khong co rao can bo nho
`Program.cs:65-66,73`

`dropReason` la bien cuc bo bi capture -> field khong `volatile` cua closure class, ghi tu
luong nen (`GopetSocket.Fail`), doc tu luong chinh. Tren x64 + `Sleep(500)` thuc te luon thay,
nhung ve mo hinh bo nho C# thi doc co the cu. Sua: dung `Volatile.Write/Read`, hoac field
static `private static volatile string _dropReason;`.

### M4. `GopetSocket.Dispose` khong idempotent — goi 2 lan se nem
`Assets/Scripts/Net/GopetSocket.cs:168-180`

Lan 2: `_outgoing.CompleteAdding()` (`:171`) tren `BlockingCollection` da dispose ->
`ObjectDisposedException` thoat ra ngoai Dispose. `IDisposable` phai chiu duoc goi nhieu lan.
Chua no trong harness (dung `using`) nhung se no khi phia Unity goi Dispose ca trong
`OnDestroy` lan `OnApplicationQuit`.

Sua: them `private int _disposed;` + `if (Interlocked.Exchange(ref _disposed, 1) == 1) return;`.

### M5. `Fail` la check-then-act, `Disconnected` co the ban 2 lan
`Assets/Scripts/Net/GopetSocket.cs:159-162`

Read thread va write thread cung co the vao `Fail` -> ca hai qua duoc `if (!_connected)`
truoc khi ai kip set false -> `Disconnected` invoke 2 lan voi 2 ly do khac nhau. Harness chi
gan bien nen vo hai, nhung phia UI se hien 2 dialog / 2 lan reconnect.

Sua: doi `_connected` thanh `int` + `if (Interlocked.Exchange(ref _connected, 0) == 0) return;`.

### M6. LiveSmoke khong nam trong solution, khong nam trong verify -> se muc
`GopetUnityClient.slnx` chi liet ke `Gopet.Net.csproj` + `Gopet.Runtime.csproj`.
`verify.ps1` build `Gopet.Net.UnityCompat` va test `Gopet.Net.Tests`, khong dung toi LiveSmoke.
Nghia la: doi ten mot API trong `Assets/Scripts/Net` lam LiveSmoke **khong compile** ma
khong cong cu nao bao. Lan sau can no thi no da hong.

Sua: them buoc **build-only** (khong chay, khong can server) vao verify.ps1:
```powershell
Step "5/5  LiveSmoke con compile duoc (khong chay)" {
    Push-Location "$root\tests\Gopet.Net.LiveSmoke"
    try {
        dotnet build --nologo -v quiet
        if ($LASTEXITCODE -ne 0) { throw "LiveSmoke khong compile" }
    } finally { Pop-Location }
}
```

### M7. Comment cua `RoundTrip_DuMoiKieuDuLieu` khai sai pham vi
`tests/Gopet.Net.Tests/MessageAndRouterTests.cs:97-99,112`

Comment: "moi kieu phai qua duoc ca duong ma hoa **lan dong khung**". Nhung test chi goi
`tea.Decrypt(tea.Encrypt(...))` — **`PacketFramer` khong he duoc goi**. Dong khung khong duoc
phu o day (co o PacketFramerTests, nhung khong phai voi "du moi kieu").

Sua: hoac bo cum "lan dong khung" khoi comment, hoac cho chay that qua framer:
```csharp
using var ms = new MemoryStream();
PacketFramer.WriteFrame(ms, tea.Encrypt(outgoing.ToWire()), true);
ms.Position = 0;
Assert.True(PacketFramer.TryReadFrame(ms, out var raw, out var enc));
var incoming = Message.FromWire(tea.Decrypt(raw), enc);
```
Cach nay moi dung voi ten test va moi bat duoc bug o cho noi framer <-> tea.

---

## LOW

### L1. Hai file test vuot rule 200 dong, va verify khong quet thu muc tests
`WireFormatTests.cs` = 228 dong, `MessageAndRouterTests.cs` = 207 dong.
`verify.ps1:63` chi `Get-ChildItem "$root\Assets\Scripts"` nen khong bat.
Sua: tach `WireFormatTests.cs` -> primitives (byte/short/int/long/bool) + handshake-vector;
va mo rong pham vi quet cua verify sang `tests\**\*.cs`. Neu chap nhan test file duoc mien
thi ghi ro ngoai le trong README de khoi tranh cai sau.

### L2. `Process.GetCurrentProcess()` khong dispose
`Program.cs:103`. `Process` la `IDisposable`; moi lan truy cap `proc.Threads` con cap phat
`ProcessThreadCollection` moi. Ro handle nho ngay trong bai test do ro.
Sua: `using var proc = ...` (hoac bo han neu ap dung H5).

### L3. `Thread.Sleep(500)` cho check A la gia dinh ngam ve RTT
`Program.cs:71`. Voi `GOPET_HOST` tro ra may khac, RST tu server co the ve sau 500ms ->
A PASS gia roi B FAIL, ly do bao sai. Sua: poll `!socket.IsConnected || dropReason != null`
trong 500ms thay vi ngu cung; hoac ghi ro A chi co y nghia voi server local.

### L4. `ClientInfoPacket` public/private khong nhat quan
`ClientInfoPacket.cs:15,18,25,27`. `Version`/`LanguageCode` private, `DisplayWidth`/`DisplayHeight`
public const nhung khong ai ngoai file dung -> YAGNI. Cho het ve private (hoac het ve public
neu dung cho golden hex o H1).

### L5. Body dau tien cua round-trip trung voi opcode
`MessageAndRouterTests.cs:102-103,114-115`: opcode = -36 va `PutSByte(-36)`. Neu `ToWire`
ghi opcode 2 lan thi 2 assert dau van xanh -> thong diep loi mo. Doi body sang gia tri khac
(vd. `PutSByte(-1)`).

### L6. Message trong hang doi `_outgoing` bi bo im lang khi Dispose
`GopetSocket.cs:170-171`. Goi da `Send` nhung chua kip ghi bi vut, khong loi canh bao.
`Message.Dispose` chi dong `MemoryStream` (khong co unmanaged) nen khong ro tai nguyen that,
nhung "gui roi ma khong bao gio len day" nen duoc ghi vao docstring cua `Send`.

### L7. `PacketLogger` cat hex o 256 byte, con cot `len` la do dai day du
`PacketLogger.cs:38,52-58`. Voi CLIENT_INFO thi vo hai, nhung neu golden-hex (H1) mo rong sang
goi lon hon, hai dump co the "khop" o phan da bi cat. Nen cho packet-diff fail som khi gap
dong co hau to `...`.

---

## Test moi: co that su fail duoc khong?

| Test | Do duoc gi | Ket luan |
|---|---|---|
| `Short_RoundTrip` | Chi doi xung writer<->reader. Dao endian ca hai ben van XANH. | Yeu nhung vo hai (co `Short_LaBigEndian` gac) |
| `Short_LaBigEndian` | Byte order that | Do duoc |
| `Short_CatDungPhanThapKhiTran` | `0x1FFFF -> (short)-1 -> ffff` | Do duoc |
| `Bool_RoundTrip` | Doi xung | Yeu, trung pham vi voi `Bool_GhiDungMotByte01` (DRY) |
| `Bool_GhiDungMotByte01` | `01`/`00` | Do duoc |
| `Bool_MoiGiaTriKhac0DeuLaTrue` | Chan `== 1` thay vi `!= 0` | Do duoc |
| `RoundTrip_DuMoiKieuDuLieu` | Tea + writer/reader + cho noi giua cac field | Do duoc, nhung KHONG phu framer (M7) |
| Live A | IsConnected sau 500ms | Do duoc (yeu voi host xa, L3) |
| Live B | opcode reply | Do duoc |
| Live C | setClientOK | Do duoc |
| Live D | `File.Exists` | **Khong bao gio do** (H1) |
| Live E1 | `Process.Threads.Count` | Do duoc voi ro vinh vien; bo sot ro cham-thoat + baseline lech (H5) |
| Live E2 | TCP Established | Bo sot CloseWait (H2), co the vacuous (H3), co the soi nham socket (H4) |

## Diem tot

- Thu tu `using` o `Program.cs:62-63` dung: socket dispose TRUOC logger, nen khong co
  `Log()` tren `StreamWriter` da dong tu luong nen. Chi tiet nay rat de sai.
- Fact-check ClientInfoPacket vs `Player.cs` khop tuyet doi, ke ca `Refcode` doc sau
  `_languageCode` (thu tu de dao).
- `ExpectFullyConsumed` duoc goi trong live check -> bat duoc ca "thua byte", khong chi "sai gia tri".
- Harness da tu chung minh kha nang fail (cong chet / version sai / dao endian) — dung tinh than.
- `PacketFramer.ReadExactly` xu ly short-read dung, kem comment giai thich vi sao.
- `JavaBinaryReader.ReadIntArray(maxCount)` chan do dai tu server — khong lap lai lo hong
  `GameController.cs:215` phia server.

## Hanh dong de xuat (theo thu tu)

1. H1 — bien Check D thanh assert golden hex (re nhat, gia tri cao nhat).
2. H2 + H4 — them CloseWait; lay local port tu `socket.LocalEndPoint` thay vi doan tu bang TCP.
3. H5 — thay `Process.Threads.Count` bang counter `Interlocked` trong Read/WriteLoop.
4. H3 — chan `localPorts == 0`.
5. M1 — tach try, them `Check(name, Func<bool>, reason)`.
6. M2 — WaitForMessage thoat som khi mat ket noi (dequeue truoc, check sau).
7. M4/M5 — Dispose idempotent, Fail dung Interlocked.
8. M6 — them buoc build-only LiveSmoke vao verify.ps1.
9. M7 + L1 — sua/mo rong test round-trip qua framer; tach 2 file test qua 200 dong.

## Cau hoi con mo

- Phia Unity (`Runtime/GopetClient.cs`) goi `GopetSocket.Dispose` o dau? Neu ca `OnDestroy`
  lan `OnApplicationQuit` thi M4 la bug that, khong con la ly thuyet.
- Co y dinh dua LiveSmoke vao CI (co container GServer) khong? Neu co thi H1 tro thanh
  chan cua CI, uu tien cao hon nua.
- `MaxHexBytes = 256` co du cho goi lon nhat ma packet-diff can so khong (goi anh/map)?
