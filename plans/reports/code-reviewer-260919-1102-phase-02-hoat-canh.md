# Code Review — Phase 02: Hoạt cảnh lao sang + hàng đợi lượt

- Plan: `plans/260919-1102-man-hinh-danh-quai-theo-luot/phase-02-hoat-canh-va-hang-doi-luot.md`
- Ngày: 2026-09-19 · Nhánh: master · Reviewer: code-reviewer
- Phạm vi: 2 file tạo + 5 file sửa (phase 02) + 4 sửa hậu-review phase 01/03
- Review trước: `code-reviewer-260919-1102-phase-01-vong-luot.md`
- **Chưa chạy Unity** — mọi kết luận suy từ code.

## Tổng quan

Kiến trúc hàng đợi đúng hướng và đọc dễ. Nhưng hai lỗi nặng nhất **không nằm ở hàng đợi**
mà ở hai giả định im lặng:

1. `_home` của **cả hai card đều là `(0,0)`** (vị trí do *anchor* quyết định, không phải
   `anchoredPosition`) ⇒ phép tính hướng lao bị suy biến ⇒ card phải chạy **ngược hướng**.
2. `Idle` suy ra từ `_running`, mà `_running` chỉ được xoá ở **đường kết thúc bình thường**
   của coroutine. Một exception bất kỳ trong `Drain()` khoá nút **vĩnh viễn** — và
   `BattleEffectView.Play` *có* đường ném thật (`JarSkin.Load` `throw`).

Ngoài ra `SyncLocalVitals` bỏ sót 2 call site server gửi `MY_PET_INFO` **trước** gói 37,
gây áp HP hai lần.

Các sửa hậu-review phase 01: **C1 đã xong** (0 file dưới `GServer/bin` còn được track),
**H4 đã xong** (`OnPotion` không còn `MarkSent`), **M5 đã xong**, **M7 đã xong**,
**M6 đã sửa nhưng bị mở lại bởi lỗi H4 dưới đây**. **H2/H3 mới đóng được ~70%.**

---

## CRITICAL

### C1. Một exception trong `Drain()` khoá nút tới hết trận — `_running` không bao giờ được xoá

`BattleTurnAnimator.cs:41,46,79-90`

```csharp
public bool Idle => _running == null && _queue.Count == 0;
public void Enqueue(BattleTurn turn) { _queue.Enqueue(turn); if (_running == null) _running = StartCoroutine(Drain()); }
private IEnumerator Drain() { while (...) { ... } _running = null; _onDrained?.Invoke(); }
```

`_running = null` chỉ chạy khi coroutine **đi hết vòng lặp**. Unity bắt exception ném ra từ
`MoveNext`, log rồi **dừng coroutine im lặng** — field vẫn giữ handle cũ.

Đường ném **có thật**, không phải giả định:

```
BattleTurnAnimator.cs:111   BattleEffectView.Play(transform, hit.EffectAnchor, effect.SkillId)
BattleEffectView.cs:38      var sprite = JarSkin.Raw($"pet/battle/...{name}")
JarSkin.cs:50-54            if (sprite == null) throw new InvalidOperationException(
                                "Không tìm thấy sprite ... Đã chạy unpack-jar-dat chưa?");
```

Kịch bản: pet/quái dùng kỹ năng mà sprite hiệu ứng tương ứng thiếu trong `Resources`
(đúng lớp lỗi mà **bước 3 của `verify.ps1` đang FAIL** vì asset jar lệch):

1. `PlayTurn` ném ở `:111` → `Drain` chết, `_running` giữ handle rác.
2. `Idle` **false vĩnh viễn** → `RefreshLocks()` (`BattleView.cs:113`) luôn cho
   `canAct = false` → Tấn công / Thuốc / toàn bộ kỹ năng xám hết trận.
3. Watchdog 8s (`BattleView.cs:174`) chỉ xoá `ActionPending`, **không** đụng `_animator` →
   không cứu được (đúng ca reviewer lo nhất ở phase 01, nay tái hiện ở tầng khác).
4. Mọi gói 37 sau đó chỉ **dồn vào `_queue`** và không bao giờ phát → thanh HP đóng băng,
   không có damage float, không có hoạt cảnh.
5. `BattleCoordinator.CheckStalled` (`:109-115`) **không** cứu: mỗi gói lượt gọi
   `RefreshTimeout` (`:71`) reset `_lastPacketAt`, mà server vẫn gửi đều (tự đánh thay sau
   25s) → overlay không bao giờ tự đóng.
6. Lối thoát duy nhất: bấm Xin thua (không gate `CanAct`) hoặc đóng game.

Cùng lỗ hổng còn mở với ca **GameObject bị `SetActive(false)`** (Unity dừng coroutine, không
ném) — hiện chưa có đường nào tắt GO của `BattleView` nên chưa chạm tới, nhưng biến thể này
để lại y hệt trạng thái kẹt.

**Sửa (2 lớp, cả hai đều rẻ):**

