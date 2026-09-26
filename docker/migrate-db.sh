#!/usr/bin/env bash
# Chạy các file SRCGOPETGOC/MariaDB_SQL/migration-*.sql CHƯA chạy, theo thứ tự tên file.
# docker/deploy-service.sh gọi script này sau khi build/pull image, trước khi thay container.
#
#   bash docker/migrate-db.sh              # chạy migration còn thiếu
#   bash docker/migrate-db.sh --baseline   # chỉ ĐÁNH DẤU mọi file là đã chạy, không chạy
#
# --baseline dùng MỘT LẦN trên máy chủ có DB đã chạy tay các migration từ trước, để
# chúng không bị chạy lại (ALTER ... ADD COLUMN chạy lần hai sẽ lỗi).
#
# Quy ước file (docs/deployment-linux-backend.md, mục 3.3):
#   - tên có chữ "seed"          → dữ liệu thử, KHÔNG BAO GIỜ chạy tự động
#   - dòng "-- database: <tên>"  → chạy trên DB đó; không có thì gopettae_tae2
#
# Sổ ghi: bảng gopettae_tae2.schema_migrations. Mỗi file chạy đúng một lần; sửa file
# đã chạy KHÔNG có tác dụng — muốn đổi thì viết file migration mới.
#
# Toàn bộ logic bọc trong flock /tmp/gopet-migrate.lock: deploy-service.sh gọi script
# này cho CẢ gserver lẫn webadmin, hai lần deploy chồng nhau (CI + chạy tay, hoặc 2 lần
# CI xếp hàng) sẽ KHÔNG chạy migrate song song — lần sau chờ lần trước xong.
# Máy dev Windows (Git Bash/MSYS) thường không có lệnh `flock` → tự bỏ qua khoá, chỉ in
# cảnh báo, vẫn chạy migration bình thường (dev không có 2 tiến trình deploy chồng nhau).
set -euo pipefail

main() {
  cd "$(dirname "$0")"
  MIGRATIONS_DIR=../SRCGOPETGOC/MariaDB_SQL
  BACKUP_DIR="${BACKUP_DIR:-$(cd .. && pwd)/backups}"
  DEFAULT_DB=gopettae_tae2
  DB_CONTAINER="${DB_CONTAINER:-gopet-mariadb}"
  ALLOWED_DBS=" gopettae_tae2 gopettae_gopet_web gp_log "

  baseline=false
  [[ ${1:-} == --baseline ]] && baseline=true

  PASS=$(grep '^MARIADB_ROOT_PASSWORD=' .env | cut -d= -f2-)
  [[ -n $PASS ]] || { echo "[migrate] Thiếu MARIADB_ROOT_PASSWORD trong docker/.env" >&2; exit 1; }

  # Mật khẩu qua biến môi trường, không qua tham số dòng lệnh (lộ trong `ps`).
  sql() { docker exec -i -e MYSQL_PWD="$PASS" "$DB_CONTAINER" mysql -uroot --default-character-set=utf8mb4 "$@"; }

  # Lần deploy đầu MariaDB có thể chưa chạy; nạp dump lần đầu mất vài phút.
  docker compose up -d mariadb
  for ((i = 0; i < 60; i++)); do
    [[ $(docker inspect -f '{{.State.Health.Status}}' "$DB_CONTAINER" 2>/dev/null) == healthy ]] && break
    sleep 5
  done
  [[ $(docker inspect -f '{{.State.Health.Status}}' "$DB_CONTAINER") == healthy ]] \
    || { echo "[migrate] MariaDB không healthy" >&2; exit 1; }

  sql "$DEFAULT_DB" -e "CREATE TABLE IF NOT EXISTS schema_migrations (
    filename VARCHAR(255) NOT NULL PRIMARY KEY,
    applied_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP)"

  pending=()
  for path in "$MIGRATIONS_DIR"/migration-*.sql; do
    f=$(basename "$path")
    [[ $f == *seed* ]] && continue
    # Tên file đi thẳng vào câu SQL bên dưới — chỉ nhận ký tự an toàn.
    [[ $f =~ ^migration-[a-z0-9-]+\.sql$ ]] || { echo "[migrate] Tên file không hợp lệ: $f" >&2; exit 1; }
    done_=$(sql -N "$DEFAULT_DB" -e "SELECT COUNT(*) FROM schema_migrations WHERE filename='$f'")
    [[ $done_ == 0 ]] && pending+=("$f")
  done

  if ((${#pending[@]} == 0)); then
    echo "[migrate] Không có migration mới."
    exit 0
  fi

  if $baseline; then
    for f in "${pending[@]}"; do
      sql "$DEFAULT_DB" -e "INSERT INTO schema_migrations(filename) VALUES ('$f')"
      echo "[migrate] Đánh dấu đã chạy: $f"
    done
    exit 0
  fi

  # Backup trước khi đổi schema: DDL của MariaDB không rollback được.
  mkdir -p "$BACKUP_DIR"
  backup="$BACKUP_DIR/pre-migrate-$(date +%Y%m%d-%H%M%S).sql.gz"
  docker exec -e MYSQL_PWD="$PASS" "$DB_CONTAINER" mysqldump -uroot --single-transaction \
    --databases gopettae_tae2 gopettae_gopet_web gp_log | gzip > "$backup" \
    || { rm -f "$backup"; echo "[migrate] Backup thất bại — không chạy migration" >&2; exit 1; }
  echo "[migrate] Backup: $backup"

  for f in "${pending[@]}"; do
    db=$(sed -n 's/^-- database: *\([a-z0-9_]*\).*/\1/p' "$MIGRATIONS_DIR/$f" | head -1)
    db=${db:-$DEFAULT_DB}
    [[ $ALLOWED_DBS == *" $db "* ]] || { echo "[migrate] $f: database không hợp lệ '$db'" >&2; exit 1; }

    echo "[migrate] Chạy $f trên $db"
    if ! sql "$db" < "$MIGRATIONS_DIR/$f"; then
      echo "[migrate] LỖI ở $f — dừng. Khôi phục nếu cần: $backup" >&2
      exit 1
    fi
    sql "$DEFAULT_DB" -e "INSERT INTO schema_migrations(filename) VALUES ('$f')"
  done
  echo "[migrate] Xong ${#pending[@]} migration."
}

LOCK_FILE=/tmp/gopet-migrate.lock

if command -v flock >/dev/null 2>&1; then
  # fd 200 giữ mở suốt tiến trình con (main) — flock tự nhả khi script thoát (mọi nhánh exit).
  exec 200>"$LOCK_FILE"
  if ! flock -n 200; then
    echo "[migrate] Một tiến trình migrate khác đang chạy — chờ..." >&2
    flock 200
  fi
  main "$@"
else
  echo "[migrate] Cảnh báo: không có lệnh 'flock' (bình thường trên Windows Git Bash dev)" \
       "— bỏ qua khoá chống chạy song song." >&2
  main "$@"
fi
