# Hệ thống chiến đấu Gopet

Tài liệu này mô tả tầng chiến đấu (PvE, PvP, đấu trường, vượt ải) giữa server
`SRCGOPETGOC/GServer` và client `GopetUnityClient`. Dựa trên audit ngày 2026-09-17
và các bản vá trong plan
[260917-1812-pvp-arena-battle-parity](../plans/260917-1812-pvp-arena-battle-parity/plan.md).

## 1. Ba đường vào PvP

Cả 3 đường đều đổ về **một hàm duy nhất**:
`GopetPlace.startFightPlayer(user_id, player, isPkMode, coinBet)`
(`Place/GopetPlace.cs:466`).

| Đường | Opcode vào | `isPkMode` | `coinBet` | Ghi chú |
|---|---|---|---|---|
| Thách đấu | `PLAYER_CHALLENGE` (12) | `false` | `> 0` (min 2000) | Đối thủ nhận Yes/No `DIALOG_INVITE_CHALLENGE` (6) |
| PK | `PET_SERVICE/PLAYER_PK` (96) | `true` | `0` | Cấm map 11/12/22, `AntiPK`, `pkPoint <= 10` |
| Đấu trường | Sự kiện tự ghép cặp | `false` | `0` | `ArenaEvent.NextTurn()` → `ArenaMap.AddBattle()` |

## 2. Vòng lượt

- Timeslot mỗi lượt = `GopetManager.TimeNextTurn = 25s`
  (`Manager/GopetManager.cs:228`). Hết giờ → server tự `nextTurn()`.
- Client gửi `PET_BATTLE` (37) + sub: `ATTACK`, `USE_SKILL` (4) + skillId,
  `USE_ITEM` (3).
- Damage **luôn do server** tính (`makeDamage()` `PetBattle.cs:1131`):
  `atk` → buff skill (`SKILL_BUFF_DAMGE`, `SKILL_SKIP_DEF`, `TRUE_DAMGE`)
  → buff trạng thái → trừ `def` đối thủ (+`DEF_PER`) → true damage cộng riêng.
  Crit ×2. Miss qua `randMiss()`. Kèm stun, độc (`updateDamageToxic`), phản đòn
  (`updateDamagePhanDoan`), hút mana (`DOT_MANA`), hồi máu (`RECOVERY_HP`).
- Skill cooldown = 3 lượt (`MAX_SKILL_COOLDOWN`), hằng số. Client tự đếm
  qua `SkillCooldownTracker` — server vẫn từ chối nếu client sai.

### 2.1 `ActorId` của gói 37 là người VỪA ra đòn

Đây là chỗ dễ hiểu ngược nhất của cả giao thức. `sendPetAttack()` ghi
`mainTurnData.petId = getUserTurnId()` rồi **mới** gọi `nextTurn()`
(`PetBattle.cs:307-308` đánh thường, `:1109-1110` kỹ năng). Nên lượt kế tiếp
thuộc về **bên còn lại**: `isLocalTurn = ActorId != localActorId`.

Ba ngoại lệ bắt buộc phải xử lý riêng — `Gopet.UiLogic.BattleTurnState` cài
đúng bảng này:

| Gói | Nguồn | Xử lý |
|---|---|---|
| `ActorId == -1` | độc `PetBattle.cs:1617`, phản đòn `:1547` | giữ nguyên lượt, vẫn áp damage |
| `type == WAIT && 0 effect` | dùng vật phẩm `:1643` — **không gọi `nextTurn()`** | giữ nguyên lượt, mở khoá nút |
| còn lại | `:307`, `:1109` | đảo lượt |

Kỹ năng cũng dùng `WAIT` nhưng **luôn kèm ≥1 effect** (`:1102`/`:1106`), nên số
effect là thứ phân biệt được hai ca.

> **BẪY: `ActorId` âm KHÔNG có nghĩa là gói hệ thống.**
> `GopetPlace.cs:90` sinh id quái bằng `-Utilities.nextInt(2, int.MaxValue - 12)` — **id quái
> luôn âm**, dải `[-(int.MaxValue-12), -2]`. Chỉ đúng hằng `-1` mới là gói hệ thống.
> Dùng `ActorId <= 0` sẽ nuốt sạch gói lượt của quái: lượt không bao giờ lật về người chơi,
> nhãn "Đến lượt bạn" không hiện, và với cách khoá nút theo lượt thì **nút đánh chết cả trận**.
> Đã xảy ra thật ngày 2026-09-19; ca hồi quy ở `BattleTurnStateTests.IdQuaiAm_VanPhaiDoiLuot`.

### 2.2 PvE: quái đi trước

Constructor PvE gọi `setIsActiveTurn(false)` (`PetBattle.cs:66`) ⇒ `getUserTurnId()`
trả `mob.getMobId()`; `MobAttackTime` khởi tạo `= DateTime.Now` (`:33`) nên đã quá
hạn ngay ⇒ `update()` gọi `mobAttack()` ở tick đầu tiên. Wire PvE **không** có cờ
"ai đi trước" nên client gán cứng `LocalStarts = false` (`BattleHandler.OnMobBattle`).

### 2.3 Lượt trôi qua vẫn phải có gói

Các nhánh "bên tới lượt không làm gì" trước đây gọi thẳng `nextTurn()` mà không gửi
gì. Client khoá nút theo lượt sẽ kẹt tới khi hết `TimeNextTurn` (25s). Nay dùng
`PetBattle.sendTurnSkipped()` — gửi đúng byte-shape của một đòn trượt
(main NORMAL + 1 effect `SKILL_MISS`), effect trỏ vào **chính bên bị định thân**
để client không cho nó lao sang đánh. Gọi ở: `petAttack` stun, `useSkill` stun,
`mobUseNormalAttack` stun, `mobUseSkill` stun/thiếu MP. Wire format không đổi,
jar cũ render như một đòn trượt.

### 2.4 Khoá nút theo lượt, không theo đồng hồ

Client khoá nút Tấn công / Thuốc / mọi nút kỹ năng khi `!IsLocalTurn` hoặc đã gửi
hành động mà chưa nhận gói 37 — mô hình `fr.b`/`fr.c` của jar
(`fr.java:240-294`). Nút "Xin thua"/"Quay lại" **không** bị khoá theo lượt: server
xếp hàng hành động này (`PetBattle.cs:211-217`).

Hai ngoại lệ đã biết:
- Nút **Thuốc** không đặt cờ chờ, vì server đáp bằng menu chọn vật phẩm
  (`MENU_SELECT_ITEM_SUPPORT_PET = 1016`) chứ không phải gói lượt.
- Ba nhánh `useSkill` bị từ chối bằng `redDialog` (`:1162`, `:1167`, `:1172`) không
  gửi gói 37 và cũng không đổi lượt → nút mở lại nhờ watchdog 8s trong
  `BattleView.Update`. Client đã tự chặn MP/cooldown nên hiếm khi chạm tới.

### 2.5 HP hồi từ vật phẩm KHÔNG đi qua opcode 37

