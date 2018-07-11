cmake -G "NMake Makefiles" -H. -B..\..\build\Extrack -DCMAKE_BUILD_TYPE=Release -DTARGET_M32=1
make --directory=..\..\build\Extrack
