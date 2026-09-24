---
title: Độ bền trang bị pet và sửa bằng Đá mài sửa chữa
description: >-
  Trang bị pet (nón, kiếm, giày, bao tay, giáp) mòn sau mỗi trận, hỏng thì mất
  chỉ số, sửa ở Thợ rèn (Thành phố Linh Thú) bằng Đá mài sửa chữa
status: in-progress
priority: P2
branch: fix/performance
tags:
  - server
  - items
  - economy
  - npc
blockedBy: []
blocks: []
created: '2026-09-24T15:21:31.248Z'
createdBy: 'ck:plan'
source: skill
---

# Độ bền trang bị pet và sửa bằng Đá mài sửa chữa

## Overview
Tính năng MỚI (jar gốc và server hiện tại đều không có độ bền). Mỗi món trang bị pet có
độ bền; sau mỗi trận các món pet đang mặc bị trừ độ bền; về 0 thì **hỏng**: không cộng chỉ
số riêng của món (bonus set vẫn giữ) tới khi sửa. Sửa ở NPC **Thợ rèn** đặt tại Thành phố Linh Thú (map 11),
mỗi lần sửa tiêu 1 **Đá mài sửa chữa**. Đá mài rơi khi đánh quái, là thưởng boss và quà điểm danh,
nằm trong túi đồ (NORMAL_INVENTORY).

Gần như toàn bộ là server: chữ độ bền do server dựng vào tên/mô tả item, NPC mới dùng hộp
chọn chung của client → client chỉ cần kiểm tra hiển thị + gợi ý NPC.

## Thông số (user chốt 2026-09-24; hằng số, chỉnh không cần sửa logic)
| Mục | Giá trị |
|---|---|
| Độ bền tối đa | 80 cho mọi món |
| Thắng quái / thắng PvP, đấu trường | −1 mỗi món đang mặc |
| Thua (kể cả xin thua) | −2 mỗi món |
| Bỏ trận (đổi map, rớt mạng — `Close()`) | không trừ |
| Cảnh báo | khi một món xuống ≤ 8 (10%) và khi hỏng |
| Món hỏng | chỉ mất chỉ số riêng của món; **bonus set vẫn giữ** |
| Sửa | 1 Đá mài = sửa ĐẦY 1 món |
| Đá mài từ quái thường | 5% mỗi trận thắng, tung riêng (không chen bảng `drop_item`), **khoá giao dịch** |
| Đá mài từ boss | 5 Đá mài cho người kết liễu |
| Đá mài từ điểm danh | ngày 3, 10, 17, 24: 1 Đá mài; ngày 28: 2 Đá mài |

Ước tính: quái hồi 3s ⇒ ~150–200 trận/giờ ⇒ mỗi món hỏng sau ~25–30 phút cày liên tục; 5 món
cần ~9–12 Đá mài/giờ, 5% rơi cho ~7–10 Đá mài/giờ ⇒ cày liên tục hụt ~20%; bù bằng boss/điểm
danh là phần thêm, dư thì bán trên chợ (Đá mài giao dịch được).

## Dụng cụ sửa
**Đá mài sửa chữa** (user đổi tên từ "Búa sửa chữa") — vật phẩm xếp chồng, **giao dịch được**,
1 viên sửa đầy 1 món. Icon sinh bằng `tools/image-gen`.

## Phases
| Phase | Name | Status |
|-------|------|--------|
| 1 | [Server độ bền và hao mòn sau trận](./phase-01-server-b-n-v-hao-m-n-sau-tr-n.md) | Completed |
| 2 | [Server Đá mài sửa chữa và nguồn rơi](./phase-02-server-b-a-s-a-ch-a-v-ngu-n-r-i.md) | Completed |
| 3 | [Server NPC Thợ rèn và menu sửa](./phase-03-server-npc-th-r-n-v-menu-s-a.md) | Completed |
| 4 | [Client hiển thị và cảnh báo](./phase-04-client-hi-n-th-v-c-nh-b-o.md) | Completed |
| 5 | [Tests và docs](./phase-05-tests-v-docs.md) | In Progress |

Thứ tự: 1 → 2 → 3 → 4 → 5 (2 cần id item Đá mài; 3 cần 1 + 2).

## Dependencies
Không chặn plan nào. Chạm `PetBattle.win()` và `DAILY_CHECKIN_GIFTS` — xem lại nếu plan khác cũng sửa.
