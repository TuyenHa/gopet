---
phase: 2
title: Nút Menu nhân vật + 12 mục
status: completed
priority: P0
effort: 1d
dependencies: []
---

# Phase 2: Nút Menu nhân vật + 12 mục

## Overview

Unity thiếu nút "Menu" — trục vào 12 chức năng của jar (Bạn bè, Hộp thư, Chat cộng đồng/bang, Chat khu vực, Tủ quần áo, Chọn pet, Đổi mật khẩu, Cài đặt, Đăng xuất, Thoát...). Phase này dựng khung trống — mỗi mục **gửi đúng opcode** như jar; UI kết quả server trả về sẽ do các phase 4/5 phủ.

## Requirements

**Functional:**
- Nút "Menu" trên HUD (icon 3-gạch, góc phải-trên hoặc phải-dưới).
- Bấm → mở `GenericMenuView` 12 dòng theo thứ tự jar (`fr.java:129-141`).
- Mỗi dòng dispatch opcode/action đúng jar.
- Submenu Bạn bè (4 mục), Hộp thư (2 mục), Cài đặt (4 mục).

**Non-functional:**
- Đóng menu bằng Esc hoặc chạm ngoài.
- Menu KHÔNG nuốt joystick khi đang mở (hoặc tạm khoá joystick — chọn "khoá" cho phù hợp jar).

## Architecture

- Tạo enum `CharacterMenuAction` để dispatch (KHÔNG hardcode magic number trong UI code).
- Tạo class `CharacterMenuActions` static — mỗi hàm nhận `GopetClient`, `LoginFlow`, `SoundManager`... và bắn đúng opcode. Đây là **fan-out** duy nhất.
- Cài đặt (Nhạc bg/eff/Ẩn UI/Restart) là client-side — không đụng server.
- Các mục Bạn bè/Hộp thư/Tủ quần áo/Chọn pet gửi opcode → chờ server trả `SHOW_MENU_ITEM` → `GenericMenuView` render tự nhiên (đã có sẵn).

## Related Code Files

**Create:**
- `Assets/Scripts/UiLogic/CharacterMenu.cs` — spec 12 dòng, hằng số opcode.
- `Assets/Scripts/UiLogic/CharacterMenuActions.cs` — hàm dispatch mỗi action.
- `Assets/Scripts/Runtime/UI/CharacterMenuButton.cs` — nút HUD.
- `Assets/Scripts/Runtime/UI/CharacterMenuView.cs` — mở/đóng, gắn `GenericMenuView`.

**Modify:**
- `Assets/Scripts/Runtime/World/GameHud.cs` — chèn `CharacterMenuButton`.
- `Assets/Scripts/Runtime/World/GameSession.cs` — khởi tạo `CharacterMenuActions` với các dependency.
- `Assets/Scripts/Runtime/UI/UiRoot.cs` — cho phép push menu này vào `DialogStack`.

**Read (context):**
- `client.jar_Decompiler.com/fr.java:127-540` — toàn bộ menu + submenu.
- `client.jar_Decompiler.com/gw.java:1-320` — bảng string dùng cho label.

## Implementation Steps

1. **Bảng label từ jar** — copy 12 chuỗi từ `gw.a(80..147)` sang `JarStrings` (nếu chưa có), verify khớp với `Assets/Resources/Jar/Strings/strings-vi.json`.
2. **`CharacterMenu.cs`** — enum + list `(label, action)`:
   | # | Label | Action |
   |---|---|---|
   | 1 | Thêm bạn | Submenu.FriendManage |
   | 2 | Hộp thư | Submenu.Mail |
   | 3 | Chat cộng đồng | Opcode 92 sub 2 |
   | 4 | Chat bang hội | Opcode 91 sub 20 |
   | 5 | Chat khu vực | UI mở input dialog |
   | 6 | Chat (khác) | Legacy — cân nhắc bỏ nếu trùng #5 |
   | 7 | Tủ quần áo | Opcode 62 |
   | 8 | Chọn pet | Opcode 5 |
   | 9 | Đổi mật khẩu | Client form 3-field |
   | 10 | Cài đặt | Submenu.Settings |
   | 11 | Đăng xuất | `LoginFlow.Reset()` |
   | 12 | Thoát | `Application.Quit()` |
3. **`CharacterMenuActions.cs`** — mỗi enum → hàm public. Ví dụ:
   ```csharp
   public static void CommunityChat(GopetClient c) =>
       c.Send(Message.Create(92).PutSByte(2));
   ```
4. **`CharacterMenuButton`** — Sprite icon menu (dùng `JarSkin` load `menu.png` nếu có, else procedural bằng `RoundedUiSprite`).
5. **`CharacterMenuView`** — reuse `GenericMenuView`; đóng bằng Esc.
6. **Submenu** — hàm `OpenSubmenu(kind)` mở tiếp `GenericMenuView` layer 2.
7. **Cài đặt** — 4 dòng: BG on/off, Eff on/off, Ẩn UI toggle, Restart connection. Nối `SoundManager` + `LoginFlow`.
8. **Test** — PlayModeTest bấm từng dòng, verify:
   - Nếu action gửi opcode → capture packet (đã có `PacketLogger`) và so byte.
   - Nếu action client-side → verify state (âm thanh tắt, UI ẩn...).

## Success Criteria

- [ ] Nút Menu hiện ở HUD, tap mở panel 12 dòng.
- [ ] Bấm từng dòng dispatch đúng opcode/action; packet dump khớp jar.
- [ ] Submenu Bạn bè/Hộp thư mở đúng cấp 2.
- [ ] Cài đặt: bật/tắt nhạc bg/eff, ẩn UI, restart connection đều hoạt động.
- [ ] "Đăng xuất" trở về màn login sạch (`LoginFlow.Reset()`), không rò state.
- [ ] Không file nào vượt 200 dòng.

## Risk Assessment

- **R1: Opcode 62/91/92 chưa register trong `MessageRouter`** — bấm cũng vô nghĩa vì response server bị drop. Đã handle ở Phase 4/5. Phase 2 chỉ gửi và log Router.OnUnhandled.
- **R2: Nút Menu che chỗ khác** — chọn góc trên-phải, bỏ padding 12px như `CharacterHud`.
- **R3: Đăng xuất giữa lúc network active** — `LoginFlow.Reset()` phải close socket sạch, tránh CLOSE_WAIT (đã có check E2 trong LiveSmoke).

## Rollout Notes

- Không dừng ở đây trước khi Phase 4/5 xong — user sẽ bấm và không thấy gì hiện lên.
- Ưu tiên demo trước với 5 mục client-side (Chat khu vực, Chọn pet nếu server đã sẵn, Cài đặt, Đăng xuất, Thoát) để có phản hồi trực quan sớm.
