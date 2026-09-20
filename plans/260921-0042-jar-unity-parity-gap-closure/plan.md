# Bịt các lỗ parity jar ↔ Unity (đợt 260921)

Nguồn: `plans/reports/parity-gap-260921-0042-jar-vs-unity.md` (audit chéo jar / GServer / Unity).

## Bối cảnh

Tầng giao thức đã phủ kín: `check-protocol-coverage` báo 74 handled / 0 missing, 187 hằng
`GopetCmd` khớp 100% server. Lỗ còn lại nằm ở **phía GỬI** — packet đã có sẵn trong
`Net/**` nhưng không có nút nào gọi, nên tính năng chết lặng.

Phần lớn "tính năng jar còn thiếu" trong giả thiết ban đầu hoá ra là **code chết ở jar**
(nhà cửa, vườn, cà phê, mini game cờ, mỹ viện, gara — `dv.java:33-57` không có case) hoặc
**chưa từng tồn tại** (chợ trời, kết hôn, VIP, chuyển sinh, vượt ải, thành tựu). Không làm.

## Phase

| # | Phase | Mức | Trạng thái |
|---|---|---|---|
| 01 | [Danh sách bang khi chưa có bang](phase-01-danh-sach-bang-khi-chua-co-bang.md) | P0 | ✅ Xong |
| 02 | [Học và thay kỹ năng pet](phase-02-hoc-va-thay-ky-nang-pet.md) | P0 | ✅ Xong |
| 03 | [Góp quỹ và ô kỹ năng bang](phase-03-gop-quy-va-o-ky-nang-bang.md) | P0 | ✅ Xong |
| 04 | [Các lỗ mức trung bình](phase-04-cac-lo-muc-trung-binh.md) | M | ✅ Xong |

## Phụ thuộc

- Phase 01–03 độc lập nhau, chạy song song được. Phase 04 nên làm sau 03 vì dùng lại
  hàng nút vừa dựng trong `GuildView`.
- Không phase nào cần sửa server: mọi endpoint đã có sẵn và đã kiểm chứng call-site.

## Không làm (đã kiểm chứng, ghi để khỏi đào lại)

- **Top điểm phát triển bang** (`GUILD_TOP_GROWTH_POINT`): server map thẳng vào
  `showTopFund()` (`GameController.cs:2568-2570`) nên nội dung y hệt Top quỹ đã có.
- **AnimationMenu gửi ngược**: server không có nhánh nhận, toast hiện tại là đúng.
- **Thách đấu cược tự nhập**: Unity dùng menu giá cố định của server (1032), khác cách
  jar tự nhập nhưng không chặn luồng nào.
- **Nạp thẻ SMS**: thuần client J2ME (`platformRequest("sms:")`), server không có opcode.
