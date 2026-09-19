# Code Review — Phase 01: Vòng lượt + khoá nút theo lượt

- Plan: `plans/260919-1102-man-hinh-danh-quai-theo-luot/phase-01-vong-luot-va-khoa-nut.md`
- Ngày: 2026-09-19 · Nhánh: master · Reviewer: code-reviewer
- Phạm vi: 3 file tạo + 6 file sửa (client Unity + `PetBattle.cs`)

## Tổng quan

Thiết kế đúng hướng: tách logic lượt thành C# thuần test được, bỏ timer 3.5s, tách
surrender khỏi vòng khoá, đóng lỗ double-send kỹ năng. Heuristic "WAIT + 0 effect =
dùng vật phẩm" đã được kiểm chứng lại và **đúng** với toàn bộ 6 call site `sendPetAttack`.

Vấn đề thật nằm ở chỗ khác: **có 3 đường server KHÔNG gửi gói 37 nào cả**. Mô hình khoá
theo lượt biến "không có gói" từ chỗ vô hại (timer tự mở) thành chỗ kẹt. Watchdog 8s chỉ
xoá `ActionPending`, **không** sửa `IsLocalTurn`, nên nó không cứu được ca nặng nhất.

---

## CRITICAL

### C1. Mật khẩu plaintext nằm trong diff sắp commit

`SRCGOPETGOC/GServer/bin/Debug/net8.0/log/packet-dump-server.log` +80 dòng, trong đó:

```
04:09:36.797  IN  1  1  39  010009676f706574746573740008616263313233343500...
                                  ^ "gopettest"      ^ "abc12345"
```

Gói login (cmd 1) với **username + password dạng chữ thường**. File nằm trong `.gitignore`
(`SRCGOPETGOC/GServer/bin*` dòng 9) nhưng **đã bị track từ trước** nên `.gitignore` vô hiệu
— thay đổi này sẽ đi vào commit. Cùng cảnh: `obj/Debug/net8.0/*.dll`, `*.pdb`, `apphost.exe`.

Vi phạm trực tiếp `.claude/rules/development-rules.md` mục Pre-commit Rules.

**Xử lý trước khi commit:**

```
git rm -r --cached SRCGOPETGOC/GServer/bin SRCGOPETGOC/GServer/obj
```

và đổi mật khẩu tài khoản test nếu nó trùng với bất kỳ tài khoản thật nào.

---

## HIGH

### H2. Quái bị định thân → không có gói 37 → nút chết ~25 giây, watchdog KHÔNG cứu

- `PetBattle.cs:1258-1260` (`mobUseNormalAttack`) và `:1353-1355` (`mobUseSkill`):
  `if (!isStun) { ...sendPetAttack... } else { IsMobFighted = true; }` — nhánh stun
  **không gửi gói nào**.
- Vòng lặp `:667` thấy `IsMobFighted == true` → `nextTurn()` → tới lượt người chơi.

Kịch bản:

1. Người chơi mang buff `TỈ_LỆ_ĐỊNH_THÂN_KHI_ĐÁNH_TRÚNG` (`:300`, `:1095`) hoặc kỹ năng
   `PER_STUN_1_TURN` (`:1493`) → quái dính STUN.
2. Người chơi đánh → gói 37 (actorId = mình) → `BattleTurnState.cs:61` đặt
   `IsLocalTurn = false`. `RefreshLocks()` tắt 3 nút hành động + mọi nút kỹ năng.
3. Tới lượt quái, quái bị stun → **không gói nào rời server**.
4. Server `nextTurn()` → lượt người chơi. Client vẫn `IsLocalTurn == false`.
5. `BattleView.cs:166` watchdog chỉ gọi `_turn.ClearPending()`; mà
   `CanAct = IsLocalTurn && !ActionPending` vẫn **false**. Nút tiếp tục chết.
6. Thoát kẹt duy nhất: `PetBattle.cs:654` hết `delaTimeTurn` (25s) → server tự
   `petAttack(activePlayer)` thay người chơi. Người chơi mất trắng 1 lượt + 25s tê liệt.

