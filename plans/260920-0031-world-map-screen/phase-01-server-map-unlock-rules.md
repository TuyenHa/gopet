# Phase 01 — Server: khoá map theo nhiệm vụ + TELE_MENU gửi lý do

## Context Links

- [plan.md](plan.md)
- `SRCGOPETGOC/GServer/Server/GameController.cs:1537` `mapTeleMenu()`
- `SRCGOPETGOC/GServer/Server/GameController.cs:5111` `IsSkyLocked` / `:5113` `CheckSky`
- `SRCGOPETGOC/GServer/Server/GameController.cs:306` `ON_PLAYER_WARPING`
- `SRCGOPETGOC/GServer/Server/GameController.cs:1579` `changeChannel`
- `SRCGOPETGOC/GServer/Data/User/PlayerData.cs:27` `wasTask`
- `SRCGOPETGOC/GServer/Server/TaskCalculator.cs:488` nơi task hoàn thành được đẩy vào `wasTask`
- `SRCGOPETGOC/GServer/Language/LanguageData.cs:231` `LawToUnlockSkyPlace`

## Overview

- **Priority:** P1 (chặn phase 02 + 05)
- **Status:** pending
- Thêm luật khoá map theo tiến độ nhiệm vụ, enforce ở CẢ menu lẫn đường warp, và mở
  rộng TELE_MENU để gửi kèm chuỗi lý do khoá cho client hiển thị.

## Key Insights

- `mapTeleMenu()` hiện chỉ gửi cờ khoá lấy từ `IsSkyLocked(j)` (`GameController.cs:1562`),
  không có lý do — client đang hardcode câu "Cần pet trùng sinh để lên thượng giới."
  (`GameSession.Teleport.cs:33`). Gửi lý do từ server là cách duy nhất để một câu chữ
  không bị nhân đôi ở hai nơi.
- Đường warp chỉ chặn bằng `CheckSky` ở đúng 2 chỗ: `GameController.cs:306`
  (`ON_PLAYER_WARPING`) và `GameController.cs:1579` (`changeChannel`). Nếu luật mới chỉ
  nằm ở menu thì client tự chế gói `ON_PLAYER_WARPING` vẫn vào được map khoá.
- `player.playerData.wasTask` là `CopyOnWriteArrayList<int>` chứa `taskTemplateId` đã
  hoàn thành (`TaskCalculator.cs:488` là nơi ghi vào) → kiểm tra mở map = `Contains(taskId)`.
- `GopetMap` lấy từ `MapManager.maps`, khoá là `int` mapId; menu sort mapId để thứ tự ổn định.

## Requirements

**Chức năng**
- Một nơi duy nhất khai báo `mapId → taskTemplateId cần hoàn thành`.
- `MapLockReason(mapId)` phủ cả luật thượng giới cũ lẫn luật nhiệm vụ mới.
- TELE_MENU gửi thêm một `putUTF` lý do khoá ngay sau cờ khoá; map mở khoá gửi chuỗi rỗng.
- Đường warp và đổi kênh từ chối map khoá bằng `redDialog` với đúng lý do đó.

**Phi chức năng**
- Không thêm truy vấn DB trong vòng lặp TELE_MENU (dữ liệu quest đã nằm trong `playerData`).
- Giữ nguyên hành vi hiện tại khi bảng luật rỗng (không regression cho người chơi đang chơi).

## Architecture

```
ON_PLAYER_WARPING ─┐
changeChannel     ─┼─> GameController.CheckMapAccess(mapId)
mapTeleMenu       ─┘        │
                            ├─> IsSkyLocked(mapId)            (luật cũ, giữ nguyên)
                            └─> MapUnlockRules.TryGetRequiredTask(mapId)
                                      └─> playerData.wasTask.Contains(taskId)?
                            => reason: string  ("" = mở khoá)
```

`MapLockReason(mapId)` trả `string.Empty` khi mở. `CheckMapAccess` = reason rỗng ? true :
(`redDialog(reason)`, false). `mapTeleMenu` dùng chính `MapLockReason` → menu và đường
warp KHÔNG THỂ lệch nhau (đúng tinh thần comment sẵn có ở `GameController.cs:5106`).

**Đánh đổi nơi khai báo bảng luật** — chọn *static class trong code*
(`Manager/MapUnlockRules.cs`) thay vì thêm cột vào `map_template`:

| | Static class | Cột DB |
|---|---|---|
| Sửa luật | phải build lại server | sửa nóng bằng SQL |
| Chi phí ban đầu | ~50 dòng | migration + loader + rollback DB |
| Rủi ro | thấp | đụng schema đang chạy |

YAGNI: luật này đổi vài lần mỗi năm, không phải mỗi ngày → chọn static class.

## Related Code Files

**Tạo mới**
- `SRCGOPETGOC/GServer/Manager/MapUnlockRules.cs` (~50 dòng)

**Sửa**
- `SRCGOPETGOC/GServer/Server/GameController.cs` — `mapTeleMenu()` (~1537), khối
  `IsSkyLocked`/`CheckSky` (~5106-5122), call site `:306`, call site `:1579`
- `SRCGOPETGOC/GServer/Language/LanguageData.cs` — thêm `TaskLockedMapHint` cạnh dòng 231

