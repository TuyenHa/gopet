---
phase: 3
title: "Client: xem trận người chơi khác trong world"
status: pending
priority: P2
effort: "1d"
dependencies: [2]
---

# Phase 3: Client — xem trận người chơi khác trong world

## Overview

Server broadcast mọi trận trong place cho tất cả người trong place đó, nhưng Unity
vứt hết gói không phải của mình. Ở đấu trường (tối đa 5 cặp cùng sân) người chơi
chỉ thấy sân trống. Phase này render trận của người khác **ngay trong world**, không
dùng overlay toàn màn hình.

## Key Insights

- `BattleCoordinator.cs:34`: `if (!start.IsParticipant) return;` — chặn thẳng.
  Comment trong code đã ghi nhận đây là khoảng trống có chủ ý, chưa làm.
- Server có sẵn đủ dữ liệu:
  - `sendStartFightPlayer()` broadcast gói của bên chủ động cho cả place trừ bên
    passive (`PetBattle.cs:479`, `place.sendMessage(message, listNoneSend)`).
  - `sendPetAttack()` broadcast gói lượt của bên chủ động cho cả place (`:355`).
  - `sendPetBattleList()` (`Place/GopetPlace.cs:675`) gửi snapshot **mọi trận đang
    chạy** cho người mới vào place → vào giữa chừng vẫn thấy.
- **Hệ quả quan trọng**: observer chỉ nhận được luồng của bên **chủ động**
  (`activePlayer`). Gói của bên passive gửi riêng (`passivePlayer.session.sendMessage`).
  Nghĩa là observer thấy đủ HP/MP của cả 2 pet (vì effect mang `petId` là mục tiêu),
  nhưng chỉ có **một** `battleId`. Không cần server đổi gì.
- `BattleHandler.OnPlayerBattle` đã xử lý snapshot thiếu cờ cuối
  (`if (r.Remaining == 1) r.ReadBool();`) — chứng tỏ path observer đã được reverse.
- `BattleStart.IsParticipant` đã tính đúng, chỉ cần dùng thay vì bỏ gói.

## Requirements

**Functional**
- Trận của người khác hiển thị tại vị trí 2 pet trên map: thanh HP/MP, số damage
  bay lên, hiệu ứng skill — **không** có nút hành động, **không** che màn hình.
- Vào place giữa chừng (snapshot qua `sendPetBattleList`) vẫn dựng được view.
- Nhiều trận cùng lúc (đấu trường 5 cặp) render song song, không rò rỉ GameObject.
- Trận kết thúc / người chơi rời place → view tương ứng biến mất.

**Non-functional**
- Chi phí: giới hạn số spectator view đồng thời (đề xuất 5, đúng `ArenaPlace` cap).
- Không đụng vào `BattleView` (overlay của chính mình) để tránh regression.

## Architecture

Tách vòng đời thành 2 loại view, cùng một coordinator:

```
BattleCoordinator
├── _own : BattleView              (overlay, chỉ khi IsParticipant)  ── đã có
└── _spectators : Dictionary<int battleId, SpectatorBattleView>      ── mới
```

`SpectatorBattleView` là MonoBehaviour gắn vào world transform (không phải Canvas
overlay), tái dùng:
- `BattlePetCard` cho HP/MP (dựng nhỏ hơn, anchor theo vị trí pet trên map)
- `BattleEffectView` / `BattleActorEffectView` cho hiệu ứng
- `BattleFloatText` cho số damage

**Không** viết lại logic parse — dùng nguyên `BattleStart`/`BattleTurn`/`BattleResult`.

Vị trí đặt view: `BattleStart` không mang toạ độ. Lấy từ entity trong world qua
`ActorId` (`WorldObjectHandler`/`MapScene` đã giữ player theo userId). Nếu không tìm
thấy actor (chưa spawn) → hoãn tới frame sau, quá 2s thì bỏ qua trận đó.

## Related Code Files

