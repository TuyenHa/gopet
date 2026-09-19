# Phase 03 — Server trừ 10% EXP khi thua PvE

## Context Links

- Overview: [plan.md](plan.md)
- Report: `plans/reports/investigation-260919-1102-battle-screen-6-yeu-cau.md` §5
- Doc giao thức: `docs/battle-system.md` §6 (`PET_BATTLE_STATS` và xin thua)

## Overview

- **Priority:** P1
- **Status:** done · **Chặn bởi:** không (chạy song song 01/02, khác repo path)
- **Effort:** 1h
- Nhánh PvE thua trong `PetBattle.win()` hiện chỉ đặt `delayTimeHealPet`, không trừ EXP.
  Thêm trừ **10% EXP của cấp hiện tại** cho **mọi lần thua PvE** (hết máu lẫn chủ động xin thua).

## Key Insights

Đã verify lại trực tiếp trên source:

1. **Vị trí chính xác của nhánh thua PvE** — `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs:856-859`:
   ```csharp
   else
   {
       activePlayer.controller.delayTimeHealPet = Utilities.CurrentTimeMillis + GopetManager.TIME_DELAY_HEAL_WHEN_MOB_KILL_PET;
   }
   win(petBattleTexts.ToArray(), coin, exp);   // :860
   ```
   `win(...)` được gọi ở `:860` — **sau** khối `else` → text thêm vào `petBattleTexts` trong `else` vẫn kịp gửi.

2. **Cả 2 kiểu thua đều rơi vào đúng nhánh này.** `getWinId()` (`:957-959`):
   ```
   isClose ? (petAttackMob ? mob.getMobId() : ...) : (petAttackMob ? (mob.hp <= 0 ? activePlayer.user.user_id : mob.getMobId()) : ...)
   ```
   - Xin thua → `surrender()` (`:1870-1879`) đặt `isClose = true` → `getWinId() == mob.getMobId()`
   - Pet chết → `mob.hp > 0` → `getWinId() == mob.getMobId()`
   Cả hai `!= activePlayer.user.user_id` → vào `else` ở `:856`. **Một chỗ sửa phủ cả hai.**

3. **Công thức PK đang dùng** (`:876-885`): `expCurrentLvl = GopetManager.PetExp.get(nonPet.lvl)`
   rồi `Utilities.round(Utilities.GetValueFromPercent(expCurrentLvl, 10f))` (nhánh `pkPoint > 0`).
   Dùng lại y nguyên cho PvE, không nhân với `pkPoint`.

4. **`Pet.subExpPK(long)` đã có sẵn** (`Pet.cs:452-462`) — trừ và kẹp sàn
   `GopetManager.MIN_PET_EXP_PK = -20000000` (`GopetManager.cs:557`). Dùng lại (DRY),
   không viết hàm trừ mới.

5. **EXP âm KHÔNG hạ cấp.** `subExpPK` chỉ đổi `pet.exp`; `Pet.lvl` không tính lại và
   `updatePetLvl()` không được gọi trong nhánh thua. Hành vi này giống hệt PK hiện hành → nhất quán.

6. **Có sẵn đường mang thông báo về client, KHÔNG cần đổi wire.**
   `win(Popup[], coin, exp, battleId)` (`:937-955`) ghi `putsbyte(petBattleTexts.Length)` rồi mỗi
   text là `putUTF(text); putUTF("2")`. Client `BattleHandler.OnResult:133-144` đọc đúng cặp đó vào
   `result.Messages`, `BattleResultPanel.cs:31` in ra. Jar cũ cũng đọc được.
   → **Không cần gate `VERSION_150`.** Chỉ cần `petBattleTexts.add(new PetBattleText("..."))`.

7. **Không dùng trường `exp` của gói 16 để mang số âm.** `exp` ở nhánh thua = 0 và client hiển thị
   "EXP: {n}" như phần thưởng — nhét số âm vào sẽ hiển thị sai nghĩa ở cả jar lẫn Unity.
   `PetBattleText` là chỗ đúng.

8. **`GopetManager.PetExp` là `HashMap<int,int>`** (`GopetManager.cs:255`). Cấp không có trong map
   trả 0 → phải guard `expSub > 0` trước khi trừ/thêm text.

## Requirements

