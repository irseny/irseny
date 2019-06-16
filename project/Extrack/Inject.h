#ifndef EXTRACK_INJECT_H
#define EXTRACK_INJECT_H

#if LINUX
#ifndef EXTRACK_EXPORT
//#define EXTRACK_EXPORT __attribute__((cdecl))
#define EXTRACK_EXPORT __declspec(dllexport)
#endif

#include "InjectLin.h"
#endif // LINUX

#if WINDOWS
#ifndef EXTRACK_EXPORT
//#define EXTRACK_EXPORT __attribute__(stdcall)
#define EXTRACK_EXPORT __declspec(dllexport)
#endif

#include "InjectWin.h"
#endif // WINDOWS

EXTRACK_EXPORT IvjContext* ivjCreateContext();
EXTRACK_EXPORT bool ivjDestroyContext(IvjContext* context);
EXTRACK_EXPORT IvjKeyboardConstructionInfo* ivjAllocKeyboardConstructionInfo();
EXTRACK_EXPORT bool ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info);
EXTRACK_EXPORT IvjKeyboard* ivjConnectKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info);
EXTRACK_EXPORT bool ivjDisconnectKeyboard(IvjContext* context, IvjKeyboard* keyboard);
EXTRACK_EXPORT bool ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, float state);
EXTRACK_EXPORT bool ivjSendKeyboard(IvjKeyboard* keyboard);
#endif // EXTRACK_INJECT_H
