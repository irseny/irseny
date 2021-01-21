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

using System;
using System.Runtime.InteropServices;
namespace Irseny.Core.Inco {
	public static class Artf {
		public enum PacketProperty {
			Yaw = 0,
			Pitch,
			Roll,
			PosX,
			PosY,
			PosZ,
			RawYaw,
			RawPitch,
			RawRoll,
			RawPosX,
			RawPosY,
			RawPosZ,
			Point1X,
			Point1Y,
			Point2X,
			Point2Y,
			Point3X,
			Point3Y,
			Point4X,
			Point4Y
		}
		const string lib = "libExtrack.dll";
		const CallingConvention cdecl = CallingConvention.Cdecl;
		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfCreateContext")]
		public static extern IntPtr CreateContext();

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfDestroyContext")]
		public static extern void DestroyContext(IntPtr context);

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfOpenDevice")]
		public static extern IntPtr OpenDevice(IntPtr context, int deviceId);

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfCloseDevice")]
		public static extern void CloseDevice(IntPtr context, IntPtr device);

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfAllocPacket")]
		public static extern IntPtr AllocatePacket();

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfFreePacket")]
		public static extern void FreePacket(IntPtr packet);

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfIncPacketId")]
		public static extern bool IncreasePacketId(IntPtr packet);

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfSetPacketCamSize")]
		public static extern bool SetPacketCameraSize(IntPtr packet, uint width, uint height);

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfSetPacketProperty")]
		public static extern bool SetPacketProperty(IntPtr packet, PacketProperty property, float value);

		[DllImport(lib, CallingConvention = cdecl, EntryPoint = "artfSubmitPacket")]
		public static extern bool SubmitPacket(IntPtr context, IntPtr device, IntPtr packet);
	}
}
