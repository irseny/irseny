
#ifndef IRSENY_VIDEO_CAPTURE_TYPES_H
#define IRSENY_VIDEO_CAPTURE_TYPES_H

#if WITH_OPENCV

#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>

typedef enum IRS_VideoCaptureProperty {
	Device = 0,
	FrameWidth = CV_CAP_PROP_FRAME_WIDTH,
	FrameHeight = CV_CAP_PROP_FRAME_HEIGHT,
	FrameRate = CV_CAP_PROP_FPS,
	Brightness = CV_CAP_PROP_BRIGHTNESS,
	Gain = CV_CAP_PROP_GAIN,
	Exposure = CV_CAP_PROP_EXPOSURE
} IRS_VideoCaptureProperty;

typedef int IRS_VideoCaptureContext;

typedef struct IRS_VideoCaptureContstructionInfo {
	int DeviceIndex;
	int Resolution[2];
	int FrameRate;
	int Brightness;
	int Gain;
	int Exposure;
} IRS_VideoCaptureConstructionInfo;

typedef cv::Mat IRS_VideoCaptureFrame;
/*typedef struct IRS_VideoCatureFrame {
	cv::Mat Data;
	int CurrentFrame;
} IRS_VideoCaptureFrame;*/

typedef struct IRS_VideoCapture {
	cv::VideoCapture Capture;
	IRS_VideoCaptureConstructionInfo Settings;
	IRS_VideoCaptureFrame* Buffer;
	int CurrentFrame;
	float PropertyMin;
	float PropertyMax;
} IRS_VideoCapture;


#endif // WITH_OPENCV

#endif // IRSENY_VIDEO_CAPTURE_TYPES_H
