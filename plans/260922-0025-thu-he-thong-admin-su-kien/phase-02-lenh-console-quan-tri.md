# Phase 02 — Lệnh console cho quản trị

## Liên kết ngữ cảnh

- Tổng quan: [plan.md](plan.md) · Phụ thuộc: [Phase 01](phase-01-dich-vu-gui-thu-he-thong.md)
- Lệnh admin hiện có: `SRCGOPETGOC/GServer/CommandLine/AdminCommand.cs`
- Lớp cơ sở: `SRCGOPETGOC/GServer/CommandLine/BaseCommand.cs`
- Đăng ký lệnh: `SRCGOPETGOC/GServer/Manager/CommandManager.cs:13-20`
- Vòng đọc phím: `SRCGOPETGOC/GServer/Manager/CommandManager.cs:22+`

## Tổng quan

- **Ưu tiên:** Cao — không có lệnh thì Phase 01 không ai gọi được
- **Trạng thái:** Đã viết xong, compile sạch — còn chờ chạy thật trên server
- Thêm nhánh `letter` vào `AdminCommand` để gõ tay từ console server.

## Nhận định then chốt

1. **Console tách từ bằng dấu cách**, mà tiêu đề và nội dung thư đều có dấu cách. Phải có
   quy ước tách trường — dùng `|` làm dấu phân cách, không phải khoảng trắng.
2. `AdminCommand.Execute(params string[] args)` nhận mảng đã tách sẵn. Muốn lấy lại phần
   đuôi nguyên vẹn thì phải `string.Join(" ", args.Skip(n))` rồi mới cắt theo `|`.
3. Lệnh gõ sai **không được làm sập server** — vòng `StartReadingKeys` mà ném là mất luôn
   khả năng nhận lệnh. Bắt hết ngoại lệ, in hướng dẫn.
4. `AdminCommand.Description` đang là bảng trợ giúp cho `HelpCommand`; thêm lệnh thì phải
   cập nhật chuỗi đó, nếu không admin không biết lệnh tồn tại.

## Yêu cầu

### Chức năng

- `admin letter <tên người chơi> <admin|event> <tiêu đề>|<nội dung>` — gửi một người.
- `admin letter all <admin|event> <tiêu đề>|<nội dung>` — gửi toàn server.
- In kết quả rõ ràng: gửi được cho ai / bao nhiêu người / vì sao trượt.

### Phi chức năng

- Thiếu tham số hoặc sai loại thì in cú pháp đúng, không ném.
- Không thêm lệnh top-level mới; gộp vào `admin` cho gọn bảng trợ giúp.

## Kiến trúc

```
admin letter all   event  Quà Trung Thu|Đăng nhập nhận 100 xu
       └─┬──┘ └┬┘   └─┬─┘  └─────┬─────┘ └──────────┬────────┘
     nhánh   đích   loại      tiêu đề            nội dung
                                  └── cắt theo '|' ──┘
```

`ShortContent` sinh tự động, không bắt admin nhập: lấy 60 ký tự đầu của nội dung (danh
sách thư chỉ hiện chừng đó). Bớt một trường phải gõ là bớt một chỗ gõ sai.

## File liên quan

**Sửa**
- `SRCGOPETGOC/GServer/CommandLine/AdminCommand.cs` — thêm `case "letter"` và cập nhật `Description`

**Đọc để tham chiếu**
- `SRCGOPETGOC/GServer/Manager/SystemLetterService.cs` (Phase 01)
- `SRCGOPETGOC/GServer/Data/User/Letter.cs` — hằng loại thư

## Các bước thực hiện

1. Trong `AdminCommand.Execute`, thêm `case "letter":` gọi một hàm riêng
   `private static void SendLetter(string[] args)` — giữ `Execute` mỏng.
2. Kiểm `args.Length >= 4`, không đủ thì in cú pháp và thoát.
3. Ánh xạ `args[2]` → loại: `"admin"` → `Letter.ADMIN`, `"event"` → `Letter.EVENT`; khác
   thì in lỗi và thoát.
4. Ghép phần đuôi: `var rest = string.Join(" ", args.Skip(3));` rồi tách `rest` theo `'|'`
   đúng **một** lần (`Split('|', 2)`) — nội dung được phép chứa dấu `|`.
5. Thiếu vế nội dung thì in lỗi, không gửi thư rỗng.
6. Sinh `shortContent` = 60 ký tự đầu của nội dung, cắt thì thêm `…`.
7. `args[1] == "all"` → `SystemLetterService.SendToAll(...)`, in số người nhận.
   Ngược lại → `SystemLetterService.SendTo(args[1], ...)`, in thành/bại.
8. Cập nhật `Description` thêm hai dòng mô tả cú pháp.
9. Bọc toàn bộ trong `try/catch`, log qua `GopetManager.ServerMonitor.LogError`.
10. Build server.

## Danh sách việc

- [x] `case "letter"` trong `Execute`, tách ra hàm `SendLetter`
- [x] Kiểm số tham số + in cú pháp khi sai
- [x] Ánh xạ `admin`/`event` → hằng `Letter.ADMIN`/`Letter.EVENT`
- [x] Ghép đuôi rồi `Split('|', 2)`
- [x] Sinh `ShortContent` tự động (60 ký tự + `…`)
- [x] Nhánh `all` và nhánh một người
- [x] Cập nhật `Description`
- [x] try/catch bao ngoài
- [x] Build server không lỗi

## Tiêu chí hoàn thành

- `help` in ra cú pháp lệnh mới.
- `admin letter kzhd9x admin Bảo trì|Máy chủ bảo trì 22h hôm nay` → tài khoản đang online
  thấy huy hiệu số tăng, tab Admin có thư, tiêu đề và nội dung đúng.
- `admin letter all event Quà|Nhận quà tại NPC Sự kiện` → in "Đã gửi cho 11 người chơi",
  đăng nhập một tài khoản khác thấy thư ở tab Sự kiện.
- `admin letter` (thiếu tham số), `admin letter kzhd9x sai_loai a|b`,
  `admin letter khong_ton_tai admin a|b` → đều in lỗi rõ ràng, server vẫn nhận lệnh tiếp.

## Đánh giá rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| Ngoại lệ làm chết vòng đọc lệnh | try/catch bao cả nhánh `letter` |
| Admin gõ nhầm `all` khi định gửi một người | In lại số người nhận để thấy ngay; cân nhắc hỏi xác nhận nếu sau này số người chơi lớn |
| Nội dung chứa `|` bị cắt sai | `Split('|', 2)` — chỉ cắt lần đầu |

## Cân nhắc bảo mật

- Lệnh chỉ chạy từ **stdin của tiến trình server**, không qua mạng — ai vào được console
  thì đã có toàn quyền máy chủ rồi, không mở thêm bề mặt tấn công.
- Không có đường nào từ gói mạng của người chơi tới `SystemLetterService` (xem Phase 01).

## Bước tiếp theo

- Phase 03 thay việc gõ tay bằng gọi tự động từ code sự kiện.
