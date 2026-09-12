# Phase 01 — Ẩn SoundToggleButton, dọn chỗ góc trên-phải, thêm global toggle vào Settings

## Context Links

- Plan tổng: `plan.md` (quyết định đã chốt 2026-09-12: chọn phương án (a) thêm row "Âm thanh: Bật/Tắt")
- `GopetUnityClient/Assets/Scripts/Runtime/GopetBootstrap.cs:93` — tạo SoundToggleButton, `SetActive(false)` sau splash
- `GopetUnityClient/Assets/Scripts/Runtime/GopetBootstrap.SplashConnection.cs:11,39` — bật loa sau splash
- `GopetUnityClient/Assets/Scripts/Runtime/UI/SoundToggleButton.cs` — nút loa (giữ file, không xóa)
- `GopetUnityClient/Assets/Scripts/Runtime/UI/ShopServiceEventHud.cs:30` — `ReservedRightFrac` chừa chỗ cho loa
- `GopetUnityClient/Assets/Scripts/Runtime/UI/SettingsView.cs` — đã có toggle Nhạc nền / Hiệu ứng

## Overview

- Priority: P1 (blocker cho phase 2 — minimap chiếm đúng vị trí này)
- Status: complete (code + compile verification)
- Mô tả: tắt hẳn SoundToggleButton khỏi màn hình, thu hẹp `ReservedRightFrac` để 3 nút Shop/Dịch vụ/Sự kiện dạt sát mép phải, thêm row "Âm thanh: Bật/Tắt" (global toggle gọi `SoundManager.SetEnabled`) vào SettingsView.

## Key Insights

- SoundToggleButton nằm trên canvas CHUNG (không phải PixelCanvas.Content) để bám góc màn hình thật — minimap phase 2 phải dùng cùng kỹ thuật neo.
- `ShopServiceEventHud.ReservedRightFrac = 0.05 + 0.025 + 0.012` chừa đúng chỗ loa; xóa loa mà không sửa → hở khoảng trống xấu ở mép phải.
- SettingsView đã có 2 toggle riêng (Nhạc nền, Hiệu ứng) nhưng KHÔNG có global on/off như SoundToggleButton (`SoundManager.SetEnabled`). User đã chốt phương án (a): thêm row "Âm thanh" ở đầu panel.

## Requirements

- Functional:
  - Không còn icon loa nào trên màn hình (splash, login, ingame).
  - 3 nút Shop/Dịch vụ/Sự kiện dạt sát mép phải, không hở khoảng trống.
  - User vẫn bật/tắt được TOÀN BỘ âm thanh qua Settings → row "Âm thanh: Bật/Tắt".
- Non-functional: không vỡ layout màn hình nhỏ; không thêm file mới.

## Architecture

- Data flow: `GopetBootstrap.Start` → không tạo SoundToggleButton nữa (hoặc tạo rồi `SetActive(false)` vĩnh viễn) → `ShopServiceEventHud` neo lại với `ReservedRightFrac` mới → `SettingsView` thêm row global toggle.
- Không đổi protocol, không đổi state.

## Related Code Files

- Sửa: `GopetBootstrap.cs` (dòng ~93: xóa/bỏ tạo soundToggle, sửa call `StartSplashConnection` không cần param soundToggle nữa)
- Sửa: `GopetBootstrap.SplashConnection.cs` (dòng ~11,39: bỏ param + bỏ `SetActive(true)`)
- Sửa: `ShopServiceEventHud.cs` (dòng ~30: `ReservedRightFrac` → chỉ còn gap, vd `0.012f`)
- Sửa: `SettingsView.cs` (thêm 1 row "Âm thanh" ở top (55f), đẩy các row cũ xuống; `Refresh()` hiển thị `OnOff(_sound.Enabled)`; click gọi `_sound.SetEnabled(!_sound.Enabled)`. Tăng panel height 310f → ~350f)
- Không xóa: `SoundToggleButton.cs` (giữ file để rollback nhanh; YAGNI — xóa hẳn chỉ khi user confirm)

## Implementation Steps

1. `GopetBootstrap.cs`: xóa dòng tạo `SoundToggleButton`, xóa biến `soundToggle`, sửa lời gọi `StartSplashConnection(canvas.transform, font)`.
2. `GopetBootstrap.SplashConnection.cs`: bỏ param `soundToggle`, xóa `soundToggle.gameObject.SetActive(true)` trong `splash.Finished`.
3. `ShopServiceEventHud.cs`: `ReservedRightFrac` → `0.012f` (chỉ gap), cập nhật comment.
4. `SettingsView.cs`: thêm row "Âm thanh" ở top (55f), đẩy các row cũ xuống; `Refresh()` hiển thị `OnOff(_sound.Enabled)`; click gọi `_sound.SetEnabled(!_sound.Enabled)`. Tăng panel height 310f → ~350f.
5. Play test: splash → login → ingame, xác nhận không còn loa, 3 nút sát mép phải, Settings bật/tắt tiếng được.

## Todo List

- [x] Xóa tạo SoundToggleButton khỏi GopetBootstrap
- [x] Dọn param soundToggle khỏi SplashConnection
- [x] Sửa ReservedRightFrac để chừa đúng chỗ cho minimap thay thế
- [x] Thêm row Âm thanh global vào SettingsView
- [ ] Play test 3 màn hình (splash/login/ingame)

## Success Criteria

- Không còn GameObject "SoundToggleButton" trong hierarchy khi Play.
- 3 nút HUD cách mép phải đúng GapFrac, không hở.
- Tắt "Âm thanh" trong Settings → im hoàn toàn; bật lại → có tiếng.

## Risk Assessment

| Risk | Likelihood x Impact | Mitigation |
|------|---------------------|------------|
| Quên chỗ `SetActive(true)` loa ở file khác | Thấp x Trung bình | Đã grep: chỉ 2 file chạm soundToggle (GopetBootstrap.cs, SplashConnection.cs) |
| Settings panel tràn màn hình nhỏ khi thêm row | Thấp x Thấp | Panel 380x350 vẫn nhỏ hơn 960x540 reference |

## Security Considerations

- Không có. Chỉ UI toggle local, không gửi gói mạng.

## Next Steps

- Xong → Phase 02 (MinimapWidget chiếm đúng góc vừa dọn).
- Rollback: revert 4 file trên; SoundToggleButton.cs còn nguyên nên chỉ cần khôi phục lời gọi Create.
