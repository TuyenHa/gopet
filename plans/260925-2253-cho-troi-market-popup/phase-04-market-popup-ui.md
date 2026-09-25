---
phase: 4
title: Market popup UI
status: completed
priority: P1
effort: 7h
dependencies:
  - 2
---

# Phase 4: Market popup UI

## Overview
Client nhận/gửi packet market (phase 2) và dựng popup "Chợ trời" giống style ShopPopupView, gồm 2 tab: **Chợ** và **Gian hàng của tôi**. Popup đăng bán thuộc phase 5.

## Key Insights (scout)
- Toàn bộ UI dựng bằng code, không có prefab.
- Bộ khung dùng chung:
  - `GamePopupFrame.Create(parent, font, title, footer)`: `Content`, `ContentWidth`, `Closed`. Chiều cao phải ≤ ~405 đơn vị tham chiếu ở landscape.
  - `PopupTabRail.Create(content, font, width, labels)`: event `Selected(int)`.
  - `PopupItemList`: panel trắng bo góc + `RectMask2D` + `ScrollRect`. `Rows`/`SetRowsHeight` cho row tuỳ biến (xem `MailboxView.List.cs`).
- Mẫu mở popup: `UiRoot.OpenShopPopup()` (`UiRoot.cs:155-166`). Khi đóng phải clear field trong `UiRoot.Close()` (`:404-421`). `ShowConfirm` (`:375-395`) và `ShowToast` (`:194-197`) có sẵn.
- Mẫu packet client (commit 65551c1):
  - `GuiderPackets.cs:150-171`.
  - `GuiderHandler` gọi `RegisterSub(COMMAND_GUIDER, sub, m => Event?.Invoke(Model.Parse(m)))`.
  - Model C# thuần, parse xong gọi `r.ExpectFullyConsumed("NAME")`.
- Nạp icon item: `_assets.Get(imagePath, ImagePackets.TypeIcon, rawImage, cb)` (`InventoryGridView.cs:124`).
- **Độ nét** (user yêu cầu viền popup và viền item phải sắc):
  - Canvas đã `pixelPerfect = true` (`GopetBootstrap.Unity.cs:37`).
  - Dùng `RoundedBorder` 2 lớp, **không dùng `Outline`** (`InventoryGridView.cs:93`, `BlacksmithRepairPopupView.cs:17`).
  - Mọi kích thước/toạ độ là số nguyên chẵn, để popup căn giữa không rơi vào nửa đơn vị.
  - Dùng `GamePopupFrame.CreateCloseButton` cho nút X.

## Requirements
- **Tab Chợ:** (món chỉ định cho người khác đã bị server ẩn; món chỉ định cho mình hiện nhãn "Chỉ định cho bạn")
  - Bộ lọc 7 chip: Tất cả / Vũ khí / Giáp / Mũ / Ngọc / Pet / Vật phẩm.
  - Nút sắp xếp xoay vòng "Mới nhất → Giá ↑ → Giá ↓".
  - Danh sách 5 hàng/trang. Mỗi hàng có ảnh, tên (kèm "x{count}" nếu > 1), giá (icon ngọc + số có dấu chấm ngăn nghìn), "Người bán: {tên}", nút **Mua** bên phải. Row `isMine` → **không có nút Mua**, người bán hiển thị "Người bán: Bạn" (giữ chỗ trống bên phải để hàng thẳng cột).
  - Giữa các hàng có đường gạch ngang 2px màu `PopupPalette.Border`.
  - Pager ở dưới gồm icon ◀, "Trang x/y", icon ▶. Nút mờ khi ở trang đầu/cuối.
- **Mua:**
  - Hiện confirm "Mua {tên} với giá {giá} ngọc?".
  - Bấm có thì gửi 49. Server trả RESULT thì hiện toast và list tự refresh.
  - Nếu không đủ ngọc (so với coin hiện có), hiện toast luôn, không gửi packet.
- **Tab Gian hàng của tôi:**
  - Danh sách cuộn, không phân trang, cùng row template. Thay "Người bán" bằng "Còn lại: {hh}h{mm}" (+ " · Chỉ bán cho: {tên}" nếu có). Bên phải 2 nút nhỏ **Chỉ định** và **Gỡ**. Gỡ có confirm.
  - **Chỉ định** (user chốt: chỉ làm sau khi đăng): mở `PopupInputForm` "Nhập tên người mua (để trống để bỏ chỉ định)" + dòng phí "Phí: 15.000 vàng (pet) / 10.000 vàng (đồ)" → gửi 57; RESULT action 4 → toast.
  - Footer có nút **Đăng bán** (GameButtonSkin) mở popup phase 5.
  - Danh sách trống thì hiện placeholder "Bạn chưa treo món nào".
- Mở popup: tab Chợ, filter Tất cả, sort Mới nhất, trang 0. Hiện "Đang tải…" đến khi nhận được state.

## Architecture
```
HUD MarketClicked ─► UiRoot.OpenMarketPopup ─► MarketPopupView
MarketPopupView ──(MarketPackets.List/Buy/Mine/Cancel)──► GopetClient.Send
MarketHandler (RegisterSub 48/51/54/56) ──events──► UiRoot.Market ──► view.Bind*/toast
```
- Popup 520×360: frame → tab rail → vùng nội dung. Vùng nội dung dùng 2 panel con và bật/tắt theo tab.
- Chợ: `MarketFilterBar` (26 cao) + nút sort ở góc phải → `PopupItemList` (5 × 46 = 230) → `MarketPagerBar` (28).
- Hàng: `MarketListingRowView.Create(parent, font, assets, width, listing, actionLabel, onAction)` dùng chung cho cả 2 tab, `sellerOrTimeText` do caller truyền vào.

