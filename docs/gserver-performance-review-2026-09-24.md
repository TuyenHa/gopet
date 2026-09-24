# Báo cáo rà soát performance và code dư thừa — GServer

Ngày kiểm tra: 24/09/2026. Mã nguồn tham chiếu: commit `3f4a816`, thư mục `SRCGOPETGOC/GServer`, target `net8.0`.

Theo dõi triển khai và các điều kiện kiểm chứng còn lại tại [bản ghi triển khai](gserver-performance-implementation-2026-09-24.md). Các bằng chứng/dòng nguồn bên dưới mô tả trạng thái trước sửa.

## 1. Kết luận

Có các vấn đề đáng sửa ở tầng mạng, vòng đời session và ghi lịch sử. Đáng chú ý nhất là gửi dữ liệu từng byte, vòng đọc có thể chạy liên tục khi gặp EOF, giữ tham chiếu đến session đã đóng, và gọi `GC.Collect()` mỗi lần đóng kết nối. Ngoài ra có code không được gọi, đối tượng được tạo rồi ghi đè ngay, truy vấn/kết nối thừa và thao tác sao chép không cần thiết.

Đây là **review mã nguồn kèm hai kiểm chứng cô lập**, chưa phải kết quả profiling server đang phục vụ người chơi. Báo cáo xác nhận cơ chế lỗi/chi phí trong code; chưa kết luận server hiện đang lag bao nhiêu, chịu được bao nhiêu CCU hoặc tối ưu sẽ giảm bao nhiêu phần trăm CPU.

Phạm vi: quét 213 file C# của server, loại trừ `packages`, `bin`, `obj`; đọc sâu các đường mạng, đóng phiên, map/battle update, lưu dữ liệu, lịch sử, collection và bảng xếp hạng. Không khởi động server, không chạy truy vấn lên DB, không load test và không sửa mã nguồn vận hành. Các đường dẫn bên dưới tính từ `SRCGOPETGOC/GServer`; số dòng tại thời điểm review.

Mức ưu tiên:

- **P1 — cao:** cơ chế có thể gây chiếm CPU, tăng bộ nhớ kéo dài hoặc tác động rộng; nên xử lý trước khi tăng tải.
- **P2 — vừa:** chi phí hoặc lỗi có điều kiện; cần đo quy mô ảnh hưởng khi triển khai sửa.
- **P3 — thấp:** dọn code, giảm chi phí nhỏ hoặc giảm rủi ro bảo trì.

## 2. Các điểm performance

| ID | Ưu tiên | Phát hiện | Vị trí chính |
| --- | --- | --- | --- |
| P01 | P1 | Gửi payload bằng từng lần ghi đơn byte | `Server/IO/IOExtension.cs:57` |
| P02 | P1 | Vòng đọc không thoát khi EOF trả về 0 | `Server/IO/MsgReader.cs:71` |
| P03 | P1 | Danh sách session giữ các phiên đã đóng | `Server/Server.cs:95` |
| P04 | P1 | Ép GC mỗi lần đóng phiên; đóng phiên chưa có chốt chạy một lần | `Server/IO/Session.cs:139` |
| P05 | P1 | Hàng đợi gửi không giới hạn và sender có thể thoát âm thầm | `Server/IO/MsgSender.cs:11` |
| P06 | P1 | Một lỗi có thể làm worker lịch sử dừng vĩnh viễn | `Manager/HistoryManager.cs:44` |
| P07 | P2 | Hai thread/session; handshake đồng bộ chưa có deadline | `Server/IO/Session.cs:47` |
| P08 | P2 | Binary search trên enumerable vẫn phải duyệt nhiều phần tử | `Util/Utilities.cs:346` |
| P09 | P2 | Tạo lại nhiều buffer cho từng người nhận broadcast | `Server/IO/Message.cs:32` |
| P10 | P2 | Autosave dồn đợt; backup dùng chung worker với các tác vụ runtime | `Runtime/AutoSave.cs:13` |
| P11 | P2 | Lịch sử mở connection không dùng và insert từng dòng | `Manager/HistoryManager.cs:64` |
| P12 | P2 | TOP Pet xóa và dựng lại bảng từ JSON định kỳ | `Data/top/TopPet.cs:27` |
| P13 | P2 | Retry bằng vòng lặp chiếm CPU khi dữ liệu spawn/quà không phù hợp | `Place/GopetPlace.cs:650` |
| P14 | P2 | Logger dùng khóa chung, I/O đồng bộ và có đường gọi lại chính nó khi lỗi | `Logging/Monitor.cs:70` |
| P15 | P2 | Quét zone trống và nhịp map cộng thêm 500 ms sau update | `Data/map/GopetMap.cs:62` |
| P16 | P2 | Deadline dọn mob chết bị dời lại mỗi tick | `Place/GopetPlace.cs:542` |

