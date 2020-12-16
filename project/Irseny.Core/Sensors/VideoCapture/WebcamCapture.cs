using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceModel.Dispatcher;
using Irseny.Core.Log;
using Irseny.Core.Util;
using Irseny.Core.Sensors;
using System.Runtime.InteropServices;


namespace Irseny.Core.Sensors.VideoCapture {
	/// <summary>
	/// Sensor frontend for capturing video data from webcams.
	/// </summary>
	public class WebcamCapture : ISensorBase {
		static IntPtr captureContext;

		IntPtr backendSettings;
		IntPtr videoFrame;
		IntPtr videoCapture;
		readonly object captureSync;

		SensorSettings settings;
		SharedRefCleaner imageCleaner;


		static WebcamCapture() {
			captureContext = VideoCaptureBackend.CreateVideoCaptureContext();
		}

		private WebcamCapture() {
			backendSettings = IntPtr.Zero;
			videoFrame = IntPtr.Zero;
			videoCapture = IntPtr.Zero;

			captureSync = new object();
			imageCleaner = new SharedRefCleaner(32);
		}
		public WebcamCapture(SensorSettings settings) : this() {
			this.settings = new SensorSettings(settings);
		}

		public bool Capturing {
			get { 
				lock (captureSync) {
					return videoFrame != IntPtr.Zero; 
				}
			}
		}
		public SensorType SensorType {
			get { return SensorType.Webcam; }
		}
		public int IntervalPrediction {
			// TODO implement prediction logic
			get { return -1; }
		}
		public SensorSettings GetSettings() {
			lock (captureSync) {
				return new SensorSettings(settings);
			}
		}
		public bool ApplySettings(SensorSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			lock (captureSync) {
				// trivial case if not started
				if (!Capturing) {
					this.settings = new SensorSettings(settings);
					return true;
				}
				// otherwise we have to stop the capture
				// provide new settings
				// and restart the capture
				if (!Stop()) {
					return false;
				}
				this.settings = new SensorSettings(settings);
				if (!Start()) {
					return false;
				}
			}
			return true;
		}
		public bool Start() {
			lock (captureSync) {
				if (Capturing) {
					return true;
				}
				if (captureContext == IntPtr.Zero) {
					return false;
				}
				// create and start the capture process
				bool started = false;
				do {
					backendSettings = VideoCaptureBackend.AllocVideoCaptureSettings();

					if (backendSettings == IntPtr.Zero) {
						break;
					}
					// TODO apply settings
					VideoCaptureBackend.SetVideoCaptureProperty(backendSettings, 
						VideoCaptureBackend.TranslateSensorProperty(SensorProperty.Exposure), 0);

					videoCapture = VideoCaptureBackend.CreateVideoCapture(captureContext, backendSettings);
					if (videoCapture == IntPtr.Zero) {
						break;
					}
					videoFrame = VideoCaptureBackend.CreateVideoCaptureFrame(videoCapture);
					if (videoFrame == IntPtr.Zero) {
						break;
					}
					if (!VideoCaptureBackend.StartVideoCapture(videoCapture, videoFrame)) {
						break;
					}
					started = true;
				} while (false);
				// clean up in the correct order if starting was unsuccessful
				if (!started) {
					if (videoFrame != IntPtr.Zero) {
						VideoCaptureBackend.DestroyVideoCaptureFrame(videoCapture, videoFrame);
						videoFrame = IntPtr.Zero;
					}
					if (videoCapture != IntPtr.Zero) {
						VideoCaptureBackend.DestroyVideoCapture(captureContext, videoCapture);
						videoFrame = IntPtr.Zero;
					}
					if (backendSettings != IntPtr.Zero) {
						VideoCaptureBackend.FreeVideoCaptureSettings(backendSettings);
						backendSettings = IntPtr.Zero;
					}
					return false;
				}
				// finally retrieve updated settings from the backend
				if (VideoCaptureBackend.GetVideoCaptureSettings(videoCapture, backendSettings)) {
					// TODO update local settings
					int width = VideoCaptureBackend.GetVideoCaptureProperty(backendSettings, VideoCaptureProperty.FrameWidth);
					int height = VideoCaptureBackend.GetVideoCaptureProperty(backendSettings, VideoCaptureProperty.FrameHeight);
					int rate = VideoCaptureBackend.GetVideoCaptureProperty(backendSettings, VideoCaptureProperty.FrameRate);
					int bright = VideoCaptureBackend.GetVideoCaptureProperty(backendSettings, VideoCaptureProperty.Brightness);
					int gain = VideoCaptureBackend.GetVideoCaptureProperty(backendSettings, VideoCaptureProperty.Gain);
					int exposure = VideoCaptureBackend.GetVideoCaptureProperty(backendSettings, VideoCaptureProperty.Exposure);

					if (width <= 0) {
						settings.SetAuto(SensorProperty.FrameWidth);
					} else {
						settings.SetInteger(SensorProperty.FrameWidth, width);
					}
					if (height <= 0) {
						settings.SetAuto(SensorProperty.FrameHeight);
					} else {
						settings.SetInteger(SensorProperty.FrameHeight, height);
					}
					if (rate <= 0) {
						settings.SetAuto(SensorProperty.FrameRate);
					} else {
						settings.SetInteger(SensorProperty.FrameRate, rate);
					}
					if (bright < 0) {
						settings.SetAuto(SensorProperty.Brightness);
					} else {
						settings.SetInteger(SensorProperty.Brightness, bright);
					}
					if (gain < 0) {
						settings.SetAuto(SensorProperty.Gain);
					} else {
						settings.SetInteger(SensorProperty.Gain, gain);
					}
					if (exposure < 0) {
						settings.SetAuto(SensorProperty.Exposure);
					} else {
						settings.SetInteger(SensorProperty.Exposure, exposure);
					}
				}
				
			}
			return true;
		}
		/// <inheritdoc/>
		public bool Stop() {
			lock (captureSync) {
				if (!Capturing) {
					return true;
				}
				if (!VideoCaptureBackend.StopVideoCapture(videoCapture)) {
					return false;
				}
				if (videoFrame != IntPtr.Zero) {
					VideoCaptureBackend.DestroyVideoCaptureFrame(videoCapture, videoFrame);
					videoFrame = IntPtr.Zero;
				}
				if (videoCapture != IntPtr.Zero) {
					VideoCaptureBackend.DestroyVideoCapture(captureContext, videoCapture);
					videoFrame = IntPtr.Zero;
				}
				if (backendSettings != IntPtr.Zero) {
					VideoCaptureBackend.FreeVideoCaptureSettings(backendSettings);
					backendSettings = IntPtr.Zero;
				}
			}
			return true;
		}
		/// <inheritdoc/>
		public SensorDataPacket Process(long timestamp) {
			lock (captureSync) {
				if (!Capturing) {
					return null;
				}

				// TODO implrement prediction service and divide split multiple calls into grab begin/end
				// get image and metadata from the backend
				if (!VideoCaptureBackend.BeginVideoCaptureFrameGrab(videoCapture)) {
					return null;
				}
				if (!VideoCaptureBackend.EndVideoCaptureFrameGrab(videoCapture)) {
					return null;
				}

				int width = VideoCaptureBackend.GetVideoCaptureFrameProperty(videoFrame, VideoFrameProperty.Width);
				int height = VideoCaptureBackend.GetVideoCaptureFrameProperty(videoFrame, VideoFrameProperty.Height);
				VideoFramePixelFormat format = (VideoFramePixelFormat)VideoCaptureBackend.GetVideoCaptureFrameProperty(
					videoFrame, VideoFrameProperty.PixelFormat);
				int pixelSize = VideoFrame.GetPixelSize(format);
				// construct a data packet as result
				if (width < 0 || width >= 16384 || height < 0 || height >= 16384 || pixelSize < 1 || pixelSize > 16) {
					return null;
				}
				int imageSize = width*height*pixelSize;
				byte[] image = new byte[imageSize];
				GCHandle imageBuffer = GCHandle.Alloc(image, GCHandleType.Pinned);
				if (!VideoCaptureBackend.CopyVideoCaptureFrame(videoFrame, imageBuffer.AddrOfPinnedObject(), imageSize)) {
					imageBuffer.Free();
					return null;
				}
				imageBuffer.Free();
				var frame = new VideoFrame(width, height, format, image);

				// TODO generate packet id
				return new SensorDataPacket(this, SensorDataType.Video, frame, 0);

			}
		}

	}




}

