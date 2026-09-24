---
phase: 2
title: Assets nền và hạt hiệu ứng
status: completed
priority: P1
effort: 3h
dependencies: []
---

# Phase 2: Assets nền và hạt hiệu ứng

## Overview
Sinh 5 ảnh nền mới cùng phong cách pixel art và cùng bố cục với `bg-forest.png` (960×640),
cộng các sprite hạt nhỏ cho hiệu ứng và icon nút.

## Key Insights
- `tools/image-gen/generate-image.py` (OpenAI gpt-image) đã dùng cho `bg-forest`, banner, icon kỹ năng.
- Bố cục phải giữ **khoảng đất trống ở giữa** và hai vị trí đứng pet trái/phải giống `bg-forest`, nếu không pet đứng lơ lửng hoặc bị che.
- Sprite nhỏ (bông tuyết, cánh hoa, tàn lửa) vẽ bằng code như `gen-flame.py`: có alpha sạch, pixel sắc.
- Bẫy `.meta`: sprite đơn phải `spriteMode: 1`, `sprites: []` (xem docs/battle-system.md §10.3). Không chép meta từ `btn-attack-big`.
- Texture pixel: `filterMode: Point`, tắt mipmap.

## Requirements
- Nền: `Resources/Battle/bg/bg-canopy.png`, `bg-sakura.png`, `bg-snow.png`, `bg-cave.png`, `bg-fire.png` — 960×640, không có chữ, không vẽ sẵn con vật hay hạt (hiệu ứng động lo phần đó).
- Hạt (`Resources/Battle/ambient/`): `butterfly-0/1` (2 khung vỗ cánh), `dragonfly-0/1`, `petal`, `snowflake`, `ember`, `bat-fly-0/1`, `bat-hang`, `flame-small` (hoặc dùng lại sprite lửa `BattleFlameFallFx`).
- Icon nút: `Resources/Battle/btn-scene-round.png`, cùng khung tròn vàng với `btn-skill-round`.
- Thumbnail popup: dùng chính ảnh nền, thu nhỏ bằng `Image` (không cần asset riêng).

## Implementation Steps
1. Viết prompt cho từng nền, dựa trên mô tả `bg-forest` + "empty dirt clearing center, side-view battle arena, pixel art, 3:2". Sinh, cắt/đổi cỡ về 960×640.
2. Mang `bg-forest` cạnh từng ảnh mới để so bố cục vùng đứng của pet.
3. Viết `tools/image-gen/gen-ambient-sprites.py` vẽ các hạt nhỏ bằng PIL (pixel art 8–24px).
4. Sinh icon nút bằng image-gen (`--transparent`), cắt sát nội dung.
5. Tạo `.meta` đúng (sprite đơn, Point, không mipmap); mở Unity kiểm không có ô trắng.

## Success Criteria
- [ ] 5 nền đúng cỡ, cùng phong cách, pet đặt vào không lệch chân đất.
- [ ] Mọi sprite hạt có alpha và nét pixel sắc.
- [ ] Unity import không cảnh báo, không có ô trắng.

## Risk Assessment
- Ảnh AI lệch bố cục/phong cách: sinh nhiều bản (`-n 4`) rồi chọn; tệ nhất sửa tay.
- Cần `OPENAI_API_KEY` trong `tools/image-gen/.env`.
