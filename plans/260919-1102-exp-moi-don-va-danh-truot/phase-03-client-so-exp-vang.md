# Phase 03 — Client: số EXP vàng + huỷ cooldown khi kỹ năng trượt

## Context Links

- [plan.md](plan.md) · [phase-02](phase-02-server-exp-moi-don.md)
- `docs/battle-system.md` §2.6 (hàng đợi hoạt cảnh), §2.5 (vì sao hoãn snapshot)
- Mockup: `plans/260917-2238-pet-vs-mob-battle-screen/visuals/battle-screen-mockup.png`
  — `+12` vàng trên đầu pet đánh, `-95` đỏ trên đầu bên trúng đòn.

## Overview

- **Priority:** P2
- **Status:** done
- **Mô tả:** Parse `PET_BATTLE_EXP` (81/33), hiện số EXP **vàng/gold** trên đầu pet mình, và huỷ
  cooldown lạc quan khi server báo kỹ năng trượt.

## Key Insights

1. `Assets/Scripts/Net/GopetCmd.cs` là **auto-generated** (`tools/gen-gopet-cmd`). Không sửa tay:
   thêm hằng số ở server rồi chạy `npm run gen:cmd` (hoặc `node tools/gen-gopet-cmd/index.js`).
   `verify.ps1` bước 1/10 chạy `--check` và sẽ đỏ nếu quên.
2. `verify.ps1` bước 10/10 **đã đỏ sẵn ở baseline** với 16 file (đã đo 2026-09-19), trong đó có
   `GopetCmd.cs` (204 dòng). Sinh lại thành 205 dòng **không thêm tên mới** vào danh sách vi phạm.
   Tiêu chí của phase này: **không có tên file MỚI** xuất hiện trong output bước 10.
   Không thêm tên nào vào `CODE_HEALTH_EXCEPTIONS.md`.
3. Ngân sách dòng các file sẽ đụng (đo 2026-09-19):
   `BattleView.cs` 186 (+3 → 189), `BattleView.State.cs` 63, `BattleView.Actions.cs` 80,
   `BattleFloatText.cs` 60, `BattleHandler.cs` 174 (+10 → 184), `BattleModels.cs` 90,
   `SkillCooldownTracker.cs` 55. Tất cả còn dư.
4. `BattleView` tự subscribe thẳng vào handler trong `Create()` (`BattleView.cs:66-67`) và gỡ ở
   `OnDestroy()` (`:71-76`) ⇒ **không cần đụng `GameSession.cs`** (file 693 dòng, legacy).
5. Số nổi phải **xếp hàng** như `_pendingVitals` (`BattleView.cs:98-113`): gói 81/33 tới cùng tick
   với gói 37 nhưng hoạt cảnh lượt còn đang chạy. Hiện ngay → số EXP nhảy ra trước cả cú lao.
6. Client `MarkUsed(skillId)` lạc quan lúc bấm (`BattleView.Actions.cs:97`). Sau phase 01, kỹ năng
   trượt **không** vào cooldown ở server ⇒ phải huỷ, nếu không nút xám 3 lượt oan.
   Gói trượt của kỹ năng (phase 01 bước 4) cố tình **không mang `skillId`**, chỉ có effect
   `SKILL_MISS` (skillId = 1) ⇒ phân biệt được với kỹ năng nổ thành công.

## Requirements

**Chức năng**
- Nhận 81/33, khớp `battleId`, hiện `+N` **màu vàng** trên đầu card pet mình.
- Nhiều gói trong một lượt thì cộng dồn thành một số.
- Kỹ năng trượt → nút kỹ năng đó bấm lại được ngay lượt sau.

**Phi chức năng**
- Mọi file đụng vào giữ ≤ 200 dòng.
- Logic thuần C# (`SkillCooldownTracker`) test được ở `tests/Gopet.Net.Tests`.
- Client **không** tự tính EXP; chỉ hiển thị số server gửi.

## Architecture

```
MessageRouter ──81/33──> BattleHandler.OnHitExp
                              └─> HitExpAwarded(BattleHitExp{BattleId, ActorId, Exp})
                                        │
BattleView.OnHitExp ── battleId khớp? ──> _pendingExp += exp
                                        │
BattleView.Update (animator.Idle) ──> BattleFloatText.CreateExp(_left.transform, _pendingExp)

BattleView.Apply(turn):
  turn.ActorId == LocalPet.ActorId && có effect SkillId==1  ──> _cooldowns.CancelLastUsed()
```

## Related Code Files

