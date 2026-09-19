---
phase: 6
title: "Vượt ải: kiểm chứng và đóng gap"
status: pending
priority: P2
effort: "1d"
dependencies: [2]
---

# Phase 6: Vượt ải (`ChallengePlace`) — kiểm chứng và đóng gap

## Overview

Audit ban đầu cho rằng "Unity chưa có gì cho Vượt ải". Đọc kỹ server cho thấy
**phần lớn đã chạy nhờ hạ tầng chung** — phase này trước hết là **kiểm chứng**, sau
đó mới đóng những gap thật sự còn lại. Không viết lại thứ đã hoạt động.

## Key Insights — hạ tầng đã có sẵn

| Thành phần server | Opcode | Unity |
|---|---|---|
| Vào map: NPC `OP_CHALLENGE` (10), trừ sao `STAR_JOIN_CHALLENGE`, `maps[12].addRandom` (`MenuController.selectNpcOption.cs:93`) | menu NPC generic | ✅ `GuiderHandler` |
| Đếm ngược phòng chờ / lượt (`ChallengePlace.sendTimePlace`) | `TIME_PLACE` (64) | ✅ `WorldStatusHandler` → `GameHud.ShowPlaceTime` |
| Chữ to "LƯỢT n" (`showBigTextEff`) | `SHOW_BIG_TEXT_EFF` (63) | ✅ `GameHud.ShowBigText` |
| Sinh mob theo lượt, boss mỗi 5 lượt | gói mob map thường | ✅ `WorldObjectHandler` |
| HP boss | `UPDATE_HP_BOSS` | ✅ `MapScene.ApplyBossHp` |
| Đánh mob | `ATTACK_MOB` (36) | ✅ `BattleHandler.SendAttackMob` |
| Bảng xếp hạng (`top_challenge`) | `OP_SHOW_TOP_CHALLENGE` (80) menu generic | ✅ (cần xác minh) |

**Kết luận:** gap thật có thể nhỏ hơn nhiều so với ước tính ban đầu. Bước 1 là đo,
không phải code.

## Luật chơi (từ `Place/ChallengePlace.cs`)

- Tối đa 4 người/phòng, chờ 60s trước khi bắt đầu.
- 25 lượt. Mỗi lượt 3 phút (`TIME_ATTACK`). Hết mob → chờ 15s sang lượt sau.
- Lượt chia hết cho 5 → boss (1 con, 2 con nếu đủ 4 người). Boss có `TimeOut`.
- Số mob thường = `4 + (numPlayer − 1) × 4`, vị trí lấy từ `MOB_XY[17]`.
- Level mob = số lượt, trừ các mốc trong `MOB_LVL` (lượt 1→lv3 … lượt 24→lv45).
- Kết thúc → ghi `top_challenge` (Type, Time, Turn, Name, TeamId).
- Trong phòng: cấm tháo pet/skin (`MenuController.selectMenu.cs:55`).

## Requirements

**Functional**
- Đăng ký → vào map 12 → chơi hết vòng đời 25 lượt trên Unity không lỗi.
- Đếm ngược và "LƯỢT n" hiện đúng.
- Boss xuất hiện đúng lượt 5/10/15/20/25, HP bar cập nhật.
- Xem được bảng xếp hạng vượt ải.

**Non-functional**
- 16 mob cùng lúc (4 người) không tụt frame nghiêm trọng.

## Architecture

Không kiến trúc mới. Nếu phát hiện gap, ưu tiên theo thứ tự:
1. Nối event đã có vào HUD sẵn có.
2. Mở rộng component sẵn có.
3. Chỉ tạo file mới khi thật sự không tái dùng được (DRY).

## Related Code Files

- Read for context (server): `Place/ChallengePlace.cs`, `Data/map/ChallengeMap.cs`,
  `Server/MenuController.selectNpcOption.cs:93,452`, `Data/top/TopChallenge*.cs`
