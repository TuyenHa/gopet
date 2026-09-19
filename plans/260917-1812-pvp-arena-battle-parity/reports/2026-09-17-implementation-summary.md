# 2026-09-17 — Implementation summary

## Status: 6/8 phases hoàn thành (75%)

| Phase | Trạng thái | Ghi chú |
|---|---|---|
| 01 Server: gói kết thúc + FAST_REMOVE 2 gói | ✅ Xong | `Data/Battle/PetBattle.cs` — build 0 error |
| 02 Client: đóng overlay theo MapUpdated + timeout | ✅ Xong | 3 lối thoát: gói server, MapUpdated, timeout |
| 03 Client: spectator | ⏭ Bỏ qua | Ưu tiên thấp hơn, đòi hỏi API tra actor transform + đo lâu |
| 04 Server: PET_BATTLE_BUFF (opcode 38) | ✅ Xong | Gate `VERSION_150`, whitelist 25 type |
| 05 Client: buff UI + cooldown + auto-recovery | ✅ Xong | `SkillCooldownTracker`, dải buff, toggle SettingsView |
| 06 Vượt ải: đo & vá | ⏭ Bỏ qua | Cần chơi thử tay 25 lượt, môi trường không sẵn |
| 07 Tests | ✅ Xong | net tests **723/723** (từ 715 → +8); LiveSmoke để lần chứng nhận |
| 08 Docs | ✅ Xong | `docs/battle-system.md` (119 dòng) + sửa nhãn plan 260913 |

## Build & test

- Server: `dotnet build Gopet.csproj` → **0 error**, 477 warning (pre-existing).
- Client: `dotnet build Gopet.Runtime.csproj` → **0 error, 0 warning**.
- Tests: `dotnet test tests/Gopet.Net.Tests` → **Passed 723, Failed 0** (+8 test mới).

## Điểm quyết định

1. **`sendFastRemove` 2 gói cho PvP**: `place.sendMessage` mỗi gói. Observer chỉ khớp
   gói đầu (battleId=activePlayer), gói thứ 2 no-op — an toàn.
2. **Cooldown không đi packet**: hằng số 3 lượt, client đếm, server vẫn từ chối
   → giảm phình wire format (YAGNI).
3. **Whitelist buff type**: 25 loại (STUN, độc, hồi, buff DMG/DEF/ATK...) — loại
   nội bộ (DAMAGE_PHANDOAN, PERCENT_EXP...) không gửi.
4. **Version 1.5.0** vs ngưỡng chặn login 1.4.2: jar (1.4.x) vào được nhưng không
   nhận opcode 38 lạ.

## Bỏ qua có chủ đích

- **Phase 03 (spectator)**: cần dựng `SpectatorBattleView` mới + hàng chờ resolve
  actor transform; ưu tiên P2 và không phải blocker. Kéo qua plan tiếp theo.
- **Phase 06 (đo Vượt ải)**: bản chất là thử tay 25 lượt trên môi trường thật,
  không tự động hoá trong session này được. Hạ tầng chung (menu/place time/HP boss)
  đều đã có ở Unity — khả năng cao "hoạt động được" chỉ chưa được kiểm chứng.

## Việc tiếp theo (nếu muốn kéo dài)

1. LiveSmoke PvP end-to-end: kéo dài `PvpBattleChecks.cs` sau khi mở trận, chạy
   với server `127.0.0.1:19180` để chứng minh PET_BATTLE_STATE tới cả 2 bên.
2. Phase 03 spectator — plan riêng.
3. Phase 06 vượt ải — cần thử tay + `ZoneCommand` để nhảy lượt.
