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

#include "InputInjection.h"

#if WITH_WINAPI

WORD ivjKeyCodes[IVJ_KEYBOARD_KEY_NO] = {
	//Q     W     E     R     T    Z      U     I     O     P
	0x51, 0x57, 0x45, 0x52, 0x54, 0x5A, 0x55, 0x49, 0x4F, 0x50
};
void ivjLogError(const char* message);

EXTRACK_EXPORT IvjContext* ivjCreateContext() {
	IvjContext* result = (IvjContext*)malloc(sizeof(IvjContext));
	memset(result, 0, sizeof(IvjContext));
#if WITH_FREETRACK
	result->FreetrackFile = CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 
		0, sizeof(IvjFreetrackPacket), "FT_SharedMem");
	result->FreetrackSync = CreateMutexA(NULL, FALSE, "FT_Mutex");
	if (result->FreetrackFile == NULL) {
		if (result->FreetrackSync != NULL) {
			CloseHandle(result->FreetrackSync);
			result->FreetrackSync = NULL;
		}
		ivjLogError("Failed to create shared freetrack memory");
	}
	if (result->FreetrackSync == NULL) {
		if (result->FreetrackFile != NULL) {
			CloseHandle(result->FreetrackFile);
			result->FreetrackFile = NULL;
		}
		ivjLogError("Failed to create shared freetrack mutex");
	}
