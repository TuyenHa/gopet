# Màn hình bản đồ thế giới

Một MÀN HÌNH RIÊNG phủ kín (không phải popup nổi trên cảnh chơi), thay cho popup lưới
`MapPickerView` cũ: tranh nền vẽ sẵn, mây trôi, mỗi map là một pin + tên đặt đúng vùng địa
hình của nó. Map chưa mở đổi pin thành ổ khoá và báo lý do do **server** gửi xuống.

View tự dựng `Canvas` riêng (`overrideSorting`, `sortingOrder = 60`) nên nằm trên toàn bộ
HUD; tranh nền phủ kín khiến cảnh chơi phía sau khuất hẳn.

## Luồng tổng thể

```
Lối vào (3 chỗ) ──> MapTeleportHandler.RequestOptions()  ──MGO_COMMAND/TELE_MENU──> server
                                                                                      │
   WorldMapView.Bind(options, currentMapId) <── OptionsReceived <── MapTeleportOption[]
              │
              ├─ bấm map mở   ──> Chosen(mapId)      ──> SendWarp + fade + đóng màn
              └─ bấm map khoá ──> LockedChosen(lý do) ──> toast, màn KHÔNG đóng
```

Ba lối vào (`GameSession`):

| Lối vào | Nơi nối |
|---------|---------|
| Menu nhân vật → Dịch chuyển | `GameSession.cs`, `CharacterMenuAction.Teleport` |
| Nút minimap góc phải-trên | `GameSession.cs`, `MinimapWidget.Clicked` |
| NPC phòng vé | `GameSession.Interactions.cs`, `LocalMenu.TicketRoom` |

## Wire-format TELE_MENU (6 trường / map)

`MGO_COMMAND` + `TELE_MENU` + `count (sbyte)`, rồi mỗi map:

| # | Kiểu | Trường | Ghi chú |
|---|------|--------|---------|
| 1 | sbyte | mapId | **≤ 127** — protocol dùng sbyte |
| 2 | UTF | name | tên hiển thị |
| 3 | UTF | description | hiện bằng name |
| 4 | sbyte | waypointIndex | |
| 5 | sbyte | locked | 1 = khoá |
| 6 | UTF | lockReason | rỗng khi map mở |

Trường 6 là phần mới. Client đọc đủ 6 trường rồi `ExpectFullyConsumed`, nên **server cũ +
client mới sẽ ném ngay**: phải **deploy server trước, client sau**. Hợp đồng này được khoá
bằng test `MapTeleportHandlerTests.Response_RejectsLegacyEntryWithoutLockReason`.

## Luật khoá map

Một nguồn sự thật duy nhất: `GameController.MapLockReason(mapId)` — dùng chung cho TELE_MENU,
`ON_PLAYER_WARPING` và đổi kênh (qua `CheckMapAccess`). Client **không** tự quyết map nào mở;
cờ khoá chỉ để vẽ giao diện.

Thứ tự kiểm:
1. `IsSkyLocked(mapId)` — map ≥ 26 khi chưa mở thượng giới → `Language.LawToUnlockSkyPlace`.
2. `MapUnlockRules.TryGetRequiredTask(mapId, out taskId)` và `playerData.wasTask` chưa chứa
   `taskId` → `Language.TaskLockedMapHint` ("Hãy chăm chỉ làm nhiệm vụ để mở map này").
3. Còn lại → chuỗi rỗng = vào được.

### Thêm một luật khoá theo nhiệm vụ

Sửa `SRCGOPETGOC/GServer/Manager/MapUnlockRules.cs`, thêm một dòng vào `Required`:

```csharp
private static readonly Dictionary<int, int> Required = new()
{
    { 21, 105 },   // Thạch Động chỉ mở khi đã xong nhiệm vụ 105
};
```

`105` là `taskTemplateId` — đúng giá trị được ghi vào `playerData.wasTask` khi hoàn thành
(xem `TaskCalculator`). Bảng nằm trong code, không phải DB: luật đổi vài lần mỗi năm nên
migration + loader + rollback DB đắt hơn một lần build lại server.

**Bảng đang rỗng** — hành vi hiện tại giữ nguyên cho tới khi chốt danh sách map ↔ nhiệm vụ.
Điền xong phải thử trên tài khoản test: điền nhầm id sẽ khoá map mà người chơi đang đứng trong.

## Bố cục bản đồ

`Assets/Scripts/UiLogic/WorldMapLayout.cs` — toạ độ theo **tỉ lệ 0..1 của tranh nền**
(0,0 = góc dưới-trái), 6 cụm khu vực đặt trùng vùng địa hình tương ứng:

