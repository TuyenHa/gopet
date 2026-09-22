---
phase: 1
title: "Import font assets"
status: pending
priority: P2
effort: "30m"
dependencies: []
---

# Phase 1: Import font assets

## Overview
Đưa file TTF Be Vietnam Pro (Regular + SemiBold) và giấy phép OFL vào `Assets/Resources`, cấu hình importer dạng Dynamic.

## Requirements
- Functional: font nạp được qua `Resources.Load<Font>` ở runtime và trong PlayMode test.
- Non-functional: tổng dung lượng < 1 MB; kèm `OFL.txt` (điều kiện giấy phép).

## Architecture
Dự án dựng UI hoàn toàn bằng code + `Resources` (đã có `Resources/Jar`, `Resources/Ui`, `Resources/Battle`) → dùng `Resources` cho nhất quán, không cần serialized reference hay Addressables.

```
Assets/Resources/Fonts/BeVietnamPro/
├── BeVietnamPro-Regular.ttf
├── BeVietnamPro-SemiBold.ttf
└── OFL.txt
```

## Related Code Files
- Create: `GopetUnityClient/Assets/Resources/Fonts/BeVietnamPro/*` (+ `.meta` do Unity sinh)

## Implementation Steps
1. Tải từ nguồn chính thức Google Fonts (`github.com/google/fonts/tree/main/ofl/bevietnampro`): `BeVietnamPro-Regular.ttf`, `BeVietnamPro-SemiBold.ttf`, `OFL.txt`.
2. Chép vào `Assets/Resources/Fonts/BeVietnamPro/`.
3. Importer (Inspector hoặc sửa `.meta`): Character = `Dynamic`, Include Font Data = ✓, Rendering Mode = `Smooth`, Font Names (fallback) = `Arial, Roboto, Noto Sans` — để glyph thiếu (★ ♥ → emoji) rơi về font hệ thống.
4. Mở Unity để sinh `.meta`, xác nhận không lỗi import.

## Success Criteria
- [ ] 3 file + `.meta` nằm đúng thư mục, commit được.
- [ ] `Resources.Load<Font>("Fonts/BeVietnamPro/BeVietnamPro-Regular")` trả khác null (kiểm ở Phase 2 test).

## Risk Assessment
- Tải nhầm bản variable font → legacy `Text` không hỗ trợ trục biến thiên. Giảm thiểu: chỉ lấy file static theo tên độ đậm.
- Quên `OFL.txt` → vi phạm điều kiện phân phối. Giảm thiểu: checklist trên.

## Security Considerations
- Chỉ tải từ repo chính thức google/fonts, không dùng mirror.
