#include "VideoCapture.h"
#include <cstring>

IRS_VideoCaptureContext* irsCreateVideoCaptureContext() {
	return (IRS_VideoCaptureContext*)(0x1);
}

bool irsDestroyVideoCaptureContext(IRS_VideoCaptureContext* context) {
	return true;
}

IRS_VideoCaptureConstructionInfo* irsAllocVideoCaptureConstructionInfo() {
	IRS_VideoCaptureConstructionInfo* info = (IRS_VideoCaptureConstructionInfo*)malloc(sizeof(IRS_VideoCaptureConstructionInfo));
	info->DeviceIndex = 0;
	info->Resolution[0] = -1;
	info->Resolution[1] = -1;
	info->FrameRate = -1;
	info->Brightness = -1;
	info->Gain = -1;
	info->Exposure = -1;
	return info;
}

bool irsFreeVideoCaptureConstructionInfo(IRS_VideoCaptureConstructionInfo* info) {
	free(info);
	return true;
}

int irsGetVideoCaptureProperty(IRS_VideoCaptureConstructionInfo* info, IRS_VideoCaptureProperty property) {
	switch (property) {
	case IRS_VideoCaptureProperty::FrameWidth:
		return info->Resolution[0];
	case IRS_VideoCaptureProperty::FrameHeight:
		return info->Resolution[1];
	case IRS_VideoCaptureProperty::FrameRate:
		return info->FrameRate;
	case IRS_VideoCaptureProperty::Brightness:
		return info->Brightness;
	case IRS_VideoCaptureProperty::Gain:
		return info->Gain;
	case IRS_VideoCaptureProperty::Exposure:
		return info->Exposure;
	default:
		return -1;
	}
}
bool irsSetVideoCaptureProperty(IRS_VideoCaptureConstructionInfo* info, IRS_VideoCaptureProperty property, int value) {
	switch (property) {
	case IRS_VideoCaptureProperty::FrameWidth:
		info->Resolution[0] = value;
		return true;
	case IRS_VideoCaptureProperty::FrameHeight:
		info->Resolution[1] = value;
		return true;
	case IRS_VideoCaptureProperty::FrameRate:
		info->FrameRate = value;
		return true;
	case IRS_VideoCaptureProperty::Brightness:
		info->Brightness = value;
		return true;
	case IRS_VideoCaptureProperty::Gain:
		info->Gain = value;
		return true;
	case IRS_VideoCaptureProperty::Exposure:
		info->Exposure = value;
		return true;
	default:
		return false;
	}
}
bool irsSetVideoCapturePropertyAuto(IRS_VideoCaptureConstructionInfo* info, IRS_VideoCaptureProperty property) {
	switch (property) {
	case IRS_VideoCaptureProperty::FrameWidth:
		info->Resolution[0] = -1;
		return true;
	case IRS_VideoCaptureProperty::FrameHeight:
		info->Resolution[1] = -1;
		return true;
	case IRS_VideoCaptureProperty::FrameRate:
		info->FrameRate = -1;
		return true;
	case IRS_VideoCaptureProperty::Brightness:
		info->Brightness = -1;
		return true;
	case IRS_VideoCaptureProperty::Gain:
		info->Gain = -1;
		return true;
	case IRS_VideoCaptureProperty::Exposure:
		info->Exposure = -1;
		return true;
	default:
		return false;
	}
}
bool irsGetVideoCapturePropertyAuto(IRS_VideoCaptureConstructionInfo* info, IRS_VideoCaptureProperty property) {
	switch (property) {
	default:
		return false;
	}
}