- Create: `GopetUnityClient/Assets/Scripts/Runtime/World/SpectatorBattleView.cs` (<200 dòng)
- Modify: `GopetUnityClient/Assets/Scripts/Runtime/World/BattleCoordinator.cs`
- Modify: `GopetUnityClient/Assets/Scripts/Runtime/World/BattlePetCard.cs` (thêm chế độ compact nếu cần)
- Read for context: `Assets/Scripts/Runtime/World/MapScene.cs`,
  `Assets/Scripts/Net/Map/WorldObjectHandler.cs`, `Assets/Scripts/Runtime/World/BattleView.cs`

## Implementation Steps

1. Đọc `MapScene` + `WorldObjectHandler` để xác định API tra actor theo `userId`
   và transform của họ trong world. Ghi lại API chính xác vào phase file này trước
   khi code (nếu khác giả định trên, điều chỉnh bước 4).
2. Tách `BattleCoordinator`:
   - `OnStarted`: nếu `IsParticipant` → như cũ; ngược lại → `EnsureSpectator(start)`.
   - `OnTurn` / `OnEnded` / `OnRemoved`: route theo `BattleId` sang `_own` hoặc
     `_spectators`.
   - `OnPlaceChanged` (phase 02): xoá **toàn bộ** spectator view.
3. Viết `SpectatorBattleView.Create(worldParent, start, resolveActorTransform)`:
   - 2 thẻ HP/MP nhỏ neo trên đầu mỗi pet.
   - `Apply(BattleTurn)`: y hệt `BattleView.Apply` nhưng bỏ phần `UnlockActions`.
   - `ShowResult(BattleResult)`: float text ngắn rồi tự huỷ sau ~2s.
4. Giải quyết định vị: nếu `resolveActorTransform(actorId)` trả null → giữ trong
   hàng chờ, thử lại mỗi frame, quá 2s thì bỏ (log warning, không throw).
5. Giới hạn 5 spectator view đồng thời; vượt thì bỏ qua trận mới nhất và log.
6. Dọn dẹp: `Close()` phải huỷ mọi GameObject con; viết PlayMode test đếm
   `GameObject` trước/sau để bắt rò rỉ.
7. `verify.ps1` + PlayMode.

## Todo List

- [ ] Xác định API tra actor theo userId trong `MapScene`
- [ ] Refactor `BattleCoordinator` thành own + spectators
- [ ] `SpectatorBattleView.cs` (<200 dòng)
- [ ] Hàng chờ định vị + timeout 2s
- [ ] Cap 5 view đồng thời
- [ ] PlayMode test chống rò rỉ GameObject
- [ ] `verify.ps1` pass

## Success Criteria

- [ ] Đứng trong đấu trường, thấy thanh HP + số damage của các cặp khác.
- [ ] Vào place giữa chừng (snapshot `sendPetBattleList`) vẫn dựng được view.
- [ ] 5 trận cùng lúc: không lỗi, không tụt frame đáng kể.
- [ ] Trận kết thúc / rời place → không còn GameObject nào sót (test đếm).
- [ ] Overlay của chính mình không đổi hành vi.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Không tra được transform của actor (chưa spawn / ngoài view) | Cao | Hàng chờ + timeout 2s + bỏ qua êm, không throw |
| Rò rỉ GameObject khi nhiều trận nối tiếp | Cao | PlayMode test đếm object; `Close()` huỷ đệ quy |
| Observer chỉ có 1 `battleId` → nhầm khi 2 trận chung 1 chủ động | Thấp | Server chặn 1 người 1 trận (`getPetBattle() != null`) |
| Trận của người khác không bao giờ nhận `PET_BATTLE_STATE` (đấu trường trước phase 01) | Trung bình | Phase 01 sửa gốc; timeout của phase 02 áp cho cả spectator |
| Phình `BattleCoordinator` quá 200 dòng | Trung bình | Tách `SpectatorBattleView` sang file riêng, coordinator chỉ route |

## Security Considerations

- Chỉ render dữ liệu server đã chủ động broadcast — không lộ thêm thông tin nào
  mà jar không thấy.
- Cap số view chặn DoS phía client nếu server (hoặc server giả) spam `PLAYER_BATTLE`.
- Mọi `count` đọc từ gói đã có guard `Count()` trong `BattleHandler` — giữ nguyên.

## Next Steps

→ Phase 06 dùng lại spectator view cho map vượt ải (nhiều người cùng đánh mob).
