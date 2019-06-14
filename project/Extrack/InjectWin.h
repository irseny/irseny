#if WINDOWS
#ifndef EXTRACK_INJECT_TYPES_H
#define EXTRACK_INJECT_TYPES_H

#if WITH_WINAPI
#include <windows.h>
#include <inttypes.h>
#include <stdbool.h>

typedef struct {
} IvjContext;

#define IVJ_MAX_BUFFERED_EVENTS 32
typedef struct {
	int32_t BufferedEventNo;
	INPUT BufferedEvents[IVJ_MAX_BUFFERED_EVENTS];
} IvjKeyboard;

typedef struct {
} IvjKeyboardConstructionInfo;
#endif // WITH_WINAPI
#endif // EXTRACK_INJECT_TYPES_H
#endif // WINDOWS