**Xoá:** không có file; chỉ xoá method `CheckSky` sau khi hết tham chiếu.

## Implementation Steps

1. Tạo `Manager/MapUnlockRules.cs`: static class,
   `private static readonly Dictionary<int,int> Required = new() { };` (RỖNG — xem câu
   hỏi #1 ở plan.md), hàm `public static bool TryGetRequiredTask(int mapId, out int taskId)`.
   Comment tiếng Việt: vì sao để trong code chứ không trong DB, và cách điền một dòng mới.
2. `LanguageData.cs`: thêm ngay cạnh `LawToUnlockSkyPlace` (`:231`):
   `public string TaskLockedMapHint { get; set; } = "Hãy chăm chỉ làm nhiệm vụ để mở map này";`
   Câu ngắn, một dòng — toast client hiển thị nguyên văn.
3. `GameController.cs`: thêm `public string MapLockReason(int mapId)` ngay dưới `IsSkyLocked`:
   - `IsSkyLocked(mapId)` → trả `player.Language.LawToUnlockSkyPlace`
   - `MapUnlockRules.TryGetRequiredTask(mapId, out var taskId)` và
     `!player.playerData.wasTask.Contains(taskId)` → trả `player.Language.TaskLockedMapHint`
   - còn lại → `string.Empty`
4. Thêm `public bool CheckMapAccess(int mapId)`: lấy reason; rỗng → true; khác rỗng →
   `player.redDialog(reason)` rồi false. Giữ `IsSkyLocked` (còn dùng trong `MapLockReason`).
5. Đổi 2 call site `CheckSky` → `CheckMapAccess`: `GameController.cs:306` và `:1579`.
   Xoá `CheckSky` sau khi grep xác nhận hết tham chiếu.
6. `mapTeleMenu()` (~1560): thay `ms.putsbyte((sbyte)(IsSkyLocked(j) ? 1 : 0));` bằng
   ```csharp
   string reason = MapLockReason(j);
   ms.putsbyte((sbyte)(reason.Length > 0 ? 1 : 0));
   ms.putUTF(reason);
   ```
   Cập nhật khối comment `:1537-1546` cho khớp luật mới (không còn "dùng CHUNG IsSkyLocked").
7. Build: `dotnet build SRCGOPETGOC/GServer/Gopet.csproj -c Debug --nologo`.

## Todo List

- [ ] 1. `MapUnlockRules.cs` (bảng rỗng + API)
- [ ] 2. `LanguageData.TaskLockedMapHint`
- [ ] 3. `MapLockReason(int)`
- [ ] 4. `CheckMapAccess(int)`
- [ ] 5. Đổi call site `:306`, `:1579`; bỏ `CheckSky`
- [ ] 6. `mapTeleMenu` gửi thêm `putUTF(reason)` + sửa comment
- [ ] 7. Build server sạch

## Success Criteria

- `dotnet build SRCGOPETGOC/GServer/Gopet.csproj -c Debug` → 0 error, không thêm warning.
- `grep -rn "CheckSky(" SRCGOPETGOC/GServer --include=*.cs` → không còn kết quả.
- Smoke phase 02 chạy xanh: mỗi entry TELE_MENU có đúng 1 chuỗi lý do; map < 26 → rỗng.
- Kiểm tay: thêm tạm 1 entry vào `MapUnlockRules` (map 21 ↔ taskId có thật) → client thấy
  ổ khoá + câu "Hãy chăm chỉ làm nhiệm vụ để mở map này"; gửi `ON_PLAYER_WARPING` tay vào
  map đó → server trả `redDialog`, không đổi map. Gỡ entry sau khi kiểm.

## Risk Assessment

| Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|--------|----------|-----------|-----------|
| Client cũ + server mới lệch 1 chuỗi → treo gói TELE_MENU | Cao nếu deploy sai thứ tự | Cao | Deploy server TRƯỚC, client sau (Next Steps) |
| Điền sai taskId → khoá nhầm map đang chơi | Trung bình | Cao | Bảng rỗng khi merge; điền sau sign-off, kiểm trên tài khoản test |
| `wasTask` chưa nạp lúc login sớm | Thấp | Trung bình | `MapLockReason` chỉ chạy theo request của player đã vào game |

**Rollback:** revert 1 commit; wire-format quay lại 5 trường. Client mới sẽ ném ở
`ExpectFullyConsumed` → phải revert client cùng lúc (xem phase 02).

## Security Considerations

- **Trọng tâm:** enforce ở `ON_PLAYER_WARPING` (`:306`) — chỉ khoá ở menu thì client giả
  mạo vẫn vào được map cấm. Bước 5 bắt buộc, không phải tuỳ chọn.
- Lý do khoá là chuỗi hằng từ `LanguageData`, không nội suy dữ liệu người chơi → không có
  đường tiêm nội dung vào `putUTF`.
- Giữ `HistoryManager.addHistory` trong `mapTeleMenu` để truy vết.

## Next Steps

- Phase 02 (client parse) chỉ cần wire-format ở bước 6, không cần server chạy.
- **Thứ tự deploy bắt buộc:** server (phase 01) trước, client (phase 02+) sau.
