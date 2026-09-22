# Kế hoạch chuyển đường tuyết thành đường đất

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. Chỉ triển khai sau khi người dùng duyệt plan; không tự động tạo commit.

## Tiến độ cập nhật ngày 2026-09-22

Người dùng đã yêu cầu triển khai tiếp và hoàn tất. Mapping cho 7 map và bộ ảnh đất đã có; **929/929 xUnit, 3/3 test pixel đạt**, 295 tệp gốc khớp hash. Đã bổ sung ảnh trước/sau đủ 24 map và rà soát độc lập không phát hiện lỗi trong phần triển khai.

**Chưa nghiệm thu toàn bộ:** môi trường không có Unity Editor 6000.5.4f1; runner đã thử và dừng với mã 2. Các ô liên quan PlayMode, screenshot Unity và đi thử trong game vẫn để trống. Chi tiết và bằng chứng: [inventory](2026-09-22-doi-duong-tuyet-thanh-duong-dat-inventory.md#kết-quả-kiểm-chứng-ngày-2026-09-22).

Các bước RED của Python/xUnit được ghi nhận trong lịch sử triển khai; không chạy lại bằng cách gỡ phần code đang hoạt động. Phân loại và đối chiếu ảnh dùng lớp tile offline, chưa thay thế nghiệm thu Unity. Không tạo commit.

**Goal:** Chuyển phần đường/nền tuyết ở các map đất liền phù hợp của Unity thành đường đất, giữ nguyên các map chủ đề biển, sông và băng.

**Architecture:** Mở rộng cơ chế thay ảnh theo `mapId` hiện có trong `MapSkinOverrides`. Dùng bộ ảnh đất riêng, giữ nguyên ảnh gốc và dữ liệu map; chỉ những map được phân loại và đưa vào danh sách cho phép mới nhận skin đất.

**Tech Stack:** Unity/C# 9, tài nguyên PNG theo ô 24×24, xUnit (.NET 8), Unity PlayMode Test Runner.

**Spec:** Phần “Thiết kế và phạm vi” trong tài liệu này. Đây là thay đổi giới hạn trong luồng render đã có; người dùng đã yêu cầu thực hiện, bao gồm tiếp tục phần còn thiếu.

## Thiết kế và phạm vi

Yêu cầu người dùng: đổi đường tuyết thành đường đất, loại trừ map biển, sông, băng.

Các giả định để duyệt:

- Phạm vi là Unity client; không sửa client JAR hoặc server.
- Loại trừ **toàn bộ map có chủ đề biển/sông/băng**. Map rừng/núi có ao nhỏ vẫn có thể đổi đường, nhưng nước và bờ nước phải giữ nguyên. Đây là giả định đang chờ người dùng xác nhận nếu muốn hiểu khác.
- Chỉ đổi đường/nền tuyết và phần tuyết ở mép tiếp giáp nền. Không đổi cây, nhà, NPC, quái, đá, mặt nước, mây hoặc toàn bộ phong cách map.
- Giữ skin cỏ/đá hiện có của map 11, 16, 19; không biến phần đã hết tuyết thành đất. Giữ bộ đất hiện có của map 13, 15.
- Dùng màu đất vàng nâu nhẹ của bộ `12151/12161/12162`, không thay bằng cỏ khiến đường mất hình dạng.

Phương án được đề xuất: danh sách map cho phép + bảng thay ảnh theo map. Không ghi đè ảnh gốc dùng chung, vì có thể đổi lây sang map bị loại trừ. Không sửa từng ô trong `.bytes`, vì đây chỉ là thay đổi hình ảnh, không phải bố cục hay đường đi.

### Kết quả khảo sát hiện tại

Đã đọc dữ liệu tile của 24 map, ID 11–34. Danh sách dưới đây phân biệt dữ liệu đã xác định với phân loại địa hình cần kiểm tra bằng hình toàn map.

| Nhóm map | Hiện trạng | Xử lý dự kiến |
|---|---|---|
| 11 Thành Phố Linh Thú; 16 Đường lên đỉnh núi; 19 Đấu trường | Có skin cỏ/đá riêng | Giữ nguyên |
| 13 Linh Lâm; 15 Đại Linh Cảnh | Đã dùng bộ đường đất | Giữ nguyên, dùng làm mẫu màu |
| 12 Ải; 14 Linh Mộc; 20 Lôi đài | Đang dùng tile tuyết 161/162; map 14 còn dùng 151 | Ứng viên chuyển sang bộ đất 12151/12161/12162 |
| 17 Thung lũng Hoàng Nham; 18 Núi Phục Quang; 22 Chợ trời | Dùng cả 179/180 và 161/162; map 17/18 còn dùng 151 | Ứng viên chuyển đồng bộ nền và mép tuyết; cần thêm bộ đất 12179/12180 |
| 21 Thạch Động | Dùng nền/vách tuyết 179/180; có strip đá 163 | Ứng viên đổi 179/180; giữ strip 163 |
| 23 Băng động 1; 24 Sông băng; 25 Băng động 2 | Bộ băng riêng 219–222 | Loại trừ bắt buộc |
| 26–34 | Các bộ ảnh khác; đã thấy tile mây ở họ 242–246 và cỏ/mây ở 334 | Mặc định giữ nguyên; kiểm tra toàn map, không coi mọi vùng trắng là tuyết |

Điểm quan trọng: map 31 khai báo ảnh 151 nhưng không có ô tile nào đang dùng strip này. Không dùng riêng danh sách tài nguyên để kết luận map có đường tuyết.

Chưa xác định thêm ID map biển/sông ngoài map 24 bằng hình toàn map. Vì vậy, danh sách ứng viên **không phải** danh sách đã duyệt để áp dụng mù quáng. Task 1 phải chốt phân loại trước khi sửa mapping.

## Ràng buộc chung

- Không sửa `Assets/Resources/Jar/Maps/*.bytes`, collision, waypoint/cổng, điểm xuất hiện hoặc giao thức server.
- Không ghi đè PNG gốc, đặc biệt 151/161/162/179/180 và bộ băng 219–222.
- Mỗi ảnh thay thế giữ nguyên kích thước, thứ tự ô 24×24 và alpha của ảnh gốc; không dịch hình hoặc thay số ô.
- Ảnh không thuộc phần tuyết cần thay phải giữ nguyên; không nhuộm màu nước, đá, cỏ hay mây.
- Chỉ thêm mapping theo danh sách map đã kiểm tra. Map không khai báo và image ID không khai báo trả lại ID đầu vào.
- Giữ `ResolveObjectImageId` và các override cây/nhà hiện có của map 15.
- Không đụng các thay đổi popup thắng quái đang có trong worktree.
- Không thêm package runtime, không thay cache/render architecture nếu không có lỗi do thay đổi này.
- Hash đối chiếu lấy từ workspace trước triển khai, không lấy JAR làm chuẩn để ghi đè: map băng hiện có bản vá collision riêng cần được giữ lại.

## Trọng tâm kiểm tra

1. Map biển/sông/băng dùng chung ảnh với map đất vẫn không đổi: Task 1 chốt danh sách, Task 3 khóa bằng test.
2. Ao nhỏ trên map đất vẫn giữ nước và bờ nước: Task 2 kiểm tra pixel, Task 4 kiểm tra trong Unity.
3. Nền núi đổi nhưng mép vách còn tuyết hoặc đường bị biến thành cỏ: Task 2 kiểm tra bộ 179/180 và Task 4 kiểm tra chỗ nối.
4. Mây trắng và strip khai báo nhưng không dùng bị nhận nhầm thành đường tuyết: Task 1 kiểm tra tile thực dùng, Task 3 giữ nguyên nhóm chưa được duyệt.
5. Chuyển map đất → map băng → map đất bị dùng nhầm ảnh cache: Task 4 kiểm tra sprite thực tế sau từng lần chuyển.

## Task 1: Chốt danh sách map và lưu mốc đối chiếu

**Files:**

- Read: `GopetUnityClient/Assets/Scripts/UiLogic/MapDisplayNames.cs`
- Read: `GopetUnityClient/Assets/Scripts/UiLogic/JarMapLayout.cs`
- Read: `GopetUnityClient/Assets/Resources/Jar/Maps/11.bytes` đến `34.bytes`
- Create: `docs/superpowers/plans/2026-09-22-doi-duong-tuyet-thanh-duong-dat-inventory.md`

**Đầu ra:** bảng đủ 24 dòng: ID, tên, strip thực dùng, chủ đề, đổi/giữ, lý do và đường dẫn ảnh chụp kiểm chứng. Danh sách map đổi là đầu vào duy nhất của Task 3.

- [x] Ghi nhận `git status --short` và hash SHA256 của `.bytes`, ảnh PNG gốc, ảnh skin hiện có. Lưu hash trong inventory; không hoàn nguyên thay đổi có sẵn.

```powershell
Get-ChildItem GopetUnityClient/Assets/Resources/Jar/Maps -Filter *.bytes |
    Get-FileHash -Algorithm SHA256
Get-ChildItem GopetUnityClient/Assets/Resources/Jar/Art/Raw/newMapData -Filter *.png |
    Get-FileHash -Algorithm SHA256
```

- [ ] Mở từng map 11–34 bằng luồng render hiện có trong Unity; lưu ảnh toàn map trước đổi. Đối chiếu tên map với hình, không suy ra biển/sông chỉ từ việc có màu xanh.
- [x] Với từng ô dữ liệu, chỉ tính strip có `JarMapLayout.StripOf(tile) >= 0` và nhỏ hơn `ImageCount`; ghi ID tài nguyên thực dùng. Xác nhận riêng map 31 không dùng 151.
- [x] Phân loại “biển”, “sông”, “băng”, “đất liền”, “mây/thượng giới” từ ảnh lớp tile offline; 33/34 còn mơ hồ nước/bầu trời nên giữ nguyên. Đất liền có ao nhỏ không tự động bị loại theo giả định của plan.
- [x] Chốt danh sách loại trừ gồm 23/24/25 và mọi map biển/sông xác định thêm. Map còn mơ hồ giữ nguyên và báo người dùng, không tự thêm vào danh sách đổi.
- [x] Chốt các ứng viên 12/14/17/18/20/21/22 qua ảnh offline. Nhóm 26–34 giữ nguyên, không mở rộng asset.

**Kiểm chứng:** inventory đủ 24 map, mỗi map có quyết định và bằng chứng hình. Nếu thiếu Unity/hình để phân loại, chưa được tuyên bố hoàn tất bước này hoặc triển khai cả danh sách ứng viên.

## Task 2: Chuẩn bị bộ đường đất và kiểm tra tính tương thích

**Files:**

- Reuse: `GopetUnityClient/Assets/Resources/Jar/Art/Raw/newMapData/12151.png`, `12161.png`, `12162.png`
- Create: `GopetUnityClient/Assets/Resources/Jar/Art/Raw/newMapData/12179.png`, `12180.png` và `.meta` riêng
- Modify: `GopetUnityClient/tools/image-gen/path-reskin-tiles.md`
- Read: `GopetUnityClient/tools/image-gen/mountain-grass-tiles.md`
- Create: `GopetUnityClient/Assets/Tests/PlayMode/DirtMapAssetTests.cs` và `.meta`

**Giao diện:** ảnh được load theo `Jar/Art/Raw/newMapData/{imageId}`, mỗi cell dài 24 px. Các ID mới chỉ dành cho bản đất của 179/180; kiểm tra chưa bị dùng trước khi tạo.

- [x] Viết test trước: mỗi cặp ảnh phải load được, cùng chiều rộng/cao, cao 24, chiều rộng chia hết cho 24. Cặp 179/12179 có 10 ô; 180/12180 có 9 ô. Kiểm tra các cặp 151/12151, 161/12161, 162/12162 tương tự.

```csharp
[TestCase(151, 12151)]
[TestCase(161, 12161)]
[TestCase(162, 12162)]
[TestCase(179, 12179)]
[TestCase(180, 12180)]
public void AnhDat_GiuNguyenKichThuoc(int originalId, int dirtId)
{
    var original = Resources.Load<Texture2D>($"Jar/Art/Raw/newMapData/{originalId}");
    var dirt = Resources.Load<Texture2D>($"Jar/Art/Raw/newMapData/{dirtId}");
    Assert.IsNotNull(original);
    Assert.IsNotNull(dirt);
    Assert.AreEqual(original.width, dirt.width);
    Assert.AreEqual(original.height, dirt.height);
    Assert.AreEqual(24, dirt.height);
    Assert.AreEqual(0, dirt.width % 24);
}
```

- [ ] Chạy test trong Unity, xác nhận hai ảnh mới chưa có làm test fail.
  - Bước lịch sử chưa chạy do thiếu Editor; đã quan sát RED tương ứng bằng Python khi thiếu 12179/12180. Không xóa asset để tái hiện RED trong Unity về sau.
- [x] Tạo bản đất 12179/12180 bằng script bảng màu đã được người dùng xác nhận: mặt tuyết và mép tuyết chuyển sang đất cùng tông bộ 12151; giữ đá nâu, bóng, cạnh và alpha. Không sinh lại bộ 12151/12161/12162 đã có.
- [x] Kiểm tra alpha và mọi pixel ngoài tuyết không đổi bằng test; xem ảnh đối chiếu đủ 24 map và rà soát strip mới, giữ đá/nước/cỏ và hình ô.
- [x] Chuẩn bị `.meta` theo ảnh liền kề: Point, không mipmap/nén, GUID mới và duy nhất. Import thực tế trong Unity còn chờ Editor.
- [ ] Chạy lại `DirtMapAssetTests`, yêu cầu tất cả PASS; ghi cách tạo ảnh và kết quả đối chiếu vào tài liệu tile.

## Task 3: Áp dụng skin theo danh sách map, khóa ngoại lệ bằng unit test

**Files:**

- Modify: `GopetUnityClient/Assets/Scripts/UiLogic/MapSkinOverrides.cs`
- Modify: `GopetUnityClient/tests/Gopet.Net.Tests/MapSkinOverridesTests.cs`

**Giao diện giữ nguyên:** `public static int ResolveImageId(int mapId, int imageId)` và `public static int ResolveObjectImageId(int mapId, int imageId)`. `MapRenderer.BuildTileLayers` đã gọi `ResolveImageId`; không cần thêm lớp render khác.

- [x] Viết test thất bại cho map đất đã duyệt. Ví dụ sau chỉ giữ các map mà inventory xác nhận là đất liền:

```csharp
[Theory]
[InlineData(12, 161, 12161)]
[InlineData(14, 151, 12151)]
[InlineData(17, 179, 12179)]
[InlineData(17, 180, 12180)]
[InlineData(18, 180, 12180)]
[InlineData(20, 162, 12162)]
[InlineData(21, 179, 12179)]
[InlineData(22, 180, 12180)]
public void MapDat_DoiDungAnh(int mapId, int original, int expected)
{
    Assert.Equal(expected, MapSkinOverrides.ResolveImageId(mapId, original));
}

[Theory]
[InlineData(23)]
[InlineData(24)]
[InlineData(25)]
[InlineData(999)]
public void MapLoaiTruHoacKhongKhaiBao_KhongDoi(int mapId)
{
    foreach (var imageId in new[] { 151, 161, 162, 179, 180, 219, 220, 221, 222 })
        Assert.Equal(imageId, MapSkinOverrides.ResolveImageId(mapId, imageId));
}
```

- [x] Thêm từng map biển/sông xác định ở Task 1 vào test loại trừ; test toàn bộ strip thực dùng của các map đó. Kiểm tra giả lập ảnh 151/161/162 ngay cả khi hiện tại map loại trừ chưa dùng chúng.
- [x] Giữ test skin cũ 11/13/15/16/19, kiểm tra 163 giữ nguyên ở map 21, ảnh vật thể 158/177 không bị thay theo bảng nền. Thêm kiểm tra nhóm 26–34 và ảnh mây 242–246/334/337 giữ nguyên theo inventory đã duyệt.
- [x] Cập nhật hai test cũ `MapRungKhac_GiuNguyenMatTuyet` và `MapKhac_GiuNguyenBoTileGoc`: chúng đang yêu cầu map 14/20 giữ tuyết, trái yêu cầu mới nếu hai map được duyệt đổi. Dùng ID 999 cho test map chưa khai báo.
- [x] Quan sát RED do thiếu mapping mới: lịch sử ghi nhận 11 test fail, 44 pass; không phải lỗi môi trường.

```powershell
& ./.superpowers/dotnet/dotnet.exe test GopetUnityClient/tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj --filter FullyQualifiedName~MapSkinOverridesTests
```

- [x] Mở rộng bảng map cho phép theo inventory. Với map rừng/đường: 151→12151, 161→12161, 162→12162. Với map núi/hang đã duyệt: thêm 179→12179, 180→12180. Không áp dụng 179/180 cho map 13/15 chỉ vì cùng bộ đất.

```csharp
// Hàm phụ cho đúng nhóm map núi/hang đã được duyệt.
private static int DirtMountainReskin(int imageId)
{
    if (imageId == 179) return 12179;
    if (imageId == 180) return 12180;
    return DirtPathReskin(imageId);
}
```

- [x] Thứ tự giải quyết: map bị loại trừ → giữ nguyên; map đất/núi đã duyệt → mapping tương ứng; các nhánh skin cũ → giữ hành vi; còn lại → trả ID gốc. Không thêm quy tắc chung như “mọi map ngoài 23–25 đều đổi”.
- [x] Chạy lại test có filter và toàn bộ xUnit. Yêu cầu không làm hỏng test bộ cỏ/đá, bộ đất cũ và vật thể nhiệt đới.

## Task 4: Kiểm thử hiển thị, chuyển map và nghiệm thu

**Files:**

- Modify: `GopetUnityClient/Assets/Tests/PlayMode/AllMapRenderingTests.cs`
- Create: `GopetUnityClient/Assets/Tests/PlayMode/DirtMapRenderingTests.cs` và `.meta`
- Update: `docs/superpowers/plans/2026-09-22-doi-duong-tuyet-thanh-duong-dat-inventory.md`

**Đầu vào:** danh sách đã duyệt, asset của Task 2 và API `ResolveImageId` của Task 3. Dùng `MapRenderer.Create(parent, mapId)`, `TileAssetProvider.Cell(imageId, cellIndex)` và `TileAssetProvider.TileFromImage(imageId, cellIndex)` hiện có.

Đã viết `DirtMapRenderingTests` kiểm tra mọi ô của 24 map và chuỗi chuyển map với cache còn nguyên; `DirtMapAssetTests` kiểm tra texture/sprite. Giữ `AllMapRenderingTests` làm smoke test, kiểm tra chi tiết nằm ở bộ test mới. Các bước runtime dưới đây chưa đánh dấu vì chưa chạy được Unity.

- [x] Dựng và xem ảnh lớp tile offline trước/sau đủ 24 map. Bảy map được duyệt có thay đổi; 17 map giữ nguyên từng pixel. Lưu ảnh đầy đủ, bảng đối chiếu và `verification.json` trong inventory.

- [ ] Với từng strip/cell thực dùng, xác nhận sprite sau mapping tồn tại, rect 24×24 và đúng texture kỳ vọng. Không chỉ kiểm tra tổng số tile > 0 vì vẫn có thể thiếu một phần map.
- [ ] Với tile kỳ vọng, kiểm tra nó thực sự xuất hiện trong các `Tilemap` do renderer tạo, không chỉ gọi lại hàm mapping rồi so với chính hàm đó.

```csharp
var expected = TileAssetProvider.TileFromImage(12151, 0);
Assert.IsNotNull(expected);
Assert.AreEqual(24f, expected.sprite.rect.width);
Assert.AreEqual(24f, expected.sprite.rect.height);
Assert.AreSame(
    Resources.Load<Texture2D>("Jar/Art/Raw/newMapData/12151"),
    expected.sprite.texture);
```

- [ ] Render theo chuỗi 14 → 24 → 17 → 23 → 14 và kiểm tra ảnh thực của từng map. Cache không được làm map băng dùng ảnh đất hoặc map đất quay lại tuyết. Nếu một map trong chuỗi bị loại ở inventory, thay bằng map cùng nhóm đã duyệt.
- [ ] Chạy smoke render đủ map 11–34. So ảnh trước/sau: map bị loại trừ không đổi; đường đất rõ hình, góc nối không còn viền tuyết thừa; nước/ao/cỏ/đá/mây không đổi.
- [ ] Đi thử đường, mép núi và cổng chuyển map ở map rừng, núi/hang, map băng. Xác nhận địa hình đi được/không đi được, cổng, NPC và quái không đổi vị trí.
- [x] Tính lại hash: mọi `.bytes` và PNG có sẵn trước triển khai phải giữ nguyên. 295/295 hash khớp; lưu `after-hashes.json`. Chỉ hai ảnh mới và mapping/test/tài liệu/công cụ kiểm chứng liên quan được thay đổi trong phạm vi này.
- [x] Chạy kiểm tra từ thư mục gốc repository:

```powershell
& ./.superpowers/dotnet/dotnet.exe test GopetUnityClient/tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj
node GopetUnityClient/tools/check-asmdef-refs/index.js
git diff --check
```

- [ ] Chạy Unity Test Runner → PlayMode: `DirtMapAssetTests`, `DirtMapRenderingTests`, `AllMapRenderingTests`. Dùng đúng phiên bản Editor trong `GopetUnityClient/ProjectSettings/ProjectVersion.txt`.
- [x] Báo kết quả thực tế và đính kèm ảnh trước/sau offline. Đã thử runner Unity: không tìm thấy Editor, mã thoát 2. Ghi rõ PlayMode/đi thử chưa chạy; không báo hoàn tất nghiệm thu Unity.

## Tiêu chí hoàn thành

- Tất cả map đất liền có đường tuyết được duyệt trong inventory đã đổi sang đất nhất quán.
- Không còn map ứng viên bị bỏ sót mà không có lý do; trường hợp chưa phân loại phải được báo rõ, không âm thầm coi là hoàn thành.
- Map biển/sông/băng giữ nguyên; mây trắng không bị biến thành đất.
- Skin 11/13/15/16/19, nước, vật thể và gameplay không bị thay đổi ngoài phạm vi.
- Unit test, kiểm tra asset và PlayMode đạt; có ảnh đối chiếu các nhóm đại diện và toàn bộ map bị loại trừ.

## Hoàn tác nếu cần

Gỡ đúng các mapping đất mới để map quay về ảnh cũ; giữ mapping 13/15 đã có trước đó. Chỉ xóa hai asset mới nếu không còn nơi tham chiếu. Không reset toàn worktree, không ghi đè `.bytes`, không động vào popup thắng quái.
