---
phase: 1
title: "Discovery + UI Contract"
status: complete
priority: P1
effort: "0.5d"
dependencies: []
---

# Phase 1: Discovery + UI Contract

## Overview

Chốt hạ tầng còn thiếu (asset, event, contract) trước khi dựng view. Sản phẩm ra: 1 file cấu trúc `CharacterCreationView` + danh sách event bắt buộc + wire-in-out xác định.

## Requirements

- **Functional:** liệt kê CSS/asset & event cần dùng — không code, chỉ chốt contract.
- **Non-functional:** khớp jar-style (bitmap font, colors) và tận dụng `AvatarAppearance` + `JarNameLabel` + `UiBuilder` đã có.

## Architecture

Component tách 3 mảnh:

```
CharacterCreationView (root, MonoBehaviour)
├─ CharacterPreviewPanel  (Phase 2 — 2 avatar sprite nam/nữ + highlight chọn)
├─ CharacterNameInput      (Phase 3 — InputField + inline validation label)
└─ Buttons: [Tạo nhân vật] [Hủy] (root)
```

**Events (root):**
- `event Action<sbyte, string> Submitted` — (gender 0|1, name)
- `event Action Cancelled` — chỉ dùng khi test/UI polish; Phase 4 wire vào `LoginFlow.Reconnect`.

**External deps đã có (không tạo mới):**
- `AvatarAppearance.Create(Transform, int gender)` — render sprite mặc định theo giới tính.
- `AuthRules.IsValidCharacterName(name, out error)` — regex + length check.
- `LoginFlow.SubmitCharacter(name, gender)` — gọi từ `LoginScreens` sau khi validate.
- `UiBuilder.BuiltinFont()`, `UiBuilder.MakeText`, `UiBuilder.Field`, `UiBuilder.ButtonFace`.

## Related Code Files

- **Đọc để tham chiếu (không sửa):**
  - `Assets/Scripts/Runtime/UI/LoginScreens.cs:180-190` — nhánh hiện tại.
  - `Assets/Scripts/UiLogic/LoginFlow.Account.cs:141-163` — `OnCharacterRequired` + `SubmitCharacter`.
  - `Assets/Scripts/Net/Auth/AuthRules.cs:44-73` — validation rules.
  - `Assets/Scripts/Runtime/World/AvatarAppearance.cs` — API tạo sprite.
- **Chưa tạo file nào** ở phase 1.

## Implementation Steps

1. Đọc lại 4 file "Đọc để tham chiếu" ở trên; ghi chú cụ thể vào `plans/260910-1836-character-creation-screen/reports/discovery.md`:
   - Asset avatar (paths gender-specific đã dùng trong `AvatarAppearance.Build`).
   - Colors/fonts hiện dùng cho login/register (khớp `LoginBackground`, `LoginFormView`).
   - Layout mockup ASCII (2 avatar side-by-side, input dưới, submit button dưới cùng).
2. Xác nhận `AvatarAppearance` có thể chạy ngoài map-scene (không phụ thuộc `MapScene` state).
3. Chốt `event Submitted(sbyte, string)` — chỉ 2 tham số, không thêm.
4. Chốt vị trí file mới sẽ tạo:
   - `Assets/Scripts/Runtime/UI/CharacterCreationView.cs`
   - `Assets/Scripts/Runtime/UI/CharacterPreviewPanel.cs`
   - `Assets/Scripts/Runtime/UI/CharacterNameInput.cs`

## Success Criteria

- [ ] `plans/260910-1836-character-creation-screen/reports/discovery.md` tồn tại với mockup ASCII + asset table.
- [ ] Không có file code nào được tạo/sửa (đây là phase khảo sát).
- [ ] Không có câu hỏi mở về API `AvatarAppearance` (đã confirm chạy được ngoài scene).

## Risk Assessment

- **R1:** `AvatarAppearance` có thể assume parent trong world-space (không phải UGUI). → Verify bằng cách xem `Create` có dùng `SpriteRenderer` (world) hay `Image` (UGUI). Nếu world-only → phase 2 phải dựng trong sub-camera/world panel, hoặc tạo `Image` bao ngoài screenshot.
