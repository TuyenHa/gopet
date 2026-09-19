# Phase 04 — Kiểm thử + cập nhật tài liệu

## Context Links

- Overview: [plan.md](plan.md)
- Phụ thuộc: [phase-01](phase-01-vong-luot-va-khoa-nut.md), [phase-02](phase-02-hoat-canh-va-hang-doi-luot.md), [phase-03](phase-03-server-tru-exp-thua-pve.md)
- Doc cập nhật: `docs/battle-system.md`

## Overview

- **Priority:** P1
- **Status:** done · **Chặn bởi:** 01, 02, 03
- **Effort:** 1.5h
- Chạy toàn bộ cổng kiểm chứng, đối chiếu 6 yêu cầu gốc trên server thật, ghi lại các quy ước
  wire vừa phát hiện vào `docs/battle-system.md` để lần sau không phải điều tra lại.

## Key Insights

1. **`verify.ps1` là cổng chính** (`GopetUnityClient/verify.ps1`), 10 bước, gồm:
   4 = compile tầng Net (netstandard2.1), 5 = unit test, 6/7 = compile Runtime/Editor với DLL Unity thật,
   8 = compile PlayMode test, 10 = chặn file `.cs` > 200 dòng (allowlist ở `CODE_HEALTH_EXCEPTIONS.md`).
2. **Unit test pure-logic nằm ở `tests/Gopet.Net.Tests`** — csproj include thẳng
   `Assets/Scripts/Net/**` và `Assets/Scripts/UiLogic/**`, `LangVersion 9.0` (khớp Unity 6).
   → cú pháp C# 10+ sẽ fail ở đây trước khi mở Editor.
3. **PlayMode test phải đóng Unity Editor mới chạy** (`run-playmode-tests.ps1`); `verify.ps1`
   bước 8 chỉ *compile*, không chạy → phần hoạt cảnh phải test tay.
4. **Server cần MariaDB Docker + `GOPET_DB_PASSWORD`** trước khi chạy `Gopet.exe`.
5. `docs/` hiện chỉ có `battle-system.md` và `journals/`. **Không có** `development-roadmap.md`
   / `project-changelog.md` → không tạo mới trong đợt này (xem Câu hỏi tồn đọng).

## Requirements

- R1: `verify.ps1` 10/10 OK.
- R2: `dotnet build SRCGOPETGOC/GServer/Gopet.csproj` 0 error.
- R3: 6 yêu cầu gốc của user đều quan sát được trên server chạy thật.
- R4: `docs/battle-system.md` ghi lại 4 quy ước wire mới phát hiện.
- R5: Không hồi quy PvP/đấu trường.

## Architecture

### Ma trận kiểm thử

**Tự động**

| Lớp | Lệnh | Bao phủ |
|---|---|---|
| Unit | `dotnet test GopetUnityClient/tests/Gopet.Net.Tests` | `BattleTurnState` 8 ca (phase 01 bước 9), `SkillCooldownTracker` cũ |
| Compile client | `powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1` | Net / Runtime / Editor / PlayMode + rule 200 dòng + đối chiếu `GopetCmd.cs` ↔ `GopetCMD.cs` |
| Compile server | `dotnet build SRCGOPETGOC/GServer/Gopet.csproj` | phase 03 |

**Tay — trên server thật (PvE, map thường)**

| # | Yêu cầu gốc | Bước | Kỳ vọng |
|---|---|---|---|
| T1 | #6 nhãn lượt | Click quái, xem overlay mở | Hiện "Đến lượt bạn" ngay |
| T2 | #6 | Bấm Tấn công | Nhãn tắt khi gói 37 về |
| T3 | #6 | Chờ quái đánh | Nhãn bật lại |
| T4 | #6 | Pet dính độc/phản đòn | Nhãn KHÔNG nhấp nháy mất |
| T5 | #1 lao sang | Bấm Tấn công | Pet chạy sang cạnh quái → FX → chạy về đúng chỗ |
| T6 | #1 | 20 lượt liên tiếp | Vị trí 2 card không trôi |
| T7 | #1 khoá nút | Bấm Tấn công 5 lần thật nhanh | Chỉ 1 gói rời client; không có `redDialog("Chưa tới lượt của bạn")` |
| T8 | #2 kỹ năng | Bấm 1 kỹ năng 2 lần thật nhanh | Chỉ 1 gói `[81][37][4]`; đúng FX của skillId |
| T9 | #2 | Mở panel kỹ năng | Danh sách khớp `pet.skill` trong DB (đối chiếu `writeMyPetInfo` `PetBattle.cs:559-570`) |
| T10 | #3 HP/MP giảm | Đánh trúng quái | Thanh HP quái trượt xuống, số damage đỏ nổi lên, MP nổi sau ~1s |
| T11 | #4 bình máu | Bấm icon bình → menu 1016 nổi trên overlay → chọn item | Thanh HP pet mình TĂNG + float `+N` xanh; nhãn lượt vẫn còn; nút vẫn sáng |
| T12 | #5 xin thua | Bấm "Xin thua" → xác nhận | Panel kết quả "THUA CUỘC" + dòng "Pet bị trừ N exp" |
| T13 | #5 | Bấm "‹ Quay lại" → xác nhận | Y hệt T12 |
| T14 | #5 | Để quái giết pet | Y hệt T12 |
| T15 | #5 | So `pet.exp` trong DB trước/sau T12 | Giảm đúng `round(0.1 × PetExp[lvl])` |
| T16 | — | Thắng quái | Không có dòng trừ exp; `pet.exp` tăng |
| T17 | — | Kết trận giữa lúc hoạt cảnh đang chạy | Panel hiện ngay, card về chỗ, HP đúng số cuối |

**Hồi quy**

