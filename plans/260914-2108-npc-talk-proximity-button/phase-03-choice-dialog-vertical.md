# Phase 03 — ChoiceDialogView xếp nút DỌC khi > 3 option

## Context
- [plan.md](plan.md)
- File đọc: `Runtime/UI/ChoiceDialogView.cs`, `Runtime/UI/UiRoot.cs:143-208`,
  `Assets/Tests/PlayMode/UiRootDialogTests.cs`, `UiContrastTests.cs`, `PointerPathTests.cs`

## Overview
- Priority: P1 — Status: pending — Effort: 1h
- Sửa layout popup option để 7 nút của TRAN CHAN đọc được. Không đổi API, không đổi luồng chọn.

## Key Insights (đã xác minh — đây là lý do BẮT BUỘC phải sửa)
- `ChoiceDialogView.MakeButton` (ChoiceDialogView.cs:103-127) xếp TẤT CẢ nút trên MỘT HÀNG NGANG:
  `width = Mathf.Min(170f, (PanelWidth - 48f - ButtonGap*(count-1)) / count)` với
  `PanelWidth = 420`, `ButtonGap = 12`. Với count = 7 → `(420-48-72)/7 ≈ 42.8px` cho nhãn 16px
  kiểu "Nhận pet miễn phí" ⇒ không đọc được. Với count = 5 → 60px, vẫn hỏng.
- `PanelHeight = 190` cố định, vùng message chiếm anchor Y 0.42→1.0, nút neo `y = 16` (dòng 113).
- `ShowNpcOptions` (UiRoot.cs:193-208) và `ShowListOption` (UiRoot.cs:143-160) đều đẩy N nhãn
  tuỳ ý vào đây ⇒ sửa ở `ChoiceDialogView` là DRY, đồng thời vá cả list-option của server.
- Caller ≤ 2-3 nút: `ShowYesNo` (2), `ShowConfirm` (2), `UiRoot.Wings/Gems`, `LoginScreens`,
  `GameSession.Channels` — giữ nguyên hàng ngang để không đổi giao diện quen thuộc.
- Test hiện có chỉ dùng `Message`, `Choose(index)`, `Buttons` — **không assert toạ độ/kích thước**
  (UiRootDialogTests.cs:46-49,83; UiRootTests.cs:82-126; WingInventoryUiTests.cs:44,59) ⇒ rủi ro
  hồi quy test thấp.
- `ChoiceDialogView.cs` đang 164 dòng, gate 200 dòng và file này KHÔNG nằm trong allowlist
  `verify.ps1:142-149` ⇒ phần layout phải tách sang partial mới.

## Requirements
**Functional**
- `count <= 3`: giữ nguyên hàng ngang hiện tại (pixel-identical).
- `count >= 4`: xếp dọc, mỗi nút rộng `PanelWidth - 48`, cao 42px, cách nhau 8px, nhãn canh giữa.
- Panel cao động: `message area + count * (rowH + gap) + padding`, clamp ≤ 88% chiều cao màn hình;
  nếu vượt → giảm `rowH` xuống tối thiểu 30px và font nhãn xuống 14 (không cần scroll cho ≤ 12 mục).
- Nút đầu vẫn giữ màu nhấn như hiện tại; thứ tự index không đổi (index = thứ tự option server).

**Non-functional**
- Không đổi chữ ký public (`Create`, `Bind`, `Choose`, `Chosen`, `Closed`, `Buttons`, `Message`).
- Mỗi file < 200 dòng.

## Architecture
```
UiRoot.ShowNpcOptions(NpcOptions)        // không đổi
   └─ view.Bind(title, labels[N])
         └─ ChoiceDialogLayout.Compute(N, panelWidth, screenHeight)
               → { vertical: bool, rowW, rowH, gap, panelHeight, fontSize }
         └─ MakeButton(label, i, layout)  // đặt anchoredPosition theo layout
   └─ Chosen(index) → _guider.SelectNpcOption(npcId, options[index].Id)   // không đổi
```

