#!/bin/sh
cmake -G "Unix Makefiles" -H. -B../../build/Extrack -DCMAKE_BUILD_TYPE=Release -DTARGET_M32=0 -DWITH_UINPUT=1
make --directory=../../build/Extrack
