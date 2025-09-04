#!/usr/bin/env bash
set -euo pipefail

# This script generates a GRUB entry to boot the built ISO from disk without USB.
# Works on systems where GRUB can access the ISO path (ext4, btrfs with GRUB btrfs module, etc.).

ISO_PATH=${1:-}
[[ -z "${ISO_PATH}" ]] && { echo "Usage: $0 /absolute/path/to/archlinux-riced.iso"; exit 1; }

[[ ${ISO_PATH} != /* ]] && { echo "Please pass an absolute path to the ISO"; exit 1; }

GRUB_DIR=/etc/grub.d
SNIPPET=/etc/grub.d/40_custom

if [[ ! -w "$SNIPPET" ]]; then
  echo "This script must be run as root to write $SNIPPET" >&2
  exit 1
fi

cat >> "$SNIPPET" <<EOF

menuentry 'Riced Arch ISO (loopback)' --class arch --class gnu-linux --class gnu --class os {
    set iso_path='${ISO_PATH}'
    loopback loop "(")${ISO_PATH}(";)"  # placeholder, replaced below
}
EOF

# Replace with proper GRUB syntax:
# We need to translate the absolute path to (hdX,Y)/path if possible. If not, use search --file.

PART=$(df --output=source "$ISO_PATH" | tail -n1)
MNT=$(df --output=target "$ISO_PATH" | tail -n1)

DISK=$(lsblk -no pkname "$PART" 2>/dev/null || true)
PART_NUM=$(lsblk -no partn "$PART" 2>/dev/null || true)

GRUB_DISK="hd0"
if [[ -n "$DISK" ]]; then
  IDX=$(lsblk -do NAME | grep -n "^$DISK$" | cut -d: -f1 | head -n1)
  if [[ -n "$IDX" ]]; then
    IDX=$((IDX-1))
    GRUB_DISK="hd${IDX}"
  fi
fi

GRUB_PART=$PART_NUM

ISO_REL=${ISO_PATH#${MNT}}

sed -i \
  -e "s|loopback loop \"(\")\
${ISO_PATH}\
(\";)\"|loopback loop (\${GRUB_DISK},\${GRUB_PART})\${ISO_REL}|" \
  "$SNIPPET"

cat >> "$SNIPPET" <<'EOF'
    linux (loop)/arch/boot/x86_64/vmlinuz-linux img_dev=/dev/disk/by-label/ARCH_202409 img_loop=$iso_path earlymodules=loop rd.luks=0 rd.md=0 rd.dm=0
    initrd (loop)/arch/boot/x86_64/initramfs-linux.img
}
EOF

echo "Updating GRUB..."
if command -v update-grub >/dev/null 2>&1; then
  update-grub | cat
elif command -v grub-mkconfig >/dev/null 2>&1; then
  grub-mkconfig -o /boot/grub/grub.cfg | cat
else
  echo "Please regenerate grub.cfg manually. Entry appended to $SNIPPET"
fi

echo "Done. Reboot and choose 'Riced Arch ISO (loopback)'."

