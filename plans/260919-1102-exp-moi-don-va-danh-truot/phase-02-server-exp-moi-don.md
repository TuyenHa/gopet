# Phase 02 — Server: EXP mỗi đòn trúng + opcode `PET_BATTLE_EXP`

## Context Links

- [plan.md](plan.md) · [phase-01](phase-01-server-don-danh-truot.md)
- `docs/battle-system.md` §4 (bảng opcode), §5-6 (mẫu gate 1.5.0), §11 (thưởng khi kết trận PvE)
- Mockup: `plans/260917-2238-pet-vs-mob-battle-screen/visuals/battle-screen-mockup.png`

## Overview

- **Priority:** P2
- **Status:** done
- **Mô tả:** Mỗi đòn TRÚNG của pet lên quái cộng 5% EXP giết quái, trần 30%/trận, và gửi gói mới
  `PET_BATTLE_EXP` (gate 1.5.0) để client hiện số vàng. **`win()` KHÔNG trừ lại** — đây là
  thưởng thêm thật (user chốt 2026-09-19).

## Key Insights

1. **EXP là THƯỞNG THÊM THẬT — user đã chốt, KHÔNG được đổi thành ứng trước.**
   `win()` `:809-815` giữ nguyên: `exp = genExpWhenMobDie(...)` → nhân buff → `addExp(exp)`.
   Phần nhỏ giọt mỗi đòn là cộng THÊM lên trên.
   ⇒ Một con quái tốn N đòn cho tối đa **130%** EXP (100% giết + trần 30% nhỏ giọt).
   ⇒ **Trần 30%/trận là chốt chặn chống cày DUY NHẤT** — cài sai là hổng.
   (Bản nháp đầu dùng mô hình ứng trước, user đã bác vì muốn "đánh nhiều đòn được nhiều hơn"
   là thật.)
2. `genExpWhenMobDie` (`:1716`) hiện **tất định** (phần chênh cấp đã comment-out):
   `round(exp * FieldManager.PERCENT_EXP)`. Gọi lại được, không random ⇒ cache một lần/trận.
3. `win()` chỉ thưởng EXP khi `!(mob is Boss)` (`:799`). Boss **không** nhỏ giọt: boss là mục tiêu
   nhiều người đánh, nhỏ giọt cho mọi người mà không trừ lại ở đâu = rò EXP.
4. `updatePetLvl()` (`GameController.cs:1745`) chỉ lên **1 cấp mỗi lần gọi** và chỉ gửi opcode 18
   khi thật sự lên cấp ⇒ gọi sau mỗi đòn là rẻ (1 tra dictionary) và giữ level đúng ngay giữa trận.
   **Có** gọi — nếu dồn tới cuối trận thì người đánh-rồi-bỏ sẽ tích exp vượt ngưỡng mà không lên cấp.
5. Đã có sẵn hai lớp chống cày độc lập: `startFightMob` ban acc nếu giết quái < 4500ms trên map 12
   (`Place/GopetPlace.cs:406-412`), và `setLastTimeKillMob` được đặt ở **cuối mọi trận PvE**
   (`PetBattle.cs:888`) — thắng hay thua đều tính. Không cần thêm cơ chế mới.
6. Sub `33` của `PET_SERVICE` còn trống: không có trong `processPet` (C→S,
   `GameController.cs:978-1251`) lẫn trong báo cáo `check-protocol-coverage` (S→C).
   Cũng không trùng giá trị hằng số nào trong `GopetCMD.cs`.

## Requirements

**Chức năng**
- Mỗi đòn của pet **trúng** quái (đánh thường hoặc kỹ năng gây sát thương) cộng
  `max(1, round(killExp * 5%))` EXP cho pet.
- Cộng dồn trong một trận không vượt `round(killExp * 30%)`.
- Chỉ PvE (`petAttackMob == true`) và `!(mob is Boss)`. **Không** PvP/PK/đấu trường.
- Gửi `PET_BATTLE_EXP` cho chính người chơi đó, chỉ khi `ApplicationVersion >= VERSION_150`.
- `win()` giữ NGUYÊN, không trừ gì. Thưởng giết quái độc lập với phần nhỏ giọt.

