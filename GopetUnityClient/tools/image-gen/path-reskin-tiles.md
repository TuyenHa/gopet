# Dải ô nền thay mặt tuyết của lối đi (map rừng)

```bash
cd GopetUnityClient/tools
python image-gen/make-path-reskin-tiles.py   # hoặc: npm run gen:path-tiles
```

| Bản | File sinh ra | Từ | Map dùng |
|---|---|---|---|
| Đất | `12151`, `12161`, `12162` | `151`, `161`, `162` | 12, 13, 14, 15, 17, 18, 20, 22 |
| Đá | `13151`, `13161`, `13162` | `151`, `161`, `162` | *(chưa map nào dùng)* |

Bản đá còn giữ lại vì bảng màu của nó suy từ `3.png` → `11003.png`, muốn dùng lại chỉ
cần trỏ map vào tiền tố `13000` trong `MapSkinOverrides.cs`.

`151` là mặt lối đi, `161`/`162` là cỏ giáp lối (mang mảng tuyết ở rìa). Phải đổi cả ba
thì lối mới liền mạch; chỉ đổi `151` sẽ ra lối đất/đá viền tuyết.

Bảng đổi nằm ở `UiLogic/MapSkinOverrides.cs`: id bản mới = **tiền tố + id gốc**
(12000 = đất, 13000 = đá) nên thêm bản mới chỉ cần thêm một bảng màu và một tiền tố.
Map 14, 17, 18 dùng cùng bộ đất với Linh Lâm; map băng/sông và thượng giới giữ nguyên.

## Vì sao phải có bộ riêng, không dùng 11xxx

`11161`/`11162` đã là bản **cỏ xanh** của áo mùa hè (Thành Phố Linh Thú, Đấu trường,
Đường lên đỉnh núi). Ở hai map này phần tuyết phải thành **đất/đá**, không phải thành
cỏ: dùng bộ 11xxx thì lối đi tan vào bãi cỏ và map không còn đường.

## Ràng buộc khi sửa

- **Thay màu, không vẽ lại.** Ba dải gốc chỉ 16 màu mỗi dải; script tra bảng 8 màu
  tuyết/băng nên hình dạng, mép nối và thứ tự ô giữ nguyên tuyệt đối — map nhị phân
  không phải sửa một byte nào.
- **Bảng màu có nguồn rõ ràng**, không tự chế:
  - đất ← lấy mẫu từ ảnh tham chiếu bản gốc mobile: nền chính đúng `(190,174,114)`,
    tông cát khô. (Bản trước lấy từ vách núi `179.png` nên ra vàng cam, lệch mẫu.)
  - đá ← đúng bảng `3.png` → `11003.png` (lối đá Thành Phố Linh Thú), nên hai map
    cùng một chất đá.
- **Rìa/viền phải ở trong họ đất, không được thành viền đen.** Hai màu băng
  `(56,193,234)`/`(55,153,182)` và hai màu rìa là dải chuyển 1-2 px giữa lối và cỏ.
  Đổi sang nâu đậm là lối đi bị bo một viền đen sì — ảnh mẫu không có viền đó.
- **Cỏ, ô xám và ô nước giữ nguyên.** Màu `(116,117,121)` và `(144,242,255)` /
  `(75,193,209)` ở cuối dải `162` là ô xám và ô nước, không thuộc lối đi.
- Giữ tương quan độ sáng: sáng ứng sáng, tối ứng tối, để hoạ tiết lấm tấm của mặt
  tuyết thành hoạ tiết sỏi chứ không bẹt thành mảng phẳng.
# Mở rộng đường đất ngày 2026-09-22

- Map 12/14/20 dùng cùng bộ 12151/12161/12162 của Linh Lâm.
- Map 17/18/22 dùng thêm 12179/12180; map 21 chỉ đổi 179/180.
- Map 11/13/15/16/19 giữ skin cũ; map 23–34 giữ nguyên.
- Hai ảnh núi được tạo bằng script đổi bảng màu, theo xác nhận của người dùng.
  Script tái sử dụng bảng màu đất của bộ rừng và thêm màu bóng tuyết
  (219,236,240) → (178,162,103). Không sửa pixel đá hoặc alpha.

Chạy từ thư mục repository:

```powershell
python GopetUnityClient/tools/image-gen/make-mountain-dirt-tiles.py
python -m unittest discover -s GopetUnityClient/tools/image-gen -p test_dirt_map_assets.py -v
```

Lệnh tạo ảnh chỉ ghi 12179.png (240×24) và 12180.png (216×24).
Không chạy lại generator rừng để tránh ghi đè các asset có sẵn.
