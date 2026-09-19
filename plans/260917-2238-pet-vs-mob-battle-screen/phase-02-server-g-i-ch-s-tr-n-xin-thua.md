---
phase: 2
title: "Server: gói chỉ số trận & Xin thua"
status: pending
priority: P1
effort: "3h"
dependencies: [1]
---

# Phase 2: Server: gói chỉ số trận & Xin thua

## Overview
Thêm 1 gói S→C mang dữ liệu HUD mà gói cũ thiếu (level thật, ATK, DEF, % chí mạng, MP
của skill cả 2 bên) và 1 sub C→S "Xin thua". Cả hai chỉ dành cho client ≥ 1.5.0.

## Requirements
- Functional:
  - `PET_BATTLE_STATS` gửi khi mở trận (PvE + PvP) và khi observer nhận snapshot
    (`sendBattleInfo`), ngay cạnh `sendBuffState()`.
  - `PET_BATTLE_SURRENDER` (sub của `PET_BATTLE`): người đang trong trận bỏ cuộc → thua
    ngay, không nhận thưởng; PvP: đối thủ thắng và nhận kết quả như thắng thường.
- Non-functional: gate `ApplicationVersion >= VERSION_150`; jar không nhận gói mới;
  jar gửi sub lạ thì server bỏ qua (đã là hành vi `switch` hiện tại).

## Architecture

Wire format (sub mới của `PET_SERVICE`, chọn giá trị **chưa dùng** — grep `GopetCMD.cs`
và `GopetCmd.cs` phía client; 39/40/41 đã bị dùng ở nhóm khác nên tránh để dễ đọc):

```
PET_BATTLE_STATS
int   battleId                  (= userId người nhận, theo quy ước docs/battle-system.md §3)
sbyte actorCount (≤2)
├─ int   actorId
│  int   level
│  int   atk        (GameObject.getAtk())
│  int   def        (GameObject.getDef())
│  short critPermille (CritPercent × 10, clamp 0..1000)
│  sbyte skillCount (≤16)
│  └─ int skillId
│     int mpCost
```

```
C→S: PET_SERVICE / PET_BATTLE / PET_BATTLE_SURRENDER (sbyte, ví dụ 5)   — không payload
```

- `GameObject`: tách `public virtual float CritPercent` từ `isCrit()`; `isCrit()` dùng lại
  nó (**giữ nguyên công thức**, kể cả `4 / 100` = 0 do chia nguyên — ghi chú, không sửa
  cân bằng trong plan này).
- Surrender: tái dùng đường thua hiện có. PvE: `mob` thắng (giống `getWinId()` khi
  `isClose`), gọi `win()` để gửi `PET_BATTLE_STATE`, hồi máu quái như khi pet thua. PvP:
  đặt `ClosePlayer = người bỏ cuộc` → `win()` gửi kết quả cho cả hai (phase-01 plan
  260917-1812 đã đảm bảo 2 gói). Đọc `win()` `:736-900` trước khi quyết định cờ.
- Chặn spam: bỏ qua nếu trận đã `hadFinished`.
- Surrender đi qua hàng đợi `actions` (PvE) giống attack để không race với lượt quái.

## Related Code Files
- Modify: `Server/GopetCMD.cs` (2 hằng số), `Base/GameObject.cs` (`CritPercent`),
  `Data/Battle/PetBattle.cs` (`sendStatsState`, `onMessage` nhánh PvE + PvP, surrender)
- Create (nếu `PetBattle.cs` phình thêm): `Data/Battle/PetBattleStatsWriter.cs` — giữ
  logic ghi gói ra ngoài file 1810 dòng.

## Implementation Steps
1. Thêm hằng số, comment gate 1.5.0 giống `PET_BATTLE_BUFF`.
2. Tách `CritPercent`.
3. `PetBattleStatsWriter.Write(PetBattle, Player target)` — lấy actor + skill từ
   `pet.skill` (pet) / `mob.Skills` (quái, phase 1). Tái dùng hàm gate version hiện có
   ở `:1743` (trích ra helper nếu đang inline).
4. Gọi ở `sendStartFightMob`, `sendStartFightPlayer` (cả 2 bên), `sendBattleInfo`.
5. Nhánh `PET_BATTLE_SURRENDER` trong `onMessage` (PvE + PvP) + hàm `surrender(Player)`.
6. Build + restart; test bằng LiveSmoke (phase 6).

## Success Criteria
- [ ] Client 1.5.0 nhận `PET_BATTLE_STATS` đúng thứ tự sau gói mở trận; jar không nhận.
- [ ] Level/ATK/DEF khớp màn thông tin pet.
- [ ] Xin thua PvE → client nhận `PET_BATTLE_STATE` với winner = mobId, 0 thưởng; quái
      vẫn còn trên map, đánh lại được.
- [ ] Xin thua PvP → cả hai nhận kết quả, người còn lại thắng.
- [ ] Gửi surrender khi không trong trận / trận đã xong → không lỗi, không gói.

## Risk Assessment
- Lạm dụng xin thua để né phạt? Đi cùng hình phạt thua hiện có (không hơn, không kém).
- Đấu trường: xin thua phải cộng điểm cho đối thủ như thua thường — kiểm tra `ArenaPlace`.