## Related Code Files
- Create (Net): `GopetUnityClient\Assets\Scripts\Net\Market\MarketListing.cs` (Row + Parse), `MarketListState.cs`, `MarketSellableItem.cs` (dùng ở phase 5), `MarketResult.cs`, `MarketPackets.cs` (List/Buy/Mine/Cancel/Sellable/Sell builders), `MarketHandler.cs` (RegisterSub + events)
- Modify: `GopetUnityClient\Assets\Scripts\Runtime\World\GameSession.cs` (đăng ký `MarketHandler` cạnh `KioskHandler`, ~`:333-338`)
- Create (UI):
  - `Runtime\UI\MarketPopupView.cs` (tạo frame/tab, API Bind)
  - `MarketPopupView.MarketTab.cs`
  - `MarketPopupView.MineTab.cs`
  - `MarketListingRowView.cs`
  - `MarketFilterBar.cs`
  - `MarketPagerBar.cs`
- Create: `Runtime\UI\UiRoot.Market.cs` (partial: `_marketPopup`, `OpenMarketPopup`, forward events từ handler, xử lý RESULT → toast + refresh)
- Modify: `Runtime\UI\UiRoot.cs` (clear `_marketPopup` trong `Close()`)
- Asset: icon ◀ ▶. Nếu có sprite mũi tên sẵn trong `Resources/Ui` thì dùng lại; nếu chưa có thì tạo `Resources/Ui/Hud/arrow-left.png`/`arrow-right.png` bằng image-gen (cùng script phase 3, thêm tham số) hoặc vẽ glyph "◀ ▶" trong nút GameButtonSkin nhỏ.

## Implementation Steps
1. Model + parser + packets cho sub 47..56, theo mẫu `BattleSceneState`/`GuiderPackets`. `MarketHandler` phát các event `ListReceived`, `MineReceived`, `SellableReceived`, `ResultReceived`.
2. `MarketListingRowView`:
   - Icon 36×36 trong khung `RoundedBorder` (không Outline). Tên đậm 12. Dòng 2 là giá (icon `HudSkin` coin + số). Dòng 3 là người bán/thời gian.
   - Nút phải dùng GameButtonSkin, rộng 64.
   - Đường kẻ 2px ở đáy. Hàng cuối trang không có kẻ.
3. `MarketFilterBar`:
   - 7 chip tự co, chip chọn màu vàng như PopupTabRail. Có thể dùng lại `PopupTabRail` nếu chiều rộng vừa. Kiểm tra: 7 chip trong ~420 đơn vị.
   - Event `FilterChanged(sbyte)`. Map chip → filter: Tất cả -1, Vũ khí 1, Giáp 2, Mũ 0, Ngọc 3, Pet 4, Vật phẩm 5.
4. `MarketPagerBar`: nút ◀/▶, text "Trang {page+1}/{max(1,total)}", event `PageRequested(int)`, `Interactable` theo biên.
5. `MarketPopupView`:
   - Đổi filter/sort thì về trang 0 và gửi 47. Đổi trang thì gửi 47.
   - Chuyển sang tab 2 thì gửi 50.
   - Hàm `BindList(MarketListState)` / `BindMine(...)` rebuild các hàng, bỏ qua state cũ nếu filter/sort/page không khớp lựa chọn hiện tại.
6. `UiRoot.Market.cs`:
   - `OpenMarketPopup()`: tạo view, `Push`, wire `Closed`/`ConfirmRequested`/`Message`, gửi 47 mặc định.
   - `ResultReceived` → `ShowToast(msg)`. Server đã tự gửi state mới, view chỉ cần bind.
7. Unity compile, chạy thử với server local: mở/đóng, lọc, sort, phân trang, mua, gỡ.
8. Chụp màn hình ở 16:9 để xác nhận viền popup và viền item sắc (zoom 200%, không nhòe, không lệch nửa pixel).

## Success Criteria
- [ ] Popup mở từ HUD ở map bất kỳ, style đồng bộ ShopPopupView (khung, badge tiêu đề, nút X, tab rail).
- [ ] Lọc 7 loại, sort 3 chế độ, phân trang đúng `totalPages`. Nút ◀/▶ khoá ở biên.
- [ ] Mỗi hàng có ảnh, tên, giá, người bán, nút Mua bên phải và gạch ngang ngăn cách.
- [ ] Mua thành công thì hiện toast, món biến mất khỏi list, ngọc bị trừ. Khi lỗi (hết hàng, thiếu ngọc) thì toast đúng thông báo.
- [ ] Tab Gian hàng hiện đồ đang treo cùng thời gian còn lại. Gỡ thì đồ về túi.
- [ ] Viền popup/item sắc nét trên screenshot. Unity compile 0 error. Mỗi file < 200 dòng.

## Risk Assessment
- 7 chip không đủ chỗ: rút gọn nhãn (Tất cả → "Tất cả", Vật phẩm → "Vật phẩm" cỡ chữ 10) hoặc cho rail cuộn ngang.
- State đến trễ khi người dùng đã đổi filter: so khớp filter/sort/page trước khi bind.
- Icon remote chưa tải xong: dùng placeholder giống ShopItemRow.

## Security Considerations
- Client chỉ gửi id. Giá và quyền sở hữu do server quyết định. Kiểm tra "đủ ngọc" ở client chỉ để UX tốt hơn.