`BattleCoordinator.CheckStalled` cũng không đóng overlay vì gói vẫn về (sau 25s).

Trước thay đổi này, `Unlock()` 3.5s tự mở nên ca stun chỉ gây nhãn sai, không kẹt nút.
→ **đây là regression thật do phase 01 tạo ra.**

**Hướng sửa (chọn 1):**

- Server (sạch nhất): nhánh stun gửi một gói 37 kiểu `TurnEffect.createWait(0,
  getUserTurnId())` với ≥1 effect (ví dụ effect `NONE` mang skill "định thân") để client
  vừa lật lượt vừa vẽ được hiệu ứng.
- Client (phòng thủ): mở rộng watchdog thành *turn watchdog* — nếu `!IsLocalTurn` mà không
  có gói nào trong `TurnDurationMs` (25s, đã có sẵn `BattleView.TurnDurationMs`), gọi một
  `_turn.ForceLocalTurn()`. Chỉ `ClearPending()` là chưa đủ.

### H3. Người chơi bị định thân → cũng không có gói 37, và watchdog mở khoá SAI lượt

`PetBattle.cs:250-255` (`petAttack`) và `:996-1001` (`useSkill`):

```csharp
if (isStun) { nextTurn(); return; }   // không gửi gói nào
```

Client: `ActionPending = true`, `IsLocalTurn = true`. Server đã sang lượt quái.
Sau 8s watchdog xoá pending → `CanAct == true` → nút sáng **trong lượt quái**.

- PvP (`:233-240` gọi thẳng `petAttack`/`useSkill`): click tiếp → `checkWhoseTurn` fail →
  `redDialog("Chưa tới lượt của bạn")` → **vi phạm S7**.
- PvE (`:190-206` xếp hàng `actions`): click bị enqueue và tự nổ ở lượt sau → người chơi
  mất quyền chọn hành động của lượt đó.

Cùng gốc với H2; sửa server một lần là xong cả hai.

### H4. `OnPotion` đánh dấu "đã gửi hành động" trong khi gói đó chỉ MỞ MENU

`BattleView.Actions.cs:19-24` gọi `MarkSent()` sau `SendUseItem()`. Nhưng server:

```
PetBattle.cs:208-209 (PvE) / :233-235 (PvP)
case GopetCMD.PET_BATTLE_USE_ITEM:
    MenuController.sendMenu(MenuController.MENU_SELECT_ITEM_SUPPORT_PET, player);
```

→ **không có gói 37 nào được gửi**. Gói 37 (`:1643`) chỉ xuất hiện khi người chơi thực sự
chọn được một item VÀ `GameController.checkCount` pass.

Kịch bản: bấm "Thuốc" → huỷ menu (hoặc không còn bình máu) → toàn bộ nút hành động +
kỹ năng chết đúng **8 giây**. Code cũ mở lại sau 3.5s → đây là bước lùi UX, và xảy ra
thường xuyên chứ không hiếm.

**Sửa:** bỏ `MarkSent()` khỏi `OnPotion` (chỉ giữ gate `if (!_turn.CanAct) return;`).
Việc dùng vật phẩm đã không đổi lượt nên không có gì cần khoá; lỗ double-send không tồn
tại vì mở menu 2 lần là vô hại.

---

## MEDIUM

### M5. Comment ở `BattleHandler.cs:79-83` khẳng định sai — quái đi trước, không phải người chơi

Comment viết: *"Người chơi luôn đánh trước: server chỉ rút hành động của người chơi khi
`mob.getMobId() != getUserTurnId()` (PetBattle.cs:675)"*. Đọc lại source:

