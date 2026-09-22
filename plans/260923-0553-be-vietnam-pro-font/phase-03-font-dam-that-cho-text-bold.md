---
phase: 3
title: "Font dam that cho text Bold"
status: pending
priority: P3
effort: "1.5h"
dependencies: [2]
---

# Phase 3: Font đậm thật cho text Bold

## Overview
Legacy `Text` chỉ gắn 1 TTF/Font → `FontStyle.Bold` trên Be Vietnam Pro-Regular là faux-bold (Unity tự làm dày, hơi nhòe). Phase này cho 105 chỗ Bold dùng file SemiBold thật. **Có thể hoãn** nếu Phase 4 thấy faux-bold chấp nhận được.

## Requirements
- Functional: chữ đậm dùng `BeVietnamPro-SemiBold`; không còn faux-bold trên các view dựng bằng `UiBuilder`.
- Non-functional: một helper duy nhất (DRY), thay thế cơ học, không đổi layout ngoài độ đậm.

## Architecture
**Quyết định:** chỉ dùng SemiBold (600), không nhập file Bold (700).

**Bước 0 — spike (15 phút):** kiểm tra Unity có tự lấy `BeVietnamPro-SemiBold` cùng family khi đặt `fontStyle = Bold` hay không. Unity thường chỉ khớp style "Bold" → nhiều khả năng KHÔNG tự nhận SemiBold; nếu vậy dùng helper bên dưới. Nếu có tự nhận → bỏ helper.

Nếu không, thêm helper vào `UiBuilder`:
```csharp
private const string BoldFontPath = "Fonts/BeVietnamPro/BeVietnamPro-SemiBold";
private static Font _boldFont;

/// Đậm thật bằng file SemiBold; thiếu asset → faux-bold trên font mặc định.
public static void MakeBold(Text text)
{
    if (_boldFont == null) _boldFont = Resources.Load<Font>(BoldFontPath);
    if (_boldFont != null) { text.font = _boldFont; text.fontStyle = FontStyle.Normal; }
    else text.fontStyle = FontStyle.Bold;
}
```
Thay `X.fontStyle = FontStyle.Bold;` → `UiBuilder.MakeBold(X);`.

Lưu ý: chỗ nào gán lại `text.font = _font` SAU khi làm đậm sẽ mất Bold → grep kiểm tra thứ tự.

## Related Code Files
- Modify: `GopetUnityClient/Assets/Scripts/Runtime/UI/UiBuilder.cs`
- Modify: các file có `fontStyle = FontStyle.Bold` (105 chỗ, `grep -rn "fontStyle = FontStyle.Bold" Assets/Scripts`)
- Không đổi: 4 chỗ Italic (placeholder input) — faux-italic chấp nhận được, YAGNI.
- Không đổi: chỗ Bold có điều kiện (vd. `fontStyle = cond ? Bold : Normal`) → xử lý tay từng chỗ nếu có.

## Implementation Steps
1. Spike bước 0; ghi kết quả vào phase này.
2. Thêm `MakeBold` (nếu cần).
3. Replace cơ học bằng regex `(\w[\w.]*)\.fontStyle = FontStyle\.Bold;` → `UiBuilder.MakeBold($1);`; rà tay các chỗ không khớp regex.
4. Grep `\.font = ` sau `MakeBold` trong cùng hàm để chắc không bị ghi đè.
5. Compile sạch.

## Success Criteria
- [ ] `grep -rn "fontStyle = FontStyle.Bold" Assets/Scripts` chỉ còn trong `MakeBold`.
- [ ] Tiêu đề popup / nút nhìn đậm sắc nét, không nhòe viền.

## Risk Assessment
- SemiBold rộng hơn Regular → thêm nguy cơ tràn nút; gộp chung vào kiểm tra Phase 4.
- Test so sánh `fontStyle == Bold` (nếu có) sẽ fail → sửa test kiểm `font.name` thay vì `fontStyle`.
