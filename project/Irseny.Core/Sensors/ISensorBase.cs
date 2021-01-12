using System;

namespace Irseny.Core.Sensors {
	/// <summary>
	/// Interface for sensors that generate standalone packets of data on a regular basis.
	/// Especially suited for video cameras or similar devices that operate at a fixed framerate.
	/// The sensor implements an interval prediction service to predict when the next packet of data will be available.
	/// This helps a managing system to time <see cref="Process"/> calls precisely 
	/// which may reduce delay with minimal processor time wasted.
	/// Sensors are supposed to be implemented thread safe.
	/// </summary>
	public interface ISensorBase {
		/// <summary>
		/// Gets the sensor type.
		/// </summary>
		/// <value>The type of the sensor.</value>
		SensorType SensorType { get; }
		/// <summary>
		/// Gets the predicted time interval until the next data packet should be available.
		/// </summary>
		/// <value>The predicted interval in milliseconds. A negative value indicates no prediction possible.</value>
		int IntervalPrediction { get; }
		/// <summary>
		/// Indicates whether the sensor is capturing data.
		/// </summary>
		/// <value><c>true</c> if capturing; otherwise, <c>false</c>.</value>
		bool Capturing { get; }
		/// <summary>
		/// Starts the data capturing process.
		/// </summary>
		bool Start();
		/// <summary>
		/// Stops the data capturing process.
		/// </summary>
		bool Stop();
		/// <summary>
		/// Does all major sensor interactions and returns captured data.
		/// </summary>
		/// <param name="timestamp">Current timestamp in milliseconds for interval prediction.</param>
		/// <returns>The latest data packet. May be null in case of errors or no new data available</returns>
		SensorDataPacket Process(long timestamp);
	}
}

