# Phân tích source game goPet (SRCGOPETGOC)

Ngày: 2026-09-05 | Phạm vi: `D:\game\SRCGOPETGOC`

## 1. Tổng quan

| Thành phần | Nội dung |
|---|---|
| `GServer/` | Game server C# .NET 8, ~205 file `.cs`, ~35.000 LOC |
| `client.jar` | Client J2ME (MIDP-2.1 / CLDC-1.1), 221 class **đã obfuscate** — không có source |
| `MariaDB_SQL/` | 3 dump: `server_db` (50 bảng), `web_db` (18 bảng), `log_db` |
| `GServer/assets/` | 141 MB tài nguyên (map, pet, item, skill, effect...) |

**Bản chất:** đây là bản **port thủ công từ Java sang C#**. Dấu vết rõ: `CopyOnWriteArrayList`, `HashMap`, `JArrayList`, `printStackTrace()`, `gI()`, `sbyte[]` thay cho `byte[]`, `DataInputStream`/`DataOutputStream`, toán tử `>>>`. Chỉ có server là source thật; client là binary.

## 2. Kiến trúc

```
Program.cs -> App/Main.StartServer()
   |-- GopetManager.init()      nạp template (item, pet, skill, mob, shop, task) từ MySQL
   |-- MapManager / ClanManager / FieldManager / BXHManager (bảng xếp hạng)
   |-- RuntimeServer            AutoSave . DBBackup . Maintenance
   |-- EventManager / ScheduleManager / PlayerManager
   |-- HttpServer :8082         ASP.NET Core + Swagger (API quản trị)
   +-- MServer.Server :19180    TCP game socket
```

**Luồng mạng:** `Server.Runner` (accept, throttle 2s/IP) -> `Session` -> mỗi kết nối tạo **2 thread riêng** (`MsgSender`, `MsgReader`) -> `Player.onMessage` (login/register) -> `GameController.onMessage` (gameplay) -> `MenuController` (NPC/dialog/shop).

**Giao thức:** `[int length][sbyte encrypted][payload]`, mã hoá **TEA 128-bit**, key 8 byte do **client gửi lên** trong 9 byte handshake đầu tiên. Opcode 1 byte (`GopetCMD`, 177 hằng số).

**Lưu trữ:** MySQL + Dapper. Toàn bộ trạng thái người chơi (`items`, `pets`, `letters`, `achievements`...) serialize thành **JSON nhét vào cột `longtext`** của bảng `player` — một câu `UPDATE` khổng lồ ~45 cột.

## 3. Vấn đề bảo mật

### 3.1 HTTP API không có xác thực — NGHIÊM TRỌNG

`APIs/HttpServer.cs:68` bind `http://0.0.0.0:8082`, Swagger UI mở ở route gốc. `ServerController.cs` có ~25 endpoint, **không endpoint nào kiểm tra quyền**:

- `GET /api/server/shutdown` — tắt server
- `GET /api/BuffItem/{name}/{itemId}/...` — tạo item bất kỳ cho nhân vật bất kỳ
- `GET /api/maintenance/{min}`, `/api/maintenance/reboot`
- `POST /api/SendMail/{to}/{subject}/{body}` — dùng SMTP server làm open relay
- `GET /api/server/opensqlweb`, `opensqlgame`

Trường `apiKey` có trong `config/server.json` và `ServerSetting.cs` nhưng **không được đọc ở bất kỳ đâu** (`grep apiKey` chỉ ra 3 hit, đều trong chính file setting). Cơ chế auth đã thiết kế nhưng chưa nối vào.

### 3.2 Position hoàn toàn do client quyết định

`GameController.cs:222-224` — toạ độ lấy thẳng từ packet, không validate. Toàn bộ khối chống hack-move ở dòng 227-241 **đã bị comment**.

### 3.3 Cấp phát mảng theo độ dài client gửi

`GameController.cs:215` `new int[message.reader().readInt()]` và `:784` `new String[message.readInt()]`. Client gửi `readInt()` cực lớn -> `OutOfMemoryException`. Ngoài ra `points[points.Length - 2]` panic khi mảng < 2 phần tử. **DoS từ xa bằng 1 packet.**

### 3.4 Ban speed-hack đã bị tắt

`Player.cs:320` — lệnh `user.ban(...)` và `session.Close()` bị comment, chỉ còn ghi log.

### 3.5 Dump SQL chứa dữ liệu thật

`web_db.sql` có bản ghi user thật kèm email, hash mật khẩu, IP. **Không được commit lên git.** Hash trộn 2 định dạng: bcrypt (`$2a$12$...`) cho `admin`, còn `tester`/`admi11`/`adm111` là SHA-256 trần 64 hex — code hiện chỉ verify bcrypt nên các tài khoản cũ này không đăng nhập được, nhưng hash yếu vẫn nằm trong DB.

### 3.6 Bảng `user` dùng MyISAM

Là bảng duy nhất không phải InnoDB (`web_db.sql`). Không transaction, khoá cấp bảng -> nghẽn khi login đồng thời, và `GET_LOCK` trong `Player.login()` phải bù cho việc thiếu row-lock.