### P01. Payload được gửi từng byte

**Bằng chứng:** `MsgSender.doSendMessage()` gọi `session.dos.Write(data)` tại `Server/IO/MsgSender.cs:116`, với `data` là `sbyte[]`. Overload extension tại `Server/IO/IOExtension.cs:57–65` lặp qua toàn bộ mảng và gọi `BinaryWriter.Write(buffer[i])`. Header `WriteInt()` ở dòng 68 cũng đi qua overload này. Writer được tạo trực tiếp trên `NetworkStream` tại `Session.cs:53–55`.

**Ảnh hưởng:** mỗi byte tạo một lần gọi ghi xuống stream, tăng đáng kể số lời gọi I/O; đặc biệt tốn với ảnh, danh sách item và broadcast. Không nên hiểu số lần ghi là số TCP packet, vì hệ điều hành có thể gộp packet.

**Kiểm chứng:** dùng chính source `IOExtension.cs` trong một probe với stream đếm số lần ghi. Payload 4.096 byte cộng header/flag cho kết quả **4.101 lần `WriteByte`, 0 lần ghi block**. Đây là phép đếm lời gọi, không phải benchmark throughput mạng.

**Đề xuất:** chuyển payload thành vùng byte và ghi cả block; cân nhắc ghép header + flag + payload thành frame. Giữ nguyên byte order, opcode và mã hóa. Kiểm tra byte-for-byte với client cũ và Unity trước khi thay đường gửi.

### P02. EOF giữa payload có thể làm reader quay vòng liên tục

**Bằng chứng:** `Server/IO/MsgReader.cs:71–78` dùng điều kiện `len != -1`, nhưng chỉ tăng `sbyteRead` khi `len > 0`. Khi `BinaryReader.Read(byte[], ...)` trả 0 do hết stream, không có lệnh thoát hoặc ném lỗi.

**Trigger:** client gửi header với độ dài lớn hơn phần payload thực gửi rồi đóng chiều gửi. Sau khi đọc hết dữ liệu còn lại, reader có thể tiếp tục nhận 0 và lặp mà không tiến triển. Việc một đường khác sau đó đóng/dispose stream có thể chấm dứt vòng lặp, nhưng vòng đọc này không tự xử lý EOF.

**Kiểm chứng:** mô phỏng payload cần 8 byte, stream chỉ có 2 byte; dừng probe chủ động sau 10.000 vòng. Kết quả: `read=2/8`, `len=0`, điều kiện vòng lặp gốc vẫn `True`.

**Đề xuất:** coi `Read() == 0` khi còn thiếu payload là EOF và thoát qua đường đóng phiên; hoặc dùng cơ chế đọc đủ số byte có xử lý EOF. Kiểm tra cả độ dài âm/không hợp lệ trước khi cấp phát. Thêm ca kiểm tra payload bị cắt và nhiều lần đọc từng phần.

### P03. Session đã đóng vẫn bị giữ tham chiếu

**Bằng chứng:** `Server/Server.cs:18` giữ `CopyOnWriteArrayList<Session> sessions`; `setupClient()` luôn `sessions.Add(session)` tại dòng 95. Không tìm thấy đường `Remove/Clear` tương ứng trong source server. `Session.Exit()` đóng stream/socket nhưng không gỡ session khỏi danh sách, không xóa `messageHandler` (`Session.cs:163–190`).

**Ảnh hưởng:** server còn sống thì danh sách có thể giữ toàn bộ session lịch sử, cùng `Player`, controller và dữ liệu mà chúng còn tham chiếu. Bộ nhớ tăng theo tổng số kết nối đã xử lý, không chỉ CCU. Ngay cả `session.run()` gặp lỗi và tự gọi `Close()`, `setupClient()` vẫn có thể thêm session sau khi hàm trả về.

