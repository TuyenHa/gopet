# Phase 04 — Client: phân biệt thư hệ thống

## Liên kết ngữ cảnh

- Tổng quan: [plan.md](plan.md) — làm song song được với Phase 01–03
- Hộp thư: `GopetUnityClient/Assets/Scripts/Runtime/UI/MailboxView.cs`,
  `MailboxView.List.cs`
- Dòng thư: `GopetUnityClient/Assets/Scripts/Runtime/UI/PopupTextRow.cs`
- Đọc thư: `GopetUnityClient/Assets/Scripts/Runtime/UI/LetterDetailView.cs`
- Model: `GopetUnityClient/Assets/Scripts/Net/Social/Letter.cs`
- Test sẵn có: `GopetUnityClient/Assets/Tests/PlayMode/MailboxPopupTests.cs`

## Tổng quan

- **Ưu tiên:** Trung bình
- **Trạng thái:** Đã viết xong, compile sạch — test PlayMode mới compile, chưa chạy
- Client hiện lọc đúng theo `Type` rồi, nhưng **nhìn vào không biết thư nào là của ai**:
  mọi dòng đều hiện `Title`, mà thư hệ thống thì `Title` là chữ do admin đặt.

## Nhận định then chốt

1. **Phần lọc đã xong** ở đợt làm hộp thư — không phải làm lại. Phase này chỉ lo phần
   *nhận biết*.
2. Ở tab "Tất cả", thư admin và thư bạn bè nằm lẫn nhau, không có gì phân biệt. Thư hệ
   thống cần nổi hơn.
3. **Thư hệ thống không nên xoá được như thư thường** — đền bù mà lỡ tay xoá là mất bằng
   chứng. Ít nhất phải hỏi lại rõ hơn.
4. `LetterDetailView` vẫn là overlay kiểu cũ, lạc tông với popup mới. Sửa ở đây thì gọn
   một công, nhưng **là việc riêng** — ghi vào bước tiếp theo, không nhồi vào phase này.

## Yêu cầu

### Chức năng

- Dòng thư hệ thống có dấu hiệu nhận biết ngay trong danh sách (nhãn loại).
- Ở tab "Tất cả", thư admin xếp **trên** thư bạn bè cùng trạng thái đọc.
- Thư `ADMIN` hỏi xác nhận kỹ hơn khi xoá.

### Phi chức năng

- Không đổi gói mạng.
- Giữ mọi file dưới 200 dòng.
- Bổ sung test PlayMode cho phần mới.

## Kiến trúc

```
PopupTextRow.Bind(title, subtitle, unread)
        └── thêm tham số nhãn loại (tuỳ chọn, null = không hiện)
              └── chip nhỏ bên trái tiêu đề: "BQT" (xanh) / "SK" (vàng)
```

Dùng lại `ShopChip` nếu nó đủ tổng quát; không thì thêm một nhãn chữ nhỏ bo góc trong
`PopupTextRow`. **Kiểm `ShopChip` trước khi viết mới.**

## File liên quan

**Sửa**
- `PopupTextRow.cs` — thêm nhãn loại
- `MailboxView.List.cs` — truyền loại vào `Bind`, sắp xếp thư admin lên trước
- `GameSession.Mail.cs` — lời xác nhận xoá riêng cho thư `ADMIN`
- `MailboxPopupTests.cs` — test cho nhãn và thứ tự

**Đọc để tham chiếu**
- `ShopChip.cs`, `PopupPalette.cs`

## Các bước thực hiện

1. Đọc `ShopChip.cs` xem có tái dụng được làm nhãn loại không. Được thì dùng, không thì
   thêm một nhãn nhỏ ngay trong `PopupTextRow` (đừng tạo file mới cho một cái nhãn).
2. Thêm tham số loại vào `PopupTextRow.Bind`, mặc định không hiện nhãn — để chỗ khác dùng
   `PopupTextRow` không phải sửa.
3. `MailboxView.List.cs`: truyền loại thư; ánh xạ `2 → "BQT"`, `3 → "SK"`, `1 → không nhãn`
   (thư bạn bè đã có tên người gửi ở tiêu đề rồi).
4. Sắp xếp danh sách: chưa đọc lên trước, trong cùng nhóm thì `ADMIN` trước `EVENT` trước
   `FRIEND`. Sắp ở `Filtered()`, không đụng dữ liệu gốc.
5. `GameSession.Mail.cs` — `ConfirmRemoveLetter`: nếu thư là `ADMIN` thì đổi câu hỏi thành
   cảnh báo rõ hơn ("Thư của ban quản trị. Xoá rồi không lấy lại được."). Cần truyền cả
   `Letter` thay vì mỗi `id` — kiểm lại chữ ký `RemoveRequested`.
6. Thêm test vào `MailboxPopupTests.cs`: thư admin có nhãn, thư bạn bè không có, thứ tự
   sắp xếp đúng.
7. Chạy `verify.ps1`.

## Danh sách việc

- [x] Rà `ShopChip` xem tái dụng được không
- [x] `PopupTextRow.Bind` nhận nhãn loại (mặc định không hiện)
- [x] `MailboxView.List.cs` truyền nhãn theo `Letter.Type`
- [x] Sắp xếp: chưa đọc trước, rồi ADMIN → EVENT → FRIEND
- [x] Xác nhận xoá riêng cho thư ADMIN
- [x] Test: nhãn đúng loại, thứ tự đúng
- [x] `verify.ps1` qua các bước compile + test

## Tiêu chí hoàn thành

- Gửi một thư `ADMIN` và một thư `FRIEND` cho cùng tài khoản: tab "Tất cả" hiện thư admin
  có nhãn "BQT" và nằm trên.
- Tab "Admin" chỉ có thư admin; tab "Bạn bè" chỉ có thư bạn bè.
- Bấm xoá thư admin: câu xác nhận khác câu của thư thường.
- Test PlayMode mới chạy qua (cần đóng Unity Editor để chạy batch-mode).

## Đánh giá rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| Nhãn làm dòng thư chật, tiêu đề bị cắt | Nhãn nhỏ, đặt trước tiêu đề; kiểm với tiêu đề dài nhất |
| Đổi chữ ký `Bind` làm vỡ chỗ dùng khác | Tham số có giá trị mặc định |
| Sắp xếp làm lệch ánh xạ chỉ số ↔ thư | `PopupTextRow` bắt sự kiện bằng closure giữ chính đối tượng `Letter`, không theo chỉ số — giữ nguyên cách này |

## Cân nhắc bảo mật

- Nội dung thư do admin nhập và hiển thị thẳng: **tắt rich text** ở `PopupTextRow` và
  `LetterDetailView` để không ai chèn thẻ màu/kích thước. Kiểm `supportRichText = false`.
- Không tin `Type` từ server để cấp quyền gì — nó chỉ dùng cho hiển thị.

## Bước tiếp theo

- Việc riêng, chưa lên lịch: chuyển `LetterDetailView` sang khung popup chung
  (`GamePopupFrame`) cho khớp tông với hộp thư mới.
