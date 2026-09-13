# Trạng thái thực thi parity certification

Ngày cập nhật: 2026-09-13.

## Kết luận

Phần audit và sửa code tự động đã hoàn tất, nhưng **chưa đủ bằng chứng để gắn nhãn
`certified`**. Phase 0–2 đã hoàn thành; Phase 3 bị chặn bởi Unity license mutex; Phase 4–5 cần
server, tài khoản fixture và thiết bị/UI reference đang không có trong phiên chạy này.

## Đã hoàn thành

- `verify.ps1`: **10/10 pass**.
- Unit test: **669/669 pass**.
- Net, Runtime Editor, Runtime, PlayMode test assembly và LiveSmoke: build thành công,
  **0 warning / 0 error**.
- Tách `ChatBubble`, `GameHud`, `MapWorldPlayModeTests`; không còn file C# vượt gate 200 dòng.
- Protocol coverage tự sinh: **71 handled, 1 intentionally-ignored, 2 server-unused, 0 missing**.
- Gate protocol đã được thêm vào bước 1 của `verify.ps1`; route mới chưa phân loại làm CI fail.
- Đã thêm handler/test cho:
  - opcode `100` — server từ chối đổi mật khẩu;
  - `PET_SERVICE/66` — public chat;
  - `PET_SERVICE/47` — material cường hóa bị ẩn sau `messagePetService(cmd)`.
- Đã thêm Guild fixture cho clan info/state, guild list, chat history/incoming và packet cắt ngắn.
- Đã thêm PlayMode fixture đổi VN/EN và kiểm tra persist `PlayerPrefs` (đã compile, chưa chạy).
- Đã xóa `MiniGamePackets` sai protocol và test stub liên quan.
- Type building 13–16 cùng 0–4, 7, 10, 19–21, 23 đã được khóa `Noop/JAR-inert`.

## Phân loại protocol có chủ đích

- `81/9` — `intentionally-ignored`: endpoint ảnh pet legacy; Unity dùng `COMMAND_IMAGE` 96.
- `81/65` — `server-unused`: chỉ nằm trong `testMsg65()`, call-site đã comment.
- `108` — `server-unused`: writer `Place`/`crossChat` legacy; active place flow override và không có
  external caller.

Chi tiết từng route và call-site nằm trong `protocol-coverage.generated.json`; quyết định ổn định
nằm trong `GopetUnityClient/tools/check-protocol-coverage/decisions.json`.

## Blocker còn lại

1. `run-playmode-tests.ps1` không sinh XML: Unity Licensing báo không lấy được global mutex do
   `Unity.Licensing.Client` của Unity Hub đang chạy. Batch process do audit tạo đã được dọn; không
   tự ý đóng Unity Hub/license process của người dùng.
2. Không có listener tại `127.0.0.1:19180`, nên LiveSmoke và kịch bản hai tài khoản chưa thể chạy.
   Server packet dump đang dirty từ trước audit và được giữ nguyên.
3. Không có JAR screenshot chuẩn, ba viewport/thiết bị thật và phiên nghe audio, nên Phase 5 chưa
   thể chứng nhận.
4. Đổi ngôn ngữ hiện refresh ngay màn Settings và bảng `JarStrings`; nhiều UI runtime vẫn chứa
   text Việt hard-code, nên chưa thể tuyên bố toàn bộ UI refresh VN/EN.

## Lệnh tiếp tục sau khi gỡ blocker

```powershell
cd D:\game\GopetUnityClient
powershell -ExecutionPolicy Bypass -File run-playmode-tests.ps1
dotnet run --project tests\Gopet.Net.LiveSmoke\Gopet.Net.LiveSmoke.csproj
```

Chỉ chuyển plan sang `certified` khi PlayMode XML xanh, LiveSmoke chạy trên server fixture không có
`OnUnhandled`, checklist hai tài khoản hoàn tất, và visual/audio/device evidence đã được lưu.
