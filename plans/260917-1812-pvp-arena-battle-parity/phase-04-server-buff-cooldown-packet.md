---
phase: 4
title: 'Server: đẩy buff/debuff & cooldown skill'
status: completed
priority: P2
effort: 1d
dependencies: []
---

# Phase 4: Server — đẩy trạng thái buff/debuff & cooldown skill

## Overview

Server tính cả một hệ thống buff/debuff/stun/độc/phản đòn/cooldown nhưng không gửi
gì cho client ngoài delta HP/MP. Người chơi bấm skill và bị từ chối bằng dialog đỏ
"Chưa hồi kỹ năng xong" mà không có cách nào biết trước. Phase này thêm **một**
opcode mới, gate theo version để jar cũ không bị ảnh hưởng.

## Key Insights

- Cooldown: `MAX_SKILL_COOLDOWN = 3` (`Manager/GopetManager.cs:405`) là **hằng số**.
  Client hoàn toàn suy được: bấm skill thành công → khoá 3 lượt.
  → **Quyết định: KHÔNG gửi cooldown qua packet.** Client tự đếm (phase 05). YAGNI.
  Server vẫn là nguồn chân lý — nếu client đoán sai, server từ chối như hiện tại.
- Buff/debuff thì **không suy được**: nguồn rất đa dạng — `ApplyHiddenStat` (`:75`),
  `addWingBuff` (`:107`), skill (`applySkill` `:1425`), vật phẩm (`useItem` `:1603`),
  và stun ngẫu nhiên khi đánh trúng (`petAttack` `:283`). Bắt buộc phải gửi.
- `PetBattleInfo.getBuffs()` trả `JArrayList<Buff>`; `Buff` có `ItemInfo[]` + số lượt
  còn lại (`Data/Battle/Buff.cs`). `nextTurn()` (`PetBattle.cs:697`) đã giảm lượt.
- Điểm chèn tự nhiên: cuối `nextTurn()`, ngay sau `sendMyPetInfo()` — lúc đó buff đã
  tick xong và cả 2 bên đều được cập nhật.
- **Gate version**: `Player.ApplicationVersion` parse từ `CLIENT_INFO`
  (`Server/Player.cs:109`). Jar báo ≤ `1.4.2` (`VERSION_142` là ngưỡng chặn hiện tại),
  Unity báo `1.4.3` (`Assets/Scripts/Net/Auth/ClientInfo.cs:19`).
  → Thêm `VERSION_150 = 1.5.0`, Unity nâng lên `1.5.0`, opcode mới chỉ gửi khi
  `ApplicationVersion >= VERSION_150`. Jar tuyệt đối không nhận gói lạ.

## Requirements

**Functional**
- Sau mỗi lượt, mỗi người trong trận nhận danh sách buff của **cả hai** pet:
  `{actorId, [{typeId, value, turnsLeft}]}`.
- Chỉ gửi cho client >= 1.5.0.
- Snapshot lúc mở trận cũng gửi (người vào giữa chừng thấy đúng trạng thái).

**Non-functional**
- Không đổi byte nào của opcode cũ.
- Gói phải có giới hạn số phần tử để client validate được.

## Architecture

Opcode mới: `PET_BATTLE_BUFF` — chọn một sbyte **chưa dùng** trong `GopetCMD.cs`
(phải grep xác nhận, không đoán). Là sub-command của `PET_SERVICE` như các gói battle khác.

```
PET_SERVICE
  └── PET_BATTLE_BUFF (sbyte mới)
        int   battleId
        sbyte actorCount            (0..2)
        ├─ int   actorId
        │  sbyte buffCount          (0..32)
        │  └─ int   typeId
        │     int   value
        │     sbyte turnsLeft
```

Gửi ở cuối `PetBattle.nextTurn()` và trong `sendBattleInfo()`/`sendStartFightPlayer()`.
`battleId` dùng đúng quy ước sẵn có: gói gửi cho ai thì `battleId` = userId của người đó.

**Không** gửi cooldown trong gói này (xem Key Insights). Nếu sau này phát hiện client
đoán sai cooldown trong ca thật, mở rộng gói ở plan khác.

## Related Code Files

