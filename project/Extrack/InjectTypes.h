#ifndef EXTRACK_INJECT_TYPES_H
#define EXTRACK_INJECT_TYPES_H

#if WITH_UINPUT
#if LINUX
#define EXTRACK_EXPORT

#include <stdlib.h>
#include <stdbool.h>
#include <string.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <linux/input.h>
#include <linux/uinput.h>

typedef struct {
	int32_t FileHandle;
	char* FilePath;
} IvjContext;

typedef struct {
	int32_t FileHandle;
	char* FilePath;
} IvjKeyboard;

typedef struct {
	char Name[UINPUT_MAX_NAME_SIZE];	
	int32_t Vendor;
	int32_t Product;
} IvjKeyboardConstructionInfo;
#endif // LINUX
#endif // WITH_UINPUT



#if WINDOWS
#define EXTRACK_EXPORT __cdecl 

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
#endif // WINDOWS
#endif // EXTRACK_INJECT_TYPES_H