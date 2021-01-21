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
#include <stdio.h>
#include <stdbool.h>
#include "windows.h"
#include "TestWin.h"

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
	HANDLE console = GetStdHandle(STD_OUTPUT_HANDLE);
	if (console == NULL || console == INVALID_HANDLE_VALUE) {
		return -1;
	}
	{
		const char* message = "Setting up freetrack ...\n";
		WriteConsole(console, message, strlen(message), NULL, NULL);
	}
	HANDLE freetrackFile = OpenFileMappingA(FILE_MAP_READ, FALSE, "FT_SharedMem");
	HANDLE freetrackSync = CreateMutexA(NULL, FALSE, "FT_Mutex");
	if (freetrackFile == NULL) {
		const char* message = "Shared memory not available\n";
		WriteConsole(console, message, strlen(message), NULL, NULL);
		//printf("Shared memory not available");
		cleanup(freetrackFile, freetrackSync, NULL);
		return -1;
	}
	if (freetrackSync == NULL) {
		printf("Shared mutex not available");
		cleanup(freetrackFile, freetrackSync, NULL);
		return -1;
	}
	IvjFreetrackPacket* freetrackMap = (IvjFreetrackPacket*)MapViewOfFile(freetrackFile, FILE_MAP_READ, 0, 0, sizeof(IvjFreetrackPacket));
	if (freetrackMap == NULL) {
		printf("Failed to map file\n");
		cleanup(freetrackFile, freetrackSync, freetrackMap);
		return -1;
	}
	{
		const char* message = "Setup finished. Starting to listen ...\n";
		WriteConsole(console, message, strlen(message), NULL, NULL);
	}
	IvjFreetrackPacket freetrackPacket;
	int lastPacketID = -1;
	bool running = true;
	while (running) {
		if (WaitForSingleObject(freetrackSync, 16) == WAIT_OBJECT_0) {
			memcpy(&freetrackPacket, freetrackMap, sizeof(IvjFreetrackPacket));
			ReleaseMutex(freetrackSync);
		} else {
			printf("sync blocked\n");
		}
		if (freetrackPacket.PacketID != lastPacketID) {
			lastPacketID = freetrackPacket.PacketID;
			COORD origin = {
				0, 2
			};
			//SetConsoleCursorPosition(console, origin);
			char message[80];
			snprintf(message, 80, "Packet ID: %d\n", freetrackPacket.PacketID);
			WriteConsole(console, message, strlen(message), NULL, NULL);
			snprintf(message, 80, "Yaw: %f\n", freetrackPacket.Yaw);
			WriteConsole(console, message, strlen(message), NULL, NULL);
			snprintf(message, 80, "Pitch: %f\n", freetrackPacket.Pitch);
			WriteConsole(console, message, strlen(message), NULL, NULL);
		}
		if (GetKeyState(0x51) & 0x8000) {
			running = false;
		}
		Sleep(16);
	}
	cleanup(freetrackFile, freetrackSync, freetrackMap);
	{
		const char* message = "Terminated by user input\n";
		WriteConsole(console, message, strlen(message), NULL, NULL);
	}
	return 0;
}
#else
#error "Require WITH_WINAPI
#endif // WITH_WINAPI

