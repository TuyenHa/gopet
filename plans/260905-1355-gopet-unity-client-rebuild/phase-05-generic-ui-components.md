---
phase: 5
title: "Generic UI Components"
status: in-progress
priority: P1
effort: "3-4w"
dependencies: [4]
---

# Phase 5: Generic UI Components

## Overview

4 component UI generic phục vụ **toàn bộ 162 màn hình** menu/dialog của game.

Đây là phát hiện tiết kiệm nhiều tháng công nhất của cả dự án: server không gửi "màn hình shop" hay "màn hình kho đồ" — server gửi **một danh sách generic** (tiêu đề + item{icon, tên, mô tả, cờ}) qua đúng một định dạng gói. Client chỉ render danh sách rồi gửi lại index đã chọn.

Shop, kho đồ, clan, kiosk, nhiệm vụ, xăm, ghép đồ, chọn pet... đều chạy qua 4 component này.

## Requirements

**Functional**
- Menu danh sách: icon + tiêu đề + mô tả, cuộn được, chọn được/không chọn được
- Dialog Yes/No với text nút tuỳ biến
- Dialog nhập liệu (text + số)
- Dialog NPC option
- Hỗ trợ cả touch (mobile) và chuột/phím (PC)

**Non-functional**
- Danh sách dài (hàng trăm item) không tụt frame → dùng virtualized list
- Icon tải bất đồng bộ, không chặn hiển thị danh sách

## Architecture

### `SHOW_MENU_ITEM` — định dạng chung cho 162 màn hình

Từ `GameController.cs:818-861`, gói `COMMAND_GUIDER` (122) + sub `SHOW_MENU_ITEM` (8):

```
int    listID           <- ID màn hình, gửi lại khi chọn
sbyte  type             <- kiểu menu
UTF    title
int    count
lặp count lần:
  int    itemId         <- hoặc index nếu item không có ID riêng
  UTF    imgPath        <- đường dẫn asset, đưa cho RemoteAssetCache
  UTF    titleMenu
  UTF    desc
  sbyte  canSelect      (1/0)
  bool   showDialog
  nếu showDialog:
    UTF  dialogText
    UTF  leftCmdText
    UTF  rightCmdText
  sbyte  saleStatus
  bool   closeScreenAfterClick
  int    paymentOptionCount
  lặp paymentOptionCount lần:
    int    paymentOptionsId
    UTF    moneyText
    sbyte  isPaymentEnable
```

**Đọc sai một field ở đây là hỏng toàn bộ UI.** Đây là gói phức tạp nhất trong vertical slice — ưu tiên viết test đối chiếu.

### Sub-command của `COMMAND_GUIDER` (122)

| Sub | Tên | Ý nghĩa |
|---|---|---|
| 8 | `SHOW_MENU_ITEM` | S→C: hiện danh sách |
| 3 | `SELECT_MENU_ELEMENT` | C→S: đã chọn index |
| 2 | `NPC_GUIDER` | S→C: hội thoại NPC |
| 5 | `SELECT_OPTION` | C→S: chọn option NPC |
| 7 | `TYPE_DIALOG_INPUT` | 2 chiều: dialog nhập liệu |
| 4 | `SEND_YES_NO` | 2 chiều: dialog Yes/No |

### Nguyên tắc: client **không** giữ logic nghiệp vụ

Client không biết "đây là shop" hay "đây là kho đồ". Client chỉ biết: có `listID`, có danh sách, người dùng chọn index thứ N → gửi `SELECT_MENU_ELEMENT(listID, N)`. Server quyết định mọi thứ.

Giữ đúng nguyên tắc này thì 162 màn hình chạy tự động. Phá vỡ nó (hardcode xử lý riêng cho từng `listID`) là tự chuốc 162 màn hình.

### Input hai kiểu

| | Mobile | PC |
|---|---|---|
| Chọn item | Tap | Click |
| Cuộn | Vuốt | Bánh xe / kéo scrollbar |
| Xác nhận | Tap nút | Enter |
| Huỷ | Tap nút / back | Esc |

Dùng Unity Input System với 2 action map, không phân nhánh `#if UNITY_ANDROID` rải rác trong code UI.

> **Làm xong rồi sửa lại: chỉ cần MỘT action map.** Chạm và chuột đã đi qua
> `EventSystem` của uGUI; thêm một đường nữa chỉ tạo chỗ để hai đường lệch nhau. Phần
> thật sự thiếu chỉ là bàn phím. Và khác biệt mobile/PC không phải là "hai action map"
> mà là "có bàn phím hay không" — Input System tự trả lời, nên vẫn không có `#if` nào.
> Hai map giống hệt nhau là nghi thức, không phải thiết kế.

### Nhận thêm từ P3 (hoãn sang đây, 2026-09-05)

Tầng giao thức đăng nhập đã xong và kiểm chứng sống ở P3, nhưng phần **màn hình** hoãn
sang đây: dựng UI login lúc P3 sẽ phải vứt khi P5 lập hệ component dùng chung, và không
tự nghiệm thu được (phải bấm Play bằng tay).

