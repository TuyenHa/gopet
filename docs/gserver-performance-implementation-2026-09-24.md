# Triển khai performance GServer — 24/09/2026

Theo [báo cáo review](gserver-performance-review-2026-09-24.md), trên nhánh `fix/performance`. Chưa deploy, chưa chạy migration hoặc load test lên DB/server đang phục vụ.

## Phạm vi thực hiện

| ID | Trạng thái và thay đổi |
| --- | --- |
| P01 | Header big-endian và payload ghi theo block, giữ byte protocol. |
| P02 | EOF giữa payload ném lỗi; kiểm tra length trước allocation. |
| P03–P04 | Đăng ký session trước chạy; cleanup một lần, gỡ owner, giảm socket gauge, bỏ ép GC. Close trả ngay; worker drain tối đa 1 giây. Disconnect chờ handler đang chạy kết thúc; shutdown chờ cleanup. |
| P05 | Queue tối đa 1.024 message hoặc 16 MiB, tối đa 10 giây chờ; socket send timeout 5 giây. Quá giới hạn đóng phiên, không loại tùy ý packet giao dịch. Một packet đang ghi nằm ngoài số byte đang chờ. |
| P06/P11 | Snapshot lịch sử vào đĩa trước khi add trả về, không giữ Player. Batch tối đa 64 record và 256 KiB file JSON (record lớn hơn được thử riêng); tự chia batch khi MySQL báo PacketTooLarge. Retry cách 2 giây, eventId duy nhất chống trùng khi mất ACK; chỉ mở DB khi có dữ liệu, game connection chỉ dùng khi phải tra tên. Shutdown giữ phần chưa gửi trên đĩa. |
| P07 | Tối đa 64 handshake, deadline tổng 5 giây, đọc đủ 9 byte. Giữ mô hình hai thread/session; đổi sang async I/O chờ profiling. |
| P08 | Binary search trên immutable snapshot có indexer; enumerable khác chỉ materialize một lần. Với ImmutableList, độ phức tạp lookup là O(log² n), không coi indexer là O(1). |
| P09 | Đóng băng payload một lần khi gửi, dùng chung giữa các sender; TEA tạo buffer riêng. Writer giữ từ trước cũng bị đóng sau finalize. `cleanup()` giữ tương thích với DataOutputStream.Close vốn là no-op. |
| P10 | Backup chạy worker riêng, không chồng lượt, lỗi retry sau 5 phút; shutdown ngừng lịch và chờ worker. Export vào `.partial-*`, chỉ rename sang `.sql` khi thành công. Autosave giữ logic persistence hiện tại; đã thêm đo thời gian job, chưa thay lịch save/snapshot khi chưa có profiling. |
| P12 | Đã thêm đo thời gian TOP Pet qua BXHManager; giữ query/schema BXH đến khi có execution plan và số liệu DB. |
| P13 | Lọc template spawn hợp lệ một lần mỗi đợt có vị trí spawn. Quà thử tối đa 128 vòng độc lập, chọn đều trong tập vượt ngưỡng như cũ; không trừ tiền nếu hết lượt. Phân phối có điều kiện chọn thành công được giữ; xác suất trả lỗi thay từ deadline thời gian sang số lượt hữu hạn. |
| P14 | Logger nền queue 8.192 dòng; giới hạn nội dung mỗi message, batch flush, fallback độc lập. Có bộ đếm dropped và flush khi shutdown. Đây là log chẩn đoán; lịch sử giao dịch đi spool riêng. |
| P15 | Tick dùng Stopwatch, chỉ chờ phần còn lại của 500 ms; quá budget nhường 1 ms. Chưa thay số zone/nhịp zone trống để bảo toàn boss/event/respawn. |
| P16 | Deadline dọn mob chỉ đặt lần đầu, đến hạn sau 5 giây. |
| C01–C04 | Một nơi tạo Player; bỏ local ScanData không được gọi, async không await và các field không dùng. Các script ScanData cũ vẫn có thể lấy lại từ git trước thay đổi; không tạo lệnh admin ghi DB mới. |
| C05 | Bỏ chờ 1 giây ở nhánh trùng tên, có test gói cuối qua socket. Giữ sleep menu/dialog vì chưa xác định ý định rate limit. |
| C06–C09 | Clone chia sẻ ImmutableList bất biến, bỏ hai clone chỉ đọc; bỏ MongoDB.Driver/import/model chết; HashSet deduplicate người nhận và vẫn gửi mọi predicate khớp; bỏ đăng ký handler trùng. |

