﻿using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.ServiceModel.Dispatcher;

namespace Irseny.Capture.Video {
	public class CaptureStream {
		object captureSync = new object();
		object imageEventSync = new object();
		object runEventSync = new object();
		Emgu.CV.VideoCapture capture = null;
		CaptureSettings settings = new CaptureSettings();
		readonly int id;
		Util.SharedRefCleaner imageCleaner = new Util.SharedRefCleaner(32);

		event EventHandler<ImageCapturedEventArgs> imageAvailable;
		event EventHandler<StreamEventArgs> captureStarted;
		event EventHandler<StreamEventArgs> captureStopped;

		public CaptureStream(int id) {
			if (id < 0)
				throw new ArgumentOutOfRangeException("id");
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

		public CaptureSettings Settings {
			get {
				lock (captureSync) {
					return new CaptureSettings(settings);
				}
			}
		}

		private void ReceiveImage(object sender, EventArgs args) {
			lock (captureSync) {
				if (capture == null) {
					return; // null if capture is stopped but internal capture thread still running
				}
				var colorImage = Util.SharedRef.Create(new Emgu.CV.Mat());
				var grayImage = Util.SharedRef.Create(new Emgu.CV.Mat());
				capture.Retrieve(colorImage.Reference);
				Emgu.CV.CvInvoke.CvtColor(colorImage.Reference, grayImage.Reference, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
				OnImageAvailable(new ImageCapturedEventArgs(this, id, colorImage, grayImage));
				imageCleaner.AddReference(colorImage);
				imageCleaner.AddReference(grayImage);
				imageCleaner.CleanUpStep(4); // 2 images potentially added on every receive, try to free more than those added
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

		public bool Start(CaptureSettings settings) {
			if (settings == null)
				throw new ArgumentNullException("settings");
			bool result;
			lock (captureSync) {
				if (capture == null) {
					capture = new Emgu.CV.VideoCapture(0);
					if (capture.IsOpened) {

						if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 60)) {
							Log.LogManager.Instance.Log(Log.LogMessage.CreateWarning(this, "unable to apply framerate"));
						} else {
							Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "framerate set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps)));
						}
						//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, 0.5);
						//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.)
						capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 320);
						capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 240);
						//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0);
						//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 1);
						Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "auto exposure: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure)));
						capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, -20);
						Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "exposure set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure)));
						capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.ConvertRgb, 1);
						Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "convert to rgb: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.ConvertRgb)));
						capture.Start(new CaptureThreadExceptionHandler(this)); // exception thrown if started before setting properties
																				// TODO: apply settings
						this.settings = new CaptureSettings(settings);

						OnCaptureStarted(new StreamEventArgs(this, Id));
						capture.ImageGrabbed += ReceiveImage;
						result = true;
					} else {
						capture.Dispose();
						capture = null;
						result = false;
					}
				} else {
					result = false;
				}
			}
			return result;
		}

		public bool Pause() {
			bool result;
			lock (captureSync) {
				if (capture != null) {
					capture.Pause();
					result = true;
				} else {
					result = false;
				}
			}
			return result;
		}

		public bool Stop() {
			bool result;
			lock (captureSync) {
				imageCleaner.CleanUpAll(); // this is a non forced cleanup which can leave some images undisposed
				if (capture != null) {
					capture.Stop();
					capture.Dispose();
					capture = null;
					result = true;
				} else {
					result = false;
				}
			}
			if (result) {
				OnCaptureStopped(new StreamEventArgs(this, Id));
			}
			return result;
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
				Log.LogManager.Instance.Log(Log.LogMessage.CreateError(this, "Video capture failed with exception: " + exception.Message));
				Debug.WriteLine(exception.StackTrace);
				return true; // do not abort
			}

		}

	}




}
