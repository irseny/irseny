#ifndef IRSENY_INPUT_INJECTION_H
#define IRSENY_INPUT_INJECTION_H

#include "IrsenyExport.h"

#if LINUX
#include "InputInjection.lin.h"
#endif

#if WINDOWS
#include "InputInjection.win.h"
#endif



IRSENY_EXPORT IvjContext* ivjCreateContext();
IRSENY_EXPORT bool ivjDestroyContext(IvjContext* context);

IRSENY_EXPORT IvjKeyboardConstructionInfo* ivjAllocKeyboardConstructionInfo();
IRSENY_EXPORT bool ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info);
IRSENY_EXPORT IvjKeyboard* ivjConnectKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info);
IRSENY_EXPORT bool ivjDisconnectKeyboard(IvjContext* context, IvjKeyboard* keyboard);
IRSENY_EXPORT bool ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, bool pressed);
IRSENY_EXPORT bool ivjSendKeyboard(IvjKeyboard* keyboard);

IRSENY_EXPORT IvjFreetrackConstructionInfo* ivjAllocFreetrackConstructionInfo();
IRSENY_EXPORT bool ivjFreeFreetrackConstructionInfo(IvjFreetrackConstructionInfo* info);
IRSENY_EXPORT IvjFreetrackInterface* ivjConnectFreetrackInterface(IvjContext* context, IvjFreetrackConstructionInfo* info);
IRSENY_EXPORT bool ivjDisconnectFreetrackInterface(IvjContext* context, IvjFreetrackInterface* freetrack);
IRSENY_EXPORT bool ivjSetFreetrackResolution(IvjFreetrackInterface* freetrack, int width, int height);
IRSENY_EXPORT bool ivjSetFreetrackPoint(IvjFreetrackInterface* freetrack, int pointIndex, int x, int y);
IRSENY_EXPORT bool ivjSetFreetrackAxis(IvjFreetrackInterface* freetrack, int axisIndex, float smooth, float raw);
IRSENY_EXPORT bool ivjSendFreetrackInterface(IvjFreetrackInterface* freetrack);

#if WITH_JOYSTICK
IRSENY_EXPORT IvjJoystickConstructionInfo* ivjAllocJoystickConstructionInfo();
IRSENY_EXPORT bool ivjSetJoystickIndex(IvjJoystickConstructionInfo* info, int joyIndex);
IRSENY_EXPORT bool ivjFreeJoystickConstructionInfo(IvjJoystickConstructionInfo* info);
#endif // WITH_JOYSTICK

#endif // IRSENY_INPUT_INJECTION_H
