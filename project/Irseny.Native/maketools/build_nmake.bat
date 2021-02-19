@echo off
cmake -G "NMake Makefiles" -H. -B..\..\..\build\Irseny.Native ^
-DCMAKE_BUILD_TYPE=Release -DWITH_LOCAL_OPENCV=1 -DWITH_WINAPI=1 -DWITH_VJOY=1 ^
-DWITH_FREETRACK=1 -DWITH_FREETRACK_TEST=1

IF %ERRORLEVEL% NEQ 0 (
	exit /B 1
)
cd ..\..\..\build\Irseny.Native
nmake all
IF %ERRORLEVEL% NEQ 0 (
	cd ..\..\project\Irseny.Native\maketools
	exit /B 1
)
nmake install
cd ..\..\project\Irseny.Native\maketools
