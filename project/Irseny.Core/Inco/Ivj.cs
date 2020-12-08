using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Irseny.Core.Inco {
	public static class Ivj {
		static Dictionary<string, int> keyboardKeys;
		static Dictionary<string, int> freetrackAxes;

		static Ivj() {
			keyboardKeys = new Dictionary<string, int>(256);
			keyboardKeys.Add("Q", 0);
			keyboardKeys.Add("W", 2);
			keyboardKeys.Add("E", 3);
			keyboardKeys.Add("R", 4);
			keyboardKeys.Add("T", 5);
			keyboardKeys.Add("Z", 6);
			keyboardKeys.Add("U", 7);
			keyboardKeys.Add("I", 8);
			keyboardKeys.Add("O", 9);
			keyboardKeys.Add("P", 10);
			freetrackAxes = new Dictionary<string, int>(16);
			freetrackAxes.Add("Yaw", 0);
			freetrackAxes.Add("Pitch", 1);
			freetrackAxes.Add("Roll", 2);
			freetrackAxes.Add("X", 3);
			freetrackAxes.Add("Y", 4);
			freetrackAxes.Add("Z", 5);
		}
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
			X = 8,
			Y,
			Wheel,
			XMin,
			XMax,
			YMin,
			YMax,
			WheelMin,
			WheelMax
		}
		public static int GetKeyboardKeyIndex(object keyHandle) {
			string keyName = keyHandle.ToString();
			int result;
			if (!keyboardKeys.TryGetValue(keyName, out result)) {
				return -1;
			}
			return result;
		}
		public static int GetFreetrackAxisIndex(object axisHandle) {
			string axisName = axisHandle.ToString();
			int result;
			if (!freetrackAxes.TryGetValue(axisName, out result)) {
				return -1;
			}
			return result;
		}
#if WINDOWS
		const string lib = "Irseny.Native.dll";
		const CallingConvention ccon = CallingConvention.StdCall;
#elif LINUX
		const string lib = "libirseny_native.so";
		const CallingConvention ccon = CallingConvention.Cdecl;
#endif
		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjCreateContext")]
		public static extern IntPtr CreateContext();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjDestroyContext")]
		public static extern bool DestroyContext(IntPtr context);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjAllocKeyboardConstructionInfo")]
		public static extern IntPtr AllocKeyboardConstructionInfo();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjFreeKeyboardConstructionInfo")]
		public static extern bool FreeKeyboardConstructionInfo(IntPtr constructionInfo);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjConnectKeyboard")]
		public static extern IntPtr ConnectKeyboard(IntPtr context, IntPtr constructionInfo);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjDisconnectKeyboard")]
		public static extern bool DisconnectKeyboard(IntPtr context, IntPtr keyboard);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjSetKeyboardKey")]
		public static extern bool SetKeyboardKey(IntPtr keyboard, int keyIndex, bool state);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjSendKeyboard")]
		public static extern bool SendKeyboard(IntPtr keyboard);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjAllocFreetrackConstructionInfo")]
		public static extern IntPtr AllocFreetrackConstructionInfo();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjFreeFreetrackConstructionInfo")]
		public static extern bool FreeFreetrackConstructionInfo(IntPtr constructionInfo);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjConnectFreetrackInterface")]
		public static extern IntPtr ConnectFreetrackInterface(IntPtr context, IntPtr constructionInfo);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjDisconnectFreetrackInterface")]
		public static extern bool DisconnectFreetrackInterface(IntPtr context, IntPtr freetrack);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjSetFreetrackAxis")]
		public static extern bool SetFreetrackAxis(IntPtr freetrack, int axisIndex, float smooth, float raw);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjSetFreetrackResolution")]
		public static extern bool SetFreetrackResolution(IntPtr freetrack, int width, int height);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjSetFreetrackPoint")]
		public static extern bool SetFreetrackPoint(IntPtr freetrack, int pointIndex, int x, int y);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjSendFreetrackInterface")]
		public static extern bool SendFreetrackInterface(IntPtr freetrack);
	}
}