Gói lượt loại `WAIT` chỉ ghi `putInt(0); putUTF(""); putInt(mp)` (`PetBattle.cs:336-343`)
— trường `hp` của `TurnEffect.createWait(hp, mp, petId)` **không bao giờ lên wire**.
HP thật đi qua `MY_PET_INFO` (`GameController.cs:1410-1425`). Client phải nối gói này
vào cả HUD lẫn màn đấu (`BattleView.SyncLocalVitals`).

Snapshot đó mang giá trị **tuyệt đối** và vài đường server gửi nó **trước** gói lượt
(`addRecovery` `:1184`, `mobUseSkill` `:1438`), nên client hoãn ít nhất một frame và
chờ hàng đợi hoạt cảnh cạn rồi mới áp — áp ngay sẽ trừ máu hai lần.

### 2.6 Hàng đợi hoạt cảnh

`BattleTurnAnimator` phát từng lượt tuần tự (lao sang → hiệu ứng → số damage →
thanh HP → lùi về), port `di.java:82-89` + `e.java`. Ba điểm bắt buộc:

- Hướng và quãng lao tính từ **anchor** của card, không từ `anchoredPosition`
  (cả hai card đều ở `(0,0)` vì `Create` chỉ đặt anchor).
- Lỗi khi dựng hiệu ứng phải bị nuốt tại chỗ — `JarSkin` ném khi thiếu sprite, mà
  Unity dừng hẳn coroutine khi có exception.
- `Drain()` dùng `try/finally` để luôn xoá cờ đang-chạy, kể cả khi bị dừng giữa chừng.

### 2.7 Hiệu ứng xỉu

Pet hết máu thì nằm vật ra: xoay `-90°` quanh gốc chân (pivot sprite là `(0.5, 0)`), lún
xuống 12px, mờ còn 75%, trong 0.45s — `BattlePetCard.FaintIfDown()`. Góc phải tròn 90° và
lún đủ sâu thì thân mới nằm sát đất; bản đầu dùng `-80°`/6px nên dừng ở dáng ngã dở, chụp
màn hình lúc đòn kết liễu vừa xong sẽ thấy con vật treo nghiêng giữa không khí. Animation đi bộ đóng
băng và `HitsOpponent` trả `false` nên pet đã xỉu không lao đi đâu nữa.

Card bên trái có `localScale.x = -1` (lật hình) nên **cùng một góc âm** cho hai bên đổ ra
hai phía ngược nhau — không cần phân nhánh trái/phải.

Jar gốc làm khác: ép dẹp sprite xuống đất theo từng lát 4px, vẽ lát cao `4 - r` với `r`
chạy 1→4 (`ei.java:90-96`, `bd.java:241-252`), tới `r == 4` là biến mất hẳn. Bản Unity
giữ pet nằm lại trên sân vì overlay còn hiện panel kết quả.

### 2.8 Máu thật của quái LỚN HƠN `maxHp` server gửi

`Mob.initMob()` lấy máu thật từ bảng `gopet_mob` (`hp = mobLvInfo.hp`) nhưng tính trần
bằng công thức chung `getHpViaPrice() = lvl*3 + str*4 + 20`. Hai nguồn này không khớp:
quái lv45 có `hp = 384310` trong khi `maxHp = 192155` — đúng gấp đôi.

Client vì thế **không được kẹp** hp theo maxHp mà server gửi. `ReadVitals` nới trần thành
`max(maxHp, hp)`; thiếu nó thì `BattlePetCard.Apply` cắt mất nửa máu ngay đòn đầu, thanh
máu quái về 0 khi server còn nửa pool → trận vẫn chạy mà không bao giờ có băng chiến thắng.

Hệ quả còn lại phía server (chưa đổi vì là thay đổi cân bằng): `addHpPet` kẹp theo `maxHp`
nên quái hút máu khi đang trên trần sẽ **tụt** xuống `maxHp`; độc theo % (`updateDamageToxic`)
cũng tính trên trần chứ không trên máu thật.

### 2.9 `update()` phải chốt trận trước khi thoát sớm

`PetBattle.update()` mở đầu bằng `if (hasWinner()) { if (!isClose) win(); return; }`. Trước
đây nhánh này `return` trần. Vòng lặp `GopetPlace.update` dọn trận ngay sau mỗi `update()`
(`update(); if (hasWinner()) { clean(); remove; }`) nên hầu hết đường chốt trận đã được phủ;
lỗ còn lại là trạng thái thắng xuất hiện **ngoài** `update()` giữa hai tick — `useItem` chạy
thẳng trên thread mạng từ `MenuController`, không qua hàng đợi `actions` và không giữ `mutex`.
Khi đó tick sau thoát sớm và `PET_BATTLE_STATE` không bao giờ được gửi → overlay client treo
tới khi hết watchdog. `win()` idempotent nhờ cờ `hadFinished`.

`isClose` bị loại trừ có chủ đích: `hasWinner()` trả true cho mọi trận `isClose`, mà `Close()`
(đổi map / mất kết nối) cố ý **bỏ** trận — chỉ `clean()`, không thưởng không phạt. Gọi `win()`
ở đó sẽ trừ exp / cộng ngọc / dịch chuyển người chơi giữa lúc đổi map, và chỉ xảy ra khi tick
chạm đúng snapshot cũ của `petBattles` nên không tất định. Xin thua không mất đường chốt vì
`surrender()` tự gọi `win()`.

## 3. Quy ước `battleId`

**`battleId` = userId của NGƯỜI NHẬN gói**, không phải "một id trận" toàn cục.
Đây là nguồn gốc của nhiều lỗi trước phase-01/02:

- PvP: mỗi bên nhận gói riêng với `battleId` = userId của chính họ
  (`PetBattle.sendStartFightPlayer()` `:479`).
- PvE: `battleId` = `activePlayer.user.user_id`.
- Observer: chỉ nhận luồng của bên **chủ động** (`activePlayer`).

Client (`BattleCoordinator.OnRemoved`) khớp cả `LocalPet.ActorId` và
`Opponent.ActorId` để bền với server cũ chưa gửi 2 gói.

## 4. Opcode battle

Sub-command của `PET_SERVICE` (`Server/GopetCMD.cs`):

| Sbyte | Tên | Hướng | Nội dung |
|---:|---|---|---|
| 36 | `ATTACK_MOB` | S→C, C→S | Mở trận PvE / client gửi id mob để đánh |
| 37 | `PET_BATTLE` | 2 chiều | Wrapper cho lượt: sub `ATTACK=1`, `USE_ITEM=3`, `USE_SKILL=4` |
| 38 | `PET_BATTLE_BUFF` | S→C (gate 1.5.0) | Snapshot buff/debuff cả 2 pet, xem §5 |
| 43 | `PET_BATTLE_STATS` | S→C (gate 1.5.0) | Chỉ số HUD trận và MP skill, xem §6 |
| 33 | `PET_BATTLE_EXP` | S→C (gate 1.5.0) | EXP nhỏ giọt mỗi đòn trúng PvE, xem §13 |
| 5 | `PET_BATTLE_SURRENDER` | C→S (gate 1.5.0) | Người chơi xin thua trong trận |
| 16 | `PET_BATTLE_STATE` | S→C | Kết quả trận: `battleId, winnerId, coin, exp, messages[]` |
| 45 | `PET_RECOVERY_HP` | C→S | Bật/tắt tự hồi HP ngoài trận |
| 18 | `UPDATE_PET_LVL` | S→C | Pet lên cấp giữa/sau trận |
| 59 | `PLAYER_BATTLE` | S→C | Mở trận PvP — HAI gói riêng, không broadcast chung |
| 63 | `SHOW_BIG_TEXT_EFF` | S→C | Chữ to giữa màn (ví dụ "LƯỢT 5") |
| 64 | `TIME_PLACE` | S→C | Đếm ngược place (phòng chờ vượt ải, đấu trường) |
| 99 | `FAST_REMOVE_MOB` | S→C | Huỷ trận sớm — PvP có 2 gói (sau phase-01), 1 gói cho PvE |

