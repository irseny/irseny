#ifndef IRSENY_VIDEO_CAPTURE_H
#define IRSENY_VIDEO_CAPTURE_H

#include "IrsenyExport.h"

#if LINUX
#include "VideoCapture.lin.h"
#endif // LINUX

#if WINDOWS
#include "VideoCapture.win.h"
#endif // WINDOWS

IRSENY_EXPORT IRS_VideoCaptureContext* irsCreateVideoCaptureContext();
IRSENY_EXPORT void irsDestroyVideoCaptureContext(IRS_VideoCaptureContext*);
IRSENY_EXPORT IRS_VideoCaptureSettings* irsAllocVideoCaptureSettings();
IRSENY_EXPORT void irsFreeVideoCaptureSettings(IRS_VideoCaptureSettings*);
IRSENY_EXPORT int irsGetVideoCaptureProperty(IRS_VideoCaptureSettings*, IRS_VideoCaptureProperty);
IRSENY_EXPORT bool irsSetVideoCaptureProperty(IRS_VideoCaptureSettings*, IRS_VideoCaptureProperty, int);
IRSENY_EXPORT bool irsGetVideoCapturePropertyAuto(IRS_VideoCaptureSettings*, IRS_VideoCaptureProperty);
IRSENY_EXPORT bool irsSetVideoCapturePropertyAuto(IRS_VideoCaptureSettings*, IRS_VideoCaptureProperty);
IRSENY_EXPORT IRS_VideoCapture* irsCreateVideoCapture(IRS_VideoCaptureContext*, IRS_VideoCaptureSettings*);
IRSENY_EXPORT void irsDestroyVideoCapture(IRS_VideoCaptureContext*, IRS_VideoCapture*);
IRSENY_EXPORT bool irsGetVideoCaptureSettings(IRS_VideoCapture*, IRS_VideoCaptureSettings*);
IRSENY_EXPORT bool irsStartVideoCapture(IRS_VideoCapture*, IRS_VideoCaptureFrame*);
IRSENY_EXPORT bool irsStopVideoCapture(IRS_VideoCapture*);
IRSENY_EXPORT bool irsBeginVideoCaptureFrameGrab(IRS_VideoCapture*);
IRSENY_EXPORT bool irsEndVideoCaptureFrameGrab(IRS_VideoCapture*);
IRSENY_EXPORT IRS_VideoCaptureFrame* irsCreateVideoCaptureFrame(IRS_VideoCapture*);
IRSENY_EXPORT void irsDestroyVideoCaptureFrame(IRS_VideoCapture*, IRS_VideoCaptureFrame*);
IRSENY_EXPORT int irsGetVideoCaptureFrameProperty(IRS_VideoCaptureFrame*, IRS_VideoCaptureFrameProperty);
IRSENY_EXPORT bool irsCopyVideoCaptureFrame(IRS_VideoCaptureFrame*, char*, size_t);

#endif // IRSENY_VIDEO_CATPURE_H
