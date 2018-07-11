using System;

namespace Irseny.Tracap {
	public class Basic3PointCapTracker : SingleImageCapTracker {
		public Basic3PointCapTracker() : base() {
		}

		public override void Dispose() {
			throw new NotImplementedException();
		}

		public override bool Start() {
			throw new NotImplementedException();
		}

		public override bool Stop() {
			throw new NotImplementedException();
		}

		protected override bool Step(Util.SharedRef<Emgu.CV.Mat> image) {
			var result = Util.SharedRef.Copy(image);
			OnInputProcessed(new ImageEventArgs(result));
			result.Dispose();
			return true;
		}
	}
}
