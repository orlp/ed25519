SET i=%CD%
SET repo=%~dp0
SET out="%repo%\build"

if exist %out% ( rd /s /q %out% )
mkdir %out% && cd %out%

cmake -DBUILD_SHARED_LIBS=On -DORLP_ED25519_BUILD_DLL=On -DORLP_ED25519_PACKAGE=On -DCMAKE_BUILD_TYPE=Release .. 

cmake --build . --config Release

cd %i%