**Đề xuất:** quản lý đăng ký/gỡ session qua một owner rõ ràng; cleanup chạy đúng một lần cho cả handshake thất bại và disconnect bình thường. Nếu danh sách không có nhu cầu sử dụng, cân nhắc bỏ sau khi kiểm tra consumer. Không thể giải quyết tham chiếu còn sống bằng cách ép GC.

**Liên quan:** `Session.socketCount++` tại `Server.cs:96` không có đường giảm và không đồng bộ; API `APIs/ServerController.cs:85–88` vì vậy không phải số socket đang mở đáng tin cậy. Cần xác định đây là counter tích lũy hay gauge hiện tại.

### P04. Đóng phiên ép GC và có thể lặp công việc cleanup

**Bằng chứng:** `Server/IO/Session.cs:150–160` chờ drain tối đa 1 giây, xếp `Exit` vào thread pool rồi gọi `GC.Collect()`. Không có chốt atomic ngăn nhiều lần `Close()`. `MsgReader.run()` có đường gọi `onDisconnected()` trước `Close()` (`MsgReader.cs:38–44`), trong khi `Exit()` lại gọi callback đó. `App/Main.cs:240–244` cũng gọi cả `Close()` và `onDisconnected()`.

**Ảnh hưởng:** reconnect/disconnect liên tục có thể gây nhiều lần thu gom toàn heap và tăng độ trễ ngoài session đang đóng. Cleanup lặp có thể kéo theo save DB/log lặp (`Player.cs:695–711`). Riêng map tick gọi `checkSpeed()` rồi `session.Close()` (`Player.cs:286–296`) còn có thể chờ drain trên chính thread cập nhật map, làm chậm các zone cùng map.

**Đề xuất:** bỏ ép GC trên đường đóng kết nối sau khi xử lý đúng lifetime; thêm trạng thái đóng chuyển một lần bằng thao tác atomic; thống nhất nơi gọi callback disconnect. Tách việc chờ drain khỏi map tick nhưng vẫn bảo đảm gói thông báo cuối được gửi hoặc hết deadline. Không chỉ xóa cơ chế drain hiện có.

### P05. Hàng đợi gửi thiếu giới hạn

**Bằng chứng:** `MsgSender.cs:11,33–36` dùng `ConcurrentQueue<Message>` không giới hạn, enqueue không kiểm tra đóng/drain. Worker gửi đồng bộ; exception bị nuốt ở dòng 69–72 rồi worker kết thúc. `finally` chỉ báo `drained`, không tự đóng session hay gỡ sender khỏi danh sách static `msgSenders`.

**Ảnh hưởng:** client đọc chậm có thể giữ worker ở I/O trong khi broadcast tiếp tục đổ vào queue. Khi sender đã chết nhưng reader/session còn tồn tại, queue cũng có thể tiếp tục tăng. Kết hợp P03 khiến dữ liệu bị giữ lâu hơn.

**Đề xuất:** giới hạn queue theo byte và/hoặc số message, đo độ tuổi message, deadline cho send; đóng client quá chậm hoặc gộp các cập nhật trạng thái có thể thay thế nhau. Chặn enqueue khi đóng. Lỗi sender cần chuyển session sang cleanup. Không bỏ tùy ý các packet giao dịch hay thay đổi tài sản.

### P06. Worker lịch sử chết sau một exception

**Bằng chứng:** `Manager/HistoryManager.cs:46–86` đặt `try/catch` bên ngoài `while (true)`. Lỗi mở connection, serialize hoặc insert sẽ thoát toàn bộ hàm `run()`. Trong khi đó `add()` ở dòng 32–35 vẫn đẩy dữ liệu vào queue không giới hạn; không có cơ chế khởi động lại worker. Phần tử được dequeue trước `INSERT` cũng không có cơ chế đưa lại vào queue khi ghi thất bại.

**Ảnh hưởng:** sau một lỗi tạm thời, lịch sử có thể ngừng ghi và RAM tiếp tục tăng. `History` giữ cả `Player`/`obj` (`Data/User/History.cs:10–22`), nên backlog có thể giữ dữ liệu người chơi đáng kể.

