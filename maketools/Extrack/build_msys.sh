#!/bin/sh
cmake -G "Unix Makefiles" -H. -B../../build/Extrack -DCMAKE_BUILD_TYPE=Release -DWITH_WINAPI=1 -DWITH_VJOY=1 -DWITH_FREETRACK=1 -DWITH_FREETRACK_DUMMY=1
make --directory=../../build/Extrack all
make --directory=../../build/Extrack install
