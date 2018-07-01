using System;

namespace Irseny.Tracap {
	public interface IHeadDetector : IDisposable {
		event EventHandler<ImageEventArgs> InputProcessed;
		event EventHandler<EventArgs> PositionDetected;
		bool Start();
		bool Stop();
		void QueueInput(Util.SharedRef<Emgu.CV.Mat> image);
	}
}

