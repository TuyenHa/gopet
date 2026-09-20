# Dải ô nền thay mặt tuyết của lối đi (map rừng)

```bash
cd GopetUnityClient/tools
python image-gen/make-path-reskin-tiles.py   # hoặc: npm run gen:path-tiles
```

| Bản | File sinh ra | Từ | Map dùng |
|---|---|---|---|
| Đất | `12151`, `12161`, `12162` | `151`, `161`, `162` | 13 — Linh Lâm, 15 — Đại Linh Cảnh |
| Đá | `13151`, `13161`, `13162` | `151`, `161`, `162` | *(chưa map nào dùng)* |

Bản đá còn giữ lại vì bảng màu của nó suy từ `3.png` → `11003.png`, muốn dùng lại chỉ
cần trỏ map vào tiền tố `13000` trong `MapSkinOverrides.cs`.

`151` là mặt lối đi, `161`/`162` là cỏ giáp lối (mang mảng tuyết ở rìa). Phải đổi cả ba
thì lối mới liền mạch; chỉ đổi `151` sẽ ra lối đất/đá viền tuyết.

Bảng đổi nằm ở `UiLogic/MapSkinOverrides.cs`: id bản mới = **tiền tố + id gốc**
(12000 = đất, 13000 = đá) nên thêm bản mới chỉ cần thêm một bảng màu và một tiền tố.
Map 14, 17, 18 cũng dùng `151` nhưng **giữ tuyết** — có test chốt điều này.

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
