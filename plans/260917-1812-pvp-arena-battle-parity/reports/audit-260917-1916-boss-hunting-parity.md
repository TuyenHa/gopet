# Audit — Đánh boss: server vs Unity

**Kết luận: Unity ĐÃ CÓ đủ luồng đánh boss thường ngày. Có 1 lỗi nhỏ ở server
(không liên quan phase này) và 3 tính năng phụ chưa render.**

## Luồng chính — parity đầy đủ

Đối chiếu `SRCGOPETGOC/GServer` với `GopetUnityClient`.

| # | Bước | Server | Opcode | Unity |
|---|---|---|---|---|
| 1 | Boss spawn theo giờ | `DailyBossEvent.Update()` `HourDailyBoss` + `HourSummon` | — | — |
| 2 | Banner "Boss X đã xuất hiện tại Y khu Z" | `PlayerManager.showBossBanner()` phát cả legacy `BANNER_MESSAGE`(1) và `BOSS_BANNER_MESSAGE`(6) | `SERVER_MESSAGE/6` | ✅ `GuiderHandler.BossBannerShown` → `GameHud.Ticker.Show` |
| 3 | Boss xuất hiện trong world (cờ) | `GopetPlace.sendMob()` ghi `putbool(mob is Boss)` | mob spawn | ✅ `WorldObjectHandler.OnMobs` parse `IsBoss` → `WorldActorView.Factory` render `* Tên Lv.N` |
| 4 | Player tap Boss → gửi đánh | client gửi `ATTACK_MOB`(36) + mobId | `PET_SERVICE/36` | ✅ `BattleHandler.SendAttackMob` (wire qua `MapScene.SubscribeWorld`) |
| 5 | Server trừ 1 sao, mở trận | `GopetPlace.attackMob` → `star--`, task `onAttackBoss`, `PetBattle(mob, place, player)` | `PET_SERVICE/36` gói mở trận | ✅ `BattleHandler.OnMobBattle` → `BattleCoordinator` |
| 6 | Đánh lượt | `PetBattle.petAttack/useSkill` với `mob is Boss` nhánh riêng (`Mob.Mutex.WaitOne()`, cập nhật `Damage[player]`) | `PET_BATTLE`(37) | ✅ `BattleHandler.OnTurn` → `BattleView.Apply` |
| 7 | HP boss cập nhật cho cả zone (Boss có nhiều người đánh) | `PetBattle.petAttack` `if (mob is Boss) place.UpdateHpMob(mobId, hp)` → `UPDATE_HP_BOSS`(89) | `PET_SERVICE/89` | ✅ `WorldStatusHandler.BossHpUpdated` → `WorldActorLayer.ApplyBossHp` → `WorldActorView.SetBossHp` (hiển thị "Tên  HP N") |
| 8 | Kích sát (last hit) | `Boss.SetWinnerIfHpZero(player)` gán `lastHit`, chỉ `getLastHitPlayer()` được nhận quà | server-side | — |
| 9 | Nhận quà kích sát | `activePlayer.controller.onReiceiveGift(boss.Template.gift)` → gói messages trong `PET_BATTLE_STATE`(16) + `okDialog("Chúc mừng bạn kích sát ...")` gọi `POPUP_MESSAGE` | `PET_SERVICE/16` + `SERVER_MESSAGE` | ✅ `BattleView.ShowResult` hiện danh sách quà; `GuiderHandler.PopupShown` hiện okDialog |
| 10 | Xoá boss khỏi world | `place.mobDie` + `place.RemoveBattleByMobId` → `REMOVE_BATTLE_BY_MOB_ID`(96) | `PET_SERVICE/96` | ✅ `WorldObjectHandler.MobRemoved` → `WorldActorLayer.OnMobRemoved` |
| 11 | Task counter `onKillBoss` | `TaskCalculator.onKillBoss` → cập nhật nhiều task | server-side, không gói riêng | — |

**Tất cả 11 bước ĐÃ chạy được ở Unity** — không opcode nào rơi vào `OnUnhandled`,
không path nào bị bỏ trắng.

