# Icon nút đánh quái

```bash
cd GopetUnityClient/tools
python image-gen/make-attack-button.py      # hoặc: npm run gen:attack-icon
```

Ra `Assets/Resources/Ui/attack-button.png` (128×128): mặt nút tròn bóng + hai
thanh kiếm bắt chéo.

## Ràng buộc khi sửa

- Bảng màu lấy thẳng từ `Ui/dpad.png` / `Ui/dpad-knob.png` — nút đánh đứng cùng
  màn với cần điều khiển nên phải cùng một bộ màu trắng-xanh.
- Ảnh VUÔNG và icon phải ăn kín khung: `AttackButton` vẽ nó ở 84×84 với
  `preserveAspect`, ảnh lệch tỉ lệ sẽ để lại khoảng trống lệch một bên.
- Chừa vài pixel trong suốt quanh mép: vành nút sát biên ảnh thì lúc thu nhỏ
  xuống 84px rìa bị cắt cụt.
- Kiếm xoay ±40° nên phải vẽ ở độ phân giải gấp 8 rồi thu nhỏ bằng LANCZOS,
  không thì lưỡi kiếm răng cưa.
