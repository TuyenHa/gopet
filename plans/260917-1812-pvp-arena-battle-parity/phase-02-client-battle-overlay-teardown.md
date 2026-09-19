---
phase: 2
title: 'Client: đóng overlay an toàn theo place'
status: completed
priority: P1
effort: 4h
dependencies:
  - 1
---

# Phase 2: Client — đóng overlay battle an toàn theo vòng đời place

## Overview

`BattleCoordinator` hiện chỉ đóng `BattleView` khi nhận `PET_BATTLE_STATE` hoặc
`FAST_REMOVE_MOB`. Nếu thiếu cả hai (đấu trường trước phase 01, mất gói, reconnect)
thì overlay treo vĩnh viễn và chặn toàn bộ input map. Phase này thêm hai lối thoát
độc lập với gói kết thúc.

## Key Insights

- `BattleCoordinator` (`Assets/Scripts/Runtime/World/BattleCoordinator.cs:20-24`) chỉ
  đăng ký 4 event của `BattleHandler`, không biết gì về map/place.
- Server **luôn** gọi `petBattle.Close(player)` trước khi chuyển player sang place
  khác (`Place/GopetPlace.cs:48` trong `add()`, `:76` trong `remove()`,
  `Place/Place.cs:41`). Sau đó gửi `loadInfo()` → opcode 29 `ON_UPDATE_PLAYER_IN_MAP`.
- Unity đã parse opcode đó thành `MapHandler.MapUpdated`
  (`Assets/Scripts/Net/Map/MapHandler.cs:35`). Đây là tín hiệu "đã sang place mới"
  đáng tin cậy nhất và **không cần server đổi gì**.
- `OnFastRemove` (`BattleHandler.cs:181`) so `battleId` với `BattleStart.BattleId`.
  Với người bị thách đấu, `BattleId` = userId của chính họ, còn gói mang userId của
  người chủ động → không khớp. Sau phase 01 server gửi 2 gói, nhưng client vẫn nên
  chấp nhận cả `LocalPet.ActorId` lẫn `Opponent.ActorId` để bền với server cũ.

## Requirements

**Functional**
- `BattleView` tự đóng khi nhận `MapUpdated` (đổi place/map).
- `BattleRemoved` khớp khi `battleId` trùng `LocalPet.ActorId` **hoặc** `Opponent.ActorId`.
- Timeout cứng: không nhận gói lượt/kết thúc nào trong N giây → đóng overlay + toast.

**Non-functional**
- Đóng overlay phải gọi `_setBattleMode(false)` để trả input về map (đã có sẵn).
- Không đóng nhầm khi trận vẫn đang chạy: `MapUpdated` chỉ bắn khi thực sự vào place mới.

## Architecture

```
BattleHandler ──BattleStarted/Turn/Ended/Removed──► BattleCoordinator ──► BattleView
MapHandler    ──MapUpdated───────────────────────►      │ (mới)
BattleView    ──hết timeout──────────────────────►      │ (mới)
                                                        └──► Close() → setBattleMode(false)
```

Coordinator nhận thêm một `Action` đăng ký từ `GameSession` (không tự tham chiếu
`MapHandler` để giữ hướng phụ thuộc một chiều như hiện tại).

Timeout đặt ở `BattleCoordinator`, không ở `BattleView`, để logic vòng đời gom một chỗ.
Giá trị: `TurnDurationMs × 3` (server `TimeNextTurn = 25s` → ~75s), reset mỗi gói lượt.
Trận đấu trường tối đa 2 phút nên ngưỡng này an toàn.

## Related Code Files

- Modify: `GopetUnityClient/Assets/Scripts/Runtime/World/BattleCoordinator.cs`
- Modify: `GopetUnityClient/Assets/Scripts/Net/Battle/BattleHandler.cs` (`OnFastRemove` matching)
- Modify: `GopetUnityClient/Assets/Scripts/Runtime/World/GameSession.cs` (~dòng 155-160, nối `MapUpdated`)
- Read for context: `GopetUnityClient/Assets/Scripts/Net/Map/MapHandler.cs`,
  `GopetUnityClient/Assets/Scripts/Runtime/World/BattleView.cs`

