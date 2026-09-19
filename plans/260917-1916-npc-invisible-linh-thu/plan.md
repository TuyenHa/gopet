---
title: "Fix: NPC ở Linh Thú thành đôi khi chỉ hiện tên"
description: >-
  Sau 3 lần timeout xin ảnh, ImageHandler drop waiter callback → NPC đọng ở
  placeholder 1×1 gray → user chỉ thấy tên. Fix chuỗi: notify waiter khi hết
  retry, xử lý PNG hỏng, thêm fallback texture visible, tăng retry/timeout.
status: in_progress
priority: P2
branch: master
tags: [client, image, npc, resilience]
created: "2026-09-17T12:16:06+07:00"
createdBy: "ck:fix"
source: skill
---

# Fix NPC invisible Linh Thu

## Root cause chain

1. `ImageHandler.Tick` hết retry → `_pending.Remove` xoá Waiters → waiter không chạy.
2. `RemoteAssetCache.Materialize` PNG hỏng → return sớm không gọi onReady.
3. Placeholder = 1×1 gray transparent → không phân biệt "loading" vs "failed".

## Phases

| # | Phase | Status |
|---|---|---|
| 1 | Pipeline: notify + fallback texture | in_progress |
| 2 | Tests + build | pending |

## Kết quả (2026-09-17)

- Diff: 3 file production + 2 test (`ImageHandler.cs`, `RemoteAssetCache.cs`, `TextureFactory.cs`, `ImageHandlerLifecycleTests.cs`, `ImageHandlerTests.cs`).
- Build Runtime: 0 error, 0 warning.
- Test: **724/724 pass** (+1 regression mới, 1 test cũ update hợp đồng).
- Status: **completed**.
