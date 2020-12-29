#ifdef LINUX
#include "VideoCapture.h"
#include <cstring>

IRS_VideoCaptureContext* irsCreateVideoCaptureContext() {
	return (IRS_VideoCaptureContext*)(0x1);
}

void irsDestroyVideoCaptureContext(IRS_VideoCaptureContext* context) {
	return;
}

IRS_VideoCaptureSettings* irsAllocVideoCaptureSettings() {
	IRS_VideoCaptureSettings* info = (IRS_VideoCaptureSettings*)malloc(sizeof(IRS_VideoCaptureSettings));
	info->DeviceIndex = 0;
	info->Resolution[0] = -1;
	info->Resolution[1] = -1;
	info->FrameRate = -1;
	info->Brightness = -1;
	info->Gain = -1;
	info->Exposure = -1;
	return info;
}

void irsFreeVideoCaptureSettings(IRS_VideoCaptureSettings* info) {
	free(info);
}

int irsGetVideoCaptureProperty(IRS_VideoCaptureSettings* info, IRS_VideoCaptureProperty property) {
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
bool irsSetVideoCaptureProperty(IRS_VideoCaptureSettings* info, IRS_VideoCaptureProperty property, int value) {
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
bool irsSetVideoCapturePropertyAuto(IRS_VideoCaptureSettings* info, IRS_VideoCaptureProperty property) {
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
bool irsGetVideoCapturePropertyAuto(IRS_VideoCaptureSettings* info, IRS_VideoCaptureProperty property) {
	switch (property) {
	default:
		return false;
	}
}

IRS_VideoCapture* irsCreateVideoCapture(IRS_VideoCaptureContext* context, IRS_VideoCaptureSettings* info) {
	IRS_VideoCapture* result = (IRS_VideoCapture*)malloc(sizeof(IRS_VideoCapture));
	result->Buffer = NULL;
	result->Settings = *info;
	result->CurrentFrame = -1;
	printf("creating capture\n");
	result->Capture = new cv::VideoCapture();
	printf("capture created\n");
	printf("opening capture\n");
	result->Capture->open(info->DeviceIndex);
	printf("open completed\n");
	if (!result->Capture->isOpened()) {
		printf("open failed\n");
		delete result->Capture;
		free(result);
		printf("cleanup finished\n");
		return NULL;

	}
	printf("open successful\n");
	printf("applying settings\n");
	bool widthAccepted = result->Capture->set(CV_CAP_PROP_FRAME_WIDTH, (double)info->Resolution[0]);
	bool heightAccepted = result->Capture->set(CV_CAP_PROP_FRAME_HEIGHT, (double)info->Resolution[1]);
	//bool fpsAccepted = result->Capture->set(CV_CAP_PROP_FPS, (double)info->FrameRate);
	bool brightAccepted = false;
	if (info->Brightness >= 0) {
		 brightAccepted = result->Capture->set(CV_CAP_PROP_BRIGHTNESS, (double)info->Brightness);
	}
	bool gainAccepted = false;
	if (info->Gain >= 0) {
		gainAccepted = result->Capture->set(CV_CAP_PROP_GAIN, (double)info->Gain);
	}
	bool autoExposureAccepted = false;
	bool exposureAccepted = false;
	if (info->Exposure >= 0) {
		autoExposureAccepted = result->Capture->set(CV_CAP_PROP_AUTO_EXPOSURE, 0.0);
		exposureAccepted = result->Capture->set(CV_CAP_PROP_EXPOSURE, (double)info->Exposure);
	} else {
		autoExposureAccepted = result->Capture->set(CV_CAP_PROP_AUTO_EXPOSURE, 1.0);
	}


	int width = (int)result->Capture->get(CV_CAP_PROP_FRAME_WIDTH);
	int height = (int)result->Capture->get(CV_CAP_PROP_FRAME_HEIGHT);
	int frame = (int)result->Capture->get(CV_CAP_PROP_FRAME_COUNT);
	//int fps = (int)result->Capture->get(CV_CAP_PROP_FPS);
	double bright = result->Capture->get(CV_CAP_PROP_BRIGHTNESS);
	double gain = result->Capture->get(CV_CAP_PROP_GAIN);
	double exposure = result->Capture->get(CV_CAP_PROP_EXPOSURE);
	double autoExposure = result->Capture->get(CV_CAP_PROP_AUTO_EXPOSURE);


	printf("settings received\n");
	printf("accepted width %i: %i\n", info->Resolution[0], widthAccepted);
	printf("current width: %i\n", width);
	printf("accepted height %i: %i\n", info->Resolution[1], heightAccepted);
	printf("current height: %i\n", height);
	printf("accepted brightness %i: %i\n", info->Brightness, brightAccepted);
	printf("current brightness: %f\n", bright);
	printf("accepted gain %i: %i\n", info->Gain, gainAccepted);
	printf("current gain: %f\n", gain);
	printf("accepted auto exposure: %i\n", autoExposureAccepted);
	printf("current auto exposure: %f\n", autoExposure);
	printf("accepted exposure %i: %i\n", info->Exposure, exposureAccepted);
	printf("current exposure: %f\n", exposure);
	printf("current frame %i\n", frame);

	result->Settings.Resolution[0] = width;
	result->Settings.Resolution[1] = height;
	//result->Settings.FrameRate = fps;
	result->Settings.Brightness = bright;
	result->Settings.Gain = gain;
	result->Settings.Exposure = exposure;
	result->CurrentFrame = frame - 1;

	return result;
}

void irsDestroyVideoCapture(IRS_VideoCaptureContext* context, IRS_VideoCapture* capture) {
	delete capture->Capture;
	free(capture);
}
bool irsGetVideoCaptureSettings(IRS_VideoCapture* capture, IRS_VideoCaptureSettings* settings) {
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
IRS_VideoCaptureFrame* irsCreateVideoCaptureFrame(IRS_VideoCapture* capture) {
	printf("alloc frame\n");
	IRS_VideoCaptureFrame* result = new IRS_VideoCaptureFrame();

	//IRS_VideoCaptureFrame* result = (IRS_VideoCaptureFrame*)malloc(sizeof(IRS_VideoCaptureFrame));
	//printf("construct frame\n");
	//new(result) IRS_VideoCaptureFrame();
	printf("create frame\n");
	result->create(capture->Settings.Resolution[1], capture->Settings.Resolution[0], CV_8U);
	printf("frame created");
	return result;
}
void irsDestroyVideoCaptureFrame(IRS_VideoCapture* capture, IRS_VideoCaptureFrame* frame) {
	//frame->release(); // TODO check if release calls are necessary or harmful
	//frame->~IRS_VideoCaptureFrame();
	//free(frame);
	delete frame;
}
bool irsBeginVideoCaptureFrameGrab(IRS_VideoCapture* capture) {
	if (capture->Buffer == NULL) {
		return false;
	}
	if (!capture->Capture->grab()) {
		return false;
	}
	return true;

}
bool irsEndVideoCaptureFrameGrab(IRS_VideoCapture* capture) {
	if (capture->Buffer == NULL) {
		return false;
	}
	if (!capture->Capture->retrieve(*capture->Buffer)) {
		return false;
	}
	//capture->CurrentFrame = capture->Capture->get(CV_CAP_PROP_FRAME_COUNT);
}
int irsGetVideoCaptureFrameProperty(IRS_VideoCaptureFrame* frame, IRS_VideoCaptureFrameProperty property) {
	switch (property) {
	case IRS_VideoCaptureFrameProperty::Width:
		return frame->cols;
	case IRS_VideoCaptureFrameProperty::Height:
		return frame->rows;
	case IRS_VideoCaptureFrameProperty::Stride:
		return frame->cols;
	case IRS_VideoCaptureFrameProperty::PixelFormat:
		switch (frame->elemSize()) {
		case (sizeof(char)*1):
			return IRS_VideoCapturePixelFormat::GrayScale8;
		case (sizeof(char)*2):
			return IRS_VideoCapturePixelFormat::GrayScale16;
		case (sizeof(char)*3):
			return IRS_VideoCapturePixelFormat::RGB24;
		case (sizeof(char)*4):
			return IRS_VideoCapturePixelFormat::ARGB32;
		default:
			return -1;
		}
	}
}
bool irsCopyVideoCaptureFrame(IRS_VideoCaptureFrame* frame, char* buffer, size_t bufferSize) {
	size_t frameSize = frame->cols*frame->rows*frame->elemSize();
	if (frameSize > bufferSize) {
		return false;
	}
	memcpy(buffer, frame->data, frameSize);
	return true;
}
#endif // LINUX