**Sửa**
- `Assets/Scripts/Net/GopetCmd.cs` — **sinh lại**, không sửa tay
- `Assets/Scripts/Net/Battle/BattleModels.cs` — thêm `BattleHitExp`
- `Assets/Scripts/Net/Battle/BattleHandler.cs` — event + `RegisterSub` + `OnHitExp`
- `Assets/Scripts/UiLogic/SkillCooldownTracker.cs` — `CancelLastUsed()`
- `Assets/Scripts/Runtime/World/BattleFloatText.cs` — `CreateExp()`
- `Assets/Scripts/Runtime/World/BattleView.cs` — subscribe/unsubscribe + 1 dòng trong `Update`
- `Assets/Scripts/Runtime/World/BattleView.State.cs` — field `_pendingExp` + `OnHitExp` + `FlushExp`
- `Assets/Scripts/Runtime/World/BattleView.Actions.cs` — không đổi (chỉ đọc lại để xác nhận đường trượt)

**Tạo / xoá:** không (test ở phase 04).

## Implementation Steps

1. Sinh lại opcode:
   ```
   cd GopetUnityClient/tools && node gen-gopet-cmd/index.js
   ```
   Xác nhận `GopetCmd.cs` có `public const sbyte PET_BATTLE_EXP = 33;`.

2. `BattleModels.cs` — thêm:
   ```csharp
   /// <summary>EXP nhỏ giọt mỗi đòn trúng (PvE). Server quyết định số; client chỉ vẽ.</summary>
   public sealed class BattleHitExp
   {
       public int BattleId, ActorId, Exp;
   }
   ```

3. `BattleHandler.cs`:
   - `public event Action<BattleHitExp> HitExpAwarded;`
   - Trong `RegisterOn`: `router.RegisterSub(GopetCmd.PET_SERVICE, GopetCmd.PET_BATTLE_EXP, OnHitExp);`
   - ```csharp
     private void OnHitExp(Message message)
     {
         var r = message.Reader;
         var state = new BattleHitExp { BattleId = r.ReadInt(), ActorId = r.ReadInt(), Exp = r.ReadInt() };
         r.ExpectFullyConsumed("PET_BATTLE_EXP");
         HitExpAwarded?.Invoke(state);
     }
     ```

4. `BattleFloatText.cs` — thêm bên cạnh `CreateMiss`:
   ```csharp
   /// <summary>Số EXP nhỏ giọt mỗi đòn trúng — vàng/gold, khớp mockup ("+12" trên đầu pet đánh).
   /// Tách khỏi Create() vì Create() ánh xạ màu theo dấu (xanh hồi / đỏ mất), còn EXP luôn dương.</summary>
   public static void CreateExp(Transform parent, int exp) =>
       Spawn(parent, "EXP nổi", $"+{exp}", new Color(1f, 0.82f, 0.2f), 0f);
   ```
   Không đổi `Create()` — tránh đụng đường HP/MP đang chạy đúng.

5. `BattleView.State.cs`:
   ```csharp
   private int _pendingExp;

   /// <summary>Gói 81/33 tới cùng tick với gói lượt nhưng hoạt cảnh còn chạy — xếp hàng như
   /// _pendingVitals, nếu không số EXP nhảy ra trước cả cú lao. Cộng dồn nhiều gói thành một số.</summary>
   private void OnHitExp(BattleHitExp state)
   {
       if (state == null || state.BattleId != BattleId) return;
       _pendingExp += state.Exp;
   }

   private void FlushPendingExp()
   {
       var exp = _pendingExp;
       _pendingExp = 0;
       var card = Card(_start.LocalPet.ActorId);
       if (card != null) BattleFloatText.CreateExp(card.transform, exp);
   }
   ```

6. `BattleView.cs`:
   - `Create()`: `handler.HitExpAwarded += view.OnHitExp;`
   - `OnDestroy()`: `_handler.HitExpAwarded -= OnHitExp;`
   - `Update()`, ngay sau dòng `_pendingVitals`:
     ```csharp
     if (_pendingExp > 0 && (_animator == null || _animator.Idle)) FlushPendingExp();
     ```

7. `SkillCooldownTracker.cs` — nhớ skill vừa bấm và huỷ được:
   ```csharp
   private int _lastUsed = -1;

   public void MarkUsed(int skillId) { _remaining[skillId] = _initialTurns; _lastUsed = skillId; }

   /// <summary>Server báo đòn vừa rồi TRƯỢT. Kỹ năng trượt không vào cooldown ở server
   /// (PetBattle.useSkill roll miss TRƯỚC khi addSkillCoolDown), nên bỏ cooldown lạc quan
   /// client đã đặt lúc bấm — nếu không nút xám oan 3 lượt. No-op nếu lượt đó là đánh thường.</summary>
   public void CancelLastUsed()
   {
       if (_lastUsed < 0) return;
       _remaining.Remove(_lastUsed);
       _lastUsed = -1;
   }
   ```
   `Reset()` đặt `_lastUsed = -1`. `OnTurnAdvanced` khi xoá hết cooldown cũng nên clear
   `_lastUsed` nếu skill đó không còn trong `_remaining` (tránh huỷ nhầm ở lượt xa sau này) —
   đơn giản: trong `OnTurnAdvanced`, sau vòng lặp, `if (!_remaining.ContainsKey(_lastUsed)) _lastUsed = -1;`.

