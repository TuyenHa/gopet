#!/bin/bash
#
# Build FreeJ2ME bang javac, khong dung Ant.
#
# build.xml cua du an dat source/target="1.6" va bootclasspath tro toi
# ${java.home}/lib/rt.jar. JDK 17 khong con rt.jar va khong con ho tro
# target 1.6 (toi thieu la 7), nen chay Ant se hong ngay buoc dau.
#
# Build tay thi kiem soat duoc co, va du an nay khong co phu thuoc ngoai
# (thu vien asm da nam san trong src/org/objectweb).
#
#   bash build-freej2me.sh
#
# Ket qua: freej2me/build/freej2me_plus.jar

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/freej2me" && pwd)"
cd "$ROOT"

CLASSES="build/classes"
OUT="build/freej2me_plus.jar"

echo "[build] Don thu muc cu"
rm -rf "$CLASSES" "$OUT"
mkdir -p "$CLASSES"

echo "[build] Liet ke ma nguon (bo libretro/ va win32pad/ giong build.xml)"
find src -name "*.java" \
    -not -path "src/libretro/*" \
    -not -path "src/win32pad/*" \
    > /tmp/freej2me-sources.txt
echo "[build] $(wc -l < /tmp/freej2me-sources.txt) file"

# --release 8 chu KHONG phai 11 tro len.
#
# FreeJ2ME vendor san org.xml.sax va javax.xml.namespace trong src. Tu Java 9,
# nhung package do thuoc module java.xml va JDK cam "package split" - bien dich
# voi release 11 se hong 74 loi kieu "X is already defined in this compilation unit".
# Release 8 dung mo hinh classpath cu, cho phep class vendor che class cua JDK.
#
# -nowarn vi ma nguon dung nhieu API da deprecated - do la ban chat cua viec
# hien thuc lai MIDP, khong phai loi can sua.
echo "[build] Bien dich (co the mat 1-2 phut)"
javac -nowarn -encoding utf-8 --release 8 \
    -d "$CLASSES" @/tmp/freej2me-sources.txt 2>&1 \
    | grep -vE "^Note:|warning" || true

if [ ! -d "$CLASSES/org/recompile/freej2me" ]; then
    echo "[build] THAT BAI: khong sinh ra class nao"
    exit 1
fi

echo "[build] Dong goi JAR"
# Loai class Libretro: ban standalone khong can, va build.xml cung loai chung.
jar --create --file "$OUT" \
    --main-class org.recompile.freej2me.FreeJ2ME \
    -C "$CLASSES" . \
    -C resources . >/dev/null

find "$CLASSES" -name "Libretro*.class" -delete 2>/dev/null || true

echo "[build] XONG: $ROOT/$OUT ($(du -h "$OUT" | cut -f1))"
