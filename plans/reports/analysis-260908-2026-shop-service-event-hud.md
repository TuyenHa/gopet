# Phân tích: HUD Cửa hàng / Dịch vụ / Sự kiện (góc trên-phải)

**Ngữ cảnh:** Người dùng muốn thêm 3 icon HUD ở góc trên-phải (trước icon loa): **Cửa hàng**, **Dịch vụ**, **Sự kiện**. Khi bấm **Cửa hàng** hiện popup có 4 tab (**Vũ khí**, **Giáp**, **Mũ**, **Thức ăn**) — logic hệt jar. Bấm X đóng popup.

Ảnh mẫu: `c:\Users\hatuy\Downloads\OpenAI Playground 2026-09-07 at 23.58.04.png`.

---

## 1. Logic Cửa hàng bên jar (server-authoritative)

Client jar KHÔNG có shop popup nhiều tab. Mỗi shop mở riêng qua NPC. Nhưng backend data / packet flow đã có sẵn — ta tận dụng, chỉ đổi UI.

### 1.1 Shop type ID (server: `MenuController.cs:423-446`)

```
SHOP_WEAPON = 1   // Vũ khí
SHOP_ARMOUR = 2   // Giáp
SHOP_HAT    = 3   // Mũ (Nón)
SHOP_FOOD   = 4   // Thức ăn
SHOP_SKIN   = 7 ; SHOP_THUONG_NHAN = 6 ; SHOP_PET = 8 ; SHOP_ARENA = 9
SHOP_CLAN = 10 ; SHOP_ENERGY = 11 ; SHOP_GIAN_THUONG = 12 ; SHOP_BIRTHDAY_EVENT = 13
```

### 1.2 Data model shop item (`ShopTemplateItem.cs`)

Mỗi item gồm: `itemTemTempleId`, `count`, `moneyType[]` + `price[]` (mỗi item có thể có nhiều lựa chọn thanh toán vàng/ngọc/…), icon (`getIconPath()`), tên (`getName(player)`), mô tả (`getDesc(player)`), cờ `isSpceial`, `hasId`, `menuId`, `CloseScreenAfterClick`.

### 1.3 Luồng gói tin (jar-parity)

**Client → server:** `REQUEST_SHOP` = sub-cmd 2 trong envelope 122 (`COMMAND_GUIDER`? — thực tế sub-cmd 2 trong game-command; xem `GopetCmd.cs:79`) kèm 1 byte `shopId`.

**Server xử lý** (`GameController.cs:2357 requestShop`):
```csharp
switch (shopId) {
    case SHOP_ARMOUR: case SHOP_SKIN: case SHOP_WEAPON: case SHOP_HAT: case SHOP_FOOD:
        MenuController.sendMenu(shopId, player);  // đi tiếp
}
```

**`sendMenu(shopId, player)`** (`MenuController.sendMenu.cs:511-521`) dispatch shop-family sang `showShop`.

**`showShop(type, player)`** (`MenuController.cs:845-876`) build `List<MenuItemInfo>` từ `shopTemplate.getShopTemplateItems()`, mỗi dòng có:
- Tên, mô tả, đường dẫn icon.
- `paymentOptions[]` (vàng/ngọc/…), text `"500 (vang)"`, cờ `canPay=1` nếu đủ tiền.
- `showDialog=true`, `dialogText="Bạn có muốn mua nó?"`, `closeScreenAfterClick`.
- Gửi bằng `player.controller.showMenuItem(type, TYPE_MENU_PAYMENT, shopTemplate.getName(player), menuItemInfos)`.

**Server → client:** gói `SHOW_MENU_ITEM` (sub 8 trong envelope `COMMAND_GUIDER=122`) — chứa `listId=shopId`, tiêu đề, danh sách item info.

### 1.4 Chọn mua