**Đề xuất:** xử lý lỗi theo batch với retry có khoảng chờ, theo dõi heartbeat/backlog và có chính sách lưu bền hoặc giới hạn dung lượng. Thiết kế retry tránh ghi trùng; không âm thầm bỏ lịch sử giao dịch quan trọng. Xử lý cả shutdown/drain.

### P07. Thread/session và handshake không có deadline rõ ràng

**Bằng chứng:** `Server/IO/Session.cs:59–68` tạo hai `Thread` cho mỗi session. Trước đó, `Server.cs:81` chạy `setupClient` trên thread pool; `Session.readKey()` gọi `dis.Read(keys, 0, 9)` đồng bộ tại dòng 80. Không thấy thiết lập timeout nhận/gửi hoặc deadline handshake trong đường tạo kết nối đã rà soát.

**Ảnh hưởng:** N session tạo khoảng 2N thread mạng riêng; client kết nối rồi không gửi key có thể giữ worker setup chờ. Rate limit 2 giây/IP tại `Server.cs:74–79` có tồn tại nhưng không giới hạn số handshake đang chờ trên toàn server. `Read()` một lần cũng chưa bảo đảm nhận đủ 9 byte.

**Đề xuất:** thêm giới hạn handshake đang chờ và deadline, đọc đủ key. Nếu mục tiêu CCU lớn, đo chi phí thread/context switch rồi chuyển sang I/O bất đồng bộ với một luồng xử lý tuần tự theo session. Đây là thay đổi kiến trúc cần kiểm thử thứ tự message, không chỉ thay `new Thread` bằng `Task.Run`.

### P08. Binary search không có truy cập phần tử O(1)

**Bằng chứng:** `Util/Utilities.cs:346–360` nhận `IEnumerable<IBinaryObject<T>>`, gọi `Count()` và `ElementAt(mid)`. Collection thực tế `CopyOnWriteArrayList<T>` chỉ triển khai `IEnumerable<T>` (`Data/Collections/CopyOnWriteArrayList.cs:10`), dù có property/indexer riêng. Đường gọi gồm chọn item/pet (`Server/GameController.cs:2830,2849`), thư (`PlayerData.cs:352`) và tattoo (`Data/pet/Pet.cs:466`).

**Ảnh hưởng:** các lời gọi LINQ không sử dụng trực tiếp indexer tự định nghĩa của wrapper; `ElementAt` phải duyệt enumerable. Thuật toán có thể tốn O(n log n) trong trường hợp xấu thay vì kỳ vọng O(log n). Các đường thêm item/pet/thư còn quét ID và sort lại collection (`PlayerData.cs:303–347`).

**Đề xuất:** tra cứu ID bằng dictionary được cập nhật nhất quán cùng danh sách, hoặc tìm trên snapshot có indexer phù hợp. Nếu dùng indexer của `ImmutableList`, lưu ý truy cập theo index của cấu trúc cây vẫn có chi phí; không mặc định tương đương array. Đo với kích thước inventory thực tế và kiểm tra tính duy nhất của ID.

### P09. Broadcast serialize/copy lại cho mỗi người nhận

**Bằng chứng:** `Message.getBuffer()` ở `Server/IO/Message.cs:40–55` gọi `MemoryStream.ToArray()`, chuyển sang `sbyte[]` rồi cấp phát thêm buffer chứa opcode. Mỗi sender lại gọi hàm này (`MsgSender.cs:100`), kể cả khi `Place.sendMessage()` gửi chung một `Message` cho nhiều người (`Place/Place.cs:58–63`).

**Ảnh hưởng:** tối thiểu ba mảng mới trên đường `getBuffer()` có body; allocation/copy tăng theo kích thước payload nhân số người nhận. Chưa tính các buffer mã hóa.

**Đề xuất:** hoàn thiện message thành payload bất biến một lần rồi chia sẻ khi broadcast; mã hóa riêng theo session khi cần. Cần chốt rõ thời điểm không được sửa message nữa và bảo đảm hàm mã hóa không sửa buffer dùng chung. Xử lý P01 trước để giảm chi phí I/O rõ nhất.

### P10. Autosave và backup tạo tải dồn, chia sẻ một runtime worker

