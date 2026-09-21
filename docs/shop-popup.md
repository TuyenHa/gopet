# Popup cửa hàng

Popup nổi giữa màn hình khi bấm icon **Cửa hàng** ở HUD góc phải-trên. Bốn tab viên thuốc
(Vũ khí / Giáp / Mũ / Thức ăn) nằm trong một khay trắng, mỗi tab là một `shopId` riêng của
server; mỗi món là một thẻ có icon, tên + yêu cầu chỉ số, mô tả, chip chỉ số và nút giá
xanh lá.

## Luồng tổng thể

```
Icon HUD ──> UiRoot.OpenShopPopup()
                 │
                 ├─ ShopPopupView.Create(...)  ──> SelectTab(SHOP_WEAPON)
                 │                                     │
                 │                       RequestShop ──┴──REQUEST_SHOP──> server
                 │
   TryConsumeMenu(screen) <── UiRoot.ShowMenu <──SHOW_MENU_ITEM── MenuScreen
                 │
                 ├─ listId == tab đang chọn ──> Bind(): dựng lại danh sách ShopItemRow
                 └─ listId thuộc họ shop nhưng khác tab ──> nuốt im lặng

Bấm nút giá ──> TryBuy(index) ──> ConfirmRequested ──> ChoiceDialogView
                                                            │ đồng ý
                                    SELECT_MENU_ELEMENT ────┘ (kèm paymentIndex = 0)
```

## shopId ↔ tab

| Tab | Hằng số | Server |
|-----|---------|--------|
| Vũ khí | `ShopWeapon = 1` | `MenuController.SHOP_WEAPON` |
| Giáp | `ShopArmour = 2` | `SHOP_ARMOUR` |
| Mũ | `ShopHat = 3` | `SHOP_HAT` |
| Thức ăn | `ShopFood = 4` | `SHOP_FOOD` |
| _(không có tab)_ | `ShopSkin = 7` | `SHOP_SKIN` |

`SHOP_SKIN` không có tab nhưng vẫn phải nuốt: ở map 19 server tự đổi `SHOP_FOOD` thành
`SHOP_SKIN` (`GameController.cs:2367`). Không nuốt thì `UiRoot` đẻ thêm một
`GenericMenuView` đè lên popup.

Cũng phải nuốt cả gói **khác tab đang chọn** — người chơi bấm Vũ khí rồi bấm tiếp Mũ thì
gói Vũ khí về sau vẫn thuộc họ shop; để lọt là nó dựng view riêng đè popup.

## Vì sao không dùng `GenericMenuView`

`GenericMenuView` là bộ render dùng chung cho cả 162 màn hình danh sách của server và
**không được phép** rẽ nhánh theo `listId`. Thẻ cửa hàng lại cần đọc khuôn chuỗi riêng
của shop và cần nút giá — nhồi vào đó là mở nhánh đầu tiên trong số 162 nhánh. Nên popup
tự dựng `ShopItemRow`, cùng cách `PetGridView` đang làm. Danh sách cửa hàng chỉ vài chục
dòng nên bỏ luôn virtualization.

## Tách chuỗi của server (`ShopItemText`)

Server ghép sẵn tên + yêu cầu vào một chuỗi và mô tả + dải chỉ số vào chuỗi kia
(`ShopTemplateItem.getName/getDesc`):

```
title = "búa gỗ(Yêu cầu   25 (str) ,  20 (agi) ,  20 (int))"
desc  = "búa dành cho chiến binh( [80 (atk) -85 (atk) ] ,  [0 (def) -0 (def) ], ... )"
```

`ShopItemText.Parse` tách ra:

| Trường | Kết quả |
|--------|---------|
| `Name` | `Búa gỗ` |
| `Requirement` | `Yêu cầu 25 str, 20 agi, 20 int` (bỏ chỉ số bằng 0) |
| `Description` | `Búa dành cho chiến binh` |
| `Stats` | `Tấn công 80-85`, `Phòng thủ 0` — xem quy tắc chọn chip bên dưới |

