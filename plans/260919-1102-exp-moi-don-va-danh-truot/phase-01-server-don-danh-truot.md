# Phase 01 — Server: bật đòn đánh trượt thật

## Context Links

- [plan.md](plan.md)
- `docs/battle-system.md` §2 (vòng lượt), §2.3 (`sendTurnSkipped` đã dùng đúng byte-shape đòn trượt)
- Plan trước: `plans/260919-1102-man-hinh-danh-quai-theo-luot/phase-01-vong-luot-va-khoa-nut.md`

## Overview

- **Priority:** P2
- **Status:** done
- **Mô tả:** Gọi `GameObject.IsMiss()` ở cả 4 đường ra đòn. Kỹ năng bị trượt thì **hoàn MP và
  không vào cooldown** — hiện tại trừ trước, roll sau.

## Key Insights

1. `randMiss()` (`PetBattle.cs:1181-1185`) chỉ trả true khi đối phương có buff
   `MISS_IN_99999_TURN`. Không buff → **luôn trúng**. Đây là lý do "không bao giờ trượt".
2. `IsMiss` (`Base/GameObject.cs:143-146`) = `NextFloatPer() > HitRate(B)`.
   `NextFloatPer()` trả 0..100 (`Util/Utilities.cs:163-166`);
   `HitRate = (100 + agi/1000) − (15 + agiB/1000)`. `getAgi()` là `int` nên agi<1000 chia nguyên
   ra 0 ⇒ HitRate = 85 ⇒ **15% trượt**. Không viết công thức mới, không hằng số hoá.
3. `ActiveObject` / `PassiveObject` (`PetBattle.cs:433-446`) đã tự đảo theo `isActiveTurn`, nên
   **một helper duy nhất** dùng được cho cả pet đánh quái lẫn quái đánh pet. Thời điểm gọi phải
   **trước** `nextTurn()` (mọi chỗ đều vậy).
4. Client không cần đổi gì để *thấy* đòn trượt: `TurnEffect.SKILL_MISS = 1` →
   `BattleView.Actions.cs:132-141` gọi `CreateMiss()` + `s_attack_miss`;
   `BattleEffectView.ResolveName(1)` trả `null` ⇒ không vẽ sprite hiệu ứng (đúng ý: trượt thì
   không có vệt chém). `HitsOpponent` vẫn true (effect trỏ vào bên kia) nên pet vẫn lao sang
   rồi hụt — đúng cảm giác.
5. **Kỹ năng buff/hỗ trợ KHÔNG roll trượt.** `petSkill.isSkillBuff()` đã có sẵn
   (dùng ở `:1130`). Buff lên chính mình mà "trượt" là vô nghĩa, và né được câu hỏi
   "có nên chạy `applySkill` khi trượt không".

## Requirements

**Chức năng**
- Đòn đánh thường của pet có thể trượt.
- Đòn đánh thường của quái có thể trượt.
- Kỹ năng gây sát thương (pet và quái) có thể trượt; kỹ năng buff thì không.
- Kỹ năng trượt: **không trừ MP, không vào cooldown**, lượt vẫn chuyển.

**Phi chức năng**
- Wire format KHÔNG đổi ở phase này (không opcode mới) — jar 1.4.x render được nguyên xi.
- **CHỈ PvE đánh quái** — user chốt 2026-09-19. Guard `petAttackMob` trong `rollMiss`.
  PvP/PK/đấu trường giữ nguyên hành vi cũ (chỉ trượt khi có buff né), không đổi cân bằng.
  (Bản nháp đầu của phase này áp cho mọi loại trận — đã bị bác.) Ghi chú cũ giữ lại để tham khảo:
- ~~Áp dụng cho **mọi loại trận** (PvE, PvP, PK, đấu trường): `IsMiss` là cơ chế chiến đấu chung,
  không phải nguồn tài nguyên. Giới hạn "chỉ PvE" của user là dành cho EXP (phase 02).

## Architecture

