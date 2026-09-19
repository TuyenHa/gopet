---
phase: 5
title: "Client UI: dựng lại BattleView theo mockup"
status: pending
priority: P1
effort: "1d"
dependencies: [3, 4]
---

# Phase 5: Client UI: dựng lại BattleView theo mockup

## Overview
Thay overlay mờ hiện tại bằng màn hình toàn phần giống `visuals/battle-screen-mockup.png`,
cho cả PvE và PvP. Giữ nguyên vòng đời trong `BattleCoordinator` (3 lối thoát).

## Layout (tham chiếu 960×540, `CanvasScaler` hiện có)

```
┌───────────────────────────────────────────────────────────────────────┐
│[< Quay lại]        ⚔ ĐÁNH QUÁI / ĐẤU TRƯỜNG PET    [★99.4k][♦50.2k][L 0]│ top bar
├──────────────────────────┐      VS      ┌──────────────────────────────┤
│[ảnh] tên   Lv.N          │              │          tên  Lv.N   [ảnh]  │ HUD xanh / đỏ
│      HP ▓▓▓▓▓ 29580/29580│              │HP ▓▓▓▓▓ 775/823              │
│      MP ▓▓▓▓▓ 30081/30081│              │MP ▓▓▓▓▓ 1023/1023            │
│      ⚔127  🛡98  ★23%    │              │⚔65  🛡45  ★12%               │
├────────┐                                                   ┌──────────┤
│Kỹ năng │     pet trái (sprite)  ~~hiệu ứng~~  pet phải      │ Kỹ năng  │
│[ic]Tên │        +12                     -95                  │[ic]Tên   │ panel skill
│   MP10 │                                                    │   MP10   │ (phải: chỉ xem)
│ ...    │                                                    │ ...      │
├────────┘                                                   └──────────┤
│[Xin thua]                                        (●thuốc) (⚔ nút lớn) │
└───────────────────────────────────────────────────────────────────────┘
```
Bộ đếm lượt: đặt dưới chữ VS (mockup không có nhưng bắt buộc về gameplay).

## Hành vi
- Nút kiếm lớn = đánh thường (`SendNormalAttack`). Bấm 1 dòng skill bên trái = dùng skill
  (thay cho menu "Kỹ năng" cũ). Dòng skill: mờ khi thiếu MP / cooldown, badge số lượt
  cooldown (tái dùng `SkillCooldownTracker`).
- Panel phải: skill của đối thủ, **chỉ hiển thị** (không bấm). PvE: skill quái từ phase 1;
  dòng đầu luôn là "Tấn công" (icon `skills/attack`). MP lấy từ `PET_BATTLE_STATS`;
  chưa có stats (server cũ) → ẩn dòng MP.
- Nút thuốc nhỏ = `SendUseItem` (giữ tính năng).
- "Xin thua" → `YesNoDialog` xác nhận → `SendSurrender` → khoá nút. Ẩn khi observer.
- "Quay lại": đang trong trận = giống Xin thua (cùng dialog); sau khi có kết quả = đóng.
- Thanh HP/MP: fill theo tỉ lệ, text `hp/max` giữa thanh, HP ≤25% nhấp nháy nhẹ.
- ATK/DEF/crit: từ `StatsReceived`; chưa có → `--`. Crit hiển thị `permille/10` + `%`.
- Level: ưu tiên stats, fallback `BattlePet.Level`.
- Float text `+12` (xanh lá/vàng) / `-95` (đỏ) và hiệu ứng: tái dùng `BattleFloatText`,
  `BattleEffectView`, `BattleActorEffectView`.
- Buff/debuff: giữ `OnBuff`, hiển thị dưới HUD tương ứng (icon nhỏ nếu có, không thì nhãn chữ).
- Tiền tệ: đọc `PlayerStats` hiện có, format `99.4k` / `1.2M` (helper thuần trong `UiLogic`).
- Màn hình che hẳn map (nền rừng opaque); `setBattleMode(true)` vẫn ẩn HUD world như cũ.
- Kết quả trận: giữ `ShowResult`, đổi skin sang `panel-dark`.

