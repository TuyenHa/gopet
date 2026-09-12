# Phase 02 — MinimapWidget: ô vuông ảnh map + dot nhân vật

## Context Links

- Plan tổng: `plan.md` (quyết định đã chốt 2026-09-12: ảnh thu nhỏ map thật + dot di chuyển theo vị trí thật)
- Phase trước: `phase-01-an-nut-loa.md` (dọn chỗ góc trên-phải)
- Neo y nguyên kỹ thuật của `SoundToggleButton.cs:39-41` (anchor theo SizeFrac/MarginFrac, canvas CHUNG)
- `Gopet/UiLogic/JarMapLayout.cs` — `WidthPixels/HeightPixels`, `Layers`, `StripOf/CellOf`, `TileSize = 24`
- `Gopet/Runtime/World/TileAssetProvider.cs:37` — `Cell(imageId, cellIndex)` trả sprite ô 24×24 (đã cache tĩnh)
- `Gopet/UiLogic/JarMaps` — `JarMaps.Load(mapId)` trả `JarMapLayout` (pattern `JarMapBackground.cs:49`)
- `Gopet/UiLogic/MapPlacement.cs:23` — `WorldToJar(worldX, worldY, mapHeightPixels)`
- `Gopet/Runtime/World/PlayerAvatar.cs` — có `JarY` public, THIẾU `JarX`; `_interp` (PositionInterpolator) giữ vị trí nội suy hiện tại
- `Gopet/UiLogic/PositionInterpolator.cs` — `X/Y` (hiện tại, mượt), `TargetX/TargetY`

## Overview

- Priority: P1
- Status: complete (code + compile verification)
- Mô tả: widget minimap hình vuông neo góc trên-phải màn hình thật. Ảnh nền = **bake 1 lần từ tile map hiện tại** (không cần asset thumbnail mới). **Dot đỏ** hiển thị vị trí nhân vật, cập nhật mỗi frame. Bấm vào phát sự kiện `Clicked` (phase 03 bắt để mở MapPickerView).

## Key Insights

- **Không cần asset ảnh mới**: `JarMaps.Load(mapId)` cho `JarMapLayout`; duyệt `Layers`, mỗi tile lấy sprite qua `TileAssetProvider.Cell(strip, cell)` (đã cache), copy pixel 24×24 vào `Texture2D` kích thước map → 1 sprite hiển thị. Bake 1 lần khi vào map, hủy khi đổi map.
- **Chỉ bake tile nền (Layers), bỏ qua Objects/Entities** — YAGNI: hình map vẫn nhận ra được; thêm object sau nếu user yêu cầu.
- **Fallback nếu texture strip không readable**: `texture.isReadable == false` → không copy pixel được → hiện text `"Map {id}"` trên nền xanh (giữ code fallback đơn giản).
- **Dot dùng vị trí NỘI SUY hiện tại** (`_interp.X/Y` → `WorldToJar`), không dùng target — dot chạy mượt theo nhân vật trên màn hình, không nhảy cục.
- `PlayerAvatar` thiếu `JarX` public → thêm 1 property `JarPosition` (2 dòng), không đụng logic di chuyển.
- Kích thước: `SizeFrac = 0.08` (to hơn loa 0.05 vì chứa ảnh), `MarginFrac = 0.025` giữ nguyên.
- Map jar Y hướng XUỐNG (gốc trên-trái), UI Y hướng LÊN → dot.y phải flip: `y = size/2 - jarY/mapH*size`.

## Requirements

- Functional:
  - Ô vuông neo góc trên-phải, cùng layer canvas chung với ShopServiceEventHud.
  - Vào map → ảnh minimap hiện đúng hình map đó; đổi map → bake lại.
  - Dot đỏ di chuyển theo nhân vật (đi tới đâu dot tới đó).
  - Bấm → sự kiện `Clicked`.
- Non-functional: file mới <150 dòng. Bake 1 lần/map (không bake mỗi frame). Hủy texture/sprite cũ khi đổi map (tránh leak).

## Architecture

```
MapLoaded (mapId mới)
  → _minimap.SetMap(JarMaps.Load(mapId))   // bake 1 lần
  → _minimap.BindPlayer(selfAvatar)        // dot follow

MinimapWidget.Update (mỗi frame)
  → _follow.JarPosition → WorldToJar → tỉ lệ vào ô vuông → dot.anchoredPosition

MinimapWidget.Clicked → Phase 03 (RequestOptions → MapPickerView)
```

## Related Code Files

