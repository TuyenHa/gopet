---
phase: 2
title: "Character Preview Panel"
status: complete
priority: P1
effort: "1d"
dependencies: [1]
---

# Phase 2: Character Preview Panel

## Overview

Dựng panel hiển thị 2 avatar (nam/nữ), có highlight cho lựa chọn hiện tại và event `GenderChanged`. Là component tái sử dụng độc lập.

## Requirements

- **Functional:**
  - Hiện 2 avatar side-by-side, cùng scale.
  - Tap 1 avatar → chọn giới tính đó, highlight (viền vàng hoặc scale +10%).
  - Có state `SelectedGender` (0=nam, 1=nữ), default nam.
  - Event `GenderChanged(sbyte)` bắn mỗi lần đổi.
- **Non-functional:**
  - Dùng `AvatarAppearance.Create` — không sinh sprite mới.
  - Layout responsive: chia đôi width panel, chiều cao đủ avatar full-body.

## Architecture

```
CharacterPreviewPanel (GameObject có RectTransform + Canvas child cho highlight)
├─ Slot 0 (Nam)  ─ AvatarAppearance instance + BoxCollider2D + IPointerClickHandler
├─ Slot 1 (Nữ)   ─ AvatarAppearance instance + BoxCollider2D + IPointerClickHandler
└─ HighlightFrame (Image, di chuyển sang slot đang chọn)
```

**AvatarAppearance embedding:** `AvatarAppearance` render bằng `SpriteRenderer` (world-space). Panel là UGUI → cần 1 trong 2 cách:
1. Đặt panel ở **world position** riêng (ngoài Canvas), camera phụ chụp lên.
2. Dùng `SortingGroup` + child game object thuần world.

**Chọn cách 2** — đơn giản hơn: panel tự tạo 2 sub-transform world, không nằm dưới Canvas UGUI của LoginScreens. Root có `Canvas` overlay riêng cho highlight/label chữ.

## Related Code Files

- **Create:**
  - `Assets/Scripts/Runtime/UI/CharacterPreviewPanel.cs` (~140 dòng)
- **Read:**
  - `Assets/Scripts/Runtime/World/AvatarAppearance.cs` — API render.
  - `Assets/Scripts/Runtime/UI/UiBuilder.cs` — colors, font.
  - `Assets/Scripts/Runtime/World/JarNameLabel.cs` — label "Nam"/"Nữ" bằng bitmap font.

## Implementation Steps

1. Tạo class `CharacterPreviewPanel : MonoBehaviour`.
2. `static Create(Transform parent, Vector2 size)` — dựng root GameObject với 2 slot con.
3. Trong mỗi slot: `AvatarAppearance.Create(slot.transform, gender)` + `BoxCollider2D` (48×64) + tap handler.
4. Tap trên slot i → `SelectedGender = i`, move highlight frame sang slot đó, invoke `GenderChanged`.
5. Nhãn "Nam"/"Nữ" dưới avatar bằng `JarNameLabel` (đồng bộ style jar).
6. Highlight frame: `Image` bo góc, viền vàng `#FFCC33`, animate scale 1.0→1.08 pulse (tùy chọn — không bắt buộc).
7. Public API:
   ```csharp
   public sbyte SelectedGender { get; private set; }
   public event Action<sbyte> GenderChanged;
   public static CharacterPreviewPanel Create(Transform parent, Vector2 size);
   ```

## Success Criteria

- [ ] `CharacterPreviewPanel.cs` < 200 dòng.
- [ ] Compile OK ở `Gopet.Runtime.UnityCompat`.
- [ ] PlayModeTest ngắn (Phase 5 chi tiết): tap slot 1 → `SelectedGender == 1`, event fire đúng.
- [ ] Avatar render đúng gender (khác biệt sprite giữa slot 0 và 1).

## Risk Assessment

- **R1: `AvatarAppearance` cần asset load.** Nếu chưa nạp texture, sprite blank. → Fallback: `RemoteAssetCache` đã có placeholder; `AvatarAppearance` internal đã handle. Verify trong PlayModeTest.
- **R2: Physics2DRaycaster** — camera phụ hoặc main camera phải có nó (đã có ở `GameSession.cs:107` cho map, nhưng login scene có thể chưa). → Phase 4 thêm nếu thiếu.
