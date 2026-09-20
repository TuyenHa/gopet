# Phase 02 — Client: parse trường lý do khoá của TELE_MENU

## Context Links

- [plan.md](plan.md) · [phase-01](phase-01-server-map-unlock-rules.md)
- `GopetUnityClient/Assets/Scripts/Net/Map/MapTeleportHandler.cs` (57 dòng)
- `GopetUnityClient/tests/Gopet.Net.Tests/MapTeleportHandlerTests.cs`
- `GopetUnityClient/tests/Gopet.Net.LiveSmoke/TeleportMenuChecks.cs`

## Overview

- **Priority:** P1 (chặn phase 05)
- **Status:** pending · **Phụ thuộc:** phase 01 (wire-format)
- Đọc thêm một `UTF` lý do khoá sau cờ `Locked`, phơi ra `MapTeleportOption.LockReason`.

## Key Insights

- `OnMessage` (`MapTeleportHandler.cs:33-55`) đọc tuần tự từng trường rồi gọi
  `ExpectFullyConsumed("MGO_COMMAND/TELE_MENU")` — thêm trường mà quên đọc sẽ ném ngay,
  đây là lưới an toàn có sẵn, giữ nguyên.
- Test `Response_RejectsLegacyEntryWithoutLockedFlag` (`MapTeleportHandlerTests.cs:50`)
  đang khoá hợp đồng "server cũ phải làm client ném". Sau phase 01, hợp đồng đó đổi
  thành "thiếu chuỗi lý do cũng phải ném" → đổi tên + nội dung test, KHÔNG xoá.
- Guard `count > 100` (`:39`) vẫn đủ: 24 map thật.

## Requirements

**Chức năng**
- `MapTeleportOption` có `public string LockReason;`
- Parse đúng thứ tự: mapId(sbyte), Name(UTF), Description(UTF), WaypointIndex(sbyte),
  Locked(sbyte), LockReason(UTF).
- Map mở khoá → `LockReason` là chuỗi rỗng, không null.

**Phi chức năng**
- Không đổi chữ ký công khai khác (`RequestOptions`, `OptionsReceived`) — phase 05 đang dựa vào.

## Architecture

```
socket ─> MessageRouter ─> MapTeleportHandler.OnMessage
                                 └─> MapTeleportOption[] { .., Locked, LockReason }
                                          └─> OptionsReceived  (GameSession phase 05)
```

## Related Code Files

**Sửa**
- `GopetUnityClient/Assets/Scripts/Net/Map/MapTeleportHandler.cs`
- `GopetUnityClient/tests/Gopet.Net.Tests/MapTeleportHandlerTests.cs`
- `GopetUnityClient/tests/Gopet.Net.LiveSmoke/TeleportMenuChecks.cs`

**Tạo mới / Xoá:** không có.

## Implementation Steps

1. `MapTeleportHandler.cs`: thêm vào `MapTeleportOption` (sau `Locked`, dòng ~12):
   ```csharp
   /// <summary>Lý do khoá do server gửi (rỗng khi map mở). Client KHÔNG tự chế câu chữ.</summary>
   public string LockReason;
   ```
2. Trong vòng lặp `OnMessage` (`:44-51`) thêm `LockReason = message.Reader.ReadUtf()`
   ngay sau `Locked`. Giữ `ExpectFullyConsumed`.
3. Sửa comment XML của class (`:11`, `:15`) cho khớp: lý do khoá đến từ server
   (thượng giới hoặc nhiệm vụ), không chỉ "pet có cánh".
4. `MapTeleportHandlerTests.cs`:
   - `Response_ParsesAllMapFields`: thêm `.PutUtf("")` cho entry mở và
     `.PutUtf("Hãy chăm chỉ làm nhiệm vụ để mở map này")` cho entry khoá; assert
     `received[0].LockReason == ""` và `received[1].LockReason` đúng câu.
   - Đổi tên `Response_RejectsLegacyEntryWithoutLockedFlag` →
     `Response_RejectsLegacyEntryWithoutLockReason`, gói thiếu đúng chuỗi cuối, vẫn `ThrowsAny`.
5. `TeleportMenuChecks.cs`: thêm check `RR. TELE_MENU — lý do khoá đi kèm cờ khoá`:
   mọi option `Locked` phải có `LockReason` khác rỗng và mọi option mở phải rỗng.
   In kèm lý do vào dòng `Console.WriteLine` (`:52`) cho dễ soi.
6. Chạy `dotnet test GopetUnityClient/tests/Gopet.Net.Tests` và build LiveSmoke.

## Todo List

- [ ] 1. Thêm field `LockReason`
- [ ] 2. Đọc thêm `ReadUtf` trong `OnMessage`
- [ ] 3. Cập nhật comment XML
- [ ] 4. Sửa 2 unit test
- [ ] 5. Thêm check vào `TeleportMenuChecks`
- [ ] 6. Chạy test + build smoke

## Success Criteria

- `dotnet test GopetUnityClient/tests/Gopet.Net.Tests --nologo` → xanh toàn bộ.
- `pwsh GopetUnityClient/verify.ps1` qua bước LiveSmoke build và bước 10/10 (file vẫn < 200 dòng).
- Chạy smoke với server phase 01: check `NN/OO/OO2/PP/QQ/RR` đều xanh.

## Risk Assessment

| Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|--------|----------|-----------|-----------|
| Deploy client mới lên server cũ → mọi lần mở bản đồ đều ném | Trung bình | Cao | Test `Response_RejectsLegacyEntryWithoutLockReason` ghi rõ hợp đồng; quy trình deploy server trước |
| `ReadUtf` nhận null từ server nếu gửi `putUTF(null)` | Thấp | Trung bình | Phase 01 luôn gửi `string.Empty`, không bao giờ null |

**Rollback:** revert cùng lúc với phase 01 (cặp thay đổi protocol, không revert lẻ).

## Security Considerations

- `LockReason` là chuỗi server gửi và được hiển thị nguyên văn trong toast → chỉ dùng
  `Text` của uGUI (không rich text / không HTML) nên không có đường thực thi nội dung.
- Không tin `Locked` để quyết định quyền: client vẫn chỉ gửi warp, server chốt (phase 01 bước 5).

## Next Steps

- Phase 05 dùng `option.LockReason` thay câu hardcode ở `GameSession.Teleport.cs:33`.