Ba điểm dễ sai đã bịt:

- **Không bám vào chữ "Yêu cầu"** — chuỗi đó lấy từ `Language.Request`, bản EN khác hẳn.
  Chỉ bám vào tag `(str)/(agi)/(int)/(atk)/(def)/(hp)/(mp)` mà server luôn chèn.
- **Dấu `-` giữa hai đầu dải là dấu nối, không phải dấu âm.** Đọc `-85` thành số âm thì
  ra `80--85`.
- **Chuỗi không khớp khuôn** (thức ăn, pet, item thường) trả nguyên văn và không có chip
  nào — hỏng khuôn thì mất trang trí chứ không mất thông tin.
- **Item ngoại hình không có ngoặc bao ngoài.** `getDesc` trả thẳng `"+%s +%s +%s +%s"`,
  tức cả chuỗi là khối chỉ số. Chỉ lùi tìm `'('` thì không thấy gì và nguyên đoạn
  `+[0 (atk) -0 (atk) ] +[0 (def)…` đổ ra dòng mô tả.

Nhãn giá đi qua `ShopItemText.ShortMoney`: bỏ hai tag kiểu J2ME `(vang)`/`(ngoc)` vì đã
có icon đồng tiền nói thay; đơn vị viết bằng chữ ("3 thỏi bạc") giữ nguyên.

### Chọn chip nào để hiện

Tối đa **3 chip** — quá số đó là vượt bề ngang thẻ. Thứ tự xét atk → def → hp → mp:

1. Lấy các chỉ số **khác 0** trước. Mũ cộng def + hp mà vẫn nhét "Tấn công 0" là thành 4
   chip, chip nào cũng bị bóp và chữ tràn khỏi viền.
2. Chưa đủ hai chip thì bù bằng atk/def kể cả khi bằng 0 — trang bị nào cũng có hai con
   số đó, để trống là người chơi tưởng dòng chưa tải xong. Nhờ vậy vũ khí vẫn ra
   "Tấn công 80-85" + "Phòng thủ 0" đúng như ảnh mẫu.

Ở tầng layout, chip tính bề rộng từ `Text.preferredWidth` ngay lúc dựng rồi ghi vào
`LayoutElement` làm **cả `minWidth` lẫn `preferredWidth`**, còn hàng chip BẬT
`childControlWidth`. Hai chỗ đã sai và đều chỉ lộ ra bằng mắt:

- Thiếu `minWidth`: tổng bề rộng vượt chỗ trống thì hàng chip bóp mọi chip lại — nền chip
  co còn chữ thì không, chữ tràn đè lên chip bên cạnh.
- Dùng `ContentSizeFitter` + tắt `childControlWidth`: hàng chip xếp chỗ theo bề rộng
  *hiện tại* của chip (vẫn là 0), fitter nong ra sau — cả ba chip chồng lên nhau ở mép
  trái.

`RectMask2D` trên hàng chip là lưới an toàn cuối: chip không còn chỗ thì bị cắt gọn ở mép
chứ không đè lên nút giá. `ShopItemRowTests` đo toạ độ thật sau một lượt dựng layout để
khoá cả hai lỗi trên.

## Chữ dài trong thẻ item

`UiBuilder.MakeText` bật `Overflow` cả hai chiều, nên để nguyên là chữ dài **chạy đè lên
nút giá và tràn ra ngoài thẻ** — thấy rõ nhất ở tab Thức ăn, nơi mô tả dài cả câu. Tiêu
đề, mô tả và nhãn giá đều đặt `Wrap` + `Truncate`: xuống dòng khi hết chỗ ngang, cắt khi
hết chỗ dọc.

Chiều cao dành cho mô tả đổi theo item: có chip chỉ số thì chỉ còn một dòng (hàng chip
chiếm đáy thẻ), không chip thì lấy hết chỗ xuống sát đáy — khoảng 2 dòng.

