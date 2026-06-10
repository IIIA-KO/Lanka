#!/bin/sh
set -e

CONFIG_FILE="/usr/share/nginx/html/config.js"

if [ -z "$LANKA_API_URL" ]; then
  echo "[entrypoint] FATAL: LANKA_API_URL is not set." >&2
  exit 1
fi

if [ ! -f "$CONFIG_FILE" ]; then
  echo "[entrypoint] FATAL: $CONFIG_FILE missing from the image." >&2
  exit 1
fi

echo "[entrypoint] apiUrl=${LANKA_API_URL}"
sed -i "s|apiUrl: '[^']*'|apiUrl: '${LANKA_API_URL}'|" "$CONFIG_FILE"