**Bằng chứng:** `Runtime/AutoSave.cs:15–36` cứ 10 phút quét toàn bộ người chơi, save tuần tự người đủ điều kiện và ghi thêm history chứa `playerData`. `Player.TIME_SAVE_DATA` là 15 phút (`Player.cs:41`), nên điều kiện đến hạn chỉ được kiểm tra theo nhịp quét 10 phút. `PlayerData.saveStatic()` cập nhật nhiều cột, gồm collection qua JSON adapter (`Data/User/PlayerData.cs:192–254`). Clan và market cũng lưu theo đợt.

`App/Main.cs:218–222` đăng ký `AutoSave`, `DBBackup`, `Maintenance` vào cùng danh sách; `RuntimeServer.cs:41–52` gọi lần lượt, đồng bộ. `DBBackup.cs:20–29` export game/web DB, rồi hẹn lần sau 300 phút.

**Ảnh hưởng:** CPU serialize và DB write có thể tăng theo đợt; lịch sử backup serialize lại dữ liệu sau lần save. Backup lâu trì hoãn các tác vụ còn lại trên runtime thread. Đây không phải việc backup chạy trực tiếp trên map thread; tác động gameplay cần đo qua cạnh tranh CPU/DB/I/O.

**Đề xuất:** giãn lịch save theo người chơi, batch nhỏ có ngân sách thời gian, theo dõi bản ghi thay đổi. Tách backup khỏi worker lịch vận hành và ngăn các lượt backup chồng nhau. Khi thay đổi persistence cần bảo đảm snapshot nhất quán, không để bản save cũ ghi đè dữ liệu mới.

### P11. Connection và round trip thừa trong ghi lịch sử

**Bằng chứng:** `HistoryManager.cs:64–78` mở cả log connection lẫn game connection mỗi lượt, kể cả queue rỗng. Truyền game connection vào `history.charName(...)`, nhưng `Data/User/History.cs:31–37` không dùng tham số đó: nếu thiếu `Player`, hàm tự mở connection khác và SELECT tên. Mỗi history được ghi bằng một `Execute` riêng.

**Ảnh hưởng:** mượn/trả connection thừa, thêm truy vấn tra tên và round trip trên log DB. Không đồng nhất `Open()` với tạo TCP connection vật lý mới: chi phí thực phụ thuộc pooling/cấu hình chạy.

**Đề xuất:** chờ có dữ liệu trước khi mở connection, tái sử dụng đúng tham số hoặc lấy tên vào bản ghi ngay từ nguồn, ghi theo batch có giới hạn. Ưu tiên phục hồi worker ở P06 trước khi tối ưu throughput.

### P12. Dựng lại TOP Pet định kỳ

**Bằng chứng:** `Data/top/TopPet.cs:29–32` xóa toàn bộ `top_pet`, chạy hai `INSERT ... SELECT` đọc JSON từ `player`, rồi sort lấy 30 kết quả. `Manager/BXHManager.cs:45–51` chạy chu kỳ khoảng 15 phút.

**Ảnh hưởng:** lượng công việc phụ thuộc số bản ghi đủ điều kiện và kích thước JSON, trong khi đầu ra chỉ cần 30 dòng. Có thêm write amplification khi xóa/chèn lại bảng.

**Đề xuất:** đo thời gian thực thi, số dòng đọc/ghi và execution plan trên DB phù hợp; cân nhắc bảng chỉ số pet cập nhật tăng dần hoặc dựng vào bảng staging trước khi đổi dữ liệu công bố. Chưa có bằng chứng để khẳng định thiếu index hay đề nghị một index cụ thể. Đây là tác vụ định kỳ, không phải query chạy mỗi tick.

### P13. Retry chiếm CPU thay vì xử lý tập dữ liệu hợp lệ

**Bằng chứng:** `Place/GopetPlace.cs:650–663` chọn ngẫu nhiên template trong vòng lặp tối đa 3 giây cho mỗi vị trí spawn; nếu không có pet template hợp lệ thì lặp liên tục. `Server/MenuController.cs:657–675` lọc toàn bộ danh sách quà và `ToArray()` lặp đến 20 ms khi chưa chọn được quà.

**Ảnh hưởng:** dữ liệu map sai có thể giữ thread map khoảng 3 giây mỗi vị trí và trì hoãn toàn bộ zone cùng map. Đường đổi quà tạo allocation lặp; tiền đã bị trừ ở dòng 646–649 trước khi chắc chắn chọn được quà.

