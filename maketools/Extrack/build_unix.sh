#!/bin/sh

cmake -G "Unix Makefiles" -H. -B../../build/Extrack -DCMAKE_BUILD_TYPE=Release -DWITH_FREETRACK=1 -DWITH_UINPUT=1 -DWITH_FREETRACK_DUMMY=1 ../../maketools/Extrack/
make --directory=../../build/Extrack all
make --directory=../../build/Extrack install
