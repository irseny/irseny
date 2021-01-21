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
	/// Universal sensor data packet.
	/// </summary>
	public class SensorDataPacket {
		/// <summary>
		/// Gets the packet ID. 
		/// The ID is used to distinguish between new and old data.
		/// </summary>
		/// <value>The packet ID.</value>
		public long PacketID { get; private set; }
		/// <summary>
		/// Gets the sensor that generated data.
		/// </summary>
		/// <value>The sensor.</value>
		public ISensorBase Sensor { get; private set; }
		/// <summary>
		/// Gets the data generated.
		/// </summary>
		/// <value>The generated data.</value>
		public object GenericData { get; private set; }
		/// <summary>
		/// Gets a type specifier for the provided data.
		/// </summary>
		/// <value>The type of the data.</value>
		public SensorDataType DataType { get; private set; }

		public SensorDataPacket(ISensorBase sensor, SensorDataType type, object data, long packetID) {
			Sensor = sensor;
			DataType = type;
			GenericData = data;
			PacketID = packetID;
		}
	}
}