**Chức năng**
- R1: Thua PvE (hết máu HOẶC xin thua HOẶC bấm "‹ Quay lại") → pet bị trừ `round(10% × PetExp[lvl])`.
- R2: Người chơi thấy dòng "Pet bị trừ N exp" trong panel kết quả.
- R3: Thắng PvE không bị trừ gì.
- R4: PvP/PK/đấu trường giữ nguyên hành vi cũ.

**Phi chức năng**
- N1: Không đổi wire format bất kỳ opcode nào.
- N2: Dùng lại `subExpPK`, không thêm hàm trừ EXP thứ hai.
- N3: `dotnet build` sạch.

## Architecture

```
mob giết pet   ─┐
xin thua (sub 5)─┴→ hasWinner() → win()  [PetBattle.cs:760]
                       └─ petAttackMob == true
                            └─ getWinId() != activePlayer.user_id      [:775 else]
                                 ├─ delayTimeHealPet = now + TIME_DELAY_HEAL_WHEN_MOB_KILL_PET
                                 ├─ expSub = round(10% × GopetManager.PetExp[activePet.lvl])   ◄ MỚI
                                 ├─ activePet.subExpPK(expSub)                                  ◄ MỚI
                                 └─ petBattleTexts.add("Pet bị trừ N exp")                      ◄ MỚI
                       └─ win(petBattleTexts.ToArray(), coin, exp)      [:860]
                            └─ opcode 16 PET_BATTLE_STATE (wire KHÔNG đổi)
                                 └─ client BattleHandler.OnResult → BattleResult.Messages
                                      └─ BattleResultPanel dòng thân
                       └─ activePlayer.controller.sendMyPetInfo()       [:929]
```

Không có nhánh nào khác gọi tới chỗ sửa → bán kính ảnh hưởng đúng bằng "PvE thua".

## Related Code Files

**Sửa**
- `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs` — duy nhất khối `else` tại `:856-859`

**Đọc để đối chiếu (không sửa)**
- `SRCGOPETGOC/GServer/Data/pet/Pet.cs:452-462` (`subExpPK`)
- `SRCGOPETGOC/GServer/Manager/GopetManager.cs:255` (`PetExp`), `:557` (`MIN_PET_EXP_PK`)
- `SRCGOPETGOC/GServer/Data/Battle/PetBattleText.cs`
- `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs:876-899` (mẫu công thức PK)

**Tạo / Xoá:** không.

## Implementation Steps

1. Mở `PetBattle.cs`, tới khối `else` ở `:856`. Thay bằng:
   ```csharp
   else
   {
       activePlayer.controller.delayTimeHealPet = Utilities.CurrentTimeMillis + GopetManager.TIME_DELAY_HEAL_WHEN_MOB_KILL_PET;
       // Thua PvE (hết máu hoặc xin thua) trừ 10% exp của cấp hiện tại — cùng công thức
       // nhánh PK ở :876-885. subExpPK kẹp sàn MIN_PET_EXP_PK và KHÔNG hạ cấp.
       //
       // Nhánh này CHỈ đạt được khi thua thật, không cần guard thêm:
       //   getWinId() != activePlayer  ⟺  isClose || mob.hp > 0
       //   mà hasWinner() (:719-722) đòi  mob.hp <= 0 || activePet.hp <= 0 || isClose
       //   ⇒ mob.hp > 0 kéo theo activePet.hp <= 0 (pet gục).
       // Boss bị người khác kết liễu KHÔNG rơi vào đây: mob.hp <= 0 nên getWinId() trả
       // activePlayer ⇒ đi nhánh thắng :775, vào else boss :832, không phải lastHitPlayer
       // nên không nhận gì rồi thoát.
       // Rời map/disconnect cũng KHÔNG rơi vào đây: Close() (:962-967) gọi clean() (:991)
       // gỡ trận khỏi place.petBattles ⇒ update() ngừng chạy ⇒ win() không bao giờ tới.
       long expCurrentLvl = GopetManager.PetExp.get(activePet.lvl);
       long expSub = Utilities.round(Utilities.GetValueFromPercent(expCurrentLvl, 10f));
       if (expSub > 0)
       {
           activePet.subExpPK(expSub);
           petBattleTexts.add(new PetBattleText(
               Utilities.Format("Pet bị trừ %s exp", Utilities.FormatNumber(expSub))));
       }
   }
   ```
