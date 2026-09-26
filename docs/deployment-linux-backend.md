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
| Trang quản trị webadmin | Next.js (Docker) | `443` qua Caddy | **Có** — chỉ IP trong allowlist, xem [mục 11](#11-trang-quản-trị-webadmin-nextjs--caddy) |

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

Image `gserver` và `webadmin` được GitHub Actions build sẵn trên GHCR (mục 4.6), nên với
Cách A máy chủ **không cần mã nguồn** — chỉ cần 2 thư mục, **giữ đúng cấu trúc** (compose
tham chiếu `../SRCGOPETGOC/MariaDB_SQL`):

```
/opt/gopet/
├── docker/                       # docker-compose.yml, initdb/, gserver/server.json, caddy/, .env
└── SRCGOPETGOC/
    └── MariaDB_SQL/              # dump + migration
```

Repo chứa cả client Unity, webadmin, tài liệu… Dùng **partial clone + sparse-checkout**
để máy chủ chỉ tải và chỉ hiện đúng 2 thư mục trên — thư mục Unity không xuất hiện, và nội
dung file của nó cũng không bao giờ được tải về:

```bash
git clone --filter=blob:none --sparse git@github.com:TuyenHa/gopet.git /opt/gopet
cd /opt/gopet
git sparse-checkout set docker SRCGOPETGOC/MariaDB_SQL
```

- `--filter=blob:none`: chỉ tải lịch sử commit; nội dung file chỉ tải khi cần đưa ra đĩa.
- `sparse-checkout set`: chỉ các thư mục liệt kê (và file ở gốc repo) có trên đĩa.
- `git fetch` + `git merge --ff-only` của CI (mục 4.6) chạy bình thường; commit chỉ đổi
  Unity/webadmin không làm máy chủ tải thêm gì.
- Cần thêm thư mục (vd. Cách B build GServer ngay trên máy): `git sparse-checkout add
  SRCGOPETGOC/GServer`. Xem đang lấy gì: `git sparse-checkout list`.

Máy chủ đã clone **đầy đủ** từ trước: chạy `git sparse-checkout set docker
SRCGOPETGOC/MariaDB_SQL` là thư mục thừa biến mất khỏi đĩa, nhưng dữ liệu cũ vẫn nằm trong
`.git`. Muốn lấy lại dung lượng thì clone lại như trên (nhớ chép `docker/.env` và
`docker/gserver/Gopet.dll.config` ra ngoài trước).

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

Mọi thay đổi schema nằm ở `SRCGOPETGOC/MariaDB_SQL/migration-<yymmdd>-<mô-tả>.sql`, chạy bằng
`docker/migrate-db.sh`. Mỗi lần deploy (mục 4.6), CI/CD tự gọi script này **sau khi build image và trước
khi thay container**:

- Chạy các file **chưa chạy**, theo thứ tự tên file. File đã chạy được ghi vào bảng
  `gopettae_tae2.schema_migrations` và **không bao giờ chạy lại**. Muốn sửa gì thì viết file mới,
  đừng sửa file cũ.
- Trước khi chạy có backup cả 3 DB vào `/opt/gopet/backups/pre-migrate-*.sql.gz`, vì DDL của MariaDB
  không rollback được.
- Một file lỗi thì dừng: không chạy các file sau, không deploy, server cũ vẫn chạy.
- Tên file có chữ `seed` (dữ liệu thử cho tài khoản test) thì **không bao giờ** chạy tự động.
- Mặc định chạy trên `gopettae_tae2`. File cho DB khác thì ghi ở dòng đầu, ví dụ `-- database: gp_log`.

**Danh sách migration (production)**:

| Tên file | Mô tả | Database |
|---|---|---|
| `migration-260919-mob-atk-def.sql` | Thêm cột `atk`, `def` vào `gopet_mob` | gopettae_tae2 |
| `migration-260924-battle-background.sql` | Khung cảnh trận đấu: `BattleBgOwned`, `BattleBgSelected` vào `player` | gopettae_tae2 |
| `migration-260924-equip-durability-repair.sql` | Độ bền trang bị: item 1000091, NPC -42, set NPC map 11 | gopettae_tae2 |

```bash
cd /opt/gopet
bash docker/migrate-db.sh              # chạy tay các migration còn thiếu
bash docker/migrate-db.sh --baseline   # xem bên dưới
```

**Máy chủ đã từng chạy migration bằng tay** (trước khi có script này): chạy `--baseline` **đúng một
lần** trước lần deploy tự động đầu tiên. Lệnh này ghi mọi file hiện có là đã chạy nhưng không thực thi
file nào. Nếu quên bước này, lần deploy đầu sẽ dừng ở lỗi `Duplicate column` (server cũ vẫn chạy):
khi đó kiểm tra xem DB đã có đủ các thay đổi chưa, rồi mới chạy baseline. Nếu thiếu file nào thì chạy
file đó bằng tay trước, rồi mới baseline.

DB mới nạp từ dump (mục 3.2) thì **không** chạy baseline: script sẽ tự chạy hết các migration.

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
| `docker/deploy-service.sh` | pull image từ GHCR, chạy migration, thay container, rollback nếu không healthy |

Image chạy là `ghcr.io/<GHCR_OWNER>/gopet-gserver:<GSERVER_TAG>` — build bởi GitHub Actions
(mục 4.6). Máy chủ **không build** .NET; `build:` trong compose chỉ để máy dev thử container
(`docker compose --profile server build gserver`).

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
| `GHCR_OWNER` | — | bắt buộc: owner GitHub **viết thường** (vd. `tuyenha`) |
| `GSERVER_TAG` | `latest` | `deploy-service.sh gserver` tự cập nhật (git sha 12 ký tự) sau mỗi lần deploy thành công |

### 4.2 Chạy lần đầu

Đăng nhập GHCR một lần (PAT chỉ `read:packages`, xem mục 11.4), rồi:

```bash
cd /opt/gopet/docker
bash deploy-service.sh gserver        # pull image, migration, khởi động, chờ healthy
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

CI tự làm sau mỗi lần merge (mục 4.6). Chạy tay trên máy chủ:

```bash
cd /opt/gopet
git pull                                        # lấy migration + cấu hình docker mới
bash docker/deploy-service.sh gserver <git-sha> # bỏ tham số = dùng lại GSERVER_TAG trong .env
```

Script: pull `gopet-gserver:<tag>` (pull lỗi thì dừng, server cũ vẫn chạy) → migration (mục
3.3, có backup trước) → thay container (container cũ được dừng an toàn, có lưu dữ liệu) → chờ
healthy; không healthy thì tự chạy lại tag cũ. Thành công mới ghi `GSERVER_TAG` vào `.env`
và lưu tag cũ vào `docker/.gserver-prev-tag`.

Nếu bản mới thêm khoá vào `SRCGOPETGOC/GServer/config/server.json`, thêm khoá đó vào `docker/gserver/server.json`.

**Rollback tay:**

```bash
cd /opt/gopet/docker
bash deploy-service.sh gserver "$(cat .gserver-prev-tag)"
```

(nếu migration đã đổi DB thì khôi phục thêm bản backup).

### 4.5 Không dùng GitHub Actions

Build ở máy có Docker rồi đẩy lên GHCR bằng PAT quyền `write:packages`, sau đó deploy như 4.4:

```bash
cd SRCGOPETGOC/GServer
docker build -t ghcr.io/<owner>/gopet-gserver:manual-1 .
docker push ghcr.io/<owner>/gopet-gserver:manual-1
# trên máy chủ
bash /opt/gopet/docker/deploy-service.sh gserver manual-1
```

### 4.5b Chuyển máy chủ đang build tại chỗ sang GHCR (làm một lần)

Máy chủ cài theo bản cũ của tài liệu này (clone đầy đủ, `docker compose up -d --build`):

1. Thêm vào `docker/.env`:
   ```
   GHCR_OWNER=<owner-github-viết-thường>
   GSERVER_TAG=latest
   ```
2. Đăng nhập GHCR bằng PAT chỉ `read:packages` (mục 11.4): `docker login ghcr.io`.
3. Bỏ thư mục thừa khỏi đĩa (mục 2.1): `cd /opt/gopet && git sparse-checkout set docker
   SRCGOPETGOC/MariaDB_SQL`. Muốn giải phóng dung lượng `.git` thì clone lại theo 2.1 —
   chép `docker/.env` và `docker/gserver/Gopet.dll.config` ra ngoài trước.
4. Đợi CI chạy xong job `build-push` ít nhất một lần (image `gopet-gserver` đã có trên
   GHCR), rồi `bash docker/deploy-service.sh gserver`. Chạy trước khi có image thì script
   báo pull lỗi và giữ nguyên server cũ.
5. Image `gopet-gserver:latest` / `:prev` build tại chỗ lúc trước không còn dùng:
   `docker image rm gopet-gserver:latest gopet-gserver:prev`.

### 4.6 CI/CD tự động bằng GitHub Actions

`.github/workflows/gserver-ci-cd.yml` — **một** workflow dùng chung cho gserver **và**
webadmin (mục 11), một concurrency group `gopet-deploy` (hai lần deploy không chồng nhau).
Job `changes` (dorny/paths-filter) quyết định lần push này đổi gserver và/hoặc webadmin,
các job sau chỉ chạy cho phần thực sự đổi:

1. **test-gserver** — chạy `tests/GServer.Performance.Tests` trên Ubuntu (chỉ khi đổi
   `SRCGOPETGOC/GServer/**` hoặc file docker liên quan gserver). Test FAIL thì dừng, không deploy.
2. **test-webadmin** — `npm ci && npm run lint && npx tsc --noEmit && npm test && npm run
   build` trong `webadmin/` (chỉ khi đổi `webadmin/**`).
3. **build-push** (matrix gserver/webadmin) — build `SRCGOPETGOC/GServer/Dockerfile` và/hoặc
   `webadmin/Dockerfile` cho service đã đổi, push `ghcr.io/<owner>/gopet-<service>:<sha12>` +
   `:latest` (có cache layer giữa các lần chạy). Test lỗi thì không build.
4. **deploy** — SSH vào máy chủ → `git merge --ff-only` (chỉ `docker/` + migration, mục 2.1)
   → gọi `docker/deploy-service.sh <gserver|webadmin> <sha12>` cho ĐÚNG service đã đổi (có
   thể cả hai). Máy chủ **chỉ pull**, không build: pull lỗi thì dừng, container cũ vẫn chạy;
   sau đó migration (mục 3.3, có khoá `flock`), thay ĐÚNG container của service đó (không
   đụng service kia), chờ `healthy` tối đa 5 phút; không healthy thì **tự rollback** về tag
   đang chạy trước đó (lưu trong `docker/.<service>-prev-tag`).

Chạy tay: tab **Actions → GServer + webadmin CI/CD → Run workflow** (deploy cả hai service),
hoặc trên máy chủ `bash docker/deploy-service.sh <gserver|webadmin> [tag]`
(`docker/deploy-gserver.sh` cũ vẫn chạy được — chỉ là wrapper 1 dòng).

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
# nếu chưa clone: clone sparse như mục 2.1 (rồi làm mục 3)

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
- Migration SQL chạy tự động (mục 3.3). Nếu phải rollback image thì schema **không** tự quay lại;
  migration chỉ thêm cột có giá trị mặc định thì code cũ vẫn chạy được, còn không thì khôi phục từ
  file `pre-migrate-*.sql.gz`.
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
sudo ufw allow 80/tcp        # chỉ cần nếu chạy webadmin (mục 11) — Caddy xin chứng chỉ qua đây
sudo ufw allow 443/tcp       # webadmin qua HTTPS (mục 11)
sudo ufw enable
```

- **Không** mở 8082 và 3306.
- Lưu ý: cổng Docker **publish** đi thẳng qua iptables, **không** bị `ufw` chặn. Vì vậy
  không bao giờ đổi compose thành `3306:3306` hay publish 8082 — chỉ `127.0.0.1:3306` và `19180` như hiện tại.
  Service `webadmin` (mục 11) cũng **không publish cổng** — chỉ `caddy` (`80`/`443`) và `gserver` (`19180`) publish ra host.
- Máy chủ cloud (AWS/GCP/Vultr…): mở thêm TCP 19180 (và 80/443 nếu có webadmin) trong Security Group / firewall của nhà cung cấp.

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

## 11. Trang quản trị webadmin (Next.js + Caddy)

Chạy trong cùng compose stack (profile `server`), **3 container**: `mariadb` (mục 3),
`gserver` (mục 4), `webadmin` (Next.js quản trị) + `caddy` (reverse proxy công khai duy
nhất). Không nhét Next.js và .NET vào một container — khác runtime, không
restart/rollback độc lập được, healthcheck lẫn nhau.

```
Internet ──443──▶ caddy (Let's Encrypt tự động, chặn IP ngoài allowlist)
                    └──▶ webadmin:3000 (mạng compose — KHÔNG publish ra host)
```

File liên quan: `webadmin/Dockerfile` (multi-stage, non-root, `output: "standalone"` nên
image nhỏ), `docker/caddy/Caddyfile`, service `webadmin` + `caddy` trong
`docker/docker-compose.yml`.

### 11.1 DNS + firewall

1. Trỏ một domain con (vd. `admin.gopet.example.com`) về IP máy chủ (bản ghi A).
2. Mở TCP `80` và `443` (mục 6) — Caddy cần `80` để xin chứng chỉ Let's Encrypt (ACME
   HTTP-01) và `443` cho HTTPS. **Không** publish thẳng cổng `3000` của webadmin ra host.
3. `WEBADMIN_ALLOWED_IPS` trong `docker/.env`: danh sách IP/CIDR cách nhau khoảng trắng
   được phép truy cập (vd. IP tĩnh văn phòng, VPN). IP ngoài danh sách nhận `403` ngay ở
   Caddy, không tới được Next.js. Đổi danh sách xong: `docker compose --profile server
   restart caddy` (không ảnh hưởng gserver/webadmin).

Test không có domain thật: đặt `WEBADMIN_DOMAIN=localhost` — Caddy tự dùng chứng chỉ nội
bộ (self-signed) thay vì xin Let's Encrypt (xem comment trong `docker/caddy/Caddyfile`).

### 11.2 Tạo user DB `gopet_admin`

Quyền tối thiểu, tách biệt với user `gopet`/`root` của gserver (mục 3.4) — theo bảng,
không theo toàn bộ database:

```bash
cd /opt/gopet
PASS=$(grep '^MARIADB_ROOT_PASSWORD=' docker/.env | cut -d= -f2-)
WEBADMIN_PASS=$(openssl rand -hex 24)
docker exec -i -e MYSQL_PWD="$PASS" gopet-mariadb mysql -uroot -e \
  "CREATE USER IF NOT EXISTS 'gopet_admin'@'%' IDENTIFIED BY '$WEBADMIN_PASS';"

# Chạy migration TRƯỚC (bảng admin_audit_log phải tồn tại — grants có INSERT vào bảng này)
bash docker/migrate-db.sh

docker exec -i -e MYSQL_PWD="$PASS" gopet-mariadb mysql -uroot < docker/webadmin-grants.sql
printf 'WEBADMIN_DB_PASSWORD=%s\n' "$WEBADMIN_PASS" >> docker/.env
```

`docker/webadmin-grants.sql` chạy lại nhiều lần an toàn (GRANT cộng dồn) — chạy lại sau
khi thêm bảng mới vào registry (phase 7) và thêm dòng GRANT tương ứng.

### 11.3 Biến môi trường (`docker/.env`)

| Biến | Bắt buộc | Ghi chú |
|---|---|---|
| `WEBADMIN_DB_USER` | không (mặc định `gopet_admin`) | user tạo ở mục 11.2 |
| `WEBADMIN_DB_PASSWORD` | **có** | mật khẩu user ở mục 11.2 |
| `WEBADMIN_SESSION_SECRET` | **có** | `openssl rand -base64 48`, ≥ 32 ký tự |
| `WEBADMIN_SUPERADMIN_USER_IDS` | không (mặc định `1`) | `user_id` được quyền super-admin, cách nhau dấu phẩy — tài khoản admin gốc là `1` |
| `WEBADMIN_DOMAIN` | **có** | domain đã trỏ DNS (mục 11.1), hoặc `localhost` để test |
| `WEBADMIN_ALLOWED_IPS` | **có** | IP/CIDR cách nhau khoảng trắng (mục 11.1) |
| `GHCR_OWNER` | **có** | owner GitHub, **viết thường** (vd. `tuyenha`) — ghcr.io không nhận hoa |
| `WEBADMIN_TAG` | không (mặc định `latest`) | `deploy-service.sh webadmin` tự cập nhật (git sha) mỗi lần deploy qua CI |

### 11.4 Đăng nhập GHCR trên máy chủ (một lần)

Image `gopet-gserver` và `gopet-webadmin` build + push trên GitHub Actions (job
`build-push`, `.github/workflows/gserver-ci-cd.yml`, dùng `GITHUB_TOKEN` sẵn có, quyền
`packages: write`). Máy chủ **chỉ pull, không build** — cần đăng nhập GHCR bằng Personal
Access Token (PAT) quyền `read:packages` (KHÔNG dùng token `write`/`admin` trên máy chủ).
Để package ở chế độ **private** (mặc định với repo private) — image chứa mã server:

```bash
# GitHub → Settings → Developer settings → Personal access tokens (classic hoặc fine-grained)
# → scope "read:packages" — chỉ đọc, không thể push đè image
docker login ghcr.io -u <github-username>
# Password: dán PAT (không phải mật khẩu GitHub)
```

Đăng nhập một lần là đủ (Docker lưu credential ở `~/.docker/config.json`);
`deploy-service.sh` chỉ gọi `docker compose pull <service>`, không cần đăng nhập lại.

### 11.5 Deploy + rollback

```bash
cd /opt/gopet
docker compose --profile server up -d          # lần đầu: dựng đủ mariadb + gserver + webadmin + caddy
docker compose --profile server ps             # cả 4 container đều "healthy"
```

Deploy bản mới (CI tự làm sau khi build-push xong; chạy tay cũng được):

```bash
bash docker/deploy-service.sh webadmin <git-sha-hoặc-tag>   # bỏ tham số = dùng lại WEBADMIN_TAG hiện có trong .env
```

Script: chạy `migrate-db.sh` (có khoá `flock`, mục 3.3) → `compose pull webadmin` →
`compose up -d --no-build webadmin` → chờ healthy tối đa 5 phút → **không** healthy thì tự
đổi `WEBADMIN_TAG` về tag đang chạy trước đó (lưu sẵn trong `docker/.webadmin-prev-tag`,
đọc trực tiếp từ container chứ không phải từ `.env`) và tái tạo container. Deploy webadmin
**không rebuild/restart gserver** — người chơi đang online không bị ảnh hưởng (và ngược
lại, deploy gserver không đụng tới webadmin).

Rollback tay (khi tự động thất bại):

```bash
cd /opt/gopet/docker
cat .webadmin-prev-tag                      # tag lần deploy thành công gần nhất
sed -i "s/^WEBADMIN_TAG=.*/WEBADMIN_TAG=$(cat .webadmin-prev-tag)/" .env
docker compose --profile server up -d --no-build --force-recreate webadmin
```

### 11.6 Múi giờ (TZ) — quyết định

`docker-compose.yml` đặt `TZ=Asia/Ho_Chi_Minh` cho **cả 3** container (`mariadb`,
`gserver`, `webadmin`) và MariaDB chạy thêm `--default-time-zone=+07:00`.

Trước khi bật, đã kiểm tra (phase 11):

- **GServer đã chạy giờ VN từ trước** (`SRCGOPETGOC/GServer/Dockerfile` có sẵn `ENV
  TZ=Asia/Ho_Chi_Minh`) — mọi mốc tính bằng `DateTime.Now` của .NET (bảo trì định kỳ
  `AutoMaintenance.cs`/`hourMaintenance` trong `server.json`, sự kiện theo ngày, quà hằng
  ngày...) **không đổi hành vi**, vì container gserver vốn đã ở múi giờ VN. Đặt lại `TZ`
  trong compose chỉ để hiển thị nhất quán, không đổi giá trị.
- **MariaDB trước phase 11 không đặt time zone** (mặc định `SYSTEM`, tương đương UTC của
  base image) — đây là thay đổi thật: `NOW()`/`CURRENT_TIMESTAMP()` phía DB từ giờ tính
  theo giờ VN.
- Rà toàn bộ schema (`SRCGOPETGOC/MariaDB_SQL/*.sql`): hầu hết cột thời gian là
  **`datetime`** (lưu giá trị đúng như app truyền vào, KHÔNG tự quy đổi khi đổi time zone
  của server — vd. `gift_code.expire`, `letter.time`, `player.loginDate`). Chỉ có **2 cột
  kiểu `timestamp` thật** (MariaDB lưu nội bộ theo UTC rồi quy đổi hiển thị theo time zone
  phiên): `payment.time_create` và `user.update_date` — đọc lại các dòng **đã có sẵn** ở
  2 cột này sau khi đổi time zone sẽ hiển thị lệch múi giờ so với trước (giá trị tuyệt đối
  không đổi, chỉ hiển thị khác).
- Các cột `datetime` dùng `DEFAULT current_timestamp()` mà code không truyền giá trị rõ
  ràng: dòng tạo **trước** khi đổi cấu hình mang giờ UTC, dòng tạo **sau** mang giờ VN —
  lệch một lần, không tự khớp lại (bản chất `datetime` không quy đổi).

**Quyết định:** chấp nhận lệch 7h một lần cho dữ liệu hiện có (môi trường vẫn là dữ liệu
thử/test theo `docker/README.md`), **không viết migration `+7h`** cho các cột trên — chi
phí một migration dữ liệu không tương xứng với dữ liệu test. Nếu triển khai với dữ liệu
thật về sau, cân nhắc migrate 2 cột `timestamp` (`payment.time_create`, `user.update_date`)
và mọi cột `datetime` được ghi bằng `DEFAULT current_timestamp()` phía DB trước thời điểm
đổi cấu hình.
