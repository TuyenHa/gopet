# Phase 04 — WorldMapView: canvas pan, cụm khu vực, node thumbnail + ổ khoá

## Context Links

- [plan.md](plan.md) · [phase-03](phase-03-jar-map-thumbnail-shared-bake.md)
- `GopetUnityClient/Assets/Scripts/Runtime/UI/MapPickerView.cs` / `MapPickerView.Layout.cs` — mẫu popup + ổ khoá sẽ thay thế
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs:648` `CreateHudOverlayCanvas` (ref 960×540, match **chiều cao**)
- `GopetUnityClient/Assets/Scripts/UiLogic/MapDisplayNames.cs` — tên map 11…34
- `GopetUnityClient/Assets/Scripts/Runtime/UI/MailboxView.cs:40-60`, `GenericMenuView.cs:185-190` — mẫu `ScrollRect`
- `GopetUnityClient/Assets/Scripts/Runtime/UI/ToastView.cs` (61 dòng) — toast một dòng
- `GopetUnityClient/Assets/Scripts/Runtime/UI/UiBuilder.cs` — `Stretch`, `MakeText`, `BuiltinFont`, màu chuẩn

## Overview

- **Priority:** P1
- **Status:** pending · **Phụ thuộc:** phase 03
- Dựng màn bản đồ thế giới: nền kéo/pan hai chiều, các map gom theo cụm khu vực, mỗi map
  là một node thumbnail bake từ tile jar + nhãn tiếng Việt, map khoá phủ xám + ổ khoá.

## Key Insights

- Canvas HUD ref **960×540, `matchWidthOrHeight = 1`** → 540 ref-unit luôn là trọn chiều
  cao màn hình. Content 1920×1080 = đúng 2×2 màn hình, pan thấy rõ mà không lạc.
- Máy này KHÔNG có asset ảnh bản đồ thế giới; nền là chính thumbnail các map ghép lại
  theo cụm — quyết định đã chốt, không tạo asset mới.
- Bake là chi phí **CPU** (mỗi map là WidthTiles × HeightTiles × số layer lệnh
  `Graphics.DrawTexture`), không phải VRAM (24 map × 128² ARGB32 ≈ 1,5 MB). Vì thế:
  bake **lười theo node lọt viewport**, và bake rồi thì **giữ** tới lúc đóng màn — thả
  ra rồi bake lại khi pan ngược là tự tạo khựng.
- `ToastView` hiện là một dòng cao 44, `Text` không wrap (`ToastView.cs:31-39`). Câu lý do
  server gửi ("Hãy chăm chỉ làm nhiệm vụ để mở map này", 44 ký tự) sẽ tràn → phải nâng
  toast lên đa dòng ở phase này vì đây là phase sở hữu file đó.
- `Gopet.Runtime.asmdef` đã tham chiếu `Gopet.Net` → `WorldMapView.Bind` nhận thẳng
  `MapTeleportOption[]`, không cần bịa struct trung gian (DRY).

## Requirements

**Chức năng**
- Nền tối full-screen, panel = trọn màn, nút X góc phải-trên (`HudSkin.Get(HudSkin.Close)`).
- Kéo bằng ngón/chuột theo cả 2 trục, có quán tính, `MovementType.Elastic` để biết đã chạm biên.
- Mỗi cụm khu vực có nhãn tiêu đề; mỗi map có node: thumbnail + tên + (ổ khoá nếu khoá).
- Bấm node mở → `Chosen(mapId)`; bấm node khoá → `LockedChosen(reason)`, màn KHÔNG đóng.
- Map server gửi nhưng không có trong bảng toạ độ → xếp vào cụm "Khác" ở cuối, không rơi mất.
- Mở màn: tự pan tới node của map hiện tại và đánh dấu nó (viền sáng).

**Phi chức năng**
- Mỗi file `.cs` < 200 dòng.
- Mở màn ≤ 1 frame khựng: chỉ bake các node đang thấy.
- Thả toàn bộ `RenderTexture` khi `OnDestroy`.

## Architecture

```
WorldMapView (root, full-screen, nền mờ)
└─ Panel
   ├─ Title "Bản đồ thế giới"      ├─ Close (X)
   └─ Viewport (Mask + ScrollRect horizontal+vertical)
      └─ Content 1920×1080
         ├─ RegionLabel × N         (từ WorldMapLayout.Regions)
         └─ MapNode × M             (Button)
            ├─ Thumb (RawImage)  ← JarMapThumbnail.Bake(JarMaps.Load(id), 128)
            ├─ Name  (Text)
            └─ Lock  (Image, JarSkin.Raw("lock"))  — chỉ khi locked

