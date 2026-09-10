---
phase: 4
title: "Screen Wire-up + Reconnect Flow"
status: complete
priority: P1
effort: "1d"
dependencies: [2, 3]
---

# Phase 4: Screen Wire-up + Reconnect Flow

## Overview

Ráp `CharacterPreviewPanel` + `CharacterNameInput` thành `CharacterCreationView`, replace nhánh `BuildCharacterCreation` trong `LoginScreens`, và verify flow "server đóng kết nối → reconnect → auto-login" chạy đúng.

## Requirements

- **Functional:**
  - View mới xuất hiện khi `LoginStage.CreatingCharacter`.
  - Nút submit disabled đến khi `NameInput.IsValid == true`.
  - Bấm submit → `LoginFlow.SubmitCharacter(name, gender)`.
  - Server từ chối (tên trùng, sai regex): hiện thông báo trong error label của `CharacterNameInput` VÀ giữ text người dùng.
  - Server chấp nhận → server đóng kết nối → view hiện overlay "Đang tạo nhân vật…" → `LoginFlow` reconnect + auto-login → chuyển sang `LoginStage.Ready`.
- **Non-functional:**
  - Không đụng wire format.
  - `LoginScreens.BuildCharacterCreation` thay hoàn toàn, không giữ `FormView` cũ (YAGNI — nếu cần fallback thì thêm sau).

## Architecture

```
CharacterCreationView
├─ Header: "Chọn nhân vật"
├─ CharacterPreviewPanel (Phase 2)
├─ CharacterNameInput (Phase 3)
├─ Notice label (từ LoginFlow.Notice — reject nếu có)
├─ [Tạo nhân vật] button
└─ [Quay lại] button (call LoginFlow.Reconnect nếu cần)

LoginScreens.BuildCharacterCreation()
├─ Instantiate CharacterCreationView
├─ Bind view.Submitted → _flow.SubmitCharacter(name, gender)
│  └─ Nếu return false → view.ShowNotice(_flow.Notice)
├─ Bind view.Cancelled → _flow.Reconnect()  # về EnteringCredentials
└─ Subscribe _flow.RejectionRaised (nếu event có) → view.SetServerError
```

**Reconnect flow (đã có ở LoginFlow):**
1. `SubmitCharacter` set `_expectingCloseAfterCreate = true` + `ArmTimeout(true)`.
2. Server đóng socket → `LoginFlow.OnDisconnected` thấy cờ, chuyển sang stage `Reconnecting`.
3. `Reconnecting` → mở socket mới → `Resume()` → `SendLogin()` (auto-login).
4. `LoginSucceeded` → `LoginStage.Ready`.

Việc phase 4: hiển thị overlay "Đang tạo nhân vật…" khi ở stage `Reconnecting` (đã có `LoginScreens.OnStageChanged`, chỉ cần verify).

## Related Code Files

- **Create:**
  - `Assets/Scripts/Runtime/UI/CharacterCreationView.cs` (~180 dòng)
- **Modify:**
  - `Assets/Scripts/Runtime/UI/LoginScreens.cs` — thay `BuildCharacterCreation` (~15 dòng thay ~15 dòng cũ, không tăng tổng).
- **Read:**
  - `Assets/Scripts/UiLogic/LoginFlow.Account.cs:141-179` — `OnCharacterRequired`, `SubmitCharacter`, `Resume`.
  - `Assets/Scripts/UiLogic/LoginFlow.Timeout.cs` — timeout logic (không sửa, chỉ verify chạy).

## Implementation Steps

1. Tạo `CharacterCreationView`:
   ```csharp
   public event Action<sbyte, string> Submitted;
   public event Action Cancelled;
   public void SetServerNotice(string message);
   public static CharacterCreationView Create(Transform parent, Font font);
   ```
2. Trong `Create`: dựng RectTransform full-canvas, đặt `CharacterPreviewPanel` ở giữa (~40% chiều cao), `CharacterNameInput` dưới, notice label + 2 button dưới cùng.
3. Bind:
   - `_preview.GenderChanged += g => UpdateSubmitEnabled();`
   - `_input.Changed += _ => UpdateSubmitEnabled();`
   - `_submit.onClick += () => Submitted?.Invoke(_preview.SelectedGender, _input.Value);`
   - `_cancel.onClick += () => Cancelled?.Invoke();`
4. `UpdateSubmitEnabled`: `_submit.interactable = _input.IsValid;`
5. `SetServerNotice(msg)`: gán vào `CharacterNameInput.SetServerError(msg)`.
6. Sửa `LoginScreens.BuildCharacterCreation`:
   ```csharp
   var view = CharacterCreationView.Create(transform, _font);
   view.Submitted += (gender, name) =>
   {
       if (!_flow.SubmitCharacter(name, gender)) view.SetServerNotice(_flow.Notice);
   };
   view.Cancelled += () => _flow.Reconnect();
   ```
7. Xoá cả block cũ dùng `FormView` (không giữ dead code — YAGNI).
8. Verify `OnStageChanged(Reconnecting)`: hiện `_reconnectView` hoặc gọi `ConnectionPopupView.Show("Đang tạo nhân vật…")`. Nếu chưa có, dùng lại `ConnectionPopupView` sẵn có.

## Success Criteria

- [ ] `CharacterCreationView.cs` < 200 dòng.
- [ ] `LoginScreens.cs` không tăng > 200 dòng (hiện 231 — chỉ replace không add).
- [ ] verify.ps1 9/10 pass, không đẻ violation mới.
- [ ] Đăng ký + đăng nhập → thấy view mới → nhập tên hợp lệ + chọn nữ → submit → server đóng → auto-reconnect → vào map.

## Risk Assessment

- **R1: Server đóng kết nối → LoginFlow không detect close.** Có thể do `OnDisconnected` chưa handle cờ `_expectingCloseAfterCreate`. → Verify code path trong `LoginFlow.Timeout.cs` và `GopetSocket.OnClose`. Nếu miss → phase 4 patch thêm.
- **R2: Auto-relogin gửi lại username/password chưa cached.** → `LoginFlow._username` được lưu ở lần đầu login; `Resume()` check `_autoLogin` flag. Verify flag set đúng lúc.
- **R3: `Physics2DRaycaster` chưa có ở login camera.** Preview panel sẽ không tap được. → Thêm 1 dòng trong `LoginScreens.Initialize`.
