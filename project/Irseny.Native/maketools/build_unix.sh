#!/bin/sh

cmake -G "Unix Makefiles" -H. -B../../../build/Irseny.Native \
-DCMAKE_BUILD_TYPE=Release -DWITH_OPENCV=1 -DWITH_UINPUT=1 \
-DWITH_FREETRACK=1 -DBUILD_TESTING=1

if [ "$?" -ne "0" ]
then
	exit 1
fi

make --directory=../../../build/Irseny.Native all

if [ "$?" -ne "0" ]
then
	exit 1
fi

make --directory=../../../build/Irseny.Native install

exit "$?"