## 5. `PET_BATTLE_BUFF` (opcode 38) — thêm ở phase-04

Server gửi **chỉ cho** client `ApplicationVersion >= 1.5.0` sau mỗi lượt, mở trận,
và snapshot người vào place giữa chừng. Jar cũ (1.4.x) không nhận opcode này.

```
int   battleId
sbyte actorCount            (0..2)
├─ int   actorId
│  sbyte buffCount          (0..32)
│  └─ int   typeId          (ItemInfo.Type — chỉ whitelist người chơi hiểu)
│     int   value
│     sbyte turnsLeft
```

Whitelist typeId lấy từ `PetBattle.BuffTypeWhitelist` — loại nội bộ
(`DAMAGE_PHANDOAN`, `PERCENT_EXP`...) không được gửi.

**Cooldown skill KHÔNG đi qua gói này** — client tự đếm (xem §2).

## 6. `PET_BATTLE_STATS` và xin thua

Server gửi snapshot chỉ số khi mở trận và khi observer nhận lại trạng thái. Gói chỉ
được gửi cho client từ phiên bản 1.5.0:

```
int   battleId
sbyte actorCount (0..2)
int   actorId
int   level
int   atk
int   def
short critPermille
sbyte skillCount (0..16)
int   skillId
int   mpCost
```

Client dùng snapshot để cập nhật HUD và danh sách kỹ năng; gói đến sau gói mở trận
vẫn được áp dụng theo `battleId`.

`PET_BATTLE_SURRENDER` là sub-command không có payload. Server bỏ qua nếu không có
trận hoặc trận đã kết thúc; trong PvE quái thắng, trong PvP đối thủ thắng theo cùng
luồng kết thúc trận thông thường.

## 7. Kỹ năng quái

Bảng `gopet_mob_skill` ánh xạ `petId` template sang `skillID`, `skillLv` và `useRate`.
`GopetManager` nạp bảng sau bảng skill, bỏ qua dòng tham chiếu không hợp lệ. Mob chọn
kỹ năng theo tỷ lệ ở mỗi lượt khi đủ MP và không cooldown; nếu không đủ điều kiện thì
dùng đòn đánh thường. Mob không có dòng cấu hình giữ nguyên hành vi cũ.

Unity dùng `BattleSkill` từ gói mở trận/stats để hiển thị panel kỹ năng hai bên; panel
kỹ năng đối thủ chỉ đọc.

## 8. Đấu trường (`ArenaEvent`)

- Mở lúc **6/8/11/13/17/19/21/23** giờ, phút ≤5 (`Data/Event/ArenaEvent.cs:26`).
- Đăng ký 30 phút → `ArenaEvent.NextTurn()` random ghép cặp → 5 cặp/sân
  (`Place/ArenaPlace.cs`).
- Giới hạn **2 phút** mỗi cặp. HP ≤ 0 thua ngay; hết giờ → HP cao hơn thắng
  (hoà → PlayerOne được điểm).
- Người thắng: `AccumulatedPoint++`, đẩy về map 19 (ngoài đấu trường), rồi vào
  `IdPlayerJoin` cho vòng sau.
- Vào bằng building 26 (`BuildingDispatcher.cs:117`) → NPC `ARENA_MENU` (58).

## 9. Vượt ải (`ChallengePlace`)

- Vào từ NPC `OP_CHALLENGE` (10) — trừ `STAR_JOIN_CHALLENGE`, vào map 12.
- 4 người tối đa. Chờ 60s → 25 lượt, mỗi lượt 3 phút.
- Mob: `4 + (numPlayer − 1) × 4` con thường mỗi lượt; lượt chia hết cho 5 → boss
  (1 con, 2 nếu đủ 4 người).
- Kết thúc → ghi `top_challenge`.

## 10. Lối thoát overlay client

`BattleCoordinator` có **3 lối thoát** để overlay không bao giờ treo
(`Assets/Scripts/Runtime/World/BattleCoordinator.cs`):

1. `PET_BATTLE_STATE` / `FAST_REMOVE_MOB` — nhánh chuẩn.
2. `MapHandler.MapUpdated` (opcode 29) → `OnPlaceChanged()` — server luôn gọi
   `petBattle.Close(player)` trước khi chuyển place (`GopetPlace.cs:48,76`).
3. Timeout `TurnDurationMs × 3` không nhận gói lượt/kết thúc nào → đóng + toast
   "Trận đấu đã kết thúc."

### 10.1 Băng chữ kết quả, không còn popup

Hết trận hiện **ảnh chữ nằm ngang** giữa màn hình — `Battle/banner-victory`
("CHIẾN THẮNG", vàng kim) hoặc `Battle/banner-defeat` ("THUA CUỘC", xanh thép), cả hai viền
trắng. Dựng ở `BattleResultBanner`. Popup có nút "Tiếp tục" đã bị gỡ.

Vòng đời băng chữ: bật ra `0.35s` → đứng yên `2.5s` → mờ dần `2s` (kèm nhích lên 24px cho
cảm giác tan đi). `BattleView.ResultAutoCloseSeconds` lấy thẳng `BattleResultBanner.TotalSeconds`
nên không thể lệch — lệch là chữ bị cắt giữa chừng hoặc màn hình treo sau khi chữ đã tan.

Ảnh sinh bằng `tools/image-gen` (OpenAI `gpt-image`), đã cắt sát nội dung rồi đặt vào
`Assets/Resources/Battle/`. Thiếu asset thì rơi về chữ thường cùng nội dung, không để trống màn.

Phần thưởng **không nằm trong băng chữ** mà bay thành số trên đầu pet, giống jar gốc
(`e.java:57-63`): "+N Ngọc" hồng rồi "+N EXP" vàng so le 1 giây, và **chỉ hiện khi > 0** —
đúng điều kiện của bản gốc. Số EXP mỗi đòn trong trận là số trần `+12` không kèm đơn vị
(khớp mockup), chỉ phần thưởng cuối trận mới ghi đơn vị.

### 10.2 Thắng hay thua đều tự về map

Panel kết quả nán `2.5s` rồi tự gọi `Closed` (`BattleView.RequestClose`, chống gọi hai
lần bằng `_closeRequested`). Nút "Tiếp tục" chỉ để đóng sớm hơn.

Trước đây hai đằng hành xử khác nhau: khi **thắng**, `PetBattle.win()` gửi
`PET_BATTLE_STATE` rồi `sendFastRemove()` ngay sau (`:951-955`, vì quái đã chết), mà
`BattleCoordinator.OnRemoved` đóng overlay tức thì → panel loé một frame rồi biến mất.
Khi **thua** thì `mob.hp > 0` nên không có gói `FAST_REMOVE_MOB`, panel ở lại chờ bấm nút.
Nay `OnRemoved` bỏ qua nếu `_view.HasResult` — để panel tự hết giờ, hai đằng như nhau.

