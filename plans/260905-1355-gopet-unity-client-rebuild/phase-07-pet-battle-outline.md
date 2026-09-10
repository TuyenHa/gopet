---
phase: 7
title: "Pet Battle"
status: in-progress
priority: P2
effort: "4-6w"
dependencies: [6]
---

# Phase 7: Pet Battle

## Overview

Hệ thống chiến đấu pet: render trận đấu, gửi hành động, hiệu ứng skill.

> Tài liệu khởi đầu là outline. Phần cuối ghi lại giao thức đã reverse, phạm vi đã triển
> khai và kết quả kiểm tra thực tế ngày 2026-09-08.

## Bối cảnh

Toàn bộ logic chiến đấu nằm ở server (`Data/Battle/PetBattle.cs`, 1.705 dòng). Client chỉ:
1. Render trạng thái trận đấu server gửi
2. Gửi hành động người chơi chọn (đánh thường / dùng skill / dùng item / chạy)
3. Phát animation và hiệu ứng

Client **không tính damage**. Điều này làm phase dễ hơn đáng kể so với vẻ ngoài.

## Opcode liên quan

| Opcode | Tên |
|---|---|
| 37 | `PET_BATTLE` |
| 16 | `PET_BATTLE_STATE` |
| 1 | `PetBattle_ATTACK` |
| 3 | `PET_BATTLE_USE_ITEM` |
| 4 | `PET_BATTLE_USE_SKILL` |
| 36 | `ATTACK_MOB` |
| 45 | `PET_RECOVERY_HP` |
| 18 | `UPDATE_PET_LVL` |

Đều là sub-command của `PET_SERVICE` (81).

## Hướng tiếp cận

1. Đọc `Data/Battle/PetBattle.cs` để rút ra máy trạng thái trận đấu
2. Đọc `Data/Battle/{BattleAction,Buff,Debuff,TurnEffect,PetDamgeInfo}.cs` để biết field
3. Dựng scene battle: 2 pet, thanh HP/MP, menu hành động
4. Animation skill — asset ở `pet/battle/skills/` (30+ skill: Meteor, Tornado, ThorHammer, SamSet...)
5. Buff/debuff hiện icon trạng thái
6. Text nổi damage (`Data/Battle/PetBattleText.cs`)

## Việc cần làm rõ khi plan chi tiết

- Định dạng chính xác của `PET_BATTLE_STATE` — nhiều field, cần dump thật để đối chiếu
- Format animation skill: file `.anu` (6 file trong jar) — chưa khảo sát
- PvP có khác PvE về giao thức không (`PLAYER_PK`, `INVITE_MATCH`, `WaitUserPK`)
- Battle theo lượt hay realtime — ảnh hưởng lớn tới thiết kế scene

## Rủi ro đã nhận diện

| Rủi ro | Ghi chú |
|---|---|
| `PetBattle.cs` 1.705 dòng, nhiều state | Cần dump gói tin thật của 1 trận đầy đủ làm chuẩn |
| Format `.anu` chưa biết | Có thể phải reverse thêm; dự phòng: dựng animation từ sprite sheet PNG |
| Hiệu ứng skill nhiều và đa dạng | Làm 3-5 skill trước để chốt kiến trúc, còn lại là lặp |

## Next Steps

Chạy một trận PvE và một trận PvP với server thật để nghiệm thu hình ảnh, nhịp animation
và các biến thể dữ liệu chưa xuất hiện trong fixture. Phần triển khai và kiểm tra tự động đã xong.
Unity 6000.5 batch runner đã được thử cả trong và ngoài sandbox nhưng crash ở URP
`RenderTexture.Create`, trước khi sinh XML kết quả; đây là lỗi native của runner, không phải
một test case báo fail.

## Báo cáo triển khai (2026-09-08)

### Giao thức đã đối chiếu

- Đã đọc cả server `Data/Battle/PetBattle.cs` và bytecode/decompile của JAR (`dj`, `di`,
  `dx`, `ei`, `fr`) thay vì suy đoán từ tên opcode.
- Battle là **theo lượt, server-authoritative**. Unity chỉ gửi lựa chọn và áp delta HP/MP
  từ server; không tính damage ở client.
- Đã hỗ trợ start PvE (`36`), start PvP (`59`), turn (`37`), result (`16`) và update
  level (`18`) dưới `PET_SERVICE` (`81`). Parser PvP chấp nhận cả snapshot observer cũ
  thiếu boolean cuối và packet participant đầy đủ.
- Ba hành động hiện trên JAR và server hiện tại hỗ trợ là đánh thường (`1`), vật phẩm
  (`3`) và skill (`4`). Nhánh chạy (`2`) còn trong JAR cũ nhưng server hiện tại không xử
  lý, vì vậy không dựng nút giả không có tác dụng.

### Unity đã triển khai

- `BattleHandler` và model thuần C# cho toàn bộ vòng đời trận, packet gửi/nhận có test
  round-trip theo đúng thứ tự byte.
- Click mob trên map gửi `ATTACK_MOB`; lúc vào trận khóa di chuyển, ẩn HUD, giữ map làm
  nền; lúc bấm Tiếp tục khôi phục map/HUD.
- Sân đấu có hai pet động, tên/cấp, HP/MP, bộ đếm lượt, đúng ba nút Đánh/Kỹ năng/Vật
  phẩm, danh sách skill theo MP và bảng thắng/thua/phần thưởng.
- Delta damage/heal/mana, miss và hiệu ứng skill đều lấy từ packet server.
- Pipeline asset đã đưa 27 metadata animation `dy` và 6 animation actor `.anu` từ JAR
  vào Resources. Hai parser kiểm tra toàn bộ asset gốc; renderer phát cả hai định dạng.
