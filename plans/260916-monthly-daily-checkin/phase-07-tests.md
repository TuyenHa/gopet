# Phase 07 — Tests: Server logic + client render

## Context
- Client Unity có test .NET chạy được ngoài Editor: `GopetUnityClient/tests/Gopet.Net.Tests` (698 test hiện pass), + `Gopet.PlayMode.Compile` (compile-check PlayMode). PlayMode test mẫu: `Assets/Tests/PlayMode/UiRootDialogTests.cs`.
- Server: kiểm chủ yếu bằng compile + test thủ công qua packet-dump (`GServer/bin/.../log/packet-dump-server.log`).

## Overview
- **Priority**: trung bình (chạy sau mỗi phase liên quan).
- **Status**: chưa làm.
- Bảo đảm logic điểm danh + parse/render client đúng, không hồi quy.

## Requirements
### Server (unit thuần, tách khỏi DB nếu được)
- `DoCheckin`:
  - Ngày X lần đầu → set bit X, phát đúng `DAILY_CHECKIN_GIFTS[X-1]`.
  - Gọi lại cùng ngày → bị chặn (không set thêm, báo đã điểm danh).
  - Đổi `monthKey` (giả lập sang tháng) → mask reset về 0.
  - Ngày < today chưa nhận → MISSED, không cho nhận.
- `GetDayState`: phủ 4 trạng thái LOCKED/CLAIMABLE/RECEIVED/MISSED.
- `OpenBox`: phân phối loot nằm trong pool; đúng tỉ lệ xấp xỉ (chạy N lần, jackpot ~5% ±). Trừ đúng 1 hộp/lần.
- `BuildLabel`: ghép nhãn quà đúng (tên item + count).
> Nếu logic phụ thuộc `Player`/DB nặng → tách phần thuần (mask/state/label) ra hàm static test được, giống cách client tách `ChoiceDialogLayout`.

### Client (PlayMode + net tests)
- Parse gói STATE: bơm wire giả (giống `TestPackets` trong UiRootDialogTests) → `DailyCheckinState` đúng số ngày/label/state.
- `DailyCheckinView.Bind(state)`: đúng số ô = daysInMonth; nút "Điểm danh" enable đúng khi có ngày CLAIMABLE; bấm → gửi đúng gói DO.
- Không hồi quy: chạy lại toàn bộ `Gopet.Net.Tests` + `Gopet.PlayMode.Compile`.

## Related Code Files
- Tạo: test server (theo khung test hiện có của GServer nếu có; nếu không, test thủ công + ghi checklist).
- Tạo: `GopetUnityClient/tests/Gopet.Net.Tests/DailyCheckinTests.cs`, `Assets/Tests/PlayMode/DailyCheckinViewTests.cs`.

## Todo
- [ ] Test server logic (mask/reset/state/label/loot).
- [ ] Test client parse + bind.
- [ ] `dotnet test` Gopet.Net.Tests xanh; `dotnet build` Gopet.PlayMode.Compile xanh.
- [ ] Test tay: điểm danh 2 ngày liên tiếp (đổi giờ máy), mở 2 hộp, xem loot + lưới.

## Success Criteria
- Toàn bộ test tự động xanh, không hồi quy 698 test cũ.
- Checklist test tay pass: điểm danh/chặn nhận lại/reset tháng/mở hộp.

## Risk
- Loot random test dễ flaky nếu assert tỉ lệ chặt → assert khoảng rộng hoặc seed RNG.
- PlayMode test chỉ chạy khi Editor đóng → luôn chạy compile-check trước.