## 11. Thưởng / phạt khi kết trận PvE

`PetBattle.win()` nhánh `petAttackMob`:

- **Thắng** (`getWinId() == activePlayer`): cộng coin + exp, `place.mobDie(mob)`,
  quay drop item theo `GopetManager.dropItem[mapID]`. Boss chỉ thưởng cho
  `getLastHitPlayer()`.
  Quái hồi lại ở đúng vị trí sau `GopetPlace.TIME_NEW_MOB` = **3s** (gốc 25s), khớp lúc
  người chơi về map. Quái mới random theo `gopet_map_moblvl` của map, không phải đúng con cũ.
- **Thua** (hết máu HOẶC xin thua): **không phạt gì** ngoài `delayTimeHealPet`.
  **KHÔNG trừ EXP.** Trừ EXP là hành vi riêng của nhánh PK (`isPK`, 10%/5% exp cấp hiện
  tại qua `Pet.subExpPK`, sàn `MIN_PET_EXP_PK = -20_000_000`) — đừng bê sang PvE.

  Đã tra client jar gốc để xác nhận: nhánh thua `e.java:76` chỉ hỏi *"Thua rồi, bạn có
  muốn về thành phố để điều trị?"*; phần hiện thưởng `e.java:57-63` chỉ chạy khi số `> 0`;
  grep toàn bộ bảng text không có chuỗi nào về trừ exp. Không có bằng chứng bản gốc phạt
  EXP khi thua quái.

Nhánh thua chỉ đạt được khi thua thật, không cần guard thêm:
`getWinId() != activePlayer ⟺ isClose || mob.hp > 0`, mà `hasWinner()` đòi
`mob.hp <= 0 || activePet.hp <= 0 || isClose` ⇒ `mob.hp > 0` kéo theo pet đã gục.
Boss bị người khác kết liễu đi nhánh **thắng** (vì `mob.hp <= 0`) rồi thoát ở chỗ
kiểm `lastHitPlayer`. Rời map/disconnect không tới đây vì `Close()` gọi `clean()`
gỡ trận khỏi `place.petBattles` nên `update()` ngừng chạy.

## 11.2 Tự hồi HP ngoài trận

`Player.cs:377` hồi **20% HP và 20% MP mỗi 3 giây** (`TIME_PET_RECOVERY`) khi đủ các điều
kiện: `isPetRecovery`, không đang trong trận, qua `delayTimeHealPet`, qua `TimeDieZ`, pet
không chết vì PK. Thua quái đặt `delayTimeHealPet = now + 10s`
(`TIME_DELAY_HEAL_WHEN_MOB_KILL_PET`, `GopetManager.cs`) — phạt chờ rồi mới hồi.
Từ lúc chết tới lúc đầy máu khoảng **25 giây**: 10s phạt + 5 nhịp hồi 20%.

`isPetRecovery` là **tuỳ chọn của người chơi**, bật/tắt qua opcode `PET_RECOVERY_HP` (45),
server khởi tạo `false` và **không nhớ giữa các phiên** nên client phải gửi lại mỗi lần
đăng nhập (`GameSession.RestoreAutoRecoveryOnLogin`). Client mặc định **BẬT**.

> Trước 2026-09-19 `PetBattle` constructor đặt `isPetRecovery = false` mỗi khi vào trận và
> không chỗ nào bật lại — đánh một con quái là mất luôn tuỳ chọn, pet không bao giờ hồi máu
> nữa. Dòng đó còn **thừa**: vòng hồi máu đã tự kiểm `getPetBattle() == null`. Đã gỡ.

## 11.1 Chỉ số tấn công / phòng thủ của quái

Pet và quái **không dùng chung công thức**. `Pet.applyInfo()` (`Pet.cs:304-305`) nhân hệ số
lớn — `atk = str*30`, `def = agi*20` — rồi cộng trang bị, cánh, trang phục, danh hiệu.
Quái KHÔNG bao giờ chạy hàm đó (nó chỉ có ở lớp `Pet`), nên trường `atk`/`def` của quái
là 0 và chỉ còn công thức gốc `GameObject.cs:90-101`:

```
getAtk() = atk + str/3 + 5
getDef() = def + agi/3
```

Sát thương quái = `ATK quái − DEF pet`, **kẹp sàn 1** (`PetBattle.mobUseNormalAttack`). Với
bảng `gopet_mob` chỉ có str/agi/_int/hp, quái cấp 110 mới đạt ~45.700 ATK trong khi một pet
có trang bị đã vượt 11.000 DEF ngay từ cấp 3 — quái gõ mãi cũng chỉ trừ **đúng 1 máu**.

Nay `gopet_mob` có thêm cột `atk` và `def` (migration
`SRCGOPETGOC/MariaDB_SQL/migration-260919-mob-atk-def.sql`). `MobLvInfo.atk`/`def` là `int?`;
`Mob.getAtk()`/`getDef()` ưu tiên chúng, **NULL thì quay về công thức cũ** nên đổi ngược lại
được. Boss không đụng tới: `BossTemplate` không có cột `def`, `MobLvInfoImp` để null.

Số khởi điểm neo theo `hp` của chính con quái cho nhịp trận nhất quán mọi cấp:
`atk = round(hp * 0.25)`, `def = round(hp * 0.06)`. **Chỉnh cân bằng bằng `UPDATE`, không
cần sửa code.**

## 12. Đòn đánh trượt

Trước 2026-09-19 trận đấu **gần như không bao giờ trượt**: `PetBattle.randMiss()` chỉ true
khi đối phương mang buff né `MISS_IN_99999_TURN`. Trong khi đó `GameObject.IsMiss()`
(`Base/GameObject.cs:143`) đã viết sẵn từ đầu mà **không nơi nào gọi**.

Nay `PetBattle.rollMiss()` gộp cả hai:

```csharp
randMiss(nonPetBattleInfo) || (petAttackMob && ActiveObject.IsMiss(PassiveObject))
```

- Tỉ lệ cơ bản: `HitRate = AccuracyPercent − B.SkipPercent = (100 + agi/1000) − (8 + agiB/1000)`.
  Với agi < 1000 thì phép chia nguyên ra 0 ⇒ `HitRate = 92` ⇒ **8% trượt**, và co giãn theo
  chênh lệch nhanh nhẹn hai bên. `NextFloatPer()` trả 0..100.
- **Chỉ PvE** (`petAttackMob`) — PvP/PK/đấu trường giữ nguyên cân bằng cũ.
- **Kỹ năng trượt không tốn gì**: roll đặt TRƯỚC khi trừ MP và `addSkillCoolDown`. Thứ tự cũ
  trừ MP rồi mới roll, nên một đòn không xảy ra vẫn ngốn MP lẫn 3 lượt hồi chiêu.
