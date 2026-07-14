#!/usr/bin/env bash
set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

command_exists() {
    command -v "$@" > /dev/null 2>&1
}

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

echo "SCRIPT_DIR is set to: $SCRIPT_DIR"
echo "REPO_ROOT is set to: $REPO_ROOT"
cd "$REPO_ROOT"

echo "[post-start] Verifying Docker daemon availability"
MAX_DOCKER_WAIT_SECONDS=60
SLEEP_BETWEEN_DOCKER_CHECKS=2
DOCKER_DEADLINE=$(( $(date +%s) + MAX_DOCKER_WAIT_SECONDS ))

while ! docker info >/dev/null 2>&1; do
  if [ "$(date +%s)" -ge "$DOCKER_DEADLINE" ]; then
    echo "[post-start] Docker daemon is not ready after ${MAX_DOCKER_WAIT_SECONDS}s; giving up"
    exit 1
  fi
  echo "[post-start] Docker daemon not ready yet; waiting ${SLEEP_BETWEEN_DOCKER_CHECKS}s..."
  sleep "$SLEEP_BETWEEN_DOCKER_CHECKS"
done
echo "[post-start] Docker daemon is ready"


echo "[post-start] Devcontainer is ready"

# Ensure squad CLI is installed or updated
if command_exists squad; then
    log_info "Updating squad CLI to latest version..."
    npm install --ignore-scripts -g @bradygaster/squad-cli@latest > /dev/null 2>&1
    log_success "squad CLI updated to latest version."
else
    log_info "Installing squad CLI..."
    npm install --ignore-scripts -g @bradygaster/squad-cli > /dev/null 2>&1
    log_success "squad CLI installed successfully."
fi

# Check if Squad is already initialized in the workspace
if [ -d ".squad" ]; then
    log_success "Squad is already initialized in this workspace."
else
    log_info "Squad is not yet initialized."
    log_info ""
    log_info "To set up your Squad team, run:"
    log_info "  squad init"
fi

# Check GitHub authentication status
log_info ""
log_info "Checking GitHub authentication status..."
if gh auth status > /dev/null 2>&1; then
    log_success "You are authenticated with GitHub."
    GH_USERNAME=$(gh api user -q '.login' 2>/dev/null || echo "unknown")
    log_info "Logged in as: $GH_USERNAME"
else
    log_warning "You are not authenticated with GitHub."
    log_info "To authenticate, run: gh auth login"
fi
