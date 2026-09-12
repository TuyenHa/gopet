# Phase 04 — Wire hoàn chỉnh + test end-to-end

## Context Links

- Plan tổng: `plan.md`
- Phase trước: `phase-03-map-picker-view.md` (MapPickerView tạo xong)
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs` — Start(), _minimap wiring, MapLoaded event
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Teleport.cs` — OnTeleportOptionsReceived
- `GopetUnityClient/Assets/Scripts/Runtime/World/MapScene.cs:25` — `MapId` property, `MapLoaded` event

## Overview

- Priority: P1
- Status: partial — implementation và compile verification hoàn tất; chờ PlayMode/E2E
- Mô tả: Kết nối tất cả mảnh — minimap widget hiển thị **ảnh map thật + dot nhân vật di chuyển** đúng, click mở picker, picker teleport, camera recenter, HUD cập nhật. Chạy end-to-end trên 3–4 map.

## Key Insights

- `_scene.MapLoaded` event đã có (GameSession.cs:262,279) — cần thêm 2 lambda: 1) `_minimap.SetMap(_scene.Renderer.Map)` (bake ảnh tile) + `_minimap.BindPlayer(selfAvatar)` (bật dot theo nhân vật).
- `MapTeleportHandler.RequestOptions()` gọi 1 lần → server trả 1 lần. Nếu bấm minimap nhiều lần cần gọi lại. Solution: gọi lại trong `_mapPickerView` close rồi reopen hoặc gọi `RequestOptions()` mỗi lần MinimapWidget.Clicked.
- Teleport bằng `SendWarp` → server tự gửi `ON_PLAYER_WARPING` → MapScene reload → `MapLoaded` fires → minimap tự bake lại + dot vẫn giữ (selfAvatar được tạo mới mỗi map → cần gọi `BindPlayer` lại sau MapLoaded).

## Requirements

- Functional:
  - Minimap hiện **ảnh map thật** + **dot đỏ di chuyển theo nhân vật**.
  - Bấm minimap → danh sách map hiện.
  - Bấm map → teleport thành công, minimap bake lại ảnh map mới, dot cập nhật vị trí.
  - Đóng picker bằng backdrop hoặc nút.
  - Sound toggle vẫn hoạt động qua Settings.
- Non-functional: không thêm file mới. Tổng dòng code mới < 300 (MinimapWidget ~140 + MapPickerView ~130).

## Architecture

```
┌─ GopetBootstrap ─────────────────────────────────────────────┐
│  canvas → MinimapWidget.Create (SetActive(false))            │
│  login success → GameSession.Start → SetActive(true)         │
└──────────────────────────────────────────────────────────────┘
          │
          ▼
┌─ GameSession.Start ──────────────────────────────────────────┐
│  _minimap.SetMap(DefaultMapId)                                │
│  _scene.MapLoaded += _minimap.SetMap(_scene.MapId)           │
│  _minimap.Clicked += _mapTeleportHandler.RequestOptions      │
│  _mapTeleportHandler.OptionsReceived += OnTeleportOptionsRecv │
└──────────────────────────────────────────────────────────────┘
          │
          ▼
┌─ OnTeleportOptionsReceived ──────────────────────────────────┐
│  MapPickerView.Create(_hudParent) → Bind(options)            │
│  row click → SendWarp + FadeOut + Close                       │
└──────────────────────────────────────────────────────────────┘
```

## Related Code Files

- Sửa: `GameSession.cs` — wire _minimap với MapLoaded, Clicked
- Sửa: `GameSession.Teleport.cs` — _mapPickerView + close/open lifecycle
- Sửa: `GopetBootstrap.cs` — minhap field + pass cho session (nếu cần session truy cập)
- Sửa: `GopetBootstrap.SplashConnection.cs` — same param swap (đã làm ở phase 01)

## Implementation Steps

1. `GopetBootstrap.cs`:
   - Tạo `MinimapWidget` (đã làm phase 02).
   - Lưu field `_minimap` để `GameSession.Start` có thể truy cập.
   - `GameSession.Start` cần nhận `minimap` param hoặc bootstrap inject sau.

