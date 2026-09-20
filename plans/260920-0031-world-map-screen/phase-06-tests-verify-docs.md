# Phase 06 — Tests, verify và tài liệu

## Context Links

- [plan.md](plan.md) · [phase-04](phase-04-world-map-view-ui.md) · [phase-05](phase-05-gamesession-wiring-entrypoints.md)
- `GopetUnityClient/verify.ps1` — 10 bước, bước 10/10 chặn file > 200 dòng
- `GopetUnityClient/run-playmode-tests.ps1`
- `GopetUnityClient/Assets/Tests/PlayMode/MinimapPickerTests.cs` (đã đổi tên ở phase 05)
- `GopetUnityClient/Assets/Tests/PlayMode/AllMapRenderingTests.cs` — mẫu duyệt map 11…34
- `D:/game/docs/` — hiện chỉ có `battle-system.md` và `journals/`

## Overview

- **Priority:** P2
- **Status:** pending · **Phụ thuộc:** phase 05
- Khoá hành vi mới bằng test tự động, chạy trọn bộ verify, và ghi lại tài liệu + giới hạn
  đã biết (đặc biệt: cách điền bảng `MapUnlockRules`).

## Key Insights

- Repo đã có 3 tầng test: PlayMode (UI/Unity), `Gopet.Net.Tests` (parse gói, xUnit),
  `Gopet.Net.LiveSmoke` (server thật). Phase 02 đã phủ tầng 2 và 3 cho protocol → phase
  này chỉ còn tầng 1 (UI) + kiểm tổng thể.
- `AllMapRenderingTests` chứng minh mọi map 11…34 có tile để render → test bake thumbnail
  cho toàn dải là hợp lệ, không cần bỏ qua map nào.
- Không có `docs/development-roadmap.md` / `project-changelog.md` trong repo này → không
  bịa file mới ngoài phạm vi; chỉ thêm đúng một trang mô tả tính năng.

## Requirements

**Chức năng (test)**
- `WorldMapView` dựng đúng số node, node khoá có ổ khoá và không phát `Chosen`.
- Bấm node khoá phát `LockedChosen` với đúng chuỗi lý do.
- Thumbnail được bake cho node trong viewport; đóng màn thả hết `RenderTexture`.
- Minimap trên HUD không chồng lên `CharacterMenuButton` / `CurrencyBar`.
- `ToastView` nhiều dòng cao hơn toast một dòng.

**Phi chức năng**
- File test < 200 dòng (bước 10/10 quét cả `Assets/Tests`).
- Không dùng dữ liệu giả: test nạp map jar thật qua `JarMaps.Load`.

## Architecture

```
Tầng 1 PlayMode   : WorldMapViewTests, WorldMapThumbnailTests, MinimapHudLayoutTests, ToastViewTests
Tầng 2 Gopet.Net  : MapTeleportHandlerTests            (đã cập nhật ở phase 02)
Tầng 3 LiveSmoke  : TeleportMenuChecks                 (đã cập nhật ở phase 02)
Cổng phát hành    : verify.ps1 (10 bước) + run-playmode-tests.ps1
```

## Related Code Files

**Tạo mới**
- `GopetUnityClient/Assets/Tests/PlayMode/WorldMapViewTests.cs` (+ `.meta`)
- `GopetUnityClient/Assets/Tests/PlayMode/WorldMapThumbnailTests.cs` (+ `.meta`)
- `GopetUnityClient/Assets/Tests/PlayMode/MinimapHudLayoutTests.cs` (+ `.meta`)
- `D:/game/docs/world-map-screen.md`

**Sửa**
- `GopetUnityClient/Assets/Tests/PlayMode/MinimapWidgetTests.cs` — bổ sung test toast đa dòng
  (hoặc tách `ToastViewTests.cs` nếu vượt 200 dòng)

**Xoá:** không có.

## Implementation Steps

1. `WorldMapViewTests.cs` (NUnit `[Test]`, dựng root `GameObject` có `Canvas` như
   `MinimapPickerTests.SetUp`):
   - `WorldMap_DungDuNode_ChoMoiMapServerGui`: `Bind` 24 `MapTeleportOption` (11…34) →
     đếm Button node = 24.
   - `WorldMap_MapKhoa_HienOKhoaVaKhongChon`: 1 option `Locked = true`,
     `LockReason = "Hãy chăm chỉ làm nhiệm vụ để mở map này"` → bấm: `Chosen` không phát,
     `LockedChosen` nhận đúng chuỗi, node có con `Lock`.
   - `WorldMap_MapMo_PhatMapIdChuKhongPhaiIndex`: bấm node map 22 → `Chosen == 22`.
   - `WorldMap_MapLa_KhongLamSapMan`: option `MapId = 99` → vẫn dựng node (cụm "Khác"),
     không ném.
   - `WorldMap_NutDong_PhatSuKienDong`: bấm `Panel/Close` → `CloseRequested`.