- Kỹ năng buff/hỗ trợ (`isSkillBuff()`) **không** roll trượt.
- Gói gửi đi dùng byte-shape của **đòn thường trượt** (main NORMAL + 1 effect `SKILL_MISS`),
  không phải shape WAIT của kỹ năng. Nhờ đó jar cũ render được, và client biết kỹ năng chưa
  thực sự nổ để gọi `SkillCooldownTracker.CancelLastUsed()` gỡ cooldown lạc quan đã đánh dấu
  lúc bấm — nếu không nút xám oan 3 lượt.

Client không cần sửa gì để hiện chữ "TRƯỢT": đường `SkillId == 1` → `BattleFloatText.CreateMiss()`
+ `s_attack_miss` đã có sẵn, và `BattleEffectView.ResolveName(1)` trả null nên không vẽ sprite chém.

### 10.3 Kỹ năng: nút tròn + popup

Không còn hai bảng kỹ năng cố định hai bên. Thay bằng **nút tròn "KỸ NĂNG"** bên trái sân
(`BattleSkillButton`, sprite `Battle/btn-skill-round`); bấm mở **popup** danh sách
(`BattleSkillPopup` + `.Layout.cs` + `.Row.cs`). Mỗi dòng: icon tròn | tên | `MP: n`.

- **Kỹ năng của quái KHÔNG hiển thị.** Quái tự chọn kỹ năng theo `useRate` trong
  `gopet_mob_skill` (`PetBattle.mobAttack`), người chơi không cần biết trước. Gói
  `PET_BATTLE_STATS` vẫn mang danh sách đó nhưng client bỏ qua.
- **Icon**: `Resources/Battle/skills/<skillID>.png`, đủ 30 cái cho ID 101..130, sinh bằng
  `tools/image-gen/generate-battle-skills.py`. Thiếu thì rơi về `skills/unknown`.
- **Khung popup vẽ bằng code** (`Runtime/UI/PanelSprites.Rounded`) chứ không sinh ảnh:
  9-slice đòi viền dày đều tuyệt đối ở bốn cạnh, ảnh AI luôn lệch vài pixel và hay kèm quầng
  sáng. Chỉ ornament khó vẽ mới sinh ảnh (`btn-skill-round`, `skill-icon-ring`).
- **Cuộn khi nhiều kỹ năng**: danh sách cao tối đa `MaxListHeight` (~4.5 dòng), quá thì
  `ScrollRect` kéo được. Bản cũ chặn cứng ở 5 dòng và **giấu luôn** phần còn lại.
- Nút tròn luôn bấm được để xem; từng dòng mới khoá theo lượt/MP/cooldown. Chọn xong thì
  popup tự đóng, và popup đóng luôn khi trận kết thúc.

> **Bẫy `.meta` khi thêm sprite mới:** copy meta từ `btn-attack-big.png.meta` là SAI — file đó
> `spriteMode: 2` (Multiple) kèm rect sprite con cố định 1213×1234. Ảnh kích thước khác sẽ có
> rect vượt ngoài texture, Unity bỏ sprite, `Image` vẽ **ô trắng trơn**. Sprite đơn phải dùng
> `spriteMode: 1`, `internalIDToNameTable: []`, `sprites: []`. Đã dính một lần 2026-09-19 với
> `skill-header`, `skill-icon-ring`, `banner-victory`, `banner-defeat`.

### 12.1 Ánh xạ hiệu ứng kỹ năng

Bảng `skill` trong DB có **30 dòng, skillID 101..130**. `TurnEffect.skillId` mang hai nghĩa:
marker cho đòn thường (`SKILL_NORMAL=0`, `SKILL_MISS=1`, `SKILL_CRIT=2`) hoặc skillID thật.

`Gopet.UiLogic.BattleEffectNames.Resolve` quy đổi:

| skillId | Ra gì | Nguồn asset |
|---|---|---|
| 0, 2 | **`BattleSlashFx`** — vệt lửa dùng chung với nút đánh ngoài map | `Resources/Ui/fx-slash.png` |
| 1 | `null` — chỉ hiện chữ "TRƯỢT", không vẽ sprite | — |
| 101..124 | `Overrides[skillId]` nếu có, không thì `Atlas[skillId - 101]` | `pet/battle/skills/<tên>` + `Jar/BattleAnimations/skills/<tên>.bytes` |
| 125..130 | `<skillId>` qua `BattleActorEffectView` | `pet/battle/skills/<id>.anu.bytes` |

`BattleEffectView.Play` xét theo thứ tự: ảnh ghi đè `Battle/fx/<skillId>.png` (`BattleSkillFx`)
→ **đòn thường/chí mạng** (`BattleSlashFx`) → atlas jar theo `BattleEffectNames.Resolve`.
`Resolve(0)` vẫn trả `SlashEffect` và giữ nguyên để đối chiếu ngược với jar, nhưng đường đó
không còn được dùng: dải gốc là 4 khung vệt vàng mảnh vẽ cho màn hình 240px, phóng lên màn
hình bây giờ thành hai que chéo to đùng. Chí mạng dùng cùng ảnh, phóng 1.3×.

`Atlas` là bản chép **nguyên trạng** bảng tra của jar (`dx.java` case 0..23) — giữ y nguyên để
còn đối chiếu ngược. Muốn đổi hiệu ứng cho một kỹ năng thì thêm vào `Overrides`, đừng sửa mảng.

| Override | Vì sao |
|---|---|
| 123 "Phản Nghịch" → `fandame` | Jar cho nó dùng `ZeusWraith` (sét của thần Zeus), chẳng liên quan gì tới phản đòn. Dùng chung hiệu ứng với 110 "phản đòn" cho khớp nghĩa. |

> `Resolve` **không đơn ánh** kể từ khi có `Overrides` — hai kỹ năng được phép chung một hiệu
> ứng. Bất biến "24 tên khác nhau" giờ kiểm trên chính mảng `Atlas`, không kiểm qua `Resolve`.

> **KHÔNG cộng thêm 8 vào index.** Phép `type-101+8` trong jar (`di.java`) là index vào bảng FX
> của `bd.java`, **không phải** index vào danh sách tên. Đã sai một lần: 101..116 phát nhầm hiệu
> ứng của kỹ năng khác, 117..124 vượt mảng ⇒ `Resolve` trả null ⇒ **mất hẳn hiệu ứng**.
> Ánh xạ đúng khớp 1-1 với tên tiếng Việt trong DB: 101 "song kích"→`SongKich`,
> 107 "hạ độc"→`hadoc`, 111 "hút máu"→`hutmau`. Khoá bằng `BattleEffectNamesTests`.

Lưu ý `JarSkin.Raw` **ném** khi thiếu sprite; `BattleTurnAnimator.PlayEffect` bắt và log để một
asset thiếu không làm chết hàng đợi lượt — nên hiệu ứng thiếu sẽ âm thầm không hiện, phải xem
Console mới biết.

### 12.2 Ghi đè hiệu ứng bằng ảnh rời

Trước cả hai nhánh trên, `BattleEffectView.Play` hỏi `BattleSkillFx.TryPlay`: **có file
`Resources/Battle/fx/<skillId>.png` thì dùng nó và bỏ qua atlas jar.** Thả thêm một file là đổi
được hiệu ứng, không đụng code.

Ảnh rời không phải sprite sheet nên không có khung hình; chuyển động sinh bằng code.
`BattleSkillFx` chọn kiểu diễn theo kỹ năng — để phép chọn ở đó để hai lớp hiệu ứng không phải
biết nhau:

