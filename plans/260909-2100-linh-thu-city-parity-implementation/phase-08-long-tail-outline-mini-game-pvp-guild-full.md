---
phase: 8
title: Long-tail outline (mini-game/PVP/guild full)
status: completed
priority: P3
effort: 8-12w (outline)
dependencies:
  - 1
  - 2
  - 4
  - 5
---

# Phase 8: Long-tail outline (mini-game/PVP/guild full)

> **BACKLOG PARTIAL CLOSE (2026-09-10) — packets + view hạ tầng đủ cho các mảng.**
>
> Đã dựng:
> - `TargetPlayerMenu` view + `TargetPlayerPackets` (Challenge/RequestInfo/SendPk/ViewEquipment).
> - `FriendPackets` — AddFriendById + AddFriendByName.
> - `GuildPackets` — 9 sub-command của CLAN family (info/donate/search/topFund/topGrowth/requestJoin/chatHistory + PlayerDonate với amount).
> - `ChannelPackets` — GetChannelInfo (opcode 7) + ChangeChannel (opcode 24, 4 int).
> - `JarStrings.Language` toggle + PlayerPrefs persist; `Get(index)` dispatch theo Current.
> - `EmoteController` stub (Sit/Wave/Cheer) — client-side ONLY (server không có opcode).
> - `LinhThuCityNpcOptions` bảng hằng số 15 option ID cho 5 NPC map 11 — dispatch qua `GuiderPackets.SelectNpcOption` sẵn có.
> - `MiniGamePackets.OpenMiniGame` trigger 4 game (opcode 81/2/sbyte gameType).
> - 22 test byte-for-byte cho packets P8 (`P8PacketsTests.cs`).
>
> Còn KHÔNG dựng (ngoài scope):
> - **UI game engine cho 4 mini-game** — mỗi cái 6-8w, cần scene riêng.
> - **Tap-detect avatar khác** để mở `TargetPlayerMenu` — cần wire trong `WorldActorView` (Runtime layer chưa có tap event). API `GameSession.OpenTargetPlayerMenu(userId, name)` đã public.
> - **Sit/emote animation clip mapping** — cần map clip index Sit/Wave trong .anu của actor.
> - **Guild view (top ranking / member list)** — server trả về menu qua Guider generic hoặc format riêng; cần verify.
> - **PVP invitation accept dialog** — server dùng SERVER_MESSAGE/SEND_YES_NO, GuiderHandler generic đã xử lý; chưa test end-to-end.
> - **Vườn / cà phê / phòng vé / gara** — building type click, thuộc plan `260907-2210-map-portal-warp-shop-interaction`.
> - **Trade / gift** — chưa reverse opcode phía server, cần đọc kỹ hơn.

## Overview

**Đây là phác thảo, không phải plan chi tiết.** Viết cụ thể khi 7 phase trước ổn định. Ghi ở đây để không quên phạm vi còn lại cần cover.

Chồng lấp có ý với `260905-1355-gopet-unity-client-rebuild/phase-08-long-tail-features-outline.md` — plan đó bao phủ **toàn cục** (mọi map); phase này focus **những mảng đặc thù đến từ map 11 hub** và các NPC ở đó.

## Nhóm 1 — Mini-game bàn cờ (Building type 13-16)

`eg.java` case 13-16 → mở Caro / Cờ tướng / Tiến lên / Phỏm qua `cd(602/603/606/607)`.

| Mini-game | Effort | Ghi chú |
|---|---|---|
| Caro | ~1w | Board 15×15, ràng buộc 5 quân |
| Cờ tướng | 2w | 32 quân, luật đầy đủ, checkmate detection |
| Tiến lên | 2w | Bài 52, luật miền Nam |
| Phỏm | 2w | Bài, luật miền Bắc |

- Mỗi game là scene riêng, tách khỏi world map.
- Server đóng vai trò matcher + validator; client render UI.
- Có thể coi mỗi game là 1 sub-project — thuê riêng nếu cần.

