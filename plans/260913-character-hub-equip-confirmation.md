# Kế hoạch xác nhận và trang bị quần áo/cánh trong HUD Nhân vật

## 1. Mục tiêu

Trong popup **Hành trang**, tab **Nhân vật**:

- Khi người chơi chọn một món trong **Tủ quần áo**, hiển thị popup hỏi có sử dụng món đó hay không.
- Khi người chơi chọn một món trong **Cánh**, hiển thị popup xác nhận tương tự.
- Chỉ gửi yêu cầu trang bị sau khi người chơi bấm **Đồng ý**.
- Khi server chấp nhận, ngoại hình hoặc cánh mới phải được cập nhật ngay trên nhân vật.
- Nếu bấm **Không**, bấm ra ngoài, đổi tab hoặc đóng Hành trang thì không gửi yêu cầu trang bị.

## 2. Luồng chuẩn đối chiếu từ game Java/JAR

### 2.1. Tủ quần áo

- Client mở Tủ quần áo bằng packet `PET_SERVICE / 62`.
- Mã tương ứng trong JAR: `fr.java`, nhánh lệnh `330`.
- Server trả về `MenuScreen` có `listId = 803`.
- Mỗi dòng vật phẩm đã có các trường do server cung cấp:
  - `showDialog`
  - `dialogText`
  - `leftCmdText`
  - `rightCmdText`
  - `closeScreenAfterClick`
  - `itemId`
- Khi đồng ý sử dụng, client gửi `SELECT_MENU_ELEMENT` với:
  - `listId = 803`
  - `itemId` của dòng được chọn.

### 2.2. Cánh

- Client mở kho Cánh bằng packet `PET_SERVICE / 92 / 2`.
- Màn hình Cánh trong JAR dùng `listId = 81040`, được xử lý trong `fj.java`.
- Khi sử dụng cánh, JAR gửi:

  `PET_SERVICE / WING / WING_TYPE_USE / inventoryIndex`

- Giá trị protocol:
  - `WING = 92`
  - `WING_TYPE_INVENTORY = 2`
  - `WING_TYPE_USE = 4`
  - `WING_TYPE_UNEQUIP = 5`
  - `WING_TYPE_ENCHANT = 6`

### 2.3. Cập nhật ngoại hình

Server đã có sẵn nghiệp vụ:

- Đưa trang phục/cánh cũ trở lại kho.
- Lấy món mới ra khỏi kho và gắn vào nhân vật.
- Tính lại chỉ số pet nếu cần.
- Gửi cập nhật skin bằng `SEND_SKIN`.
- Gửi cập nhật cánh bằng `WING / 3`.

Vì vậy không cần thay đổi cấu trúc database hoặc tự cập nhật ngoại hình ở client trước khi server phản hồi.

## 3. Hiện trạng và nguyên nhân

`GenericMenuView` đã hỗ trợ cờ `showDialog` và phát sự kiện `ConfirmRequested`. `UiRoot.ShowMenu()` cũng đã nối sự kiện này với `ChoiceDialogView` cho các menu thông thường.

Tuy nhiên, menu `803` và `81040` đang được `CharacterHubPopupView.TryConsumeMenu()` chặn lại và nhúng trực tiếp vào popup Hành trang. Do đó chúng không đi qua `UiRoot.ShowMenu()`, khiến phần đăng ký `ConfirmRequested` bị bỏ qua.

Ngoài ra, luồng Cánh chuyên biệt trong `UiRoot.Wings.cs` chưa được áp dụng cho danh sách Cánh nhúng trong HUD.

## 4. Thiết kế triển khai

### 4.1. Popup xác nhận trong Hành trang

Bổ sung một lớp điều phối xác nhận trong `CharacterHubPopupView`:

