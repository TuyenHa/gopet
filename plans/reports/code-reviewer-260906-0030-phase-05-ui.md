# Code Review — Phase 5: Tang UI generic + giao thuc menu

Ngay: 2026-09-06 00:30 | Reviewer: code-reviewer | Nguon su that: SRCGOPETGOC/GServer

## Pham vi
19 muc theo yeu cau: 6 file Net/Guider, 3 file UiLogic, 5 view Runtime/UI, 3 PlayMode test,
4 unit test, LiveSmoke, check-asmdef-refs, verify.ps1, run-playmode-tests.ps1. ~1.900 LOC.

## Danh gia chung
Parser SHOW_MENU_ITEM khop **tung field, dung thu tu** voi `GameController.showMenuItem()`
(GameController.cs:826-871), ke ca khoi dieu kien `showDialog`. `ExpectFullyConsumed` co o moi
parser. Tach UiLogic thuan C# la lua chon dung. Khong co mot cho nao re nhanh theo `listId`.

Nhung **gia tri gui nguoc len server dang sai**: client gui VI TRI DONG, con server cho doi
GIA TRI NO DA GHI trong dong do. Hai gia tri nay trung nhau o cac man hinh da thu (ATM, shop
rong) nen test va live smoke deu xanh. Loi C1/C2 duoi day lam sai chuc nang o cac man hinh co
ID rieng.

---

## CRITICAL

### C1. Chon dong menu gui INDEX thay vi ItemId server da gui
`Assets/Scripts/Net/Guider/GuiderHandler.cs:74-76` (va `GuiderPackets.cs:18-41`)

Server ghi vao dong: `itemId` neu `isHasId()`, nguoc lai ghi `i` (GameController.cs:837-844).
Tuc la truong int dau moi dong la "gia tri echo", KHONG phai chi so.

Bang chung phia tieu thu — `MenuController.selectMenu(menuId, index, ...)`:
- `MenuController.selectMenu.cs:1467+` MENU_APPROVAL_CLAN_MEMBER -> `clan.getJoinRequestByUserId(index)`
  (itemId = `clanRequestJoin.user_id`, sendMenu.cs:440)
- `MenuController.selectMenu.cs:423` MENU_PET_INVENTORY -> `if (index == -1)` (itemId bat dau tu -1
  khi dang deo pet, sendMenu.cs:64-83)
- `MenuController.selectMenu.cs:659` SHOP_CLAN -> `getShopTemplateItem(index)` = tim nhi phan theo
  `menuId` cua item (ShopClan.cs:60-72); shop thuong thi `.get(index)` theo vi tri — va dung cho do
  ma server dat `setHasId(shopTemplateItem.isHasId())` (MenuController.cs:870)
- MENU_UPGRADE_MEMBER_DUTY (sendMenu.cs:466): itemId = `user_id`

Hau qua: duyet thanh vien clan, chon pet trong tui, mua do shop clan -> gui sai so -> server thao
tac nham doi tuong hoac bao "yeu cau da bi go". Khong co loi giao thuc nao no ra, rat kho lan.

Sua (dung cho CA HAI truong hop, khong them nhanh nao):
```csharp
// GuiderHandler.Select(MenuScreen, ...)
var echo = screen.Items[index].ItemId;   // server tu ghi i khi item khong co id rieng
_send(paymentIndex >= 0
    ? GuiderPackets.SelectMenuElementWithPayment(screen.ListId, echo, paymentIndex)
    : GuiderPackets.SelectMenuElement(screen.ListId, echo));
```
Doi ten tham so cua `GuiderPackets.SelectMenuElement(int listId, int index)` thanh `itemId` va sua
doc-comment "Da chon dong thu <index>" — comment hien tai dang khang dinh sai.

### C2. ListOption gui INDEX thay vi Option.Id
`Assets/Scripts/Net/Guider/GuiderHandler.cs:84-93`

`sendListOption` ghi `option.getOptionId()` (GameController.cs:4614). Vi tri != id o thuc te:
`MenuController.sendMenu.cs:243-250` MENU_LIST_REQUEST_ADD_FRIEND_OPTION co id theo thu tu
**0, 1, 3, 2**; handler `selectMenu.cs:2054+` la `switch (index)` tren chinh cac id do.
Client gui vi tri -> dong thu 3 ("Tu choi tat ca") chay nhanh 2 ("Tu choi va chan") va nguoc lai.

