# Phase 03 — Nguồn tự động: đền bù và sự kiện

## Liên kết ngữ cảnh

- Tổng quan: [plan.md](plan.md) · Phụ thuộc: [Phase 01](phase-01-dich-vu-gui-thu-he-thong.md)
- Khung sự kiện: `SRCGOPETGOC/GServer/Data/Event/EventBase.cs`
- Sự kiện hiện có: `SRCGOPETGOC/GServer/Data/Event/` (`ArenaEvent`, `DailyBossEvent`,
  `DailyCheckin/DailyCheckinEvent`, `Year2024/`, `Year2025/GameBirthdayEvent`)
- Bảng xếp hạng: `SRCGOPETGOC/GServer/Data/top/` (`TopEvent`, `TopGem`, `TopAccumulatedPoint`)

## Tổng quan

- **Ưu tiên:** Trung bình — gõ tay ở Phase 02 đã dùng được, phase này để khỏi phải gõ
- **Trạng thái:** Xong — đã đổi sang hai đích khác sau khảo sát; nghiệm thu chạy thật OK
- Cho code sự kiện tự gửi thư thay vì admin ngồi gõ console.

## Kết quả khảo sát (2026-09-22): không có chỗ để cắm

Đọc code thì cả hai đích plan nhắm tới đều không dùng được, và **server không có đường
trao thưởng hàng loạt nào** để gắn thư vào:

| Đích dự kiến | Thực tế |
|---|---|
| `TopEvent.cs` | `Update()` chỉ **dựng lại bảng xếp hạng** theo chu kỳ, không trao thưởng gì. Cắm thư vào đây là gửi lại mỗi lần refresh — đúng cái rủi ro đã ghi ở bảng cuối trang |
| `DailyCheckinEvent.DoCheckin` | Người chơi **tự bấm** nên đang online, và đã nhận `okDialog` liệt kê quà. Thư chỉ là bản sao thừa |

Grep `onReiceiveGift` khắp `Data/Event` và `Data/top`: chỉ 3 chỗ, đều là người chơi tự bấm
nhận. Tìm chỗ kết thúc sự kiện: `EventManager` chỉ gỡ sự kiện khỏi danh sách, không phát
thưởng. **Tiền đề "người nhận thưởng thường đang offline" không đúng với server này.**

## Đích thay thế ĐÃ LÀM (user chốt 2026-09-22: làm cả hai)

1. **Thư chào mừng người chơi mới** — `SystemLetterService.SendWelcome` gọi ngay sau
   `PlayerData.create` trong `GameController.onClienSendCharInfo`. Tạo xong nhân vật là
   phiên đóng ngay, nên người nhận chắc chắn offline → thư đi đường bảng `letter` và chờ
   sẵn ở lần đăng nhập đầu tiên.
2. **Thông báo sự kiện mở** — broadcast `Letter.EVENT` trong `EventManager.AddEvent`.

**Vì sao là `AddEvent` chứ không phải `Init` hay `Condition`:**

| Chỗ cắm | Vì sao KHÔNG |
|---|---|
| `EventBase.Init` | Chạy lại **mỗi lần khởi động máy chủ** → mọi người nhận lại thư sau mỗi lần restart |
| `Condition` bật | Là **cửa sổ lặp theo giờ** (đấu trường mở lúc 6, 8, 11, 13, 17, 19, 21 giờ) → 7 thư/ngày/người. Khung giờ lặp là việc của băng chữ chạy `BannerEvent`, không phải của thư |
| `AddEvent` ✅ | Chỉ được gọi khi thêm sự kiện **lúc máy chủ đang chạy**. Các sự kiện cố định đăng ký thẳng bằng `_events.Add` trong static constructor, KHÔNG đi qua đây → không bắn lúc boot |

**Lưu ý:** hôm nay `AddEvent` mới chỉ có một nơi gọi là endpoint test
`/api/test/TestBossDaily`, nên nhánh này đang **nằm chờ**. Nó đúng về mặt ngữ nghĩa và sẽ
tự hoạt động khi có sự kiện thật được mở lúc đang chạy.

## Nhận định then chốt

1. **Đây là phase dễ phình nhất.** Có hàng chục sự kiện; nhồi thư vào hết là một đợt sửa
   lớn mà chưa biết có ai đọc. Chỉ làm **hai chỗ** có giá trị rõ ràng, phần còn lại để sau.
2. Người nhận thưởng sự kiện **thường đang offline** lúc sự kiện kết thúc — đó đúng là
   trường hợp thư hệ thống giải quyết được mà `okDialog` thì không (dialog chỉ tới được
   người đang online).
