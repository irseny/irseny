cmake -G "NMake Makefiles" -H. -B..\..\..\build\Irseny.Native ^
-DCMAKE_BUILD_TYPE=Release -DWITH_WINAPI=1 -DWITH_VJOY=1 ^
-DWITH_FREETRACK=1 -DWITH_FREETRACK_DUMMY=1 -DWITH_FREETRACK_TEST=1
cd ..\..\..\build\Irseny.Native
nmake all
nmake install
cd ..\..\project\Irseny.Native\maketools

REM Commented out because CMakeFiles\Makefile2 can not be found with the /F flag
REM nmake /F ..\..\..\build\Irseny.Native\Makefile all
REM nmake /F ..\..\..\build\Irseny.Native\Makefile install
