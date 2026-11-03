#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CORE_DIR="$SCRIPT_DIR/tor-core"

if [[ $# -ne 2 ]]; then
  echo "usage: $0 <os>-<arch> <bin folder>" >&2
  exit 2
fi

PLATFORM="$1"
BIN_DIR="$2"

if ! command -v go >/dev/null 2>&1; then
  echo "[tor-core] go not found; install the Go toolchain" >&2
  exit 1
fi

goos_for() {
  case "$1" in
    windows-*) echo "windows" ;;
    linux-*)   echo "linux" ;;
    macos-*)   echo "darwin" ;;
    *)         return 1 ;;
  esac
}

goarch_for() {
  case "$1" in
    *-64)    echo "amd64" ;;
    *-arm64) echo "arm64" ;;
    *)       return 1 ;;
  esac
}

GOOS_TARGET="$(goos_for "$PLATFORM")" || {
  echo "[tor-core] unsupported platform: $PLATFORM"
  exit 0
}
GOARCH_TARGET="$(goarch_for "$PLATFORM")" || {
  echo "[tor-core] unsupported architecture: $PLATFORM"
  exit 0
}

EXT=""
if [[ "$GOOS_TARGET" == "windows" ]]; then
  EXT=".exe"
fi

TOR_OUT="$BIN_DIR/tor"
AETHER_OUT="$BIN_DIR/aether"
mkdir -p "$TOR_OUT" "$AETHER_OUT"

echo "[tor-core] building tor-core and conjure-client for $GOOS_TARGET/$GOARCH_TARGET"
(
  cd "$CORE_DIR"
  env CGO_ENABLED=0 GOOS="$GOOS_TARGET" GOARCH="$GOARCH_TARGET" \
    go build -trimpath -ldflags "-s -w" -o "$TOR_OUT/tor-core$EXT" ./cmd/torcore
  env CGO_ENABLED=0 GOOS="$GOOS_TARGET" GOARCH="$GOARCH_TARGET" \
    go build -trimpath -ldflags "-s -w" -o "$TOR_OUT/conjure-client$EXT" ./cmd/conjure
)

cp -f "$TOR_OUT/conjure-client$EXT" "$AETHER_OUT/conjure-client$EXT"
ls -la "$TOR_OUT/tor-core$EXT" "$TOR_OUT/conjure-client$EXT"
