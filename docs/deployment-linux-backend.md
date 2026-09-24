# Deploy backend (GServer + MariaDB) lên Linux

Hướng dẫn đưa máy chủ game goPet (`SRCGOPETGOC/GServer`) và database lên một máy
Linux (Ubuntu 22.04 / 24.04 hoặc Debian 12).

Có hai cách chạy GServer, database thì cách nào cũng chạy trong Docker:

| | **Cách A — Docker (khuyến nghị)** | Cách B — systemd |
|---|---|---|
| Máy chủ cần cài | Docker | Docker + .NET 8 ASP.NET runtime + font Arial |
| Build | trên máy chủ (`docker compose --build`) | trên Windows (`dotnet publish`), chép sang |
| Cập nhật / rollback | đổi image | chép đè thư mục, giữ bản cũ |
| Mục | [4](#4-cách-a--chạy-gserver-bằng-docker-khuyến-nghị) | [5](#5-cách-b--chạy-gserver-bằng-systemd) |

## 1. Tổng quan

```
 Client Unity ──TCP 19180──▶ GServer (.NET 8)  ──3306──▶ MariaDB 10.4 (Docker)
                             └─ HTTP 8082 (API quản trị, CHỈ loopback)
```

| Thành phần | Công nghệ | Cổng | Mở ra Internet? |
|---|---|---|---|
| Game server | .NET 8 + ASP.NET Core runtime | TCP `19180` | **Có** — client nối vào đây |
| API quản trị | ASP.NET Core (cùng tiến trình) | HTTP `8082` | **Không** — có lệnh `shutdown`, mở SQL, tạo vật phẩm |
| Database | MariaDB **10.4** trong Docker | `3306` | **Không** |

3 database (tên phải khớp `App.config`): `gopettae_tae2` (game), `gopettae_gopet_web`
(tài khoản), `gp_log` (lịch sử). Chi tiết: [`docker/README.md`](../docker/README.md).

**Tắt máy chủ an toàn:** GServer bắt `SIGTERM`/`SIGINT` (`App/GracefulShutdown.cs`) và chạy
lệnh `shutdown` — lưu market, clan, dữ liệu người chơi online — rồi mới thoát. Nên
`docker stop` / `systemctl stop` đều an toàn, miễn là cho đủ thời gian (60 giây, đã cấu hình sẵn).
Log khi dừng đúng:

```
Nhận SIGTERM: đang lưu dữ liệu trước khi dừng máy chủ...
Đã lưu xong, thoát tiến trình.
```

## 2. Chuẩn bị máy Linux

Cấu hình gợi ý: 2 vCPU, 4 GB RAM (server bật Server GC + MariaDB buffer 512 MB), 20 GB đĩa.

```bash
# Docker + compose plugin
curl -fsSL https://get.docker.com | sudo sh
sudo systemctl enable --now docker
docker compose version

# Múi giờ máy (log, cron backup). Container GServer tự đặt TZ riêng.
sudo timedatectl set-timezone Asia/Ho_Chi_Minh

# Thư mục làm việc
sudo mkdir -p /opt/gopet && sudo chown $USER /opt/gopet
```

Chỉ đi theo Cách B mới cần cài thêm .NET và font — xem mục 5.1.

### 2.1 Đưa mã nguồn lên máy

Cần 3 thư mục, **giữ đúng cấu trúc** (compose tham chiếu `../SRCGOPETGOC/...`):

```
/opt/gopet/
├── docker/                       # docker-compose.yml, initdb/, gserver/server.json, .env
└── SRCGOPETGOC/
    ├── MariaDB_SQL/              # dump + migration
    └── GServer/                  # mã nguồn server (Cách A build image từ đây)
```

Cách gọn nhất là `git clone` repo vào `/opt/gopet` (repo có cả client Unity — nặng nhưng
tiện cập nhật bằng `git pull`). Hoặc chép từ máy Windows (Git Bash), bỏ build output:

```bash
rsync -av --exclude bin --exclude obj --exclude publish --exclude log --exclude '*.log' \
  docker SRCGOPETGOC/MariaDB_SQL SRCGOPETGOC/GServer \
  user@server:/opt/gopet/staging/
# rồi trên máy chủ sắp lại đúng cây thư mục ở trên:
#   mv /opt/gopet/staging/docker /opt/gopet/
#   mkdir -p /opt/gopet/SRCGOPETGOC && mv /opt/gopet/staging/{MariaDB_SQL,GServer} /opt/gopet/SRCGOPETGOC/
```

## 3. Database (MariaDB 10.4 trong Docker)

### 3.1 Vì sao phải đúng bản 10.4

`Data/top/TopPet.cs:30` dùng `JSON_VALUE(player.pets, '$[*].exp')`. MariaDB 10.11+ báo
`ERROR 4044` với wildcard, 10.4 thì chạy. **Không nâng bản** khi chưa sửa và test lại query.
10.4 đã hết hỗ trợ (6/2024) → 3306 chỉ bind loopback, không mở ra ngoài.

### 3.2 Khởi tạo

```bash
cd /opt/gopet/docker
cp .env.example .env
# Mật khẩu root MẠNH, không dùng lại mật khẩu dev:
sed -i "s/^MARIADB_ROOT_PASSWORD=.*/MARIADB_ROOT_PASSWORD=$(openssl rand -hex 24)/" .env
chmod 600 .env

docker compose up -d                 # KHÔNG có --profile server: chỉ bật database
docker compose logs -f mariadb       # chờ "[gopet-init] === Nạp xong ===" (1–2 phút), Ctrl+C
```

Dump chỉ nạp **một lần** khi volume `gopet-db-data` còn rỗng. Nạp lại từ đầu:
`docker compose down -v && docker compose up -d` (**xoá sạch dữ liệu**).

### 3.3 Chạy migration

Các file `migration-*.sql` KHÔNG tự chạy. Chạy theo thứ tự ngày, **bỏ file seed dữ liệu test**:

| File | Chạy trên production? |
|---|---|
| `migration-260919-mob-atk-def.sql` | Có |
| `migration-260921-remove-santa-npc.sql` | Có |
| `migration-260922-seed-social-data-gopettest.sql` | **Không** — dữ liệu thử giao diện cho tài khoản test |
| `migration-260924-battle-background.sql` | Có — **bắt buộc trước khi deploy** bản có khung cảnh màn đấu: thiếu 2 cột `BattleBg*` thì mọi lần lưu người chơi lỗi `Unknown column` |

```bash
cd /opt/gopet/SRCGOPETGOC/MariaDB_SQL
PASS=$(grep ^MARIADB_ROOT_PASSWORD= /opt/gopet/docker/.env | cut -d= -f2)
for f in migration-260919-mob-atk-def.sql migration-260921-remove-santa-npc.sql \n         migration-260924-battle-background.sql; do
  echo ">> $f"
  docker exec -i gopet-mariadb mysql -uroot -p"$PASS" --default-character-set=utf8mb4 gopettae_tae2 < "$f"
done
```

### 3.4 User riêng cho GServer (khuyến nghị)

Không để game server dùng `root`:

```bash
APP_PASS=$(openssl rand -hex 24)
docker exec -i gopet-mariadb mysql -uroot -p"$PASS" <<SQL
CREATE USER 'gopet'@'%' IDENTIFIED BY '$APP_PASS';
GRANT ALL PRIVILEGES ON gopettae_tae2.*      TO 'gopet'@'%';
GRANT ALL PRIVILEGES ON gopettae_gopet_web.* TO 'gopet'@'%';
GRANT ALL PRIVILEGES ON gp_log.*             TO 'gopet'@'%';
FLUSH PRIVILEGES;
SQL
# Cách A đọc 2 biến này từ docker/.env:
printf 'GOPET_DB_USER=gopet\nGOPET_DB_PASSWORD=%s\n' "$APP_PASS" >> /opt/gopet/docker/.env
```

`'%'` là cần thiết: GServer nối vào từ mạng Docker (IP nguồn `172.x`), không phải `localhost`.
Cổng 3306 vẫn chỉ bind `127.0.0.1` nên bên ngoài không vào được.

## 4. Cách A — Chạy GServer bằng Docker (khuyến nghị)

Các file liên quan:

| File | Vai trò |
|---|---|
| `SRCGOPETGOC/GServer/Dockerfile` | build bằng .NET SDK 8, chạy trên `aspnet:8.0`; đã cài font Arial, `TZ=Asia/Ho_Chi_Minh`, chạy bằng user `gopet` (uid 10001) |
| `SRCGOPETGOC/GServer/.dockerignore` | loại `bin/ obj/ log/ backup_sql/` khỏi build context |
| `docker/docker-compose.yml` → service `gserver` | profile `server`, chỉ publish cổng 19180, `stop_grace_period: 60s`, healthcheck TCP |
| `docker/gserver/server.json` | cấu hình dùng trong container (mount đè `config/server.json`) |

### 4.1 Cấu hình

`docker/gserver/server.json` đã đặt sẵn cho container:

- `gameBindAddress: "0.0.0.0"` — **bắt buộc** trong container, nếu để `127.0.0.1` thì Docker không chuyển được kết nối vào.
- `httpBindAddress: "127.0.0.1"` — API quản trị chỉ nghe bên trong container và **không** được publish.
- `enablePacketLog: false`, `isServerTest: false`.

Sửa file này ngay trên máy chủ nếu cần (giờ bảo trì, thông báo khi đăng nhập…), rồi
`docker compose --profile server restart gserver`.

Biến môi trường (trong `docker/.env`, compose tự đọc):

| Biến | Mặc định | Ghi chú |
|---|---|---|
| `MARIADB_ROOT_PASSWORD` | — | bắt buộc (mục 3.2) |
| `GOPET_DB_USER` | `root` | nên đặt `gopet` (mục 3.4) |
| `GOPET_DB_PASSWORD` | = `MARIADB_ROOT_PASSWORD` | mật khẩu của `GOPET_DB_USER` |
| `GOPET_GAME_PORT` | `19180` | cổng phía máy chủ mà client nối vào |

### 4.2 Build và chạy

```bash
cd /opt/gopet/docker
docker compose --profile server up -d --build     # lần đầu build ~3–5 phút
docker compose logs -f gserver
```

Khởi động đúng:

```
[DB] OK  GameConnectString -> gopettae_tae2@mariadb
[DB] OK  WebConnectString -> gopettae_gopet_web@mariadb
[DB] OK  LogConnectString -> gp_log@mariadb
... Cổng game nghe ở 0.0.0.0:19180 (MỌI giao diện mạng)
... Không có đầu vào bàn phím (stdin đã đóng). Ngừng đọc lệnh; máy chủ vẫn chạy bình thường.
```

Kiểm tra:

```bash
docker compose --profile server ps          # gserver: Up (healthy)
ss -ltn | grep -E ':19180|:3306|:8082'      # 19180 trên 0.0.0.0; 3306 trên 127.0.0.1; KHÔNG có 8082
```

Trong container: log ở volume `gopet-gserver-log`, backup SQL tự động ở `gopet-gserver-backup`.

### 4.3 Thao tác thường dùng

```bash
cd /opt/gopet/docker
docker compose --profile server stop gserver       # dừng an toàn (lưu dữ liệu, tối đa 60s)
docker compose --profile server restart gserver
docker compose --profile server logs --tail 200 gserver
docker exec -it gopet-gserver bash                 # vào container (user gopet)
```

**Không** dùng `docker kill` hay `docker compose down -t 0`: bỏ qua bước lưu.

Gọi API quản trị (chỉ nghe trong container):
`docker exec gopet-gserver curl -s http://127.0.0.1:8082/api/server/socketCount`
(image `aspnet` không có sẵn `curl`; nếu cần thì dùng Cách B hoặc thêm `curl` vào Dockerfile).

### 4.4 Cập nhật phiên bản mới

```bash
cd /opt/gopet
# 1. Backup DB trước (mục 8)
# 2. Lấy mã mới
git pull                      # hoặc rsync lại SRCGOPETGOC/GServer như mục 2.1
# 3. Giữ image cũ để rollback
docker tag gopet-gserver:latest gopet-gserver:prev
# 4. Build image mới rồi thay container (container cũ được dừng AN TOÀN, có lưu)
cd docker && docker compose --profile server up -d --build gserver
# 5. Chạy migration mới nếu có (mục 3.3)
docker compose --profile server logs -f gserver
```

Nếu bản mới thêm khoá vào `SRCGOPETGOC/GServer/config/server.json`, thêm khoá đó vào `docker/gserver/server.json`.

**Rollback:**

```bash
cd /opt/gopet/docker
docker compose --profile server stop gserver
docker tag gopet-gserver:prev gopet-gserver:latest
docker compose --profile server up -d --no-build gserver
```

(nếu migration đã đổi DB thì khôi phục thêm bản backup).

### 4.5 Build trên máy khác (máy chủ yếu / không có mạng tốt)

```bash
# trên máy build
cd SRCGOPETGOC/GServer && docker build -t gopet-gserver:latest .
docker save gopet-gserver:latest | gzip | ssh user@server 'gunzip | docker load'
# trên máy chủ
cd /opt/gopet/docker && docker compose --profile server up -d --no-build gserver
```

### 4.6 CI/CD tự động bằng GitHub Actions

`.github/workflows/gserver-ci-cd.yml`: mỗi lần merge/push vào `master` có đổi phần server
(`SRCGOPETGOC/GServer/`, `tests/GServer.Performance.Tests/`, `docker/`) thì:

1. **test** — chạy `tests/GServer.Performance.Tests` trên Ubuntu. Có test FAIL thì dừng, không deploy.
2. **deploy** — SSH vào máy chủ → `git merge --ff-only` mã mới → `docker/deploy-gserver.sh`:
   gắn tag `prev` cho image cũ, build image mới (build lỗi thì server cũ vẫn chạy), thay container
   (có lưu dữ liệu), chờ `healthy` tối đa 5 phút; không healthy thì **tự rollback** về `prev`.

Chạy tay: tab **Actions → GServer CI/CD → Run workflow**, hoặc trên máy chủ `bash docker/deploy-gserver.sh`.

**Cài đặt một lần:**

```bash
# Trên máy chủ, user deploy (nằm trong nhóm docker, sở hữu /opt/gopet):
# a) Cho máy chủ kéo được repo private: tạo deploy key CHỈ ĐỌC
ssh-keygen -t ed25519 -N '' -f ~/.ssh/github_gopet
cat >> ~/.ssh/config <<'EOF'
Host github.com
  IdentityFile ~/.ssh/github_gopet
EOF
cat ~/.ssh/github_gopet.pub     # → GitHub repo Settings → Deploy keys → Add (không tick write)
git clone git@github.com:TuyenHa/gopet.git /opt/gopet   # nếu chưa clone (rồi làm mục 3)

# b) Khoá để GitHub Actions SSH vào máy chủ
ssh-keygen -t ed25519 -N '' -f ~/gh-actions-deploy
cat ~/gh-actions-deploy.pub >> ~/.ssh/authorized_keys
cat ~/gh-actions-deploy          # → secret DEPLOY_SSH_KEY, rồi XOÁ file này
```

GitHub repo → **Settings → Environments → New environment `production`**, thêm secrets:

| Secret | Giá trị |
|---|---|
| `DEPLOY_HOST` | IP / tên miền máy chủ |
| `DEPLOY_USER` | user SSH ở trên |
| `DEPLOY_SSH_KEY` | nội dung private key `~/gh-actions-deploy` |
| `DEPLOY_KNOWN_HOSTS` | kết quả `ssh-keyscan -p <port> <host>` (chạy từ máy tin cậy) |
| `DEPLOY_PORT` | tuỳ chọn, mặc định `22` |

Variable tuỳ chọn `DEPLOY_PATH` (mặc định `/opt/gopet`). Trong environment `production` có thể
bật **Required reviewers** nếu muốn duyệt tay trước mỗi lần deploy.

Lưu ý:
- Workflow **không** chạy migration SQL (mục 3.3) — bản có migration thì chạy tay trước khi merge,
  hoặc ngay sau khi deploy.
- Sửa file đang được git theo dõi ngay trên máy chủ (vd. `docker/gserver/server.json`) mà trùng chỗ
  với commit mới thì `git merge --ff-only` báo lỗi và deploy dừng, server cũ vẫn chạy. Nên sửa
  cấu hình qua commit thay vì sửa tay trên máy chủ.

### 4.7 Email xác thực (tuỳ chọn)

Cấu hình SMTP nằm ở `appSettings/email-serivce-config` trong `App.config` (trong image là
`/app/Gopet.dll.config`). Không sửa file trong repo; chép ra, điền thông tin, rồi mount đè
bằng cách thêm vào `volumes` của service `gserver`:

```yaml
      - ./gserver/Gopet.dll.config:/app/Gopet.dll.config:ro
```

File `docker/gserver/Gopet.dll.config` chứa mật khẩu SMTP → **không commit** (thêm vào `.gitignore`).

## 5. Cách B — Chạy GServer bằng systemd

### 5.1 Cài runtime và font

```bash
# .NET 8: cần cả ASP.NET Core runtime (Gopet.runtimeconfig.json đòi Microsoft.AspNetCore.App)
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0
dotnet --list-runtimes     # phải có Microsoft.AspNetCore.App 8.x và Microsoft.NETCore.App 8.x

# Font Arial cho captcha (Data/User/GopetCaptcha.cs). SixLabors tìm đúng tên "Arial",
# không dùng alias của fontconfig → Liberation Sans KHÔNG thay được.
sudo add-apt-repository -y multiverse
sudo apt install -y ttf-mscorefonts-installer fontconfig      # đồng ý EULA
fc-list | grep -i arial

# User chạy dịch vụ
sudo useradd --system --create-home --home-dir /opt/gopet/server --shell /usr/sbin/nologin gopet
```

Ubuntu 22.04 bản cũ chưa có `aspnetcore-runtime-8.0` trong kho mặc định → cài theo
<https://learn.microsoft.com/dotnet/core/install/linux-ubuntu>.

### 5.2 Build trên Windows và chép sang

```bash
cd SRCGOPETGOC/GServer
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish   # = publish-linux.bat
scp -r publish/* user@server:/tmp/gopet-publish/
```

Dùng **Release** (Debug bật `DEBUG_LOG`). `publish/` phải có `Gopet.dll`,
`Gopet.runtimeconfig.json`, `Gopet.dll.config` (= `App.config`), `config/`, `assets/`.

```bash
# trên máy chủ
sudo rsync -a /tmp/gopet-publish/ /opt/gopet/server/
sudo mkdir -p /opt/gopet/server/{log,backup_sql}
sudo chown -R gopet:gopet /opt/gopet/server
```

### 5.3 Cấu hình

`/opt/gopet/server/config/server.json` — không đè được bằng biến môi trường, sửa trực tiếp:
`gameBindAddress: "0.0.0.0"`, giữ `httpBindAddress: "127.0.0.1"`, `enablePacketLog: false`.

Mật khẩu DB qua biến môi trường (thứ tự ưu tiên trong `Manager/MYSQLManager.cs`:
`GOPET_<TÊN>_CONNSTR` → `GOPET_DB_*` → `App.config`):

```bash
sudo mkdir -p /etc/gopet
sudo tee /etc/gopet/gopet.env >/dev/null <<'EOF'
GOPET_DB_HOST=127.0.0.1
GOPET_DB_PORT=3306
GOPET_DB_USER=gopet
GOPET_DB_PASSWORD=<APP_PASS ở mục 3.4>
DOTNET_CLI_TELEMETRY_OPTOUT=1
EOF
sudo chown root:gopet /etc/gopet/gopet.env && sudo chmod 640 /etc/gopet/gopet.env
```

### 5.4 Dịch vụ systemd

```bash
sudo tee /etc/systemd/system/gopet.service >/dev/null <<'EOF'
[Unit]
Description=goPet game server
After=network-online.target docker.service
Wants=network-online.target
Requires=docker.service

[Service]
User=gopet
Group=gopet
# BẮT BUỘC: config/, assets/, log/, backup_sql/ đều tính từ thư mục làm việc.
WorkingDirectory=/opt/gopet/server
EnvironmentFile=/etc/gopet/gopet.env
# Chờ MariaDB sẵn sàng — không thì lỗi kết nối giữa lúc nạp template.
ExecStartPre=/bin/sh -c 'for i in $(seq 1 60); do docker inspect -f "{{.State.Health.Status}}" gopet-mariadb 2>/dev/null | grep -q healthy && exit 0; sleep 2; done; exit 1'
ExecStart=/usr/bin/dotnet /opt/gopet/server/Gopet.dll
StandardInput=null
# SIGTERM → server tự lưu rồi thoát (App/GracefulShutdown.cs); cho nó 60s.
KillSignal=SIGTERM
TimeoutStopSec=60
Restart=on-failure
RestartSec=5
LimitNOFILE=65535

[Install]
WantedBy=multi-user.target
EOF

sudo usermod -aG docker gopet        # để ExecStartPre đọc trạng thái container
sudo systemctl daemon-reload
sudo systemctl enable --now gopet
journalctl -u gopet -f
```

Cập nhật bản mới: `systemctl stop gopet` → `cp -a /opt/gopet/server /opt/gopet/server.prev`
→ rsync bản mới **bỏ qua** `config/ Gopet.dll.config log/ backup_sql/` → `systemctl start gopet`.
Rollback: đổi lại `server.prev`.

Muốn gõ lệnh console của server (`help`, `shutdown`…): dừng service rồi chạy tay trong `tmux`:
`sudo -u gopet bash -c 'set -a; . /etc/gopet/gopet.env; cd /opt/gopet/server; dotnet Gopet.dll'`.

## 6. Tường lửa

```bash
sudo ufw allow OpenSSH
sudo ufw allow 19180/tcp
sudo ufw enable
```

- **Không** mở 8082 và 3306.
- Lưu ý: cổng Docker **publish** đi thẳng qua iptables, **không** bị `ufw` chặn. Vì vậy
  không bao giờ đổi compose thành `3306:3306` hay publish 8082 — chỉ `127.0.0.1:3306` và `19180` như hiện tại.
- Máy chủ cloud (AWS/GCP/Vultr…): mở thêm TCP 19180 trong Security Group / firewall của nhà cung cấp.

API quản trị từ máy mình (Cách B): `ssh -L 8082:127.0.0.1:8082 user@server` rồi mở `http://127.0.0.1:8082`.

## 7. Trỏ client Unity về máy chủ

Địa chỉ server là field serialize trong scene, mặc định `127.0.0.1:19180`:

- `Assets/Scripts/Runtime/GopetBootstrap.cs` → `host`, `port`
- `Assets/Scripts/Runtime/GopetClient.cs` → `host`, `port`

Đổi `host` thành IP công khai / tên miền **trong Inspector của GameObject bootstrap** (giá trị
trong scene đè mặc định trong code), `port` = `GOPET_GAME_PORT`, rồi build lại client.

## 8. Sao lưu

GServer tự dump SQL định kỳ (`Runtime/DBBackup.cs`) vào `backup_sql/` (Cách A: volume
`gopet-gserver-backup`). Thêm backup ngoài tiến trình game, giữ 14 ngày:

```bash
sudo mkdir -p /opt/gopet/backups
sudo tee /etc/cron.d/gopet-backup >/dev/null <<'EOF'
# 03:30 mỗi ngày
30 3 * * * root PASS=$(grep ^MARIADB_ROOT_PASSWORD= /opt/gopet/docker/.env | cut -d= -f2); docker exec gopet-mariadb mysqldump -uroot -p"$PASS" --single-transaction --databases gopettae_tae2 gopettae_gopet_web gp_log | gzip > /opt/gopet/backups/db-$(date +\%F).sql.gz; find /opt/gopet/backups -type f -mtime +14 -delete
EOF
```

Khôi phục:
`gunzip -c db-YYYY-MM-DD.sql.gz | docker exec -i gopet-mariadb mysql -uroot -p"$PASS"`
(dừng GServer trước). Nên chép backup ra ngoài máy chủ (S3, máy khác…).

## 9. Xử lý sự cố

| Triệu chứng | Nguyên nhân / cách xử lý |
|---|---|
| `Access denied for user '...'@'172.x'` | Sai `GOPET_DB_USER`/`GOPET_DB_PASSWORD` (Cách A: `docker/.env`; Cách B: `/etc/gopet/gopet.env`), hoặc user chưa tạo với host `'%'` |
| gserver không lên, log `dependency failed` | MariaDB chưa `healthy` — `docker compose logs mariadb` |
| Client không kết nối được | `gameBindAddress` còn `127.0.0.1`; thiếu `ufw allow 19180/tcp`; Security Group cloud chưa mở; client còn trỏ `127.0.0.1` |
| `Could not find file '.../config/server.json'` (Cách B) | Sai `WorkingDirectory` — phải là thư mục chứa `Gopet.dll` |
| `You must install ... Microsoft.AspNetCore.App` (Cách B) | Cài `aspnetcore-runtime-8.0`, không chỉ `dotnet-runtime` |
| Lỗi captcha / `FontFamilyNotFoundException: Arial` (Cách B) | Cài `ttf-mscorefonts-installer` (mục 5.1). Image Docker đã có sẵn |
| Build image lỗi ở bước `ttf-mscorefonts-installer` | Gói tải font từ sourceforge lúc build — thử build lại, hoặc build trên máy khác rồi `docker save/load` (mục 4.5) |
| `ERROR 4044 Wildcards or range in JSON path` | DB không phải MariaDB 10.4 — mục 3.1 |
| `FileNotFoundException` với file trong `assets/` (Windows thì chạy) | **Linux phân biệt hoa/thường** tên file — sửa tên file hoặc đường dẫn cho khớp |
| Mất dữ liệu sau khi restart | Bị giết trước khi lưu xong: dùng `docker kill` / `down -t 0`, hoặc giảm `stop_grace_period` / `TimeoutStopSec`. Log đúng phải có `Đã lưu xong, thoát tiến trình.` |
| Giờ bảo trì lệch | Cách B: chưa `timedatectl set-timezone`. Cách A: `TZ` trong Dockerfile |

## 10. Checklist trước khi mở cho người chơi

- [ ] `docker/.env` quyền 600 (Cách B thêm `/etc/gopet/gopet.env` 640), không nằm trong git
- [ ] Mật khẩu root MariaDB và user `gopet` là mật khẩu mới, không dùng lại của môi trường dev
- [ ] `ss -ltn`: 19180 mở; 3306 chỉ `127.0.0.1`; 8082 không nghe trên host (Cách A) / chỉ `127.0.0.1` (Cách B)
- [ ] Tường lửa chỉ mở SSH và 19180
- [ ] `enablePacketLog=false`, `isServerTest=false`
- [ ] Đã thử `stop` một lần và thấy log `Đã lưu xong, thoát tiến trình.`
- [ ] **Dump `web_db.sql` chứa dữ liệu người dùng thật** (email, hash mật khẩu, IP). Máy chủ cho người chơi mới thì cân nhắc xoá dữ liệu người dùng cũ trước khi mở
- [ ] Backup tự động đã chạy (mục 8) và đã thử khôi phục một lần
