#!/bin/sh
cmake -G "Unix Makefiles" -H. -B../../build/Extrack -DCMAKE_BUILD_TYPE=Release -DWITH_UINPUT=1 -DWITH_FREETRACK=1
make --directory=../../build/Extrack
