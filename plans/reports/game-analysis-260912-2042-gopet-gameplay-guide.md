# goPet (Gopet) — Hướng Dẫn Cách Chơi

**Nguồn phân tích:** `client.jar_Decompiler.com/` (221 file Java obfuscated)
**Nền tảng:** J2ME MIDP 2.1 / CLDC 1.1 (điện thoại feature phone)
**Nhà phát triển:** Moder Entertainment (TAE)
**Phiên bản:** 1.4.3 | **Ngôn ngữ:** Tiếng Việt (+ tiếng Anh) | **Giao diện:** Ngang (landscape)

---

## Tổng quan

**goPet** là game **MMO nuôi thú chiến đấu** trên mobile. Người chơi nuôi thú cưng (pet), rèn luyện chỉ số, trang bị vật phẩm, tham gia bang hội, giao dịch và chiến đấu PvP/PvE theo lượt.

---

## 1. Hệ thống Pet (Nuôi thú)

### Chỉ số & Thuộc tính
- Bộ chỉ số: **STR, AGI, INT, ATK, DEF, HP, MP**
- Kinh nghiệm (XP) dạng `long`, có XP hiện tại / XP cần thiết cho level kế
- 2 loại tiền: **Vàng (Gold)** và **Ngọc (Gems)** — có thể đổi vàng sang ngọc

### 7 Nguyên tố (Element)
| Nguyên tố | Ghi chú |
|---|---|
| Lửa (Fire) | - |
| Cây / Thiên nhiên (Nature) | - |
| Đá / Đất (Earth) | - |
| Sét (Thunder) | - |
| Nước (Water) | - |
| Bóng tối (Dark) | - |
| Ánh sáng (Light) | - |

### Rèn luyện
- **"Luyện sức mạnh"** (STR) / **"Luyện tốc độ"** (AGI) / **"Luyện thông minh"** (INT)
- Tương tác thân mật: **Hôn, Chơi, Poke** (nhấn chọc)

### Hệ thống Tiến hóa
- **Kết hợp 2 pet** để tạo pet mới
- Yêu cầu đặt **tên mới** và trả **phí tiến hóa**

### Hiển thị
- Sprite animation, 200ms/frame
- Pet follower hiển thị thanh HP, số sát thương nổi, hiệu ứng trạng thái

---

## 2. Hệ thống Chiến đấu (Battle)

- **Combat theo lượt** (turn-based), có PvP qua hệ thống "thách đấu"
- Phân loại sát thương theo dải giá trị:
  - `-1` trở xuống: Hồi máu / hiệu ứng đặc biệt
  - `0-2`: Đánh thường (map với loại chỉ số)
  - `101-124`: Buff / debuff
  - `125+`: Kỹ năng đặc biệt
- **Số sát thương nổi** trên đầu pet
- Hỗ trợ **auto-battle**
- Kết quả thắng/thua có thưởng
- Âm thanh: tấn công, chí mạng, trượt, kết thúc

### Skill có thể đổi trước mỗi trận đánh

---

## 3. Hệ thống Kỹ Năng (24+ skill)

| Nhóm | Ví dụ |
|---|---|
| Tấn công | Sát thương, Sóng kích (SongKich), Liên hoàn quyền, Dao găm |
| Nguyên tố | Băng (Bang), Sét (SamSet), Lửa (Lua), Hắc độc (hadoc) |
| Buff/Debuff | Cuồng nộ, Tăng HP, Hút máu (hutmau), Đốt mana (manaburn) |
| Đặc biệt | Tornado, Meteor, Thor Hammer, Moon Shine, Zeus Wraith, Xayda |

- Kỹ năng có file animation riêng (`.anu` + `.png`)
- Skill bang hội tách biệt với skill pet
- Skill có thể **thay đổi / hoán đổi** giữa các trận

---

## 4. Bản Đồ & Thế Giới

- **24 bản đồ** (mã 11–34), dạng tile-based có dữ liệu collision
- Di chuyển bằng lệnh **"tới"** / **"quay lui"**
- Hệ thống **kênh (zone)**: "chuyển kênh"
- Hiệu ứng âm thanh khi chuyển map (`s_outMap_0`, `s_outMap_1`)
- Tên bản đồ lấy từ binary data (không thấy tên tiếng Việt trong client)

---

## 5. Hệ thống Vật Phẩm

- **Kho đồ** ("thùng đồ") + **Trang bị** (5 slot cho pet)
- **Cánh** (wings), **Hình xăm** ("hình xam") — xem/áp dụng/gỡ với "mục tay" (mực tẩy)
- **Tủ quần áo** / skin
- **Ký gửi** ("ky gui") — gian hàng giao dịch
- Vật phẩm có thuộc tính: loại, tên, chỉ số, cờ boolean (`gp.java`)

---

## 6. Hệ thống Bang Hội (Guild)

- Tạo / tham gia bang ("bang hoi")
- Thông tin bang: tên, **điểm vinh danh**, **đóng góp**
- **Kênh chat bang**, **nộp quỹ bang**
- **Kỹ năng bang hội**, tuyển thành viên ("tìm bang"), kick/invite
- Vai trò: bang chủ, admin

---

## 7. Hệ thống Xã hội

- **Bạn bè:** thêm / xóa / chặn
- **Hộp thư** ("ho thu"): inbox, gửi thư ("gui thu")
- **Chat đa kênh:** chat thế giới, chat bang, chat cộng đồng
- Tương tác với pet của người chơi khác

---

## 8. Hệ thống Nhiệm vụ (Quest)

- **NPC** (`dg.java`) có hội thoại đa lựa chọn + giao nhiệm vụ
- Lệnh **"kiểm tra nhiệm vụ"** (`dg.java`, `k.java`)
- NPC hiển thị tên + chỉ báo tương tác

---

## 9. Giao diện / UI

- Màn hình đăng nhập với chọn máy chủ
- Menu chính 13+ mục
- Điều hướng màn hình qua `fw.java`, cuộn qua `ge.java`
- Hỗ trợ cảm ứng (touch) + con trỏ crosshair
- Đa font size / style, localization 147+ chuỗi (VN/EN)

---

## Bắt đầu chơi

1. **Login** → chọn server
2. **Tạo nhân vật** → chọn thú cưng
3. **Khám phá bản đồ** → tương tác NPC → nhận nhiệm vụ
4. **Rèn chỉ số** pet (STR/AGI/INT)
5. **Ghép trang bị** → học skill
6. **Thách đấu PvP** hoặc đánh NPC
7. **Tham gia bang hội** → giao dịch trên ký gửi

---

## Lưu ý

- Code bị obfuscate (tên file 1 chữ cái), một số công thức sát thương chưa giải mã hoàn toàn từ client-side
- Nội dung nhiệm vụ, tên bản đồ, cơ sở dữ liệu item load từ server/binary data

---

## Các câu hỏi chưa giải quyết

1. Cấu trúc gói tin mạng đầy đủ (server-side obfuscated, không thấy từ client)
2. Công thức tính sát thương chính xác (đoạn `dj.java:131` không decompile được)
3. Toàn bộ nội dung nhiệm vụ (chỉ thấy phần UI client)
4. Bản đồ tên (map đánh số 11-34, không có tên hiển thị)
5. Cơ sở dữ liệu item đầy đủ (load từ server/binary)
