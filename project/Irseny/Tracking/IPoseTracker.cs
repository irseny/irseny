using System;
namespace Irseny.Tracking {
	public interface IPoseTracker : IDisposable {
		/// <summary>
		/// Occurs when a pose has been detected.
		/// </summary>
		event EventHandler<PositionDetectedEventArgs> PositionDetected;
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
		/// Indicates whether the detector is currently active.
		/// </summary>
		bool Running {  get; }
		/// <summary>
		/// Uses the current pose as tracking origin.
		/// </summary>
		/// <returns></returns>
		bool Center();
		/// <summary>
		/// Applies tracker settings.
		/// </summary>
		/// <param name="settings">Settings.</param>
		/// <returns>Whether the application was successful.</returns>
		bool ApplySettings(TrackerSettings settings);
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