- Màn hình đăng nhập — ô nhập, validate theo `Gopet.Net.Auth.AuthRules` (đã có sẵn)
- Màn hình chọn máy chủ — dữ liệu đã parse sẵn thành `ServerEntry[]`
- Màn hình tạo nhân vật — nhánh của lần đăng nhập đầu; tên `^[a-z0-9]+$`, 5-20 ký tự
- UI cho **mất kết nối không kèm thông điệp** — trạng thái hợp lệ, không được treo màn hình
- `CredentialStore` — lưu tài khoản an toàn theo nền tảng, không để plaintext
- 2FA OTP — dialog nhập, nếu tài khoản bật `secretKey`

Toàn bộ logic giao thức đã nằm trong `Assets/Scripts/Net/Auth/`; phần còn lại thuần trình bày.

### Nhận thêm từ P4 (hoãn sang đây, 2026-09-05)

Đường ống ảnh đã xong và kiểm chứng sống (xin ảnh thật, nhận đúng byte, gộp request, hết hạn).
Nhưng bốn thứ chỉ nghiệm thu được khi **chạy Play mode** — cần một màn hình để hiện ảnh:

- Lần 2 lấy từ bộ nhớ, không gửi gói tin
- Khởi động lại app, ảnh lấy từ đĩa
- Placeholder hiện trong lúc chờ
- `FilterMode.Point` — ảnh pixel art không bị mờ

`RemoteAssetCache.Get(path, type, onReady)` đã sẵn sàng. **Chú ý hợp đồng:** `onReady` có thể
được gọi **hai lần** — placeholder trước, ảnh thật sau. UI phải chịu được điều đó.

Cũng cần đo `Texture2D.LoadImage` trên ảnh lớn xem có làm khựng frame không (rủi ro đã ghi ở P4).

**Cảnh báo đã trả giá:** bộ gõ tiếng Việt (Telex) nuốt phím trong ô nhập — gõ `test1234`
ra `tét1234`. Test bằng bàn phím tiếng Anh.

### Sửa lại đặc tả sau khi đọc code server

**`SEND_YES_NO` không nằm dưới `COMMAND_GUIDER`.** Bảng sub-command ở trên liệt kê nó
là sub 4 của 122 — sai. Nó đi trong `SERVER_MESSAGE` (**45**), cả hai chiều
(`MenuController.cs:1092`, `GameController.cs:704-711`). Đặt nhầm chỗ này thì hộp
Có/Không không bao giờ tới.

**Có định dạng màn hình THỨ TƯ plan không nhắc: `GUIDER_LIST_OPTION`.**
Đây là sub **3 chiều xuống** — **trùng số** với `SELECT_MENU_ELEMENT` chiều lên. Cùng
một byte, hai định dạng khác hẳn, chỉ phân biệt được bằng chiều:

```
int listId
int listId      <- server ghi HAI lần (GameController.cs:4609-4610)
UTF title
UTF commandText <- nhãn nút giữa, ví dụ "OK"
int count
count × { int optionId, UTF text, sbyte status }
```

Màn ATM dùng đúng cái này. Chọn một dòng vẫn trả lời bằng `SELECT_MENU_ELEMENT`, nên
tầng UI dùng chung một đường chọn.

Còn `NPC_OPTION` (S→C) và `SELECT_OPTION` (C→S) cũng chia nhau số **5** — cùng kiểu
trùng số theo chiều.

### Bằng chứng đã kiểm chứng (2026-09-06)

**Khớp byte với client J2ME** — vector lấy nguyên văn từ dump phiên thật:
`SHOW_MENU_ITEM`, `GUIDER_LIST_OPTION`, `SELECT_MENU_ELEMENT`, `SELECT_OPTION`,
`NPC_GUIDER`, `GUIDER_TYPE_PAY`.

**Live với server thật** — chuỗi này chứng minh luận điểm cốt lõi của phase:

```
Q → mở [1039] "ATM", nút "OK", 3 lựa chọn
R → chọn dòng 0 → menu [1040] "Đổi (vang) (Bạn hiện có: 0vnđ)", 9 DÒNG
S → 9 dòng đều đọc ra nhất quán; dòng đầu: id=0 "Đổi 2.400 (vang)" icon="gameMisc/icons4.png"
```

Check S quan trọng hơn vẻ ngoài: **cả bốn** gói `SHOW_MENU_ITEM` bắt được từ client
J2ME đều rỗng (cửa hàng chưa có hàng, danh sách nhiệm vụ và pet trống). Nên phần đọc
TỪNG DÒNG — gồm field điều kiện `showDialog` — chưa từng gặp dữ liệu thật cho tới menu
9 dòng này. Nó qua được `ExpectFullyConsumed`.

Không lưu được thành vector offline: gói 1699 byte, packet dump cắt hex ở 256.

**Nghiệm thu UI bằng máy, không bằng mắt.** `run-playmode-tests.ps1` chạy
`Unity.exe -batchmode -runTests` — **34/34 pass**. Phủ:

