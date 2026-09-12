---
title: "Phase 1: Wire Building → Handler"
status: done
---

# Phase 1: Wire Building → Handler

## Overview

- **Priority**: P1
- **Status**: Pending
- Nối building entity bấm trên map → handler đổi kênh / dịch chuyển map đã có sẵn

## Key Insights

- `GameSession.Interactions.cs:41-43` xử lý `LocalMenu` chỉ hiện toast — cần thêm
  case cụ thể cho `ChangeZone` và `TicketRoom`
- `ChannelHandler.RequestChannels()` + `OnChannelsReceived()` đã chạy từ character menu
- `MapTeleportHandler.RequestOptions()` + `OnTeleportOptionsReceived()` đã chạy từ character menu
- JAR eg.java: building type 11 → trực tiếp request channel info (không confirm)
- JAR eg.java: building type 8 → show YES/NO dialog "Bạn có muốn mua vé?" → nếu OK request teleport list

## Related Code Files

### Files cần sửa
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Interactions.cs` — thêm case `ChangeZone`, `TicketRoom`

### Files tham khảo (chỉ đọc)
- `GopetUnityClient/Assets/Scripts/UiLogic/BuildingDispatcher.cs` — enum `LocalMenu`
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs` — `_channelHandler`, `_mapTeleportHandler`
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Channels.cs` — `OnChannelsReceived`
- `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.Teleport.cs` — `OnTeleportOptionsReceived`
- `client.jar_Decompiler.com/eg.java` — line 208-260, building action handler

## Implementation Steps

### 1. Sửa `GameSession.Interactions.cs` — xử lý LocalMenu cụ thể

Trong method `OnBuildingSelected`, thay vì xử lý tất cả `LocalMenu` bằng toast chung,
thêm case riêng:

```csharp
case BuildingAction.Kind.LocalMenu:
    switch (action.Menu)
    {
        case BuildingDispatcher.LocalMenu.ChangeZone:
            _channelHandler.RequestChannels();
            break;
        case BuildingDispatcher.LocalMenu.TicketRoom:
            _mapTeleportHandler.RequestOptions();
            break;
        default:
            ShowToast($"'{label}' — chưa mở trong Unity (menu local, chờ phase kế).");
            break;
    }
    break;
```

### 2. (Optional) Confirm dialog cho TicketRoom

JAR hiện dialog YES/NO trước khi mở teleport list. Nếu muốn parity:
- Hiển thị `ChoiceDialogView` "Bạn có muốn xem bản đồ dịch chuyển?" → YES → `RequestOptions()`
- Tuy nhiên **có thể bỏ qua** bước confirm vì character menu cũng gọi thẳng
  `RequestOptions()` mà không confirm → giữ UX nhất quán

### 3. Throttle check

Building click đã có throttle `_actionThrottle.TryAcquire` cho `Kind.Send` — cân nhắc
thêm throttle cho ChangeZone/TicketRoom để tránh spam request. Dùng cùng mechanism:

```csharp
if (!_actionThrottle.TryAcquire($"building:{entity.BuildingType}", 500, out var ms))
{
    ShowToast($"Thao tác quá nhanh, thử lại sau {ms} ms.");
    return;
}
```

## Todo List

- [x] Sửa `OnBuildingSelected` switch `LocalMenu` → ChangeZone gọi `_channelHandler.RequestChannels()`
- [x] Sửa `OnBuildingSelected` switch `LocalMenu` → TicketRoom gọi `_mapTeleportHandler.RequestOptions()`
- [x] Thêm throttle cho 2 case trên
- [x] Compile check (dotnet build)

## Success Criteria

- Bấm building "Khu" trên map → hiện dialog chọn khu vực (y như bấm menu Channels)
- Bấm building "Phòng vé" trên map → hiện dialog chọn map dịch chuyển (y như menu Teleport)
- Các building khác vẫn hiện toast "chưa mở" như cũ
- Không lỗi compile