Luồng dữ liệu:
MapTeleportOption[] ──Bind──> node (tên: option.Name, fallback MapDisplayNames.Get)
onValueChanged(scroll) ──> RefreshVisibleThumbnails() ──> bake node mới lọt viewport
click node  ──> locked ? LockedChosen(option.LockReason) : Chosen(option.MapId)
```

**Chia file để giữ < 200 dòng** (partial theo quy ước `X.cs` / `X.Layout.cs` sẵn có):

| File | Nội dung | Ước lượng |
|------|----------|-----------|
| `WorldMapView.cs` | hằng số, field, event, `Create`, `Bind`, `Choose`, `OnDestroy` | ~120 |
| `WorldMapView.Layout.cs` | dựng panel/scroll/content, `MakeRegionLabel`, `MakeNode`, `MakeLockIcon` | ~150 |
| `WorldMapView.Thumbnails.cs` | `RefreshVisibleThumbnails`, kiểm giao viewport, dict `RenderTexture`, thả | ~80 |
| `WorldMapLayout.cs` (UiLogic) | bảng `mapId → (regionId, x, y)` + danh sách cụm | ~90 |

## Related Code Files

**Tạo mới**
- `GopetUnityClient/Assets/Scripts/UiLogic/WorldMapLayout.cs`
- `GopetUnityClient/Assets/Scripts/Runtime/UI/WorldMapView.cs`
- `GopetUnityClient/Assets/Scripts/Runtime/UI/WorldMapView.Layout.cs`
- `GopetUnityClient/Assets/Scripts/Runtime/UI/WorldMapView.Thumbnails.cs`
- (`.meta` do Unity sinh khi import — commit kèm, như `MapPickerView.Layout.cs.meta`)

**Sửa**
- `GopetUnityClient/Assets/Scripts/Runtime/UI/ToastView.cs` — cho phép xuống dòng

**Xoá:** không có ở phase này (`MapPickerView` bị xoá ở phase 05).

## Implementation Steps

1. `WorldMapLayout.cs` (namespace `Gopet.UiLogic`, static):
   - `public readonly struct Node { int MapId; int RegionId; float X; float Y; }`
   - `public static readonly string[] RegionNames` — "Thành thị", "Rừng Linh", "Vùng núi",
     "Băng nguyên", "Thượng giới", "Khác".
   - `public static readonly Vector2[] RegionAnchors` — tâm mỗi cụm trong content 1920×1080:
     Thành thị (300,760), Rừng Linh (760,860), Vùng núi (1240,740), Băng nguyên (1580,420),
     Thượng giới (860,200), Khác (1620,120).
   - Bảng node: 11,22,19,20 → Thành thị; 12,13,14,15 → Rừng Linh; 16,17,18,21 → Vùng núi;
     23,24,25 → Băng nguyên; 26…34 → Thượng giới. Toạ độ = anchor cụm + offset lưới 2 cột,
     bước (160, -120). `public static Node Of(int mapId)` — không có → cụm "Khác", toạ độ
     xếp tuần tự theo thứ tự gọi (deterministic theo mapId).
   - Comment: bảng này là *bố cục hiển thị*, không phải dữ liệu game — server vẫn là
     nguồn sự thật cho danh sách map.
2. `WorldMapView.cs`: `public sealed partial class WorldMapView : MonoBehaviour`.
   Hằng: `ContentWidth = 1920f`, `ContentHeight = 1080f`, `NodeWidth = 132f`,
   `ThumbHeight = 84f`, `LabelHeight = 24f`, `LockSize = 26f`, `ThumbMaxPixels = 128`,
   `VisibleMargin = 160f`. Màu lấy từ `UiBuilder` + cùng bảng màu `MapPickerView` để
   nhất quán (nền panel `JarBackground`, node mở xanh, node khoá xám).
   Event: `Action<int> Chosen`, `Action<string> LockedChosen`, `Action CloseRequested`.
   `Create(Transform parent, Font font)` giống `MapPickerView.Create:62` (nền bấm ra
   ngoài → `CloseRequested`).
3. `Bind(MapTeleportOption[] options, int currentMapId)`:
   - xoá node cũ, dựng node mới theo `WorldMapLayout.Of(option.MapId)`
   - tên node: `string.IsNullOrEmpty(option.Name) ? MapDisplayNames.Get(id) : option.Name`
     (server là nguồn chính, `MapDisplayNames` chỉ là lưới đỡ)
   - node của `currentMapId`: thêm viền sáng + không gửi warp khi bấm (đang ở đó rồi)
   - cuộn `Content` sao cho node hiện tại nằm giữa viewport
   - gọi `RefreshVisibleThumbnails()`
4. `WorldMapView.Layout.cs`:
   - `Build()`: Panel (`UiBuilder.Stretch` + `RoundedUiSprite.Apply`), Title, Close
     (`HudSkin.Get(HudSkin.Close)` như `MapPickerView.Layout.cs`), Viewport có `Mask` +
     `ScrollRect { horizontal = true, vertical = true, movementType = Elastic,
     inertia = true, scrollSensitivity = 0 }`, Content `sizeDelta = (1920,1080)`,
     `pivot/anchor = (0,0)`.
   - `MakeRegionLabel(int regionId)` — `UiBuilder.MakeText` cỡ 18, màu `TextMuted`.
   - `MakeNode(...)` — Button + RawImage thumb + Text tên + `MakeLockIcon` (bê nguyên
     cách đặt của `MapPickerView.Layout.cs:146-158`, `JarSkin.Raw("lock")`).
   - Node khoá: `RawImage.color` xám hoá (`0.55,0.58,0.62`) để đọc được ổ khoá đè lên.
5. `WorldMapView.Thumbnails.cs`:
   - `Dictionary<int, RenderTexture> _baked`, `List<(RectTransform rect, int mapId, RawImage img)> _nodes`
   - `RefreshVisibleThumbnails()`: tính rect viewport trong không gian content
     (`-Content.anchoredPosition` + kích thước viewport, nới `VisibleMargin`), node nào
     giao nhau mà chưa bake → `JarMapThumbnail.Bake(JarMaps.Load(mapId), ThumbMaxPixels)`.
     Bọc `try/catch` → lỗi thì để nền phẳng + `Debug.LogWarning`, KHÔNG ném lên UI.
   - Gắn vào `ScrollRect.onValueChanged`.
   - `OnDestroy`: `foreach` thả bằng `JarMapThumbnail.Release`.
6. `ToastView.cs`: `label.horizontalOverflow = HorizontalWrapMode.Wrap;`
   `label.verticalOverflow = VerticalWrapMode.Overflow;` và sau khi set text:
   `rect.sizeDelta = new Vector2(360f, Mathf.Max(44f, label.preferredHeight + 16f));`
   Cập nhật comment XML (đang ghi "Toast một dòng").
7. `pwsh GopetUnityClient/verify.ps1` — kiểm compile + rule 200 dòng.

## Todo List

- [ ] 1. `WorldMapLayout.cs` (cụm + toạ độ + fallback "Khác")
- [ ] 2. `WorldMapView.cs` (state, `Create`, event)
- [ ] 3. `Bind` + auto-pan tới map hiện tại
- [ ] 4. `WorldMapView.Layout.cs` (panel, scroll, node, ổ khoá)
- [ ] 5. `WorldMapView.Thumbnails.cs` (bake lười + thả)
- [ ] 6. `ToastView` đa dòng
- [ ] 7. `verify.ps1` xanh

## Success Criteria

- `pwsh GopetUnityClient/verify.ps1` → xanh, đặc biệt bước 10/10 (4 file mới đều < 200 dòng,
  KHÔNG thêm tên nào vào `CODE_HEALTH_EXCEPTIONS.md`).
- PlayMode test (viết ở phase 06) dựng `WorldMapView`, `Bind` 24 option:
  đếm đúng 24 node, node khoá có con `Lock`, bấm node khoá không phát `Chosen`.
- Chạy game thật: kéo mượt hai chiều; thumbnail hiện dần khi pan; đóng màn → không còn
  `RenderTexture` rò (kiểm bằng Profiler → Memory → Render Textures trở về mức trước khi mở).

## Risk Assessment

| Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|--------|----------|-----------|-----------|
| Bake nhiều map một lúc gây khựng frame | Cao | Cao | Bake lười theo viewport; `ThumbMaxPixels = 128`; nếu vẫn khựng → rải bake qua coroutine 1 map/frame |
| Rò `RenderTexture` khi đóng/mở lại nhiều lần | Trung bình | Cao | Dict một chủ sở hữu + `OnDestroy` thả hết; test đóng-mở 3 lần |
| Toast đa dòng làm lệch các toast khác trong game | Trung bình | Thấp | Chỉ *nới* chiều cao theo nội dung, câu ngắn giữ nguyên 44 |
| Node đè lên nhau khi map lạ rơi vào cụm "Khác" | Thấp | Thấp | Xếp lưới tuần tự theo mapId, không dùng toạ độ tự do |
| Panel trọn màn che HUD/chat khi đang mở | Thấp | Thấp | Đặt trên `_hudParent` (sortOrder 35) giống `MapPickerView`, đóng bằng X hoặc bấm nền |

**Rollback:** phase này chưa có ai gọi `WorldMapView` (wiring ở phase 05) → revert an toàn,
trừ `ToastView.cs` (thay đổi độc lập, giữ được).

## Security Considerations

- `option.Name` và `option.LockReason` là chuỗi từ server, hiển thị bằng `UnityEngine.UI.Text`
  không bật rich text → không có đường chèn markup/thực thi.
- `Bind` phải chịu được `MapId` ngoài 11…34 (server có thể thêm map): rơi vào cụm "Khác",
  `JarMaps.Load` ném → `catch` ở bước 5 giữ node trống thay vì sập màn hình.
- Không tin cờ `Locked` để chặn quyền — server đã chốt ở phase 01; client chỉ dừng UI.

## Next Steps

- Phase 05 nối `WorldMapView` vào `GameSession` và gỡ `MapPickerView`.
