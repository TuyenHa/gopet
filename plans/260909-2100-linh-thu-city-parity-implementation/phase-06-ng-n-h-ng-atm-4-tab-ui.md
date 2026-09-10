---
phase: 6
title: Ngân hàng (ATM) 4-tab UI
status: completed
priority: P2
effort: 3-4d
dependencies:
  - 1
---

# Phase 6: Ngân hàng (ATM) 4-tab UI

> **DONE — Bank chạy được, không cần UI riêng.**
>
> **Đảo ngược kết luận cũ:** `GameController.requestBank()` là stub rỗng
> (line 2380), nhưng bank IS implemented — tại **`Player.cs:143`**, bắt opcode
> `CHARGE_MONEY_INFO (44)` **top-level** *trước* khi vào `controller.onMessage`
> và gọi `MenuController.sendMenu(MENU_ATM=1039)`.
>
> Server bơm menu 3 dòng qua Guider generic → Unity `GenericMenuView` render tự
> nhiên → user chọn → server bơm InputDialog nhập số → user submit → server đổi.
> **Toàn bộ luồng tận dụng hạ tầng Guider đã có.**
>
> **Đã tạo:**
> - `Assets/Scripts/Net/Bank/BankPackets.cs` — `OpenBankMenu()` trả opcode 44 (không body).
> - Menu char thêm mục "Ngân hàng" → dispatch qua `TryBuildServerMessage`.
> - Test `CharacterMenuTests.BankAction_Wire_44_KhongBody` verify byte layout.
>
> **Trap tránh được:** client J2ME jar dùng `cx.b(int)` = `en(81).a(44)` — opcode
> PET_SERVICE / sub 44, nhưng `processPet` **không** có case 44 → gói bị silently
> dropped. Client Unity dùng TOP-LEVEL 44 để Player.cs bắt được — port máy móc từ
> jar sẽ hỏng câm.
>
> **Ngoài phạm vi:** 4 tab UI (Nạp/Rút/Đổi/Chuyển) độc lập không cần dựng vì server
> điều hướng qua Guider. Nếu tương lai muốn UI thân thiện hơn cho từng thao tác,
> viết view riêng thay `ListOptionScreen` — nhưng không cần cho parity map hub.


## Overview

`fn.java` (235 dòng) + `br.java` (57 dòng) tạo UI Ngân hàng của jar với 4 nhóm thao tác: **Nạp**, **Đổi tỉ giá**, **Chuyển khoản**, **Xem số dư**. Menu char (Phase 2) mục "Chức năng khác..." → mở Ngân hàng qua opcode 81/101/1 (`dc.a(Object)` — `dc.java:2-10`).

Đây là UI có **state phức tạp** — 4 tiền tệ + 4 tab + validate input — nên tách riêng thay vì cố nhét vào `GenericMenuView`.

## Requirements

**Functional:**
- 4 tab: Nạp (Deposit) / Rút (Withdraw) / Đổi (Exchange) / Chuyển (Transfer).
- **Nạp**: Vàng → gửi 81/101/1, Đậu → 81/101/2, Ngọc → 81/101/(int) (theo `fn.java:200-224`).
- **Đổi**: Vàng ↔ Đậu / Vàng ↔ Thóc / Vàng ↔ Ngọc (br.java case 6/7/9).
- **Chuyển khoản**: chuyển tiền cho user khác qua SMS xác nhận (`fn.java:106-107`).
- **Xem số dư**: hiển thị mGold + Ngọc hiện có (đã có ở HUD từ Phase 1 — chỉ mirror).

**Non-functional:**
- Input số chấp nhận đến `int.MaxValue`.
- Xác nhận trước mỗi giao dịch (Có/Không dialog).
- Kết quả server (thành công/thất bại) hiển thị toast.

## Architecture

- Reuse gói opcode 81 (`PET_SERVICE`) — sub 101 cho ATM.
- `BankPackets` — 4 hàm gửi.
- `BankView` — 4 tab (Unity Toggle Group), mỗi tab một sub-panel.
- Sub-panel dùng chung `AmountInputField` (số + validation).

## Related Code Files