**Đề xuất:** xác thực template lúc nạp; lọc trước tập spawn hợp lệ và báo lỗi rõ khi rỗng. Đổi quà cần thuật toán chọn có giới hạn công việc và kết quả thất bại rõ ràng. Phải giữ đúng xác suất nghiệp vụ; không thay cơ chế lấy mẫu bằng một thuật toán khác mà chưa đối chiếu phân phối phần thưởng.

### P14. Logger đồng bộ và đường lỗi đệ quy

**Bằng chứng:** `Logging/Monitor.cs:68–93` dùng một `Mutex` static cho mọi logger, ghi console, ghi file và flush trong khóa. Nếu ghi lỗi, catch gọi `e.printStackTrace()`; `Util/Utilities.cs:313–315` lại gọi `ServerMonitor.LogError()`, quay về cùng logger.

**Ảnh hưởng:** log nhiều khiến các thread chờ chung khóa/I/O. Nếu lỗi ghi lặp lại, logger có thể gọi đệ quy đến tràn stack. Mutex ở đây có thể được lấy lại bởi cùng thread, nên không nên mô tả tình huống này đơn giản là tự deadlock.

**Đề xuất:** log nền theo batch với queue hữu hạn, giảm/giới hạn log lặp; đường lỗi fallback phải độc lập, không gọi lại logger đang lỗi.

**Phân biệt:** `Logging/PacketLogger.cs` đã có worker nền, queue tối đa 8.192 dòng, `TryAdd()` và chỉ khởi tạo khi bật cấu hình. Không kết luận packet logger là queue vô hạn hoặc luôn chạy. Khi bật vẫn có chi phí format hex trên thread gọi (`PacketLogger.cs:75–101`), cần đo riêng.

### P15. Chi phí nền của zone và nhịp update

**Bằng chứng:** `Data/map/GopetMap.cs:62–74` tạo 30 zone mặc định cho loại map dùng implementation này; mỗi tick duyệt mọi place (`112–129`). `GopetPlace.update()` vẫn duyệt mob và tạo danh sách respawn kể cả không có người (`Place/GopetPlace.cs:510–584`). `GopetMap.run()` nếu update dưới 500 ms thì ngủ thêm nguyên 500 ms (`87–92`).

**Ảnh hưởng:** có chi phí nền theo số zone/mob, kể cả CCU thấp. Ví dụ update mất 200 ms thì chu kỳ khoảng 700 ms, không phải 500 ms. Nếu update quá 500 ms thì vòng sau bắt đầu ngay; đây là tải liên tục khi quá ngân sách, không tự nó chứng minh vòng lặp rỗng chiếm CPU.

**Đề xuất:** đo số zone hoạt động, thời gian tick và allocation khi rỗng; tạo zone theo nhu cầu hoặc giảm nhịp zone vắng nếu bảo toàn timer boss/event/respawn. Nếu mục tiêu tick là 500 ms, tính thời gian chờ còn lại bằng đồng hồ đo elapsed và có chính sách rõ khi quá hạn.

### P16. Deadline dọn mob chết bị đặt lại liên tục

**Bằng chứng:** `Place/GopetPlace.cs:542–551` khi `hp <= 0`, nếu `TimeEndUpdate` chưa tới thì nhánh `else` lại gán `Now + 5 giây`. Với tick đều dưới 5 giây, deadline luôn bị dời về sau.

**Ảnh hưởng:** mob chết còn trong collection và phụ thuộc nhánh này có thể không được dọn/đưa vào lịch respawn, tiếp tục bị quét mỗi tick. Không khẳng định mọi mob chết đều bị giữ: `PetBattle.cs:854,898` có các đường gọi `place.mobDie(mob)` trực tiếp và `mobDie()` xóa mob khỏi collection.

**Đề xuất:** chỉ khởi tạo deadline khi chưa có; khi đã có thì giữ nguyên và so sánh đến hạn. Kiểm tra trường hợp mob chết ngoài đường kết thúc battle thông thường.

## 3. Code dư thừa và cơ hội dọn dẹp

