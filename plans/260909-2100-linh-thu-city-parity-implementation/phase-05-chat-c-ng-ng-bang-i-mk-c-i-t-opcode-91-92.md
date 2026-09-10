---
phase: 5
title: Chat cộng đồng/bang + Đổi MK + Cài đặt (opcode 91/92)
status: completed
priority: P1
effort: 2d
dependencies:
  - 4
---

# Phase 5: Chat cộng đồng/bang + Đổi MK + Cài đặt (opcode 91/92)

> **BACKLOG CLOSED (2026-09-10).**
> - `ChatChannelSelector` — 3 kênh chip (KV / CĐ / BANG) cạnh chat input; đổi channel đổi hàm gửi.
> - `ChangePasswordView` — form 3 InputField (Password), verify local (new == confirm, ≥ 6 chars) trước khi gửi opcode 93 (CHANGE_NEW_PASSWORD).
> - `ChatChannelPackets` byte-for-byte (test đầy đủ).
> - Còn thiếu: cài đặt runtime UI (Nhạc BG/EFF/Ẩn UI/Restart) chưa dựng — SoundManager có sẵn, chỉ cần dựng view.
> - Còn thiếu: Guild chat SEND thật cần `clanId` (client hiện chưa có); tạm gửi RequestHistory. Đợi CLAN_INFO parse để có clanId.

## Overview

Opcode 91 và 92 là 2 gia đình sub-command lớn của server. Jar dùng:
- **91**: chat bang (sub 20), tủ trang bị pet (sub 1/3/14/15/16/26/27), một số action pet (`dc.java:92-155`).
- **92**: chat cộng đồng (sub 2), chuyển kênh (sub 3), đổi mật khẩu.

Phase 4 đã cover trục xã hội (bạn/thư/tủ char). Phase 5 cover **giao tiếp cross-map** (chat cộng đồng/bang), **quản trị tài khoản** (đổi MK) và **cài đặt runtime** (đã stub ở Phase 2, giờ hoàn thiện).

## Requirements

**Functional:**
- Chat cộng đồng: gửi text → hiện ở channel cộng đồng cho mọi map.
- Chat bang hội: chỉ hiện cho thành viên bang.
- Đổi mật khẩu: form 3-field (MK cũ / MK mới / Nhập lại) → gửi opcode `cx.c()` (auth family).
- Cài đặt: 4 dòng bật/tắt nhạc BG, nhạc EFF, ẩn UI, restart connection (hoàn thiện từ Phase 2).

**Non-functional:**
- Chat có limit 120 chars client-side (khớp `GameHud.MakeInput`).
- MK không log ra `PacketLogger` (thêm mask cho opcode auth).

## Architecture

- Chat cộng đồng/bang **không** dùng `ON_PLACE_CHAT` (đó là place-local). Cần opcode riêng — grep `GameController` case 91/92.
- View chat: **thêm channel selector** trong chat input (Place / Community / Guild).
- Đổi MK: reuse `FormView` với 3 field password (`InputField.contentType = Password`).
- Cài đặt: reuse `CharacterMenuActions.Settings` (đã stub Phase 2).

## Related Code Files

**Create:**
- `Assets/Scripts/Net/Chat/ChannelChatPackets.cs` — hàm gửi CommunityChat/GuildChat.
- `Assets/Scripts/Net/Chat/ChannelChatHandler.cs` — parse gói recv (broadcast) → bắn event `ChannelChatReceived(channel, sender, text)`.
- `Assets/Scripts/Net/Auth/ChangePasswordPackets.cs` — gửi opcode auth đổi MK.
- `Assets/Scripts/Runtime/UI/ChatChannelSelector.cs` — dropdown Place/Community/Guild.
- `Assets/Scripts/Runtime/UI/ChangePasswordView.cs` — form 3 field.

**Modify:**
- `Assets/Scripts/Runtime/World/GameHud.cs` — thêm `ChatChannelSelector` bên trái input.
- `Assets/Scripts/UiLogic/CharacterMenuActions.cs` (Phase 2) — thay stub `ChangePassword`.
- `Assets/Scripts/Net/PacketLogger.cs` — mask body opcode đổi MK (đừng log plaintext).
- `Assets/Scripts/Runtime/World/GameSession.cs` — register handler mới.

**Read (context):**
- `client.jar_Decompiler.com/fr.java:377-411` — flow đổi MK jar (cd 327/328).
- `client.jar_Decompiler.com/fr.java:377-399` — chat cộng đồng / bang (cd 334/335).
- `client.jar_Decompiler.com/dc.java:92-155` — hàm gửi opcode 91.
- `SRCGOPETGOC/GServer/Server/GameController.cs` — case 91, 92.

## Implementation Steps

1. **Xác định sub-command chat cộng đồng/bang** — grep server:
   ```
   grep -n "case 91\|case 92\|COMMUNITY_CHAT\|GUILD_CHAT\|SEND_CHAT" GServer/
   ```
2. **`ChannelChatPackets`**:
   ```csharp
   public static Message Community(string text) =>
       Message.Create(92).PutSByte(2).PutUtf(text);
   public static Message Guild(string text) =>
       Message.Create(91).PutSByte(20).PutUtf(text);
   ```
3. **`ChannelChatHandler`** — recv wire: sub bytes → parse `senderId + text`, bắn event tương ứng channel.
4. **`ChatChannelSelector`** — dropdown 3 option; đổi channel đổi hàm `_sendFunc` dùng khi Submit.
5. **`GameHud.SubmitChat`** — dispatch theo channel hiện chọn.
6. **`ChangePasswordView`** — 3 field password + nút OK; verify `MK mới == Nhập lại` trước khi gửi.
7. **`ChangePasswordPackets`** — theo `cx.c(old, new)` trong jar; opcode nằm ở auth family (cần grep).
8. **`PacketLogger` mask** — nếu opcode = đổi MK thì log `[REDACTED]` thay vì hex body.
9. **Cài đặt** — hoàn thiện Phase 2 stub: 4 dòng dùng `GenericMenuView`, mỗi dòng có label + trạng thái hiện tại (BG ON/OFF).
10. **PlayModeTest** — bơm chat từ channel khác, verify hiện bong bóng đúng màu (Community/Guild khác Place).

## Success Criteria

- [ ] Selector 3 channel trong chat input.
- [ ] Chat cộng đồng gửi → hiện ở channel cộng đồng của tài khoản khác (verify cross-account với 2 client).
- [ ] Chat bang chỉ hiện cho member.
- [ ] Đổi MK thành công đổi được đăng nhập lại với MK mới.
- [ ] Cài đặt bật/tắt nhạc, ẩn UI, restart hoạt động.
- [ ] `PacketLogger` KHÔNG log plaintext MK.
- [ ] Không file nào vượt 200 dòng.

## Risk Assessment

- **R1: MK nằm plaintext trong `PutUtf`** — client jar gửi plaintext, server bcrypt phía sau. Cần confirm — nếu jar tự hash trước khi gửi thì Unity phải hash cùng thuật toán.
- **R2: `ChangePassword` opcode nằm ngoài họ Guider** — có thể là `SERVER_MESSAGE` sub riêng, dễ nhầm.
- **R3: Cross-account test cần 2 tài khoản đăng nhập cùng lúc** — có `gopettest` + `gopetsmoke`, dùng được.

## Rollout Notes

- Chat cộng đồng thường có spam filter server-side; nếu bị chặn thì log warn, không throw.
- Sau phase này, tập hợp "trục thoại + trục tài khoản" cơ bản đủ dùng.