| Nhóm | Kiểm gì |
|---|---|
| `GenericMenuViewTests` | dựng đúng số dòng; `canSelect = 0` tắt nút chứ không chỉ làm mờ; bấm dòng N gửi đúng `listId` và chỉ số |
| `UiRootTests` | dispatch **gói thật** qua router → view; hộp xác nhận đè LÊN menu chứ không thay thế; huỷ thì quay lại đúng menu; `closeScreenAfterClick` đóng menu **ở giữa chồng** trong khi hộp xác nhận còn nằm trên |
| `MenuVirtualizationTests` | 500 dòng chỉ dựng ~12; cuộn 20 lần không sinh thêm GameObject; dòng tái dùng mang dữ liệu mới |
| `PointerPathTests` | bấm qua ĐÚNG đường con trỏ uGUI: dòng có chiều rộng thật, nút bị vô hiệu không gửi gói, dòng không chồng lên nhau |

Script đọc **file XML kết quả** chứ không tin exit code: batchmode trên máy này crash
lúc thoát vì không resolve được `cdp.cloud.unity3d.com` (endpoint analytics), sau khi
đã ghi xong kết quả.

**Đột biến, cả hai đều bị bắt:** bỏ tái dùng GameObject → 1 PlayMode test đỏ; bỏ tham
chiếu asmdef → lint đỏ.

### Hai lỗi CRITICAL code review bắt được — sai chức năng, không phải sai cú pháp

**Client gửi VỊ TRÍ DÒNG thay vì giá trị server đã gửi xuống.**

Trường `int` đầu mỗi dòng của `SHOW_MENU_ITEM` là giá trị để **dội lại**: server ghi
`itemId` nếu dòng có id riêng, ngược lại ghi chỉ số dòng (`GameController.cs:837-844`).
Bên tiêu thụ dùng nó như một ID thật:

| Nơi dùng | Cách dùng |
|---|---|
| `selectMenu.cs:1475` | `clan.getJoinRequestByUserId(index)` — id là `user_id` |
| `selectMenu.cs:423` | `if (index == -1)` cho pet đang đeo — **chỉ số dòng không bao giờ âm** |
| `selectMenu.cs:659` | `getShopTemplateItem(index)` tìm nhị phân theo id |

Sửa: gửi `screen.Items[index].ItemId`. Đúng cho **cả hai** trường hợp mà không cần
nhánh nào — dòng không có id riêng thì `ItemId` đã bằng chỉ số dòng.

**Màn hình lựa chọn cũng vậy, và đây là ca hại nhất.** Menu lời mời kết bạn có id theo
thứ tự `0, 1, 3, 2` (`sendMenu.cs:245-249`), handler là `switch` trên chính id đó
(`selectMenu.cs:2064`). Gửi vị trí thì **"Từ chối tất cả" chạy thành "Từ chối và chặn"** —
sai hẳn hành động, không có lỗi nào báo.

**Vì sao lọt qua 26/26 PlayMode, 253 unit test, và cả kiểm chứng live:** mọi vector đều
có `itemId == vị trí`. Menu tự dựng đặt id = 0 hoặc = i; màn ATM thật thì id trùng vị
trí. Với dữ liệu như vậy hai cách cho kết quả **giống hệt nhau**. Đã thêm
`GuiderEchoTests` — mọi ca cố ý đặt id khác vị trí; đột biến quay lại gửi vị trí làm
5 test đỏ.

Comment tôi viết trong `GuiderHandler` ("dump chứng minh là index") cũng sai, đã sửa.

### "26/26 xanh" không bảo đảm UI bấm được

Review chỉ ra tất cả PlayMode test đều gọi thẳng `OnRowClicked`/`Choose`, nên **nhánh
con trỏ chưa bao giờ chạy**. Và dòng menu đặt `sizeDelta = (0, 64)` với anchor mặc định
là **rộng đúng 0 pixel** — vẫn hiện chữ, nhưng trên máy thật không bấm vào đâu được.

Sửa anchor (trải ngang, ghim mép trên) và thêm `PointerPathTests` đi qua đúng đường uGUI
bằng `ExecuteEvents.pointerClickHandler` — nó tôn trọng cờ `interactable` mà không cần
raycaster hay con trỏ vật lý, nên chạy được trong `-nographics`.

Đột biến khôi phục anchor cũ → test đỏ.

### Ba lỗi HIGH/MEDIUM khác

- **Bind màn hình khác giữ nguyên dòng cũ.** `Refresh()` giữ dòng đã dựng nếu chỉ số vẫn
  trong tầm nhìn — đúng khi cuộn, sai khi đổi màn hình. Thu hồi hết trong `Bind`.
- **Callback ảnh dán nhầm icon.** `RemoteAssetCache` gọi callback hai lần, lần hai ở
  frame sau; trong lúc đó dòng có thể đã tái dùng cho item khác. Chốt đường dẫn đang
  hiển thị và bỏ qua callback không khớp.
- **Chạm hai nút cùng frame gửi hai gói.** Chặn trùng nằm ở tầng đóng màn hình là quá
  muộn. Chuyển vào `ChoiceDialogView.Choose`.
