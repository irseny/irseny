using System;
using System.ServiceModel.Dispatcher;

namespace Irseny.Capture.Video {
	public class CaptureStream {


		object captureSync = new object();
		object eventSync = new object();
		Emgu.CV.VideoCapture capture = null;
		CaptureSettings settings;
		event EventHandler<CaptureImageEventArgs> imageAvailable;
		event EventHandler captureStarted;
		event EventHandler capturePaused;
		event EventHandler captureStopped;

		public CaptureStream() {
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
				lock (eventSync) {
					imageAvailable += value;
				}
			}
			remove {
				lock (eventSync) {
					imageAvailable -= value;
				}
			}
		}
		public CaptureSettings Settings {
			get {
				lock (captureSync) {
					return new CaptureSettings(); //TODO: copy settings
				}
			}
		}
		private void ReceiveImage(object sender, EventArgs args) {
			lock (captureSync) {
				OnImageAvailable(args);
			}
		}
		protected void OnImageAvailable(EventArgs args) {
			EventHandler<CaptureImageEventArgs> handler;
			lock (eventSync) {
				handler = imageAvailable;
			}
			if (handler != null) {
				var image = new Emgu.CV.Mat(); ;
				capture.Retrieve(image);
				handler(this, new CaptureImageEventArgs(image));
			}
		}
		public bool Start(CaptureSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");
			lock (captureSync) {
				if (capture == null) {
					capture = new Emgu.CV.VideoCapture(0);
					capture.ImageGrabbed += ReceiveImage;
					capture.Start(new CaptureThreadExceptionHandler(this));
					// TODO: apply settings
					this.settings = settings;
					return capture.IsOpened;
				}
			}
			return false;
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
			lock (captureSync) {
				if (capture != null) {
					capture.Stop();
					capture.Dispose();
					capture = null;
					return true;
				}
			}
			return false;
		}
		private class CaptureThreadExceptionHandler : ExceptionHandler {
			CaptureStream target;
			public CaptureThreadExceptionHandler(CaptureStream target) : base() {
				if (target == null) throw new ArgumentNullException("target");
				this.target = target;
			}
			public override bool HandleException(Exception exception) {
				target.Stop(); // not capturing any longer
							   // TODO: create log message
				return false;
			}

		}

	}
	public class CaptureImageEventArgs : EventArgs {

		public Emgu.CV.Mat Image { get; private set; }

		public CaptureImageEventArgs(Emgu.CV.Mat image) : base() {
			if (image == null) throw new ArgumentNullException("image");
			Image = image;
		}
	}
}