## Nhóm 2 — NPC-specific flows

Từ báo cáo [`analysis-260909-2036`](../reports/analysis-260909-2036-linh-thu-city-parity-jar-vs-unity.md) §4.3:

### NPC -1 Trần Trấn
- Nhận pet miễn phí (starter)
- Shop Pet (mua/bán pet) — cần preview pet
- Top Pet / Top Đại Gia / Top Phú Hộ — bảng xếp hạng
- Nhập mã quà tặng
- Gộp đồ server cũ

### NPC -7 Bác sĩ xì tin
- Hồi sinh pet sau PK — flow xác nhận cost
- Nhận nhiệm vụ hằng ngày — quest list view
- Tẩy gym — reset training

### NPC -15 Sứ giả bang hội (guild full)
- Vào khu vực bang (warp)
- Top lvl bang hội (ranking view)
- Tạo bang hội (form input + phí)
- Sự kiện bang hội
- Cống hiến bang hội
- **Guild chat đã có** ở Phase 5.

### NPC -24 Ông già Noel
- Điểm danh — calendar 30 ngày + reward preview.

### NPC -25 Sứ giả thiên thần
- Hướng dẫn lên thiên đình — dialog tour dài
- Hiến tặng thú cưng — chọn pet + confirm.

## Nhóm 3 — PVP giữa người chơi

- Mời PK (opcode 12 challenge — `fr.java:224-230`)
- Nhận PK request → dialog accept
- Arena queue (building type 26, opcode 58)

## Nhóm 4 — Xã hội mở rộng

- Target-player menu khi tap avatar khác (info/thách đấu/kết bạn/xem đồ pet/report/block).
- Trade / gift item giữa 2 người.
- Chuyển kênh (`gw.a(53)`).
- Đổi ngôn ngữ VN/EN toggle (`gw.a(143..145)`).

## Nhóm 5 — Ngoại cảnh

- Sit/emote animation (`ei.java`).
- Ẩn/hiện tên người chơi (cd 3285).
- Vườn (trồng cây/thu hoạch) — building type 7.
- Cà phê — building type 10.
- Phòng vé sự kiện — building type 8.
- Gara/Xe — building type 23.

## Thứ tự đề xuất khi mở phase 8 chi tiết

1. Guild full flow (5 mục sứ giả bang) — chọn nhiều user quan tâm.
2. NPC -1 Trần Trấn — starter flow, gate của người mới chơi.
3. PVP + arena.
4. Target-player menu + xã hội mở rộng.
5. Điểm danh + quest hằng ngày.
6. Mini-game (chọn 1-2 game phổ biến nhất trước; 4 game là tham vọng).
7. Vườn / cà phê / vé — ưu tiên thấp.

## Success Criteria (khi mở phase chi tiết)

- Mỗi nhóm có phase riêng với opcode + wire format đã reverse.
- Mỗi phase đi kèm PlayModeTest + LiveSmoke check.
- Anti-cheat lớp cuối (phase 8 của plan gốc) bật lại trước khi ship.

## Risk Assessment

- **R1: Mini-game rule engine** — dễ sai luật, cần test rules với người chơi thật.
- **R2: Guild full flow phụ thuộc guild state phía server** — có thể phát hiện bug server chưa lộ.
- **R3: PVP + trade là 2 vector scam lớn** — cần validate server-side kỹ trước khi bật.
- **R4: Effort đội** — 4 mini-game + guild + PVP + xã hội = 8-12w. Realistic hơn: cắt còn 60% (mini-game 2 game, guild 3 mục, PVP core only).

## Ghi chú

- Anti-cheat, animation `.anu`, âm thanh — thuộc phase 8 của plan gốc `260905-1355-gopet-unity-client-rebuild`, không lặp ở đây.
- Sự kiện theo năm (`Data/Event/Year2024`) — cân nhắc bỏ theo phase 8 gốc.
