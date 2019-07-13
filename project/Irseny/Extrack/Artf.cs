using System;
using System.Runtime.InteropServices;
namespace Irseny.Extrack {
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
