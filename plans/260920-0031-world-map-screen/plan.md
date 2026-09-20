---
title: "Màn hình bản đồ thế giới chọn map"
description: "Thay MapPickerView bằng bản đồ thế giới pan được, thumbnail bake từ tile jar, khoá map theo nhiệm vụ ở server."
status: in-progress
priority: P2
effort: 14h
branch: master
tags: [unity, gserver, ui, protocol, teleport]
created: 2026-09-20
---

# Màn hình bản đồ thế giới chọn map

Canvas lớn kéo/pan được, mỗi map là một node thumbnail bake từ tile jar thật + nhãn
tiếng Việt; map chưa mở hiện ổ khoá, bấm vào báo lý do server gửi xuống. Thay thế
hoàn toàn popup lưới `MapPickerView`. Vào từ 3 lối: menu nhân vật → Dịch chuyển,
nút minimap trên HUD, NPC phòng vé.

## Phases

| # | Phase | Trạng thái | Phụ thuộc | Ước lượng |
|---|-------|-----------|-----------|-----------|
| 01 | [Server: khoá map theo nhiệm vụ + TELE_MENU gửi lý do](phase-01-server-map-unlock-rules.md) | done | — | 3h |
| 02 | [Client: parse trường lý do khoá](phase-02-client-tele-menu-lock-reason.md) | done | 01 | 1h |
| 03 | [Tách bake thumbnail dùng chung (JarMapThumbnail)](phase-03-jar-map-thumbnail-shared-bake.md) | done | — | 2h |
| 04 | [WorldMapView: canvas pan, cụm khu vực, node + ổ khoá](phase-04-world-map-view-ui.md) | done | 03 | 4h |
| 05 | [Wiring GameSession: 3 lối vào, gắn MinimapWidget, xoá MapPickerView](phase-05-gamesession-wiring-entrypoints.md) | done | 02, 04 | 2h |
| 06 | [Tests + verify + docs](phase-06-tests-verify-docs.md) | done (chờ PlayMode) | 05 | 2h |

Phase 01 và 03 chạy song song được (khác repo/khác file). Phase 02 chỉ cần wire-format
của 01 đã chốt, không cần server chạy.

## Nguyên tắc xuyên suốt

- Mọi file `.cs` mới < 200 dòng (`verify.ps1` bước 10/10 chặn). Allowlist ngoại lệ ở
  `GopetUnityClient/CODE_HEALTH_EXCEPTIONS.md` — **không thêm tên file mới vào đó**.
- Tách partial theo quy ước sẵn có: `X.cs` (state + API công khai) / `X.Layout.cs`
  (dựng UI). Tên file PascalCase như phần còn lại của repo C#.
- Comment tiếng Việt, giải thích *tại sao*, theo style `MapPickerView.cs`.
- Không mock/fake: thumbnail là tile jar thật, khoá map là dữ liệu quest thật.

## Rủi ro xuyên phase

| Rủi ro | Ảnh hưởng | Giảm thiểu |
|--------|-----------|-----------|
| Bake 24 map một lúc gây khựng frame | Cao | Bake lười theo node lọt viewport, `MaxTextureSize` 128 cho thumbnail |
| Đổi wire-format TELE_MENU lệch 2 đầu | Cao | **Deploy server trước client**; test `Response_RejectsLegacyEntryWithoutLockReason` giữ hợp đồng |
| Toast một dòng không xuống dòng, câu lý do dài bị tràn | Trung bình | Nâng `ToastView` thành đa dòng (phase 04) |
| `MinimapWidget` chưa từng chạy thật trong HUD | Trung bình | Phase 05 + PlayMode test kiểm không đè `CharacterMenuButton` |
| `mapId` truyền bằng `sbyte` (≤127) | Thấp | Ghi giới hạn vào doc; hiện map cao nhất là 34 |

## Câu hỏi chưa chốt

1. **Bảng map ↔ nhiệm vụ**: cần danh sách `mapId → taskTemplateId` cụ thể. Phase 01
   ship *cơ chế* + bảng rỗng (hành vi không đổi) — điền dữ liệu sau khi chốt.
2. Có cần zoom (pinch/nút +/−) không? Plan hiện chỉ pan.
3. Có giữ hai nhóm hạ giới (11–25) / thượng giới (26–34) làm hai cụm tách rời trên
   canvas, hay trộn theo địa lý jar?
4. Nút minimap trên HUD đặt góc phải-trên có đè `CurrencyBar`/`ExpBuffIndicator` không
   — cần xem ảnh chụp thật ở phase 05.