```
petAttack / useSkill / mobUseNormalAttack / mobUseSkill
        │
        └─> rollMiss(nonPetBattleInfo)
                 ├─ randMiss(nonInfo)                 (buff MISS_IN_99999_TURN — giữ nguyên)
                 └─ ActiveObject.IsMiss(PassiveObject) (MỚI — 15% base theo agi)
```

Luồng kỹ năng sau khi đảo thứ tự:

```
đủ MP?  ──no──> redDialog "không đủ thể lực"
   │yes
roll miss (bỏ qua nếu isSkillBuff)
   ├─ MISS ─> gửi gói trượt (shape y hệt đòn thường trượt) ─> nextTurn() ─> return
   │          KHÔNG trừ MP, KHÔNG addSkillCoolDown, KHÔNG applySkill
   └─ HIT  ─> trừ MP ─> addSkillCoolDown ─> applySkill ─> makeDamage ─> ... (như cũ)
```

## Related Code Files

**Sửa**
- `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs`

**Tạo / xoá:** không.

## Implementation Steps

1. Thêm helper ngay dưới `randMiss` (`PetBattle.cs:1185`):

   ```csharp
   /// <summary>Roll trượt của một đòn. Gộp hai nguồn:
   /// - randMiss: buff MISS_IN_99999_TURN của đối phương (hành vi cũ, giữ nguyên);
   /// - GameObject.IsMiss: tỉ lệ trượt cơ bản theo agi (Base/GameObject.cs:143) — trước
   ///   đây viết ra nhưng KHÔNG nơi nào gọi, nên trận đấu chưa bao giờ trượt.
   /// ActiveObject/PassiveObject tự đảo theo isActiveTurn (:433-446) nên helper này dùng
   /// chung cho cả pet đánh quái lẫn quái đánh pet — phải gọi TRƯỚC nextTurn().</summary>
   private bool rollMiss(PetBattleInfo nonPetBattleInfo)
       => randMiss(nonPetBattleInfo) || (petAttackMob && ActiveObject.IsMiss(PassiveObject));
   ```

2. `petAttack` `:261` — đổi `randMiss(...)` → `rollMiss(...)`. Không đổi gì khác: nhánh `else`
   ngay dưới đã add `TurnEffect.SKILL_MISS` tại `getFocus()`.

3. `mobUseNormalAttack` `:1290` — đổi `randMiss(...)` → `rollMiss(...)`.
   **Giữ nguyên** override `isMiss = false` của boss sinh nhật ở dưới (nó nằm sau dòng này).

4. `useSkill` — chèn khối trượt **trước** `:1059` (`pet.mp -= petSkillLv.mpLost;`),
   tức ngay sau `if (pet.mp - petSkillLv.mpLost >= 0) {`:

   ```csharp
   // Trượt phải roll TRƯỚC khi trừ MP và vào cooldown: kỹ năng trượt thì hoàn toàn
   // không tốn gì. Thứ tự cũ (trừ MP :1059 → cooldown :1060 → roll :1062) khiến người
   // chơi mất 3 lượt hồi cho một đòn không xảy ra.
   // Kỹ năng buff/hỗ trợ không roll trượt — buff lên chính mình mà "trượt" là vô nghĩa.
   if (!petSkill.isSkillBuff() && rollMiss(nonPetBattleInfo))
   {
       JArrayList<TurnEffect> missEffects = new();
       missEffects.add(new TurnEffect(TurnEffect.SKILL_MISS, getFocus(), TurnEffect.SKILL_MISS, 0, 0));
       sendPetAttack(missEffects, TurnEffect.createNormalAttack(activePet.mp, 0, getUserTurnId()));
       nextTurn();
       return;
   }
   ```

   Lý do dùng shape `createNormalAttack` + 1 effect `SKILL_MISS` thay vì shape `WAIT` của kỹ năng:
   byte-shape trùng đúng đòn thường trượt nên jar cũ và Unity đều đã render được, và **không mang
   `skillId`** — client nhờ đó biết kỹ năng không thực sự nổ (xem phase 03, huỷ cooldown lạc quan).

