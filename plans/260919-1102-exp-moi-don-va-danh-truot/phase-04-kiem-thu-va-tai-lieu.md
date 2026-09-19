# Phase 04 — Kiểm thử + tài liệu

## Context Links

- [plan.md](plan.md) · [phase-01](phase-01-server-don-danh-truot.md) ·
  [phase-02](phase-02-server-exp-moi-don.md) · [phase-03](phase-03-client-so-exp-vang.md)
- `docs/battle-system.md` (file duy nhất trong `docs/` cần đụng — `docs/` hiện chỉ có
  `battle-system.md` và `journals/`, **không** có roadmap/changelog để cập nhật)

## Overview

- **Priority:** P2
- **Status:** done
- **Mô tả:** Test đơn vị cho phần thuần C# của client, kịch bản thủ công cho phần server
  (GServer không có test project — **không** dựng mới, YAGNI), và cập nhật tài liệu giao thức.

## Key Insights

1. `tests/Gopet.Net.Tests` compile thẳng `Assets/Scripts/Net/**` + `Assets/Scripts/UiLogic/**`
   (`Gopet.Net.Tests.csproj:19-21`) ⇒ test được `BattleHandler` và `SkillCooldownTracker`,
   **không** test được `BattleFloatText`/`BattleView` (MonoBehaviour, tầng Runtime).
2. `BattleHandlerTests.cs` đã 270 dòng (nằm trong danh sách vi phạm 200 dòng ở baseline)
   ⇒ test gói mới đi vào **file mới** `BattleExpPacketTests.cs`, không làm file cũ phình thêm.
3. `SkillCooldownTrackerTests.cs` 78 dòng — còn dư, thêm test `CancelLastUsed` tại chỗ.
4. Không có test project ở `SRCGOPETGOC/GServer`. Phần server kiểm bằng compile + kịch bản chạy tay
   có tiêu chí số học kiểm được (tổng EXP bất biến).

## Test Matrix

| Tầng | Đối tượng | Cách kiểm | Nơi |
|---|---|---|---|
| Unit | Parse 81/33 (`OnHitExp`) | xUnit | `BattleExpPacketTests.cs` (mới) |
| Unit | Payload sai độ dài → `ProtocolException` | xUnit | `BattleExpPacketTests.cs` |
| Unit | `SkillCooldownTracker.CancelLastUsed` | xUnit | `SkillCooldownTrackerTests.cs` |
| Unit | `CancelLastUsed` no-op khi chưa bấm kỹ năng nào | xUnit | `SkillCooldownTrackerTests.cs` |
| Tích hợp | route `81/33` = `handled` | `verify.ps1` bước 1 | tool coverage |
| Tích hợp | client compile (Net/Runtime/Editor/PlayMode) | `verify.ps1` bước 4,6,7,8 | — |
| Tích hợp | server compile | `dotnet msbuild ... -t:Compile` | — |
| E2E tay | 20 đòn thường → ~15% trượt | đếm trên màn | client 1.5.0 |
| E2E tay | kỹ năng trượt: MP không giảm, không cooldown | HUD + nút | client 1.5.0 |
| E2E tay | **Σ EXP nhỏ giọt + EXP panel kết quả = EXP cũ** | log server | so với build trước |
| E2E tay | đánh-rồi-xin-thua: EXP ≤ 30% EXP giết quái, dừng hẳn khi chạm trần | log server | — |
| E2E tay | boss / PvP / đấu trường: không có gói 81/33 | log server | — |
| E2E tay | jar 1.4.x vào trận bình thường (không nhận 81/33) | client.jar | `SRCGOPETGOC/client.jar` |

## Implementation Steps

1. Tạo `tests/Gopet.Net.Tests/BattleExpPacketTests.cs` (< 80 dòng), theo mẫu
   `BattleHandlerTests.NewHandler`/`Dispatch`:
   ```csharp
   [Fact] public void HitExp_DocDungBaTruong()      // battleId/actorId/exp
   [Fact] public void HitExp_ThuaByte_ThiNem()      // ExpectFullyConsumed
   ```
   Nếu `NewHandler`/`Dispatch` là `private` trong `BattleHandlerTests`, dựng lại tại chỗ
   (3-4 dòng: `new MessageRouter()`, `handler.RegisterOn(router)`) — **không** đổi file cũ
   thành `public` chỉ để dùng chung.

2. Bổ sung vào `SkillCooldownTrackerTests.cs`:
   - bấm skill → `IsReady(id) == false` → `CancelLastUsed()` → `IsReady(id) == true`;
   - `CancelLastUsed()` khi chưa bấm gì → không ném, không đổi gì;
   - bấm skill A, qua đủ lượt cho hết cooldown, rồi `CancelLastUsed()` → không ảnh hưởng skill khác.

