---
title: "EXP mỗi đòn trúng + đòn đánh trượt (PvE đánh quái)"
description: "Bật đòn trượt thật bằng GameObject.IsMiss và nhỏ giọt EXP mỗi đòn trúng với số vàng bay trên đầu pet."
status: done
priority: P2
effort: 6h
branch: master
tags: [battle, pve, protocol, unity, gserver]
created: 2026-09-19
---

# Tổng quan

Hai tính năng, một mạch: server quyết định, client chỉ render.

1. **Đòn trượt** — hiện tại KHÔNG BAO GIỜ trượt (`randMiss` `PetBattle.cs:1181` chỉ true
   khi đối phương có buff `MISS_IN_99999_TURN`). Bật `GameObject.IsMiss()` (`GameObject.cs:143`).
   **CHỈ PvE đánh quái** (user chốt 2026-09-19) — PvP/đấu trường giữ nguyên cân bằng cũ, tức
   `rollMiss()` phải guard `petAttackMob`. Kỹ năng trượt → hoàn MP + không cooldown.
2. **EXP mỗi đòn trúng** — cộng 5% EXP giết quái mỗi đòn trúng, trần 30%/trận,
   **THƯỞNG THÊM THẬT** (user chốt 2026-09-19): `win()` KHÔNG trừ lại, pet đánh nhiều đòn thì
   ăn nhiều EXP hơn. Chấp nhận lạm phát tối đa +30% EXP mỗi con quái.
   Opcode mới `PET_BATTLE_EXP` (sub 33 của `PET_SERVICE`), gate 1.5.0. Client hiện số vàng.

## Phát hiện then chốt

| # | Phát hiện | Nguồn |
|---|---|---|
| 1 | `IsMiss` viết sẵn nhưng KHÔNG ai gọi; `HitRate = 100+agi/1000 − (15+agiB/1000)` → 15% trượt (agi<1000 chia nguyên ra 0) | `Base/GameObject.cs:121-146` |
| 2 | `useSkill` trừ MP (`:1059`) + cooldown (`:1060`) TRƯỚC khi roll miss (`:1062`) → phải đảo | `PetBattle.cs` |
| 3 | Đường render trượt của client ĐÃ chạy: `SkillId==1` → `BattleFloatText.CreateMiss()` + `s_attack_miss`; `ResolveName(1)` trả null nên không có sprite hiệu ứng — đúng ý | `BattleView.Actions.cs:132-141`, `BattleEffectView.cs:57-66` |
| 4 | `BattleFloatText` mới có 2 màu (mana xanh dương / hp xanh-đỏ) → thêm kiểu vàng | `BattleFloatText.cs:18-26` |
| 5 | `updatePetLvl()` chỉ lên **1 cấp/lần gọi** và chỉ gửi opcode 18 khi thật sự lên cấp → gọi mỗi đòn là an toàn, rẻ | `GameController.cs:1745-1771` |
| 6 | Client `MarkUsed(skillId)` lạc quan khi bấm → kỹ năng trượt (server không cooldown) sẽ **lệch 3 lượt** nếu không huỷ | `BattleView.Actions.cs:97`, `SkillCooldownTracker.cs:23` |
| 7 | `verify.ps1` bước 10 **đã đỏ sẵn** 16 file ở baseline (gồm `GopetCmd.cs` 204 dòng) — không phải lỗi của plan này | chạy `verify.ps1` 2026-09-19 |
| 8 | Sub 33 của `PET_SERVICE` còn trống cả 2 chiều (đối chiếu `processPet` + `check-protocol-coverage`) | `GameController.cs:978`, tool coverage |

## Phase

| # | Tên | Trạng thái | Chặn bởi | Ước lượng |
|---|---|---|---|---|
| 01 | [Server: bật đòn trượt thật](phase-01-server-don-danh-truot.md) | done | — | 1.5h |
| 02 | [Server: EXP mỗi đòn + opcode mới](phase-02-server-exp-moi-don.md) | done | 01 | 2h |
| 03 | [Client: số EXP vàng + huỷ cooldown khi trượt](phase-03-client-so-exp-vang.md) | done | 02 | 1.5h |
| 04 | [Kiểm thử + tài liệu](phase-04-kiem-thu-va-tai-lieu.md) | done | 03 | 1h |

Phase chạy **tuần tự** — 01 và 02 cùng sửa `PetBattle.cs`, không song song được.

## Sở hữu file

| Phase | File sửa | File tạo |
|---|---|---|
| 01 | `GServer/Data/Battle/PetBattle.cs` | — |
| 02 | `GServer/Data/Battle/PetBattle.cs`, `GServer/Server/GopetCMD.cs` | `GServer/Data/Battle/HitExpReward.cs` |
| 03 | `Net/GopetCmd.cs` (sinh lại), `Net/Battle/BattleHandler.cs`, `Net/Battle/BattleModels.cs`, `UiLogic/SkillCooldownTracker.cs`, `Runtime/World/BattleFloatText.cs`, `BattleView.cs`, `BattleView.State.cs`, `BattleView.Actions.cs` | — |
| 04 | `docs/battle-system.md` | `tests/Gopet.Net.Tests/BattleExpPacketTests.cs` |

## Số đã chốt

- **5% EXP/đòn trúng**: quái chết trong ~3-6 đòn ⇒ mỗi đòn thấy một con số có nghĩa mà không
  nuốt hết phần thưởng cuối trận.
- **Trần 30%/trận**: chặn vòng lặp "đánh 1 đòn → xin thua → đánh lại". Người chơi đánh thật
  gần như không chạm trần.
- **Thưởng thêm thật, KHÔNG ứng trước**: `win()` giữ nguyên, không trừ lại. User đã cân nhắc
  và chấp nhận lạm phát ≤ +30% EXP/quái để "đánh nhiều đòn thì được nhiều hơn" là thật.
  ⇒ Trần 30% trở thành chốt chặn chống cày DUY NHẤT, phải cài đúng.
- **Chỉ PvE**: cả EXP lẫn đòn trượt đều guard `petAttackMob`. PvP/đấu trường không đổi.

## Kiểm chứng sau mỗi phase

```
dotnet msbuild SRCGOPETGOC/GServer/Gopet.csproj -t:Compile
powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1
```
