# Ba Phase Hoàn Thành — Nhưng Lỗi Nặng Nhất Lọt Qua Hết

**Date**: 2026-09-05, phiên 20:30–23:00
**Severity**: High
**Component**: AuthHandler (CHECK_SPEED) + Code Review Process + Testing Strategy
**Status**: Resolved (sửa theo code-reviewer report)

---

## What Happened

Phase 1, 2, 3 của dự án goPet Unity client đều đóng — lúc đó là 138 unit test xanh, 14/14 live smoke xanh, verify 5/5 xanh. Tôi đã báo cáo là xong.

Rồi code review chạy và tìm ra: `AuthHandler` trả lời `CHECK_SPEED` **tức thì thay vì chờ
đúng số mili-giây server gửi**. Server đo thời gian trả lời, thấy sớm thì coi là speed-hack,
rồi bắn lại nhịp mới mỗi 500ms. Đo trên dump: 805 gói <1 giây (Unity) so với 45 gói 15-31
giây (emulator J2ME) — lệch khoảng 20 lần.

Không một bài test nào bắt được, vì tất cả đều đo bytes chứ không đo thời điểm.

---

## The Brutal Truth

Điều này thật tệ vì:

1. **Lỗi lọt qua toàn bộ test đang có.** `AuthHandler` không hề có unit test — nó là file duy nhất chứa logic hành vi, và đúng nó là file duy nhất không được test. Live smoke giữ kết nối 40 giây cũng không bắt được. Lỗi nằm ở thời điểm, mà mọi bài test đều chỉ đo bytes.

2. **Test không chứng minh được nó tuyên bố.** Bản harness đầu tiên có check L: `Check("trả lời CHECK_SPEED", _speedReplies > before, ...)`. Trả lời ngay (805 lần) cũng xanh. Trả lời đúng (1-2 lần) cũng xanh. Check không thể phân biệt đúng/sai ở đây.

3. **Tôi tự viết ra lời khẳng định sai rồi tin theo nó.** Plan ghi "đọc `int`, trả lời lại" mà không nói phải chờ. Rồi tôi viết comment "chỉ để tham khảo" ngay trong code — sai hoàn toàn. Comment đó biến một giả định chưa kiểm thành thứ trông như đã kiểm, và nó nằm đó cho tới khi có người đọc kỹ server.

---

## Technical Details

**Lỗi ở `AuthHandler.OnCheckSpeed` (`Assets/Scripts/Net/Auth/AuthHandler.cs:100-109`):**

```csharp
private void OnCheckSpeed(Message m)
{
    m.Reader.ReadInt();  // <-- đọc rồi VỨT, không dùng
    _send(AuthPackets.CheckSpeedReply());  // <-- trả lời ngay
}
```

**Server trả về** (Player.cs:307): `int waitMs` — đây là **hạn phải chờ**, không phải "thông tin tham khảo". 
`onClientSpeedRespose()` (Player.cs:330-336) **đo thời gian**:

```csharp
if (s_SpeedStopWatch.Elapsed + 2s < waitMs) {
    // nhánh speed-hack
    return;  // stopwatch KHÔNG chạy lại
}
```

Khi rơi vào nhánh đó, stopwatch đứng → tick 500ms sau lại gửi `CHECK_SPEED` mới → **vòng lặp chặt: 2 gói/giây**.

**Bằng chứng từ dump server thực tế (2026-09-05 soak 29 phút):**

| Độ trễ trả lời | Số nhịp | Client |
|---|---|---|
| < 1s | **805** | Unity (bản lỗi) |
| 15–31s | 45 | J2ME emulator |

J2ME ba mẫu: 24000ms → trả 24.08s · 22000ms → 22.07s · 17000ms → 17.03s. Chờ đúng waitMs, sai số ~30-80ms.

**Hậu quả nếu để lại:**
- Mỗi trả lời sớm rơi vào nhánh `user.ban(..., "HackSpeed", +1 giờ)` (`Player.cs:334`) — hiện comment nhưng build nào bỏ comment là khoá tài khoản
- Phá vỡ "không đổi một byte nào so với client J2ME" ở mức hành vi

---

## What We Tried

1. **138 unit test:** Toàn bộ xanh. Không có test hành vi cho `AuthHandler`.
2. **14/14 live smoke:** Toàn bộ xanh. Check L chỉ assert `_speedReplies > before`.
3. **verify.ps1 compile check:** Xanh (chỉ kiểm compile, không chạy tầng giao thức).
4. **5/5 verify offline:** Xanh (không liên quan tới CHECK_SPEED).

Không cái nào bắt được. Tất cả đều xanh vì chúng tôi đo nhầm thứ — đo cái xấp xỉ (có bao nhiêu lần) thay vì cái tất định (khoảng thời gian).

---

## Root Cause Analysis

**Nguyên nhân sâu — ba lớp:**

