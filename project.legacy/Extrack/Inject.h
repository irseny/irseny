#ifndef EXTRACK_INJECT_H
#define EXTRACK_INJECT_H

#if LINUX
#ifndef EXTRACK_EXPORT
//#define EXTRACK_EXPORT __attribute__((cdecl))
#define EXTRACK_EXPORT __attribute__ ((dllexport))
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
EXTRACK_EXPORT bool ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, bool pressed);
EXTRACK_EXPORT bool ivjSendKeyboard(IvjKeyboard* keyboard);

#if WITH_FREETRACK
EXTRACK_EXPORT IvjFreetrackConstructionInfo* ivjAllocFreetrackConstructionInfo();
EXTRACK_EXPORT bool ivjFreeFreetrackConstructionInfo(IvjFreetrackConstructionInfo* info);
EXTRACK_EXPORT IvjFreetrackInterface* ivjConnectFreetrackInterface(IvjContext* context, IvjFreetrackConstructionInfo* info);
EXTRACK_EXPORT bool ivjDisconnectFreetrackInterface(IvjContext* context, IvjFreetrackInterface* freetrack);
EXTRACK_EXPORT bool ivjSetFreetrackResolution(IvjFreetrackInterface* freetrack, int width, int height);
EXTRACK_EXPORT bool ivjSetFreetrackPoint(IvjFreetrackInterface* freetrack, int pointIndex, int x, int y);
EXTRACK_EXPORT bool ivjSetFreetrackAxis(IvjFreetrackInterface* freetrack, int axisIndex, float smooth, float raw);
EXTRACK_EXPORT bool ivjSendFreetrackInterface(IvjFreetrackInterface* freetrack);
#endif // WITH_FREETRACK

#if WITH_JOYSTICK
EXTRACK_EXPORT IvjJoystickConstructionInfo* ivjAllocJoystickConstructionInfo();
EXTRACK_EXPORT bool ivjSetJoystickIndex(IvjJoystickConstructionInfo* info, int joyIndex);
EXTRACK_EXPORT bool ivjFreeJoystickConstructionInfo(IvjJoystickConstructionInfo* info);
#endif // WITH_JOYSTICK

#endif // EXTRACK_INJECT_H
