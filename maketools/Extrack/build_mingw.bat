
cmake -G "MinGW Makefiles" -H. -B../../build/Extrack -DCMAKE_BUILD_TYPE=Release -DTARGET_M32=0 -DWITH_WINAPI=1 -DWITH_VJOY=1 -DWITH_FREETRACK=1
make --directory=../../build/Extrack
