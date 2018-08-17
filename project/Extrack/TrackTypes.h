#ifndef EXTRACK_TRACK_TYPES_H
#define EXTRACK_TRACK_TYPES_H

#if WITH_TIFC
#if WINDOWS
#define EXTRACK_EXPORT __cdecl

#include <stddef.h>
#include <inttypes.h>
#include <stdbool.h>
#include <windows.h>

typedef struct {
	uint32_t PacketId;
	uint32_t CameraWidth;
	uint32_t CameraHeight;
	float Yaw;
	float Pitch;
	float Roll;
	float PosX;
	float PosY;
	float PosZ;
	
	float RawYaw;
	float RawPitch;
	float RawRoll;
	float RawPosX;
	float RawPosY;
	float RawPosZ;
	
	float Point1X;
	float Point1Y;
	float Point2X;
	float Point2Y;
	float Point3X;
	float Point3Y;
	float Point4X;
	float Point4Y;
} ArtfPacket;
	
typedef struct {
	HANDLE File;
	HANDLE Sync;	
} ArtfContext;

typedef struct {
	int32_t DeviceId;
	ArtfPacket* FileMap;
} ArtfDevice;
#endif // WINDOWS




#if LINUX
#define EXTRACK_EXPORT
#endif // LINUX

#endif // WITH_TIFC
#endif // EXTRACK_TRACK_TYPES_H