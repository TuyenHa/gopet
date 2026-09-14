# Phase 04 — Test, verify và QA tay trên map Thành Phố Linh Thú

## Context
- [plan.md](plan.md) · [phase-01](phase-01-proximity-core.md) · [phase-02](phase-02-talk-prompt-button.md) · [phase-03](phase-03-choice-dialog-vertical.md)
- File đọc: `GopetUnityClient/verify.ps1`, `run-playmode-tests.ps1`,
  `Assets/Tests/PlayMode/GameHudPlayModeTests.cs` (mẫu dựng scene + avatar),
  `Assets/Scripts/Net/Npc/LinhThuCityNpcOptions.cs`

## Overview
- Priority: P1 — Status: pending — Effort: 1h — Phụ thuộc: 01, 02, 03

## Key Insights
- Hai tầng test có sẵn: thuần C# `tests/Gopet.Net.Tests` (chạy trong `verify.ps1` step 5) và
  PlayMode `Assets/Tests/PlayMode` (chỉ COMPILE trong verify step 8; muốn CHẠY phải đóng
  Unity Editor rồi `run-playmode-tests.ps1`).
- `GameHudPlayModeTests.cs:112-113` là mẫu sẵn có để dựng avatar + gắn bubble trong PlayMode.
- Bảng QA: TRAN CHAN id **-1**, vị trí jar (374, 117), map 11, 7 optionId
  = 1, 2, 3, 4, 41, 60, 81 (`LinhThuCityNpcOptions.cs:19-26`).
- File test cũng bị gate 200 dòng (`verify.ps1:151-153` quét cả `Assets/Tests`).

## Test Matrix
| Tầng | File | Nội dung |
|------|------|----------|
| Unit (thuần C#) | `tests/Gopet.Net.Tests/NpcProximityTests.cs` (phase 01) | nearest, ngoài tầm, hysteresis, rỗng, id âm |
| Unit (thuần C#) | `tests/Gopet.Net.Tests/ChoiceDialogLayoutTests.cs` | count ≤ 3 → ngang & số liệu y hệt công thức cũ; count 7 → dọc, rowWidth ≥ 300, panelHeight ≤ 88% màn hình; count 20 → vẫn trong màn hình |
| PlayMode | `Assets/Tests/PlayMode/NpcTalkPromptTests.cs` | (a) `Attach` + `SetVisible(true/false)` bật/tắt object; (b) `OnPointerClick` gọi callback đúng 1 lần; (c) `WorldActorView.SetTalkPromptVisible(true)` rồi click nút → nhận đúng npcId |
| PlayMode | `Assets/Tests/PlayMode/ChoiceDialogLayoutUiTests.cs` | Bind 7 nhãn TRAN CHAN → `Buttons.Count == 7`, các `anchoredPosition.y` đôi một khác nhau, width ≥ 300 |
| Integration (tay) | — | Chạy Unity + GServer, map 11 |

Không viết test cho `WorldActorLayer.Update` (cần Self + handler mạng): phủ bằng QA tay —
ghi rõ đây là lỗ hổng test có chủ đích.

## Implementation Steps
1. Viết 2 file test thuần C# (chạy nhanh, không cần Unity): `NpcProximityTests`,
   `ChoiceDialogLayoutTests`.
2. Viết 2 file PlayMode test; theo mẫu setup của `GameHudPlayModeTests` (tạo `GameObject` tạm,
   `TearDown` destroy hết).
3. `powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1` → cả 10 bước xanh.
4. Đóng Unity Editor → `run-playmode-tests.ps1` → 0 fail.
5. QA tay (checklist dưới). Bật GServer theo cách thường dùng của repo.
6. Nếu ngưỡng 110/150px thấy chưa hợp lý khi chơi → chỉnh hằng số trong `NpcProximity` và chạy
   lại test (test dùng hằng số, không hardcode số).
7. Cập nhật `docs/project-changelog.md` (mục feature mới) — giao `docs-manager`.

## QA Checklist (map 11 — Thành Phố Linh Thú)
- [ ] Vào map, đứng xa TRAN CHAN → KHÔNG có nút.
- [ ] Đi tới gần → nút "Nói chuyện" hiện trên đầu NPC, không đè nhãn tên, không đè bong bóng gợi ý.
- [ ] Bấm nút → popup 7 dòng: Nhận pet miễn phí / Shop Pet / Top Pet / Top Đại gia / Top Phù hộ /
      Nhập mã quà tặng / Gộp đồ server cũ — đọc được, không tràn màn hình.
- [ ] Bấm "Shop Pet" → mở đúng flow shop (chứng minh `SelectNpcOption` không lệch index).
- [ ] Đóng popup, đi xa → nút biến mất.
- [ ] Đi qua Bác sĩ xì tin (-7) → nút chuyển sang NPC đó; không lúc nào có 2 nút.
- [ ] Click thẳng vào sprite NPC (không dùng nút) → vẫn mở đúng popup (không hồi quy).
- [ ] Qua portal sang map khác rồi quay lại → nút hoạt động bình thường, không nút mồ côi.
- [ ] Vào trận (battle mode) → popup/nút không chặn giao diện trận.
- [ ] FPS không tụt rõ rệt khi đứng giữa cụm NPC.

## Success Criteria
- `verify.ps1`: 10/10 OK (bao gồm gate 200 dòng — đã hết nợ `WorldActorView.cs`).
- PlayMode suite: 0 fail.
- Toàn bộ QA checklist tick hết.

## Risk Assessment
| Rủi ro | KN | TĐ | Giảm thiểu |
|--------|----|----|-----------|
| PlayMode test không chạy được vì Editor đang mở | Cao | Thấp | Đóng Editor trước, ghi rõ trong bước 4 |
| Không có server để QA | TB | Cao | Dùng GServer local (`SRCGOPETGOC/GServer`); nếu không bật được → BLOCKED, báo lại |
| Option server khác bảng hằng số | Thấp | TB | Bảng chỉ để QA đối chiếu; code luôn dùng option server gửi |
| File test vượt 200 dòng | TB | Thấp | Tách theo chủ đề, mỗi file 1 nhóm test |

## Next Steps
- `code-reviewer` review toàn bộ diff 3 phase.
- `docs-manager` cập nhật `docs/project-changelog.md` + ghi chú trong `docs/system-architecture.md`
  nếu có mục world-UI.
- Theo dõi: nếu người chơi phàn nàn nút che tầm nhìn ở map đông NPC → cân nhắc chỉ hiện khi
  đứng yên (chưa làm — YAGNI).