## Related Code Files
**Tạo mới**
- `Assets/Scripts/Runtime/UI/ChoiceDialogView.Layout.cs` — `partial class ChoiceDialogView`
  chứa struct `ButtonLayout` + `ComputeLayout(int count)` + `ApplyPanelHeight(float h)`.
  (Nếu muốn test thuần C#: tách phần tính số thành `UiLogic/ChoiceDialogLayout.cs` — chỉ math,
  không Unity type — rồi partial gọi vào. Ưu tiên cách này để test được ở `tests/Gopet.Net.Tests`.)

**Sửa**
- `Assets/Scripts/Runtime/UI/ChoiceDialogView.cs` — `sealed class` → `sealed partial class`;
  `Bind` tính layout một lần rồi truyền vào `MakeButton`; `MakeButton` dùng layout thay cho công
  thức inline dòng 110-113; gọi `ApplyPanelHeight`.

**Không đụng:** `UiRoot.cs` (kể cả `ShowNpcOptions`), `GuiderHandler`, test cũ.

## Implementation Steps
1. Tạo `UiLogic/ChoiceDialogLayout.cs`: `public readonly struct Row { float Width, Height, Gap, X, Y }`
   hoặc gọn hơn: `Compute(int count, float panelWidth, float screenHeight)` trả
   `(bool vertical, float rowWidth, float rowHeight, float gap, float panelHeight, int fontSize)`.
   Quy tắc: `vertical = count > 3`; ngang giữ đúng công thức cũ để phase này không đổi giao diện
   các hộp 2-3 nút.
2. `ChoiceDialogView.Layout.cs`: hàm đặt `anchoredPosition` cho nút thứ i:
   - ngang: giữ nguyên công thức cũ (`-total/2 + w/2 + i*(w+gap)`, y = 16).
   - dọc: `x = 0`, `y = bottomPadding + (count-1-i) * (rowH + gap)` (index 0 nằm TRÊN CÙNG —
     khớp thứ tự server gửi).
3. `Bind`: sau khi biết `count`, gọi `ApplyPanelHeight(layout.panelHeight)` (đặt
   `panelRect.sizeDelta = new Vector2(PanelWidth, h)`), rồi tạo nút.
4. Vùng message: khi dọc, dùng `offsetMin.y = buttonsTop` thay vì anchor 0.42 cố định — với
   `ShowNpcOptions` message rỗng nên vùng này co lại là đúng.
5. Chạy PlayMode test (`run-playmode-tests.ps1`, cần đóng Unity Editor) + `verify.ps1`.

## Todo List
- [ ] `UiLogic/ChoiceDialogLayout.cs` (thuần math) + test số học
- [ ] `ChoiceDialogView.Layout.cs` partial đặt vị trí nút
- [ ] `ChoiceDialogView.cs` dùng layout, panel cao động, < 200 dòng
- [ ] Hộp 2 nút (YesNo/Confirm) giữ nguyên bố cục cũ — kiểm tra bằng mắt + test cũ
- [ ] `verify.ps1` + PlayMode test xanh

## Success Criteria
- Bind 7 nhãn thật của TRAN CHAN → 7 nút xếp dọc, mỗi nút rộng ≥ 300px, nhãn không bị cắt.
- Popup không tràn khỏi màn hình ở 1280x720 và 960x540.
- `UiRootTests` / `UiRootDialogTests` / `WingInventoryUiTests` / `UiContrastTests` vẫn pass.

## Risk Assessment
| Rủi ro | KN | TĐ | Giảm thiểu |
|--------|----|----|-----------|
| Đổi layout làm xấu các hộp 2 nút | TB | TB | Chỉ đổi khi count > 3; nhánh ngang copy công thức cũ |
| Panel tràn màn hình khi N lớn (list option server) | TB | TB | Clamp 88% + giảm rowH/font |
| `UiContrastTests` đỏ do đổi màu chữ | Thấp | Thấp | Không đổi bảng màu, chỉ đổi hình học |
| File vượt 200 dòng | Cao nếu nhồi 1 file | TB | Tách partial + file math riêng |

## Security Considerations
Không — chỉ trình bày. `SelectNpcOption` vẫn dùng `options[index].Id` của server (UiRoot.cs:202),
không suy đoán id từ vị trí nút.

## Next Steps
→ Phase 04 kiểm thử tích hợp và QA tay.
