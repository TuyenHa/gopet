---
phase: 4
title: "Kiem thu va chinh bo cuc"
status: pending
priority: P1
effort: "2-3h"
dependencies: [2]
---

# Phase 4: Kiểm thử và chỉnh bố cục

## Overview
Be Vietnam Pro rộng và cao dòng hơn Arial; `MakeText` đặt `horizontalOverflow = Overflow` nên chữ tràn khỏi nút/tab cố định mà không báo lỗi. Phase này chạy test + soát mắt các màn dày chữ và sửa chỗ tràn/cắt.

## Requirements
- Functional: toàn bộ PlayMode + EditMode test pass (không skip, không nới assert cho qua).
- Functional: không chữ tràn ra ngoài nút/tab/khung, không cắt dấu trên chữ hoa (Ặ, Ế, Ỗ).
- Non-functional: sửa tối thiểu — ưu tiên giảm 1pt cỡ chữ hoặc nới width tại chỗ, không đổi hệ thống layout.

## Architecture
Không có kiến trúc mới. Checklist màn hình (ref 720×1280, kiểm thêm 1 tỉ lệ màn rộng):

| Màn | Điểm dễ vỡ |
|-----|-----------|
| Đăng nhập / splash / popup kết nối | nút, băng thông báo |
| ATM (`AtmPopupView`) | tab cỡ 11 Bold, input + placeholder italic |
| Bác sĩ / Heaven NPC (`PopupTabRail`) | rail tab chiều rộng cố định |
| Shop (`ShopItemRow`) | tên item dài + giá |
| Nhiệm vụ (`TaskListPopupView`), Animation menu | label `minHeight` 24/32 |
| HUD, minimap, nút tấn công/pet action | chữ cỡ 9–10, nút tròn |
| Popup tổng kết thắng quái, daily checkin | tiêu đề 18 Bold |

## Related Code Files
- Modify (tùy kết quả soát): các view trong `GopetUnityClient/Assets/Scripts/Runtime/UI/` bị tràn.
- Modify (nếu test fail do đo kích thước): `GopetUnityClient/Assets/Tests/PlayMode/*LayoutTests.cs` — chỉ khi assert phụ thuộc metric Arial, KHÔNG nới cho qua lỗi thật.

## Implementation Steps
1. Chạy test: `Unity.exe -batchmode -projectPath GopetUnityClient -runTests -testPlatform PlayMode -testResults <scratchpad>/playmode.xml` (và EditMode). Ghi lại fail.
2. Phân loại fail: lỗi layout thật → sửa view; assert bám metric Arial → cập nhật kỳ vọng kèm lý do trong commit.
3. Play trong Editor, đi qua từng màn trong bảng; chụp màn hình trước/sau (lưu `plans/260923-0553-be-vietnam-pro-font/visuals/`).
4. Chuỗi chứa ký hiệu đặc biệt từ server (★ ♥ → emoji): xác nhận hiển thị qua fallback font; không được thì ghi lại ký tự thiếu.
5. Build Android debug 1 lần, kiểm chữ trên thiết bị/emulator (fallback font hệ thống khác Windows).
6. Chạy lại toàn bộ test đến khi pass; `code-reviewer` review diff.

## Success Criteria
- [ ] 100% test pass.
- [ ] Mọi màn trong bảng không tràn/cắt chữ; có ảnh trước/sau.
- [ ] Android build hiển thị đúng font + dấu.

## Risk Assessment
- Sửa tràn lan man thành "redesign" → giới hạn: chỉ đổi cỡ chữ/width tại chỗ tràn.
- Fallback glyph trên Android khác Windows → bước 5 bắt được.
- Text sinh động (tên người chơi dài, chuỗi server) không thấy khi soát → thử với chuỗi dài nhất có thể (tên 16 ký tự hoa có dấu).

## Next Steps
- Cập nhật ghi chú parity trong `260913-jar-unity-parity-audit-and-certification` (UI font đã khác jar có chủ đích).
