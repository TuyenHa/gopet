# Live-smoke — Character Creation Screen (Phase 5)

Ngày: 2026-09-11.

## 1. Unit test (thuần C#)

`LoginFlowCharacterTests.cs` (đã có sẵn, xác nhận lại xanh trong lần chạy `dotnet test
tests/Gopet.Net.Tests` — 625/625 PASS) đã phủ đúng các kịch bản kế hoạch yêu cầu:

- `ChuaCoNhanVat_ChuyenSangManTao` — chuyển màn khi `playerData == null`.
- `TaoNhanVatXong_ServerDong_ThiNoiLaiChuKhongBaoLoi` — đóng kết nối sau tạo char = trạng thái
  đúng, tự nối lại (không phải lỗi mạng).
- `NoiLaiSauKhiTaoNhanVat_TuDangNhapLai` — nối lại xong tự đăng nhập lại bằng credential cũ.
- `TenNhanVatSai_KhongGuiGoiNao` (4 case: ngắn, dài, hoa, khoảng trắng) — validate client-side
  khớp `AuthRules`, không gửi gói khi sai.
- `TenNhanVatTrung_HienLyDoNgayTrenManTaoNhanVat` + `..._LyDoSongSotQuaLanDangNhapLai` — lỗi
  "tên trùng" từ server hiện ngay trên màn tạo nhân vật và sống sót qua nối lại.
- `MatKetNoiKhongLyDo_VanCoCauDeHien`, `ThuLai_NoiLaiToiMayChuDaChon`, `RotMang_ThuLai_...` —
  rớt mạng không lý do vẫn có câu hiển thị + thử lại nối đúng máy chủ.

`Sanitize` (`CharacterNameInput.Sanitize`) đã là `public static`, không cần đổi thành `internal`
như phase file dự kiến — có thể gọi trực tiếp từ test.

## 2. PlayMode test (đã có sẵn, CHƯA chạy được trong phiên này)

`Assets/Tests/PlayMode/CharacterCreationViewTests.cs` đã viết đủ 2 case:

- `NameIsSanitizedAndSubmitStartsDisabledUntilValid` — gõ "AB C1@" → lọc còn "abc1", nút Submit
  vô hiệu tới khi tên hợp lệ.
- `ClickingFemaleSlotSubmitsFemaleAndName` — bấm avatar Nữ (qua `ExecuteEvents.Execute`, đúng
  đường vào con trỏ thật, không gọi thẳng phương thức) + Submit → event `Submitted` bắn đúng
  `(gender=1, name)`.

**Chưa chạy được `run-playmode-tests.ps1` trong phiên này**: script bắt buộc đóng Unity Editor
trước (batchmode khoá project); `Unity.exe` đang chạy 3 tiến trình trên máy lúc kiểm tra. Không
tự ý đóng Editor của người dùng — cần người dùng đóng Editor rồi chạy tay:
```
powershell -File D:\game\GopetUnityClient\run-playmode-tests.ps1
```

## 3. Live-smoke đăng ký mới → tạo char → vào map: KHÔNG chạy được trong phiên này

Hai rào cản xác nhận được khi thử:

1. **`REGISTER` bị chặn chính sách server** — gửi gói `REGISTER` thật (`RegisterCheck.cs`, đã
   có sẵn trong `Gopet.Net.LiveSmoke`) nhận về: `"Tài khoản không được phép có những từ này :
   admin,test,banquantri,gofarm"` — server đọc và phản hồi đúng (gói không sai wire), nhưng chặn
   theo danh sách từ cấm cho các tên test thường dùng. Không phải lỗi client.
2. **DB Docker chứa dữ liệu người dùng THẬT** — `docker/docker-compose.yml` ghi rõ: *"database
   này chứa dữ liệu người dùng thật lấy từ dump"*. Muốn live-smoke tạo-nhân-vật đầy đủ cần một
   account có `playerData == null` (chưa có nhân vật) — cách chắc ăn nhất là chèn thẳng 1 dòng
   account test vào DB, nhưng đây là hành động ghi vào DB chứa dữ liệu thật, **cần người dùng
   xác nhận trước** khi làm, không tự ý thực hiện.

**Đề xuất cho người dùng**: hoặc (a) cho phép chèn 1 account test riêng (không đụng dữ liệu có
sẵn) để chạy live-smoke tạo-nhân-vật, hoặc (b) chấp nhận unit+PlayMode test (đã xanh) là đủ bằng
chứng, để dành live-smoke thật cho lúc thao tác tay bình thường.

## Kết luận

Code + unit test đã hoàn chỉnh và xanh. Phần còn thiếu là 2 việc CẦN NGƯỜI DÙNG: đóng Unity
Editor để chạy PlayMode, và quyết định có chèn account test vào DB thật để live-smoke đầy đủ hay
không.
