---
phase: 3
title: Client hiệu ứng khung cảnh
status: completed
priority: P2
effort: 5h
dependencies:
  - 2
---

# Phase 3: Client hiệu ứng khung cảnh

## Overview
Tách lớp nền ra khỏi `BattleView.Build` thành component `BattleBackdrop` biết đổi ảnh nền và
chạy hiệu ứng động theo id khung cảnh. Hiệu ứng dùng **một bộ phát hạt chung** (UI `Image` +
coroutine/Update), cấu hình khác nhau cho từng khung cảnh.

## Key Insights
- Nền hiện ở `BattleView.cs:138-145` (GameObject "Nền", stretch, `raycastTarget=false`).
- Hiệu ứng skill đã vẽ bằng code trên canvas (`BattleFlameFallFx`, `BattleGroundSigil`), nên giữ cùng cách; không dùng ParticleSystem (khó xếp lớp với UI).
- Hạt phải nằm **ngay trên nền, dưới card pet và HUD**: đặt sibling index 1, giống `BattleGroundSigil`.
- Phóng sprite theo hằng số nguyên `BattleSkin.SpriteScale` để giữ nét pixel.
- File ≤ 200 dòng: tách catalog, bộ phát hạt, kiểu bay.

## Architecture
- `Gopet.UiLogic.BattleSceneCatalog` (thuần C#, test được): `id → (bgPath, AmbientKind)`; id lạ → 0.
- `BattleBackdrop : MonoBehaviour` — `Create(parent)`, `Apply(int sceneId)`: đổi sprite, huỷ hiệu ứng cũ, gắn hiệu ứng mới. Thiếu ảnh → rơi về `bg-forest`.
- `BattleAmbientFx : MonoBehaviour` — bộ phát hạt dùng chung, nhận `AmbientConfig`:
  sprites/khung, số hạt tối đa, tốc độ, rơi (fall) hay bay (wander), lắc ngang, xoay, vỗ cánh (đổi khung mỗi N giây), vùng xuất hiện. Pool Image cố định, không Instantiate mỗi frame.
- `BattleAmbientPresets` — cấu hình cho 5 khung cảnh:

| Khung cảnh | Kiểu | Ghi chú |
|---|---|---|
| Rừng cây che | wander | 3–4 bướm + 2 chuồn chuồn, bay đường cong, vỗ cánh 2 khung, chuồn chuồn lao ngắt quãng |
| Hoa anh đào | fall | ~25 cánh hoa, rơi chậm, lắc hình sin, xoay |
| Tuyết | fall | ~40 bông, 2 lớp cỡ (xa nhỏ-chậm, gần to-nhanh), lắc nhẹ |
| Hang động | tĩnh + wander | 3–4 dơi `bat-hang` treo cố định ở mép trên (đung đưa nhẹ) + 2–3 dơi bay, vỗ cánh nhanh |
| Mưa lửa | fall + tĩnh | tàn lửa rơi chéo, vài ngọn lửa nhỏ cháy nhấp nháy ở mép dưới/2 bên (không che vùng pet) |

## Related Code Files
- Create: `Assets/Scripts/UiLogic/BattleSceneCatalog.cs`
- Create: `Assets/Scripts/Runtime/World/Battle/BattleBackdrop.cs`, `BattleAmbientFx.cs`, `BattleAmbientPresets.cs`
- Modify: `Assets/Scripts/Runtime/World/BattleView.cs` (thay khối nền bằng `BattleBackdrop.Create`; thêm `ApplyScene(int)`)

## Implementation Steps
1. `BattleSceneCatalog` + test EditMode.
2. `BattleAmbientFx`: pool, spawn, update chuyển động (fall/wander), vỗ cánh, tái sinh khi ra khỏi màn hình; dùng `Time.unscaledDeltaTime`.
3. `BattleAmbientPresets` cho 5 khung cảnh.
4. `BattleBackdrop` + nối vào `BattleView.Build` (nhận sceneId ban đầu từ `BattleSceneState`, xem phase 4) cho **mọi** `BattleKind` (PvE, PvP, đấu trường).
5. Mở Unity, xem từng khung cảnh; chụp màn hình so với mockup.

## Success Criteria
- [ ] Mỗi khung cảnh có nền đúng + hiệu ứng mô tả; rừng mặc định không có hiệu ứng.
- [ ] Hạt không che card pet, HUD, nút; không chặn click.
- [ ] Đổi khung cảnh giữa trận không rò GameObject (số object ổn định sau 10 lần đổi).
- [ ] Không tụt FPS rõ rệt trên máy yếu (≤ 60 Image hạt).

## Risk Assessment
- Quá nhiều hạt gây rối mắt khi đang đánh: mật độ để trong preset, dễ chỉnh.
- Hạt đè số damage/banner: giữ sibling index thấp.
