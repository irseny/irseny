
#include <windows.h>
#include <inttypes.h>

#if WITH_FREETRACK

typedef struct {
	int32_t PacketID;
	int32_t CameraWidth;
	int32_t CameraHeight;
	
	float Yaw;
	float Pitch;
	float Roll;
	float X;
	float Y;
	float Z;
	
	float RawYaw;
	float RawPitch;
	float RawRoll;
	float RawX;
	float RawY;
	float RawZ;
	
	int Point1X;
	int Point1Y;
	int Point2X;
	int Point2Y;
	int Point3X;
	int Point3Y;
	int Point4X;
	int Point4Y;
} IvjFreetrackPacket;
#else
#error "Require WITH_FREETRACK"
#endif // WITH_FREETRACK