| Kiểu | Lớp | Kỹ năng | Nhịp |
|---|---|---|---|
| Mưa lửa | `BattleFlameFallFx` | mặc định (106 "lửa") | Hai giai đoạn: ngọn lửa **dội từ trên xuống**, chạm đất thì **bùng thành cột** cao. Mười một ngọn, lệch nhau về thời điểm, cháy một nhịp rồi lụi thấp dần. |
| Sét giáng | `BattleBoltStrikeFx` | 105 "sấm sét" (`BattleEffectNames.UsesBoltStrike`) | Một tia lớn hiện **tức thì** ở cường độ tối đa, sáu nhánh toé quanh điểm chạm, giật vài nhịp rồi tắt. **Không có vòng phép dưới chân.** |

Vòng phép vàng dưới chân (`BattleGroundSigil`) chỉ vẽ cho hiệu ứng **không phải sét**: sét giáng
thẳng từ trời nên thêm vòng triệu hồi dưới chân là sai nguồn gốc đòn đánh.

Mưa lửa dựng **hai lớp** sau/trước card pet (lớp sau dùng `SetSiblingIndex(1)` như
`BattleGroundSigil`): ảnh mẫu cho thấy pet đứng GIỮA đám lửa. Dồn hết vào một lớp thì lửa đè kín
pet, vì hiệu ứng thêm vào canvas sau các card nên mặc định nằm trên. Cột đứng trước pet còn phải
lệch khỏi tâm, nếu không nó che đúng mặt.

> Sét **không** được dâng từ từ như lửa — cho nó có quãng dựng thì thành cây gậy xanh mọc lên,
> mất hết chất. Nhánh toé dùng lại chính ảnh tia, thu nhỏ và xoay quanh điểm chạm (pivot đáy),
> nên không cần asset thứ hai. Góc nhánh trải từ gần ngang tới gần thẳng đứng; dồn hết vào góc
> chúc xuống thì chúng chụm lại trông như bộ rễ.

- Ảnh phải **cắt sát nội dung**. Pivot đặt ở đáy (0.5, 0) nên đáy sprite chính là chân ngọn lửa;
  thừa viền trong suốt là lửa cháy lơ lửng.
- `impactSeconds` trả về là lúc ngọn **cuối** chạm đích (`FallSeconds + (n-1) × Stagger` ≈ 0.72s),
  không phải ngọn đầu — trừ máu sớm thì pet gục khi lửa còn đang rơi.
- Làn rơi phải **xáo** so với thứ tự thả, nếu không lửa rớt tuần tự trái sang phải, trông như
  một cái quét chứ không phải mưa.
- Cỡ và vùng rải tính theo **kích thước pet**, không phải hằng số pixel. Ngưỡng tìm được bằng
  cách dựng khung hình mô phỏng rồi ghép lên nền thật: cột cao **2.0 lần chiều cao pet** thì pet
  vẫn nhìn rõ giữa đám lửa, **2.6 lần thì chôn mất pet** — khung hình chỉ còn một bức tường lửa.
  Vùng rải nửa cụm ±1.05 lần bề ngang pet.
- **Hướng rơi chéo lấy từ phía pet RA ĐÒN**, không chọn bừa một bên. `BattleEffectView.Play`
  vốn đã nhận `fromWorld` (điểm xuất phát tính theo vị trí kẻ tấn công) cho nhánh atlas jar;
  nhánh ghi đè nay dùng chung tham số đó. Buff lên chính mình thì `fromWorld` null ⇒ rơi thẳng.
- **Góc nghiêng thân lửa phải bằng đúng góc quỹ đạo** (`-atan(lean)`), không phải số chọn tay.
  Lệch nhau thì thân lửa không nằm trên đường bay, nhìn ra trượt ngang. Chạm đất thì dựng thẳng
  dần trong lúc bùng lên — lửa cháy thì bốc lên, không nằm nghiêng.
- **Hai giai đoạn đọc ra từ số đo, không phải đoán.** Ngọn lơ lửng trên không chỉ cao 41–83
  đơn vị canvas trong khi cột dưới đất cao 260, và ngọn càng xuống thấp càng to ⇒ đó là lửa
  đang RƠI chứ không phải tàn lửa bay lên. Bỏ mất giai đoạn rơi thì hiệu ứng sai hẳn ý.
- **Cỡ hiệu ứng neo vào CHIỀU CAO CANVAS, không neo vào pet.** Sprite pet mỗi con một cỡ
  (texture cao 36–56 px × `BattleSkin.SpriteScale` ⇒ 108–168 đơn vị canvas), buộc vào đó thì con
  pet nhỏ kéo cả hiệu ứng bé theo. Đã dính: cột lửa buộc theo pet ra thấp hơn ảnh mẫu 17% và cụm
  hẹp hơn 32%. Số đo lấy từ ảnh mẫu vốn đã là đơn vị canvas tuyệt đối (cột 260, cụm 260) — dùng
  thẳng. Chỉ độ lệch sâu trước/sau pet mới tính theo chiều cao pet.
- **Khung mô phỏng phải dùng cỡ pet THẬT.** Mock từng vẽ pet 128×96 trong khi pet thật 108×84,
  nên hiệu ứng trông cân đối ở mock mà vào game thì bé.
- Sprite dựng bằng `tools/image-gen/gen-flame.py` (vẽ vector phẳng bằng code, không dùng ảnh
  stock). Ảnh stock tải về thường là bản xem trước: **không có kênh alpha**, ô caro là pixel vẽ
  thật, kèm watermark in đè lên thân ảnh.

**Tỉ lệ:** art gốc của jar vẽ cho màn hình ~240px nên ở 1× trông tí xíu — sprite pet chỉ
19×29 pixel. Mọi pixel art trong màn đấu (sprite pet + hoạt cảnh hiệu ứng) phóng theo **một
hằng số duy nhất** `BattleSkin.SpriteScale`.

- **Phải là số NGUYÊN.** Toàn bộ texture đã là `filterMode: Point`, mipmap tắt, nên phóng
  bằng bội số nguyên giữ nét tuyệt đối (1 pixel gốc → đúng N×N pixel). Số lẻ như 2.5 làm chỗ
  2px chỗ 3px — đó mới là "vỡ hình".
- **Một hằng số cho cả hai** để chúng không lệch nhau. Đã từng lệch: pet vẽ 2× còn hiệu ứng
  1×, khiến hiệu ứng bé bằng nửa và mọi offset hoạt cảnh co lại, dồn xuống sát chân pet —
  nhìn như không có hiệu ứng.
- Hiệu ứng phóng ở **gốc**, không phóng từng mảnh: gốc neo đúng vị trí pet nên offset con
  được nhân theo, giữ nguyên bố cục. Phóng từng mảnh thì mảnh to ra mà vẫn nằm chỗ cũ.

## 13. EXP mỗi đòn trúng (PvE)

Mỗi đòn pet TRÚNG quái thường được cộng **5% EXP giết quái**, trần **30%/trận**
(`Data/Battle/HitExpReward.cs`). Server gửi `PET_BATTLE_EXP` (81/33, gate 1.5.0) —
`int battleId, int actorId, int expDelta` — client hiện số vàng bay trên đầu pet
(`BattleFloatText.CreateExp`).