2. `WorldMapThumbnailTests.cs` (`[UnityTest]`):
   - `Thumbnail_BakeMapThat`: `JarMapThumbnail.Bake(JarMaps.Load(11), 128)` → texture
     khác null, cạnh lớn nhất ≤ 128, đúng tỉ lệ map.
   - `WorldMap_DongMan_ThaHetRenderTexture`: đếm
     `Resources.FindObjectsOfTypeAll<RenderTexture>().Length` trước khi mở và sau khi
     `DestroyImmediate` → bằng nhau (đây là test bắt rò VRAM, đừng bỏ).
3. `MinimapHudLayoutTests.cs` (`[UnityTest]`): dựng `MinimapWidget` + `CharacterMenuButton`
   + `CurrencyBar` trên cùng một canvas ref 960×540, so `RectTransform` world corners →
   khẳng định không giao nhau. Đây là test duy nhất chặn lỗi "minimap đè HUD" tái diễn.
4. Bổ sung test toast: `Toast_CauDai_CaoHonMotDong` — `ToastView.Create` với câu 44 ký tự
   → `sizeDelta.y > 44f`.
5. Chạy `pwsh GopetUnityClient/run-playmode-tests.ps1` đến khi xanh sạch.
6. Chạy `pwsh GopetUnityClient/verify.ps1` (bao gồm `dotnet test` + build LiveSmoke + rule
   200 dòng). **Không** thêm file mới nào vào `CODE_HEALTH_EXCEPTIONS.md`.
7. Build server: `dotnet build SRCGOPETGOC/GServer/Gopet.csproj -c Debug --nologo`.
8. Chạy LiveSmoke với server phase 01 đang chạy: các check `NN/OO/OO2/PP/QQ/RR` xanh.
9. Viết `docs/world-map-screen.md`: wire-format TELE_MENU mới (6 trường), quy tắc khoá
   map + **cách thêm một dòng vào `MapUnlockRules`**, bố cục cụm khu vực, 3 lối vào,
   giới hạn đã biết (`mapId` là `sbyte` nên ≤ 127; thumbnail 128px; cần deploy server
   trước client).
10. Ghi nhật ký vào `docs/journals/` theo mẫu có sẵn nếu gặp sự cố đáng nhớ.

## Todo List

- [ ] 1. `WorldMapViewTests.cs` (5 test)
- [ ] 2. `WorldMapThumbnailTests.cs` (bake + rò RenderTexture)
- [ ] 3. `MinimapHudLayoutTests.cs` (không chồng lấn)
- [ ] 4. Test toast đa dòng
- [ ] 5. `run-playmode-tests.ps1` xanh
- [ ] 6. `verify.ps1` xanh
- [ ] 7. Build server sạch
- [ ] 8. LiveSmoke xanh
- [ ] 9. `docs/world-map-screen.md`
- [ ] 10. Nhật ký (nếu cần)

## Success Criteria

- `pwsh GopetUnityClient/run-playmode-tests.ps1` → 0 fail.
- `pwsh GopetUnityClient/verify.ps1` → "VERIFY OK" (10/10).
- `dotnet build SRCGOPETGOC/GServer/Gopet.csproj -c Debug` → 0 error.
- LiveSmoke: toàn bộ check TELE_MENU xanh với server thật.
- `docs/world-map-screen.md` tồn tại và trả lời được: cách thêm một map cần nhiệm vụ,
  và thứ tự deploy.

## Risk Assessment

| Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|--------|----------|-----------|-----------|
| Test đếm `RenderTexture` toàn cục chập chờn (Unity giữ RT nội bộ) | Trung bình | Thấp | Đếm *delta* quanh vòng mở/đóng, không đếm tuyệt đối; nếu vẫn chập chờn → kiểm dict của view thay vì toàn cục |
| Test layout minimap phụ thuộc độ phân giải chạy test | Trung bình | Thấp | Ép `CanvasScaler` ref 960×540 `matchWidthOrHeight = 1` ngay trong `SetUp` |
| File test vượt 200 dòng | Trung bình | Thấp | Tách theo chủ đề như đã chia ở bước 1-3 |
| LiveSmoke không chạy được vì thiếu server | Trung bình | Trung bình | Cần `GOPET_DB_PASSWORD` + MariaDB Docker; nếu không dựng được thì ghi rõ là bước kiểm tay chưa làm, KHÔNG đánh dấu done |

**Rollback:** phase chỉ thêm test + docs → revert không ảnh hưởng runtime.

## Security Considerations

- Test không được nhúng mật khẩu/DB credential; LiveSmoke lấy từ biến môi trường như hiện tại.
- `docs/world-map-screen.md` mô tả wire-format nhưng không chứa thông tin kết nối máy chủ.
- Ghi rõ trong docs: cờ `Locked` ở client chỉ là hiển thị; chốt chặn thật ở
  `GameController.CheckMapAccess` — để người sau không gỡ nhầm.

## Next Steps

- Điền bảng `MapUnlockRules` sau khi có sign-off danh sách map ↔ nhiệm vụ (câu hỏi #1).
- Cân nhắc zoom (câu hỏi #2) như một phase riêng nếu người chơi phản hồi khó nhìn.