Sua: `_send(GuiderPackets.SelectMenuElement(screen.ListId, screen.Options[index].Id));`

Kem theo: comment `GuiderHandler.cs:80-83` khang dinh "quan sat tren dump: chon ATM gui sub 3,
listId, index". Dump khong chung minh duoc dieu do — man ATM co `Option(0..2)` nen id == vi tri
(sendMenu.cs:777-782). Comment nay se keo nguoi doc sau tin nham.

---

## HIGH

### H1. Bind lai GenericMenuView sang man hinh khac giu nguyen du lieu dong cu
`Assets/Scripts/Runtime/UI/GenericMenuView.cs:74-81, 123-127`

`Bind()` chi doi `_screen` roi goi `Refresh()`, ma `Refresh()` bo qua moi index da co trong
`_realized` (`if (_realized.ContainsKey(i)) continue;`). Bind man hinh thu hai -> cac dong 0..n van
hien tieu de, mo ta, icon va `Item` cua man hinh cu; bam vao chung se gui `_screen` moi kem index
cu. UiRoot hien luon tao view moi nen chua no ra, nhung day la API public va khong test nao phu.

Sua: dau `Bind()`, thu hoi tat ca truoc khi Refresh:
```csharp
foreach (var index in new List<int>(_realized.Keys)) Recycle(index);
```

### H2. Icon tai bat dong bo dat nham dong (race) + icon cu con lai tren dong tai dung
`Assets/Scripts/Runtime/UI/MenuItemRow.cs:75-85`

Hop dong cua `RemoteAssetCache.Get`: callback co the goi HAI lan, lan hai o frame sau
(RemoteAssetCache.cs:17-19, 103-105). Callback hien chi kiem `this != null && _icon != null` —
khong kiem dong con dang hien dung item do. Cuon nhanh: dong bi thu hoi va Bind lai cho index
khac, anh cu ve sau -> dan len dong moi. Ngoai ra `LoadIcon` return som khi path rong ma khong
xoa `_icon.texture` -> dong tai dung giu icon cua item truoc.

Sua:
```csharp
private void LoadIcon(string path, RemoteAssetCache assets)
{
    _icon.texture = null;                       // dong tai dung khong duoc giu anh cu
    if (assets == null || string.IsNullOrEmpty(path)) return;
    var expected = path;
    assets.Get(path, ImagePackets.TypeIcon, texture =>
    {
        if (this == null || _icon == null) return;
        if (Item == null || Item.ImagePath != expected) return;   // dong da doi chu
        _icon.texture = texture;
    });
}
```

### H3. View chua gan duoc vao scene that — va test khong the bat duoc dieu do
- Khong file nao trong `Assets/Scripts/Runtime` tao `Canvas` / `GraphicRaycaster` / `EventSystem`
  (grep sach). `UiRoot.Create` chi tao GameObject + RectTransform.
- `MenuItemRow.Create:42-43` dat `rect.sizeDelta = new Vector2(0f, Height)` voi anchor mac dinh
  (0.5, 0.5) -> **rong 0** -> khong co vung raycast, chuot/cham khong bao gio trung.
  ChoiceDialogView / InputDialogView khong dat kich thuoc gi ca.
- Moi PlayMode test goi thang `OnRowClicked()` / `Choose()` / `Submit()`. Do la cau tra loi cho
  "test co luon xanh khong": nhanh con tro + layout **khong duoc phu**, nen `_button.interactable`,
  kich thuoc raycast, su ton tai cua canvas deu chua tung duoc kiem chung.

Toi thieu: dat anchor stretch ngang cho row va dialog; them mot PlayMode test dung
`ExecuteEvents.Execute<IPointerClickHandler>` de nhanh click that su chay it nhat mot lan.

---

## MEDIUM

### M1. Bam hai lan trong cung frame -> gui hai goi
`UiRoot.cs:86-90, 101-105, 114-118, 130-134, 143-147`

