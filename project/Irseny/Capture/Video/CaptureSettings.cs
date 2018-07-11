using System;

namespace Irseny.Capture.Video {
	public class CaptureSettings {
		public CaptureSettings() {
		}
		public CaptureSettings(CaptureSettings source) {
			if (source == null) throw new ArgumentNullException("source");
		}
	}
}
