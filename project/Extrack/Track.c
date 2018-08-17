#include "TrackTypes.h"

#if WITH_TIFC
#if WINDOWS

EXTRACK_EXPORT ArtfContext* artfCreateContext() {	
	ArtfContext* context = (ArtfContext*)malloc(sizeof(ArtfContext));	
	memset(context, 0, sizeof(ArtfContext));
	// get shared file and mutex
	context->File = CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, sizeof(ArtfPacket), "FT_SharedMem"); // null if unsuccessful
	context->Sync = CreateMutexA(NULL, FALSE, "FT_Mutext");	
	if (context->File == NULL || context->Sync == NULL) {
		// cleanup if not successful
		if (context->File != NULL) {
			CloseHandle(context->File);
			context->File = NULL;
		}
		if (context->Sync != NULL) {
			CloseHandle(context->Sync);
			context->Sync = NULL;
		}
	}
	return context;
}
EXTRACK_EXPORT bool artfContextCreated(ArtfContext* context) {
	if (context == NULL) {
		return false;
	}
	return context->File != NULL;
}
EXTRACK_EXPORT void artfDestroyContext(ArtfContext* context) {
	if (artfContextCreated) {
		CloseHandle(context->Sync);
		CloseHandle(context->File);
	}
	free(context);	
}

EXTRACK_EXPORT ArtfDevice* artfOpenDevice(ArtfContext* context, int32_t deviceId) {
	ArtfDevice* device = (ArtfDevice*)malloc(sizeof(ArtfDevice));
	memset(device, 0, sizeof(ArtfDevice));
	if (artfContextCreated(context)) {
		// map shared file to memory
		device->FileMap = (ArtfPacket*)MapViewOfFile(context->File, FILE_MAP_WRITE, 0, 0, sizeof(ArtfPacket)); // null if unusccessful
	}
	return device;
}
EXTRACK_EXPORT bool artfDeviceOpened(ArtfDevice* device) {
	if (device == NULL) {
		return false;
	}
	return device->FileMap != NULL;
}
EXTRACK_EXPORT void artfCloseDevice(ArtfContext* context, ArtfDevice* device) {
	if (artfDeviceOpened(device)) {
		// should be called before the context is destroyed
		UnmapViewOfFile(device->FileMap);
	}
	free(device);
}
EXTRACK_EXPORT ArtfPacket* artfAllocPacket() {
	ArtfPacket* packet = (ArtfPacket*)malloc(sizeof(ArtfPacket));
	memset(packet, 0, sizeof(ArtfPacket));
	return packet;
}
EXTRACK_EXPORT void artfFreePacket(ArtfPacket* packet) {
	free(packet);
}
EXTRACK_EXPORT bool artfSetPacketProperty(ArtfPacket* packet, int32_t property, float value) {
	if (property < 0 || property > 19) {
		return false;
	}
	float* begin = &packet->Yaw; // first property in the struct
	begin[property] = value; // index the struct starting from the first property
	return true;
}
EXTRACK_EXPORT bool artfIncPacketId(ArtfPacket* packet) {
	packet->PacketId += 1; // eventually restarts at 0	
	return true;
}
EXTRACK_EXPORT bool artfSetPacketCamSize(ArtfPacket* packet, uint32_t width, uint32_t height) {
	packet->CameraWidth = width;
	packet->CameraHeight = height;
	return true;
}
EXTRACK_EXPORT bool artfSubmitPacket(ArtfContext* context, ArtfDevice* device, ArtfPacket* packet) {
	if (!artfContextCreated(context) || !artfDeviceOpened(device)) {
		return false;
	}
	if (WaitForSingleObject(context->Sync, 16) == WAIT_OBJECT_0) { // wait for the mutex for some time
		memcpy(device->FileMap, packet, sizeof(ArtfPacket));
		ReleaseMutex(context->Sync);
		return true;
	}
	return false;
}
#endif

#if LINUX
#endif

#endif