## Điều kiện triển khai lịch sử

1. Chạy `SRCGOPETGOC/MariaDB_SQL/migration-260924-history-event-id.sql` trên **log DB** trước khi chạy bản mới. Bảng history phải dùng InnoDB (schema trong repo đang dùng InnoDB). Migration thêm cột nullable và unique index; không xóa dữ liệu cũ.
2. Đặt `GOPET_HISTORY_SPOOL` vào thư mục/volume bền vững, ghi được, chỉ dành cho một tiến trình GServer. Mặc định là `history-spool` dưới thư mục binary; khi thay container phải mount volume này để giữ backlog.
3. `GOPET_HISTORY_MAX_BYTES` mặc định 256 MiB. Hết dung lượng hoặc lỗi ghi đĩa sẽ ném lỗi rõ tại producer. Việc thêm history chưa có transaction chung với cập nhật tài sản; không được diễn giải thành bảo đảm audit nguyên tử cho giao dịch gameplay.
4. Không xóa file `.json` còn chờ. Worker chỉ xóa sau commit thành công; khi thiếu migration hoặc DB lỗi, backlog còn trên đĩa và worker retry. File hỏng cần vận hành kiểm tra, không tự bỏ record. File `.tmp` chưa được rename là add chưa hoàn tất; giữ để điều tra nếu tiến trình bị ngắt giữa ghi.
5. Theo dõi `HistoryManager.Backlog`, `BacklogBytes`, `HeartbeatUtc`, lỗi console; `MsgSender.QueuedMessages`, `QueuedBytes`, `OldestMessageAgeMs`; `Monitor.DroppedMessages`.

Một record riêng lẻ vượt `max_allowed_packet` vẫn được giữ và báo lỗi, cần chỉnh giới hạn DB hoặc xử lý dữ liệu đó. Không tự bỏ record. Shutdown có thể phải chờ một export dài; khi tiến trình bị cưỡng bức dừng, file chưa xong mang tên `.partial-*`, không được dùng như backup hoàn chỉnh.

Snapshot ghi/flush đĩa đồng bộ ở producer đổi lấy không giữ object sống và khả năng phục hồi sau restart. Cần đo độ trễ đĩa thực tế trước triển khai rộng. Không tuyên bố exactly-once cho toàn giao dịch; eventId chỉ chống insert trùng cho một sự kiện đã được spool thành công.

## Kiểm chứng

```powershell
dotnet build tests/GServer.Performance.Tests -p:NuGetAudit=false
dotnet run --project tests/GServer.Performance.Tests --no-build --no-restore
dotnet test GopetUnityClient/tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj
```

Máy hiện tại dùng `.superpowers/dotnet/dotnet.exe` (.NET 8.0.425). Bộ test GServer là console runner, exit code khác 0 khi có lỗi; dùng `dotnet run`, không dùng `dotnet test` cho project này. Test có config cô lập, không mở DB/server thật; socket test chỉ dùng loopback cổng ngẫu nhiên. Test wire liên kết trực tiếp decoder/TEA của Unity và đối chiếu frame literal cho protocol thường/iWin, vector TEA đã có trong repo.

Kết quả: **24/24 test GServer**, **942/942 test mạng/logic client**, build Debug và Release thành công; `git diff --check` sạch. Các lượt build còn 455–456 cảnh báo (baseline 476), không tuyên bố codebase hết cảnh báo. Dependency graph sau restore không còn MongoDB. Review độc lập phát hiện hai lỗi quan trọng (batch quá lớn và ownership backup khi shutdown); cả hai đã được sửa, thêm test trước và chạy lại suite.

Meter `Gopet.Server`, histogram `gopet.operation.duration` (ms), tag `operation`: map-tick, AutoSave, DBBackup, db-backup, từng lớp TOP. Dùng dữ liệu này cùng CPU/heap trace và số liệu DB để quyết định phần P07/P10/P12/P15 còn phụ thuộc profiling.

Chưa xác minh trên JAR/Unity chạy thật, lỗi/mất ACK của MySQL thực, mất điện filesystem, tải CCU lớn, hoặc thời gian p95/p99. Cần staging fault-injection và profiling theo bảng kịch bản trong báo cáo gốc trước rollout. Không có cam kết phần trăm CPU/CCU.
