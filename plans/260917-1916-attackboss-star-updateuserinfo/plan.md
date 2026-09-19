---
title: 'Fix: attack boss quên updateUserInfo sau khi trừ sao'
description: >-
  GopetPlace.cs:438 trừ sao trực tiếp qua star-- mà không gọi updateUserInfo(),
  dẫn tới HUD sao stale sau khi tap boss. Fix một dòng, dùng MineStar(1) như các
  call site khác. Phát hiện trong audit boss parity 2026-09-17.
status: completed
priority: P3
branch: master
tags:
  - server
  - hud
  - boss
  - one-liner
blockedBy: []
blocks: []
created: '2026-09-17T12:16:06+07:00'
createdBy: 'ck:plan'
source: skill
---

# Fix attack boss star update

## Overview

Bug audit trong plan 260917-1812 (`reports/audit-260917-1916-boss-hunting-parity.md`).

`Place/GopetPlace.cs:436-439`:

```csharp
if (player.playerData.star - 1 >= 0) {
    player.playerData.star--;
    player.controller.getTaskCalculator().onAttackBoss((Boss)mob);
}
```

Không gọi `updateUserInfo()` → gói `STAR_INFO`(94) không tới client → thanh sao
HUD hiển thị số cũ cho tới lần update kế tiếp (login/warp/level up...).

Mọi chỗ trừ sao khác đều dùng `player.MineStar(n)` (`Server/Player.cs:870`) tự
gọi `updateUserInfo()`. Chỉ ca này bị bỏ qua.

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Sửa GopetPlace.cs:438](./phase-01-fix-attackboss-star.md) | Completed |
| 2 | [Build & xác minh](./phase-02-build-verify.md) | Completed |

## Dependencies

Không.