| ID | Mức độ xác nhận | Vị trí | Nhận xét và hướng xử lý |
| --- | --- | --- | --- |
| C01 | Chắc chắn tạo thừa trên đường setup | `Server/Server.cs:93`; `Server/IO/Session.cs:52` | `setupClient()` tạo `Player` rồi `Session.run()` tạo `Player` khác ghi đè handler. Constructor `Player.cs:66–75` còn tạo controller/task calculator và các field phụ trợ. Chọn một nơi sở hữu việc khởi tạo. |
| C02 | Không được gọi trong luồng hiện tại | `App/Main.cs:61–214` | Khối local function `ScanData` khoảng 150 dòng cùng nhiều hàm scan/fix; lời gọi duy nhất ở cuối đã comment. Nên tách công cụ migration/admin có entry point rõ. Không tính SELECT toàn bảng bên trong khối này là tải startup hiện tại. |
| C03 | `async` không có `await` | `Manager/PlayerManager.cs:130–136` | `sendMessage()` thực chất duyệt và enqueue đồng bộ. Bỏ `async` hoặc thiết kế API trả `Task` nếu thật sự cần bất đồng bộ. Lưu ý bỏ `async void` thay đổi cách exception truyền về caller, cần kiểm tra chỗ gọi; đây không phải tối ưu lớn về CPU. |
| C04 | Field private chỉ thấy khai báo | `Server/Server.cs:21`; `Server/IO/MsgReader.cs:7`; `Manager/PlayerManager.cs:21`; `Logging/Monitor.cs:13` | `_event`, `idleTime`, `WaitLogin` viết hoa, `__LOCK` chưa được dùng trong source rà soát. Có thể dọn sau build kiểm chứng. Không nhầm `WaitLogin` với `waitLogin` viết thường đang dùng cho cooldown. |
| C05 | Chờ có khả năng dư sau khi đã có drain | `Server/GameController.cs:748–755`; `Server/IO/Session.cs:136–160` | Nhánh trùng tên chờ cố định 1 giây rồi `Close()`, còn giữ connection trong `using`. `Close()` hiện có cơ chế drain riêng. Có thể bỏ chờ nếu kiểm tra được dialog cuối vẫn tới client. Các `Sleep(1000)` ở default menu/dialog (`MenuController.selectMenu.cs:2861`, `MenuController.answerYesNo.cs:437`) cũng chặn reader; xác định có chủ đích hạn chế request trước khi thay. |
| C06 | Sao chép thừa ở đường chỉ đọc | `Data/Collections/CopyOnWriteArrayList.cs:188–191`; `Place/GopetPlace.cs:693,749` | `clone()` chuyển immutable list thành array rồi tạo lại immutable list; wrapper mới còn tạo Mutex. `GetEnumerator()` đã duyệt một phiên bản immutable tại thời điểm lấy enumerator. Có thể bỏ clone ở các đường chỉ duyệt đọc; các clone để sort/sửa độc lập phải giữ đúng ý nghĩa. Không được mô tả mọi `Add()` của collection này là copy toàn bộ array: implementation dùng `ImmutableList`. |
| C07 | Ứng viên dọn dependency/code cũ | `Manager/HistoryManager.cs:50–62`; `Data/User/History.cs:133–160`; `Gopet.csproj:38` | Nhánh ghi MongoDB đã comment; `HistoryMongoDB` còn tồn tại. `MongoDB.Driver` là ứng viên kiểm tra gỡ, nhưng còn `using` ở `Util/EmailService.cs:10`, `Manager/ScheduleManager.cs:4`. Cần dọn import, build và kiểm tra dependency graph trước khi kết luận bỏ package an toàn. Không xem toàn bộ thư viện NuGet là code thừa chỉ vì tìm ít tên trong source. |
| C08 | Theo dõi người đã gửi không có ích trong luồng thông thường | `Place/Place.cs:75–94` | `sentPlayer.Contains(player)` chạy trước vòng duyệt các phiên bản message nhưng mỗi player thông thường chỉ được duyệt một lần. Nó không ngăn hai predicate cùng đúng trong cùng lượt. Call site ở `GopetPlace.cs:238–240` hiện dùng hai điều kiện phiên bản loại trừ nhau. Làm rõ yêu cầu gửi một hay nhiều biến thể, rồi đơn giản hóa bằng nhánh chọn phù hợp; không bỏ hỗ trợ protocol cũ. |
| C09 | Đăng ký trùng cùng một type handler | `Manager/GopetManager.cs:923,927` | `JsonAdapter<CopyOnWriteArrayList<ClanMember>>` được đăng ký hai lần trong cùng khối khởi tạo. Giữ một lần đăng ký. Chi phí nhỏ và chỉ ở lúc khởi tạo, không phải nguyên nhân lag theo tick. |

