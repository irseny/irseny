using System;
using System.Collections.Generic;

namespace Irseny.Core.Sensors {
	/// <summary>
	/// Public interface of <see cref="SensorObservable"/>.
	/// </summary>
	public interface ISensorObservable {
		/// <summary>
		/// Subscribes the observer to the sensor state.
		/// </summary>
		/// <param name="observer">Sensor observer.</param>
		/// <returns>Subscription object that can be disposed to end the subscription.</returns>
		IDisposable Subscribe(ISensorObserver observer);


	}
}