**Là thưởng THÊM, không phải ứng trước** (user chốt 2026-09-19): `win()` KHÔNG trừ lại phần đã
nhỏ giọt, nên một con quái cho tối đa 130% EXP. Đừng "sửa cho nhất quán" thành ứng trước.

Hệ quả: **trần 30% là chốt chặn chống cày duy nhất** — nó chặn vòng lặp "đánh một đòn → xin thua
→ đánh lại" ăn EXP mà không cần giết quái. Hai lớp bảo vệ sẵn có hỗ trợ thêm: `GopetPlace.cs:406`
ban acc nếu giết quái < 4500ms trên map 12, và `setLastTimeKillMob` đặt ở cuối **mọi** trận PvE
nên vòng lặp vẫn bị tính.

Loại trừ:
- **Boss**: `win()` không thưởng EXP boss (`:799`) và boss là mục tiêu nhiều người → nhỏ giọt
  cho boss là rò EXP không có đối trọng.
- **PvP/PK/đấu trường**: tránh hai người hẹn nhau đứng đánh qua lại cày EXP.
- Kỹ năng thuần buff (0 sát thương) không sinh EXP.

`updatePetLvl()` gọi mỗi đòn là an toàn: nó chỉ lên 1 cấp mỗi lần gọi và chỉ gửi opcode 18 khi
thật sự lên cấp (`GameController.cs:1745-1771`).

## 14. Version gate

- Server chặn login `< VERSION_142` (1.4.2) tại `Player.cs:120-129`.
- Opcode mở rộng (PET_BATTLE_BUFF, và các opcode tương lai) gate theo
  `VERSION_150` (1.5.0). Jar cũ vào được nhưng không nhận opcode lạ.
- Unity `ClientInfo.Version = "1.5.0"` (`Assets/Scripts/Net/Auth/ClientInfo.cs`).

## 15. Khung cảnh màn đấu (mua bằng vàng)

Client chọn khung cảnh nền qua nút tròn bên phải sân đấu (`BattleSceneButton`, sprite
`Battle/btn-scene-round`). Popup `BattleScenePopup` liệt kê toàn bộ danh mục, hiện giá và nút
mua/chọn. Chọn sẽ áp được ngay kể cả giữa trận, mỗi người chơi thấy lựa chọn của riêng họ.
Mua tự động bôi đen lựa chọn. Lựa chọn được lưu DB giữa các phiên.

### 15.1 Danh mục khung cảnh

Server quản lý giá; client nhận danh sách kèm số vàng từ gói `TYPE_BATTLE_BG_STATE` (43).

| Id | Tên | Giá (vàng) | Ảnh | Hiệu ứng |
|---|---|---:|---|---|
| 0 | Rừng | 0 (free) | `Battle/bg-forest` | Không | mặc định |
| 1 | Rừng cây che | 7.000 | `Battle/bg/bg-canopy` | Bướm + chuồn chuồn |
| 2 | Hoa anh đào | 10.000 | `Battle/bg/bg-sakura` | Lá hoa rơi |
| 3 | Tuyết trắng | 12.000 | `Battle/bg/bg-snow` | Tuyết rơi (2 lớp) |
| 4 | Hang động đá | 17.000 | `Battle/bg/bg-cave` | Dơi treo + bay |
| 5 | Mưa lửa | 22.000 | `Battle/bg/bg-fire` | Lửa rơi + cánh lửa |

Thêm khung cảnh mới: nối vào CUỐI mảng trong cả server (`BattleBackgroundCatalog`) và client
(`BattleSceneCatalog`), id **phải khớp**.

### 15.2 Gói điều khiển khung cảnh

Toàn bộ là sub-command của `PET_SERVICE` (81):

| Opcode | Tên | Hướng | Nội dung |
|---:|---|---|---|
| 43 | `TYPE_BATTLE_BG_STATE` | S→C | Danh sách khung cảnh: `sbyte selectedId, sbyte n, [sbyte id, utf name, long priceGold, bool owned]×n` |
| 44 | `TYPE_BATTLE_BG_OPEN` | C→S | (trống) Xin cập nhật STATE |
| 45 | `TYPE_BATTLE_BG_BUY` | C→S | `sbyte id` Mua khung cảnh |
| 46 | `TYPE_BATTLE_BG_SELECT` | C→S | `sbyte id` Đặt làm lựa chọn hiện tại |

Server **luôn trả STATE sau BUY/SELECT** kể cả khi từ chối (nên nút không bị khoá sai), và client
**yêu cầu OPEN ngay sau login** (`GameSession`) để trận đầu tiên có khung cảnh đúng. Các server
cũ không hỗ trợ sẽ không trả gói, client rơi về rừng mặc định.

### 15.3 Database

Migration `migration-260924-battle-background.sql` thêm hai cột:
- `player.BattleBgOwned`: mediumtext JSON list (mảng id), mặc định `'[]'` (chỉ rừng id 0 không lưu)
- `player.BattleBgSelected`: int, mặc định `0` (rừng)

Cả hai được lưu qua `PlayerData.saveStatic()`.

### 15.4 Luật mua

`BattleBackgroundRules` (hàm thuần):

- Khoá theo người chơi: `Player.BattleBgLock` tránh tính tiền hai lần trong tick
- Từ chối id 0 (rừng) và id lạ ngoài danh mục
- Kiểm tra sở hữu: không bán lại
- Kiểm tra vàng: `Player.checkGold()` + `Player.mineGold()` (mineGold tính thêm
  spendGold hàng đợi)

Nếu từ chối, server gửi dialog `notEnoughGold` nhưng vẫn gửi STATE để nút không bị khoá lạc.
Id đã chọn mà không sở hữu hoặc ngoài danh mục → được coi là 0 (rừng).

### 15.5 Hiển thị nền

`BattleBackdrop` chứa `Image` nền + `BattleAmbientFx` là CON của nó.

- Ảnh: `BattleSkin.Load(BattleSceneCatalog.BackgroundPath(sceneId))`, thiếu thì rơi về rừng,
  vẫn thiếu thì tô màu nền gelid mặc định
- Hiệu ứng: `BattleAmbientFx.Create()` tái dùng hạt theo pool cố định, motion (Fall/Wander/Static)
  cấu hình bằng `BattleAmbientPresets` — đổi cơ số/tốc độ ở đó không cần code
- Raycast: tắt trên cả nền lẫn hiệu ứng nên không chặn click
- Áp dụng: `BattleBackdrop.Apply()` đổi được kể cả giữa trận, gọi lại cùng id không làm gì

### 15.6 Cách thêm khung cảnh mới

1. Thêm `BattleBackgroundDef(id, "Tên", giá)` vào `BattleBackgroundCatalog.All` (server)
2. Thêm ảnh nền `Resources/Battle/bg/` (960×640, sinh bằng `tools/image-gen/gen-battle-backgrounds.py`
   dùng `bg-forest.png` làm layout tham chiếu)
3. Thêm preset `BattleAmbientKind` enum + `BattleAmbientPresets.For()` + cấu hình hiệu ứng
4. Thêm cùng id vào `BattleSceneCatalog.Backgrounds` và `.Ambients` (client)
5. Sprite hiệu ứng: `Resources/Battle/ambient/*.png` sinh bằng `tools/image-gen/gen-ambient-sprites.py`
   (ASCII lưới pixel, ghi .meta với Point filter)
