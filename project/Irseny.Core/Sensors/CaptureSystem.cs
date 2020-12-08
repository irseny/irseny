using System;
using System.Collections.Generic;
using System.Threading;
using Irseny.Core.Log;
using Irseny.Core.Sensors.VideoCapture;

namespace Irseny.Core.Sensors {
	/// <summary>
	/// Handles sensors management.
	/// Provides a collection of <see cref="ISensorBase"/> instances alongside functionlity 
	/// to simplify sensor event handling and data capturing.
	/// </summary>
	public class CaptureSystem {
		static CaptureSystem instance = null;

		Thread thread;
		volatile bool running;
		readonly object invokeSync;
		readonly object sensorSync;
		readonly object observableSync;
		Queue<EventHandler> toInvoke;
		readonly AutoResetEvent invokeSignal;
		readonly ISensorBase[] sensors;
		readonly SensorObservable[] observables;
		readonly IDisposable[] subscriptions;
		IList<WebcamCapture> streams = new List<WebcamCapture>(4);


		public static CaptureSystem Instance {
			get { return instance; }
		}

		public CaptureSystem() {
			invokeSync = new object();
			sensorSync = new object();
			observableSync = new object();
			running = false;
			thread = null;
			toInvoke = new Queue<EventHandler>();
			invokeSignal = new AutoResetEvent(false);
			sensors = new ISensorBase[16];
			subscriptions = new IDisposable[16];
			observables = new SensorObservable[16];
			for (int i = 0; i < observables.Length; i++) {
				observables[i] = new SensorObservable();
			}
		}

		public static void MakeInstance(CaptureSystem instance) {
			if (CaptureSystem.instance != null) {
				CaptureSystem.instance.SignalStop();
				CaptureSystem.instance.thread.Join(2048);
				if (CaptureSystem.instance.thread.IsAlive) {
					LogManager.Instance.LogWarning(instance, "Capture thread does not terminate. Aborting.");
					CaptureSystem.instance.thread.Abort();
				}
				CaptureSystem.instance.thread = null;
			}
			if (instance != null && instance.thread != null) {
				throw new ArgumentException("Has a running thread", "instance");
			}
			CaptureSystem.instance = instance;
			if (instance != null) {
				instance.thread = new Thread(instance.Run);
				instance.thread.Start();
			}

		}
		private void InvokePending() {
			Queue<EventHandler> pending;
			lock (invokeSync) {
				pending = toInvoke;
				toInvoke = new Queue<EventHandler>();
			}
			foreach (EventHandler handler in pending) {
				handler(this, new EventArgs());
			}

		}
		private void SignalStop() {
			running = false;
			invokeSignal.Set(); // wake up the operation invoking thread for exit
		}
		private void Run() {
			running = true;
			while (running) {
				invokeSignal.WaitOne();
				InvokePending();
			}
			// clean up
			lock (sensorSync) {
				for (int id = streams.Count - 1; id >= 0; id--) { // streams removed due to descending id
					DestroyStream(id);
				}

			}
		}
		public int ConnectSensor(ISensorBase sensor, int preferredIndex=-1) {
			if (sensor == null) throw new ArgumentNullException("sensor");
			int iSensor = -1;
			lock (sensorSync) {
				// first try to assign the sensor to its preferred index
				if (preferredIndex >= 0 && preferredIndex < sensors.Length) {
					
					if (sensors[preferredIndex] == null) {
						sensors[preferredIndex] = sensor;
						iSensor = preferredIndex;
					}
				}
				// if the index is already taken, find the next unassigned index
				if (iSensor < 0) {
					for (int i = 0; i < sensors.Length; i++) {
						if (sensors[i] == null) {
							sensors[i] = sensor;
							iSensor = i;
							break;
						}
					}
				}
				if (iSensor < 0) {
					// all indexes assigned, increase storage size
					return -1;
				}
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[iSensor];
			}
			observable.OnConnected(sensor);
			return iSensor;
		}
		public bool DisconnectSensor(int index) {
			ISensorBase sensor;
			lock (sensorSync) {
				// free the index if it has been assigned
				if (index < 0 || index >= sensors.Length) {
					return false;
				}
				if (sensors[index] == null) {
					return false;
				}
				sensor = sensors[index];
				sensors[index] = null;
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[index];
			}
			observable.OnDisconnected(sensor);
			return true;
		}
		public bool StartSensor(int index) {
			ISensorBase sensor;
			lock (sensorSync) {
				if (index < 0 || index >= sensors.Length) {
					return false;
				}
				if (sensors[index] == null) {
					return false;
				}
				sensor = sensors[index];
			}
			if (!sensor.Start()) {
				return false;
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[index];
			}
			observable.OnStarted(sensor);
			return true;
		}
		public bool StopSensor(int index) {
			ISensorBase sensor;
			lock (sensorSync) {
				if (index < 0 || index >= sensors.Length) {
					return false;
				}
				if (sensors[index] == null) {
					return false;
				}
				sensor = sensors[index];
			}
			if (!sensor.Stop()) {
				return false;
			}
			SensorObservable observable;
			lock (observableSync) {
				observable = observables[index];
			}
			observable.OnStopped(sensor);
			return true;
		}
		public ISensorBase GetSensor(int index) {
			lock (sensorSync) {
				if (index < 0 || index >= sensors.Length) {
					return null;
				}
				return sensors[index];
			}
		}
		public ISensorObservable ObserveSensor(int index) {
			lock (observableSync) {
				if (index < 0 || index >= observables.Length) {
					return null;
				}
				return observables[index];
			}
		}

		public int CreateStream() {
			WebcamCapture stream;
			int index;
			lock (sensorSync) {
				// find unused stream id
				for (index = 0; index < streams.Count; index++) {
					if (streams[index] == null) {
						break;
					}
				}
				// create stream
				stream = new WebcamCapture(index);
				if (index < streams.Count) {
					streams[index] = stream;
				} else {
					streams.Add(stream);
				}
			}
			return index;
		}
		public WebcamCapture GetStream(int id) {
			lock (sensorSync) {
				if (id >= 0 && id < streams.Count) {
					return streams[id];
				}
			}
			return null;
		}
		public bool DestroyStream(int id) {
			lock (sensorSync) {
				if (id >= 0 && id < streams.Count) {
					if (streams[id] != null) {
						streams[id].Stop();
					}
					if (id == streams.Count - 1) {
						streams.RemoveAt(id);
					} else {
						streams[id] = null;
					}
					return true;
				}
			}
			return false;
		}

		public void Invoke(EventHandler handler) {
			lock (invokeSync) {
				toInvoke.Enqueue(handler);
				invokeSignal.Set();
			}
		}

	}
}

