using System;
using System.ServiceModel.Dispatcher;

namespace Irseny.Capture.Video {
	public class CaptureStream {


		object captureSync = new object();
		object imageEventSync = new object();
		object startedEventSync = new object();
		object stoppedEventSync = new object();
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
				lock (startedEventSync) {
					captureStarted += value;
				}
			}
			remove {
				lock (startedEventSync) {
					captureStarted -= value;
				}
			}
		}
		public event EventHandler<StreamEventArgs> CaptureStopped {
			add {
				lock (stoppedEventSync) {
					captureStopped += value;
				}
			}
			remove {
				lock (stoppedEventSync) {
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
				var image = new Emgu.CV.Mat();
				capture.Retrieve(image);
				OnImageAvailable(new CaptureImageEventArgs(this, id, image));
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
				args.Image.Dispose();
			}
		}
		protected void OnCaptureStarted(StreamEventArgs args) {
			EventHandler<StreamEventArgs> handler;
			lock (startedEventSync) {
				handler = captureStarted;
			}
			if (handler != null) {
				handler(this, args);
			}
		}
		protected void OnCaptureStopped(StreamEventArgs args) {
			EventHandler<StreamEventArgs> handler;
			lock (stoppedEventSync) {
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
					capture.Start(new CaptureThreadExceptionHandler(this));
					if (capture.IsOpened) {
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
			lock (captureSync) {
				if (capture != null) {
					capture.Pause();
					return true;
				}
			}
			return false;
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
				return false;
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
		Emgu.CV.Mat image;
		public CaptureImageEventArgs(CaptureStream stream, int streamId, Emgu.CV.Mat image) : base(stream, streamId) {
			if (image == null) throw new ArgumentNullException("image");
			this.image = image;
		}
		public Emgu.CV.Mat Image { 
			// TODO: create shared ref instance
			get { return image; }
		}

	}
}
