using System;
using System.Threading;
using System.ServiceModel.Dispatcher;

namespace Irseny.Capture.Video {
	public class CaptureStream {
		object captureSync = new object();
		object imageEventSync = new object();
		object runEventSync = new object();
		Emgu.CV.VideoCapture capture = null;
		CaptureSettings settings = new CaptureSettings();
		readonly int id;
		event EventHandler<CaptureImageEventArgs> imageAvailable;
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
		public event EventHandler<CaptureImageEventArgs> ImageAvailable {
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
				var image = new Util.SharedRef<Emgu.CV.Mat>(new Emgu.CV.Mat());
				capture.Retrieve(image.Reference);
				OnImageAvailable(new CaptureImageEventArgs(this, id, image));
				if (!image.LastReference) {
					Console.WriteLine("image is not last reference");
				}
				image.Dispose(); // TODO: remove when finished testing
				long total = GC.GetTotalMemory(true);
				//Console.WriteLine("total memory used {0:#,##0}k", total/1000);
			}
		}
		protected void OnImageAvailable(CaptureImageEventArgs args) {
			EventHandler<CaptureImageEventArgs> handler;
			lock (imageEventSync) {
				handler = imageAvailable;
			}
			if (handler != null) {
				handler(this, args);
			} else {
				// TODO: put image into auto disposing structure
				//args.Image.Dispose();
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
			if (settings == null) throw new ArgumentNullException("settings");
			bool result;
			lock (captureSync) {
				if (capture == null) {
					capture = new Emgu.CV.VideoCapture(0);
					if (capture.IsOpened) {

						if (!capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 30)) {
							Log.LogManager.Instance.Log(Log.LogMessage.CreateWarning(this, "unable to apply framerate"));
						} else {
							Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "framerate set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps)));
						}
						//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.)
						capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 320);
						capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 240);
						//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Autograb, 0);
						//capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 1);
						Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "auto exposure: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure)));
						capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, 0.0);
						Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "exposure set to: " + capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure)));

						capture.Start(new CaptureThreadExceptionHandler(this)); // exception thrown if started before setting properties
						// TODO: apply settings
						this.settings = new CaptureSettings(settings);

						OnCaptureStarted(new StreamEventArgs(this, Id));
						capture.ImageGrabbed += delegate {

							//Thread.Sleep(1);
						};
						capture.ImageGrabbed += ReceiveImage;
						/*int count = 0;
						capture.ImageGrabbed += delegate {
							Console.WriteLine("image grabbed: " + count++);
						};*/
						result = true;
					} else {
						capture.Dispose();
						capture = null;
						result = false;
						Log.LogManager.Instance.Log(Log.LogMessage.CreateMessage(this, "unable to open capture"));
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
				if (target == null) throw new ArgumentNullException("target");
				this.target = target;
			}
			public override bool HandleException(Exception exception) {
				target.Stop(); // not capturing any longer
				Console.WriteLine("exception in capture stream:\n" + exception);
				// TODO: create log message
				return true; // do not stop
			}

		}

	}

	public class StreamEventArgs : EventArgs {
		public StreamEventArgs(CaptureStream stream, int streamId) {
			if (stream == null) throw new ArgumentNullException("stream");
			if (streamId < 0) throw new ArgumentOutOfRangeException("streamId");
			Stream = stream;
			StreamId = streamId;
		}
		public CaptureStream Stream { get; private set; }
		public int StreamId { get; private set; }

	}

	public class CaptureImageEventArgs : StreamEventArgs {
		Util.SharedRef<Emgu.CV.Mat> image;
		public CaptureImageEventArgs(CaptureStream stream, int streamId, Util.SharedRef<Emgu.CV.Mat> image) : base(stream, streamId) {
			if (image == null) throw new ArgumentNullException("image");
			this.image = image;
		}
		public Util.SharedRef<Emgu.CV.Mat> Image {
			// TODO: create shared ref instance
			get { return new Util.SharedRef<Emgu.CV.Mat>(image); }
		}

	}
}