1. **Plan sai.** Implementation Step 6: "`CHECK_SPEED` — nhận, đọc `int`, trả lời lại" không nói phải chờ. Comment "logic ban vì trả lời quá nhanh hiện bị comment nên chỉ ghi log" **che giấu** sự thật: nhánh đó `return` sớm → stopwatch không chạy lại → tick 500ms gửi gói mới.

2. **Code comment sai.** `AuthHandler` ghi "chỉ để tham khảo" trực tiếp dẫn dev không care về giá trị `waitMs` đã đọc.

3. **Test không test hành vi.** `AuthHandler` là file **duy nhất** có logic điều khiển thời gian (khi nào trả lời) mà lại là file **duy nhất** không có test hành vi. 138 test toàn bộ test bytes (parser/builder), không test timing. Byte thật không chứng minh thời điểm đúng.

**Vì sao lỗi này lọt?**
- Code review bắt được (phát hiện check L yếu, phát hiện `AuthHandler` không có test)
- Nhưng nó báo ở level "HIGH" + mô tả chi tiết, không phải "CRITICAL" — có thể là lý do dev áp dụng muộn

---

## Lessons Learned

1. **Mỗi test phải chứng minh được là nó biết đỏ.** Check L xanh với cả 1 lẫn 805 nhịp → check đó vô dụng. Cách phát hiện: đột biến code rồi chạy lại; test không đổi màu là test có vấn đề.

2. **Đo cái tất định, đừng đo cái xấp xỉ.** "Bao nhiêu lần" ≠ "sau bao lâu". Đếm số luồng để tìm rò ô cũng vậy — lệch về âm tính giả. Hỏi thẳng đối tượng: `GopetSocket.DisposedCleanly`, `elapsed time`, hoặc counter rõ ràng.

3. **File có logic hành vi phải có test hành vi.** `AuthHandler.OnCheckSpeed` điều khiển khi nào trả lời — đó là logic hành vi, không phải bytes. Test byte-exact chứng minh gói đúng HÌNH DẠNG, không chứng minh đúng THỜI ĐIỂM.

4. **Bảng đặc tả suy từ code là giả thuyết; wire thật mới là sự thật.** Plan ghi nông ("đọc `int`, trả lời lại") còn wire thật — đo từ dump của J2ME — cho thấy quy tắc thật là chờ đúng `waitMs`. Phải bắt gói thật trước khi viết handler.

5. **Chặn TRÊN cũng phải kiểm, không chỉ chặn dưới.** Check L chỉ có cận dưới (`> 0`) nên xanh với cả 805 nhịp. Thêm cận trên rồi thì đột biến khôi phục hành vi cũ làm test đỏ ngay.

6. **Plan cần ghi rõ TƯỜNG MINH những điều dễ nhầm.** Comment "chỉ để tham khảo" làm dev bỏ qua một khía cạnh đủ để hỏng toàn bộ. Nên viết: "đọc `int waitMs` — **server ĐO thời gian**, trả lời sớm hơn `waitMs - 2s` bị ghi speed-hack".

---

## Đã Sửa Ngay Trong Phiên — Không Phải Việc Để Lại

Toàn bộ phát hiện của cả hai báo cáo code review đã được áp dụng và kiểm chứng trước khi
đóng phiên. Ghi lại kèm bằng chứng, vì "đã sửa" mà không đo được thì cũng chỉ là lời nói.

1. **Hoãn trả lời `CHECK_SPEED` đúng `waitMs`.** `AuthHandler.Tick()` gọi mỗi frame, không
   `Timer` cũng không `Thread` — gói phải đi từ luồng chính. Đo lại trên dump server:
   **1 nhịp trong 40 giây, trả lời sau 29.04s, 0 nhịp trả lời sớm** (trước đó 805 nhịp <1s).

2. **Cận trên cho check L.** `1..hold/15+1`. Đột biến khôi phục hành vi cũ → 2 test đỏ ngay.

3. **`AuthHandlerTests` + `AuthRulesTests`.** Đồng hồ giả, kiểm cả cửa sổ hợp lệ của server
   `[waitMs-2s, 3*waitMs)`. Tổng 160 unit test.

4. **Sửa plan.** Bảng opcode tách rõ 3 / 4 / 10 / 21 / 71; Step 6 nói thẳng phải chờ.

5. **Handler `okDialog`** (opcode 71, bao ngoài + sub 0).

6. **`packet-diff`:** 0 gói → exit 2; validate `--direction`; so cả cột `enc`.

Thêm hai thứ ngoài báo cáo: bỏ bản sao `ClientInfoPacket` trong harness (check D đang xác
thực bản sao chứ không phải code ship), và thêm `GopetClient.Ticked` để P5 không vấp bẫy
"quên gọi Tick".

Kết quả sau khi sửa: verify 5/5, 160 unit test, live smoke 15/15.

---

