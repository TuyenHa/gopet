---
phase: 1
title: "Server: skill cho quái từ DB"
status: pending
priority: P1
effort: "4h"
dependencies: []
---

# Phase 1: Server: skill cho quái từ DB

## Overview
Quái có danh sách kỹ năng lưu trong DB, được gửi về client trong gói mở trận và thực
sự dùng trong lượt của quái.

## Requirements
- Functional:
  - Bảng mới `gopet_mob_skill` gán skill cho từng loại quái (theo `petId` template).
  - `writeMobInfo` gửi danh sách skill (định dạng passive `id + name` — jar đã đọc được
    định dạng này trong PvP nên không phá jar).
  - `mobAttack()` chọn skill theo tỉ lệ; hết MP / đang cooldown / không có skill → đánh thường.
- Non-functional: quái không có dòng nào trong bảng → hành vi y như hiện tại.

## Architecture

```sql
CREATE TABLE IF NOT EXISTS gopet_mob_skill (
  petId    INT NOT NULL,          -- = gopet_map_moblvl.petId (template quái)
  skillID  INT NOT NULL,          -- FK logic → skill.skillID
  skillLv  INT NOT NULL DEFAULT 1,-- 1-based, index vào skilllv
  useRate  TINYINT NOT NULL DEFAULT 30, -- % cơ hội dùng skill mỗi lượt
  PRIMARY KEY (petId, skillID)
);
```

- `GopetManager`: `HashMap<int, JArrayList<MobSkill>> MOB_SKILL_HASH_MAP`, nạp cạnh chỗ nạp
  `gopet_mob` (`Manager/GopetManager.cs:1010`) — **sau** khi nạp `skill` (`:1072`) để
  resolve `PetSkill`/`PetSkillLv`. Bỏ qua (log warn) dòng có skillID/lv không tồn tại.
- `Mob.Skills` (property đọc từ map theo `petIdTemplate`). `Boss` kế thừa nên tự có.

### Lỗi phải sửa trong `mobUseSkill()` (`PetBattle.cs:1326`)
Trong trận PvE, thông tin trận của **quái là `passiveBattleInfo`** (xem
`getUserPetBattleInfo()` `:421`), nhưng `mobUseSkill` đang dùng `activeBattleInfo`
(của người chơi) cho: kiểm tra stun, `petBattleInfo`, cooldown. Hệ quả: quái bị stun theo
buff của pet người chơi, buff của quái áp vào pet người chơi. Đổi đúng:
`petBattleInfo = passiveBattleInfo`, `nonPetBattleInfo = activeBattleInfo`.
Đồng thời đồng bộ nhịp với `mobUseNormalAttack()` (đặt `MobAttackTime`, `IsMobFighted`)
thay vì gọi `nextTurn()` trực tiếp — đọc `:640-700` để khớp vòng lặp lượt trước khi sửa.

## Related Code Files
- Create: `SRCGOPETGOC/GServer/Data/mob/MobSkill.cs` (skill + lv + rate)
- Create: `SRCGOPETGOC/GServer/backup_sql/migrations/gopet_mob_skill.sql` (DDL + seed)
- Modify: `Manager/GopetManager.cs` (load), `Data/mob/Mob.cs` (Skills),
  `Data/Battle/PetBattle.cs` (`writeMobInfo`, `mobAttack`, `mobUseSkill`)

## Implementation Steps
1. Kiểm tra thư mục migration hiện có (`backup_sql`, `docker/`) và đặt file SQL theo quy ước sẵn có.
2. Viết DDL + seed: mỗi `petId` trong `gopet_map_moblvl` lấy 2 skill cùng `nClass` với
   template (`skill.nClass`), `skillLv = 1`, `useRate = 30`. Boss: 3 skill, `useRate = 45`.
   Seed bằng `INSERT ... SELECT` để admin chỉnh tay sau.
3. Áp migration vào `gopettae_tae2` qua `docker exec gopet-mariadb mariadb ...`.
4. `MobSkill` + loader + `Mob.Skills`.
5. `writeMobInfo`: thay `putsbyte(0)` bằng count + `(skillID, name theo ngôn ngữ player)`.
   Hàm đang `static` không nhận `Player` → thêm tham số `Player` (2 call site: `:461`, `:522`).
6. Bật lại logic chọn skill trong `mobAttack()`: roll `useRate`, lọc skill không cooldown
   (`passiveBattleInfo.isCoolDown`) và đủ MP; không có → `mobUseNormalAttack()`.
7. Sửa `mobUseSkill` như mục Architecture.
8. `dotnet build` (build ra thư mục temp nếu `Gopet.exe` đang chạy), restart server, đánh
   1 quái có skill và xem log turn.

## Success Criteria
- [ ] Bảng tồn tại, có seed; server khởi động không lỗi, log số dòng nạp.
- [ ] Quái có skill dùng được skill (thấy `skillId` ≠ 0/1/2 trong `TurnEffect` của lượt quái).
- [ ] Buff/stun của quái áp đúng bên (không còn áp nhầm lên pet người chơi).
- [ ] Quái không có dòng trong bảng vẫn đánh thường như cũ.
- [ ] Jar (1.4.2) vẫn mở được trận PvE.

## Risk Assessment
- Skill của quái quá mạnh → cân bằng qua `skillLv`/`useRate` trong DB, không hardcode.
- Sửa `mobUseSkill` đổi nhịp lượt → test kỹ timeout lượt (25s) và chuỗi skill liên tiếp.
- `skill` có skill cần thẻ (`IsNeedCard`) — seed loại các skill này.