6. Nút: `Resources/Battle/btn-scene-round.png` sinh bằng `tools/image-gen/gen-scene-button.py`

### 15.7 Tests

- Server: `tests/GServer.Performance.Tests/BattleBackgroundTests.cs`
- Client: `Assets/Tests/PlayMode/BattleSceneTests.cs`

## 16. Độ bền trang bị pet

Trang bị (mũ, áo, vũ khí, giày, găng tay) mài mòn qua chiến đấu. Hỏng sẽ mất chỉ số tấn công / phòng thủ / HP / MP.

### 16.1 Luật mài mòn

Áp dụng cho `WEAPON` (1), `ARMOUR` (2), `HAT` (3), `SHOE` (104), `GLOVE` (105) — `ItemTemplate.IsEquip`.
Hằng số ở `Data/item/EquipDurability.cs`: `Max = 80`, `WearWin = 1`, `WearLose = 2`, `WarnAt = 8`.

- Hook duy nhất: `EquipWearService.Apply` ở đầu `PetBattle.win(Popup[], coin, exp)` — mọi loại trận
  (quái, boss, PK, thách đấu, đấu trường, xin thua) đều qua đây, đúng một lần nhờ `hadFinished`.
- MỌI món pet đang mặc: thắng `−1`, thua (kể cả xin thua) `−2`. PvP trừ cả hai bên; bên thua xác định bằng `getWinId()`.
- `Close()` (đổi map, rớt mạng) không gọi `win()` ⇒ **không mòn**.
- Hook bọc `try/catch`: lỗi mòn đồ không được chặn gói kết thúc trận (overlay treo).
- Trừ độ bền giữ `PlayerData.EquipRepairLock` — cùng khoá với sửa, tránh sửa xong bị ghi đè số cũ.
- Thông báo **gộp một popup mỗi trận**: "sắp hỏng" khi món vượt qua mốc `≤ 8`, "đã hỏng" khi về 0 (mỗi mốc báo một lần).
- Có món vừa hỏng ⇒ gọi lại `pet.applyInfo` (tự gửi `MY_PET_INFO`).

### 16.2 Món hỏng và hiển thị

- `Pet.applyInfo`: món hỏng vẫn ghi `ItemEquipType[...]` (bonus set/hidden stat **giữ nguyên**) rồi
  `continue` — chỉ mất atk/def/hp/mp **riêng** của món đó.
- `Item.durability` là field JSON trong `player.items`, **không có cột DB**. Item cũ không có trường này
  được Newtonsoft dựng qua `Item()` nên nhận giá trị khởi tạo `Max` (đầy). `ShouldSerializedurability()`
  chỉ ghi trường này cho trang bị pet — JSON các item khác không phình.
- Chữ: `EquipDurability.Describe` nối vào `Item.getEquipName` (tên hiện ở màn pet, túi đồ):
  " Độ bền: n/80" hoặc " (Hỏng - mang tới Thợ Rèn để sửa)". Không nối vào `getDescription` để menu
  hiện cả tên lẫn mô tả không lặp hai lần.

### 16.3 Sửa chữa (NPC -42 "Thợ Rèn", map 11)

NPC ở vị trí `x=440, y=108` — thế chỗ cây ATM đã bỏ (Thành phố Linh Thú), ảnh `npcs/Tho_Ren.png` (strip 6 frame gõ búa). Migration: `migration-260924-equip-durability-repair.sql` (tạo) + `migration-260926-blacksmith-replace-atm.sql` (dời chỗ).

**Menu MENU_REPAIR_EQUIP = 1092** (`Server/MenuController.equipRepair.cs`): liệt kê mọi trang bị pet
**chưa đầy** độ bền, thấp nhất trước.
- Chỉ mở/chọn được khi đứng ở map 11 và **không trong trận** (chặn gói tự chế sửa từ xa và đua luồng với hook mòn).
- Tiêu đề hiện số Đá mài đang có. Danh sách itemId lưu ở `objectPerformed[74]` để chỉ số dòng luôn trỏ đúng món đã hiện.
- Chọn → hộp xác nhận (ghi độ bền hiện tại) → trừ **1 viên** từ bất kỳ chồng nào → `durability = 80` → `pet.applyInfo()`.

**Lock**: `PlayerData.EquipRepairLock` tránh dùng 2 viên đá cùng lúc (tính tiền 2 lần giữa tick).

**Sau sửa**: gửi `MY_PET_INFO` để client cập nhật chỉ số + tên item (có/không còn chữ "Hỏng").

Hai option thêm: 98 "Sửa trang bị", 99 "Độ bền là gì?" (hỏi thêm hint).

### 16.4 Đá mài sửa chữa (item 1000091)

Tên: "Đá mài sửa chữa", type 29 (`GopetManager.ITEM_REPAIR_STONE`), stackable, tradable.

**Ảnh**: `SRCGOPETGOC/GServer/assets/items/1000091.png` (sinh bằng `tools/image-gen/gen-repair-stone-icon.py`).

**Dùng từ túi**: chỉ hiện hint "Đổi trực tiếp ở Thợ Rèn", không tiêu.

**Nguồn**:
- **Mob thường** (5% xác suất mỗi trận, roll độc lập — không trong `drop_item`): `REPAIR_STONE_DROP_PERCENT`. Đá từ quái **khoá giao dịch** (chặn bot cày đá đem bán); khi sửa ưu tiên tiêu đá khoá trước
- **Boss** (kết liễu cuối): `+5` viên (`REPAIR_STONE_BOSS_COUNT`)
- **Hàng ngày** (danh sách quà 28 ngày): ngày 3, 10, 17, 24 `+1`; ngày 28 `+2` (tradable flag = 1)

### 16.5 Cân bằng

Nếu mob respawn 3s, ~150–200 trận/giờ → mỗi trang bị hỏng sau ~25–30 phút cày liên tục.
Mỗi trận thắng tốn 5/80 = 1/16 viên (5 món × −1); rơi 5% = 1/20 viên ⇒ cày liên tục **hụt ~20%**
(~9–12 viên cần/giờ, ~7–10 viên rơi/giờ), bù bằng boss/điểm danh (đá giao dịch được) — đá là chỗ tiêu hao thật.

**Điều chỉnh**: sửa hằng số trong `EquipDurability.cs` và `GopetManager.cs`, không cần schema thay đổi.

### 16.6 Database

Migration `SRCGOPETGOC/MariaDB_SQL/migration-260924-equip-durability-repair.sql` (runner `docker/migrate-db.sh` chạy đúng một lần):
- Thêm item 1000091 "Đá mài sửa chữa" — **không** `ON DUPLICATE KEY`: prod đã có id này thì lỗi rõ ràng thay vì âm thầm ghi đè món khác.
- Thêm NPC -42 "THO REN".
- **Nối** `-42` vào cuối `map.npc` của map 11 (không ghi đè danh sách, prod có thể đã thêm/bớt NPC).

### 16.7 Tests

Server: `tests/GServer.Performance.Tests/EquipDurabilityTests.cs`