IRS_VideoCapture* irsCreateVideoCapture(IRS_VideoCaptureContext* context, IRS_VideoCaptureConstructionInfo* info) {
	IRS_VideoCapture* result = (IRS_VideoCapture*)malloc(sizeof(IRS_VideoCapture));
	result->Buffer = NULL;
	result->Settings = *info;
	result->CurrentFrame = -1;
	result->Capture.open(info->DeviceIndex);
	if (!result->Capture.isOpened()) {
		free(result);
		return NULL;
	}

	bool widthAccepted = result->Capture.set(CV_CAP_PROP_FRAME_WIDTH, (double)info->Resolution[0]);
	bool heightAccepted = result->Capture.set(CV_CAP_PROP_FRAME_HEIGHT, (double)info->Resolution[1]);
	bool fpsAccepted = result->Capture.set(CV_CAP_PROP_FPS, (double)info->FrameRate);
	bool brightAccepted = result->Capture.set(CV_CAP_PROP_BRIGHTNESS, (double)info->Brightness);
	bool gainAccepted = result->Capture.set(CV_CAP_PROP_GAIN, (double)info->Gain);
	bool exposureAccepted = result->Capture.set(CV_CAP_PROP_EXPOSURE, (double)info->Exposure);

	int width = (int)result->Capture.get(CV_CAP_PROP_FRAME_WIDTH);
	int height = (int)result->Capture.get(CV_CAP_PROP_FRAME_HEIGHT);
	int fps = (int)result->Capture.get(CV_CAP_PROP_FPS);
	double bright = result->Capture.get(CV_CAP_PROP_BRIGHTNESS);
	double gain = result->Capture.get(CV_CAP_PROP_GAIN);
	double exposure = result->Capture.get(CV_CAP_PROP_EXPOSURE);

	int frame = (int)result->Capture.get(CV_CAP_PROP_FRAME_COUNT);

	result->Settings.Resolution[0] = width;
	result->Settings.Resolution[1] = height;
	result->Settings.FrameRate = fps;
	result->Settings.Brightness = bright;
	result->Settings.Gain = gain;
	result->Settings.Exposure = exposure;
	result->CurrentFrame = frame - 1;

	return result;
}

bool irsDestroyVideoCapture(IRS_VideoCaptureContext* context, IRS_VideoCapture* capture) {
	capture->Capture.release();
	free(capture);
	return true;
}
bool irsGetVideoCaptureSettings(IRS_VideoCapture* capture, IRS_VideoCaptureConstructionInfo* settings) {
	*settings = capture->Settings;
	return true;
}
bool irsStartVideoCapture(IRS_VideoCapture* capture, IRS_VideoCaptureFrame* frame) {
	if (capture->Buffer != NULL) {
		return false;
	}
	capture->Buffer = frame;
	return true;
}
bool irsStopVideoCapture(IRS_VideoCapture* capture) {
	capture->Buffer = NULL;
	return true;
}
IRS_VideoCaptureFrame* irsAllocVideoCaptureFrame(IRS_VideoCapture* capture) {
	IRS_VideoCaptureFrame* result = (IRS_VideoCaptureFrame*)malloc(sizeof(IRS_VideoCaptureFrame));
	result->create(capture->Settings.Resolution[1], capture->Settings.Resolution[0], sizeof(char));
	return result;
}
bool irsFreeVideoCaptureFrame(IRS_VideoCapture* capture, IRS_VideoCaptureFrame* frame) {
	frame->release(); // TODO check if release calls are necessary or harmful
	free(frame);
	return true;
}
bool irsBeginVideoFrameGrab(IRS_VideoCapture* capture) {
	if (capture->Buffer == NULL) {
		return false;
	}
	if (!capture->Capture.grab()) {
		return false;
	}
	return true;

}
bool irsEndVideoFrameGrab(IRS_VideoCapture* capture) {
	if (capture->Buffer == NULL) {
		return false;
	}
	if (!capture->Capture.retrieve(*capture->Buffer)) {
		return false;
	}
	capture->CurrentFrame = capture->Capture.get(CV_CAP_PROP_FRAME_COUNT);
}
bool irsCopyVideoFrame(IRS_VideoCaptureFrame* frame, char* buffer, size_t bufferSize) {
	size_t frameSize = frame->cols*frame->rows*frame->elemSize();
	if (frameSize > bufferSize) {
		return false;
	}
	memcpy(buffer, frame->data, frameSize);
	return true;
}
