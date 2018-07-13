﻿using System;

namespace Irseny.Tracap {
	public class Basic3PointCapTracker : SingleImageCapTracker {
		CapTrackerOptions options;
		Util.SharedRefCleaner imageCleaner = new Util.SharedRefCleaner(32);
		public Basic3PointCapTracker(CapTrackerOptions options) : base() {
			this.options = new CapTrackerOptions(options);
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
			imageCleaner.DisposeAll(); // should not matter if some images are disposed on non detection threads
			base.Dispose();
		}
		protected override bool Step(Util.SharedRef<Emgu.CV.Mat> image) {
			var result = Util.SharedRef.Copy(image);
			OnInputProcessed(new ImageProcessedEventArgs(result));
			result.Dispose();
			var position = new CapPosition();
			OnPositionDetected(new PositionDetectedEventArgs(position));

			//imageCleaner.CleanUpStep(2);
			//imageCleaner.AddReference(result);
			return true;
		}

	}
}