**Create:**
- `Assets/Scripts/Net/Bank/BankPackets.cs` — Deposit, Withdraw, Exchange, Transfer.
- `Assets/Scripts/Net/Bank/BankHandler.cs` — parse response (số dư mới, kết quả giao dịch).
- `Assets/Scripts/Runtime/UI/BankView.cs` — root view + tab controller.
- `Assets/Scripts/Runtime/UI/BankTabDeposit.cs`
- `Assets/Scripts/Runtime/UI/BankTabWithdraw.cs`
- `Assets/Scripts/Runtime/UI/BankTabExchange.cs` — 3 cặp đổi.
- `Assets/Scripts/Runtime/UI/BankTabTransfer.cs`
- `Assets/Scripts/Runtime/UI/AmountInputField.cs` — reuse component.

**Modify:**
- `Assets/Scripts/UiLogic/CharacterMenuActions.cs` (Phase 2) — thêm entry "Ngân hàng" mở `BankView`.
- `Assets/Scripts/Runtime/World/GameSession.cs` — register `BankHandler`.

**Read (context):**
- `client.jar_Decompiler.com/fn.java` — toàn bộ UI ATM.
- `client.jar_Decompiler.com/br.java` — enum thao tác.
- `client.jar_Decompiler.com/dc.java:1-10` — opcode ATM.
- `SRCGOPETGOC/GServer/Server/GameController.cs` — case 81 sub 101 (nạp/rút), sub các lệnh exchange.

## Implementation Steps

1. **Xác định wire format** — đọc `GameController` sub 101, xác nhận:
   - Field order: `sbyte type + int amount` cho Nạp/Rút.
   - Exchange có field khác — cần grep.
   - Transfer có `UTF recipient + int amount`.
2. **`BankPackets`**:
   ```csharp
   public static Message DepositGold(int amount) =>
       Message.Create(81).PutSByte(101).PutSByte(1).PutInt(amount);
   public static Message DepositDau(int amount) => ...
   public static Message Exchange(int fromCurrency, int toCurrency, int amount) => ...
   public static Message Transfer(string recipient, int amount) => ...
   ```
3. **`BankHandler`** — parse server trả về:
   - Update số dư mới → trigger `PlayerStatsHandler.StatsUpdated` (từ Phase 1).
   - Toast success/fail với lý do.
4. **`BankView`** — dùng `TabGroup` custom (nếu chưa có, viết mới trong `Runtime/UI/`).
5. **`AmountInputField`** — `InputField` với `contentType = IntegerNumber`, min=1, max=int.Max, disable Send khi rỗng.
6. **Xác nhận** — trước Submit gọi `DialogStack.PushYesNo("Nạp X vàng?", onYes)`.
7. **Toast kết quả** — dùng `ToastView` đã có.
8. **PlayModeTest**:
   - Bơm gói kết quả Nạp OK → verify số dư HUD update.
   - Bơm Fail → toast lỗi hiện.
   - Verify từng nút Deposit/Exchange gửi đúng byte.

## Success Criteria

- [ ] `BankView` mở từ menu char, 4 tab hiển thị đúng label.
- [ ] Nạp/Rút gửi byte khớp jar (verify diff).
- [ ] Đổi 3 cặp tỷ giá hoạt động — số dư client update sau khi server trả.
- [ ] Chuyển khoản gửi + hiện confirm SMS như jar.
- [ ] Fail (không đủ tiền, sai tên) hiển thị toast lỗi.
- [ ] Không file nào vượt 200 dòng (chia 4 tab riêng file).

## Risk Assessment

- **R1: Server nạp qua SMS thật** — jar gọi `dc.a(int,int)` (`dc.java:66-73`) opcode 46 — có thể trigger gửi SMS thật. Cần disable tab "Nạp mGold qua SMS" trong dev/staging, chỉ enable "Nạp từ ví game".
- **R2: Transfer scam** — thêm confirm 2 bước (nhập tên → confirm số tiền → confirm cuối) để giảm rủi ro.
- **R3: Đổi tỷ giá rate động** — server trả rate hiện tại; hiển thị rate ở tab Exchange, không hardcode.
- **R4: Race giữa Nạp response và HUD stats update** — `BankHandler` gọi thẳng `_stats.NotifyUpdated()` để tránh race với gói stats khác.

## Rollout Notes

- Ngân hàng là entry cho nhiều flow khác (kiosk, mua vật phẩm...) — làm chuẩn từ đầu để reuse.
- Test transfer với 2 tài khoản trước khi ship.
