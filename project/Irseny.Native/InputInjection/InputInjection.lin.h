
#ifndef IRSENY_INPUT_INJECTION_TYPES_H
#define IRSENY_INPUT_INJECTION_TYPES_H

#if WITH_UINPUT
#include <stdlib.h>
#include <stdbool.h>
#include <string.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>
#include <linux/input.h>
#include <linux/uinput.h>

#define IVJ_KEYBOARD_KEY_NO 10
#define IVJ_KEYBOARD_MAX_BUFFERED_EVENT_NO 32
#define IVJ_CONTEXT_MAX_PATH_LENGTH 128

typedef struct {
	char FilePath[IVJ_CONTEXT_MAX_PATH_LENGTH];
} IvjContext;

typedef struct {
	IvjContext* Context;
	int32_t FileHandle;
	struct input_event BufferedEvents[IVJ_KEYBOARD_MAX_BUFFERED_EVENT_NO];
	int32_t BufferedEventNo;
} IvjKeyboard;

//typedef int IvjKeyboardKey;
typedef struct {
	char Name[UINPUT_MAX_NAME_SIZE];
	int32_t Vendor;
	int32_t Product;
} IvjKeyboardConstructionInfo;
#else
#error "Require WITH_UINPUT"
#endif // WITH_UINPUT

#if WITH_FREETRACK
typedef int32_t IvjFreetrackConstructionInfo;

typedef struct {
	int32_t FileHandle;
	void* Map;
} IvjFreetrackInterface;
#else
#error "Require WITH_FREETRACK"
#endif // WITH_FREETRACK

#endif // IRSENY_INPUT_INJECTION_TYPES_H

