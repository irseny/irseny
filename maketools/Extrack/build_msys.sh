#!/bin/sh
cmake -G "Unix Makefiles" -H. -B../../build/Extrack -DCMAKE_BUILD_TYPE=Release -DTARGET_M32=0 -DWITH_VJOY=1 -DWITH_TIFC=1
make --directory=../../build/Extrack
