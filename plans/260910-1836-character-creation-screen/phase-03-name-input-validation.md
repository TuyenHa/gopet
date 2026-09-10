---
phase: 3
title: "Name Input & Validation"
status: complete
priority: P1
effort: "0.5d"
dependencies: [1]
---

# Phase 3: Name Input & Validation

## Overview

Component input tên nhân vật với validation inline mỗi keystroke, khớp `AuthRules.IsValidCharacterName`. Kỹ thuật: tách khỏi root view để test độc lập.

## Requirements

- **Functional:**
  - `InputField` chấp nhận 0-20 ký tự (character limit = 20).
  - Ép chữ thường + a-z0-9: filter `onValueChanged` — bỏ ký tự sai thay vì báo lỗi mỗi lần gõ.
  - Label lỗi phía dưới field, cập nhật realtime:
    - < 5 char → "Tên phải từ 5 đến 20 ký tự."
    - Chứa ký tự ngoài `[a-z0-9]` sau khi filter (không nên xảy ra) → cùng thông báo.
    - Hợp lệ → label rỗng.
  - Public `bool IsValid` để root query.
- **Non-functional:**
  - **DRY:** validation lấy từ `AuthRules.IsValidCharacterName` — không copy regex.
  - Filter nhân bản input case-insensitive nhưng gửi lên server bằng lowercase.

## Architecture

```
CharacterNameInput (RectTransform + InputField + Text (label lỗi))
├─ Placeholder "Tên nhân vật (5-20, a-z 0-9)"
├─ Input field (uppercase-off, character limit 20)
└─ Error label (đỏ, empty khi hợp lệ)
```

**Filter logic (onValueChanged):**
```csharp
private static string Sanitize(string raw)
{
    var sb = new StringBuilder(raw.Length);
    foreach (var c in raw)
    {
        if (c >= 'A' && c <= 'Z') sb.Append((char)(c + 32));
        else if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) sb.Append(c);
    }
    return sb.ToString();
}
```

Gọi lại `field.text = sanitized` khi khác → tránh loop bằng flag guard.

## Related Code Files

- **Create:**
  - `Assets/Scripts/Runtime/UI/CharacterNameInput.cs` (~130 dòng)
- **Read:**
  - `Assets/Scripts/Net/Auth/AuthRules.cs:44-73` — dùng constant + method có sẵn.
  - `Assets/Scripts/Runtime/UI/UiBuilder.cs` — style field + label.
  - `Assets/Scripts/Runtime/UI/LoginFormView.Fields.cs` — tham khảo pattern InputField hiện có.

## Implementation Steps

1. Tạo `CharacterNameInput : MonoBehaviour`.
2. `static Create(Transform parent, Font font)` — dựng field + placeholder + error label.
3. `onValueChanged`: sanitize + validate:
   ```csharp
   var clean = Sanitize(value);
   if (clean != value) { _guard = true; field.text = clean; _guard = false; return; }
   IsValid = AuthRules.IsValidCharacterName(clean, out var error);
   _errorLabel.text = error ?? string.Empty;
   Changed?.Invoke(clean);
   ```
4. Public API:
   ```csharp
   public bool IsValid { get; private set; }
   public string Value => _field.text;
   public event Action<string> Changed;
   public void SetServerError(string message); // set label khi server phản hồi từ chối
   public static CharacterNameInput Create(Transform parent, Font font);
   ```
5. `SetServerError`: dùng khi Phase 4 nhận `LoginFailed`/`redDialog` với tên trùng — hiển thị lên label mà không xoá text người dùng.

## Success Criteria

- [ ] `CharacterNameInput.cs` < 200 dòng.
- [ ] Compile OK.
- [ ] Test đơn giản: gõ "ABC12" → text hiển thị "abc12"; `IsValid == true` khi >= 5 char.
- [ ] Ký tự Việt/space bị lọc mất.
- [ ] Label lỗi rỗng khi hợp lệ.

## Risk Assessment

- **R1: IME/Unicode input** — user dán tiếng Việt có dấu → filter phải strip đúng. Test: "Tên A" → "n" (giữ n). OK vì filter dứt khoát.
- **R2:** `AuthRules.IsValidCharacterName` chưa xử lý null → `Sanitize` luôn trả string non-null, an toàn.
