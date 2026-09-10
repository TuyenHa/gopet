---
phase: 3
title: Pet interaction UI (Cảm xúc + Hồi phục)
status: completed
priority: P0
effort: 0.5-1d
dependencies: []
---

# Phase 3: Pet interaction UI (Cảm xúc + Hồi phục)

## Overview

Unity **chỉ receive** `ON_PET_INTERACT` (`WorldObjectHandler.OnPetInteraction`) chứ không có UI **gửi** — nghĩa là pet của người khác vẫy tay được nhưng pet của mình thì không tương tác được. Jar (`fr.java:150-183`) có 3 nút Chơi/Hôn/Xoa (opcode 81/17/[0|1|2]) + 1 nút Hồi phục (81/45/1).

Effect visual đã có (`PetInteractionEffect.cs`) — chỉ thiếu UI trigger.

## Requirements

**Functional:**
- 3 nút Cảm xúc: **Chơi với pet** / **Hôn pet** / **Xoa đầu pet** — gửi `PET_SERVICE 17 / [1|0|2]`.
- 1 nút **Hồi phục pet** — gửi `PET_SERVICE 45 / 1`.
- Gọi ra khi tap pet của SELF, hoặc từ pet HUD.

**Non-functional:**
- Nút disable khi pet gần chết + đã full HP (client biết cấp thấp thông qua stats từ Phase 1).
- Cooldown local để tránh spam (jar không có; nhưng thêm 500ms client-side để giảm noise).

## Architecture

- Reuse `WorldObjectHandler` (đã cache pet spawn) → biết vị trí pet.
- Detect tap: `PetAvatar.OnPointerClick` → mở `PetActionRadial` (menu tròn nhỏ 4 nút).
- Gửi opcode qua `PetServicePackets` (tạo mới nếu chưa có; hoặc mở rộng `BattleHandler` — chọn tách file riêng vì đây không phải battle).

## Related Code Files

**Create:**
- `Assets/Scripts/Net/Pet/PetActionPackets.cs` — hàm static `Interact(int type)`, `Heal()`.
- `Assets/Scripts/Runtime/World/PetActionRadial.cs` — menu tròn 4 nút.
- `Assets/Scripts/Runtime/World/PetAvatar.cs` (nếu chưa có class riêng cho pet của SELF) — tap detect.

**Modify:**
- `Assets/Scripts/Runtime/World/PetInteractionEffect.cs` — hiện thêm effect cho action gửi đi (không đợi server echo — smoother UX).
- `Assets/Scripts/Runtime/World/GameSession.cs` — wire `PetActionPackets.Send = client.Send`.

**Read (context):**
- `client.jar_Decompiler.com/fr.java:150-200, 295-298` — cấu trúc menu Cảm xúc jar.
- `client.jar_Decompiler.com/dc.java:12-18, 53-64` — opcode `dc.a(int)` và `dc.a(bool)`.
- `SRCGOPETGOC/GServer/Server/GopetCMD.cs` — tra `PET_INTERACT`, `PET_RECOVERY_HP` để confirm sub-command.

## Implementation Steps

1. **Xác định lại sub-command** — grep `PET_INTERACT` trong `GServer/Server/GopetCMD.cs`. Đối chiếu jar `en(81).a(17)` → sub=17. `PET_RECOVERY_HP` = 45.
2. **`PetActionPackets`** — thuần Net, không đụng Unity:
   ```csharp
   public const sbyte Play = 1, Kiss = 0, Poke = 2;
   public static Message Interact(sbyte type) =>
       Message.Create(GopetCmd.PET_SERVICE).PutSByte(17).PutSByte(type);
   public static Message Heal() =>
       Message.Create(GopetCmd.PET_SERVICE).PutSByte(45).PutSByte(1);
   ```
3. **`PetActionRadial`** — 4 nút tròn xung quanh pet; icon từ `pet/button/play.png/kiss.png/puke.png/heal.png` (đã có ở jar assets, đã unpack sang `Assets/Resources/Jar/Art/`).
4. **Tap detect** — `PhysicsRaycaster` sẵn có (đã setup ở `GameSession.Start`) — thêm collider cho pet self.
5. **Cooldown** — 500ms local trước khi nút re-enable; hiển thị fade khi disable.
6. **Effect optimistic** — gọi `PetInteractionEffect.Play(type)` ngay khi gửi (không đợi server echo). Server `ON_PET_INTERACT` broadcast lại cho người khác — self cũng nhận nhưng ignore vì đã play rồi (guard bằng cooldown).
7. **Test** — PlayModeTest: bấm 4 nút, verify packet ra đúng byte + effect visual play.

## Success Criteria

- [ ] Tap pet self → menu tròn 4 nút hiện quanh pet.
- [ ] Bấm mỗi nút gửi đúng byte (verify qua `PacketLogger` diff).
- [ ] Effect visual play ngay khi bấm, không đợi server round-trip.
- [ ] Nút Hồi phục disable khi HP đã full (dùng stats từ Phase 1).
- [ ] Không double-effect khi server echo về (self ignore trong cooldown).
- [ ] Không file nào vượt 200 dòng.

## Risk Assessment

- **R1: Server echo cùng ID với self** — `ON_PET_INTERACT` trả `int userId + sbyte type`. Cần check `userId == localUserId` → ignore để tránh double effect.
- **R2: Cooldown quá dài che UX** — 500ms là cân bằng. Nếu server latency thấp có thể giảm 300ms.
- **R3: Pet self chưa có collider** — Unity `PlayerAvatar` có, nhưng pet của SELF có thể chưa tách class riêng. Kiểm ở step 4.

## Rollout Notes

- Sau khi Phase 1 xong, disable nút Hồi phục nếu HP full.
- Tương lai: cân nhắc thêm long-press pet → menu con Cường hoá/Tiến hoá (thuộc Phase 7).
