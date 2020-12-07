cmake -G "NMake Makefiles" -H. -B..\..\..\build\Irseny.Native ^
-DCMAKE_BUILD_TYPE=Release -DWITH_LOCAL_OPENCV=1 -DWITH_WINAPI=1 -DWITH_VJOY=1 ^
-DWITH_FREETRACK=1 -DWITH_FREETRACK_TEST=1
cd ..\..\..\build\Irseny.Native
nmake all
nmake install
cd ..\..\project\Irseny.Native\maketools

