# Phase 03 — Server: Logic điểm danh (calendar + reset tháng)

## Context
- Logic cũ: `GameController.noelDaily()` (GameController.cs:5277-5300) — streak 7 mốc, chống nhận 2 lần/ngày bằng so ngày `DailyNoelTime`.
- Phát quà: `player.controller.onReiceiveGift(int[][] giftData)` trả `JArrayList<Popup>` (GameController.cs:4060+). Dùng lại nguyên.
- Event pattern: class kế thừa `EventBase` trong `Data/Event/...` (mẫu `TeacherDay2024`, `GameBirthdayEvent`), có `Condition`, `Init()`, đăng ký NPC option.

## Overview
- **Priority**: cao.
- **Status**: chưa làm.
- Viết logic điểm danh theo ngày dương lịch + reset đầu tháng, dùng state phase-02 và data phase-01.

## Architecture
Tạo class `Data/Event/DailyCheckin/DailyCheckinEvent.cs` (EventBase). Chứa:
- Tham chiếu `GopetManager.DAILY_CHECKIN_GIFTS`.
- `Condition` = luôn bật (event thường trực) — hoặc theo cờ bật/tắt config.

### Hàm chính `DoCheckin(Player player)`
```
now = DateTime.Now
monthKey = now.Year*100 + now.Month
day = now.Day                     // 1..31
// 1) reset nếu sang tháng mới
if (player.playerData.DailyCheckinMonthKey != monthKey) {
    player.playerData.DailyCheckinMask = 0
    player.playerData.DailyCheckinMonthKey = monthKey
}
bit = 1 << (day - 1)
// 2) hôm nay đã nhận?
if ((mask & bit) != 0) { player.redDialog("Hôm nay đã điểm danh"); return }
// 3) phát quà ngày `day`
int[][] gift = DAILY_CHECKIN_GIFTS[day - 1]
popups = player.controller.onReiceiveGift(gift)
player.playerData.DailyCheckinMask |= bit
// 4) thông báo + log + gửi lại state cho client (phase-05)
player.okDialog(string.Join(",", popups.map(getText)))
HistoryManager.addHistory(...log ngày...)
SendCheckinState(player)          // phase-05
```

### Trạng thái từng ngày (cho client render — phase-05)
Với mỗi ngày `d` (1..daysInMonth):
- **RECEIVED**: `(mask & (1<<(d-1))) != 0`.
- **CLAIMABLE**: `d == today && !received`.
- **MISSED**: `d < today && !received`.
- **LOCKED**: `d > today`.
Helper `GetDayState(playerData, d)` trả enum → dùng cả khi build packet.

### Điểm quyết định
- **Lỡ ngày = mất quà** (MISSED không cho nhận). Đã chốt. Nếu sau muốn "lấy bù": bỏ điều kiện `d==today`, cho nhận mọi `d<=today` chưa nhận.
- Reset dựa `monthKey` → sang tháng, lần điểm danh đầu tiên tự reset mask. Không cần cron.

## Related Code Files
- Tạo: `GServer/Data/Event/DailyCheckin/DailyCheckinEvent.cs`.
- Sửa: `GServer/Manager/GopetManager.cs` (đăng ký instance nếu cần, giống các event khác).
- Dùng: `GameController.onReiceiveGift`, `HistoryManager`.
- (Trigger từ client: qua packet phase-05, không nhất thiết qua NPC option.)

## Todo
- [ ] Tạo `DailyCheckinEvent` + `DoCheckin`.
- [ ] Helper `GetDayState` + `DaysInMonth`.
- [ ] Ngôn ngữ: thêm chuỗi "Hôm nay đã điểm danh", "Điểm danh thành công" vào `LanguageData` (VI/EN).
- [ ] Compile.

## Success Criteria
- Điểm danh ngày X phát đúng quà `DAILY_CHECKIN_GIFTS[X-1]`, set bit, chặn nhận lại trong ngày.
- Sang tháng mới → mask reset, điểm danh lại từ ngày hiện tại.
- Ngày đã qua chưa nhận → không cho nhận (MISSED).

## Risk
- Lệch múi giờ server (`DateTime.Now`) — thống nhất dùng giờ server như code cũ.
- Ngày 29-31 ở tháng ngắn: `day` không bao giờ vượt `DaysInMonth` nên không truy cập ô thừa; nhưng bảng data vẫn cần đủ 31 phần tử (phase-01).
