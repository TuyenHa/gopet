# Phase 08 — Ảnh icon 2 hộp quà bí ẩn (image-gen)

## Context
- Icon item game: pixel-art chibi, **RGBA nền trong suốt**, ~**32×32** (dao động 32×30..40×37). Mẫu: `GServer/assets/items/240023.png` (hộp Tết, hồng/vàng), `240018.png` (bó hoa).
- Đường dẫn dùng trong DB: `items/240024.png`, `items/240025.png` (phase-01/02).
- Nơi đặt file: `GServer/assets/items/` (server phục vụ) + asset icon phía client Unity nếu client không tải icon từ server.

## Overview
- **Priority**: thấp (song song, không chặn logic).
- **Status**: ✅ ĐÃ XONG (ảnh đã tạo & đặt vào assets).
- Tạo 2 icon hộp quà bí ẩn khác nhau, khớp style pixel-art item hiện có.

## Đã thực hiện
- Tool: `tools/image-gen/generate-image.py` (OpenAI `gpt-image-2`, key trong `tools/image-gen/.env`) — KHÔNG phải skill ai-multimodal (skill đó thiếu key).
- Gen 2 ảnh 1024×1024 transparent → crop theo alpha bbox (bỏ glow rìa) → square-pad → resize 32×32 LANCZOS.
- Output cuối: `GServer/assets/items/240024.png` (hộp xanh-bạc), `240025.png` (hộp vàng-tím), RGBA nền trong suốt.
- Client Unity tải icon từ **server** qua `RemoteAssetCache` theo path `items/{id}.png` → KHÔNG cần copy sang client (giải quyết Unresolved bên dưới).
- Bản gốc 1024px giữ ở `tools/image-gen/output/box-week4.png`, `box-month.png` (nếu cần regen/tinh chỉnh).

## Requirements
- **Hộp 1 (`240024`) "tuần 4"**: hộp quà tông **xanh dương/bạc**, nơ, có dấu "?" nhẹ → gợi bí ẩn tầm trung.
- **Hộp 2 (`240025`) "cuối tháng"**: hộp quà tông **vàng/tím sang trọng**, ánh lấp lánh, nơ lớn → gợi cao cấp hơn hộp 1.
- Cả 2: nền trong suốt, viền/outline rõ để nổi trên khung túi đồ, cùng "gu" pixel-art với `240023`.

## Cách làm (image-gen)
- Dùng skill `ai-multimodal` (Imagen/Nano Banana) tạo ảnh hộp quà nền trong suốt/nền phẳng dễ tách, prompt "cute pixel-art game item icon, mystery gift box, transparent background".
- Hậu xử lý bằng `imagemagick`/PIL: resize về ~32×32, đảm bảo RGBA + alpha (xoá nền nếu gen ra nền đặc), so màu/độ tương phản khớp `240023`.
- Xuất 2 file → copy vào `GServer/assets/items/240024.png`, `240025.png`.

## Related Files
- Tạo: `GServer/assets/items/240024.png`, `GServer/assets/items/240025.png`.
- (Nếu client Unity đóng gói icon riêng: copy tương ứng vào thư mục asset item của client.)

## Todo
- [x] Gen ảnh 2 hộp (2 tông khác nhau).
- [x] Resize 32×32 + đảm bảo nền trong suốt (RGBA).
- [x] Copy vào `GServer/assets/items/`.
- [ ] Kiểm hiển thị thực tế trong túi đồ sau khi seed DB (phase-02) — chờ item row.

## Success Criteria
- 2 file PNG RGBA 32×32, nền trong suốt, phân biệt rõ 2 hộp, hợp gu icon cũ. ✅

## Đã chốt
- Client load icon từ server qua `RemoteAssetCache` (path `items/{id}.png`) → chỉ đặt file ở `GServer/assets/items/`, không cần asset client.