#endif // WITH_FREETRACK
	return result;
}
EXTRACK_EXPORT bool ivjDestroyContext(IvjContext* context) {
	if (context == NULL) {
		return true;
	}
#if WITH_FREETRACK
	if (context->FreetrackFile != NULL) {
		CloseHandle(context->FreetrackFile);
	}
	if (context->FreetrackSync != NULL) {
		CloseHandle(context->FreetrackSync);
	}
#endif // WITH_FREETRACK
	free(context);
	return true;
}
EXTRACK_EXPORT IvjKeyboardConstructionInfo* ivjAllocKeyboardConstructionInfo() {
	return (IvjKeyboardConstructionInfo*)1;
}
EXTRACK_EXPORT bool ivjFreeKeyboardConstructionInfo(IvjKeyboardConstructionInfo* info) {
	//free(info);
	return true;
}
EXTRACK_EXPORT IvjKeyboard* ivjConnectKeyboard(IvjContext* context, IvjKeyboardConstructionInfo* info) {
	IvjKeyboard* keyboard = (IvjKeyboard*)malloc(sizeof(IvjKeyboard));
	keyboard->BufferedEventNo = 0;
	return keyboard;
}
EXTRACK_EXPORT bool ivjDisconnectKeyboard(IvjContext* context, IvjKeyboard* keyboard) {
	free(keyboard);
	return true;
}
EXTRACK_EXPORT bool ivjSetKeyboardKey(IvjKeyboard* keyboard, int32_t keyIndex, bool pressed) {
	if (keyIndex < 0 || keyIndex >= IVJ_KEYBOARD_KEY_NO) {
		return false;
	}
	if (keyboard->BufferedEventNo >= IVJ_MAX_BUFFERED_EVENT_NO) {
		ivjSendKeyboard(keyboard);
	}
	if (keyboard->BufferedEventNo >= IVJ_MAX_BUFFERED_EVENT_NO) {
		return false;
	}
	INPUT* event = &keyboard->BufferedEvents[keyboard->BufferedEventNo];
	event->type = INPUT_KEYBOARD;
	event->ki.wVk = ivjKeyCodes[keyIndex];
	event->ki.wScan = 0;
	if (pressed) {
		event->ki.dwFlags = 0; // keydown
	} else {
		event->ki.dwFlags = KEYEVENTF_KEYUP;
	}
	event->ki.time = 0; // provided by system
	event->ki.dwExtraInfo = GetMessageExtraInfo();
	keyboard->BufferedEventNo += 1;
	return true;
}
EXTRACK_EXPORT bool ivjSendKeyboard(IvjKeyboard* keyboard) {
	bool result = true;
	if (keyboard->BufferedEventNo < 1) {
		return true;
	}
	uint32_t errorCode = SendInput(keyboard->BufferedEventNo, keyboard->BufferedEvents, sizeof(INPUT));
	if (errorCode != keyboard->BufferedEventNo) {
		ivjLogError("Failed to inject keyboard input");
		if (errorCode == 0) {
			ivjLogError("I.njection blocked by another thread");
		}
		ivjLogError("Injection might have been blocked by UIPI");
		result = false;
	}
	keyboard->BufferedEventNo = 0;
	return  result;
}
#if WITH_FREETRACK
EXTRACK_EXPORT IvjFreetrackConstructionInfo* ivjAllocFreetrackConstructionInfo() {
	return (IvjFreetrackConstructionInfo*)1;
}
EXTRACK_EXPORT bool ivjFreeFreetrackConstructionInfo(IvjFreetrackConstructionInfo* info) {
	return true;
}
EXTRACK_EXPORT IvjFreetrackInterface* ivjConnectFreetrackInterface(IvjContext* context, IvjFreetrackConstructionInfo* info) {
	if (context->FreetrackFile == NULL || context->FreetrackSync == NULL) {
		return NULL;
	}
	// create a file map
	IvjFreetrackPacket* map = (IvjFreetrackPacket*)MapViewOfFile(context->FreetrackFile, FILE_MAP_WRITE, 
		0, 0, sizeof(IvjFreetrackPacket));
	if (map == NULL) {
		return NULL;
	}
	// initialize critical members
	IvjFreetrackInterface* result = (IvjFreetrackInterface*)malloc(sizeof(IvjFreetrackInterface));
	memset(result, 0, sizeof(IvjFreetrackInterface));
	result->Sync = context->FreetrackSync;
	result->Map = map;
	result->Packet.PacketID = 0;
	result->Packet.Resolution[0] = 640;
	result->Packet.Resolution[1] = 480;
	// the stored axis and point values stay 0
	return result;
}
EXTRACK_EXPORT bool ivjDisconnectFreetrackInterface(IvjContext* context, IvjFreetrackInterface* freetrack) {
	if (freetrack->Map != NULL) {
		UnmapViewOfFile(freetrack->Map);
	}
	free(freetrack);
	return true;
}
EXTRACK_EXPORT bool ivjSetFreetrackResolution(IvjFreetrackInterface* freetrack, int width, int height) {
	if (width < 0 || height < 0) {
		return false;
	}
	freetrack->Packet.Resolution[0] = width;
	freetrack->Packet.Resolution[1] = height;
	return true;
}
EXTRACK_EXPORT bool ivjSetFreetrackPoint(IvjFreetrackInterface* freetrack, int pointIndex, int x, int y) {
	if (pointIndex < 0 || pointIndex >= IVJ_FREETRACK_POINT_NO) {
		return false;
	}
	freetrack->Packet.Points[pointIndex][0] = (float)x;
	freetrack->Packet.Points[pointIndex][1] = (float)y;
	return true;
}
EXTRACK_EXPORT bool ivjSetFreetrackAxis(IvjFreetrackInterface* freetrack, int axisIndex, float smooth, float raw) {
	if (axisIndex < 0 || axisIndex >= IVJ_FREETRACK_AXIS_NO) {
		return false;
	}
	freetrack->Packet.SmoothAxes[axisIndex] = smooth;
	freetrack->Packet.RawAxes[axisIndex] = raw;
	return true;
}
EXTRACK_EXPORT bool ivjSendFreetrackInterface(IvjFreetrackInterface* freetrack) {
	if (WaitForSingleObject(freetrack->Sync, 16) != WAIT_OBJECT_0) {
		ivjLogError("Failed to lock shared freetrack mutex");
		// timeout or error
		return false;
	}
	freetrack->Packet.PacketID = freetrack->Packet.PacketID + 1;
	//int id = freetrack->Packet.PacketID += 1;
	//if (id < 0) {
		// TODO: fix undefined behaviour instead
		//freetrack->Packet.PacketID = 0;
	//}
	memcpy(freetrack->Map, &(freetrack->Packet), sizeof(IvjFreetrackPacket));
	ReleaseMutex(freetrack->Sync);
	return true;
}
#endif // WITH_FREETRACK
#if WITH_VJOY
EXTRACK_EXPORT IvjJoystickConstructionInfo* ivjAllocJoystickConstructionInfo() {
	IvjJoystickConstructionInfo* result = (IvjJoystickConstructionInfo*)malloc(sizeof(IvjJoystickConstructionInfo));
	result->JoystickIndex = 0;
	return result;
}
EXTRACK_EXPORT bool ivjSetJoystickIndex(IvjJoystickConstructionInfo* info, int joyIndex) {
	if (joyIndex < 0) {
		return false;
	}
	info->JoystickIndex = joyIndex;
	return true;
}
EXTRACK_EXPORT bool ivjFreeJoystickConstructionInfo(IvjJoystickConstructionInfo* info) {
	free(info);
	return true;
}
#endif // WITH_VJOY
void ivjLogError(const char* message) {
	printf("Ivj Error: %s\n", message);
}
#endif // WITH_WINAPI