- Read for context (client): `Assets/Scripts/Runtime/World/GameHud.Status.cs`,
  `Assets/Scripts/Net/Map/WorldObjectHandler.cs`, `Assets/Scripts/Net/Map/WorldStatusHandler.cs`
- Modify: **xác định sau bước đo** — ghi danh sách vào phase file này trước khi code.

## Implementation Steps

1. **Đo trước.** Dựng server local, một account có đủ sao, vào NPC vượt ải, chơi
   ít nhất tới lượt 6 (qua 1 boss). Ghi lại bằng:
   - packet dump server,
   - log client + screenshot mỗi mốc,
   - danh sách opcode rơi vào `OnUnhandled`.
2. Lập bảng "kỳ vọng vs thực tế" cho từng dòng trong bảng Key Insights. Ghi ra
   `plans/260917-1812-pvp-arena-battle-parity/reports/vuot-ai-do-luong.md`.
3. Chỉ những dòng ❌ mới thành việc. Cập nhật mục "Related Code Files" và
   "Todo List" của phase này bằng kết quả thật.
4. Gap khả năng cao (kiểm chứng, đừng giả định):
   - `TIME_PLACE` dùng chung ô HUD với đấu trường → cần nhãn phân biệt lượt/phòng chờ.
   - Không có chỉ báo "còn N mob" của lượt hiện tại (server không gửi → có thể đếm
     entity phía client).
   - Bảng `top_challenge` render qua menu generic — xác minh cột hiện đủ.
   - Overlay battle khi đánh mob giữa đám đông (phase 02/03 phải cover).
5. Sửa các gap xác nhận được, ưu tiên P1 trước.
6. Chơi lại trọn 25 lượt (hoặc dùng `ZoneCommand.cs:43` để nhảy lượt nếu có sẵn
   đường tắt) → xác nhận ghi được `top_challenge`.

## Todo List

- [ ] Chơi thử tới lượt 6, thu dump + log + screenshot
- [ ] Viết `reports/vuot-ai-do-luong.md` với bảng kỳ vọng vs thực tế
- [ ] Cập nhật danh sách file cần sửa dựa trên kết quả đo
- [ ] Sửa các gap xác nhận được
- [ ] Chạy trọn vòng đời, xác nhận `top_challenge` có bản ghi
- [ ] `verify.ps1` / PlayMode pass

## Success Criteria

- [ ] Có report đo lường với bằng chứng (dump/log/ảnh) cho từng mục.
- [ ] 0 opcode của luồng vượt ải rơi vào `OnUnhandled`.
- [ ] Boss xuất hiện đúng lượt và HP bar chạy.
- [ ] Hết 25 lượt → có dòng mới trong `top_challenge` với đúng `Turn` và `TeamId`.
- [ ] Người chơi biết đang ở lượt mấy và còn bao nhiêu thời gian.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Chơi đủ 25 lượt tốn ~75 phút | Cao | Dùng `ZoneCommand` / chỉnh `TIME_ATTACK` tạm trên server local để rút ngắn; **không** commit thay đổi tuning |
| Cần 4 account để test đủ số mob | Trung bình | Test 1 người trước (4 mob), 4 người chỉ cho lần chứng nhận cuối |
| Ước tính scope sai vì chưa đo | Cao | Đây chính là lý do bước 1 là đo, không phải code |
| 16 mob + battle overlay gây tụt frame | Trung bình | Đo frame time ở lượt 20 với 4 người; cap spectator view từ phase 03 |

## Security Considerations

- `OP_CHALLENGE` trừ sao trước khi vào map — xác nhận client không có đường gửi lại
  gây trừ nhiều lần (server đã kiểm `checkStar` trước `MineStar`).
- Không được thêm đường client tự báo "đã qua lượt" — lượt do server điều khiển.

## Next Steps

→ Phase 07 gom kết quả đo thành test tự động ở mức làm được.
