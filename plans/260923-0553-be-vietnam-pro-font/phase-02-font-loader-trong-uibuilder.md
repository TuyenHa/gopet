---
phase: 2
title: "Font loader trong UiBuilder"
status: pending
priority: P1
effort: "1h"
dependencies: [1]
---

# Phase 2: Font loader trong UiBuilder

## Overview
Đổi `UiBuilder.BuiltinFont()` thành `DefaultFont()` nạp Be Vietnam Pro, fallback LegacyRuntime/Arial. Vì mọi view đi qua hàm này, đây là thay đổi lõi duy nhất.

## Requirements
- Functional: mọi `Text` dựng qua `UiBuilder` dùng Be Vietnam Pro; thiếu asset → vẫn có chữ (Arial) + log warning.
- Non-functional: cache kết quả (tránh `Resources.Load` lặp ~100 lần khi dựng UI); `UiBuilder.cs` vẫn < 200 dòng.

## Architecture
```csharp
private const string DefaultFontPath = "Fonts/BeVietnamPro/BeVietnamPro-Regular";
private static Font _defaultFont;

/// Font UI mặc định: Be Vietnam Pro (dấu tiếng Việt chuẩn). Thiếu asset → font dựng sẵn của Unity.
public static Font DefaultFont()
{
    if (_defaultFont != null) return _defaultFont;
    _defaultFont = Resources.Load<Font>(DefaultFontPath);
    if (_defaultFont == null)
    {
        Debug.LogWarning($"[Gopet] Thiếu font {DefaultFontPath} — dùng font dựng sẵn.");
        _defaultFont = LegacyFont();
    }
    return _defaultFont;
}

// Giữ nguyên logic + comment cũ của BuiltinFont() (LegacyRuntime.ttf ?? Arial.ttf, LogError khi null).
private static Font LegacyFont() { ... }
```

Đổi tên `BuiltinFont` → `DefaultFont`: tên cũ sai nghĩa sau thay đổi. Đổi tên cơ học (replace toàn bộ symbol), không đổi chữ ký.

## Related Code Files
- Modify: `GopetUnityClient/Assets/Scripts/Runtime/UI/UiBuilder.cs`
- Modify (đổi tên cơ học): ~60 file trong `Assets/Scripts` + `Assets/Tests` đang gọi `UiBuilder.BuiltinFont()`
- Modify: `GopetUnityClient/Assets/Tests/PlayMode/UiContrastTests.cs` — thêm assert font là Be Vietnam Pro

## Implementation Steps
1. Tách thân `BuiltinFont()` hiện tại thành `private static Font LegacyFont()`.
2. Thêm `DefaultFont()` + cache như trên.
3. Replace `UiBuilder.BuiltinFont()` → `UiBuilder.DefaultFont()` toàn bộ `Assets/Scripts` và `Assets/Tests` (grep xác nhận còn 0 lời gọi cũ).
4. `UiContrastTests`: giữ assert not-null, thêm `StringAssert.Contains("BeVietnamPro", UiBuilder.DefaultFont().name)` → bắt lỗi asset bị xóa/đổi tên (fallback im lặng sẽ không lộ nếu không có test này).
5. Biên dịch (Unity batchmode hoặc mở Editor), đảm bảo 0 compile error.

## Success Criteria
- [ ] `grep -rn "BuiltinFont" Assets` = 0 kết quả.
- [ ] Compile sạch; test mới pass.
- [ ] Play: màn đăng nhập hiển thị Be Vietnam Pro.

## Risk Assessment
- Cache static giữ tham chiếu qua domain reload bị tắt (Enter Play Mode Options) → asset vẫn hợp lệ vì là Resources asset; không rủi ro thực.
- Đổi tên hàng loạt có thể lỡ chỗ trong comment/doc — vô hại, grep bắt được.