## Đường vào riêng: `ChallengePlace` (Vượt ải)

Boss chạy trong vượt ải là **cùng class `Boss`** nhưng khác điều kiện:

- Không trừ sao (đã trừ khi vào map).
- Không kiểm `OwnerClan`.
- Có `boss.TimeOut = DateTime.Now.AddMilliseconds(TIME_ATTACK)` (`Place/ChallengePlace.cs:174`) — hết giờ tự bị dọn bởi `GopetPlace.update()` (`:535`).

Cùng bộ opcode ở trên nên Unity **đã chạy được** — chỉ chưa được kiểm chứng
end-to-end (phase 06 của plan gốc).

## Boss sự kiện

Đều dùng cùng luồng:
- `TeacherDay2024`, `GameBirthdayEvent` — spawn `Boss` với template khác, có
  `TYPE_BIRTHDAY_EVENT` mỗi lần trúng chỉ trừ 1 HP.
- `ID_BOSS_CHALLENGE[]` — boss lượt 5/10/15/20/25 trong vượt ải.

Cùng wire format. Không có opcode riêng cho boss sự kiện.

## Gap — thứ Unity CHƯA render

| Tính năng | Server đã có? | Wire format? | Unity |
|---|---|---|---|
| **Boss ranking damage** (`Boss.GetRank()`) | ✅ trong RAM | ❌ **không có packet nào** | ❌ Cả jar cũng không thấy được → không phải Unity thiếu |
| **Thanh HP dài cho boss** | HP đưa qua opcode 89 | text-only | ⚠️ Unity chỉ hiện `"Tên  HP N"` dưới nhãn NPC, không có thanh HP dài. Jar cũng vậy — parity đúng, nhưng UX kém so với game hiện đại |
| **Chỉ báo "boss của bang mình"** | `boss.OwnerClan` check server-side, deny bằng `Popup` | Popup text | ✅ Popup hiển thị đúng, nhưng người chơi không thấy trước khi tap. Jar cũng vậy |
| **Đếm ngược boss timeout** | `boss.TimeOut` (10 phút) | ❌ không gửi | ❌ Cả 2 client đều mù về thời gian còn lại của boss |

Ba mục cuối là "gap cùng cấp với jar" — nếu muốn thêm phải mở rộng protocol, KHÔNG
phải parity thiếu.

## 🐛 Phát hiện bên lề — 1 bug nhỏ ở server

`GopetPlace.cs:438` trừ sao trực tiếp:
```csharp
if (player.playerData.star - 1 >= 0) {
    player.playerData.star--;
    player.controller.getTaskCalculator().onAttackBoss((Boss)mob);
}
```

Nhưng **không gọi** `updateUserInfo()` để đẩy `STAR_INFO`(94) mới xuống client.
Hệ quả: sau khi đánh boss, thanh sao trên HUD hiển thị số cũ cho tới lần update
kế tiếp (login/warp/level up...). Các chỗ khác trừ sao dùng `player.MineStar(n)`
tự gọi `updateUserInfo` (xem `Server/Player.cs:870-873`).

**Fix một dòng**: đổi `player.playerData.star--` thành `player.MineStar(1)`.
Không nằm trong phạm vi plan này — ghi nhận vào backlog.

## Kết luận

- **Đánh boss thường ngày, boss vượt ải, boss sự kiện — Unity ĐỦ.** Wire format
  khớp jar 100% cho boss, không thiếu opcode nào cần thiết.
- **Bug hiển thị sao**: 1 dòng ở server. Không phải Unity thiếu tính năng.
- **UX boss (thanh HP dài, đếm ngược, bảng damage)**: chưa có ở cả jar và Unity.
  Nếu muốn thêm phải mở rộng protocol (nên vào plan mới nếu cần).

## Câu hỏi mở

- Có cần plan riêng bổ sung thanh HP dài + đếm ngược boss + bảng damage top-N
  (mở rộng protocol qua `VERSION_150`)?
- Bug `MineStar(1)` sửa ngay hay gom vào batch fix sau?
