---
phase: 5
title: Sell popup UI
status: completed
priority: P1
effort: 5h
dependencies:
  - 4
---

# Phase 5: Sell popup UI

## Overview
Popup "Đăng bán" mở từ tab Gian hàng của tôi, chia 2 cột:
- **Trái:** danh sách đồ trong rương có thể đăng bán (trang bị, ngọc, vật phẩm, pet).
- **Phải:** ảnh + thông tin món đang chọn, ô **giá** (ngọc), ô **số lượng** (khi món có số lượng > 1), nút **Đăng bán** ở dưới.

Đăng thành công thì hiện thông báo "Đăng bán vật phẩm thành công". Lỗi thì hiện thông báo lỗi do server trả về.

## Key Insights (scout)
- Mẫu 2 cột có sẵn là `Runtime\UI\MailboxView.cs:30-89`: `GamePopupFrame` 520×350, `PopupItemList` bên trái chiếm `ListWidthFraction=0.44`, `SplitGap=8`, pane chi tiết bên phải (`LetterDetailPane.cs`) có các nút ở đáy.
- Ô nhập dùng `PopupField.Create(parent, font, label, top, height, characterLimit)` (`PopupField.cs:28`), đặt `contentType = IntegerNumber`. Lưu ý bàn phím Telex nuốt phím khi test tay (`InputDialogView.cs:21-24`).
- Client không có cờ khóa. Server gửi `tradable` + `blockReason` trong SellableRow (phase 2).
- Nút dùng `GameButtonSkin.Apply` / `PopupButtonRow`.

## Requirements
- Mở popup thì gửi 53 và hiện "Đang tải…" đến khi nhận 54.
- Cột trái:
  - Mỗi hàng có icon, tên, "x{count}".
  - Món không bán được hiển thị mờ (alpha 0.5) kèm nhãn "Khóa".
  - **Click vào món đang khóa** (user chốt 2026-09-25): hiện toast "Vật phẩm đang khóa, bạn không thể bán được.", không chọn món đó và pane phải giữ nguyên. Các lý do chặn khác (đang mặc cho pet, có ngọc khảm, pet thử) cũng hiện toast `blockReason` tương ứng.
  - Có tab nhỏ lọc nguồn: Tất cả / Trang bị / Ngọc / Vật phẩm / Pet. Dùng lại `MarketFilterBar` nếu hợp, nếu không thì bỏ (YAGNI) và chỉ dùng một list sắp theo loại.
- Cột phải:
  - Icon 48×48 trong khung sắc, tên, mô tả (cuộn nếu dài).
  - Ô "Giá bán (ngọc)" chỉ nhận số, 1..2.000.000.000.
  - Ô "Số lượng" chỉ hiện khi count > 1, mặc định bằng count, giới hạn 1..count.
  - Dòng gợi ý "Thực nhận: {95%} ngọc (thuế 5%)".
  - Nút **Đăng bán** bị disable khi chưa chọn món, món bị khóa, hoặc giá không hợp lệ.
- Bấm Đăng bán thì gửi 55 (source, id, count, price) và khóa nút đến khi nhận 56.
  - `ok=true`: toast "Đăng bán vật phẩm thành công", đóng popup đăng bán, tab Gian hàng tự refresh (server đã gửi 51).
  - `ok=false`: hiện message lỗi (toast, hoặc dòng đỏ trong pane phải) và giữ popup mở.
- Sau khi thành công, danh sách trái cũng refresh (gửi lại 53) nếu người dùng mở lại popup.

## Architecture
```
MarketPopupView (tab Mine) ─Đăng bán─► UiRoot.OpenMarketSellPopup ─► MarketSellPopupView
MarketSellPopupView ─53/55─► server ─54/56─► MarketHandler ─► UiRoot.Market ─► view.BindSellable / OnSellResult
```
- `MarketSellPopupView` được push trên stack, nằm trên popup Chợ trời. Đóng nó thì quay lại popup Chợ trời.
- Pane phải tách thành `MarketSellDetailPane` để file < 200 dòng.

## Related Code Files
- Create: `GopetUnityClient\Assets\Scripts\Runtime\UI\MarketSellPopupView.cs` (frame 520×360 + list trái + nối pane)
- Create: `GopetUnityClient\Assets\Scripts\Runtime\UI\MarketSellDetailPane.cs` (ảnh, thông tin, 2 ô nhập, dòng thực nhận, nút Đăng bán, dòng lỗi)
- Modify: `GopetUnityClient\Assets\Scripts\Runtime\UI\UiRoot.Market.cs` (`OpenMarketSellPopup`, route SellableReceived / ResultReceived(action=3), clear khi Close)
- Modify: `GopetUnityClient\Assets\Scripts\Runtime\UI\MarketPopupView.MineTab.cs` (event `SellRequested`)
- Reuse: `MarketListingRowView`, hoặc một row gọn hơn cho cột trái nếu row market quá cao (40 thay vì 46)

## Implementation Steps
1. Dựng `MarketSellPopupView` theo layout MailboxView: tiêu đề "Đăng bán", list trái chiếm 0.44, pane phải.
2. `BindSellable(List<MarketSellableItem>)`: tạo các hàng, hàng không tradable thì mờ. Click hàng tradable thì `detail.Show(item)`; click hàng không tradable thì `Message?.Invoke(item.BlockReason)` (toast), không đổi pane.
3. `MarketSellDetailPane`:
   - Tạo 2 `PopupField` (IntegerNumber). Validate trên `onValueChanged` và cập nhật "Thực nhận". Dùng `long` để tránh tràn số khi tính.
4. Nút Đăng bán gửi `MarketPackets.Sell(source, id, count, price)`, sau đó `SetBusy(true)`.
5. Trong `UiRoot.Market`, khi `ResultReceived` có action=3:
   - ok thì toast, đóng popup đăng bán.
   - lỗi thì gọi `detail.ShowError(msg)` + toast, rồi `SetBusy(false)`.
6. Compile. Test tay:
   - Bán trang bị, ngọc, vật phẩm có số lượng (bán 5/50), và pet.
   - Thử đồ khóa, giá 0, giá > 2e9, số lượng > đang có.
7. Chụp màn hình kiểm tra độ nét của viền pane, khung icon và ô nhập.

## Success Criteria
- [ ] Popup 2 cột đúng yêu cầu. Chọn món thì pane phải hiện ảnh, thông tin, ô giá (và số lượng nếu cần).
- [ ] Click đồ khóa hiện toast "Vật phẩm đang khóa, bạn không thể bán được." và không chọn được.
- [ ] Đăng thành công thì toast "Đăng bán vật phẩm thành công", món xuất hiện trong Gian hàng, biến mất/giảm số lượng trong rương.
- [ ] Lỗi server thì hiện đúng thông báo và popup vẫn mở.
- [ ] Unity compile 0 error, mỗi file < 200 dòng, viền sắc nét.

## Risk Assessment
- Rương có nhiều món (hàng trăm): `PopupItemList` không ảo hoá. Chấp nhận được với khoảng 200 món. Nếu lag thì chuyển sang `MenuVirtualizer`.
- Bàn phím mobile che ô nhập: đặt các ô ở nửa trên pane phải.
- Người dùng bấm Đăng bán nhiều lần liên tiếp: `SetBusy` khóa nút. Server vẫn validate lại số lượng còn trong túi.

## Security Considerations
- Client validate chỉ để UX tốt hơn. Server (phase 1 `TryList`) là nơi quyết định cuối cùng: khóa, số lượng, giá, quyền sở hữu.
