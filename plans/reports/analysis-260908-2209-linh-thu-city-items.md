# Báo cáo: Mũ, Vũ khí, Giáp, ATM, Thức ăn — Thành phố Linh Thú (Gopet)

**Nguồn:** `client.jar_Decompiler.com/` + `SRCGOPETGOC/client.jar`
**Ngày:** 2026-09-08

---

## 1. Bối cảnh nhanh

- Client Gopet J2ME (`client.jar`) chạy trên FreeJ2ME, kết nối `127.0.0.1:19180`.
- "Thành phố Linh Thú" là map hub. Client **không** hard-code tên map/tên NPC — map/entities do server gửi qua opcode; client vẽ dựa vào `type` byte (xem `eg.java:19`).
- File then chốt:
  - `eg.java` — NPC/công trình trên map (đọc từ stream server).
  - `fu.java` — UI trang bị pet (5 slot).
  - `fn.java` — UI Ngân hàng ("ATM").
  - `br.java` — kiểu thao tác ngân hàng.
  - `dj.java` — icon/stats/currency toàn cục.
  - `r.java` — Cường hoá / Tiến hoá.
  - `fr.java` — HUD Pet + Battle.
  - `dc.java` — gói opcode gửi server (17/45/…).

---

## 2. Hệ stat & tiền tệ nền (`dj.java:54`)

```java
gg.a = new String[]{"(ngoc)","(dau)","(thoc)","(vang)",
                    "(str)","(agi)","(int)","(atk)","(def)","(hp)","(mp)",
                    "(water)","(thunder)","(rock)","(fire)","(dark)","(tree)","(light)",
                    "(sao)","(chien)","(bthu)","(codo)","(coxanh)","(nha)","(nguoi)",
                    "(saoden)","(chienluc)","(nluong)","(diem)","(lua)"};
```

**Tiền tệ 4 loại:** `vang` (mGold), `dau` (Đậu), `thoc` (Thóc), `ngoc` (Ngọc).
**Chỉ số pet:** `str, agi, int, atk, def, hp, mp` + hệ nguyên tố `water/thunder/rock/fire/dark/tree/light`.
→ Toàn bộ giá trị stat cụ thể do server tính, client chỉ nhận `dn.a[]/b[]` (mảng String tên+giá trị) trong `dn.java`.

---

## 3. Trang bị Pet (Mũ / Vũ khí / Giáp) — `fu.java`

Pet có **5 slot** cố định (`fu.java:53-57`):

| Index | Slot | Subtype code |
|-------|------|--------------|
| 0 | **Nón (Mũ)** | 3 |
| 1 | **Giáp** | 2 |
| 2 | **Vũ khí** | 1 |
| 3 | Giày | 104 |
| 4 | Bao tay | 105 |

Shop switcher trong `fu.java:161-183`:

```java
case 0: this.a = "Nón";    this.e(3);   return;
case 1: this.a = "Giáp";   this.e(2);   return;
case 2: this.a = "Vũ khí"; this.e(1);   return;
```

### 3.1 Chức năng chung của trang bị

- Chỉ **Pet** mặc (nhân vật chính có "Tủ quần áo" `gw.a(91)` riêng — thời trang, không stat).
- Mỗi món có socket ngọc — thao tác trong `fu.java:232-241`:
  - `Gắn ngọc` → opcode **73** (`fu.java:299-307`)
  - `Tháo ngọc` → opcode **75** (`fu.java:309-317`)
  - `Tháo ngọc nhanh` → opcode **78** (`fu.java:319-327`)
- **Cường hoá** (`gw.a(28)` = "Cường hóa") — 3 nguyên liệu, opcode **46/76** (`r.java:116-146`).
- **Tiến hoá** (`gw.a(29)` = "Tiến hóa") — 2 nguyên liệu, opcode **49/79** (`r.java:128,152`).
- **Huỷ trang bị** (`gw.a(109)` = "Hủy") — opcode **56** (`fu.java:293-297`).
- **Tháo ra** khỏi pet — opcode **39** (`fu.java:214-218`).

