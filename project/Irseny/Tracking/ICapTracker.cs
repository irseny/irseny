using System;
namespace Irseny.Tracking {
	public interface ICapTracker : IPoseDetector {
		/// <summary>
		/// Occurs when the head position has been detected.
		/// </summary>
		event EventHandler<PositionDetectedEventArgs> PositionDetected;
		/// <summary>
		/// Gets a value indicating whether the inner center position representation could be generated.
		/// </summary>
		/// <value><c>true</c> if centered; otherwise, <c>false</c>.</value>
		bool Centered { get; }
		/// <summary>
		/// Memorizes the currently detected position as the centered position.
		/// </summary>
		/// <returns>Whether identification was successful.</returns>
		bool Center();
	}
}
