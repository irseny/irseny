@echo off
cmake -G "Visual Studio 16 2019" -H. -B..\..\..\build\Irseny.Native ^
-DCMAKE_BUILD_TYPE=Release -DWITH_LOCAL_OPENCV=1 -DWITH_WINAPI=1 -DWITH_VJOY=1 ^
-DWITH_FREETRACK=1 -DWITH_FREETRACK_TEST=1
