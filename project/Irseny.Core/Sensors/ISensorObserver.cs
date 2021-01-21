// This file is part of Irseny.
//
// Copyright (C) 2021  Thilo Gabel
//
// Irseny is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Irseny is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;

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