- Tạo mới: `GopetUnityClient/Assets/Scripts/Runtime/UI/MinimapWidget.cs`
- Sửa: `GopetUnityClient/Assets/Scripts/Runtime/World/PlayerAvatar.cs` (thêm property `JarPosition`, ~2 dòng)
- Sửa: `GopetBootstrap.cs` — tạo MinimapWidget thay SoundToggleButton, truyền vào StartSplashConnection
- Sửa: `GopetBootstrap.cs` — giữ minimap ẩn qua splash/login/register, chỉ bật trong callback đăng nhập thành công
- Sửa: `GameSession.cs` — wire SetMap + BindPlayer khi MapLoaded (chi tiết phase 04)

## Implementation Steps

1. `PlayerAvatar.cs`: thêm property (đặt cạnh `JarY`, dòng ~37):
   ```
   public (int jarX, int jarY) JarPosition =>
       MapPlacement.WorldToJar(_interp.X, _interp.Y, _mapHeightPixels);
   ```
2. Tạo `MinimapWidget.cs`:
   - `Create(Transform parent, Font font)`: GameObject + RectTransform anchor góc trên-phải (`SizeFrac = 0.08`, `MarginFrac = 0.025`); Image viền; Image `_mapImage` (stretch đầy ô); Image `_dot` (12×12, đỏ, trên cùng); Image `_label` fallback (ẩn mặc định); Button onClick → `Clicked`.
   - `SetMap(JarMapLayout map)`: hủy texture/sprite cũ; nếu map null → fallback text. Tạo `Texture2D(map.WidthPixels, map.HeightPixels)`; duyệt mọi layer/row/col: `strip = StripOf(tile)`, `cell = CellOf(tile)` → `TileAssetProvider.CellFromMap(map, strip, cell)` → `GetPixels` 24×24 → `SetPixels(col*24, mapH-(row+1)*24, 24, 24)` (flip Y vì Texture2D gốc dưới-trái). Nếu sprite null hoặc texture không readable → fallback text `"Map {id}"`, return. `Apply()` → `Sprite.Create` → gán `_mapImage`. Lưu `_mapW/_mapH`.
   - `BindPlayer(PlayerAvatar avatar)`: `_follow = avatar`.
   - `Update()`: nếu `_follow == null` return; `(jx, jy) = _follow.JarPosition`; clamp 0..mapW/mapH; `dot.anchoredPosition = (jx/mapW*size - size/2, size/2 - jy/mapH*size)`.
   - `OnDestroy()`: hủy texture + sprite đã bake.
   - Event `Action Clicked`.
3. `GopetBootstrap.cs`: thay `SoundToggleButton.Create(...)` bằng `MinimapWidget.Create(canvas.transform, font)`; `SetActive(false)`; truyền vào `StartSplashConnection`.
4. `GopetBootstrap.cs`: callback đăng nhập thành công → `minimap.gameObject.SetActive(true)` sau khi tạo `GameSession`.
5. Play test: splash/login/register không có minimap; vào game → minimap hiện hình map 11; điều khiển nhân vật đi → dot chạy theo; chuyển map → ảnh bake lại.

## Todo List

- [x] Thêm `JarPosition` vào PlayerAvatar.cs
- [x] Tạo MinimapWidget.cs (146 dòng): Create + SetMap(bake GPU) + BindPlayer + Update(dot) + OnDestroy
- [x] Sửa GopetBootstrap.cs: tạo minimap thay soundToggle
- [x] Giữ minimap ẩn qua SplashConnection; chỉ SetActive sau đăng nhập thành công
- [ ] Play test: ảnh map đúng, dot chạy theo nhân vật, đổi map bake lại

## Success Criteria

- Widget minimap xuất hiện góc trên-phải, ảnh nhìn ra được hình map (đường đi, mảng màu tile).
- Nhân vật đi tới đâu dot đỏ tới đó (không delay rõ rệt, không nhảy cục).
- Đổi map → ảnh minimap đổi theo trong 1 lần bake (không giật frame liên tục).
- Bấm minimap → event Clicked fire (log trong test).
- Không leak: chuyển map 5 lần, memory texture không tăng vô hạn (đã hủy cũ).

## Risk Assessment

| Risk | L x I | Mitigation |
|------|-------|------------|
| Texture strip `isReadable == false` → bake đen/trắng | Trung bình x Trung bình | Check `isReadable` trước khi GetPixels; fallback text "Map {id}". Verify `Editor/JarAssetImportSettings.cs` có bật readable cho newMapData |
| Map lớn (vd 60×40 tile = 1440×960 texture ~5.5MB) | Thấp x Thấp | Bake 1 lần/map, hủy cũ; vẫn nhẹ hơn 1 sprite nhân vật HD |
| Dot lệch do flip Y sai | Thấp x Thấp | Công thức flip đã ghi rõ 2 chỗ (bake + dot); test bằng cách đứng 4 góc map |

## Security Considerations

- Không có. Chỉ UI hiển thị, không gửi gói mạng.

## Next Steps

- Xong → Phase 03 (MapPickerView: danh sách map + teleport).
- Rollback: revert 3 file, khôi phục SoundToggleButton.
