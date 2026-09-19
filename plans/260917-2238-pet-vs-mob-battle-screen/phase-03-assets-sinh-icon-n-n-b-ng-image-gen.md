---
phase: 3
title: "Assets: sinh icon/nền bằng image-gen"
status: done
priority: P2
effort: "3h"
dependencies: []
---

# Phase 3: Assets: sinh icon/nền bằng image-gen

## Overview
Sinh toàn bộ hình mà mockup cần nhưng project chưa có, bằng `tools/image-gen`
(`gpt-image-1.5`), cùng phong cách pixel-art của mockup.

## Requirements
- Functional: đủ asset cho phase 5 (danh sách dưới).
- Non-functional: nền trong suốt cho icon; ảnh cuối đã resize (không nhét ảnh 1024px
  vào build); import setting `Point` filter, không nén mờ.

## Danh sách asset

Đích: `GopetUnityClient/Assets/Resources/Battle/` (load qua `Resources.Load<Sprite>`).

| File | Kích thước cuối | Prompt gợi ý (thêm `--transparent` trừ nền) |
|---|---|---|
| `bg-forest.png` | 1536×1024 → 960×640 | pixel-art forest clearing dirt path battle arena, top-down 3/4, no characters |
| `vs.png` | 256 | golden pixel "VS" text badge |
| `btn-attack-big.png` | 256 | round bronze button with two crossed pixel swords |
| `btn-item.png` | 128 | round small button with red health potion |
| `icon-title-swords.png` | 64 | small crossed swords icon |
| `icon-back.png` | 64 | white left chevron |
| `icon-atk.png`, `icon-def.png`, `icon-crit.png` | 48 | crossed swords / shield / gold star |
| `icon-gold.png`, `icon-ruby.png`, `icon-lua.png` | 48 | kiểm tra `HudSkin` trước — có rồi thì tái dùng |
| `panel-blue.png`, `panel-red.png`, `panel-dark.png` | 9-slice 96 | dark navy frame with blue / red border, pixel style |
| `skills/{skillID}.png` × 30 | 64 | 1 icon/skill, prompt dựng từ `skill.name` + `description` |
| `skills/attack.png` | 64 | pixel sword (đòn thường "Tấn công") |
| `skills/unknown.png` | 64 | fallback |

## Implementation Steps
1. `cd tools/image-gen`, kiểm tra `.env` có key, cài requirements (có Pillow chưa — nếu
   chưa thì thêm vào `requirements.txt`; máy không có ImageMagick).
2. Xuất danh sách skill: `SELECT skillID, name, description FROM skill` → CSV trong
   `tools/image-gen/output/`.
3. Viết `tools/image-gen/generate-battle-assets.py`: đọc CSV + bảng asset tĩnh, gọi hàm
   sinh ảnh của `generate-image.py` (import, không copy code), có prompt style chung
   `"16-bit pixel art game UI icon, bold outline, vibrant, same style as ..."`, **bỏ qua
   file đã tồn tại** (chạy lại không tốn tiền), rồi resize bằng Pillow `NEAREST`.
4. Chạy thử 3 icon, cho user xem trước khi sinh cả bộ (tốn phí theo ảnh, ~45 ảnh).
5. Sinh toàn bộ, copy vào `Assets/Resources/Battle/`, đặt `.meta` (Sprite, Point, no
   compression, border 9-slice cho `panel-*`). Dùng script Editor sẵn có nếu project có
   AssetPostprocessor; nếu không thì thêm postprocessor cho thư mục `Battle/`.
6. Dùng `ai-multimodal`/đọc ảnh để soát nhanh đồng bộ phong cách, sinh lại cái lệch.

## Success Criteria
- [ ] Đủ file trong bảng; mỗi `skillID` trong DB có icon.
- [ ] Unity import không cảnh báo; sprite sắc nét (Point).
- [ ] Script chạy lại không sinh trùng.

## Risk Assessment
- Phong cách không đồng đều giữa các lần gọi → prompt style cố định + soát tay.
- Chi phí API → sinh thử trước, cache theo file.
- `.gitignore` của image-gen bỏ qua `output/` — asset thật phải nằm trong `Assets/`.