Tiêu đề và nhãn giá thêm `resizeTextForBestFit` — tiêu đề 10–14 vì tên kèm đủ ba yêu cầu
chỉ số ("Mũ Sắt (Yêu cầu 25 str, 20 agi, 20 int)") dài gấp rưỡi chỗ có; nhãn giá 7–12 vì
có loại tiền tên rất dài ("điểm hoa ngọc" ở cửa hàng thức ăn). Chữ co lại còn đọc được
hết, cắt ngang là mất chữ.

## Nút giá

Lấy `PaymentOptions[0]` của dòng. Màu lấy đúng mã đo trên ảnh mẫu: ruột `rgb(95,195,55)`,
viền 1px sẫm hơn `rgb(74,167,47)`.

Nút **giữ nguyên xanh tươi kể cả khi không đủ tiền** — người chơi cần thấy giá món mình
đang nhắm tới, không phải một nút xỉn khó đọc. Chỉ xám (`PriceLocked`) khi
`MenuItemInfo.CanSelect = false`, tức server khoá hẳn dòng; lúc đó gửi lên server cũng im
lặng bỏ qua.

Thiếu tiền (`PaymentOption.IsEnabled = 0`) thì `TryBuy` **báo tại chỗ qua sự kiện
`Message`** (UiRoot đẩy ra toast: "Bạn cần 100 điểm hoa ngọc để thực hiện giao dịch") và
không gửi gì lên. Server có nhánh trả lời thiếu tiền
cho vàng/ngọc/thỏi bạc… nhưng `selectMenu.cs:797-823` bỏ sót các loại tiền sự kiện như
điểm hoa ngọc — đúng loại cửa hàng thức ăn đang dùng — nên gửi lên là im lặng tuyệt đối.

`Button.transition` đặt `None`: mặc định `Selectable` nhân màu disabled lên ảnh viền và đè
mất màu vừa gán.

Mua luôn đi qua hộp xác nhận vì `showShop` đặt `showDialog = true` cho mọi dòng
(`MenuController.cs:869`). `UiRoot` nối `ShopPopupView.ConfirmRequested` sang
`ShowConfirm`; thiếu dây nối này thì bấm nút giá không có gì xảy ra.

## Các file

