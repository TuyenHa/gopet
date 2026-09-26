#!/usr/bin/env bash
# Tương thích ngược: logic đã dồn vào deploy-service.sh (dùng chung cho gserver + webadmin).
set -euo pipefail
exec bash "$(dirname "$0")/deploy-service.sh" gserver "$@"
