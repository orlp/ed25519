#!/bin/bash

if [ "$(whoami)" = "root" ]; then
  echo "  Please don't run as root/using sudo..."
  exit
fi

PREVCC="$CC"
PREVCXX="$CXX"

if command -v clang &> /dev/null
then
    echo "-- Clang found on system, great! Long live LLVM! :D"
    export CC=clang
    export CXX=clang++
fi

REPO=$(dirname "$0")
rm -rf "$REPO"/out
rm -rf "$REPO"/build
mkdir -p "$REPO"/build/include && cd "$REPO"/build || exit

cmake -DBUILD_SHARED_LIBS=On -DORLP_ED25519_BUILD_DLL=On -DORLP_ED25519_PACKAGE=On -DCMAKE_BUILD_TYPE=Release ..

cmake --build . --config Release

export CC="$PREVCC"
export CXX="$PREVCXX"

cd "$REPO" || exit

echo "  Done. Exported build into $REPO/build"
