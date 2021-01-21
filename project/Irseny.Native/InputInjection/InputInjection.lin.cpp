// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#include "InputInjection.h"

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
void ivjLogWarning(const char* message);

IvjContext*  ivjCreateContext() {
	for (int i = 0; i < IVJ_CONTEXT_CANDIDATE_NUM; i++) {
		int32_t fileHandle = open(ivjContextCandidates[i], O_WRONLY | O_NDELAY);
		if (fileHandle >= 0) {
			close(fileHandle);
			IvjContext* context = (IvjContext*)malloc(sizeof(IvjContext));
			memset(context, 0, sizeof(IvjContext));
			strncpy(context->FilePath, ivjContextCandidates[i], IVJ_CONTEXT_MAX_PATH_LENGTH);
			return context;
		}
	}
	ivjLogError("uinput file not found or insufficient permission");
	return NULL;
}

bool  ivjDestroyContext(IvjContext* context) {
	free(context);
	return true;
}

IvjKeyboardConstructionInfo*  ivjAllocKeyboardConstructionInfo() {
	IvjKeyboardConstructionInfo* info = (IvjKeyboardConstructionInfo*)malloc(sizeof(IvjKeyboardConstructionInfo));
	const char* name = "Ivj Virtual Keyboard";
	strcpy(info->Name, name);
	info->Vendor = 0x159;
	info->Product = 0x51A1;
	return info;
}
bool  ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info) {
	free(info);
	return true;
}

IvjKeyboard*  ivjConnectKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info) {
	// activate all keys
	int32_t fileHandle = open(context->FilePath, O_WRONLY | O_NDELAY);
	if (fileHandle < 0) {
		ivjLogError("failed to open uinput file");
		return NULL;
	}
	char number[16];
	if (ioctl(fileHandle, UI_SET_EVBIT, EV_KEY) < 0) {
		ivjLogWarning("failed to activate key events");
	}
	if (ioctl(fileHandle, UI_SET_EVBIT, EV_SYN) < 0) {
		ivjLogWarning("failed to activate syn events");
	}
	if (ioctl(fileHandle, UI_SET_EVBIT, EV_REP) < 0) {
		ivjLogWarning("failed to activate report events");
	}
	bool keyBitError = false;
	for (int i = 0; i < IVJ_KEYBOARD_KEY_NO; i++) {
		if (ioctl(fileHandle, UI_SET_KEYBIT, ivjKeyCodes[i]) < 0) {
			keyBitError = true;
		}
	}
	if (keyBitError) {
		ivjLogWarning("failed to active key codes");
	}
	// transmit description
	struct uinput_user_dev deviceDescription;
	memset(&deviceDescription, 0, sizeof(struct uinput_user_dev));
	snprintf(deviceDescription.name, UINPUT_MAX_NAME_SIZE, info->Name);
	deviceDescription.id.bustype = BUS_USB;
	deviceDescription.id.version = 0x1;
	deviceDescription.id.vendor = info->Vendor;
	deviceDescription.id.product = info->Product;
	if (ioctl(fileHandle, UI_DEV_SETUP, &deviceDescription) < 0) {
		ivjLogError("Keyboard setup failed");
		close(fileHandle);
		return NULL;
	}
	// create device
	if (ioctl(fileHandle, UI_DEV_CREATE, NULL) < 0) {
		ivjLogError("Keyboard creation failed");
		close(fileHandle);
		return NULL;
	}
	// allocate result
	IvjKeyboard* keyboard = (IvjKeyboard*)malloc(sizeof(IvjKeyboard));
	keyboard->Context = context;
	keyboard->FileHandle = fileHandle;
	//iotcl(deviceHandle, UI_GET_SYSNAME(sizeof(char)*IVJ_MAX_FILE_PATH_SIZE), keyboard->FilePath);
	keyboard->BufferedEventNo = 0;
	return keyboard;
}
bool 	ivjDisconnectKeyboard(IvjContext* context, IvjKeyboard* keyboard) {
	if (ioctl(keyboard->FileHandle, UI_DEV_DESTROY) < 0) {
		ivjLogWarning("Keyboard destruction failed");
	}
	close(keyboard->FileHandle);
	free(keyboard);
	return true;
}
bool  ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, bool pressed) {
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
	if (pressed) {
		event->value = 1;
	} else {
		event->value = 0;
	}
	event->time.tv_sec = 0;
	event->time.tv_usec = 0;
	keyboard->BufferedEventNo += 1;
	return true;
}
bool  ivjSendKeyboard(IvjKeyboard* keyboard) {
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
void ivjLogWarning(const char* message) {
	printf("Ivj Warning: %s\n", message);
}
#endif // WITH_UINPUT

#if WITH_FREETRACK
 IvjFreetrackConstructionInfo* ivjAllocFreetrackConstructionInfo() {
	return (IvjFreetrackConstructionInfo*)1;
}
 bool ivjFreeFreetrackConstructionInfo(IvjFreetrackConstructionInfo* info) {
	return true;
}
 IvjFreetrackInterface* ivjConnectFreetrackInterface(IvjContext* context, IvjFreetrackConstructionInfo* info) {
	IvjFreetrackInterface* result = (IvjFreetrackInterface*)malloc(sizeof(IvjFreetrackInterface));
	memset(result, 0, sizeof(IvjFreetrackInterface));
	return result;
}
 bool ivjDisconnectFreetrackInterface(IvjContext* context, IvjFreetrackInterface* freetrack) {
	free(freetrack);
	return true;
}
 bool ivjSetFreetrackResolution(IvjFreetrackInterface* freetrack, int width, int height) {
	// TODO: implement
	return true;
}
 bool ivjSetFreetrackPoint(IvjFreetrackInterface* freetrack, int pointIndex, int x, int y) {
	return true;
}
 bool ivjSetFreetrackAxis(IvjFreetrackInterface* freetrack, int axisIndex, float smooth, float raw) {
	return true;
}
 bool ivjSendFreetrackInterface(IvjFreetrackInterface* freetrack) {
	return true;
}
#endif // WITH_FREETRACK