- Modify: `SRCGOPETGOC/GServer/Server/GopetCMD.cs` (hằng số opcode mới)
- Modify: `SRCGOPETGOC/GServer/Manager/GopetManager.cs` (`VERSION_150`)
- Modify: `SRCGOPETGOC/GServer/Data/Battle/PetBattle.cs` (`nextTurn`, `sendBattleInfo`, `sendStartFightPlayer`)
- Read for context: `SRCGOPETGOC/GServer/Data/Battle/Buff.cs`, `PetBattleInfo.cs`,
  `Data/GopetItem/ItemInfo.cs` (bảng `ItemInfo.Type`), `Server/Player.cs:100-130`

## Implementation Steps

1. `grep` toàn bộ `GopetCMD.cs` liệt kê sbyte đã dùng trong không gian `PET_SERVICE`,
   chọn giá trị trống. **Ghi lại giá trị đã chọn vào phase file này.**
2. Thêm `VERSION_150` vào `GopetManager.cs`. **Không** đổi ngưỡng chặn login
   (`VERSION_142`) — jar cũ vẫn phải vào được.
3. Viết `PetBattle.sendBuffState(Player target)`:
   - Dựng `Message` theo layout trên từ `activeBattleInfo` / `passiveBattleInfo`.
   - Clamp `buffCount` ≤ 32; nếu vượt, cắt và log warning.
   - Bỏ qua nếu `target.ApplicationVersion < GopetManager.VERSION_150`.
4. Gọi `sendBuffState` ở:
   - cuối `nextTurn()` cho `activePlayer` và `passivePlayer` (nếu có),
   - cuối `sendStartFightPlayer()` cho cả 2,
   - cuối `sendBattleInfo(playerInZone)` cho người vừa vào place.
5. Rà `ItemInfo.Type` để chốt tập `typeId` cần hiển thị (stun, độc, phản đòn, buff
   damage/def, hồi máu...). Loại bỏ type nội bộ không có nghĩa với người chơi
   (ví dụ `DAMAGE_PHANDOAN` là giá trị trung gian). Ghi bảng ánh xạ vào report.
6. Build server, chụp packet dump một trận có stun + độc.

## Todo List

- [ ] Chọn và ghi lại opcode `PET_BATTLE_BUFF`
- [ ] `VERSION_150` (không đổi ngưỡng chặn login)
- [ ] `sendBuffState()` + gate version + clamp
- [ ] Gọi ở 3 điểm: `nextTurn`, `sendStartFightPlayer`, `sendBattleInfo`
- [ ] Bảng ánh xạ `ItemInfo.Type` → nhãn hiển thị (ghi ra report)
- [ ] Packet dump trận có stun/độc
- [ ] Build 0 error

## Success Criteria

- [ ] Client 1.5.0 nhận gói buff sau mỗi lượt, đúng số buff server đang giữ.
- [ ] Client 1.4.x (jar) **không** nhận gói nào mới — xác minh bằng dump khi login jar.
- [ ] Người vào place giữa chừng nhận snapshot buff đúng.
- [ ] Không opcode cũ nào đổi byte layout.
- [ ] Build server 0 error.

## Risk Assessment

| Rủi ro | Mức | Giảm thiểu |
|---|---|---|
| Chọn trùng opcode đang dùng → hỏng gói khác | Cao | Bắt buộc grep liệt kê trước, ghi lại bằng chứng; thêm test gate protocol hiện có |
| Jar nhận gói lạ và crash | Cao | Gate `ApplicationVersion >= 1.5.0`; test đăng nhập bằng jar thật và đọc dump |
| Tăng lưu lượng mỗi lượt | Thấp | Gói nhỏ (~vài chục byte), 1 lượt/25s |
| `ItemInfo.Type` quá nhiều, UI không tải nổi | Trung bình | Lọc whitelist ở bước 5, không gửi type nội bộ |
| Quên gate ở một trong 3 điểm gọi | Trung bình | Gate nằm **bên trong** `sendBuffState`, không ở call site |

## Security Considerations

- Gói chỉ đi một chiều server→client, không nhận input mới → không mở bề mặt tấn công.
- **Không** lộ thông tin ẩn của đối thủ vượt quá thứ JAR đã thấy: chỉ gửi buff đang
  hiệu lực trong trận, không gửi chỉ số ẩn (`ApplyHiddenStat`) dạng thô.
- Clamp `buffCount` chặn gói khổng lồ nếu state server bị lỗi.

## Next Steps

→ Phase 05 dựng UI tiêu thụ gói này + tự đếm cooldown.
