# Kiểm kê map — chuyển đường tuyết thành đất

Ngày: 2026-09-22. Phạm vi: Unity client, map 11–34.

**Trạng thái:** đã triển khai mapping và bộ ảnh đất; kiểm chứng tự động và ảnh lớp tile offline đạt. Chưa hoàn tất nghiệm thu Unity PlayMode và đi thử trong game vì máy không có Unity Editor 6000.5.4f1. Không còn chờ duyệt script tạo ảnh.

## Bằng chứng và giới hạn

Đã dựng và xem ảnh trước/sau của toàn bộ **lớp tile** của 24 map từ `.bytes` và PNG thật. Mapping sau đổi được xuất bằng chính `MapSkinOverrides.ResolveImageId` của mã nguồn C# hiện tại, lưu trong [mapping-after.json](assets/doi-duong-dat/mapping-after.json). Đây là ảnh đối chiếu offline, không phải screenshot Unity; chưa bao gồm vật thể, animation, UI hoặc kiểm chứng import. Ảnh trước đổi đã lưu từ lần khảo sát ban đầu; không tạo lại baseline sau khi thêm asset.

- [Map 11–18 trước đổi](assets/doi-duong-dat/before-11-18.png)
- [Map 19–26 trước đổi](assets/doi-duong-dat/before-19-26.png)
- [Map 27–34 trước đổi](assets/doi-duong-dat/before-27-34.png)
- Hash trước triển khai: [baseline-hashes.json](assets/doi-duong-dat/baseline-hashes.json). Bao gồm tất cả `.bytes` và PNG hiện có, lấy từ workspace, không ghi đè bản vá map băng từ JAR.
- Đối chiếu trước/sau: [map 11–18](assets/doi-duong-dat/compare-11-18.png), [map 19–26](assets/doi-duong-dat/compare-19-26.png), [map 27–34](assets/doi-duong-dat/compare-27-34.png).
- Mỗi map có ảnh nguyên kích thước tại `assets/doi-duong-dat/before/<ID>.png` và `assets/doi-duong-dat/after/<ID>.png`. [verification.json](assets/doi-duong-dat/verification.json) liệt kê đường dẫn, kích thước và số pixel thay đổi của đủ 24 map.
- Hash sau triển khai: [after-hashes.json](assets/doi-duong-dat/after-hashes.json). **295/295 tệp khớp baseline**, không có tệp gốc thiếu hoặc thay đổi.

## Danh sách đã phân loại

| ID | Tên | Strip tile thực dùng | Chủ đề quan sát / quyết định |
|---|---|---|---|
| 11 | Thành Phố Linh Thú | 3,161,162 | Thành phố cỏ/đá có mép nước; giữ skin hiện có |
| 12 | Ải | 161,162 | Đường tuyết giữa cỏ; đổi đất |
| 13 | Linh Lâm | 151,161,162 | Rừng đã có đất; giữ |
| 14 | Linh Mộc | 151,161,162 | Đường tuyết giữa cỏ; đổi đất |
| 15 | Đại Linh Cảnh | 151,161,162 | Rừng đã có đất; giữ cả ao và vật thể hiện có |
| 16 | Đường lên đỉnh núi | 3,161,162,179,180 | Núi đã có cỏ/đá; giữ |
| 17 | Thung lũng Hoàng Nham | 151,161,162,179,180 | Đường/nền tuyết trên bậc núi; đổi đất |
| 18 | Núi Phục Quang | 151,161,162,179,180 | Nền tuyết trên bậc núi; đổi đất |
| 19 | Đấu trường | 3,161,162 | Cỏ/đá có mép nước; giữ |
| 20 | Lôi đài | 161,162 | Bốn vùng đường tuyết giữa cỏ; đổi đất |
| 21 | Thạch Động | 163,179,180 | Đường tuyết giữa vách hang; đổi 179/180, giữ 163 |
| 22 | Chợ trời | 161,162,179,180 | Nền tuyết cạnh bậc núi/cỏ/đường xám; đổi tuyết, giữ đường xám |
| 23 | Băng động 1 | 219,220,221,222 | Băng/nước; loại trừ |
| 24 | Sông băng | 219,220,221,222 | Sông/băng; loại trừ |
| 25 | Băng động 2 | 219,220,221,222 | Băng/nước; loại trừ |
| 26 | Vùng đất phong ấn | 243,244,245,246,295 | Đảo nổi, mây và lát đá; giữ |
| 27 | Đài tưởng niệm | 243,244,246,295 | Lát đá/mây; giữ |
| 28 | Quảng trường chính | 241,242,243,245,246,295,299 | Lát đá/mây; giữ |
| 29 | TP Thiên Thần | 241,242,243,244,245,299 | Đảo nổi/cỏ/mây; giữ |
| 30 | Khu vực bang hội | 242,243,244,245,246 | Đảo nổi/lát đá/mây; giữ |
| 31 | Ải thượng giới | 242,243,244,245,246,299 | Lát đá/mây; 151 khai báo nhưng không dùng, giữ |
| 32 | Chốt chặn cuối cùng | 242,243,246,334,337 | Lát đá với viền mây/cỏ; giữ |
| 33 | Những cây cầu | 334,337,371 | Cầu/lát đá, nền xanh quanh cầu; giữ, không coi viền trắng là đường tuyết |
| 34 | Vùng chiến sự | 334,337,371 | Nền vàng/cầu, nền xanh bên dưới; giữ, không đổi vùng nước/bầu trời |

