# Phase 05 — Wiring GameSession: 3 lối vào, gắn MinimapWidget vào HUD, xoá MapPickerView

## Context Links

- [plan.md](plan.md) · [phase-02](phase-02-client-tele-menu-lock-reason.md) · [phase-04](phase-04-world-map-view-ui.md)
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Teleport.cs` (54 dòng)
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs:75` field `_mapPickerView`
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs:168-178` dựng HUD overlay
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs:419-420` `CharacterMenuAction.Teleport`
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Interactions.cs:54-55` `LocalMenu.TicketRoom`
- `GopetUnityClient/Assets/Scripts/Runtime/World/MapScene.cs:26-28` `MapId`, `Self`; `:53` `MapLoaded`
- `GopetUnityClient/Assets/Scripts/Runtime/UI/MinimapWidget.cs` — `Clicked`, `SetMap`, `BindPlayer`

## Overview

- **Priority:** P1
- **Status:** pending · **Phụ thuộc:** phase 02 + phase 04
- Thay `MapPickerView` bằng `WorldMapView` ở mọi điểm gọi, gắn `MinimapWidget` lên HUD
  làm lối vào thứ ba, và xoá hẳn picker cũ.

## Key Insights

- Chỉ có **2** chỗ gọi `RequestOptions()`: `GameSession.cs:420` (menu nhân vật → Dịch
  chuyển) và `GameSession.Interactions.cs:55` (NPC phòng vé). Cả hai đều chỉ *yêu cầu*
  dữ liệu; màn hình luôn mở ở `OnTeleportOptionsReceived` → thêm lối vào thứ ba chỉ là
  thêm một nơi gọi `RequestOptions()`, không đụng logic hiển thị (KISS).
- `MinimapWidget` **chưa từng được dựng ngoài test** (`MinimapPickerTests.cs`,
  `BootstrapLayeringTests.cs`) — lần đầu chạy thật sẽ lộ các vấn đề chưa ai thấy:
  vị trí neo góc phải-trên đụng `CurrencyBar`/`ExpBuffIndicator`, và bake map hiện tại
  ở mỗi lần `MapLoaded`.
- Câu khoá đang hardcode ở `GameSession.Teleport.cs:32-33` cùng comment giải thích toast
  một dòng. Sau phase 02+04, thay bằng `option.LockReason` và xoá comment đã lỗi thời.
- `GameSession.cs` nằm trong allowlist ngoại lệ 200 dòng (`CODE_HEALTH_EXCEPTIONS.md`) →
  được sửa nhưng **không được phình thêm**: mọi logic mới đặt vào `GameSession.Teleport.cs`.

## Requirements

**Chức năng**
- Menu nhân vật → Dịch chuyển, NPC phòng vé, nút minimap trên HUD: cả ba mở cùng một màn.
- Chọn map mở → giữ nguyên chuỗi hiện có: `PlayEffect("s_outMap_1")` → `SendWarp` →
  `_warpFade.FadeOut()` → đóng màn.
- Chọn map khoá → toast nội dung `option.LockReason`, màn **không** đóng.
- Minimap hiện map đang đứng, chấm đỏ theo nhân vật, bấm vào mở bản đồ thế giới.
- `MapPickerView` bị xoá hoàn toàn khỏi repo.

**Phi chức năng**
- Không tăng số dòng `GameSession.cs` quá 5 dòng.
- Minimap không che nút nào đang có trên HUD overlay.

## Architecture

```
CharacterMenuAction.Teleport ─┐
LocalMenu.TicketRoom         ─┼─> _mapTeleportHandler.RequestOptions()
MinimapWidget.Clicked        ─┘            │  (server: TELE_MENU)
                                           ▼
                         GameSession.OnTeleportOptionsReceived(options)
                                           │
                    WorldMapView.Create(_hudParent, UiBuilder.BuiltinFont())
                              .Bind(options, _scene.MapId)
                    ├─ Chosen(mapId)        -> SendWarp + fade + đóng
                    ├─ LockedChosen(reason) -> ShowToast(reason)
                    └─ CloseRequested       -> đóng

