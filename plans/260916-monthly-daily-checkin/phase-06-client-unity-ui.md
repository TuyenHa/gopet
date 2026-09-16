# Phase 06 — Client Unity: Icon sự kiện → tab điểm danh + list

## Context
- Router: `Assets/Scripts/Net/MessageRouter.cs` — `RegisterSub(COMMAND_GUIDER, subType, handler)` (dòng 63-81).
- Handler guider hiện có: `Assets/Scripts/Net/Guider/GuiderHandler.cs`, gói định nghĩa ở `GuiderPackets.cs`/`GuiderDialogs.cs`.
- View dựng bằng code (không prefab): mẫu `ShopPopupView`, `TranChanTabsView`, `ChoiceDialogView`, `InputDialogView` (vừa sửa). UI helper: `Assets/Scripts/Runtime/UI/UiBuilder.cs`. Root quản popup: `UiRoot.cs` (Push/Close/DialogStack).
- HUD/nút thế giới: `Assets/Scripts/Runtime/World/` (CurrencyBar, MapScene...).

## Overview
- **Priority**: cao (mặt tiền tính năng).
- **Status**: chưa làm.
- Thêm icon sự kiện trên HUD → mở popup có tab "Điểm danh" → nút "Điểm danh" + lưới 31 ngày (đã nhận/claim/mất/khoá). Style tối thiểu, user chỉnh sau.

## Requirements
### 1) Model + parse gói STATE (phase-05)
- `Assets/Scripts/Net/DailyCheckin/DailyCheckinState.cs`: `int TodayDay, ReceivedMask, DaysInMonth; DayEntry[] Days` với `DayEntry { byte State; string Label; int IconItemId; }`.
- `DailyCheckinHandler.cs`: `RegisterSub(COMMAND_GUIDER, TYPE_DAILY_CHECKIN_STATE, ReadState)` → parse → raise event `StateReceived(DailyCheckinState)`.
- Gửi request: `SendOpen()` (TYPE_DAILY_CHECKIN_OPEN), `SendDoCheckin()` (TYPE_DAILY_CHECKIN_DO) qua writer giống các handler khác.

### 2) View — LẤY STYLE TỪ `ShopPopupView` (popup cửa hàng)
- `Assets/Scripts/Runtime/UI/DailyCheckinView.cs` (giữ <200 dòng; tách layout sang file phụ nếu cần).
- **Tái dùng đúng style tokens của `ShopPopupView.cs`** (dòng 36-56): panel giữa màn hình `RoundedUiSprite.Apply` + `PanelBg` trắng, viền `PanelBorder` xanh; tab `TabActive` vàng / `TabInactive` xanh + `TabText`; nút X `CloseSize` chờm góc trên-phải; kích thước tham chiếu canvas 720×1280, popup ~380×220 (điều chỉnh cao hơn chút cho lưới ngày, nhưng vẫn < ~430 ref-unit chiều cao khả kiến landscape — xem cảnh báo ShopPopupView.cs:33-35).
  - Backdrop + panel neo giữa (như ShopPopupView, không phải sidebar).
  - Header/tab "Điểm danh" style y hệt tab shop (cho phép thêm tab khác sau — 1 tab lúc này).
  - Lưới ngày (grid ~7 cột): mỗi ô = số ngày + icon (theo `IconItemId`) + label + trạng thái màu:
    - RECEIVED: mờ + dấu tick. CLAIMABLE: nổi bật/viền sáng. MISSED: xám tối. LOCKED: khoá mờ.
  - Nút **"Điểm danh"**: enable khi hôm nay CLAIMABLE; bấm → `SendDoCheckin()`. Sau khi server trả STATE mới → `Bind` lại lưới.
  - Nút X đóng (như `InputDialogView`).
- Icon item: dùng `RemoteAssetCache`/loader ảnh item hiện có của client (theo cách shop/inventory load icon `items/{id}.png`).

### 3) Icon sự kiện trên HUD
- Thêm nút "Sự kiện" (icon) vào HUD thế giới (`MapScene`/HUD builder). Bấm → `_ui`/UiRoot mở `DailyCheckinView` + gọi `SendOpen()`.
- Nếu HUD chưa có chỗ, đặt tạm cạnh `CurrencyBar`/nút menu; user chỉnh vị trí sau.

### 4) Nối vào UiRoot
- `UiRoot`: hàm `OpenDailyCheckin()` → tạo `DailyCheckinView`, subscribe `StateReceived` để bind, `Closed → Close(view)`, `Push`. Gọi `SendOpen()` sau khi mở (giống `ShowTranChanTabs` tự load list — UiRoot.cs:220-233).

## Related Code Files
- Tạo: `Net/DailyCheckin/DailyCheckinState.cs`, `Net/DailyCheckin/DailyCheckinHandler.cs`, `Runtime/UI/DailyCheckinView.cs` (+ `.Layout.cs` nếu cần).
- Sửa: `Net/GopetCmd.cs` (thêm hằng TYPE_* khớp server), đăng ký handler ở nơi khởi tạo router (giống GuiderHandler được wire), `UiRoot.cs` (OpenDailyCheckin), HUD builder (nút icon).

## Todo
- [ ] Hằng TYPE_* client khớp server.
- [ ] `DailyCheckinState` + `DailyCheckinHandler` (parse + gửi request) + đăng ký RegisterSub.
- [ ] `DailyCheckinView` (grid + nút điểm danh + đóng).
- [ ] `UiRoot.OpenDailyCheckin` + nút icon sự kiện HUD.
- [ ] Chạy client, mở tab, điểm danh thử, xem lưới cập nhật.

## Success Criteria
- Bấm icon sự kiện → popup mở, gọi OPEN, lưới hiện đúng 28-31 ô theo tháng.
- Ô hôm nay CLAIMABLE, nút "Điểm danh" bấm được → nhận quà, ô chuyển RECEIVED, nút disable.
- Ngày đã qua chưa nhận hiện MISSED, ngày tương lai LOCKED.

## Risk
- Client dựng UI bằng code + neo sai = "rộng 0px" (bài học `UiBuilder`/`InputDialogView`). Bám `PlaceRow`/`Stretch`.
- Load icon item theo id: tái dùng đúng loader hiện có, tránh CSP/asset path sai.

## Đã chốt
- Popup điểm danh **lấy style từ `ShopPopupView`** (popup cửa hàng): cùng bảng màu, panel bo góc, kiểu tab, nút X.
- Icon 2 hộp: dùng `items/240024.png`/`240025.png` (tạo ở phase-08), load qua loader icon item sẵn có.

## Unresolved
- Vị trí icon sự kiện trên HUD (user chỉnh giao diện sau).
- Có cần animation/hiệu ứng nhận quà không (giai đoạn sau).
