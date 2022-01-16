#!/bin/bash

if [ "$(whoami)" = "root" ]; then
  echo "  Please don't run as root/using sudo..."
  exit
fi

REPO=$(dirname "$0")
rm -rf "$REPO"/out
rm -rf "$REPO"/build-mingw
mkdir -p "$REPO"/build-mingw/include && cd "$REPO"/build-mingw || exit

cmake -G "MinGW Makefiles" -DBUILD_SHARED_LIBS=On -DORLP_ED25519_SYSNAME="mingw-w64" -DORLP_ED25519_BUILD_DLL=On -DORLP_ED25519_PACKAGE=On -DCMAKE_BUILD_TYPE=Release ..

mingw32-make.exe

cp -r ../include .

cd "$REPO" || exit

echo "  Done. Exported build into $REPO/build-mingw"