3. **Thư báo thưởng ≠ phát thưởng.** Phase này chỉ gửi *thông báo bằng chữ*. Vật phẩm vẫn
   phát bằng đường sẵn có của từng sự kiện. Đính kèm quà vào thư là Phase 05.

## Yêu cầu

### Chức năng

- Khi bảng xếp hạng một sự kiện chốt: gửi thư `EVENT` cho những người có thưởng, kể cả
  người đang offline.
- Khi admin cần đền bù sau sự cố: đã có lệnh console ở Phase 02, phase này **không** làm
  thêm gì cho đền bù.

### Phi chức năng

- Không đổi logic phát thưởng đang chạy — chỉ **thêm** một lời gọi gửi thư.
- Sự kiện không có người thắng thì không gửi gì.

## Kiến trúc

```
<Sự kiện chốt thưởng>
   ├── phát thưởng (đường cũ, giữ nguyên)
   └── SystemLetterService.SendTo(userId, Letter.EVENT, tiêu đề, tóm tắt, nội dung)
```

Không thêm lớp trung gian nào: `SystemLetterService` đã là API đủ hẹp.

## File liên quan

**Sửa (chọn đúng hai chỗ, không hơn)**
- `SRCGOPETGOC/GServer/Data/top/TopEvent.cs` — chốt top sự kiện
- `SRCGOPETGOC/GServer/Data/Event/DailyCheckin/DailyCheckinEvent.cs` — mốc điểm danh

**Đọc để tham chiếu**
- `SRCGOPETGOC/GServer/Manager/SystemLetterService.cs`

## Các bước thực hiện

1. Đọc `TopEvent.cs`, tìm đúng chỗ chốt danh sách người thắng (sau khi đã có `user_id` và
   phần thưởng). **Chưa sửa gì** — xác định vị trí trước.
2. Thêm một lời gọi `SystemLetterService.SendTo(...)` cho mỗi người thắng, ngay sau khi
   phần thưởng được ghi nhận thành công. Đặt sau để thưởng hụt thì không gửi thư sai.
3. Nội dung thư: nêu tên sự kiện, hạng đạt được, phần thưởng đã nhận. Tiêu đề cố định
   `"Sự kiện"` cho khớp cách client hiển thị.
4. Làm tương tự ở `DailyCheckinEvent.cs` cho mốc thưởng lớn (nếu có mốc; không có thì bỏ
   qua file này và ghi rõ lý do vào changelog).
5. Nếu vòng lặp người thắng chạy trong transaction DB: gọi gửi thư **sau khi** transaction
   commit, đừng gọi giữa chừng.
6. Build server.

## Danh sách việc

- [ ] Xác định vị trí chốt thưởng trong `TopEvent.cs`
- [ ] Thêm gửi thư `EVENT` sau khi thưởng ghi nhận thành công
- [ ] Soạn nội dung thư (tên sự kiện + hạng + phần thưởng)
- [ ] Rà `DailyCheckinEvent.cs`, thêm nếu có mốc thưởng; không có thì ghi lý do
- [ ] Bảo đảm gửi thư nằm ngoài transaction
- [ ] Build server không lỗi

## Tiêu chí hoàn thành

- Chạy chốt top sự kiện trên môi trường dev với ít nhất một tài khoản **offline** có
  thưởng: `SELECT * FROM letter WHERE Type=3` thấy dòng cho tài khoản đó.
- Đăng nhập tài khoản đó: thư nằm ở tab Sự kiện, huy hiệu số đếm đúng.
- Sự kiện không có người thắng: không sinh dòng thư nào.

## Đánh giá rủi ro

| Rủi ro | Giảm thiểu |
|---|---|
| Phình ra sửa hàng chục sự kiện | Giới hạn cứng hai file; cái khác mở phase mới |
| Gửi thư báo thưởng nhưng thưởng thật ra thất bại | Gọi sau khi thưởng ghi nhận xong, ngoài transaction |
| Sự kiện chạy định kỳ gửi trùng thư mỗi lần chạy | Gắn vào đúng nhánh "chốt một lần", không phải nhánh tick định kỳ — phải đọc kỹ `TopEvent` trước khi chèn |
| Spam hộp thư người chơi | Chỉ gửi cho người **có thưởng**, không gửi toàn server |

## Cân nhắc bảo mật

- Không có đầu vào từ người chơi ở phase này; nội dung thư do code sinh.
- Không ghi `user_id` hay dữ liệu tài khoản của người khác vào nội dung thư.

## Bước tiếp theo

- Phase 04 làm client phân biệt được thư hệ thống với thư bạn bè.
- Nếu cần đền bù kèm vật phẩm thì mở Phase 05.
