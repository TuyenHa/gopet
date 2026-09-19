# Phase 02 — Lao sang đánh + hàng đợi lượt + lerp/float so le

## Context Links

- Overview: [plan.md](plan.md) · Trước đó: [phase-01](phase-01-vong-luot-va-khoa-nut.md)
- Report: `plans/reports/investigation-260919-1102-danh-quai-jar-vs-unity.md` §6
- Tham chiếu jar: `di.java:38-89` (hàng đợi), `e.java` (step), `ei.java:202-211` (lunge),
  `ei.java:213-291` (float so le), `bd.java:196-240` (lerp thanh)

## Overview

- **Priority:** P1
- **Status:** done · **Chặn bởi:** phase 01
- **Effort:** 4h
- Biến `BattleView.Apply` từ "nổ hết trong 1 frame" thành hàng đợi phát tuần tự:
  lao sang → hiệu ứng → damage float → HP lerp → lùi về. Kèm vá đường hiển thị HP bình máu.

## Key Insights

1. **Hiện tại apply tất cả trong 1 frame.** `BattleView.cs:61-78` duyệt `turn.Effects` và
   gọi `target.Apply` + `BattleEffectView.Play` + `PlayHitSound` liên tiếp không chờ.

2. **Sprite pet là 1 strip ngang N frame dùng chung.** `BattlePetCard.SetTexture:65-67` chia
   `texture.width / _pet.FrameCount`; `Update:71-77` quay vòng mỗi `0.16s`;
   `ShowFrame:79-83` set `uvRect`. **Không có bộ frame "chạy" riêng.**
   → "chạy" = quay vòng chính các frame đó nhanh hơn (0.08s) trong lúc di chuyển. KHÔNG tạo sprite sheet mới.

3. **Card là anchor điểm → dời bằng `anchoredPosition`.**
   `BattlePetCard.Create:29` đặt `anchorMin = anchorMax = (left ? 0.31 : 0.66, 0.45)`.
   Anchor min == max nên `anchoredPosition` là offset pixel thuần → lunge an toàn.
   Khoảng cách 2 card ở độ phân giải tham chiếu 960px: `(0.66-0.31)*960 ≈ 336px`.

4. **HP từ bình máu KHÔNG có trong opcode 37.**
   `PetBattle.cs:336-343` — khi `type == TYPE_EFFECT_WAIT`, wire chỉ ghi `putInt(0); putUTF(""); putInt(mainTurnData.mp)`.
   Trường `hp` của `TurnEffect.createWait(hp, mp, petId)` (`TurnEffect.cs:38-41`) **không bao giờ được ghi**.
   Client khớp đúng: `BattleHandler.cs:112-115` chỉ đọc `MainMpDelta`.
   HP thật đi qua `MY_PET_INFO` (`GameController.cs:1410-1425`: hp/maxHp/mp/maxMp),
   `PetBattle.useItem:1651` gọi `sendMyPetInfo()`. Client nhận ở
   `PetZoneHandler.OnMyPetInfo:65-77` → `GameSession.cs:250-254` **chỉ nối vào `CharacterHud`**.
   → Phải nối thêm vào `BattleView`. Không đổi wire.

5. **`MY_PET_INFO` cũng tới mỗi lượt.** `PetBattle.nextTurn:740-748` gọi `sendMyPetInfo()`.
   Nếu áp thẳng khi hàng đợi đang phát, HP sẽ nhảy trước hoạt cảnh → phải hoãn tới khi queue rỗng.

6. **Thanh HP đang snap.** `BattleStatBar.Set:96-101` → `Refresh:103-108` set `anchorMax` ngay.

7. **Float HP/MP hiện cùng lúc.** `BattlePetCard.Apply:43-44` tạo 2 `BattleFloatText` liền nhau.

8. **Ngân sách dòng sau phase 01:** `BattleView.cs` ≈ 160, `BattlePetCard.cs` 85,
   `BattleStatBar.cs` 125, `BattleFloatText.cs` 51. Đủ chỗ; hàng đợi ra file riêng.

## Requirements