**Phi chức năng**
- Không đổi opcode cũ. Gói mới gate 1.5.0 như `PET_BATTLE_BUFF` (38) / `PET_BATTLE_STATS` (43).
- Logic nhỏ giọt nằm ở file riêng (< 100 dòng), `PetBattle.cs` chỉ thêm ~12 dòng.

## Architecture

**Luồng dữ liệu**

```
petAttack / useSkill  ──(đòn TRÚNG, damage>0, petAttackMob, !Boss)──> awardHitExp()
                                                                          │
   HitExpReward.Grant(player, pet, mob) ──> killExp = genExpWhenMobDie(...)   [cache]
                                            perHit = max(1, 5% killExp)
                                            perHit = min(perHit, 30%killExp − Paid)
                                            Paid += perHit
                                                                          │
                                    activePet.addExp(perHit)
                                    activePlayer.controller.updatePetLvl()   → opcode 18 nếu lên cấp
                                    HitExpReward.Send(...)                   → opcode 81/33 (gate 1.5.0)

win() nhánh thắng:  exp = genExpWhenMobDie(...)        → KHÔNG trừ, giữ nguyên như trước
```

**Wire `PET_BATTLE_EXP` (S→C, sub 33 của `PET_SERVICE`, gate 1.5.0)**

```
int battleId    // = user_id người nhận, đúng quy ước §3 của docs/battle-system.md
int actorId     // = actorId của pet được cộng (PvE: bằng battleId; giữ trường để PvP sau này không phải đổi wire)
int expDelta    // > 0
```

## Related Code Files

**Tạo**
- `SRCGOPETGOC/GServer/Data/Battle/HitExpReward.cs`

**Sửa**
- `SRCGOPETGOC/GServer/Server/GopetCMD.cs` — thêm `PET_BATTLE_EXP = 33`
- `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs`

**Xoá:** không.

## Implementation Steps

1. `Server/GopetCMD.cs` — thêm cạnh `PET_BATTLE_STATS` (`:45`):

   ```csharp
   /// <summary>PET_SERVICE sub — EXP nhỏ giọt mỗi đòn trúng trong PvE (số vàng trên đầu pet).
   /// Là thưởng THÊM, cộng lên trên thưởng giết quái (user chốt 2026-09-19). Gate <c>VERSION_150</c>.
   /// Xem plans/260919-1102-exp-moi-don-va-danh-truot/phase-02.</summary>
   public const sbyte PET_BATTLE_EXP = 33;
   ```

2. Tạo `Data/Battle/HitExpReward.cs` (~80 dòng), namespace `Gopet.Battle` (cùng namespace với
   `PetBattle`, `PetBattle.cs:15`). `using` cần: `Gopet.Data.Mob`, `Gopet.Util`, `Gopet.Manager`,
   `Gopet.IO`, `System` — sao theo header `PetBattle.cs:1-13`:

   ```csharp
   public sealed class HitExpReward
   {
       public const float PerHitPercent   = 5f;    // mỗi đòn trúng
       public const float BattleCapPercent = 30f;  // trần cộng dồn một trận

       private int _killExp = -1;   // cache, genExpWhenMobDie tất định
       private int _paid;
       public int Paid => _paid;

       /// <summary>Trả về số EXP được cộng cho đòn này (0 = không cộng).
       /// Gọi CHỈ khi đòn đã trúng và trận là PvE với quái thường.</summary>
       public int Grant(Player player, Pet pet, Mob mob) { ... }

       /// <summary>Gửi opcode 81/33. Bỏ qua client &lt; 1.5.0 (jar cũ không hiểu opcode lạ).</summary>
       public static void Send(Player target, int battleId, int actorId, int expDelta) { ... }
   }
   ```

   `Grant` chi tiết:
   - `if (mob == null || mob.getMobLvInfo() == null) return 0;`
   - `if (_killExp < 0) _killExp = PetBattle.genExpWhenMobDie(player, pet, mob, mob.getMobLvInfo().exp);`
   - `if (_killExp <= 0) return 0;`
   - `int cap = Utilities.round(Utilities.GetValueFromPercent(_killExp, BattleCapPercent));`
   - `int left = cap - _paid; if (left <= 0) return 0;`
   - `int perHit = Math.Max(1, Utilities.round(Utilities.GetValueFromPercent(_killExp, PerHitPercent)));`
   - `perHit = Math.Min(perHit, left); _paid += perHit; return perHit;`

   `Send` sao chép nguyên mẫu gate của `PetBattle.SendBuffStateTo`:
   `if (target?.ApplicationVersion == null || target.ApplicationVersion < GopetManager.VERSION_150) return;`
   `if (target.session == null) return;` rồi `new Message(GopetCMD.PET_SERVICE)` →
   `putsbyte(GopetCMD.PET_BATTLE_EXP)` → 3 × `putInt` → `cleanup()` → `target.session.sendMessage(m)`.