2. Kiểm tra kiểu trả về của `Utilities.round` và `Utilities.GetValueFromPercent` khớp với cách
   dùng ở `:879` (`long exp_sub = Utilities.round(Utilities.GetValueFromPercent(expCurrentLvl, 10f));`)
   — nếu overload khác nhau do `expCurrentLvl` là `long` vs `int`, ép kiểu đúng như dòng `:876-879`.
3. KHÔNG thêm `activePlayer.okDialog(...)` — panel kết quả đã hiện thông báo, dialog thứ hai
   chồng lên overlay là nhiễu (khác PK: PK không có panel kết quả kiểu này).
4. KHÔNG đụng `win(Popup[], int, int, int)` (`:937-955`) — wire giữ nguyên.
5. Compile: `dotnet build SRCGOPETGOC/GServer/Gopet.csproj`

## Todo List

- [x] Sửa khối `else` `PetBattle.cs:856-859`
- [x] Đối chiếu overload `Utilities.round` / `GetValueFromPercent` với `:876-885`
- [x] Xác nhận không có `okDialog` thêm, không đổi `win(...)` wire
- [x] `dotnet build SRCGOPETGOC/GServer/Gopet.csproj` sạch
- [ ] Kiểm tra DB: `pet.exp` giảm đúng sau 1 trận thua — **cần chạy game thật, chưa làm**

## Success Criteria

| # | Đo được bằng |
|---|---|
| S1 | Thua do hết máu → `pet.exp` trong DB giảm đúng `round(0.1 × PetExp[lvl])` |
| S2 | Xin thua → giảm đúng cùng số đó |
| S3 | Bấm "‹ Quay lại" → xác nhận → giảm đúng cùng số đó |
| S4 | Panel kết quả hiện dòng "Pet bị trừ N exp" |
| S5 | Thắng quái → `pet.exp` chỉ tăng, không có dòng trừ |
| S6 | PvP/PK 1 trận → số exp trừ không đổi so với trước khi sửa |
| S7 | Pet cấp có `PetExp` = 0/thiếu → không trừ, không hiện dòng thừa, không exception |
| S8 | `dotnet build` 0 error |

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| EXP âm gây hỏng hiển thị/tính cấp ở nơi khác | Thấp | Trung bình | `subExpPK` kẹp sàn `MIN_PET_EXP_PK`; PK đã chạy cơ chế này từ trước → đường dẫn đã được chứng minh |
| Người chơi thấy hình phạt quá nặng khi cày quái | Trung bình | Trung bình | Quyết định đã chốt (10%). Số nằm ở 1 chỗ, đổi percent là 1 dòng nếu cần cân bằng lại |
| `PetExp.get(lvl)` thiếu key → 0 | Trung bình | Thấp | Guard `expSub > 0` (bước 1) |
| ~~Nhánh `else` bắt nhầm Boss do người khác hạ~~ | — | — | **ĐÃ LOẠI TRỪ** bằng đọc code (`getWinId()` `:957-959` + `hasWinner()` `:719-722` + `clean()` `:991`). Boss do người khác hạ đi nhánh thắng; rời map thì trận đã bị gỡ khỏi `place.petBattles`. Vẫn giữ 2 ca này trong test matrix để xác nhận thực địa |
| Jar cũ vỡ vì text mới | Rất thấp | Cao | Không đổi wire; `petBattleTexts` là danh sách có độ dài động jar vốn đã duyệt (`e.java` case 8) |

## Security Considerations

- Thay đổi hoàn toàn server-side; client không gửi thêm gì. Không có đường cho client
  tự khai báo số EXP bị trừ.
- Không có rủi ro trừ hai lần: `win()` mở đầu bằng guard `hadFinished` (`:762-769`) và
  `surrender()` cũng `if (hadFinished) return;` (`:1872`).
- Số EXP trừ tính từ `activePet.lvl` (dữ liệu server), không từ gói nào của client.

## Next Steps

- Chặn: phase 04 (test matrix + cập nhật `docs/battle-system.md` mục xin thua).
- Triển khai: server cần restart; client không cần build lại cho riêng phase này.
