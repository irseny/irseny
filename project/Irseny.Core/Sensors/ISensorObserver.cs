﻿using System;

namespace Irseny.Core.Sensors {
	/// <summary>
	/// Sensor observers are typically supplied by a <see cref="ISensorObservable"/> instance.
	/// The interface provides all methods for notification of state changes.
	/// </summary>
	public interface ISensorObserver {
		/// <summary>
		/// Notifies of a newly connected sensor.
		/// </summary>
		/// <param name="sensors">Connected sensor.</param>
		void OnConnected(ISensorBase sensor);
		/// <summary>
		/// Notifies of a newly started sensor.
		/// </summary>
		/// <param name="sensor">Started sensor.</param>
		void OnStarted(ISensorBase sensor);
		/// <summary>
		/// Notifies of available data from a sensor.
		/// </summary>
		/// <param name="args">Sensor data.</param>
		void OnDataAvailable(SensorDataPacket args);
		/// <summary>
		/// Notifies of a newly stopped sensor.
		/// </summary>
		/// <param name="sensor">Stopped sensor.</param>
		void OnStopped(ISensorBase sensor);
		/// <summary>
		/// Notifies of a newly disconnected sensor.
		/// </summary>
		/// <param name="sensor">Disconnected sensor.</param>
		void OnDisconnected(ISensorBase sensor);
	}
}

