cmake -G "NMake Makefiles" -H. -B..\..\..\build\Irseny.Native \
-DCMAKE_BUILD_TYPE=Release -DTARGET_M32=1 -DWITH_WINAPI=1 -DWITH_VJOY=1 \
-DWITH_FREETRACK=1 -DWITH_FREETRACK_DUMMY=1 -DWITH_FREETRACK_TEST=1

nmake --directory=..\..\..\build\Irseny.Native all
nmake --directory=..\..\..\build\Irseny.Native install
