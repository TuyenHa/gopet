# Môi trường database cục bộ cho GServer

MariaDB trong Docker, nạp sẵn 3 dump của goPet.

## Chạy

```bash
cd docker
cp .env.example .env        # rồi đổi mật khẩu trong .env
docker compose up -d
docker compose logs -f mariadb    # theo dõi lúc nạp dump lần đầu (~1-2 phút)
```

Nạp xong sẽ thấy:

```
[gopet-init] gopettae_tae2: 50 bảng
[gopet-init] gopettae_gopet_web: 18 bảng
[gopet-init] gp_log: 1 bảng
[gopet-init] === Nạp xong ===
```

## Ba database, ba dump

Tên phải khớp chính xác với connection string trong `GServer/App.config` — sai tên thì GServer vẫn chạy nhưng không thấy dữ liệu.

| Connection string | Database | Dump | Nội dung |
|---|---|---|---|
| `GameConnectString` | `gopettae_tae2` | `server_db.sql` | template item/pet/skill/map, dữ liệu người chơi |
| `WebConnectString` | `gopettae_gopet_web` | `web_db.sql` | tài khoản, lịch sử đăng nhập, thanh toán |
| `LogConnectString` | `gp_log` | `log_db.sql` | bảng `history` |

## Tại sao MariaDB 10.4 chứ không phải bản mới hơn

Dump sinh từ 10.4.32, và **code game phụ thuộc vào hành vi của 10.4**.

Đã thử 10.11 trước và gặp lỗi lúc chạy thật:

```
MySqlConnector.MySqlException: Wildcards or range in JSON path
not allowed in argument 2 to function 'json_value'
   at TopPet.Update() in Data/top/TopPet.cs:line 30
```

`TopPet.cs:30` dùng `JSON_VALUE(player.pets, '$[*].exp')`. Kiểm chứng bằng thực nghiệm trên cùng một câu INSERT:

| Version | Kết quả |
|---|---|
| 10.4 | Chấp nhận, trả về phần tử khớp đầu tiên, insert 1 dòng |
| 10.11 | `ERROR 4044` |

Sửa query cho hợp 10.11 sẽ làm đổi hành vi bảng xếp hạng pet — nằm ngoài phạm vi "chỉ vá bảo mật, giữ nguyên hành vi". Và gần như chắc chắn còn chỗ 10.4-ism khác chưa lộ ra; khớp version là cách duy nhất tránh phát hiện lẻ tẻ về sau.

> **Đánh đổi:** 10.4 hết vòng đời hỗ trợ từ 6/2024, không còn bản vá bảo mật. Chấp nhận được vì container chỉ bind loopback và chứa dữ liệu test. **Không dùng cấu hình này cho production mà không xem lại.**

## Bảo mật

- Container **chỉ bind `127.0.0.1:3306`**, không phải `0.0.0.0`. Dump chứa email và IP người dùng thật.
- Mật khẩu nằm trong `.env`, đã gitignore. `.env.example` là bản mẫu.
- Dump mount **chỉ đọc** (`:ro`).

## Nối GServer vào

`App.config` không chứa mật khẩu (cố ý — nó nằm trong repo). Credential đến từ biến môi trường:

```bash
cd SRCGOPETGOC/GServer
GOPET_DB_HOST=127.0.0.1 \
GOPET_DB_PORT=3306 \
GOPET_DB_USER=root \
GOPET_DB_PASSWORD=<lấy từ docker/.env> \
./bin/Debug/net8.0/Gopet.exe
```

Khởi động đúng sẽ in ra:

```
[DB] OK  GameConnectString -> gopettae_tae2@127.0.0.1
[DB] OK  WebConnectString -> gopettae_gopet_web@127.0.0.1
[DB] OK  LogConnectString -> gp_log@127.0.0.1
```

Thứ tự ưu tiên khi ghép connection string (`Manager/MYSQLManager.cs`):

1. `GOPET_<TÊN>_CONNSTR` — thay trọn chuỗi
2. `GOPET_DB_HOST` / `_PORT` / `_USER` / `_PASSWORD` — đè từng phần
3. `App.config`

## Thao tác thường dùng

```bash
docker compose ps                    # trạng thái
docker compose logs -f mariadb       # log
docker compose down                  # dừng, GIỮ dữ liệu
docker compose down -v               # dừng, XOÁ dữ liệu (nạp lại từ dump)
docker compose restart mariadb

# Vào shell SQL
docker exec -it gopet-mariadb mysql -uroot -p gopettae_tae2
```

Script nạp chỉ chạy **một lần** khi volume còn rỗng. Muốn nạp lại phải `down -v`.

## Kiểm tra nhanh

```bash
docker exec gopet-mariadb mysql -uroot -p<mật khẩu> -e "
SELECT COUNT(*) AS players FROM gopettae_tae2.player;
SELECT COUNT(*) AS users   FROM gopettae_gopet_web.user;"
```

Giá trị đúng của dump hiện tại: 4 player, 5 user, 937 item, 24 map.

## Cảnh báo về dữ liệu

`web_db.sql` chứa **dữ liệu người dùng thật** — email, hash mật khẩu, địa chỉ IP. Xem mục 3.5 của `plans/reports/analysis-260905-1330-gopet-server-source.md`. Đừng đẩy database này ra ngoài mạng và đừng commit dump vào repo công khai.