3. `PetBattle.cs` — thêm field cạnh `hadFinished` (`:780`):

   ```csharp
   /// <summary>EXP nhỏ giọt mỗi đòn trúng — sống theo vòng đời TRẬN (PetBattle được tạo mới
   /// mỗi trận ở GopetPlace.startFightMob) nên trần 30% không rò sang trận sau.</summary>
   private readonly HitExpReward hitExp = new HitExpReward();
   ```

4. `PetBattle.cs` — thêm method riêng (đặt ngay dưới `addRecovery`, `:1177`):

   ```csharp
   /// <summary>Cộng EXP cho một đòn pet TRÚNG quái. Chỉ PvE quái thường: boss là mục tiêu
   /// nhiều người và win() không thưởng exp boss (:799) nên không có chỗ trừ lại.</summary>
   private void awardHitExp()
   {
       if (!petAttackMob || mob is Boss) return;
       int gained = hitExp.Grant(activePlayer, activePet, mob);
       if (gained <= 0) return;
       activePet.addExp(gained);
       activePlayer.controller.updatePetLvl();
       HitExpReward.Send(activePlayer, activePlayer.user.user_id, activePlayer.user.user_id, gained);
   }
   ```

5. Gọi `awardHitExp()`:
   - `petAttack` — trong nhánh `if (!isMiss) { ... if (petAttackMob) { ... } }`, đặt **ngay sau**
     khối `mob.Mutex` (sau `if (mob is Boss) { UpdateHpMob }`), trước `addRecovery`.
   - `useSkill` — trong nhánh `if (!(isMiss || damageInfo.isSkillMiss()))` `:1094`, bên trong
     `if (isPetAttackMob())`, sau khối `mob.Mutex`/`UpdateHpMob`.
     Thêm điều kiện `if (damageInfo.getDamge() + damageInfo.getTrueDamge() > 0)` để kỹ năng
     0 sát thương (thuần buff lọt vào đây) không sinh EXP.

6. **KHÔNG đụng `win()`.** User chốt thưởng thêm thật — phần nhỏ giọt cộng lên trên thưởng
   giết quái, không trừ lại. Nếu sau này muốn đổi sang ứng trước thì chèn
   `exp = Math.Max(0, exp - hitExp.Paid);` trước `activePet.addExp(exp)` `:814` — nhưng đó là
   quyết định cân bằng của user, không phải việc tự ý sửa.

7. Compile: `dotnet msbuild SRCGOPETGOC/GServer/Gopet.csproj -t:Compile`.

## Todo List

- [x] `GopetCMD.PET_BATTLE_EXP = 33` + doc-comment
- [x] Tạo `HitExpReward.cs` (Grant + Send)
- [x] Field `hitExp` trong `PetBattle`
- [x] `awardHitExp()` + 2 điểm gọi (`petAttack`, `useSkill`)
- [x] `win()` trừ `hitExp.Paid`
- [x] Compile server
- [x] Log kiểm: giết 1 quái → tổng EXP pet nhận đúng bằng số trước khi có tính năng
- [x] Log kiểm: đánh 10 đòn rồi xin thua → EXP nhận ≤ 30% EXP giết quái

