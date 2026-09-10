---
phase: 1
title: HUD số liệu thật (stats/currency/level)
status: completed
priority: P0
effort: 1-2d
dependencies: []
---

# Phase 1: HUD số liệu thật (stats/currency/level)

## Overview

`CharacterHud.SetStats(100,100,100,100,0,100)` (`Runtime/World/CharacterHud.cs:127`) đang cứng. Người chơi không thấy HP/MP/EXP/tiền thật → chặn mọi cảm nhận về tài nguyên. Phase này nối HUD vào gói stats server và bổ sung dòng tiền tệ + level.

## Requirements

**Functional:**
- HP/MP/EXP hiển thị số thật từ server, cập nhật realtime.
- Level nhân vật hiện bên cạnh tên.
- 4 tiền tệ (mGold/Đậu/Thóc/Ngọc) — icon + số.
- Thanh Energy (`nluong`) — jar hiển thị ở HUD (`fr.java:606`).

**Non-functional:**
- HUD không nuốt input joystick (raycast target off cho panel).
- Layout responsive theo `PixelCanvas`.

## Architecture

**Nguồn dữ liệu:**
- `INIT_PLAYER` (opcode 31) đã có trong `MapHandler.PlayerInitReceived` — chỉ trả userId/name/gender. Không có stats.
- Stats/currency đến qua các opcode khác (chưa reverse hết). Cần đọc `GServer/Data/Player/PlayerData.cs` + `Player.sendInfo*()` để tìm opcode chính xác. Đề cử: `PET_INVENTORY` (81/?), `LOGIN_SUCCES` phần đuôi (đã có), hoặc `UPDATE_MONEY` (grep server).

**Pattern:**
- Tạo `PlayerStatsHandler` (thuần C#, `Net/Player/`) — cùng khuôn với `MapHandler`.
- Bắn event `StatsUpdated(PlayerStats stats)` → `CharacterHud.SetStats(...)` subscribe.

## Related Code Files

**Create:**
- `Assets/Scripts/Net/Player/PlayerStats.cs` — DTO {Hp, MaxHp, Mp, MaxMp, Exp, ExpToNext, Level, MGold, Dau, Thoc, Ngoc, Energy}.
- `Assets/Scripts/Net/Player/PlayerStatsHandler.cs` — parse gói + bắn event.
- `Assets/Scripts/Runtime/World/CurrencyBar.cs` — panel 4 icon + số, đặt góc phải HUD.

**Modify:**
- `Assets/Scripts/Runtime/World/CharacterHud.cs` — bỏ dòng `SetStats(100,100,...)` fake; thêm text `Level`; nối event.
- `Assets/Scripts/Runtime/World/GameSession.cs` — khởi tạo `PlayerStatsHandler`, wire vào `_hud.Character`.
- `Assets/Scripts/Runtime/World/GameHud.cs` — chèn `CurrencyBar` vào layout.

**Read (context):**
- `client.jar_Decompiler.com/fr.java:566-608` — layout HUD jar.
- `client.jar_Decompiler.com/dj.java:54` — bảng icon 30 loại.
- `SRCGOPETGOC/GServer/Data/Player/PlayerData.cs` — nguồn stats.
- `SRCGOPETGOC/GServer/Server/Player.cs` — hàm `sendInfo*` (find opcode).

## Implementation Steps

1. **Reverse opcode stats** — grep server:
   ```
   grep -rn "Message(.*).*put.*hp\|maxHp\|money\|mGold" GServer/
   grep -rn "sendInfo\|updateMoney\|sendStats" GServer/Server/Player.cs
   ```
   Xác định opcode + wire format. Ghi vào comment header `PlayerStatsHandler.cs`.
2. **Nếu stats đi kèm nhiều opcode nhỏ** — mỗi opcode một handler riêng trong cùng file (money, hp, level). Không tự gộp thành gói tổng.
3. **DTO `PlayerStats`** — read-only struct, immutable field, tránh mutate share.
4. **`PlayerStatsHandler`** — pattern y hệt `MapHandler`: `RegisterOn(router)`, `event StatsUpdated`.
5. **`CurrencyBar`** — 4 hàng icon+text, dùng `RoundedUiSprite` để đồng bộ style với `CharacterHud`.
6. **`CharacterHud`** — bỏ `SetStats(100,100,...)`; thêm `SetLevel(int)`, `SetEnergy(int)`; expose subscription hook.
7. **`GameSession.Start`** — sau `_worldHandler.RegisterOn`, thêm:
   ```
   var stats = new PlayerStatsHandler();
   stats.RegisterOn(client.Router);
   stats.StatsUpdated += s => { hud.Character.SetStats(...); hud.Currency.Set(...); };
   ```
8. **Live smoke** — thêm check trong `tests/Gopet.Net.LiveSmoke/` bắt gói stats sau login.
9. **PlayModeTest** — bơm gói fake, verify HUD số cập nhật.

## Success Criteria

- [ ] HUD hiển thị HP/MP/EXP số THẬT của server, không placeholder.
- [ ] Level xuất hiện cạnh tên.
- [ ] 4 tiền tệ hiện đúng giá trị (verify bằng cách so với query SQL `SELECT gold,dau,thoc,ngoc FROM player WHERE id=?`).
- [ ] Energy hiện + đếm ngược nếu server bơm.
- [ ] Live smoke pass check "stats received sau login".
- [ ] PlayModeTest bơm gói fake → HUD update.
- [ ] Không file nào vượt 200 dòng.

## Risk Assessment

- **R1: Không tìm được opcode stats duy nhất** — server có thể chia nhỏ vào nhiều gói (money, hp, exp riêng). Xử lý: mỗi opcode 1 handler nhỏ, HUD kéo state chung từ store.
- **R2: Energy có logic countdown client-side** — jar tính `System.currentTimeMillis()` (`fr.java:596`). Đảm bảo Unity không double-count nếu server cũng đẩy.
- **R3: Level nhân vật vs level pet dễ nhầm** — `UPDATE_PET_LVL` (đã có) là của pet, không phải char.

## Rollout Notes

- Sau khi opcode xác định, cập nhật `docs/system-architecture.md` mục Player Stats.
- Log warn nếu stats gói không parse được (`ExpectFullyConsumed`).
