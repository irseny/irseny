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

#if WITH_WINAPI

#include <inttypes.h>
#include <stdbool.h>
#include <windows.h>
#include <stdio.h>

#if WITH_FREETRACK

#include "FreetrackTest.win.h"

void cleanup(HANDLE file, HANDLE sync, void* map) {
	if (map != NULL) {
		UnmapViewOfFile(map);
	}
	if (file != NULL) {
		CloseHandle(file);
	}
	if (sync != NULL) {
		CloseHandle(sync);
	}
}
int main(int argc, char** args) {
	printf("Setting up freetrack ...");
	HANDLE freetrackFile =  CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 
		0, sizeof(IvjFreetrackPacket), "FT_SharedMem");
	HANDLE freetrackSync = CreateMutexA(NULL, FALSE, "FT_Mutex");
	if (freetrackFile == NULL) {
		printf("Failed to create shared memory\n");
		cleanup(freetrackFile, freetrackSync, NULL);
		return -1;
	}
	if (freetrackSync == NULL) {
		printf("Failed to create shared mutex");
		cleanup(freetrackFile, freetrackSync, NULL);
		return -1;
	}
	IvjFreetrackPacket* freetrackMap = (IvjFreetrackPacket*)MapViewOfFile(freetrackFile, FILE_MAP_WRITE, 0, 0, sizeof(IvjFreetrackPacket));
	if (freetrackMap == NULL) {
		printf("Failed to map file\n");
		cleanup(freetrackFile, freetrackSync, freetrackMap);
		return -1;
	}
	printf("Setup finished. Starting to supply ...\n");

	IvjFreetrackPacket freetrackPacket;
	memset(&freetrackPacket, 0, sizeof(IvjFreetrackPacket));
	int lastPacketId = 0;
	int moveDirection = 1;
	float moveSpeed = 0.001f;
	bool running = true;
	while (running) {
		freetrackPacket.PacketID += 1;
		if (freetrackPacket.PacketID < 0) {
			freetrackPacket.PacketID = 0;
		}
		freetrackPacket.Yaw += moveDirection*moveSpeed;
		if (freetrackPacket.Yaw < -2) {
			freetrackPacket.Yaw = -2;
			moveDirection = 1;
		}
		if (freetrackPacket.Yaw > 2) {
			freetrackPacket.Yaw = 2;
			moveDirection = -1;
		}
		
		if (WaitForSingleObject(freetrackSync, 16) == WAIT_OBJECT_0) {
			memcpy(freetrackMap, &freetrackPacket, sizeof(IvjFreetrackPacket));
			ReleaseMutex(freetrackSync);
		} else {
			printf("Sync blocked\n");
		}
		if (GetKeyState(0x51) & 0x8000) {
			running = false;
		}
		Sleep(16);
	}
	cleanup(freetrackFile, freetrackSync, freetrackMap);
	printf("Terminated by user input\n");
	return 0;
}
#endif // WITH_FREETRACK
#endif // WITH_WINAPI
