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
#define IVJ_MAX_BUFFERED_EVENT_NO 32

typedef int32_t IvjContext;
//typedef struct {
//	int dummy;
//} IvjContext;

typedef struct {
	int32_t BufferedEventNo;
	INPUT BufferedEvents[IVJ_MAX_BUFFERED_EVENT_NO];
} IvjKeyboard;

typedef int32_t IvjKeyboardConstructionInfo;
//typedef struct {
//	int dummy;
//} IvjKeyboardConstructionInfo;
#endif // WITH_WINAPI
#endif // EXTRACK_INJECT_TYPES_H
#endif // WINDOWS