### 3.2 Ý nghĩa từng loại

| Loại | Vai trò trên nhân vật/pet |
|------|---------------------------|
| **Mũ / Nón** | Trang bị đầu của pet. Cộng chỉ số phòng thủ/phụ trợ (int/def…). NPC shop type **18** ("Nón") trong `eg.java:111-113`. |
| **Vũ khí** | Trang bị tay pet. Cộng ATK — quyết định sát thương khi Pet vào Đấu trường/Battle. Trong `fr.java:747` có `attack.png` — nút đánh sát thương lấy từ vũ khí. |
| **Giáp** | Trang bị thân pet. Cộng DEF/HP — quyết định chịu đòn. |

> Cả 3 loại đều là **đồ Pet**, không phải nhân vật chính. Nhân vật chính chỉ có thời trang cosmetic.

---

## 4. ATM ↔ Ngân hàng (`fn.java` + `br.java`)

**Không có NPC riêng gọi "ATM" phía client.** Trong code UI này tên là **"Ngân hàng"** (`a.a(24)`, `fn.java:16`). NPC "ATM" trên map (nếu server render) chỉ trigger cùng UI `fn`.

### Thao tác (từ `fn.java` + `br.java`)

| # | Chức năng | Opcode (81/…) |
|---|-----------|---------------|
| Nạp mGold | Gửi vàng vào ATM | `cx.a(1, x)` — `fn.java:204` |
| Nạp Đậu | Gửi đậu vào ATM | `cx.a(2, x)` — `fn.java:213` |
| Nạp Ngọc | Gửi ngọc vào ATM | `cx.b(x)` — `fn.java:222` |
| **Đổi Vàng → Đậu** | `br` case 6, string `a.a(185)` |
| **Đổi Vàng → Thóc** | `br` case 7, string `a.a(186)` |
| **Đổi Vàng → Ngọc** | `br` case 9, string `gw.a(40)` |
| Chuyển khoản | `br` case 0 — SMS "Gửi tới …" (`a.a(441)`) |
| Xem số dư | `fn.java:33-35` — hiển thị mGold + Ngọc hiện có |

### Vai trò với nhân vật

1. **Kho tài sản an toàn** — cất vàng/đậu/thóc/ngọc, không mất khi PK/thua trận.
2. **Sàn đổi tiền** — chuyển giữa 4 loại tiền tệ (Vàng là gốc).
3. **Chuyển khoản** — gửi tiền cho người chơi khác qua SMS xác nhận (`fn.java:106-107`).
4. **Nạp mGold** — mua tiền game qua SMS (opcode 46 phía dc `dc.a(int,int)`).

---

## 5. Thức ăn

Client **không có NPC "Thức ăn" riêng** trong `eg.java`. Ý niệm "thức ăn" chia làm 3 nhóm:

### 5.1 Thóc — food currency nuôi pet

- Icon `(thoc)` trong `dj.java:54` — nằm ngang hàng với Vàng/Đậu/Ngọc.
- String `a.a(400)` = "Thóc", `a.a(186)` = "Đổi vàng lấy thóc".
- Là **loại tiền tệ chuyên biệt để duy trì Pet** — Pet tự tiêu thụ theo thời gian (logic phía server; client chỉ hiển thị lượng thóc).
- Nguồn: đổi từ Vàng qua Ngân hàng, hoặc quest/reward server.

### 5.2 Vật phẩm / Potion — dùng trong battle

- NPC shop "**Vật phẩm**" — `eg.java:123-125` (type 22) → command **1009** đến `dv`.
- Trong Battle (`fr.java:740-775`) có 3 nút:
  - `attack.png` — dùng vũ khí
  - `skill.png` — dùng kĩ năng (`gw.a(125)` = "Dùng kĩ năng")
  - `potion.png` — **"Dùng item"** (`gw.a(126)`)
- Chức năng: hồi HP/MP/buff tạm thời trong trận.

### 5.3 Tương tác pet (giữ độ thân/mood)

