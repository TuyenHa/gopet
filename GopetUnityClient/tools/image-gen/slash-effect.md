# Vệt chém của nút đánh quái

```bash
cd GopetUnityClient/tools
python image-gen/make-slash-effect.py   # hoặc: npm run gen:slash-fx
```

Ghi ra `Assets/Resources/Ui/fx-slash.png` (192×192): vệt lửa vàng quét thành vòng cung
rộng, hướng sang **phải (+X)**.

Dựng theo clip tham chiếu người chơi gửi (game 2D mobile). Chỉ một ảnh cho mọi hướng —
`Runtime/World/WorldSlashEffect` xoay theo góc pet → quái, lại quay thêm một nhịp
(−42° → +34°) cho ra động tác vung tay. Cỡ vẽ trong world quy ra từ `sprite.bounds`
chứ không hằng số hoá, nên đổi `pixelsPerUnit` lúc import cũng không làm vệt to nhỏ
bất thường.

## Ràng buộc khi sửa

- **NHIỀU SỢI chồng lệch nhau, không phải một dải đặc.** Tám sợi lệch bán kính/góc/bề
  dày; chính chỗ lệch đó cho chất "cuộn" của lửa. Gộp thành một dải trơn là ra hình
  trăng khuyết, không ra nhát chém.
- **Quầng sáng dựng từ chính mấy sợi** (làm mờ Gauss rồi nhuộm vàng cam, đặt DƯỚI lớp
  lửa) — vẽ quầng riêng thì nó không bám theo hình vệt.
- **Bảng màu lấy mẫu từ clip**: lõi `(255,252,214)` gần trắng, thân vàng chanh
  `(248,240,64)`, rìa `(250,196,36)`, quầng `(252,176,24)`.
- **Đốm sao bốn cánh** rải quanh cung là chi tiết của clip gốc, bỏ đi là vệt trông
  trơ; nhưng để to quá thì át cả vệt.
- **Vẽ gấp `SUPERSAMPLE` lần rồi thu nhỏ bằng LANCZOS.** Vệt cong vẽ thẳng ở cỡ thật
  thì mép răng cưa thấy rõ.
- File nằm trong `Resources/Ui/` nên `LoginAssetImportSettings` tự ép Sprite +
  Bilinear. Để chỗ khác phải tự lo import setting, `Resources.Load<Sprite>` trả null
  là mất hiệu ứng mà không có lỗi rõ ràng.
