# Xác minh Caro / Cờ tướng / Tiến lên / Phỏm

Ngày xác minh: 2026-09-13.

## Kết luận

Bốn mini-game **không phải chức năng đang hoạt động trong client JAR và server hiện tại**,
vì vậy **không phải khoảng trống migrate JAR → Unity**.

`MiniGamePackets.OpenMiniGame` của Unity là stub dựa trên suy luận sai. Packet
`PET_SERVICE / 2 / gameType` thực tế là `REQUEST_SHOP / shopId`; gửi `1..4` sẽ mở lần lượt
shop Vũ khí, Giáp, Mũ và Thức ăn, không mở mini-game.

## Bằng chứng JAR

1. `eg.java:96-106` chỉ gán nhãn cho building type 13–16:
   `Caro`, `Cờ tướng`, `Tiến lên`, `Phỏm`.
2. Khi bấm, `eg.java:253-263` tạo command local `602/603/606/607` rồi gọi listener
   `dv` đang giữ trong field `private dv a` (`eg.java:5`).
3. Dispatcher `dv.a(Object)` (`dv.java:33-57`) chỉ xử lý command:
   `101`, `900`, `1103`, `1204`, `2020`. Không có `602/603/606/607`; nhánh `default`
   không làm gì. Do đó bấm bốn building này trong JAR không gửi packet và không mở screen.
4. Tìm chính xác toàn bộ `*.java` cho `602/603/606/607` chỉ thấy bốn dòng tạo command trong
   `eg.java`; tìm bốn tên game cũng chỉ thấy phần gán nhãn trong cùng file.
5. Không tìm thấy class/asset có tên hoặc dấu hiệu board/card/chess tương ứng trong source/asset JAR.

## Bằng chứng dữ liệu map

Script `scan-map-building-types.js` parse đúng format `JarMapLayout` trên 24 map `11..34`.
Kết quả không có building type `13`, `14`, `15` hoặc `16` ở bất kỳ map nào.

Các building thực sự có trong dữ liệu:

- Map 11: `31, 9, 32, 30, 27, 28, 29`.
- Map 19: `30, 9`.
- Map 22: `9`.
- Map 29: `27, 28, 29, 30, 31, 32`.
- Các map còn lại: không có building local.

Như vậy người chơi cũng không có điểm vào bốn command chết nói trên trong bộ map hiện tại.

## Bằng chứng server

1. Toàn bộ 235 file C# server không có implementation hoặc tên liên quan Caro/Cờ tướng/
   Tiến lên/Phỏm/mini-game.
2. `GopetCMD.REQUEST_SHOP = 2` (`GopetCMD.cs:74`).
3. `GameController.cs:1052-1054` xử lý sub-command 2 bằng
   `requestShop(message.readsbyte())`.
4. JAR `dc.d(int)` (`dc.java:45-51`) cũng tạo `PET_SERVICE / 2 / value`; đây là helper mở
   shop, không liên quan các command local `602/603/606/607` của `eg.java`.

## Sai lệch đã được sửa trong Unity/tài liệu

- Đã xóa `Assets/Scripts/Net/MiniGame/MiniGamePackets.cs`, vì file này mô tả sai rằng
  `dc.d(1..4)` mở mini-game.
- Đã xóa `P8PacketsTests.OpenMiniGame_81_2_SbyteType`; test cũ chỉ chứng minh stub serialize
  được packet và nếu chạy live thì packet mở shop tương ứng.
- Một số báo cáo/phase cũ đã nối nhầm command local `602/603/606/607` với helper shop
  `dc.d(1..4)`, rồi ước lượng xây bốn game. Nhận định đó không phù hợp code hiện tại.

## Hành động đã thực hiện

1. Đã loại bốn mini-game khỏi checklist parity và ước lượng migrate.
2. Đã xóa `MiniGamePackets` và test liên quan để tránh gửi nhầm request shop.
3. Đã sửa `BuildingDispatcher` type 13–16 thành `Noop` kèm comment `JAR-inert`.
4. Báo cáo certification trỏ về kết luận này làm nguồn chính.
5. Nếu muốn có bốn game trong tương lai, cần lập epic **Unity/server feature mới**, bao gồm luật,
   matchmaking, protocol state/move/result, persistence, anti-cheat, UI và asset. Không gọi là migrate JAR.