- Menu vật phẩm do server gửi qua `UiRoot` được ép nằm trên canvas battle để không bị
  che và vẫn nhận click.

### Kiểm tra

- 479 unit test thuần C# đạt, gồm packet PvE/PvP/turn/result/action và toàn bộ 33 file
  animation battle gốc.
- Net, Runtime, Editor, PlayMode-test assembly và LiveSmoke đều compile thành công qua
  `verify.ps1`; kiểm tra asset/opcode/asmdef/giới hạn 200 dòng đều đạt.
- Có PlayMode test cho ba nút chính, packet đánh thường, áp turn/result, chống result
  trùng và dựng hiệu ứng `dy` + `.anu`.

### Nghiệm thu còn lại

- [x] **Đánh một mob thật — xác nhận wire format bằng LiveSmoke với GServer thật** (2026-09-10, xem `tests/Gopet.Net.LiveSmoke/BattleChecks.cs`). Luồng đầy đủ, tự động, lặp lại được (idempotent — chạy 2 lần liên tiếp đều xanh):
  1. Nói chuyện NPC `-1` (TRAN CHAN, map 11) → nhận danh sách "Nhận pet miễn phí"
  2. Chọn pet miễn phí ("rùa baby") → server `addPet`
  3. Mở túi pet (`PET_INVENTORY`) → chọn pet → server set `petSelected`
  4. Warp sang map 13 (Linh Lâm — xác nhận có 12 quái qua `gopet_mob_location`, map 11 hub không có quái)
  5. Nhận `SEND_LIST_MOB_ZONE` → bắt "tiểu miêu" (mobId thật, số âm — `int`, không phải index)
  6. `ATTACK_MOB` → server trả `PET_SERVICE/ATTACK_MOB` — `BattleHandler.OnMobBattle` parse **LocalPet + Opponent đầy đủ** (skill, stat, HP/MP) từ **byte thật**, không phải fixture
  7. `SendNormalAttack` → nhận `PET_BATTLE` turn, effects parse sạch — `ExpectFullyConsumed` qua được nghĩa là field-by-field khớp tuyệt đối
  8. Ghi nhận (không phải bug): server `PetBattle.writeMobInfo` **hard-code level=1** trong gói battle bất kể level thật của mob (`SEND_LIST_MOB_ZONE` báo lv6, battle packet báo lv1) — hành vi server có sẵn, client parse đúng những gì server gửi
  - Sprite/effect rendering trên thiết bị đích **chưa xác nhận** — cần Unity Editor/device, ngoài khả năng của phiên làm việc này (headless)
- [x] **Chạy một trận PvP thật — xác nhận cả góc nhìn participant lẫn observer** (2026-09-10, xem `tests/Gopet.Net.LiveSmoke/PvpBattleChecks.cs` + `SmokeLogin.cs`). 2 tài khoản riêng (`gopetpvp1`/`gopetpvp2`) — không tái dùng account PvE ở trên vì nó còn giữ trận đang dở (`getPetBattle() != null` chặn `PLAYER_CHALLENGE`).
  - **Tạo 2 tài khoản test**: `REGISTER` (opcode 35) bị khoá cứng phía server (`Player.cs`, xác nhận qua check M) — không có đường nào khác trong giao thức để có tài khoản mới. Seed thẳng vào `gopettae_gopet_web.user` (DB local, chỉ bind loopback, dữ liệu test) bằng bcrypt cost 12 khớp `GopetHashHelper.ComputeHash` (script tạo hash: `BCrypt.Net.BCrypt.HashPassword("abc12345", 12)`, cùng thư viện server dùng). Tài khoản mới mặc định `coin=50000` (cột DB default) — đủ trả mọi mức cược PvP (2.000-15.000) không cần seed thêm.
  - **Bug thật tìm được lúc build check**: `guider.MenuShown` không nhận được `MENU_INTIVE_CHALLENGE` — server trả màn hình 3 mức cược qua **`GUIDER_LIST_OPTION`** (sub 3), không phải `SHOW_MENU_ITEM`, giống màn ATM đổi vàng/ngọc (check Q). Sửa bằng cách nghe `guider.ListOptionShown` thay vì `MenuShown` — đây là lỗi trong CHECK MỚI VIẾT (không phải bug production code), sửa xong xanh ngay.
  - **Luồng xác nhận xanh**: A `PLAYER_CHALLENGE`(B) → nhận list mức cược `2.000 | 10.000 | 15.000 (ngọc)` → chọn 2.000 → B nhận đúng dialog `"Người chơi pvp... muốn thách đấu bạn với mức cược 2.000 (ngọc). Bạn có đồng ý lời mời này không?"` → B trả lời Có → **CẢ HAI đầu nhận `PLAYER_BATTLE` — hai gói RIÊNG BIỆT** (không phải broadcast chung): A thấy `LocalStarts=False`, B thấy `LocalStarts=True`, cả hai `IsParticipant=true`, `LocalPet`/`Opponent` parse sạch từ byte thật (`writePetPassiveInfo`, khác `writeMobInfo` — level PvP là level thật, không hard-code)
  - Lặp lại được (idempotent) — chạy 2 lần liên tiếp đều xanh (42/42 check, gồm cả PvE)
- [ ] Chạy PlayMode suite trong Editor Test Runner (batchmode hiện crash native ở URP) — cần Unity đóng để chạy `run-playmode-tests.ps1`, đang chờ user đóng Editor