### 3.7 Rò rỉ credential trong repo

`App.config` và `config/database.json` chứa host thật (`160.30.160.83:19999`), user `root`. `App.config` để `PASSWORD=` rỗng.

### 3.8 SQL injection (mức thấp)

`MenuController.sendMenu.cs:221` nối chuỗi `IN ({ints.ToArray().Join(",")})`. Dữ liệu là `int[]` từ friend-list nội bộ nên chưa khai thác được, nhưng là pattern nguy hiểm. 99% query còn lại dùng parameter đúng cách.

### 3.9 Kiểm tra admin thiếu lớp cuối

`GameController.cs:1190,1193` dispatch `ADMIN_GET_ITEM`/`ADMIN_GIVE_ITEM` không check quyền; `MenuController.selectMenu.cs:2261,2322` xử lý cũng không check. Quyền chỉ được kiểm ở bước set state (`checkIsAdmin()` tại `selectMenu.cs:1005,1029`). **Chưa khai thác trực tiếp được** vì cần state `objectPerformed` mà chỉ admin mới set, nhưng thiếu defense-in-depth.

## 4. Lỗi logic (kinh tế trong game)

### 4.1 Thiếu `return` — mua item của chính mình

`Data/map/Kiosk.cs:172-175`:

```csharp
if (sellItem.user_id == player.user.user_id)
{
    player.redDialog(player.Language.CannotBuyThisItemOfYourself);
}   // <- THIẾU return, code chạy tiếp xuống mua thật
```

Trong `buy()` (dòng 134) có `else` nên đúng; chỉ `buyRetail()` bị lỗi. Người chơi tự mua lẻ đồ của mình -> chỉ mất phí `KIOSK_PER_SELL`, nhưng kết hợp với 4.3 thì thành lỗ hổng nhân tiền.

### 4.2 TOCTOU trên tiền tệ

`Kiosk.cs:245` `player.checkCoin(...)` chạy **ngoài** mutex; `:252` `player.addCoin(-price)` chạy **trong** mutex, không check lại. `Player.cs:746-749` `addCoin` là `playerData.coin += coin` — không atomic, không lock, không chặn âm. Hai thread mua song song -> số dư âm / nhân item.

### 4.3 Kế toán sai ở luồng mua lẻ

- `buyRetail` cộng `sellItem.sumVal += price` nhưng **không trả tiền cho người bán ngay**.
- `confirmBuy` (`:264`) tính `priceReiceived` từ **`sellItem.price` đầy đủ**, trong khi người mua cuối chỉ trả `price - sumVal` (`:252`).

-> Người bán nhận đủ giá gốc dù đã thu tiền phần bán lẻ trước đó. Không cân bằng dòng tiền.

- `buyRetail` cũng **không kiểm tra `sellItem.hasSell`** (trong khi `confirmBuy` có).

### 4.4 `ReleaseMutex()` trên mutex chưa sở hữu

`Kiosk.cs:191/218` và `:249/291`: `WaitOne()` nằm **bên trong** `try`, `ReleaseMutex()` ở `finally`. Nếu `WaitOne()` ném (`AbandonedMutexException`), `finally` gọi `ReleaseMutex()` -> `ApplicationException`, nuốt mất lỗi gốc. Pattern đúng là `WaitOne()` đặt trước `try`.

## 5. Hiệu năng

### 5.1 `Mutex` thay vì `lock` — tác động lớn nhất

`Data/Collections/CopyOnWriteArrayList.cs:15` dùng `System.Threading.Mutex` — **kernel object**, mỗi lần acquire là một syscall, chậm hơn `Monitor`/`lock` khoảng 50-100 lần. Class này là backbone: `PlayerManager.players`, `pets`, `items`, `kioskItems`, `THREADS`... Cùng pattern ở `PetBattle.cs:31`, `Clan.cs:31`, `SellItem`.

-> Đổi sang `lock` (hoặc bỏ hẳn vì `ImmutableList` + `Interlocked.Exchange` là đủ) là tối ưu rẻ nhất, hiệu quả nhất.

### 5.2 `GC.Collect()` mỗi lần disconnect

`Session.cs:142`. Full blocking GC trên server GC heap. 50 người disconnect cùng lúc = 50 lần stop-the-world.

### 5.3 Hai thread OS cho mỗi kết nối

`Session.run()` tạo `sendThread` + `readThread`. 1.000 người online = 2.000 thread, khoảng 2 GB stack reserve. Đã có `SocketAsyncEventArgs _event` khai báo ở `Server.cs:20` nhưng **không dùng** — có vẻ đã định chuyển sang async I/O rồi bỏ dở.

### 5.4 Save toàn bộ player 10 phút/lần

`AutoSave.cs` lặp tuần tự qua mọi player trên **một** connection, mỗi player một `UPDATE` 45 cột chứa JSON lớn. Vừa gây spike I/O định kỳ, vừa mất tối đa 10 phút dữ liệu khi crash.

