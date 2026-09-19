---
title: "Màn hình chiến đấu Pet vs Quái theo mockup"
description: >-
  Bấm vào quái → chuyển sang màn hình chiến đấu toàn màn hình giống hệt mockup
  (HUD HP/MP/ATK/DEF/chí mạng, panel kỹ năng 2 bên, nút kiếm, Xin thua). Kỹ năng
  của pet và của quái lấy từ DB; icon/nền sinh bằng tools/image-gen.
status: in-progress
priority: P2
branch: "master"
tags: [unity, server, battle, ui, pve, image-gen]
blockedBy: []
blocks: []
related: [260917-1812-pvp-arena-battle-parity]
created: "2026-09-17T15:43:53.291Z"
createdBy: "ck:plan"
source: skill
---

# Màn hình chiến đấu Pet vs Quái theo mockup

Mockup: [visuals/battle-screen-mockup.png](./visuals/battle-screen-mockup.png)

## Overview

Luồng bấm quái → `ATTACK_MOB` → `BattleView` **đã chạy được** (`GameSession.cs:142`,
`BattleCoordinator`). Việc cần làm là thay overlay mờ hiện tại bằng màn hình đầy đủ như
mockup, và bổ sung dữ liệu mà server chưa gửi.

### Gap giữa mockup và dữ liệu thật (đã kiểm tra 2026-09-17)

| Mockup cần | Hiện trạng | Xử lý |
|---|---|---|
| `Lv.N` đúng | `writeMyPetInfo`/`writeMobInfo` gửi cứng `1` (`PetBattle.cs:536,567`) | Gói mới (phase 2), không đụng gói cũ |
| ATK / DEF / % chí mạng | Không gửi. Crit chỉ là phép random trong `isCrit()` (`GameObject.cs:148`) | Tách `CritPercent`, gửi trong gói mới |
| Skill quái + MP | Quái không có skill (`writeMobInfo` gửi `0`); `mobAttack()` chỉ đánh thường, code chọn skill bị comment (`PetBattle.cs:1401`) | Bảng `gopet_mob_skill` + bật AI (phase 1) |
| MP skill đối thủ | Gói passive chỉ có `id + name` | Gói mới mang `mpCost` |
| Icon skill | Bảng `skill` (30 dòng) không có cột icon; `pet/battle/skills/*` là sprite hiệu ứng, không phải icon | Sinh icon theo `skillID` (phase 3) |
| Xin thua | Server không có sub-command | `PET_BATTLE_SURRENDER` (phase 2) |
| Vàng / Ngọc / Lựa | Có sẵn `PlayerStats.Gold/Coin/Lua` | Tái dùng |

### Quyết định đã chốt với user

- Quái **có skill thật** lấy từ DB, dùng trong trận (không chỉ hiển thị).
- Màn hình mới dùng cho **cả PvE và PvP** (một `BattleView`); tiêu đề đổi theo loại:
  `ĐÁNH QUÁI` / `ĐẤU TRƯỜNG PET`.
- **Giữ** nút Vật phẩm (nút tròn nhỏ cạnh nút kiếm).
- **Làm thật** "Xin thua".

### Nguyên tắc

- Không đổi wire format opcode cũ. Opcode/sub mới gate `GopetManager.VERSION_150`
  giống `PET_BATTLE_BUFF` (`PetBattle.cs:1743`). Jar cũ không bị ảnh hưởng.
- Damage do server quyết định; client chỉ render.
- File code < 200 dòng — `BattleView.cs` (267) phải tách.

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Server: skill cho quái từ DB](./phase-01-server-skill-cho-qu-i-t-db.md) | Done |
| 2 | [Server: gói chỉ số trận & Xin thua](./phase-02-server-g-i-ch-s-tr-n-xin-thua.md) | Done |
| 3 | [Assets: sinh icon/nền bằng image-gen](./phase-03-assets-sinh-icon-n-n-b-ng-image-gen.md) | Done |
| 4 | [Client net: parse chỉ số & gửi Xin thua](./phase-04-client-net-parse-ch-s-g-i-xin-thua.md) | Done |
| 5 | [Client UI: dựng lại BattleView theo mockup](./phase-05-client-ui-d-ng-l-i-battleview-theo-mockup.md) | Done |
| 6 | [Tests & docs](./phase-06-tests-docs.md) | In Progress |

```
01 ──► 02 ──► 04 ──► 05 ──► 06
03 ─────────────────►┘
```
Phase 3 (assets) độc lập, chạy song song với 1/2/4.

## Dependencies

- **related** `260917-1812-pvp-arena-battle-parity`: phase 03 (spectator) và 06 (vượt ải)
  của plan đó dùng chung `BattleCoordinator`/`BattleView`. Không chặn nhau, nhưng ai làm
  sau phải rebase lên cấu trúc file mới của phase 5 ở đây.
- Môi trường: server `SRCGOPETGOC/GServer` (.NET 8, MariaDB Docker `gopet-mariadb`,
  cần `GOPET_DB_PASSWORD`); client `GopetUnityClient` (`verify.ps1`,
  `run-playmode-tests.ps1`); `tools/image-gen` (OpenAI key trong `.env`).
