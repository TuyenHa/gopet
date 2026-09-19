---
phase: 2
title: Build & xác minh
status: completed
priority: P3
effort: 10min
dependencies:
  - 1
---

# Phase 2 — Build & xác minh

## Implementation Steps

1. `dotnet build SRCGOPETGOC/GServer/Gopet.csproj -c Debug -o <temp>` — chờ 0 error.
2. Ghi nhận cảnh báo mới (nên = 0).

## Success Criteria

- [x] Build 0 error.
- [x] Không phát sinh warning mới (còn 3 warning dependency/RID có sẵn: NU1902 x2, NETSDK1206 x1).
- [x] `grep -n "player.playerData.star--" Place/GopetPlace.cs` không còn kết quả.