### 5.5 `HashMap<K,V> : Dictionary<K,V>` không thread-safe

`Data/Collections/HashMap.cs` chỉ là alias của `Dictionary`. Nhưng `PlayerData.items` là `HashMap<sbyte, CopyOnWriteArrayList<Item>>`, bị đọc bởi thread AutoSave trong khi thread game ghi -> `Dictionary` có thể hỏng cấu trúc nội bộ (vòng lặp vô hạn / mất phần tử). 45 chỗ dùng `HashMap<`.

### 5.6 `Session.Close()` không đóng socket ngay

Chỉ `Interrupt()` thread và queue `Exit` vào ThreadPool. Nếu ThreadPool nghẽn, socket + buffer treo.

## 6. Chất lượng code

- **File quá lớn:** `GameController.cs` 5.224 dòng, `MenuController.selectMenu.cs` 2.831 dòng, `PetBattle.cs` 1.705 dòng. Vi phạm nặng quy tắc 200 dòng/file của dự án. `MenuController` đã tách partial (5 file) nhưng vẫn quá to.
- **`GameController.onMessage`** là switch lồng nhau nhiều tầng (`processPet` một mình có ~60 case) — không thể test đơn vị.
- **Nuốt lỗi:** 63 chỗ `printStackTrace()`, 74 `catch (Exception)`. Lỗi in ra console rồi đi tiếp — logic hỏng vẫn chạy tiếp với state sai.
- **`objectPerformed` là `HashMap<int, dynamic>`** — state machine hội thoại NPC không kiểu, không thread-safe, là gốc của vấn đề 3.9.
- **Code chết:** `Main.StartServer()` chứa hàm cục bộ `ScanData()` ~130 dòng với 9 hàm lồng nhau (script sửa dữ liệu one-off), lời gọi ở dòng cuối đã comment. `AutoMaintenance` bị comment. `Server.cs:20` `_event` không dùng.
- **Logic hết hạn hardcode:** `Player.cs:757` `CanAddSpendGold` return `DateTime.Now <= new DateTime(2025, 2, 1, ...)` — đã hết hạn, tính năng đang tắt vĩnh viễn.
- **Nghiệp vụ theo năm nằm trong code:** `Data/Event/Year2024/`, `Year2025/` — mỗi sự kiện là một class mới thay vì cấu hình dữ liệu.
- **Không có test** — không tìm thấy project test nào.
- Trộn tiếng Việt/Anh trong tên định danh và comment; đặt tên không nhất quán (`getPlayer()` kiểu Java cạnh `PlayerData` property C#).

## 7. Ưu tiên xử lý

| # | Việc | Mức | Chi phí |
|---|---|---|---|
| 1 | Bind HTTP API vào `127.0.0.1` + bật kiểm tra `apiKey` | Nghiêm trọng | Thấp |
| 2 | Giới hạn độ dài mảng đọc từ packet (`GameController.cs:215,784`) | Nghiêm trọng | Thấp |
| 3 | Gỡ credential khỏi `App.config`/`database.json`, dùng biến môi trường | Nghiêm trọng | Thấp |
| 4 | Thêm `return` thiếu ở `Kiosk.cs:175` | Cao | Rất thấp |
| 5 | Bọc check-then-act tiền tệ vào một lock chung; `addCoin/addGold` chặn âm | Cao | Trung bình |
| 6 | `Mutex` -> `lock` trong `CopyOnWriteArrayList`, `PetBattle`, `Clan`, `SellItem` | Cao | Thấp |
| 7 | Bỏ `GC.Collect()` ở `Session.cs:142` | Cao | Rất thấp |
| 8 | `HashMap` -> `ConcurrentDictionary` cho `PlayerData.items` | Cao | Trung bình |
| 9 | Validate server-side toạ độ di chuyển | Trung bình | Trung bình |
| 10 | Đổi bảng `user` sang InnoDB | Trung bình | Thấp |
| 11 | Tách `GameController`/`MenuController` theo domain (battle, inventory, clan, shop) | Trung bình | Cao |
| 12 | Xoá `ScanData()` và code chết trong `Main.cs` | Thấp | Rất thấp |
| 13 | Chuyển sang async socket I/O (`SocketAsyncEventArgs` đã khai báo sẵn) | Thấp | Cao |

## 8. Câu hỏi còn treo

1. Server này dùng để vận hành thật hay chỉ nghiên cứu? Nếu vận hành thật thì mục 1-3 phải sửa **trước khi mở port**.
2. `client.jar` obfuscate — có source client ở đâu khác không? Nếu không, mọi thay đổi giao thức đều bị chặn (không sửa được client).
3. Dump SQL có dữ liệu người dùng thật (email, IP) — đã có sự đồng ý xử lý dữ liệu chưa?
4. `160.30.160.83:19999` trong `database.json` là server production đang chạy hay đã bỏ?
5. Ý định về `SocketAsyncEventArgs` chưa dùng — có kế hoạch chuyển async không, hay bỏ luôn?