Chan trung nam trong `Close()`, khong nam truoc hanh dong:
```csharp
view.Chosen += index => { _guider.Select(screen, index); Close(view); };
```
`Destroy` chi co hieu luc cuoi frame, nen hai nut cua CUNG hop thoai bi cham cung frame
(multi-touch) se ban `Chosen` hai lan -> hai goi SELECT khac nhau. Voi `ShowConfirm`, lan hai van
chay `onYes()` vi `Close` tra false ma khong chan gi.

Sua: chan ngay dau handler (`if (!_stack.Remove(view)) return; DestroyView(view); ...`) hoac them
co "da dung mot lan" trong `ChoiceDialogView.Choose` / `InputDialogView.Submit`.

### M2. Push vuot MaxDepth de lai lop che khong go duoc
`UiRoot.cs:152-156`

`_views[screen] = view;` chay TRUOC `_stack.Push(screen)`. `DialogStack.Push` nem khi du 32 lop
(DialogStack.cs:38-42) -> GameObject da nam trong `_views`, dang hien, nhung khong co trong stack
-> `Back()` khong bao gio dong duoc no. Dung tinh trang "dong het ma con lop che" ma doc-comment
cua DialogStack noi la se tranh. Ngoai ra `MessageRouter.OnError` khong duoc set o dau ca (grep: 0
ket qua) nen ngoai le thoat ra khoi `GopetClient.Update()`, bo not phan con lai cua frame
(`Ticked` khong ban, `_pendingDisconnectReason` khong duoc xu ly).

Sua: `_stack.Push(screen)` truoc, thanh cong moi ghi `_views`; bat `InvalidOperationException` de
huy view va bao loi thay vi de no noi len router.

### M3. UiRoot lo `DialogStack` ra ngoai duoi dang co the sua
`UiRoot.cs:26` `public DialogStack Stack => _stack;`

Ai goi `Stack.Pop()/Clear()/Push()` truc tiep se lam `_views` lech: view bi an vinh vien
(TopChanged -> SetActive(false)) va khong bao gio bi Destroy, hoac co man hinh trong stack ma
khong co view. Nen lo `Depth` / `Top` read-only, giu `Stack` internal cho test.

### M4. `Initialize()` goi hai lan -> dang ky trung
`UiRoot.cs:41-53` khong co chot. Goi lai (doi ket noi, reload scene) se tao HAI view cho moi goi.
Them `if (_guider != null) throw ...` hoac go dang ky cu truoc khi dang ky moi.

### M5. verify.ps1 buoc 8 khong quet `Assets\Tests`
`verify.ps1:104-122` chi quet `$root\Assets\Scripts` va `$root\tests`. Trong khi do
`Assets/Tests/PlayMode/UiRootTests.cs` = **259 dong**, `GenericMenuViewTests.cs` = **202 dong**,
vuot rule 200 ma verify van bao OK. Them `"$root\Assets\Tests"` vao danh sach roi tach file.

### M6. Back() tren hop Co/Khong lam roi cau hoi cua server
`UiRoot.cs:56-64`. Back huy view va khong gui `SEND_YES_NO`. Server khong treo (chi khong lam gi)
nhung `objectPerformed` con giu trang thai. Nen quyet dinh ro: hoac Back = tra loi "khong", hoac
ghi chu ro rang la co y bo qua.

---

## LOW

- **L1** `GuiderPackets.cs:14-15` `NpcOption = 5` trung `GopetCmd.NPC_OPTION = 5` da co san
  (GopetCmd.cs:34). Bo hang so nay, dung `GopetCmd.NPC_OPTION` — comment hien tai con goi y sai
  rang phai tu dinh nghia vi trung so voi SELECT_OPTION.
- **L2** `tools/check-asmdef-refs/index.js:64` comment noi bat ca "Gopet.X.Y dung truc tiep trong
  code", nhung regex chi bat `using ...;`. Loai dung fully-qualified khong co `using` se lot.
- **L3** `verify.ps1:17` header ghi "Bon buoc:" roi liet ke 8 buoc.
- **L4** `ListOptionScreen.cs:25-26` goi truong thu ba la "Nhan nut giua". Server dat ten no la
  `message`, co cho truyen tieu de (sendMenu.cs:276 truyen `titleStr` hai lan) hoac chuoi rong.
  Sua thanh "chu server gui kem, thuong la nhan nut OK".
