using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceModel.Dispatcher;
using Irseny.Log;
using Irseny.Util;

namespace Irseny.Capture.Video {
	public class CaptureStream {
		object captureSync = new object();
		object imageEventSync = new object();
		object runEventSync = new object();
		Emgu.CV.VideoCapture capture = null;
		CaptureSettings settings = new CaptureSettings();
		readonly int id;
		SharedRefCleaner imageCleaner = new SharedRefCleaner(32);

		event EventHandler<ImageCapturedEventArgs> imageAvailable;
		event EventHandler<StreamEventArgs> captureStarted;
		event EventHandler<StreamEventArgs> captureStopped;

		public CaptureStream(int id) {
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

		public CaptureSettings GetSettings() {
			lock (captureSync) {
				return new CaptureSettings(settings);
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
				OnImageAvailable(new ImageCapturedEventArgs(this, id, colorImage, grayImage));
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
		public bool ApplySettings(CaptureSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			lock (captureSync) {
				if (capture == null) {
					return false;
				}
				capture.Stop();
				// go through the restart path if the source changed
				int oldCamera = settings.GetInteger(CaptureProperty.CameraId, 0);
				int newCamera = settings.GetInteger(CaptureProperty.CameraId, 0);
				if (oldCamera != newCamera) {
					this.settings = new CaptureSettings(settings);
					Stop();
					return Start(settings);
				}
				this.settings = new CaptureSettings(settings);
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
			/*bool autoWidth = settings.IsAuto(CaptureProperty.FrameWidth);
			if (!autoWidth) {
				int width = settings.GetInteger(CaptureProperty.FrameWidth, 640);
				autoWidth = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, width);
			}
			if (autoWidth) {
				int width = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth);
				settings.SetInteger(CaptureProperty.FrameWidth, width);
			}
			bool autoHeight = settings.IsAuto(CaptureProperty.FrameHeight);
			if (!autoHeight) {
				int height = settings.GetInteger(CaptureProperty.FrameHeight, 480);
				autoHeight = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, height);
			}
			if (autoHeight) {
				int height = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight);
				settings.SetInteger(CaptureProperty.FrameHeight, height);
			}
			bool autoRate = settings.IsAuto(CaptureProperty.FrameRate);
			if (!autoRate) {
				int rate = settings.GetInteger(CaptureProperty.FrameRate, 30);
				autoRate = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, rate);
			}
			if (autoRate) {
				int rate = (int)capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
				settings.SetInteger(CaptureProperty.FrameRate, rate);
			}
			if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0.0)) {

			}
			if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0.0)) {
				capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, -1.0);
			}
			bool autoExposure = settings.IsAuto(CaptureProperty.Exposure);
			if (!autoExposure) {
				double exposure = settings.GetDecimal(CaptureProperty.Exposure, 0.0);
				autoExposure = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, exposure);
			}
			if (autoExposure) {
				double exposure = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure);
				settings.SetDecimal(CaptureProperty.Exposure, exposure);
			}
			bool autoBrightness = settings.IsAuto(CaptureProperty.Brightness);
			if (!autoBrightness) {
				double brightbness = settings.GetDecimal(CaptureProperty.Brightness, 0.0);
				autoBrightness = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, brightbness);
			}
			if (autoBrightness) {
				double brightness = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness);
				settings.SetDecimal(CaptureProperty.Brightness, brightness);
			}
			bool autoContrast = settings.IsAuto(CaptureProperty.Contrast);
			if (!autoContrast) {
				double contrast = settings.GetDecimal(CaptureProperty.Contrast, 0.0);
				autoContrast = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast, contrast);
			}
			if (autoContrast) {
				double contrast = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Contrast);
				settings.SetDecimal(CaptureProperty.Contrast, contrast);
			}
			bool autoGain = settings.IsAuto(CaptureProperty.Gain);
			if (!autoGain) {
				double gain = settings.GetDecimal(CaptureProperty.Gain, 0.0);
				autoContrast = !capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Gain, gain);
			}
			if (autoGain) {
				double gain = capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Gain);
				settings.SetDecimal(CaptureProperty.Gain, gain);
			}*/

			if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 60)) {
				LogManager.Instance.Log(LogMessage.CreateWarning(this, "unable to apply framerate"));
			} else {
				LogManager.Instance.Log(LogMessage.CreateMessage(this, "framerate set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps)));
			}
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, 0.5);
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.)
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 320);
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 240);
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0);
			//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 1);
			LogManager.Instance.Log(LogMessage.CreateMessage(this, "auto exposure: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure)));
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, -20);
			LogManager.Instance.Log(LogMessage.CreateMessage(this, "exposure set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure)));
			capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.ConvertRgb, 1);
			LogManager.Instance.Log(LogMessage.CreateMessage(this, "convert to rgb: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.ConvertRgb)));
			capture.Start(new CaptureThreadExceptionHandler(this)); // exception thrown if started before setting properties

			return true;
		}
		public bool Start(CaptureSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			lock (captureSync) {
				if (capture != null) {
					return false;
				}
				//new Emgu.CV.VideoCapture(Emgu.CV.CvEnum.CaptureType.);
				capture = new Emgu.CV.VideoCapture(settings.GetInteger(CaptureProperty.CameraId, 0));
				//capture = new Emgu.CV.VideoCapture(Emgu.CV.CvEnum.CaptureType.V4L2);
				if (!capture.IsOpened) {
					capture.Dispose();
					capture = null;
					return false;
				}
				this.settings = new CaptureSettings(settings);
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

		private class CaptureThreadExceptionHandler : ExceptionHandler {
			CaptureStream target;

			public CaptureThreadExceptionHandler(CaptureStream target) : base() {
				if (target == null)
					throw new ArgumentNullException("target");
				this.target = target;
			}

			public override bool HandleException(Exception exception) {
				target.Stop(); // not capturing any longer
				LogManager.Instance.Log(LogMessage.CreateError(this, "Video capture failed with exception: " + exception.Message));
				Debug.WriteLine(exception.StackTrace);
				return true; // do not abort
			}

		}

	}




}
