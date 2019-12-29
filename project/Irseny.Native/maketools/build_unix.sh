#!/bin/sh

cmake -G "Unix Makefiles" -H. -B../../../build/Irseny.Native \
-DCMAKE_BUILD_TYPE=Release  -DWITH_UINPUT=1 \
-DWITH_FREETRACK=1 -DWITH_FREETRACK_TEST=1 -DWITH_FREETRACK_DUMMY=1

make --directory=../../../build/Irseny.Native all
make --directory=../../../build/Irseny.Native install
