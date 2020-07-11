#if WINDOWS
#include <inttypes.h>
#include <stdbool.h>
#include <windows.h>
#include <stdio.h>

#if WITH_FREETRACK

//#include "Inject.h"

/*int main(int argc, char** args) {
	IvjContext* context = ivjCreateContext();
	IvjFreetrackConstructionInfo* info = ivjAllocFreetrackConstructionInfo();
	IvjFreetrackInterface* freetrack = ivjConnectFreetrackInterface(context, info);
	ivjFreeFreetrackConstructionInfo(info);
	
	float yaw = 0.0f;
	float speed = 0.01f;
	int direction = 1;
	bool running = true;
	while (running) {
		ivjSetFreetrackAxis(freetrack, 0, yaw);
		yaw += speed*direction;
		if (yaw < -2) {
			yaw = -2;
			direction = 1;
		}
		if (yaw > 2) {
			yaw = 2;
			direction = -1;
		}
		//ivjSendFreetrackInterface(freetrack);
		{
			if (WaitForSingleObject(freetrack->Sync, 16) != WAIT_OBJECT_0) {
				ivjLogError("Failed to lock shared freetrack mutex");
				// timeout or error
			}
			freetrack->Packet.PacketID = freetrack->Packet.PacketID + 1;
			memcpy(&(freetrack->Map), &(freetrack->Packet), sizeof(IvjFreetrackPacket));
			ReleaseMutex(freetrack->Sync);
		}
		//ivjSendFreetrackInterface(freetrack);
		if (GetKeyState(0x51) & 0x8000) {
			running = false;
		}
		
		Sleep(16);
	}
	ivjDisconnectFreetrackInterface(context, freetrack);
	ivjDestroyContext(context);
	return 0;
}*/

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
#endif // WINDOWS