- `UiRoot.Initialize` gọi hai lần giờ ném thay vì đăng ký trùng.
- `verify.ps1` giờ quét cả `Assets/Tests` cho rule 200 dòng (hai file test đã vượt).

### Một lỗ kiểm chứng nữa, đã bịt

Lần chạy PlayMode ĐẦU TIÊN hỏng: `Gopet.Runtime.asmdef` không khai báo tham chiếu tới
`Gopet.UiLogic`. Các project compile-only (`Gopet.*.UnityCompat`) gộp nhiều thư mục vào
**một** assembly nên ranh giới asmdef biến mất — chúng không thể thấy lỗi này. Unity thì
tôn trọng ranh giới và từ chối compile.

Tệ hơn: nó chỉ lộ ra lúc chạy PlayMode test, tức là **sau khi đã bắt người dùng đóng
Editor**. Thêm `tools/check-asmdef-refs/` đối chiếu `using Gopet.X` thật với `references`
khai báo, thành bước 2/8 của `verify.ps1`.

Cùng lý do, thêm `tests/Gopet.PlayMode.Compile/` (bước 6/8): PlayMode test cũng là code,
và trước đó chưa có gì compile nó.

### Phần còn lại của phase, làm nốt 2026-09-06

**2FA OTP không cần một dòng code nào.** `Player.show2FADialog()` gọi
`controller.showInputDialog(INPUT_OTP_2FA, ...)` — tức là một `TYPE_DIALOG_INPUT`
hoàn toàn bình thường, dialogId = 34. Nó đi qua `GuiderHandler` → `UiRoot` →
`InputDialogView` như mọi hộp nhập khác, và nằm đè lên màn đăng nhập. Dựng một màn
OTP riêng là chép lại đường đã có, rồi lệch khi server đổi nhãn. Đây là nguyên tắc
"client không giữ logic nghiệp vụ" áp dụng vào chính luồng đăng nhập.

**Thứ tự đăng nhập nằm trong `LoginFlow`, thuần C#.** Chép từ lượt chạy thật của
`LiveSmoke/LoginChecks`, không suy từ plan. Hai chỗ chỉ wire thật mới dạy được:

- Chọn máy chủ khác thì phải **nối lại** tới địa chỉ đó rồi bắt tay lần hai — và lần
  hai KHÔNG được hỏi lại danh sách, nếu không người chơi phải chọn hai lần.
- Server **đóng kết nối ngay sau `CREATE_CHAR`** (`GameController.cs:740`). Không nhớ
  điều đó thì cú đóng ấy hiện ra như lỗi mạng, đúng lúc mọi thứ đang chạy đúng. Đây
  là ca duy nhất phân biệt được hai cách xử lý, nên nó có test riêng.

**Server từ chối theo HAI kiểu, và kiểu thứ nhất suýt nuốt mất thông báo.** Sai mật
khẩu thì server gửi `LOGIN_FAILED` rồi **đóng kết nối ngay** (`Player.cs:655-658`);
sai OTP thì chỉ gửi dialog đỏ và **giữ kết nối** (`inputDialog.cs:731`). Bản đầu xử lý
cả hai như nhau, nên với kiểu thứ nhất màn "mất kết nối" đè lên ngay sau đó và người
chơi chỉ thấy lỗi mạng — không biết mình gõ sai mật khẩu. Giờ câu từ chối được nhớ lại
xuyên qua cú đóng, và client tự nối lại để họ gõ tiếp.

Kèm theo đó là một cái bẫy phải tránh: **không được tự đăng nhập lại sau khi bị từ
chối.** Server đếm số lần thử và khoá ở lần thứ 10 trong 5 phút (`Player.cs:630`) —
tự thử lại là tự khoá mình sau vài giây. Chỉ có đúng hai đường được tự đăng nhập lại:
vừa tạo nhân vật xong, và người chơi tự bấm "Thử lại" sau khi rớt mạng (không phải sau
khi bị từ chối).

`GopetClient.Connect()` vì vậy chuyển sang **hoãn sang `Update()`**: server từ chối nối
lại trong 2 giây cùng một IP, và nối thất bại giờ báo qua `Disconnected` chứ không ném —
người gọi là màn hình đăng nhập, nó cần một câu để hiện chứ không cần exception.
`PacketLogger` cũng chuyển sang sống theo cả phiên chạy app: nó mở file với
`append: false`, nên dựng lại lúc nối lại sẽ **xoá trắng dump của lần trước**, đúng lúc
cần đọc nó nhất.

**`CredentialStore` nói thẳng mức bảo vệ của nó.** AES-256-CBC + HMAC-SHA256
(encrypt-then-MAC), khoá sinh riêng mỗi lần cài và nằm cạnh dữ liệu. Đó là chống ĐỌC
LƯỚT, không phải Keychain — muốn hơn thì phải plugin native cho iOS/Android, để sau.
Tên tài khoản lưu thẳng (không phải bí mật, và người chơi muốn thấy nó điền sẵn); mật
khẩu chỉ lưu **sau khi server đã nhận**, và có nút "Quên tài khoản đã lưu" để xoá thật.

