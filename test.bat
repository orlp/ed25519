SET i=%CD%
SET repo=%~dp0
SET out="%repo%\build"

if exist %out% ( rd /s /q %out% )
mkdir %out% && cd %out%

cmake -DORLP_ED25519_BUILD_TESTS=On -DBUILD_SHARED_LIBS=Off -DORLP_ED25519_BUILD_DLL=Off -DORLP_ED25519_PACKAGE=Off -DCMAKE_BUILD_TYPE=Release ..

cmake --build . --config Release

call Release\run_tests.exe

cd %i%
