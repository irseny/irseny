#!/bin/sh

cmake -G "Unix Makefiles" -H. -B../../../build/Irseny.Native \
-DCMAKE_BUILD_TYPE=Release -DWITH_OPENCV=1 -DWITH_UINPUT=1 \
-DWITH_FREETRACK=1

make --directory=../../../build/Irseny.Native all
make --directory=../../../build/Irseny.Native install
