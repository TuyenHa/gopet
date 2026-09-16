# Điểm danh theo tháng (Monthly Daily Check-in)

## Mục tiêu
Thay cơ chế điểm danh Noel (7 mốc streak) bằng **event điểm danh theo tháng**: mỗi ngày dương lịch điểm danh 1 lần, quà gắn theo ngày (1→31), reset đầu tháng. Client: icon sự kiện → tab điểm danh → nút "Điểm danh" + lưới 31 ngày hiển thị quà đã nhận / chưa nhận / chưa tới ngày. Ngày 28 & 29 phát **hộp quà bí ẩn** (item mới, nhận vào túi, người chơi tự mở sau → random loot).

## Phạm vi & quyết định thiết kế
- **Event mới độc lập**, KHÔNG sửa `TeacherDay2024`/Noel cũ. Theo pattern `EventBase` (`Data/Event/...`).
- **Calendar thuần**: lỡ ngày = mất quà ngày đó (không lấy bù). Đã chốt với user.
- **Reset tự động** theo tháng qua so `DailyCheckinMonth`.
- **Loot hộp thuần item** (dùng `GIFT_RANDOM_ITEM` có sẵn, không random ngọc/skin).
- UI target: **Unity client** (`GopetUnityClient`). Jar cũ ngoài phạm vi (nếu cần, dùng NPC menu server-driven).
- Popup điểm danh **lấy style từ `ShopPopupView`** (popup cửa hàng). Giao diện chi tiết user chỉnh sau.
- 2 hộp `canTrade=0` (khoá). Ngày 31 chỉ ngọc (chưa gắn danh hiệu). Icon 2 hộp riêng, tạo bằng image-gen (phase-08).

## Các phase
| # | Phase | Trạng thái |
|---|-------|-----------|
| 01 | [Server: data quà 31 ngày + 2 hộp bí ẩn + item rows](phase-01-server-data-config.md) | ✅ Xong |
| 02 | [Server: PlayerData state + DB migration](phase-02-server-state-migration.md) | ✅ Xong (migration đã chạy trên `gopettae_tae2` 2026-09-16) |
| 03 | [Server: logic điểm danh (calendar + reset tháng)](phase-03-server-checkin-logic.md) | ✅ Xong |
| 04 | [Server: hộp quà bí ẩn UseItem + loot random](phase-04-server-mystery-box.md) | ✅ Xong |
| 05 | [Server: packet gửi trạng thái tháng cho client](phase-05-server-packet-protocol.md) | ✅ Xong |
| 06 | [Client Unity: icon sự kiện → tab điểm danh + list](phase-06-client-unity-ui.md) | ✅ Xong |
| 07 | [Tests: server logic + client render](phase-07-tests.md) | ✅ Client 703/703; server test tay |
| 08 | [Ảnh icon 2 hộp quà bí ẩn (image-gen)](phase-08-box-icons-imagegen.md) | ✅ Xong |

## Trạng thái build (2026-09-16)
- Server GServer: build 0 error (build ra temp vì exe đang chạy khoá output).
- Client Unity: PlayMode compile 0 error; net-test 703/703 pass (thêm 5 test DailyCheckin).
- **Migration DB: ✅ ĐÃ CHẠY (2026-09-16)** lên schema `gopettae_tae2` (không phải `game` như comment trong file .sql). Đã thêm 2 cột `player` (`DailyCheckinMask`, `DailyCheckinMonthKey`) + 2 item hộp (240024, 240025). Xác minh OK.
- **CÒN LẠI:** không còn blocker DB. Chỉ cần restart server để load data/item mới.

## Phụ thuộc
- 02 → 03 (logic cần state). 01 → 03/04 (logic cần data). 03/05 → 06 (client cần packet). 04 độc lập sau 01/02. 08 song song (chỉ cần trước khi test hiển thị icon).
- Thứ tự đề xuất: 01 → 02 → 03 → 04 → 05 → 06 → 07; 08 làm song song bất kỳ lúc nào.

## Bảng quà tham chiếu (chốt ở các câu trước)
- Ngày thường: bình EXP/máu/mana, nhân sâm, đá cường hoá, ngọc ghép (kim cương/lam ngọc/huyết ngọc), mực xăm, thẻ kĩ năng.
- Mốc tuần 7/14/21: kim cương + mực xăm + (ngọc ở ngày 21).
- Ngày 28: **Hộp quà bí ẩn tuần 4** (item `240024`). Ngày 29: **Hộp quà bí ẩn cuối tháng** (item `240025`).
- Ngày 30/31: mực xăm cực hiếm, thẻ xăm hoà kì lân, danh hiệu/ngọc (tùy chỉnh).
- Chi tiết đầy đủ trong `phase-01`.

## Rủi ro chính
- DB migration cột mới trên bảng `player` đang chạy → cần ALTER an toàn, default 0.
- Chọn TYPE_ sub-command mới không trùng opcode `COMMAND_GUIDER` hiện có.
- Tỉ giá quà (ngọc) phải khớp kinh tế server — số trong plan là ước lượng.
