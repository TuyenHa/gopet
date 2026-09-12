# Báo cáo triển khai parity JAR → Unity

Ngày kiểm tra: 2026-09-12.

## Đã hoàn thành

| Nhóm | Kết quả |
|---|---|
| Packet black hole | Đã xử lý `CHAT_GLOBAL`, `MAGIC`, `GYM`, `UP_TIEM_NANG`, `TATTOO/1,7`, `USE_EQUIP_ITEM`, `UNEQUIP_ITEM`, `REMOVE_ITEM_EQUIP`, `ON_UNQUIP_GEM` |
| Pet | Có profile self/other, Gym và cập nhật stat tại chỗ |
| Tattoo | Có danh sách slot, tạo/xoá, chọn lần lượt 2 nguyên liệu và gửi confirm enchant |
| Equipment/gem | Nhận delta và chủ động refresh snapshot đang mở |
| Chat | Nhận global, transcript khu vực 50 dòng và cộng đồng 200 dòng, vẫn giữ bubble world |
| Mail | Đọc full content, mark, xoá, soạn/gửi và refresh mailbox |
| Settings | Tách music/effect, đổi VN/EN, auto-attack gửi ngay và lặp 4 giây |
| Session | Logout đóng transport và reload scene login |
| Building | Type 12 mở mailbox; type 32 mở `MAGIC` đúng hành vi JAR |

## Bằng chứng tự động

- `verify.ps1`: 10/10 bước pass.
- Unit test: 650/650 pass.
- Net, Runtime UnityCompat, Editor, PlayMode test và LiveSmoke: build 0 warning/error.
- LiveSmoke server thật: pass toàn bộ, gồm 5 vòng connect/dispose không còn
  `ESTABLISHED`/`CLOSE_WAIT`, login, menu, warp, ChallengePlace, MarketPlace, battle,
  PvP và ba check mới `MAGIC`, `GYM`, `LETTER_BOX`.
- Asset gate: 54 ảnh DAT, 327 PNG, 10 WAV, 24 map, 9 map animation,
  27 battle animation và 6 actor animation khớp nguồn JAR.

## Chưa thể chứng nhận tự động

- Chạy PlayMode test bên trong Unity Editor (project compile được nhưng CLI gate hiện chỉ compile).
- Screenshot-diff 16:9, 19.5:9 và 4:3 cho 24 map.
- Nghe và xác nhận timing/volume 10 SFX bằng thiết bị thật.
- ArenaPlace đúng cửa sổ sự kiện và ClanPlace bằng tài khoản fixture có bang.
- Global chat hai client, mail gửi/mark/xoá và tattoo enchant end-to-end cần tài khoản
  có gold/material/pet đủ level; parser và wire tương ứng đã có unit test.

Các mục trên là certification phụ thuộc Editor, thiết bị, thời điểm sự kiện hoặc dữ liệu
tài khoản; không còn là phần code client bị bỏ trống.
