#include "InjectTypes.h"

#if LINUX
#define IVJ_CONTEXT_CANDIDATE_NUM 2
char* ivjContextCandidates[IVJ_CONTEXT_CANDIDATE_NUM] = {
	"/dev/uinput",
	"/dev/input/uinput"
};

IvjContext* EXTRACK_EXPORT ivjCreateContext() {	
	for (int i = 0; i < IVJ_CONTEXT_CANDIDATE_NUM; i++) {
		int32_t file = open(ivjContextCandidates[i], O_WRONLY | O_NDELAY);
		if (file >= 0) {
			IvjContext* context = (IvjContext*)malloc(sizeof(IvjContext));
			context->fileHandle = file;
			context->filePath = ivjContextCandidates[i];
			return context;
		}
	}
	return NULL;
}

bool EXTRACK_EXPORT ivjDestroyContext(IvjContext* context) {
	close(context->fileHandle);
	free(context);
	return true;
}

IvjKeyboardConstructionInfo* EXTRACK_EXPORT ivjAllocKeyboardConstructionInfo() {
	IvjKeyboardConstructionInfo* info = (IvjKeyboardConstructionInfo*)malloc(sizeof(IvjKeyboardConstructionInfo));
	info->Name = "Ivj Virtual Keyboard";
	info->Vendor = 0x159;
	info->Product = 0x51A1;
	return info;
}
bool EXTRACK_EXPORT ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info) {
	free(info);
	return true;
}

IvjKeyboard* EXTRACK_EXPORT ivjOpenKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* constructionInfo) {
	// description
	struct uinput_user_dev deviceDescription;
	memset(&deviceDescription, 0, sizeof(struct uinput_user_dev));
	snprintf(deviceDescription.name, UINPUT_MAX_NAME_SIZE, constructionInfo->Name);
	deviceDescription.id.bustype = BUS_USB;
	deviceDescription.id.version = 0x1;
	deviceDescription.id.vendor = constructionInfo->Vendor;
	deviceDescription.id.product = constructionInfo->Product;
	// transmit description
	if (write(context->FileHandle, &deviceDescription, sizeof(struct uinput_user_dev)) < sizeof(struct uinput_user_dev)) {
		return NULL;
	}
	ioctl(context->FileHandle, UI_SET_EVBIT, EV_KEY);
	ioctl(context->FileHandle, UI_SET_EVBIT, EV_SYN);
	ioctl(context->FileHandle, UI_SET_EVBIT, EV_REP);
	for (char key = KEY_ESC; key < KEY_F24; key++) {
		ioctl(context->FileHandle, UI_SET_KEYBIT, key);
	}
	// create device
	int32_t deviceHandle = context->FileHandle;
	if (ioctl(deviceHandle, UI_DEV_CREATE, NULL) < 0) {
		return NULL;
	}
	IvjKeyboard* keyboard = (IvjKeyboard*)malloc(sizeof(IvjKeyboard));
	keyboard->FileHandle = deviceHandle;
	//keyboard->FilePath = iotcl(deviceHandle, UI_GET_SYSNAME, NULL);
	return keyboard;
}
	
	
#endif



#if WINDOWS

#endif