8. `BattleView.Apply` (`BattleView.cs:78-88`) — thêm trước `_animator?.Enqueue(turn)`:
   ```csharp
   if (turn.ActorId == _start.LocalPet.ActorId && HasMiss(turn)) _cooldowns.CancelLastUsed();
   ```
   `HasMiss` là static helper 3 dòng trong `BattleView.State.cs`:
   duyệt `turn.Effects`, trả true nếu có `effect.SkillId == 1` (`TurnEffect.SKILL_MISS`).

9. `powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1`.

## Todo List

- [x] `node tools/gen-gopet-cmd/index.js` → `PET_BATTLE_EXP` xuất hiện ở `GopetCmd.cs`
- [x] `BattleHitExp` trong `BattleModels.cs`
- [x] `HitExpAwarded` + `RegisterSub` + `OnHitExp` ở `BattleHandler.cs`
- [x] `BattleFloatText.CreateExp` (vàng)
- [x] `_pendingExp` + `OnHitExp` + `FlushPendingExp` ở `BattleView.State.cs`
- [x] Subscribe/unsubscribe + 1 dòng `Update` ở `BattleView.cs`
- [x] `CancelLastUsed` ở `SkillCooldownTracker.cs` + gọi trong `BattleView.Apply`
- [x] `verify.ps1`: bước 1 xanh, bước 10 **không có tên file mới**
- [x] Chạy thật: đánh quái thấy `+N` vàng trên đầu pet, `-N` đỏ trên đầu quái

## Success Criteria

- Đánh trúng: số vàng `+N` hiện trên đầu pet mình **sau** khi hoạt cảnh lượt xong, không chồng
  lên số damage đỏ của quái.
- Đánh trượt: chữ "TRƯỢT" vàng trên đầu bên bị đánh, **không** có số EXP.
- Kỹ năng trượt: MP không giảm trên HUD, nút kỹ năng sáng lại ở lượt kế tiếp.
- `check-protocol-coverage` báo `81/33` = `handled`.
- `verify.ps1` bước 1-9 xanh; bước 10 giữ nguyên đúng 16 tên cũ (`GopetCmd.cs` vẫn trong danh sách,
  không có tên mới).

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| Sửa tay `GopetCmd.cs` → bước 1 đỏ | Trung bình | Thấp | Bước 1 ghi rõ dùng generator |
| Một file battle vượt 200 dòng | Trung bình | Trung bình | Đã tính ngân sách (mục Key Insights #3); nếu chạm trần thì đẩy code sang `BattleView.State.cs` (63 dòng) |
| `FlushPendingExp` không bao giờ chạy vì animator kẹt | Thấp | Thấp | Cùng điều kiện với `_pendingVitals` đã chạy ổn; `Drain()` có `try/finally` xoá cờ |
| `CancelLastUsed` huỷ nhầm khi đòn **thường** trượt ngay sau một kỹ năng thành công | Trung bình | Thấp | Bước 7: `OnTurnAdvanced` xoá `_lastUsed` khi skill đó không còn cooldown; sai sót còn lại chỉ mở khoá sớm 1 kỹ năng, server vẫn từ chối bằng `redDialog` + `ResyncDenied` |
| Gói 81/33 tới khi overlay đã đóng | Trung bình | Thấp | `OnHitExp` lọc theo `BattleId`; `OnDestroy` gỡ subscribe |
| Rollback client mà server còn gửi 81/33 | Thấp | Thấp | `MessageRouter` đưa sub lạ vào `OnUnhandled` (`MessageRouter.cs:97`) — không ném |

## Security Considerations

- Client chỉ **hiển thị**; không cộng EXP cục bộ, không gửi gói nào cho tính năng này.
- `ExpectFullyConsumed` chặn payload thừa/thiếu — cùng chuẩn với các handler battle khác.
- Không dựng cooldown/MP từ dữ liệu client tự đoán vượt quá những gì server xác nhận.

## Next Steps

- Phase 04: test đơn vị cho parse gói + `CancelLastUsed`, cập nhật `docs/battle-system.md`.

## Rollback

`git checkout --` 7 file trên rồi chạy lại `node tools/gen-gopet-cmd/index.js` nếu server vẫn giữ
hằng số. Server tiếp tục gửi 81/33 — client cũ bỏ qua qua `OnUnhandled`, trận vẫn chạy.