Client bấm dòng → gửi `SELECT_MENU_ELEMENT` với `listId=shopId`, `echo=itemId` **và** kèm `paymentIndex` (đường `GUIDER_TYPE_PAY=9`, sub 2 mode `modeSelectWithPayment=2` — xem `GuiderPackets.SelectMenuElementWithPayment`).

---

## 2. Hạ tầng Unity client đã có sẵn (tận dụng tối đa)

**Đã có** — không cần viết lại:

| Layer | File | Vai trò |
|-------|------|--------|
| Packet parse | `Net/Guider/MenuScreen.cs`, `MenuItemInfo.cs` | Parse `SHOW_MENU_ITEM` thành `MenuScreen` (`Items[]` gồm tên, mô tả, icon path, payment options, cờ close). |
| Event bus | `Net/Guider/GuiderHandler.cs:27 MenuShown` | Bắn `MenuScreen` khi server gửi. Có sẵn `Select(screen, index, paymentIndex)` để gửi mua. |
| Render dòng | `Runtime/UI/MenuItemRow.cs`, `GenericMenuView.cs` | Đã dựng dòng có icon + tên + mô tả + xử lý confirm/payment. |
| Xử lý chọn | `UiLogic/MenuSelection.cs`, `MenuVirtualizer.cs` | Logic quyết định bấm dòng: Blocked / Confirm / Send. |
| Chồng dialog | `UiLogic/DialogStack.cs`, `Runtime/UI/UiRoot.cs` | Push/pop popup, Esc đóng, dialog đè menu. |
| Nút góc trên-phải | `Runtime/UI/SoundToggleButton.cs` | Neo anchor top-right, mẫu để bắt chước cho 3 icon HUD mới. |

**Chưa có** — phải viết:

1. **`REQUEST_SHOP` packet** (client → server). `GopetCmd.REQUEST_SHOP=2` đã declare, nhưng chưa có helper build message. Cần thêm vào `GuiderPackets.cs` (hoặc `GameControllerPackets.cs`) với đúng envelope. **Cần verify envelope**: server đọc `REQUEST_SHOP` trong `GameController.cs:1029` — envelope là gói game chính, KHÔNG phải `COMMAND_GUIDER`; verify bằng cách trace `readsbyte()` header.

2. **HUD bar 3 icon top-right** (`ShopServiceEventHud.cs`): 3 nút anchor top-right, cách nhau đều, cùng chiều với `SoundToggleButton` (xếp trước nó theo trục X). Mỗi nút = icon (Image) + label chữ ("CỬA HÀNG" / "DỊCH VỤ" / "SỰ KIỆN"). Icon lấy từ asset (chưa có → tạm dùng emoji-fallback như `SoundToggleButton` khi chưa có sprite).

3. **`ShopTabbedPopupView`** (`Runtime/UI/ShopTabbedPopupView.cs`):
   - Frame trắng bo góc + shadow (giống mẫu).
   - Hàng tab: 4 nút (Vũ khí / Giáp / Mũ / Thức ăn) — tab active vàng, inactive xanh.
   - Nút X close (góc trên-phải popup, chồng lên hàng tab).
   - Vùng danh sách item: dùng thẳng `GenericMenuView` embedded (fill vùng dưới tab).
   - Khi đổi tab → gửi `REQUEST_SHOP(shopId)` mới, chờ `MenuShown` → replace nội dung `GenericMenuView`.

4. **Chặn `UiRoot.ShowMenu`** khi popup shop đang mở. Cách clean: `ShopTabbedPopupView` subscribe `GuiderHandler.MenuShown` trực tiếp và **swallow** event nếu `listId` ∈ {SHOP_WEAPON, SHOP_ARMOUR, SHOP_HAT, SHOP_FOOD}. `UiRoot` phải kiểm cờ "shop popup đang mở" trước khi dựng `GenericMenuView` — nếu có thì bỏ qua. Hoặc đơn giản hơn: mở popup rồi self-manage, và `UiRoot` vẫn dựng thêm `GenericMenuView` → không được (2 view chồng nhau).
   - **Đề xuất:** Đưa "shop menu router" thành cờ boolean trong `UiRoot` (hoặc event `MenuShownIntercept` với `handled`): `ShopTabbedPopupView` set khi mở, unset khi đóng.

