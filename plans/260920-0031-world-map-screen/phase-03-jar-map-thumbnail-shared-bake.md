# Phase 03 — Tách bake thumbnail dùng chung (JarMapThumbnail)

## Context Links

- [plan.md](plan.md)
- `GopetUnityClient/Assets/Scripts/Runtime/UI/MinimapWidget.cs` (147 dòng) — `Bake`/`DrawTile`/`ReleaseBake`
- `GopetUnityClient/Assets/Scripts/Runtime/UI/JarMaps.cs` — `JarMaps.Load(mapId)` (có cache layout)
- `GopetUnityClient/Assets/Scripts/UiLogic/JarMapLayout.cs` — `Layers`, `WidthTiles`, `StripOf`, `CellOf`
- `GopetUnityClient/Assets/Tests/PlayMode/MinimapPickerTests.cs` — test bake hiện có

## Overview

- **Priority:** P1 (chặn phase 04)
- **Status:** pending · **Phụ thuộc:** không
- Rút logic vẽ map ra `RenderTexture` khỏi `MinimapWidget` thành helper dùng chung cho
  cả minimap lẫn node bản đồ thế giới. DRY thuần tuý, không đổi hành vi minimap.

## Key Insights

- `MinimapWidget.Bake` (`:88-110`) + `DrawTile` (`:111-122`) là ~30 dòng độc lập với
  MonoBehaviour: chỉ cần `JarMapLayout` + kích thước tối đa. Phase 04 cần y hệt, chỉ khác
  `MaxTextureSize` (128 thay vì 512) → copy-paste sẽ tạo bản sao thứ hai của thuật toán.
- `JarMaps.Load` đã cache `JarMapLayout` theo mapId → phần tốn kém còn lại là *vẽ*
  (WidthTiles × HeightTiles × số layer lệnh `Graphics.DrawTexture`), không phải parse.
- `Cache` của `RenderTexture` nên do *người gọi* giữ, không phải helper: minimap giữ 1 cái
  và đổi khi vào map mới; world map giữ một dict và thả hết khi đóng. Cache toàn cục sẽ
  làm hai chủ sở hữu tranh nhau `Release()` (lỗi "RenderTexture đã huỷ" khó truy).

## Requirements

**Chức năng**
- `JarMapThumbnail.Bake(JarMapLayout map, int maxSize)` → `RenderTexture` đã vẽ xong,
  giữ đúng tỉ lệ map, nền `(0.03,0.06,0.08)` như minimap hiện tại.
- `JarMapThumbnail.Release(ref RenderTexture rt)` — `Release()` + `Object.Destroy`, an toàn khi null.
- `MinimapWidget` gọi helper, hành vi và test hiện có không đổi.

**Phi chức năng**
- Không nuốt exception: ném ra cho caller (minimap đang bắt ở `:71` và rơi về fallback).
- File mới < 200 dòng; `MinimapWidget.cs` giảm xuống ~120 dòng.

## Architecture

```
JarMaps.Load(mapId) ──> JarMapLayout (cache sẵn)
                              │
             JarMapThumbnail.Bake(layout, maxSize)
                              │  GL.LoadPixelMatrix + Graphics.DrawTexture (tile 24px)
                              ▼
                        RenderTexture  ── caller sở hữu ──┬─ MinimapWidget (1 cái, maxSize 512)
                                                          └─ WorldMapView  (dict theo mapId, maxSize 128)
```

## Related Code Files

**Tạo mới**
- `GopetUnityClient/Assets/Scripts/Runtime/UI/JarMapThumbnail.cs` (~60 dòng) + `.meta`

**Sửa**
- `GopetUnityClient/Assets/Scripts/Runtime/UI/MinimapWidget.cs`

**Xoá:** không có.

## Implementation Steps

1. Tạo `JarMapThumbnail.cs`, namespace `Gopet.Runtime.UI`, `public static class JarMapThumbnail`.
   Comment đầu file (tiếng Việt): vì sao helper *không* cache `RenderTexture` — quyền sở
   hữu thuộc về caller để tránh double-release.
2. Chuyển nguyên `Bake` thành
   `public static RenderTexture Bake(JarMapLayout map, int maxSize)`:
   - tính `scale = Mathf.Min(1f, (float)maxSize / Mathf.Max(map.WidthPixels, map.HeightPixels))`
   - tạo `RenderTexture(width, height, 0, ARGB32) { filterMode = Bilinear }`, `Create()`
   - `GL.Clear` + `GL.PushMatrix/LoadPixelMatrix` + 3 vòng lặp layer/row/col như cũ
   - `finally { GL.PopMatrix(); RenderTexture.active = previous; }`
   - ném `ArgumentException` nếu `map == null` hoặc kích thước ≤ 0 (minimap đang tự kiểm
     trước khi gọi ở `:57`, world map sẽ dựa vào ngoại lệ này).
3. Chuyển `DrawTile` thành `private static` trong cùng file, giữ nguyên phép tính UV.
4. Thêm `public static void Release(ref RenderTexture rt)`.
5. `MinimapWidget.cs`: xoá `Bake`/`DrawTile`; `SetMap` gọi
   `_baked = JarMapThumbnail.Bake(map, MaxTextureSize);` trong `try` cũ. `ReleaseBake`
   gọi `JarMapThumbnail.Release(ref _baked)` (giữ `_mapImage.texture = null` trước).
6. Chạy PlayMode test: `Minimap_BakeMapThat_VaDotTheoNhanVat` phải vẫn xanh.

## Todo List

- [ ] 1. Tạo `JarMapThumbnail.cs` + comment quyền sở hữu
- [ ] 2. Chuyển `Bake` sang helper (có guard tham số)
- [ ] 3. Chuyển `DrawTile`
- [ ] 4. Thêm `Release(ref)`
- [ ] 5. `MinimapWidget` gọi helper
- [ ] 6. Chạy PlayMode test minimap

## Success Criteria

- `pwsh GopetUnityClient/run-playmode-tests.ps1` → `MinimapPickerTests` xanh, không đổi assert.
- `pwsh GopetUnityClient/verify.ps1` → qua bước 10/10; `MinimapWidget.cs` < 130 dòng,
  `JarMapThumbnail.cs` < 80 dòng.
- `grep -n "Graphics.DrawTexture" Assets/Scripts` → chỉ còn 1 kết quả (trong helper).

## Risk Assessment

| Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|--------|----------|-----------|-----------|
| Đổi `RenderTexture.active` mà quên khôi phục → camera vẽ sai | Thấp | Cao | Giữ nguyên khối `try/finally` có sẵn, không viết lại |
| Refactor làm hỏng minimap (chưa dùng thật nên ít người phát hiện) | Trung bình | Trung bình | Test bake hiện có chạy trước/sau; phase 05 gắn minimap vào HUD để thấy thật |
| Double-release nếu về sau thêm cache toàn cục | Thấp | Trung bình | Comment đầu file nêu rõ quy ước sở hữu |

**Rollback:** revert 1 commit; helper không có consumer nào ngoài minimap ở thời điểm này.

## Security Considerations

- Không đụng mạng, không đụng dữ liệu người chơi. Chỉ đọc `Resources` cục bộ qua `JarMaps`.
- `maxSize` do code gọi quyết định (hằng), không lấy từ gói server → không có đường ép
  cấp phát `RenderTexture` khổng lồ từ xa.

## Next Steps

- Phase 04 dùng `JarMapThumbnail.Bake(JarMaps.Load(mapId), 128)` cho từng node.
