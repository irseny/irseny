using System;

namespace Irseny.Tracap {
	public class Basic3PointCapTracker : SingleImageCapTracker {
		public Basic3PointCapTracker() : base() {
		}

		public override bool Start() {
			Running = true;
			return true;
		}

		public override bool Stop() {
			Running = false;
			return true;
		}
		public override void Dispose() {
			base.Dispose();
		}
		protected override bool Step(Util.SharedRef<Emgu.CV.Mat> image) {
			var result = Util.SharedRef.Copy(image);
			OnInputProcessed(new ImageEventArgs(result));
			var position = new CapPosition();
			OnPositionDetected(new CapPositionArgs(position));
			result.Dispose();
			return true;
		}

	}
}