```csharp
private IEnumerator Drain()
{
    try
    {
        while (_queue.Count > 0)
        {
            _inFlight = _queue.Dequeue();
            _appliedEffects = 0;
            yield return PlayTurn(_inFlight);
            _inFlight = null;
        }
    }
    finally { _running = null; }   // chạy cả khi Dispose/StopCoroutine
    _onDrained?.Invoke();
}
```

và bọc phần render mỗi effect (`:111-112`) trong `try/catch` ghi `Debug.LogException` —
thiếu một sprite hiệu ứng **không được phép** làm hỏng vòng lượt. Lưu ý `finally` trong
iterator *không* chạy khi `MoveNext` ném ra ngoài block `try`, nên vẫn cần `try/catch` ở
`:105-117`; chỉ có `finally` là chưa đủ.

Thêm một chốt chặn rẻ: cho `Idle` cũng đúng khi coroutine đã chết —
`public bool Idle => (_running == null || _inFlight == null) && _queue.Count == 0;`
không đủ chặt; tốt hơn là watchdog trong `BattleView.Update()` — nếu `!_animator.Idle` quá
`TurnDurationMs` thì gọi `FlushImmediate()`.

---

### C2. Hướng lao suy biến: `HomeX == 0` ở **cả hai** card ⇒ card phải chạy ra mép màn hình

`BattlePetCard.cs:38-47,72` + `BattleTurnAnimator.cs:101-102`

```csharp
// BattlePetCard.Create
rect.anchorMin = rect.anchorMax = new Vector2(left ? 0.31f : 0.66f, 0.45f);
rect.sizeDelta = new Vector2(160f, 200f);
...
card._home = rect.anchoredPosition;     // <-- KHÔNG BAO GIỜ được set ⇒ (0,0)
```

Vị trí card do **anchor** quyết định (`0.31` vs `0.66`), `anchoredPosition` giữ nguyên mặc
định `(0,0)` cho cả hai. Plan cũng ghi đúng điều này (bước 2: *"mặc định `Vector2.zero`"*)
nhưng phần tính hướng lại dựa trên `HomeX`:

```csharp
var dir = Mathf.Sign(target.HomeX - attacker.HomeX);   // Sign(0f - 0f)
yield return attacker.PlayLunge(attacker.HomeX + dir * LungeGapPixels, LungeSeconds);
```

`Mathf.Sign(0f)` trả **`1f`** (Unity: `f >= 0 ? 1 : -1`). Nên **mọi** lượt đều lao sang
**phải** 246px:

| Bên ra đòn | Từ (px, ref 960) | Tới | Kết quả |
|---|---|---|---|
| Pet trái (0.31 → 298) | 0 | +246 | ~544px — cạnh đối phương ✔ (đúng do may) |
| Pet phải / quái (0.66 → 634) | 0 | +246 | ~880px — **chạy ra mép phải màn hình, xa người chơi** |

⇒ **R1/S1 sai cho quái và cho đối thủ PvP**: quái đánh mình thì nó chạy *ra xa* rồi mới
"đánh". Đây là lỗi thấy ngay ở lần chạy đầu tiên (mà quái lại là bên **cầm lượt 1** sau khi
`LocalStarts = false`, nên là hoạt cảnh đầu tiên người chơi nhìn thấy).

**Sửa:** lấy hướng từ cờ `left` (hiện `Create` nhận `left` nhưng **không lưu**) hoặc từ
anchor thật:

```csharp
// BattlePetCard
private bool _left;                       // gán trong Create
public bool IsLeft => _left;
public float AnchorX => _rect.anchorMin.x;
```

và trong animator `var dir = attacker.IsLeft ? 1f : -1f;` — đồng thời xử lý luôn C3.

---

## HIGH

### H3. `MY_PET_INFO` tới **trước** gói 37 ở 2 call site ⇒ áp HP hai lần

`BattleView.cs:84-97` + `GameSession.cs:255`