MapScene.MapLoaded ─> _minimap.SetMap(JarMaps.Load(_scene.MapId), _scene.MapId)
MapScene.SelfSpawned ─> _minimap.BindPlayer(_scene.Self)
```

`Chosen` trả **mapId** chứ không phải index (khác `MapPickerView`) → cần tra ngược option
để lấy `WaypointIndex`. Dùng `Array.Find(options, o => o.MapId == mapId)`; không tìm thấy
thì bỏ qua (server đổi danh sách giữa chừng).

## Related Code Files

**Sửa**
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Teleport.cs` (viết lại nội dung)
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs` — field `:75`, HUD `:168-178`, `MapLoaded`/`SelfSpawned` `:286-295`
- `GopetUnityClient/Assets/Tests/PlayMode/MinimapPickerTests.cs` — gỡ 4 test của `MapPickerView`
- `GopetUnityClient/Assets/Tests/PlayMode/BootstrapLayeringTests.cs` — nếu còn tham chiếu picker (grep trước khi sửa)

**Xoá**
- `GopetUnityClient/Assets/Scripts/Runtime/UI/MapPickerView.cs` + `.meta`
- `GopetUnityClient/Assets/Scripts/Runtime/UI/MapPickerView.Layout.cs` + `.meta`

**Tạo mới:** không có.

## Implementation Steps

1. `GameSession.cs:75`: đổi `private MapPickerView _mapPickerView;` →
   `private WorldMapView _worldMapView;` và thêm `private MinimapWidget _minimap;`.
2. `GameSession.cs` (sau khối `_petButton`, ~`:177`): dựng minimap
   ```csharp
   s._minimap = MinimapWidget.Create(s._hudParent, UiBuilder.BuiltinFont());
   s._minimap.Clicked += () => s._mapTeleportHandler.RequestOptions();
   ```
3. `GameSession.cs` gần `:286-295`: `s._scene.MapLoaded += s.RefreshMinimap;` và
   `s._scene.SelfSpawned += _ => s._minimap.BindPlayer(s._scene.Self);`
   (`RefreshMinimap` định nghĩa ở `GameSession.Teleport.cs` để `GameSession.cs` không phình).
4. Viết lại `GameSession.Teleport.cs`:
   - `OnTeleportOptionsReceived`: `CloseMapPicker()` → đổi tên `CloseWorldMap()`; rỗng →
     toast cũ; dựng `WorldMapView`, `Bind(options, _scene.MapId)`
   - `Chosen += mapId => OnWorldMapChosen(options, mapId)` — tra option theo mapId, giữ
     nguyên `PlayEffect`/`SendWarp`/`FadeOut`/đóng màn (bê từ `OnMapPickerChosen:37-45`)
   - `LockedChosen += reason => ShowToast(string.IsNullOrEmpty(reason) ? "Map này chưa mở." : reason);`
     và xoá comment "ToastView là một dòng…" (`:30-31`) vì phase 04 đã sửa toast
   - `CloseRequested += CloseWorldMap`
   - thêm `private void RefreshMinimap()`: `try { _minimap?.SetMap(JarMaps.Load(_scene.MapId), _scene.MapId); }`
     `catch (Exception ex) { Debug.LogWarning(...); }` — map thiếu file `.bytes` không được
     làm hỏng luồng vào map
   - file vẫn phải < 200 dòng; nếu vượt → tách `GameSession.Minimap.cs`
5. Grep `MapPickerView` toàn repo, gỡ hết tham chiếu, rồi xoá 2 file `.cs` + 2 `.meta`.
6. `MinimapPickerTests.cs`: giữ test bake minimap, xoá 4 test picker (chúng được thay bằng
   `WorldMapViewTests` ở phase 06). Đổi tên file thành `MinimapWidgetTests.cs` (kèm `.meta`).
7. Chạy `pwsh GopetUnityClient/verify.ps1` và `pwsh GopetUnityClient/run-playmode-tests.ps1`.
8. Chạy game thật với server phase 01: thử đủ 3 lối vào, chụp màn hình kiểm minimap không
   đè `CurrencyBar`/`ExpBuffIndicator`/`CharacterMenuButton`; lệch thì chỉnh
   `MinimapWidget.MarginFrac/SizeFrac` (`MinimapWidget.cs:10`).

## Todo List

- [ ] 1. Đổi field trong `GameSession.cs`
- [ ] 2. Dựng `MinimapWidget` + nối `Clicked`
- [ ] 3. Nối `MapLoaded` / `SelfSpawned`
- [ ] 4. Viết lại `GameSession.Teleport.cs` dùng `WorldMapView` + `LockReason`
- [ ] 5. Xoá `MapPickerView.cs` / `.Layout.cs` + `.meta`
- [ ] 6. Dọn `MinimapPickerTests` → `MinimapWidgetTests`
- [ ] 7. `verify.ps1` + `run-playmode-tests.ps1` xanh
- [ ] 8. Kiểm tay 3 lối vào + vị trí minimap

## Success Criteria

- `grep -rn "MapPickerView" GopetUnityClient` → 0 kết quả.
- `pwsh GopetUnityClient/verify.ps1` xanh; `GameSession.cs` không tăng quá 5 dòng.
- `pwsh GopetUnityClient/run-playmode-tests.ps1` xanh.
- Kiểm tay: cả 3 lối vào mở đúng một màn; chọn map mở → đổi map thật; chọn map khoá →
  toast đúng câu server gửi, màn vẫn mở; minimap hiện map hiện tại và chấm đỏ di chuyển.

## Risk Assessment

| Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|--------|----------|-----------|-----------|
| Minimap đè HUD (lần đầu chạy thật) | Cao | Trung bình | Bước 8 kiểm bằng ảnh chụp; test layout ở phase 06; chỉnh `MarginFrac` |
| `MapLoaded` bake lại minimap mỗi lần đổi map gây khựng | Trung bình | Trung bình | `JarMaps.Load` có cache layout; bake 512 chỉ 1 map; nếu khựng → hạ `MaxTextureSize` |
| Tra option theo `mapId` không thấy → bấm không phản hồi | Thấp | Trung bình | Log `Debug.LogWarning` khi không tìm thấy, để debug được |
| Xoá `MapPickerView` làm vỡ test/asset chưa grep ra | Thấp | Trung bình | Bước 5 grep trước khi xoá; `verify.ps1` compile toàn bộ |
| Bấm minimap liên tục spam TELE_MENU | Thấp | Thấp | Nếu thấy → bọc `_actionThrottle.TryAcquire("teleport", 500, …)` như `GameSession.Interactions.cs:44` |

**Rollback:** revert commit; `MapPickerView` quay lại từ git. Phase 05 là commit *duy nhất*
xoá file → revert sạch, không cascade.

## Security Considerations

- Toast hiển thị nguyên văn chuỗi server (`LockReason`) — `Text` không rich text; có
  fallback khi rỗng để không hiện toast trống.
- Client vẫn gửi `SendWarp` cho map mình cho là mở; server phase 01 là chốt chặn thật.
- Không log `LockReason` kèm dữ liệu tài khoản vào console phát hành.

## Next Steps

- Phase 06: test tự động cho `WorldMapView` + vị trí minimap, chạy verify, cập nhật docs.
