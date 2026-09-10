---
phase: 6
title: "Map Rendering & Movement"
status: in-progress
priority: P1
effort: "3-4w"
dependencies: [5]
---

# Phase 6: Map Rendering & Movement

## Overview

Parse format `.dat` → Unity Tilemap, hiện nhân vật, đi lại, thấy người chơi khác, chat.

**Hết phase này là đạt mốc vertical slice:** login → vào map → đi lại → chat → mở menu. Đủ chứng minh toàn bộ kiến trúc chạy thật.

Đây cũng là phase reverse engineering thật sự đầu tiên — format `.dat` là thứ **chỉ có ở client**, server không mô tả. Bản decompile là nguồn duy nhất.

### Luồng trước khi vào map — tạo nhân vật thuộc P3, không thuộc phase này

P6 **chỉ nhận** phiên đã `LOGIN_SUCCES`, tức tài khoản **đã có nhân vật**. Việc "chưa có nhân vật thì tạo" nằm ở [`phase-03-login-handshake.md`](phase-03-login-handshake.md):

- Login mà tài khoản chưa có nhân vật → server trả opcode **21** `CREATE_CHAR`, **không** phải opcode 3 `LOGIN_SUCCES`.
- Client đáp lại gói `CREATE_CHAR`: `UTF name` + `sbyte gender`. Tên khớp `^[a-z0-9]+$`, 5-20 ký tự.
- Game gốc **1 tài khoản = 1 nhân vật** → chỉ có màn **tạo** nhân vật (đặt tên + chọn giới tính), **không có** màn chọn giữa nhiều slot. Đừng dựng UI chọn nhân vật ở phase này.
- Tạo xong **server đóng kết nối ngay** (`GameController.cs:740`) → client phải tự kết nối lại và login lần nữa mới có `LOGIN_SUCCES`.

Luồng đầy đủ:

```
login → (chưa có nhân vật) tạo nhân vật [P3] → server đóng kết nối
      → kết nối lại (chờ ReconnectCooldownMs) → login lại → LOGIN_SUCCES
      → INIT_PLAYER + ON_PLAYER_ENTER_MAP → vào map, hiện nhân vật [P6]
```

> Nhân vật "hiện" ở P6 là avatar trong map dựng từ `INIT_PLAYER` (31) và `ON_PLAYER_ENTER_MAP` (24) — dữ liệu server gửi, không phải do người chơi chọn ở đây.

## Requirements

**Functional**
- Parse `maps/*.dat` → Tilemap nhiều layer
- Hiện nhân vật mình + người chơi khác + NPC + quái
- Đi lại theo waypoint, đồng bộ với server
- Chat khu vực

**Non-functional**
- Map lớn không tụt frame trên mobile
- Chuyển map không giật

## Architecture

### Format `.dat` — ĐÃ GIẢI VÀ ĐÃ CÀI ĐẶT Ở P5.1

