# Phase 02 — Học và thay kỹ năng pet

## Context Links
- Audit: `plans/reports/parity-gap-260921-0042-jar-vs-unity.md` (P0-1)
- Client: `Assets/Scripts/Runtime/UI/PetProfileView.cs`, `Assets/Scripts/Net/Pet/PetProfilePackets.cs`
- Server: `GameController.cs:1034-1037` (nhận `MAGIC_LEARN_SKILL`), `:1391-1399` (`learnSkill`),
  `MenuController.selectMenu.cs:340-400` (xử lý chọn kỹ năng)

## Overview
- Ưu tiên: P0 — pet không học được kỹ năng mới, cũng không thay được kỹ năng cũ.
- Trạng thái: xong.

## Key Insights
- `GopetCmd.MAGIC_LEARN_SKILL` (81/31) có trong bảng hằng nhưng **0 call-site** phía Unity.
- Wire: `PET_SERVICE` + sbyte 31 + **int skillId**, trong đó `skillId` mang hai nghĩa
  (lưu vào `Player.skillId_learn`):
  - `-1` → **học mới** vào ô trống, cần `pet.skillPoint > 0`;
  - id một kỹ năng pet ĐANG có → **thay** đúng kỹ năng đó.
- Server không tự reset `skillId_learn` về -1 sau khi dùng, nên client phải gửi đúng giá trị
  ở MỖI lần bấm — không được dựa vào trạng thái cũ còn sót.
- Server trả `MENU_LEARN_NEW_SKILL = 799`, tức menu chuẩn → `GenericMenuView` sẵn có tự render,
  không phải dựng màn mới.

## Requirements
- Màn hồ sơ pet của CHÍNH MÌNH (`editable`) có nút "Học kỹ năng".
- Mỗi kỹ năng pet đang có kèm một lối "Thay".
- Hồ sơ pet người khác không có hai thứ trên.

## Architecture
`PetProfileView` phát `LearnSkillRequested(int skillId)`; `GameSession` gửi
`PetProfilePackets.LearnSkill(skillId)`. Danh sách kỹ năng chuyển từ một khối text sang từng
dòng bấm được để có chỗ gắn nút Thay.

## Related Code Files
- Sửa: `Runtime/UI/PetProfileView.cs`, `Runtime/World/GameSession.Interactions.cs`
- Thêm: packet `LearnSkill` trong `Net/Pet/PetProfilePackets.cs`
- Test: `tests/Gopet.Net.Tests/PetProfilePacketsTests.cs`

## Implementation Steps
1. Thêm `PetProfilePackets.LearnSkill(int skillId)` — `PET_SERVICE` + 31 + int.
2. `PetProfileView`: tách khối kỹ năng thành từng dòng, mỗi dòng một nút "Thay"; thêm nút
   "Học kỹ năng" (gửi -1). Chỉ dựng khi `editable`.
3. Nối event ở chỗ `GameSession` dựng `PetProfileView`.
4. Test wire cho cả hai giá trị (-1 và id thật).

## Todo List
- [x] Packet + test
- [x] UI + event
- [x] Nối `GameSession`
- [x] `verify.ps1` xanh

## Success Criteria
- Bấm "Học kỹ năng" → server mở menu 799 liệt kê kỹ năng học được.
- Bấm "Thay" ở một kỹ năng → cũng menu 799; chọn xong thì kỹ năng đó bị thay đúng ô.

## Risk Assessment
- Gửi nhầm `skillId` sẽ THAY nhầm kỹ năng. Giảm bằng test wire và lấy id thẳng từ
  `PetProfile.Skills[i].Id`, không đánh số lại.

## Security Considerations
- Server tự trừ tiền/điểm kỹ năng và kiểm trùng kỹ năng; client không tự quyết gì.

## Next Steps
- Không.
