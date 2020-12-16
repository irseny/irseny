using System;
using System.Runtime.InteropServices;

namespace Irseny.Core.Sensors.VideoCapture
{
	public partial class VideoCaptureBackend {
		// TODO implement
		#if WINDOWS
		const string lib = "Irseny.Native.dll";
		const CallingConvention ccon = CallingConvention.StdCall;
		#elif LINUX
		const string lib = "libirseny_native.so";
		const CallingConvention ccon = CallingConvention.Cdecl;
		#endif
		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsCreateVideoCaptureContext")]
		public static extern IntPtr CreateVideoCaptureContext();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsDestroyVideoCaptureContext")]
		public static extern void DestroyVideoCaptureContext(IntPtr context);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsAllocVideoCaptureSettings")]
		public static extern IntPtr AllocVideoCaptureSettings();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsFreeVideoCaptureSettings")]
		public static extern void FreeVideoCaptureSettings(IntPtr settings);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsCreateVideoCapture")]
		public static extern IntPtr CreateVideoCapture(IntPtr context, IntPtr settings);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsDestroyVideoCapture")]
		public static extern bool DestroyVideoCapture(IntPtr context, IntPtr capture);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsGetVideoCaptureProperty")]
		public static extern int GetVideoCaptureProperty(IntPtr settings, VideoCaptureProperty property);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsSetVideoCaptureProperty")]
		public static extern bool SetVideoCaptureProperty(IntPtr settings, VideoCaptureProperty property, int value);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsGetVideoCapturePropertyAuto")]
		public static extern bool GetVideoCapturePropertyAuto(IntPtr settings, VideoCaptureProperty property);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsSetVideoCapturePropertyAuto")]
		public static extern bool SetVideoCapturePropertyAuto(IntPtr settings, VideoCaptureProperty property);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsGetVideoCaptureSettings")]
		public static extern bool GetVideoCaptureSettings(IntPtr capture, IntPtr settings);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsStartVideoCapture")]
		public static extern bool StartVideoCapture(IntPtr capture, IntPtr frame);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsStopVideoCapture")]
		public static extern bool StopVideoCapture(IntPtr capture);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsBeginVideoCaptureFrameGrab")]
		public static extern bool BeginVideoCaptureFrameGrab(IntPtr capture);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsEndVideoCaptureFrameGrab")]
		public static extern bool EndVideoCaptureFrameGrab(IntPtr capture);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsCreateVideoCaptureFrame")]
		public static extern IntPtr CreateVideoCaptureFrame(IntPtr capture);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsDestroyVideoCaptureFrame")]
		public static extern void DestroyVideoCaptureFrame(IntPtr capture, IntPtr frame);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsGetVideoCaptureFrameProperty")]
		public static extern int GetVideoCaptureFrameProperty(IntPtr frame, VideoFrameProperty property);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsCopyVideoCaptureFrame")]
		public static extern bool CopyVideoCaptureFrame(IntPtr frame, IntPtr buffer, long bufferSize);

		public static VideoCaptureProperty TranslateSensorProperty(SensorProperty property) {
			switch (property) {
			case SensorProperty.FrameWidth: 
				return VideoCaptureProperty.FrameWidth;
			case SensorProperty.FrameHeight:
				return VideoCaptureProperty.FrameHeight;
			case SensorProperty.FrameRate:
				return VideoCaptureProperty.FrameRate;
			case SensorProperty.Brightness:
				return VideoCaptureProperty.Brightness;
			case SensorProperty.Gain:
				return VideoCaptureProperty.Gain;
			case SensorProperty.Contrast:
				throw new ArgumentException();
			case SensorProperty.Exposure:
				return VideoCaptureProperty.Exposure;
			default:
				throw new ArgumentException();
			}
		}
		public static SensorProperty TranslateCaptureProperty(VideoCaptureProperty property) {
			switch (property) {
			case VideoCaptureProperty.FrameWidth:
				return SensorProperty.FrameWidth;
			case VideoCaptureProperty.FrameHeight:
				return SensorProperty.FrameHeight;
			case VideoCaptureProperty.FrameRate:
				return SensorProperty.FrameRate;
			case VideoCaptureProperty.Brightness:
				return SensorProperty.Brightness;
			case VideoCaptureProperty.Gain:
				return SensorProperty.Gain;
			case VideoCaptureProperty.Exposure:
				return SensorProperty.Exposure;
			default:
				throw new ArgumentException();
			}
		}
	}
}