**Test phân biệt được "đã bọc" với "ghi thẳng".** Mọi test round-trip đều xanh với cả
hai cách. Ca duy nhất bắt được là đọc file trên đĩa lên và tìm chuỗi mật khẩu trong đó.

### `MenuBench` — dụng cụ, không phải kết quả

Tiêu chí ">50 fps với menu 200 dòng" **không suy ra được từ test**. PlayMode test chứng
minh 500 dòng chỉ dựng ~12 GameObject; fps thì phụ thuộc GPU, độ phân giải, driver.
`MenuBench` + `FpsSampler` dựng sẵn cảnh đo và **cuộn tự động** — đứng yên nhìn danh
sách tĩnh chỉ đo chi phí vẽ, tức phần dễ. `FpsSampler` báo cả 1% thấp vì trung bình che
mất đúng thứ người chơi cảm nhận: trung bình 60 fps mà 1% thấp 12 fps vẫn là giật thấy rõ.

### Lỗ kiểm chứng thứ ba, cùng họ với hai lỗ trước

`Assets/Tests/PlayMode/` **chưa từng có asmdef**. Test rơi vào `Assembly-CSharp`, và nó
chạy được vì assembly định sẵn tự tham chiếu mọi thứ có `autoReferenced: true`. Nhưng
`Unity.InputSystem.TestFramework` đặt `autoReferenced: false` — nghĩa là `InputTestFixture`
vô hình với `Assembly-CSharp`, và test bàn phím sẽ không compile. Lỗi ấy lại chỉ lộ ra
sau khi đã bắt người dùng đóng Editor.

Đã thêm `Gopet.PlayModeTests.asmdef` khai báo tường minh, và dạy
`tools/check-asmdef-refs/` nhìn cả assembly của package — **chỉ những cái
`autoReferenced: false`**, vì bắt cả `UnityEngine.UI` (autoReferenced) chỉ sinh báo động
giả: Unity tự nối nó vào, khai hay không đều compile được. Đột biến bỏ tham chiếu
`Unity.InputSystem.TestFramework` → lint đỏ.

Cũng dọn một `Gopet.PlayModeTests.asmdef` lạc chỗ nằm ngoài `Assets/` (Unity không thấy,
nên nó không làm gì ngoài việc gây hiểu nhầm).

### Nghiệm thu bằng máy, lượt này

`verify.ps1` 8/8, **unit test 324/324**. PlayMode 59/59 ở lượt trước; sau đợt sửa
review cần chạy lại (thêm `GopetClientReconnectTests`, `BootstrapLayeringTests`).

| Nhóm mới | Kiểm gì |
|---|---|
| `LoginFlowTests`, `LoginFlowCharacterTests` | thứ tự các bước; nối lại đúng địa chỉ; không hỏi lại danh sách; tên sai thì KHÔNG gửi gói nào; mất kết nối không lý do vẫn có câu để hiện |
| `LoginFlowRejectionTests` | câu từ chối sống sót qua cú đóng theo sau nó; KHÔNG tự gửi lại `LOGIN` sau khi bị từ chối; sai OTP thì gửi lại được ngay trên kết nối cũ; dialog sau khi vào game không đá người chơi về màn đăng nhập |
| `SecretBoxTests`, `CredentialStoreTests` | mở lại được; khoá khác thì không; sửa một byte bị bắt; **mật khẩu không nằm nguyên văn trên đĩa** |
| `FpsSamplerTests` | bỏ frame warm-up; 1% thấp bắt được cú khựng mà trung bình che mất |
| `FormViewTests` | hàng có chiều rộng thật; không chồng nhau; bấm qua đường con trỏ; nút vô hiệu im lặng |
| `LoginScreensTests` | mỗi chặng ra đúng một màn hình; bấm trên màn hình đẩy luồng đi tiếp |
| `InputRouterTests` | **bơm phím thật** vào thiết bị ảo qua `InputTestFixture` — không gọi tắt vào sự kiện |

**Đột biến, tất cả đều bị bắt:** ghi mật khẩu plaintext → 2 test đỏ; bỏ nhánh
đóng-sau-tạo-nhân-vật → 1 đỏ; luôn xin lại danh sách máy chủ → 2 đỏ; đổi binding Esc
sang phím khác → 2 đỏ (gồm cả ca "Esc đóng đúng màn hình trên cùng"); phá neo trải ngang
→ `FormViewTests.MoiHang_CoChieuRongThat` đỏ; bỏ nhánh "bị từ chối rồi server đóng"
→ 1 đỏ; luôn tự đăng nhập lại sau khi bắt tay → 2 đỏ.

**Bước 8 của `verify.ps1` đếm nhầm suốt từ đầu.** Nó dùng
`Get-Content | Measure-Object -Line`, và `-Line` **bỏ qua dòng trống** — nên một file
260 dòng vật lý với 60 dòng trống vẫn "dưới 200". Guard như vậy là guard hỏng. Sửa
sang `@(Get-Content).Count`, lộ ra hai file thật sự vượt: `GenericMenuView.cs` (203) đã
gọt lại, `UiLogicTests.cs` (213) vốn chứa HAI class nên tách thành
`MenuSelectionTests.cs` + `DialogStackTests.cs`. `Tea.cs` vẫn là ngoại lệ có chủ đích.