Plan (Key Insight #5) chỉ khảo sát `nextTurn()` — nơi `sendMyPetInfo()` chạy **sau**
`sendPetAttack`. Nhưng có **2 đường ngược lại**:

| Server | Thứ tự |
|---|---|
| `PetBattle.addRecovery` `:1184-1187` | `pet.addHpPet(rec)` → **`sendMyPetInfo()`** → về `useSkill:1147` `sendPetAttack(...)` mang effect `+rec` |
| `PetBattle.mobUseSkill` `:1435-1443` | `activePet.subHp(dmg)` (`:1422`) → mob hồi máu → **`sendMyPetInfo()`** (`:1438`) → `sendPetAttack(...)` (`:1443`) mang effect `-dmg` |

Client nhận `MY_PET_INFO` **trước**, lúc đó hàng đợi đang **rỗng** (lượt trước đã drain) nên
`_animator.Idle == true` → `SyncLocalVitals` áp ngay. Rồi gói 37 tới và `PlayTurn` áp
**delta lần thứ hai** lên đúng giá trị đã bao gồm delta đó.

- Kỹ năng hút máu của mình: HP cộng 2 lần (kẹp ở `MaxHp`) + **2 float `+N` xanh**.
- Quái dùng kỹ năng hồi máu: HP mình **trừ 2 lần**. Với đòn ăn ~50% máu còn lại, thanh HP
  tụt về 0 → nhấp nháy cảnh báo (`BattleStatBar.cs:127`) → người chơi tưởng pet chết.
  `nextTurn()` ở lượt sau mới chữa lại (gán tuyệt đối) — tức là sai trong **cả một lượt**.

Rủi ro này plan đã liệt kê ("MY_PET_INFO áp đè giữa hoạt cảnh") nhưng biện pháp giảm thiểu
(`chỉ áp khi Idle`) **không phủ được ca gói tới trước**.

**Sửa (đơn giản nhất, giữ đúng tinh thần "hoãn"):** đừng áp trong cùng frame nhận gói —
hoãn tối thiểu 1 frame:

```csharp
public void SyncLocalVitals(int hp,int maxHp,int mp,int maxMp)
{
    _pendingVitals = new[]{hp,maxHp,mp,maxMp};   // luôn cất
    _vitalsDueAt  = Time.unscaledTime + 0.05f;    // áp ở Update khi _animator.Idle
}
```

rồi trong `Update()`: `if (_pendingVitals != null && _animator.Idle && Time.unscaledTime >= _vitalsDueAt) ApplyVitals();`
Gói 37 cùng tick server sẽ kịp `Enqueue` (làm `Idle` false) trước khi tới hạn, nên snapshot
tự động rơi về đường `OnAnimatorDrained` vốn **đã đúng**.

### H4. `SyncLocalVitals` **mở lại panel kỹ năng sau khi trận kết thúc** — mở lại M6 của phase 01

`BattleView.cs:96`

```csharp
_skillLeft?.RefreshState(_left.Mp, !_turn.CanAct);   // không xét _result
```

`PetBattle.win(Popup[],coin,exp)` `:959-967` gửi **gói kết quả trước, `sendMyPetInfo()` sau**:

```csharp
place?.sendMessage(win(petBattleTexts, coin, exp, activePlayer.user.user_id));  // :961
...
activePlayer.controller.sendMyPetInfo();                                        // :966
```

Kịch bản (quái kết liễu pet mình — ca thua PvE, tức ca phổ biến nhất của phase 03):

1. Gói 37 của quái → `ApplyTurnPacket` (`BattleTurnState.cs:61`) → `IsLocalTurn = true`,
   `ActionPending = false` ⇒ **`CanAct == true`**.
2. Gói kết quả → `ShowResult` → `FlushImmediate` → `_actionBar` ẩn,
   `_skillLeft.RefreshState(mp, true)` — khoá đúng (M6 fix).
3. `MY_PET_INFO` tới ngay sau → `SyncLocalVitals` → `_animator.Idle == true` →
   `RefreshState(_left.Mp, !true)` = **`locked: false`** ⇒ nút kỹ năng **sáng lại**.
4. Panel kết quả chỉ che giữa màn hình, panel kỹ năng nằm ở mép trái (`anchorMax.x = 0.18`)
   → bấm được → `OnSkillUsed` thấy `_turn.CanAct == true` → gửi `[81][37][4]` vào trận đã
   đóng.

`OnAnimatorDrained() → RefreshLocks()` (`:107`) có cùng lỗ hổng (không xét `_result`).

**Sửa:** đưa điều kiện kết trận vào **một chỗ** và dùng chung:

```csharp
private void RefreshLocks()
{
    var canAct = _result == null && _turn.CanAct && (_animator == null || _animator.Idle);
    _actionBar?.SetActionsInteractable(canAct);
    _skillLeft?.RefreshState(_left.Mp, !canAct);
}
```
rồi `SyncLocalVitals` gọi `RefreshLocks()` thay vì tự gọi `RefreshState` (DRY — hiện đang có
2 công thức khoá khác nhau trong cùng file).

---

## MEDIUM

### M5. `FlushImmediate` áp `MainMpDelta` **lần thứ hai** — off-by-one đúng như nghi vấn

`BattleTurnAnimator.cs:66-71,83-85,95`

`Drain` đặt `_appliedEffects = 0` **trước** khi `PlayTurn` áp `MainMpDelta`; `ApplyRemaining`
lại coi `fromIndex == 0` nghĩa là "chưa áp gì cả":

```csharp
// PlayTurn:95      if (turn.MainMpDelta != 0) actor?.Apply(0, turn.MainMpDelta);
// ApplyRemaining:68 if (fromIndex == 0 && turn.MainMpDelta != 0) Card(...)?.Apply(0, turn.MainMpDelta);
```

Cửa sổ lỗi = từ lúc `PlayTurn` bắt đầu tới `_appliedEffects++` đầu tiên, tức **toàn bộ
0.22s lunge** (và cả lượt nếu 0 effect).

Đây **không** phải ca hiếm: server gửi gói 37 rồi `win()` **trong cùng một tick**
(`useSkill:1147-1152`). Client xử lý cả hai trong cùng frame (hoặc 2 frame liền) ⇒ mỗi trận
kết thúc bằng **kỹ năng** đều trừ MP hai lần + 2 float `-MP` (một cái trễ 1s, hiện đè lên
panel kết quả). Số MP trên panel kết quả sai.

Phần effect thì **đúng** (`_appliedEffects++` ở `:115` chạy sau khi apply, `ApplyRemaining`
bắt đầu từ `fromIndex`) — không thiếu, không lặp.

**Sửa:** chuyển việc áp `MainMpDelta` ra khỏi `PlayTurn`, đặt trong `Drain` trước khi
`_appliedEffects = 0`, hoặc thêm cờ `_mainApplied` và đổi điều kiện `:68` thành
`if (!_mainApplied && turn.MainMpDelta != 0)`.

### M6. `sendTurnSkipped()` khiến bên **bị định thân lao sang đánh** — hành vi sai về ngữ nghĩa

`PetBattle.cs:330-335`

```csharp
turnEffects.add(new TurnEffect(TurnEffect.SKILL_MISS, getFocus(), TurnEffect.SKILL_MISS, 0, 0));
sendPetAttack(turnEffects, TurnEffect.createNormalAttack(activePet.mp, 0, getUserTurnId()));
```

`getFocus()` = **đối phương** (`:770-772`), `getUserTurnId()` = bên bị stun (`:443-446`). Trên
client:

- `HitsOpponent` (`BattleTurnAnimator.cs:126-134`): có effect với `ActorId != turn.ActorId`
  ⇒ **true** ⇒ bên bị định thân **lao sang** (và theo C2 thì quái lao *sai hướng*).
- `PlayHitSound` (`BattleView.Actions.cs:71-75`): `SkillId == 1` (= `SKILL_MISS`) ⇒ vẽ chữ
  **"TRƯỢT"** trên đầu đối phương + phát `s_attack_miss`.

⇒ Người chơi thấy "pet mình xông lên đánh trượt" trong khi thực tế **pet đang bị định thân
không làm gì**. Về kỹ thuật thì gói hợp lệ (đúng hình dạng gói miss có sẵn ở `:306-308`,
jar render được, không đổi wire) và **lượt lật đúng** (`ApplyTurnPacket` thấy NORMAL + 1
effect). Nhưng thông tin truyền đạt sai.

**Sửa (1 dòng):** để effect trỏ vào **chính bên bị stun** thì `HitsOpponent` trả false
(đứng yên) và `ResolveName(0)` vẫn render `SlashEffect` tại chỗ:

```csharp
turnEffects.add(new TurnEffect(TurnEffect.NONE, getUserTurnId(), TurnEffect.NONE, 0, 0));
```
Tốt hơn nữa: dùng skillId của hiệu ứng "định thân" nếu có, và thêm `player.Popup("Pet bị
định thân")` cho rõ. (Đúng như đề xuất H2 của review phase 01: *effect `NONE` mang skill
"định thân"*.)

Đã kiểm và **không** có lỗi ở các điểm còn lại của `sendTurnSkipped`:
`activePet.mp` chỉ được ghi lên wire khi `type == WAIT` (`:353-359`) nên với NORMAL nó là dữ
liệu chết — không rò rỉ, không lệch format; `activePet` không null hơn nhánh `:1377` sẵn có;
PvP được gửi cho **cả hai** người chơi qua nhánh `!petAttackMob` (`:385-...`); `getFocus()`
/ `getUserTurnId()` hợp lệ ở cả 4 call site (mob attack chạy khi `isActiveTurn == false`).

### M7. `_animator` bị dereference không guard trong `Apply()`

`BattleView.cs:77` `_animator.Enqueue(turn);` — trong khi `:87` và `:113` đều dùng
`_animator != null`. `_animator` được gán ở **cuối** `Build` (`:165`), sau
`BattleActionBar.Create` (`:161`) và `BattleSkillPanel.Create` (`:157`). Nếu một trong số đó
ném (cùng lớp rủi ro `BattleSkin.Load`/`JarSkin` như C1) thì `_animator` null và gói 37 đầu
tiên gây NRE.

Đây **đúng là lỗi M7 của phase 01** đã được sửa cho `_turn` (hoist lên đầu `Build` — làm
tốt) nhưng thành phần mới lại rơi vào đúng cái bẫy đó. `BattleTurnAnimator.Attach` không phụ
thuộc UI nào ngoài `gameObject`, nên **chuyển lên ngay sau dòng `_turn = new ...`** —
nhưng lưu ý `Attach` nhận `Card`/`RefreshHuds` là method group (`_left`/`_hudLeft` đọc lúc
gọi, không lúc bind) nên di chuyển an toàn.

### M8. `LungeGapPixels = 246f` là hằng số nhưng khoảng cách 2 card **phụ thuộc tỉ lệ màn hình**

`BattleTurnAnimator.cs:17` + `BattleView.cs:52-53`

`CanvasScaler.matchWidthOrHeight = 1f` = khớp theo **chiều cao** ⇒ chiều cao luôn 540 đơn vị,
**chiều rộng = 540 × aspect**. Con số 336px trong plan chỉ đúng ở 16:9.

| Màn hình | Rộng (đơn vị) | Khoảng cách 2 card (0.35×W) | Lao 246 |
|---|---|---|---|
| 4:3 (720) | 720 | 252 | **vượt qua** đối phương (đè lên nhau) |
| 16:9 (960) | 960 | 336 | 90 — đúng thiết kế |
| 20:9 (1200) | 1200 | 420 | dừng cách 174 — đứng giữa sân, không "cạnh" |

Sửa chung với C2: tính đích từ vị trí thật của 2 rect thay vì hằng số, ví dụ
`targetX = (target.AnchorX - attacker.AnchorX) * canvasWidth - 90f * sign`.

### M9. H2/H3 của phase 01 chưa đóng hết: 3 nhánh `useSkill` của **người chơi** vẫn im lặng

`PetBattle.cs:1160-1173` — `useSkill` vẫn có 3 đường chỉ `redDialog` và **không gửi gói 37**:

```csharp
else { player.redDialog("Thú cưng của bạn không đủ thể lực"); }   // :1162
else { player.redDialog("Không có kỹ năng này"); }                 // :1167
else { player.redDialog("Chưa hồi kỹ năng xong..."); }             // :1172
```

Client đã `MarkSent()` ở `OnSkillUsed` (`BattleView.Actions.cs:34`) ⇒ `ActionPending = true`
⇒ nút chết đúng 8s. Khác C1/H2 cũ ở chỗ **lượt server không trôi qua**, nên chỉ mất 8s chứ
không kẹt vĩnh viễn.

Khả năng chạm tới: `BattleSkillPanel.cs:186` đã tắt nút khi `MpCost > _currentMp` hoặc còn
cooldown, nên đường chính an toàn. Nhưng **PvE xếp hàng hành động** (`:190-206`): click được
enqueue và chỉ chạy khi tới lượt — tới lúc đó MP/cooldown có thể đã khác (ví dụ quái hút MP
bằng `DOT_MANA` giữa chừng) ⇒ rơi vào `:1162`. Không phải lý thuyết suông.

Ghi vào nợ kỹ thuật, hoặc gửi `sendTurnSkipped()`-không-lật-lượt (gói WAIT + 0 effect, đúng
hình dạng gói vật phẩm ở `:1687`) cho 3 nhánh này để `ActionPending` được xoá ngay.

### M10. Nhánh "quái không đủ MP" vừa thêm là **code chết**

`PetBattle.cs:1446-1448`. `mobAttack` (`:1476`) đã kiểm `mob.mp >= petSkillLv.mpLost` **trước
khi** gọi `mobUseSkill`, mà `mobUseSkill` là `private` và không có caller nào khác
(grep sạch). Điều kiện `mob.mp - petSkillLv.mpLost >= 0` ở `:1398` không bao giờ false.
Vô hại nhưng ngược YAGNI và làm người đọc sau tưởng có ca đó. Nếu giữ thì thêm comment
"defensive"; nếu bỏ thì bỏ luôn cả `IsMobFighted = true` thừa.

---

## LOW / Ghi nhận

### L11. Trừ EXP thua PvE: công thức đúng, nhưng "không bao giờ tụt cấp" che mất chuyện exp **âm tới −20 triệu**

`PetBattle.cs:888-895`. Đã kiểm:

| Nghi vấn | Kết luận |
|---|---|
| `Utilities.round` trả `int` gán vào `long expSub` | An toàn. `PetExp` là `HashMap<int,int>`, 10% của `int` không tràn. Mất chính xác `float` (`GetValueFromPercent` `Utilities.cs:243` trả `float`) ở mức ±vài chục exp với giá trị > 16.7M — bằng đúng nhánh PK `:915` nên giữ parity. |
| `PetExp.get(lvl)` với cấp không có trong bảng | `HashMap.get` (`HashMap.cs:53-59`) trả `default(int)` = **0** (không ném) ⇒ `expSub = 0` ⇒ `if (expSub > 0)` chặn. **Đúng.** Nhánh PK `:913` cũng không guard nên nhất quán. |
| Trừ 2 lần? | Không. `win()` `:777-783` có `hadFinished` guard; khối mới nằm trong `else` của `getWinId() == activePlayer` trong `if (petAttackMob)` — một đường duy nhất. Lập luận trong comment (`:874-881`) khớp với `hasWinner()` `:736` và `getWinId()` `:993`. |

**Nhưng** comment `:878` *"subExpPK kẹp sàn MIN_PET_EXP_PK nên không bao giờ tụt cấp"* dễ gây
hiểu nhầm: `GopetManager.MIN_PET_EXP_PK = **-20_000_000**` (`GopetManager.cs:557`). Cấp
không giảm, đúng — nhưng `exp` **xuống âm** và người chơi phải cày bù toàn bộ phần âm đó
trước khi thanh exp nhúc nhích. Với PvE (chết vì quái là chuyện thường ngày, khác PK), chuỗi
5-6 lần chết liên tiếp đẩy exp âm sâu. Nên:
- viết lại comment cho đúng ("exp có thể âm tới −20M, cấp không đổi"), **và**
- xác nhận với chủ dự án đây là ý đồ (jar gốc có trừ exp khi thua quái không?) — nếu không,
  cân nhắc kẹp sàn `0` riêng cho nhánh PvE.
- kiểm client hiển thị exp âm ra sao (`GameController.cs:673-675` gửi exp thô).

### L12. `Other()` đúng nhưng mong manh và cấp phát mỗi lượt

`BattleTurnAnimator.cs:138-145`. `GetComponentsInChildren<BattlePetCard>(true)` gọi trên GO
của `BattleView`; 2 card là con trực tiếp (`BattleView.cs:151-152`) nên **lấy đúng, không
phụ thuộc thứ tự** (chỉ có đúng 2 phần tử, "phần tử != card" là duy nhất). `BattleFloatText`
/ `BattleEffectView` không mang `BattlePetCard` nên không nhiễu. Observer không tồn tại
(`BattleCoordinator.cs:58` chặn `!IsParticipant`).

Vẫn nên thay bằng `Card(actorId)` — `BattleView` biết sẵn `OpponentActorId`; bỏ được một
`GetComponentsInChildren` (cấp phát mảng) mỗi lượt và bỏ được giả định "chỉ có 2 card".

### L13. `BattleStatBar._display` khởi tạo đúng — không có lỗi

Đã soi kỹ theo yêu cầu: `_display = -1f` (`:14`), `Set` `:103` `if (_display < 0f) _display = _current;`.

- Ca `_current == 0` lần đầu (pet chết lúc mở trận): `_display = 0`. Lần `Set` sau `_display`
  không còn `< 0` ⇒ **lerp từ 0 lên**, đúng ý.
- `_max = Mathf.Max(1, max)` (`:102`) ⇒ `Refresh()` `:109` không bao giờ chia 0.
- `Update` `:116` `if (_max <= 0) return;` chỉ đúng trước `Set` đầu tiên — chặn luôn NRE giả
  định. `_fill`/`_label` gán trong `Create` cùng frame với `AddComponent` nên `Update` không
  chạy trước chúng.

Nhược điểm duy nhất: halve-delta theo **frame**, không theo `deltaTime`. Ở 60fps delta 100
mất ~8 frame ≈ 0.13s; ở 144fps ≈ 0.055s ⇒ **S5 ("trượt trong ~0.3s") khó đạt** trên máy
nhanh, gần như snap lại. Nếu S5 là yêu cầu thật thì dùng
`_display = Mathf.Lerp(_display, _current, 1f - Mathf.Pow(0.5f, Time.unscaledDeltaTime * 30f))`.

### L14. `BattleFloatText` với `delaySeconds` — đúng, 2 điểm nhỏ

Đã soi theo yêu cầu:
- Alpha khôi phục **đúng**: `Update:54-56` đọc lại `_text.color` rồi **ghi đè** `color.a` theo
  `age` ⇒ alpha 0 lúc hoãn không "dính". ✔
- `_text` không thể null: `Spawn` gán trước frame `Update` đầu tiên. ✔
- Không sống mãi: tuổi thọ tối đa = `delaySeconds + 1.2s` = 2.2s với call site duy nhất
  (`BattlePetCard.cs:60`). ✔
- Nhỏ: trong lúc hoãn object vẫn `Update` mỗi frame nhưng không làm gì — không đáng sửa.
- Nhỏ: 2 float text cùng spawn tại `anchor (0.5, 0.6)` của card ⇒ số MP trễ 1s sẽ xuất phát
  đúng chỗ số HP đang mờ dần đi — chồng hình nhẹ. Jar tách bằng vị trí, ta chỉ tách bằng
  thời gian. Chấp nhận được.
- Float bám vào card ⇒ khi card lao/lùi thì chữ trôi theo. Plan đã ghi nhận và chấp nhận.

### L15. `MoveX` gọi `SnapHome()` cho ca `seconds <= 0` kể cả khi đang **lao đi**

`BattlePetCard.cs:91`. `PlayLunge(targetX, 0f)` sẽ về `_home` chứ không tới `targetX`. Hiện
`LungeSeconds` là hằng `0.22f` nên không chạm tới; chỉ là ngữ nghĩa lệch chờ người sau vấp.

### L16. `BattlePetCard.Update` dùng `Time.time` (scaled) trong khi phần còn lại dùng unscaled

`:132-133`. Nếu `Time.timeScale == 0` (pause/dialog) thì sprite đứng hình nhưng `MoveX` vẫn
chạy bằng `Time.unscaledDeltaTime` ⇒ pet **trượt băng** sang ngang. Lỗi có sẵn, nay dễ thấy
hơn vì có chuyển động.

### L17. `MainMpDelta` áp ở `PlayTurn:95` nhưng **không** gọi `_refreshHuds()`

Thanh MP chỉ nhúc nhích ở effect đầu tiên (`:113`) hoặc cuối lượt (`:121`) — trễ tới 0.34s so
với con số float. Một dòng.

### L18. Chưa có test nào cho logic mới

`HitsOpponent`, `ApplyRemaining(fromIndex)` và phép tính hướng lao là **logic thuần**, nhưng
nằm trong `MonoBehaviour` nên không test được từ `Gopet.Net.Tests`. Chính M5 (off-by-one) và
C2 (hướng) là loại lỗi một unit test bắt được ngay. Cân nhắc tách sang class thuần C#
(`BattleTurnPlayback`) giống cách `BattleTurnState` đã làm ở phase 01 — đó là quyết định tốt
nhất của phase 01 và đáng lặp lại.

---

## Đã kiểm chứng ĐÚNG (không có lỗi)

| Nghi vấn | Kết luận |
|---|---|
| `_onDrained` gián tiếp gọi `Enqueue` gây kẹt? | Không, và cũng không kẹt nếu xảy ra: `Drain:88` đặt `_running = null` **trước** `_onDrained`, nên `Enqueue` lồng nhau sẽ khởi động coroutine mới và gán lại `_running` hợp lệ. Hiện `OnAnimatorDrained → SyncLocalVitals/RefreshLocks` không enqueue. |
| `Enqueue` khi coroutine vừa kết thúc nhưng `_running` chưa null? | Không tới được: cả hai chạy trên main thread; `Drain` chỉ kết thúc bên trong `MoveNext` của Unity, và `_running = null` là lệnh ngay trước `_onDrained`. |
| `ApplyRemaining` bỏ sót hoặc lặp **effect**? | Không. `_appliedEffects++` (`:115`) chạy **sau** apply và **trước** `WaitForSecondsRealtime`, kể cả khi `hit == null` — khớp với `ApplyRemaining` vốn cũng bỏ qua card null. Chỉ `MainMpDelta` sai (M5). |
| `FlushImmediate` bỏ sót turn còn trong queue? | Không: `:60` `while (_queue.Count > 0) ApplyRemaining(dequeue, 0)` — với turn chưa chạy thì `fromIndex = 0` là **đúng** (kể cả `MainMpDelta`). |
| Gói WAIT dùng vật phẩm (0 effect) có làm animator lỗi? | Không. `HitsOpponent` false, vòng effect rỗng, vẫn `yield` ở `:119` nên `StartCoroutine` trả handle hợp lệ (không rơi vào ca "coroutine hoàn tất đồng bộ ⇒ `_running = null`"). |
| Gói độc / phản đòn (`petId = -1`) có lao sang? | Không: `HitsOpponent:128` `turn.ActorId <= 0` ⇒ false. Effect vẫn áp lên `getFocus()`. ✔ |
| Kỹ năng buff lên chính mình có lao sang? | Không, **đúng ý đồ** — `PetBattle.cs:1140`/`:1429` đặt `petId = getUserTurnId()` khi `isSkillBuff()` ⇒ effect trùng `turn.ActorId`. *Ngoại lệ nhỏ:* buff **bị trượt** thì `:1144` add thêm effect `SKILL_MISS` lên `getFocus()` ⇒ vẫn lao. Hiếm, chấp nhận. |
| `MY_PET_INFO` mỗi lượt (`nextTurn:758`) có gây float `+N` giả? | Không — `nextTurn()` gửi **sau** `sendPetAttack`, lúc đó animator đang bận ⇒ hoãn ⇒ `OnAnimatorDrained` áp khi HP client đã khớp ⇒ `healed == 0`. Đúng thiết kế. (Vấn đề chỉ ở 2 call site nghịch thứ tự — H3.) |
| `_pendingVitals` bị nuốt khi nhiều gói liên tiếp? | Không phải lỗi: snapshot là **giá trị tuyệt đối**, gói sau đè gói trước là đúng ngữ nghĩa. |
| Bình máu (R7) có hiện `+N` không? | **Có.** `useItem:1687` gửi gói 37 **trước**, `:1695` `sendMyPetInfo()` sau ⇒ animator bận ⇒ hoãn ⇒ drain (≈0.25s) → `healed > 0` → float xanh + thanh HP lerp. S7 đạt. |
| `sendTurnSkipped` có phá PvP? | Không. `sendPetAttack` nhánh `!petAttackMob` gửi bản sao cho `passivePlayer`; `getFocus()`/`getUserTurnId()` ở PvP trả đúng 2 user_id. |
| Jar cũ nhận gói `sendTurnSkipped` có crash? | Không — cùng hình dạng byte-for-byte với gói đánh-trượt sẵn có (`:306-308`). Không đổi wire format. |
| `LocalStarts = false` có đúng? | Đúng, khớp phân tích M5 phase 01: ctor PvE `setIsActiveTurn(false)` ⇒ `getUserTurnId()` = `mob.getMobId()`, `MobAttackTime` khởi tạo `= Now` ⇒ `mobAttack()` ngay tick đầu. Comment mới ở `BattleHandler.cs:79-83` đã viết đúng. |
| `OnPotion` bỏ `MarkSent` — còn lỗ double-send? | Không. Mở menu 2 lần vô hại; gói 37 thật chỉ tới sau khi chọn item. H4 phase 01 đóng đúng. |
| C1 phase 01 (mật khẩu trong `bin/`) | **Đã xử lý**: `git ls-files SRCGOPETGOC/GServer/bin` trả 0; file vẫn còn trên đĩa. Vẫn nên đổi mật khẩu `gopettest` nếu dùng lại ở nơi khác. |
| Sau `Close()` còn NRE qua `GameSession.cs:255`? | Không: `BattleCoordinator.Close:93` đặt `_view = null` **trước** `DestroyWorldObject`, và `?.` chặn. |
| Ngân sách dòng (N1) | Tất cả ≤ 200: `BattleView` 182, `.Actions` 80, `.State` 39, `BattlePetCard` 144, `BattleFloatText` 60, `BattleStatBar` 139, `BattleTurnAnimator` 152, `BattleSkillPanel` 199 (sát trần). Không thêm tên mới vào allowlist. ✔ |
| Bảo mật | Không có input client nào ảnh hưởng damage/HP thật; `checkWhoseTurn` vẫn là nguồn chân lý; `SyncLocalVitals` áp giá trị tuyệt đối từ server. Không thêm đường gửi gói. ✔ |

## Điểm tốt

- Tách `BattleView.State.cs` đúng ranh giới ngữ nghĩa (không phải cắt cơ học cho đủ 200
  dòng) — `partial` chỉ chứa method, không field/ctor.
- `_turn = new BattleTurnState(...)` được hoist lên đầu `Build` với comment giải thích lý do
  — sửa M7 phase 01 đúng gốc chứ không vá triệu chứng.
- `ReturnHome` gán **tuyệt đối** về `_home` thay vì cộng dồn ⇒ không trôi tích luỹ (S3 đạt,
  kể cả khi hoạt cảnh bị cắt giữa chừng nhờ `SnapHome`).
- `WaitForSecondsRealtime` xuyên suốt, khớp `Time.unscaledTime` của phần còn lại.
- `_appliedEffects` để `FlushImmediate` biết áp tiếp từ đâu — đúng ý tưởng, chỉ hụt 1 biến
  cho `MainMpDelta`.
- Comment server trỏ số dòng cụ thể (`:336-343`, `:1337`, `:1401`) và giải thích **tại sao**
  giá trị cũ sai — loại comment hiếm và đáng giá.
- Lập luận "không cần guard trừ exp 2 lần" ở `:874-881` được chứng minh bằng 3 đường phản
  chứng, kiểm lại đều đúng.

## Hành động đề xuất (theo thứ tự)

1. **C2** — lưu cờ `left` trong `BattlePetCard`, tính hướng lao từ đó (gộp luôn M8). Không
   sửa thì hoạt cảnh của quái sai ngay lần chạy đầu.
2. **C1** — `try/catch` quanh render effect (`BattleTurnAnimator.cs:105-117`) +
   `try/finally { _running = null; }` trong `Drain` + watchdog animator trong
   `BattleView.Update`.
3. **H4** — đưa `_result == null` vào `RefreshLocks()` và cho `SyncLocalVitals` gọi
   `RefreshLocks()` (1 chỗ khoá duy nhất).
4. **H3** — hoãn `SyncLocalVitals` 1 frame để gói 37 cùng tick server kịp `Enqueue`.
5. **M5** — chuyển `MainMpDelta` ra `Drain` hoặc thêm cờ `_mainApplied`.
6. **M6** — `sendTurnSkipped` trỏ effect vào chính bên bị định thân.
7. **M7** — hoist `_animator = BattleTurnAnimator.Attach(...)` lên đầu `Build`.
8. M9/M10, L11 (comment exp + xác nhận ý đồ trừ exp PvE), L17, L18.

## Câu hỏi còn treo

- **Trừ exp khi thua quái có đúng ý đồ không?** Jar gốc có hành vi này không, hay đây là
  tính năng mới? Nếu mới thì sàn `-20_000_000` (dùng chung với PK) là quá nặng cho PvE.
- Có sprite hiệu ứng nào đang thiếu trong `Resources/Jar/...` không? Nếu có, C1 không phải
  rủi ro lý thuyết mà là bug xảy ra ngay — nên kiểm trước khi merge (bước 3 `verify.ps1`
  đang FAIL sẵn cho thấy asset **có** lệch).
- `TỈ_LỆ_ĐỊNH_THÂN_KHI_ĐÁNH_TRÚNG` / `PER_STUN_1_TURN` đã có item cấp ở bản live chưa? (câu
  hỏi treo từ phase 01, quyết định mức ưu tiên của M6.)
