---
title: "Minimap thay nút loa — ẩn SoundToggleButton, thêm minimap ảnh + chọn map"
description: "Ẩn icon loa góc trên-phải, thay bằng ô vuông minimap (ảnh thu nhỏ map + dot nhân vật), bấm mở màn hình chọn map & teleport"
status: superseded
priority: P2
effort: 5h
branch: master
tags: [ui, minimap, teleport, map-selection, sound-settings]
created: 2026-09-12
---

# Minimap thay SoundToggleButton

## Tổng quan

Ẩn nút loa (SoundToggleButton) ở góc trên-phải, giữ nguyên chức năng bật/tắt âm thanh bằng cách thêm row "Âm thanh: Bật/Tắt" vào SettingsView (đã có sẵn toggle nhạc nền + hiệu ứng). Thay vị trí bằng minimap widget: ô vuông hiển thị **ảnh thu nhỏ của map hiện tại** + **dot nhân vật di chuyển theo vị trí thật**. Bấm minimap mở màn hình chọn map với danh sách tên map server gửi, chọn tên map thì teleport.

## Phases

| # | Phase | Status | File chính |
|---|-------|--------|------------|
| 1 | Ẩn SoundToggleButton, dọn ShopServiceEventHud, thêm global toggle vào SettingsView | complete | `GopetBootstrap.cs`, `ShopServiceEventHud.cs`, `SettingsView.cs` |
| 2 | Tạo MinimapWidget — ô vuông góc trên-phải, ảnh map + dot nhân vật | complete | `MinimapWidget.cs` (mới, 146 dòng) |
| 3 | Tạo MapPickerView — danh sách map + teleport | complete | `MapPickerView.cs` (mới, 121 dòng) |
| 4 | Wire vào GameSession — hiển thị map hiện tại, cập nhật dot, mở picker, teleport | partial — code + compile test xanh; chờ PlayMode/E2E sau khi đóng Unity Editor | `GameSession.Teleport.cs`, `GameSession.cs` |

## Implementation status (2026-09-12)

- Thanh chat dưới cùng đã đổi theo mẫu tham chiếu: mặc định thu gọn, hai tab **Khu vực / Thế giới**, bấm tab hoặc mũi tên để mở lịch sử và ô nhập. Chat khu vực tiếp tục hiện bong bóng trên đầu đúng nhân vật và lịch sử dùng tên avatar thay cho userId khi có thể phân giải.
- **Quyết định mới:** minimap đã được gỡ khỏi Bootstrap và không còn hiển thị ở bất kỳ màn hình nào. Người chơi dùng mục **Đổi khu vực** trong menu để mở popup `Chọn khu vực` hiện có.
- HUD hiển thị tên map hiện tại ngay dưới thanh MP và tự cập nhật khi `MapLoaded` chạy.
- Popup **Đổi khu vực** đã rollback về `ChoiceDialogView` cũ: tiêu đề “Chọn khu vực” và hiện số người. Tên map dưới MP vẫn dùng màu trắng.
- Fix màn đen khi chọn map bị khóa: server lọc map 26–28 nếu người chơi chưa mở thượng giới và loại map 22 bị lặp; client hủy fade khi server trả error dialog.
- Đã thay nút loa bằng minimap, chuyển global sound toggle vào Settings.
- Minimap chỉ bật sau khi đăng nhập thành công; splash, đăng nhập và đăng ký luôn ẩn.
- Style minimap là hình chữ nhật, viền xanh dương tạo từ nền + inset; không có border GameObject và không bo góc.
- Minimap bake tile map bằng GPU vào `RenderTexture` tối đa 512 px, không phụ thuộc texture bật Read/Write; dot dùng vị trí nội suy của avatar.
- Map picker dùng đúng options server trả và `SendWarp(mapId, waypointIndex, 1)`, có scroll, đóng backdrop/nút và chặn multi-touch gửi hai lần.
- `verify.ps1`: **OK** — 650 unit test pass, Runtime/Editor/PlayMode/LiveSmoke đều compile, rule 200 dòng pass.
- Chưa chạy PlayMode thật và live teleport vì Unity Editor đang mở, batchmode không giành được project lock.

