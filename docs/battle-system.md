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

## 11. Version gate

- Server chặn login `< VERSION_142` (1.4.2) tại `Player.cs:120-129`.
- Opcode mở rộng (PET_BATTLE_BUFF, và các opcode tương lai) gate theo
  `VERSION_150` (1.5.0). Jar cũ vào được nhưng không nhận opcode lạ.
- Unity `ClientInfo.Version = "1.5.0"` (`Assets/Scripts/Net/Auth/ClientInfo.cs`).
