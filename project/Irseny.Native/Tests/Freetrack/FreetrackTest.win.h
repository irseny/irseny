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