## Key Decisions (đã chốt với user 2026-09-12)

1. **Âm thanh**: thêm row "Âm thanh: Bật/Tắt" vào SettingsView (gọi `SoundManager.SetEnabled`). Không bỏ hẳn global toggle.
2. **Phân quyền map**: CHƯA CHỐT — tạm để nguyên hành vi hiện tại (server trả danh sách theo quyền player qua `MapTeleportHandler.RequestOptions()`). Client không filter thêm. Nếu sau này cần filter level → thêm sau.
3. **Minimap hiển thị**: **ảnh thu nhỏ của map thật** (render từ `JarMapLayout` — tile + object, không cần asset thumbnail mới) + **dot nhân vật di chuyển theo vị trí thật** (đọc `PlayerAvatar` position mỗi frame, map qua `WorldToJar` → tỉ lệ vào ô vuông).

## Key Insights (research 2026-09-12)

- **Ảnh minimap KHÔNG cần asset mới**: `JarMaps.Load(mapId)` trả `JarMapLayout` (WidthPixels/HeightPixels, Layers, Objects). Render tile + object vào `RenderTexture`/`Texture2D` một lần khi vào map → dùng làm sprite. Không cần `newMapData/{id}.png` (ảnh đó là ảnh dải tile, không phải ảnh map).
- **Dot nhân vật**: `PlayerAvatar` có `JarY` (public) nhưng KHÔNG có `JarX` public — cần thêm property `JarX` (đã có `_targetJarX` trong `SetTarget`/`SnapTo`). `MapPlacement.WorldToJar(worldX, worldY, mapHeightPixels)` chuyển world → jar. Tỉ lệ: `dot.x = jarX / map.WidthPixels * size`, `dot.y = (1 - jarY / map.HeightPixels) * size` (jar Y hướng xuống, UI Y hướng lên).
- **Map hiện tại**: `MapScene.MapId` (property) + `MapLoaded` event (GameSession.cs:262,279). `MapRenderer.Map` là `JarMapLayout` đang render.
- **Teleport**: dùng đúng `MapTeleportHandler.RequestOptions()` (gửi `MGO_COMMAND/TELE_MENU`) + `MapHandler.SendWarp(mapId, waypointIndex, 1)` — y hệt `GameSession.Teleport.cs:29`. Không tự chế protocol.
- **Neo góc trên-phải**: pattern `SoundToggleButton.cs:39-41` (anchor theo SizeFrac/MarginFrac, canvas CHUNG không phải PixelCanvas.Content) — minimap dùng cùng kỹ thuật.

## Files affected

| File | Thay đổi |
|------|----------|
| `GopetBootstrap.cs:93` | Bỏ/giấu SoundToggleButton, tạo MinimapWidget |
| `ShopServiceEventHud.cs:30` | Giảm `ReservedRightFrac` vì SoundToggleButton đã biến mất |
| `SettingsView.cs` | Thêm row "Âm thanh: Bật/Tắt" (global toggle) |
| `GameSession.cs` | Wire MinimapWidget, truyền MapId/MapName, cập nhật dot |
| `GameSession.Teleport.cs` | Mở MapPickerView thay vì ChoiceDialogView |
| `PlayerAvatar.cs` | Thêm property `JarX` (public, đọc `_targetJarX`) |
| `MinimapWidget.cs` (mới) | Ô vuông góc trên-phải, ảnh map + dot nhân vật, click mở picker |
| `MapPickerView.cs` (mới) | Panel danh sách map, click tên → teleport |

## Câu hỏi chưa rõ

1. **Phân quyền map**: Server đã filter danh sách map theo level/VIP chưa? (Tạm để nguyên, chưa chốt — user xác nhận sau)
2. **Kích thước minimap**: `SizeFrac = 0.07` (to hơn loa 0.05) — có vừa không? Có thể chỉnh sau khi xem thực tế.
3. **Dot nhân vật**: hiển thị dot trắng/đỏ đơn giản là đủ, hay cần mũi tên hướng nhân vật đang nhìn?
