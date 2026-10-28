#!/usr/bin/env bash
# Copy uxplay.exe, the GStreamer plugins it uses and every MSYS2 DLL they need
# into a standalone folder: <out>/bin, <out>/lib/gstreamer-1.0, <out>/libexec/gstreamer-1.0.
# Run inside an MSYS2 UCRT64 shell.
set -euo pipefail
exe=$1 out=$2
prefix=${MINGW_PREFIX:-/ucrt64}
plugdir=$prefix/lib/gstreamer-1.0

# shortcut: plugin whitelist read from UxPlay's renderers; if a new UxPlay uses another element, add its plugin here.
plugins="coreelements app typefindfunctions playback autodetect videoparsersbad audioparsers libav
  videoconvertscale audioconvert audioresample volume d3d11 d3d12 d3d wasapi directsound jpeg
  debugutilsbad"

mkdir -p "$out/bin" "$out/lib/gstreamer-1.0" "$out/libexec/gstreamer-1.0"
cp "$exe" "$prefix/bin/gst-inspect-1.0.exe" "$out/bin/"
cp "$prefix/libexec/gstreamer-1.0/gst-plugin-scanner.exe" "$out/libexec/gstreamer-1.0/"
for p in $plugins; do
  if [[ -f $plugdir/libgst$p.dll ]]; then cp "$plugdir/libgst$p.dll" "$out/lib/gstreamer-1.0/"
  else echo "note: plugin $p not available, skipped"; fi
done

# Pull in transitive MSYS2 DLL dependencies (ldd lists the whole closure).
for f in "$out"/bin/*.exe "$out"/lib/gstreamer-1.0/*.dll "$out"/libexec/gstreamer-1.0/*.exe; do
  ldd "$f" | awk '{print $3}' | grep -i "^$prefix/" || true
done | sort -u | while read -r dll; do
  cp -n "$dll" "$out/bin/"
done

du -sh "$out"
