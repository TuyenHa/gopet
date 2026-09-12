---
title: "Phase 2: Test & Verify"
status: pending
---

# Phase 2: Test & Verify

## Overview

- **Priority**: P1
- **Status**: Pending
- Xác nhận end-to-end: bấm building trên map → dialog hiện → chọn → chuyển map/zone thành công

## Implementation Steps

### 1. Compile check

```bash
cd GopetUnityClient
dotnet build tests/Gopet.Net.Tests/Gopet.Net.Tests.csproj
```

### 2. Unit test (nếu cần)

Thay đổi ở Phase 1 rất nhỏ (nối event → gọi method đã có), không cần test mới.
Logic channel/teleport đã có test từ plan 260907.

### 3. Manual test với GServer

1. Chạy GServer + Unity client
2. Đăng nhập → vào map 11 (TP Linh Thú)
3. Tìm building "Khu" trên map → bấm → phải hiện dialog chọn khu vực
4. Chọn khu → phải chuyển zone thành công (avatar respawn, danh sách người chơi mới)
5. Tìm building "Phòng vé" trên map → bấm → phải hiện dialog chọn map dịch chuyển
6. Chọn map → phải chuyển map thành công (fade out → load map mới → fade in → avatar spawn)
7. Kiểm tra các building khác (shop, gym...) vẫn hoạt động như cũ

### 4. Edge cases

- Bấm portal khi đang trong dialog → dialog phải tự đóng hoặc block
- Bấm building liên tục nhanh → throttle phải chặn
- Chuyển map khi đang battle → server reject (đã xử lý server-side)

## Todo List

- [ ] Compile pass
- [ ] Manual test ChangeZone qua building
- [ ] Manual test Teleport qua building
- [ ] Kiểm tra regression building khác

## Success Criteria

- Không lỗi compile
- Bấm "Khu" building → dialog zone → chọn → chuyển zone thành công
- Bấm "Phòng vé" building → dialog teleport → chọn map → chuyển map thành công
- Không regression các building/portal/menu khác
