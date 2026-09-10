#!/bin/bash
#
# Nạp 3 dump của goPet vào 3 database mà GServer mong đợi.
#
# Tên database phải khớp chính xác với connection string trong
# GServer/App.config — sai tên là GServer chạy nhưng không thấy dữ liệu:
#
#   GameConnectString -> gopettae_tae2        (server_db.sql)
#   WebConnectString  -> gopettae_gopet_web   (web_db.sql)
#   LogConnectString  -> gp_log               (log_db.sql)
#
# Nạp qua mariadb CLIENT chứ không đẩy thẳng vào server: web_db.sql chứa
# `DELIMITER $$` cho stored function ComputeSha256Hash, mà DELIMITER là
# lệnh phía client — server không hiểu nó.
#
# Script chỉ chạy khi volume dữ liệu còn rỗng. Nạp lại:
#   docker compose down -v && docker compose up -d

set -euo pipefail

DUMP_DIR=/dumps

# MariaDB 10.4 chi co binary `mysql`; ban moi hon co ca `mariadb`.
# Chon cai nao co san de script chay duoc tren ca hai.
MYSQL_CLI=$(command -v mariadb || command -v mysql)

load_dump() {
    local db="$1"
    local file="${DUMP_DIR}/$2"

    if [ ! -f "$file" ]; then
        echo "[gopet-init] THIẾU $file — kiểm tra mount ../SRCGOPETGOC/MariaDB_SQL"
        exit 1
    fi

    echo "[gopet-init] Tạo database $db"
    $MYSQL_CLI -uroot -p"${MARIADB_ROOT_PASSWORD:-$MYSQL_ROOT_PASSWORD}" \
        -e "CREATE DATABASE IF NOT EXISTS \`${db}\` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;"

    echo "[gopet-init] Nạp $2 -> $db ($(du -h "$file" | cut -f1))"
    $MYSQL_CLI -uroot -p"${MARIADB_ROOT_PASSWORD:-$MYSQL_ROOT_PASSWORD}" --default-character-set=utf8mb4 "$db" < "$file"

    local tables
    tables=$($MYSQL_CLI -uroot -p"${MARIADB_ROOT_PASSWORD:-$MYSQL_ROOT_PASSWORD}" -N -B \
        -e "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='${db}';")
    echo "[gopet-init] $db: $tables bảng"
}

echo "[gopet-init] === Bắt đầu nạp dump goPet ==="

load_dump gopettae_tae2      server_db.sql
load_dump gopettae_gopet_web web_db.sql
load_dump gp_log             log_db.sql

echo "[gopet-init] === Nạp xong ==="
