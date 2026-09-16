# Phase 01 — Server: Data quà 31 ngày + 2 hộp bí ẩn + item rows

## Context
- Pattern data quà: `GopetManager.NOEL_DAILYS` (GopetManager.cs:798) — mảng tuple `int[][]`, mỗi phần tử = quà 1 mốc.
- Loại quà: `GIFT_ITEM=2`, `GIFT_COIN=1`, `GIFT_TITLE=13`, `GIFT_SKIN=14`, `GIFT_RANDOM_ITEM=9` (GopetManager.cs:194-224).
- Format 1 quà item: `{ GIFT_ITEM, itemId, count, 0 }`. Skin: `{ GIFT_SKIN, itemId, isInfinity(0/1), min, hours, day }`. Title: `{ GIFT_TITLE, titleId, isInfinity, min,... }`. Coin: `{ GIFT_COIN, amount }`.

## Overview
- **Priority**: cao (mọi phase logic phụ thuộc data này).
- **Status**: chưa làm.
- Định nghĩa bảng quà 31 ngày + data loot 2 hộp bí ẩn + thêm 2 dòng item vào DB.

## Requirements
### Bảng quà 31 ngày (`DAILY_CHECKIN_GIFTS`)
Mảng `int[][][]` (31 phần tử, mỗi ngày = danh sách quà như `NOEL_DAILYS`). Item ID tra từ bảng `item`:
- bình x2/x3/x4 EXP = 198/199/200; nhân sâm=180; nấm linh chi=179; lam ngọc=184; kim cương=185; huyết ngọc=188; mực cường hoá=125; bùa cường hoá=181; lam thạch=178; mực xăm hiếm=121; cực hiếm=122; thẻ kĩ năng=127; thẻ xăm hoà kì lân=140; bình máu L=191; bình mana L=194.

| Ngày | Quà (tuple) |
|---|---|
| 1 | `{GIFT_ITEM,198,3,0}` |
| 2 | `{GIFT_ITEM,191,5,0}`,`{GIFT_ITEM,194,5,0}` |
| 3 | `{GIFT_ITEM,179,3,0}` |
| 4 | `{GIFT_ITEM,198,5,0}` |
| 5 | `{GIFT_ITEM,180,3,0}` |
| 6 | `{GIFT_ITEM,199,2,0}` |
| 7 | `{GIFT_ITEM,185,3,0}`,`{GIFT_ITEM,199,3,0}` |
| 8 | `{GIFT_ITEM,125,3,0}` |
| 9 | `{GIFT_ITEM,178,3,0}` |
| 10 | `{GIFT_ITEM,199,3,0}` |
| 11 | `{GIFT_ITEM,184,5,0}` |
| 12 | `{GIFT_ENERGY,10}` |
| 13 | `{GIFT_ITEM,181,3,0}` |
| 14 | `{GIFT_ITEM,121,2,0}`,`{GIFT_ITEM,185,5,0}` |
| 15 | `{GIFT_ITEM,200,2,0}` |
| 16 | `{GIFT_ITEM,184,8,0}` |
| 17 | `{GIFT_ITEM,188,3,0}` |
| 18 | `{GIFT_ITEM,200,3,0}` |
| 19 | `{GIFT_ITEM,180,5,0}` |
| 20 | `{GIFT_ITEM,127,1,0}` |
| 21 | `{GIFT_ITEM,122,1,0}`,`{GIFT_COIN,100}` |
| 22 | `{GIFT_ITEM,200,3,0}` |
| 23 | `{GIFT_ITEM,185,5,0}` |
| 24 | `{GIFT_ITEM,125,5,0}` |
| 25 | `{GIFT_ITEM,185,8,0}` |
| 26 | `{GIFT_ITEM,121,3,0}` |
| 27 | `{GIFT_ENERGY,15}` |
| 28 | `{GIFT_ITEM,240024,1,0}` ← Hộp bí ẩn tuần 4 |
| 29 | `{GIFT_ITEM,240025,1,0}` ← Hộp bí ẩn cuối tháng |
| 30 | `{GIFT_ITEM,122,2,0}`,`{GIFT_ITEM,140,1,0}` |
| 31 | `{GIFT_COIN,200}` (+ danh hiệu nếu user cấp titleId) |

> Ngọc (GIFT_COIN) ở 21/31 — user đã đồng ý giữ ngọc ở mốc lớn. Danh hiệu ngày 31 cần titleId cụ thể (chưa có → tạm chỉ ngọc).

### Loot 2 hộp bí ẩn (`GIFT_RANDOM_ITEM`)
Format: `{ GIFT_RANDOM_ITEM, soLanBoc=1, itemId1,count1, itemId2,count2, ... }`. `RandomArray` bốc **đều** → tỉ lệ = số lần lặp cặp / tổng cặp. Mảnh ghép quý dùng mã âm (GameController.cs:4138-4184): pet tier2=`-125`, part pet tier3=`-128`, part wing tier2=`-131`.

- **Hộp `240024`** — 20 cặp, jackpot 1/20 = 5%:
  `198,3`×6, `185,5`×5, `184,8`×4, `188,5`×3, `121,3`×1, `-125,1`×1
- **Hộp `240025`** — 20 cặp, jackpot 1/20 = 5%:
  `185,8`×7, `122,2`×5, `140,1`×4, `-131,1`×3, `-128,1`×1

> User muốn jackpot hiếm hơn: giữ jackpot đúng **1 cặp / 20** (5%). Nếu muốn 3.3% → nâng tổng cặp lên 30 (thêm entry thường).

### 2 item rows mới (bảng `item`)
- `240024` "Hộp quà bí ẩn tuần 4", `240025` "Hộp quà bí ẩn cuối tháng".
- `type` = loại consumable/event (theo item `240023` Hộp quà Tết 2025 làm mẫu), `isStackable=1`, `iconPath` tạm dùng icon hộp có sẵn, `canTrade` tùy (đề xuất 0 = không trade để tránh farm).

## Related Code Files
- Sửa: `GServer/Manager/GopetManager.cs` (thêm `DAILY_CHECKIN_GIFTS`, `BOX_TUAN4_DATA`, `BOX_CUOITHANG_DATA`, const `ID_BOX_TUAN4=240024`, `ID_BOX_CUOITHANG=240025`).
- Sửa DB: INSERT 2 dòng vào bảng `item` (migration script, xem phase-02).

## Todo
- [ ] Khai báo const 2 item id + bảng `DAILY_CHECKIN_GIFTS` (31 phần tử).
- [ ] Khai báo `BOX_TUAN4_DATA`, `BOX_CUOITHANG_DATA`.
- [ ] Soạn INSERT item 240024/240025 (đưa vào migration phase-02).
- [ ] Compile server kiểm lỗi.

## Success Criteria
- Server build sạch. `DAILY_CHECKIN_GIFTS.Length == 31`. 2 item id load được từ `itemTemplate` sau khi seed DB.

## Quyết định (đã chốt)
- Ngày 31: **chỉ ngọc** `{GIFT_COIN,200}`, chưa gắn danh hiệu (để sau khi có titleId).
- 2 hộp: **`canTrade=0`** (khoá, chống farm).
- **Icon riêng** cho từng hộp: `items/240024.png`, `items/240025.png` — tạo mới bằng image-gen (xem phase-08).