| # | Bước | Kỳ vọng |
|---|---|---|
| T18 | 1 trận PvP thách đấu | Nhãn lượt đúng cho cả 2 bên (server PvP gửi `LocalStarts` thật ở `BattleHandler.cs:89`) |
| T19 | 1 trận PK | Số exp trừ không đổi so với trước |
| T20 | 1 trận đấu trường (`coinBet = 0`) | Overlay vẫn đóng đúng (gói kết thúc `:916`) |
| T21 | Client jar cũ (`SRCGOPETGOC/client.jar`) đánh quái + thua | Đọc được gói 16, hiện dòng trừ exp, không văng |

### Nội dung cập nhật `docs/battle-system.md`

| Mục | Nội dung thêm |
|---|---|
| §2 Vòng lượt | `ActorId` của opcode 37 = **người vừa ra đòn** (`PetBattle.cs:307-308`, `:1109-1110`), client phải đảo để biết lượt kế |
| §2 | Gói `type = WAIT` + `0 effect` = **dùng vật phẩm**, KHÔNG đổi lượt (`:1643`, không có `nextTurn()`) |
| §2 | Gói `petId = -1` = độc/phản đòn (`:1547`, `:1617`), bỏ qua khi tính lượt |
| §4 Opcode battle | Gói 37 loại WAIT **chỉ ghi `mp`, không ghi `hp`** (`:336-343`). HP hồi từ vật phẩm đi qua `MY_PET_INFO` (opcode 40) |
| §6 xin thua | Thua PvE (hết máu HOẶC xin thua) trừ 10% EXP cấp hiện tại; thông báo đi qua danh sách `PetBattleText` của opcode 16, **không đổi wire** |
| §11 Version gate | Ghi rõ đợt này **không** thêm gate nào |

## Related Code Files

**Sửa**
- `docs/battle-system.md`

**Chạy (không sửa)**
- `GopetUnityClient/verify.ps1`
- `GopetUnityClient/run-playmode-tests.ps1`
- `SRCGOPETGOC/GServer/Gopet.csproj`

## Implementation Steps

1. `powershell -ExecutionPolicy Bypass -File GopetUnityClient/verify.ps1` — sửa tới khi 10/10 xanh.
   Nếu bước 10 báo file > 200 dòng: tách partial theo kế hoạch dự phòng đã ghi ở phase 01 bước 7
   và phase 02 (`BattlePetCard.Motion.cs`). **Không** thêm tên vào `CODE_HEALTH_EXCEPTIONS.md`.
2. `dotnet build SRCGOPETGOC/GServer/Gopet.csproj`.
3. Khởi động MariaDB Docker + `GOPET_DB_PASSWORD`, chạy `Gopet.exe`.
4. Chạy T1–T17 trên client Unity. Ghi kết quả từng dòng.
5. Chạy hồi quy T18–T21.
6. Cập nhật `docs/battle-system.md` theo bảng ở mục Architecture. Giữ giọng văn/cấu trúc hiện có,
   mọi khẳng định kèm `file:line`.
7. Nếu có mục nào FAIL: quay lại phase tương ứng, KHÔNG vá tạm ở phase này.

## Todo List

- [x] `verify.ps1` 10/10
- [x] `dotnet build` server sạch
- [x] T1–T4 nhãn lượt
- [x] T5–T8 lao sang + khoá nút + kỹ năng
- [x] T9–T11 skill từ DB, HP/MP, bình máu
- [x] T12–T16 trừ EXP thua PvE
- [x] T17 kết trận giữa hoạt cảnh
- [x] T18–T21 hồi quy PvP/PK/đấu trường/jar cũ
- [x] Cập nhật `docs/battle-system.md` (6 mục)

## Success Criteria

- Toàn bộ T1–T21 PASS, ghi lại trong phần Todo của phase này.
- `verify.ps1` 10/10 và `dotnet build` 0 error.
- `docs/battle-system.md` có đủ 6 mục bổ sung, mọi khẳng định kèm `file:line`.
- Không file `.cs` nào mới vượt 200 dòng; `CODE_HEALTH_EXCEPTIONS.md` không thêm dòng nào.

## Risk Assessment

| Rủi ro | Khả năng | Tác động | Giảm thiểu |
|---|---|---|---|
| Không dựng được server thật để test tay | Trung bình | Cao | T1–T10, T17 vẫn chạy được với server local Docker; nếu vẫn kẹt, báo blocker thay vì đánh dấu PASS khống |
| Hoạt cảnh chỉ lộ lỗi khi mạng trễ cao | Trung bình | Trung bình | T7/T8 spam nút để ép chồng gói; T17 ép kết trận giữa hoạt cảnh |
| Jar cũ (T21) không dựng được | Trung bình | Trung bình | Phase 03 không đổi wire → rủi ro thấp; nếu không test được, ghi rõ là chưa xác minh chứ không bỏ trắng |
| Sửa docs xong rồi code còn đổi | Trung bình | Thấp | Làm bước 6 **sau cùng**, sau khi T1–T21 xanh |

## Security Considerations

- Không commit `GOPET_DB_PASSWORD`, dump DB, hay bất kỳ thông tin kết nối nào vào repo.
- Ảnh chụp màn hình dùng làm bằng chứng test phải che tên tài khoản thật.
- Chạy lint + test trước commit; commit theo conventional commits, không nhắc tới AI.

## Next Steps

- Sau khi xanh: `code-reviewer` review toàn bộ diff của 3 phase.
- Cân nhắc cho đợt sau (ngoài phạm vi, đã ghi ở report §8 P1): đồng hồ lượt (pie đỏ),
  auto-attack nhánh tự đánh trong trận, throttle `ATTACK_MOB` (rủi ro ban trên map 12),
  popup `UPDATE_PET_LVL`.
