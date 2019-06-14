#if LINUX
#ifndef EXTRACK_INJECT_TYPES_H
#define EXTRACK_INJECT_TYPES_H

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

typedef struct {
	int32_t FileHandle;
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

#endif // WITH_UINPUT
#endif // EXTRACK_INJECT_TYPES_H
#endif // LINUX