## Còn Lại Thật Sự

- **UI đăng nhập, `CredentialStore`, 2FA** — hoãn có chủ đích sang P5. Dựng bây giờ sẽ phải
  vứt khi P5 lập hệ component dùng chung, và không tự nghiệm thu được.
- **Đăng ký** — `doRegister` khoá cứng bằng `if (true)`. Tiêu chí không áp dụng với server này.
- **`Thread.Sleep(1000)` ở `Player.cs:658`** — không còn cần cho việc gói tới nơi, nhưng còn
  tác dụng hãm brute-force. Giữ hay gỡ là quyết định bảo mật, để người quyết.
- **Phase 4** — Remote Asset Pipeline. Dump đã bắt sẵn luồng thật: opcode 96 xin đường dẫn
  `npcs/....png`, server trả thẳng PNG.

---

## Những Lỗi Lớp 2 — Nhỏ Hơn Nhưng Cùng Một Gốc

Hai báo cáo code review còn chỉ ra một loạt lỗi cùng gốc "test đo nhầm thứ":

- **Phase 2:** check D dùng `File.Exists` nên xanh vĩnh viễn (constructor tạo file ngay);
  E1/E2 đo rò bằng cách đếm luồng của tiến trình — lệch về âm tính giả.
- **Phase 3:** check L chỉ có cận dưới; `AuthHandler` không có test hành vi; harness bỏ qua
  check I/J/K/L mà vẫn in "LIVE SMOKE OK"; thiếu `okDialog`; `packet-diff` báo OK khi so 0 gói.

Đáng chú ý: gần như tất cả đều là **test yếu**, không phải code sai. Code sai chỉ có một
(C1) — và nó lọt được chính vì đám test yếu kia.

---

## Một Điều Xấu Hổ Khác

**Chẩn đoán lỗi mất gói ở Phase 1 sai ban đầu.** Tôi đoán TCP abortive close, triệu chứng treo vĩnh viễn có nghĩa là mất gói trước khi client kịp đọc. Thực tế: `Session.Close()` ngắt luồng gửi trước khi `MsgSender.stop()` drain. `sleep(1000)` ở một nhánh có nghĩa là may mắn (gói kịp đi); thiếu ở nhánh khác là gói bị xoá. Phải đọc kỹ code mới thấy, chứ không phải đoán từ triệu chứng.

**Sai lầm thứ ba, thuộc loại khác:** khi `SetForegroundWindow` bị Windows chặn, tôi vẫn bơm
chuột theo toạ độ màn hình — cú click rơi vào cửa sổ khác của người dùng đang nằm trên.
Sau đó bắt buộc script phải xác nhận `GetForegroundWindow()` đúng cửa sổ đích rồi mới bơm
input, không thì abort. Tự động hoá GUI mà không kiểm tra mình đang gõ vào đâu là nguy hiểm.

**Đoán sai thứ hai:** `dotnet run --nologo -v quiet -- "abc12345"` không truyền được tham số. Chương trình đi băm chuỗi "--nologo" thành mật khẩu, nghi database một lúc. Bài tự kiểm (đúng → MATCH, sai → NO MATCH) vừa loại ra ngay.

**Bài học chung của cả hai:** đừng suy nguyên nhân từ triệu chứng khi còn đọc được code. Và mọi công cụ tự viết phải có bước tự kiểm — bài kiểm "đúng → MATCH, sai → NO MATCH" loại được lỗi `dotnet run` trong một lần chạy, sau khi tôi đã nghi oan database.

---

## Kết Luận

Tầng giao thức (P1-3) xong. Bytes khớp J2ME ngay từ đầu — nhưng **hành vi** thì sai, lệch
thời điểm gấp khoảng 20 lần, và mãi tới khi code review đọc kỹ server mới lộ ra. Đây là lỗi không bắt được bằng test bytes hay test online đơn lẻ — cần test timing hoàn toàn riêng (`Tick` với đồng hồ giả) hoặc đo trực tiếp trên dump thật. Quy tắc từ nay: đụng vào tầng giao thức thì phải chạy live smoke với server thật, và khi cần so parity thì dùng `packet-diff --opcodes-only` trên dump hai đầu — không đánh dấu xanh chỉ vì compile sạch và test offline qua.

---

**Status:** DONE
**Summary:** Phase 1-3 đóng. Code review bắt được lỗi hành vi nặng trong `AuthHandler` (trả lời `CHECK_SPEED` ngay thay vì chờ) làm flood ~20x gói; đã sửa và đo lại trên dump: 0 nhịp trả lời sớm. Mọi phát hiện của cả hai báo cáo đã áp dụng xong trong phiên.
**Concerns/Blockers:** Không có việc treo về kỹ thuật. UI/CredentialStore/2FA hoãn có chủ đích sang P5; đăng ký bị server khoá cứng nên tiêu chí không áp dụng.
