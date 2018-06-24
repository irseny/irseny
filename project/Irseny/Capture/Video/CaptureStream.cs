using System;

namespace Irseny.Capture.Video {
	public class CaptureStream {

		object captureSync = new object();
		Emgu.CV.VideoCapture capture = null;


		public CaptureStream() {
		}
		public bool Capturing {
			get {
				lock (captureSync) {
					return capture != null;
				}
			}
		}
		public CaptureSettings Settings {
			get {
				lock (captureSync) {
					return new CaptureSettings();
				}
			}
		}

		public bool Start(CaptureSettings settings) {
			if (settings == null) throw new ArgumentNullException("settings");

			return false;
		}
		public bool Stop() {
			lock (captureSync) {
				capture.Stop();
				capture.Dispose();
				capture = null;
			}
			return true;
		}

	}
}
