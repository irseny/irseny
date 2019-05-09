#!/bin/sh
TARGET_DIR=$1
# alt: cp -u "lib/Emgu.CV/libcvextern.so" "${TargetDir}"
cp -u ../lib/Emgu.CV/libcvextern.so $TARGET_DIR
cp -u ../lib/Extrack/libExtrack.so $TARGET_DIR