5. **Dịch vụ / Sự kiện**: chưa biết cụ thể user muốn logic gì. Mẫu chỉ show mock (Gym / Magic / ATM cho Dịch vụ). Cần user confirm scope: (a) chỉ dựng UI 3 tab tương tự, hay (b) hook thẳng vào NPC service của server?

---

## 3. Đề xuất phương án (KISS)

**Ưu tiên bám hạ tầng có sẵn — không rẽ nhánh nghiệp vụ mới:**

1. **HUD bar**: 3 nút anchor top-right, offset X trước `SoundToggleButton` — mỗi nút wire event `Clicked`.

2. **Shop click**:
   - Open `ShopTabbedPopupView` (push vào `DialogStack` như dialog khác).
   - Tab đầu (Vũ khí) mặc định active → gửi `REQUEST_SHOP(SHOP_WEAPON)`.
   - Popup tự subscribe `MenuShown` khi mở, filter theo listId ∈ shop set, hydrate `GenericMenuView` bên trong.

3. **Đóng popup**: X hoặc Esc → pop khỏi `DialogStack`, unsubscribe `MenuShown`.

4. **Dịch vụ / Sự kiện**: PHASE 2 (chưa làm ngay). Trong phase này chỉ dựng nút, click hiện toast "sắp có" — user confirm sau.

**Files sẽ tạo/sửa:**

| File | Thay đổi |
|------|---------|
| `Net/Guider/GuiderPackets.cs` (sửa) | Thêm `RequestShop(shopId)` |
| `Net/Guider/GuiderHandler.cs` (sửa) | Public API `RequestShop(sbyte type)` |
| `Runtime/UI/ShopServiceEventHud.cs` (mới) | HUD bar 3 nút top-right |
| `Runtime/UI/ShopTabbedPopupView.cs` (mới) | Popup 4 tab + close + embed `GenericMenuView` |
| `Runtime/UI/UiRoot.cs` (sửa) | Cờ suppress `ShowMenu` cho shop listId khi popup mở |
| `Runtime/GopetBootstrap.cs` (sửa) | Dựng `ShopServiceEventHud` sau `SoundToggleButton`, wire click → `UiRoot` mở popup |

---

## 4. Rủi ro & mở

- **Envelope của `REQUEST_SHOP`**: cần trace `GameController.cs` phần đọc header để biết chính xác gửi qua kênh nào (COMMAND_GUIDER? hay opcode game khác?). Nếu sai, server drop packet. **→ Cần xác thực trước khi code.**
- **Icon cho HUD**: chưa có sprite trong assets, sẽ fallback về chữ giống `SoundToggleButton` khi sprite null. User có muốn tôi tạo icon tạm (procedural / SVG-to-sprite) hay chấp nhận fallback text?
- **Dịch vụ / Sự kiện**: chưa rõ scope. Phase này chỉ để nút placeholder — cần user confirm bước tiếp theo.
- **Icon shop item** (Kiếm sắt, Rìu đồng…): server gửi `iconPath` (VD `"npcs/fone2.png"` hoặc path item). Client cần `RemoteAssetCache` load từ server — hạ tầng `MenuItemRow` đã dùng, không cần thêm.

---

## Câu hỏi còn mở

1. Icon cho HUD (Cửa hàng / Dịch vụ / Sự kiện) — dùng fallback text hay tôi tự vẽ tạm?
2. Nút **Dịch vụ** và **Sự kiện** phase này chỉ cần placeholder (chưa mở popup), đúng không?
3. Có muốn tôi verify envelope `REQUEST_SHOP` bằng cách chạy thử với server không, hay code trước theo assumption và fix nếu miss?
