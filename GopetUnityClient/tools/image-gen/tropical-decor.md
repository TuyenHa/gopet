# Cây dừa + ao sen (Đại Linh Cảnh)

```bash
cd GopetUnityClient/tools
python image-gen/make-palm-tree.py     # hoặc: npm run gen:palm
python image-gen/make-lotus-pond.py    # hoặc: npm run gen:lotus-pond
python image-gen/make-thatch-house.py  # hoặc: npm run gen:thatch-house
```

| Asset | File sinh ra | Thay gì | Map dùng |
|---|---|---|---|
| Cây dừa | `14158.png` (52×62) | `158.png` — cây thông có tuyết | 15 — Đại Linh Cảnh |
| Nhà lá | `14177.png` (103×91) | `177.png` — nhà gỗ mái ngói phủ tuyết | 15 — Đại Linh Cảnh |
| Ao sen | `14200.png` (132×84) | *(vật thể mới, không thay gì)* | 15 — Đại Linh Cảnh |

Tiền tố `14xxx` là bộ **nhiệt đới** của Đại Linh Cảnh, tách khỏi `11xxx` (cỏ xanh),
`12xxx` (lối đất) và `13xxx` (lối đá) — xem `path-reskin-tiles.md`.

Cả ba script dùng chung `pixel_art.py` (canvas, vẽ đoạn thẳng, tô đa giác lồi, bo
viền). Import chạy được vì Python tự đưa thư mục của script vào `sys.path`, nên gọi
từ `tools/` hay từ `image-gen/` đều được.

## Hai đường vẽ khác nhau

Ô nền tra qua `MapSkinOverrides.ResolveImageId`, vật thể tra qua
`MapSkinOverrides.ResolveObjectImageId`. **Không gộp**: hai bên có thể trùng id ảnh
mà ý nghĩa khác nhau, gộp là đổi nền kéo theo đổi vật thể.

Ao sen không nằm trong `.dat` nên không tra bảng nào — `MapRenderer.Decor.cs` tự thêm
vào sau khi dựng map, đúng cách vườn bắc Thành Phố Linh Thú đang làm.

## Ràng buộc khi sửa

- **Bản thay phải giữ ĐÚNG khung và neo giữa-đáy của bản gốc** (dừa 52×62, nhà 103×91).
  Engine vẽ vật thể tại `(x - w/2, y - h)`, đổi khung là vật thể lệch chỗ trên map.
- **Cây dừa: bảng màu lấy từ art có sẵn** — lá/thân của `158.png`.
- **Nhà lá dựng bằng hình khối, không vẽ tay từng pixel**: mọi mặt là đa giác lồi suy
  từ một cái móng hình thoi, đổi tỉ lệ nhà chỉ cần sửa `FOOT_*`. Hàm tô đa giác phải
  nhận CẢ HAI chiều quay — ép một chiều thì nửa số mặt rỗng không.
- **Nhà lá cố ý KHÔNG dùng vàng rơm tươi.** Nền map là đất cát `(190,174,114)`; rơm
  tươi sáng gần đúng tông đó nên cả cái nhà chìm vào nền. Phải hạ sáng, ngả nâu.
- **Ao sen: dáng và màu lấy mẫu từ ảnh tham chiếu người chơi gửi**, không phải từ jar:
  hồ méo (hợp của 5 đĩa chồng nhau) chứ không phải bầu dục; mặt nước phẳng
  `(33,172,191)` với vành mép mỏng; **không có bờ cát** — nước giáp thẳng nền; lá sen
  TO, gân toả từ tâm, mép dưới có vệt nước sáng; hoa sen vẽ TỪNG CÁNH (6 cánh quanh
  nhị vàng). Vẽ hoa bằng mấy vòng tròn đồng tâm thì ra cái bia bắn — đã thử.
- **Bỏ hẳn hai màu tuyết** `(56,193,234)`/`(55,153,182)` trên cây dừa: map đã chuyển
  sang nền đất, để tuyết lại thì cây dừa đội tuyết.
- **Chỗ đặt ao là kết quả quét, không phải chọn tay.** Quét toàn map 15 tìm ô 132×84
  không đụng tán cây (lề 26 px ngang, 62 px dọc) và không đụng cổng dịch chuyển, ra
  đúng một vùng quanh `(494..506, 264..276)`; code chốt `(500, 268)`. **Đổi cỡ ao hay
  đổi số trong `MapRenderer.Decor.cs` thì phải quét lại** — vùng trống chỉ vừa đủ.
- **Ao chỉ là cảnh, không chặn đường.** Lớp va chạm nằm trong `.dat` mà client không
  sửa, nên nhân vật đi xuyên qua ao được.

## Mây tuyết bị giấu

Map 15 có hai cụm mây tuyết (`30`, `34`) đặt ngay trên mái nhà. Đổi sang nhà lá thì
chúng thành đống tuyết đọng trên mái tranh, nên `MapRenderer.BuildObjects` bỏ qua dải
`30..36` ở map này — đúng cách Thành Phố Linh Thú đang làm.

## Còn sót tuyết

Map 15 vẫn còn vật thể mùa đông chưa đụng tới: gốc cây `164`, khúc gỗ `165`, tảng đá
`186`, biển chỉ đường `168`. Muốn hết tuyết hẳn thì thêm bản `14xxx` cho từng cái rồi
khai báo thêm một dòng trong `TropicalObjects`.