Trong HUD pet (`fr.java:150-183`) có 3 nút cảm xúc — gửi opcode **17** (`dc.a(int)`, `dc.java:12-18`):

| Nút | String | Sub-op |
|-----|--------|--------|
| Chơi với pet | `gw.a(94)` | 1 |
| Hôn pet | `gw.a(95)` | 0 |
| Xoa đầu pet | `gw.a(96)` | 2 |

Ngoài ra nút **Hồi phục** `gw.a(115)` (code 323, `fr.java:295-298`) → opcode **45** (`dc.a(true)`) — hồi HP toàn phần cho pet (có thể tốn thóc/đậu tuỳ server).

---

## 6. Bản đồ shop NPC (`eg.java:55-155`)

| Type | Tên (client string) | Ghi chú |
|------|---------------------|---------|
| 0-4 | Nhà hẻm / Nhà mặt tiền / Biệt thự / Dinh thự / Nhà | Bất động sản |
| 6 | Thú cưng | Menu pet |
| 7 | Vườn | Trồng cây |
| 8 | Phòng vé | Vé sự kiện |
| 10 | Cà phê | Chỗ tụ họp |
| 11 | Khu | Chuyển khu |
| 12 | Hộp thư | Thư từ |
| 13-16 | Caro / Cờ tướng / Tiến lên / Phỏm | Mini-game (→ `dc.d(1..4)`) |
| 17 | Thời trang | Đồ char |
| **18** | **Nón** | Mũ pet |
| 19 | Giày | Giày pet |
| 20 | Mỹ viện | Beauty |
| 21 | Tóc | Tóc char |
| **22** | **Vật phẩm** | Potion/item (thức ăn battle) |
| 23 | Gara | Xe |
| 24 | Trò chơi trong nhà | |
| 25 | Pet shop | Mua/bán pet |
| 26 | Đấu trường | PVP/PVE |

> **Không thấy** NPC type riêng cho "Vũ khí", "Giáp", "ATM", "Thức ăn" phía client. Các món này (nếu xuất hiện trên map thành phố linh thú) đều do server bind vào một type có sẵn (thường tái dùng 18/22) hoặc mở qua menu game (`cg.j = a.a(24)` — Ngân hàng nằm ở main menu).

---

## 7. Tóm lược 1 dòng cho từng khái niệm

| Vật phẩm | Ai dùng | Chức năng |
|----------|---------|-----------|
| **Mũ / Nón** | Pet | Trang bị slot đầu, tăng chỉ số phòng thủ/int |
| **Vũ khí** | Pet | Trang bị slot tay, tăng ATK — quyết định sát thương trận đấu |
| **Giáp** | Pet | Trang bị slot thân, tăng DEF/HP |
| **ATM (Ngân hàng)** | Nhân vật | Cất/rút Vàng-Đậu-Thóc-Ngọc; đổi tỷ giá; chuyển khoản |
| **Thức ăn** | Pet | (a) *Thóc* — energy nuôi pet dài hạn; (b) *Vật phẩm/potion* — hồi HP/MP trong battle; (c) tương tác/hồi phục giữ mood |

---

## Câu hỏi chưa giải quyết

1. Trên map "Thành phố Linh Thú" cụ thể server bind NPC "ATM" và "Thức ăn" vào entity type nào? Cần đọc file `maps/*.dat` (binary) hoặc log server để xác thực — client chỉ dựng UI, không biết đó là NPC nào.
2. Cơ chế "đói" chi tiết — mỗi bao lâu Pet mất bao nhiêu HP nếu hết thóc? Logic phía server, không có ở client.
3. Danh sách stat cụ thể mỗi cấp trang bị (Mũ +? DEF, Vũ khí +? ATK) — client render string từ `dn.a[]` do server gửi, không nội suy.
4. "Cường hoá" và "Tiến hoá" tiêu tốn tài nguyên gì (vàng/đậu/ngọc/nguyên liệu) — server side; client chỉ gửi 3 hoặc 2 item ID lên.
