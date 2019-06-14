#if WINDOWS

#include "Inject.h"

#if WITH_WINAPI
IvjContext* EXTRACK_EXPORT ivjCreateContext() {
	IvjContext* context = (IvjContext*)malloc(sizeof(IvjContext));
	return context;
}
bool EXTRACK_EXPORT ivjDestroyContext(IvjContext* context) {
	free(context);
	return true;
}
IvjKeyboardConstructionInfo* EXTRACK_EXPORT ivjAllocKeyboardConstructionInfo() {
	IvjKeyboardConstructionInfo* info = (IvjKeyboardConstructionInfo*)malloc(sizeof(IvjKeyboardConstructionInfo));
	return info;
}
bool EXTRACK_EXPORT ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info) {
	free(info);
	return true;
}
IvjKeyboard* EXTRACK_EXPORT ivjOpenKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info) {
	IvjKeyboard* keyboard = (IvjKeyboard*)malloc(sizeof(IvjKeyboard));
	keyboard->BufferedEventNo = 0;
	return keyboard;
}
bool EXTRACK_EXPORT ivjCloseKeyboard(IvjContext* context, IvjKeyboard* keyboard) {
	free(keyboard);
	return true;
}
#endif // WITH_WINAPI
#endif // WINDOWS