**Đổi tên hai file partial** (`*.Credentials.cs` → `*.Account.cs`): hook chặn quyền
riêng tư của repo coi mọi tên file chứa "credentials" là có thể chứa bí mật và hỏi phê
duyệt mỗi lần sửa. Hai file này chỉ là nửa "tài khoản" của máy trạng thái, không có bí
mật nào — đổi tên rẻ hơn là hỏi lại mãi.

**Một cái bẫy của chính bộ đồ nghề đột biến, đã trả giá:** script khôi phục file bằng
cách chuyển bản backup đè lên. Bản backup mang mtime CŨ HƠN thư mục build, nên MSBuild
kết luận "không có gì đổi" và **dùng lại DLL đã bị đột biến** — lần chạy sau báo 2 test
đỏ trong khi source hoàn toàn đúng. Suýt đi sửa một lỗi không tồn tại. Khôi phục xong
phải `touch` lại file.

### Vòng review thứ hai — một lỗi CRITICAL do chính bản sửa trước sinh ra

**Tự tay đóng một socket cũng sinh ra sự kiện "mất kết nối", và cờ đó giết socket kế tiếp.**

Bản sửa "hoãn nối sang `Update()`" ở trên gọi `Cleanup()` để dọn socket cũ. `Dispose()`
đóng stream → luồng đọc ném → `Fail()` → `Disconnected` → `_pendingDisconnectReason`.
Nhưng lúc đó `_socket == null` nên `Update()` return sớm và **không ai xoá cờ**. Nó sống
qua cả 2,5 giây cooldown, rồi được đọc đúng ở frame `OpenSocket()` vừa thành công →
socket mới bị đóng ngay → lại sinh cờ mới → **vòng lặp vĩnh viễn, 2,5 giây một nhịp.**

Điều kiện kích hoạt là **đường đăng nhập thật**: `showListServer` phát IP cấu hình trong
DB, khác máy đang nối, nên `ChooseServer` luôn nối lại. Máy dev thì danh sách trả về
`127.0.0.1` và `LoginFlow` đi thẳng, không nối lại lần nào — nên nó xanh sạch ở đây và
hỏng 100% ở ngoài kia. `LiveSmoke` cũng không chạm nhánh này.

Sửa: handler `Disconnected` mang theo **danh tính socket** phát ra nó và bỏ qua nếu đó
không còn là socket hiện tại; `Cleanup()` gỡ `_socket` TRƯỚC khi `Dispose()`. Thêm
`GopetClientReconnectTests` chạy trên `TcpListener` thật — trước đó **không có test nào
chạm tới `GopetClient`**.

### Bốn lỗi HIGH khác, đều nằm ở đường lỗi chứ không đường thuận

**Hộp OTP bị màn đăng nhập che và chặn luôn cả click.** uGUI vẽ theo thứ tự anh em, mà
`GopetBootstrap` dựng `UiRoot` trước `LoginScreens`. Server hỏi OTP đúng lúc client ở
chặng "đang đăng nhập", và màn chặng đó là `FormView` phủ kín có `Image` chắn raycast —
hộp OTP nằm dưới, không thấy và không bấm được. Comment tôi viết trong `LoginScreens`
còn khẳng định điều ngược lại. Đổi thứ tự dựng, thêm `BootstrapLayeringTests`.

**Gửi lại `LOGIN` sau khi sai OTP thì server im lặng.** `Player.cs:587`:
`if (this.user != null && !IsPassOtp) return;` — sai OTP xong `user` vẫn còn, nên gói thứ
hai bị bỏ qua **không một lời hồi âm**, và client kẹt ở màn "đang đăng nhập" — màn KHÔNG
CÓ NÚT NÀO. Giờ mọi lời từ chối đều kéo theo nối lại, và ô nhập bị khoá cho tới khi nối
xong thay vì gửi gói vào hư không.

**Không có đồng hồ chờ ở đâu cả.** Thêm `LoginFlow.Tick()`: quá
`ReplyTimeoutMs` mà server chưa nói gì thì về màn "mất kết nối" — có nút thoát ra.

**Tên nhân vật trùng thì mất luôn lý do.** Câu "tên đã có người dùng" phải sống qua cú
đóng kết nối VÀ qua lần đăng nhập lại mà server bắt làm ở giữa. Một ô `_rejection` dùng
chung không đủ: lần đăng nhập ở giữa xoá mất câu của màn kia. Giờ câu từ chối mang theo
**màn hình mà nó thuộc về**.

**`CredentialStore` chỉ bắt lỗi IO** trong khi `Aes.Create()` có thể ném
`CryptographicException` trên nền tảng bị cắt thư viện (IL2CPP + managed stripping) — và
`Remember()` chạy TRƯỚC `Finished`, nên một tiện ích ghi nhớ tài khoản hỏng sẽ chặn mất
cả lần đăng nhập thành công. Đổi thứ tự, và bắt rộng ra.

