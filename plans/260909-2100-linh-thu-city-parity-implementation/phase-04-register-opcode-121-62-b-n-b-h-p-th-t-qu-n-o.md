---
phase: 4
title: Register opcode 121/62 + Bạn bè/Hộp thư/Tủ quần áo
status: completed
priority: P1
effort: 2-3d
dependencies:
  - 2
---

# Phase 4: Register opcode 121/62 + Bạn bè/Hộp thư/Tủ quần áo

## Overview

3 mảng dùng menu **server-driven** đã có `GenericMenuView` render sẵn — chỉ cần **register opcode gia đình 121 và 62** vào `MessageRouter`, và **wire nút gửi** (đã dựng khung ở Phase 2). Sau đó chạy được ngay.

Opcode 121 (`COMMAND_FRIEND`/`LETTER`?) trong jar dùng cho Bạn/Thư (`fr.java:441-521`). Opcode 62 dùng cho Tủ quần áo (`fr.java:346`).

## Requirements

**Functional:**
- Register envelope opcode 121 + các sub 1/2/10/13/4 (danh sách/yêu cầu/hộp thư/sổ đen/gửi thư).
- Register envelope opcode 62 (tủ quần áo char cosmetic).
- Bấm mục "Bạn bè" (Phase 2) → server trả menu → `GenericMenuView` hiện danh sách.
- Bấm "Hộp thư" → tương tự.
- Bấm "Tủ quần áo" → tương tự.
- Hành động trong list (Thêm bạn, Xoá bạn, Đọc thư, Gửi thư, Mặc/Cởi đồ) → gửi opcode con.

**Non-functional:**
- Handler thuần C#, không đụng Unity.
- Test được ngoài Editor.

## Architecture

- Xác định opcode 121 & 62 thuộc **họ envelope + sub** hay **top-level flat**. Đọc `GServer/Server/GameController.cs` case 121/62 để chốt.
- Nếu là envelope: `router.RegisterEnvelope(121)` + `router.RegisterSub(121, sub, handler)`. Nếu flat: `router.Register(121, handler)`.
- 3 file handler nhỏ, tránh gộp một file to.

## Related Code Files

**Create:**
- `Assets/Scripts/Net/Social/FriendPackets.cs` — hàm gửi (list, request, add, remove, block).
- `Assets/Scripts/Net/Social/FriendHandler.cs` — parse gói server trả về.
- `Assets/Scripts/Net/Social/MailPackets.cs` — hàm gửi (inbox, outbox, read, compose, delete).
- `Assets/Scripts/Net/Social/MailHandler.cs`.
- `Assets/Scripts/Net/Wardrobe/WardrobePackets.cs` — hàm gửi (open, wear, unwear).
- `Assets/Scripts/Net/Wardrobe/WardrobeHandler.cs`.

**Modify:**
- `Assets/Scripts/UiLogic/CharacterMenuActions.cs` (từ Phase 2) — wire các hàm gửi thật.
- `Assets/Scripts/Runtime/World/GameSession.cs` — khởi tạo + register 3 handler mới.
- `Assets/Scripts/Runtime/UI/GenericMenuView.cs` — không đổi, chỉ verify handle được menu 121/62.

**Read (context):**
- `client.jar_Decompiler.com/fr.java:437-531` — flow bấm dòng bạn/thư → gửi opcode nào.
- `client.jar_Decompiler.com/gw.java:70-108` — chuỗi label ("Danh sách bạn", "Hộp thư đến"...).
- `SRCGOPETGOC/GServer/Server/GameController.cs` — case 121, 62.
- `SRCGOPETGOC/GServer/Server/GopetCMD.cs` — hằng số opcode.

## Implementation Steps

1. **Xác định wire format 121/62** — đọc `GameController` case 121:
   ```
   grep -n "case 121\|case 62\|COMMAND_FRIEND\|COMMAND_LETTER" GServer/Server/GameController.cs
   ```
   Ghi vào comment header từng handler.
2. **`FriendPackets`** — dựa `fr.java:494-511`:
   - `RequestFriendList()` → 121/1
   - `RequestPendingRequests()` → 121/13
   - `RequestBlockList()` → 121/2
   - `AddFriend(string name)` → 121/4 + UTF
   - Còn lại theo dòng con menu server trả (server trả sub → client echo).
3. **`FriendHandler`** — parse menu trả về (dựa vào `MenuScreen` hay text-only?). Nếu server dùng `SHOW_MENU_ITEM` bọc trong 121 thì reuse `MenuScreen`. Nếu dùng format riêng thì parse.
4. **`MailPackets` + `MailHandler`** — tương tự, sub 10 mở inbox (`fr.java:508`).
5. **`WardrobePackets` + `WardrobeHandler`** — opcode 62 mở menu (`fr.java:346`); sub-command tra `GameController` case 62.
6. **Wire vào Phase-2 menu** — thay stub trong `CharacterMenuActions`:
   ```csharp
   public static void OpenFriends(GopetClient c) => c.Send(FriendPackets.RequestFriendList());
   public static void OpenMail(GopetClient c) => c.Send(MailPackets.OpenInbox());
   public static void OpenWardrobe(GopetClient c) => c.Send(WardrobePackets.Open());
   ```
7. **PlayModeTest** — bơm gói response server (dựng bằng tay từ dump jar) → verify `GenericMenuView` render đúng dòng.
8. **Live smoke** — nếu tài khoản `gopetsmoke` có bạn/thư → verify parse xong không throw `ProtocolException`.

## Success Criteria

- [ ] Router register opcode 121 (envelope + sub 1/2/10/13/4) và 62.
- [ ] Bấm "Bạn bè" → menu hiện, có ít nhất 1 dòng.
- [ ] Bấm "Hộp thư" → inbox hiện, đếm đúng số thư.
- [ ] Bấm "Tủ quần áo" → menu items char cosmetic hiện.
- [ ] Thao tác con (Thêm bạn, Đọc thư, Mặc đồ) gửi packet đúng byte.
- [ ] `ProtocolException` không xảy ra trên tài khoản thật.
- [ ] Live smoke thêm 3 check "friend menu / mail inbox / wardrobe open" — pass.
- [ ] Không file nào vượt 200 dòng.

## Risk Assessment

- **R1: Opcode 121/62 KHÔNG bọc trong `SHOW_MENU_ITEM`** — server có thể trả format riêng (đặc biệt danh sách bạn với avatar + trạng thái online). Cần đọc kỹ handler server, có thể phải viết view riêng cho danh sách bạn.
- **R2: Sub-command 4 (gửi thư) cần compose UI** — nhiều field (recipient, subject, body, item attachment). Phase 4 chỉ mở inbox; compose để phase 8 hoặc mở rộng.
- **R3: Tủ quần áo cần preview visual** — nếu server chỉ trả list ID, cần fetch icon → xin thêm ảnh qua `RemoteAssetCache`. Không blocker vì đã có infra.

## Rollout Notes

- Sau Phase 4 tính năng "trục xã hội" (bạn bè + thư) tối thiểu chạy được — điều kiện chính để chơi thử với người khác.
- Nếu bung ra thấy custom format phức tạp (avatar bạn với timestamp online), tách 1 phase-4b riêng.