/*
namespace Irseny.Core.Sensors.VideoCapture {
	/// <summary>
	/// Sensor frontend for capturing video data from webcams.
	/// </summary>
	public class WebcamCaptureDisabled : ISensorBase {
		static IntPtr captureContext;

		IntPtr constructionInfo;
		IntPtr captureFrame;
		IntPtr videoCapture;


		object captureSync = new object();
		object imageEventSync = new object();
		object runEventSync = new object();
		Emgu.CV.VideoCapture capture = null;
		SensorSettings settings = new SensorSettings();
		readonly int id;
		SharedRefCleaner imageCleaner = new SharedRefCleaner(32);

		event EventHandler<ImageCapturedEventArgs> imageAvailable;
		event EventHandler<StreamEventArgs> captureStarted;
		event EventHandler<StreamEventArgs> captureStopped;

		public SensorType SensorType {
			get { return SensorType.Webcam; }
		}
		public int IntervalPrediction {
			get { return -1; }
		}
		static WebcamCaptureDisabled() {
			captureContext = VideoCaptureBackend.CreateVideoCaptureContext();
		}

		public WebcamCaptureDisabled(int id) {
			if (id < 0) throw new ArgumentOutOfRangeException("id");
			this.id = id;
		}

		private int Id {
			get { return id; }
		}

		public bool Capturing {
			get {
				lock (captureSync) {
					return capture != null;
				}
			}
		}

		public event EventHandler<ImageCapturedEventArgs> ImageAvailable {
			add {
				lock (imageEventSync) {
					imageAvailable += value;
				}
			}
			remove {
				lock (imageEventSync) {
					imageAvailable -= value;
				}
			}
		}

		public event EventHandler<StreamEventArgs> CaptureStarted {
			add {
				lock (runEventSync) {
					captureStarted += value;
				}
			}
			remove {
				lock (runEventSync) {
					captureStarted -= value;
				}
			}
		}

		public event EventHandler<StreamEventArgs> CaptureStopped {
			add {
				lock (runEventSync) {
					captureStopped += value;
				}
			}
			remove {
				lock (runEventSync) {
					captureStopped -= value;
				}
			}
		}

		public SensorSettings GetSettings() {
			lock (captureSync) {
				return new SensorSettings(settings);
			}
		}

		private void ReceiveImage(object sender, EventArgs args) {
			lock (captureSync) {
				if (capture == null) {
					return; // null if capture is stopped but internal capture thread still running
				}
				var colorImage = SharedRef.Create(new Emgu.CV.Mat());
				var grayImage = SharedRef.Create(new Emgu.CV.Mat());
				capture.Retrieve(colorImage.Reference);
				Emgu.CV.CvInvoke.CvtColor(colorImage.Reference, grayImage.Reference, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
				// TODO enable
				//OnImageAvailable(new ImageCapturedEventArgs(this, id, colorImage, grayImage));
				imageCleaner.CleanUpStep(4); // 2 images added on every receive, try to free more than those added
				imageCleaner.AddReference(colorImage);
				imageCleaner.AddReference(grayImage);

			}
		}

		protected void OnImageAvailable(ImageCapturedEventArgs args) {
			EventHandler<ImageCapturedEventArgs> handler;
			lock (imageEventSync) {
				handler = imageAvailable;
			}
			if (handler != null) {
				handler(this, args);
			}
		}

		protected void OnCaptureStarted(StreamEventArgs args) {
			EventHandler<StreamEventArgs> handler;
			lock (runEventSync) {
				handler = captureStarted;
			}
			if (handler != null) {
				handler(this, args);
			}
		}

		protected void OnCaptureStopped(StreamEventArgs args) {
			EventHandler<StreamEventArgs> handler;
			lock (runEventSync) {
				handler = captureStopped;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		public bool ApplySettings(SensorSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			lock (captureSync) {
				if (capture == null) {
					this.settings = new SensorSettings(settings);
					return true;
				}
				capture.Stop();
				// go through the restart path if the source changed
				int oldCamera = settings.GetInteger(SensorProperty.CameraId, 0);
				int newCamera = settings.GetInteger(SensorProperty.CameraId, 0);
				if (oldCamera != newCamera) {
					this.settings = new SensorSettings(settings);
					Stop();
					return Start();
				}
				this.settings = new SensorSettings(settings);
				ApplySettings();
				capture.Start();
			}
			return true;
		}
		private bool ApplySettings() {
			// assumes the lock is set
			if (capture == null) {
				return false;
			}
			if (settings == null) {
				return false;
			}
			// TODO: fix so that settings are actually applied like below
//			bool autoWidth = settings.IsAuto(CaptureProperty.FrameWidth);
//			if (!autoWidth) {
//				int width = settings.GetInteger(CaptureProperty.FrameWidth, 640);
//				autoWidth = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, width);
//			}
//			if (autoWidth) {
//				int width = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
//				settings.SetInteger(CaptureProperty.FrameWidth, width);
//			}
//			bool autoHeight = settings.IsAuto(CaptureProperty.FrameHeight);
//			if (!autoHeight) {
//				int height = settings.GetInteger(CaptureProperty.FrameHeight, 480);
//				autoHeight = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, height);
//			}
//			if (autoHeight) {
//				int height = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
//				settings.SetInteger(CaptureProperty.FrameHeight, height);
//			}
//			bool autoRate = settings.IsAuto(CaptureProperty.FrameRate);
//			if (!autoRate) {
//				int rate = settings.GetInteger(CaptureProperty.FrameRate, 30);
//				autoRate = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, rate);
//			}
//			if (autoRate) {
//				int rate = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
//				settings.SetInteger(CaptureProperty.FrameRate, rate);
//			}
//			if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0.0)) {
//
//			}
//			if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0.0)) {
//				capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, -1.0);
//			}
//			bool autoExposure = settings.IsAuto(CaptureProperty.Exposure);
//			if (!autoExposure) {
//				double exposure = settings.GetDecimal(CaptureProperty.Exposure, 0.0);
//				autoExposure = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, exposure);
//			}
//			if (autoExposure) {
//				double exposure = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure);
//				settings.SetDecimal(CaptureProperty.Exposure, exposure);
//			}
//			bool autoBrightness = settings.IsAuto(CaptureProperty.Brightness);
//			if (!autoBrightness) {
//				double brightbness = settings.GetDecimal(CaptureProperty.Brightness, 0.0);
//				autoBrightness = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, brightbness);
//			}
//			if (autoBrightness) {
//				double brightness = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness);
//				settings.SetDecimal(CaptureProperty.Brightness, brightness);
//			}
//			bool autoContrast = settings.IsAuto(CaptureProperty.Contrast);
//			if (!autoContrast) {
//				double contrast = settings.GetDecimal(CaptureProperty.Contrast, 0.0);
//				autoContrast = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, contrast);
//			}
//			if (autoContrast) {
//				double contrast = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast);
//				settings.SetDecimal(CaptureProperty.Contrast, contrast);
//			}
//			bool autoGain = settings.IsAuto(CaptureProperty.Gain);
//			if (!autoGain) {
//				double gain = settings.GetDecimal(CaptureProperty.Gain, 0.0);
//				autoContrast = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Gain, gain);
//			}
//			if (autoGain) {
//				double gain = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Gain);
//				settings.SetDecimal(CaptureProperty.Gain, gain);
//			}

			if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 60)) {
				LogManager.Instance.Log(LogEntry.CreateWarning(this, "unable to apply framerate"));
			} else {
				LogManager.Instance.Log(LogEntry.CreateMessage(this, "framerate set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps)));
			}
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, 0.5);
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.)
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 320);
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 240);
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0);
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 1);
			LogManager.Instance.Log(LogEntry.CreateMessage(this, "auto exposure: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure)));
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, -20);
			LogManager.Instance.Log(LogEntry.CreateMessage(this, "exposure set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure)));
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.ConvertRgb, 1);
			LogManager.Instance.Log(LogEntry.CreateMessage(this, "convert to rgb: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.ConvertRgb)));
			// TODO enabnled
			//capture.Start(new CaptureThreadExceptionHandler(this)); // exception thrown if started before setting properties

			return true;
		}
		public bool Start() {
			lock (captureSync) {
				if (capture != null) {
					return false;
				}
				//new Emgu.CV.VideoCapture(Emgu.CV.CvEnum.CaptureType.);
#if WINDOWS
				capture = new Emgu.CV.VideoCapture(settings.GetInteger(CaptureProperty.CameraId, 0));
#elif LINUX
				//capture = new Emgu.CV.VideoCapture(Emgu.CV.CvEnum.CaptureType.V4L);
				capture = new Emgu.CV.VideoCapture(settings.GetInteger(SensorProperty.CameraId, 0));
#endif
				if (!capture.IsOpened) {
					capture.Dispose();
					capture = null;
					return false;
				}
				this.settings = new SensorSettings(settings);
				ApplySettings();
				capture.ImageGrabbed += ReceiveImage;
				// all settings have to be set before the capture may start
				capture.Start(new CaptureThreadExceptionHandler(this));
			}
			OnCaptureStarted(new StreamEventArgs(this, Id));
			return true;
		}
		public bool Stop() {
			lock (captureSync) {
				if (capture == null) {
					return false;
				}
				capture.ImageGrabbed -= ReceiveImage;
				capture.Stop();
				capture.Dispose();
				capture = null;
				imageCleaner.CleanUpAll(); // this is can still leave some images laying around
			}
			OnCaptureStopped(new StreamEventArgs(this, Id));
			return true;
		}
		/// <inheritdoc />
		public SensorDataPacket Process(long timestamp) {
			// TODO implement
			return null;
		}

		private class CaptureThreadExceptionHandler : ExceptionHandler {
			WebcamCapture target;

			public CaptureThreadExceptionHandler(WebcamCapture target) : base() {
				if (target == null)
					throw new ArgumentNullException("target");
				this.target = target;
			}

			public override bool HandleException(Exception exception) {
				target.Stop(); // not capturing any longer
				LogManager.Instance.Log(LogEntry.CreateError(this, "Video capture failed with exception: " + exception.Message));
				Debug.WriteLine(exception.StackTrace);
				return true; // do not abort
			}

		}

	}




}

*/