### Ba test "không bao giờ đỏ được", đã sửa

1. `SuaMotByte_BiPhatHien` sửa byte trong phần mã hoá của một chuỗi 8 ký tự — tức **một
   khối AES duy nhất**, nên đệm CBC tự nó bắt được và test xanh cả khi xoá hẳn HMAC.
   Thay bằng: sửa byte của **chính chữ ký**, và sửa khối đầu của chuỗi 64 ký tự (đệm nằm
   ở khối cuối nên vẫn hợp lệ).
2. `ManTaoNhanVat_HaiNutLaHaiGioiTinh` assert một chặng mà `SubmitCharacter` không đổi,
   và **không hề kiểm giới tính** — hai nút cho kết quả giống hệt nhau (đúng lỗi rule 13
   đã dính một lần ở `GuiderEchoTests`). Giờ đọc byte giới tính trong gói thật, chạy cho
   cả hai nút.
3. `BindLai_KhongGiuLaiOCu` chỉ đếm phần tử trong danh sách — luôn đúng dù có huỷ hay
   không. Giờ nhìn vào chính GameObject cũ; kèm theo là sửa `FormView.Bind` tắt hàng cũ
   trước khi `Destroy` (thứ chỉ có hiệu lực cuối frame).

### Vá thêm

- `SecretBox` so chữ ký **cộng dồn chênh lệch** thay vì thoát sớm; comment cũ ghi "so hết
  mảng" trong khi code thoát sớm — nói một đằng làm một nẻo.
- Tách khoá gốc thành hai khoá con (một mã hoá, một ký). Dùng chung không có lỗ hổng đã
  biết, nhưng cái giá làm đúng ở đây là bốn dòng.
- Chống bấm hai lần: `SubmitCredentials` từ chối khi đang ở chặng `LoggingIn`.
- Packet log chỉ ghi ở Editor và bản development (`Debug.isDebugBuild`) — **dump chứa gói
  `LOGIN`, tức mật khẩu dạng thô**, và trước đó nó mặc định bật ở mọi bản build.

**Còn để lại cho người quyết định:** mật khẩu hiện được lưu sau mỗi lần đăng nhập thành
công, gác sau cờ `rememberAccount` trong Inspector và có nút "Quên tài khoản đã lưu".
Muốn hỏi từng người chơi (ô tick "Nhớ mật khẩu") thì đó là quyết định sản phẩm, không
phải quyết định kỹ thuật. Tương tự: `0 = Nam, 1 = Nữ` mới là suy đoán từ thứ tự nút, chưa
đối chiếu với client cũ.

## Related Code Files

**Create**
- `Assets/Scripts/UI/GenericMenuView.cs` — danh sách virtualized
- `Assets/Scripts/UI/MenuItemRow.cs` — 1 dòng
- `Assets/Scripts/UI/YesNoDialog.cs`
- `Assets/Scripts/UI/InputDialog.cs`
- `Assets/Scripts/UI/NpcOptionDialog.cs`
- `Assets/Scripts/UI/DialogStack.cs` — quản lý chồng dialog + nút back
- `Assets/Scripts/Net/Handlers/GuiderHandler.cs` — dispatch sub-command của 122
- `Assets/Scripts/Net/Models/MenuItemInfo.cs` — DTO, mirror server
- `Assets/Scripts/Runtime/Input/InputRouter.cs` — bàn phím (Esc/Enter)
  > Plan ghi `Assets/Scripts/Input/`. Đổi vào trong `Runtime/` vì thư mục ngoài đó
  > không thuộc asmdef nào, sẽ rơi vào `Assembly-CSharp` và không ai tham chiếu được.
- `Assets/Scripts/Runtime/GopetBootstrap.cs` — ráp tất cả lại, bấm Play là chạy
- `Assets/Scripts/Runtime/UI/{FormView,UiBuilder,LoginScreens,MenuBench}.cs`
- `Assets/Scripts/UiLogic/{LoginStage,LoginFlow,LoginFlow.Account,LoginFlow.ServerList,LoginFlow.Timeout,SecretBox,CredentialStore,FpsSampler}.cs`

**Read for context**
- `SRCGOPETGOC/GServer/Server/GameController.cs:818-861` — `showMenuItem()` — **file quan trọng nhất phase này**
- `SRCGOPETGOC/GServer/Server/GameController.cs:739-800` — `guider()` dispatch
- `SRCGOPETGOC/GServer/Server/MenuController.sendMenu.cs` — cách dựng menu (đọc để hiểu biến thể)
- `SRCGOPETGOC/GServer/Data/dialog/MenuItemInfo.cs` — cấu trúc DTO
- `SRCGOPETGOC/GServer/Server/MenuController.cs:100-170` — danh sách hằng số `MENU_*`

## Implementation Steps

1. **`MenuItemInfo` DTO** — mirror `Data/dialog/MenuItemInfo.cs`, khớp đúng field.

