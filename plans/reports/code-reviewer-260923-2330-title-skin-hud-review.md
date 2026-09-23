# Code review — danh hiệu, chuẩn hoá skin, HUD/minimap (2026-09-23)

Phạm vi: PlayerAvatar, AvatarAppearance, CharacterSkinView, JarNameLabel, CharacterAnimationView/Layer, MinimapWidget, NotificationTicker, CharacterHud/GameSession + test liên quan.

## Đã sửa
- **H1** Ảnh chờ 1x1 của `RemoteAssetCache` (gọi đồng bộ khi cache miss) bị coi là skin đã tải → tên/danh hiệu rơi xuống chân, cánh ×1.15 tới khi ảnh thật về. Fix: `CharacterSkinView` bỏ qua `assets.Placeholder`.
- **H2** Danh hiệu (sort 31990/32010) đè lên chữ bong bóng chat (20000/20001, y=84). Fix: danh hiệu sort 19990/19995 — trên mọi actor (10000+y), dưới bong bóng.
- **M1** `JarNameLabel.VisibleTopIn`: renderer tắt/ẩn trả bounds rỗng ở gốc world. Fix: guard enabled/activeInHierarchy + `IsNullOrWhiteSpace`.
- **M2** `ApplySkin` giờ gỡ `Loaded` của skin cũ trước khi huỷ.

## Chưa làm / ghi nhận
- M3: chưa có PlayMode test cho luồng skin tải bất đồng bộ (cần stub RemoteAssetCache).
- L4: lề phải minimap 0.003 nhưng `ShopServiceEventHud` vẫn chừa `ReservedRightFrac` 0.012 → khe minimap–nút rộng hơn (chỉ thẩm mỹ).
- L5 (có sẵn từ trước): `GuildNameLayer` cố định y=50 (đè thân khi có skin); nhiều danh hiệu cùng lúc chồng cùng mốc; DB `IsVertically=1` nhưng client cắt frame ngang (ảnh thực tế là 2 frame ngang, nên hiện đúng).
- Chiều cao danh hiệu phụ thuộc nét chữ từng tên (tên có dấu cao → danh hiệu cao hơn chút).

## Kiểm chứng
- `verify.ps1`: 10/10 OK (942 unit test, compile Runtime/Editor/PlayMode/LiveSmoke, rule 200 dòng).
- PlayMode test mới (`CharacterAnimationViewTests`, `CharacterSkinScaleTests`) mới chỉ compile, chưa chạy — Unity Editor đang giữ lock project.

## Câu hỏi còn mở
- Có muốn danh hiệu cố định độ cao (đo theo chuỗi mẫu) thay vì theo nét chữ từng tên?
