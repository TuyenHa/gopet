# Dải ô nền hết tuyết cho Đường lên đỉnh núi (map 16)

```bash
cd GopetUnityClient/tools
python image-gen/make-mountain-grass-tiles.py   # hoặc: npm run gen:mountain-tiles
```

| File sinh ra | Từ | Nội dung |
|---|---|---|
| `11180.png` | `180.png` | Mặt tuyết → thảm cỏ xanh |
| `11179.png` | `179.png` | Vách đá phủ tuyết → vách đá, phần tuyết thành cỏ |

Map 16 còn dùng chung `11161`/`11162` (cỏ, thay `161`/`162`) và `11003` (viền đá,
thay `3`) với Thành Phố Linh Thú. Bảng đổi nằm ở `UiLogic/MapSkinOverrides.cs`.

## Ràng buộc khi sửa

- **Thay màu, không vẽ lại.** Dải gốc chỉ 6–16 màu; script tra bảng 6 màu
  tuyết/băng nên hình dạng, mép nối và thứ tự ô giữ nguyên tuyệt đối — map nhị
  phân không phải sửa một byte nào.
- **Số ô phải khớp dải gốc** (180 có 9 ô, 179 có 10 ô). Lệch một ô là cả map lát sai.
- **Cỏ lấy hoạ tiết từ `11161`, không tô màu phẳng.** `11161` chính là dải cỏ phủ
  nửa dưới map 16; hai vùng giáp nhau mà khác chất cỏ là thấy đường nối ngay.
  Mẫu lấy theo toạ độ TRONG ô (`x % 24`) nên cỏ liền mạch qua các ô.
- **Đá và đất nâu của `179` giữ nguyên** — chỉ tuyết mới phải đi, mất luôn đá thì
  vách núi thành đồng cỏ phẳng lì.
