# Phase 01 — Dịch vụ gửi thư hệ thống

## Liên kết ngữ cảnh

- Tổng quan: [plan.md](plan.md)
- Model thư: `SRCGOPETGOC/GServer/Data/User/Letter.cs:13-15`
- Đường gửi hiện có (người chơi → người chơi): `SRCGOPETGOC/GServer/Server/GameController.cs:429-480`
- Nạp thư lúc đăng nhập: `SRCGOPETGOC/GServer/Server/Player.cs:494-502`
- Sinh `LetterId`: `SRCGOPETGOC/GServer/Util/Utilities.cs:364-378`
- Báo có thư mới: `SRCGOPETGOC/GServer/Server/GameController.cs:631-641`

## Tổng quan

- **Ưu tiên:** Cao — mọi phase khác đều gọi vào đây
- **Trạng thái:** Đã viết xong, compile sạch — còn chờ chạy thật trên server
- Một lớp tĩnh gửi thư **không cần người gửi**, xử lý cả người online lẫn offline, dùng
  chung cho lệnh console (Phase 02) và code sự kiện (Phase 03).

## Nhận định then chốt

1. **Không được tái dụng `GameController.sendLetter`.** Nó là hàm của một `GameController`
   gắn với một `player` cụ thể và làm ba việc không hợp với thư hệ thống:
   - chặn 30 giây giữa hai lần gửi (`LettersSendTime`) — đền bù hàng loạt sẽ bị chặn;
   - bắn `redDialog`/`okDialog` về phía người gửi — thư hệ thống không có ai để báo;
   - gán `letter.userId = player.user.user_id` — thư hệ thống không có người gửi thật.
2. **Hai đường giao khác nhau, phải làm cả hai.** Online thì `playerData.addLetter` rồi
   `sendHasLetter()` để chấm đỏ/huy hiệu số cập nhật ngay. Offline thì INSERT vào bảng
   `letter`, đăng nhập sẽ tự nạp và xoá.
3. **`userId` của bảng `letter` là NOT NULL.** Thư hệ thống không có người gửi nên dùng
   một hằng số quy ước `SystemSenderId = 0`; 0 không trùng `user_id` thật nào.
4. **`IsMark` không có cột trong bảng `letter`.** Thư giao qua bảng luôn về ở trạng thái
   chưa đọc — đúng cái ta muốn, không phải làm gì thêm.
5. **Đừng tự sinh `LetterId`.** `addLetter` → `BinaryObjectAdd` tự gán id ngẫu nhiên chưa
   trùng trong danh sách của người đó. Tự đặt id là sinh trùng.

## Yêu cầu

### Chức năng

- Gửi thư loại bất kỳ (`ADMIN`/`EVENT`) cho **một** người chơi theo `user_id` hoặc theo tên.
- Gửi cho **tất cả** người chơi (online + offline).
- Người online thấy ngay: thư vào danh sách và cờ có-thư-mới được bắn.
- Người offline nhận đủ khi đăng nhập lần sau.

### Phi chức năng

- Không đổi bất kỳ gói mạng nào → client jar cũ không bị ảnh hưởng.
- Không chặn luồng gọi: broadcast cho N người không được giữ khoá lâu.
- Hàm phải gọi được từ luồng console (khác luồng game).

## Kiến trúc

```
SystemLetterService (tĩnh)
├── SendTo(int userId, sbyte type, title, shortContent, content) -> bool
│     ├── PlayerManager.get(userId) != null  → playerData.addLetter + sendHasLetter
│     └── ngược lại                          → INSERT INTO letter
├── SendTo(string playerName, ...) -> bool     (tra user_id qua bảng player)
└── SendToAll(sbyte type, title, shortContent, content) -> int (số người nhận)
      ├── mọi Player trong PlayerManager.players → đường online
      └── mọi user_id còn lại trong bảng player  → INSERT theo lô
```

Luồng dữ liệu giữ nguyên đường sẵn có: thư vào `playerData.letters` → lần
`LETTER_BOX` kế tiếp client nhận đủ; `sendHasLetter` làm huy hiệu số cập nhật ngay mà
không cần gói mới.

## File liên quan

**Tạo mới**
- `SRCGOPETGOC/GServer/Manager/SystemLetterService.cs`

**Sửa**
- Không sửa file nào ở phase này. Chỉ thêm một lớp mới, không đụng đường gửi của người chơi.

