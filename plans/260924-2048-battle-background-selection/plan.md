---
title: Chọn khung cảnh đánh quái (mua bằng vàng)
description: >-
  Nút bên phải màn Đánh Quái mở popup chọn/mua 1 trong 6 khung cảnh (rừng mặc
  định + 5 khung cảnh vàng có hiệu ứng động)
status: in-progress
priority: P2
branch: fix/performance
tags:
  - battle
  - ui
  - server
  - economy
blockedBy: []
blocks: []
created: '2026-09-24T13:51:05.954Z'
createdBy: 'ck:plan'
source: skill
---

# Chọn khung cảnh đánh quái (mua bằng vàng)

## Overview
Màn Đánh Quái hiện vẽ cố định `Battle/bg-forest` (`BattleView.cs:142`). Thêm nút tròn bên
phải sân; bấm mở popup liệt kê khung cảnh. Mua một lần bằng **vàng** (`PlayerData.gold`) là
sở hữu vĩnh viễn và được chọn ngay. Server giữ quyền sở hữu, trừ vàng và lưu lựa chọn; client
chỉ đổi ảnh nền và chạy hiệu ứng động.

## Bảng khung cảnh và giá đề xuất
Giá neo theo các mức vàng đang có: chat thế giới 200, cường hoá xăm 2.000, giữ ngọc 5.000,
lên bậc pet 10.000, lập bang 20.000. User chốt: mỗi mức +2.000 so với đề xuất đầu.

| id | Khung cảnh | Hiệu ứng | Giá (vàng) |
|---:|---|---|---:|
| 0 | Rừng (mặc định, `bg-forest`) | — | Miễn phí |
| 1 | Rừng cây che | chuồn chuồn + bướm bay | Completed |
| 2 | Hoa anh đào | cánh hoa rơi | Completed |
| 3 | Tuyết trắng | tuyết rơi | Completed |
| 4 | Hang động đá | dơi bám trên đá + dơi bay | Completed |
| 5 | Mưa lửa | lửa cháy + tàn lửa | In Progress |

Giá đặt ở một bảng trên server (`BattleBackgroundCatalog`), client nhận qua gói tin, nên chỉnh
giá không phải phát hành lại client.

## Phases

| Phase | Name | Status |
|-------|------|--------|
| 1 | [Server dữ liệu và gói tin](./phase-01-server-d-li-u-v-g-i-tin.md) | Completed |
| 2 | [Assets nền và hạt hiệu ứng](./phase-02-assets-n-n-v-h-t-hi-u-ng.md) | Completed |
| 3 | [Client hiệu ứng khung cảnh](./phase-03-client-hi-u-ng-khung-c-nh.md) | Completed |
| 4 | [Client nút và popup chọn mua](./phase-04-client-n-t-v-popup-ch-n-mua.md) | Completed |
| 5 | [Tests và docs](./phase-05-tests-v-docs.md) | In Progress |

Thứ tự: phase 1 và phase 2 làm song song được; phase 3 cần phase 2; phase 4 cần phase 1 và 3; phase 5 làm cuối.

## Key decisions
- User chốt (2026-09-24): áp cho **mọi màn đấu** — Đánh Quái, PvP, đấu trường. Mỗi client thấy khung cảnh mình chọn (chỉ là hiển thị phía client).
- Mua xong **tự chọn luôn** khung cảnh vừa mua.
- Rừng mặc định và Rừng cây che là **2 khung cảnh riêng**, không gộp.
- Gói tin đi qua `COMMAND_GUIDER=122`, giống điểm danh (sub 40–42). Sub mới dự kiến 43–46.
- Lưu 2 cột mới trên bảng `player`: `BattleBgOwned` (JSON list int) + `BattleBgSelected` (int).
- Hiệu ứng động vẽ bằng code trên UI canvas, chung một bộ phát hạt cấu hình theo khung cảnh.

## Dependencies
Không chặn/không bị chặn bởi plan nào đang mở.
