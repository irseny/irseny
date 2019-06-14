#ifndef EXTRACK_INJECT_H
#define EXTRACK_INJECT_H

#if LINUX
#ifndef EXTRACK_EXPORT
//#define EXTRACK_EXPORT __attribute__((cdecl))
#define EXTRACK_EXPORT
#endif

#include "InjectLin.h"
#endif // LINUX

#if WINDOWS
#ifndef EXTRACK_EXPORT
#define EXTRACK_EXPORT __attribute__(stdcall)
#endif

#include "InjectWin.h"
#endif // WINDOWS

EXTRACK_EXPORT IvjContext*  ivjCreateContext();
bool EXTRACK_EXPORT ivjDestroyContext(IvjContext* context);
IvjKeyboardConstructionInfo* EXTRACK_EXPORT ivjAllocKeyboardConstructionInfo();
bool EXTRACK_EXPORT ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info);
IvjKeyboard* EXTRACK_EXPORT ivjConnectKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info);
bool EXTRACK_EXPORT ivjDisconnectKeyboard(IvjContext* context, IvjKeyboard* keyboard);
bool EXTRACK_EXPORT ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, float state);
bool EXTRACK_EXPORT ivjSendKeyboard(IvjKeyboard* keyboard);
void ivjLogError(char* message);
#endif // EXTRACK_INJECT_H