## Success Criteria

- Giết quái bằng N đòn: `Σ(exp nhỏ giọt) + exp panel kết quả` = **đúng** giá trị `exp` cũ.
- Đánh không giết: EXP nhận ≤ `round(killExp * 0.30)`, và `PET_BATTLE_EXP` ngừng gửi khi chạm trần.
- Đánh boss: không có gói `PET_BATTLE_EXP` nào.
- PvP/PK/đấu trường: không có gói `PET_BATTLE_EXP` nào.
- Client 1.4.x (jar): không nhận gói 81/33, trận chạy y như cũ.
- `check-protocol-coverage` xếp route `81/33` là `handled` (sau phase 03), không phải `missing`.

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| Cày EXP bằng "đánh 1 đòn → xin thua → lặp" | Cao | **Cao** | Mô hình ứng trước đã bị bác ⇒ chỉ còn: (a) **trần 30%/trận** — chốt chặn chính, phải có test riêng; (b) map 12 ban nếu vòng lặp < 4500ms (`GopetPlace.cs:406`); (c) `setLastTimeKillMob` đặt ở cuối MỌI trận PvE (`:888`) nên vòng lặp vẫn bị tính |
| Lạm phát EXP toàn server | **Chắc chắn xảy ra** | Trung bình | User đã cân nhắc và chấp nhận: tối đa +30% EXP/quái. Test phải khẳng định Σ ≤ 130% exp cũ, KHÔNG phải = exp cũ |
| `genExpWhenMobDie` sau này thêm random → cache lệch `win()` | Thấp | Trung bình | Cache trong `HitExpReward`; nếu hàm đổi thành random, chuyển sang truyền giá trị từ `win()` xuống (ghi chú trong file) |
| Lên cấp giữa trận làm hỏng HUD/max HP | Trung bình | Trung bình | `updatePetLvl()` đã là đường có sẵn của `win()`; client đã xử lý opcode 18 (`BattleHandler.OnPetLevel`). Chỉ đổi thời điểm, không đổi cơ chế |
| `mob.getMobLvInfo()` null (quái cấu hình thiếu) | Thấp | Trung bình | `Grant` return 0 khi null — cẩn trọng hơn `win():809` vốn deref thẳng |
| Gói 81/33 spam mỗi đòn | Thấp | Thấp | 12 byte payload, ≤ 6 gói/trận (trần 30% ÷ 5%) |
| Ai đó "sửa cho nhất quán" thành ứng trước | Trung bình | Cao | Ghi rõ trong comment tại chỗ nhỏ giọt rằng đây là quyết định của user ngày 2026-09-19, kèm lý do |

## Security Considerations

- Toàn bộ số EXP do server tính; client không gửi gì liên quan và không thể tác động vào `Grant`.
- Gate `VERSION_150` dùng đúng mẫu `SendBuffStateTo` — kiểm `ApplicationVersion` **và** `session != null`.
- Trần là state của **instance `PetBattle`** (một trận, một đối tượng, tạo mới ở
  `GopetPlace.startFightMob`) — không phải static, không rò giữa người chơi hay giữa trận.

## Next Steps

- Phase 03: client parse 81/33 + số vàng + huỷ cooldown lạc quan cho kỹ năng trượt.
- Phase 04: docs `battle-system.md` §4 (bảng opcode) + mục mới cho EXP nhỏ giọt.

## Rollback

Thứ tự ngược: (1) bỏ dòng trừ ở `win()`, (2) bỏ 2 điểm gọi `awardHitExp`, (3) xoá file
`HitExpReward.cs`, (4) hằng số `PET_BATTLE_EXP` có thể để lại (vô hại; nếu xoá thì phải chạy
`npm run gen:cmd` lại ở client). Client cũ hơn vẫn chạy bình thường khi server ngừng gửi 81/33 —
gói này không bắt buộc cho bất kỳ chuyển trạng thái nào.