**Chức năng**
- R1: Bên ra đòn lao sang cạnh đối phương, phát hiệu ứng, rồi lùi về đúng chỗ cũ.
- R2: Trong lúc lao/lùi, nhịp frame sprite tăng gấp đôi (0.08s) rồi trả về 0.16s.
- R3: Hiệu ứng, số damage, thanh HP diễn ra tuần tự theo hàng đợi — không chồng lấn.
- R4: Số HP hiện trước, số MP hiện sau ~1s.
- R5: Thanh HP/MP trượt dần (halve-delta mỗi frame) thay vì nhảy.
- R6: Nút hành động khoá suốt thời gian hàng đợi đang phát, mở lại theo `_turn.CanAct` khi cạn.
- R7: Dùng bình máu → thanh HP pet mình **tăng thấy được** + float text `+N` màu xanh.

**Phi chức năng**
- N1: Mọi file ≤ 200 dòng.
- N2: Hàng đợi không được treo: luôn có đường thoát (kết trận / huỷ view).
- N3: Không đổi wire format; không thêm opcode.

## Architecture

### Luồng dữ liệu

```
BattleView.Apply(turn)
  ├─ _turn.ApplyTurnPacket(...)        (phase 01 — tức thì, không hoãn)
  ├─ _vsIndicator.SetTurn(...)          (tức thì)
  ├─ _animator.Enqueue(turn)            (hoãn phần render)
  └─ RefreshLocks()  →  CanAct && _animator.Idle

BattleTurnAnimator (coroutine, 1 turn/lần)
  cho mỗi turn trong hàng đợi:
    1. mainMpDelta  → Card(turn.ActorId).Apply(0, mp)       [float MP]
    2. xác định attacker = Card(turn.ActorId) nếu ActorId > 0
       và có effect gây HpDelta < 0 lên card KHÁC
    3. attacker.PlayLunge(targetX)      chờ 0.22s   (nhịp frame 0.08s)
    4. mỗi effect:  BattleEffectView.Play → PlayHitSound
                    → target.Apply(hp, mp)   [float HP ngay, float MP +1s]
                    → refreshHuds()           [StatBar lerp tự chạy]
                    → chờ 0.12s giữa 2 effect
    5. chờ 0.25s
    6. attacker.ReturnHome()            chờ 0.22s
  khi cạn: onDrained()  →  BattleView.RefreshLocks() + áp MY_PET_INFO đang treo

PetZoneHandler.MyPetInfoReceived (GameSession.cs:250)
  ├─ CharacterHud.SetStats(...)                      (giữ nguyên)
  └─ _battle?.View?.SyncLocalVitals(hp,maxHp,mp,maxMp)
        └─ nếu _animator.Idle  → áp ngay + float text chênh lệch HP nếu > 0
           ngược lại           → cất vào _pendingVitals, áp ở onDrained()
```

### Lunge — số liệu

| Tham số | Giá trị | Lý do |
|---|---|---|
| Đích | `home.x ± 246px` | 336px giữa 2 card trừ ~90px để đứng cạnh, không đè |
| Hướng | trái: `+x`, phải: `−x` | `left` flag đã có ở `Create` |
| Thời gian lao | 0.22s | đủ thấy, không làm chậm nhịp lượt |
| Thời gian lùi | 0.22s | |
| Nhịp frame lúc di chuyển | 0.08s | "chạy" = quay vòng chính frame hiện có, gấp đôi tốc độ |
| Nhịp frame thường | 0.16s | giá trị hiện tại `BattlePetCard.cs:74` |
| Easing | `Mathf.SmoothStep` | tránh cảm giác trượt băng |

Tổng 1 lượt 1 đòn ≈ 0.22 + 0.12 + 0.25 + 0.22 ≈ **0.81s** — an toàn dưới lượt server
(`GopetManager.TimeNextTurn`, client đọc qua `TurnDurationMs`).

### Đường thoát (không treo)