Danh sách đổi mới: **12, 14, 17, 18, 20, 21, 22**. Không thấy chủ đề biển/sông trong bảy map này qua lớp tile. Các map có nước hoặc phân loại nước/bầu trời chưa chắc chắn đều nằm ngoài danh sách đổi. Không cần phân loại ép 33/34 để áp dụng skin vì chúng được giữ nguyên hoàn toàn.

## Mapping được duyệt từ khảo sát

- 12/14/20: 151→12151, 161→12161, 162→12162; các strip khác giữ nguyên.
- 17/18/22: bộ đường trên và 179→12179, 180→12180.
- 21: chỉ 179→12179, 180→12180; strip 163 và các strip không khai báo giữ nguyên.
- 11/13/15/16/19: giữ đúng mapping trước triển khai.
- 23–34, ID lạ và vật thể: không thêm thay đổi.

## Kết quả kiểm chứng ngày 2026-09-22

| Hạng mục | Kết quả | Bằng chứng |
|---|---|---|
| Toàn bộ xUnit | **929/929 PASS**, 0 fail, 0 skip | [xunit-results.trx](assets/doi-duong-dat/xunit-results.trx) |
| Kích thước, alpha, pixel ngoài tuyết và nước/xám | **3/3 PASS** | [pixel-tests.txt](assets/doi-duong-dat/pixel-tests.txt) |
| Ảnh mới 12179/12180 | 240×24 (10 ô), 216×24 (9 ô), RGBA; giữ alpha/đá | Test pixel và `MapDirtAssetContractTests` |
| Tài nguyên gốc | **295/295 hash khớp** | [verification.json](assets/doi-duong-dat/verification.json) |
| Map thay đổi | Đúng 12/14/17/18/20/21/22 | Ảnh đối chiếu và số pixel thay đổi trong JSON |
| Map giữ nguyên | Cả 17 map còn lại có **0 pixel thay đổi** ở lớp tile | Bao gồm 11/13/15/16/19 và 23–34 |
| Tham chiếu assembly | PASS, đủ tham chiếu cho 4 asmdef | `node GopetUnityClient/tools/check-asmdef-refs/index.js` |
| Kiểm tra diff | PASS | `git diff --check` |
| Rà soát độc lập | Không phát hiện lỗi Critical/Important/Minor trong mapping, asset và test | Chưa đánh giá runtime Unity |
| Unity PlayMode | **CHƯA CHẠY** | [unity-attempt.txt](assets/doi-duong-dat/unity-attempt.txt); runner dừng với mã 2 do không tìm thấy Editor |
| Đi thử trong game | **CHƯA CHẠY** | Cần Unity và phiên chạy game |

Hai ảnh mới được tạo bằng `make-mountain-dirt-tiles.py` sau khi người dùng xác nhận dùng script đổi bảng màu, cùng tông Linh Lâm. Ảnh thử AI trước đó không đáp ứng kích thước/alpha đã bị loại và không được đưa vào Resources. Lịch sử RED đã ghi nhận thiếu hai asset và 11 test mapping thất bại trước khi triển khai; kết quả hiện tại là GREEN ở Python/xUnit. Không tuyên bố đã chạy RED/GREEN trong Unity.

Đã xem ba trang đối chiếu đủ 24 map: mặt đường và mép tuyết của bảy map đổi thành đất; vách đá và đường xám giữ hình; các map băng/mây giữ nguyên. Ao được tạo bằng object của map 15 không nằm trong ảnh lớp tile; code object không đổi nhưng vẫn cần xem trong Unity.

## Lệnh kiểm tra lại

Từ thư mục gốc repository:

```powershell
& ./.superpowers/dotnet/dotnet.exe test GopetUnityClient/tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj --no-restore
python -B -m unittest discover -s GopetUnityClient/tools/image-gen -p test_dirt_map_assets.py -v
python -B GopetUnityClient/tools/image-gen/verify-dirt-map-evidence.py
node GopetUnityClient/tools/check-asmdef-refs/index.js
git diff --check
```

Script `verify-dirt-map-evidence.py` kiểm tra hash tài nguyên hiện tại và so sánh **ảnh offline đã lưu**; không chạy renderer Unity và không tự dựng lại ảnh khi mapping đổi. Khi sửa mapping/asset sau lần nghiệm thu này phải xuất lại ảnh trước khi dùng báo cáo làm bằng chứng.

## Phần còn lại cần Unity

1. Cài/mở đúng Editor **6000.5.4f1** và cho phép Unity import dự án.
2. Đóng Editor rồi chạy script sẵn có (đổi đường dẫn bên dưới theo máy):

   ```powershell
   $env:UNITY_EXE = 'D:\Unity Editor\6000.5.4f1\Editor\Unity.exe'
   powershell -NoProfile -ExecutionPolicy Bypass -File GopetUnityClient/run-playmode-tests.ps1
   ```

3. Kiểm tra XML `GopetUnityClient/Logs/playmode-results.xml`: `DirtMapAssetTests`, `DirtMapRenderingTests`, `AllMapRenderingTests` phải chạy và PASS. Test render đã kiểm tra từng tile/texture và chuỗi cache **14 → 24 → 17 → 23 → 14**; hiện chỉ mới viết, chưa có kết quả Unity.
4. Chụp hình Unity, đi thử đường/mép núi/cổng; kiểm tra ao map 15, NPC/quái và cập nhật các ô Unity còn trống trong plan.

Không đánh dấu nghiệm thu toàn bộ trước khi hoàn tất bốn bước này. Không tạo commit trong lần thực hiện này.
