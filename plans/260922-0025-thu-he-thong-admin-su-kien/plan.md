# Thư hệ thống: làm sống tab Admin và Sự kiện

## Bối cảnh

Hộp thư client đã lọc theo `Letter.Type` (1 = FRIEND, 2 = ADMIN, 3 = EVENT), nhưng **server
chưa bao giờ tạo thư loại 2 hay 3**. Toàn bộ server chỉ có đúng một chỗ sinh thư —
`GameController.cs:514`, người chơi gửi cho nhau, luôn gán `Letter.FRIEND`. DB xác nhận:
bảng `letter` có 3 dòng, cả 3 đều `Type = 1`.

Mục tiêu: mở đường cho server gửi thư hệ thống (thông báo, đền bù, quà sự kiện) để hai tab
đó có dữ liệu thật.

## Ràng buộc đã khảo sát

- `GameController.sendLetter` **không tái dụng được**: nó buộc vào một người gửi (giới hạn
  30 giây `LettersSendTime`, `redDialog`/`okDialog`, gán `letter.userId` = người gửi).
- Bảng `letter` **không có** `LetterId` lẫn `IsMark` — nó chỉ là hàng đợi giao cho người
  đang offline. `LetterId` do `Utilities.BinaryObjectAdd` sinh ngẫu nhiên lúc thư vào
  `playerData.letters` (`Utilities.cs:364-378`).
- Người online nhận thẳng vào bộ nhớ (`GameController.cs:472-474`); người offline nhận qua
  bảng `letter`, được nạp và xoá lúc đăng nhập (`Player.cs:494-502`).
- `Letter` **không có ô đính kèm vật phẩm** — đền bù kèm quà cần đổi schema, tách hẳn ra
  Phase 05.
- Server **không có project test**. Nghiệm thu bằng lệnh console + tra DB + client thật.

## Các phase

| # | Phase | Trạng thái | Mô tả ngắn |
|---|---|---|---|
| 01 | [Dịch vụ gửi thư hệ thống](phase-01-dich-vu-gui-thu-he-thong.md) | **Xong, nghiệm thu chạy thật OK** | Lõi: gửi được cho 1 người hoặc tất cả, online lẫn offline |
| 02 | [Lệnh console cho quản trị](phase-02-lenh-console-quan-tri.md) | **Xong, nghiệm thu chạy thật OK** | `admin letter` để gõ tay từ console server |
| 03 | [Nguồn tự động: đền bù & sự kiện](phase-03-nguon-tu-dong.md) | **Xong (đổi đích)** | Gọi dịch vụ từ code sự kiện, thay vì gõ tay |
| 04 | [Client: phân biệt thư hệ thống](phase-04-client-phan-biet-thu.md) | **Xong (chờ chạy test)** | Nhãn loại thư, bỏ nút Xoá với thư admin |
| 05 | [Quà đính kèm](phase-05-qua-dinh-kem.md) | Tuỳ chọn | Đổi schema + phát vật phẩm. To nhất, hoãn được |

## Phụ thuộc

- 02 cần 01. 03 cần 01. 04 độc lập, làm song song được.
- 05 cần 01 và 04, nên làm sau cùng và chỉ khi thật sự cần đền bù kèm vật phẩm.
- Phase 01–02 là mốc **dùng được**: admin gõ lệnh là người chơi nhận thư.

## Rủi ro chính

- **Mất thư khi server chết**: thư gửi người đang online chỉ nằm trong RAM tới lần lưu
  `player` kế tiếp. Xem cách giảm thiểu ở Phase 01.
- **Broadcast tốn ghi**: gửi toàn server là một INSERT cho mỗi người chơi offline (hiện 11
  người, tương lai thì cần ghi theo lô).
- **Client jar cũ dùng chung server**: không đổi gói mạng nào trong plan này, chỉ thêm dữ
  liệu vào luồng LETTER_BOX sẵn có, nên jar cũ không bị ảnh hưởng.