| File | Việc |
|------|------|
| `Runtime/UI/GamePopupFrame.cs` | **khung popup dùng chung**: nền, badge tiêu đề, nút X, băng chân |
| `Runtime/UI/PawBadge.cs` | dấu chân, có/không vòng tròn trắng |
| `Runtime/UI/ShopPopupView.cs` | trạng thái, nuốt gói menu, đổi tab, luồng mua |
| `Runtime/UI/ShopPopupView.Chrome.cs` | khay tab và 4 tab |
| `Runtime/UI/ShopPopupView.List.cs` | đặt khung danh sách vào chỗ |
| `Runtime/UI/PopupItemList.cs` | **khung danh sách thẻ dùng chung** |
| `Runtime/UI/ShopItemRow.cs` | bind một thẻ item |
| `Runtime/UI/ShopItemRow.Build.cs` | dựng hình thẻ item |
| `Runtime/UI/ShopItemRow.Price.cs` | nút giá |
| `Runtime/UI/ShopChip.cs` | viên chip chỉ số |
| `Runtime/UI/PopupPalette.cs` | bảng màu dùng chung cho mọi popup |
| `Runtime/UI/CircleUiSprite.cs` | sprite tròn cho huy hiệu dấu chân |
| `Runtime/UI/RoundedBorder.cs` | khung bo góc + viền đều, hai lớp ảnh |
| `UiLogic/ShopItemText.cs` | tách chuỗi server (thuần C#, test ngoài Unity) |
| `UiLogic/ShopItemText.Scan.cs` | chọn chip, đọc dải chỉ số ra khỏi chuỗi |

## Khung dùng chung (`GamePopupFrame`)

Popup cửa hàng **không tự dựng khung**. `GamePopupFrame` lo nền bo góc viền xanh, badge
tiêu đề chờm mép trên, nút X và băng chân; màn hình chỉ đặt nội dung vào `Content` — vùng
đã trừ sẵn badge và băng chân (lề hai bên 11, mép trên 18).

Component của màn hình gắn lên **cùng GameObject** với khung, nên `DialogStack` vẫn chỉ
quản một object như trước.

Đang dùng chung khung này:

| Popup | Icon HUD | Tiêu đề | Băng chân |
|-------|----------|---------|-----------|
| `ShopPopupView` | Cửa hàng | Cửa hàng | tên cửa hàng server gửi theo tab |
| `AtmPopupView` | Dịch vụ | Dịch vụ | Ngân hàng và quy đổi tiền tệ |
| `DailyCheckinView` | Sự kiện | Sự kiện | Điểm danh mỗi ngày để nhận quà |
| `TranChanTabsView` | _(NPC Trần Trấn)_ | Pet | Chọn một pet để nhận hoặc mua |
| `BacSiNpcTabsView` | _(NPC Bác Sĩ Xì Tin)_ | Bác sĩ | Hồi sinh pet và tẩy điểm tiềm năng |
| `HeavenNpcTabsView` | _(NPC Sứ Giả Thiên Đình)_ | Sứ giả | đổi theo tab |

Tiêu đề popup bác sĩ lấy theo **tên NPC** chứ không theo tab: nó có hai tab "Hồi sinh pet
sau PK" và "Tẩy gym", đặt tên theo một tab là bỏ rơi tab kia.

Popup Sự kiện có hai tab: **Điểm danh** (lưới ngày) và **Quà tặng** (nhập giftcode).

Popup điểm danh đặt khung 414×302 để `Content` ra đúng 392×248 — bằng vùng trong của
panel 408×296 cũ sau khi trừ lề và thanh tiêu đề vàng. Nhờ vậy grid 6 cột và panel trái
giữ nguyên kích thước đã căn. Nó cũng bỏ lớp nền tối phía sau để giống cửa hàng.

Popup Dịch vụ dùng lại cả khay tab, khung nội dung trắng và bảng màu của cửa hàng; nút
bên trong đổi từ navy xám (`UiBuilder.ButtonFace`) sang xanh của badge tiêu đề.

### Khay tab (`PopupTabRail`)

Khay trắng + các tab viên thuốc chia đều bề ngang, vàng chữ trắng khi chọn. Cửa hàng
(4 tab), Sự kiện (2 tab), Trần Trấn và Bác sĩ (số tab do server gửi) dùng chung.

**Nối `Selected` SAU khi chọn tab đầu.** `Select` chỉ bắn khi tab đổi, nên nối trước là
lần chọn đầu cũng gửi option lên server — với tab tốn vàng (hồi sinh pet) thì đó là trừ
tiền oan. Popup Dịch vụ giữ tab "ATM" một mình rộng cố định
77 nên không qua component này — một tab mà kéo dài hết khay thì trông như thanh tiêu đề
chứ không phải tab.

`Select` bỏ qua khi trùng tab đang chọn, nên `ShopPopupView.SelectTab` gọi ngược vào khay
không tạo vòng lặp.

Bề ngang khay truyền **tường minh** (`GamePopupFrame.ContentWidth`) chứ không đọc
`parent.rect.width`: rect của một con neo-giãn chưa chắc đã tính xong ngay lúc dựng, và
đọc trúng lúc nó còn 0 thì mọi tab rộng 0.

### Mã quà tặng (`GiftCodeForm`)

Form dựng thẳng trong tab, không chờ server mở hộp thoại: handler
`INPUT_TYPE_GIFT_CODE = 17` (`MenuController.inputDialog.cs:65`) đọc mã từ chính gói gửi
lên, không phụ thuộc việc hộp thoại đã mở trước đó. Đi đường NPC thì phải đứng cạnh NPC
mới bấm được.

Mã rỗng bị chặn tại chỗ, không gửi lên. "Huỷ" quay lại tab Điểm danh chứ không đóng popup.

### Dòng danh sách nền sáng

`MenuItemRow.SetLightCard(true)` và `PetGridView` đều **không tô nền dòng** — khung chứa
đã trắng sẵn — mà phân tách nhau bằng vạch ngang `Hairline` ở chân dòng, dòng cuối không
kẻ.

Tô nền đặc gây ba lỗi cùng lúc, và cả ba chỉ thấy được bằng mắt:

- dòng đầu/cuối phủ **vuông** lên bốn góc bo của khung → góc trắng;
- mép dòng cắt vụn đường viền khung ở chỗ nó chạm vào → viền thành **nét đứt**;
- các dòng rời nhau, không có đường phân tách.

Chữ mô tả của dòng sáng dùng `PopupPalette.TextMuted`, không phải `UiBuilder.TextMuted` —
màu sau thiết kế cho nền tối, đặt trên nền trắng thì gần như không đọc được.

### Danh sách thẻ (`PopupItemList`)

Khung danh sách cũng dùng chung: nền trắng bo góc viền mảnh, cuộn dọc, mỗi dòng một
`ShopItemRow`. Menu quy đổi vàng (1040) của popup Dịch vụ trước đây nhúng
`GenericMenuView` với thẻ nền tối, lạc hẳn khỏi tông sáng của khung; nay đi qua
`PopupItemList` nên trông y hệt danh sách cửa hàng.

Chữ trên nút hành động đổi theo ngữ cảnh: "Mua" ở cửa hàng, "Đổi" ở Dịch vụ — dòng nào
kèm giá thì hiện giá thay cho chữ đó.

`withPanel: false` khi vùng chứa đã là khung trắng có viền, tránh hai đường viền chồng
nhau. Danh sách vài chục dòng nên dựng thẳng, không virtualization — cuộn hàng trăm dòng
vẫn là việc của `GenericMenuView`.

## Viền

Mọi khung bo góc có viền đều dựng bằng `RoundedBorder.Apply` — hai lớp ảnh: lớp ngoài màu
viền, lớp trong thụt vào đúng độ dày và mang màu nền, bán kính trong nhỏ hơn đúng độ dày
đó.

Sprite bo góc (`RoundedUiSprite`) dựng ở **4 texel mỗi ref-unit**, lọc bilinear, **không
mipmap**. Dựng 1-1 thì dải khử răng cưa ở mép cong rộng đúng một ref-unit — khoảng 2px
trên màn hình — và mọi góc bo trông như bị lè ra. `pixelsPerUnit` của sprite đặt 100 × hệ
số đó nên biên 9-slice vẫn quy về đúng số ref-unit như cũ.

Con số 4 và việc tắt mipmap đều đến từ đo đạc: dựng lại đúng đường vẽ ngoài Unity (dựng
texture → cắt 9-slice → lấy mẫu bilinear như GPU) rồi so với hình lý tưởng, ở góc bo 6
đơn vị của tab, sai lệch trung bình trên 255:

| canvas | T=1 | T=2 | T=4 | T=4 + mipmap |
|--------|-----|-----|-----|--------------|
| 1.64 | 6.91 | 1.44 | **1.48** | 2.82 |
| 2.67 | 9.41 | 3.63 | **1.21** | 2.53 |
| 3.50 | 9.87 | 4.17 | **0.95** | 1.45 |

Mipmap nghe hợp lý vì góc bị thu nhỏ lúc vẽ, nhưng số đo nói ngược: mức mip gần nhất thô
hơn cỡ cần vẽ nên trilinear trộn vào là nhoè thêm gấp đôi. Chỉ ở canvas ≈ 1.0 (cửa sổ
game hẹp 720px) mipmap mới thắng.

**Đừng dùng `Outline` của uGUI cho viền.** Nó không vẽ viền mà nhân bản mesh ra bốn vị trí
chéo (±x, ±y), nên nét dồn dày ở góc, mỏng dần ở giữa cạnh và luôn trông nhoè. Viền mảnh
trong popup (chip, khay tab, khung danh sách) dày 1; đường bao ngoài cùng của popup và
vòng trắng quanh badge dày 2.

`Outline` vẫn đúng chỗ khi cần **viền chữ** để đọc được trên nền bất kỳ — xem
`ShopServiceEventHud`.

## Icon

Sinh bằng `tools/image-gen` (gpt-image, nền trong suốt), cắt quầng sáng và hạ xuống 96px
rồi đặt ở `Assets/Resources/Ui/Hud/`, nạp qua `HudSkin`:

| File | Chỗ dùng |
|------|----------|
| `paw.png` | huy hiệu tròn ở badge tiêu đề, băng chân |
| `stat-attack.png` | chip Tấn công |
| `stat-defense.png` | chip Phòng thủ |
| `coin-gold.png` | nút giá |

Thiếu file nào thì `HudSkin.Get` trả `null` và chỗ đó chỉ mất icon, chữ vẫn còn.

## Kích thước

Mọi con số đo thẳng từ ảnh mẫu (rộng 1631px) rồi quy về ref-unit của canvas — hệ số
**0.4415**. Đổi một con số mà không đo lại là hàng tab lệch khỏi khay ngay.

| Thành phần | Ref-unit |
|------------|----------|
| Popup | 400 × 300 |
| Khay tab | cách mép 11, cách đỉnh 18, cao 38 |
| Tab | cao 20, rộng 77, cách nhau 16, lề trong khay 11, bo góc 6, chữ cỡ 11 |
| Badge tiêu đề | 174 × 31, tâm gần trùng mép trên popup |
| Khung danh sách | cách đỉnh 62 (hở 6 dưới khay tab), cách đáy 36 |
| Thẻ item | cao 68 |
| Băng chân | 226 × 20, cách đáy 9 |
| Nút giá | 57 × 21, bo góc 4.5 |
| Nút X | cạnh 34, tâm lùi vào 6 và xuống 2 so với góc trên-phải |

Hai chỗ dễ vỡ:

- **Biên 9-slice phải nhỏ hơn nửa chiều cao ô.** Unity ép biên xuống theo *từng trục
  riêng* (`Image.GetAdjustedBorders`): ô thấp hơn tổng biên dọc thì biên dọc co lại còn
  biên ngang giữ nguyên, và bốn góc bị kéo dài thành **hình bầu dục**. Vì thế
  `RoundedUiSprite.Get(radius)` dựng biên chỉ nhỉnh hơn bán kính 2 đơn vị, và ô thấp
  (tab cao 20, chip cao 17) phải truyền bán kính nhỏ hơn mặc định 10 — tab dùng 6, chip
  dùng 8. Muốn góc tròn hơn thì tăng bán kính, đừng trông vào việc Unity co biên giúp.
- **Badge không được cao quá 31**: cao hơn là nó chờm xuống đè lên khay tab.

Ref canvas là 720×1280 nhưng `CanvasScaler` khớp **chiều rộng**, mà game chạy landscape —
nên chiều cao khả kiến chỉ khoảng 405 ref-unit. Popup 300 cộng badge chờm lên trên là đã
gần kịch khung; đừng nới chiều cao thêm.

## Test

- `tests/Gopet.Net.Tests/ShopItemTextTests.cs` — tách chuỗi, chạy ngoài Unity.
- `Assets/Tests/PlayMode/ShopPopupTests.cs` — nuốt gói, dựng thẻ, đổi tab xoá danh sách
  cũ, bấm giá phải hỏi rồi mới gửi.
- `Assets/Tests/PlayMode/ShopItemRowTests.cs` — hàng chip: không chồng nhau, không lọt ra
  ngoài khung, chuỗi dài cho chip rộng hơn chuỗi ngắn.
- `Assets/Tests/PlayMode/ShopBuyTests.cs` — luồng mua: hỏi rồi mới gửi; thiếu tiền thì báo
  tại chỗ, không gửi.
- `Assets/Tests/PlayMode/EventPopupTests.cs` — hai tab Sự kiện: đổi tab đổi trang, mã rỗng
  không gửi, Huỷ quay về tab điểm danh.
