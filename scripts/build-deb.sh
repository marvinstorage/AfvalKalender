#!/usr/bin/env bash
# Builds a .deb for the Ubuntu/Debian (amd64) console or desktop app from a self-contained publish folder.
# Usage: scripts/build-deb.sh <console|desktop> <version> <publish-dir> <output-dir>
set -euo pipefail

variant="${1:?variant: console or desktop}"
version="${2:?version, for example 1.3.0}"
publish_dir="${3:?publish directory}"
out_dir="${4:?output directory}"

case "$variant" in
  console)
    package="afvalkalender-console"
    binary="AfvalKalender.ConsoleUI"
    summary="AfvalKalender terminal app"
    long="Interactieve terminal-app (TUI) die afvalophaalmomenten van Nederlandse afvalverwerkers via de Ximmio API ophaalt en als .ics bestand exporteert."
    depends="libc6, libgcc-s1, libstdc++6, zlib1g"
    ;;
  desktop)
    package="afvalkalender-desktop"
    binary="AfvalKalender.DesktopUI"
    summary="AfvalKalender desktop app"
    long="Grafische app (Avalonia) die afvalophaalmomenten van Nederlandse afvalverwerkers via de Ximmio API ophaalt en als .ics bestand exporteert."
    depends="libc6, libgcc-s1, libstdc++6, zlib1g, libfontconfig1, libx11-6, libice6, libsm6"
    ;;
  *) echo "Unknown variant: $variant" >&2; exit 1 ;;
esac

[ -x "$publish_dir/$binary" ] || { echo "Missing executable $publish_dir/$binary" >&2; exit 1; }

root="$(mktemp -d)"
trap 'rm -rf "$root"' EXIT

install -d "$root/opt/$package" "$root/usr/bin" "$root/DEBIAN"
cp -r "$publish_dir"/. "$root/opt/$package/"
ln -s "/opt/$package/$binary" "$root/usr/bin/$package"

if [ "$variant" = "desktop" ]; then
  install -d "$root/usr/share/applications" "$root/usr/share/icons/hicolor/256x256/apps"
  install -m 644 "$(dirname "$0")/../assets/logo-256.png" "$root/usr/share/icons/hicolor/256x256/apps/$package.png"
  cat > "$root/usr/share/applications/$package.desktop" <<DESKTOP
[Desktop Entry]
Type=Application
Name=AfvalKalender
Comment=Afvalkalender exporteren naar je agenda
Exec=$package
Icon=$package
Terminal=false
Categories=Utility;Office;Calendar;
DESKTOP
fi

size_kb="$(du -sk "$root" | cut -f1)"
cat > "$root/DEBIAN/control" <<CONTROL
Package: $package
Version: $version
Section: utils
Priority: optional
Architecture: amd64
Maintainer: marvinstorage <marvinstorage@users.noreply.github.com>
Installed-Size: $size_kb
Depends: $depends
Recommends: libicu74 | libicu72 | libicu70 | libicu67
Homepage: https://github.com/marvinstorage/AfvalKalender
Description: $summary
 $long
CONTROL

find "$root" -type d -exec chmod 755 {} +
chmod 755 "$root/opt/$package/$binary"

mkdir -p "$out_dir"
dpkg-deb --root-owner-group --build "$root" "$out_dir/${package}_${version}_amd64.deb"
