#!/usr/bin/env bash
set -euo pipefail

echo "[container] Installing archiso and preparing profile..."

workdir=/work
outdir="$workdir/out"
profile_dir="$workdir/profile"
overlay_dir="$workdir/overlay"

mkdir -p "$outdir" "$profile_dir"

echo "[container] Copying releng profile..."
cp -r /usr/share/archiso/configs/releng/* "$profile_dir/"

echo "[container] Applying custom packages list..."
if [[ -f "$overlay_dir/packages.x86_64" ]]; then
  cp "$overlay_dir/packages.x86_64" "$profile_dir/packages.x86_64"
fi

echo "[container] Applying airootfs overlay..."
if [[ -d "$overlay_dir/airootfs" ]]; then
  rsync -aHAX --delete "$overlay_dir/airootfs/" "$profile_dir/airootfs/"
fi

echo "[container] Injecting /etc/skel ..."
if [[ -d "$overlay_dir/skel" ]]; then
  mkdir -p "$profile_dir/airootfs/etc/skel"
  rsync -aHAX "$overlay_dir/skel/" "$profile_dir/airootfs/etc/skel/"
fi

echo "[container] Enabling services..."
mkdir -p "$profile_dir/airootfs/etc/systemd/system/multi-user.target.wants"
ln -sf /usr/lib/systemd/system/NetworkManager.service "$profile_dir/airootfs/etc/systemd/system/multi-user.target.wants/NetworkManager.service" || true

echo "[container] Building ISO... (this may take a while)"
mkarchiso -v -o "$outdir" "$profile_dir"

echo "[container] Build complete. Output in $outdir"

