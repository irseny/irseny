#ifndef IRSENY_VIDEO_CAPTURE_H
#define IRSENY_VIDEO_CAPTURE_H

#include "IrsenyExport.h"

#if LINUX
#include "VideoCapture.lin.h"
#endif // LINUX

#if WINDOWS
#include "VideoCapture.win.h"
#endif // WINDOWS

#if 0
IRSENY_EXPORT IRS_VideoCaptureContext* irsCreateVideoCaptureContext();
IRSENY_EXPORT bool irsDestroyVideoCaptureContext(IRS_VideoCaptureContext*);
IRSENY_EXPORT IRS_VideoCaptureConstructionInfo* irsAllocVideoCaptureConstructionInfo();
IRSENY_EXPORT bool irsFreeVideoCaptureConstructionInfo(IRS_VideoCaptureConstructionInfo*);
IRSENY_EXPORT float irsGetVideoCaptureProperty(IRS_VideoCaptureConstructionInfo*, int property);
IRSENY_EXPORT bool irsSetVideoCaptureProperty(IRS_VideoCaptureConstructionInfo*, int property, float value);
IRSENY_EXPORT IRS_VideoCapture* irsCreateVideoCapture(IRS_VideoCaptureContext*, IRS_VideoCaptureConstructionInfo*);
IRSENY_EXPORT bool irsDestroyVideoCapture(IRS_VideoCaptureContext*, IRS_VideoCapture*);
IRSENY_EXPORT bool irsStartVideoCapture(IRS_VideoCapture*);
IRSENY_EXPORT bool irsBeginVideoFrameGrab(IRS_VideoCapture*);
IRSENY_EXPORT bool irsEndVideoFrameGrab(IRS_VideoCapture*);
IRSENY_EXPORT bool irsStopVideoCapture(IRS_VideoCapture*);
#endif

#endif // IRSENY_VIDEO_CATPURE_H