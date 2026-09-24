---
phase: 1
title: Server dữ liệu và gói tin
status: completed
priority: P1
effort: 4h
dependencies: []
---

# Phase 1: Server dữ liệu và gói tin

## Overview
Server làm chủ danh mục khung cảnh, giá, quyền sở hữu và lựa chọn. Thêm 2 cột vào `player`,
bảng danh mục, 4 sub-command của `COMMAND_GUIDER` và luồng mua/chọn an toàn.

## Key Insights
- Tiền: `PlayerData.gold` = vàng (`Data/User/PlayerData.cs:15-18`). `Player.checkGold` :849,
  `mineGold` :825 (tự cộng `spendGold`/top tiêu vàng), **không có** bản atomic kiểu
  `TrySpendCoin` :771 (`_coinLock`). Mọi hàm đổi tiền tự gọi `controller.updateUserInfo()` →
  client nhận `MONEY_INFO` nên thanh vàng trên màn đấu tự cập nhật.
- Lưu: Dapper map `SELECT * FROM player` (`Player.cs:472`); UPDATE lớn trong
  `PlayerData.saveStatic` (`PlayerData.cs:192-254`). List lưu JSON qua `JsonAdapter<T>`
  (`GopetManager.cs:915-940`) — `CopyOnWriteArrayList<int>` đã có handler (`MoneyDisplays`).
- Mẫu tính năng tự thêm: điểm danh — hằng `GopetCMD.cs:129-131`, dispatch
  `GameController.guider` :770 (case :833-838), gói trạng thái `SendDailyCheckinState` :5391.

## Requirements
- Functional: xem danh mục; mua (trừ vàng, sở hữu vĩnh viễn, tự chọn luôn); chọn khung cảnh đã sở hữu; id 0 luôn miễn phí.
- Non-functional: không trừ vàng hai lần khi bấm liên tục; từ chối id lạ; giá chỉ ở server.

## Architecture
Danh mục `Data/BattleBackground/BattleBackgroundCatalog.cs` (static, bất biến):
```csharp
public sealed record BattleBackgroundDef(int Id, string Name, long PriceGold);
Defs = { (0,"Rừng",0), (1,"Rừng cây che",7000), (2,"Hoa anh đào",10000),
         (3,"Tuyết trắng",12000), (4,"Hang động đá",17000), (5,"Mưa lửa",22000) };
```
Dịch vụ `Data/BattleBackground/BattleBackgroundService.cs`: `SendState(player)`, `Buy(player,id)`, `Select(player,id)`.

Gói tin (sub của `COMMAND_GUIDER`; kiểm lại 43–46 chưa dùng ở cả `GopetCMD.cs` server lẫn `Net/GopetCmd.cs` client):
| Sub | Hướng | Payload |
|---:|---|---|
| 43 `TYPE_BATTLE_BG_STATE` | S→C | `sbyte selectedId, sbyte n, [sbyte id, UTF name, long priceGold, bool owned]×n` |
| 44 `TYPE_BATTLE_BG_OPEN` | C→S | — |
| 45 `TYPE_BATTLE_BG_BUY` | C→S | `sbyte id` |
| 46 `TYPE_BATTLE_BG_SELECT` | C→S | `sbyte id` |

Mua: `lock` theo player → id hợp lệ, id≠0, chưa sở hữu → `TrySpendGold(price)` → thêm vào
`BattleBgOwned`, đặt `BattleBgSelected=id` → `SendState` + toast "Đã mua ...". Thiếu vàng →
`redDialog` "Không đủ vàng". Chọn: id=0 hoặc đã sở hữu → đặt, `SendState`; không thì bỏ qua.

## Related Code Files
- Create: `SRCGOPETGOC/MariaDB_SQL/migration-260924-battle-background.sql` —
  `ALTER TABLE player ADD COLUMN BattleBgOwned TEXT NULL, ADD COLUMN BattleBgSelected INT NOT NULL DEFAULT 0;`
- Create: `GServer/Data/BattleBackground/BattleBackgroundCatalog.cs`, `BattleBackgroundService.cs`
- Modify: `GServer/Data/User/PlayerData.cs` (2 property + 2 cột trong UPDATE `saveStatic`)
- Modify: `GServer/Server/Player.cs` (thêm `TrySpendGold` giống `TrySpendCoin`, tái dùng thân `mineGold`)
- Modify: `GServer/Server/Bot.cs` (override `TrySpendGold` nếu Bot override các hàm vàng)
- Modify: `GServer/Server/GopetCMD.cs` (4 hằng), `GServer/Server/GameController.cs` (3 case trong `guider`)

## Implementation Steps
1. Viết migration; áp vào DB local (Docker MariaDB, cần `GOPET_DB_PASSWORD`).
2. `PlayerData`: `CopyOnWriteArrayList<int> BattleBgOwned = new()` (null-safe khi đọc NULL) + `int BattleBgSelected`; thêm vào UPDATE.
3. `Player.TrySpendGold(long)` atomic (khoá riêng `_goldLock` hoặc dùng chung khoá coin nếu gold/coin cùng khoá).
4. Catalog + Service; khi load player, nếu `BattleBgSelected` không hợp lệ/không sở hữu → về 0.
5. Hằng GopetCMD + case dispatch trong `guider`; gọi Service.
6. `dotnet build` GServer, chạy server, test bằng client.

## Todo
- [ ] Migration + PlayerData
- [ ] TrySpendGold (+ Bot)
- [ ] Catalog + Service
- [ ] Opcode + dispatch
- [ ] Build sạch

## Success Criteria
- [ ] Mở popup nhận STATE đủ 6 dòng, id 0 luôn owned.
- [ ] Mua đủ vàng: vàng giảm đúng giá, owned=true, selected=id; relog vẫn giữ.
- [ ] Bấm mua 5 lần liên tiếp chỉ trừ 1 lần. Thiếu vàng không trừ gì.
- [ ] Gửi BUY/SELECT id ngoài 0..5 hoặc SELECT id chưa sở hữu → không đổi gì.

## Risk Assessment
- Trùng số sub-command → route sai: grep cả hai phía trước khi chốt.
- Server crash trước AutoSave → mất giao dịch vừa mua: chấp nhận như các giao dịch vàng khác; có thể gọi `save()` ngay sau mua nếu muốn chắc.

## Security Considerations
Server tự kiểm id, giá, sở hữu; client không gửi giá. Mua dùng trừ tiền atomic.