Một số đoạn không nên xóa chỉ vì trông cũ:

- `AutoMaintenance` vẫn được khởi tạo tại `Manager/GopetManager.cs:1334`, dù `App/Main.cs` có lời gọi cũ bị comment.
- `ListWriterMessage` và `sendMessageWithCheckVersion` còn phục vụ tương thích phiên bản tại `Place/GopetPlace.cs:221–240`.
- `NoSession` được `Bot` sử dụng (`Server/Bot.cs:13`).
- Không xóa hàng loạt event 2024/2025 chỉ dựa vào tên năm: vẫn có tham chiếu trong gameplay, template và dữ liệu lưu.

## 4. Thứ tự xử lý đề xuất

1. **Sửa mạng và vòng đời session:** P02, P03, P04, P05; loại C01. Đảm bảo đóng phiên đúng một lần, không giữ session đã chết và không tăng queue vô hạn.
2. **Giảm I/O gửi:** P01, sau đó P09. So sánh dữ liệu gửi trước/sau cho cả hai client; giữ nguyên các biến thể protocol.
3. **Bảo đảm worker lịch sử phục hồi:** P06 trước, rồi P11. Đo backlog và xác nhận không mất/ghi trùng lịch sử khi DB gián đoạn.
4. **Giảm tải theo quy mô:** P08, P10, P12, P15 dựa trên profiling. P13 và P16 sửa kèm kiểm tra dữ liệu/logic spawn.
5. **Dọn code không hoạt động và dependency:** C02–C09 trong thay đổi riêng để dễ review và kiểm tra hành vi.

## 5. Kiểm chứng đã làm và giới hạn

Hai probe đã chạy trong PowerShell bằng `Add-Type`, không mở socket và không gọi server/DB:

```text
EOF: read=2/8, len=0, iterations=10000, originalLoopStillTrue=True
Packet4096: bytes=4101, byteWrites=4101, blockWrites=0
```

- Probe EOF dùng cùng điều kiện lặp/đọc của `MsgReader`, thêm giới hạn 10.000 vòng để không treo tiến trình kiểm tra.
- Probe ghi sử dụng source `IOExtension.cs`, bỏ hai import không liên quan và thêm `System.IO` cho compiler của probe; không thay thân các hàm ghi. Stream đếm là `MemoryStream` tùy biến, không phải `NetworkStream` thật.
- Các probe xác nhận cơ chế ở mức code/stream. Chưa chạy benchmark trên tiến trình GServer .NET 8, chưa dùng heap dump/CPU trace và chưa thực hiện build toàn server. Thay đổi trong lần review này chỉ là file báo cáo.

Khi triển khai sửa, nên đo trên môi trường kiểm thử với tải đại diện:

| Kịch bản | Chỉ số/điều kiện cần kiểm tra |
| --- | --- |
| Kết nối/ngắt lặp lại, handshake thiếu key, payload bị cắt | CPU trở về nền; số session sống/thread/handle/bộ nhớ sau nhiều đợt không tăng vô hạn; cleanup một lần |
| Client đọc chậm, broadcast nhiều, gói ảnh lớn | Số lần ghi stream, allocation/giây, queue byte/message, tuổi packet, thời gian gửi p95/p99; byte protocol không đổi |
| Nhiều battle trên cùng map và zone không có người | Tick elapsed p50/p95/p99, số tick quá ngân sách, độ trễ xử lý hành động, chi phí nền |
| Autosave/BXH/backup trùng thời điểm | Thời gian từng job, độ trễ DB, số dòng đọc/ghi, thời gian chờ pool, độ trễ gameplay |
| Log DB lỗi rồi phục hồi | Worker còn sống, backlog có giới hạn và rút hết, lịch sử quan trọng không bị mất hoặc nhân đôi |
| Inventory/thư lớn | Số phần tử duyệt cho mỗi lookup, thời gian thêm/tìm/sort, tính đúng của ID sau cập nhật |

Chưa có số liệu để cam kết CCU hoặc phần trăm cải thiện. Các mục P1 có đủ bằng chứng mã nguồn để ưu tiên xử lý mà không cần chờ một sự cố tải cao xảy ra.
