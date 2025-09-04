FROM archlinux:latest

ENV TERM=xterm-256color

RUN pacman -Syu --noconfirm && \
    pacman -S --noconfirm archiso git sudo arch-install-scripts bash sed coreutils findutils grep util-linux && \
    rm -rf /var/cache/pacman/pkg/*

WORKDIR /work

# Copy overlay and scripts from build context
COPY overlay /work/overlay
COPY scripts/container-build.sh /usr/local/bin/container-build

RUN chmod +x /usr/local/bin/container-build

ENTRYPOINT ["/usr/local/bin/container-build"]

