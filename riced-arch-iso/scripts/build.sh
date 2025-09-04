#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
IMAGE_NAME=riced-arch-iso:latest

docker build -t "$IMAGE_NAME" "$ROOT_DIR"

docker run --rm \
  -v "$ROOT_DIR/overlay:/work/overlay:ro" \
  -v "$ROOT_DIR/out:/work/out" \
  "$IMAGE_NAME"

echo "ISO(s) available in $ROOT_DIR/out"

