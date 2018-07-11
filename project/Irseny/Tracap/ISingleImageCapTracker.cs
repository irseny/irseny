using System;
namespace Irseny.Tracap {
	public interface ISingleImageCapTracker : ICapTracker {
		/// <summary>
		/// Occurs when an input image has been processed.
		/// </summary>
		event EventHandler<ImageEventArgs> InputProcessed;
		/// <summary>
		/// Queues an image for pose detection. Creates a copy of the given reference.
		/// </summary>
		/// <param name="image">Image.</param>
		void QueueInput(Util.SharedRef<Emgu.CV.Mat> image);
	}
}
