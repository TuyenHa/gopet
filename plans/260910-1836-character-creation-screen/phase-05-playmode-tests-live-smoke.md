---
phase: 5
title: "PlayMode Tests + Live Smoke"
status: partial
priority: P2
effort: "0.5d"
dependencies: [4]
---

## Kết quả (2026-09-11)

- Unit test `LoginFlowCharacterTests.cs`: đã có sẵn, phủ đủ mọi case phase yêu cầu (happy/reject/
  tên sai/rớt mạng/nối lại) — xác nhận lại xanh, 625/625 trong `dotnet test tests/Gopet.Net.Tests`.
  `Sanitize` đã public, không cần đổi signature.
- PlayMode test `CharacterCreationViewTests.cs`: đã có sẵn, đủ 2 case (sanitize+validate,
  submit nữ). **Chưa chạy được** — `run-playmode-tests.ps1` cần đóng Unity Editor, Editor đang mở
  lúc kiểm tra; không tự ý đóng.
- Live-smoke đăng ký→tạo char→vào map: **chặn bởi 2 lý do ngoài tầm code** — REGISTER bị chính
  sách server chặn theo danh sách từ cấm, và DB Docker chứa dữ liệu người dùng thật nên không tự
  ý chèn account test. Xem `reports/live-smoke.md` để biết chi tiết + 2 lựa chọn đề xuất cho
  người dùng.
- **Cần người dùng**: (1) đóng Unity Editor rồi chạy `run-playmode-tests.ps1` tay; (2) quyết định
  có cho chèn 1 account test vào DB để live-smoke đầy đủ hay không.

# Phase 5: PlayMode Tests + Live Smoke

## Overview

Test hai tầng: unit (thuần C# — validation + LoginFlow state machine) và PlayMode (dựng view thật, giả lập tap/type). Live-smoke với server docker cuối cùng để confirm reconnect flow.

## Requirements

- **Functional:** cover 3 kịch bản chính:
  1. **Happy path:** đăng ký mới → login → view hiện → gõ tên valid → chọn nữ → submit → server accept → reconnect → vào map.
  2. **Tên trùng:** submit tên đã có → server trả reject → view giữ tên gõ, error label hiện lý do.
  3. **Tên sai format client-side:** gõ 3 ký tự → submit disabled; gõ "AB C1@" → filter thành "abc1"; đủ 5 char thì submit enabled.
- **Non-functional:**
  - Unit test chạy dưới `Gopet.Net.Tests` (thuần C#, không cần Unity).
  - PlayMode chạy dưới `Gopet.PlayMode.Compile`.
  - Live-smoke tay: script `run-playmode-tests.ps1` + doc note.

## Architecture

**Unit test:**
- `CharacterNameInputSanitizeTests` — pure logic của `Sanitize` (không cần MonoBehaviour). Cho vào `Gopet.UiLogic.Tests` nếu tách được; nếu Sanitize là private → expose thành `internal static` cho test.
- `LoginFlowCreateCharacterTests` — mở rộng test hiện có: happy, reject, timeout.

**PlayMode test:**
- `CharacterCreationViewTests.cs` — dựng view, giả lập:
  - Gõ text bằng `field.text = "abc12"` + `field.onValueChanged.Invoke("abc12")`.
  - Tap avatar bằng `ExecuteEvents.Execute<IPointerClickHandler>`.
  - Bấm submit bằng `button.onClick.Invoke()`.
  - Assert `Submitted` event fire với (gender=1, name="abc12").

**Live-smoke:**
- Tài liệu bước tay trong `plans/260910-1836-character-creation-screen/reports/live-smoke.md`.

## Related Code Files

- **Create:**
  - `tests/Gopet.Net.Tests/UiLogic/LoginFlowCreateCharacterTests.cs`
  - `tests/Gopet.PlayMode.Compile/CharacterCreationViewTests.cs`
  - `plans/260910-1836-character-creation-screen/reports/live-smoke.md`
- **Modify (có thể):**
  - `Assets/Scripts/Runtime/UI/CharacterNameInput.cs` — đổi `Sanitize` thành `internal static` để test được.

## Implementation Steps

1. Viết unit test `CharacterNameInputSanitizeTests`:
   ```csharp
   [Fact] void RemovesUppercase() => Assert.Equal("abc", Sanitize("aBc"));
   [Fact] void KeepsDigits() => Assert.Equal("abc12", Sanitize("abc12"));
   [Fact] void StripsUnicode() => Assert.Equal("", Sanitize("Tên"));
   [Fact] void HandlesEmpty() => Assert.Equal("", Sanitize(""));
   ```
2. Viết unit test `LoginFlowCreateCharacterTests`:
   - `SubmitInvalidName_ReturnsFalse_SetsNotice`
   - `SubmitValidName_SendsPacket_ArmsTimeout`
   - `OnDisconnect_AfterSubmit_TransitionsToReconnecting`
   - `Reconnect_AutoLoginsSameCredentials`
3. Viết PlayMode test `CharacterCreationViewTests`:
   - Setup: mock `LoginFlow` (hoặc dùng `FakeLoginFlow`).
   - Cases: happy submit, reject shows error, submit disabled while invalid.
4. Chạy:
   ```
   powershell -File D:\game\GopetUnityClient\verify.ps1
   powershell -File D:\game\GopetUnityClient\run-playmode-tests.ps1
   ```
5. Live-smoke tay:
   - `docker compose up` server.
   - Chạy Unity build; đăng ký tài khoản mới; sang view; nhập tên; submit; verify vào map.
   - Chụp lại flow (jar side-by-side nếu cần đối chiếu).
6. Viết `plans/260910-1836-character-creation-screen/reports/live-smoke.md` ghi kết quả.

## Success Criteria

- [ ] `verify.ps1` 10/10 (nếu có violation mới do plan này, phải fix).
- [ ] `run-playmode-tests.ps1` PASS toàn bộ.
- [ ] Live-smoke: đăng ký + tạo char + vào map trong <30 giây end-to-end.
- [ ] Report `live-smoke.md` có screenshot/log kết quả.

## Risk Assessment

- **R1:** `Gopet.PlayMode.Compile` chỉ compile — không chạy được assertion runtime nếu không có Unity Editor. → Chạy tay bằng `run-playmode-tests.ps1` cần Unity đóng; nếu CI chưa có, ghi vào docs.
- **R2:** Server docker có thể chưa seed tài khoản mới được. → Verify `docker compose` config cho DB reset trước live-smoke.
- **R3:** LoginFlow test dùng cờ private `_expectingCloseAfterCreate`. → Nếu chưa `internal`, để test qua behavior thay vì state.