- **L5** `ChoiceDialogView.Bind:58-63` / `InputDialogView.Bind:59-64`: `Destroy` hoan den cuoi frame
  nen nut cu con bam duoc trong frame do sau khi `_buttons.Clear()`. Hien khong ai bind lai nen
  chua no; nen `SetActive(false)` ngay truoc khi Destroy.
- **L6** `MenuVirtualizer.cs:71` `if (lastExclusive <= first) return new VisibleRange(first, 0);`
  khong the xay ra (sau clamp luon co lastExclusive > first). Nhanh chet.
- **L7** `GuiderPackets.SelectMenuElementWithPayment` gui vi tri paymentOption. Hien dung vi server
  dat `PaymentOption(i, ...)` (MenuController.cs:862) nen id == vi tri, nhung nen gui
  `PaymentOptions[i].Id` cho nhat quan voi C1.

---

## Lo hong test (vi sao C1/C2 lot)
- `MenuScreenTests.cs` khong co mot assert nao tren `Items[i].ItemId`.
- `UiRootTests.cs:55` moi dong deu `PutInt(0)` -> itemId == 0 cho tat ca;
  `MenuVirtualizationTests.cs:47` dat `ItemId = i` -> luon trung index. Khong vector nao co
  itemId != index.
- `GuiderWireTests.SelectMenuElement_KhopTungByteVoiClientJ2me` dung ATM (id == vi tri) nen khong
  phan biet duoc hai gia thuyet.

Test can them (fail voi code hien tai, pass sau khi sua C1/C2):
1. `MenuScreen` 3 dong voi itemId = {100, 7, -1}; `Select(screen, 2)` phai ra `... ffffffff`.
2. `ListOptionScreen` voi id {0,1,3,2}; `Select(screen, 2)` phai gui 3.
3. `Bind()` lan hai sang man hinh khac -> `RowAt(0).Item.Title` phai la cua man hinh moi (H1).

## Fact-check plan (phase-05-generic-ui-components.md)
- "Tu `GameController.cs:818-861`" -> thuc te `showMenuItem` o **826-871**.
- Bang sub-command liet ke `4 SEND_YES_NO` nam trong `COMMAND_GUIDER (122)`. Sai: no di trong
  `SERVER_MESSAGE (45)` (MenuController.cs:1092-1100). **Code lam dung, plan sai.**
- Bang ghi `2 NPC_GUIDER | S->C`. Sai chieu: NPC_GUIDER la C->S (GameController.cs:751), chieu
  xuong la NPC_OPTION (5). Code lam dung.
- "nguoi dung chon index thu N -> gui SELECT_MENU_ELEMENT(listID, N)" — day chinh la goc cua C1.
  Phai sua cau nay trong plan cung luc voi code.

## Diem tot
- Thu tu doc `showMenuItem` khop tuyet doi, ke ca khoi 3 chuoi dieu kien; test tron dong co va
  khong co dialog trong cung mot goi la cach bat lech stream re nhat.
- Doc `listId` hai lan dung y server (GameController.cs:4609-4610).
- Tach `SEND_YES_NO` sang bao ngoai `SERVER_MESSAGE` — cho rat de dat nham, lam dung.
- `ExpectFullyConsumed` + tran so luong (`MaxItems`, `MaxOptions`, `MaxPaymentOptions`,
  `MaxFields = 32` khop nguong server o GameController.cs:790) — bien gioi tin cay xu ly nghiem tuc.
- `MenuVirtualizerTests` phu du canh: rong, viewport 0, cuon am, cuon qua cuoi, lech nua dong.
- `check-asmdef-refs` va `run-playmode-tests.ps1` (doc XML thay vi tin exit code) that su bit duoc
  hai lo hong da tung dinh.
- Khong mot cho nao re nhanh theo `listId` — nguyen tac cot loi cua phase duoc giu nguyen.

## Thu tu de nghi
1. C1, C2 + 3 test moi (chan sai chuc nang tren server that).
2. H1, H2 (hai bug view co that, sua re).
3. M5 (verify dang bo sot rule), M2, M1.
4. H3 (canvas + anchor) truoc khi ai do thu bam bang tay.
5. M3, M4, L1-L7 khi tien tay.
