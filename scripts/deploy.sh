#!/usr/bin/env bash
set -euo pipefail

# Evropiyum deployment helper
# Runs: git fetch/pull + docker compose build/up
#
# Usage examples:
#   ./scripts/deploy.sh
#   ./scripts/deploy.sh --branch main
#   ./scripts/deploy.sh --services "cms-admin web-partexo web-rutenyum web-tnmotomotiv web-veraotomotiv web-evropiyum"
#   ./scripts/deploy.sh --skip-git

APP_DIR="${APP_DIR:-/root/cms}"
BRANCH="${BRANCH:-main}"
SERVICES="${SERVICES:-cms-admin web-partexo web-rutenyum web-tnmotomotiv web-veraotomotiv web-evropiyum}"
SKIP_GIT="false"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --app-dir)
      APP_DIR="$2"
      shift 2
      ;;
    --branch)
      BRANCH="$2"
      shift 2
      ;;
    --services)
      SERVICES="$2"
      shift 2
      ;;
    --skip-git)
      SKIP_GIT="true"
      shift 1
      ;;
    *)
      echo "Unknown argument: $1"
      exit 1
      ;;
  esac
done

echo "==> App directory: ${APP_DIR}"
cd "${APP_DIR}"

echo "==> Ensure required data folders"
mkdir -p /root/cms/data/postgres /root/cms/data/media

if [[ "${SKIP_GIT}" != "true" ]]; then
  echo "==> Update code from git (${BRANCH})"
  git fetch --all --prune
  git checkout "${BRANCH}"
  git pull --ff-only origin "${BRANCH}"
fi

echo "==> Validate docker compose"
docker compose config >/dev/null

echo "==> Build and start services"
# shellcheck disable=SC2086
docker compose up -d --build ${SERVICES}

echo "==> Cleanup old images"
docker image prune -f >/dev/null || true

echo "==> Done."
docker compose ps

