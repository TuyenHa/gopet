---
title: "Nút 'Nói chuyện' nổi khi tới gần NPC"
description: "Hiện nút world-space 'Nói chuyện' khi người chơi tới gần NPC, bấm vào chạy flow TalkToNpc có sẵn và mở popup option."
status: pending
priority: P2
effort: 4.5h
branch: master
tags: [unity-client, npc, world-ui, proximity]
created: 2026-09-14
---

# Nút "Nói chuyện" theo khoảng cách tới NPC

## Mục tiêu
Người chơi đi tới gần NPC bất kỳ (map hiện tại) → hiện nút nổi "Nói chuyện" trên đầu NPC.
Bấm → `GuiderHandler.TalkToNpc(npcId)` (đã có) → server trả `NpcOptions` → `UiRoot.ShowNpcOptions`
render popup N nút. Đi xa → nút tự ẩn. Click thẳng vào sprite NPC vẫn giữ nguyên.

## Phạm vi
- KHÔNG đụng protocol/server. Không packet mới.
- Nút generic cho mọi NPC. TRAN CHAN (id -1, map 11, 7 option) chỉ là case QA.
- CÓ sửa layout popup: `ChoiceDialogView` hiện xếp nút NGANG một hàng → 7 option = nút rộng
  ~42px, chữ 16px không đọc được (đã xác minh, xem phase 03). Đây là chặn cứng của yêu cầu.

## Các phase

| # | Phase | Effort | Trạng thái | Phụ thuộc |
|---|-------|--------|-----------|-----------|
| 01 | [Tách WorldActorView + logic khoảng cách thuần C#](phase-01-proximity-core.md) | 1h | pending | — |
| 02 | [Nút world-space NpcTalkPrompt + wiring layer](phase-02-talk-prompt-button.md) | 1.5h | pending | 01 |
| 03 | [ChoiceDialogView xếp nút dọc khi >3 option](phase-03-choice-dialog-vertical.md) | 1h | pending | — (song song 01/02) |
| 04 | [Test + verify + QA tay trên map 11](phase-04-tests-and-verification.md) | 1h | pending | 01, 02, 03 |

Phase 03 độc lập file với 01/02 (UI vs World) → chạy song song được.

## Phụ thuộc / mốc đã xác minh
- `MapScene.Self` (MapScene.cs:27) — avatar người chơi; null trước `ON_UPDATE_PLAYER_IN_MAP`.
- NPC và avatar cùng parent `MapScene.transform` → so sánh `localPosition` hợp lệ.
  `MapPlacement.PixelsPerUnit = 1` (MapPlacement.cs:14) → 1 unit = 1 pixel jar.
- `WorldActorLayer._npcs` (WorldActorLayer.cs:12) = đúng tập NPC map hiện tại.
- `Physics2DRaycaster` đã gắn vào camera (GameSession.cs:145-146) → collider world nhận click.
- Di chuyển bằng joystick/phím (`MovementController.ReadDirection`), KHÔNG click-to-move →
  bấm nút không gây lệnh đi.
- `UiRoot.ShowNpcOptions` (UiRoot.cs:193-208) đã gọi `SelectNpcOption` đúng — giữ nguyên.

## Rủi ro chính (chi tiết trong từng phase)
1. `WorldActorView.cs` hiện 213 dòng > gate 200 và KHÔNG có trong allowlist của
   `verify.ps1` (step 10) / `CODE_HEALTH_EXCEPTIONS.md` → verify đang đỏ sẵn. Phase 01 tách
   partial để vừa sửa nợ vừa có chỗ thêm code.
2. NPC id là số ÂM (-1, -7, -15…) → sentinel "không có NPC" phải là `int.MinValue`, không
   được dùng -1.
3. Nút và `NpcPurposeBubble`/`ChatBubble` cùng bám transform NPC → không tái dùng component
   `ChatBubble` (AttachOrUpdate tìm `GetComponentInChildren<ChatBubble>()` sẽ ghi đè text).

## Rollback
Mỗi phase là một commit độc lập, revert được riêng: 01 (thuần refactor + class mới không ai
gọi), 02 (xoá component + 1 khối Update), 03 (revert layout về hàng ngang).

## Definition of done
Chạy Unity, vào map 11, đi lại gần TRAN CHAN → nút "Nói chuyện" hiện; bấm → popup 7 option
đọc được; đi xa → nút biến mất; `verify.ps1` xanh cả 10 bước.