| Cụm | Map | Vùng trên tranh |
|-----|-----|-----------------|
| Thành thị | 11, 22, 19, 20 | thành có tường bao, dưới-trái |
| Rừng Linh | 12, 13, 14, 15 | rừng cây cổ thụ, giữa-trái |
| Vùng núi | 16, 17, 18, 21 | núi đá nâu, bên phải |
| Băng nguyên | 23, 24, 25 | băng hà, trên-phải |
| Thượng giới | 26 … 34 | đảo bay, trên-giữa |
| Khác | mọi map không khai báo ở trên | mép dưới |

Đây là dữ liệu **trình bày**, không phải dữ liệu game — danh sách map vẫn do server chốt.
File này không được dùng kiểu `UnityEngine`: thư mục `UiLogic` còn compile dưới
netstandard2.1 không có UnityEngine (verify bước 4/10).

## Tranh nền và mây

Asset sinh bằng `tools/image-gen` (gpt-image), đặt ở `Assets/Resources/Ui/WorldMap/`:

| File | Vai trò |
|------|---------|
| `world-map-background.png` | Tranh toàn cảnh 1536×1024, pixel-art + ánh sáng điện ảnh |
| `cloud-1/2/3.png` | 3 cụm mây rời, nền trong suốt, đã cắt viền mờ |

- Tranh phủ kín màn kiểu **envelope** (giữ tỉ lệ, tràn ra và bị cắt) — kéo giãn cho vừa thì
  cảnh méo. Pin là con của tranh, neo theo **tỉ lệ 0..1 của tranh**, nên vẫn dính đúng chỗ dù
  tranh bị cắt ở máy tỉ lệ khác.
- Mây: 6 cụm lấy ngẫu nhiên từ 3 sprite, mỗi cụm một cao độ / cỡ / tốc độ riêng, trôi ngang
  rồi vòng lại. Mây to = ở gần = trôi nhanh và mờ hơn, tạo chiều sâu. Trôi theo
  `Time.unscaledDeltaTime`, **không** theo frame.
- Mây nằm dưới lớp pin và `raycastTarget = false` nên không che tên map, không nuốt cú chạm.
- Vị trí/cỡ mây giữ theo tỉ lệ và quy ra pixel mỗi frame: lúc dựng, `rect.size` còn bằng 0
  (chưa qua layout) — chốt pixel ngay lúc đó thì mây đứng im ở góc.

`JarMapThumbnail` (bake map jar ra `RenderTexture`) vẫn còn nhưng nay **chỉ minimap dùng**;
màn bản đồ không bake gì nên không có đường rò VRAM.

## Giới hạn đã biết

- `mapId` truyền bằng `sbyte` → tối đa 127. Map cao nhất hiện tại là 34.
- Pin đặt tay theo tranh: **đổi tranh nền thì phải chỉnh lại bảng toạ độ** trong
  `WorldMapLayout`, nếu không pin rơi lung tung. Cụm Thượng giới có 9 map nên dùng bước dòng
  riêng (`RegionStepY`), xếp thưa hơn là dòng cuối đè lên cụm Rừng Linh.
- Nhãn cụm đặt tay (`LabelX`/`LabelY`), không suy từ ô đầu: cụm sát mép trên mà cộng thêm
  một bước là nhãn văng ra ngoài màn hoặc đâm vào tiêu đề.
- Chưa có zoom, cũng không cần pan: cả bản đồ nằm gọn trong một màn.
- Minimap neo dưới hàng icon Cửa hàng/Dịch vụ/Sự kiện (`MinimapWidget.TopRowFrac`) — đổi
  `ShopServiceEventHud.SizeFrac` thì phải chỉnh theo, `MinimapHudLayoutTests` sẽ báo đỏ.

## File liên quan

| Vai trò | File |
|---------|------|
| Màn hình | `Assets/Scripts/Runtime/UI/WorldMapView{,.Layout,.Nodes,.Clouds}.cs` |
| Bố cục | `Assets/Scripts/UiLogic/WorldMapLayout.cs` |
| Tranh nền, mây | `Assets/Resources/Ui/WorldMap/` (sinh bằng `tools/image-gen`) |
| Bake ảnh minimap | `Assets/Scripts/Runtime/UI/JarMapThumbnail.cs` |
| Minimap | `Assets/Scripts/Runtime/UI/MinimapWidget.cs` |
| Nối luồng | `Assets/Scripts/Runtime/World/GameSession.Teleport.cs` |
| Protocol | `Assets/Scripts/Net/Map/MapTeleportHandler.cs` |
| Luật khoá | `SRCGOPETGOC/GServer/Manager/MapUnlockRules.cs`, `GameController.MapLockReason` |
