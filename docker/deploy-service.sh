#!/usr/bin/env bash
# Deploy một service (gserver HOẶC webadmin) từ image đã build sẵn trên GHCR, tự rollback
# nếu bản mới không lên healthy. Máy chủ KHÔNG build gì — GitHub Actions (job build-push,
# .github/workflows/gserver-ci-cd.yml) build và đẩy image `<service>:<git-sha-12>`.
#
#   cd /opt/gopet && bash docker/deploy-service.sh gserver  [tag]
#   cd /opt/gopet && bash docker/deploy-service.sh webadmin [tag]
#
# Không truyền tag → dùng GSERVER_TAG / WEBADMIN_TAG trong docker/.env (mặc định latest).
# Chỉ đụng tới container CỦA CHÍNH service đó (`compose up -d --no-build <service>`) —
# deploy webadmin không restart gserver và ngược lại. Migration luôn chạy trước khi thay
# container (migrate-db.sh tự khoá bằng flock, an toàn nếu hai lần deploy chồng nhau).
#
# Xem mục 4 và 11 của docs/deployment-linux-backend.md.
set -euo pipefail

cd "$(dirname "$0")"
compose() { docker compose --profile server "$@"; }

SERVICE="${1:-}"
NEW_TAG="${2:-}"
case "$SERVICE" in
  gserver) TAG_VAR=GSERVER_TAG; REQUIRED=(GHCR_OWNER) ;;
  webadmin)
    TAG_VAR=WEBADMIN_TAG
    REQUIRED=(GHCR_OWNER WEBADMIN_DB_PASSWORD WEBADMIN_SESSION_SECRET WEBADMIN_DOMAIN WEBADMIN_ALLOWED_IPS)
    ;;
  *)
    echo "Cách dùng: deploy-service.sh <gserver|webadmin> [tag]" >&2
    exit 1
    ;;
esac

# Compose không bắt buộc các biến này (để `docker compose up -d` trên máy dev không cần
# chúng) → kiểm ở đây, trước khi đụng vào container.
[[ -f .env ]] || { echo "[deploy] Thiếu docker/.env" >&2; exit 1; }
for v in "${REQUIRED[@]}"; do
  grep -q "^$v=." .env || { echo "[deploy] Thiếu $v trong docker/.env" >&2; exit 1; }
done

CONTAINER="gopet-$SERVICE"
PREV_TAG_FILE=".$SERVICE-prev-tag"
# Tối đa 5 phút cho start_period (30-120s tuỳ service) + nạp dữ liệu/template lúc khởi động.
HEALTH_TIMEOUT="${HEALTH_TIMEOUT:-300}"

wait_healthy() {
  local status waited=0
  while (( waited < HEALTH_TIMEOUT )); do
    status=$(docker inspect -f '{{.State.Health.Status}}' "$CONTAINER" 2>/dev/null || echo missing)
    echo "[deploy] $SERVICE: $status (${waited}s)"
    [[ $status == healthy ]] && return 0
    [[ $status == unhealthy ]] && return 1
    sleep 10
    waited=$((waited + 10))
  done
  return 1
}

# Chạy compose với tag chỉ định qua biến shell (compose ưu tiên biến shell hơn .env khi
# nội suy `${GSERVER_TAG:-latest}`) — thử tag mới mà KHÔNG sửa .env.
with_tag() { local tag="$1"; shift; env "$TAG_VAR=$tag" docker compose --profile server "$@"; }

env_tag=$(grep "^$TAG_VAR=" .env | cut -d= -f2- || true)
target_tag="${NEW_TAG:-${env_tag:-latest}}"

# Tag đang chạy TRƯỚC khi thay container — để rollback. Đọc từ container, không từ .env.
prev_image=$(docker inspect -f '{{.Config.Image}}' "$CONTAINER" 2>/dev/null || true)
prev_tag="${prev_image##*:}"
[[ -n "$prev_image" ]] || prev_tag=""

# Pull TRƯỚC khi đụng vào container/schema: pull lỗi (mạng, tag không tồn tại...) thì
# container cũ vẫn chạy nguyên và .env không đổi.
if ! with_tag "$target_tag" pull "$SERVICE"; then
  echo "[deploy] Pull $SERVICE:$target_tag thất bại — giữ nguyên container cũ và docker/.env." >&2
  exit 1
fi

# Đổi schema trước khi code mới chạy. Migration lỗi thì dừng, bản cũ vẫn chạy.
bash ./migrate-db.sh

# gserver: container cũ nhận SIGTERM và được 60s (stop_grace_period) để lưu dữ liệu.
with_tag "$target_tag" up -d --no-build "$SERVICE"

if wait_healthy; then
  echo "[deploy] OK — $SERVICE:$target_tag"
  # Chỉ ghi .env SAU KHI bản mới đã pull + healthy: reboot/`compose up` sau này dùng đúng tag.
  if grep -q "^$TAG_VAR=" .env; then
    sed -i "s/^$TAG_VAR=.*/$TAG_VAR=$target_tag/" .env
  else
    echo "$TAG_VAR=$target_tag" >> .env
  fi
  [[ -n "$prev_tag" ]] && echo "$prev_tag" > "$PREV_TAG_FILE"
  docker image prune -f >/dev/null
  exit 0
fi

echo "[deploy] Bản mới KHÔNG healthy. Log gần nhất:"
compose logs --tail 100 "$SERVICE" || true

if [[ -n "$prev_tag" && "$prev_tag" != "$target_tag" ]]; then
  echo "[deploy] Rollback về $SERVICE:$prev_tag (docker/.env không đổi)"
  with_tag "$prev_tag" up -d --no-build --force-recreate "$SERVICE"
  wait_healthy || echo "[deploy] Rollback cũng không healthy — cần xử lý tay."
else
  echo "[deploy] Không có tag trước đó để rollback (lần deploy đầu?) — cần xử lý tay."
fi
exit 1
