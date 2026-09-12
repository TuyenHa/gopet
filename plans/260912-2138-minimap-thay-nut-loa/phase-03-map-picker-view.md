# Phase 03 — MapPickerView: màn hình chọn map

## Context Links

- Plan tổng: `plan.md`
- Phase trước: `phase-02-minimap-widget.md` (widget bấm → mở picker)
- `GopetUnityClient/Assets/Scripts/Net/Map/MapTeleportHandler.cs` — `MapTeleportOption { MapId, Name, Description, WaypointIndex }`, `RequestOptions()`
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Teleport.cs` — `OnTeleportOptionsReceived` hiện tại dùng ChoiceDialogView
- Tham chiếu layout: `CharacterMenuView.cs` (panel trung tâm, backdrop đen, list row)

## Overview

- Priority: P1
- Status: complete (code + compile verification)
- Mô tả: Màn hình chọn map: backdrop phủ màn, panel trung tâm liệt kê tên các map server gửi (qua `MapTeleportHandler`), bấm tên → teleport bằng `MapHandler.SendWarp(mapId, waypointIndex, 1)` — đúng cơ chế warp hiện có, không tự chế protocol.

## Key Insights

- **Không tự chế protocol**: dùng đúng `MapTeleportHandler.RequestOptions()` (gửi `MGO_COMMAND/TELE_MENU`) và `MapHandler.SendWarp(mapId, waypointIndex, 1)` — y hệt `GameSession.Teleport.cs:29`.
- `MapTeleportOption` có `Description` — hiển thị kèm nếu khác Name (pattern `GameSession.Teleport.cs:15-17`).
- Server trả danh sách map theo quyền của player (VIP/admin teleport) — client không cần filter thêm. **[User 2026-09-12: phần filter level TẠM ĐỂ ĐÓ, giữ nguyên hành vi server trả gì hiện nấy]**
- View này thay thế ChoiceDialogView trong `OnTeleportOptionsReceived` — giữ nguyên luồng request/response, chỉ đổi UI.

## Requirements

- Functional:
  - Mở từ MinimapWidget.Clicked → gọi `MapTeleportHandler.RequestOptions()`.
  - Khi `OptionsReceived` → hiện panel danh sách tên map (Name + Description nếu có).
  - Bấm 1 map → `SendWarp(mapId, waypointIndex, 1)` + đóng view.
  - Có nút Đóng / backdrop click để thoát.
- Non-functional: <150 dòng. Dùng `UiBuilder` + `RoundedUiSprite` pattern. Không phụ thuộc sprite mới.

## Architecture

```
MinimapWidget.Clicked
  → GameSession: _mapTeleportHandler.RequestOptions()
  → server trả TELE_MENU → MapTeleportHandler.OptionsReceived
  → GameSession.OnTeleportOptionsReceived → MapPickerView.Create(_hudParent)
       → Bind(options) → list row
       → row click → SendWarp(mapId, waypointIndex, 1) → close
```

## Related Code Files

- Tạo mới: `GopetUnityClient/Assets/Scripts/Runtime/UI/MapPickerView.cs`
- Sửa: `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Teleport.cs` — thay ChoiceDialogView bằng MapPickerView
- Sửa: `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs` — wire MinimapWidget.Clicked → RequestOptions

## Implementation Steps

1. Tạo `MapPickerView.cs`:
   - `Create(Transform parent, Font font)` → backdrop stretch + panel trung tâm (pattern CharacterMenuView).
   - `Bind(IReadOnlyList<MapTeleportOption> options)` → dựng row cho từng map: label = Name (+ "\n" + Description nếu khác Name).
   - Row click → `Chosen?.Invoke(option)`.
   - `CloseRequested` event; backdrop click → close.
   - Panel height tính theo số map: `TitleHeight + ItemHeight * count + padding`.

2. `GameSession.Teleport.cs`:
   - `OnTeleportOptionsReceived`: thay `ChoiceDialogView` bằng `MapPickerView`.
   - `_mapPickerView.Chosen += option => { SoundManager.PlayEffect("s_outMap_1"); _mapHandler.SendWarp(option.MapId, option.WaypointIndex, 1); _warpFade?.FadeOut(); Close(); }`.
   - Giữ nguyên `_teleportDialog` field hoặc đổi tên `_mapPickerView` (đồng bộ cả 2 chỗ).

3. `GameSession.cs`:
   - Wire `_minimap.Clicked += () => _mapTeleportHandler.RequestOptions();` (sau khi tạo minimap).

4. Play test: bấm minimap → panel hiện danh sách map → bấm map khác → fade + teleport sang map đó.

## Todo List

- [x] Tạo MapPickerView.cs (121 dòng)
- [x] Sửa GameSession.Teleport.cs dùng MapPickerView
- [x] Wire MinimapWidget.Clicked → RequestOptions
- [ ] Play test teleport thật

## Success Criteria

- Bấm minimap → panel danh sách tên map hiện ra (không cần ảnh).
- Bấm tên map → player teleport sang map đó (server xác nhận, camera recenter, HUD cập nhật).
- Đóng được panel bằng backdrop hoặc nút Đóng.

## Risk Assessment

| Risk | L x I | Mitigation |
|------|-------|------------|
| Server trả danh sách rỗng (player không có quyền) | Trung bình x Thấp | Hiện toast "Không có map nào khả dụng" thay vì panel rỗng |
| Bấm 2 map cùng lúc → 2 gói warp | Thấp x Trung bình | `_decided` flag như ChoiceDialogView.cs:30 |
| MapPickerView chồng lên popup khác | Thấp x Thấp | Dùng `_hudParent` (order 35) như các view khác |

## Security Considerations

- Không có dữ liệu nhạy. Warp dùng đúng gói server đã định nghĩa; server tự validate quyền.

## Next Steps

- Xong → Phase 04 (wire hoàn chỉnh + test end-to-end).
- Rollback: revert GameSession.Teleport.cs về ChoiceDialogView, xóa MapPickerView.cs.
