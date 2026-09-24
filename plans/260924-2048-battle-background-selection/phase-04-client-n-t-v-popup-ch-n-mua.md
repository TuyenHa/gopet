---
phase: 4
title: Client nút và popup chọn mua
status: completed
priority: P1
effort: 5h
dependencies:
  - 1
  - 3
---

# Phase 4: Client nút và popup chọn mua

## Overview
Thêm gói tin client cho 4 sub-command, trạng thái khung cảnh dùng chung cả phiên, nút tròn bên
phải sân đấu và popup chọn/mua.

## Key Insights
- Mẫu gói tin: điểm danh — `Net/Guider/GuiderPackets.cs:137,144`, `GuiderHandler.cs:66`
  (`router.RegisterSub(COMMAND_GUIDER, TYPE_..., ...)`), model `DailyCheckinState.cs`.
- Nút kỹ năng tròn bên trái: `BattleSkillButton`; popup `BattleSkillPopup` (+ `.Layout`, `.Row`), khung vẽ bằng `PanelSprites.Rounded`. Nút khung cảnh đối xứng bên phải, cùng độ cao.
- Xác nhận giá: `YesNoDialog.Create(parent, msg, "Mua", "Huỷ")`.
- Vàng hiện ở `BattleTopBar._levelLabel` (`L{Gold}`), tự cập nhật qua `MONEY_INFO`.

## Architecture
- `Net/Guider/BattleSceneState.cs`: `SelectedId`, `List<Entry{Id, Name, PriceGold, Owned}>`.
- `GuiderPackets`: `RequestBattleScenes()`, `BuyBattleScene(id)`, `SelectBattleScene(id)`; `GuiderHandler`: event `BattleSceneStateReceived`.
- `GameSession`: giữ `BattleSceneState` mới nhất. Gửi `OPEN` sau khi vào game (cạnh `RestoreAutoRecoveryOnLogin`) để biết khung cảnh đã chọn trước trận đầu. Khi nhận STATE mà đang có `BattleView` → `ApplyScene(selectedId)`.
- `BattleSceneButton` (bên phải, trên nút Thuốc, dưới HUD đối thủ) → mở `BattleScenePopup`.
- `BattleScenePopup` (+ `.Row`): mỗi dòng = thumbnail nền | tên | nhãn trạng thái | nút:
  - Đang dùng → nhãn "Đang dùng", không nút.
  - Đã sở hữu → nút "Chọn" → `SelectBattleScene`.
  - Chưa sở hữu → nút "{giá} vàng" → `YesNoDialog` "Mua khung cảnh X với N vàng?" → `BuyBattleScene`. Không đủ vàng thì làm mờ nút, bấm hiện toast "Không đủ vàng".
  - Nút khoá chờ tới khi nhận STATE mới (chống bấm hai lần).
- Popup là overlay trong `BattleView`; không khoá nút đánh theo lượt (đổi cảnh không ảnh hưởng trận).

## Related Code Files
- Create: `Assets/Scripts/Net/Guider/BattleSceneState.cs`
- Create: `Assets/Scripts/Runtime/World/Battle/BattleSceneButton.cs`, `BattleScenePopup.cs`, `BattleScenePopup.Row.cs`
- Modify: `Assets/Scripts/Net/GopetCmd.cs`, `Net/Guider/GuiderPackets.cs`, `Net/Guider/GuiderHandler.cs`
- Modify: `Runtime/World/BattleView.cs` (tạo nút + popup, đóng popup khi có kết quả), `GameSession` (partial mới `GameSession.BattleScene.cs`)

## Implementation Steps
1. Hằng 43–46 + parse STATE (`ExpectFullyConsumed`) + 3 hàm gửi.
2. `GameSession.BattleScene.cs`: cache state, gửi OPEN sau login, đẩy state vào BattleView đang mở.
3. Nút + popup theo phong cách `BattleSkillPopup`.
4. Nối vào `BattleView.Build` cho mọi `BattleKind` (cả PvP/đấu trường, kể cả khi chỉ đang xem trận); đóng popup khi trận kết thúc.
5. Compile Unity, chạy với server phase 1: mua, chọn, relog.

## Success Criteria
- [ ] Nút hiện bên phải màn Đánh Quái và màn PvP, không đè HUD/nút Thuốc/nút Tấn công.
- [ ] Popup đủ 6 dòng với giá lấy từ server.
- [ ] Mua: xác nhận → vàng trên top bar giảm → nền đổi ngay trong trận.
- [ ] Chọn cảnh đã có → đổi ngay; trận sau vẫn giữ; relog vẫn giữ.
- [ ] Chưa nhận STATE (server cũ) → nền rừng, nút vẫn mở popup trống/"Đang tải".

## Risk Assessment
- Màn hình hẹp: popup phải co theo chiều cao, cuộn nếu thiếu chỗ (giống `BattleSkillPopup.MaxListHeight`).
- Server cũ không hiểu sub 44 → không trả gì: client phải chịu được không có STATE.
