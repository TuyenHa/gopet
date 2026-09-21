# Phase 05 — Quà đính kèm trong thư (tuỳ chọn)

## Liên kết ngữ cảnh

- Tổng quan: [plan.md](plan.md) · Phụ thuộc: [Phase 01](phase-01-dich-vu-gui-thu-he-thong.md), [Phase 04](phase-04-client-phan-biet-thu.md)
- Model thư server: `SRCGOPETGOC/GServer/Data/User/Letter.cs`
- Bảng `letter`: không có PK, cột hiện tại là `userId, targetId, time, Type, Title, ShortContent, Content`
- Đọc thư phía client: `GopetUnityClient/Assets/Scripts/Runtime/UI/LetterDetailView.cs`
- Gói thư: `GopetUnityClient/Assets/Scripts/Net/Social/LetterHandler.cs`,
  `SRCGOPETGOC/GServer/Server/GameController.cs:610-630`

## Tổng quan

- **Ưu tiên:** Thấp — **hoãn được**, và nên hoãn cho tới khi thật sự cần đền bù kèm vật phẩm
- **Trạng thái:** Tuỳ chọn, chưa cam kết
- Đây là phase **to nhất và rủi ro nhất** trong plan: đụng schema DB, đụng gói mạng, đụng
  đường phát vật phẩm.

## Vì sao tách riêng

Phase 01–04 chỉ **thêm dữ liệu vào luồng sẵn có**, không đổi gói mạng nào, nên client jar
cũ dùng chung server không hề bị ảnh hưởng. Phase này thì ngược lại:

1. **Phải đổi gói `LETTER_BOX`.** `GameController.cs:610-630` ghi 6 trường mỗi thư; thêm
   quà là thêm trường. Client jar cũ đọc theo đúng thứ tự đó sẽ **lệch khung và hỏng
   toàn bộ hộp thư**, không chỉ mất phần quà.
2. **Phải đổi schema** bảng `letter` và cấu trúc JSON `player.letters` đang lưu.
3. **Phát vật phẩm là đường nhạy cảm nhất của server** — sai là nhân bản đồ.

## Quyết định cần chốt trước khi bắt tay

Không viết dòng code nào trước khi trả lời được:

- **Còn phải đỡ client jar cũ không?** Nếu còn thì phải gắn cờ phiên bản
  (`GopetManager.VERSION_133` đã có tiền lệ) và gửi hai khuôn gói khác nhau. Nếu bỏ jar
  được thì phase này nhẹ đi đáng kể.
- **Quà là gì?** Chỉ tiền (xu/gem) hay cả vật phẩm/pet? Chỉ tiền thì đơn giản hơn nhiều
  và đủ cho đa số ca đền bù.
- **Nhận một lần hay nhiều lần?** Phải có cờ đã-nhận **ghi bền**, không thì bấm nhiều lần
  là nhân đồ. Lưu ý `IsMark` hiện **không có cột** trong bảng `letter`.

## Phác hoạ hướng làm (chưa chốt)

```
Letter + Reward (chuỗi JSON) + IsClaimed (bool)
   ├── server: cột mới ở bảng letter + trường mới trong JSON player.letters
   ├── gói LETTER_BOX: thêm trường, gắn cờ phiên bản nếu còn đỡ jar cũ
   ├── gói mới: CLAIM_LETTER_REWARD (client → server)
   │     └── kiểm IsClaimed, phát quà, đặt IsClaimed, LƯU NGAY, trả kết quả
   └── client: nút "Nhận quà" trong LetterDetailView, ẩn khi đã nhận
```

Điểm chí tử: bước phát quà phải **idempotent**. Kiểm `IsClaimed` và ghi cờ trong **cùng
một transaction** với việc cộng đồ; hai lời gọi song song phải chỉ một cái thắng.

## Rủi ro

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Nhân bản vật phẩm do bấm nhận nhiều lần / hai kết nối | **Nghiêm trọng** | Kiểm-và-đặt cờ trong một transaction, khoá theo `letterId`; test bằng hai lời gọi song song |
| Hỏng hộp thư của client jar cũ | Cao | Gắn cờ phiên bản cho gói, hoặc chốt bỏ hẳn jar trước khi làm |
| Mất quà khi server chết trước lúc lưu `player` | Cao | Ghi bền cờ đã-nhận ngay tại thời điểm phát, không đợi chu kỳ lưu |
| Di trú dữ liệu `player.letters` đang có | Trung bình | Trường mới phải có giá trị mặc định khi deserialize JSON cũ |

## Tiêu chí hoàn thành (nếu làm)

- Gửi thư đền bù kèm quà cho tài khoản offline; đăng nhập, mở thư, bấm nhận → đồ vào túi
  đúng một lần.
- Bấm nhận lần hai → báo "đã nhận", số lượng đồ **không đổi**.
- Hai kết nối cùng bấm nhận một thư → chỉ một lần phát.
- Server restart giữa chừng → không mất quà, cũng không phát hai lần.
- Client jar cũ mở hộp thư → vẫn đọc được danh sách (hoặc đã chốt bỏ jar).

## Khuyến nghị

**Chưa làm.** Phase 01–02 đã đủ để tab Admin và Sự kiện hết rỗng, mà không đụng schema hay
gói mạng. Chỉ mở phase này khi có nhu cầu đền bù kèm vật phẩm thật, và khi đã chốt được
câu hỏi "còn đỡ jar cũ không".

## Câu hỏi chưa có lời giải

- Còn phải tương thích client jar cũ tới bao giờ?
- Đền bù dự kiến kèm gì: chỉ tiền, hay cả vật phẩm và pet?
- Thư có quà chưa nhận thì có được xoá không?
