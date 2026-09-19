# Gap: 6 yêu cầu màn hình đánh quái vs code hiện tại

Ngày 2026-09-19. Ảnh tham chiếu: BattleView đang chạy (rùa baby Lv.2 vs gà rừng Lv.1).
Đối chiếu `GopetUnityClient/Assets/Scripts` + `SRCGOPETGOC/GServer`.

## Tổng quan

| # | Yêu cầu | Trạng thái |
|---|---|---|
| 1 | Đánh theo lượt, click nút 2 kiếm → nhân vật lao sang đánh quái | Lượt ✅ / **lao sang ❌** / **nút không khoá theo lượt ❌** |
| 2 | Dùng kỹ năng → hiệu ứng tương ứng lên đối phương; skill lấy từ DB | ✅ (đã chạy) — còn lỗ double-send |
| 3 | HP/MP đối phương giảm khi trúng đòn/kỹ năng | ✅ (đã chạy) — thiếu lerp/so le |
| 4 | Click bình máu cạnh nút kiếm → cộng HP | ✅ (đủ đường, cần verify runtime) |
| 5 | Xin thua / Quay lại → xin thua, **bị trừ kinh nghiệm** | Xin thua ✅ / **trừ EXP ❌ (server chưa có)** |
| 6 | Dưới icon VS hiện "Đến lượt bạn"; lượt quái thì trống | UI ✅ / **logic sai 3 chỗ ❌** |

---

## 1. Đánh theo lượt + lao sang đánh

**Đã có:** vòng lượt do server điều khiển hoàn toàn. Nút Tấn công
(`Battle/BattleActionBar.cs:104` — sprite `Battle/btn-attack-big`, chính là icon 2 kiếm
trong ảnh) → `BattleView.OnAttack()` (`BattleView.cs:145`) → `[81][37][1]`.

**Thiếu — animation lao sang:** `BattlePetCard` không có animation di chuyển; chỉ có
animation frame đứng yên (`BattlePetCard.cs:70-76`). JAR có: `ei.java:202-211` — lao
±20px về phía mục tiêu → phát FX → lùi về vị trí cũ.

**Sai — nút không khoá theo lượt:** hiện khoá bằng **timer 3.5s**
(`BattleActionBar.cs:145-149`). Nghĩa là hết 3.5s nút mở lại dù vẫn đang lượt quái →
bấm sẽ bị server trả `redDialog("Chưa tới lượt của bạn")` (`PetBattle.cs:317`).
Thêm nữa **nút kỹ năng không nằm trong `_buttons`** → không bị khoá chút nào → double-send.
JAR khoá theo **trạng thái lượt** (`fr.b`/`fr.c`, reset về -1 khi gói 37 về) — đúng hơn.

## 2. Kỹ năng + hiệu ứng

**Đã chạy đủ.** Skill lấy từ DB: server `writeMyPetInfo` (`PetBattle.cs:559-570`) đọc
`pet.skill` (DB) → gửi `{id, name, description, mpCost}` → `BattleSkillPanel` render.
Ảnh cho thấy "song kích 1 / MP 50" và "giảm sát thương 1 / MP 50" → đường này ổn.

Hiệu ứng: `BattleView.cs:71` → `BattleEffectView.Play(transform, target.EffectAnchor, effect.SkillId)`.
Ánh xạ (`BattleEffectView.cs:56-65`) khớp JAR:
- `0` / `2` → `SlashEffect` (đánh thường / chí mạng)
- `101..124` → atlas theo tên (`SongKich`, `Satthuong`, `CuongNo`, `Bang`, `SamSet`, `Lua`...)
- `>=125` → `BattleActorEffectView` nạp `.anu`

Lưu ý: `effect.SkillId` mang **2 nghĩa** — với đánh thường nó là marker
(`SKILL_NORMAL=0`, `SKILL_MISS=1`, `SKILL_CRIT=2` — `TurnEffect.cs:23-25`), với kỹ năng
nó là skillId thật (`PetBattle.cs:1102`). Đúng thiết kế, không phải lỗi.

**Cần sửa:** khoá nút kỹ năng cùng cơ chế với nút đánh (mục 1).

## 3. HP/MP giảm

**Đã chạy.** `BattlePetCard.Apply` (`:39-45`) clamp HP/MP + `BattleFloatText`
→ `BattleView.RefreshHuds()` (`:139`) → `BattleHudPanel.UpdateVitals`.

**Polish so với JAR:**
- Thanh HP **snap**; JAR **lerp** (chia đôi delta mỗi frame — `bd.java:196-240`)
- Số HP và MP hiện **cùng lúc**; JAR hiện HP trước, MP sau **~1s** (`ei.java:213-291`)
- Unity apply **toàn bộ effect trong 1 frame**; JAR có hàng đợi, **1 step/frame**
  (`di.java:82-89`)

## 4. Bình máu → cộng HP

**Đủ đường, chưa verify runtime.** `BattleActionBar.MakeItemBtn` (`:82`, sprite
`Battle/btn-item` — icon bình đỏ trong ảnh) → `SendUseItem()` → `[81][37][3][int 0]`
→ server mở menu `MENU_SELECT_ITEM_SUPPORT_PET = 1016` (`PetBattle.cs:209`)
→ Unity render bằng `MenuScreen` generic (UiRoot sortingOrder **100** > BattleView **40**
nên menu nổi trên overlay) → chọn → `petBattle.useItem(player, item)`
(`MenuController.selectMenu.cs:978-985`) → server gửi gói 37 có `HpDelta` dương.