3. Chạy:
   ```
   dotnet msbuild SRCGOPETGOC/GServer/Gopet.csproj -t:Compile
   powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1
   ```
   Bước 10 phải giữ **đúng danh sách 16 tên cũ** (baseline 2026-09-19):
   `GopetCmd.cs, ImageHandler.cs, RemoteAssetCache.cs, AtmPopupView.cs, BacSiNpcTabsView.cs,
   CharacterHubPopupView.Chrome.cs, CharacterHubPopupView.Content.cs, CharacterHubPopupView.cs,
   DailyCheckinView.cs, HeavenNpcTabsView.cs, InputDialogView.cs, MenuItemRow.cs, PetGridView.cs,
   TranChanTabsView.cs, GameHud.Chat.cs, BattleHandlerTests.cs`.
   Có tên mới = phase chưa xong.

4. Cập nhật `docs/battle-system.md`:
   - §2 gạch đầu dòng damage: sửa "Miss qua `randMiss()`" → mô tả `rollMiss()` =
     `randMiss()` (buff) ∪ `GameObject.IsMiss()` (15% base theo agi), áp cho cả 4 đường ra đòn,
     mọi loại trận.
   - §2 mới (2.8): "Kỹ năng trượt không mất gì" — roll trước khi trừ MP/cooldown, gói trượt của
     kỹ năng cố tình dùng byte-shape của đòn thường trượt và **không** mang `skillId` để client
     huỷ được cooldown lạc quan.
   - §4 bảng opcode: thêm dòng `33 | PET_BATTLE_EXP | S→C (gate 1.5.0) | EXP nhỏ giọt mỗi đòn trúng PvE`.
   - Mục mới §11.1 "EXP nhỏ giọt mỗi đòn trúng": 5%/đòn, trần 30%/trận, **ứng trước** phần thưởng
     giết quái (`win()` trừ lại ⇒ tổng bất biến), chỉ PvE quái thường, lý do không áp PvP
     (hai người hẹn nhau đánh qua lại để cày), và các lớp chống cày đã có
     (`GopetPlace.cs:406`, `setLastTimeKillMob` `PetBattle.cs:888`).

5. Nếu phát sinh bài học đáng ghi (ví dụ sai lệch cooldown client/server), thêm một mục vào
   `docs/journals/`.

## Todo List

- [x] `BattleExpPacketTests.cs` (2 test)
- [x] 3 test `CancelLastUsed` trong `SkillCooldownTrackerTests.cs`
- [x] `verify.ps1` — bước 1-9 xanh, bước 10 đúng 16 tên cũ
- [x] `dotnet msbuild ... -t:Compile` xanh
- [x] E2E tay: 6 kịch bản trong bảng test matrix
- [x] `docs/battle-system.md`: §2, §2.8, §4, §11.1

## Success Criteria

- Toàn bộ test trong `tests/Gopet.Net.Tests` xanh (bước 5 của `verify.ps1`).
- `docs/battle-system.md` mô tả đúng hành vi mới; không còn câu "Miss qua `randMiss()`" gây hiểu
  nhầm là đã có trượt từ trước.
- Chạy tay xác nhận **tổng EXP một con quái không đổi** so với build trước — đây là tiêu chí chặn
  quan trọng nhất của cả plan.

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| Test mới làm file vượt 200 dòng | Thấp | Thấp | File mới, ước 80 dòng; `SkillCooldownTrackerTests.cs` 78 + ~25 |
| Không đo được "tổng EXP bất biến" vì thiếu log | Trung bình | Cao | Thêm log tạm `Console.WriteLine` trong `awardHitExp`/`win()` khi kiểm, gỡ trước khi commit |
| Doc lệch với code sau khi tinh chỉnh số 5%/30% | Trung bình | Thấp | Doc dẫn hằng số `HitExpReward.PerHitPercent`/`BattleCapPercent` thay vì chép số |

## Security Considerations

- Không commit log kiểm thử có user_id/dữ liệu người chơi thật.
- Không nới lỏng `ExpectFullyConsumed` để test dễ chạy.

## Next Steps

- `code-reviewer` sau khi 4 phase xong.
- Nếu muốn đổi sang mô hình "EXP thưởng thêm" thay vì ứng trước: gỡ **duy nhất** bước 6 của
  phase 02 và cân nhắc lại trần 30% — cần quyết định cân bằng, không phải quyết định kỹ thuật.

## Rollback

Phase 04 chỉ thêm test + doc ⇒ rollback là `git checkout --` các file đó, không ảnh hưởng runtime.
