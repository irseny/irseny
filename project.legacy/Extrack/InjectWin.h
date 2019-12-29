#if WINDOWS
#ifndef EXTRACK_INJECT_TYPES_H
#define EXTRACK_INJECT_TYPES_H

#if WITH_WINAPI
#include <windows.h>
#include <inttypes.h>
#include <stdbool.h>
#include <stdlib.h>
#include <stdio.h>

#define IVJ_KEYBOARD_KEY_NO 10
#define IVJ_FREETRACK_AXIS_NO 6
#define IVJ_FREETRACK_POINT_NO 4
#define IVJ_MAX_BUFFERED_EVENT_NO 32

typedef struct IvjFreetrackPacket IvjFreetrackPacket;

typedef struct IvjContext {
	HANDLE FreetrackSync;
	HANDLE FreetrackFile;
} IvjContext;

typedef struct {
	int32_t BufferedEventNo;
	INPUT BufferedEvents[IVJ_MAX_BUFFERED_EVENT_NO];
} IvjKeyboard;

typedef int32_t IvjKeyboardConstructionInfo;

typedef struct IvjFreetrackPacket{
	int32_t PacketID;
	int32_t Resolution[2];
	float SmoothAxes[IVJ_FREETRACK_AXIS_NO];
	float RawAxes[IVJ_FREETRACK_AXIS_NO];
	float Points[IVJ_FREETRACK_POINT_NO][2];
	int32_t Junk[4];
} IvjFreetrackPacket;

typedef struct {
	HANDLE Sync;
	IvjFreetrackPacket* Map;
	IvjFreetrackPacket Packet;
} IvjFreetrackInterface;

typedef int32_t IvjFreetrackConstructionInfo;

#ifdef WITH_VJOY

typedef struct {
	int32_t JoystickIndex;
} IvjJoystickConstructionInfo;

#endif // WITH_VJOY
#endif // WITH_WINAPI
#endif // EXTRACK_INJECT_TYPES_H
#endif // WINDOWS
