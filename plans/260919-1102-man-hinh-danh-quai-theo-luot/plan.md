---
title: "Màn hình đánh quái (PvE) theo lượt"
description: "Sửa vòng lượt/khoá nút, thêm hoạt cảnh lao sang đánh + hàng đợi lượt, server trừ 10% EXP khi thua PvE."
status: done
priority: P2
effort: 9h
branch: master
tags: [unity, gserver, battle, pve, parity-jar]
created: 2026-09-19
---

# Màn hình đánh quái (PvE) theo lượt

Nguồn điều tra (không research lại):
- `plans/reports/investigation-260919-1102-battle-screen-6-yeu-cau.md`
- `plans/reports/investigation-260919-1102-danh-quai-jar-vs-unity.md`

## Phases

| # | Phase | Trạng thái | Effort | Chặn bởi |
|---|-------|-----------|--------|----------|
| 01 | [Vòng lượt đúng + khoá nút theo lượt](phase-01-vong-luot-va-khoa-nut.md) | done | 2.5h | — |
| 02 | [Lao sang đánh + hàng đợi lượt + lerp/float](phase-02-hoat-canh-va-hang-doi-luot.md) | done | 4h | 01 |
| 03 | [Server trừ 10% EXP khi thua PvE](phase-03-server-tru-exp-thua-pve.md) | done | 1h | — (song song 01/02) |
| 04 | [Kiểm thử + cập nhật tài liệu](phase-04-kiem-thu-va-tai-lieu.md) | done | 1.5h | 01, 02, 03 |

## Sở hữu file (không chồng lấn)

| Phase | File sở hữu |
|-------|-------------|
| 01 | `Net/Battle/BattleHandler.cs`, `UiLogic/BattleTurnState.cs` (mới), `Runtime/World/BattleView.cs`, `Runtime/World/BattleView.Actions.cs` (mới), `Runtime/World/Battle/BattleActionBar.cs`, `Runtime/World/Battle/BattleSkillPanel.cs`, `Runtime/World/Battle/BattleVsIndicator.cs`, `tests/Gopet.Net.Tests/BattleTurnStateTests.cs` (mới) |
| 02 | `Runtime/World/Battle/BattleTurnAnimator.cs` (mới), `Runtime/World/BattlePetCard.cs`, `Runtime/World/BattleFloatText.cs`, `Runtime/World/Battle/BattleStatBar.cs`, `Runtime/World/GameSession.cs` (1 lambda), + sửa tiếp `BattleView.cs` |
| 03 | `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs` |
| 04 | `docs/battle-system.md` |

Phase 01 và 02 cùng chạm `BattleView.cs` → **chạy tuần tự**, không song song.
Phase 03 (server) độc lập hoàn toàn → chạy song song được với 01/02.

## Phát hiện then chốt (đã verify lại bằng grep, không lấy từ report)

1. `PetBattle.cs:307-308` / `:1109-1110` — `sendPetAttack()` gửi `petId = getUserTurnId()`
   **rồi mới** `nextTurn()` → `ActorId` = người VỪA ra đòn. Nhãn lượt phải đảo.
2. `PetBattle.cs:1643` — `useItem` gửi gói 37 **không gọi `nextTurn()`** và `turnDatas` rỗng.
   Đảo vô điều kiện sẽ khoá chết nút. Phải nhận diện gói này (`type=WAIT && effectCount=0`).
3. `PetBattle.cs:336-343` — gói 37 loại WAIT **chỉ ghi `mp`, không ghi `hp`** → HP hồi từ
   bình máu KHÔNG đi qua opcode 37. Nó đi qua `MY_PET_INFO` (`GameController.cs:1410-1425`),
   mà client hiện chỉ nối vào `CharacterHud` (`GameSession.cs:250-254`), **không nối vào BattleView**.
   → yêu cầu #4 đang hỏng phần hiển thị, sửa ở phase 02, không cần đổi wire.
4. `Pet.cs:452-462 subExpPK` đã có sẵn (clamp `MIN_PET_EXP_PK = -20000000`) → phase 03 dùng lại.
5. `petBattleTexts` đi qua `putUTF` list của opcode 16 → thêm dòng "bị trừ N exp"
   **không đổi wire format**, jar cũ vẫn đọc được. Không cần gate `VERSION_150`.

## Ràng buộc chung

- Mọi file `.cs` mới/sửa phải **≤ 200 dòng** (`verify.ps1` bước 10/10 chặn).
  Allowlist ngoại lệ ở `GopetUnityClient/CODE_HEALTH_EXCEPTIONS.md` — **không thêm tên mới vào đó**.
- Damage/EXP do server quyết; client chỉ render.
- Không tạo file `*-enhanced`/`*V2`. Sửa thẳng file hiện có.
- Không đổi wire format opcode cũ. Nếu buộc phải, gate `GopetManager.VERSION_150`.
- Sau mỗi phase client: `powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1`
- Sau phase server: `dotnet build SRCGOPETGOC/GServer/Gopet.csproj`
