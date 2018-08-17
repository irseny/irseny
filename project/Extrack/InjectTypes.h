

#if WITH_UINPUT
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
	char* Name[UINPUT_MAX_NAME_SIZE];	
	int32_t Vendor;
	int32_t Product;
} IvjKeyboardConstructionInfo;

#endif

#if WINDOWS
#define EXTRACK_EXPORT __cdecl 

#include <windows.h>
#endif
