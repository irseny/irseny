using System;
using System.Collections.Generic;

namespace Irseny.Core.Sensors {
	/// <summary>
	/// Observable for sensors. Only used internally in <see cref="CaptureSystem"/>.
	/// </summary>
	public class SensorObservable : ISensorObservable {
		readonly object observerSync;
		List<ISensorObserver> observers;

		public SensorObservable () {
			observerSync = new object();
			observers = new List<ISensorObserver>();
		}
		/// <inheritdoc/>
		public IDisposable Subscribe(ISensorObserver observer) {
			if (observer == null) throw new ArgumentNullException("observer");
			lock (observerSync) {
				observers.Add(observer);
				return new SensorSubscription(this, observer);
			}
		}
		/// <summary>
		/// Unsubscribes the specified observer.
		/// </summary>
		/// <param name="observer">Observer to unsubscribe.</param>
		/// <returns>if the subscription was cancelled</returns>
		public bool Unsubscribe(ISensorObserver observer) {
			lock (observerSync) {
				int iObserver = observers.IndexOf(observer);
				if (iObserver < 0) {
					return false;
				}
				// we receive an exception if an observer is removed while iterating through them
				// therefore we keep the original observer list untouched
				// by creating a new one that will be used for future operations
				int capacity = observers.Count;
				var nextObservers = new List<ISensorObserver>(capacity);
				for (int i = 0; i < capacity; i++) {
					if (i != iObserver) {
						nextObservers.Add(observers[iObserver]);
					}
				}
				observers = nextObservers;
				return true;
			}
		}
		/// <summary>
		/// Reports that the sensor has been connected.
		/// </summary>
		/// <param name="sensor">Connected sensor.</param>
		public void OnConnected(ISensorBase sensor) {
			lock (observerSync) {
				
				foreach (var observer in observers) {
					observer.OnConnected(sensor);
				}

			}
		}
		/// <summary>
		/// Reports that the sensor has started operating
		/// and is expected to generate data packets.
		/// </summary>
		/// <param name="sensor">Started sensor.</param>
		public void OnStarted(ISensorBase sensor) {
			lock (observerSync) {
				
				foreach (var observer in observers) {
					observer.OnStarted(sensor);
				}

			}
		}
		/// <summary>
		/// Reports data availablility of the observed sensor.
		/// </summary>
		/// <param name="args">Sensor and data information.</param>
		public void OnDataAvailable(SensorDataPacket args) {
			lock (observerSync) {
				
				foreach (var observer in observers) {
					observer.OnDataAvailable(args);
				}

			}
		}
		/// <summary>
		/// Reports that the sensor has stopped operation.
		/// The sensor will not generate more data until started again.
		/// </summary>
		/// <param name="sensor">Stopped sensor.</param>
		public void OnStopped(ISensorBase sensor) {
			lock (observerSync) {
				
				foreach (var observer in observers) {
					observer.OnStopped(sensor);
				}

			}
		}
		/// <summary>
		/// Reports disconnects to the observers of the sensor.
		/// </summary>
		/// <param name="sensor">Disconnected sensor.</param>
		public void OnDisconnected(ISensorBase sensor) {
			lock (observerSync) {
				foreach (var observer in observers) {
					observer.OnDisconnected(sensor);
				}
			}
		}
		/// <summary>
		/// Incorporates enough information to unsubscribe an <see cref="ISensorObserver"/> 
		/// from a <see cref="SensorObservable"/> when the object is disposed.
		/// </summary>
		private class SensorSubscription : IDisposable {
			ISensorObserver observer;
			SensorObservable observable;

			public SensorSubscription(SensorObservable observable, ISensorObserver observer) {
				if (observable == null) throw new ArgumentNullException("observable");
				if (observer == null) throw new ArgumentNullException("observer");
				this.observable = observable;
				this.observer = observer;
			}
			public void Dispose() {
				observable.Unsubscribe(observer);
			}
		}
	}
}

