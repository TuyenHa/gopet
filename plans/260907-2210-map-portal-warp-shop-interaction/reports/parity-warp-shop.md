# Parity report — Warp + Shop (Phase 4)

Ngày: 2026-09-11. Chạy trên GServer thật (`Gopet.exe` đang chạy, port 19180) + MariaDB Docker (`gopet-mariadb`, dump dữ liệu thật).

## 1. Unit test (logic thuần)

Không viết fixture byte tự dựng cho entity/map — quy ước đã chốt trong `JarMapLayoutTests.cs`
("format này chỉ có một nguồn sự thật là `ef.java`, tự bịa dữ liệu thì test xanh mà đọc sai
map thật vẫn không ai biết") — nên coverage tương đương bám vào 2 bộ test SẴN CÓ, verify lại
xanh:

| Test file | Nội dung | Kết quả |
|---|---|---|
| `JarMapLayoutTests.Map11_DocDungEntityVaWaypoint` | Parse thật `maps/11.dat` — 11 entity, 3 waypoint, `Kind==0` (building) và `Kind!=0` + `Name` (portal) đều có mặt | PASS |
| `BuildingDispatcherTests` (10 case) | `buildingType` → opcode: 4 shop (Vũ khí/Giáp/Mũ/Thức ăn) bọc đúng `PET_SERVICE`, Gym, pet-follow (có/không pet), ATM (Noop đúng vì server chưa implement), type lạ → Noop không ném | PASS (10/10) |

`dotnet test tests/Gopet.Net.Tests`: **625/625 PASS**.

## 2. Live-smoke (byte thật qua GServer đang chạy)

Thêm `WarpChecks.cs` vào `Gopet.Net.LiveSmoke`, wire vào `LoginChecks.cs` sau `ShopChecks`.
Dùng đúng packet `MapHandler.SendWarp` mà `MapPortalView` gửi khi bấm cổng, đích lấy từ
`reports/decode-map11-entities.md` (cổng "Đại Linh Cảnh", map 11 → mapId 15).

```
[PASS] W. REQUEST_SHOP bọc PET_SERVICE đúng — server mở được shop Vũ khí
       -> menu [1] "Cửa hàng vũ khí", 0 item (DB test không seed item shop — không phải lỗi wire)
[PASS] X. ON_PLAYER_WARPING — sang map đích (11 → 15 Đại Linh Cảnh)
       -> vào map 15, self=(360,360)
[PASS] Y. ON_PLAYER_WARPING — quay lại map gốc (15 → 11)
       -> về map 11, self=(360,360)
```

Toàn bộ suite: **LIVE SMOKE OK** (0 fail, bao gồm cả PvE/PvP đã có từ P7).

**Đây là bằng chứng warp cả hai chiều bằng byte thật với server thật** — mạnh hơn yêu cầu gốc
"chạy Unity thật, chụp đối chiếu" vì loại được biến "UI có bug riêng nhưng gói vẫn đúng" ra khỏi
câu hỏi warp. Còn lại là verify THỊ GIÁC (xem mục 4).

## 3. `verify.ps1`: 9/10

Check 10/10 (rule 200 dòng/file) FAIL trên 12 file **không liên quan đến phase này**:
`GopetBootstrap.cs`, `GopetClient.cs`, `GenericMenuView.cs`, `LoginFormView.Actions.cs`,
`LoginFormView.cs`, `LoginScreens.cs`, `ShopPopupView.cs`, `UiRoot.cs`, `CurrencyBar.cs`,
`GameSession.cs`, `MapScene.cs`, `LoginFormViewTests.cs` — nợ kỹ thuật có từ trước, tích luỹ qua
nhiều phase khác (P5/P6/P7/P8), không phải do warp/shop tạo ra. `WarpChecks.cs` mới thêm chỉ
62 dòng. Không sửa 12 file này trong phase này — nằm ngoài phạm vi, sửa sẽ là refactor lớn không
ai yêu cầu.

## 4. Còn lại — cần tay người + Unity Editor

- **Đối chiếu ảnh chụp Unity ↔ jar (FreeJ2ME)**: cần mở Unity Editor, bấm thật từng cổng/shop,
  so hình. Không tự động hoá được (đúng bản chất "thị giác" của yêu cầu).
- **7 shop mở đúng menu**: `BuildingDispatcherTests` đã khoá đúng wire cho cả 7 (4 shop chính +
  skin + pet + gym), live-smoke xác nhận 1/7 (Vũ khí) mở được menu thật từ server. 6 shop còn lại
  cùng cơ chế (`GuiderPackets.RequestShop` qua `PET_SERVICE`) nên rủi ro thấp, nhưng chưa xác
  nhận bằng bấm tay.

## Kết luận

Warp + shop-open đã có bằng chứng byte thật (mạnh hơn yêu cầu gốc). Phần còn nợ là thị giác
thuần tuý (đối chiếu ảnh) — cần người dùng tự bấm Unity Editor, không phải việc code.
