using System;

namespace Irseny.Core.Sensors {
	public interface ISensorObserver {
		void OnConnected(ISensorBase sensors);
		void OnStarted(ISensorBase sensor);
		void OnDataAvailable(SensorDataPacket args);
		void OnStopped(ISensorBase sensor);
		void OnDisconnected(ISensorBase sensor);
	}
}