- `PetBattle.cs:24` `isActiveTurn = false`; ctor PvE `:65` `setIsActiveTurn(false)`.
- `getUserTurnId()` `:428` với `petAttackMob=true, isActiveTurn=false` → **`mob.getMobId()`**.
- Nên nhánh `:675` (`mob.getMobId() != getUserTurnId()`) **không** chạy ở lượt đầu.
- Nhánh chạy là `:671`: `getUserTurnId() == mob.getMobId() && MobAttackTime < Now &&
  !IsMobFighted` → `mobAttack()`. `MobAttackTime` khởi tạo `= DateTime.Now` ở `:33`
  (không phải `AddSeconds(2)`) nên điều kiện đúng gần như ngay lập tức.

→ **Quái cầm lượt 1.** Kết quả cuối cùng vẫn đúng một cách tình cờ: gói tấn công đầu tiên
của quái (actorId = mobId) đặt `IsLocalTurn = true`, trùng với giá trị `LocalStarts=true`
đoán trước. Không phải bug hành vi, nhưng comment sai sẽ dẫn người sau đi lạc — và nó là
cơ sở duy nhất cho `start.LocalStarts = start.IsParticipant`.

**Sửa:** viết lại comment cho đúng ("quái cầm lượt 1 nhưng đánh ngay trong ~0s, nên đặt
LocalStarts=true để nhãn không nhấp nháy"), và sửa Key Insight #4 trong plan.

### M6. Panel kỹ năng vẫn bấm được sau khi panel kết quả hiện

`BattleView.cs:93-99` `ShowResult` ẩn `_actionBar` nhưng **không** gọi `RefreshLocks()` và
không khoá `_skillLeft`. `BattleResultPanel.cs:17-18` chỉ là hộp 460×230 ở giữa màn hình,
trong khi panel kỹ năng nằm ở `anchorMin.x = 0f … anchorMax.x = 0.18f`
(`BattleSkillPanel.cs:38-39`) — **không bị che**. Nếu trận kết thúc trong lượt mình
(`CanAct == true`), nút kỹ năng vẫn sáng → gửi `[81][37][4]` vào trận đã đóng.

Là lỗi có sẵn (trước đây panel kỹ năng không có cơ chế khoá nào), nhưng giờ chỉ tốn 1 dòng:
thêm `_skillLeft?.RefreshState(_left.Mp, true);` vào `ShowResult`.

### M7. `_turn` bị dereference không có guard trong `Update()` và `Apply()`

`BattleView.cs:70` và `:166`. Đường đi bình thường an toàn (`Create` gọi `Build` đồng bộ,
`_turn` gán ở `:119`). Nhưng `:119` nằm SAU `BattleTopBar`/`BattleHudPanel`/`BattlePetCard`
(`:112-117`) — nếu bất kỳ cái nào ném (ví dụ `BattleSkin.Load` với asset thiếu), `_turn`
để null còn `Update()` đã chạy → NRE **mỗi frame, vĩnh viễn**, kéo theo `Ticked` không bao
giờ nổ nên `CheckStalled` cũng chết → overlay không bao giờ đóng.

**Sửa:** chuyển `_turn = new BattleTurnState(...)` lên đầu `Build` (nó không phụ thuộc UI),
hoặc thêm `if (_turn == null) return;` ở đầu `Update`.

---

## LOW / Ghi nhận

### L8. `sendBattleInfo` PvP ghi sai "pet của tôi" → khoá đảo ngược cho người chơi thụ động

`PetBattle.cs:474-484` luôn ghi `activePlayer.user.user_id` làm owner và `putsbyte(0)` cho
`LocalStarts`, bất kể người nhận. Nếu `passivePlayer` nhận gói này thì
`_start.LocalPet.ActorId != _localUserId` → `BattleTurnState.cs:61` đảo ngược, nút khoá
đúng lúc phải mở. Chỉ tới được qua `GopetPlace.sendPetBattleList` (`:679`) khi người chơi
vào place, mà đang đấu PvP thì không đổi place được → coi như không tới được. Ghi nhận
để không bị bất ngờ nếu sau này thêm tính năng xem trận.

### L9. `SkillCooldownTracker` không áp dụng quy tắc "vật phẩm không đổi lượt"

`BattleView.cs:72` gọi `_cooldowns.OnTurnAdvanced(turn.ActorId, ...)` cho **mọi** gói, kể
cả gói vật phẩm (WAIT+0) và gói `actorId = -1`. `SkillCooldownTracker.cs:32` dedupe theo
`_lastActorId` nên thực tế vẫn ra 1 tick/vòng — không lỗi hiển thị. Nhưng logic lượt giờ
nằm ở 2 nơi với 2 quy tắc khác nhau (DRY). Cân nhắc cho `BattleTurnState` trả về "gói này
có lật lượt không" rồi dùng chung.

### L10. Giá trị ms mới là thời gian còn lại của lượt VỪA KẾT THÚC

`sendPetAttack` chạy trước `nextTurn()` (`:307-308`, `:1109-1110`), mà `nextTurn()` mới
reset `delaTimeTurn` (`:728`). Nên `delaTimeTurn - CurrentTimeMillis` là phần dư của lượt
cũ, không phải 25000 của lượt mới. **Giống hệt nhánh PvP cũ** (`:328` trước khi sửa) nên
parity với jar được giữ, không đổi wire format, không đổi hành vi Unity (`RemainingMs`
chưa được đọc ở đâu). Ghi lại để sau này không có ai "sửa" nhầm.
Jar `ei.java:162-171` clamp `var28 < 0 → 0` nên giá trị âm cũ chỉ làm pie không vẽ; sau
sửa pie vẽ được. Đúng như plan mô tả.

### L11. Lệch plan: thiếu `SetActionsInteractable(false)` cuối `BattleActionBar.Create`

Plan bước 6 yêu cầu, code không có. Vô hại vì `Build` kết thúc bằng `RefreshLocks()` trong
cùng call stack. Chỉ là plan/code drift.

### L12. Thiếu ca test

`BattleTurnStateTests.cs` 8/8 đúng nhưng chưa phủ: `actorId == 0` (biên của guard `<= 0`),
`localStarts=false` + gói vật phẩm, và ca "không có gói nào" (H2/H3 — chưa test được vì
`BattleTurnState` chưa có API force-turn).

---

## Đã kiểm chứng ĐÚNG (không có lỗi)

| Nghi vấn | Kết luận |
|---|---|
| Heuristic `Wait && effectCount == 0` có đụng đường nào khác không? | **Không.** Grep cả 6 call site: `:307`/`:1339` là NORMAL; `:1109`/`:1403` luôn `turnEffects.add` trước (`:1101-1102`, `:1391-1392`); `:1547`/`:1617` dùng `petId = -1`. Chỉ `:1643` sinh WAIT + list rỗng. |
| Gói độc/phản đòn có thật là `petId = -1`? | Đúng. `TurnEffect(sbyte type, int petId, ...)` → `new TurnEffect(NONE, -1, 0,0,0)` = type 0, petId -1. Type 0 ≠ `Wait(4)` nên `BattleHandler.cs:117` cũng không đọc nhầm phần WAIT. |
| `delaTimeTurn` có được duy trì ở PvE? | Có: ctor `:46`/`:65`, `nextTurn()` `:728`. Giá trị mới là ms dương thật. |
| `MobAttackTime` ở `:667`, `:671` có bị ảnh hưởng? | Không — server diff chỉ đụng `sendPetAttack`. |
| Còn caller nào gọi `Lock()`/`Unlock()`/`RefreshState(int)` 1 tham số? | Không. Grep toàn `Assets/Scripts` + `tests` sạch. |
| `_left` có thể null tại call site `RefreshLocks`? | Không tới được: `_skillLeft?.` short-circuit nên `_left.Mp` không được evaluate khi `_skillLeft` null; mọi đường đi đều gán `_left` ở `:116` trước `:131`. |
| `BattleSkillPanel._locked = true` ảnh hưởng panel đối phương? | Không. `interactive = false` → `Button` bị `Destroy` (`:88`), `row.Button == null` → `RefreshLabels` bỏ qua. `_skillRight` không bao giờ nhận `RefreshState`. |
| Observer (`IsParticipant == false`) có vỡ? | Không tới được: `BattleCoordinator.OnStarted:59` `if (!start.IsParticipant) return;` — `BattleView` chỉ tồn tại cho người tham gia. |
| `BattleActionBar` còn cần `using UnityEngine`? | Có — `MonoBehaviour`, `Sprite`, `Vector2`, `Color32`, `Mathf`. |
| Nút xin thua có bị khoá ngoài ý muốn? | Không. Đã tách khỏi `_actionButtons`; `OnSurrenderClicked`/`OnBackClicked` không gate `CanAct`. R6 đạt. |
| PvP `LocalStarts` đọc từ wire còn đúng? | Đúng cho đường chính: `sendStartFightPlayer` (`:489-513`) gửi 2 gói riêng, mỗi bên nhận pet của chính mình + byte `LocalStarts` đúng. (Ngoại lệ ở L8.) |
| Wire format có đổi? | Không. Vẫn đúng 1 `int` ở đúng vị trí cũ, chỉ đổi giá trị. |
| Ngân sách dòng | Tất cả ≤ 200: `BattleTurnState` 65, `BattleView` 174, `BattleView.Actions` 77, `BattleActionBar` 159, `BattleSkillPanel` 199 (sát trần), `BattleVsIndicator` 50, test 104. Không thêm allowlist. |

## Điểm tốt

- `BattleTurnState` thuần C#, mỗi nhánh quyết định có comment trỏ `PetBattle.cs:<line>` —
  đúng chỗ cần nhất, vì đây là loại logic dễ bị "tối ưu" nhầm.
- Tách `_surrenderBtn` khỏi `_actionButtons` sửa đúng gốc của bug `LockSurrender` bị huỷ.
- `partial` chỉ chứa method, không field/ctor → không rủi ro thứ tự khởi tạo.
- Đóng lỗ double-send kỹ năng bằng cả 2 lớp (nút `interactable = false` + gate `CanAct`).
- Bảo mật: không đưa dữ liệu client vào quyết định damage; `checkWhoseTurn` vẫn là nguồn
  chân lý; khoá nút đúng vai trò tiện ích UI.

## Hành động đề xuất (theo thứ tự)

1. **C1** — `git rm -r --cached SRCGOPETGOC/GServer/bin SRCGOPETGOC/GServer/obj` trước khi
   commit; đổi mật khẩu tài khoản `gopettest` nếu dùng lại ở nơi khác.
2. **H4** — xoá `MarkSent()` khỏi `OnPotion` (1 dòng, rủi ro ~0).
3. **H2 + H3** — nhánh stun ở server gửi một gói 37 mang ≥1 effect. Nếu chưa muốn đụng
   server trong phase này thì tối thiểu mở rộng watchdog thành turn-watchdog (lật
   `IsLocalTurn` khi im lặng quá `TurnDurationMs`) và ghi rõ nợ kỹ thuật vào plan.
4. **M7** — chuyển `_turn = new BattleTurnState(...)` lên đầu `Build`.
5. **M6** — khoá `_skillLeft` trong `ShowResult`.
6. **M5** — sửa comment `BattleHandler.cs:79-83` và Key Insight #4 của plan.
7. L9/L12 — gom quy tắc lật lượt về một chỗ; bổ sung ca test biên.

## Câu hỏi còn treo

- Buff định thân (`TỈ_LỆ_ĐỊNH_THÂN_KHI_ĐÁNH_TRÚNG`, `PER_STUN_1_TURN`) hiện có item/kỹ năng
  nào của người chơi cấp ở bản live không? Nếu có thì H2 là bug gặp hàng ngày, nếu chưa có
  thì là bom hẹn giờ. Chưa tra được data item.
- `S10` ("trường đầu dương ≤ 25000 và giảm dần trong lượt") chưa có bằng chứng log thực tế
  — mới suy luận từ source.