## Implementation Steps

1. Đọc `BattleCoordinator.cs`, `GameSession.cs:130-170`, `MapHandler.cs` trước khi sửa.
2. `BattleHandler.OnFastRemove`: giữ nguyên parse, nhưng đổi ngữ nghĩa event thành
   "battleId bị huỷ" và để `BattleCoordinator` quyết định khớp — sửa `OnRemoved`:
   ```csharp
   private void OnRemoved(int battleId)
   {
       if (_view == null) return;
       if (_view.BattleId == battleId || _view.OpponentActorId == battleId) Close();
   }
   ```
   Thêm `public int OpponentActorId => _start.Opponent.ActorId;` vào `BattleView`.
3. Thêm `public void OnPlaceChanged()` vào `BattleCoordinator` → gọi `Close()`.
4. Ở `GameSession.Create`, sau khi dựng `_battle`, nối:
   `s._mapHandler.MapUpdated += _ => s._battle.OnPlaceChanged();`
   (dùng đúng tên field của `MapHandler` trong `GameSession`).
   **Kiểm tra thứ tự**: `MapUpdated` cũng bắn lần đầu lúc vào game — lúc đó `_view`
   là null nên `Close()` no-op, an toàn.
5. Thêm timeout vào `BattleCoordinator`:
   - `BattleView` expose `public float LastPacketAt` hoặc coordinator tự giữ timestamp,
     cập nhật ở `OnStarted` và `OnTurn`.
   - Cần một tick: `BattleView.Update()` đã chạy sẵn — cho `BattleView` bắn event
     `Stalled` khi quá hạn, coordinator bắt và `Close()`. Giữ state ở coordinator,
     chỉ mượn `Update()` của view làm nhịp (KISS, không thêm MonoBehaviour mới).
6. Khi đóng vì timeout hoặc đổi place mà chưa có kết quả: hiện toast
   "Trận đấu đã kết thúc." qua `GameSession.ShowToast` thay vì đóng im lặng.
7. Chạy `verify.ps1` và `run-playmode-tests.ps1`.

## Todo List

- [ ] Đọc 4 file liên quan
- [ ] `BattleView.OpponentActorId` + `OnRemoved` khớp 2 chiều
- [ ] `BattleCoordinator.OnPlaceChanged()`
- [ ] Nối `MapUpdated` ở `GameSession`
- [ ] Timeout `TurnDurationMs × 3` + toast
- [ ] `verify.ps1` pass, PlayMode pass

## Success Criteria

- [ ] Kết thúc trận đấu trường (kể cả khi server CHƯA có phase 01) → overlay đóng,
      input map trả lại được.
- [ ] `FAST_REMOVE_MOB` đóng overlay ở cả bên chủ động lẫn bên bị thách đấu.
- [ ] Ngắt mạng giữa trận → sau ~75s overlay tự đóng kèm toast, không treo.
- [ ] Trận bình thường không bị đóng sớm: đánh đủ 5 lượt PvE không rơi overlay.
- [ ] Không có test nào đang pass bị hỏng.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| `MapUpdated` bắn giữa trận đang chạy → đóng nhầm | Cao | Xác minh bằng packet dump: server chỉ gửi opcode 29 trong `loadInfo()` khi vào place mới, và luôn `Close()` battle trước đó. Viết PlayMode test cho đúng ca này |
| Timeout quá ngắn với lag cao | Trung bình | Dùng `TurnDurationMs` server gửi (không hardcode), nhân 3 |
| Đóng overlay nhưng `_setBattleMode(false)` không chạy → input kẹt | Trung bình | `Close()` đã gọi; thêm assert trong PlayMode test |
| Double-close khi cả `MapUpdated` và `PET_BATTLE_STATE` cùng tới | Thấp | `Close()` đã guard `_view != null` |

## Security Considerations

- Không tin `battleId` từ server để cấp quyền gì — chỉ dùng để khớp view.
- Timeout là quyết định thuần client, không gửi packet nào → không tạo đường cho
  client giả kết thúc trận (server vẫn giữ `PetBattle` thật).

## Next Steps

→ Phase 03 (spectator) dùng chung vòng đời này cho các view phụ.