## Architecture — tách file (mỗi file < 200 dòng)

| File | Vai trò |
|---|---|
| `Runtime/World/BattleView.cs` | Điều phối: tạo canvas, lắp các phần, nhận Apply/ShowResult/OnBuff/OnStats |
| `Runtime/World/Battle/BattleTopBar.cs` | Quay lại, tiêu đề, 3 ô tiền tệ |
| `Runtime/World/Battle/BattleHudPanel.cs` | Khung xanh/đỏ: avatar, tên, Lv, 2 thanh, 3 chỉ số, buff |
| `Runtime/World/Battle/BattleStatBar.cs` | Thanh HP/MP có text |
| `Runtime/World/Battle/BattleSkillPanel.cs` | Panel "Kỹ năng" (interactive / read-only) |
| `Runtime/World/Battle/BattleActionBar.cs` | Nút kiếm lớn, nút thuốc, Xin thua, khoá/mở |
| `Runtime/World/BattlePetCard.cs` | Chỉ còn sprite pet + anchor hiệu ứng (bỏ panel chữ) |
| `Runtime/UI/BattleSkin.cs` | Load sprite `Resources/Battle/*`, fallback khi thiếu |
| `UiLogic/CompactNumberFormat.cs` | `99400 → 99.4k` (test được) |
| `UiLogic/BattleSkillIconKey.cs` | `skillId → "Battle/skills/{id}"`, fallback `unknown` |

Kiểm tra `UiBuilder`/`HudSkin`/`JarSkin` trước khi viết helper mới (DRY).
Namespace thư mục mới: theo quy ước `.asmdef` hiện tại (kiểm tra asmdef ở `Runtime/`).

## Related Code Files
- Modify: `Runtime/World/BattleView.cs`, `Runtime/World/BattlePetCard.cs`,
  `Runtime/World/BattleCoordinator.cs` (truyền `PlayerStats` + `BattleKind`),
  `Runtime/World/GameSession.cs` (nơi tạo coordinator)
- Create: các file trong bảng trên

## Implementation Steps
1. `BattleSkin` + 2 helper UiLogic.
2. `BattleStatBar`, `BattleHudPanel`, `BattleSkillPanel`, `BattleActionBar`, `BattleTopBar`.
3. Rút gọn `BattlePetCard`; đặt vị trí 2 pet theo mockup (≈ x 0.31 / 0.66, y 0.45), pet
   phải lật ngang như hiện tại.
4. Viết lại `BattleView` lắp ghép; giữ API public (`BattleId`, `IsParticipant`,
   `OpponentActorId`, `TurnDurationMs`, `Closed`, `Ticked`, `Apply`, `ShowResult`) để
   coordinator và test PlayMode hiện có không vỡ.
5. Nối `StatsReceived`, `SendSurrender`, dialog xác nhận.
6. Compile (`verify.ps1`), chạy Unity, đánh quái thật trên server local: chụp màn hình
   so với mockup ở 16:9 và 4:3 (không tràn/đè).

## Success Criteria
- [ ] Bấm quái → màn hình khớp mockup về bố cục, màu khung, thứ tự thành phần.
- [ ] Skill 2 bên lấy từ server (không hardcode); icon theo `skillID`.
- [ ] HP/MP/ATK/DEF/crit/level đúng dữ liệu server; cập nhật mỗi lượt.
- [ ] Đánh thường, skill, thuốc, xin thua hoạt động; kết thúc trận quay về map.
- [ ] PvP/đấu trường dùng cùng màn hình, tiêu đề đúng.
- [ ] Không file nào > 200 dòng; test PlayMode battle cũ vẫn xanh.

## Risk Assessment
- Tên skill dài ("giảm sát thương 3") tràn dòng → `resizeTextForBestFit` + panel rộng cố định.
- Pet nhiều skill (>4) → panel cuộn hoặc tối đa 5 dòng + co chiều cao.
- Màn hình hẹp: HUD 2 bên chồng VS → anchor theo tỉ lệ, kiểm tra 4:3.