`PutInt(0)` thừa trong `SendUseItem` là **giống JAR**, không phải bug.

## 5. Xin thua / Quay lại → trừ EXP

**Client đã đủ:** nút "Xin thua" (`BattleActionBar.cs:62`) và "‹ Quay lại"
(`BattleTopBar.cs:43`) đều vào `OnSurrenderClicked` → `YesNoDialog` → `SendSurrender()`
→ `[81][37][5]`.

**SERVER CHƯA TRỪ EXP TRONG PvE.** Đường đi:
`PetBattle.onMessage` enqueue `IsSurrender` (`:211-217`) → `surrender()` (`:1870`)
đặt `isClose = true` → `hasWinner()` true → `win()`.
Trong `win()`, `getWinId()` khi `isClose` trả về **`mob.getMobId()`** (`:959`) → rơi vào
nhánh thua (`:857-860`):

```csharp
else
{
    activePlayer.controller.delayTimeHealPet = ... TIME_DELAY_HEAL_WHEN_MOB_KILL_PET;
}
```

Chỉ đặt thời gian chờ hồi máu, **không trừ EXP**. Đoạn trừ EXP duy nhất nằm ở nhánh PvP/PK
(`:872+`, `exp_sub = 10% exp của cấp hiện tại`, gắn với `pkPoint`).

→ Muốn "xin thua bị trừ kinh nghiệm" phải **sửa server**, và cần chốt **công thức trừ**.

## 6. Nhãn lượt dưới VS — 3 lỗi

UI đã đúng ý: `BattleVsIndicator.SetTurn(bool)` (`BattleVsIndicator.cs:45`)
→ `isLocalTurn ? "Đến lượt bạn" : ""`. Nhưng logic gọi sai:

**Lỗi 1 — ngược lượt.** `BattleView.cs:64`:
```csharp
_vsIndicator?.SetTurn(turn.ActorId == _start.LocalPet.ActorId);
```
Server gửi `mainTurnData.petId = getUserTurnId()` = **người vừa ra đòn**, rồi mới gọi
`nextTurn()` (`PetBattle.cs:307-308`). Nên khi `ActorId == mình` nghĩa là mình **vừa đánh
xong** → giờ tới lượt quái. Đang hiển thị ngược 100%. Phải đảo thành `!=`.

**Lỗi 2 — trạng thái mở trận sai với PvE.** `BattleStart.LocalStarts` chỉ được đọc trong
`OnPlayerBattle` (opcode 59, `BattleHandler.cs:87`); `OnMobBattle` (opcode 36) **không hề
gán** → mặc định `false`. `BattleView.Build` gọi `SetTurn(_start.LocalStarts)`
(`BattleView.cs:107`) → **mở trận đánh quái luôn hiện trống** dù người chơi đi trước.
Đúng khớp với ảnh chụp. PvE người chơi luôn đi trước (`isActiveTurn` khởi tạo true)
→ gán cứng `LocalStarts = IsParticipant` trong `OnMobBattle`.

**Lỗi 3 — gói hệ thống xoá nhãn.** Độc/phản đòn gửi `petId = -1`
(`PetBattle.cs:1547`, `:1617`) → `SetTurn(false)` → nhãn biến mất giữa lượt mình.
Phải bỏ qua khi `ActorId <= 0`.

**Vị trí:** nhãn đang ở `anchorY 0.54..0.59 + deltaY`, badge VS ở `0.58..0.76 + deltaY`
→ chồng 0.01 vào đáy badge. Nên hạ nhãn xuống dưới badge hẳn.

---

## Việc cần làm

**Client (Unity)**
1. `BattleHandler.OnMobBattle`: gán `start.LocalStarts = start.IsParticipant`.
2. `BattleView.Apply`: đảo điều kiện `SetTurn`, bỏ qua `ActorId <= 0`.
3. `BattleVsIndicator`: hạ nhãn xuống dưới badge.
4. Thay `BattleActionBar.Lock()` timer 3.5s bằng **khoá theo lượt**; đưa nút kỹ năng vào
   cùng cơ chế (chặn double-send).
5. `BattlePetCard`: animation lao sang đánh (theo `ei.java:202-211`).
6. (polish) hàng đợi animation 1 step/frame, lerp thanh HP, float text HP/MP so le.
7. Verify runtime đường bình máu (menu 1016 nổi trên overlay).

**Server**
8. `PetBattle.win()` nhánh PvE thua: trừ EXP theo công thức được chốt.

## Câu hỏi cần chốt

- **Công thức trừ EXP khi thua/xin thua PvE?** (PvP đang dùng 10% exp của cấp hiện tại)
- Trừ EXP cho **mọi lần thua** hay **chỉ khi chủ động xin thua**?
- Animation lao sang: giữ biên độ nhỏ như JAR (±20px, về chỗ cũ) hay lao hẳn sang cạnh quái?
- Có làm phần polish (hàng đợi animation, lerp, float text so le) trong đợt này không?
