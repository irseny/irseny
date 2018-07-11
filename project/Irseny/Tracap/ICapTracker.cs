using System;
namespace Irseny.Tracap {
	public interface ICapTracker : IPoseDetector {
		/// <summary>
		/// Occurs when the head position has been detected.
		/// </summary>
		event EventHandler<EventArgs> PositionDetected;
	}
}