## Các bước thực hiện

1. Tạo `SystemLetterService.cs` trong `Manager/`, lớp `public static class`.
2. Khai báo hằng `public const int SystemSenderId = 0;` kèm chú thích vì sao là 0.
3. Viết `private static Letter Build(sbyte type, string title, string shortContent, string content)`
   — dựng `Letter` với `time = DateTime.Now`, `userId = SystemSenderId`.
4. Viết `SendTo(int userId, ...)`:
   - `PlayerManager.get(userId)`; nếu có → `p.playerData.addLetter(letter)` rồi
     `p.controller.sendHasLetter()`; trả `true`.
   - nếu không → mở `MYSQLManager.create()`, INSERT bản ghi với `targetId = userId`.
     Dùng đúng câu INSERT như `GameController.cs:464` để không lệch tên cột.
5. Viết `SendTo(string playerName, ...)` — `SELECT user_id FROM player WHERE name = @name`,
   không thấy thì trả `false`.
6. Viết `SendToAll(...)`:
   - lặp `PlayerManager.players` (là `CopyOnWriteArrayList`, duyệt an toàn) → đường online,
     ghi lại tập `user_id` đã xử lý;
   - `SELECT user_id FROM player` rồi INSERT cho những id chưa xử lý, ghi theo lô bằng một
     câu `Execute` nhận mảng tham số (Dapper tự gộp).
7. Bọc toàn bộ thân hàm trong `try/catch`, log qua `GopetManager.ServerMonitor.LogError`
   và trả `false`/`0` — lệnh console gõ sai không được làm sập server.
8. Build server: `dotnet build SRCGOPETGOC/GServer`.

## Danh sách việc

- [x] Tạo `SystemLetterService.cs` với hằng `SystemSenderId`
- [x] `Build()` dựng Letter chuẩn
- [x] `SendTo(int userId, ...)` — nhánh online
- [x] `SendTo(int userId, ...)` — nhánh offline (INSERT)
- [x] `SendTo(string playerName, ...)` — tra tên
- [x] `SendToAll(...)` — online + INSERT theo lô
- [x] try/catch + log ở mọi hàm public
- [x] Build server không lỗi

## Tiêu chí hoàn thành

- Gọi `SystemLetterService.SendTo(1458, Letter.ADMIN, "Ban quản trị", "Thông báo", "…")`
  từ `ExecuteCommand` (hoặc một chỗ tạm) trong lúc tài khoản 1458 **đang online**: client
  thấy huy hiệu số tăng, mở hộp thư tab Admin thấy thư.
- Làm lại khi tài khoản đó **offline**: kiểm `SELECT * FROM letter WHERE targetId=1458`
  thấy dòng `Type=2`; đăng nhập xong thư xuất hiện và bảng `letter` sạch dòng đó.
- `SendToAll` chạy với 11 người chơi hiện có: không lỗi, số trả về = 11.

## Đánh giá rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| Thư gửi người đang online chỉ nằm trong RAM, server chết là mất | Chấp nhận ở phase này (đúng bằng mức an toàn của thư người chơi hiện nay). Cần chắc hơn thì ghi cả vào bảng `letter` rồi xoá sau khi lưu `player` — để Phase 05 tính cùng |
| Broadcast khoá DB lâu khi nhiều người chơi | Ghi theo lô một câu lệnh; chỉ đụng người offline |
| Gọi từ luồng console đụng dữ liệu luồng game | `PlayerManager.players` là `CopyOnWriteArrayList`, duyệt an toàn; `addLetter` chỉ thêm vào cuối |
| Sinh `LetterId` trùng | Không tự sinh — để `BinaryObjectAdd` lo |

## Cân nhắc bảo mật

- `SystemLetterService` là API **nội bộ server**, không gắn với gói mạng nào → người chơi
  không tự gọi được.
- Người chơi không được tự gửi `ADMIN`/`EVENT`: đường của người chơi
  (`GameController.cs:514`) **luôn ép** `Letter.FRIEND`, không đọc loại từ gói. Giữ nguyên
  như vậy, tuyệt đối không cho client truyền `type` lên.
- `title`/`content` do admin nhập, đi thẳng vào client — Phase 04 phải chặn rich text nếu
  client có bật.

## Bước tiếp theo

- Phase 02 dùng dịch vụ này cho lệnh console.
- Phase 03 gọi từ code sự kiện.
