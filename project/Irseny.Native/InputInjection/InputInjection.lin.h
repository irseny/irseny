// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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

