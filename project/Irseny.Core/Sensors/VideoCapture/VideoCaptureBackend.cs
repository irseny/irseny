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

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsAllocVideoCaptureConstructionInfo")]
		public static extern IntPtr AllocVideoCaptureConstructionInfo();

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsFreeVideoCaptureConstructionInfo")]
		public static extern void FreeVideoCaptureConstructionInfo(IntPtr info);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsCreateVideoCapture")]
		public static extern IntPtr CreateVideoCapture(IntPtr context, IntPtr info);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsDestroyVideoCapture")]
		public static extern bool DestroyVideoCapture(IntPtr context, IntPtr capture);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsGetVideoCaptureProperty")]
		public static extern int GetVideoCaptureProperty(IntPtr info, SensorProperty property);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsSetVideoCaptureProperty")]
		public static extern bool SetVideoCaptureProperty(IntPtr info, SensorProperty property, int value);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsGetVideoCapturePropertyAuto")]
		public static extern bool GetVideoCapturePropertyAuto(IntPtr info, int property);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsSetVideoCapturePropertyAuto")]
		public static extern bool SetVideoCapturePropertyAuto(IntPtr info, int property);

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
		public static extern int GetVideoCaptureFrameProperty(IntPtr frame, int property);

		[DllImport(lib, CallingConvention = ccon, EntryPoint = "irsCopyVideoCaptureFrame")]
		public static extern bool CopyVideoCaptureFrame(IntPtr capture, IntPtr buffer, long bufferSize);

		public static int TranslateProperty(SensorProperty property) {
			// values originate from /usr/include/opencv2/highgui/highgui_c.h
			/*
			CV_CAP_PROP_POS_MSEC       =0,
		    CV_CAP_PROP_POS_FRAMES     =1,
		    CV_CAP_PROP_POS_AVI_RATIO  =2,
		    CV_CAP_PROP_FRAME_WIDTH    =3,
		    CV_CAP_PROP_FRAME_HEIGHT   =4,
		    CV_CAP_PROP_FPS            =5,
		    CV_CAP_PROP_FOURCC         =6,
		    CV_CAP_PROP_FRAME_COUNT    =7,
		    CV_CAP_PROP_FORMAT         =8,
		    CV_CAP_PROP_MODE           =9,
		    CV_CAP_PROP_BRIGHTNESS    =10,
		    CV_CAP_PROP_CONTRAST      =11,
		    CV_CAP_PROP_SATURATION    =12,
		    CV_CAP_PROP_HUE           =13,
		    CV_CAP_PROP_GAIN          =14,
		    CV_CAP_PROP_EXPOSURE      =15,
		    CV_CAP_PROP_CONVERT_RGB   =16,
		    CV_CAP_PROP_WHITE_BALANCE_BLUE_U =17,
		    CV_CAP_PROP_RECTIFICATION =18,
		    CV_CAP_PROP_MONOCROME     =19,
		    CV_CAP_PROP_SHARPNESS     =20,
		    CV_CAP_PROP_AUTO_EXPOSURE =21, // exposure control done by camera,
		                                   // user can adjust refernce level
		                                   // using this feature
		    CV_CAP_PROP_GAMMA         =22,
		    CV_CAP_PROP_TEMPERATURE   =23,
		    CV_CAP_PROP_TRIGGER       =24,
		    CV_CAP_PROP_TRIGGER_DELAY =25,
		    CV_CAP_PROP_WHITE_BALANCE_RED_V =26,
		    CV_CAP_PROP_ZOOM          =27,
		    CV_CAP_PROP_FOCUS         =28,
		    CV_CAP_PROP_GUID          =29,
		    CV_CAP_PROP_ISO_SPEED     =30,
		    CV_CAP_PROP_MAX_DC1394    =31,
		    CV_CAP_PROP_BACKLIGHT     =32,
		    CV_CAP_PROP_PAN           =33,
		    CV_CAP_PROP_TILT          =34,
		    CV_CAP_PROP_ROLL          =35,
		    CV_CAP_PROP_IRIS          =36,
		    CV_CAP_PROP_SETTINGS      =37,

		    */
			switch (property) {
			case SensorProperty.FrameWidth: 
				return 3;
			case SensorProperty.FrameHeight:
				return 4;
			case SensorProperty.FrameRate:
				return 5;
			case SensorProperty.Brightness:
				return 10;
			case SensorProperty.Gain:
				return 14;
			case SensorProperty.Contrast:
				return 11;
			case SensorProperty.Exposure:
				return 15;
			
			default:
				return -1;
			}
		}
		public static int TranslateProperty(VideoFrameProperty property) {
			switch (property) {
			case VideoFrameProperty.Width:
				return 0x0;
			case VideoFrameProperty.Height:
				return 0x1;
			case VideoFrameProperty.Stride:
				return 0x2;
			case VideoFrameProperty.PixelFormat:
				return 0x3;
			default:
				return -1;
			}
		}
		public static int TranslatePixelFormat(VideoFramePixelFormat format) {
			switch (format) {
			case VideoFramePixelFormat.Gray8:
				return 0x8;
			case VideoFramePixelFormat.Gray16:
				return 0x16;
			case VideoFramePixelFormat.RGB24:
				return 0x24;
			case VideoFramePixelFormat.ARGB32:
				return 0x32;
			default:
				return -1;
			}
		}
		public static VideoFramePixelFormat TranslatePixelFormat(int format) {
			switch (format) {
			case 0x8:
				return VideoFramePixelFormat.Gray8;
			case 0x16:
				return VideoFramePixelFormat.Gray16;
			case 0x24:
				return VideoFramePixelFormat.RGB24;
			case 0x32:
				return VideoFramePixelFormat.ARGB32;
			default:
				throw new ArgumentException("format");
			}
		}
	}
}