5. `mobUseSkill` — chèn khối tương tự **trước** `:1391` (`mob.mp -= petSkillLv.mpLost;`),
   dùng `skill.isSkillBuff()` và `TurnEffect.createNormalAttack(mob.mp, 0, getUserTurnId())`;
   sau `sendPetAttack` đặt `this.MobAttackTime = DateTime.Now.AddSeconds(2); this.IsMobFighted = true;`
   rồi `return;` — **không** gọi `nextTurn()` (khớp nhánh thành công `:1431-1434` cũng không gọi;
   vòng lượt của quái do `update()` điều khiển).

6. Không đụng `randMiss`, `makeDamage`, `PetDamgeInfo.isSkillMiss` — `SKILL_MISS` của skillInfo là
   cơ chế riêng, vẫn chạy song song.

## Todo List

- [x] Thêm `rollMiss` dưới `randMiss`
- [x] `petAttack:261` dùng `rollMiss`
- [x] `mobUseNormalAttack:1290` dùng `rollMiss`
- [x] `useSkill`: chèn khối trượt trước `:1059`, xác nhận MP/cooldown không bị đụng
- [x] `mobUseSkill`: chèn khối trượt trước `:1391`
- [x] `dotnet msbuild SRCGOPETGOC/GServer/Gopet.csproj -t:Compile`
- [x] Chạy server, đánh 20 đòn thường: đếm được ~3 lần chữ "TRƯỢT" vàng trên đầu quái
- [x] Dùng kỹ năng tới khi trượt: MP không giảm, nút kỹ năng **vẫn bấm được ngay lượt sau** (sau phase 03)

## Success Criteria

- Log/quan sát: tỉ lệ trượt đòn thường ≈ 15% ± sai số mẫu (agi hai bên < 1000).
- Kỹ năng trượt: `pet.mp` không đổi trước/sau; `petBattleInfo.isCoolDown(skillId)` trả `false`.
- Kỹ năng buff không bao giờ trượt.
- Không có gói 37 nào bị nuốt: mỗi hành động đều sinh đúng một gói lượt (nếu không, nút client
  kẹt tới watchdog 8s).

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| ~~PvP/đấu trường đổi cân bằng~~ | — | — | **ĐÃ LOẠI TRỪ**: `rollMiss` guard `petAttackMob` nên PvP/đấu trường không đụng tới |
| Quên `nextTurn()` ở nhánh kỹ năng trượt → client kẹt 25s | Trung bình | Cao | Bước 4 ghi rõ; test thủ công "dùng kỹ năng tới khi trượt" |
| `mobUseSkill` gọi thừa `nextTurn()` → mất lượt người chơi | Trung bình | Cao | Bước 5 nói rõ KHÔNG gọi; đối chiếu nhánh thành công `:1431-1434` |
| Boss sinh nhật mất override `isMiss=false` | Thấp | Trung bình | Chỉ đổi tên hàm ở `:1290`, override nằm sau |
| `ActiveObject` sai vế nếu gọi sau `nextTurn()` | Thấp | Cao | Cả 4 chỗ chèn đều nằm trước `nextTurn()`; ghi trong doc-comment của `rollMiss` |

## Security Considerations

- Roll trượt hoàn toàn server-side; client không gửi và không đọc tham số nào của roll.
- Nhánh trượt của kỹ năng **return sớm** trước mọi thay đổi trạng thái ⇒ không có đường nào
  client ép server trừ MP mà không cooldown hay ngược lại.

## Next Steps

- Chặn phase 02 (cùng file `PetBattle.cs`).
- Phase 03 phải huỷ cooldown lạc quan phía client cho kỹ năng trượt.

## Rollback

`git checkout -- SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs` (phase 01 chỉ đụng 1 file, không
đổi wire format, không đổi schema) → về hành vi "không bao giờ trượt" ngay lập tức, client không
cần build lại.