- Dùng lại `ChoiceDialogView` để đồng nhất với các popup xác nhận hiện tại.
- Popup phủ lên toàn bộ Hành trang và chặn raycast xuống danh sách bên dưới.
- Nội dung ưu tiên lấy từ `MenuItemInfo.DialogText` do server gửi.
- Nhãn nút ưu tiên lấy từ `LeftCommandText` và `RightCommandText`.
- Nếu server không gửi nhãn phù hợp, dùng mặc định:
  - `Đồng ý`
  - `Không`
- Chỉ cho tồn tại một popup xác nhận tại một thời điểm.

Thêm trạng thái:

- `_confirmationDialog`: dialog đang mở.
- `_equipRequestPending`: ngăn double-click hoặc gửi hai yêu cầu liên tiếp.

### 4.2. Đăng ký sự kiện cho menu nhúng

Trong `CharacterHubPopupView.BindMenu()`:

- Khi tạo `GenericMenuView`, đăng ký `ConfirmRequested` đúng một lần.
- Khi người chơi xác nhận:
  1. Đóng popup xác nhận.
  2. Đánh dấu yêu cầu đang gửi.
  3. Thực hiện callback gửi packet.
  4. Xử lý `CloseScreenAfterClick` theo đúng dữ liệu server.
- Khi người chơi hủy, chỉ đóng popup xác nhận và giữ nguyên danh sách.

### 4.3. Trang bị quần áo

Đối với `listId = 803`:

- Giữ nguyên quyết định của `MenuSelection.Decide()`.
- Nếu dòng có `ShowDialog = true`, mở popup xác nhận.
- Khi đồng ý, gọi `GuiderHandler.Select(screen, index)`.
- `GuiderHandler` phải gửi `MenuItemInfo.ItemId`, không gửi vị trí dòng Unity tự tính.
- Sau khi gửi, đóng danh sách Tủ quần áo nếu `CloseScreenAfterClick = true`, nhưng giữ popup Hành trang mở.
- Chờ `CharacterSkinHandler` nhận `SEND_SKIN` rồi cập nhật `CharacterSkinLayer`.

### 4.4. Trang bị cánh

Đối với `listId = 81040`:

- Gắn `SelectionOverride` cho `GenericMenuView` đang nhúng trong HUD.
- Truyền cùng một instance `WingHandler` từ `GopetBootstrap`/`GameSession` vào `CharacterHubPopupView`.
- Khi chọn cánh chưa trang bị:
  1. Hiển thị popup xác nhận.
  2. Nếu đồng ý, gọi `WingHandler.Use(item.ItemId)`.
  3. Packet gửi lên phải là `PET_SERVICE / 92 / 4 / itemId`.
  4. Đóng danh sách hiện tại giống hành vi của JAR.
  5. Chờ server trả menu mới và `WING / 3`.
- `CharacterWingLayer` chỉ cập nhật sau phản hồi `WING / 3` của server.

### 4.5. Vật phẩm đang được sử dụng

Server đánh dấu trang phục hoặc cánh đang dùng bằng `itemId = -1` và thêm trạng thái “đang sử dụng” vào tiêu đề.

Hành vi dự kiến:

- Trang phục đang dùng:
  - Không hiển thị câu hỏi “Sử dụng”.
  - Hiển thị xác nhận `Bạn có muốn tháo [Tên trang phục] không?`.
  - Khi đồng ý, gửi lựa chọn `listId = 803`, `itemId = -1` để server mở/tiếp tục luồng tháo trang phục hiện có.
- Cánh đang dùng:
  - Hiển thị xác nhận `Bạn có muốn tháo [Tên cánh] không?`.
  - Khi đồng ý, gọi `WingHandler.Unequip()`.
  - Packet phải là `PET_SERVICE / 92 / 5` và không có `itemId` phía sau.

### 4.6. Vòng đời popup

