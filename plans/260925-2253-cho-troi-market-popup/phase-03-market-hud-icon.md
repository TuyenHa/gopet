---
phase: 3
title: Market HUD icon
status: completed
priority: P2
effort: 2h
dependencies: []
---

# Phase 3: Market HUD icon

## Overview
Tạo icon "Chợ trời" bằng `tools\image-gen` và thêm nút thứ 5 vào hàng HUD, đặt cạnh "Cửa hàng". Bấm vào gọi `UiRoot.OpenMarketPopup()` (phase 4).

## Key Insights (scout)
- Hàng HUD nằm ở `Runtime\UI\ShopServiceEventHud.cs`. Các nút xếp từ phải sang trái sau minimap theo thứ tự Mail, Event, Service, Shop (`:66-81`). `MakeButton(parent, font, sprite, label, right, onClick)` ở `:89-97`, kích thước `SizeFrac=0.075`, `GapFrac=0.006`.
- Sprite nạp qua `HudSkin` từ `Resources/Ui/Hud/<name>.png` (128×128, nền trong suốt, import Bilinear, không mipmap, uncompressed — `Editor\LoginAssetImportSettings.cs:22-35`).
- Wiring ở `Runtime\GopetBootstrap.cs:121-128`.
- Test layout `Tests\PlayMode\MinimapHudLayoutTests.cs` kiểm tra minimap không đè hàng HUD.
- Mẫu script ảnh: `tools\image-gen\gen-scene-button.py` dùng `images.edit` với ảnh tham chiếu để giữ đúng style.

## Requirements
- Icon cùng style với `shop.png`: cùng khung/viền, cùng độ dày nét. Hình là quầy/sạp chợ có mái che sọc và túi tiền. Label "Chợ trời" giống các nút khác (label do code vẽ, ảnh không chứa chữ).
- Nút đặt ngay bên trái "Cửa hàng", khoảng cách giống các nút khác.
- Hình sắc nét: xuất 128×128, downscale LANCZOS từ bản đã crop giống các icon HUD khác.

## Related Code Files
- Create: `tools\image-gen\gen-market-hud-icon.py`
- Create: `GopetUnityClient\Assets\Resources\Ui\Hud\market.png` (+ `.meta` do Unity sinh; importer tự áp setting)
- Modify: `GopetUnityClient\Assets\Scripts\Runtime\UI\HudSkin.cs` (`public const string Market = "market";`)
- Modify: `GopetUnityClient\Assets\Scripts\Runtime\UI\ShopServiceEventHud.cs` (event `MarketClicked`, gọi `MakeButton` sau Shop)
- Modify: `GopetUnityClient\Assets\Scripts\Runtime\GopetBootstrap.cs` (`hud.MarketClicked += () => _ui.OpenMarketPopup();`)
- Modify (nếu fail): `GopetUnityClient\Assets\Tests\PlayMode\MinimapHudLayoutTests.cs`

## Implementation Steps
1. Viết `gen-market-hud-icon.py` theo mẫu `gen-scene-button.py`:
   - Ảnh tham chiếu `Resources/Ui/Hud/shop.png`, gọi `images.edit`, model `gpt-image-2`, `background="transparent"`.
   - Prompt: "Redraw this HUD icon in exactly the same style, outline weight and palette, but replace the shop subject with a small open-air market stall: striped red-yellow awning, wooden counter with goods and a coin pouch. No text, transparent background."
   - Crop theo bbox, pad vuông, lưu `market.png` 128×128 và bản preview 512 ở `tools/image-gen/output/`.
2. Chạy script (cần `OPENAI_API_KEY` trong `tools/image-gen/.env`). Xem preview. Nếu chưa ưng thì chạy lại, script hỗ trợ `-n` hoặc chạy nhiều lần.
3. Thêm `HudSkin.Market` và nút trong `ShopServiceEventHud.Create`:
   ```csharp
   right += SizeFrac + GapFrac;
   MakeButton(go.transform, font, HudSkin.Market, "Chợ trời", right, () => view.MarketClicked?.Invoke());
   ```
4. Wire trong `GopetBootstrap`. Tạm để stub `OpenMarketPopup` nếu phase 4 chưa xong.
5. Mở Unity, compile, chạy `MinimapHudLayoutTests`. Kiểm tra ở 16:9 và 4:3 rằng nút không đè avatar/HUD nhân vật.

## Success Criteria
- [ ] `market.png` 128×128, nền trong suốt, style khớp `shop.png`.
- [ ] Nút "Chợ trời" hiện cạnh "Cửa hàng" sau khi login, bấm vào mở popup.
- [ ] `MinimapHudLayoutTests` pass, Unity compile 0 error.

## Risk Assessment
- Hàng HUD 5 nút có thể chạm HUD nhân vật trên màn hẹp. Nếu vậy thì giảm `GapFrac` hoặc `SizeFrac` chút ít và cập nhật test.
- Ảnh AI lệch style: dùng `images.edit` với ảnh tham chiếu, không dùng `generate`.
- API key: đọc từ `.env`, không commit.
