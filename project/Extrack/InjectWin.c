#if WINDOWS
#include "Inject.h"

#if WITH_WINAPI

WORD ivjKeyCodes[IVJ_KEYBOARD_KEY_NO] = {
	0x51, 0x57, 0x45, 0x52, 0x54, 0x5A, 0x55, 0x49, 0x4F, 0x50
};
void ivjLogError(const char* message);

EXTRACK_EXPORT IvjContext* ivjCreateContext() {
	return (IvjContext*)1;
}
EXTRACK_EXPORT bool ivjDestroyContext(IvjContext* context) {
	//free(context);
	return true;
}
EXTRACK_EXPORT IvjKeyboardConstructionInfo* ivjAllocKeyboardConstructionInfo() {
	return (IvjKeyboardConstructionInfo*)1;
}
EXTRACK_EXPORT bool ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info) {
	//free(info);
	return true;
}
EXTRACK_EXPORT IvjKeyboard* ivjConnectKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info) {
	IvjKeyboard* keyboard = (IvjKeyboard*)malloc(sizeof(IvjKeyboard));
	keyboard->BufferedEventNo = 0;
	return keyboard;
}
EXTRACK_EXPORT bool ivjDisconnectKeyboard(IvjContext* context, IvjKeyboard* keyboard) {
	free(keyboard);
	return true;
}
EXTRACK_EXPORT bool ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, float state) {
	if (keyIndex < 0 || keyIndex >= IVJ_KEYBOARD_KEY_NO) {
		return false;
	}
	if (keyboard->BufferedEventNo >= IVJ_MAX_BUFFERED_EVENT_NO) {
		ivjSendKeyboard(keyboard);
	}
	if (keyboard->BufferedEventNo >= IVJ_MAX_BUFFERED_EVENT_NO) {
		return false;
	}
	INPUT* event = &keyboard->BufferedEvents[keyboard->BufferedEventNo];
	event->type = INPUT_KEYBOARD;
	event->ki.wVk = ivjKeyCodes[keyIndex];
	event->ki.wScan = 0;
	if (state <= 0.0f) {
		event->ki.dwFlags = KEYEVENTF_KEYUP;
	} else {
		event->ki.dwFlags = 0; // keyup
	}
	event->ki.time = 0; // provided by system
	event->ki.dwExtraInfo = GetMessageExtraInfo();
	keyboard->BufferedEventNo += 1;
	return true;
}
EXTRACK_EXPORT bool ivjSendKeyboard(IvjKeyboard* keyboard) {
	bool result = true;
	if (keyboard->BufferedEventNo < 1) {
		return true;
	}
	uint32_t errorCode = SendInput(keyboard->BufferedEventNo, keyboard->BufferedEvents, sizeof(INPUT));
	if (errorCode != keyboard->BufferedEventNo) {
		ivjLogError("Failed to inject keyboard input");
		if (errorCode == 0) {
			ivjLogError("Injection blocked by another thread");
		}
		ivjLogError("Injection might have been blocked by UIPI");
		result = false;
	}
	keyboard->BufferedEventNo = 0;
	return  result;
}
void ivjLogError(const char* message) {
	printf("Ivj Error: %s\n", message);
}
#endif // WITH_WINAPI
#endif // WINDOWS
