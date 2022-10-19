#!/bin/sh

REPO=$(dirname "$0")

PREVCC="$CC"
PREVCXX="$CXX"

if command -v clang &> /dev/null
then
    echo "-- Clang found on system, great! Long live LLVM! :D"
    export CC=clang
    export CXX=clang++
fi

rm -rf "$REPO"/build
mkdir -p "$REPO"/build && cd "$REPO"/build || exit

cmake -DBUILD_SHARED_LIBS=Off -DORLP_ED25519_BUILD_DLL=Off -DORLP_ED25519_BUILD_TESTS=On ..
cmake --build . --config Release || exit

export CC="$PREVCC"
export CXX="$PREVCXX"

./run_tests || ./Release/run_tests.exe || exit

cd "$REPO" || exit