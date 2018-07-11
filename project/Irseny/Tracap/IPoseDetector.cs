using System;
namespace Irseny.Tracap {
	public interface IPoseDetector : IDisposable {
		/// <summary>
		/// Occurs when the algorithm has data to process.
		/// </summary>
		event EventHandler InputAvailable;
		/// <summary>
		/// Starts the detection process.
		/// </summary>
		/// <returns>Whether the operation was successful.</returns>
		bool Start();
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
