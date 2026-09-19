---
phase: 1
title: 'Sửa GopetPlace.cs:438'
status: completed
priority: P3
effort: 10min
dependencies: []
---

# Phase 1 — Sửa GopetPlace.cs:438

## Overview

Đổi `player.playerData.star--` thành `player.MineStar(1)` để tự đẩy STAR_INFO.

## Related Code Files

- Modify: `SRCGOPETGOC/GServer/Place/GopetPlace.cs` (~ dòng 436-439)
- Read for context: `SRCGOPETGOC/GServer/Server/Player.cs:870-873` (MineStar)

## Implementation Steps

1. Đọc block hiện tại để chắc kiểm điều kiện `star - 1 >= 0` vẫn ổn.
2. `MineStar(1)` đã chứa cùng check gián tiếp qua `player.playerData.star -= n` +
   `updateUserInfo`. Kiểm nội bộ `MineStar` xem có clamp âm không — nếu không,
   giữ check `if (star - 1 >= 0)` bên ngoài.
3. Đổi 1 dòng:
   ```csharp
   -    player.playerData.star--;
   +    player.MineStar(1);
   ```

## Success Criteria

- [x] Diff chỉ 1 dòng thay đổi.
- [x] `star - 1 >= 0` guard vẫn còn.
- [x] Không đụng nhánh boss OwnerClan / task calculator.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| MineStar có side-effect khác (log, achievement) | Thấp | Đọc Player.cs:870 xác nhận chỉ gồm subtraction + updateUserInfo |
| Spam STAR_INFO nếu boss có nhiều lượt tap liên tiếp | Thấp | Không — client chỉ đánh 1 lần khi mở trận |