**Không phải ẩn số nữa.** P5.1 dựng nền màn đăng nhập bằng chính `maps/11.dat`, nên
parser đã có thật, chạy trên dữ liệu thật, có test: `Assets/Scripts/UiLogic/JarMapLayout.cs`
(thuần C#, không `UnityEngine`) + `tests/Gopet.Net.Tests/JarMapLayoutTests.cs`.

Format thật (theo `ef.java:104-190`, đã kiểm chứng):

```
byte    imageCount           <- số ảnh DẢI dùng cho lớp nền
byte    extraCount           <- phần còn lại: tài nguyên của vật thể
lặp (imageCount + extraCount) lần:
  short   resourceId
  byte    resourceType       <- 1 = hoạt ảnh, 2 = ảnh dải ô nền
byte    widthInTiles
byte    heightInTiles
byte    layerCount
lặp layerCount lần:
  lặp heightInTiles lần:
    byte[widthInTiles]       <- ô nền ĐÃ NÉN, xem dưới
lặp heightInTiles lần:
  byte[widthInTiles]         <- LỚP VA CHẠM
int     objectCount
lặp objectCount lần:
  byte    resourceIndex      <- chỉ số trong bảng tài nguyên, KHÔNG phải resourceId
  short   x
  short   y
  sbyte   yOffset            <- CÓ DẤU
  nếu resourceType[resourceIndex] == 1: byte[5]   <- khung cắt + id hoạt ảnh
(đuôi: eg = NPC/cổng dịch chuyển, z = điểm mốc — CHƯA đọc, việc của phase này)
```

**Ba chỗ plan gốc ghi sai, đã trả giá ở P5.1:**

1. **Ô nền nén trong MỘT byte**, không phải chỉ số tile trần: 4 bit cao = chỉ số ảnh
   dải, 4 bit thấp = ô thứ mấy trong dải **cộng một** (`0` = ô trống, không vẽ).
   `JarMapLayout.StripOf/CellOf` đã cài đúng.
2. **Có lớp va chạm** ngay sau các lớp nền — plan gốc không nhắc. Bỏ qua không đọc thì
   phần vật thể ngay sau đó lệch con trỏ, sai toàn bộ.
3. **`yOffset` có dấu.** Map 11 có vật thể `-14`; đọc không dấu thì nó tụt 270px ra
   ngoài map. Test đột biến đã bắt lỗi này.

Tile **24×24 px**. Kích thước pixel = `widthInTiles * 24` × `heightInTiles * 24`.

### Asset map — ĐÃ COPY TỪ JAR SANG REPO Ở P5.1

Không cần copy lại, không cần tải từ server:

| Nguồn | Nội dung | Trạng thái |
|---|---|---|
| `client.jar/maps/*.dat` | **24 map** — dữ liệu tile chính | ✅ đã copy → `Assets/Resources/Jar/Maps/*.bytes` (đủ 24) |
| `client.jar/newMapData/*.png` | **256 ảnh tile** (`N.png`, `N_a.png`) | ✅ đã copy → `Assets/Resources/Jar/Art/Raw/newMapData/` (đủ 256) |
| `client.jar/newMapData/*_b` | **9 file nhị phân nhỏ** (74-812 byte), metadata theo map, chưa rõ nội dung | ⬜ chưa giải |
| `GServer/assets/maps/` | 44 file phía server | ⬜ chưa đối chiếu với bản jar |

> **Phải đổi đuôi `.dat` → `.bytes`.** Unity không sinh `TextAsset` cho `.dat`,
> `Resources.Load` trả `null` dù file nằm đúng chỗ. `tools/unpack-jar-dat` đã làm.

**Hệ quả: tile KHÔNG đi qua `RemoteAssetCache` (P4).** Plan gốc giả định phải xin ảnh
tile từ server theo `tileImageId`; thực tế 256 ảnh đã nằm trong `Resources`. Dùng bản
local — nhanh hơn, không phụ thuộc mạng, và đã có `JarMapBackground` chứng minh chạy
được. `RemoteAssetCache` vẫn dùng cho **avatar/pet/item** (thứ server sinh động theo
người chơi), không dùng cho tile map tĩnh.

### Đưa sang Unity

Format này ánh xạ gần như 1:1 sang `Tilemap`:
- Mỗi layer `.dat` → 1 `Tilemap` component, `sortingOrder` tăng dần
- `tileImageId` → `Tile` asset dựng lúc chạy từ texture tải về
- Grid cell size = 24×24 px, `pixelsPerUnit` khớp

### Opcode di chuyển

| Opcode | Tên | Ghi chú |
|---|---|---|
| 24 | `ON_PLAYER_ENTER_MAP` | S→C: vào map |
| 27 | `ON_OTHER_USER_MOVE` | 2 chiều: di chuyển |
| 29 | `ON_UPDATE_PLAYER_IN_MAP` | S→C |
| 30 | `ON_PLAYER_EXIT_PLACE` | S→C |
| 31 | `INIT_PLAYER` | S→C: khởi tạo nhân vật |
| 25 | `ON_PLAYER_WARPING` | dịch chuyển |
| 9 | `ON_PLACE_CHAT` | chat khu vực |

### `ON_OTHER_USER_MOVE` (27) — từ `GameController.cs:210-241`

Client gửi:
```
int    userId
sbyte  direction
int    ?               <- b2, đọc nhưng server không dùng
int    pointCount
int[pointCount]        <- toạ độ, cặp (x,y) nối tiếp
```

Server lấy `points[len-2]`, `points[len-1]` làm vị trí mới, broadcast cho người khác.

> **Lưu ý quan trọng:** vị trí hoàn toàn do client quyết định — server không kiểm tra gì (khối chống hack ở `GameController.cs:227-241` đã bị comment). P1 đã vá phần giới hạn độ dài mảng nhưng **không** bật validate vị trí (vì đó là đổi hành vi, ngoài phạm vi "chỉ vá bảo mật"). Ghi nhận lại: cần bật validate server-side **trước khi phát hành**, xem P8.

### Chat khu vực có lệnh đặc biệt

`GameController.cs:244-274` — chuỗi chat `"kiss"`, `"play"`, `"poke"` được xử lý thành animation tương tác, không phải chat thường. Asset tương ứng nằm ở `pet/petInteract/`.

## Related Code Files

**Extend (đã có, KHÔNG tạo bản sao)**
- `Assets/Scripts/UiLogic/JarMapLayout.cs` — parser `.dat`. Thêm: phơi lớp va chạm, đọc đuôi `eg`/`z`
- `Assets/Scripts/Runtime/UI/JarMapBackground.cs` — tham khảo cách cắt sprite 24×24 + cache tĩnh
- `Assets/Scripts/Runtime/UI/JarMaps.cs` — nạp `Resources/Jar/Maps/*.bytes`

**Create**
- `Assets/Scripts/World/MapRenderer.cs` — dựng Tilemap từ `JarMapLayout`
- `Assets/Scripts/World/TileAssetProvider.cs` — (strip, cell) → Tile, nguồn local jar
- `Assets/Scripts/World/PlayerAvatar.cs` — nhân vật mình
- `Assets/Scripts/World/RemotePlayerAvatar.cs` — người chơi khác
- `Assets/Scripts/World/MovementController.cs` — input → waypoint → gửi gói
- `Assets/Scripts/Net/Handlers/MapHandler.cs`
- `Assets/Scripts/Net/Handlers/ChatHandler.cs`

**Read for context**
- `client.jar_Decompiler.com/ef.java:104-190` — **format `.dat`, nguồn gốc**
- `tests/Gopet.Net.Tests/JarMapLayoutTests.cs` — test đang xanh, đừng làm đỏ
- `client.jar_Decompiler.com/a.java` — logic map/render (32 KB, đọc chọn lọc)
- `SRCGOPETGOC/GServer/Server/GameController.cs:210-274` — move + chat
- `SRCGOPETGOC/GServer/Place/GopetPlace.cs` — logic khu vực server
- `SRCGOPETGOC/GServer/Data/map/{GopetMap,MapTemplate,Waypoint}.cs`

## Implementation Steps

1. ~~Viết `dat-inspect` CLI trước~~ — **đã xong ở P5.1.** `JarMapLayout.Parse` giải đúng
   `maps/11.bytes` thật và `JarMapBackground` dựng ra đúng khung cảnh trong ảnh chụp bản
   jar (cây thông, nhà GYM/MAGIC, máy TAE). Không viết lại parser, **mở rộng** cái đang có.

2. **Chạy `JarMapLayout.Parse` trên cả 24 map** — viết test tham số hoá lặp qua
   `Resources/Jar/Maps/*.bytes`, khẳng định không ném và kích thước hợp lý. Map nào lỗi →
   format có biến thể, đọc lại `ef.java`. Đây là bước đầu tiên thật sự của phase, rẻ và
   bắt lỗi sớm.

   2b. **Đọc nốt phần đuôi** — `eg` (NPC/cổng dịch chuyển) và `z` (điểm mốc) hiện
   `JarMapLayout` bỏ qua (`JarMapLayout.cs:124`). Phase này cần cả hai để đặt NPC và xử lý
   chuyển map.

3. **Làm rõ 9 file `newMapData/*_b`** — đây là metadata hoạt ảnh của atlas `<id>_a.png`, đọc bởi `dy.java`: danh sách vùng cắt, frame ghép nhiều phần, transform và clip/duration. Không liên quan cache `mapDynamicData_<mapId>`. Renderer phải đọc `_b`; vẽ nguyên `_a.png` sẽ làm mọi frame bung/chồng lên map.

4. ~~`MapDatParser`~~ — **dùng `JarMapLayout` sẵn có**, chỉ bổ sung phần đuôi (bước 2b) và
   phơi lớp va chạm ra ngoài (hiện đọc rồi vứt, `JarMapLayout.cs:96-98`). **Không tạo file
   parser thứ hai** — vi phạm DRY và sẽ lệch nhau ngay lần sửa format đầu tiên.

5. **`TileAssetProvider`** — nạp ảnh dải từ `Resources/Jar/Art/Raw/newMapData/`, cắt
   `Tile` 24×24 theo `StripOf`/`CellOf`. **Cache tĩnh**, không cache cục bộ: `Sprite.Create`
   sinh đối tượng mới mỗi lần gọi, cache theo màn nghĩa là mỗi lần chuyển map lại bỏ lại
   vài chục sprite không ai thu hồi (bài học `JarMapBackground` ở P5.1).

6. **`MapRenderer`** — mỗi layer 1 `Tilemap`, `sortingOrder` theo thứ tự layer. Camera pixel-perfect, `FilterMode.Point`.

7. **`INIT_PLAYER` + `ON_PLAYER_ENTER_MAP`** — parse, dựng nhân vật, đặt camera. Điều kiện vào bước này là phiên đã `LOGIN_SUCCES`; nhánh `CREATE_CHAR` (opcode 21) đã xử lý xong ở P3, không lặp lại ở đây.

8. **`MovementController` — GIỐNG JAR, không click-to-move**
   - Input LIÊN TỤC (WASD/mũi tên/joystick) → walk mỗi frame theo `WalkSpeed` (mặc định 96 px/s)
   - `PathSampler` (thuần C#, `UiLogic/PathSampler.cs`) lấy mẫu vị trí + **xoá điểm giữa nếu 3 điểm cuối thẳng hàng** (cross-product = 0, khớp `ed.a(...)` của jar)
   - Cứ 2 giây (`FlushIntervalMs = 2000`, khớp `TIME_MOVE_SEND` server) → `Flush()` interleave `[x,y,x,y…]` → `MapHandler.SendMove`
   - Camera bám nhân vật qua `CameraFollower` (world-space, offset theo `faceDir` giống `ew.a(int)` của jar, clamp cạnh map giống `ew.a(int, boolean)`)
   - Nhân vật MÌNH: `PlayerAvatar.SnapTo` (authoritative locally). Người chơi KHÁC: `MoveTo` + `PositionInterpolator` (server chân lý, client nội suy)

9. **Người chơi khác** — nhận broadcast, nội suy vị trí, hiện tên. Vào/ra map thì thêm/xoá avatar.

10. **Chat khu vực** — gửi/nhận `ON_PLACE_CHAT` (9), có ô nhập + nút gửi và hiện bong bóng chat. `kiss`/`play`/`poke` đi qua `PET_SERVICE/ON_PET_INTERACT` và hiện hiệu ứng riêng.

11. **Đối chiếu dump** — đi cùng một đường trên client cũ và Unity, diff gói `ON_OTHER_USER_MOVE`: số điểm và giá trị toạ độ phải cùng dạng.

## Success Criteria

- [x] `JarMapLayout.Parse` chạy sạch trên cả 24 `Resources/Jar/Maps/*.bytes` — 24/24, không dư 1 byte
- [x] Đọc được phần đuôi `eg` (NPC/cổng dịch chuyển) và `z` (điểm mốc) — thêm `JarMapEntity`/`JarMapWaypoint`
- [x] Làm rõ và triển khai 9 file `newMapData/*_b` — metadata `dy.java` cho atlas hoạt ảnh `_a.png`; cả 9/9 parse hết byte và mọi region/frame/clip reference đều hợp lệ
- [x] Ảnh sắc nét, không mờ — tile texture đã `filterMode: 0` (Point). Camera snap về integer pixel trong `CameraFollower.LateUpdate` chống viền mờ khi cuộn
- [x] Code xử lý `INIT_PLAYER`/`ON_PLAYER_ENTER_MAP` đúng wire format (test round-trip)
- [x] Code xử lý `ON_UPDATE_PLAYER_IN_MAP` (opcode 29) — mang toạ độ spawn của SELF + list người khác. Đây mới là gói thực gán vị trí self, không phải ENTER_MAP (server broadcast ENTER_MAP TRƯỚC khi thêm self vào `players`)
- [x] Code xử lý `ON_OTHER_USER_MOVE` cả 2 chiều + rate limit `TIME_MOVE_SEND=2000ms`
- [x] Chat khu vực hoàn chỉnh (`ChatHandler` + `GameHud` + `ChatBubble`); `kiss`/`play`/`poke` có hiệu ứng từ asset `pet/petInteract/`
- [x] Bootstrap wire: `GameSession.Start` sau LOGIN_SUCCES tạo `MapScene` + `CameraFollower` + `GameHud` và gắn/reset `MovementController` khi SELF spawn. Client **không** gọi `SendJoinChannel(11, 0)` vì server tự đưa người chơi vào map ngay sau `loginOK()`
- [x] Mobile có joystick cảm ứng; desktop vẫn dùng WASD/mũi tên. Movement chặn biên/vật cản theo collision mask 0..15 của `ef.java`
- [x] NPC/quái được parse từ `GAME_OBJECT`/`SEND_LIST_MOB_ZONE`, dựng sprite động + tên; bấm NPC gọi guider
- [x] Cổng map gửi đúng opcode 25 `(mapId, waypointIndex, mapVersion)`; controller được reset mapId/toạ độ sau mọi `ON_UPDATE_PLAYER_IN_MAP`
- [x] Map dùng một `Tilemap` cho mỗi layer, đúng `sortingOrder`, không còn một GameObject/SpriteRenderer cho từng ô. Sau khi dựng đúng object hoạt ảnh, PlayMode dựng tuần tự 24 map: 2.653 ms lượt lạnh sau import; 288 ms lượt cache ấm, map chậm nhất 36 ms
- [x] Căn chỉnh theo JAR: tile neo góc trên-trái cell, object/NPC/quái neo giữa đáy, avatar cắt riêng thân/chân/tay theo `v.java`; đã render ảnh kiểm tra map 11 trong Unity
- [x] Nhân vật hiện đúng vị trí server gửi — LiveSmoke `T. INIT_PLAYER` + `U. MAP_UPDATE` xanh; nhận `self=(96,96)` cho map 11, khớp giá trị `PlayerData` server lưu
- [x] Đi lại mượt, server nhận đúng vị trí — LiveSmoke `V. ON_OTHER_USER_MOVE` xanh; gửi đích `(120,96) dir=3`, server broadcast lại đúng byte
- [x] Thấy người chơi khác di chuyển realtime — pipeline chung với `V`: `MapHandler.PlayerMoved` fires cho mọi userId; test 1 acc thấy echo cho self (server không phân biệt self vs other), 2 acc = same code path
- [x] **Avatar runtime** — ghép bộ phận nam/nữ từ `avatar.dat`, hiện tên, lật theo hướng và có nhịp bước; đã bỏ ô đỏ debug cùng Texture2D/Sprite tạo lại mỗi lần spawn
- [ ] Chuyển map không rò texture, không tụt frame — renderer và sprite đã cache, object cũ được huỷ; vẫn cần Memory Profiler trên phiên chơi dài để nghiệm thu tuyệt đối
- [ ] Giữ >30 fps trên máy Android tầm trung — máy kiểm thử có Android SDK nhưng không có thiết bị/AVD kết nối nên chưa thể đo trung thực
- [ ] Diff dump di chuyển khớp với client cũ — chưa dump

## Risk Assessment

| Rủi ro | Xử lý |
|---|---|
| Format `.dat` có biến thể — parser mới chứng minh trên **1/24** map (map 11) | Bước 2 chạy `JarMapLayout.Parse` trên **toàn bộ** 24 map TRƯỚC khi viết renderer. Rủi ro thật, chỉ là đã nhỏ đi nhiều |
| Sửa `JarMapLayout` làm hỏng nền màn đăng nhập (P5.1 đang dùng chung) | Chỉ **thêm** trường, không đổi trường cũ. `JarMapLayoutTests` + `JarLoginViewBackgroundTests` phải xanh sau mỗi lần đụng |
| 9 file `newMapData/*_b` không rõ mục đích | Đã xác nhận từ `dy.java` và test 9/9 file: metadata cắt/ghép atlas hoạt ảnh map; `MapAnimatedObjectView` dùng trực tiếp |
| `tileType` chưa rõ ngữ nghĩa (va chạm?) | Đã đối chiếu `ef.java`: 0 đi được, 15 chặn cả ô, 1..14 là mặt nạ bốn góc 12×12; `MapCollision` dùng đúng bảng đó |
| Rò texture khi chuyển map | Không còn tạo texture debug theo avatar; tile/object sprite dùng cache tĩnh và GameObject map cũ được huỷ. Đo thêm bằng Memory Profiler trên phiên chơi dài |
| Nội suy làm lệch vị trí so với server | Server là nguồn chân lý cho vị trí cuối; nội suy chỉ để hiển thị |

## Next Steps

**Hết P6 = mốc vertical slice.** Dừng lại, đánh giá thực tế, rồi mới plan chi tiết P7-P8.