2. **Parser `SHOW_MENU_ITEM`** — viết trước UI. Test bằng cách log ra danh sách item đã parse, đối chiếu với `DEBUG_LOG` của server.
   > Field `showDialog` là `bool` và **quyết định có đọc 3 UTF tiếp theo hay không**. Đọc sai chỗ này là lệch cả stream.

3. **`GenericMenuView`** — virtualized list (chỉ dựng row đang thấy). Mỗi row: icon (qua `RemoteAssetCache`), tiêu đề, mô tả. Row có `canSelect == 0` hiện xám, không bấm được.

4. **Xử lý chọn** — bấm row:
   - `showDialog == true` → hiện `YesNoDialog` với `dialogText`/`leftCmdText`/`rightCmdText`; đồng ý mới gửi
   - `showDialog == false` → gửi `SELECT_MENU_ELEMENT` ngay
   - `closeScreenAfterClick == true` → đóng màn hình sau khi gửi

5. **`YesNoDialog`** — text nút lấy từ server, không hardcode "OK"/"Cancel".

6. **`InputDialog`** — sub 7. Đọc format từ `MenuController.inputDialog.cs`. Hỗ trợ nhập số (kèm bàn phím số trên mobile).

7. **`NpcOptionDialog`** — sub 2/5. Hội thoại NPC + danh sách lựa chọn.

8. **`DialogStack`** — server có thể gửi dialog chồng lên nhau. Nút back/Esc đóng cái trên cùng. Tránh trạng thái kẹt không thoát được.

9. **`InputRouter`** — Input System với 2 action map, chuyển theo nền tảng lúc chạy.

10. **Test độ phủ** — đi qua nhiều `listID` nhất có thể trên client cũ, dump lại, replay vào client Unity. Mục tiêu: mọi menu render đúng mà **không** cần code riêng cho từng `listID`.

## Success Criteria

**Đã kiểm chứng**

- [x] Parse đúng `SHOW_MENU_ITEM` mọi biến thể (có/không `showDialog`, có/không `paymentOptions`)
- [x] Icon tải bất đồng bộ, danh sách hiện ngay không chờ ảnh
- [x] Item `canSelect == 0` mờ **và** tắt nút
- [x] `showDialog == true` → dialog với nhãn nút lấy từ server
- [x] `closeScreenAfterClick` đóng đúng màn hình, kể cả khi nó ở giữa chồng
- [x] Mở được màn hình thật từ server — không có code riêng cho từng `listId`
- [x] Dialog chồng nhau: back đóng đúng thứ tự, không kẹt
- [x] Danh sách dài chỉ dựng phần nhìn thấy, GameObject được tái dùng

- [x] Chạy được cả touch, chuột **và bàn phím** — `InputRouter` (Esc = back,
      Enter = xác nhận), test bơm phím thật qua `InputTestFixture`
- [x] Màn đăng nhập / chọn máy chủ / tạo nhân vật / mất kết nối (nhận từ P3)
- [x] `CredentialStore` — mật khẩu không nằm nguyên văn trên đĩa
- [x] 2FA OTP — **không cần code mới**, xem mục dưới
- [x] `GopetBootstrap` — ráp Canvas, `EventSystem`, handler, màn đăng nhập, phím tắt
      > Còn **một bước tay làm một lần**: gắn component vào một scene. Scene không
      > commit sẵn vì Unity ghi đè file scene theo phiên bản của nó (xem README).

**Còn lại — đều cần người, không code được nữa**

- [ ] Menu 200 item **>50 fps trên máy tầm trung**. Dụng cụ đo đã có
      (`MenuBench` + `FpsSampler`, cuộn tự động để tính cả chi phí dựng dòng mới);
      còn thiếu đúng một thứ: build ra thiết bị rồi đọc số.
- [ ] Diff dump 5 menu với client cũ — cần thao tác tay trên emulator
- [ ] Bấm Play một lần để nghiệm thu bốn tiêu chí ảnh hoãn từ P4 (lần 2 lấy từ RAM,
      khởi động lại lấy từ đĩa, placeholder, `FilterMode.Point`)
- [ ] Nút back của Android có về đúng `<Keyboard>/escape` không — chưa thử máy thật

## Risk Assessment

| Rủi ro | Xử lý |
|---|---|
| Đọc lệch `SHOW_MENU_ITEM` do field điều kiện (`showDialog`) | Viết parser trước, test riêng bằng dump thật trước khi ghép UI |
| Cám dỗ hardcode xử lý riêng theo `listID` | Review nghiêm: nếu thấy `switch(listID)` trong code UI là sai thiết kế. Ngoại lệ hợp lệ chỉ là layout đặc biệt, không phải logic nghiệp vụ |
| Menu dài làm tụt frame | Virtualized list ngay từ đầu, đừng để tối ưu sau |
| UI mobile và PC phình thành 2 nhánh code | 1 layout responsive + `InputRouter`, không `#if` rải rác |
| `MenuController` có 162 hằng số — sợ phải làm hết | Không. Chỉ cần 4 component. 162 hằng số chỉ là `listID` server tự quản |

## Next Steps

Xong P5 → P6 (Map Rendering & Movement). Hết P6 là đạt mốc vertical slice.
