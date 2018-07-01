using System;

namespace Irseny.Tracap {
	public class CapTracker : HeadDetector {
		public CapTracker() : base() {
		}
		protected override void Step(Emgu.CV.Mat image) {
			var result = Util.SharedRef.Create(image.Clone());
			OnInputProcessed(new ImageEventArgs(result));
			result.Dispose();
		}
	}
}
