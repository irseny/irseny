cmake -G "NMake Makefiles" -H. -B..\..\build\Extrack -DCMAKE_BUILD_TYPE=Release -DTARGET_M32=1 -DWITH_VJOY=1 -DWITH_TIFC=1
make --directory=..\..\build\Extrack