| Tình huống | Xử lý |
|---|---|
| `ShowResult` (opcode 16) tới khi queue còn | `_animator.FlushImmediate()` — áp hết delta còn lại, dừng coroutine, rồi dựng panel |
| `BattleCoordinator.Close()` huỷ GameObject | Coroutine chết cùng GO — không cần dọn thêm |
| Gói tới nhanh hơn queue drain (độc + phản đòn ngay sau đòn) | Xếp hàng theo thứ tự tới; tổng < 2s, vẫn kịp lượt kế |
| Queue kẹt bất thường | `BattleCoordinator.CheckStalled` (`:109-115`) vẫn đóng overlay sau `TurnDurationMs × 3` |

## Related Code Files

**Tạo**
- `GopetUnityClient/Assets/Scripts/Runtime/World/Battle/BattleTurnAnimator.cs` (~130 dòng)

**Sửa**
- `GopetUnityClient/Assets/Scripts/Runtime/World/BattlePetCard.cs` (:39-45 `Apply`, :71-83 `Update`/`ShowFrame`) — thêm lunge + nhịp frame biến thiên
- `GopetUnityClient/Assets/Scripts/Runtime/World/BattleFloatText.cs` (:12-26, :43-49) — thêm `delaySeconds`
- `GopetUnityClient/Assets/Scripts/Runtime/World/Battle/BattleStatBar.cs` (:96-108) — lerp
- `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.cs` (:61-78 `Apply`, :80-85 `ShowResult`, `Build`) — nối animator + `SyncLocalVitals`
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs` (:250-254) — thêm 1 dòng gọi `_battle.View?.SyncLocalVitals`

**Xoá:** không.

> `GameSession.cs` (691 dòng) đã nằm trong allowlist `CODE_HEALTH_EXCEPTIONS.md` — thêm 1 dòng
> không làm hỏng `verify.ps1`, nhưng **không được thêm gì nữa** vào file này ngoài dòng đó.

## Implementation Steps

1. **`BattleFloatText.cs`** — gộp `Create`/`CreateMiss` dùng chung phần dựng Text (DRY),
   thêm tham số `float delaySeconds = 0f`. Cài bằng `_born = Time.unscaledTime + delaySeconds`
   và trong `Update` `if (age < 0f) { _text.color = trong suốt; return; }`.
   Không đổi chữ ký cũ (default param) → không phải sửa call site nào ngoài `BattlePetCard`.

2. **`BattlePetCard.cs`**
   - `Apply(hpDelta, mpDelta)` (`:39-45`): đổi float MP thành
     `BattleFloatText.Create(transform, mpDelta, true, 1f)` (so le 1s theo `ei.java:213-291`).
   - thêm `private Vector2 _home; private float _frameInterval = 0.16f;`
     `_home` gán trong `Create` sau khi set `anchoredPosition` (mặc định `Vector2.zero`,
     do `Create:29-30` chỉ set anchor + sizeDelta → `anchoredPosition` là `(0,0)`; vẫn đọc lại
     từ rect để không phụ thuộc giả định).
   - `Update` (`:71-77`) dùng `_frameInterval` thay hằng `0.16f`.
   - thêm:
     ```csharp
     public float HomeX => _home.x;
     public IEnumerator PlayLunge(float targetX, float seconds);  // _frameInterval = 0.08f khi chạy
     public IEnumerator ReturnHome(float seconds);                // trả _frameInterval = 0.16f
     public void SnapHome();                                      // dùng cho FlushImmediate
     ```
     Cả hai coroutine nội suy `anchoredPosition.x` bằng `Mathf.SmoothStep`.
   - `using System.Collections;` ở đầu file.
   - Ước tính 85 → ~130 dòng.

3. **`BattleStatBar.cs`** — thêm `private float _display;`
   - `Set(current, max)` (`:96-101`): giữ nguyên gán `_current/_max`; nếu `_display == 0 && _current > 0`
     (lần đầu) thì `_display = _current` để không lerp từ 0 lúc mở trận.
   - `Refresh` dùng `_display / _max` cho `anchorMax`, `_label.text` vẫn `$"{_current}/{_max}"`.
   - trong `Update` (`:110-123`) thêm đầu hàm:
     ```csharp
     if (Mathf.Abs(_display - _current) > 0.5f) { _display += (_current - _display) * 0.5f; Refresh(); }
     else if (_display != _current) { _display = _current; Refresh(); }
     ```
     (halve-delta mỗi frame — port `bd.java:196-240`).
   - 125 → ~140 dòng.

4. **`BattleTurnAnimator.cs`** (mới, `Gopet.Runtime.World.Battle`) — MonoBehaviour gắn trên
   GameObject của `BattleView`.
   ```csharp
   public sealed class BattleTurnAnimator : MonoBehaviour
   {
       public static BattleTurnAnimator Attach(GameObject host,
           Func<int, BattlePetCard> card, Action refreshHuds,
           Action<BattleEffect, Transform> playHitSound, Action onDrained);
       public bool Idle { get; }
       public void Enqueue(BattleTurn turn);
       public void FlushImmediate();   // áp hết delta còn lại, SnapHome, xoá hàng đợi
   }
   ```
   - `Queue<BattleTurn> _queue`; `Coroutine _running`; `Enqueue` khởi động `Drain()` nếu chưa chạy.
   - `Drain()` thực hiện đúng chuỗi 1..6 ở mục Architecture.
   - `FlushImmediate()`: `StopAllCoroutines()`, với mọi turn còn trong hàng đợi (kể cả turn đang dở
     — giữ `_inFlight` để biết đã áp tới effect thứ mấy) áp nốt `Apply(hp, mp)` không hoạt cảnh,
     `SnapHome()` cả 2 card, `refreshHuds()`, `onDrained()`.
   - Không dùng `WaitForSeconds` (ảnh hưởng `timeScale`) → dùng `WaitForSecondsRealtime`,
     đồng bộ với phần còn lại của battle UI vốn dùng `Time.unscaledTime`.

5. **`BattleView.cs`**
   - `Build`: sau khi dựng card/hud, `_animator = BattleTurnAnimator.Attach(gameObject, Card,
     RefreshHuds, PlayHitSound, OnAnimatorDrained);`
   - `Apply` (`:61-78`) rút gọn còn: gate battleId → `_turn.ApplyTurnPacket` → `SetTurn` →
     `_cooldowns.OnTurnAdvanced` → `_animator.Enqueue(turn)` → `RefreshLocks()`.
     **Bỏ** vòng lặp effect và `RefreshHuds()` khỏi đây (animator lo).
   - `RefreshLocks()` (phase 01) thêm điều kiện `_turn.CanAct && _animator.Idle`.
   - `OnAnimatorDrained()`: `RefreshLocks()`; nếu `_pendingVitals != null` → áp rồi xoá.
   - `ShowResult` (`:80-85`): gọi `_animator?.FlushImmediate()` **trước** khi dựng panel.
   - thêm:
     ```csharp
     public void SyncLocalVitals(int hp, int maxHp, int mp, int maxMp)
     ```
     Nếu `_animator.Idle`: tính `delta = hp - _left.Hp`; `_left.SetVitals(hp, maxHp, mp, maxMp)`;
     nếu `delta > 0` → `BattleFloatText.Create(_left.transform, delta, false)`; `RefreshHuds()`.
     Ngược lại cất `_pendingVitals`.
   - `BattlePetCard` cần thêm `public void SetVitals(int hp,int maxHp,int mp,int maxMp)` (gán tuyệt đối,
     không phải delta) — 4 dòng.

6. **`GameSession.cs:250-254`** — trong lambda `MyPetInfoReceived` thêm đúng 1 dòng:
   `s._battle?.View?.SyncLocalVitals(p.Hp, p.MaxHp, p.Mp, p.MaxMp);`
   (`_battle` khai báo `:59`, `BattleCoordinator.View` có sẵn `:44`.)

7. **Compile check:** `powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1`

## Todo List

- [x] `BattleFloatText`: gộp dựng Text + tham số `delaySeconds`
- [x] `BattlePetCard`: `_home`, `_frameInterval`, `PlayLunge`, `ReturnHome`, `SnapHome`, `SetVitals`
- [x] `BattlePetCard.Apply`: float MP trễ 1s
- [x] `BattleStatBar`: `_display` + halve-delta lerp
- [x] Tạo `BattleTurnAnimator.cs` (queue, `Drain`, `FlushImmediate`)
- [x] `BattleView.Apply` chuyển sang enqueue; `RefreshLocks` xét `_animator.Idle`
- [x] `BattleView.ShowResult` gọi `FlushImmediate()` trước
- [x] `BattleView.SyncLocalVitals` + hoãn khi queue bận
- [x] `GameSession.cs:250` nối `MyPetInfoReceived` vào `SyncLocalVitals`
- [x] Chạy `verify.ps1` — 10/10 xanh

## Success Criteria

| # | Đo được bằng |
|---|---|
| S1 | Bấm Tấn công → pet trái chạy sang cạnh pet phải, phát FX, chạy về đúng vị trí ban đầu |
| S2 | Trong lúc chạy, sprite quay frame nhanh gấp đôi; đứng yên trở lại 0.16s |
| S3 | Sau 20 lượt, `anchoredPosition` của cả 2 card bằng đúng `_home` (không trôi tích luỹ) |
| S4 | Số HP hiện trước, số MP hiện sau ~1s (quay màn hình đếm khung hình) |
| S5 | Thanh HP trượt trong ~0.3s thay vì nhảy |
| S6 | Nút hành động xám suốt hoạt cảnh, sáng lại đúng lúc queue cạn + tới lượt mình |
| S7 | Bình máu qua menu 1016 → thanh HP pet mình tăng + float `+N` xanh |
| S8 | Trận kết thúc giữa hoạt cảnh → panel kết quả hiện ngay, card về chỗ, HP đúng số cuối |
| S9 | Trúng độc + phản đòn trong 1 lượt → 2 hiệu ứng phát nối nhau, không chồng |
| S10 | `verify.ps1` 10/10 OK |

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| Card trôi dần khỏi vị trí gốc sau nhiều lượt | Trung bình | Trung bình | Lưu `_home` **một lần** lúc `Create`; `ReturnHome` gán tuyệt đối `_home`, không cộng dồn. S3 kiểm chứng |
| Hoạt cảnh dài hơn lượt server → tụt hậu tích luỹ | Thấp | Cao | Tổng ngân sách 0.81s/lượt; `FlushImmediate` khi kết trận; queue nối tiếp không bỏ gói |
| `MY_PET_INFO` áp đè giữa hoạt cảnh làm HP nhảy ngược | Trung bình | Trung bình | Hoãn vào `_pendingVitals`, chỉ áp khi `Idle` (bước 5) |
| Float text bám vào card đang lao → chữ trôi theo | Trung bình | Thấp | Chỉ bên **bị đánh** mới có float, mà bên bị đánh không lao. Nếu phản đòn gây float trên kẻ lao, chấp nhận — ghi chú trong code |
| `WaitForSeconds` bị `timeScale` làm treo | Thấp | Cao | Dùng `WaitForSecondsRealtime` (bước 4) |
| Coroutine chạy sau khi GameObject bị huỷ → `MissingReferenceException` | Thấp | Trung bình | Coroutine gắn trên chính GO của `BattleView`; Unity dừng khi `Destroy`. Thêm `if (this == null) yield break;` đầu mỗi vòng |
| `BattlePetCard` vượt 200 dòng | Thấp | Thấp | Ước tính ~130; nếu vượt, tách `BattlePetCard.Motion.cs` partial |

## Security Considerations

- Toàn bộ thay đổi là lớp trình bày. Không có input nào của client ảnh hưởng damage/HP thật.
- `SyncLocalVitals` áp **giá trị tuyệt đối từ server**, không cộng dồn phía client
  → không thể lệch trạng thái do gói trùng/mất thứ tự.
- Không thêm đường gửi gói mới → không tăng bề mặt spam về phía server.

## Next Steps

- Chặn: phase 04 (test matrix phụ thuộc hoạt cảnh đã xong).
- Không chặn phase 03.
