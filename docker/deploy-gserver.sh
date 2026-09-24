#!/usr/bin/env bash
# Build lại image GServer và thay container, tự rollback nếu bản mới không lên healthy.
# Được GitHub Actions (.github/workflows/gserver-ci-cd.yml) gọi qua SSH SAU khi đã
# cập nhật mã nguồn; cũng chạy tay được trên máy chủ:
#
#   cd /opt/gopet && bash docker/deploy-gserver.sh
#
# Các bước giống mục 4.4 của docs/deployment-linux-backend.md.
set -euo pipefail

cd "$(dirname "$0")"
compose() { docker compose --profile server "$@"; }

# Tối đa 5 phút cho start_period (120s) + nạp template.
HEALTH_TIMEOUT="${HEALTH_TIMEOUT:-300}"

wait_healthy() {
  local status waited=0
  while (( waited < HEALTH_TIMEOUT )); do
    status=$(docker inspect -f '{{.State.Health.Status}}' gopet-gserver 2>/dev/null || echo missing)
    echo "[deploy] gserver: $status (${waited}s)"
    [[ $status == healthy ]] && return 0
    [[ $status == unhealthy ]] && return 1
    sleep 10
    waited=$((waited + 10))
  done
  return 1
}

# Giữ image đang chạy để rollback. Lần deploy đầu chưa có image thì bỏ qua.
has_prev=false
if docker image inspect gopet-gserver:latest >/dev/null 2>&1; then
  docker tag gopet-gserver:latest gopet-gserver:prev
  has_prev=true
fi

# Build trước khi đụng vào container: build lỗi thì server cũ vẫn chạy nguyên.
compose build gserver
# Container cũ nhận SIGTERM và được 60s (stop_grace_period) để lưu dữ liệu.
compose up -d --no-build gserver

if wait_healthy; then
  echo "[deploy] OK"
  docker image prune -f >/dev/null
  exit 0
fi

echo "[deploy] Bản mới KHÔNG healthy. Log gần nhất:"
compose logs --tail 100 gserver || true

if $has_prev; then
  echo "[deploy] Rollback về gopet-gserver:prev"
  docker tag gopet-gserver:prev gopet-gserver:latest
  compose up -d --no-build --force-recreate gserver
  wait_healthy || echo "[deploy] Rollback cũng không healthy — cần xử lý tay."
fi
exit 1
