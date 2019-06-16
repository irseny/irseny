#if LINUX

#include "Inject.h"

#if WITH_UINPUT
#define IVJ_CONTEXT_CANDIDATE_NUM 2
const char* ivjContextCandidates[IVJ_CONTEXT_CANDIDATE_NUM] = {
	"/dev/uinput",
	"/dev/input/uinput"
};
const int32_t ivjKeyCodes[IVJ_KEYBOARD_KEY_NO] = {
	KEY_Q, KEY_W, KEY_E, KEY_R, KEY_T, KEY_Z, KEY_U, KEY_I, KEY_O, KEY_P
};

void ivjLogError(const char* message);

IvjContext* EXTRACK_EXPORT ivjCreateContext() {
	for (int i = 0; i < IVJ_CONTEXT_CANDIDATE_NUM; i++) {
		int32_t file = open(ivjContextCandidates[i], O_WRONLY | O_NDELAY);
		if (file >= 0) {
			IvjContext* context = (IvjContext*)malloc(sizeof(IvjContext));
			context->FileHandle = file;
			return context;
		}
	}
	ivjLogError("uinput file not found");
	return NULL;
}

bool EXTRACK_EXPORT ivjDestroyContext(IvjContext* context) {
	close(context->FileHandle);
	free(context);
	return true;
}

IvjKeyboardConstructionInfo* EXTRACK_EXPORT ivjAllocKeyboardConstructionInfo() {
	IvjKeyboardConstructionInfo* info = (IvjKeyboardConstructionInfo*)malloc(sizeof(IvjKeyboardConstructionInfo));
	const char* name = "Ivj Virtual Keyboard";
	strcpy(info->Name, name);
	info->Vendor = 0x159;
	info->Product = 0x51A1;
	return info;
}
bool EXTRACK_EXPORT ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info) {
	free(info);
	return true;
}

IvjKeyboard* EXTRACK_EXPORT ivjConnectKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info) {
	ivjLogError("activating events");
	// activate all keys
	ioctl(context->FileHandle, UI_SET_EVBIT, EV_KEY);
	ioctl(context->FileHandle, UI_SET_EVBIT, EV_SYN); // TODO: are the last two needed?
	ioctl(context->FileHandle, UI_SET_EVBIT, EV_REP);
	for (int i = 0; i < IVJ_KEYBOARD_KEY_NO; i++) {
		ivjLogError("activating key " + ivjKeyCodes[i]);
		ioctl(context->FileHandle, UI_SET_KEYBIT, ivjKeyCodes[i]);
	}
	ivjLogError("transmitting description");
	// transmit description
	struct uinput_user_dev deviceDescription;
	memset(&deviceDescription, 0, sizeof(struct uinput_user_dev));
	snprintf(deviceDescription.name, UINPUT_MAX_NAME_SIZE, info->Name);
	deviceDescription.id.bustype = BUS_USB;
	deviceDescription.id.version = 0x1;
	deviceDescription.id.vendor = info->Vendor;
	deviceDescription.id.product = info->Product;
	int32_t deviceHandle = context->FileHandle;
	ivjLogError("attempting setup");
	if (ioctl(deviceHandle, UI_DEV_SETUP, &deviceDescription) < 0) {
		ivjLogError("Keyboard setup failed");
		return NULL;
	}
	ivjLogError("attempting keyboard creation");
	// create device
	if (ioctl(deviceHandle, UI_DEV_CREATE, NULL) < 0) {
		ivjLogError("Keyboard creation failed");
		return NULL;
	}
	ivjLogError("allocating result");
	// allocate result
	IvjKeyboard* keyboard = (IvjKeyboard*)malloc(sizeof(IvjKeyboard));
	keyboard->Context = context;
	keyboard->FileHandle = deviceHandle;
	//iotcl(deviceHandle, UI_GET_SYSNAME(sizeof(char)*IVJ_MAX_FILE_PATH_SIZE), keyboard->FilePath);
	keyboard->BufferedEventNo = 0;
	return keyboard;
}
bool EXTRACK_EXPORT	ivjDisconnectKeyboard(IvjContext* context, IvjKeyboard* keyboard) {
	free(keyboard);
	return true;
}
bool EXTRACK_EXPORT ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, float state) {
	if (keyIndex < 0 || keyIndex >= IVJ_KEYBOARD_KEY_NO) {
		ivjLogError("Key index out of range");
		return false;
	}
	// make sure we can write the event somewhere
	if (keyboard->BufferedEventNo >= IVJ_KEYBOARD_MAX_BUFFERED_EVENT_NO) {
		ivjSendKeyboard(keyboard);
	}
	// TODO: compare with current keyboard state
	// init event
	struct input_event* event = &keyboard->BufferedEvents[keyboard->BufferedEventNo];
	event->type = EV_KEY;
	event->code = ivjKeyCodes[keyIndex];
	if (state > 0.0f) {
		event->value = 1;
	} else {
		event->value = 0;
	}
	event->time.tv_sec = 0;
	event->time.tv_usec = 0;
	keyboard->BufferedEventNo += 1;
	return true;
}
bool EXTRACK_EXPORT ivjSendKeyboard(IvjKeyboard* keyboard) {
	if (keyboard->BufferedEventNo < 1) {
		return true;
	}
	bool result = true;
	size_t size = sizeof(struct input_event)*keyboard->BufferedEventNo;
	// send events
	if (write(keyboard->FileHandle, keyboard->BufferedEvents, size) < size) {
		ivjLogError("Could not send keyboard events");
		result = false;
	}
	keyboard->BufferedEventNo = 0;
	// send syn
	struct input_event syn;
	syn.type = EV_SYN;
	syn.code = SYN_REPORT;
	syn.value = 0;
	syn.time.tv_sec = 0;
	syn.time.tv_usec = 0;
	size = sizeof(struct input_event);
	if (write(keyboard->FileHandle, &syn, size) < size) {
		ivjLogError("Could not send keyboard syn event");
		result = false;
	}
	return result;
}
void ivjLogError(const char* message) {
	printf("Ivj Error: %s\n", message);
}
#endif // WITH_UINPUT
#endif // LINUX


