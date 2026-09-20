# Icon điểm đến trên bản đồ thế giới

Sinh bằng script, không phải bằng AI:

```bash
cd GopetUnityClient/tools
python image-gen/make-world-map-pins.py     # hoặc: npm run gen:map-pins
```

Cần `pillow` (`pip install pillow`). Script ghi đè thẳng vào
`Assets/Resources/Ui/WorldMap/`, chạy lại bao nhiêu lần cũng ra đúng file đó.

| File | Cỡ | Dùng cho |
|------|----|----------|
| `world-map-pin.png` | 64×80 | Map mở khoá — pin vàng, lòng kem |
| `world-map-pin-current.png` | 64×80 | Map đang đứng — pin xanh ngọc, ngôi sao trắng |

## Vì sao vẽ bằng script

Icon chỉ hiện ở cỡ 18×22 trên màn. Ảnh sinh bằng AI ở cỡ đó rìa lem, alpha bẩn,
lần nào cũng phải dọn tay và không tái tạo lại được. Script thì đổi màu/tỉ lệ là
chạy lại một dòng, và diff của asset luôn giải thích được.

Hình vẽ ở độ phân giải gấp 8 lần rồi thu nhỏ bằng LANCZOS — vẽ thẳng ở cỡ thật
thì đường cong của giọt nước răng cưa thấy rõ.

## Ràng buộc khi sửa

- Giữ tỉ lệ 64×80: `WorldMapView.PinWidth/PinHeight` (18×22) bám theo tỉ lệ này,
  đổi tỉ lệ ảnh mà quên đổi hằng số là pin bị bóp méo.
- Mũi nhọn phải chạm đúng mép dưới ảnh: pin neo bằng mũi, lệch là điểm đánh dấu
  trỏ sai chỗ trên tranh.
- Viền tối dày 3px là bắt buộc — tranh nền nhiều màu, bỏ viền thì icon chìm.
- Mọi thứ trong `Assets/Resources/Ui/` được `LoginAssetImportSettings` ép thành
  Sprite, lọc Bilinear, không nén. Không cần chỉnh importer bằng tay.