2. `GameSession.cs`:
   - Thêm param `MinimapWidget minimap = null` vào `GameSession.Start()`.
   - `s._minimap = minimap;`
   - `s._scene.MapLoaded += () => { minimap?.SetMap(s._scene.Renderer.Map); minimap?.BindPlayer(s._selfAvatar); };`
   - Gọi `minimap?.SetMap(...)` + `minimap?.BindPlayer(...)` ngay lần đầu sau khi tạo.
   - `minimap.Clicked += () => _mapTeleportHandler.RequestOptions();`

3. `GameSession.Teleport.cs`:
   - Field `_mapPickerView` thay `_teleportDialog`.
   - `OnTeleportOptionsReceived`:
     ```
     if options.Length == 0: ShowToast("Không có map"); return;
     _mapPickerView = MapPickerView.Create(_hudParent, UiBuilder.BuiltinFont());
     _mapPickerView.Bind(options);
     _mapPickerView.Chosen += OnMapPickerChosen;
     _mapPickerView.CloseRequested += CloseMapPicker;
     ```
   - `OnMapPickerChosen(option)`: play effect + SendWarp + FadeOut + Close.
   - `CloseMapPicker`: Destroy + null.

4. Kiểm tra ¬ có `_teleportDialog` field nào dùng ở chỗ khác không.
   - Grep `_teleportDialog` trong GameSession.cs: chỉ dùng trong `OnTeleportOptionsReceived` (dòng 12,19,21,32,33) → rename an toàn.

5. Play test end-to-end:
   - Vào game → minimap hiện ảnh map 11 + dot đỏ ở vị trí nhân vật.
   - Điều khiển nhân vật đi → dot chạy theo.
   - Bấm minimap → danh sách map hiện.
   - Bấm map khác → fade → teleport thành công → minimap bake ảnh map mới, dot cập nhật.
   - Mở Settings → "Âm thanh" bật/tắt OK.
   - Chuyển map bằng portal (không qua minimap) → minimap vẫn bake lại.
   - Chuyển map bằng NPC teleport → minimap bake lại.

## Todo List

- [x] Inject MinimapWidget vào GameSession.Start
- [x] Wire MapLoaded → minimap.SetMap + BindPlayer
- [x] Wire Clicked → RequestOptions
- [x] Đổi _teleportDialog → _mapPickerView trong Teleport.cs
- [x] Handle empty options (toast)
- [ ] Play test full flow: minimap → picker → teleport → minimap bake lại
- [ ] Play test: portal warp → minimap bake lại
- [ ] Play test: dot chạy theo nhân vật khi đi bộ
- [ ] Play test: Settings sound toggle
- [x] Kiểm tra file < 200 dòng (`MinimapWidget.cs`: 146, `MapPickerView.cs`: 121)

## Success Criteria

- E2E: bấm minimap → chọn map → player ở map mới, camera recenter, minimap bake lại ảnh map mới + dot chạy tiếp.
- Minimap: ảnh map nhận ra hình dạng, dot đỏ chạy theo nhân vật mượt.
- Sound toggle: Settings vẫn bật/tắt được.
- Không có regression: login, chat, NPC dialog, portal warp vẫn hoạt động.
- Không file nào > 200 dòng.

## Risk Assessment

| Risk | L x I | Mitigation |
|------|-------|------------|
| NullReference: _minimap trước khi vào game | Thấp x Trung bình | Guard `?.` everywhere, giữ SetActive(false) tới login success |
| MapPickerView chưa đóng trước khi reopen | Thấp x Thấp | Guard: if (_mapPickerView != null) Destroy trước khi tạo mới |
| Splash/login/register: minimap hiện quá sớm | Thấp x Thấp | SetActive(false) ở Start, chỉ SetActive(true) sau login success |

## Security Considerations

- Teleport dùng đúng gói server đã validate. Không có bypass.

## Next Steps

- Review cuối: code review agent chạy trên 4 file đã sửa + 2 file mới.
- Rollback: revert tất cả file; SoundToggleButton.cs còn nguyên.
