using System;
namespace Irseny.Tracap {
	public interface IPoseDetector : IDisposable {
		/// <summary>
		/// Occurs when the algorithm has data to process.
		/// </summary>
		event EventHandler InputAvailable;
		/// <summary>
		/// Occurs when the detection process is started.
		/// </summary>
		event EventHandler Started;
		/// <summary>
		/// Occurs when the detection process is stopped.
		/// </summary>
		event EventHandler Stopped;
		/// <summary>
		/// Indicates whether this instance has been started.
		/// </summary>
		/// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
		bool Running { get; }
		/// <summary>
		/// Starts the detection process.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		bool Start(TrackerSettings settings);
		/// <summary>
		/// Executes one detection iteration.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		bool Step();
		/// <summary>
		/// Stops the detection process.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		bool Stop();
	}
}
