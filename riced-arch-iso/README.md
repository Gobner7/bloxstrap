## Riced Arch ISO (Hyprland Edition)

This project builds a pre-riced Arch ISO with Hyprland, Waybar, Wofi, Kitty, PipeWire, and a tasteful Catppuccin-like theme. It also includes a helper to boot the ISO directly from disk via GRUB (no USB required).

### Requirements
- Docker (rootless or root)
- Linux host

### Build the ISO
```bash
cd riced-arch-iso
bash scripts/build.sh
```
The ISO will appear under `out/`.

### Boot ISO without USB (GRUB loopback)
1. Copy ISO to a filesystem GRUB can read (e.g., `/boot/iso/riced.iso`).
2. Run as root:
```bash
bash scripts/grub-boot-from-iso.sh /absolute/path/to/riced.iso
```
3. Regenerate GRUB if the script prompts.
4. Reboot and select "Riced Arch ISO (loopback)".

### Live user
- Auto-login root on TTY1. Start Hyprland with `hyprland`.

### Customize
- Edit `overlay/packages.x86_64` for packages.
- Put dotfiles in `overlay/skel` to seed `/etc/skel`.
- Add system overlays under `overlay/airootfs` (merged into live FS).

