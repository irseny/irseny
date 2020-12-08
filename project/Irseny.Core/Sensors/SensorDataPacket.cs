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

