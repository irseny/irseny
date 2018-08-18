using System;
using System.Runtime.InteropServices;

namespace Irseny.Extrack {
	public static class Ivj {
		public enum JoystickCapability {
			Button1 = 0,
			Axis1 = 32,
			Axis1Min = 40,
			Axis1Max = 48
		}

		public enum MouseCapability {
			Left = 0,
			Right = 1,
			Middle = 2,
			Button4 = 3,
			AxisX = 8,
			AxisY,
			Wheel,
			XMin,
			XMax,
			YMin,
			YMax,
			WheelMin,
			WheelMax
		}

		const string lib = "libExtrack.dll";
		const CallingConvention ccon = CallingConvention.Cdecl;
		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjCreateContext")]
		public static extern IntPtr CreateContext();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjDestroyContext")]
		public static extern bool DestroyContext();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjAllocKeyboardConstructionInfo")]
		public static extern IntPtr AllocKeyboardConstructionInfo();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjFreeKeyboardConstructionInfo")]
		public static extern bool FreeKeyboardConstructionInfo();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjOpenKeybaord")]
		public static extern IntPtr OpenKeyboard(IntPtr context, IntPtr constructionInfo);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjCloseKeyboard")]
		public static extern bool CloseKeyboard(IntPtr context, IntPtr keyboard);
	}
}