- Bấm **Không**: đóng xác nhận, trở lại đúng vị trí cuộn trước đó.
- Bấm ngoài popup: xử lý như **Không**.
- Đổi tab: đóng và hủy callback xác nhận cũ.
- Đóng Hành trang: hủy popup xác nhận và trạng thái pending.
- Khi server trả menu Cánh/Tủ quần áo mới: bind lại dữ liệu, bỏ trạng thái pending.
- Khi server trả lỗi hoặc thông báo: bỏ trạng thái pending để người chơi có thể thử lại.

## 5. Các file dự kiến thay đổi

### Unity client

- `GopetUnityClient/Assets/Scripts/Runtime/UI/CharacterHubPopupView.cs`
  - Lưu `WingHandler`.
  - Quản lý dialog xác nhận và trạng thái pending.
  - Nối `ConfirmRequested` cho menu nhúng.
  - Gắn override cho danh sách Cánh.
- `GopetUnityClient/Assets/Scripts/Runtime/UI/CharacterHubPopupView.Content.cs`
  - Điều chỉnh vòng đời danh sách Tủ quần áo/Cánh nếu cần.
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.CharacterHub.cs`
  - Truyền `WingHandler` vào popup Hành trang.
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs`
  - Lưu instance `WingHandler` dùng chung thay vì chỉ truyền cho `CharacterWingLayer`.
- `GopetUnityClient/Assets/Scripts/Runtime/UI/ChoiceDialogView.cs`
  - Chỉ sửa nếu cần hỗ trợ đóng khi bấm nền hoặc expose trạng thái phục vụ test.

### Server

Không dự kiến sửa server. Chỉ sửa nếu kiểm thử thực tế chứng minh packet hiện tại không trả đủ cập nhật hoặc không lưu trạng thái.

## 6. Kế hoạch kiểm thử

### 6.1. Tủ quần áo

1. Mở HUD → Nhân vật → Tủ quần áo.
2. Chọn một trang phục chưa dùng.
3. Xác nhận popup xuất hiện và chưa có packet nào được gửi.
4. Bấm **Không**:
   - Không gửi packet.
   - Danh sách vẫn mở.
   - Vị trí cuộn được giữ nguyên.
5. Chọn lại và bấm **Đồng ý**:
   - Gửi đúng `SELECT_MENU_ELEMENT / 803 / itemId`.
   - Nhân vật đổi ngoại hình sau khi nhận `SEND_SKIN`.

### 6.2. Cánh

1. Mở HUD → Nhân vật → Cánh.
2. Chọn một cánh chưa dùng.
3. Xác nhận chưa gửi packet trước khi đồng ý.
4. Bấm **Không** và xác nhận không có packet.
5. Bấm **Đồng ý** và xác nhận packet:

   `PET_SERVICE / 92 / 4 / inventoryIndex`

6. Xác nhận `CharacterWingLayer` hiển thị cánh sau khi nhận `WING / 3`.
7. Chọn cánh đang sử dụng, đồng ý tháo và xác nhận packet:

   `PET_SERVICE / 92 / 5`

### 6.3. Trường hợp biên

- Double-click một vật phẩm chỉ gửi một packet.
- Đổi tab khi dialog đang mở không thực thi callback cũ.
- Đóng HUD khi dialog đang mở không để lại backdrop chặn màn hình.
- Danh sách dài vẫn giữ cuộn đúng sau khi hủy xác nhận.
- Không cập nhật ngoại hình giả ở client nếu server từ chối.
- Trang phục/cánh cũ quay lại kho sau khi trang bị món mới.

## 7. Tiêu chí hoàn thành

- Cả Tủ quần áo và Cánh đều hỏi xác nhận trước khi sử dụng.
- Bấm **Không** tuyệt đối không gửi packet.
- Bấm **Đồng ý** gửi đúng protocol JAR cho từng loại.
- Trang phục/cánh được cập nhật trên nhân vật bằng phản hồi từ server.
- Không có double-send, popup rác hoặc mất tương tác sau khi đổi tab/đóng HUD.
- Toàn bộ unit test và PlayMode test liên quan đều vượt qua.

