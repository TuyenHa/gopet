# extract-jar-strings

Bóc bảng chuỗi VN + EN từ `a.java` của client J2ME cũ vào
`GopetUnityClient/Assets/Resources/Jar/Strings/strings-{vi,en}.json`.

## Dùng

```bash
node index.js            # bóc, ghi đè
node index.js --check    # chỉ so sánh, dùng cho CI — exit 1 nếu lệch
```

## Nguồn: `public static String a(int)` trong `a.java`

```java
public static String a(int var0) {
   switch (a) {              // a = ngôn ngữ: 0 = VN, 1 = EN
      case 0:  switch (var0) { case 3: return "Tiện ích"; ... default: return String.valueOf(var0); }
      case 1:  switch (var0) { case 3: return "Utilities"; ... default: return String.valueOf(var0); }
   }
}
```

Tách hai khối bằng cách tìm chuỗi đánh dấu `public static String a(int var0) {`
rồi hai lần xuất hiện của `default: return String.valueOf(var0);` — lần thứ nhất
đóng khối VN, lần thứ hai đóng khối EN.

**`default` trả về CHÍNH CON SỐ** khi chưa dịch — tiện lúc chạy (không nổ), nhưng
nghĩa là thiếu một mục sẽ hiện ra số trên màn hình chứ không có gì báo lỗi. Vì vậy
tool này **so số lượng** giữa hai bảng và cảnh báo nếu lệch, thay vì im lặng.

## Kết quả đã kiểm chứng

**134 = 134** — hai bảng cân nhau tuyệt đối. (Ghi chú lịch sử: bản đánh giá đầu ước
lượng tay bằng `grep -c "case "` trên một dải dòng, ra 135 vs 134 — sai vì đếm luôn
dòng `case 0:`/`case 1:` chọn ngôn ngữ. Bóc đúng phạm vi thì hai bảng bằng nhau.)

| Chỉ số | VN | EN |
|---|---|---|
| 5 | "T.Khoản" | — |
| 266 | "Đăng nhập" | "Log" |
| 330 | "Nếu chưa có tên," | — |
| 331 | "Nếu chưa có tên, xin đăng ký" | "If you don't have a name, please register" |
| 348 | "M.Kh:" | "Pass:" |
| 353 | dòng bản quyền (`\n`-prefixed) | **rỗng** — bản EN không dịch dòng này |

## Escape

Java chỉ dùng `\n \t \r \" \\` trong bảng này. Gặp escape lạ, tool ném lỗi thay vì
đoán — sai một ký tự trong dòng bản quyền còn khó phát hiện hơn cả thiếu hẳn một mục.
