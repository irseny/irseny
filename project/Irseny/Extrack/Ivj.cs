using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Irseny.Extrack {
	public static class Ivj {
		static Dictionary<string, int> keyboardKeys;

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
		public static int GetKeyboardKeyIndex(object keyHandle) {
			string keyName = keyHandle.ToString();
			int result;
			if (!keyboardKeys.TryGetValue(keyName, out result)) {
				return -1;
			}
			return result;
		}
#if WINDOWS
		const string lib = "libExtrack.dll";
		const CallingConvention ccon = CallingConvention.StdCall;
#elif LINUX
		const string lib = "libExtrack.dll";
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
		public static extern bool SetKeyboardKey(IntPtr keyboard, int keyIndex, float state);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "ivjSendKeyboard")]
		public static extern bool SendKeyboard(IntPtr keyboard);
	}
}

